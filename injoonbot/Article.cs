using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace injoonbot
{
    public class Article
    {
        public string title;
        public string link;
        public string desc;
        public DateTime date;

        public Article(string title, string link, string desc, DateTime date)
        {
            this.title = title;
            this.link = link;
            this.desc = desc;
            this.date = date;
        }

        public static void ArticleListAddXmlList(XmlNodeList xmlList, ref List<Article> articleList, DateTime min, DateTime max, string dateNodeName,string linkNodeName, string dateForm)
        {
            foreach (XmlNode i in xmlList)
            {
                Article a = xmlToArticle(i, min, max, dateNodeName,linkNodeName, dateForm);
                if(a != null) articleList.Add(a);
            }
        }

        public static Article xmlToArticle(XmlNode xml, DateTime min, DateTime max, string dateNodeName,string linkNodeName, string dateForm)
        {
            DateTime date = new DateTime();
            if (DateTime.TryParseExact(xml[dateNodeName].InnerText, dateForm, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                if(date.Date >= min.Date && date.Date <= max.Date)
                {
                    return new Article(
                        xml["title"].InnerText,
                        xml[linkNodeName].InnerText,
                        xml["description"].InnerText,
                        date
                    );
                }
            }

            return null;
        }
    }
}
