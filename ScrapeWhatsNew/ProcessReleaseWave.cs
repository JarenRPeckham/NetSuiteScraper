using HtmlAgilityPack;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
//using ScrapeWhatsNew.Entities;

namespace ScrapeWhatsNew
{
    public class ProcessReleaseWave
    {
        //Base url shows the url that will be built on to create the link to each page
        const string baseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html"; //"https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html";
        const string releaseWaveName = "2022 Wave 1";//"2021 Wave 2";
        string releaseWaveURL = "";
        const string docsMicrosoftBaseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/";
        const string topHomePage = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html";
        public List<ColorSet> colorSetList = new List<ColorSet>();
        public int colorIndex = 0;
        List<DescriptionRow> descriptionRows = new List<DescriptionRow>();
        List<UserAddedRow> userAddedRows = new List<UserAddedRow>();
        List<ExcelRow> excelRows = new List<ExcelRow>();
        List<Feature> features = new List<Feature>();
        ReleaseWave releaseWave = null;
        bool showDebugMessages = true;

        public ReleaseWave Process()
        {
            releaseWave = new ReleaseWave(releaseWaveName, baseURL, null);
            releaseWaveURL = baseURL;

            releaseWave.AreaList = GetAreaList(releaseWave.Url);

            //Creates the filename for each needed portion of sunburst
            string fileName = @"NetSuiteWhatsNew2022.csv";
            string descriptionFileName = @"NetSuiteWhatsNewDescriptionData2022.csv";
            string userAddedFileName = @"NetSuiteWhatsNewUserAddedData2022.csv";
            string excelFileName = @"ReleaseWaveData2022.csv";

            releaseWave = SetAreaListComplete(releaseWave);

            SetColorSetList();
            //AddDummyDescriptionRow();

            var powerWheelRowsUnordered = ConvertToPowerWheelRows(releaseWave);

            //orders the rows based on their Ring0 value
            var powerWheelRows = powerWheelRowsUnordered.OrderBy(o => o.Ring0).ToList(); //reorder powerwheel rows by ring0.
            powerWheelRows = ApplyColors(powerWheelRows);

            //create a csv out of the rows
            var csvString = PowerWheelRow.GetCSVString(powerWheelRows);
            WriteFile(csvString, fileName);

            if (descriptionRows.Count > 0)
            {
                var csvStringDescriptionData = DescriptionRow.GetCSVString(descriptionRows);
                WriteFile(csvStringDescriptionData, descriptionFileName);
            }

            CreateUserAddedRows();
            if (userAddedRows.Count > 0)
            {
                var csvStringUserAddedData = UserAddedRow.GetCSVString(userAddedRows);
                WriteFile(csvStringUserAddedData, userAddedFileName);
            }

            //ReleaseWave client facing excel spreadsheet.
            //excelRows = excelRows.OrderBy(o => o.Product).ToList(); //reorder powerwheel rows by Product, which is the first column.

            if (excelRows.Count > 0)
            {
                var csvStringUserAddedData = ExcelRow.GetCSVString(excelRows);
                WriteFile(csvStringUserAddedData, excelFileName);
            }

            return releaseWave;
        }

        //Hard Coded function ******** Needs to be changed for each version
        public ReleaseWave SetAreaListComplete(ReleaseWave releaseWave)
        {

           Feature test = null;
            Feature test2 = null;
            List<String> groupsName = new List<String>();
            
            foreach (var area in releaseWave.AreaList)
            {
                foreach (var product in area.ProductList)
                {
                    List<Group> groups = new List<Group>();
                    foreach (var group in product.GroupList)
                    {
                        //checks each group to see if it contains a duplicate value with 20 leaf nodes
                        //if it does then it overwrites the same test2 value to give only one of the same features
                        if(group.FeatureList.Count() != 0)
                        {
                            //if (group.FeatureList[0].HeaderList.Count() == 20)
                            //{
                            //    test = group.FeatureList[0];
                            //    test2 = test;
                            //}

                            if (!groupsName.Contains(group.FeatureList[0].HeaderList[0].Name))
                            {
                                groupsName.Add(group.FeatureList[0].HeaderList[0].Name);
                                groups.Add(group);

                            }
                            else
                            {

                                Console.WriteLine("Removed");
                            }

                            List<Header> headers = group.FeatureList[0].HeaderList.GroupBy(p => p.Name).Select(g => g.First()).ToList();
                            group.FeatureList[0].HeaderList = headers;

                            //Count is also hardcoded for version
                            //group.FeatureList.RemoveAll(s => s.HeaderList.Count() == 20);
                            //if (group.FeatureList[0].HeaderList.Count() == 20)
                            //{
                            //    group.FeatureList.RemoveAt(0);
                            //}

                        }
                    }
                    product.GroupList = groups;
                }
            }
            //adds back in one of the duplicate values making it so the wheel only has one value
            if(test2 != null)
            {
                releaseWave.AreaList.First().ProductList.First().GroupList.First().FeatureList.Add(test2);
                Console.WriteLine("testing");
            }
            return releaseWave;
        }

