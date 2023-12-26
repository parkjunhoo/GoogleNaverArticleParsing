using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace injoonbot
{
    public class HttpXmlParse
    {

        HttpWebRequest request = null;
        HttpWebResponse response = null;

        public string parse(string url ,string query, Dictionary<string, string> header = null)
        {
            string result = "";
            request = (HttpWebRequest)WebRequest.Create(url + query);
            if(header != null)
            {
                foreach(KeyValuePair<string, string> i in header)
                {
                    request.Headers.Add(i.Key, i.Value);
                }
            }
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode.ToString() == "OK")
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                result = reader.ReadToEnd();
            }
            request = null;
            response = null;
            return result;
        }
    }
}