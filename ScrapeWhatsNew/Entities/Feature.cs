using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Feature
    {
        //TODO
        public string FeatureDetails { get; set; }
        public string BusinessValue { get; set; }

        public string URL { get; set; }

        public string Name { get; set; }

        public Feature(string _featureDetails, string _businessValue, string _url, string _name)
        {
            FeatureDetails = ProcessReleaseWave.CleanInput(_featureDetails);
            BusinessValue = ProcessReleaseWave.CleanInput(_businessValue);
            URL = _url;
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
        }
    }
}
