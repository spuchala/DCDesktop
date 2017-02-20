using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class WhatsNew
    {
        public int Id { get; set; }
        public string Heading { get; set; }
        public string Details { get; set; }
        public string ImagePath { get; set; }
    }
}
