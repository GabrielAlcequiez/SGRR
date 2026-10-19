using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _service;
    private readonly CircularActivityLogRepository _logRepository;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        
        _context = new AppDbContext(options);
        _logRepository = new CircularActivityLogRepository();

        _service = new AuthService(
            new AuthRepository(_context),
            new UserRepository(_context),
            new RestaurantRepository(_context),
            _logRepository
        );
    }

    [Fact]
    public void Register_ConEmailUnico_CreaElUsuarioYRegistraLog()
    {
        var user = _service.Register("Juan", "Pérez", "juan@test.com", "Clave123", Roles.Client);

        var savedUser = _context.Users.FirstOrDefault(u => u.Email == "juan@test.com");
        Assert.NotNull(savedUser);
        Assert.Equal("Juan", savedUser.Name);
        Assert.Equal(Roles.Client, savedUser.Role);
        
        var logs = _logRepository.GetLast100Logs();
        Assert.Contains(logs, l => l.Severity == LogSeverity.Info && l.Message.Contains("Nuevo usuario registrado"));
    }

    [Fact]
    public void Register_ConEmailDuplicado_LanzaInvalidOperationException()
    {
        _service.Register("Juan", "Pérez", "juan@test.com", "Clave123", Roles.Client);

        Assert.Throws<InvalidOperationException>(() =>
            _service.Register("Otro", "Pérez", "juan@test.com", "Clave456", Roles.Client));
    }

    [Fact]
    public async Task RegisterProprietaryAsync_ConDatosValidos_CreaPropietarioYRestauranteYRegistraLog()
    {
        var user = await _service.RegisterProprietaryAsync(
            "Carlos", "Gómez", "propietario@test.com", "Clave123", 
            "Mi Restaurante", "Carnes", "Zona Centro", "Descripción");

        var savedUser = _context.Users.Include(u => u.Restaurant).FirstOrDefault(u => u.Email == "propietario@test.com");
        
        Assert.NotNull(savedUser);
        Assert.Equal(Roles.Propietary, savedUser.Role);
        Assert.NotNull(savedUser.Restaurant);
        Assert.Equal("Mi Restaurante", savedUser.Restaurant.Name);

        var logs = _logRepository.GetLast100Logs();
        Assert.Contains(logs, l => l.Severity == LogSeverity.Info && l.Message.Contains("Nuevo propietario registrado"));
    }

    [Fact]
    public async Task RegisterProprietaryAsync_ConRestauranteExistente_LanzaInvalidOperationException()
    {
        await _service.RegisterProprietaryAsync(
            "Carlos", "Gómez", "prop1@test.com", "Clave123", 
            "La Parrilla", "Carnes", "Naco", null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterProprietaryAsync(
                "Maria", "López", "prop2@test.com", "Clave456", 
                "La Parrilla", "Pastas", "Naco", null));
    }

    [Fact]
    public void Login_ConCredencialesValidasYUsuarioActivo_RetornaUsuarioYRegistraLog()
    {
        _service.Register("Ana", "García", "ana@test.com", "MiClaveSecreta", Roles.Client);

        var user = _service.Login("ana@test.com", "MiClaveSecreta");

        Assert.NotNull(user);
        Assert.True(user.IsActive);
        
        var logs = _logRepository.GetLast100Logs();
        Assert.Contains(logs, l => l.Severity == LogSeverity.Info && l.Message.Contains("inició sesión"));
    }
    
    [Fact]
    public void Login_ConUsuarioInactivo_LanzaUnauthorizedAccessExceptionYRegistraLogWarning()
    {
        var user = _service.Register("Luis", "Mena", "luis@test.com", "Clave123", Roles.Client);
        user.SetActive(false); // Asumiendo que existe este método como vi en tus ejemplos, o puedes usar: user.IsActive = false;
        _context.SaveChanges();

        var ex = Assert.Throws<UnauthorizedAccessException>(() =>
            _service.Login("luis@test.com", "Clave123"));

        Assert.Contains("bloqueada", ex.Message);

        var logs = _logRepository.GetLast100Logs();
        Assert.Contains(logs, l => l.Severity == LogSeverity.Warning && l.Message.Contains("denegado"));
    }

    public void Dispose() => _context.Dispose();
}