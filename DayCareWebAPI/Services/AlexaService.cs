using AlexaSkillsKit.Speechlet;
using System;
using System.Collections.Generic;
using System.Linq;
using DayCareWebAPI.Repository;
using AlexaSkillsKit.UI;
using AlexaSkillsKit.Slu;
using DayCareWebAPI.Models;
using System.Xml;
using System.Text;

namespace DayCareWebAPI.Services
{
    public class AlexaService : Speechlet
    {
        public Guid Id { get; set; }
        public string Role { get; set; }
        public bool IsDev { get; set; }

        private readonly DayCareRepository _repo;
        public AlexaService()
        {
            this._repo = new DayCareRepository();
            Id = Guid.Empty;
        }

        public override SpeechletResponse OnIntent(IntentRequest intentRequest, Session session)
        {
            // Get intent from the request object.
            Intent intent = intentRequest.Intent;
            string intentName = (intent != null) ? intent.Name : null;

            //handle help or stop or cancel without authenticating
            if (Constants.HelpIntent.Equals(intentName))
            {
                return GetHelpIntentResponse();
            }
            else if (Constants.StopIntent.Equals(intentName) || Constants.CancelIntent.Equals(intentName))
            {
                return GetStopOrCancelIntentResponse();
            }

            //authenticate the user
            if (!IsDev)
                if (!IsTokenValid(session))
                    return GetExceptionWarningResponse("Sorry! your Giggles account is not linked with Alexa. Please go to Giggles skill on Alexa skills and select link account.", "Link your Account", "LinkAccount");

            //log the session to track alexa
            _repo.LogAlexaSession(Id, intentRequest.RequestId, session.SessionId, "On Intent", intentName);

            // Note: If the session is started with an intent, no welcome message will be rendered;
            // rather, the intent specific response will be returned.
            if (Constants.LogActionIntent.Equals(intentName))
            {
                if (Role == Constants.DayCareRole)
                    return ProcessLogRequestIntent(intent, session);
                else
                    return GetExceptionWarningResponse("You are not authorized as daycare to perform this action. Please register as a day care with GigglesWare.", "Warning", null);
            }
            else if (Constants.RequestLogIntent.Equals(intentName))
            {
                if (Role == Constants.DayCareRole || Role == Constants.ParentRole)
                    return ProcessRequestIntent(intent, session);
                else
                    return GetExceptionWarningResponse("You are not authorized to perform this action. Please register at gigglesware.com.", "Warning", null);
            }
            else if (Constants.AddChildIntent.Equals(intentName))
            {
                if (Role == Constants.DayCareRole)
                {
                    return ProcessAddKidIntent(intent, session);
                }
                else
                    return GetExceptionWarningResponse("You are not authorized to perform this action. Please register as a day care with GigglesWare.", "Warning", null);
            }
            else if (Constants.DayStatusIntent.Equals(intentName))
            {
                if (Role == Constants.DayCareRole || Role == Constants.ParentRole)
                    return ProcessDayStatusIntent(intent, session);
                else
                    return GetExceptionWarningResponse("You are not authorized to perform this action. Please register at gigglesware.com.", "Warning", null);
            }
            else
            {
                return GetExceptionWarningResponse("Invalid Request", "Invalid Request", null);
            }
        }

        public override SpeechletResponse OnLaunch(LaunchRequest launchRequest, Session session)
        {
            _repo.LogAlexaSession(Id, launchRequest.RequestId, session.SessionId, "On Launch", string.Empty);
            return GetWelcomeResponse();
        }

        public override void OnSessionEnded(SessionEndedRequest sessionEndedRequest, Session session)
        {
            _repo.LogAlexaSession(Id, sessionEndedRequest.RequestId, session.SessionId, "Session Ended", string.Empty);
        }

        public override void OnSessionStarted(SessionStartedRequest sessionStartedRequest, Session session)
        {
            _repo.LogAlexaSession(Id, sessionStartedRequest.RequestId, session.SessionId, "Session Started", string.Empty);
        }



        #region Helper Methods       

