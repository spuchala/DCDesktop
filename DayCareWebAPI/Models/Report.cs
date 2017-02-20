using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class Report
    {
        public List<Kid> Kids { get; set; }
        public string Day { get; set; }
        public string StartDay { get; set; }
        public string EndDay { get; set; }
        public string UserRole { get; set; }
        public Guid Id { get; set; }
    }
}