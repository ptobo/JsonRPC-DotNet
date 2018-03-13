using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonRPC
{
    public class RequestBuilder
    {
        private Random rnd = new Random();
        private JObject json;
        public int id { set; get; }
        public String method { set; get; }
        public Object parameters { set; get; }

        public RequestBuilder()
        {
            Random random = new Random();
        }

        public RequestBuilder(String method)
        {
            this.method = method;
        }

        public RequestBuilder(String method, Object parameters)
        {
            this.method = method;
            this.parameters = parameters;
        }

        public String Build()
        {
            json = new JObject();
            json["jsonrpc"] = "2.0";
            json["method"] = method;
            json["id"] = (id == 0) ? rnd.Next(1000, 99999) : id;

            if (parameters != null)
            {
                if (parameters.GetType().Equals(typeof(Dictionary<String, String>)))
                {
                    Dictionary<String, String> prmts = (Dictionary<String, String>)parameters;

                    if (prmts.Count > 0)
                    {
                        var props = new JObject();

                        foreach (KeyValuePair<string, string> kvp in prmts)
                        {
                            props.Add(kvp.Key, kvp.Value);
                        }

                        json.Add(new JProperty("params", props));
                    }
                }

                if (parameters.GetType().Equals(typeof(String[])))
                {
                    String[] prmts = (String[])parameters;

                    if (prmts.Length > 0)
                    {
                        JArray props = new JArray();

                        foreach (var p in prmts)
                        {
                            props.Add(p);
                        }

                        json.Add(new JProperty("params", props));
                    }
                }

            }

            return JsonConvert.SerializeObject(json);
        }
    }
}
