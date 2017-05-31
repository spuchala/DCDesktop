using DayCareWebAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using DayCareWebAPI.Models;
using System.Text;
using System.IO;

namespace DayCareWebAPI.Services
{
    public class DayCareService
    {
        private readonly DayCareRepository _repo;

        public DayCareService()
        {
            this._repo = new DayCareRepository();
        }

        public User LoginUser(string email, string password)
        {
            return _repo.LoginUser(email, password);
        }

        public Kid GetKid(int kidId, Guid dayCareId)
        {
            return _repo.GetKid(kidId, dayCareId);
        }

        public List<Kid> GetKidsFromDayCare(Guid dayCareId)
        {
            return _repo.GetKidsFromDayCare(dayCareId);
        }

        public List<Kid> GetKidsInClass(Guid classId)
        {
            return _repo.GetKidsInClass(classId);
        }

        public List<Class> GetClassesForDayCare(Guid dayCareId)
        {
            return _repo.GetClassesForDayCare(dayCareId);
        }

        public string SaveCustomReport(CustomReport customReport)
        {
            foreach (var question in customReport.questions)
            {
                SanitizeCustomReportInputAnswers(question.answers);
            }
            return _repo.SaveCustomReport(customReport);
        }

        public List<Kid> GetKidsFromParent(Guid parentId)
        {
            return _repo.GetKidsFromParent(parentId);
        }

        public string SaveSettings(Settings settings)
        {
            return _repo.SaveSettings(settings);
        }

        public bool CheckCustomReportExists(Guid dayCareId)
        {
            return _repo.CheckCustomReportExists(dayCareId);
        }

