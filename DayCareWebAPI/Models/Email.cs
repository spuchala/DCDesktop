using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class Email
    {
        public Guid From { get; set; }
        public List<int> To { get; set; }
    }
}