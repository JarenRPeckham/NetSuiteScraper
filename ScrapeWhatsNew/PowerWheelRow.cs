using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrapeWhatsNew
{
    public class PowerWheelRow
    {
        public string Ring0 { get; set;}
        public string Ring1 { get; set; }
        public string Ring2 { get; set; }
        public string Ring3 { get; set; }
        public string Ring4 { get; set; }
        public string Ring5 { get; set; }
        public string Color { get; set; }
        public string size { get; set; }
        public string color { get; set; }
        public string color1 { get; set; }
        public string color2 { get; set; }
        public string color3 { get; set; }
        public string color4 { get; set; }
        public string color5 { get; set; }
        public string description { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public string description3 { get; set; }
        public string description4 { get; set; }
        public string description5 { get; set; }
        public string state { get; set; }
        public string state1 { get; set; }
        public string state2 { get; set; }
        public string state3 { get; set; }
        public string state4 { get; set; }
        public string state5 { get; set; }
        public string level { get; set; }
        public string level1 { get; set; }
        public string level2 { get; set; }
        public string level3 { get; set; }
        public string level4 { get; set; }
        public string level5 { get; set; }
        public string friendlyname { get; set; }
        public string friendlyname1 { get; set; }
        public string friendlyname2 { get; set; }
        public string friendlyname3 { get; set; }
        public string friendlyname4 { get; set; }
        public string friendlyname5 { get; set; }

        public string hyperlink { get; set; }
        public string hyperlink1 { get; set; }
        public string hyperlink2 { get; set; }
        public string hyperlink3 { get; set; }
        public string hyperlink4 { get; set; }
        public string hyperlink5 { get; set; }
        public string metadata { get; set; }
        public string metadata1 { get; set; }
        public string metadata2 { get; set; }
        public string metadata3 { get; set; }
        public string metadata4 { get; set; }
        public string metadata5 { get; set; }

        public static string GenerateReport<T>(List<T> items) where T : class
        {
            var output = "";
            var delimiter = ',';
            var properties = typeof(T).GetProperties()
             .Where(n =>
             n.PropertyType == typeof(string)
             || n.PropertyType == typeof(bool)
             || n.PropertyType == typeof(char)
             || n.PropertyType == typeof(byte)
             || n.PropertyType == typeof(decimal)
             || n.PropertyType == typeof(int)
             || n.PropertyType == typeof(DateTime)
             || n.PropertyType == typeof(DateTime?));

            using (var sw = new StringWriter())
            {
                var header = properties
                .Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);

                sw.WriteLine(header);

                foreach (var item in items)
                {
                    var row = properties
                    .Select(n => n.GetValue(item, null))
                    .Select(n => n == null ? "null" : n.ToString())
                    .Aggregate((a, b) => a + delimiter + b);
                    sw.WriteLine(row);
                }
                output = sw.ToString();
            }
            return output;
        }

        public static string GetCSVString(List<PowerWheelRow> _powerWheelRows)
        {
            var output = GenerateReport<PowerWheelRow>(_powerWheelRows);
            return output;
        }

    }

    
}
