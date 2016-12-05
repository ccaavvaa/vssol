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
            var parameters = new BuilderParameters
            {
                SolutionFolder = @"c:\tmp\working",
                SolutionName = "Working",
                Projects = new ProjectParam[]
                {
                    new ProjectParam(@"C:\tmp\MySolution\ClassLibrary1") {Name = "x",}
                },
            };

            var _dte = ServiceProvider.GetService(typeof(DTE)) as DTE;
            var solution = (Solution4)_dte.Solution;
            if (solution.IsOpen)
            {
                bool doSave = Helper.Ask("Save solution {0} ?", solution.FullName);
                solution.Close(doSave);
            }
            if (!Helper.EmptyFolder(parameters.SolutionFolder, true))
            {
                return;
            }
            solution.Create(parameters.SolutionFolder, parameters.SolutionName);

            foreach (var p in parameters.Projects)
            {
                CreateProject(parameters, solution, p);
            }
        }

        private void CreateProject(BuilderParameters parameters, Solution4 solution, ProjectParam p)
        {
            string template = solution.GetProjectTemplate("Microsoft.CSharp.ClassLibrary", "CSharp");
            solution.AddFromTemplate(template, Path.Combine(parameters.SolutionFolder, p.Name), p.Name, false);

            var solutionProjects = Helper.GetSolutionProjects(solution);
            var project = solutionProjects.FirstOrDefault(i => i.Name == p.Name);
            if (project != null)
            {
                SetTargetFramework(project);
            }
            project = solutionProjects.FirstOrDefault(i => i.Name == p.Name);
            CleanProject(project);
            AddFiles(parameters, project, p);
        }

        private void AddFiles(BuilderParameters parameters, Project project, ProjectParam para)
        {
            foreach(var file in para.Files)
            {
                project.ProjectItems.AddFromFileCopy(file);
            }
        }

        private static void SetTargetFramework(Project project)
        {
                //CSharpProjectProperties6 properties = project.Properties as CSharpProjectProperties6;
                //properties.TargetFrameworkMoniker = new FrameworkName(".NETFramework", new Version(4, 6)).FullName;
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