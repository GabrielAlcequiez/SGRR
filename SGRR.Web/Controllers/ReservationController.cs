using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public IActionResult Create(Guid restaurantId, string restaurantName)
        {
            var model = new CreateReservationViewModel
            {
                RestaurantId = restaurantId,
                RestaurantName = restaurantName
            };

            return View("/Views/ReservationPreview/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSummary(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("/Views/ReservationPreview/Create.cshtml", model);
            }

            var available = await _reservationService.ValidateAvailabilityAsync(model.RestaurantId, model.Date, model.TimeSlot, model.PeopleCount);
            if (!available)
            {
                model.ErrorMessage = "El restaurante ha superado la capacidad disponible para este horario.";
                return View("/Views/ReservationPreview/Create.cshtml", model);
            }

            
            return View("/Views/ReservationPreview/Summary.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitConfirmation(CreateReservationViewModel model)
        {
            var clientId = Guid.NewGuid();
            var reservationCode = await _reservationService.RegisterReservationAsync(model, clientId);

            if (string.IsNullOrEmpty(reservationCode))
            {
                model.ErrorMessage = "No se pudo procesar la reserva. Intente con otro horario.";
                return View("/Views/ReservationPreview/Create.cshtml", model);
            }

            var detailsModel = new ReservationDetailsViewModel
            {
                ReservationCode = reservationCode,
                RestaurantName = model.RestaurantName,
                Date = model.Date.ToString("dd/MM/yyyy"),
                TimeSlot = model.TimeSlot,
                PeopleCount = model.PeopleCount,
                TableLocation = model.TableLocation,
                Status = "Active"
            };

            
            return View("/Views/ReservationPreview/Success.cshtml", detailsModel);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid reservationId)
        {
            var success = await _reservationService.CancelReservationAsync(reservationId);
            if (!success)
            {
                return BadRequest("No se pudo procesar la cancelación de la reserva.");
            }
            return RedirectToAction("Index", "Client");
        }
    }
}
