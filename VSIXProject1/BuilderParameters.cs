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
        public string WorkingRoot { get; set; }

        public string SolutionName { get; set; }

        public string ServerOutput { get; set; }

        public string ClientOutput { get; set; }
        public string ComposantFile { get; set; }

        public string WorkingDir { get { return Path.Combine(this.WorkingRoot, this.Composant.Name); } }
        public MGComposant Composant { get; set; }
        public void Parse()
        {
            MGComposant composant = new MGComposant(this.ComposantFile);
            composant.Destination[MGTarget.Client] = this.ClientOutput;
            composant.Destination[MGTarget.Server] = this.ServerOutput;
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
                        if(secondExtension.ToLower()==".designer")
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
