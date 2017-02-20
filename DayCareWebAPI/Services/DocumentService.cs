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
using System.Net.Http;
using System.Net.Http.Headers;

namespace DayCareWebAPI.Services
{
    public class DocumentService
    {
        private readonly DayCareRepository _repo;

        public DocumentService()
        {
            this._repo = new DayCareRepository();
        }

        public DayCareSnap SaveSnap(Stream data, string fileName, Guid dayCareId, long? length)
        {
            try
            {
                var path = string.Format(Constants.FtpSnapsPath, dayCareId);
                if (!CheckDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd))
                {
                    CreateDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd);
                }
                if (CheckFileCountLimit(path, Constants.FtpUser, Constants.FtpPswd) != 5)
                {
                    if (!CheckFileExistsInDirectory(path + "/" + fileName, Constants.FtpUser, Constants.FtpPswd))
                    {
                        using (StreamReader stream = new StreamReader(data))
                        {
                            //byte[] buffer = Encoding.Unicode.GetBytes(stream.ReadToEnd());
                            byte[] buffer = new byte[length.Value];
                            data.Read(buffer, 0, (int)length.Value);

                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/" + fileName);
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.Credentials = new NetworkCredential(Constants.FtpUser, Constants.FtpPswd);
                            Stream reqStream = request.GetRequestStream();
                            reqStream.Write(buffer, 0, buffer.Length);
                            reqStream.Close();
                            return _repo.InsertDayCareSnap(dayCareId, fileName);
                        }
                    }
                    else
                    {
                        return new DayCareSnap() { Error = "Image already exists! Rename the image." };
                    }
                }
                else
                {
                    return new DayCareSnap() { Error = "Sorry! Maximum of five snaps allowed." };
                }
            }
            catch (Exception)
            {
                return new DayCareSnap() { Error = Constants.Error };
            }
        }

        public DayCareSnap SaveSnapBase64(byte[] data, string fileName, Guid dayCareId)
        {
            try
            {
                var path = string.Format(Constants.FtpSnapsPath, dayCareId);
                if (!CheckDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd))
                {
                    CreateDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd);
                }
                if (CheckFileCountLimit(path, Constants.FtpUser, Constants.FtpPswd) != 5)
                {
                    if (!CheckFileExistsInDirectory(path + "/" + fileName, Constants.FtpUser, Constants.FtpPswd))
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/" + fileName);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(Constants.FtpUser, Constants.FtpPswd);
                        Stream reqStream = request.GetRequestStream();
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                        return _repo.InsertDayCareSnap(dayCareId, fileName);
                    }
                    else
                    {
                        return new DayCareSnap() { Error = "Image already exists! Rename the image." };
                    }
                }
                else
                {
                    return new DayCareSnap() { Error = "Sorry! Maximum of five snaps allowed." };
                }
            }
            catch (Exception)
            {
                return new DayCareSnap() { Error = Constants.Error };
            }
        }

        public Document SaveDocument(Stream data, string title, string fileName, string type, Guid dayCareId, long? length)
        {
            try
            {
                var path = string.Format(Constants.FtpSnapsPath, dayCareId);
                if (!CheckDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd))
                {
                    CreateDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd);
                }
                if (!CheckFileExistsInDirectory(path + "/" + fileName, Constants.FtpUser, Constants.FtpPswd))
                {
                    using (StreamReader stream = new StreamReader(data))
                    {
                        //byte[] buffer = Encoding.Unicode.GetBytes(stream.ReadToEnd());
                        byte[] buffer = new byte[length.Value];
                        data.Read(buffer, 0, (int)length.Value);

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/" + fileName);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(Constants.FtpUser, Constants.FtpPswd);
                        Stream reqStream = request.GetRequestStream();
                        reqStream.Write(buffer, 0, buffer.Length);
                        reqStream.Close();
                        return _repo.InsertDocument(new Document() { DayCareId = dayCareId, Name = fileName, Type = fileName.Split('.')[1], MimeType = type, Title = title, Error = string.Empty });
                    }
                }
                else
                {
                    return new Document() { Error = "Document already exists! Rename the document." };
                }
            }
            catch (Exception)
            {
                return new Document() { Error = Constants.Error };
            }
        }

        public HttpResponseMessage DownloadDocument(int id)
        {
            var doc = _repo.GetDocument(id);
            var filePath = HttpContext.Current.Server.MapPath("~/ftp/documents/" + doc.DayCareId.ToString() + "/" + doc.Name);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(doc.MimeType);
            result.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, stream.Length);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = doc.Name
            };
            result.Headers.Add("x-filename", doc.Name);
            return result;
        }

        public void CreateDocuDirectory(string path, string user, string pswd)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                request.Credentials = new NetworkCredential(user, pswd);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
            }
        }

        public bool CheckDocuDirectory(string path, string user, string pswd)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/");
                request.Credentials = new NetworkCredential(user, pswd);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                return false;
            }
        }

        public int CheckFileCountLimit(string path, string user, string pswd)
        {
            try
            {
                var count = 0;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                request.Credentials = new NetworkCredential(user, pswd);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (StreamReader resp = new StreamReader(response.GetResponseStream()))
                {
                    var line = resp.ReadLine();
                    while (line != null)
                    {
                        count++;
                        line = resp.ReadLine();
                    }
                }
                return count;
            }
            catch (WebException)
            {
                return 0;
            }
        }

        private bool CheckFileExistsInDirectory(string path, string user, string pswd)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                request.Credentials = new NetworkCredential(user, pswd);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        public List<Document> GetDocuments(Guid dayCareId)
        {
            return _repo.GetDocuments(dayCareId);
        }

        public string DeleteDocument(int id)
        {
            return _repo.DeleteDocument(id);
        }
    }
}
