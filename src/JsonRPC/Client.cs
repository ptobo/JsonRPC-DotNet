using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRPC
{
    public class Client
    {
        private Uri url;
        private CookieContainer cookies = new CookieContainer();
        private ICredentials credentials = null;
        private HttpWebRequest request = null;
        private HttpWebResponse response = null;
        public int timeout { set; get; }
        public RequestBuilder jsonRequest { set; get; }

        private void initRequest()
        {
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Timeout = (timeout == 0) ? 100 * 1000 : timeout * 1000;
                request.CookieContainer = cookies;

                if (credentials == null)
                {
                    credentials = CredentialCache.DefaultCredentials;
                }

                request.Credentials = credentials;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Client(Uri url, ICredentials credentials = null)
        {
            this.url = url;
            this.credentials = credentials;
        }

        public void Authentication(String user, String password)
        {

        }

        public String Execute(String method, Object parameters = null)
        {
            if (jsonRequest == null)
            {
                jsonRequest = new RequestBuilder(method, parameters);
            }

            String json = jsonRequest.Build();
            jsonRequest = null;

            initRequest();

            byte[] reqBytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = reqBytes.Length;

            try
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(reqBytes, 0, reqBytes.Length);
                    requestStream.Close();
                }

                response = (HttpWebResponse)request.GetResponse();
                cookies = request.CookieContainer;
                request = null;

                string result;

                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    result = rdr.ReadToEnd();
                    JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(result);
                    JToken value;

                    if (jsonResponse.TryGetValue("error", out value))
                    {
                        throw new Exception("Error " + value["code"] + ": " + value["message"]);
                    }
                    else if (jsonResponse.TryGetValue("result", out value))
                    {
                        return value.ToString();
                    }
                    else
                    {
                        throw new Exception("Unexpected error: " + result);
                    }
                }
            }
            catch (WebException wex)
            {
                throw wex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
