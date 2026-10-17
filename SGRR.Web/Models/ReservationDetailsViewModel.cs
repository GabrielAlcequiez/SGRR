using System;

namespace SGRR.Web.Models
{
    public class ReservationDetailsViewModel
    {
        public string ReservationCode { get; set; } = string.Empty; // Ej: RES-9821
        public string RestaurantName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public int PeopleCount { get; set; }
        public string TableLocation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active o Canceled
    }
}