        public string CreateCustomReportQuestions(CustomReport customReport)
        {
            var customReportId = Guid.NewGuid();
            var response = string.Empty;
            if (customReport.questions != null && customReport.questions.Any())
            {
                foreach (Question question in customReport.questions)
                {
                    var options = string.Empty;
                    SanitizeCustomReportQuestionValues(question.values);
                    if (question.id == (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions ||
                        question.id == (int)Constants.QuestionTypes.QuestionOptions)
                    {
                        SanitizeCustomReportQuestionOptions(question.options);
                        options = string.Join(Constants.SepString, (from e in question.options select e.value).ToList());
                    }
                    response = _repo.InsertCustomReportQuestions(customReportId, customReport.DayCareId, question.id, question.type, string.Join(Constants.SepString, question.values), options);
                    if (!string.IsNullOrEmpty(response))
                        return response;
                }
            }
            return response;
        }

        public CustomReport GetCustomReport(Guid dayCareId, int kidId, string day)
        {
            return _repo.GetCustomReport(dayCareId, kidId, day);
        }

        public InstantLog GetInstantLog(int kidId, string day)
        {
            return _repo.GetInstantLog(kidId, (string.IsNullOrEmpty(day) || day == Constants.UnDefined) ? DateTime.Today.ToString("yyyy-MM-dd")
                : DateTime.Parse(day.Replace(",", "").Replace(" ", "-")).ToString("yyyy-MM-dd"));
        }

        public Schedule InsertSchedule(Schedule schedule)
        {
            return _repo.InsertSchedule(schedule);
        }

        public string SaveScheduleMessage(ScheduleMessage message, Guid dayCareId)
        {
            return _repo.SaveScheduleMessage(message, dayCareId);
        }

        public List<Schedule> GetSchedules(Guid dayCareId)
        {
            return _repo.GetSchedules(dayCareId);
        }

        public Schedule GetSchedule(Guid dayCareId, int scheduleId)
        {
            return _repo.GetSchedule(dayCareId, scheduleId);
        }

        public DayCareInfo GetDayCareInfo(Guid dayCareId)
        {
            return _repo.GetDayCareInfo(dayCareId);
        }

        public string ManageDayCareInfo(DayCareInfo info)
        {
            return _repo.ManageDayCareInfo(info);
        }

        public string ManageDayCareInfoHome(DayCareInfo info)
        {
            return _repo.ManageDayCareInfoHome(info);
        }

        public string ManageDayCareInfoAbout(DayCareInfo info)
        {
            return _repo.ManageDayCareInfoAboutUs(info);
        }

        public string ManageDayCareInfoProgram(DayCareInfo info)
        {
            return _repo.ManageDayCareInfoProgram(info);
        }

        public Message InsertInstantLogMessage(Message message, int kidId, int? logId)
        {
            message.Type = GetInstantMessageType(message.Value);
            return _repo.InsertInstantLogMessage(message, kidId, logId);
        }

        public string GetInstantMessageType(string input)
        {
            var words = input.Split(' ');
            foreach (var item in words)
            {
                if (item.Length < 3) continue;
                if (Constants.Foods.Contains(item.ToLower()))
                    return Constants.Food;
                else if (Constants.Naps.Contains(item.ToLower()))
                    return Constants.Nap;
                else if (Constants.Pottys.Contains(item.ToLower()))
                    return Constants.Potty;
                else if (Constants.Activities.Contains(item.ToLower()))
                    return Constants.Activity;
                else if (Constants.Moods.Contains(item.ToLower()))
                    return Constants.Mood;
                else if (Constants.Sick.Contains(item.ToLower()))
                    return Constants.Sicks;
            }
            return Constants.Misc;
        }

        public List<Kid> GetReportForDayCare(Guid dayCareId, string day)
        {
            var kids = _repo.GetDayCareReport(dayCareId, day);
            foreach (var kid in kids)
                if (kid.Log != null) SanitizeKidLog(kid.Log, false);
            return kids;
        }

        public Parent GetParent(Guid parentId, string day)
        {
            var parent = _repo.GetParent(parentId, day);
            foreach (var kid in parent.Kids)
                if (kid.Log != null) SanitizeKidLog(kid.Log, false);
            return parent;
        }

        public Guid? GetParentsDayCare(Guid parentId)
        {
            return _repo.GetParentsDayCare(parentId);
        }

        public Parent CheckParentByEmail(string email)
        {
            return _repo.CheckParentByEmail(email);
        }

        public KidLog GetKidLogByLogId(Guid logId)
        {
            return _repo.GetKidLogByLogId(logId);
        }

        public KidLog GetKidLogOnADay(int kidId, Guid id, string day, bool forReport)
        {
            var log = _repo.GetKidLogOnADay(kidId, id, (day == string.Empty || day == Constants.UnDefined) ? DateTime.Today.ToString("yyyy-MM-dd")
                : DateTime.Parse(day.Replace(",", "").Replace(" ", "-")).ToString("yyyy-MM-dd"));
            if (!forReport)
                if (log != null) SanitizeKidLog(log, false);
            return log;
        }

        public CustomReport GetCustomReportOnADay(int kidId, Guid id, string day)
        {
            return _repo.GetCustomReportOnADay(id, kidId, (day == string.Empty || day == Constants.UnDefined) ? DateTime.Today.ToString("yyyy-MM-dd")
                : DateTime.Parse(day.Replace(",", "").Replace(" ", "-")).ToString("yyyy-MM-dd"));
        }

        public List<KidLog> GetKidLogsInDayRange(int kidId, Guid dayCareId, DateTime startDay, DateTime endDay)
        {
            return _repo.GetKidLogsInDayRange(kidId, dayCareId, startDay, endDay);
        }

        public DayCare GetDayCareData(Guid dayCareId)
        {
            return _repo.GetDayCare(dayCareId);
        }

        public Kid InsertKid(Kid kid)
        {
            var returnKid = _repo.InsertKid(kid);
            //email parent
            if (string.IsNullOrEmpty(returnKid.Error))
            {
                var settings = _repo.GetSettings(kid.DayCareId);
                if (settings != null && settings.EmailOnRegisterKid)
                    EmailParent(kid);
            }
            return returnKid;
        }

        public void InsertAlexaToken(Token token)
        {
            _repo.InsertAlexaToken(token);
        }

        public Kid InsertKidShort(Kid kid)
        {
            var returnKid = _repo.InsertKidShort(kid);
            return returnKid;
        }

        public string UpdateKid(Kid kid)
        {
            var returnKid = _repo.UpdateKid(kid);
            //email parent
            if (!string.IsNullOrEmpty(returnKid.Error))
                return returnKid.Error;
            return string.Empty;
        }

        public string SendEmailToParents(Guid dayCareId, List<int> emailList)
        {
            var logs = _repo.GetEmailsForParents(dayCareId, emailList);
            if (logs == null)
                return "Emailing reports Failed";
            else
            {
                //send emails here
                foreach (var log in logs)
                {
                    var response = EmailReportToParent(log);
                    if (response != string.Empty)
                        return "Emailing reports Failed";
                }
                return string.Empty;
            }
        }

        public string SendDocumentEmail(Document data)
        {
            if (data.EmailList != null && data.EmailList.Any())
            {
                EmailDocumentLink(string.Join(";", data.EmailList), data.DayCareName, data.Title, data.Url);
            }
            else
                return "Emailing documents failed";
            return string.Empty;
        }

        public string SendScheduleEmail(Schedule schedule)
        {
            if (schedule.EmailList != null && schedule.EmailList.Any())
            {
                EmailScheduleLink(string.Join(";", schedule.EmailList), schedule.DayCareName, schedule.Name, schedule.Url);
            }
            else
                return "Emailing schedule failed";
            return string.Empty;
        }

        public string SendCustomEmailsToParents(Guid dayCareId, List<int> kidList)
        {
            foreach (var item in kidList)
            {
                var customReport = _repo.GetCustomEmailsForParents(dayCareId, item);
                if (customReport == null)
                    return "Emailing reports Failed";
                else
                {
                    var response = EmailCustomReportToParent(customReport);
                    if (response != string.Empty)
                        return "Emailing reports Failed";
                }
            }
            return string.Empty;
        }

        public string BuildEmailTemplate(string heading, string mainBody, string footer, string finalFooter)
        {
            var body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/Email.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Heading}", heading);
            body = body.Replace("{Body}", mainBody);
            body = body.Replace("{Footer}", footer);
            body = body.Replace("{FinalFooter}", finalFooter);
            return body;
        }

