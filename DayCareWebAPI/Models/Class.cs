using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class Class
    {
        public Guid ClassId { get; set; }
        public Guid DayCareId { get; set; }
        public string ClassName { get; set; }
        public List<SmallUser> Kids { get; set; }
        public int NoOfKids { get; set; }
        public string Error { get; set; }
    }
}