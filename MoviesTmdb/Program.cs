using Microsoft.EntityFrameworkCore;
using MoviesTmdb.Data;
using MoviesTmdb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<MoviesTmdbDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Register TMDB API service with HttpClient
builder.Services.AddHttpClient<TmdbService>((sp, client) =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
}).Services.AddScoped<TmdbService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Set default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.Run();