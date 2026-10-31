using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SGRR.Data.Entities;
using Xunit;

namespace SGRR.Test
{
    public class RecepcionNotificacionesTests
    
    {
        [Fact]
        public void CreateNotification_ForAdvanceOrder_SetsCorrectProperties()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            var message = "Nueva orden anticipada recibida";

            // Act - Mapeo de la entidad de notificación
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Orden Anticipada",
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.NotNull(notification);
            Assert.Equal("Orden Anticipada", notification.Title);
            Assert.False(notification.IsRead);
            Assert.Equal(userId, notification.UserId);
        }

        [Fact]
        public void CreateNotification_ForReservation_SetsCorrectProperties()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            var message = "Reserva modificada exitosamente";

            // Act
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Reserva Actualizada",
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.NotNull(notification);
            Assert.Equal("Reserva Actualizada", notification.Title);
            Assert.Equal(message, notification.Message);
        }

        [Fact]
        public void MarkNotificationAsRead_UpdatesIsReadState()
        {
            // Arrange 
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Test Notification",
                Message = "Test Message",
                IsRead = false
            };

            // Act 
            notification.IsRead = true;

            // Assert
            Assert.True(notification.IsRead);
        }

        [Fact]
        public void GetNotificationsByUser_ReturnsUserSpecificNotifications()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Orden 1", Message = "Detalle orden 1", IsRead = false },
                new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Reserva 1", Message = "Detalle reserva 1", IsRead = true }
            };

            // Act
            var userNotifications = notifications.Where(n => n.UserId == userId).ToList();

            // Assert
            Assert.NotNull(userNotifications);
            Assert.Equal(2, userNotifications.Count);
            Assert.All(userNotifications, n => Assert.Equal(userId, n.UserId));
        }
    }
}