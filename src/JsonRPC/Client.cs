using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRPC
{
    public class Client
    {
        private Uri url;
        private CookieContainer cookies = new CookieContainer();
        private ICredentials credentials = null;
        private HttpClient request;
        private HttpClientHandler handler;
        private HttpResponseMessage response;
        public int timeout { set; get; }
        public RequestBuilder jsonRequest { set; get; }

        private void initRequest()
        {
            try
            {
                if (credentials == null)
                {
                    credentials = CredentialCache.DefaultCredentials;
                }

                handler = new HttpClientHandler();
                handler.UseCookies = true;
                handler.CookieContainer = cookies;
                handler.Credentials = credentials;

                request = new HttpClient(handler);
                request.BaseAddress = url;
                request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Timeout = new TimeSpan((timeout == 0) ? 100 * 1000 : timeout * 1000);
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

        public async Task<string> Execute(String method, Object parameters = null)
        {
            if (jsonRequest == null)
            {
                jsonRequest = new RequestBuilder(method, parameters);
            }

            String json = jsonRequest.Build();
            jsonRequest = null;

            try
            {
                initRequest();

                response = await request.PostAsync(url.ToString(), new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    cookies = handler.CookieContainer;
                    string result;

                    result = await response.Content.ReadAsStringAsync();

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
                else
                {
                    throw new Exception("Unexpected error");
                }
            }
            catch (HttpRequestException hre)
            {
                throw hre;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
