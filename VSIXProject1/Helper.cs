using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE100;
using System.Collections;
using System.IO;

namespace VSIXProject1
{
    static class Helper
    {
        private static Package _package;
        public static readonly int YES = 6;
        public static bool Ask(string question, params object[] args)
        {
            string message = args != null && args.Length > 0 ? String.Format(question, args) : question;
            return VsShellUtilities.ShowMessageBox(
                _package, message, "Question"
                , OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
            ) == YES;
        }

        public static void Error(Exception ex)
        {
            var messages = new string[]
            {
                ex.Message,
                "StackTrace:",
                ex.StackTrace,
            };
            var message = String.Join(Environment.NewLine, messages);
            VsShellUtilities.ShowMessageBox(
                _package, message, "Error"
                , OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
            );
        }

        public static void Execute(Action a)
        {
            try
            {
                a();
            }
            catch(Exception ex)
            {
                Error(ex);
            }
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> toExecute)
        {
            foreach (var item in enumerable)
            {
                toExecute(item);
            }
        }

        public static bool IsEmptyDirectory(this string path)
        {
            if (!Directory.Exists(path))
            {
                return true;
            }
            return Directory.EnumerateFileSystemEntries(path).FirstOrDefault() == null;
        }
        public static IEnumerable<T> GetEnumerable<T>(object o)
        {
            var enumerable = o as IEnumerable;
            if (enumerable != null)
            {
                foreach (var obj in enumerable)
                {
                    yield return (T)obj;
                }
            }
        }

        internal static bool EmptyFolder(string path, bool ask)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }

            if (ask && !Helper.IsEmptyDirectory(path))
            {
                if (!Ask("Folder {0} is not empty.{1}Do you want to remove his content?", path, Environment.NewLine))
                {
                    return false;
                }
            }

            Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ForEach(f => File.Delete(f));
            Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).ForEach(d => Directory.Delete(d, true));
            return true;
        }

        public static IEnumerable<Project> GetSolutionProjects(Solution4 solution)
        {
            return GetEnumerable<Project>(solution.Projects);
        }

        internal static void Initialize(Package package)
        {
            _package = package;
        }

        internal static IEnumerable<ProjectItem> GetProjectItems(Project project)
        {
            return GetEnumerable<ProjectItem>(project.ProjectItems);
        }
        internal static IEnumerable<ProjectItem> GetProjectItems(ProjectItem projectItem)
        {
            return GetEnumerable<ProjectItem>(projectItem.ProjectItems);
        }
    }
}
