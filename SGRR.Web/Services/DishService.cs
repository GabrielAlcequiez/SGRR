using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Services
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _repository;

        public DishService(IDishRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Dish>> GetByRestaurantIdAsync(Guid restaurantId)
        {
            return await _repository.GetByRestaurantIdAsync(restaurantId);
        }

        public async Task<Dish?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Dish> CreateAsync(CreateDishDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            if (dto.Price <= 0)
                throw new ArgumentException("El precio debe ser mayor que 0.");

            var existentes = await _repository.GetByRestaurantIdAsync(dto.RestaurantId);
            if (existentes.Any(d => d.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Ya existe un plato con ese nombre en este menú.");

            var dish = new Dish
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description
            };

            await _repository.AddAsync(dish);
            return dish;
        }

        public async Task UpdateAsync(UpdateDishDto dto)
        {
            var dish = await _repository.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException("Plato no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("El nombre es obligatorio.");

            if (dto.Price <= 0)
                throw new ArgumentException("El precio debe ser mayor que 0.");

            var existentes = await _repository.GetByRestaurantIdAsync(dish.RestaurantId);
            if (existentes.Any(d => d.Id != dto.Id && d.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Ya existe un plato con ese nombre en este menú.");

            dish.Name = dto.Name;
            dish.Price = dto.Price;
            dish.Description = dto.Description;

            await _repository.UpdateAsync(dish);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
