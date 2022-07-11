using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Area
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Product> ProductList { get; set; }

        public Area()
        {

        }

        public Area(string _name, string _url, List<Product> _productList)
        {
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
            Url = _url;
            ProductList = _productList;
        }
    }
}