        private bool IsTokenValid(Session session)
        {
            if (string.IsNullOrEmpty(session.User.AccessToken))
                return false;
            var tokenData = _repo.GetTokenData(session.User.AccessToken);
            if (tokenData == null)
                return false;
            else if (tokenData != null && string.IsNullOrEmpty(tokenData.AccessToken))
                return false;
            else
            {
                //check for the expiry
                if ((DateTime.Now - tokenData.DateIssued).Days > 365)
                    return false;
            }
            Id = tokenData.Id;
            Role = tokenData.Role;
            return true;
        }

        private SpeechletResponse ProcessAddKidIntent(Intent intent, Session session)
        {
            // Get the slots from the intent.
            Dictionary<string, Slot> slots = intent.Slots;

            // Get the name slot from the list slots.
            Slot nameSlot = slots[Constants.NameSlot];
            string speechOutput = "";
            string kidNameForOutput = "";
            // Check for name and create output to user.
            if (nameSlot != null && !string.IsNullOrEmpty(nameSlot.Value))
            {
                var fName = string.Empty; var lName = string.Empty;
                if (nameSlot.Value.Contains(' '))
                {
                    var nameList = nameSlot.Value.Split(' ');
                    if (nameList != null && nameList.Any() && nameList.Count() == 2)
                    {
                        fName = nameList[0];
                        lName = nameList[1];
                    }
                }
                else
                    fName = nameSlot.Value;
                kidNameForOutput = nameSlot.Value;
                var kid = new Kid() { FName = fName, LName = lName, DayCareId = Id };
                kid = _repo.InsertKidShort(kid);
                if (kid != null && kid.Error != string.Empty)
                {
                    speechOutput = string.Format("{0} is now registered in your account", nameSlot.Value);
                }
                else
                {
                    speechOutput = "Something went wrong. Please try again";
                    kidNameForOutput = "Error";
                }
            }
            else
            {
                // Render an error since we don't know what the users name is.
                speechOutput = "Please mention the kid's name in the request";
                kidNameForOutput = "Error";
            }
            return BuildSpeechletResponse(intent.Name, speechOutput, kidNameForOutput, true);
        }

        private SpeechletResponse ProcessDayStatusIntent(Intent intent, Session session)
        {
            // Get the slots from the intent.
            Dictionary<string, Slot> slots = intent.Slots;

            // Get the name slot from the list slots.
            Slot nameSlot = slots[Constants.NameSlot];
            string speechOutput = "";
            string kidNameForOutput = "";

            // Check for name and create output to user.
            if (nameSlot != null && !string.IsNullOrEmpty(nameSlot.Value))
            {
                string kidName = nameSlot.Value;
                kidNameForOutput = nameSlot.Value;
                var assistant = new Assistant() { Subject = kidName, DayCareId = Id };
                if (Role == Constants.ParentRole)
                {
                    var kid = _repo.GetKidForParentByName(Id, kidName);
                    if (kid != null)
                    {
                        Id = kid.DayCareId;
                        assistant.Subject = kid.FName + " " + kid.LName;
                        assistant.KidId = kid.KidId;
                    }
                }
                else if (Role == Constants.DayCareRole)
                {
                    assistant = _repo.GetKidByName(assistant);
                }
                if (assistant.KidId != 0 && assistant.KidId > 0)
                {
                    //get date 
                    var day = string.Empty;
                    Slot dateSlot = slots[Constants.DateSlot];
                    if (dateSlot != null && !string.IsNullOrEmpty(dateSlot.Value))
                    {
                        day = dateSlot.Value;
                    }
                    else
                        day = DateTime.Today.ToString("yyyy-MM-dd");
                    var log = _repo.GetKidLogOnADay(assistant.KidId, Id, day);
                    if (log == null)
                        speechOutput = string.Format("No reports were logged for {0}", kidName);
                    else
                        speechOutput = ProcessCompleteLog(log, kidName);
                }
            }
            else
            {
                // Render an error since we don't know what the users name is.
                speechOutput = "Please mention the kid's name in the request";
                kidNameForOutput = "Error";
            }
            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse(intent.Name, speechOutput, kidNameForOutput, true);
        }

