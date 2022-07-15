using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Header
    {
        public string HeaderDetails { get; set; }

        public string URL { get; set; }

        public string Name { get; set; }

        public Header(string _headerDetails, string _url, string _name)
        {
            HeaderDetails = ProcessReleaseWave.CleanInput(_headerDetails);
            URL = _url;
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
        }
    }
}
