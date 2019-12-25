﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;


namespace Kipon.Xrm.Tools.CodeWriter
{
    public class ToolsCustomizeCodeDomService : ICustomizeCodeDomService
    {
        void ICustomizeCodeDomService.CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            var ns = (from c in Environment.GetCommandLineArgs() where c.StartsWith("/namespace:") select c.Split(':')[1]).Single();

            var entities = CodeWriterFilter.ENTITIES;
            using (var writer = new System.IO.StreamWriter("CrmUnitOfWork.Design.cs", false))
            {
                var sharedService = new SharedCustomizeCodeDomService(writer);

                writer.WriteLine($"// Tools Version {Kipon.Xrm.Tools.Version.No} Dynamics 365 svcutil extension tool by Kipon ApS, Kjeld Ingemann Poulsen");
                writer.WriteLine("// This file is autogenerated. Do not touch the code manually");
                writer.WriteLine("");
                writer.WriteLine("namespace " + ns);
                writer.WriteLine("{");
                writer.WriteLine("\tpublic partial class CrmUnitOfWork");
                writer.WriteLine("\t{");
                foreach (var logicalname in entities.Keys)
                {
                    var uowname = entities[logicalname].ServiceName;
                    writer.WriteLine("\t\tprivate IRepository<" + logicalname + "> _" + uowname.ToLower() + "; ");
                    writer.WriteLine("\t\tpublic IRepository<" + logicalname + "> " + uowname);
                    writer.WriteLine("\t\t{");
                    writer.WriteLine("\t\t\tget");
                    writer.WriteLine("\t\t\t{");
                    writer.WriteLine("\t\t\t\tif (_" + uowname.ToLower() + " == null)");
                    writer.WriteLine("\t\t\t\t\t{");
                    writer.WriteLine("\t\t\t\t\t\t_" + uowname.ToLower() + " = new CrmRepository<" + logicalname + ">(this.context);");
                    writer.WriteLine("\t\t\t\t\t}");
                    writer.WriteLine("\t\t\t\treturn _" + uowname.ToLower() + ";");
                    writer.WriteLine("\t\t\t}");
                    writer.WriteLine("\t\t}");
                }
                writer.WriteLine("\t}");

                sharedService.GlobalOptionSets(CodeWriterFilter.GLOBAL_OPTIONSET_INDEX.Values);
                sharedService.EntityOptionsetProperties(
                    CodeWriterFilter.ENTITIES,
                    CodeWriterFilter.GLOBAL_OPTIONSET_INDEX,
                    CodeWriterFilter.ATTRIBUTE_SCHEMANAME_MAP,
                    CodeWriterFilter.SUPRESSMAPPEDSTANDARDOPTIONSETPROPERTIES);

                writer.WriteLine("}");
            }

            using (var writer = new System.IO.StreamWriter("IUnitOfWork.Design.cs", false))
            {
                writer.WriteLine("// Dynamics 365 svcutil extension tool by Kipon ApS, Kjeld Ingemann Poulsen");
                writer.WriteLine("// This file is autogenerated. Do not touch the code manually");
                writer.WriteLine("");
                writer.WriteLine("namespace " + ns);
                writer.WriteLine("{");
                writer.WriteLine("\tpublic partial interface IUnitOfWork");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\t#region entity repositories");
                foreach (var logicalname in entities.Keys)
                {
                    var uowname = entities[logicalname];
                    writer.WriteLine("\t\tIRepository<" + logicalname + "> " + uowname + " { get; }");
                }
                writer.WriteLine("\t\t#endregion");
                writer.WriteLine("\t}");

                writer.WriteLine("}");
            }
        }
    }
}
