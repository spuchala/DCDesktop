using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class SmallUser
    {
        public int KidId { get; set; }
        public string Name { get; set; }
        public bool HasClass { get; set; }
        public string Sex { get; set; }
        public string Avatar { get; set; }
    }
}