        public void CreateUserAddedRows()
        {
            foreach (var descriptionRow in descriptionRows)
            {
                var userAddedRow = new UserAddedRow();
                userAddedRow.NodeName = descriptionRow.NodeName;

                //There are rows like 'CE', and 'Marketing' that are not actually new features,
                //and are just used as parent grouping nodes in the powerWheel.
                //Those rows won't have a business value.
                ////Set the GroupNode to 'Yes' so we know which these are and we can filter them out in excel.
                if (descriptionRow.BusinessValue == "") 
                {
                    userAddedRow.GroupNode = "Yes";
                }
                userAddedRows.Add(userAddedRow);
            }
        }

        public string GetParentAreaName(string _areaName)
        {
            //find the parent name for each of the areas for easier grouping
            string parentAreaName = _areaName;
            switch (_areaName)
            {
                case "Account Setup and Maintenance":
                case "Authentication":
                case "CSV Import":
                case "NetSuite Connector":
                case "User Interface":
                case "Taxation":
                    parentAreaName = "Tax";
                    break;


                case "Banking":
                case "Commerce":
                case "Commerce Sales and Marketing":
                case "SuiteCommerce InStore":
                case "Manufacturing":
                case "Accounting":
                    parentAreaName = "FS";
                    break;

                case "Employee Management":
                case "Inventory Management":
                case "Order Management":
                case "Projects":
                case "SuiteAnalytics":
                case "SuiteTalk Web Services Integration":
                case "SuiteScript":
                    parentAreaName = "MS";
                    break;

                case "Globalization":
                case "Mobile":
                case "SuiteApp Distribution":
                case "SuiteApps (Bundles) Released by NetSuite":
                case "SuiteCloud SDK":
                parentAreaName = "SA";
                    break;


                default:
                    parentAreaName = "FS";
                    break;

            }
            return parentAreaName; 
        }

        public string GetParentAreaFriendlyName(string _parentAreaName)
        {
            //set the easier to read name for the hovering view
            string parentFriendlyName = "";
            switch (_parentAreaName)
            {
                case "FS":
                    parentFriendlyName = "Finance and Operations";
                    break;

                case "Tax":
                    parentFriendlyName = "Taxation and User";
                    break;

                case "MS":
                    parentFriendlyName = "Management Services";
                    break;

                case "SA":
                    parentFriendlyName = "SuiteApps";
                    break;

            }
            return parentFriendlyName;
        }
        //Need to re-order by the first ring, since that is a ring we are adding after scrubbing the data.
        public List<PowerWheelRow> ReorderPowerWheelRows(List<PowerWheelRow> _powerWheelRowsUnordered)
        {
            List<PowerWheelRow> powerWheelRows = _powerWheelRowsUnordered.OrderBy(o => o.Ring0).ToList();

            return powerWheelRows;
        }


        public List<PowerWheelRow> ApplyColors(List<PowerWheelRow> _powerWheelRows)
        {
            string lastValue = "";
            foreach (var row in _powerWheelRows)
            {
                if (row.Ring0 != lastValue
                    && lastValue != "")
                {
                    if (colorIndex < colorSetList.Count - 1)
                    {
                        colorIndex++; //Change color for each Ring0
                    }
                }

                var colorSet = GetColorSet();

                if (colorSet.colorName != "")
                { 
                    row.Color = colorSet.colorName;
                }
                row.color = colorSet.color;
                row.color1 = colorSet.color1;
                row.color2 = colorSet.color2;
                row.color3 = colorSet.color3;
                row.color4 = colorSet.color4;
                row.color5 = colorSet.color5;

                
                lastValue = row.Ring0;
            }
            return _powerWheelRows;
        }

