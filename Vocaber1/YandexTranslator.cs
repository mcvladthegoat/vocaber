using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;

namespace Vocaber1
{
    class YandexTranslator
    {
        List<string> apiKeys;
        int currentApiKey;
        public YandexTranslator(List<string> apiKeys)
        {
            this.apiKeys = apiKeys;
            this.currentApiKey = 0;
        }

        public string RepeatRequest(string s, string lang) {
            if (currentApiKey < this.apiKeys.Count)
            {
                this.currentApiKey++;
                return this.Translate(s, lang);
            }
            return "";
        }

        public string Translate(string s, string lang)
        {
            if (s.Length > 0)
            {
                try
                {
                    WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                        + "key=" + this.apiKeys[currentApiKey]
                        + "&text=" + HttpUtility.UrlEncode(s)
                        + "&options=5&lang=" + lang); 

                    WebResponse response = request.GetResponse();

                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        string line;

                        if ((line = stream.ReadLine()) != null)
                        {
                            Translation translation = JsonConvert.DeserializeObject<Translation>(line);
                            if (translation.code != "200")
                            {
                                return this.RepeatRequest(s, lang);
                            }
                            s = "";

                            foreach (string str in translation.text)
                            {
                                s += str;
                            }
                        }
                    }

                    return s;
                }
                catch(WebException e) {
                    return this.RepeatRequest(s, lang);
                }
            }
            else
                return "";
        }
    }

    class Translation
    {
        public string code { get; set; }
        public string lang { get; set; }
        public string[] text { get; set; }
    }
}