        private string EmailCustomReportToParent(CustomReport customReport)
        {
            var subject = "Today's report for " + customReport.KidName;
            var body = new StringBuilder();
            var header = string.Format(Constants.HeaderForEmail, "Today's report for " + customReport.KidName);
            body.Append("<br/><br/>");
            foreach (var item in customReport.questions)
            {
                switch (item.id)
                {
                    case 1:
                        body.Append("&nbsp;&nbsp;<span style='color: #d02090;'><strong>" + item.values[0] + "</strong></span><br/><br/>");
                        break;
                    case 2:
                        body.Append("&nbsp;&nbsp;<strong>" + item.values[0] + ": </strong>" + item.answers[0] + "<br/><br/>");
                        break;
                    case 3:
                        body.Append("&nbsp;&nbsp;<strong>" + item.values[0] + ": </strong>" + item.answers[0] + "&nbsp;<strong>" + item.values[1] + ": </strong>" + item.answers[1] + "<br/><br/>");
                        break;
                    case 4:
                        body.Append("&nbsp;&nbsp;<strong>" + item.values[0] + ": </strong><br/>");
                        foreach (var option in item.options)
                            body.Append("&nbsp;&nbsp;" + (option.check ? "<span style='color:#d02090;'>" : "") + option.value + (option.check ? "</span>" : ""));
                        body.Append("<br/><br/>");
                        break;
                    case 5:
                        body.Append("&nbsp;&nbsp;<strong>" + item.values[0] + ": </strong>: " + item.answers[0] + "<br/>");
                        foreach (var option in item.options)
                            body.Append("&nbsp;&nbsp;" + (option.check ? "<span style='color:#d02090;'>" : "") + option.value + (option.check ? "</span>" : ""));
                        body.Append("<br/><br/>");
                        break;
                    default:
                        break;
                }
            }
            var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.RegisterParentUrl + "\">Register Here!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
            var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
            var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
            try
            {
                var mailMessage = new MailMessage(Constants.FromUserId, customReport.ParentEmail,
                                                      subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "emailing custom report to parent kidid provided", customReport.KidId.ToString(), ex.StackTrace);
                return ex.Message;
            }
        }

