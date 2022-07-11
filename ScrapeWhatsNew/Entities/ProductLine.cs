using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class ProductLine
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Version> VersionList { get; set; }

        public ProductLine()
        {

        }

        public ProductLine(string _name, string _url, List<Version> _versionList)
        {
            Name = _name;
            Url = _url;
            VersionList = _versionList;
        }
    }
}
