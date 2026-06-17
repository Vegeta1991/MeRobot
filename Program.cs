using MeRobot.Models;
using MeRobot.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RobotService>();//jeden robot dla wszytskich opcji ecc
builder.Services.AddHostedService(sp => sp.GetRequiredService<RobotService>());//kiedy uruchomi sie apka urochom robotservice i wez go z DI nie nowy.

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddCors(options =>//apke strone mozna otwierac wszedzie zawsze wysyla zapytania do API , otwiera droge (niebezpieczne)
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()// bezpieczniej dodac tutaj konkretna domene https://www.mojapp.com np
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())//jezeli aplikacja nie jest w developerze to :
{
    app.UseExceptionHandler("/Error");//wyswietl ladny error bez szczegolow
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();// wymus HTTPS (HSTS)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.UseCors();

app.Run();

