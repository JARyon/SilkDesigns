var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Location",
    pattern: "{controller=Location}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "CustomerLocation",
    pattern: "{controller=Location}/{action=CreateCustomerLocation}/{id?}");

app.MapControllerRoute(
    name: "LocationArrangement",
    pattern: "{controller=Location}/{action=CreateLocationArrangement}/{id?}");

app.MapControllerRoute(
    name: "LocationArrangement",
    pattern: "{controller=Location}/{action=UpdateLocationArrangement}/{id?}");

app.MapControllerRoute(
    name: "LocationArrangement",
    pattern: "{controller=Arrangement}/{action=CreateArrangementInventory}/{id?}");

app.MapControllerRoute(
    name: "ArrangementInventory",
    pattern: "{controller=Arrangement}/{action=CreateArrangementInventory}/{id?}");

app.Run();
