using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class User
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string Sex { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Error { get; set; }
        public string Role { get; set; }
        public Guid Id { get; set; }
    }
}