        public List<PowerWheelRow> ConvertToPowerWheelRows(ReleaseWave _releaseWave)
        {
            List<PowerWheelRow> powerWheelRows = new List<PowerWheelRow>();
            //need to go through every level and set the description
            foreach (var area in _releaseWave.AreaList)
            {
                SetDescriptionDataForAreaParent(GetParentAreaName(area.Name), _releaseWave, area);
                SetDescriptionDataForArea(_releaseWave, area);

                foreach (var product in area.ProductList)
                {
                    SetDescriptionDataForProduct(_releaseWave, area, product);
                    foreach (var group in product.GroupList)
                    {
                        SetDescriptionDataForGroup(_releaseWave, area, product, group);
                        foreach (var feature in group.FeatureList)
                        {
                            //feature can be null if it is a copy of another page
                            if(feature != null)
                            {
                                SetDescriptionDataForFeature(_releaseWave, area, product, group, feature);
                                foreach (var header in feature.HeaderList)
                                {
                                    SetDescriptionDataForHeader(_releaseWave, area, product, group, feature, header);
                                    powerWheelRows.Add(populatePowerWheelRow(_releaseWave, area, product, group, feature, header));

                                }
                            }
                            
                        }
                    }

                }

            }

            return powerWheelRows;
        }

        public ExcelRow populateExcelRow(string _parentProduct, ReleaseWave _releaseWave, Area _area, Product _product, Group _group, Feature _feature, Header _header, string _groupNode = "")
        {
            ExcelRow row = new ExcelRow();

            row.Product = _parentProduct;
            string productName = "";
            if (_product != null)
            {
                productName = _product.Name;
            }
            row.Category = productName; //_area.Name;

            string groupName = "";
            if (_group != null)
            {
                groupName = _group.Name;
            }
            row.SubCategory = groupName;

            row.FeatureType = "";

            if (_feature != null)
            {
                row.FeatureTitle = _feature.Name;
                row.BusinessValue = _feature.BusinessValue;
                row.FeatureDetails = _feature.FeatureDetails;
                row.ReleaseWave = _releaseWave.Name;
            }
            else
            {
                row.FeatureTitle = "";
                row.BusinessValue = "";
                row.FeatureDetails = "";
                row.ReleaseWave = "";
            }

            if (_header != null)
            {
                row.HeaderTitle = _header.Name;
                row.HeaderDetails = _header.HeaderDetails;
                row.FeatureDetails = _feature.FeatureDetails;
                row.ReleaseWave = _releaseWave.Name;
            }
            else
            {
                row.FeatureTitle = "";
                row.BusinessValue = "";
                row.FeatureDetails = "";
                row.ReleaseWave = "";
            }

            row.ImpactLevel = "";
            row.BusinessAndProfessionalServices = "";
            row.FoodAndBeverage = "";
            row.Retail = "";
            row.LifeSciences = "";
            row.NonProfit = "";
            row.HealthCare = "";
            row.Industrials = "";
            row.PublicSector = "";

            row.GroupNode = _groupNode;

            return row;
        }

        public PowerWheelRow populatePowerWheelRow(ReleaseWave __releaseWave, Area _area, Product _product, Group _group, Feature _feature, Header _header)
        {
            PowerWheelRow row = new PowerWheelRow();

            //We decided to get rid of the 'Area','group' and just move everything up a ring and add 'Header'.
            row.Ring0 = GetParentAreaName(_product.Name);
            row.Ring1 = _product.Name;
            row.Ring2 = _feature.Name;
            row.Ring3 = _header.Name;
            row.Ring4 = "";
            row.Ring5 = "";

            row.friendlyname = GetParentAreaFriendlyName(row.Ring0);
            row.friendlyname1 = _product.Name;
            row.friendlyname2 = _feature.Name;
            row.friendlyname3 = _header.Name;
            row.friendlyname4 = "";
            row.friendlyname5 = "";

            row.hyperlink = "";
            row.hyperlink1 = _product.Url;
            row.hyperlink2 = _feature.URL;
            row.hyperlink3 = _header.URL;
            row.hyperlink4 = "";
            row.hyperlink5 = "";

            row.Color = "Blue"; //not used anymore, but still needed
            row.size = "1"; //this can always be set to 1.

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

            row.metadata = "";
            row.metadata1 = "";
            row.metadata2 = "";
            row.metadata3 = "";
            row.metadata4 = "";
            row.metadata5 = "";

            return row;
        }

