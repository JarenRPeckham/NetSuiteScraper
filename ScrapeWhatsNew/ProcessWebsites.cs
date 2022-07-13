using HtmlAgilityPack;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ScrapeWhatsNew
{
    class ProcessWebsites
    {

        //TODO
        const string baseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/";//https://docs.microsoft.com/en-us/dynamics365/finance/
        //const string homepage = baseURL + "get-started/whats-new-home-page";
        const string docsMicrosoftBaseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/";

        //const string topHomePage = "https://docs.microsoft.com/en-us/dynamics365/get-started/whats-new/";

        public List<ColorSet> colorSetList = new List<ColorSet>();

        public int colorIndex = 0;

        List<DescriptionRow> descriptionRows = new List<DescriptionRow>();

        public void Process()
        {
            //TODO
            string fileName = @"NetSuiteWhatsNew.csv";
            string descriptionFileName = @"NetSuiteWhatsNew_DescriptionData.csv";

            SetColorSetList();
            AddDummyDescriptionRow();

            var powerLineList = GetProductLineList();

            var powerWheelRows = ConvertToPowerWheelRows(powerLineList);

            var csvString = PowerWheelRow.GetCSVString(powerWheelRows);
            WriteFile(csvString, fileName);

            if (descriptionRows.Count > 0)
            { 
                var csvStringDescriptionData = DescriptionRow.GetCSVString(descriptionRows);
                WriteFile(csvStringDescriptionData, descriptionFileName);
            }

            //GetFeatureDetails("https://docs.microsoft.com/en-us/dynamics365-release-plan/2020wave2/finance-operations/dynamics365-finance/vendor-invoice-automation-analytics-metrics");
        }

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                //string cleaned =  Regex.Replace(strIn, @"[^\w\.@-]", "",
                //                     RegexOptions.None, TimeSpan.FromSeconds(1.5));

                string cleaned = Regex.Replace(strIn, @"<[^>]+>|&nbsp;", "").Trim();
                cleaned = Regex.Replace(cleaned, @"[^\u0020-\u007F]", String.Empty);
                cleaned = cleaned.Replace(",", "");

                cleaned = cleaned.Replace("\n", "").Replace("\r", "");
                return cleaned;
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public void WriteFile(string _csvString, string _fileName)
        {
            string folderPath = Directory.GetCurrentDirectory() + "\\";
            
            string fileEx = folderPath + _fileName;

            File.WriteAllText(fileEx, _csvString);

            Console.WriteLine("Wrote file: " + fileEx);

        }

        

        public List<ColorSet> SetColorSetList()
        {
            var colorSet = new ColorSet("#F1B434", "#F4C35D", "#F7D285", "#F9E1AE", "#F9E1AE", "#F9E1AE");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#E40046", "#E9336B", "#EF6690", "#F499B5", "#F499B5", "#F499B5");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#A2A569", "#B5B787", "#C7C9A5", "#DADBC3", "#DADBC3", "#DADBC3");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#E87722", "#ED924E", "#F1AD7A", "#F6C9A7", "#F6C9A7", "#F6C9A7");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#009CDE", "#33B0E5", "#66C4EB", "#99D7F2", "#99D7F2", "#99D7F2");
            colorSetList.Add(colorSet);

            return colorSetList;
        }

        public ColorSet GetColorSet()
        {
            var colorSetArray = colorSetList.ToArray();
            return colorSetArray[colorIndex];
        }

        public List<PowerWheelRow> ConvertToPowerWheelRows(List<ProductLine> _productLines)
        {
            List<PowerWheelRow> powerWheelRows = new List<PowerWheelRow>();

            foreach (var productLine in _productLines)
            {
                SetDescriptionDataForProductLine(productLine);
                foreach (var version in productLine.VersionList)
                {
                    SetDescriptionDataForVersion(version);
                    foreach (var feature in version.FeatureList)
                    {
                        PowerWheelRow row = new PowerWheelRow();
                        row.Ring0 = productLine.Name;
                        row.Ring1 = version.Name;
                        row.Ring2 = feature.Name;
                        row.Ring3 = "";
                        row.Ring4 = "";
                        row.Ring5 = "";

                        row.Color = "Blue";
                        row.size = "1";

                        var colorSet = GetColorSet();

                        row.color = colorSet.color;
                        row.color1 = colorSet.color1;
                        row.color2 = colorSet.color2;
                        row.color3 = colorSet.color3;
                        row.color4 = colorSet.color4;
                        row.color5 = colorSet.color5;

                        row.description = "";
                        row.description1 = "";
                        row.description2 = "";
                        row.description3 = "";
                        row.description4 = "";
                        row.description5 = "";

                        row.state = "Enable";
                        row.state1 = "Enable";
                        row.state2 = "Enable";
                        row.state3 = "Enable";
                        row.state4 = "Enable";
                        row.state5 = "Enable";

                        row.level = "0";
                        row.level1 = "0";
                        row.level2 = "0";
                        row.level3 = "0";
                        row.level4 = "0";
                        row.level5 = "0";

                        row.friendlyname = productLine.Name;
                        row.friendlyname1 = version.Name;
                        row.friendlyname2 = feature.Name;
                        row.friendlyname3 = "";
                        row.friendlyname4 = "";
                        row.friendlyname5 = "";

                        row.hyperlink = productLine.Url;
                        row.hyperlink1 = version.Link;
                        row.hyperlink2 = feature.URL;
                        row.hyperlink3 = "";
                        row.hyperlink4 = "";
                        row.hyperlink5 = "";

                        row.metadata = "";
                        row.metadata1 = "";
                        row.metadata2 = "";
                        row.metadata3 = "";
                        row.metadata4 = "";
                        row.metadata5 = "";

                        powerWheelRows.Add(row);

                        SetDescriptionDataForFeature(feature);
                    }

                }

                colorIndex++; //Change color for each productLine
            }

            return powerWheelRows;
        }

        public void SetDescriptionDataForVersion(Version _version)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.NodeName = _version.Name;
            descriptionRow.Title = _version.Name;
            descriptionRow.Description = _version.VersionNumber;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = _version.Link;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = ""; //_version.Link;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);
        }

        public void SetDescriptionDataForProductLine(ProductLine _productLine)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.NodeName = _productLine.Name;
            descriptionRow.Title = _productLine.Name;
            descriptionRow.Description = _productLine.Name;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = _productLine.Url;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = ""; //_productLine.Url;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);
        }

        public void SetDescriptionDataForFeature(Feature _feature)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.NodeName = _feature.Name;
            descriptionRow.Title = _feature.Name;
            descriptionRow.Description = _feature.BusinessValue + " /r/n " + _feature.FeatureDetails;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = _feature.URL;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = "";
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);
        }

        public void AddDummyDescriptionRow()
        {
            //This method can eventually be deleted.  But the format currently includes this row, 
            //so as to not break it we will keep it.
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.NodeName = "0";
            descriptionRow.Title = "1";
            descriptionRow.Description = "2";
            descriptionRow.ContactName = "3";
            descriptionRow.EmailAddress = "4";
            descriptionRow.ImageFileName = "5";
            descriptionRow.ExternalURL = "6";
            descriptionRow.InternalURL = "7";
            descriptionRow.ExternalOnePage = "8";
            descriptionRow.EmailSubject = "9";
            descriptionRow.HTMLResult = "10";

            descriptionRows.Add(descriptionRow);
        }

        public List<ProductLine> GetProductLineList()
        {
            List<ProductLine> productLineList = new List<ProductLine>();

            var productLine = GetProductLine("Accounting", "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/"//https://docs.microsoft.com/en-us/dynamics365/finance/
                            , "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/");//https://docs.microsoft.com/en-us/dynamics365/finance/get-started/whats-new-home-page
            if (productLine != null)
            {
                productLineList.Add(productLine);
            }

            //productLine = GetProductLine("Supply Chain", "https://docs.microsoft.com/en-us/dynamics365/supply-chain/"
            //                , "https://docs.microsoft.com/en-us/dynamics365/supply-chain/get-started/whats-new-home-page");
            //if (productLine != null)
            //{
            //    productLineList.Add(productLine);
            //}

            //productLine = GetProductLine("Commerce", "https://docs.microsoft.com/en-us/dynamics365/commerce/"
            //                , "https://docs.microsoft.com/en-us/dynamics365/commerce/get-started/whats-new-home-page");
            //if (productLine != null)
            //{
            //    productLineList.Add(productLine);
            //}


            return productLineList;

        }

        public ProductLine GetProductLine(string _name, string _baseURL, string _url)
        {
            
            var versionList = GetVersionList(_baseURL, _url);

            var productLine = new ProductLine(_name, _url, versionList);

            return productLine;

        }

        public List<Version> GetVersionList(string _baseURL, string _url)
        {
            List<Version> versionList = new List<Version>();
            var web = new HtmlWeb();
            var doc = web.Load(_url);

            try
            { 
                //TODO
                var body = doc.DocumentNode.SelectNodes(
                   @"/html/body/div[2]/div/section/div/div/main/table[1]/tbody"
                    ).First();

                var tr= body.SelectNodes("//tr").First();

                var tdList = tr.SelectNodes("//td");
                var tdArray = tdList.ToArray();

                for (int i=0; i<=24; i=i+4) //40 is only grab through 10.0.6, 24 is 10.0.10
                {
                    var linkNode = tdArray[i + 3]; //every 4th node is the one with the link in it.
                    if (linkNode != null 
                        && linkNode.ChildNodes != null
                        && linkNode.ChildNodes.Count > 0 )
                    { 
                        var linkNodeChildren = linkNode.ChildNodes.ToArray();  //in the children of the 'link' node, exists the name of the link and the url itself.
                        string name = ProcessWebsites.CleanInput(linkNodeChildren[0].InnerText);
                        string link = _baseURL + "get-started/" + linkNodeChildren[0].Attributes[0].Value;
                        var version = populateVersion(ProcessWebsites.CleanInput(tdArray[i].InnerText)
                                , ProcessWebsites.CleanInput(tdArray[i+1].InnerText)
                                , ProcessWebsites.CleanInput(tdArray[i+2].InnerText)
                                , name
                                , link);
                        versionList.Add(version);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetVersions: " + ex.Message);
            }

            return versionList;
        }

        public Version populateVersion(string _versionNumber, string _buildNumber, string _autoUpdateAvailability, string _name, string _link)
        {
            Version version = new Version();

            version.VersionNumber = _versionNumber;
            version.BuildNumber = _buildNumber;
            version.AutoUpdateAvailability = _autoUpdateAvailability;
            version.Name = _name;
            version.Link = _link;

            //This is the section with header 'Features included in this release
            version.FeatureList = GetVersionFeatureList(_link);

            //TODO
            //Platform updates

            //Bug fixes

            //

            //Removed and deprecated features

            return version;
        }

        public List<Feature> GetVersionFeatureList(string _url)
        {
            List<Feature> featureList = new List<Feature>();
            var web = new HtmlWeb();
            var doc = web.Load(_url);

            HtmlNodeCollection featureNodeList = null;;

            try {
                //TODO
                featureNodeList = doc.DocumentNode.SelectNodes(
                    @"/html/body/div[2]/div/section/div/div[1]/main/ul[3]/li/a"
                    ); //path to link on page: https://docs.microsoft.com/en-us/dynamics365/finance/get-started/whats-new-changed-10-0-16

                //If the path wasn't found, try this alternate path to the links.
                if (featureNodeList == null)
                {
                    featureNodeList = doc.DocumentNode.SelectNodes(
                        @"/html/body/div[2]/div/section/div/div[1]/main/ul[3]/li/p/a"
                        ); //path to link on page: https://docs.microsoft.com/en-us/dynamics365/finance/get-started/whats-new-changed-10-0-15
                }
            

                if (featureNodeList != null)
                {
                    var featureNodeArray = featureNodeList.ToArray();

                    for (int i = 0; i < featureNodeArray.Length; i++) //loop through all links on the page, and grab their names and links.
                    {
                        var linkNode = featureNodeArray[i];
                        string name = ProcessWebsites.CleanInput(linkNode.InnerText);
                        string link = docsMicrosoftBaseURL + linkNode.Attributes[0].Value; //this url uses the docsMicrosoftBaseURL then appends on the value.
                        var feature = GetFeatureDetails(link, name);
                        if (feature != null)
                        { 
                            featureList.Add(feature);
                        }
                    }
                }
                else
                {
                    //error occured;
                    Console.WriteLine("Node not found in GetVersionFeatureList");
                }

            }
            catch (Exception ex)
            {
                //error occured;
                Console.WriteLine("error in GetVersionFeatureList: " + ex.Message);
            }

            return featureList;
        }

        public Feature GetFeatureDetails(string _url, string _featureName)
        {
            var web = new HtmlWeb();
            var doc = web.Load(_url);

            //var generalAvailability = doc.DocumentNode.SelectNodes(
            //   @"/html/body/div[2]/div/section/div/div[1]/main/div/table/tbody/tr/td[3]/text()" //@" / html/body/div[2]/div/section/div/div[1]/main/div[1]/table/tbody/tr[1]/td[4]/a"   //@" / html/body/div[2]/div"
            //    ///following::tr[1]/td"
            //    ).First();

            Feature feature = null;

            try
            {
                //TODO
                var businessValue = doc.DocumentNode.SelectNodes(
                   @"/html/body/div[2]/div/section/div/div[1]/main/p[1]"
                    ).First();

                var featureDetails = doc.DocumentNode.SelectNodes(
                   @"/html/body/div[2]/div/section/div/div[1]/main/p[2]"
                    ).First();
                //TODODOODODOODO
                feature = new Feature(ProcessWebsites.CleanInput(featureDetails.InnerText), ProcessWebsites.CleanInput(businessValue.InnerText), _url, _featureName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetFeatureDetails: " + ex.Message);
            }

            return feature;
        }
    }
}
