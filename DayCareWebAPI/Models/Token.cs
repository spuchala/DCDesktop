using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class Token
    {
        public string AccessToken { get; set; }
        public Guid Id { get; set; }
        public DateTime DateIssued { get; set; }
        public string Role { get; set; }
    }
}
