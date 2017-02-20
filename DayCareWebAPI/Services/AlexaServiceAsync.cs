using AlexaSkillsKit.Speechlet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using DayCareWebAPI.Repository;
using AlexaSkillsKit.UI;
using AlexaSkillsKit.Slu;
using DayCareWebAPI.Models;

namespace DayCareWebAPI.Services
{
    public class AlexaServiceAsync : SpeechletAsync
    {
        public Guid DayCareId { get; set; }

        private readonly DayCareRepository _repo;
        public AlexaServiceAsync()
        {
            this._repo = new DayCareRepository();
        }

        public override Task<SpeechletResponse> OnIntentAsync(IntentRequest intentRequest, Session session)
        {
            // Get intent from the request object.
            Intent intent = intentRequest.Intent;
            string intentName = (intent != null) ? intent.Name : null;

            _repo.LogAlexaSession(DayCareId, intentRequest.RequestId, session.SessionId, "On Intent", intentName);

            // Note: If the session is started with an intent, no welcome message will be rendered;
            // rather, the intent specific response will be returned.
            if (Constants.LogActionIntent.Equals(intentName))
            {
                return ProcessLogRequestIntent(intent, session);
            }
            else
            {
                throw new SpeechletException("Invalid Intent");
            }
        }

        private Task<SpeechletResponse> ProcessLogRequestIntent(Intent intent, Session session)
        {
            // Get the slots from the intent.
            Dictionary<string, Slot> slots = intent.Slots;

            // Get the name slot from the list slots.
            Slot nameSlot = slots[Constants.NameSlot];
            string speechOutput = "";

            // Check for name and create output to user.
            if (nameSlot != null)
            {
                string kidName = nameSlot.Value;
                //get kid data
                var assistant = new Assistant() { Subject = kidName, DayCareId = DayCareId };
                assistant = _repo.GetKidByName(assistant);
                Slot actionSlot = slots[Constants.ActionSlot];
                //get action slot
                if (actionSlot != null)
                {
                    var dayCareServ = new DayCareService();
                    assistant.Predicate = dayCareServ.GetInstantMessageType(actionSlot.Value);
                }
                //get time slot
                Slot timeSlot = slots[Constants.TimeSlot];
                if (timeSlot != null)
                {
                    assistant.Time = timeSlot.Value;
                }
                Slot fromTimeSlot = slots[Constants.FromTimeSlot];
                if (fromTimeSlot != null)
                {
                    assistant.Time = "From " + fromTimeSlot.Value;
                    Slot toTimeSlot = slots[Constants.ToTimeSlot];
                    assistant.Time = assistant.Time + " To " + toTimeSlot.Value;
                }
                //get decoration
                Slot decoSlot = slots[Constants.DecorationSlot];
                if (decoSlot != null)
                {
                    assistant.Message = decoSlot.Value;
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
                // Render an error since we don't know what the users name is.
                speechOutput = "Some thing went wrong, please try again";
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse(intent.Name, speechOutput, true);
        }

        public override Task<SpeechletResponse> OnLaunchAsync(LaunchRequest launchRequest, Session session)
        {
            _repo.LogAlexaSession(DayCareId, launchRequest.RequestId, session.SessionId, "On Launch", string.Empty);
            return GetWelcomeResponse();
        }

        public override Task OnSessionEndedAsync(SessionEndedRequest sessionEndedRequest, Session session)
        {
            _repo.LogAlexaSession(DayCareId, sessionEndedRequest.RequestId, session.SessionId, "Session Ended", string.Empty);
            return null;
        }

        public override Task OnSessionStartedAsync(SessionStartedRequest sessionStartedRequest, Session session)
        {
            _repo.LogAlexaSession(DayCareId, sessionStartedRequest.RequestId, session.SessionId, "Session Started", string.Empty);
            return null;
        }

        private Task<SpeechletResponse> GetWelcomeResponse()
        {
            // Create the welcome message.
            string speechOutput =
                "Welcome to GigglesWare.";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Welcome", speechOutput, false);
        }

        private Task<SpeechletResponse> BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content.
            SimpleCard card = new SimpleCard();
            card.Title = String.Format("SessionSpeechlet - {0}", title);
            card.Subtitle = String.Format("SessionSpeechlet - Sub Title");
            card.Content = String.Format("SessionSpeechlet - {0}", output);

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            return Task.FromResult(response);
        }
    }
}