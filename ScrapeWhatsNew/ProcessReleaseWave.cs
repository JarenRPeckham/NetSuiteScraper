﻿using HtmlAgilityPack;
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
        //TODO
        const string baseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html"; //"https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html";
        const string releaseWaveName = "2022 Wave 1";//"2021 Wave 2";
        string releaseWaveURL = "";
        //const string homepage = baseURL + "get-started/whats-new-home-page";
        const string docsMicrosoftBaseURL = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/";
        const string topHomePage = "https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/chapter_N3944673.html";
        public List<ColorSet> colorSetList = new List<ColorSet>();
        public int colorIndex = 0;
        List<DescriptionRow> descriptionRows = new List<DescriptionRow>();
        List<UserAddedRow> userAddedRows = new List<UserAddedRow>();
        List<ExcelRow> excelRows = new List<ExcelRow>();
        ReleaseWave releaseWave = null;
        bool showDebugMessages = true;

        public ReleaseWave Process()
        {
            releaseWave = new ReleaseWave(releaseWaveName, baseURL, null);
            releaseWaveURL = baseURL;

            releaseWave.AreaList = GetAreaList(releaseWave.Url);

            ////TODO?
            string fileName = @"NetSuiteWhatsNew2022.csv";
            string descriptionFileName = @"NetSuiteWhatsNewDescriptionData2022.csv";
            string userAddedFileName = @"NetSuiteWhatsNewUserAddedData2022.csv";
            string excelFileName = @"ReleaseWaveData2022.csv";

            SetColorSetList();
            //AddDummyDescriptionRow();

            var powerWheelRowsUnordered = ConvertToPowerWheelRows(releaseWave);

            var powerWheelRows = powerWheelRowsUnordered.OrderBy(o => o.Ring0).ToList(); //reorder powerwheel rows by ring0.
            powerWheelRows = ApplyColors(powerWheelRows);

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

        /// <summary>
        /// Use the descriptionRow list, which has one row per node, to create a similar list but with blank values
        /// for use in the MicrosoftWhatsNewUserAddedData file
        /// </summary>
        /// <returns></returns>
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
            string parentAreaName = _areaName;
            switch (_areaName)
            {
                //TODO
                case "Account Setup and Maintenance":
                case "Authentication":
                case "CSV Import":
                case "NetSuite Connector":
                case "User Interface":
                case "SuiteScript":
                case "Taxation":
                    parentAreaName = "CE";
                    break;

                //case "Accounting":
                //    parentAreaName = "Acc";
                //    break;

                case "Banking":
                case "Commerce":
                case "Commerce Sales and Marketing":
                case "SuiteCommerce InStore":
                case "Manufacturing":
                    parentAreaName = "FS";
                    break;

                case "Employee Management":
                case "Inventory Management":
                case "Order Management":
                case "Projects":
                case "SuiteAnalytics":
                case "SuiteTalk Web Services Integration":
                    parentAreaName = "BC";
                    break;

                case "Globalization":
                case "Mobile":
                case "SuiteApp Distribution":
                case "SuiteApps (Bundles) Released by NetSuite":
                case "SuiteCloud SDK":
                case "Accounting":
                    parentAreaName = "Other";
                    break;

                //case "Taxation":
                //    parentAreaName = "Tax";
                //    break;

                default:
                    parentAreaName = "FS";
                    break;

                //default:
                //    parentAreaName = _areaName;
                //    break;

            }
            return parentAreaName; 
        }

        public string GetParentAreaFriendlyName(string _parentAreaName)
        {
            string parentFriendlyName = "";
            switch (_parentAreaName)
            {
                case "FS":
                    parentFriendlyName = "Finance and Operations";
                    break;

                //case "Acc":
                //    parentFriendlyName = "Accounting";
                //    break;

                case "CE":
                    parentFriendlyName = "Customer Engagement";
                    break;

                //case "Tax":
                //    parentFriendlyName = "Taxation";
                //    break;

                case "BC":
                    parentFriendlyName = "Business Central";
                    break;

                case "Other":
                    parentFriendlyName = "Other";
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
                //TODO?
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
                            SetDescriptionDataForFeature(_releaseWave, area, product, group, feature);
                            foreach (var header in feature.HeaderList)
                            {
                                SetDescriptionDataForHeader(_releaseWave, area, product, group, feature, header);
                                powerWheelRows.Add(populatePowerWheelRow(_releaseWave, area, product, group, feature, header));

                            }
                        }
                    }

                }

                //if (colorIndex < colorSetList.Count - 1)
                //{
                //    //colorIndex++; //Change color for each Area
                //}
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
            //row.Group = groupName;

            row.FeatureType = "";

            if (_feature != null)
            {
                //TODO
                row.FeatureTitle = _feature.Name;
                row.BusinessValue = _feature.BusinessValue;
                row.FeatureDetails = _feature.FeatureDetails;
                row.ReleaseWave = _releaseWave.Name;
                //row.PublicPreview = _feature.PublicPreview;
                //row.DateAvailable = _feature.GeneralAvailability;
            }
            else
            {
                row.FeatureTitle = "";
                row.BusinessValue = "";
                row.FeatureDetails = "";
                row.ReleaseWave = "";
                //row.PublicPreview = "";
                //row.DateAvailable = "";
            }

            if (_header != null)
            {
                //TODO
                row.HeaderTitle = _header.Name;
                row.HeaderDetails = _header.HeaderDetails;
                row.FeatureDetails = _feature.FeatureDetails;
                row.ReleaseWave = _releaseWave.Name;
                //row.PublicPreview = _feature.PublicPreview;
                //row.DateAvailable = _feature.GeneralAvailability;
            }
            else
            {
                row.FeatureTitle = "";
                row.BusinessValue = "";
                row.FeatureDetails = "";
                row.ReleaseWave = "";
                //row.PublicPreview = "";
                //row.DateAvailable = "";
            }

            //TODO?
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
                //TODO?
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

            //TODO?
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

            //TODO?
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

            //TODO?
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
            DescriptionRow descriptionRow = new DescriptionRow();

            descriptionRow.ParentNodeName = GetParentAreaName(_area.Name);
            descriptionRow.NodeName = _feature.Name;
            descriptionRow.Title = _feature.Name;

            //TODO/UPDATE
            //string businessValue = "";
            //if (!String.IsNullOrEmpty(_feature.BusinessValue))
            //{
            //    businessValue = "<h2>Business Value</h2> <p>" + _feature.BusinessValue + "</p>";
            //}

            //string featureDetails = "";
            //if (!String.IsNullOrEmpty(_feature.FeatureDetails))
            //{
            //    featureDetails = "<h2>Feature details</h2> <p>" + _feature.FeatureDetails + "</p>";
            //}

            //TODO?
            descriptionRow.Description = _feature.Name;
            descriptionRow.ContactName = "";
            descriptionRow.EmailAddress = "";
            descriptionRow.ImageFileName = "";
            descriptionRow.ExternalURL = ""; //_feature.URL;
            descriptionRow.InternalURL = "";
            descriptionRow.ExternalOnePage = _feature.URL;
            descriptionRow.EmailSubject = "";
            descriptionRow.HTMLResult = "";
            //descriptionRow.PublicPreview = _feature.PublicPreview;
            //descriptionRow.GeneralAvailability = _feature.GeneralAvailability;
            descriptionRow.BusinessValue = _feature.BusinessValue;
            descriptionRow.FeatureDetails = _feature.FeatureDetails;

            descriptionRows.Add(descriptionRow);

            excelRows.Add(populateExcelRow(descriptionRow.ParentNodeName, _releaseWave, _area, _product, _group, _feature,null, "No"));
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
            //descriptionRow.PublicPreview = _feature.PublicPreview;
            //descriptionRow.GeneralAvailability = _feature.GeneralAvailability;
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
                //TODO
                ///html/body/main/div/div/div/article/div/h5[1]
                //var body = doc.DocumentNode.SelectNodes(
                //   @"/html/body/main/div/div/div/article/header/h1").First();

                var body = doc.DocumentNode.SelectNodes(
                    @"/html/body").First();

                var tdList = body.SelectNodes("//h5");
                var tdArray = tdList.ToArray();

                for (int i = 0; i < tdArray.Count(); i++)
                {
                    var areaNode = tdArray[i];
                    //var name = areaNode.ChildNodes[1].InnerHtml;
                    //var Thref = areaNode.ChildNodes[1].Attributes[0].Value;
                    //var testingUrl = docsMicrosoftBaseURL + Thref;
                    if (areaNode != null
                        //TODO
                        //Find areas not needed in the same list
                        && areaNode.ChildNodes != null
                        && areaNode.ChildNodes.Count > 0
                        //&& areaNode.InnerHtml != "In this article"
                        //&& areaNode.InnerHtml != "Is this page helpful?"
                        //&& areaNode.InnerHtml != "Industry accelerators"
                        //&& areaNode.InnerHtml != "Microsoft Cloud For Industry"
                        //&& areaNode.InnerHtml != "Microsoft Cloud for Industry Solutions"
                        ) //purposely exclude this node and everything in it.
                    {
                        if (showDebugMessages)
                        {
                            Console.WriteLine("Area: " + areaNode.InnerText);
                        }
                        Area area = new Area(areaNode.InnerText, _releaseWaveUrl, GetProductList(areaNode));
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



        public List<Product> GetProductList(HtmlNode _areaNode)
        {
            var productList = new List<Product>();
            //use the _areaNode to find all of the product nodes, and add them to an list object.
            //var web = new HtmlWeb();
            //var doc = web.Load(_releaseWaveUrl);

            try
            {

                //HtmlNodeCollection tdList = null;
                ////var checkNode = _areaNode.NextSibling.NextSibling;
                ////TODO
                //if (_areaNode.Name == "p")  //checkNode is a 'paragraph' node.
                //{
                //    while (_areaNode.Name == "p") //loop through each paragraph as there may be more than one within a 'header' tag.
                //    {
                //        var node = _areaNode.FirstChild; //The first child should be an 'a' link node.
                //        HtmlNode href = null;
                //        string productName = "";
                //        if (node.Name == "a") //wave 2 does not have a bullet point.
                //        {
                //            href = node;
                //            productName = href.InnerText;
                //        }
                //        var name = _areaNode.ChildNodes[1].InnerHtml;
                //        var Thref = _areaNode.ChildNodes[1].Attributes[0].Value;
                //        var url = docsMicrosoftBaseURL + Thref;

                //        //var url = releaseWaveURL + href.Attributes[0].Value;

                //        if (showDebugMessages)
                //        {
                //            Console.WriteLine("    Product: " + productName);
                //        }

                //        Product product = new Product(productName, url, AltGetFeatureList(_areaNode, productName));
                //        productList.Add(product);

                //        _areaNode = _areaNode.NextSibling.NextSibling; //go to the next paragraph.  The first sibling is 'text', so we need to move two.
                //    }
                //}
                //else if (_areaNode.Name == "ul")
                //{
                //    //TODO
                //    var body = _areaNode.NextSibling.NextSibling; //this is the ul node
                //    tdList = body.ChildNodes;

                //    var tdArray = tdList.ToArray();

                //    for (int i = 0; i < tdArray.Count(); i++)
                //    {
                //        var node = tdArray[i];
                //        if (node != null
                //            && node.ChildNodes != null
                //            && node.ChildNodes.Count > 0
                //            && (node.Name == "li"
                //                || node.Name == "a"))

                //        {
                //            /*HtmlNode strongTag = null;
                //            HtmlNode href = null;
                //            string productName = "";
                //            //TODO
                //            if (node.Name == "li")  //wave 1 has a bullet point
                //            {
                //                strongTag = node.FirstChild;
                //                productName = strongTag.InnerText;
                //                href = strongTag.FirstChild;
                //            }*/
                //            /*
                //            if (node.Name == "a") //wave 2 does not have a bullet point.
                //            {
                //                href = node;
                //                productName = href.InnerText;
                //            }
                //            */
                //            //var productName = _areaNode.ChildNodes[1].InnerHtml;
                //            //var Thref = _areaNode.ChildNodes[1].Attributes[0].Value;
                //            //var url = docsMicrosoftBaseURL + Thref;
                //            var productName = node.ChildNodes[1].ChildNodes[1].InnerHtml;
                //            var Thref = node.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
                //            var url = docsMicrosoftBaseURL + Thref;


                //            //var url = releaseWaveURL + href.Attributes[0].Value;

                //            if (showDebugMessages)
                //            {
                //                Console.WriteLine("    Product: " + productName);
                //            }

                //            Product product = new Product(productName, url, AltGetFeatureList(_areaNode, url));
                //            productList.Add(product);
                //        }
                //    }
                //}
                var productName = _areaNode.ChildNodes[1].InnerHtml;
                var Thref = _areaNode.ChildNodes[1].Attributes[0].Value;
                var url = docsMicrosoftBaseURL + Thref;
                Console.WriteLine("        Product:  " + productName);
                Product product = new Product(productName, url, GetGroupList(url, productName));
                productList.Add(product);

            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetProductList: " + ex.Message);
            }

            return productList;
        }

        public List<Group> GetGroupList(string _productLineURL, string _productName)
        {
            _productName = CleanInput(_productName);
            var plannedFeaturesURL = _productLineURL;
            var web = new HtmlWeb();
            var doc = web.Load(plannedFeaturesURL);
            var groupList = new List<Group>();
            //use the _productNode to find all of the product nodes, and add them to an list object.

            try
            {
                //TODO
                var body = doc.DocumentNode.SelectNodes(
                    @"/html/body").First();

                var tdList = body.SelectNodes("//p");
                var h2Nodes = body.SelectNodes("//h2");
                var count = 0;
                
                    for (int j = 0; j < tdList.Count; j++)
                    {
                        if (tdList[j].ChildNodes.Count > 1)
                        {
                            if (tdList[j].ParentNode.Name == "li" && tdList[j].ChildNodes[1].Name == "a")
                            {
                                count++;
                                var url = docsMicrosoftBaseURL + tdList[j].ChildNodes[1].Attributes[0].Value;
                                Group group = new Group(_productName + " Testing", url, AltGetFeatureList(tdList[j], url));
                                groupList.Add(group);

                            }
                            else if (tdList[j].ChildNodes[1].Name == "a")
                            {
                                if (tdList[j].ParentNode.Name == "li")
                                {
                                    count++;
                                    Group group = new Group(_productName + " Testing", plannedFeaturesURL, AltGetFeatureList(tdList[j], plannedFeaturesURL));
                                    groupList.Add(group);
                                }
                                else if (tdList[j].ParentNode.ParentNode.Name == "tr")
                                {
                                    count++;
                                
                                    Group group = new Group(_productName + " Testing", plannedFeaturesURL, AltGetFeatureList(tdList[j], plannedFeaturesURL));
                                    groupList.Add(group);
                                }
                                Console.WriteLine("      Group: "+_productName);
                            }
                        }
                    }
                
                if(count == 0)
                {
                    for (int z = 0; z < h2Nodes.Count; z++)
                    {
                        var name = h2Nodes[z].InnerText;
                        Group group = new Group(name, plannedFeaturesURL, AltGetFeatureList(h2Nodes[z], plannedFeaturesURL));
                        groupList.Add(group);
                    }
                }    
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetGroupList: " + ex.Message);
            }
            return groupList;
        }


        public List<Feature> AltGetFeatureList(HtmlNode _tableNode, string _productLineURL)
        {
            var featureList = new List<Feature>();

            try
            {
                if (_tableNode.ChildNodes.Count > 1)
                {
                    var body = _tableNode.ChildNodes[1];
                    var name = body.InnerHtml;
                    var feature = GetFeatureDetails(_productLineURL, name);
                    featureList.Add(feature);
                }
                else
                {
                    var body = _tableNode.ChildNodes[0];
                    var name = body.InnerHtml;
                    var feature = GetFeatureDetails(_productLineURL, name);
                    featureList.Add(feature);
                }

                //var tdArray = bodyChildren.ToArray();
                //List<HtmlNode> trList = new List<HtmlNode>();
                //Get tr's

                //for (int j = 0; j < tdArray.Count(); j++)
                //{
                //    var node = tdArray[j];
                //    if (node.Name == "tr")
                //    {
                //        trList.Add(node);
                //    }
                //}

                //loop through each tr and get the feature information
                //var trArray = trList.ToArray();

                //for (int j = 0; j < trArray.Count(); j++)
                //{

                //    var node = trArray[j];
                //    var td = node.ChildNodes[1];
                //    var aNode = td.ChildNodes[0];
                //    string featureName = aNode.InnerText;
                //    string url = _productLineURL;

                //    var feature = GetFeatureDetails(url, featureName);
                //    featureList.Add(feature);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in AltGetFeaturesList: " + ex.Message);
            }
            return featureList;
        }
        //public List<Feature> GetFeatureList(HtmlNode _groupNode, string _productLineURL)
        //{
        //    var featureList = new List<Feature>();
        //    //use the _areaNode to find all of the product nodes, and add them to an list object.
        //    //var web = new HtmlWeb();
        //    //var doc = web.Load(_releaseWaveUrl);

        //    try
        //    {
        //        int counter = 0;

        //        var table = _groupNode.NextSibling;
        //        while (true)
        //        {
        //            if (table.Name == "table" || counter == 20)
        //            {
        //                break;
        //            }
        //            counter++;
        //            table = table.NextSibling;
        //        }

        //        var body = table.ChildNodes[3];
        //        var bodyChildren = body.ChildNodes;
        //        var tdArray = bodyChildren.ToArray();
        //        List<HtmlNode> trList = new List<HtmlNode>();
        //        //Get tr's

        //        for (int j = 0; j < tdArray.Count(); j++)
        //        {
        //            var node = tdArray[j];
        //            if (node.Name == "tr")
        //            {
        //                trList.Add(node);
        //            }

        //        }
        //        //loop through each tr and get the feature information

        //        var trArray = trList.ToArray();

        //        for (int j = 0; j < trArray.Count(); j++)
        //        {
        //            var node = trArray[j];
        //            var td = node.ChildNodes[1];
        //            var aNode = td.ChildNodes[0];
        //            string featureName = aNode.InnerText;
        //            string url = _productLineURL + aNode.Attributes[0].Value;

        //            if (showDebugMessages)
        //            {
        //                Console.WriteLine("            Feature: " + featureName);
        //            }

        //            var feature = GetFeatureDetails(url, featureName);
        //            featureList.Add(feature);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("error: node not found in GetFeatureList: " + ex.Message);
        //    }
        //    return featureList;
        //}

        public Feature GetFeatureDetails(string _url, string _featureName)
        {
            var web = new HtmlWeb();
            var doc = web.Load(_url);

            var body = doc.DocumentNode.SelectNodes(
                    @"/html/body/article").First();
            //var body = doc.DocumentNode.SelectNodes(
            //        @"/html/body").First();

            //var tdList = body.SelectNodes("//h5");
            //var tdArray = tdList.ToArray();
            var div = body.SelectNodes("//div");
            var tdArray = div.ToArray();
            var actualDiv = tdArray[1];
            Feature feature = null;

            try
            {
                feature = new Feature("", "", _url, _featureName,GetHeaderList(actualDiv,_url));
                Console.WriteLine("                     Individual Feature: " + _featureName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: node not found in GetFeatureDetails: " + ex.Message);
            }

            return feature;
        }

        public List<Header> GetHeaderList(HtmlNode _tableNode, string _featureLineURL)
        {
            var web = new HtmlWeb();
            var doc = web.Load(_featureLineURL);

            var body = doc.DocumentNode.SelectNodes(
                    @"/html/body/article").First();

            var headerList = new List<Header>();
            var h2nodes = body.SelectNodes("//div");
            if(h2nodes.Count > 0)
            {
                var startNode = h2nodes[2];
                while (startNode != null)
                {
                    if (startNode.ChildNodes.Count > 1 && startNode.ChildNodes[1].Name == "h2")
                    {
                        var name = startNode.ChildNodes[1].InnerText;
                        Header header = new Header(startNode.InnerHtml, _featureLineURL, name);
                        headerList.Add(header);
                        Console.WriteLine("             Header: " + name);
                    }
                    startNode = startNode.NextSibling.NextSibling;
                }
            }
            //var bar = headerList.GroupBy(x => x.ParentNode).Select(x => x.First()).ToList();
            return headerList;
        }

        //public string GetListValuesString(HtmlNode node,String result)
        //{
        //    while (node != null)
        //    {
        //        if (node!=null && node.ChildNodes[1].Name == "a")
        //        {
        //            //CONTINUE FROM HERE
        //            //NEXT STEPS: Add for other options besides list and other options beside href value
        //            result += "\t\u2022" + "<a href =\""+CleanInput(node.ChildNodes[1].Attributes[0].Value)+"\">" + CleanInput(node.InnerText)+"</a>" + "\n\n";
        //        }
        //        node = node.NextSibling.NextSibling;
        //    }

        //    return ProcessReleaseWave.CleanInput(result);
        //}

        //public string GetFeatureDetailsString(HtmlNode _featureDetailsNode)
        //{
        //    string result = "";
        //    while (_featureDetailsNode != null && (_featureDetailsNode.Name != "h2" || _featureDetailsNode.Name != "div" || _featureDetailsNode.Name != "img"))
        //    {
        //        _featureDetailsNode = _featureDetailsNode.NextSibling;
        //        if (_featureDetailsNode != null)
        //        {
        //            result += CleanInput(_featureDetailsNode.InnerText) + " ";
        //        }

        //    }
        //    return ProcessReleaseWave.CleanInput(result);
        //}



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
                //string cleaned =  Regex.Replace(strIn, @"[^\w\.@-]", "",

                //                     RegexOptions.None, TimeSpan.FromSeconds(1.5));

                string cleaned = Regex.Replace(strIn, @"<[^>]+>|&nbsp;", "").Trim();
                /*cleaned = Regex.Replace(cleaned, @"[^\u0020-\u007F]", String.Empty);*/
                cleaned = cleaned.Replace(",", "");

                cleaned = cleaned.Replace("\n", "").Replace("\r", "");
                cleaned = cleaned.Replace("Ã¢Â€Â“","-");
                cleaned = cleaned.Replace("Ã¢Â€Â™","'");
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
