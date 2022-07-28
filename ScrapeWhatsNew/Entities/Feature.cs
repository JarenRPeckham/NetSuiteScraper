using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Feature
    {

        public string URL { get; set; }

        public string Name { get; set; }

        public List<Header> HeaderList { get; set; }

        public Feature( string _url, string _name, List<Header> _headerList)
        {
            
            HeaderList = _headerList; //Added list for headers inside
            URL = _url;
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
        }
    }
}
