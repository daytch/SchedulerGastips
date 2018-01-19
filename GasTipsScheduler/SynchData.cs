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
        public static void PopulateData()
        {
            #region Gather Url from gasbuddy.com
            for (int i = 0; i < MainUrl.Length; i++)
            {
                //get url from country
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument doc = hw.Load(url + "GasPrices");
                List<string> listHrefCity = new List<string>();
                List<string> listHrefBranch= new List<string>();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value.Contains("GasPrices"))
                    {
                        listHrefCity.Add(att.Value);
                    }
                }
                //get url from city
                foreach (var item in listHrefCity)
                {
                    HtmlWeb hwBranch = new HtmlWeb();
                    HtmlDocument docBranch = hwBranch.Load(url + item);
                    foreach (HtmlNode linkBranch in docBranch.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute attBranch = linkBranch.Attributes["href"];
                        if (attBranch.Value.Contains("GasPrices"))
                        {
                            listHrefBranch.Add(attBranch.Value);
                        }
                    }
                }
            }
            #region Canada
            #endregion
            #region US
            #endregion
            #endregion

            #region Get Features and other informations
            string urlAddress = "https://www.gasbuddy.com/Station/12329";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
            #endregion

        }
    }
}