        private SpeechletResponse ProcessRequestIntent(Intent intent, Session session)
        {
            // Get the slots from the intent.
            Dictionary<string, Slot> slots = intent.Slots;

            // Get the name slot from the list slots.
            Slot nameSlot = slots[Constants.NameSlot];
            string speechOutput = "";
            string kidNameForOutput = "";

            // Check for name and create output to user.
            if (nameSlot != null && !string.IsNullOrEmpty(nameSlot.Value))
            {
                string kidName = nameSlot.Value;
                kidNameForOutput = nameSlot.Value;
                var assistant = new Assistant() { Subject = kidName, DayCareId = Id };
                if (Role == Constants.ParentRole)
                {
                    var kid = _repo.GetKidForParentByName(Id, kidName);
                    if (kid != null)
                    {
                        Id = kid.DayCareId;
                        assistant.Subject = kid.FName + " " + kid.LName;
                        assistant.KidId = kid.KidId;
                    }
                }
                else if (Role == Constants.DayCareRole)
                {
                    assistant = _repo.GetKidByName(assistant);
                }
                if (assistant.KidId != 0 && assistant.KidId > 0)
                {
                    Slot actionSlot = slots[Constants.ActionSlot];
                    //get action slot
                    if (actionSlot != null && !string.IsNullOrEmpty(actionSlot.Value))
                    {
                        var dayCareServ = new DayCareService();
                        assistant.Predicate = dayCareServ.GetInstantMessageType(actionSlot.Value);
                    }
                    if (string.IsNullOrEmpty(assistant.Predicate))
                        assistant.Predicate = Constants.Misc;
                    //get date 
                    var day = string.Empty;
                    Slot dateSlot = slots[Constants.DateSlot];
                    if (dateSlot != null && !string.IsNullOrEmpty(dateSlot.Value))
                    {
                        day = dateSlot.Value;
                    }
                    else
                        day = DateTime.Today.ToString("yyyy-MM-dd");
                    //get supplies
                    Slot supplySlot = slots[Constants.SuppliesSlot];
                    if (supplySlot != null && !string.IsNullOrEmpty(supplySlot.Value))
                    {
                        assistant.Predicate = Constants.SuppliesSlot;
                    }
                    var log = _repo.GetKidLogOnADay(assistant.KidId, Id, day);
                    if (log == null)
                        speechOutput = string.Format("No reports were logged for {0}", kidName);
                    else
                        speechOutput = ProcessLog(log, assistant.Predicate, kidName);
                }
            }
            else
            {
                // Render an error since we don't know what the users name is.
                speechOutput = "Please mention the kid's name in the request";
                kidNameForOutput = "Error";
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse(intent.Name, speechOutput, kidNameForOutput, true);
        }

        private string ProcessCompleteLog(KidLog log, string kidName)
        {
            var serv = new DayCareService();
            serv.SanitizeKidLog(log, false);
            var foods = string.Empty;
            var naps = string.Empty;
            var pottys = string.Empty;
            var activities = string.Empty;
            var response = new StringBuilder();
            //foods            
            if (log.Foods != null && log.Foods.Any())
            {
                response.Append("Food ");
                foreach (var food in log.Foods)
                {
                    if (!string.IsNullOrEmpty(food.WhatKidAte))
                        foods = foods + food.WhatKidAte + " ";
                    if (!string.IsNullOrEmpty(food.WhenKidAte))
                        foods = foods + " at " + food.WhenKidAte + " ";
                    if (!string.IsNullOrEmpty(food.HowKidAte))
                        foods = foods + food.HowKidAte + " ";
                    if (!string.IsNullOrEmpty(food.AnySnack))
                        foods = foods + " snacks " + food.HowKidAte + " ";
                }
                if (string.IsNullOrEmpty(foods)) response.Append("Not Available "); else response.Append(foods);
            }
            //naps
            if (log.Naps != null && log.Naps.Any())
            {
                response.Append("Naps ");
                foreach (var nap in log.Naps)
                {
                    if (!string.IsNullOrEmpty(nap.NapTime))
                        naps = naps + nap.NapTime + " ";
                }
                if (string.IsNullOrEmpty(naps)) response.Append("Not Available "); else response.Append(naps);
            }
            //pottys
            if (log.Pottys != null && log.Pottys.Any())
            {
                response.Append("Pottys ");
                foreach (var potty in log.Pottys)
                {
                    if (!string.IsNullOrEmpty(potty.DiaperCheckTime))
                        pottys = pottys + "Diaper check time at " + potty.DiaperCheckTime;
                    if (!string.IsNullOrEmpty(potty.DiaperPottyType))
                        pottys = pottys + ", diaper type " + potty.DiaperPottyType;
                    if (!string.IsNullOrEmpty(potty.PottyTime))
                        pottys = pottys + ", potty time at " + (potty.PottyTime == "0 am" ? "12" : potty.PottyTime);
                    pottys = pottys + (" ");
                }
                if (string.IsNullOrEmpty(pottys)) response.Append("Not Available "); else response.Append(pottys);
            }
            //activities
            if (log.Activities != null && log.Activities.Any())
            {
                response.Append("Activities ");
                foreach (var activity in log.Activities)
                {
                    if (!string.IsNullOrEmpty(activity.Activities))
                    {
                        activities = activities + (activity.Activities + (!string.IsNullOrEmpty(activity.ActivityTime) ? " at " : string.Empty) + activity.ActivityTime);
                        activities = activities + (" ");
                    }
                }
                if (string.IsNullOrEmpty(activities)) response.Append("Not Available "); else response.Append(activities);
            }
            if (!string.IsNullOrEmpty(log.ProblemsConcerns))
                response.Append("problems/concerns " + log.ProblemsConcerns);
            if (!string.IsNullOrEmpty(log.Comments))
                response.Append("comments " + log.Comments);
            response.Append(!string.IsNullOrEmpty(log.SuppliesNeeded) ? ("Supplied Needed " + log.SuppliesNeeded) : string.Empty);
            if (string.IsNullOrEmpty(response.ToString()))
                response.Append("No report was logged for " + kidName);
            return response.ToString();
        }

        private string ProcessLog(KidLog log, string action, string kidName)
        {
            var serv = new DayCareService();
            serv.SanitizeKidLog(log, false);
            var response = new StringBuilder();
            switch (action)
            {
                case Constants.Food:
                    //response = response.Append(kidName + " had ");
                    if (log.Foods != null && log.Foods.Any())
                    {
                        foreach (var food in log.Foods)
                        {
                            response.Append(food.WhatKidAte + (!string.IsNullOrEmpty(food.WhenKidAte) ? " at " : string.Empty)
                                + food.WhenKidAte + " ");
                        }
                    }
                    break;
                case Constants.Nap:
                    if (log.Naps != null && log.Naps.Any())
                    {
                        foreach (var nap in log.Naps)
                        {
                            response.Append(nap.NapTime + " ");
                        }
                    }
                    break;
                case Constants.Potty:
                    if (log.Pottys != null && log.Pottys.Any())
                    {
                        foreach (var potty in log.Pottys)
                        {
                            response.Append(!string.IsNullOrEmpty(potty.DiaperCheckTime) ? "Diaper check time at " + potty.DiaperCheckTime : string.Empty);
                            response.Append(!string.IsNullOrEmpty(potty.DiaperPottyType) ? ", diaper type " + potty.DiaperPottyType : string.Empty);
                            response.Append(!string.IsNullOrEmpty(potty.PottyTime) ? ", potty time at " + (potty.PottyTime == "0 am" ? "12" : potty.PottyTime) : string.Empty);
                            response.Append(" ");
                        }
                    }
                    break;
                case Constants.Activity:
                    if (log.Activities != null && log.Activities.Any())
                    {
                        foreach (var activity in log.Activities)
                        {
                            response.Append(activity.Activities + (!string.IsNullOrEmpty(activity.ActivityTime) ? " at " : string.Empty)
                                + activity.ActivityTime);
                            response.Append(" ");
                        }
                    }
                    break;
                case Constants.Mood:
                    if (log.Activities != null && log.Activities.Any())
                    {
                        foreach (var activity in log.Activities)
                        {
                            response.Append(activity.Mood + (!string.IsNullOrEmpty(activity.ActivityTime) ? " at " : string.Empty)
                                + activity.ActivityTime);
                            response.Append(" ");
                        }
                    }
                    break;
                case Constants.Sicks:
                    response.Append("problems/concerns " + log.ProblemsConcerns);
                    break;
                case Constants.Misc:
                    response.Append("comments: " + log.Comments);
                    break;
                case Constants.SuppliesSlot:
                    response.Append(!string.IsNullOrEmpty(log.SuppliesNeeded) ? ("Supplied Needed " + log.SuppliesNeeded) : "None");
                    break;
                default:
                    break;
            }
            if (string.IsNullOrEmpty(response.ToString()))
                response.Append("No report was logged for " + kidName);
            return response.ToString();
        }

        private SpeechletResponse ProcessLogRequestIntent(Intent intent, Session session)
        {
            // Get the slots from the intent.
            Dictionary<string, Slot> slots = intent.Slots;

            // Get the name slot from the list slots.
            Slot nameSlot = slots[Constants.NameSlot];
            string speechOutput = "";
            string kidNameForOutput = "";

            // Check for name and create output to user.
            if (nameSlot != null && !string.IsNullOrEmpty(nameSlot.Value))
            {
                string kidName = nameSlot.Value;
                kidNameForOutput = nameSlot.Value;
                //get kid data
                var assistant = new Assistant() { Subject = kidName, DayCareId = Id };
                assistant = _repo.GetKidByName(assistant);
                if (assistant.KidId != 0 && assistant.KidId > 0)
                {
                    Slot actionSlot = slots[Constants.ActionSlot];
                    //get action slot
                    if (actionSlot != null && !string.IsNullOrEmpty(actionSlot.Value))
                    {
                        var dayCareServ = new DayCareService();
                        assistant.Predicate = dayCareServ.GetInstantMessageType(actionSlot.Value);
                    }
                    if (string.IsNullOrEmpty(assistant.Predicate))
                        assistant.Predicate = Constants.Misc;
                    //get time slot
                    Slot timeSlot = slots[Constants.TimeSlot];
                    if (timeSlot != null && !string.IsNullOrEmpty(timeSlot.Value))
                    {
                        assistant.Time = TimeHelper(timeSlot.Value);
                    }
                    Slot fromTimeSlot = slots[Constants.FromTimeSlot];
                    if (fromTimeSlot != null && !string.IsNullOrEmpty(fromTimeSlot.Value))
                    {
                        assistant.Time = "From " + fromTimeSlot.Value;
                        Slot toTimeSlot = slots[Constants.ToTimeSlot];
                        if (toTimeSlot != null && !string.IsNullOrEmpty(toTimeSlot.Value))
                        {
                            assistant.Time = assistant.Time + " To " + toTimeSlot.Value;
                        }
                    }
                    //get decoration
                    Slot decoSlot = slots[Constants.DecorationSlot];
                    if (decoSlot != null && !string.IsNullOrEmpty(decoSlot.Value))
                    {
                        assistant.Object = decoSlot.Value;
                    }
                    if (assistant.Predicate == Constants.Sicks || assistant.Predicate == Constants.Misc)
                        assistant.Object = actionSlot.Value + " " + decoSlot.Value;
                    //get duration
                    Slot duraSlot = slots[Constants.DurationSlot];
                    if (duraSlot != null && !string.IsNullOrEmpty(duraSlot.Value) && string.IsNullOrEmpty(assistant.Time))
                    {
                        var time = XmlConvert.ToTimeSpan(duraSlot.Value);
                        if (time.Hours > 0)
                            assistant.Time = time.Hours.ToString() + " hours ";
                        if (time.Minutes > 0)
                            assistant.Time = string.IsNullOrEmpty(assistant.Time) ? time.Minutes.ToString() + " mins" :
                                (assistant.Time + time.Minutes.ToString() + " mins");
                    }
                    //get date 
                    var day = string.Empty;
                    Slot dateSlot = slots[Constants.DateSlot];
                    if (dateSlot != null && !string.IsNullOrEmpty(dateSlot.Value))
                    {
                        day = dateSlot.Value;
                    }
                    else
                        day = DateTime.Today.ToString("yyyy-MM-dd");
                    //get supplies
                    Slot supplySlot = slots[Constants.SuppliesSlot];
                    if (supplySlot != null && !string.IsNullOrEmpty(supplySlot.Value))
                    {
                        assistant.SuppliesNeeded = supplySlot.Value;
                    }
                    var assitntServ = new AssistantService();
                    var response = assitntServ.Process(assistant);
                    if (response == string.Empty)
                        speechOutput = String.Format("Status successfully logged for {0}", kidName);
                    else
                        speechOutput = Constants.Error;
                }
                else
                {
                    speechOutput = string.Format("No kid with name {0} exists in your account", kidName);
                    kidNameForOutput = "Error";
                }
            }
            else
            {
                // Render an error since we don't know what the users name is.
                speechOutput = "Please mention the kid's name in the request";
                kidNameForOutput = "Error";
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse(intent.Name, speechOutput, kidNameForOutput, true);
        }

        private SpeechletResponse BuildSpeechletResponse(string title, string output, string kidName, bool shouldEndSession)
        {
            // Create the Simple card content.
            SimpleCard card = new SimpleCard();
            card.Title = GetFriendlyTitleName(title, kidName);
            card.Content = output;

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            return response;
        }

        private SpeechletResponse BuildSpeechletResponseForLinkAccount(string title, string output, string kidName, bool shouldEndSession)
        {
            // Create the Simple card content.
            LinkAccountCard card = new LinkAccountCard();
            card.Title = GetFriendlyTitleName(title, kidName);

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            return response;
        }

        private string GetFriendlyTitleName(string intent, string kidName)
        {
            if (intent == "Welcome")
                return intent;
            if (intent == "Help")
                return intent;
            if (kidName != "Error")
            {
                if (intent == Constants.RequestLogIntent)
                    return string.Format("{0}'s Log", kidName);
                else if (intent == Constants.LogActionIntent)
                    return string.Format("{0}'s Action Logged", kidName);
                else if (intent == Constants.DayStatusIntent)
                    return string.Format("{0}'s Report", kidName);
                else if (intent == Constants.AddChildIntent)
                    return string.Format("Register {0}", kidName);
            }
            else
                return "Error";
            return intent;
        }

        private SpeechletResponse GetWelcomeResponse()
        {
            // Create the welcome message.
            string speechOutput =
                "Welcome. You can register a child by saying Alexa, ask giggles to add Emilio to my account. You can ask for child's activity by saying Alexa, ask giggles what did Hannah eat today. What would you like to do?";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Welcome", speechOutput, string.Empty, false);
        }

        private SpeechletResponse GetHelpIntentResponse()
        {
            // Create help message
            string speechOutput =
                "Register a child by saying Alexa, ask giggles to add Emilio to my account. Ask for child's activity by saying Alexa, ask giggles what did Hannah eat today. Report child's activity by saying Alexa, tell giggles Kyle took a nap. Also check on child by saying Alexa, ask giggles how was Hannah today. What would you like to do?";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Help", speechOutput, string.Empty, false);
        }

        private SpeechletResponse GetStopOrCancelIntentResponse()
        {
            // Create help message
            string speechOutput =
                "Thanks for trying Giggles";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Stop", speechOutput, string.Empty, true);
        }

        private SpeechletResponse GetExceptionWarningResponse(string output, string cardName, string cardType)
        {
            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            if (!string.IsNullOrEmpty(cardType) && cardType == "LinkAccount")
                return BuildSpeechletResponseForLinkAccount(cardName, output, string.Empty, true);
            return BuildSpeechletResponse(cardName, output, string.Empty, true);
        }

        private string TimeHelper(string time)
        {
            //convert ISO 8609 format to am/pm format
            var returnTime = string.Empty;
            if (time.Contains(":"))
            {
                var tList = time.Split(':');
                if (tList.Any())
                {
                    var hours = Convert.ToInt32(tList[0]);
                    if (hours >= 13)
                        returnTime = (hours - 12).ToString();
                    else
                        returnTime = (hours).ToString();
                    if (tList[1] != "00")
                        returnTime = returnTime + " " + tList[1];
                    if (hours >= 12)
                        returnTime = returnTime + " pm";
                    else
                        returnTime = returnTime + " am";
                }
            }
            else if (time == "EV" || time == "NI" || time == "MO" || time == "AF")
            {
                switch (time)
                {
                    case "MO":
                        returnTime = "Morning";
                        break;
                    case "AF":
                        returnTime = "afternoon";
                        break;
                    case "EV":
                        returnTime = "Evening";
                        break;
                    case "NI":
                        returnTime = "Night";
                        break;
                }
            }
            else if (string.IsNullOrEmpty(returnTime))
                returnTime = time;
            return returnTime;
        }

        #endregion        
    }
}
