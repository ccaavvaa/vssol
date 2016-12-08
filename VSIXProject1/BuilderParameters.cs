using MG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class BuilderParameters
    {
        public static string[] BinariesList = new string[]
        {
            "AglNETCommon.dll",
            "AglNETCompression.dll",
            "AglNETDB.dll",
            "App_Licenses.dll",
            "Aspose.Cells.dll",
            "Aspose.Pdf.dll",
            "Aspose.Pdf.Kit.dll",
            "Aspose.Slides.dll",
            "Aspose.Words.dll",
            "avalon-framework-4.2.0.dll",
            "avalon-framework-cvs-20020806.dll",
            "batik-all-1.7.dll",
            "batik.dll",
            "BouncyCastle.Crypto.dll",
            "ChartFX.WinForms.Adornments.dll",
            "ChartFX.WinForms.Annotation.dll",
            "ChartFX.WinForms.Base.dll",
            "ChartFX.WinForms.dll",
            "commons-io-1.3.1.dll",
            "commons-logging-1.0.4.dll",
            "Devart.Data.dll",
            "Devart.Data.Oracle.dll",
            "Devart.Data.PostgreSql.dll",
            "DundasWinMap.dll",
            "EDBDataProvider2.0.2.dll",
            "fop-0.20.5.dll",
            "fop.dll",
            "HtmlAgilityPack.dll",
            "ICSharpCode.SharpZipLib.dll",
            "IKVM.OpenJDK.ClassLibrary.dll",
            "IKVM.Runtime.dll",
            "log4net.dll",
            "MGDIS.Evenement.ServicePublic.dll",
            "MGDIS.Exploitation.ServicePublic.dll",
            "MGDIS.GestionDossier.ServicePublic.dll",
            "MGDIS.M01.DB.dll",
            "MGDIS.M01.Soap.dll",
            "MGDIS.M01.Work.dll",
            "MGDIS.M01.Work.Interfaces.dll",
            "MGDIS.M17.DB.dll",
            "MGDIS.M17.Soap.dll",
            "MGDIS.M17.Work.dll",
            "MGDIS.M19.Soap.dll",
            "MGDIS.M19.Work.dll",
            "MGDIS.M75.Soap.dll",
            "MGDIS.M75.Work.dll",
            "MGDIS.M82.DB.dll",
            "MGDIS.M82.Soap.dll",
            "MGDIS.M82.Work.dll",
            "MGDIS.N01.DB.dll",
            "MGDIS.N01.Integration.dll",
            "MGDIS.N01.ServiceSvc.dll",
            "MGDIS.N01.Soap.dll",
            "MGDIS.N01.Work.dll",
            "MGDIS.N01.WS.dll",
            "MGDIS.N02.Common.dll",
            "MGDIS.N02.DB.dll",
            "MGDIS.N02.Soap.dll",
            "MGDIS.N02.Work.dll",
            "MGDIS.N02.Work.Interfaces.dll",
            "MGDIS.N03.Common.dll",
            "MGDIS.N03.Common.Interface.dll",
            "MGDIS.N03.Fop20.dll",
            "MGDIS.N03.Fop95.dll",
            "MGDIS.N03.Plugin.dll",
            "MGDIS.N03.Soap.dll",
            "MGDIS.N03.Web.dll",
            "MGDIS.N03.WFCommon.dll",
            "MGDIS.N03.WFWork.dll",
            "MGDIS.N03.Work.dll",
            "MGDIS.N03.Work.Interface.dll",
            "MGDIS.N05.DB.dll",
            "MGDIS.N05.Soap.dll",
            "MGDIS.N05.Work.Axes.dll",
            "MGDIS.N05.Work.Consult.dll",
            "MGDIS.N05.Work.dll",
            "MGDIS.N06.Soap.dll",
            "MGDIS.N06.Work.dll",
            "MGDIS.N06.Work.Interfaces.dll",
            "MGDIS.N11.Common.dll",
            "MGDIS.N11.Soap.dll",
            "MGDIS.N11.WFCommon.dll",
            "MGDIS.N11.WFWork.dll",
            "MGDIS.N11.Work.dll",
            "MGDIS.N14.DB.dll",
            "MGDIS.N14.Soap.dll",
            "MGDIS.N14.Work.dll",
            "MGDIS.N14.Work.Interfaces.dll",
            "MGDIS.N15.Common.dll",
            "MGDIS.N15.Soap.dll",
            "MGDIS.N15.Work.dll",
            "MGDIS.N16.Common.dll",
            "MGDIS.N16.DB.dll",
            "MGDIS.N16.Soap.dll",
            "MGDIS.N16.Work.dll",
            "MGDIS.N17.DB.dll",
            "MGDIS.N17.Soap.dll",
            "MGDIS.N17.Work.dll",
            "MGDIS.N19.Work.dll",
            "MGDIS.N19.Work.Execution.dll",
            "MGDIS.N20.Common.dll",
            "MGDIS.N22.Moteur.dll",
            "MGDIS.N22.Soap.dll",
            "MGDIS.N22.Work.Application.dll",
            "MGDIS.N22.Work.dll",
            "MGDIS.N27.Soap.dll",
            "MGDIS.N27.Work.dll",
            "MGDIS.N28.Work.dll",
            "MGDIS.N50.Common.dll",
            "MGDIS.N50.GenerateurSQL.dll",
            "MGDIS.N50.Work.dll",
            "MGDIS.Securite.ServicePublic.dll",
            "Microsoft.Practices.EnterpriseLibrary.Common.dll",
            "Microsoft.Practices.EnterpriseLibrary.Validation.dll",
            "Microsoft.Practices.ObjectBuilder.dll",
            "Microsoft.Practices.ObjectBuilder2.dll",
            "Microsoft.Practices.Unity.Configuration.dll",
            "Microsoft.Practices.Unity.dll",
            "Microsoft.Web.Services.dll",
            "Mono.Cecil.dll",
            "Mono.Security.dll",
            "N01-SOGOSEventPlugin-ATOM.dll",
            "N01-SOGOSEventPlugin-RSS.dll",
            "N01-SOGOSEventPlugin-TRTM.dll",
            "Newtonsoft.Json.dll",
            "Newtonsoft.Json.Schema.dll",
            "Npgsql.dll",
            "nunit.framework.dll",
            "PdfSharp.dll",
            "Rhino.Mocks.dll",
            "Solmetra.Spaw2.dll",
            "Solmetra.Spaw2.SpawFM.dll",
            "SPO.M82.Commun.dll",
            "SPO.SupportExterne.Common.dll",
            "SPO.SupportExterne.Work.dll",
            "SPOImportAPI.dll",
            "SPOServices.dll",
            "SpreadsheetGear.dll",
            "System.IdentityModel.Tokens.Jwt.dll",
            "System.Workflow.Activities.dll",
            "System.Workflow.ComponentModel.dll",
            "System.Workflow.Runtime.dll",
            "Telerik.Reporting.dll",
            "xalan-2.4.1.dll",
            "xercesImpl-2.2.1.dll",
            "xml-apis-1.3.04.dll",
            "xml-apis.dll",
            "xmldiffpatch.dll",
            "xmlgraphics-commons-1.3.1.dll",
            "MGDIS.FusionTiers.Console.exe",
            "MGDIS.N03.ImageGraphCarto.exe",
            "FarPoint.CalcEngine.dll",
            "FarPoint.Excel.dll",
            "FarPoint.PDF.dll",
            "FarPoint.PluginCalendar.WinForms.dll",
            "FarPoint.Win.Chart.Design.dll",
            "FarPoint.Win.Chart.dll",
            "FarPoint.Win.Design.dll",
            "FarPoint.Win.dll",
            "FarPoint.Win.Ink.dll",
            "FarPoint.Win.Spread.Design.dll",
            "FarPoint.Win.Spread.dll",
            "FarPoint.Win.Spread.Html.dll",
            "ILOG.Views.dll",
            "ILOG.Views.Gantt.dll",
            "ILOG.Views.Internal.dll",
            "Interop.tom.dll",
            "MGDIS.LanceurNET.dll",
            "MGDIS.M01.Comp.dll",
            "MGDIS.M17.Comp.dll",
            "MGDIS.M19.Comp.dll",
            "MGDIS.M82.Comp.dll",
            "MGDIS.N01.Comp.dll",
            "MGDIS.N01.WinForms.dll",
            "MGDIS.N02.Comp.dll",
            "MGDIS.N03.Comp.dll",
            "MGDIS.N05.Comp.dll",
            "MGDIS.N06.Comp.dll",
            "MGDIS.N11.Comp.dll",
            "MGDIS.N14.Comp.dll",
            "MGDIS.N15.Comp.dll",
            "MGDIS.N16.Comp.dll",
            "MGDIS.N17.Comp.dll",
            "MGDIS.N21.Comp.dll",
            "MontageProxy.dll",
            "policy.5.0.FarPoint.Win.Chart.Design.dll",
            "policy.5.0.FarPoint.Win.Design.dll",
            "policy.5.0.FarPoint.Win.dll",
            "policy.5.0.FarPoint.Win.Spread.Design.dll",
            "policy.5.0.FarPoint.Win.Spread.dll",
            "MGDIS.LanceurNET.exe",
        };
        public string WorkingRoot { get; set; }

        public string SolutionName { get; set; }

        public string ServerOutput { get; set; }

        public string ClientOutput { get; set; }
        public string ComposantFile { get; set; }

        public string KeyFile { get; set; }

        public string WorkingDir { get { return Path.Combine(this.WorkingRoot, this.Composant.Name); } }
        public MGComposant Composant { get; set; }
        public void Parse()
        {
            MGComposant composant = new MGComposant(this.ComposantFile);
            composant.Destination[MGTarget.Client] = this.ClientOutput;
            composant.Destination[MGTarget.Server] = this.ServerOutput;
            composant.Filter = BinariesList;
            composant.Parse();
            composant.FillSystemFiles();
            composant.CopySystemFiles();
            this.SolutionName = composant.Name;
            this.Composant = composant;
        }
    }

    public class ProjectParam
    {
        public ProjectParam(string root)
        {
            DirectoryInfo info = new DirectoryInfo(root);
            int rootLength = info.FullName.Length;
            Files = info.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(i =>
                {
                    var extension = i.Extension.ToLower();
                    if (extension == ".cs")
                    {
                        var secondExtension = Path.GetExtension(Path.GetFileNameWithoutExtension(i.FullName));
                        if (secondExtension.ToLower() == ".designer")
                        {
                            return false;
                        }
                        return true;
                    }
                    return false;
                })

                .OrderBy(i => i.FullName)
                .Select(i => i.FullName)
                .ToList();
            Source = info.FullName;
        }
        public string Name { get; set; }

        public string Source { get; set; }

        public readonly List<string> Files;
    }
}
