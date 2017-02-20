using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class DayCare : User
    {
        public Guid DayCareId { get; set; }
        public string DayCareName { get; set; }
        public List<Kid> Kids { get; set; }
        public Settings Settings { get; set; }
        public string Source { get; set; }
    }
}