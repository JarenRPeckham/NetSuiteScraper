using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class ParentArea
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Area> AreaList { get; set; }

        public ParentArea()
        {

        }

        public ParentArea(string _name, string _url, List<Area> _areaList)
        {
            Name = _name.Replace(",", "");
            Url = _url;
            AreaList = _areaList;
        }
    }
}