        private string EmailReportToParent(KidLog log)
        {
            var subject = "Today's report for " + log.Name;
            var header = string.Format(Constants.HeaderForEmail, "Today's report for " + log.Name);
            var body = new StringBuilder();
            body.Append("<br/><span style='color: #d02090;'><b>Foods:</b></span><br/>");
            foreach (var food in log.Foods)
            {
                body.Append("&nbsp;Food:<br/>");
                body.Append("&nbsp;&nbsp;What Kid Ate: " + food.WhatKidAte + "<br/>");
                body.Append("&nbsp;&nbsp;When Kid Ate: " + food.WhenKidAte + "<br/>");
                body.Append("&nbsp;&nbsp;How Kid Ate: " + food.HowKidAte + "<br/>");
                body.Append("&nbsp;&nbsp;Any Snacks: " + food.AnySnack + "<br/>");
            }
            body.Append("<br/><span style='color: #d02090;'><b>Pottys:</b></span><br/>");
            foreach (var potty in log.Pottys)
            {
                body.Append("&nbsp;Potty:<br/>");
                body.Append("&nbsp;&nbsp;Diaper Check Time: " + potty.DiaperCheckTime + "<br/>");
                body.Append("&nbsp;&nbsp;Potty Time: " + potty.PottyTime + "<br/>");
                body.Append("&nbsp;&nbsp;Diaper Potty Type: " + potty.DiaperPottyType + "<br/>");
            }
            body.Append("<br/><span style='color: #d02090;'><b>Naps:</b></span><br/>");
            foreach (var nap in log.Naps)
            {
                body.Append("&nbsp;Nap:<br/>");
                body.Append("&nbsp;&nbsp;Nap Time: " + nap.NapTime + "<br/>");
            }
            body.Append("<br/><span style='color: #d02090;'><b>Activities:</b></span><br/>");
            foreach (var activity in log.Activities)
            {
                body.Append("&nbsp;Activity:<br/>");
                body.Append("&nbsp;&nbsp;Mood: " + activity.Mood + "<br/>");
                body.Append("&nbsp;&nbsp;Activity Time: " + activity.ActivityTime + "<br/>");
                body.Append("&nbsp;&nbsp;Activities: " + activity.Activities + "<br/>");
            }
            body.Append("<br/><span style='color: #d02090;'><b>Problems Concerns:</b></span><br/>");
            body.Append("&nbsp;Problems/Concerns: " + log.ProblemsConcerns + "<br/>");
            body.Append("&nbsp;Supplies Needed: " + log.SuppliesNeeded + "<br/>");
            body.Append("&nbsp;&nbsp;Comments: " + log.Comments + "<br/><br/>");
            var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.RegisterParentUrl + "\">Register Here!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
            var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
            var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
            try
            {
                var mailMessage = new MailMessage(Constants.FromUserId, log.Email,
                                                      subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "emailing report to parent logid provided", log.LogId.ToString(), ex.StackTrace);
                return ex.Message;
            }
        }

        private void EmailParent(Kid kid)
        {
            var settings = _repo.GetSettings(kid.DayCareId);
            if (!settings.EmailOnRegisterKid)
                return;
            try
            {
                var subject = "DayCare " + kid.DayCareName + " has added your little Giggler " + kid.FName + " " + kid.LName + " to their system";
                var header = string.Format(Constants.HeaderForEmail, "Family DayCare " + kid.DayCareName + " has added your kid " + kid.FName + " " + kid.LName + " to their system");
                var body = "<br/>Congrats!<br/>DayCare " + kid.DayCareName + " has added your kid " + kid.FName + " " + kid.LName + " to their system.";
                body = body + "<br/>Click below links to create or view your account.<br/>";
                var regLink = "<a href=\"" + Constants.RegisterParentUrl + "\">Register Here!</a><br/>";
                body = body + regLink;
                var loginLink = "<a href=\"" + Constants.LoginUrl + "\">Login If Registered!</a><br/><br/>";
                body = body + loginLink;
                var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
                var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
                var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
                var mailMessage = new MailMessage(Constants.FromUserId, kid.Email,
                                                      subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "emailing parent about kid added and kid id provided", kid.KidId.ToString(), ex.StackTrace);
            }
        }

