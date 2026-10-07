using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _repository;

        public RestaurantService(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Restaurant> CreateAsync(CreateRestaurantDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Specialty))
                throw new ArgumentException("La especialidad es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new ArgumentException("La ubicación es obligatoria.");

            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Specialty = dto.Specialty,
                Location = dto.Location,
                Description = dto.Description
            };

            await _repository.AddAsync(restaurant);
            return restaurant;
        }

        public async Task UpdateAsync(UpdateRestaurantDto dto)
        {
            var restaurant = await _repository.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException("Restaurante no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Specialty))
                throw new ArgumentException("La especialidad es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new ArgumentException("La ubicación es obligatoria.");

            restaurant.Name = dto.Name;
            restaurant.Specialty = dto.Specialty;
            restaurant.Location = dto.Location;
            restaurant.Description = dto.Description;

            await _repository.UpdateAsync(restaurant);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
