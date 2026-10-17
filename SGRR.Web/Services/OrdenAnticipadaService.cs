using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Ordenes;

namespace SGRR.Web.Services;

public class OrdenAnticipadaService : IOrdenAnticipadaService
{
    // Serializa la asignación del correlativo de compra (ORD-{año}-{secuencia}).
    private static readonly SemaphoreSlim OrderLock = new(1, 1);

    private const int LeadMinutes = 30;
    private const string TipoRecoger = "Recoger";
    private const string TipoDelivery = "Delivery";

    private readonly IOrdenAnticipadaRepository _ordenes;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menus;
    private readonly IUserRepository _users;
    private readonly IOwnerNotificationService _ownerNotifications;
    private readonly INotificacionRepository _clientNotifications;
    private readonly ISystemActivityLogRepository _logRepository;

    public OrdenAnticipadaService(
        IOrdenAnticipadaRepository ordenes,
        IRestaurantRepository restaurants,
        IMenuRepository menus,
        IUserRepository users,
        IOwnerNotificationService ownerNotifications,
        INotificacionRepository clientNotifications,
        ISystemActivityLogRepository logRepository)
    {
        _ordenes = ordenes;
        _restaurants = restaurants;
        _menus = menus;
        _users = users;
        _ownerNotifications = ownerNotifications;
        _clientNotifications = clientNotifications;
        _logRepository = logRepository;
    }

    public async Task<string> CrearAsync(CreateOrdenAnticipadaDto dto, Guid clientId)
    {
        var client = _users.GetById(clientId);
        if (client is null || client.Role != Roles.Client || !client.IsActive)
        {
            dto.ErrorMessage = "Se requiere una cuenta de cliente activa para ordenar.";
            return string.Empty;
        }

        var restaurante = await _restaurants.GetByIdAsync(dto.RestaurantId);
        if (restaurante is null)
        {
            dto.ErrorMessage = "El restaurante seleccionado no existe.";
            return string.Empty;
        }

        if (!ValidarTipoEntrega(dto))
            return string.Empty;

        if (!await ValidarFechaHoraProgramadaAsync(dto))
            return string.Empty;

        var seleccionados = dto.Items.Where(item => item.Cantidad > 0).ToList();
        if (seleccionados.Count == 0)
        {
            dto.ErrorMessage = "Debe seleccionar al menos un producto del menú con cantidad mayor a cero.";
            return string.Empty;
        }

        // Solo platillos disponibles del menú actual: cubre existencia, pertenencia al restaurante y disponibilidad.
        var menu = (await _menus.GetByRestaurantIdAsync(dto.RestaurantId))
            .ToDictionary(item => item.Id);

        var orden = new OrdenAnticipada
        {
            ClientId = clientId,
            RestaurantId = dto.RestaurantId,
            FechaOrden = DateTime.Now,
            FechaProgramada = dto.FechaProgramada.Date,
            HoraProgramada = dto.HoraProgramada,
            TipoEntrega = dto.TipoEntrega,
            DireccionEntrega = dto.TipoEntrega == TipoDelivery ? dto.DireccionEntrega?.Trim() : null,
            Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim(),
            Estado = "Pendiente"
        };

        foreach (var item in seleccionados)
        {
            if (!menu.TryGetValue(item.MenuItemId, out var menuItem))
            {
                dto.ErrorMessage = "Uno de los productos seleccionados no está disponible en el menú del restaurante.";
                return string.Empty;
            }

            orden.Productos.Add(new OrdenProducto
            {
                OrdenAnticipadaId = orden.Id,
                MenuItemId = menuItem.Id,
                NombreProducto = menuItem.Name,
                PrecioUnitario = menuItem.Price,
                Cantidad = item.Cantidad,
                Subtotal = menuItem.Price * item.Cantidad
            });
        }

        orden.Total = orden.Productos.Sum(producto => producto.Subtotal);
        if (orden.Total <= 0)
        {
            dto.ErrorMessage = "El total de la orden debe ser mayor a cero.";
            return string.Empty;
        }

        await OrderLock.WaitAsync();
        try
        {
            var year = DateTime.Now.Year;
            var correlativo = await _ordenes.CountByYearAsync(year) + 1;
            orden.PurchaseCode = $"ORD-{year}-{correlativo:D4}";
            await _ordenes.AddAsync(orden);
        }
        finally
        {
            OrderLock.Release();
        }

        await NotificarPropietarioAsync(orden);
        var clientName = $"{client.Name} {client.LastName}".Trim();
        _logRepository.AddLog(
            LogSeverity.Info,
            $"Nueva orden anticipada creada por el cliente '{(string.IsNullOrWhiteSpace(clientName) ? client.Email : clientName)}' para el restaurante '{restaurante.Name}'.");
        return orden.PurchaseCode;
    }

