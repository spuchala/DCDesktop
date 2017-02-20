using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class Schedule
    {
        public Schedule()
        {
            Messages = new List<ScheduleMessage>();            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<ScheduleMessage> Messages { get; set; }
        public Guid DayCareId { get; set; }
        public List<string> EmailList { get; set; }
        public string Url { get; set; }
        public string DayCareName { get; set; }
        public string Error { get; set; }
    }

    public class ScheduleMessage
    {
        public int MessageId { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
