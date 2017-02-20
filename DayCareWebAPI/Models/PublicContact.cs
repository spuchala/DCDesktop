using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class PublicContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string To { get; set; }
        public string Phone { get; set; }
        public string Comments { get; set; }
        public Guid DayCareId { get; set; }
    }
}
