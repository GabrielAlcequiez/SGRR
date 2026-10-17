using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Context;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Asegura que la base de datos en memoria esté creada
        context.Database.EnsureCreated();

        // Si ya existen usuarios, no hacemos nada para no duplicar datos
        if (context.Users.Any())
        {
            return;
        }

        // 1. Crear Super Administrador
        var admin = new User(
            name: "Super",
            lastName: "Admin",
            email: "admin@foodbook.com",
            password: "123456",
            role: Roles.Administrator
        );

        // 2. Crear Cliente de prueba
        var client = new User(
            name: "Juan",
            lastName: "Pérez",
            email: "cliente@foodbook.com",
            password: "123456",
            role: Roles.Client
        );

        // 3. Crear Propietario de prueba con su Restaurante
        var proprietary = new User(
            name: "Carlos",
            lastName: "Dueño",
            email: "propietario@foodbook.com",
            password: "123456",
            role: Roles.Propietary
        );

        var restaurant = new Restaurant
        {
            Name = "El Sazón Dominicano",
            Specialty = "Comida Criolla",
            Location = "Santo Domingo Este",
            Description = "El mejor mangú y yaroa de la zona oriental."
        };

        // Asociar el restaurante al propietario usando el método de tu entidad
        proprietary.SetRestaurant(restaurant);


        // Guardar todos los registros en la base de datos
        context.Users.AddRange(admin, client, proprietary);
        context.Restaurants.Add(restaurant);
        context.SaveChanges();

        //Platos vinculados al ID que se acaba de generar
        var seededMenuItems = new List<MenuItem>
        {
            new MenuItem("Mangú con los Tres Golpes", "Mangú sedoso acompañado de queso frito, salami dominicano y huevo.", 350.00m, "Platos Fuertes", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Mofongo de Chicharrón", "Plátano verde majado con ajo y chicharrón crujiente.", 450.00m, "Platos Fuertes", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Yaroa de Pollo", "Capa de papas fritas con pollo desmechado, mucho queso y salsas.", 300.00m, "Entradas", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Jugo de Morir Soñando", "Tradicional combinación de leche evoporada, naranja y hielo.", 150.00m, "Bebidas", restaurant.Id, "https://picsum.photos")
        };
        context.MenuItems.AddRange(seededMenuItems);

        // Horarios semanales de atención
        for (int i = 0; i < 7; i++)
        {
            context.RestaurantSchedules.Add(
                new RestaurantSchedule(
                    restaurantId: restaurant.Id,
                    dayOfWeek: (DayOfWeek)i,
                    openingTime: new TimeSpan(12, 0, 0), // 12:00 PM
                    closingTime: new TimeSpan(22, 0, 0)  // 10:00 PM
                )
            );
        }

        // Pedidos de prueba para los reportes admin (Rendimiento por Restaurante y Balance de Ventas).
        // Se reparten en los últimos 45 días con montos variados y mezcla de estados para conciliaciones.
        var random = new Random(42); // semilla fija para resultados reproducibles
        var seededOrders = new List<Order>();

        for (int daysAgo = 0; daysAgo < 45; daysAgo++)
        {
            int ordersThatDay = random.Next(0, 4); // 0 a 3 pedidos por día

            for (int j = 0; j < ordersThatDay; j++)
            {
                // 80% completados, 10% pendientes y 10% cancelados
                int statusRoll = random.Next(0, 10);
                var status = statusRoll < 8 ? "Completed" : statusRoll < 9 ? "Pending" : "Cancelled";

                seededOrders.Add(new Order
                {
                    RestaurantId = restaurant.Id,
                    ClientId = client.Id,
                    TotalAmount = random.Next(2, 13) * 100.00m, // de 200.00 a 1,200.00
                    Status = status,
                    CreatedAt = DateTime.Today.AddDays(-daysAgo).AddHours(random.Next(12, 21))
                });
            }
        }

        context.Orders.AddRange(seededOrders);

        // Reservas activas de prueba en los últimos 30 días para la columna de reservas confirmadas
        var seededReservations = Enumerable.Range(0, 8).Select(i => new Reservation
        {
            ClientId = client.Id,
            RestaurantId = restaurant.Id,
            Date = DateTime.Today.AddDays(-(i * 3 + 1)), // cada 3 días hacia atrás
            TimeSlot = "19:00",
            PeopleCount = random.Next(1, 6),
            TableLocation = "Interior",
            ReservationCode = $"BK-SEED-{i + 1:D4}",
            Status = "Active"
        }).ToList();

        context.Reservations.AddRange(seededReservations);

        // Órdenes anticipadas de prueba (RF05) con estados y modalidades mezclados
        var year = DateTime.Now.Year;
        var ahora = DateTime.Now;

        OrdenAnticipada CrearOrden(int correlativo, string estado, string tipoEntrega,
            DateTime fechaProgramada, TimeSpan horaProgramada, DateTime fechaOrden,
            params (MenuItem Item, int Cantidad)[] productos)
        {
            var orden = new OrdenAnticipada
            {
                ClientId = client.Id,
                RestaurantId = restaurant.Id,
                PurchaseCode = $"ORD-{year}-{correlativo:D4}",
                FechaOrden = fechaOrden,
                FechaProgramada = fechaProgramada,
                HoraProgramada = horaProgramada,
                TipoEntrega = tipoEntrega,
                Estado = estado
            };

            foreach (var (item, cantidad) in productos)
            {
                orden.Productos.Add(new OrdenProducto
                {
                    OrdenAnticipadaId = orden.Id,
                    MenuItemId = item.Id,
                    NombreProducto = item.Name,
                    PrecioUnitario = item.Price,
                    Cantidad = cantidad,
                    Subtotal = item.Price * cantidad
                });
            }

            orden.Total = orden.Productos.Sum(producto => producto.Subtotal);
            return orden;
        }

        var seededOrdenesAnticipadas = new List<OrdenAnticipada>
        {
            CrearOrden(1, "Pendiente", "Recoger", DateTime.Today.AddDays(1), new TimeSpan(13, 0, 0), ahora.AddHours(-1),
                (seededMenuItems[0], 2), (seededMenuItems[3], 1)),
            CrearOrden(2, "Aceptada", "Delivery", DateTime.Today.AddDays(1), new TimeSpan(18, 30, 0), ahora.AddHours(-3),
                (seededMenuItems[1], 1), (seededMenuItems[2], 1)),
            CrearOrden(3, "Pendiente", "Recoger", DateTime.Today.AddDays(2), new TimeSpan(19, 0, 0), ahora.AddHours(-5),
                (seededMenuItems[2], 3)),
            CrearOrden(4, "Aceptada", "Recoger", DateTime.Today.AddDays(2), new TimeSpan(12, 30, 0), ahora.AddHours(-26),
                (seededMenuItems[0], 1), (seededMenuItems[1], 1))
        };

        seededOrdenesAnticipadas[1].DireccionEntrega = "Av. Las Américas #12, Santo Domingo Este";
        seededOrdenesAnticipadas[3].Notas = "Empacar aparte el queso frito.";

        context.OrdenesAnticipadas.AddRange(seededOrdenesAnticipadas);

        context.SaveChanges();
    }

    public static void InitializeNotifications(
        AppDbContext context,
        INotificacionRepository notificationRepository)
    {
        var client = context.Users.FirstOrDefault(user =>
            user.Email == "cliente@foodbook.com" && user.Role == Roles.Client);

        if (client is null || notificationRepository.GetByClientId(client.Id).Any())
            return;

        var reservation = new ClientNotification(
            client.Id,
            NotificationType.Reservation,
            "Reserva confirmada",
            "Tu reserva está confirmada para mañana a las 7:00 p. m.");

        var order = new ClientNotification(
            client.Id,
            NotificationType.Order,
            "Pedido listo",
            "Tu pedido está listo para recoger.");
        order.MarkAsRead();

        var reminder = new ClientNotification(
            client.Id,
            NotificationType.VisitReminder,
            "Recordatorio de visita",
            "Recuerda tu visita al restaurante mañana.");

        notificationRepository.Add(reservation);
        notificationRepository.Add(order);
        notificationRepository.Add(reminder);
    }
    
}