        public void SetDescriptionDataForAreaParent(string _parentName, ReleaseWave _releaseWave, Area _area)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            if (descriptionRows.Where(x => x.NodeName == _parentName).FirstOrDefault() == null)
            {
                descriptionRow.ParentNodeName = _parentName;
                descriptionRow.NodeName = _parentName;
                descriptionRow.Title = _parentName;
                descriptionRow.Description = _parentName;
                descriptionRow.ContactName = "";
                descriptionRow.EmailAddress = "";
                descriptionRow.ImageFileName = "";
                descriptionRow.ExternalURL = "";
                descriptionRow.InternalURL = "";
                descriptionRow.ExternalOnePage = "";
                descriptionRow.EmailSubject = "";
                descriptionRow.HTMLResult = "";

                descriptionRows.Add(descriptionRow);

                excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, null, _area, null, null, null,null, "Yes"));
            }
        }

        public void SetDescriptionDataForArea(ReleaseWave _releaseWave, Area _area)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
            descriptionRow.NodeName = _area.Name;
            descriptionRow.Title = _area.Name;
            descriptionRow.Description = _area.Name;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = ""; //_area.Url;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = _area.Url;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);

            excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, null, null, null, null, "Yes"));
        }


        public void SetDescriptionDataForProduct(ReleaseWave _releaseWave, Area _area, Product _product)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
            descriptionRow.NodeName = _product.Name;
            descriptionRow.Title = _product.Name;
            descriptionRow.Description = _product.Name;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = ""; //_product.Url;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = _product.Url;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);

            excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, _product, null, null,null, "Yes"));
        }

        public void SetDescriptionDataForGroup(ReleaseWave _releaseWave, Area _area, Product _product, Group _group)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
            descriptionRow.NodeName = _group.Name;
            descriptionRow.Title = _group.Name;
            descriptionRow.Description = _group.Name;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = ""; //_group.Url;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = _group.Url;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";

            descriptionRows.Add(descriptionRow);

            excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, _product, _group, null,null, "Yes"));
        }

        public void SetDescriptionDataForFeature(ReleaseWave _releaseWave, Area _area, Product _product, Group _group, Feature _feature)
        {
            if(_feature != null)
            {
                DescriptionRow descriptionRow = new DescriptionRow();

                descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
                descriptionRow.NodeName = _feature.Name;
                descriptionRow.Title = _feature.Name;

                descriptionRow.Description = _feature.Name;
                descriptionRow.ContactName = "";
                descriptionRow.EmailAddress = "";
                descriptionRow.ImageFileName = "";
                descriptionRow.ExternalURL = ""; //_feature.URL;
                descriptionRow.InternalURL = "";
                descriptionRow.ExternalOnePage = _feature.URL;
                descriptionRow.EmailSubject = "";
                descriptionRow.HTMLResult = "";
                descriptionRow.BusinessValue = _feature.BusinessValue;
                descriptionRow.FeatureDetails = _feature.FeatureDetails;

                descriptionRows.Add(descriptionRow);

                excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, _product, _group, _feature, null, "No"));
            }
        }

        public void SetDescriptionDataForHeader(ReleaseWave _releaseWave, Area _area, Product _product, Group _group, Feature _feature, Header _header)
        {
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
            descriptionRow.NodeName = _header.Name;
            descriptionRow.Title = _header.Name;
            descriptionRow.Description =  _header.HeaderDetails;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = ""; //_feature.URL;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = _header.URL;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";
            descriptionRow.HeaderDetails = _header.HeaderDetails;
            descriptionRow.BusinessValue = _feature.BusinessValue;
            descriptionRow.FeatureDetails = _feature.FeatureDetails;

            descriptionRows.Add(descriptionRow);

            excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, _product, _group, _feature, _header, "No"));
        }

        public List<Area> GetAreaList(string _releaseWaveUrl)
        {
            var areaList = new List<Area>();
            //use the releasewaveURL to find all of the area nodes, and add them to an AreaList object.
            var web = new HtmlWeb();
            var doc = web.Load(_releaseWaveUrl);

            try
            {
                //grabs the broad view
                var body = doc.DocumentNode.SelectNodes(
                    @"/html/body").First();

                //splits it up by each of the h5 title tags
                var tdList = body.SelectNodes("//h5");
                var tdArray = tdList.ToArray();
                    List<string> urls = new List<string>();

                for (int i = 0; i < tdArray.Count(); i++)
                {
                    var areaNode = tdArray[i];
                    if (areaNode != null
                        && areaNode.ChildNodes != null
                        && areaNode.ChildNodes.Count > 0
                        ) //purposely exclude this node and everything in it.
                    {
                        if (showDebugMessages)
                        {
                            Console.WriteLine("Area: " + CleanInput(areaNode.InnerText));
                        }
                        //create an area category with products inside
                        Area area = new Area(CleanInput(areaNode.InnerText), _releaseWaveUrl, GetProductList(areaNode,urls));
                        areaList.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetAreaList: " + ex.Message);
            }

            return areaList;

        }



        public List<Product> GetProductList(HtmlNode _areaNode,List<string> urls)
        {
            var productList = new List<Product>();

            try
            {
                //set up the information for the product value
                var productName = _areaNode.ChildNodes[1].InnerHtml;
                productName = CleanInput(productName);
                var Thref = _areaNode.ChildNodes[1].Attributes[0].Value;
                var url = docsMicrosoftBaseURL + Thref;

                if (showDebugMessages)
                {
                    Console.WriteLine("        Product:  " + productName);
                }
                //move to group level without breaking down further (just set up the url)
                Product product = new Product(productName, url, GetGroupList(url, productName, urls));
                productList.Add(product);

            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetProductList: " + ex.Message);
            }

            return productList;
        }

        public List<Group> GetGroupList(string _productLineURL, string _productName, List<string> urls)
        {
            _productName = CleanInput(_productName);
            var plannedFeaturesURL = _productLineURL;
            var web = new HtmlWeb();
            var doc = web.Load(plannedFeaturesURL);
            var groupList = new List<Group>();
            //use the _productNode to find all of the product nodes, and add them to an list object.

            try
            {
                //grab the body value
                var body = doc.DocumentNode.SelectNodes(
                    @"/html/body").First();

                //split up by p and h2 nodes seperately
                var tdList = body.SelectNodes("//p");
                var h2Nodes = body.SelectNodes("//h2");
                var count = 0;
                
                    for (int j = 0; j < tdList.Count; j++)
                    {
                        if (tdList[j].ChildNodes.Count > 1)
                        {
                        //for each p node check to see if it is a link that should be followed
                            if (tdList[j].ParentNode.Name == "li" && tdList[j].ChildNodes[1].Name == "a")
                            {
                            //if here then a link has been followed and there is a page further to go to
                                
                                
                                    count++;
                                    if (tdList[j].ChildNodes[1].Attributes[0].Value.Contains("section"))
                                    {
                                        var url = docsMicrosoftBaseURL + tdList[j].ChildNodes[1].Attributes[0].Value;
                                        Group group = new Group(_productName + " Testing", url, AltGetFeatureList(tdList[j], url, urls));
                                        groupList.Add(group);
                                    }
                                    else
                                    {
                                        var url = _productLineURL + tdList[j].ChildNodes[1].Attributes[0].Value;
                                        Group group = new Group(_productName + " Testing", url, AltGetFeatureList(tdList[j], url, urls));
                                        groupList.Add(group);
                                    }
                                
                                
                            }
                            else if (tdList[j].ChildNodes[1].Name == "a")
                            {
                                //check if the inner portion is a list
                                if (tdList[j].ParentNode.Name == "li")
                                {
                                    count++;
                                    var url = docsMicrosoftBaseURL + tdList[j].ChildNodes[1].Attributes[0].Value;
                                    Group group = new Group(_productName + " Testing", url, AltGetFeatureList(tdList[j], url, urls));
                                    groupList.Add(group);
                                }
                                //check if inner portion is a link in a table
                                else if (tdList[j].ParentNode.ParentNode.Name == "tr")
                                {
                                    count++;
                                    var url = docsMicrosoftBaseURL + tdList[j].ChildNodes[1].Attributes[0].Value;
                                    Group group = new Group(_productName + " Testing", url, AltGetFeatureList(tdList[j], url, urls));
                                    groupList.Add(group);
                                }
                                if (showDebugMessages)
                                {
                                    Console.WriteLine("      Group: " + _productName);
                                }
                                
                            }
                            else if (tdList[j].InnerText == "SuiteTax")
                            {
                                count++;
                                var url = docsMicrosoftBaseURL + tdList[j+1].ChildNodes[1].Attributes[0].Value;
                                Group group = new Group("SuiteTax", url, AltGetFeatureList(tdList[j+1], url, urls));
                            }
                        }
                    }
                //if no links were found just use that page for features
                if(count == 0)
                {
                    for (int z = 0; z < h2Nodes.Count; z++)
                    {
                        var name = h2Nodes[z].InnerText;
                        var url = docsMicrosoftBaseURL + "#" + h2Nodes[z].Id;
                        Group group = new Group(name, url, AltGetFeatureList(h2Nodes[z], url, urls));
                        groupList.Add(group);
                    }
                }    
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetGroupList: " + ex.Message);
            }

            var bar = groupList.GroupBy(x => x.Url).Select(x => x.First()).ToList();
            return bar;
        }


        public List<Feature> AltGetFeatureList(HtmlNode _tableNode, string _productLineURL, List<string> urls)
        {
            var featureList = new List<Feature>();

            try
            {
                //check if there are multiple nodes to split up or if just text
                if (_tableNode.ChildNodes.Count > 1)
                {
                    var body = _tableNode.ChildNodes[1];
                    var name = body.InnerHtml;
                    var feature = GetFeatureDetails(_productLineURL, name, urls);
                    featureList.Add(feature);
                }
                else
                {
                    var body = _tableNode.ChildNodes[0];
                    var name = body.InnerHtml;
                    var feature = GetFeatureDetails(_productLineURL, name, urls);
                    featureList.Add(feature);
                }

              

            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in AltGetFeaturesList: " + ex.Message);
            }
            
            return featureList;
        }
        

        public Feature GetFeatureDetails(string _url, string _featureName, List<string> urls)
        {
            var web = new HtmlWeb();
            var doc = web.Load(_url);

            var body = doc.DocumentNode.SelectNodes(
                    @"/html/body/article").First();
            var div = body.SelectNodes("//div");
            //split into each "div" node
            var tdArray = div.ToArray();
            var actualDiv = tdArray[1];
            Feature feature = null;

            try
            {
                
                    //check if this url has been scraped yet
                    //if not then add to scraped list and create a new feature
                    _featureName = RemoveCountry(_featureName);
                    feature = new Feature("", "", _url.Split("#")[0], _featureName, GetHeaderList(actualDiv, _url));
                    Console.WriteLine("                Individual Feature: " + _featureName);
                    urls.Add(_url);
                features.Add(feature);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetFeatureDetails: " + ex.Message);
            }

            return feature;
        }

        public String RemoveCountry(string c)
        {
            //remove the starting country value in features
            c = c.Replace("Australia ", "");
            c = c.Replace("Belgium ", "");
            c = c.Replace("China ", "");
            c = c.Replace("Germany ", "");
            c = c.Replace("India ", "");
            c = c.Replace("Ireland ", "");
            c = c.Replace("Japan ", "");
            c = c.Replace("Mexico ", "");
            c = c.Replace("Netherlands ", "");
            c = c.Replace("Norway ", "");
            c = c.Replace("Philippines ", "");
            c = c.Replace("Portugal ", "");
            c = c.Replace("Sweden ", "");
            c = c.Replace("United Kingdom ", "");
            return c;
        }

        public List<Header> GetHeaderList(HtmlNode _tableNode, string _featureLineURL)
        {
            var web = new HtmlWeb();
            var doc = web.Load(_featureLineURL);

            var body = doc.DocumentNode.SelectNodes(
                    @"/html/body/article").First();

            var headerList = new List<Header>();
            var h2nodes = body.SelectNodes("//div");
            var counter = 0;
            //split up by div

            if (h2nodes.Count > 0)
            {
                var startNode = h2nodes[2];


                while (startNode != null)
                {
                    //as long as nodes exist continue to make new header objects
                    if (startNode.ChildNodes.Count > 1 && startNode.ChildNodes[1].Name == "h2")
                    {
                        var name = startNode.ChildNodes[1].InnerText;
                        if (name != "Legacy Tax Suite Apps" && name != "SuiteTaxSuiteApps")
                        {
                            var url = _featureLineURL.Split("#")[0] + "#" + startNode.ChildNodes[1].Id;
                            Header header = new Header(startNode.InnerHtml, url, name);
                            headerList.Add(header);
                            Console.WriteLine("             Header: " + name);
                            counter++;
                        }
                    }
                    

                    startNode = startNode.NextSibling.NextSibling;
                }

                
                //if (counter == 0)
                //{
                //    var body2 = doc.DocumentNode.SelectNodes(@"/html/body/article").First();
                //    var h3nodes = body2.SelectNodes("//h3");
                //    var h3Arr = h3nodes.ToArray();
                //    HtmlNode start = null;
                //    if (h3Arr.Count() > 0)
                //    {
                //        start = h3Arr[0];
                //    }
                //    while (start != null)
                //    {
                //        if (start.Name == "h3" && start.ChildNodes[1].InnerText != "Related Topics")
                //        {
                //            var name = start.ChildNodes[1].InnerText;
                //            var url = _featureLineURL.Split("#")[0] + "#" + start.ChildNodes[1].Id;
                //            Header header = new Header(CleanInput(start.InnerHtml), url, name);
                //            headerList.Add(header);
                //            counter++;
                //        }
                //        start=start.NextSibling.NextSibling;
                //    }
                //}

                if (counter == 0)
                {
                    var h1nodes = body.SelectNodes(@"/html/body/article/header").First();

                    if (h1nodes.ChildNodes[3].Name == "h1")
                    {
                        var name = h1nodes.ChildNodes[3].InnerText;
                        var url = _featureLineURL;
                        Header header = new Header(CleanInput(h1nodes.ChildNodes[3].InnerHtml), url, name);
                        headerList.Add(header);
                        Console.WriteLine("             Header: " + name);
                    }
                    
                }
            }

            return headerList;
        }





        public List<ColorSet> SetColorSetList()
        {
            var colorSet = new ColorSet("#F1B434", "#F4C35D", "#F7D285", "#F9E1AE", "#F9E1AE", "#F9E1AE", "Yello");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#9949C0", "#9F5CC0", "#B27CCC", "#C59DD9", "#D8BDE5", "#EBDEF2", "Purple");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#23A796", "#34A798", "#5CB8AC", "#85CAC1", "#ADDBD5", "#D6EDEA", "Teal");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#E87722", "#ED924E", "#F1AD7A", "#F6C9A7", "#F6C9A7", "#F6C9A7", "RedOrange");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#E40046", "#E9336B", "#EF6690", "#F499B5", "#F499B5", "#F499B5", "Red");
            colorSetList.Add(colorSet);

            colorSet = new ColorSet("#A2A569", "#B5B787", "#C7C9A5", "#DADBC3", "#DADBC3", "#DADBC3", "Tan");
            colorSetList.Add(colorSet);

            //colorSet = new ColorSet("#009CDE", "#33B0E5", "#66C4EB", "#99D7F2", "#99D7F2", "#99D7F2", "Blue");
            //colorSetList.Add(colorSet);

            return colorSetList;
        }

        public ColorSet GetColorSet()
        {
            var colorSetArray = colorSetList.ToArray();
            if (colorSetList.Count > colorIndex)
            {
                return colorSetArray[colorIndex];
            }
            else
            {
                return colorSetArray[colorIndex - 1];
            }
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

        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {


                string cleaned = Regex.Replace(strIn, @"<[^>]+>|&nbsp;", "").Trim();
                cleaned = cleaned.Replace(",", "");

                cleaned = cleaned.Replace("\n", "").Replace("\r", "");
                cleaned = cleaned.Replace("Ã¢Â€Â“","-");
                cleaned = cleaned.Replace("Ã¢Â€Â”","-");
                cleaned = cleaned.Replace("â", "");
                cleaned = cleaned.Replace("??", "");
                cleaned = cleaned.Replace("A·", "");
                cleaned = cleaned.Replace("Ã¢Â€Â™","'"); 
                cleaned = cleaned.Replace("\u0080\u0094", "");
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
    }
}
