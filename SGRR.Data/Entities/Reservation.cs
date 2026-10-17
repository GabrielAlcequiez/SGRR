using System;

namespace SGRR.Data.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid RestaurantId { get; set; }
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int PeopleCount { get; set; }
        public string TableLocation { get; set; } = string.Empty;
        public string ReservationCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active o Canceled
    }
}
