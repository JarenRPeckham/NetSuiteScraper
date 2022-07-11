using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class UserAddedRow
    {
        public string NodeName { get; set; }
        public string Recommendation { get; set; }
        public string UpgradeType { get; set; }
        
        public string FoodAndBeverage { get; set; }
        public string Retail { get; set; }
        public string Industrials { get; set; }
        public string LifeSciences { get; set; }
        public string PublicSector { get; set; }
        public string HealthCare { get; set; }
        public string NonProfit { get; set; }
        public string BusinessAndProfessionalServices { get; set; }

        public string GroupNode { get; set; }


        public List<string> stringList { get; set; }

        public UserAddedRow()
        {
            Recommendation = "G";
            UpgradeType = "x";
            
            FoodAndBeverage = "x";
            Retail = "x";
            Industrials = "x";
            LifeSciences = "x";
            PublicSector = "x";
            HealthCare = "x";
            NonProfit = "x";
            BusinessAndProfessionalServices = "x";
        }

        public static string GetCSVString(List<UserAddedRow> _rows)
        {
            var output = PowerWheelRow.GenerateReport<UserAddedRow>(_rows);
            return output;
        }
    }
}

