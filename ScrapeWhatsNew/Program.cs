using HtmlAgilityPack;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace ScrapeWhatsNew
{
    class Program
    {

        static void Main(string[] args)
        {
            //ProcessWebsites processWebsites = new ProcessWebsites();
            //processWebsites.Process();

            ProcessReleaseWave processReleaseWave = new ProcessReleaseWave();
            var releaseWave = processReleaseWave.Process();

            Console.WriteLine("done");
            Console.ReadLine();
        }

        
    }
}
