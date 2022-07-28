using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Header
    {
        //setter for the header details section for excel
        public string HeaderDetails { get; set; }

        //specific url of the header (should have a #[NUMBER] format at the end)
        public string URL { get; set; }

        public string Name { get; set; }

        //Constructor for creating the header
        public Header(string _headerDetails, string _url, string _name)
        {
            HeaderDetails = _headerDetails;
            URL = _url;
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
        }
    }
}
