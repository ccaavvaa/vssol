using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MG;

namespace TestMG
{
    [TestClass]
    public class MGTest
    {
        static readonly string N00ComposantPath = @"D:\Projets\sol\N00-Redist\N00-Redist.xml";
        [TestMethod]
        public void TestComp2()
        {
            MGComposant n00Composant = new MGComposant(N00ComposantPath);
            n00Composant.FillSystemFiles();
            n00Composant.Destination[MGTarget.Client] = @"D:\Projets\sol\debug\Client";
            n00Composant.Destination[MGTarget.Server] = @"D:\Projets\sol\debug\Server";
            n00Composant.CopySystemFiles();
        }
    }
}
