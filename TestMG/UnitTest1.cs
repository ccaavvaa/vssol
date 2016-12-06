using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MG;
using System.IO;
using System.Linq;

namespace TestMG
{
    [TestClass]
    public class MGTest
    {
        static readonly string N00ComposantPath = @"c:\tmp\Gizeh\N00-Redist\N00-Redist.xml";
        [TestInitialize]
        public void TestInitialize()
        {
            string[] directories = new string[]
            {
                 @"c:\tmp\Gizeh\debug\Client",
                 @"c:\tmp\Gizeh\debug\Server",
            };
            foreach(var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                if(!dirInfo.Exists)
                {
                    continue;
                }
                dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                    .ToList()
                    .ForEach(d => d.Delete(true));
                dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                    .ToList()
                    .ForEach(f => f.Delete());
            }
        }
        [TestMethod]
        public void TestComp2()
        {
            MGComposant n00Composant = new MGComposant(N00ComposantPath);
            n00Composant.FillSystemFiles();
            n00Composant.Destination[MGTarget.Client] = @"c:\tmp\Gizeh\debug\Client";
            n00Composant.Destination[MGTarget.Server] = @"c:\tmp\Gizeh\debug\Server";
            n00Composant.CopySystemFiles();
        }
    }
}
