using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    /*
     ReleaseWave
	URL  
	Name (ex: 2021 release wave 1 plan)

	Area (Ex: Marketing)
	
		Product (ex Dynamics 365 Marketing)
			Group (ex AI-powered
				Feature (ex AI-driven recommendations for images)
					Name
					URL
					FeatureDetails
					BusinessValue
    */
    public class ReleaseWave
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Area> AreaList { get; set; }

        public ReleaseWave()
        {

        }

        public ReleaseWave(string _name, string _url, List<Area> _areaList)
        {
            Name = _name.Replace(",", "");
            Url = _url;
            AreaList = _areaList;
        }
    }
}
