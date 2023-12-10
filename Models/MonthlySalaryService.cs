using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Task4.Data;
using Task4.Models;

namespace Task4.Services
{
    public class MonthlySalaryService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public MonthlySalaryService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ClientsAPIDbContext>();
                    var clients = dbContext.ClientsInDatabase.ToList();

                    foreach (var client in clients)
                    {
                        client.balance += client.salary;
                        await dbContext.SaveChangesAsync();

                       
                    }

                    await UpdateCsv(clients);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task UpdateCsv(IEnumerable<Client> clients)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "clients.csv");

            using (var writer = new StreamWriter(filePath, false))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(clients);
            }
        }
    }
}