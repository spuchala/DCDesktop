using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class InstantLog
    {
        public int? LogId { get; set; }
        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        public int MessageId { get; set; }
        public int? LogId { get; set; }
        public int KidId { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
        public string Error { get; set; }
    }
}
