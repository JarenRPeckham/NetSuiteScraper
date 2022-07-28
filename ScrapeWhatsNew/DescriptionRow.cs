using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class DescriptionRow
    {
        public string NodeName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string EmailAddress { get; set; }
        public string ImageFileName { get; set; }
        public string ExternalURL { get; set; }
        public string InternalURL { get; set; }
        public string ExternalOnePage { get; set; }
        public string EmailSubject { get; set; }
        public string HTMLResult { get; set; }
       

        
        public string HeaderDetails { get; set; }

        public string ParentNodeName { get; set; }

        public DescriptionRow()
        {
            Description = "";
            HeaderDetails = "";
        }

        public static string GetCSVString(List<DescriptionRow> _descriptionRows)
        {
            var output = PowerWheelRow.GenerateReport<DescriptionRow>(_descriptionRows);
            return output;
        }
    }
}

