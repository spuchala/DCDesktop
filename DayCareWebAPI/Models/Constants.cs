using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DayCareWebAPI.Models
{
    public class Constants
    {
        public const string Error = "Sorry! something went wrong";
        public const string DayCareRole = "dayCare";
        public const string ParentRole = "parent";
        public const char Sep = '^';
        public const string SepString = "^";
        public const string SanityVal = "n/a";
        public const string DeSanityVal = "";
        public const string UnDefined = "undefined";
        public const string EnglishLang = "en-us";
        public const string SpanishLang = "es";
        public const string CodeChange = "A new version is available";
        public const string NotAvl = "N/A";

        public const string FromUserId = "gigglesware@gmail.com";
        //public const string WebUrl = "http://localhost:55285/app/index.html";
        public const string WebUrl = "https://www.gigglesware.com";
        //public const string WebUrl = "http://localhost:51653";
        public const string DocumentsUrl = "/api/document/DownloadDoc/";
        public const string RegisterParentUrl = WebUrl + "/registerParent";
        public const string LoginUrl = WebUrl + "/login";
        public const string EditKidUrlForMobileApp = "/admin/edit/";
        public const string GmailSmtpServer = "smtp.gmail.com";
        public const int GmailSmtpPort = 587;
        public const string GmailSmtpUserId = "gigglesware@gmail.com";
        public const string GmailSmtpPassword = "Hyderabad_123";
        public const string HeaderForEmail = "<h2 style='color: #000000; font-family: Helvetica, Arial, sans-serif; font-size: 24px; font-weight: normal; line-height: 24px; margin-top: 20px; margin-right: 0; margin-bottom: 20px; margin-left: 20px; padding-top: 0; padding-right: 0; padding-bottom: 0; padding-right: 0;'>{0}</h2>";
        public const string ParaForEmail = "<p style='color: #000000; font-family: Georgia, Times, serif; font-size: 15px; line-height: 22px; margin-top: 0; margin-right: 10px; margin-bottom: 20px; margin-left: 20px; padding-top: 0; padding-right: 0; padding-bottom: 0; padding-right: 0;'>{0}</p>";

        public static string[] Foods = { "meal", "ate", "eat", "munch", "munched", "break fast", "breakfast", "lunch", "bottle", "food", "fed", "feed", "snack", "bottles", "chew", "fluids", "fluid", "formula", "juice", "oz", "oz.", "milk", "cracker", "crackers", "carrots", "dip", "sandwich", "pretzel", "pretzels", "cheese", "breast" };
        public static string[] Naps = { "sleep", "slept", "nap", "naps", "napped", "awoke", "awake", "rest", "doze", "bed", "crib" };
        public static string[] Pottys = { "potty", "diaper", "diapers", "dry", "wet", "toilet", "bathroom", "loose", "poop", "poops", "poopy", "pee", "poo poo", "chew", "bowel", "hard" };
        public static string[] Activities = { "lesson", "work", "worked", "activity", "activities", "play", "played", "outside", "swing", "floor", "bouncy", "video", "videos", "bumbo", "Exersaucer", "bouncer", "exersaucer", "book", "books", "read", "tummy", "art", "science", "nature", "learn", "learned", "learnt" };
        public static string[] Moods = { "mood", "happy", "fussy", "fussiness", "cranky", "bloody", "period", "periods", "sad", "cry", "cried", "grouchy", "sleepy", "crying", "good", "playful", "talkative" };
        public static string[] Sick = { "runny", "nose", "eye", "throat", "ear", "ears", "eyes", "pink", "cough", "cold", "diarrhea", "congestion", "temperature", "rash", "bruise", "bruises", "accident", "sick", "problem", "problems", "concern", "concerns", "well", "fever" };

        public const string Food = "Food";
        public const string Nap = "Nap";
        public const string Potty = "Potty";
        public const string Activity = "Activity";
        public const string Mood = "Mood";
        public const string Sicks = "Sick";
        public const string Misc = "Misc";

        public const string Delete = "delete";
        public const string Edit = "edit";
        public const string Update = "update";
        public const string Insert = "insert";
        public const string Add = "add";
        public const string Email = "email";

        public enum QuestionTypes
        {
            Heading = 1,
            QuestionAndAnswer = 2,
            PairedQuestionAndAnswer = 3,
            QuestionOptions = 4,
            QuestionAnswerFollowedByOptions = 5
        }

        public const string HeadingType = "Heading";
        public const string HeadingTemplate = "<button ng-click=\"removeQuestion()\"><span class=\"fa fa-trash-o fa-1\"></span></button>&nbsp;<input placeholder=\"type heading here\" class=\"textwithoutBorder\" ng-model=\"myDirectiveVar[0]\"></input><br/><br/>";

        public const string QAType = "Question & Answer";
        public const string QATemplate = "<div class=\"form - inline\"><button ng-click=\"removeQuestion()\"><span class=\"fa fa-trash-o fa-1\"></span></button>&nbsp;<input placeholder=\"type question here\" class=\"input - small\" ng-model=\"myDirectiveVar[0]\"></input>&nbsp;<b>:</b>&nbsp;<input type=\"text\" class=\"input - small\" data-ng-disabled=\"true\" placeholder=\"Answer\"></input></div></div><br/>";

        public const string PairedQAType = "Paired Question & Answer";
        public const string PairedQATemplate = "<div class=\"form - inline\"><button ng-click=\"removeQuestion()\"><span class=\"fa fa-trash-o fa-1\"></span></button>&nbsp;<input placeholder=\"question 1\" class=\"input - small\" ng-model=\"myDirectiveVar[0]\"></input>&nbsp;<b>:</b>&nbsp;<input type=\"text\" class=\"input - small\" data-ng-disabled=\"true\" placeholder=\"Answer\"/>&nbsp;<input placeholder=\"question 2\" class=\"input - small\" ng-model=\"myDirectiveVar[1]\"></input>&nbsp;<b>:</b>&nbsp;<input type=\"text\" class=\"input - small\" data-ng-disabled=\"true\" placeholder=\"Answer\"/></div><br/>";

        public const string QOptionsType = "Question & Options";
        public const string QOptionsTypeTemplate = "<div class=\"form - inline\"><button ng-click=\"removeQuestion()\"><span class=\"fa fa-trash-o fa-1\"></span></button>&nbsp;<input placeholder=\"question\" class=\"input - small\" ng-model=\"myDirectiveVar[0]\"></input><span ng-repeat=\"option in customQuesOptions\"><input placeholder=\"{{option.text }}\" ng-model=\"option.value\" class=\"input-small\"></input>&nbsp;</span><button ng-click=\"addOption()\"><span class=\"fa fa-plus-circle fa-1\"></span></button><button ng-click=\"removeOption()\"><span class=\"fa fa-minus-circle fa-1\"></span></button></div><br/>";

        public const string QAnsOptionsType = "Question & Answer followed by Options";
        public const string QAnsOptionsTypeTemplate = "<div class=\"form - inline\"><button ng-click=\"removeQuestion()\"><span class=\"fa fa-trash-o fa-1\"></span></button>&nbsp;<input placeholder=\"question\" class=\"input - small\" ng-model=\"myDirectiveVar[0]\"></input>&nbsp;<b>:</b>&nbsp;<input type=\"text\" class=\"input - small\" data-ng-disabled=\"true\" placeholder=\"Answer\"/>&nbsp;<span ng-repeat=\"option in customQuesOptions\"><input placeholder=\"{{option.text }}\" data-ng-disabled=\"option.disabled\" ng-model=\"option.value\" class=\"input-small\"></input>&nbsp;</span><button ng-click=\"addOption()\"><span class=\"fa fa-plus-circle fa-1\"></span></button><button ng-click=\"removeOption()\"><span class=\"fa fa-minus-circle fa-1\"></span></button></div><br/>";

        //Alexa Constants
        public const string LogActionIntent = "LogAction";
        public const string RequestLogIntent = "RequestLog";
        public const string AddChildIntent = "AddChild";
        public const string DayStatusIntent = "DayStatus";
        public const string HelpIntent = "HelpIntent";
        public const string StopIntent = "StopIntent";
        public const string CancelIntent = "CancelIntent";
        public const string NameSlot = "Name";
        public const string ActionSlot = "Action";
        public const string DurationSlot = "Duration";
        public const string TimeSlot = "Time";
        public const string FromTimeSlot = "FromTime";
        public const string ToTimeSlot = "ToTime";
        public const string DateSlot = "Date";
        public const string DecorationSlot = "Decoration";
        public const string SuppliesSlot = "Supplies";

        //avatar constants
        public const string BoyUrl = "images/avatars/boy-{0}.png";
        public const string GirlUrl = "images/avatars/girl-{0}.png";
        public const string UniUrl = "images/avatars/uni-{0}.png";
        public const string LogoUrl = "https://gigglesware.com/ftp/logos/{0}/";
        public const string SnapUrl = "https://gigglesware.com/ftp/snaps/{0}/";

        //schedules paths
        public const string ScheduleUrl = "http://go.gigglesware.com/DayCares/{0}/Schedule/{1}";

        //ftp details
        public const string FtpIp = "104.168.144.202";
        public const string FtpUser = "Administrator";
        public const string FtpPswd = "P4gEPKgchKsMJxU9";
        public const string FtpDocumentsPath = "ftp://" + FtpIp + "/documents/{0}";
        public const string FtpSnapsPath = "ftp://" + FtpIp + "/snaps/{0}";
        public const string FtpLogosPath = "ftp://" + FtpIp + "/logos/{0}";
    }
}