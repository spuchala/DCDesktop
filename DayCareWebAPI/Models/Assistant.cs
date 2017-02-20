using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class Assistant
    {
        public string Subject { get; set; }
        public string Predicate { get; set; }
        public string Object { get; set; }
        public string Possession { get; set; }
        public string Time { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public Guid DayCareId { get; set; }
        public bool Analyzed { get; set; }
        public bool IsCommand { get; set; }
        public int KidId { get; set; }
        public string DestUrl { get; set; }
        public string SuppliesNeeded { get; set; }
    }
}
