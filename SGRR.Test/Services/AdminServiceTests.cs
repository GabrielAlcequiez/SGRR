using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using SGRR.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SGRR.Test.Services;

public class AdminServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AdminService _service;
    private readonly CircularActivityLogRepository _logRepository;
    private readonly User _admin;
    private readonly User _client;

    public AdminServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        
        _context = new AppDbContext(options);
        _logRepository = new CircularActivityLogRepository();

        _service = new AdminService(
            new UserRepository(_context),
            _logRepository
        );

        // Preparar datos de prueba iniciales
        _admin = new User("Admin", "Root", "admin@foodbook.com", "Admin123!", Roles.Administrator);
        _client = new User("Cliente", "Uno", "cliente@foodbook.com", "Clave123", Roles.Client);
        
        _context.Users.Add(_admin);
        _context.Users.Add(_client);
        _context.SaveChanges();
    }

    [Fact]
    public void GetAllUsers_ConRolAdmin_RetornaListaDeUsuarios()
    {
        var users = _service.GetAllUsers(_admin);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public void GetAllUsers_ConRolNoAdmin_LanzaUnauthorizedAccessException()
    {
        Assert.Throws<UnauthorizedAccessException>(() => _service.GetAllUsers(_client));
    }

    [Fact]
    public void ToggleUserStatus_UsuarioExistenteYRolAdmin_CambiaEstadoYRegistraLog()
    {
        // Act: Bloquear al cliente
        _service.ToggleUserStatus(_client.Id, false, _admin);

        // Assert: Verificar en BD
        var updatedUser = _context.Users.Find(_client.Id);
        Assert.False(updatedUser!.IsActive);

        // Assert: Verificar el Log
        var logs = _logRepository.GetLast100Logs();
        Assert.Contains(logs, l => l.Severity == LogSeverity.Warning && l.Message.Contains("Inactivo"));
    }

    [Fact]
    public void ToggleUserStatus_AdminSeBloqueaASiMismo_LanzaInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => 
            _service.ToggleUserStatus(_admin.Id, false, _admin));
    }

    [Fact]
    public void ToggleUserStatus_UsuarioNoExiste_LanzaKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => 
            _service.ToggleUserStatus(Guid.NewGuid(), false, _admin));
    }

    [Fact]
    public void ToggleUserStatus_ConRolNoAdmin_LanzaUnauthorizedAccessException()
    {
        Assert.Throws<UnauthorizedAccessException>(() => 
            _service.ToggleUserStatus(_client.Id, false, _client));
    }

    public void Dispose() => _context.Dispose();
}