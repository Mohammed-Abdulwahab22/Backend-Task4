using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using Task4.Data;
using Task4.Services;
using System.Globalization;
using Task4.Models;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ClientsAPIDbContext>(options => options.UseInMemoryDatabase("MyDataBase"));
builder.Services.AddHostedService<MonthlySalaryService>();

InitializeClientsData(builder.Services);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void InitializeClientsData(IServiceCollection services)
{
    using (var serviceProvider = services.BuildServiceProvider())
    {
        var dbContext = serviceProvider.GetRequiredService<ClientsAPIDbContext>();

        if (!dbContext.ClientsInDatabase.Any())
        {
            SeedDataFromCsv(dbContext);
        }
    }
}

void SeedDataFromCsv(ClientsAPIDbContext dbContext)
{
    string filePath = "D:/Backend-Tasks/Task4/Clients.csv";

    if (File.Exists(filePath))
    {
        var clients = File.ReadAllLines(filePath)
            .Skip(1)
            .Select(line => line.Split(','))
            .Select(columns => new Client
            {
                Id = Guid.Parse(columns[0]),
                Name = columns[1],
                salary = float.Parse(columns[2], CultureInfo.InvariantCulture),
                balance = float.Parse(columns[3], CultureInfo.InvariantCulture),
                creationDate = columns[4]
            });

        dbContext.ClientsInDatabase.AddRange(clients);
        dbContext.SaveChanges();
    }
}