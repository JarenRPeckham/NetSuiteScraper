﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class Product
    {
        public string Name { get; set;}

        public string Url { get; set; }

        public List<Feature> GroupList { get; set; }

        public Product()
        {

        }

        public Product(string _name, string _url, List<Feature> _groupList)
        {
            Name = ProcessReleaseWave.CleanInput(_name.Replace(",", ""));
            Url = _url;
            GroupList = _groupList;
        }
    }
}
