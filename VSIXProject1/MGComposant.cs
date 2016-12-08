using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MG
{
    public class MGComposant
    {
        public MGComposant(string fileName)
        {
            this.FileName = fileName;
        }

        public bool IsCompilable { get; set; }
        public string Name { get; private set; }
        public readonly string FileName;
        public readonly List<MGProject> Projects = new List<MGProject>();
        public string Root
        {
            get
            {
                return Path.GetDirectoryName(this.FileName);
            }
        }

        public IEnumerable<string> Filter { get; internal set; }

        public readonly Dictionary<MGTarget, List<string>> SystemFiles = new Dictionary<MGTarget, List<string>>();
        public readonly Dictionary<MGTarget, string> Destination = new Dictionary<MGTarget, string>();
        public static readonly Dictionary<MGTarget, List<string>> SystemFolders = new Dictionary<MGTarget, List<string>>
            {
                {
                    MGTarget.Client, new List<string>
                    {
                        @"system\ClickOnce\",
                        @"system\ClickOnce_NET45\",
                    }
                },
                {
                    MGTarget.Server, new List<string>
                    {
                        @"system\Applicatif_NET20\",
                        @"system\Applicatif_NET45\",
                    }
                },
            };


        public void FillSystemFiles()
        {
            this.SystemFiles.Clear();

            foreach (var key in SystemFolders.Keys)
            {
                var keyFolders = SystemFolders[key];
                var files = new List<string>();
                this.SystemFiles[key] = files;
                keyFolders.Where(f => Directory.Exists(Path.Combine(Root, f)))
                    .Select(f => Directory.EnumerateFiles(Path.Combine(Root, f), "*", SearchOption.AllDirectories))
                    .ToList()
                    .ForEach(i =>
                    {
                        files.AddRange(i);
                    });
            }
        }

        public void CopySystemFiles()
        {
            foreach (var target in new MGTarget[] { MGTarget.Client, MGTarget.Server })
            {
                string destination = Destination[target];
                Directory.CreateDirectory(destination);
                var filesToCopy = SystemFiles[target];
                CopySystemFiles(target, destination, filesToCopy);
            }
        }

        private void CopySystemFiles(MGTarget target, string destination, List<string> filesToCopy)
        {
            var prefixes = SystemFolders[target].Select(f => Path.Combine(Root, f)).ToList();
            foreach (var file in filesToCopy)
            {
                foreach (var prefix in prefixes)
                {
                    if (file.StartsWith(prefix))
                    {
                        string dir = Path.GetDirectoryName(file);
                        string relativeDir = dir.Length > prefix.Length ? dir.Substring(prefix.Length) : String.Empty;
                        string destFolder = Path.Combine(destination, relativeDir);
                        string destFileName = Path.Combine(destFolder, Path.GetFileName(file));
                        Directory.CreateDirectory(destFolder);
                        File.Copy(file, destFileName, true);
                        break;
                    }
                }
            }
        }

        public void Parse()
        {
            this.Projects.Clear();
            var doc = XDocument.Load(this.FileName, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            var composantElement = doc.Descendants("composant").Single();
            this.Name = composantElement.Attribute("nom").Value;

            var csProjectsElements = composantElement.Descendants("projet")
                .Where(i => (i.Attribute("langage").Value == "cs") && 
                (i.Attribute("actif") == null || i.Attribute("actif").Value != "false"));
            foreach (var projectElement in csProjectsElements)
            {
                string output = MGProject.GetOutput(projectElement);
                bool isFiltered = Filter.FirstOrDefault(f => String.Compare(f, output, true) == 0) == null;
                if(isFiltered)
                {
                    continue;
                }
                this.IsCompilable = true;
                var project = this.CreateProject(projectElement);
                this.Projects.Add(project);
            }
        }


        public MGProject CreateProject(XElement projectElement)
        {
            var project = new MGProject();
            project.Parse(projectElement);
            return project;
        }
    }


    public enum OutputType
    {
        Unknown = 0,
        Library,
        WinExe,
    }

    [Flags]
    public enum MGTarget
    {
        None = 0,
        Client = 1,
        Server = 2,
        ClientAndServer = 3,
    }

    public enum MGReferenceType
    {
        Unknown = 0,
        Framework,
        Project,
        Runtime,
        Web,
    }

    public class MGReference
    {
        public MGReferenceType Type { get; set; }
        public string Composant { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Wsdl { get; private set; }

        public void Parse(XElement referenceElement)
        {
            var nameAtt = referenceElement.Attribute("nom");
            if (nameAtt != null) Name = nameAtt.Value;
            string type = referenceElement.Attribute("type").Value;
            switch (type)
            {
                case "framework":
                    Type = MGReferenceType.Framework;
                    break;
                case "projet":
                    Type = MGReferenceType.Project;
                    break;
                case "runtime":
                    Type = MGReferenceType.Runtime;
                    Composant = referenceElement.Attribute("composant").Value;
                    Version = referenceElement.Attribute("version").Value;
                    break;
                case "web":
                    Type = MGReferenceType.Web;
                    Wsdl = referenceElement.Attribute("wsdl").Value;
                    break;
                default:
                    throw new NotImplementedException(String.Concat("Reference type: ", type));
            }
        }

        public static MGReference FromReferenceElement(XElement referenceElement)
        {
            MGReference result = new MGReference();
            result.Parse(referenceElement);
            return result;
        }
    }

    public class MGProject
    {
        public string Root { get; private set; }
        public string Name { get; private set; }
        public OutputType Type { get; private set; }
        public string Output { get; private set; }
        public MGTarget OutputTarget { get; private set; }
        public List<string> Sources { get; private set; }
        public List<MGReference> References { get; private set; }
        public string RootNamespace { get; private set; }

        public void Parse(XElement projectElement)
        {
            this.Root = projectElement.Attribute("code").Value.Replace("/", "\\");
            this.Output = GetOutput(projectElement);
            this.Name = Path.GetFileNameWithoutExtension(this.Output);
            this.RootNamespace = projectElement.Attribute("rootnamespace").Value;

            this.SetOutputType(projectElement);
            this.SetTarget(projectElement);
            this.AddSources(projectElement);
            this.AddReferences(projectElement);
        }

        private static string[] ServerRefs = new string[]
        {
            "System.Configuration.dll",
            "System.Web.dll",
            "System.Web.Services.dll",
        };
        private static string[] ClientRefs = new string[]
        {
            "System.Configuration.dll",
            "System.Web.Services.dll",
        };
        private void AddReferences(XElement projectElement)
        {
            var referencesElement = projectElement.Elements("references")
                .Single();
            this.References = referencesElement.Descendants("reference")
                .Select(i => MGReference.FromReferenceElement(i))
                .ToList();
            AddAdditionalRefs(MGTarget.Client, ClientRefs);
            AddAdditionalRefs(MGTarget.Server, ServerRefs);
        }

        private void AddAdditionalRefs(MGTarget target, IEnumerable<string> refs)
        {
            if ((OutputTarget & target) == target)
            {
                foreach (var sr in refs)
                {
                    if (this.References.Where(r => r.Type == MGReferenceType.Framework
                        && String.Compare(r.Name, sr, true) == 0).FirstOrDefault() == null)
                    {
                        this.References.Add(
                            new MGReference()
                            {
                                Type = MGReferenceType.Framework,
                                Name = sr,
                            });
                    }
                }
            }
        }

        private void AddSources(XElement projectElement)
        {
            var sourceElement = projectElement.Descendants("sources")
                .Single();
            this.Sources = sourceElement.Descendants("include")
                .Select(i => i.Attribute("nom").Value.Replace("/", "\\"))
                .ToList();
        }

        private void SetTarget(XElement projectElement)
        {
            var targetElement = projectElement.Descendants("serveurcible").Single();
            var target = MGTarget.None;
            if (targetElement.Attribute("interface").Value.Contains("CLICKONCE"))
            {
                target = target | MGTarget.Client;
            }
            if (!String.IsNullOrEmpty(targetElement.Attribute("applicatif_NET20").Value))
            {
                target = target | MGTarget.Server;
            }
            this.OutputTarget = target;
        }


        private void SetOutputType(XElement projectElement)
        {
            string target = GetProjectAttribute(projectElement, "target");
            switch (target)
            {
                case "Library":
                    this.Type = OutputType.Library;
                    break;
                case "WinExe":
                    this.Type = OutputType.WinExe;
                    break;
                default:
                    throw new NotImplementedException(String.Concat("Target: ", target));
            }
        }

        private static string GetProjectAttribute(XElement projectElement, string attributeName)
        {
            var attElement = projectElement.Descendants("attribut")
                .Where(i => i.Attribute("nom").Value == attributeName)
                .Single();
            return attElement.Attribute("valeur").Value;
        }

        internal static string GetOutput(XElement projectElement)
        {
            return GetProjectAttribute(projectElement, "output");
        }
    }
}
