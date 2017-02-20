using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using System.Net;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Web;
//using Microsoft.Office.Interop.Word;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;

namespace DayCareWebAPI.Controllers
{
    public class CustomReportController : ApiController
    {
        private readonly DayCareService _rep;

        public CustomReportController()
        {
            _rep = new DayCareService();
        }

        // GET kid log for today
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/customReport/GetCustomReportToday/{kidId}/{dayCareId}")]
        public IHttpActionResult GetCustomReportToday(int kidId, Guid dayCareId)
        {
            var customReport = _rep.GetCustomReport(dayCareId, kidId, DateTime.Today.ToString("yyyy-MM-dd"));
            return Ok(customReport);
        }

        // GET kid log on a day
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/customReport/GetCustomReportOnADay/{kidId}/{id}/{day}")]
        public IHttpActionResult GetCustomReportOnADay(int kidId, Guid id, string day)
        {
            var customReport = _rep.GetCustomReportOnADay(kidId, id, day);
            return Ok(customReport);
        }

        // GET kid log for today
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/customReport/SaveCustomReport")]
        public HttpResponseMessage SaveCustomReport(CustomReport customReport)
        {
            var response = _rep.SaveCustomReport(customReport);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // upload custom report 
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/customReport/UploadCustomReport")]
        public async Task<IHttpActionResult> UploadCustomReport()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Content(HttpStatusCode.InternalServerError, "Not a Valid word document!");
            }
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                Stream customRepData = await file.ReadAsStreamAsync();
                var questions = UploadCustomReportUsingAspose(customRepData);
                return Ok(questions);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, Constants.Error);
            }
        }

        private List<Question> UploadCustomReportUsingAspose(Stream stream)
        {
            var questions = new List<Question>();
            var j = 0;
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);

            //remove any tables
            foreach (Node item in doc.GetChildNodes(NodeType.Table, true))
                item.Remove();

            Range docRange = doc.Range;
            if (docRange != null)
            {
                var listData = docRange.Text.Split('\r');
                if (listData != null && listData.Any())
                {
                    foreach (var line in listData)
                    {
                        if (line.Contains("Aspose.Words") || line.Contains("\u001f") || line.Contains("\f") || line.Length < 3 || line.Replace(" ", "").Length < 3)
                            continue;
                        //check for question answer
                        if (line.Contains("_"))
                        {
                            var list = line.Split('_');
                            list = list.Where((source, index) => list[index] != "").ToArray();
                            if (list.Length == 1)
                            {
                                var options = CheckOptionQuestion(list[0].Replace(":", ""));
                                if (!options.Any())
                                {
                                    var question = GetQuestion(Constants.QAType, j, (int)Constants.QuestionTypes.Heading, new List<string> { list[0].Replace(":", ""), "" }, Constants.QATemplate);
                                    questions.Add(question);
                                    j++;
                                    continue;
                                }
                                else
                                {
                                    var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { options[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
                                    question.options = new List<Option>();
                                    for (var k = 1; k < options.Count(); k++)
                                    {
                                        question.options.Add(new Option() { value = options[k], text = "option" });
                                    }
                                    questions.Add(question);
                                    j++;
                                    continue;
                                }
                            }
                            else if (list.Length == 2)
                            {
                                var options = CheckOptionQuestion(list[1].Replace(":", ""));
                                if (!options.Any())
                                {
                                    var question = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[0].Replace(":", ""), list[1].Replace(":", "") }, Constants.PairedQATemplate);
                                    questions.Add(question);
                                    j++;
                                    continue;
                                }
                                else
                                {
                                    var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { list[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
                                    question.options = new List<Option>();
                                    for (var k = 0; k < options.Count(); k++)
                                    {
                                        question.options.Add(new Option() { value = options[k], text = "option" });
                                    }
                                    questions.Add(question);
                                    j++;
                                    continue;
                                }
                            }
                            else if (list.Length == 4)
                            {
                                var question = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[0].Replace(":", ""), list[1].Replace(":", "") }, Constants.PairedQATemplate);
                                questions.Add(question);
                                j++;

                                var question1 = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[2].Replace(":", ""), list[3].Replace(":", "") }, Constants.PairedQATemplate);
                                questions.Add(question1);
                                j++;
                                continue;
                            }
                        }
                        else
                        {
                            var options = CheckOptionQuestion(line.Replace(":", ""));
                            if (!options.Any())
                            {
                                //checking for main heading
                                var question = GetQuestion(Constants.HeadingType, j, (int)Constants.QuestionTypes.Heading, new List<string> { line, "" }, Constants.HeadingTemplate);
                                questions.Add(question);
                                j++;
                                continue;
                            }
                            else
                            {
                                var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { options[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
                                question.options = new List<Option>();
                                for (var k = 1; k < options.Count(); k++)
                                {
                                    question.options.Add(new Option() { value = options[k], text = "option" });
                                }
                                questions.Add(question);
                                j++;
                                continue;
                            }
                        }

                    }
                }
            }
            return questions;
        }

        private Question GetQuestion(string type, int trackId, int id, List<string> values, string template)
        {
            var question = new Question() { id = id, trackId = trackId, type = type, template = template, values = values };
            return question;
        }

        private List<string> CheckOptionQuestion(string input)
        {
            //check for \t instead of blank space
            var sep = "  ";
            var sepAddOn = " ";
            if (input.Contains('\t'))
            {
                sep = sepAddOn = "\t";
            }
            var options = new List<string>();
            for (var i = 1; i < 10; i++)
            {
                if (input.Contains(sep))
                {
                    var list = input.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                    if (list.Length < 3)
                    {
                        sep = sep + sepAddOn;
                        continue;
                    }
                    for (var j = 0; j < list.Length; j++)
                    {
                        //get the question from first item
                        if (j == 0)
                        {
                            var items = GetQuestionFromOptions(list[j], sep, sepAddOn);
                            if (items.Any())
                                options.AddRange(items);
                        }
                        options.Add(list[j]);
                    }
                    break;
                }
                else
                {
                    sep = sep + " ";
                }
            }
            return options;
        }

        private List<string> GetQuestionFromOptions(string input, string sep, string sepAddOn)
        {
            var items = new List<string>();
            for (var i = 1; i < 5; i++)
            {
                var list = input.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                if (list.Length == 2)
                {
                    items.AddRange(list);
                    break;
                }
                sep = sep + sepAddOn;
                continue;
            }
            return items;
        }

        //private List<Question> UploadCustomReportUsingWord(string fileName)
        //{
        //    Application application = new Application();
        //    Document document = application.Documents.Open(HttpContext.Current.Server.MapPath("~/Buffer/") + fileName);
        //    var questions = new List<Question>();
        //    //delete any tables since they are tough to read
        //    foreach (Table tbl in document.Tables)
        //        tbl.Delete();
        //    var j = 0;
        //    for (int i = 0; i < document.Paragraphs.Count; i++)
        //    {
        //        string line = document.Paragraphs[i + 1].Range.Text.Trim();
        //        dynamic font = document.Paragraphs[i + 1].Range.get_Style();
        //        if (line != string.Empty && line != "\a")
        //        {
        //            //check for question answer
        //            if (line.Contains("_"))
        //            {
        //                var list = line.Split('_');
        //                list = list.Where((source, index) => list[index] != "").ToArray();
        //                if (list.Length == 1)
        //                {
        //                    var options = CheckOptionQuestion(list[0].Replace(":", ""));
        //                    if (!options.Any())
        //                    {
        //                        var question = GetQuestion(Constants.QAType, j, (int)Constants.QuestionTypes.Heading, new List<string> { list[0].Replace(":", ""), "" }, Constants.QATemplate);
        //                        questions.Add(question);
        //                        j++;
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { options[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
        //                        question.options = new List<Option>();
        //                        for (var k = 1; k < options.Count(); k++)
        //                        {
        //                            question.options.Add(new Option() { value = options[k], text = "option" });
        //                        }
        //                        questions.Add(question);
        //                        j++;
        //                        continue;
        //                    }
        //                }
        //                else if (list.Length == 2)
        //                {
        //                    var options = CheckOptionQuestion(list[1].Replace(":", ""));
        //                    if (!options.Any())
        //                    {
        //                        var question = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[0].Replace(":", ""), list[1].Replace(":", "") }, Constants.PairedQATemplate);
        //                        questions.Add(question);
        //                        j++;
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { list[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
        //                        question.options = new List<Option>();
        //                        for (var k = 0; k < options.Count(); k++)
        //                        {
        //                            question.options.Add(new Option() { value = options[k], text = "option" });
        //                        }
        //                        questions.Add(question);
        //                        j++;
        //                        continue;
        //                    }
        //                }
        //                else if (list.Length == 4)
        //                {
        //                    var question = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[0].Replace(":", ""), list[1].Replace(":", "") }, Constants.PairedQATemplate);
        //                    questions.Add(question);
        //                    j++;

        //                    var question1 = GetQuestion(Constants.PairedQAType, j, (int)Constants.QuestionTypes.PairedQuestionAndAnswer, new List<string> { list[2].Replace(":", ""), list[3].Replace(":", "") }, Constants.PairedQATemplate);
        //                    questions.Add(question1);
        //                    j++;
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                var options = CheckOptionQuestion(line.Replace(":", ""));
        //                if (!options.Any())
        //                {
        //                    if (font.Font.Size >= 10 || font.Font.Bold == true)
        //                    {
        //                        //checking for main heading
        //                        var question = GetQuestion(Constants.HeadingType, j, (int)Constants.QuestionTypes.Heading, new List<string> { line, "" }, Constants.HeadingTemplate);
        //                        questions.Add(question);
        //                        j++;
        //                        continue;
        //                    }
        //                }
        //                else
        //                {
        //                    var question = GetQuestion(Constants.QAnsOptionsType, j, (int)Constants.QuestionTypes.QuestionAnswerFollowedByOptions, new List<string> { options[0].Replace(":", ""), "" }, Constants.QAnsOptionsTypeTemplate);
        //                    question.options = new List<Option>();
        //                    for (var k = 1; k < options.Count(); k++)
        //                    {
        //                        question.options.Add(new Option() { value = options[k], text = "option" });
        //                    }
        //                    questions.Add(question);
        //                    j++;
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //    application.Documents.Close();
        //    GC.Collect();
        //    return questions;
        //}

        //private Question GetQuestion(string type, int trackId, int id, List<string> values, string template)
        //{
        //    var question = new Question() { id = id, trackId = trackId, type = type, template = template, values = values };
        //    return question;
        //}

        //private List<string> CheckOptionQuestion(string input)
        //{
        //    //check for \t instead of blank space
        //    var sep = "  ";
        //    var sepAddOn = " ";
        //    if (input.Contains('\t'))
        //    {
        //        sep = sepAddOn = "\t";
        //    }
        //    var options = new List<string>();
        //    for (var i = 1; i < 10; i++)
        //    {
        //        if (input.Contains(sep))
        //        {
        //            var list = input.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);
        //            if (list.Length < 3)
        //            {
        //                sep = sep + sepAddOn;
        //                continue;
        //            }
        //            for (var j = 0; j < list.Length; j++)
        //            {
        //                //get the question from first item
        //                if (j == 0)
        //                {
        //                    var items = GetQuestionFromOptions(list[j], sep, sepAddOn);
        //                    if (items.Any())
        //                        options.AddRange(items);
        //                }
        //                options.Add(list[j]);
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            sep = sep + " ";
        //        }
        //    }
        //    return options;
        //}

        //private List<string> GetQuestionFromOptions(string input, string sep, string sepAddOn)
        //{
        //    var items = new List<string>();
        //    for (var i = 1; i < 5; i++)
        //    {
        //        var list = input.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);
        //        if (list.Length == 2)
        //        {
        //            items.AddRange(list);
        //            break;
        //        }
        //        sep = sep + sepAddOn;
        //        continue;
        //    }
        //    return items;
        //}

        //upload 
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Post(CustomReport customReport)
        {
            var response = _rep.CreateCustomReportQuestions(customReport);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
