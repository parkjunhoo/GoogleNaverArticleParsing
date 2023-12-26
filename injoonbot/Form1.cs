using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.LinkLabel;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace injoonbot
{
    public partial class Form1 : Form
    {
        HttpXmlParse hxp = new HttpXmlParse();
        List<Article> articleList = new List<Article>();
        XmlDocument xml = new XmlDocument();
        XmlNodeList xmlList;

        ChromeDriverService service;
        ChromeOptions options;
        ChromeDriver driver;

        int naverPageCount = 1;
        int googlePageCount = 0;

        public bool IsHtml(string input)
        {
            return input.Contains("<") && input.Contains(">");
        }
        public string getTextNodes(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return string.Join(" ", htmlDoc.DocumentNode.SelectNodes("//text()")?.Select(node => node.InnerText.Trim()) ?? new List<string> { html });
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            service = ChromeDriverService.CreateDefaultService(System.IO.Directory.GetCurrentDirectory(), "chromedriver.exe");
            options = new ChromeOptions();
            //options.AddArgument("--headless"); // GUI 없이 실행
            driver = new ChromeDriver(service, options);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                Dictionary<string, string> naverAPIHeader = new Dictionary<string, string>();
                naverAPIHeader.Add("X-Naver-Client-Id", "XJecmoofCfy9IQ0Kswr_");
                naverAPIHeader.Add("X-Naver-Client-Secret", "Y3VDwHZDt6");
                string naverQuery = string.Format("?query={0}&start=1&display={1}sort={2}&pd=3&ds=2023.12.24&de=2023.12.24", "동국대학교", "100", "date");
                string naverApiResult = hxp.parse("https://openapi.naver.com/v1/search/news.xml", naverQuery, naverAPIHeader);

                xml.LoadXml(naverApiResult);
                xmlList = xml.GetElementsByTagName("item");
                Article.ArticleListAddXmlList(xmlList, ref articleList, dateTimePicker1.Value, dateTimePicker2.Value, "pubDate", "originallink" , "ddd, dd MMM yyyy HH:mm:ss zzz");

            }
            if (checkBox3.Checked == true)
            {
                string googleQuery = "?q=%EB%8F%99%EA%B5%AD%EB%8C%80%20when%3A7d&hl=ko&gl=KR&ceid=KR%3Ako";
                string googleResult = hxp.parse("https://news.google.com/rss/search", googleQuery);

                xml.LoadXml(googleResult);
                xmlList = xml.GetElementsByTagName("item");
                Article.ArticleListAddXmlList(xmlList, ref articleList, dateTimePicker1.Value, dateTimePicker2.Value, "pubDate","link", "ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            }
            articleListToClipBoard();
        }

        public void articleListToClipBoard()
        {
            articleList = articleList.OrderByDescending(article => article.date).ToList();
            string clipboardText = "제목\t내용\t날짜\t링크\n";
            var htmlDoc = new HtmlDocument();
            int count = 0;
            foreach (Article a in articleList)
            {
                if (checkBox1.Checked == true && !a.title.Contains("동국대")) continue;
                if (IsHtml(a.title))
                {
                    a.title = getTextNodes(a.title);
                    a.title = WebUtility.HtmlDecode(a.title);
                }
                if (IsHtml(a.desc))
                {
                    a.desc = getTextNodes(a.desc);
                    a.desc = WebUtility.HtmlDecode(a.desc);
                }

                a.title = a.title.Replace("&lt;", "<");
                a.title = a.title.Replace("&gt;", ">");
                a.title = a.title.Replace("&quot;", "\"");
                a.title = a.title.Replace("&#39;", "\'");
                a.title = a.title.Replace("&amp;", "&");
                a.title = a.title.Replace("\r\n", "");
                a.title = a.title.Replace("\r", "");
                a.title = a.title.Replace("\n", "");

                a.desc = a.desc.Replace("&lt;", "<");
                a.desc = a.desc.Replace("&gt;", ">");
                a.desc = a.desc.Replace("&quot;", "\"");
                a.desc = a.desc.Replace("&#39;", "\'");
                a.desc = a.desc.Replace("&amp;", "&");
                a.desc = a.desc.Replace("\r\n", "");
                a.desc = a.desc.Replace("\r", "");
                a.desc = a.desc.Replace("\n", "");


                clipboardText += a.title + "\t" + a.desc + "\t" + a.date + "\t" + a.link + "\n";
                count++;
            }
            Clipboard.SetText(clipboardText);
            MessageBox.Show(count.ToString() + "개의 기사가 클립보드에 복사되었습니다.", "기사 가져오기 성공.");
            articleList.Clear();
        }

        public void googleParsing(string url)
        {
            driver.Navigate().GoToUrl(url);
            System.Threading.Thread.Sleep(1);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);
            //string googleUrl = url;
            //HtmlWeb web = new HtmlWeb();
            //HtmlDocument googleDoc = web.Load(googleUrl);

            HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes("//div[@class='SoaBEf']");
            if (articleNodes == null) return;
            foreach(HtmlNode i in articleNodes)
            {
                string title = i.SelectSingleNode(".//div[@class='n0jPhd ynAwRc MBeuO nDgy9d']").InnerText;
                string link = i.SelectSingleNode(".//a[@class='WlydOe']").GetAttributeValue("href", "");
                string desc = i.SelectSingleNode(".//div[@class='GI74Re nDgy9d']").InnerText;
                string infoText = i.SelectSingleNode(".//div[@class='OSrXXb rbYSKb LfVVr']").FirstChild.InnerText;
                DateTime date = DateTime.Now;
                if (DateTime.TryParseExact(infoText, "yyyy. MM. dd.", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                }
                if (infoText.Contains("개월 전"))
                {
                    date = DateTime.Today.AddMonths(Convert.ToInt32(infoText.Replace("개월 전", "")) * -1);
                }
                if (infoText.Contains("주 전"))
                {
                    date = DateTime.Today.AddDays(Convert.ToInt32(infoText.Replace("주 전", "")) * -7);
                }
                if (infoText.Contains("일 전"))
                {
                    date = DateTime.Today.AddDays(Convert.ToInt32(infoText.Replace("일 전", "")) * -1);
                }
                if (infoText.Contains("시간 전"))
                {
                    date = DateTime.Now.AddHours(Convert.ToInt32(infoText.Replace("시간 전", "")) * -1);
                }
                if (infoText.Contains("분 전"))
                {
                    date = DateTime.Now.AddMinutes(Convert.ToInt32(infoText.Replace("분 전", "")) * -1);
                }
                if (infoText.Contains("초 전"))
                {
                    date = DateTime.Now.AddSeconds(Convert.ToInt32(infoText.Replace("초 전", "")) * -1);
                }
                articleList.Add(new Article(title, link, desc, date));
            }
            HtmlNode nextBtn = doc.DocumentNode.SelectSingleNode("//a[@id='pnnext']");
            if(nextBtn != null)
            {
                googlePageCount+=10;
                string startDate = dateTimePicker1.Value.ToString("MM'%2F'dd'%2F'yyyy");
                string endDate = dateTimePicker2.Value.ToString("MM'%2F'dd'%2F'yyyy");
                string googleUrl = string.Format("https://www.google.com/search?q=%EB%8F%99%EA%B5%AD%EB%8C%80&sca_esv=593812750&biw=1920&bih=945&sxsrf=AM9HkKmRIMJHTzzRqP1ci0-5d1xZMDJQ-g%3A1703618143900&source=lnt&&start={0}&tbs=cdr%3A1%2Ccd_min%3A{1}%2Ccd_max%3A{2}&tbm=nws",
                    googlePageCount, startDate, endDate);
                googleParsing(googleUrl);
            }
        }
        public void naverParsing(string url)
        {
            driver.Navigate().GoToUrl(url);
            System.Threading.Thread.Sleep(1);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);
            try
            {
                IWebElement stopButton = driver.FindElement(By.ClassName("btn_open"));
                if (stopButton != null) stopButton.Click();
            }
            catch(Exception e)
            {

            }
            //string naverUrl = url;
            //HtmlWeb web = new HtmlWeb();
            //HtmlDocument naverDoc = web.Load(naverUrl);
            HtmlNode ulNode = doc.DocumentNode.SelectSingleNode("//ul[@class='list_news']");
            if (ulNode == null)
            {
                return;
            }
            HtmlNodeCollection liNodes = ulNode.SelectNodes(".//li");
            foreach (HtmlNode a in liNodes)
            {
                HtmlNode titleNode = a.SelectSingleNode(".//a[@class='news_tit']");
                string title = titleNode.GetAttributeValue("title", "");

                string link = titleNode.GetAttributeValue("href", "");

                HtmlNode descNode = a.SelectSingleNode(".//a[@class='api_txt_lines dsc_txt_wrap']");
                string desc = descNode.InnerText;

                HtmlNodeCollection infoNodes = a.SelectNodes(".//span[@class='info']");
                DateTime date = DateTime.Now;
                foreach (HtmlNode info in infoNodes)
                {
                    string infoText = info.InnerText;
                    if (DateTime.TryParseExact(infoText, "yyyy.MM.dd.", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    {
                        break;
                    }
                    if (infoText.Contains("주 전"))
                    {
                        date = DateTime.Today.AddDays(Convert.ToInt32(infoText.Replace("주 전", "")) * -7);
                        break;
                    }
                    if (infoText.Contains("일 전"))
                    {
                        date = DateTime.Today.AddDays(Convert.ToInt32(infoText.Replace("일 전", "")) * -1);
                        break;
                    }
                    if (infoText.Contains("시간 전"))
                    {
                        date = DateTime.Now.AddHours(Convert.ToInt32(infoText.Replace("시간 전", "")) * -1);
                        break;
                    }
                    if (infoText.Contains("분 전"))
                    {
                        date = DateTime.Now.AddMinutes(Convert.ToInt32(infoText.Replace("분 전", "")) * -1);
                        break;
                    }
                    if (infoText.Contains("초 전"))
                    {
                        date = DateTime.Now.AddSeconds(Convert.ToInt32(infoText.Replace("초 전", "")) * -1);
                        break;
                    }
                }
                articleList.Add(new Article(title, link, desc, date));
            }

            HtmlNode NextBtnNode = doc.DocumentNode.SelectSingleNode("//a[@class='btn_next']");
            
            if (NextBtnNode != null)
            {
                string nextUrl = NextBtnNode.GetAttributeValue("href", "");
                if (!String.IsNullOrEmpty(nextUrl))
                {
                    naverPageCount+=10;
                    string startDate = dateTimePicker1.Value.ToString("yyyy.MM.dd");
                    string endDate = dateTimePicker2.Value.ToString("yyyy.MM.dd");
                    string nu = string.Format("https://search.naver.com/search.naver?where=news&query=%EB%8F%99%EA%B5%AD%EB%8C%80&sm=tab_opt&sort=1&photo=0&field=0&pd=3&ds={0}&de={1}&docid=&related=0&mynews=0&office_type=0&office_section_code=0&news_office_checked=&nso=so%3Add%2Cp%3Afrom20231127to20231227&is_sug_officeid=0&office_category=0&service_area=0&start={2}",
                    startDate, endDate, naverPageCount);
                    naverParsing(nu);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            naverPageCount = 0;
            googlePageCount = 0;

            if (checkBox2.Checked == true)
            {
                string startDate = dateTimePicker1.Value.ToString("yyyy.MM.dd");
                string endDate = dateTimePicker2.Value.ToString("yyyy.MM.dd");
                string naverUrl = string.Format("https://search.naver.com/search.naver?where=news&query=%EB%8F%99%EA%B5%AD%EB%8C%80&sm=tab_opt&sort=1&photo=0&field=0&pd=3&ds={0}&de={1}&docid=&related=0&mynews=0&office_type=0&office_section_code=0&news_office_checked=&nso=so%3Add%2Cp%3Afrom20231127to20231227&is_sug_officeid=0&office_category=0&service_area=0&start={2}",
                   startDate, endDate, "1");
                naverParsing(naverUrl);

            }
            
            if(checkBox3.Checked == true)
            {
                string startDate = dateTimePicker1.Value.ToString("MM'%2F'dd'%2F'yyyy");
                string endDate = dateTimePicker2.Value.ToString("MM'%2F'dd'%2F'yyyy");
                string googleUrl = string.Format("https://www.google.com/search?q=%EB%8F%99%EA%B5%AD%EB%8C%80&sca_esv=593812750&biw=1920&bih=945&sxsrf=AM9HkKmRIMJHTzzRqP1ci0-5d1xZMDJQ-g%3A1703618143900&source=lnt&tbs=cdr%3A1%2Ccd_min%3A{0}%2Ccd_max%3A{1}&tbm=nws",
                    startDate, endDate);
                googleParsing(googleUrl);
                /*
                string googleQuery = "?q=%EB%8F%99%EA%B5%AD%EB%8C%80%20when%3A7d&hl=ko&gl=KR&ceid=KR%3Ako";
                string googleResult = hxp.parse("https://news.google.com/rss/search", googleQuery);
                xml.LoadXml(googleResult);
                xmlList = xml.GetElementsByTagName("item");
                Article.ArticleListAddXmlList(xmlList, ref articleList, dateTimePicker1.Value, dateTimePicker2.Value, "pubDate", "link", "ddd, dd MMM yyyy HH:mm:ss 'GMT'");
                */
            }

            naverPageCount = 0;
            googlePageCount = 0;
            articleListToClipBoard();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            driver.Quit();
        }
    }
}