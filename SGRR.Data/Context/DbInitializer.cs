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
            Rating = 4.5m,
            Description = "El mejor mangú y yaroa de la zona oriental."
        };

        // Asociar el restaurante al propietario usando el método de tu entidad
        proprietary.SetRestaurant(restaurant);


        // Guardar todos los registros en la base de datos
        context.Users.AddRange(admin, client, proprietary);
        context.Restaurants.Add(restaurant);
        context.SaveChanges();

        //Platos vinculados al ID que se acaba de generar
        context.MenuItems.AddRange(
            new MenuItem("Mangú con los Tres Golpes", "Mangú sedoso acompañado de queso frito, salami dominicano y huevo.", 350.00m, "Platos Fuertes", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Mofongo de Chicharrón", "Plátano verde majado con ajo y chicharrón crujiente.", 450.00m, "Platos Fuertes", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Yaroa de Pollo", "Capa de papas fritas con pollo desmechado, mucho queso y salsas.", 300.00m, "Entradas", restaurant.Id, "https://picsum.photos"),
            new MenuItem("Jugo de Morir Soñando", "Tradicional combinación de leche evoporada, naranja y hielo.", 150.00m, "Bebidas", restaurant.Id, "https://picsum.photos")
        );

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