        private void EmailScheduleLink(string emailList, string dayCareName, string scheduleName, string scheduleUrl)
        {
            try
            {
                var subject = "Your DayCare " + dayCareName + " has shared a schedule with you!";
                var header = string.Format(Constants.HeaderForEmail, "Your DayCare " + dayCareName + " has shared schedule " + scheduleName + " with you!");
                var body = "<br/>";
                body = body + "<br/>Click below link to view the schedule.<br/>";
                var regLink = "<a href=\"" + scheduleUrl + "\">" + scheduleName + "</a><br/>";
                body = body + regLink;
                var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
                var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
                var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
                var mailMessage = new MailMessage(Constants.FromUserId, emailList, subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "error while emailing welcome email to daycare daycareid provided", dayCareName.ToString(), ex.StackTrace);
            }
        }

        private void EmailDocumentLink(string emailList, string dayCareName, string docName, string docUrl)
        {
            try
            {
                var subject = "Your DayCare " + dayCareName + " has shared a document with you!";
                var header = string.Format(Constants.HeaderForEmail, "Your DayCare " + dayCareName + " has shared a document with you!");
                var body = "<br/>";
                body = body + "<br/>Click below link to download the document.<br/>";
                var regLink = "<a href=\"" + docUrl + "\">" + docName + "</a><br/>";
                body = body + regLink;
                var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
                var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
                var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
                var mailMessage = new MailMessage(Constants.FromUserId, emailList, subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "error while emailing welcome email to daycare daycareid provided", dayCareName.ToString(), ex.StackTrace);
            }
        }

        private void EmailDayCareWelcome(DayCare daycare)
        {
            try
            {
                var subject = "Your DayCare " + daycare.DayCareName + " is successfully registered with GigglesWare!";
                var header = string.Format(Constants.HeaderForEmail, "Your DayCare " + daycare.DayCareName + " is registered with GigglesWare!");
                var body = "<br/>Hi " + daycare.FName + " " + daycare.LName + ",<br/><br/>Congrats! Your DayCare " + daycare.DayCareName + " is registered with GigglesWare.";
                body = body + ".<br/><br/> Click below link to view your account.<br/><br/>";
                var loginLink = "<a href=\"" + Constants.LoginUrl + "\">View your account</a><br/>";
                body = body + loginLink;
                body = body + "Welcome to GigglesWare again and Thanks for giving us a try.";
                var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
                var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
                var finalBody = BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
                var mailMessage = new MailMessage(Constants.FromUserId, daycare.Email,
                                                      subject, finalBody)
                { IsBodyHtml = true };
                var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                    EnableSsl = true
                };
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _repo.LogError(ex.Message, "error while emailing welcome email to daycare daycareid provided", daycare.Id.ToString(), ex.StackTrace);
            }
        }

        public Parent InsertParent(Parent parent)
        {
            return _repo.InsertParent(parent);
        }

        public Class InsertClass(Class item)
        {
            return _repo.InsertClass(item);
        }

        public string AssignClassToKid(Class item)
        {
            return _repo.AssignClassToKid(item);
        }

        public DayCare InsertDayCare(DayCare dayCare)
        {
            var dc = _repo.InsertDayCare(dayCare);
            if (string.IsNullOrEmpty(dc.Error))
                EmailDayCareWelcome(dc);
            return dc;
        }

