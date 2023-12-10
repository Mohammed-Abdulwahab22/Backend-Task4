using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations;

namespace Task4.Models
{
 
    public class Client
    {
        
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float salary { get; set; }
        public float balance { get; set; }
        public string creationDate { get; set; }

       
    }
}
