using DayCareWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace DayCareWebAPI.Repository
{
    public class DayCareRepository
    {
        DataSet _ds;
        readonly string _conn = ConfigurationManager.ConnectionStrings["DayCare"].ConnectionString;

        public Kid InsertKid(Kid kid)
        {
            const string sp = "InsertKid";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Name", kid.FName + "," + kid.LName);
            cmd.Parameters.AddWithValue("@Sex", kid.Sex);
            cmd.Parameters.AddWithValue("@DOB", kid.DOB);
            cmd.Parameters.AddWithValue("@Address", kid.Address);
            cmd.Parameters.AddWithValue("@GuardianName", kid.GuardianName);
            cmd.Parameters.AddWithValue("@Email", kid.Email);
            cmd.Parameters.AddWithValue("@Phone", kid.Phone);
            cmd.Parameters.AddWithValue("@ClassName", kid.ClassName);
            cmd.Parameters.AddWithValue("@ClassId", kid.ClassId == Guid.Empty ? null : kid.ClassId.ToString());
            cmd.Parameters.AddWithValue("@Allergies", kid.Allergies);
            cmd.Parameters.AddWithValue("@DayCareId", kid.DayCareId);
            cmd.Parameters.AddWithValue("@Avatar", string.IsNullOrEmpty(kid.Avatar) ? null : kid.Avatar);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                kid.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                kid.KidId = Convert.ToInt32(_ds.Tables[0].Rows[0]["KidId"].ToString());
            else
                kid.Error = Constants.Error;
            return kid;
        }

        public Kid InsertKidShort(Kid kid)
        {
            const string sp = "InsertKidShort";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Name", kid.FName + "," + kid.LName);
            cmd.Parameters.AddWithValue("@DayCareId", kid.DayCareId);
            if (kid.ClassId != null && kid.ClassId != Guid.Empty)
                cmd.Parameters.AddWithValue("@ClassId", kid.ClassId);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                kid.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                kid.KidId = Convert.ToInt32(_ds.Tables[0].Rows[0]["KidId"].ToString());
            else
                kid.Error = Constants.Error;
            return kid;
        }

        public Kid UpdateKid(Kid kid)
        {
            const string sp = "UpdateKid";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Name", kid.FName + "," + kid.LName);
            cmd.Parameters.AddWithValue("@Sex", kid.Sex);
            cmd.Parameters.AddWithValue("@DOB", kid.DOB);
            cmd.Parameters.AddWithValue("@Address", kid.Address);
            cmd.Parameters.AddWithValue("@GuardianName", kid.GuardianName);
            cmd.Parameters.AddWithValue("@Email", kid.Email);
            cmd.Parameters.AddWithValue("@Phone", kid.Phone);
            cmd.Parameters.AddWithValue("@ClassName", kid.ClassName);
            cmd.Parameters.AddWithValue("@ClassId", kid.ClassId == Guid.Empty ? null : kid.ClassId.ToString());
            cmd.Parameters.AddWithValue("@Allergies", kid.Allergies);
            cmd.Parameters.AddWithValue("@DayCareId", kid.DayCareId);
            cmd.Parameters.AddWithValue("@KidId", kid.KidId);
            cmd.Parameters.AddWithValue("@Avatar", kid.Avatar);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                kid.Error = response;
            return kid;
        }

        public string SaveCustomReport(CustomReport customReport)
        {
            var response = string.Empty;
            foreach (var question in customReport.questions)
            {
                const string sp = "SaveCustomReport";
                var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
                cmd.Parameters.AddWithValue("@DayCareId", customReport.DayCareId);
                cmd.Parameters.AddWithValue("@KidId", customReport.KidId);
                cmd.Parameters.AddWithValue("@CustomReportQuestionId", question.CustomReportQuestionId);
                cmd.Parameters.AddWithValue("@InputAnswers", (question.answers != null && question.answers.Any()) ?
                    string.Join(Constants.SepString, question.answers) : null);
                cmd.Parameters.AddWithValue("@OptionAnswers", (question.options != null && question.options.Any()) ?
                    string.Join(Constants.SepString, question.options.Select(e => e.check).ToList()) : null);
                response = ExecuteReader(cmd);
                if (response != string.Empty)
                    break;
            }
            return response;
        }

        public string SaveSettings(Settings settings)
        {
            const string sp = "SaveSettings";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", settings.DayCareId);
            cmd.Parameters.AddWithValue("@CustomReport", settings.CustomReport);
            cmd.Parameters.AddWithValue("@GigglesWareReport", settings.GigglesWareReport);
            cmd.Parameters.AddWithValue("@MakePublic", settings.MakePublic);
            cmd.Parameters.AddWithValue("@EmailOnRegisterKid", settings.EmailOnRegisterKid);
            cmd.Parameters.AddWithValue("@Language", string.IsNullOrEmpty(settings.Language) ? Constants.EnglishLang : settings.Language);
            cmd.Parameters.AddWithValue("@SettingsVisited", true);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string RemoveKid(int kidId, Guid dayCareId, string InActiveReason)
        {
            const string sp = "DeleteKid";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@InActiveReason", InActiveReason);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string RemoveNotification(Guid dayCareId, int Id)
        {
            const string sp = "MarkNotitificationRead";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@Id", Id);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string RemoveWhatsNew(Guid dayCareId, int Id)
        {
            const string sp = "MarkNewsRead";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@Id", Id);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string RemoveSchedule(Guid dayCareId, int scheduleId)
        {
            const string sp = "RemoveSchedule";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string RemoveScheduleMessage(Guid dayCareId, int scheduleId, int messageId)
        {
            const string sp = "RemoveScheduleMessage";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            cmd.Parameters.AddWithValue("@MessageId", messageId);
            var response = ExecuteReader(cmd);
            return response;
        }

        public Document InsertDocument(Document doc)
        {
            const string sp = "InsertDocument";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", doc.DayCareId);
            cmd.Parameters.AddWithValue("@Type", doc.Type);
            cmd.Parameters.AddWithValue("@Name", doc.Name);
            cmd.Parameters.AddWithValue("@MimeType", doc.MimeType);
            cmd.Parameters.AddWithValue("@Title", doc.Title);
            var response = ExecuteReader(cmd);
            return response == string.Empty ? doc : null;
        }

        public string DeleteDocument(int docId)
        {
            const string sp = "DeleteDocument";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Id", docId);
            var response = ExecuteReader(cmd);
            return response;
        }

        public string InsertCustomReportQuestions(Guid customReportId, Guid dayCareId, int id, string questionType, string values, string options)
        {
            const string sp = "CreateCustomReportQuestions";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@CustomReportId", customReportId);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@QuestionType", questionType);
            cmd.Parameters.AddWithValue("@Values", values);
            cmd.Parameters.AddWithValue("@Options", !string.IsNullOrEmpty(options) ? options : null);
            var response = ExecuteReader(cmd);
            return response;
        }

        public CustomReport GetCustomReport(Guid dayCareId, int kidId, string day)
        {
            const string sp = "GetCustomReport";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapCustomReport(_ds.Tables[0]);
            return null;
        }

        public CustomReport GetCustomReportOnADay(Guid dayCareId, int kidId, string day)
        {
            const string sp = "GetCustomReportOnADay";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapCustomReport(_ds.Tables[0]);
            return null;
        }

        private CustomReport MapCustomReport(DataTable dt)
        {
            var customReport = new CustomReport();
            customReport.questions = new List<Question>();
            if (dt.Columns.Contains("Email") && !string.IsNullOrEmpty(dt.Rows[0]["Email"].ToString()))
                customReport.ParentEmail = dt.Rows[0]["Email"].ToString();
            if (dt.Columns.Contains("Name") && !string.IsNullOrEmpty(dt.Rows[0]["Name"].ToString()))
                customReport.KidName = dt.Rows[0]["Name"].ToString();
            var i = 1;
            foreach (DataRow dr in dt.Rows)
            {
                var question = new Question();
                question.trackId = i;
                question.id = Convert.ToInt32(dr["Id"]);
                question.type = dr["QuestionType"].ToString();
                question.CustomReportQuestionId = new Guid(dr["CustomReportQuestionId"].ToString());
                //get questions
                question.values = new List<string>();
                question.values = GetCustReportQuestionValues(dr["Values"].ToString(), question.id);
                //get options for multiple choice
                if ((question.id == (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions ||
                        question.id == (int)Constants.QuestionTypes.QuestionOptions)
                        && !string.IsNullOrEmpty(dr["Options"].ToString()))
                {
                    question.options = new List<Option>();
                    GetCustReportQuestionOptions(dr["Options"].ToString(), question.options);
                }
                //get answers for questions
                if (question.id != (int)Constants.QuestionTypes.QuestionOptions && question.id != (int)Constants.QuestionTypes.Heading)
                {
                    question.answers = new List<string>();
                    GetCustReportQuestionAnswers(dr["InputAnswers"].ToString(), question.answers, question.id);
                }
                //get answers for multiple choice questions
                if ((question.id == (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions ||
                        question.id == (int)Constants.QuestionTypes.QuestionOptions)
                        && !string.IsNullOrEmpty(dr["OptionAnswers"].ToString()))
                    GetCustReportQuestionOptionAnswers(dr["OptionAnswers"].ToString(), question.options);
                customReport.questions.Add(question);
                i++;
            }
            return customReport;
        }


        private List<string> GetCustReportQuestionValues(string dr, int id)
        {
            var response = new List<string>();
            var list = dr.Split(Constants.Sep);
            for (int i = 0; i < list.Length; i++)
            {
                response.Add(list[i]);
            }
            return response;
        }

        private void GetCustReportQuestionOptions(string dr, List<Option> options)
        {
            var list = dr.Split(Constants.Sep);
            foreach (var item in list)
                options.Add(new Option() { check = false, value = item });
        }

        private void GetCustReportQuestionAnswers(string dr, List<string> answers, int questionId)
        {
            if (string.IsNullOrEmpty(dr))
            {
                if (questionId == (int)Constants.QuestionTypes.QuestionAndAnswer)
                    answers.Add(string.Empty);
                else if (questionId == (int)Constants.QuestionTypes.PairedQuestionAndAnswer)
                {
                    answers.Add(string.Empty);
                    answers.Add(string.Empty);
                }
                else if (questionId == (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions)
                    answers.Add(string.Empty);
            }
            else
            {
                var list = dr.Split(Constants.Sep);
                for (int i = 0; i < list.Length; i++)
                    answers.Add(list[i]);
            }
        }

        private void GetCustReportQuestionOptionAnswers(string dr, List<Option> options)
        {
            var list = dr.Split(Constants.Sep);
            if (list != null && list.Any())
            {
                for (int i = 0; i < list.Length; i++)
                {
                    options[i].check = Convert.ToBoolean(list[i]);
                }
            }
        }

        public bool CheckCustomReportExists(Guid dayCareId)
        {
            const string sp = "CheckCustomReportExists";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return true;
            return false;
        }

        public InstantLog GetInstantLog(int kidId, string day)
        {
            const string sp = "GetInstantLog";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@day", day);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapInstantLog(_ds.Tables[0], kidId);
            return null;
        }

        public List<Schedule> GetSchedules(Guid dayCareId)
        {
            const string sp = "GetSchedules";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapSchedules(_ds.Tables[0]);
            return null;
        }

        public Schedule GetSchedule(Guid dayCareId, int scheduleId)
        {
            const string sp = "GetSchedule";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapSchedule(_ds.Tables[0]);
            return null;
        }

        public DayCareInfo GetDayCareInfo(Guid dayCareId)
        {
            const string sp = "GetDayCareInfo";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapDayCareInfo(_ds.Tables[0], dayCareId);
            else
                return new DayCareInfo()
                {
                    DayCareId = dayCareId,
                    DescriptionHome = string.Empty,
                    DescriptionAboutUs = string.Empty,
                    DescriptionProgram = string.Empty,
                    Snaps = GetDayCareInfoImages(dayCareId)
                };
        }

        public List<DayCareSnap> GetDayCareInfoImages(Guid dayCareId)
        {
            const string sp = "GetDayCareInfoImages";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapDayCareInfoImages(_ds.Tables[0], dayCareId);
            return null;
        }

        private DayCareInfo MapDayCareInfo(DataTable dt, Guid dayCareId)
        {
            var dayCareInfo = new DayCareInfo();
            dayCareInfo.DescriptionHome = dt.Rows[0]["DescriptionHome"].ToString();
            dayCareInfo.DescriptionAboutUs = dt.Rows[0]["DescriptionAboutUs"].ToString();
            dayCareInfo.DescriptionProgram = dt.Rows[0]["DescriptionProgram"].ToString();
            dayCareInfo.DayCareId = dayCareId;
            if (!string.IsNullOrEmpty(dt.Rows[0]["Logo"].ToString()))
                string.Format(Constants.LogoUrl, dt.Rows[0]["Logo"].ToString());
            dayCareInfo.Snaps = GetDayCareInfoImages(dayCareId);
            return dayCareInfo;
        }

        private List<DayCareSnap> MapDayCareInfoImages(DataTable dt, Guid dayCareId)
        {
            var snaps = new List<DayCareSnap>();
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(dr["SnapId"].ToString()))
                {
                    var snap = new DayCareSnap();
                    snap.SnapId = Convert.ToInt32(dr["SnapId"].ToString());
                    if (!string.IsNullOrEmpty(dr["SnapTitle"].ToString()))
                        snap.SnapUrl = string.Format(Constants.SnapUrl, dayCareId) + (!dr["SnapTitle"].ToString().Contains(".") ? (dr["SnapTitle"].ToString() + ".jpeg") : dr["SnapTitle"].ToString());
                    snaps.Add(snap);
                }
            }
            return snaps;
        }

        private Schedule MapSchedule(DataTable dt)
        {
            var schedule = new Schedule();
            schedule.Id = Convert.ToInt32(dt.Rows[0]["ScheduleId"].ToString());
            schedule.Name = dt.Rows[0]["Name"].ToString();
            schedule.DayCareName = dt.Rows[0]["DayCareName"].ToString();
            schedule.DayCareId = new Guid(dt.Rows[0]["DayCareId"].ToString());
            schedule.Url = string.Format(Constants.ScheduleUrl, schedule.DayCareName.Replace(" ", "-"), schedule.Name.Replace(" ", "-"));
            foreach (DataRow dr in dt.Rows)
            {
                var message = new ScheduleMessage();
                message.MessageId = Convert.ToInt32(dr["MessageId"].ToString());
                message.Message = dr["Message"].ToString();
                message.Time = dr["Time"].ToString();
                message.From = message.Time.Split('-')[0];
                message.To = message.Time.Split('-')[1];
                schedule.Messages.Add(message);
            }
            return schedule;
        }

        private List<Schedule> MapSchedules(DataTable dt)
        {
            var schedules = new List<Schedule>();
            foreach (DataRow dr in dt.Rows)
            {
                var schedule = new Schedule();
                schedule.Id = Convert.ToInt32(dr["ScheduleId"].ToString());
                schedule.Name = dr["Name"].ToString();
                schedule.DayCareName = dr["DayCareName"].ToString();
                schedule.Url = string.Format(Constants.ScheduleUrl, schedule.DayCareName.Replace(" ", "-"), schedule.Name.Replace(" ", "-"));
                if (!schedules.Any(sch => sch.Id == schedule.Id))
                    schedules.Add(schedule);
            }
            foreach (var schedule in schedules)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (schedule.Id == Convert.ToInt32(dr["ScheduleId"].ToString()))
                    {
                        var message = new ScheduleMessage();
                        message.MessageId = Convert.ToInt32(dr["MessageId"].ToString());
                        message.Message = dr["Message"].ToString();
                        message.Time = dr["Time"].ToString();
                        message.From = message.Time.Split('-')[0];
                        message.To = message.Time.Split('-')[1];
                        schedule.Messages.Add(message);
                    }
                }
            }
            return schedules.Any() ? schedules : null;
        }

        private InstantLog MapInstantLog(DataTable dt, int kidId)
        {
            var instantLog = new InstantLog();
            instantLog.LogId = Convert.ToInt32(dt.Rows[0]["LogId"].ToString());
            instantLog.Messages = new List<Message>();
            foreach (DataRow dr in dt.Rows)
            {
                var message = new Message();
                message.Type = dr["Type"].ToString();
                message.Value = dr["Message"].ToString();
                message.Time = dr["Time"].ToString();
                message.MessageId = Convert.ToInt32(dt.Rows[0]["MessageId"].ToString());
                instantLog.Messages.Add(message);
                message.KidId = kidId;
                message.LogId = Convert.ToInt32(dr["LogId"].ToString());
            }
            return instantLog;
        }

        public Message InsertInstantLogMessage(Message message, int kidId, int? logId)
        {
            const string sp = "InsertInstantLogMessage";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@LogId", logId.HasValue ? logId.Value : (int?)null);
            cmd.Parameters.AddWithValue("@Message", message.Value);
            cmd.Parameters.AddWithValue("@Type", message.Type);
            cmd.Parameters.AddWithValue("@Time", message.Time);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                message.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                message.LogId = Convert.ToInt32(_ds.Tables[0].Rows[0]["LogId"].ToString());
                message.MessageId = Convert.ToInt32(_ds.Tables[0].Rows[0]["MessageId"].ToString());
            }
            else
                message.Error = Constants.Error;
            return message;
        }

        public string SaveScheduleMessage(ScheduleMessage message, Guid dayCareId)
        {
            var error = string.Empty;
            const string sp = "SaveScheduleMessage";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@MessageId", message.MessageId);
            cmd.Parameters.AddWithValue("@Time", message.From + "-" + message.To);
            cmd.Parameters.AddWithValue("@Message", message.Message);
            error = ExecuteReader(cmd);
            return error;
        }

        public string ManageDayCareInfo(DayCareInfo info)
        {
            var error = string.Empty;
            const string sp = "ManageDayCareInfo";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", info.DayCareId);
            cmd.Parameters.AddWithValue("@Home", info.DescriptionHome);
            cmd.Parameters.AddWithValue("@AboutUs", info.DescriptionAboutUs);
            cmd.Parameters.AddWithValue("@Program", info.DescriptionProgram);
            error = ExecuteReader(cmd);
            return error;
        }

        public string ManageDayCareInfoHome(DayCareInfo info)
        {
            var error = string.Empty;
            const string sp = "ManageDayCareInfoHome";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", info.DayCareId);
            cmd.Parameters.AddWithValue("@Home", info.DescriptionHome);
            error = ExecuteReader(cmd);
            return error;
        }

        public string ManageDayCareInfoAboutUs(DayCareInfo info)
        {
            var error = string.Empty;
            const string sp = "ManageDayCareInfoAbout";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", info.DayCareId);
            cmd.Parameters.AddWithValue("@AboutUs", info.DescriptionAboutUs);
            error = ExecuteReader(cmd);
            return error;
        }

        public string ManageDayCareInfoProgram(DayCareInfo info)
        {
            var error = string.Empty;
            const string sp = "ManageDayCareInfoProgram";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", info.DayCareId);
            cmd.Parameters.AddWithValue("@Program", info.DescriptionProgram);
            error = ExecuteReader(cmd);
            return error;
        }

        public DayCareSnap InsertDayCareSnap(Guid dayCareId, string snapTitle)
        {
            const string sp = "InsertSnapInfo";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@SnapTitle", snapTitle);
            var response = ExecuteReader(cmd);
            var snap = new DayCareSnap();
            if (response != string.Empty)
                snap.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                snap.SnapId = Convert.ToInt32(_ds.Tables[0].Rows[0]["SnapId"]);
                snap.SnapUrl = string.Format(Constants.SnapUrl, dayCareId) + snapTitle;
            }
            else
                snap.Error = Constants.Error;
            return snap;
        }

        public string InsertPublicContact(PublicContact contact)
        {
            var error = string.Empty;
            const string sp = "InsertPublicContact";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", contact.DayCareId);
            cmd.Parameters.AddWithValue("@Name", contact.FirstName + "," + contact.LastName);
            cmd.Parameters.AddWithValue("@Email", contact.Email);
            cmd.Parameters.AddWithValue("@Phone", contact.Phone);
            cmd.Parameters.AddWithValue("@Comments", contact.Comments);
            error = ExecuteReader(cmd);
            return error;
        }

        public Schedule InsertSchedule(Schedule schedule)
        {
            const string sp = "InsertSchedule";
            var messages = new List<string>();
            var times = new List<string>();
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", schedule.DayCareId);
            cmd.Parameters.AddWithValue("@Name", schedule.Name);
            if (schedule.Messages.Any())
            {
                foreach (var item in schedule.Messages)
                {
                    messages.Add(item.Message);
                }
                foreach (var item in schedule.Messages)
                {
                    times.Add(item.Time);
                }
            }
            cmd.Parameters.AddWithValue("@Message", messages.Any() ? string.Join("^", messages) : null);
            cmd.Parameters.AddWithValue("@Time", times.Any() ? string.Join("^", times) : null);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                schedule.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                schedule = new Schedule();
                schedule = MapSchedule(_ds.Tables[0]);
            }
            else
                schedule.Error = Constants.Error;
            return schedule;
        }

        public Parent InsertParent(Parent parent)
        {
            const string sp = "InsertParent";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Name", parent.FName + "," + parent.LName);
            cmd.Parameters.AddWithValue("@Email", parent.Email);
            cmd.Parameters.AddWithValue("@Password", parent.Password);
            cmd.Parameters.AddWithValue("@Phone", parent.Phone);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                parent.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                parent.ParentId = new Guid(_ds.Tables[0].Rows[0]["ParentId"].ToString());
            else
                parent.Error = Constants.Error;
            return parent;
        }

        public DayCare InsertDayCare(DayCare dayCare)
        {
            const string sp = "InsertDayCareAdmin";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareName", dayCare.DayCareName);
            cmd.Parameters.AddWithValue("@AdminName", dayCare.FName + "," + dayCare.LName);
            cmd.Parameters.AddWithValue("@Phone", dayCare.Phone);
            cmd.Parameters.AddWithValue("@Address", dayCare.Address);
            cmd.Parameters.AddWithValue("@Email", dayCare.Email);
            cmd.Parameters.AddWithValue("@Password", dayCare.Password);
            if (!string.IsNullOrEmpty(dayCare.Source))
                cmd.Parameters.AddWithValue("@Source", dayCare.Source);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                dayCare.Error = response;
            else
            {
                if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                    dayCare.DayCareId = new Guid(_ds.Tables[0].Rows[0]["DayCareId"].ToString());
                else
                    dayCare.Error = Constants.Error;
            }
            return dayCare;
        }

        public KidLog InsertKidLog(KidLog log)
        {
            const string sp = "InsertKidLog";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", log.KidId);
            var whatKidAte = string.Empty;
            var howKidAte = string.Empty;
            var whenKidAte = string.Empty;
            var anySnack = string.Empty;
            var diaperCheckTime = string.Empty;
            var pottyTime = string.Empty;
            var diaperPottyType = string.Empty;
            var napTime = string.Empty;
            var activityTime = string.Empty;
            var mood = string.Empty;
            var activities = string.Empty;
            for (var i = 0; i < log.Foods.Count; i++)
                whatKidAte += log.Foods[i].WhatKidAte + (log.Foods.Count > 1 && i != log.Foods.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Foods.Count; i++)
                howKidAte += log.Foods[i].HowKidAte + (log.Foods.Count > 1 && i != log.Foods.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Foods.Count; i++)
                whenKidAte += log.Foods[i].WhenKidAte + (log.Foods.Count > 1 && i != log.Foods.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Foods.Count; i++)
                anySnack += log.Foods[i].AnySnack + (log.Foods.Count > 1 && i != log.Foods.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Pottys.Count; i++)
                diaperCheckTime += log.Pottys[i].DiaperCheckTime + (log.Pottys.Count > 1 && i != log.Pottys.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Pottys.Count; i++)
                pottyTime += log.Pottys[i].PottyTime + (log.Pottys.Count > 1 && i != log.Pottys.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Pottys.Count; i++)
                diaperPottyType += log.Pottys[i].DiaperPottyType + (log.Pottys.Count > 1 && i != log.Pottys.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Naps.Count; i++)
                napTime += log.Naps[i].NapTime + (log.Naps.Count > 1 && i != log.Naps.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Activities.Count; i++)
                activityTime += log.Activities[i].ActivityTime + (log.Activities.Count > 1 && i != log.Activities.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Activities.Count; i++)
                mood += log.Activities[i].Mood + (log.Activities.Count > 1 && i != log.Activities.Count - 1 ? Constants.SepString : "");
            for (var i = 0; i < log.Activities.Count; i++)
                activities += log.Activities[i].Activities + (log.Activities.Count > 1 && i != log.Activities.Count - 1 ? Constants.SepString : "");
            cmd.Parameters.AddWithValue("@WhatKidAte", whatKidAte);
            cmd.Parameters.AddWithValue("@HowKidAte", howKidAte);
            cmd.Parameters.AddWithValue("@WhenKidAte", whenKidAte);
            cmd.Parameters.AddWithValue("@AnySnack", anySnack);
            cmd.Parameters.AddWithValue("@DiaperCheckTime", diaperCheckTime);
            cmd.Parameters.AddWithValue("@PottyTime", pottyTime);
            cmd.Parameters.AddWithValue("@DiaperPottyType", diaperPottyType);
            cmd.Parameters.AddWithValue("@NapTime", napTime);
            cmd.Parameters.AddWithValue("@ActivityTime", activityTime);
            cmd.Parameters.AddWithValue("@Mood", mood);
            cmd.Parameters.AddWithValue("@Activities", activities);
            cmd.Parameters.AddWithValue("@ProblemsConcerns", log.ProblemsConcerns);
            cmd.Parameters.AddWithValue("@SuppliesNeeded", log.SuppliesNeeded);
            cmd.Parameters.AddWithValue("@Comments", log.Comments);
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                log.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                log.LogId = new Guid(_ds.Tables[0].Rows[0]["LogId"].ToString());
                log.Name = _ds.Tables[0].Rows[0]["Name"].ToString();
            }
            else
                log.Error = Constants.Error;
            return log;
        }

        public DayCare GetDayCare(Guid dayCareId)
        {
            try
            {
                const string sp = "GetDayCare";
                var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
                cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
                ExecuteReader(cmd);
                if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                    return MapDayCare(_ds.Tables[0]);
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException.Message, "getting daycare data", dayCareId.ToString(), ex.StackTrace);
                return null;
            }
        }

        public Kid GetKid(int kidId, Guid dayCareId)
        {
            const string sp = "GetKid";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                var rnd = new Random();
                return MapKid(_ds.Tables[0].Rows[0], rnd);
            }
            else
                return null;
        }

        public User LoginUser(string email, string password)
        {
            const string sp = "LoginUser";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", password);
            var user = new User();
            var response = ExecuteReader(cmd);
            if (response != string.Empty)
                user.Error = response;
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapUser(_ds.Tables[0].Rows[0], user);
            else
                return null;
        }

        public List<Kid> GetKidsFromDayCare(Guid dayCareId)
        {
            const string sp = "GetKidsFromDayCare";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapKids(_ds.Tables[0]);
            else
                return null;
        }

        public List<Class> GetClassesForDayCare(Guid dayCareId)
        {
            const string sp = "GetClassesForDayCare";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapClasses(_ds.Tables[0]);
            else
                return null;
        }

        public List<SmallUser> GetKidsWithNoClass(Guid dayCareId)
        {
            const string sp = "GetKidsWithNoClass";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapSmallUsers(_ds.Tables[0]);
            else
                return null;
        }

        public List<Kid> GetKidsInClass(Guid classId)
        {
            const string sp = "GetKidsInClass";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@ClassId", classId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapKids(_ds.Tables[0]);
            else
                return null;
        }

        public List<Kid> GetKidsFromParent(Guid parentId)
        {
            const string sp = "GetKidsFromParent";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@ParentId", parentId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapKids(_ds.Tables[0]);
            else
                return null;
        }

        public List<Kid> GetDayCareReport(Guid dayCareId, string day)
        {
            const string sp = "GetDayCareReport";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            if (!string.IsNullOrEmpty(day))
                cmd.Parameters.AddWithValue("@Day", day);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                var kids = MapKids(_ds.Tables[0]);
                if (_ds.Tables[1] != null && _ds.Tables[1].Rows.Count > 0)
                {
                    if (_ds.Tables[1].Rows.Count == kids.Count)
                        for (var i = 0; i < kids.Count; i++)
                            kids[i].Log = MapLog(_ds.Tables[1].Rows[i]);
                    else
                    {
                        foreach (DataRow row in _ds.Tables[1].Rows)
                        {
                            foreach (var kid in kids)
                            {
                                if (kid.KidId == (int)row["KidId"])
                                    kid.Log = MapLog(row);
                            }
                        }
                    }
                }
                return kids;
            }
            return null;
        }

        public Parent GetParent(Guid parentId, string day)
        {
            const string sp = "GetParent";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@ParentId", parentId);
            if (!string.IsNullOrEmpty(day))
                cmd.Parameters.AddWithValue("@Day", day);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                var parent = MapParent(_ds.Tables[0].Rows[0]);
                if (_ds.Tables[1] != null && _ds.Tables[1].Rows.Count > 0)
                {
                    parent.Kids = MapKids(_ds.Tables[1]);
                    if (_ds.Tables[2] != null && _ds.Tables[2].Rows.Count > 0)
                        if (_ds.Tables[2].Rows.Count == parent.Kids.Count)
                            for (var i = 0; i < parent.Kids.Count; i++)
                                parent.Kids[i].Log = MapLog(_ds.Tables[2].Rows[i]);
                        else
                        {
                            foreach (DataRow row in _ds.Tables[2].Rows)
                            {
                                foreach (var kid in parent.Kids)
                                {
                                    if (kid.KidId == (int)row["KidId"])
                                        kid.Log = MapLog(row);
                                }
                            }
                        }
                }
                return parent;
            }
            return null;
        }

        public Guid? GetParentsDayCare(Guid parentId)
        {
            const string sp = "GetParentsDayCare";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@ParentId", parentId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                return new Guid(_ds.Tables[0].Rows[0]["DayCareId"].ToString());
            }
            return null;
        }

        public Parent CheckParentByEmail(string email)
        {
            const string sp = "CheckParentByEmail";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@EmailId", email);
            ExecuteReader(cmd);
            var parent = new Parent();
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                parent.IsRegistered = (bool)_ds.Tables[0].Rows[0]["IsRegistered"];
                parent.HasKidInSystem = (bool)_ds.Tables[0].Rows[0]["HasKidInSystem"];
                parent.Email = email;
                return parent;
            }
            return null;
        }

        public KidLog GetKidLogByLogId(Guid logId)
        {
            const string sp = "GetKidLogByLogId";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@LogId", logId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapLog(_ds.Tables[0].Rows[0]);
            else
                return null;
        }

        public KidLog GetKidLogOnADay(int kidId, Guid id, string day)
        {
            const string sp = "GetKidLogOnADay";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Day", day);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                //StringBuilder output = new StringBuilder();
                //foreach (DataRow rows in _ds.Tables[0].Rows)
                //{
                //    foreach (DataColumn col in _ds.Tables[0].Columns)
                //    {
                //        output.AppendFormat("{0} ", rows[col]);
                //    }
                //    output.AppendLine();
                //}
                //LogError("", "", "", output.ToString());
                return MapLog(_ds.Tables[0].Rows[0]);
            }
            else
                return null;
        }

        public List<KidLog> GetKidLogsInDayRange(int kidId, Guid dayCareId, DateTime startDay, DateTime endDay)
        {
            const string sp = "GetKidLogInDayRange";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@StartDay", startDay);
            cmd.Parameters.AddWithValue("@EndDay", endDay);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapLogs(_ds.Tables[0]);
            else
                return null;
        }

        public List<Attendance> GetAttendedKids(Guid dayCareId)
        {
            const string sp = "GetAttendance";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapAttendees(_ds.Tables[0]);
            return null;
        }

        public Token GetTokenData(string accessToken)
        {
            const string sp = "GetAlexaToken";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Token", accessToken);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapTokenData(_ds.Tables[0].Rows[0]);
            return null;
        }

        public bool CheckAppVersion(string osType, string version)
        {
            const string sp = "GetAppVersion";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@OsType", osType);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return _ds.Tables[0].Rows[0]["Version"].ToString() != version;
            return true;
        }

        public Settings GetSettings(Guid dayCareId)
        {
            const string sp = "GetSettings";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapSettings(_ds.Tables[0].Rows[0]);
            return null;
        }

        public List<Notification> GetNotifications(Guid dayCareId)
        {
            const string sp = "GetNotifications";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapNotifications(_ds.Tables[0]);
            return null;
        }

        public List<WhatsNew> GetWhatsNew(Guid dayCareId)
        {
            const string sp = "GetWhatsNew";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapWhatsNew(_ds.Tables[0]);
            return null;
        }

        public List<WhatsNew> GetAllWhatsNew()
        {
            const string sp = "GetAllWhatsNew";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapWhatsNew(_ds.Tables[0]);
            return null;
        }

        public List<Document> GetDocuments(Guid dayCareId)
        {
            const string sp = "GetDocuments";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapDocuments(_ds.Tables[0]);
            return null;
        }

        public Document GetDocument(int id)
        {
            const string sp = "GetDocument";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Id", id);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapDocument(_ds.Tables[0].Rows[0]);
            return null;
        }

        public Assistant GetKidByName(Assistant input)
        {
            const string sp = "GetKidByName";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", input.DayCareId);
            cmd.Parameters.AddWithValue("@Name", input.Subject);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                input.Subject = _ds.Tables[0].Rows[0]["Name"].ToString();
                input.KidId = Convert.ToInt32(_ds.Tables[0].Rows[0]["KidId"].ToString());
            }
            return input;
        }

        public Kid GetKidForParentByName(Guid parentId, string name)
        {
            const string sp = "GetParentsKidByName";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@ParentId", parentId);
            cmd.Parameters.AddWithValue("@Name", name);
            ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
            {
                var rnd = new Random();
                return MapKid(_ds.Tables[0].Rows[0], rnd);
            }
            else
                return null;
        }

        public string LogAttendance(List<Attendance> kids)
        {
            var error = string.Empty;
            foreach (var kid in kids)
            {
                const string sp = "InsertAttendance";
                var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
                cmd.Parameters.AddWithValue("@DayCareId", kid.DayCareId);
                cmd.Parameters.AddWithValue("@KidId", kid.KidId);
                cmd.Parameters.AddWithValue("@HasAttended", kid.HasAttended);
                error = ExecuteReader(cmd);
                if (error != string.Empty)
                    break;
            }
            return error;
        }

        public void LogError(string errormessage, string errorAt, string errorBy, string trace)
        {
            var connection = new SqlConnection(_conn);
            string sp = "insert into ErrorLogs(error,errorat,errorby,createddate,trace) values('" + errormessage + "','" + errorAt + "','" + errorBy + "',getdate(),'" + trace + "')";
            var cmd = new SqlCommand(sp, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void LogAlexaSession(Guid dayCareId, string requestId, string sessionId, string requestType, string intent)
        {
            const string sp = "LogAlexaSession";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@RequestId", requestId);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            cmd.Parameters.AddWithValue("@Intent", intent);
            cmd.Parameters.AddWithValue("@RequestType", requestType);
            ExecuteReader(cmd);
        }

        public void InsertAlexaToken(Token token)
        {
            const string sp = "InsertAlexaToken";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Token", token.AccessToken);
            cmd.Parameters.AddWithValue("@Id", token.Id);
            cmd.Parameters.AddWithValue("@Role", token.Role);
            ExecuteReader(cmd);
        }

        public string AssignClassToKid(Class item)
        {
            const string sp = "AssignClassToKid";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", item.DayCareId);
            cmd.Parameters.AddWithValue("@ClassId", item.ClassId);
            var selectedKids = new List<string>();
            foreach (var kid in item.Kids)
            {
                if (kid.HasClass)
                    selectedKids.Add(kid.KidId.ToString());
            }
            cmd.Parameters.AddWithValue("@KidIds", string.Join(",", selectedKids));
            var response = ExecuteReader(cmd);
            return response;
        }

        public Class InsertClass(Class item)
        {
            var response = string.Empty;
            const string sp = "InsertClass";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", item.DayCareId);
            cmd.Parameters.AddWithValue("@ClassName", item.ClassName);
            response = ExecuteReader(cmd);
            if (response != string.Empty)
                item.Error = response;
            else
            {
                if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                    item.ClassId = new Guid(_ds.Tables[0].Rows[0]["ClassId"].ToString());
                else
                    item.Error = Constants.Error;
            }
            return item;
        }

        public List<KidLog> GetEmailsForParents(Guid dayCareId, List<int> kidIds)
        {
            const string sp = "GetEmailsForParents";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@Id", dayCareId);
            cmd.Parameters.AddWithValue("@KidIds", string.Join(",", kidIds));
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapLogs(_ds.Tables[0]);
            else
                return null;
        }

        public CustomReport GetCustomEmailsForParents(Guid dayCareId, int kidId)
        {
            const string sp = "GetCustomEmailsForParents";
            var cmd = CreateCommand(_conn, CommandType.StoredProcedure, sp);
            cmd.Parameters.AddWithValue("@DayCareId", dayCareId);
            cmd.Parameters.AddWithValue("@KidId", kidId);
            var response = ExecuteReader(cmd);
            if (_ds != null && _ds.Tables.Count > 0 && _ds.Tables[0].Rows.Count > 0)
                return MapCustomReport(_ds.Tables[0]);
            else
                return null;
        }

        private List<Class> MapClasses(DataTable dt)
        {
            var classes = new List<Class>();
            foreach (DataRow row in dt.Rows)
                classes.Add(MapClass(row));
            return classes;
        }

        private Class MapClass(DataRow dr)
        {
            var item = new Class()
            {
                DayCareId = new Guid(dr["DayCareId"].ToString()),
                ClassName = dr["ClassName"].ToString(),
                ClassId = new Guid(dr["ClassId"].ToString()),
                NoOfKids = Convert.ToInt32(dr["NoOfKids"].ToString())
            };
            return item;
        }

        private Settings MapSettings(DataRow dr)
        {
            var item = new Settings()
            {
                DayCareId = new Guid(dr["DayCareId"].ToString()),
                DayCareName = dr["DayCareName"].ToString().Replace(" ", "-"),
                CustomReport = Convert.ToBoolean(dr["CustomReport"].ToString()),
                MakePublic = Convert.ToBoolean(dr["MakePublic"].ToString()),
                GigglesWareReport = Convert.ToBoolean(dr["GigglesWareReport"].ToString()),
                EmailOnRegisterKid = Convert.ToBoolean(dr["EmailOnRegisterKid"].ToString()),
                Language = dr["Language"].ToString()
            };
            return item;
        }

        private Token MapTokenData(DataRow dr)
        {
            var item = new Token()
            {
                Id = new Guid(dr["Id"].ToString()),
                AccessToken = dr["Token"].ToString(),
                Role = dr["Role"].ToString(),
                DateIssued = Convert.ToDateTime(dr["DateCreated"].ToString())
            };
            return item;
        }

        private List<Document> MapDocuments(DataTable dt)
        {
            var docs = new List<Document>();
            foreach (DataRow row in dt.Rows)
                docs.Add(MapDocument(row));
            return docs;
        }

        private Document MapDocument(DataRow dr)
        {
            var doc = new Document()
            {
                Id = Convert.ToInt32(dr["Id"].ToString()),
                Name = dr["Name"].ToString(),
                Title = dr["Title"].ToString(),
                Type = dr["Type"].ToString(),
                MimeType = dr["MimeType"].ToString(),
                DayCareId = new Guid(dr["DayCareId"].ToString())
            };
            doc.Url = Constants.WebUrl + Constants.DocumentsUrl + doc.Id.ToString();
            return doc;
        }

        private List<Notification> MapNotifications(DataTable dt)
        {
            var nots = new List<Notification>();
            foreach (DataRow row in dt.Rows)
                nots.Add(MapNotification(row));
            return nots;
        }

        private List<WhatsNew> MapWhatsNew(DataTable dt)
        {
            var news = new List<WhatsNew>();
            foreach (DataRow row in dt.Rows)
                news.Add(MapWhatsNew(row));
            return news;
        }

        private WhatsNew MapWhatsNew(DataRow dr)
        {
            var news = new WhatsNew()
            {
                Id = Convert.ToInt32(dr["Id"].ToString()),
                Heading = dr["Heading"].ToString(),
                Details = dr["Details"].ToString(),
                ImagePath = dr["ImagePath"].ToString()
            };
            return news;
        }

        private Notification MapNotification(DataRow dr)
        {
            var not = new Notification()
            {
                Id = Convert.ToInt32(dr["Id"].ToString()),
                Message = dr["Notification"].ToString()
            };
            return not;
        }

        private List<Attendance> MapAttendees(DataTable dt)
        {
            var kids = new List<Attendance>();
            var rnd = new Random();
            foreach (DataRow row in dt.Rows)
                kids.Add(MapAttendant(row, rnd));
            return kids;
        }

        private Attendance MapAttendant(DataRow dr, Random rnd)
        {
            var kid = new Attendance()
            {
                KidId = Convert.ToInt32(dr["KidId"].ToString()),
                DayCareId = new Guid(dr["DayCareId"].ToString()),
                KidName = dr["Name"].ToString(),
                HasAttended = !string.IsNullOrEmpty(dr["Attended"].ToString()) &&
                Convert.ToBoolean(dr["Attended"].ToString()),
                Avatar = dr.Table.Columns.Contains("Avatar") ?
                (!string.IsNullOrEmpty(dr["Avatar"].ToString()) ? dr["Avatar"].ToString() : Constants.NotAvl) : Constants.NotAvl,
                Sex = dr["Sex"].ToString()
            };
            if (!string.IsNullOrEmpty(kid.Avatar) && kid.Avatar == Constants.NotAvl)
            {
                if (kid.Sex == "m" || kid.Sex == "M" || kid.Sex == "male" || kid.Sex == "Male" || kid.Sex == "boy" || kid.Sex == "Boy")
                    kid.Avatar = string.Format(Constants.BoyUrl, rnd.Next(0, 22));
                else if (kid.Sex == "f" || kid.Sex == "F" || kid.Sex == "female" || kid.Sex == "Female" || kid.Sex == "girl" || kid.Sex == "Girl")
                    kid.Avatar = string.Format(Constants.GirlUrl, rnd.Next(0, 26));
                else
                    kid.Avatar = string.Format(Constants.UniUrl, rnd.Next(0, 6));
            }
            return kid;
        }

        private DayCare MapDayCare(DataTable dt)
        {
            var dayCare = new DayCare();
            dayCare.DayCareName = dt.Rows[0]["DayCareName"].ToString();
            dayCare.Email = dt.Rows[0]["Email"].ToString();
            dayCare.DayCareId = new Guid(dt.Rows[0]["DayCareId"].ToString());
            dayCare.Settings = new Settings();
            dayCare.Settings.SettingsVisited = !string.IsNullOrEmpty(dt.Rows[0]["SettingsVisited"].ToString()) ?
                Convert.ToBoolean(dt.Rows[0]["SettingsVisited"].ToString()) :
                true;
            dayCare.Settings.CustomReport = !string.IsNullOrEmpty(dt.Rows[0]["CustomReport"].ToString()) ?
                Convert.ToBoolean(dt.Rows[0]["CustomReport"].ToString()) :
                false;
            dayCare.Settings.EmailOnRegisterKid = !string.IsNullOrEmpty(dt.Rows[0]["EmailOnRegisterKid"].ToString()) ?
               Convert.ToBoolean(dt.Rows[0]["EmailOnRegisterKid"].ToString()) :
               true;
            dayCare.Settings.Language = !string.IsNullOrEmpty(dt.Rows[0]["Language"].ToString()) ?
                dt.Rows[0]["Language"].ToString() : Constants.EnglishLang;
            if (dt.Rows.Count > 0 && dt.Rows[0]["KidId"].ToString() != string.Empty)
            {
                dayCare.Kids = new List<Kid>();
                var rnd = new Random();
                foreach (DataRow row in dt.Rows)
                    dayCare.Kids.Add(MapKid(row, rnd));
            }
            else
                dayCare.Kids = null;
            return dayCare;
        }

        private List<Kid> MapKids(DataTable dt)
        {
            var kids = new List<Kid>();
            var rnd = new Random();
            foreach (DataRow row in dt.Rows)
                kids.Add(MapKid(row, rnd));
            return kids;
        }

        private Kid MapKid(DataRow dr, Random rnd)
        {
            var kid = new Kid()
            {
                KidId = Convert.ToInt32(dr["KidId"].ToString()),
                FName = dr["Name"].ToString().Split(',')[0].ToString(),
                LName = dr["Name"].ToString().Split(',')[1].ToString(),
                Sex = dr["Sex"].ToString(),
                DOB = dr["DOB"].ToString(),
                Address = dr["Address"].ToString(),
                Email = dr["Email"].ToString(),
                Phone = dr["Phone"].ToString(),
                Allergies = dr["Allergies"].ToString(),
                DayCareId = new Guid(dr["DayCareId"].ToString()),
                GuardianName = dr.Table.Columns.Contains("GuardianName") ?
                (!string.IsNullOrEmpty(dr["GuardianName"].ToString()) ? dr["GuardianName"].ToString() : string.Empty) : string.Empty,
                HasReportToday = dr.Table.Columns.Contains("HasReportToday") ?
                (!string.IsNullOrEmpty(dr["HasReportToday"].ToString()) ? Convert.ToBoolean(dr["HasReportToday"].ToString()) : false) : false,
                ClassName = dr.Table.Columns.Contains("ClassName") ?
                (!string.IsNullOrEmpty(dr["ClassName"].ToString()) ? dr["ClassName"].ToString() : Constants.NotAvl) : Constants.NotAvl,
                Avatar = dr.Table.Columns.Contains("Avatar") ?
                (!string.IsNullOrEmpty(dr["Avatar"].ToString()) ? dr["Avatar"].ToString() : Constants.NotAvl) : Constants.NotAvl
            };
            if (!string.IsNullOrEmpty(kid.Avatar) && kid.Avatar == Constants.NotAvl)
            {
                if (kid.Sex == "m" || kid.Sex == "M" || kid.Sex == "male" || kid.Sex == "Male" || kid.Sex == "boy" || kid.Sex == "Boy")
                    kid.Avatar = string.Format(Constants.BoyUrl, rnd.Next(0, 22));
                else if (kid.Sex == "f" || kid.Sex == "F" || kid.Sex == "female" || kid.Sex == "Female" || kid.Sex == "girl" || kid.Sex == "Girl")
                    kid.Avatar = string.Format(Constants.GirlUrl, rnd.Next(0, 26));
                else
                    kid.Avatar = string.Format(Constants.UniUrl, rnd.Next(0, 6));
            }
            return kid;
        }

        private User MapUser(DataRow dr, User user)
        {
            user.Role = dr["role"].ToString();
            user.Id = new Guid(dr["id"].ToString());
            user.LName = dr["name"].ToString();
            user.Email = dr["Email"].ToString();
            user.Phone = dr["Phone"].ToString();
            user.Id = new Guid(dr["id"].ToString());
            return user;
        }

        private List<SmallUser> MapSmallUsers(DataTable dt)
        {
            var sUsers = new List<SmallUser>();
            foreach (DataRow row in dt.Rows)
                sUsers.Add(MapSmallUser(row));
            return sUsers;
        }

        private SmallUser MapSmallUser(DataRow dr)
        {
            var sUser = new SmallUser()
            {
                KidId = Convert.ToInt32(dr["KidId"].ToString()),
                Name = dr["Name"].ToString(),
                Avatar = dr.Table.Columns.Contains("Avatar") ?
                (!string.IsNullOrEmpty(dr["Avatar"].ToString()) ? dr["Avatar"].ToString() : Constants.NotAvl) : Constants.NotAvl,
                Sex = dr["Sex"].ToString()
            };
            if (!string.IsNullOrEmpty(sUser.Avatar) && sUser.Avatar == Constants.NotAvl)
            {
                var rnd = new Random();
                if (sUser.Sex == "m" || sUser.Sex == "M" || sUser.Sex == "male" || sUser.Sex == "Male" || sUser.Sex == "boy" || sUser.Sex == "Boy")
                    sUser.Avatar = string.Format(Constants.BoyUrl, rnd.Next(0, 22));
                else if (sUser.Sex == "f" || sUser.Sex == "F" || sUser.Sex == "female" || sUser.Sex == "Female" || sUser.Sex == "girl" || sUser.Sex == "Girl")
                    sUser.Avatar = string.Format(Constants.GirlUrl, rnd.Next(0, 26));
                else
                    sUser.Avatar = string.Format(Constants.UniUrl, rnd.Next(0, 6));
            }
            return sUser;
        }

        private List<KidLog> MapLogs(DataTable dt)
        {
            var logs = new List<KidLog>();
            foreach (DataRow row in dt.Rows)
                logs.Add(MapLog(row));
            return logs;
        }

        private KidLog MapLog(DataRow dr)
        {
            var log = new KidLog()
            {
                KidId = Convert.ToInt32(dr["KidId"].ToString()),
                LogId = new Guid(dr["LogId"].ToString()),
                Date = Convert.ToDateTime(dr["Day"].ToString()),
                ProblemsConcerns = dr["ProblemsConcerns"].ToString(),
                SuppliesNeeded = dr["SuppliesNeeded"].ToString(),
                Comments = dr["Comments"].ToString(),
                Name = dr.Table.Columns.Contains("Name") ?
                (!string.IsNullOrEmpty(dr["Name"].ToString()) ? dr["Name"].ToString() : string.Empty) : string.Empty,
                Email = dr.Table.Columns.Contains("Email") ?
               (!string.IsNullOrEmpty(dr["Email"].ToString()) ? dr["Email"].ToString() : string.Empty) : string.Empty
            };
            //add food items
            if (!string.IsNullOrEmpty(dr["WhatKidAte"].ToString()))
            {
                var whatKidAteList = dr["WhatKidAte"].ToString().Split(Constants.Sep);
                var howKidAteList = dr["HowKidAte"].ToString().Split(Constants.Sep);
                var whenKidAteList = dr["WhenKidAte"].ToString().Split(Constants.Sep);
                var anySnackList = dr["AnySnack"].ToString().Split(Constants.Sep);
                if (whatKidAteList.Any())
                {
                    log.Foods = new List<Food>();
                    for (var i = 0; i < whatKidAteList.Count(); i++)
                    {
                        var food = new Food
                        {
                            WhatKidAte = whatKidAteList[i],
                            WhenKidAte = whenKidAteList[i],
                            HowKidAte = howKidAteList[i],
                            AnySnack = anySnackList[i]
                        };
                        log.Foods.Add(food);
                    }
                }
            }
            //add potty log
            if (!string.IsNullOrEmpty(dr["DiaperCheckTime"].ToString()))
            {
                var diaperCheckTimeList = dr["DiaperCheckTime"].ToString().Split(Constants.Sep);
                var pottyTimeList = dr["PottyTime"].ToString().Split(Constants.Sep);
                var diaperPottyTypeList = dr["DiaperPottyType"].ToString().Split(Constants.Sep);
                if (diaperCheckTimeList.Any())
                {
                    log.Pottys = new List<Potty>();
                    for (var i = 0; i < diaperCheckTimeList.Count(); i++)
                    {
                        var potty = new Potty()
                        {
                            DiaperCheckTime = diaperCheckTimeList[i],
                            PottyTime = pottyTimeList[i],
                            DiaperPottyType = diaperPottyTypeList[i]
                        };
                        log.Pottys.Add(potty);
                    }
                }
            }
            //add nap log
            if (!string.IsNullOrEmpty(dr["NapTime"].ToString()))
            {
                var napTimeList = dr["NapTime"].ToString().Split(Constants.Sep);
                if (napTimeList.Any())
                {
                    log.Naps = new List<Nap>();
                    for (var i = 0; i < napTimeList.Count(); i++)
                    {
                        var nap = new Nap()
                        {
                            NapTime = napTimeList[i]
                        };
                        log.Naps.Add(nap);
                    }
                }
            }
            //add activities log
            if (!string.IsNullOrEmpty(dr["Mood"].ToString()))
            {
                var moodList = dr["Mood"].ToString().Split(Constants.Sep);
                var activityList = dr["Activities"].ToString().Split(Constants.Sep);
                var activityTimeList = dr["ActivityTime"].ToString().Split(Constants.Sep);
                if (moodList.Any())
                {
                    log.Activities = new List<Activity>();
                    for (var i = 0; i < moodList.Count(); i++)
                    {
                        var activity = new Activity()
                        {
                            Mood = moodList[i],
                            Activities = activityList[i],
                            ActivityTime = activityTimeList[i]
                        };
                        log.Activities.Add(activity);
                    }
                }
            }
            return log;
        }

        private List<Parent> MapPrents(DataTable dt)
        {
            var parents = new List<Parent>();
            foreach (DataRow row in dt.Rows)
                parents.Add(MapParent(row));
            return parents;
        }

        private Parent MapParent(DataRow dr)
        {
            var parent = new Parent()
            {
                KidId = dr["KidId"].ToString(),
                ParentId = new Guid(dr["ParentId"].ToString()),
                FName = dr["Name"].ToString().Split(',')[0].ToString(),
                LName = dr["Name"].ToString().Split(',')[1].ToString(),
                Email = dr["Email"].ToString(),
                Phone = dr["Phone"].ToString()
            };
            return parent;
        }

        private string ExecuteReader(SqlCommand cmd)
        {
            try
            {
                _ds = new DataSet();
                cmd.Connection.Open();
                var da = new SqlDataAdapter(cmd);
                da.Fill(_ds);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        protected SqlCommand CreateCommand(string connectionString, CommandType cmdType, string cmdText)
        {
            var connection = new SqlConnection(connectionString);
            var cmd = new SqlCommand(cmdText, connection) { CommandType = cmdType, CommandTimeout = 20 };
            return cmd;
        }

        private string ExecuteQuery(SqlCommand cmd)
        {
            try
            {
                cmd.Connection.Open();
                return cmd.ExecuteNonQuery() > 0 ? "Error" : string.Empty;
            }
            catch (SqlException ex)
            {
                return ex.Message;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
    }
}