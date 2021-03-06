﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj;
using EnvDTE100;
using System.Runtime.Versioning;
using VSLangProj110;
using System.IO;
using MG;
using VSLangProj140;

namespace VSIXProject1
{
    class SolutionBuilder
    {
        public readonly IServiceProvider ServiceProvider;

        public SolutionBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }
        public void Execute()
        {
            foreach (var composantFile in new string[]
            {
                //@"C:\tmp\Gizeh\N00-Redist\N00-Redist.xml",
                //@"C:\tmp\Gizeh\N20-Lanceur\N20-Lanceur.xml",
                //@"C:\tmp\Gizeh\N01-AGL\N01-AGL-NET45.xml",
                //@"C:\tmp\gizeh\N05-TableauxDeBord\N05-TableauxDeBord.xml",
                @"C:\tmp\gizeh\N15-MoteurRegles\N15-MoteurRegles-NET45.xml",
            })
            {
                this.CreateComposantSolution(composantFile);
            }
        }

        private void CreateComposantSolution(string composantFile)
        {

            var parameters = new BuilderParameters
            {
                WorkingRoot = @"c:\tmp\working",
                ClientOutput = @"c:\tmp\working\Debug\Client",
                ServerOutput = @"c:\tmp\working\Debug\Server",
                KeyFile = @"c:\tmp\working\Key.snk",
                ComposantFile = composantFile,
            };

            parameters.Parse();

            var _dte = ServiceProvider.GetService(typeof(DTE)) as DTE;
            var solution = (Solution4)_dte.Solution;
            if (solution.IsOpen)
            {
                bool doSave = Helper.Ask("Save solution {0} ?", solution.FullName);
                solution.Close(doSave);
            }

            if (!Helper.EmptyFolder(parameters.WorkingDir, true))
            {
                return;
            }
            solution.Create(parameters.WorkingDir, parameters.SolutionName);
            //            var solutionConfiguration = solution.SolutionBuild.SolutionConfigurations. .Item("Debug");
            //            solutionConfiguration.Activate();

            foreach (var p in parameters.Composant.Projects)
            {
                CreateProject(parameters, solution, p);
            }
            foreach(var d in Helper.GetEnumerable<BuildDependency>(solution.SolutionBuild.BuildDependencies))
            {
                d.RemoveAllProjects();
            }
            foreach (var p in parameters.Composant.Projects)
            {
                SetBuildDeps(solution, p);
            }

            solution.Close(true);
            System.Diagnostics.Process.Start(Path.Combine(parameters.WorkingDir, parameters.SolutionName + ".sln"));
            _dte.Quit();
        }

        private void SetBuildDeps(Solution4 solution, MGProject p)
        {
            var bd = solution.SolutionBuild.BuildDependencies;
            var solutionProjects = Helper.GetSolutionProjects(solution);
            var project = solutionProjects.FirstOrDefault(i => i.Name == p.Name);
            if(project == null)
            {
                return;
            }
            var d = Helper.GetEnumerable<BuildDependency>(bd).FirstOrDefault(i => i.Project == project);
            if(d == null)
            {
                return;
            }

            foreach(var aref in p.References.Where(r=>r.Type == MGReferenceType.Project))
            {
                var dp = solutionProjects.FirstOrDefault(i => i.Name == Path.GetFileNameWithoutExtension(aref.Name));
                if(dp == null)
                {
                    continue;
                }
                d.AddProject(dp.UniqueName);
            }
        }

        private void CreateProject(BuilderParameters parameters, Solution4 solution, MGProject p)
        {
            string template = solution.GetProjectTemplate("Microsoft.CSharp.ClassLibrary", "CSharp");
            solution.AddFromTemplate(template, Path.Combine(parameters.WorkingDir, p.Root, p.Name), p.Name, false);

            var solutionProjects = Helper.GetSolutionProjects(solution);
            var project = solutionProjects.FirstOrDefault(i => i.Name == p.Name);
            if (project != null)
            {
                SetTargetFramework(project);
            }
            project = solutionProjects.FirstOrDefault(i => i.Name == p.Name);
            CleanProject(project);
            AddReferences(parameters, solution, project, p);
            AddFiles(parameters, project, p);
            SetKeyFile(parameters.KeyFile, project);
            SetIcon(p.Icon, project);
            SetOutput(parameters, project, p);
            SetCompileNET45(project);
            project.Save();
        }

        private void SetCompileNET45(Project project)
        {
            string constants = Convert.ToString(
                project.ConfigurationManager.ActiveConfiguration.Properties.Item("DefineConstants").Value
                );
            constants = String.Join(";", constants.Split(';').Union(new string[] { "NET45" }));
            project.ConfigurationManager.ActiveConfiguration.Properties.Item("DefineConstants").Value = constants;
        }

        private void SetIcon(string icon, Project project)
        {
            if (!string.IsNullOrEmpty(icon))
            {
                project.Properties.Item("ApplicationIcon").Value = icon;
            }
        }

