using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GasTipsScheduler
{
    class SynchData
    {
        static string url = "https://www.gasbuddy.com/";
        static string[] MainUrl = ConfigurationManager.AppSettings["MainUrl"].Split(',');

        private static void GetCityLinks(List<string> listCity)
        {
            foreach (var item in listCity)
            {
                HtmlWeb hwBranch = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docBranch = hwBranch.Load(url + item);
                List<string> listHrefBranch = new List<string>();
                foreach (HtmlNode linkBranch in docBranch.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute attBranch = linkBranch.Attributes["href"];
                    if (attBranch.Value.Contains("GasPrices"))
                    {
                        listHrefBranch.Add(attBranch.Value);
                    }
                }
                MappingData(listHrefBranch);
            }
        }

        private static void MappingData(List<string> listBranch)
        {
            foreach (var itemJSON in listBranch)
            {
                string JsonString = string.Empty;
                HtmlWeb hwJson = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docJson = hwJson.Load(url + itemJSON);
                foreach (HtmlNode script in docJson.DocumentNode.Descendants("script").ToArray())
                {
                    //HtmlAttribute attJson = linkJson.Attributes["href"];
                    if (script.OuterHtml.Contains("PreloadedState"))
                    {
                        JsonString = script.InnerText;//attJson.Value;
                    }

                }

                #region 22/01/2018
                //NameAndProp data = new NameAndProp();

                //string JsonString = string.Empty;
                //HtmlWeb hwJson = new HtmlWeb();
                //HtmlAgilityPack.HtmlDocument docJson = hwJson.Load("https://www.gasbuddy.com/Station/16108");
                //foreach (HtmlNode script in docJson.DocumentNode.Descendants("script").ToArray())
                //{
                //    if (script.InnerHtml.Contains("PreloadedState"))
                //    {
                //        JsonString = script.InnerText;
                //    }

                //}
                //JsonString = JsonString.Remove(0, 24);
                //JsonString = JsonString.Remove(JsonString.Length - 1);
                //XmlDocument xmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(JsonString, "root");
                //XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/features");
                //foreach (XmlNode node in nodeList)
                //{
                //    data.Name = node.SelectSingleNode("Product_id").InnerText;
                //    proName = node.SelectSingleNode("Product_name").InnerText;
                //    price = node.SelectSingleNode("Product_price").InnerText;
                //    MessageBox.Show(proID + " " + proName + " " + price);
                //}
                #endregion

                #region Get Features and other informations
                //string urlAddress = url + itemJSON;//"https://www.gasbuddy.com/Station/12329";

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                //request.UserAgent = "Mozilla / 5.0(Windows NT 10.0) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 63.0.3239.132 Safari / 537.36";
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //if (response.StatusCode == HttpStatusCode.OK)
                //{
                //    Stream receiveStream = response.GetResponseStream();
                //    StreamReader readStream = null;

                //    if (response.CharacterSet == null)
                //    {
                //        readStream = new StreamReader(receiveStream);
                //    }
                //    else
                //    {
                //        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                //    }

                //    string data = readStream.ReadToEnd();

                //    response.Close();
                //    readStream.Close();
                //}
                #endregion
            }
        }

        public static void PopulateData()
        {
            #region Gather Url from gasbuddy.com
            for (int i = 0; i < MainUrl.Length; i++)
            {
                //get url from country
                HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(url + "GasPrices");
                List<string> listHrefCity = new List<string>();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value.Contains("GasPrices"))
                    {
                        listHrefCity.Add(att.Value);
                    }
                }
                GetCityLinks(listHrefCity);
            }

            #endregion
        }
    }
}
