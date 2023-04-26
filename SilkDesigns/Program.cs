using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddControllersWithViews();
builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
var app = builder.Build();
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "Location",
//    pattern: "{controller=Location}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "CustomerLocation",
    pattern: "{controller=Location}/{action=CreateCustomerLocation}/{id?}");

app.MapControllerRoute(
    name: "LocationPlacement",
    pattern: "{controller=Location}/{action=CreateLocationPlacement}/{id?}");

app.MapControllerRoute(
    name: "LocationPlacement",
    pattern: "{controller=Location}/{action=UpdateLocationPlacement}/{id?}");

app.MapControllerRoute(
    name: "Arrangement",
    pattern: "{controller=Arrangement}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Arrangement",
    pattern: "{controller=Arrangement}/{action=Search}/{id?}/{search?}");

app.MapControllerRoute(
    name: "LocationPlacement",
    pattern: "{controller=Arrangement}/{action=CreateArrangementInventory}/{id?}");

app.MapControllerRoute(
    name: "ArrangementInventory",
    pattern: "{controller=Arrangement}/{action=CreateArrangementInventory}/{id?}");

app.MapControllerRoute(
    name: "RoutePlan",
    pattern: "{controller=RoutePlan}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Login",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
