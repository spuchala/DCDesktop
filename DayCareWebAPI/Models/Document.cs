using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCareWebAPI.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string MimeType { get; set; }
        public Guid DayCareId { get; set; }
        public string Url { get; set; }
        public string DayCareName { get; set; }
        public List<string> EmailList { get; set; }
        public string Error { get; set; }
    }
}
