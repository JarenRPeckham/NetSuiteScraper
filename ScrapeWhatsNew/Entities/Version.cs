using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Version
    {
        public Version(string _versionNumber, string _buildNumber, string _autoUpdateAvailability, string _link)
        {
            VersionNumber = _versionNumber;
            BuildNumber = _buildNumber;
            AutoUpdateAvailability = _autoUpdateAvailability;
            Link = _link;
        }
        public string VersionNumber { get; set;}

        public string BuildNumber { get; set; }

        public string AutoUpdateAvailability { get; set; }

        public string Name { get; set; }
        public string Link { get; set; }

        public List<Feature> FeatureList { get; set;}

        public Version()
        {
           
        }

    }
}
