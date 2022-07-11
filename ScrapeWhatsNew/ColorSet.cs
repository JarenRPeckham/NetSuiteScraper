using System;
using System.Collections.Generic;
using System.Text;

namespace ScrapeWhatsNew
{
    public class ColorSet
    {
        public string colorName { get; set; }
        public string color { get; set; }
        public string color1 { get; set; }
        public string color2 { get; set; }
        public string color3 { get; set; }
        public string color4 { get; set; }
        public string color5 { get; set; }

        public ColorSet(string _color, string _color1, string _color2, string _color3, string _color4, string _color5, string _colorName = "")
        {
            color = _color;
            color1 = _color1;
            color2 = _color2;
            color3 = _color3;
            color4 = _color4;
            color5 = _color5;
            colorName = _colorName;
        }
    }
}
