using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Group
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Feature> FeatureList { get; set; }

        public Group(string _name, string _url, List<Feature> _featureList)
        {
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
            Url = _url;
            FeatureList = _featureList;
        }
    }
}
