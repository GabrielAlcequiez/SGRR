using System.Globalization;
using Microsoft.AspNetCore.Localization;
using SGRR.Data.Context;
using SGRR.Data.Interfaces;
using SGRR.Data.Repositories;
using SGRR.Web.Interfaces;
using SGRR.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Los navegadores envian los valores de <input type="number"> y <input type="date"> siempre con
// formato invariante (punto decimal, fechas yyyy-MM-dd). Configuramos RequestLocalization para
// que el servidor los interprete con cultura invariante y no los mezcle con la cultura local
// (es-DO usa coma decimal: "180.00" se parseaba como 18000).
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var localUICulture = new CultureInfo("es-DO");
    options.DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture, localUICulture);
    options.SupportedCultures = [CultureInfo.InvariantCulture];
    options.SupportedUICultures = [localUICulture];
});
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IRestaurantSearchService, RestaurantSearchService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<IRestaurantReviewLock, RestaurantReviewLock>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
builder.Services.AddSingleton<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<SGRR.Data.Interfaces.IReservationRepository, SGRR.Data.Repositories.ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IOrdenAnticipadaRepository, OrdenAnticipadaRepository>();
builder.Services.AddScoped<IOrdenAnticipadaService, OrdenAnticipadaService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOwnerNotificationService, OwnerNotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOwnerMetricsService, OwnerMetricsService>();
builder.Services.AddScoped<IAdminReportService, AdminReportService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// === INYECTAR DATOS DE PRUEBA ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SGRR.Data.Context.AppDbContext>();

    // Llamamos al inicializador que acabamos de crear en la capa Data
    SGRR.Data.Context.DbInitializer.Initialize(context);
    var notificationRepository = services.GetRequiredService<INotificacionRepository>();
    DbInitializer.InitializeNotifications(context, notificationRepository);
}

app.Run();