        private void SanitizeCustomReportQuestionValues(List<string> Values)
        {
            if (Values != null && Values.Any())
            {
                for (int i = 0; i < Values.Count; i++)
                {
                    if (Values[i] == string.Empty)
                        Values[i] = Constants.SanityVal;
                }
            }
        }

        private void SanitizeCustomReportInputAnswers(List<string> answers)
        {
            if (answers != null && answers.Any())
            {
                for (int i = 0; i < answers.Count; i++)
                {
                    if (answers[i] == string.Empty)
                        answers[i] = Constants.SanityVal;
                }
            }
        }

        private void SanitizeCustomReportQuestionOptions(List<Option> Options)
        {
            if (Options != null && Options.Any())
            {
                Options.RemoveAll(elem => elem.value == string.Empty);
            }
        }

        public void SanitizeKidLog(KidLog log, bool sanetize)
        {
            if (log.Foods != null && log.Foods.Any())
            {
                foreach (var food in log.Foods)
                    Sanitize(food, sanetize);
                foreach (var potty in log.Pottys)
                    Sanitize(potty, sanetize);
                foreach (var nap in log.Naps)
                    Sanitize(nap, sanetize);
                foreach (var activ in log.Activities)
                    Sanitize(activ, sanetize);
            }
            if (log.SuppliesNeeded == null) log.SuppliesNeeded = string.Empty;
            if (log.ProblemsConcerns == null) log.ProblemsConcerns = string.Empty;
            if (log.Comments == null) log.Comments = string.Empty;
        }

        private void Sanitize(object obj, bool sanetize)
        {
            Type t = obj.GetType();
            foreach (var propInfo in t.GetProperties())
            {
                if ((string)propInfo.GetValue(obj, null) == string.Empty && sanetize)
                    propInfo.SetValue(obj, Constants.SanityVal, null);
                if ((string)propInfo.GetValue(obj, null) == Constants.SanityVal && !sanetize)
                    propInfo.SetValue(obj, Constants.DeSanityVal, null);
            }
        }

        public KidLog InsertKidLog(KidLog log)
        {
            SanitizeKidLog(log, true);
            return _repo.InsertKidLog(log);
        }

        public string InsertAttendance(List<Attendance> kids)
        {
            return _repo.LogAttendance(kids);
        }

        public void InsertError(string error, string errorAt, string errorBy, string trace)
        {
            _repo.LogError(error, errorAt, errorBy, trace);
        }

        public string RemoveKid(int kidId, Guid dayCareId, string reason)
        {
            return _repo.RemoveKid(kidId, dayCareId, reason);
        }

        public string RemoveSchedule(Guid dayCareId, int scheduleId)
        {
            return _repo.RemoveSchedule(dayCareId, scheduleId);
        }

        public string RemoveScheduleMessage(Guid dayCareId, int scheduleId, int messageId)
        {
            return _repo.RemoveScheduleMessage(dayCareId, scheduleId, messageId);
        }

        public string RemoveNotification(Guid dayCareId, int id)
        {
            return _repo.RemoveNotification(dayCareId, id);
        }

        public string RemoveWhatsNew(Guid dayCareId, int id)
        {
            return _repo.RemoveWhatsNew(dayCareId, id);
        }

        public List<Attendance> GetAttendance(Guid dayCareId)
        {
            return _repo.GetAttendedKids(dayCareId);
        }

        public bool CheckAppVersion(string osType, string version)
        {
            return _repo.CheckAppVersion(osType, version);
        }

        public Settings GetSettings(Guid dayCareId)
        {
            return _repo.GetSettings(dayCareId);
        }

        public List<Notification> GetNotifications(Guid dayCareId)
        {
            return _repo.GetNotifications(dayCareId);
        }

        public List<WhatsNew> GetWhatsNew(Guid daycareId)
        {
            return _repo.GetWhatsNew(daycareId);
        }

        public List<WhatsNew> GetAllWhatsNew()
        {
            return _repo.GetAllWhatsNew();
        }

        public List<SmallUser> GetKidsWithNoClass(Guid dayCareId)
        {
            return _repo.GetKidsWithNoClass(dayCareId);
        }
    }
}