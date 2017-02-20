using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class Settings
    {
        public Guid DayCareId { get; set; }
        public string DayCareName { get; set; }
        public bool CustomReport { get; set; }
        public bool MakePublic { get; set; }
        public bool SettingsVisited { get; set; }
        public bool GigglesWareReport { get; set; }
        public bool EmailOnRegisterKid { get; set; }
        public string Language { get; set; }
    }
}