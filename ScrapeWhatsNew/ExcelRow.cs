using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class ExcelRow
    {
        public string Product { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        //public string Group { get; set; }
        public string FeatureType { get; set; }
        public string FeatureTitle { get; set; }
        public string BusinessValue { get; set; }
        public string FeatureDetails { get; set; }
        public string HeaderDetails { get; set; }
        public string HeaderTitle { get; set; }
        public string ReleaseWave { get; set; }
        public string PublicPreview { get; set; }
        public string DateAvailable { get; set; }
        public string ImpactLevel { get; set; }
        
        public string FoodAndBeverage { get; set; }
        public string Retail { get; set; }
        public string Industrials { get; set; }
        public string LifeSciences { get; set; }
        public string PublicSector { get; set; }
        public string HealthCare { get; set; }
        public string NonProfit { get; set; }
        public string BusinessAndProfessionalServices { get; set; }

        public string GroupNode { get; set; }

        public ExcelRow()
        {
            Product = "";
            Category = "";
            SubCategory = "";
            FeatureType = "";
            FeatureTitle = "";
            BusinessValue = "";
            FeatureDetails = "";
            HeaderDetails = "";
            HeaderTitle = "";
            ReleaseWave = "";
            PublicPreview = "";
            DateAvailable = "";
            ImpactLevel = "";
            BusinessAndProfessionalServices = "";
            FoodAndBeverage = "";
            Retail = "";
            LifeSciences = "";
            NonProfit = "";
            HealthCare = "";
            Industrials = "";
            PublicSector = "";
            GroupNode = "";
        }

        public static string GetCSVString(List<ExcelRow> _descriptionRows)
        {
            var output = PowerWheelRow.GenerateReport<ExcelRow>(_descriptionRows);
            return output;
        }
    }
}

