using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SchedulerGasTips
{
    public class NameAndProp
    {
        public string Name { get; set; }
        public int Brand_id { get; set; }
        public string Address { get; set; }
        public string Locality { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Postal_code { get; set; }
        public string Timezone { get; set; }
        public string Phone { get; set; }
        public decimal Regular_price { get; set; }
        public decimal Midgrade_price { get; set; }
        public decimal Premium_price { get; set; }
        public decimal Diesel_price { get; set; }
        public string Features { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public partial class Form1 : Form
    {
        static string url = "https://www.gasbuddy.com";
        static string[] MainUrl = ConfigurationManager.AppSettings["MainUrl"].Split(',');

        public Form1()
        {
            InitializeComponent();
        }

        private static List<string> MappingData(List<string> listBranch, List<string> listDouble)
        {
            List<string> ListDetailStation = new List<string>();
            List<string> ListUrl = new List<string>();
            for (int i = 0; i < listBranch.Count; i++)
            {
                if (listBranch[i] != "https://www.gasbuddy.com/GasPrices" && listBranch[i] != "https://www.gasbuddy.com/GasPrices/")
                {
                    HtmlWeb hwJson = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument docJson = hwJson.Load(listBranch[i]);
                    List<string> listHrefBranch = new List<string>();
                    foreach (HtmlNode linkBranch in docJson.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute attBranch = linkBranch.Attributes["href"];
                        if (attBranch != null)
                        {
                            if (attBranch.Value.Contains("GasPrices") && !listHrefBranch.Any(x=>x==attBranch.Value))
                            {
                                listHrefBranch.Add(attBranch.Value);
                            }
                        }
                    }
                    for (int j = 0; j < listHrefBranch.Count; j++)
                    {
                        if (listHrefBranch[j] != "https://www.gasbuddy.com/GasPrices" && listHrefBranch[j] != "https://www.gasbuddy.com/GasPrices/")
                        {
                            HtmlWeb hwDetail = new HtmlWeb();
                            HtmlAgilityPack.HtmlDocument docDetail = hwDetail.Load(url + listHrefBranch[j]);
                            List<string> listHrefDetail = new List<string>();
                            if (docDetail.DocumentNode != null)
                            {
                                try
                                {
                                    var selectNode = docDetail.DocumentNode.SelectNodes("//a[@href]");
                                    if (selectNode != null)
                                    {
                                        foreach (HtmlNode linkDetail in docDetail.DocumentNode.SelectNodes("//a[@href]"))
                                        {
                                            HtmlAttribute attDetail = linkDetail.Attributes["href"];
                                            if (attDetail.Value.Contains("GasPrices") && attDetail.Value != "/GasPrices/" &&
                                                attDetail.Value != "/GasPrices" && !ListUrl.Any(x => x == (url + attDetail.Value)))
                                            {
                                                ListUrl.Add(url + attDetail.Value);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }
            }
            ListDetailStation = ListUrl.Except(listDouble).ToList();
            return ListDetailStation;
        }

        private static void GetCityLinks(List<string> listCity)
        {
            List<string> ListAllStation = new List<string>();
            List<string> ListDetail = new List<string>();
            for (int i = 0; i < listCity.Count; i++)
            {
                HtmlWeb hwBranch = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docBranch = hwBranch.Load(listCity[i]);
                List<string> listHrefBranch = new List<string>();
                foreach (HtmlNode linkBranch in docBranch.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute attBranch = linkBranch.Attributes["href"];
                    if (attBranch != null)
                    {
                        if (attBranch.Value.Contains("GasPrices") && !listHrefBranch.Any(x=>x==(url + attBranch.Value)))
                        {
                            listHrefBranch.Add(url + attBranch.Value);
                        }
                    }
                }
                ListDetail = MappingData(listHrefBranch, listCity);
                ListAllStation.AddRange(ListDetail);
            }
        }

        private List<String> GetAllUrl()
        {
            List<string> listUrl = new List<string>();

            #region Gather Url from gasbuddy.com
            for (int i = 0; i < MainUrl.Length; i++)
            {
                //get url from country
                HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(url + "/GasPrices");
                List<string> listHrefCity = new List<string>();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att != null)
                    {
                        if (att.Value.Contains("GasPrices") && !listHrefCity.Any(x=>x==(url+att.Value)))
                        {
                            listHrefCity.Add(url + att.Value);
                        }
                    }
                }
                GetCityLinks(listHrefCity);
            }
            #endregion

            return listUrl;
        }

        private async Task<NameAndProp> GetData()
        {
            NameAndProp data = new NameAndProp();
            List<string> listUrl = GetAllUrl();
            string JsonString = string.Empty;
            HtmlWeb hwJson = new HtmlWeb();

            HtmlAgilityPack.HtmlDocument docJson = hwJson.Load("https://www.gasbuddy.com/Station/16108");
            foreach (HtmlNode script in docJson.DocumentNode.Descendants("script").ToArray())
            {
                if (script.InnerHtml.Contains("PreloadedState"))
                {
                    JsonString = script.InnerText;
                }

            }
            JsonString = JsonString.Remove(0, 24);
            JsonString = JsonString.Remove(JsonString.Length - 1);

            #region Set Station info
            XmlDocument xmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(JsonString, "root");
            XmlNodeList nodeListInfo = xmlDoc.DocumentElement.SelectNodes("/root/stationInfo");
            foreach (XmlNode node in nodeListInfo)
            {
                XmlElement element = (XmlElement)node;
                data.Name = element.GetElementsByTagName("name")[0].ChildNodes[0].InnerText;
                data.Brand_id = Convert.ToInt32(element.GetElementsByTagName("brand_id")[0].ChildNodes[0].InnerText);
                data.Address = element.GetElementsByTagName("address")[0].ChildNodes[0].InnerText;
                data.Phone = element.GetElementsByTagName("phone")[0].ChildNodes[0].InnerText;
                data.Latitude = element.GetElementsByTagName("latitude")[0].ChildNodes[0].InnerText;
                data.Longitude = element.GetElementsByTagName("longitude")[0].ChildNodes[0].InnerText;
                data.Country = element.GetElementsByTagName("address")[0].ChildNodes[4].InnerText;
                data.Locality = element.GetElementsByTagName("address")[0].ChildNodes[2].InnerText;
                data.Region = element.GetElementsByTagName("address")[0].ChildNodes[2].InnerText;
                data.Postal_code = element.GetElementsByTagName("address")[0].ChildNodes[5].InnerText;
            }
            #endregion

            #region Set Price
            XmlNodeList nodeListPrice = xmlDoc.DocumentElement.SelectNodes("/root/fuels");
            foreach (XmlNode priceNode in nodeListPrice)
            {
                XmlElement element = (XmlElement)priceNode;
                for (int i = 0; i < element.GetElementsByTagName("fuelsByStationId")[0].ChildNodes.Count; i++)
                {
                    switch (element.GetElementsByTagName("fuelType")[i].ChildNodes[0].InnerText.ToLower())
                    {
                        //convert from dollar to cent
                        case "regular":
                            data.Regular_price = Convert.ToDecimal(element.GetElementsByTagName("prices")[i].ChildNodes[1].InnerText.ToString().Replace('.', ',')) * 100;
                            break;
                        case "premium":
                            data.Premium_price = Convert.ToDecimal(element.GetElementsByTagName("prices")[i].ChildNodes[1].InnerText.ToString().Replace('.', ',')) * 100;
                            break;
                        case "midgrade":
                            data.Midgrade_price = Convert.ToDecimal(element.GetElementsByTagName("prices")[i].ChildNodes[1].InnerText.ToString().Replace('.', ',')) * 100;
                            break;
                        case "diesel":
                            data.Diesel_price = Convert.ToDecimal(element.GetElementsByTagName("prices")[i].ChildNodes[1].InnerText.ToString().Replace('.', ',')) * 100;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion

            #region Set Features
            XmlNodeList nodeListFeatures = xmlDoc.DocumentElement.SelectNodes("/root/features");
            foreach (XmlNode featureNode in nodeListFeatures)
            {
                XmlElement element = (XmlElement)featureNode;
                for (int i = 0; i < element.GetElementsByTagName("byStationId")[0].ChildNodes.Count; i++)
                {
                    if (i == element.GetElementsByTagName("byStationId")[0].ChildNodes.Count - 1)
                    {
                        data.Features += element.GetElementsByTagName("displayName")[i].ChildNodes[0].InnerText.ToString();
                    }
                    else
                    {
                        data.Features += element.GetElementsByTagName("displayName")[i].ChildNodes[0].InnerText.ToString() + ", ";
                    }
                }
            }
            #endregion

            return data;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var data = await GetData();
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
