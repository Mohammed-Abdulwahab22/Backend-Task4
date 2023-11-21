using Microsoft.EntityFrameworkCore;
using Task4.Models;

namespace Task4.Data
{
    public class ClientsAPIDbContext : DbContext
    {
        

        public ClientsAPIDbContext(DbContextOptions<ClientsAPIDbContext> options)
            : base(options) { }

        public DbSet<Client> ClientsInDatabase { get; set; }

    }
}
