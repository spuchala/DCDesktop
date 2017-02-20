using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class CustomReport
    {
        public List<Question> questions { get; set; }
        public Guid DayCareId { get; set; }
        public int KidId { get; set; }
        public string ParentEmail { get; set; }
        public string KidName { get; set; }
    }

    public class Question
    {
        public int id { get; set; }
        public string type { get; set; }
        public List<string> values { get; set; }
        public List<Option> options { get; set; }
        public List<string> answers { get; set; }
        public Guid CustomReportQuestionId { get; set; }
        public int trackId { get; set; }
        public string template { get; set; }
    }

    public class Option
    {
        public string text { get; set; }
        public string value { get; set; }
        public bool check { get; set; }
    }
}