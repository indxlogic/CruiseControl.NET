using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;

namespace InDxLogic.Reversion
{

    namespace AssemblyReversion
    {
        [ReflectorType("reversion")]
        public class ReVersion : ITask
        {
            private const string regexPattern = "\\[assembly: ([A-Za-z]+)\\(.*\\)\\]";

            private string InformationalVersion
            {
                get
                {
                    switch (Minor)
                    {
                        case 0: return "Developer Release";
                        case 1: return "Early Adopter Version";
                        case 2: return "Release Candidate";
                        case 3: return "General Release";
                        default: return "General Release Patch Level " + (Minor - 3).ToString();
                    }
                }
            }

            [ReflectorProperty("date")]
            public bool Date { get; set; }

            [ReflectorProperty("major")]
            public int Major { get; set; }

            [ReflectorProperty("minor")]
            public int Minor { get; set; }

            [ReflectorProperty("build")]
            public int Build { get; set; }

            [ReflectorProperty("assemblyProduct", Required = false)]
            public string AssemblyProduct { get; set; }

            [ReflectorProperty("assemblyInfoPath")]
            public string AssemblyInfoPath { get; set; }

            public void Run(IIntegrationResult result)
            {
                var dic = new Dictionary<string, string>();
                var keyValues = new string[] { "AssemblyCopyright", "AssemblyVersion", "AssemblyFileVersion", "AssemblyInformationalVersion", "AssemblyCompany", "AssemblyProduct" };

                var lines = File.ReadAllLines(AssemblyInfoPath);
                foreach (var l in lines)
                    if (!l.StartsWith("//"))
                    {
                        var regex = new Regex(regexPattern);
                        var m = regex.Match(l);
                        if (m.Success)
                            dic.Add(m.Groups[1].Value, l);
                        else
                            dic.Add(Path.GetRandomFileName(), l);
                    }

                foreach (var kv in keyValues) if (!dic.ContainsKey(kv)) dic.Add(kv, string.Empty);

                var _Obsfucation = Date ? DateTime.Now.ToString("yyMM") : Build.ToString();
                var _Build = Date ? DateTime.Now.ToString("dd") : result.IntegrationProperties["CCNetNumericLabel"].ToString();

                dic["AssemblyVersion"] = string.Format("[assembly: {0}(\"{1}.{2}.{3}.{4}\")]", "AssemblyVersion", Major, Minor, _Obsfucation, _Build);
                dic["AssemblyFileVersion"] = string.Format("[assembly: {0}(\"{1}.{2}.{3}.{4}\")]", "AssemblyFileVersion", Major, Minor, _Obsfucation, _Build);
                dic["AssemblyInformationalVersion"] = string.Format("[assembly: {0}(\"{1}.{2} {3}\")]", "AssemblyInformationalVersion", Major, Minor, InformationalVersion);
                dic["AssemblyCopyright"] = string.Format("[assembly: AssemblyCopyright(\"Copyright © InDxLogic, Inc. 2007-{0}\")]", DateTime.Now.Year);
                dic["AssemblyCompany"] = "[assembly: AssemblyCompany(\"InDxLogic, Inc.\")]";

                if (!string.IsNullOrEmpty(AssemblyProduct))
                    dic["AssemblyProduct"] = string.Format("[assembly: AssemblyProduct(\"{0}\")]", AssemblyProduct);

                var sb = new StringBuilder();
                foreach (var kvp in dic.Values)
                    sb.AppendLine(kvp);

                File.Delete(AssemblyInfoPath);
                File.WriteAllText(AssemblyInfoPath, sb.ToString());
            }
        }
    }
}