    public async Task<IEnumerable<OrdenAnticipadaViewModel>> GetByClientIdAsync(Guid clientId)
    {
        var ordenes = await _ordenes.GetByClientIdAsync(clientId);
        var nombres = await GetRestaurantNamesAsync();
        return ordenes
            .Select(orden => ToViewModel(orden, nombres.GetValueOrDefault(orden.RestaurantId, "Restaurante no disponible")))
            .ToList();
    }

    public async Task<IEnumerable<OrdenAnticipadaViewModel>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        var ordenes = await _ordenes.GetByRestaurantIdAsync(restaurantId);
        var nombres = await GetRestaurantNamesAsync();
        var clientes = _users.GetAll().ToDictionary(user => user.Id, user => $"{user.Name} {user.LastName}".Trim());
        return ordenes
            .Select(orden => ToViewModel(
                orden,
                nombres.GetValueOrDefault(orden.RestaurantId, "Restaurante no disponible"),
                clientes.GetValueOrDefault(orden.ClientId, "Cliente no disponible")))
            .ToList();
    }

    public Task<bool> AceptarAsync(Guid ordenId, Guid restaurantId) =>
        CambiarEstadoAsync(ordenId, restaurantId, "Aceptada");

    public Task<bool> RechazarAsync(Guid ordenId, Guid restaurantId) =>
        CambiarEstadoAsync(ordenId, restaurantId, "Rechazada");

    public Task<bool> MarcarEntregadaAsync(Guid ordenId, Guid restaurantId) =>
        CambiarEstadoAsync(ordenId, restaurantId, "Entregada");

    private async Task<bool> CambiarEstadoAsync(Guid ordenId, Guid restaurantId, string nuevoEstado)
    {
        var orden = await _ordenes.GetByIdAsync(ordenId);
        if (orden is null || orden.RestaurantId != restaurantId)
            return false;

        var transicionValida = nuevoEstado switch
        {
            "Aceptada" or "Rechazada" => orden.Estado == "Pendiente",
            "Entregada" => orden.Estado == "Aceptada",
            _ => false
        };
        if (!transicionValida)
            return false;

        orden.Estado = nuevoEstado;
        if (!await _ordenes.UpdateAsync(orden))
            return false;

        await NotificarClienteAsync(orden, nuevoEstado);
        var client = _users.GetById(orden.ClientId);
        var restaurant = await _restaurants.GetByIdAsync(orden.RestaurantId);
        var clientName = client is null ? "Desconocido" : $"{client.Name} {client.LastName}".Trim();
        _logRepository.AddLog(
            LogSeverity.Info,
            $"La orden {orden.PurchaseCode} del cliente '{clientName}' en el restaurante '{restaurant?.Name ?? "Desconocido"}' fue actualizada a estado '{nuevoEstado}'.");
        return true;
    }

    private async Task NotificarPropietarioAsync(OrdenAnticipada orden)
    {
        var propietario = _users.GetOwnerByRestaurantId(orden.RestaurantId);
        if (propietario is null)
            return;

        var resumen = string.Join(", ", orden.Productos.Select(producto => $"{producto.Cantidad} × {producto.NombreProducto}"));
        var entrega = orden.TipoEntrega == TipoDelivery && !string.IsNullOrWhiteSpace(orden.DireccionEntrega)
            ? $"Delivery a {orden.DireccionEntrega}"
            : "Recoger en el restaurante";
        var programada = $"{orden.FechaProgramada:dd/MM/yyyy} a las {orden.HoraProgramada:hh\\:mm}";
        await _ownerNotifications.NotifyNewOrderAsync(orden.RestaurantId, propietario.Id, orden.PurchaseCode, $"{resumen} • {entrega} • {programada}");
    }

    private Task NotificarClienteAsync(OrdenAnticipada orden, string estado)
    {
        var (titulo, mensaje) = estado switch
        {
            "Aceptada" => ("Orden aceptada", $"Tu orden {orden.PurchaseCode} fue aceptada por el restaurante."),
            "Rechazada" => ("Orden rechazada", $"Tu orden {orden.PurchaseCode} fue rechazada por el restaurante."),
            "Entregada" => ("Orden entregada", $"Tu orden {orden.PurchaseCode} fue entregada. ¡Buen provecho!"),
            _ => (string.Empty, string.Empty)
        };

        if (titulo.Length == 0)
            return Task.CompletedTask;

        _clientNotifications.Add(new ClientNotification(orden.ClientId, NotificationType.Order, titulo, mensaje, orden.Id));
        return Task.CompletedTask;
    }

    private async Task<bool> ValidarFechaHoraProgramadaAsync(CreateOrdenAnticipadaDto dto)
    {
        var ahora = DateTime.Now;
        if (dto.FechaProgramada.Date < ahora.Date)
        {
            dto.ErrorMessage = "La fecha programada no puede estar en el pasado.";
            return false;
        }

        var programada = dto.FechaProgramada.Date.Add(dto.HoraProgramada);
        if (programada < ahora.AddMinutes(LeadMinutes))
        {
            dto.ErrorMessage = $"La hora programada debe ser al menos {LeadMinutes} minutos después de la hora actual.";
            return false;
        }

        var schedules = await _menus.GetSchedulesByRestaurantIdAsync(dto.RestaurantId);
        var daySchedule = schedules.FirstOrDefault(schedule => schedule.IsActive && schedule.DayOfWeek == dto.FechaProgramada.DayOfWeek);
        if (daySchedule is null)
        {
            dto.ErrorMessage = "El restaurante no tiene horario de atención para el día seleccionado.";
            return false;
        }

        if (dto.HoraProgramada < daySchedule.OpeningTime || dto.HoraProgramada >= daySchedule.ClosingTime)
        {
            dto.ErrorMessage = $"La hora programada debe estar dentro del horario de atención ({daySchedule.OpeningTime:hh\\:mm} - {daySchedule.ClosingTime:hh\\:mm}).";
            return false;
        }

        return true;
    }

    private static bool ValidarTipoEntrega(CreateOrdenAnticipadaDto dto)
    {
        if (dto.TipoEntrega != TipoRecoger && dto.TipoEntrega != TipoDelivery)
        {
            dto.ErrorMessage = "Seleccione un tipo de entrega válido (Recoger o Delivery).";
            return false;
        }

        if (dto.TipoEntrega == TipoDelivery && string.IsNullOrWhiteSpace(dto.DireccionEntrega))
        {
            dto.ErrorMessage = "La dirección de entrega es obligatoria para la modalidad Delivery.";
            return false;
        }

        return true;
    }

    private async Task<Dictionary<Guid, string>> GetRestaurantNamesAsync()
    {
        var restaurants = await _restaurants.GetAllAsync();
        return restaurants.ToDictionary(restaurant => restaurant.Id, restaurant => restaurant.Name);
    }

    private static OrdenAnticipadaViewModel ToViewModel(OrdenAnticipada orden, string restaurantName, string clientName = "")
    {
        return new OrdenAnticipadaViewModel
        {
            Id = orden.Id,
            PurchaseCode = orden.PurchaseCode,
            RestaurantId = orden.RestaurantId,
            RestaurantName = restaurantName,
            ClientId = orden.ClientId,
            ClientName = clientName,
            FechaOrden = orden.FechaOrden,
            FechaProgramada = orden.FechaProgramada,
            HoraProgramada = orden.HoraProgramada,
            TipoEntrega = orden.TipoEntrega,
            DireccionEntrega = orden.DireccionEntrega,
            Notas = orden.Notas,
            Total = orden.Total,
            Estado = orden.Estado,
            Productos = orden.Productos.Select(producto => $"{producto.Cantidad} × {producto.NombreProducto}").ToList()
        };
    }
}
