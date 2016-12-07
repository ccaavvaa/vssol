using System;
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
                @"C:\tmp\Gizeh\N20-Lanceur\N20-Lanceur.xml",
                //@"C:\tmp\Gizeh\N01-AGL\N01-AGL.xml",
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
            solution.Close(true);
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
            SetOutput(parameters, project, p);
            project.Save();
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
            var references = (project.Object as VSProject).References;
            AddFrameworkRefs(mgprj, references);
            AddRuntimeRefs(parameters, mgprj, references);
            AddProjectRefs(solution, mgprj, references);
        }

        private static void AddProjectRefs(Solution4 solution, MGProject mgprj, References references)
        {
            var mgProjectRef = mgprj.References
                .Where(r => r.Type == MGReferenceType.Project);
            foreach (var aref in mgProjectRef)
            {
                var projectName = Path.GetFileNameWithoutExtension(aref.Name);
                var refprj = Helper.GetEnumerable<Project>(solution.Projects)
                    .FirstOrDefault(i => String.Compare(i.Name, projectName, StringComparison.InvariantCultureIgnoreCase) == 0);
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
                references.Add(path);
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

        private void AddFiles(BuilderParameters parameters, Project project, MGProject mgproj)
        {
            foreach (var file in mgproj.Sources)
            {
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
            };
            var resources = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(f => !excludeExtensions.Contains(f.Extension.ToLower()))
                .Select(f => f.FullName).ToList();
            foreach (var file in resources)
            {
                string fileInProjectPath = file.Substring(dirInfo.FullName.Length);
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

        private static void SetTargetFramework(Project project)
        {
            project.Properties.Item("TargetFrameworkMoniker").Value = new FrameworkName(".NETFramework", new Version(4, 6)).FullName;
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