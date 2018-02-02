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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;

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
        public string Url { get; set; }
    }

    public partial class FormMain : Form
    {
        static string url = "https://www.gasbuddy.com";
        static string[] MainUrl = ConfigurationManager.AppSettings["MainUrl"].Split(',');
        private bool processing = false;

        public FormMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = !btnStart.Enabled;
            btnPause.Enabled = !btnStart.Enabled;
            this.InternalStartProcess();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (btnPause.Text == "Pa&use")
            {
                btnPause.Text = "&Resume";
            }
            else
            {
                btnPause.Text = "Pa&use";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.InternalWriteLog("Stopping...");
            btnStart.Enabled = true;
            btnPause.Enabled = !btnStart.Enabled;
            btnPause.Text = "Pa&use";

            processing = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!btnStart.Enabled)
            {
                MessageBox.Show("Please kindly stop the process first, before closing this window.", "Info", MessageBoxButtons.OK);
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //Do nothing.
            this.InternalWriteLog("Application is ready, click on the Start button to begin.");
        }

        private delegate void DelegateWriteLog(string message);

        private void InternalWriteLog(string message)
        {
            if (this.InvokeRequired)
            {
                DelegateWriteLog d = new DelegateWriteLog(InternalWriteLog);

                this.Invoke(d, message);
            }
            else
            {
                this.txtLog.AppendText(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff") + ": " + message);
                this.txtLog.AppendText("\r\n");
            }
        }

        private void InternalStartProcess()
        {
            ThreadStart ts = new ThreadStart(InternalStartThreadProccess);
            Thread t = new Thread(ts);

            t.IsBackground = true;
            t.Start();
        }

        private async void InternalStartThreadProccess()
        {
            try
            {
                processing = true;
                this.InternalWriteLog("Starting (from background thread)...");
                List<NameAndProp> data = await GetData();

                using (ExcelPackage ep = new ExcelPackage())
                {
                    ExcelWorksheet sheet1 = ep.Workbook.Worksheets.Add("Sheet1");
                    sheet1.Cells["A1"].Value = "No";
                    sheet1.Cells["B1"].Value = "Nama";
                    sheet1.Cells["C1"].Value = "Brand ID";
                    sheet1.Cells["D1"].Value = "Address";
                    sheet1.Cells["E1"].Value = "Locality";
                    sheet1.Cells["F1"].Value = "Region";
                    sheet1.Cells["G1"].Value = "Country";
                    sheet1.Cells["H1"].Value = "Postal Code";
                    sheet1.Cells["I1"].Value = "TimeZone";
                    sheet1.Cells["J1"].Value = "Phone";
                    sheet1.Cells["K1"].Value = "Reguler";
                    sheet1.Cells["L1"].Value = "Premium";
                    sheet1.Cells["M1"].Value = "Diesel";
                    sheet1.Cells["N1"].Value = "Feature";
                    sheet1.Cells["O1"].Value = "Latitude";
                    sheet1.Cells["P1"].Value = "Logitude";
                    sheet1.Cells["Q1"].Value = "Url";
                    int x = 2;
                    int z = 0;
                    #region Mapping Brand in Cell
                    for (int i = 0; i < data.Count; i++)
                    {
                        z = x + i;
                        this.InternalWriteLog("Start Mapping brand :" + data[i].Name);
                        sheet1.Cells["A" + z].Value = i + 1;
                        sheet1.Cells["B" + z].Value = data[i].Name;
                        sheet1.Cells["C" + z].Value = data[i].Brand_id;
                        sheet1.Cells["D" + z].Value = data[i].Address;
                        sheet1.Cells["E" + z].Value = data[i].Locality;
                        sheet1.Cells["F" + z].Value = data[i].Region;
                        sheet1.Cells["G" + z].Value = data[i].Country;
                        sheet1.Cells["H" + z].Value = data[i].Postal_code;
                        sheet1.Cells["I" + z].Value = data[i].Timezone;
                        sheet1.Cells["J" + z].Value = data[i].Phone;
                        sheet1.Cells["K" + z].Value = data[i].Regular_price;
                        sheet1.Cells["L" + z].Value = data[i].Premium_price;
                        sheet1.Cells["M" + z].Value = data[i].Diesel_price;
                        sheet1.Cells["N" + z].Value = data[i].Features;
                        sheet1.Cells["O" + z].Value = data[i].Latitude;
                        sheet1.Cells["P" + z].Value = data[i].Longitude;
                        sheet1.Cells["Q" + z].Value = data[i].Url;
                        this.InternalWriteLog("End Mapping brand :" + data[i].Name);
                    }
                    #endregion
                    ep.SaveAs(new FileInfo(@"D:\Users\xssnurul1396\Documents\GasTips.xlsx"));
                }
            }
            catch (NullReferenceException ex)
            {
                this.InternalWriteLog("Error: " + ex.GetBaseException().Message);
            }

            this.InternalWriteLog("Stopped.");
        }

        private List<string> GetMajorAndCountryLink(List<string> listCity)
        {
            #region Initiate List
            List<string> ListStation = new List<string>();
            List<string> ListMajor = new List<string>();
            List<string> ListLocal = new List<string>();
            //listCity.Add("https://www.gasbuddy.com/GasPrices/NewBrunswick");
            #endregion
            this.InternalWriteLog("Loading Country : " + listCity.Count.ToString());

            for (int i = 0; i < listCity.Count; i++)
            {
                #region loop
                HtmlWeb hwBranch = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docBranch = hwBranch.Load(listCity[i]);
                List<string> listHrefBranch = new List<string>();
                this.InternalWriteLog("Loading Country data: " + listCity[i]);

                #region Get Station's Link
                if (docBranch.DocumentNode.SelectNodes("//*[@id='prices - table']/tr/td/a") != null)
                {
                    foreach (HtmlNode StationLinks in docBranch.DocumentNode.SelectNodes("//*[@id='prices - table']/tr/td/a"))
                    {
                        HtmlAttribute attStation = StationLinks.Attributes["href"];
                        //this.InternalWriteLog("Loading branch data: " + linkBranch.Attributes["href"]);

                        if (attStation != null)
                        {
                            if (attStation.Value.Contains("Station"))
                            {
                                ListStation.Add(url + attStation.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                else if (docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a") != null)
                {
                    foreach (HtmlNode StationLinks in docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a"))
                    {
                        HtmlAttribute attStation = StationLinks.Attributes["href"];
                        //this.InternalWriteLog("Loading branch data: " + linkBranch.Attributes["href"]);

                        if (attStation != null)
                        {
                            if (attStation.Value.Contains("Station"))
                            {
                                ListStation.Add(url + attStation.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                #endregion

                #region Get Major Areas
                if (docBranch.DocumentNode.SelectNodes("//div[@id='suggestions']/div/a") != null)
                {
                    foreach (HtmlNode MajorLinks in docBranch.DocumentNode.SelectNodes("//div[@id='suggestions']/div/a"))
                    {
                        HtmlAttribute attMajor = MajorLinks.Attributes["href"];
                        //this.InternalWriteLog("Loading branch data: " + linkBranch.Attributes["href"]);

                        if (attMajor != null)
                        {
                            if (attMajor.Value.Contains("GasPrices"))
                            {
                                ListMajor.Add(url + attMajor.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                #endregion

                #region Get Local / Country Prices
                if (docBranch.DocumentNode.SelectNodes("/html/body/div[3]/div/div[1]/div[3]/div/div/div/div/a") != null)
                {
                    foreach (HtmlNode LocalLinks in docBranch.DocumentNode.SelectNodes("/html/body/div[3]/div/div[1]/div[3]/div/div/div/div/a"))
                    {
                        HtmlAttribute attLocal = LocalLinks.Attributes["href"];
                        //this.InternalWriteLog("Loading branch data: " + linkBranch.Attributes["href"]);

                        if (attLocal != null)
                        {
                            if (attLocal.Value.Contains("GasPrices"))
                            {
                                ListLocal.Add(url + attLocal.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                #endregion

                #region 30/01/2018
                //foreach (HtmlNode linkBranch in docBranch.DocumentNode.SelectNodes("//a[@href]"))
                //{
                //    HtmlAttribute attBranch = linkBranch.Attributes["href"];
                //    //this.InternalWriteLog("Loading branch data: " + linkBranch.Attributes["href"]);

                //    if (attBranch != null)
                //    {
                //        if (attBranch.Value.Contains("GasPrices") && !listHrefBranch.Any(x => x == (url + attBranch.Value)))
                //        {
                //            listHrefBranch.Add(url + attBranch.Value);
                //        }
                //    }

                //    //this.InternalWriteLog("Done loading branch data: " + linkBranch.Attributes["href"]);
                //}
                //listHrefBranch = listHrefBranch.Distinct().ToList();
                //ListDetail = MappingData(listHrefBranch, listCity);
                //ListAllStation.AddRange(ListDetail);
                #endregion
                #endregion
                this.InternalWriteLog("Done loading Country data: " + listCity[i]);
            }
            if (ListMajor.Count > 0)
            {
                List<string> ListFromMajor = GetDataFromMajor(ListMajor.Distinct().ToList());
                ListStation.AddRange(ListFromMajor);
            }
            if (ListLocal.Count > 0)
            {
                List<string> ListFromLocal = GetDataFromLocal(ListLocal.Distinct().ToList());
                ListStation.AddRange(ListFromLocal);
            }
            return ListStation.Distinct().ToList();
        }

        private List<string> GetDataFromMajor(List<string> ListMajor)
        {
            List<string> ListStation = new List<string>();
            List<string> ListLocal = new List<string>();
            for (int i = 0; i < ListMajor.Count; i++)
            {
                HtmlWeb hwMajor = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docBranch = hwMajor.Load(ListMajor[i]);
                this.InternalWriteLog("Loading Major data (Sub Method): " + ListMajor[i]);

                #region Get Station's Link
                if (docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a") != null)
                {
                    foreach (HtmlNode StationLinks in docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a"))
                    {
                        this.InternalWriteLog("Get Station link data: " + ListMajor[i]);
                        HtmlAttribute attStation = StationLinks.Attributes["href"];
                        if (attStation != null)
                        {
                            if (attStation.Value.Contains("Station"))
                            {
                                this.InternalWriteLog("Insert :" + attStation.Value + " to List Station");
                                ListStation.Add(url + attStation.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                #endregion

                #region Get Local / Country Prices
                if (docBranch.DocumentNode.SelectNodes("//*[@id='suggestions']/div[1]/a") != null)
                {
                    foreach (HtmlNode LocalLinks in docBranch.DocumentNode.SelectNodes("//*[@id='suggestions']/div[1]/a"))
                    {
                        this.InternalWriteLog("Get Local link data (Sub 1 Method)");
                        HtmlAttribute attLocal = LocalLinks.Attributes["href"];
                        if (attLocal != null)
                        {
                            if (attLocal.Value.Contains("GasPrices"))
                            {
                                ListLocal.Add(url + attLocal.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                    this.InternalWriteLog("Done Get Local link data (Sub 1 Method)");
                }
                else if (docBranch.DocumentNode.SelectNodes("/html/body/div[3]/div/div[1]/div[3]/div/div/div/div/a") != null)
                {
                    foreach (HtmlNode LocalLinks in docBranch.DocumentNode.SelectNodes("/html/body/div[3]/div/div[1]/div[3]/div/div/div/div/a"))
                    {
                        this.InternalWriteLog("Get Local link data (Sub 1 Method)");
                        HtmlAttribute attLocal = LocalLinks.Attributes["href"];
                        if (attLocal != null)
                        {
                            if (attLocal.Value.Contains("GasPrices"))
                            {
                                ListLocal.Add(url + attLocal.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                    this.InternalWriteLog("Done Get Local link data (Sub 1 Method)");
                }
                #endregion

            }
            if (ListLocal.Count > 0)
            {
                List<string> listLoc = GetDataFromLocal(ListLocal.Distinct().ToList());
                ListStation.AddRange(listLoc);
            }
            return ListStation.Distinct().ToList();
        }

        private List<string> GetDataFromLocal(List<string> ListLocal)
        {
            List<string> ListStation = new List<string>();
            for (int i = 0; i < ListLocal.Count; i++)
            {
                HtmlWeb hwMajor = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument docBranch = hwMajor.Load(ListLocal[i]);
                this.InternalWriteLog("Loading Local data (Sub 2 Method): " + ListLocal[i]);

                #region Get Station's Link
                if (docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a") != null)
                {
                    foreach (HtmlNode StationLinks in docBranch.DocumentNode.SelectNodes("//tbody/tr/td/a"))
                    {
                        HtmlAttribute attStation = StationLinks.Attributes["href"];
                        if (attStation != null)
                        {
                            if (attStation.Value.Contains("Station"))
                            {
                                ListStation.Add(url + attStation.Value);
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
                #endregion
            }
            return ListStation;
        }

        private List<String> GetAllState()
        {
            List<string> listUrl = new List<string>();
            List<string> ListMain = new List<string>();
            foreach (var item in MainUrl)
            {
                ListMain.Add(url + "/GasPrices/" + item);
            }

            List<string> listHrefCity = new List<string>();
            #region Gather Url from gasbuddy.com
            for (int i = 0; i < ListMain.Count; i++)
            {
                this.InternalWriteLog("Getting data from: " + ListMain[i]);
                //get url from country
                HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = hw.Load(ListMain[i]);

                this.InternalWriteLog("Loading data from: " + url + "/GasPrices");

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att != null)
                    {
                        if (att.Value.Contains("GasPrices") && !listHrefCity.Any(x => x == (url + att.Value)))
                        {
                            listHrefCity.Add(url + att.Value);
                        }
                    }
                    //this.InternalWriteLog("Done parsing data from: " + link.Attributes["href"]);
                }

                this.InternalWriteLog("Done loading data from: " + ListMain[i]);
                listHrefCity.Remove("https://www.gasbuddy.com/GasPrices");
            }
            listHrefCity = listHrefCity.Except(ListMain).ToList();
            List<string> ListStation = GetMajorAndCountryLink(listHrefCity);
            listUrl.AddRange(ListStation);
            #endregion

            return listUrl;
        }

        private List<NameAndProp> GetLowestPrices(string link)
        {
            List<NameAndProp> response = new List<NameAndProp>();
            NameAndProp data = new NameAndProp();
            string JsonString = string.Empty;
            HtmlWeb hwJson = new HtmlWeb();
            this.InternalWriteLog("Loading Gas Station : " + link);

            HtmlAgilityPack.HtmlDocument docJson = hwJson.Load(link);//"https://www.gasbuddy.com/Station/16108");
            HtmlNode script = docJson.DocumentNode.Descendants().Where(n => n.Name == "script" && n.InnerHtml.Contains("PreloadedState")).FirstOrDefault();//.ToList().ForEach(n => n.Remove());

            if (script != null)
            {
                JsonString = script.InnerText;
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

                data.Url = link;
                response.Add(data);
            }
            this.InternalWriteLog("Done Gas Station : " + link);
            return response;
        }

        private async Task<List<NameAndProp>> GetData()
        {
            this.InternalWriteLog("Begin crawling process...");
            List<NameAndProp> ListData = new List<NameAndProp>();
            NameAndProp data = new NameAndProp();

            List<string> listUrl = new List<string>(); //GetAllState();
            listUrl.Add("https://www.gasbuddy.com/Station/118935");
            listUrl.Add("https://www.gasbuddy.com/Station/190647");
            listUrl.Add("https://www.gasbuddy.com/Station/10585");
            foreach (var item in listUrl)
            {
                ListData.AddRange(GetLowestPrices(item));
            }
            ListData.Distinct().ToList();
            return ListData;
        }
    }
}