        private void SetKeyFile(string keyFile, Project project)
        {
            Property projProperty = project.Properties.Item("SignAssembly");
            bool signed = (bool)projProperty.Value;
            if (!signed)
            {
                project.Properties.Item("AssemblyOriginatorKeyFile").Value = keyFile;
                project.Properties.Item("SignAssembly").Value = true;
            }
        }

        private void SetOutput(BuilderParameters parameters, Project project, MGProject mgprj)
        {
            string serverPath = Path.Combine(parameters.ServerOutput, "bin");
            project.Properties.Item("RootNamespace").Value = mgprj.RootNamespace;
            project.Properties.Item("AssemblyName").Value = mgprj.Name;
            project.Properties.Item("OutputType").Value =
                mgprj.Type == OutputType.Library ?
                prjOutputType.prjOutputTypeLibrary : prjOutputType.prjOutputTypeWinExe;
            string path = (mgprj.OutputTarget & MGTarget.Client) == MGTarget.Client ?
                parameters.ClientOutput :
                serverPath;
            var outputPathProp = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath");
            outputPathProp.Value = path;
            if ((mgprj.OutputTarget & MGTarget.ClientAndServer) == MGTarget.ClientAndServer)
            {
                project.Properties.Item("PostBuildEvent").Value =
                    String.Format(
@"copy /y ""$(TargetPath)"" ""{0}""
copy / y ""$(TargetDir)$(TargetName).pdb"" ""{0}""
", serverPath);
            }
        }

        private void AddReferences(BuilderParameters parameters, Solution4 solution, Project project, MGProject mgprj)
        {
            var vsProject = project.Object as VSProject;
            var references = vsProject.References;
            AddFrameworkRefs(mgprj, references);
            AddRuntimeRefs(parameters, mgprj, references);
            AddProjectRefs(solution, mgprj, references);
            AddWebRefs(solution, mgprj, project);
        }

        private void AddWebRefs(Solution4 solution, MGProject mgprj, Project project)
        {
            var mgWebtRef = mgprj.References
                .Where(r => r.Type == MGReferenceType.Web);
            var vsproj = project.Object as VSProject;
            foreach (var aref in mgWebtRef)
            {
                string url = String.Concat("http://localhost/SpoNext/",
                    aref.Wsdl.Replace("?WSDL", String.Empty));
                var item = vsproj.AddWebReference(url);
                int start = url.LastIndexOf('/') + 1;
                int end = url.IndexOf(".asmx");
                string serviceName = url.Substring(start, end - start);
                item.Name = String.Concat("Service", serviceName);
            }
        }

        private static void AddProjectRefs(Solution4 solution, MGProject mgprj, References references)
        {
            var mgProjectRef = mgprj.References
                .Where(r => r.Type == MGReferenceType.Project);
            foreach (var aref in mgProjectRef)
            {
                var projectName = Path.GetFileNameWithoutExtension(aref.Name);
                var refprj = Helper.GetEnumerable<Project>(solution.Projects)
                    .FirstOrDefault(i => String.Compare(i.Name, projectName, true) == 0);
                if (refprj == null)
                {
                    throw new InvalidOperationException(String.Format("Project {0} not found", projectName));
                }
                references.AddProject(refprj);
            }
        }

        private static void AddRuntimeRefs(BuilderParameters parameters, MGProject mgprj, References references)
        {
            var mgRuntimeRef = mgprj.References
                .Where(r => r.Type == MGReferenceType.Runtime);
            foreach (var aref in mgRuntimeRef)
            {
                string path = (mgprj.OutputTarget & MGTarget.Client) == MGTarget.Client ?
                    parameters.ClientOutput :
                    Path.Combine(parameters.ServerOutput, "bin");

                path = Path.Combine(path, aref.Name);
                if (File.Exists(path))
                {
                    references.Add(path);
                }
                else
                {
                    //
                }
            }
        }

        private static void AddFrameworkRefs(MGProject mgprj, References references)
        {
            var mgFramworkRef = mgprj.References
                .Where(r => r.Type == MGReferenceType.Framework);
            foreach (var aref in mgFramworkRef)
            {
                references.Add(aref.Name);
            }
        }

        private static string[] excludeFiles = new string[]
        {
            @"Properties\Settings.designer.cs",
        };

        private void AddFiles(BuilderParameters parameters, Project project, MGProject mgproj)
        {
            foreach (var file in mgproj.Sources)
            {
                if (excludeFiles.FirstOrDefault(i => String.Compare(file, i, true) == 0) != null)
                {
                    continue;
                }

                string fileName = Path.GetFileName(file);
                string fullName = Path.Combine(parameters.Composant.Root, mgproj.Root, file);
                if (!File.Exists(fullName))
                {
                    continue;
                }
                string secondExtension = Path.GetExtension(Path.GetFileNameWithoutExtension(fileName));
                if (!String.IsNullOrEmpty(secondExtension)
                    && secondExtension.ToLower() == ".designer"
                    && File.Exists(Path.Combine(Path.GetDirectoryName(fullName),
                        Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName), ".cs"))))
                {
                    continue;
                }
                var elements = file.Split('\\', '/');
                ProjectItems projectItems;
                projectItems = elements.Length > 1 ?
                    CreateFolders(project.ProjectItems, elements.Take(elements.Length - 1)) :
                    project.ProjectItems;

                var fileWithoutPath = Path.GetFileName(fullName);
                var item = Helper.GetEnumerable<ProjectItem>(projectItems)
                    .FirstOrDefault(i => String.Compare(i.Name, fileWithoutPath, true) == 0);
                if (item == null)
                {
                    projectItems.AddFromFileCopy(fullName);
                }
            }
            AddResources(parameters, project, mgproj);
        }

        private void AddResources(BuilderParameters parameters, Project project, MGProject mgproj)
        {
            string projectSourcePath = Path.Combine(parameters.Composant.Root, mgproj.Root);
            DirectoryInfo dirInfo = new DirectoryInfo(projectSourcePath);
            if (!dirInfo.Exists)
            {
                return;
            }

            List<string> excludeExtensions = new List<string>()
            {
                ".cs",
                ".vb",
                ".xml",
                ".csproj",
                ".sln",
                ".settings",
                ".config",
            };
            var resources = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(f => !excludeExtensions.Contains(f.Extension.ToLower()))
                .Select(f => f.FullName).ToList();
            foreach (var file in resources)
            {
                string fileInProjectPath = file.Substring(dirInfo.FullName.Length);
                if (fileInProjectPath.StartsWith("Web References"))
                {
                    continue;
                }
                string projectPath = Path.GetDirectoryName(project.FileName);
                string fileInProject = Path.Combine(projectPath, fileInProjectPath);
                if (File.Exists(fileInProject))
                {
                    continue;
                }
                var elements = fileInProjectPath.Split('\\', '/');

                ProjectItems projectItems;
                projectItems = elements.Length > 1 ?
                    CreateFolders(project.ProjectItems, elements.Take(elements.Length - 1)) :
                    project.ProjectItems;

                var fileWithoutPath = Path.GetFileName(file);
                var item = Helper.GetEnumerable<ProjectItem>(projectItems)
                    .FirstOrDefault(i => String.Compare(i.Name, fileWithoutPath, true) == 0);
                projectItems.AddFromFileCopy(file);
            }
        }

        private ProjectItems CreateFolders(ProjectItems items, IEnumerable<string> folders)
        {
            foreach (var folder in folders)
            {
                var item = Helper.GetEnumerable<ProjectItem>(items).Where(i => i.Name == folder).FirstOrDefault();
                if (item == null)
                {
                    item = items.AddFolder(folder);
                }
                items = item.ProjectItems;
            }
            return items;
        }

        private static string[] SpecialVersion2 = new string[]
        {
            //"MGDIS.N20.Common",
            //"AglNETCommon",
        };
        private static void SetTargetFramework(Project project)
        {
            Version v = SpecialVersion2.FirstOrDefault(p => String.Compare(project.Name, p, true) == 0) != null ?
                new Version(2, 0) : new Version(4, 5);

            project.Properties.Item("TargetFrameworkMoniker").Value = new FrameworkName(".NETFramework", v).FullName;
        }

        //_dte.Solution.Close(true);


        private void CleanProject(Project project)
        {
            (project.Object as VSProject).Refresh();
            RemoveFiles(project);
            RemoveReferences(project);
        }

        private void RemoveReferences(Project project)
        {
            VSProject csproject = project.Object as VSProject;
            var references = Helper.GetEnumerable<Reference>(csproject.References);
            var exclude = new string[]
            {
                "mscorlib",
            };
            references.ForEach(r =>
            {
                if (exclude.FirstOrDefault(n => r.Name == n) == null)
                {
                    try
                    {
                        r.Remove();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(String.Format("Reference {0} can not be removed", r.Name), ex);
                    }
                }
            });
        }

        private void RemoveFiles(Project project)
        {
            var toRemove = new string[]
            {
                "Class1.cs",
                "AssemblyInfo.cs"
            };

            var projectItems = Helper.GetProjectItems(project);
            RecursiveDelete(toRemove, projectItems);
        }

        private void RecursiveDelete(IEnumerable<string> toRemove, IEnumerable<ProjectItem> items)
        {
            foreach (var item in items)
            {
                var subItems = Helper.GetProjectItems(item);
                this.RecursiveDelete(toRemove, subItems);
            }
            items.Where(i => toRemove.Where(n => n == i.Name).FirstOrDefault() != null)
                .ToList()
                .ForEach(i => i.Delete());
        }
    }

}