using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task4.Data;
using Task4.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using Serilog;
using System;

namespace Task4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankClientsController : ControllerBase
    {
        private static readonly Mutex CsvMutex = new Mutex();

        private readonly ClientsAPIDbContext dbContext;

        public BankClientsController(ClientsAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("createAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] createAccount create)
        {
            LogRequestInfo(nameof(CreateAccount), create);



            if (create.Name != null && create.salary != null )
            {
                bool clientExists = await dbContext.ClientsInDatabase
                    .AnyAsync(c => c.Name == create.Name && c.salary == create.salary);

                if (clientExists)
                {

                    return BadRequest("Client already exist.");
                }
                else
                {
                    string dateFormat = "yyyy-MM-dd HH:mm:ss";

                    var newClient = new Client
                    {
                        Id = Guid.NewGuid(),
                        Name = create.Name,
                        salary = create.salary,
                        balance = create.salary,
                        creationDate = DateTime.Now.ToString(dateFormat),
                    };
                    dbContext.ClientsInDatabase.Add(newClient);
                    await dbContext.SaveChangesAsync();

                    await WriteToCsv(new List<Client> { newClient });
                    return Ok(newClient);

                    
                }
                
                
               
            }
            else
            {
                Log.Error("Name and Salary are required.");

                return BadRequest("Name And Salary required");
            }
        }

        [HttpPost("deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromForm] deleteAccount delete)
        {
            LogRequestInfo(nameof(DeleteAccount), delete);

            var fileToRemove = dbContext.ClientsInDatabase.FirstOrDefault(f => f.Id == delete.Id);
            if (fileToRemove != null)
            {
                dbContext.Remove(fileToRemove);
                dbContext.SaveChanges();

                var remainingClients = dbContext.ClientsInDatabase.ToList();

                await DeleteCsv(remainingClients);

                return Ok("Deleted succefully!");
            }
            else
            {
                Log.Error("File Doesn't exist");

                return BadRequest("File Doesn't exist");
            }
           

        }

        [HttpPost("deposit")]
        public async Task<IActionResult> DepositAccount([FromForm] depositAccount deposit)
        {
            if (deposit.depositAmount <= 0)
            {
                return BadRequest("Deposit amount should be greater than zero.");
            }

            var clientToUpdate = dbContext.ClientsInDatabase.FirstOrDefault(c => c.Id == deposit.Id);

            if (clientToUpdate != null)
            {
                clientToUpdate.balance += deposit.depositAmount;

                await dbContext.SaveChangesAsync();

                var allClients = dbContext.ClientsInDatabase.ToList();
                await DeleteCsv(allClients);

                return Ok("Deposit successful. New balance: " + clientToUpdate.balance);
            }
            else
            {
                return BadRequest("Client not found.");
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> WithdrawAccount([FromForm] withdrawAccount withdraw)
        {
            if (withdraw.withdrawAmount <= 0)
            {
                return BadRequest("WithDraw amount should be greater than zero.");
            }

            var clientToUpdate = dbContext.ClientsInDatabase.FirstOrDefault(c => c.Id == withdraw.Id);

            if (clientToUpdate != null)
            {
                clientToUpdate.balance -= withdraw.withdrawAmount;

                await dbContext.SaveChangesAsync();

                var allClients = dbContext.ClientsInDatabase.ToList();
                await DeleteCsv(allClients);

                return Ok("Withdraw successful. New balance: " + clientToUpdate.balance);
            }
            else
            {
                return BadRequest("Client not found.");
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAccount([FromForm] transferAccount transfer)
        {
           
                var recieverid = dbContext.ClientsInDatabase.FirstOrDefault(f => f.Id == transfer.receiverId);
                var senderid = dbContext.ClientsInDatabase.FirstOrDefault(f => f.Id == transfer.senderId);
                if (recieverid != null || senderid != null)
                {
                    if (transfer.transferAmount <= 0) { return BadRequest("transfer amount should be greater than zero!!"); }
                    else {
                    recieverid.balance += transfer.transferAmount;
                    senderid.balance -= transfer.transferAmount;

                    await dbContext.SaveChangesAsync();

                    var allClients = dbContext.ClientsInDatabase.ToList();
                    await DeleteCsv(allClients);

                    return Ok(transfer.transferAmount + " transfered to: " + recieverid.Name);
                }
                }
                else
                {
                    return BadRequest("Wrong sender or reciever id!!");
                }
            
        }

        [HttpGet("RetrieveByID")]
        public IActionResult GetClientById([FromQuery] Guid clientId) 
        {
            var client = dbContext.ClientsInDatabase
                       .FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                return Ok(client);
            }
            else
            {
                return NotFound("Client not found.");
            }

        }

        [HttpGet("RetrieveBySalary")]
        public IActionResult GetBySalary()
        {
            var highSalaryClients = dbContext.ClientsInDatabase
                                   .Where(c => c.salary > 50000)
                                   .OrderByDescending(c => c.salary)
                                   .ToList();

            return Ok(highSalaryClients);
        }

        [HttpGet("RetrieveByBalance")]
        public IActionResult GetByBalance()
        {
            var highBalanceClients = dbContext.ClientsInDatabase.Where(c => c.balance > 50000)
                .OrderByDescending(c => c.balance)
                .ToList();

            return Ok(highBalanceClients);
        }

        [HttpGet("RetrieveByCreationDate")]
        public IActionResult GetByDate([FromQuery] DateTime creationdate)
        {
            var clientsCreatedAfter = dbContext.ClientsInDatabase
                .Where(c => DateTime.Compare(DateTime.Parse(c.creationDate), creationdate) > 0)
                .ToList();

            return Ok(clientsCreatedAfter);
        }

        [HttpGet("RetrieveTheClientWithTheHighestSalary")]
        public IActionResult GetHighestSalary () 
        {

            var highestsalary = dbContext.ClientsInDatabase
                .OrderByDescending(c => c.salary)
                .FirstOrDefault();

            return Ok(highestsalary);
        }

        [HttpGet("RetrieveTheClientWithTheLowestSalary")]
        public IActionResult GetLowestSalary()
        {

            var lowestsalary = dbContext.ClientsInDatabase
                .OrderBy(c => c.salary)
                .FirstOrDefault();

            return Ok(lowestsalary);
        }

        [HttpGet("RetrieveTheClientWithTheHighestBalance")]
        public IActionResult GetHighestBalance()
        {

            var highestbalance = dbContext.ClientsInDatabase
                .OrderByDescending(c => c.balance)
                .FirstOrDefault();

            return Ok(highestbalance);
        }

        [HttpGet("RetrieveTheClientWithTheLowestBalance")]
        public IActionResult GetLowestBalance()
        {

            var lowestbalance = dbContext.ClientsInDatabase
                .OrderBy(c => c.salary)
                .FirstOrDefault();

            return Ok(lowestbalance);
        }
        [HttpGet("RetrieveOldestAccount")]
        public IActionResult GetOldestAccount()
        {
            var oldestAccount = dbContext.ClientsInDatabase
                    .OrderBy(client => DateTime.Parse(client.creationDate))
                    .FirstOrDefault();

            return Ok(oldestAccount);
        }
        [HttpGet("RetrieveNewestAccount")]
        public IActionResult GetNewestAccount() 
        {
            var newestAccount = dbContext.ClientsInDatabase
                .OrderByDescending(client => DateTime.Parse(client.creationDate))
                .FirstOrDefault();

            return Ok(newestAccount);
        }

        /*
        private async Task WriteToCsv(IEnumerable<Client> clients)
        {
            string filePath = "D:/Backend-Tasks/Task4/Clients.csv";

            if (!System.IO.File.Exists(filePath))
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<Client>();
                    csv.NextRecord();
                }
            }

            using (var writer = new StreamWriter(filePath, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var client in clients)
                {
                    csv.WriteRecord(client);
                    csv.NextRecord();
                }
            }
        }

        private async Task DeleteCsv(IEnumerable<Client> clients)
        {
            string filePath = "D:/Backend-Tasks/Task4/Clients.csv";

            using (var writer = new StreamWriter(filePath, false)) 
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(clients);
            }
        }
        */
       


            private async Task WriteToCsv(IEnumerable<Client> clients)
        {
            string filePath = "D:/Backend-Tasks/Task4/Clients.csv";

            try
            {
                CsvMutex.WaitOne(); 

                if (!System.IO.File.Exists(filePath))
                {
                    using (var writer = new StreamWriter(filePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<Client>();
                        csv.NextRecord();
                    }
                }

                using (var writer = new StreamWriter(filePath, true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var client in clients)
                    {
                        csv.WriteRecord(client);
                        csv.NextRecord();
                    }
                }
            }
            finally
            {
                CsvMutex.ReleaseMutex(); 
            }
        }

        private async Task DeleteCsv(IEnumerable<Client> clients)
        {
            string filePath = "D:/Backend-Tasks/Task4/Clients.csv";

            try
            {
                CsvMutex.WaitOne(); 

                using (var writer = new StreamWriter(filePath, false))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(clients);
                }
            }
            finally
            {
                CsvMutex.ReleaseMutex(); 
            }
        }

        private void LogRequestInfo(string actionName, object requestData)
        {
            var requestInfo = new
            {
                Action = actionName,
                Timestamp = DateTime.Now,
                Data = requestData
            };

            Log.Information("Request Info: {@RequestInfo}", requestInfo);
        }

    }
}
