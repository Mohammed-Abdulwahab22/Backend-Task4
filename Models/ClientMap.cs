using CsvHelper.Configuration;

namespace Task4.Models
{
    public class ClientMap : ClassMap<Client>
    {
        public ClientMap()
        {
            Map(m => m.Id).Name("ID");
            Map(m => m.Name).Name("Name");
            Map(m => m.salary).Name("Salary");
            Map(m => m.balance).Name("Balance");
            Map(m => m.creationDate).Name("RegistrationDate");
        }
    }
}
