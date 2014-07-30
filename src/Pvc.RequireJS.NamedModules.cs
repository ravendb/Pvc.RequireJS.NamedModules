using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcPlugins
{
    /// <summary>
    /// PVC plugin that goes through RequireJS modules and sets their names in the RequireJS definition.
    /// 
    /// This is useful for concatenating all the RequireJS class files into a single file; the modules
    /// will still be identified correctly when concatenated into a single file by loaders like DurandalJS or binding systems like KnockoutJS.
    /// 
    /// Before: 
    ///     define(["require", "exports"], function(require, exports) {
    ///         var MyFoo = (function () {
    ///            ...
    ///         })();
    ///     }
    ///     
    /// After:
    ///     define("MyFoo", ["require", "exports"], function(require, exports) {
    ///         var MyFoo = (function () {
    ///            ...
    ///         })();
    ///     })();
    /// </summary>
    public class PvcRequireJSNamedModules : PvcPlugin
    {
        private readonly Func<PvcStream, string> moduleNameGetter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleNameGetter">Function for getting the module name. If not specified, module name will be the name of the file without file extension.</param>
        public PvcRequireJSNamedModules(Func<PvcStream, string> moduleNameGetter = null)
        {
            if (moduleNameGetter == null)
            {
                moduleNameGetter = PvcRequireJSNamedModules.DefaultGetModuleName;
            }

            this.moduleNameGetter = moduleNameGetter;
        }

        public override string[] SupportedTags
        {
            get
            {
                return new[] { ".js" };
            }
        }

        public override IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
        {
            var typeScriptEmittedClasses = inputStreams
                .Where(s => this.IsAnonymousModule(s))
                .ToList();

            return inputStreams
                .Except(typeScriptEmittedClasses)
                .Concat(typeScriptEmittedClasses.Select(s => this.ConvertToNamedModule(s)))
                .ToList();
        }

        private PvcStream ConvertToNamedModule(PvcStream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var moduleName = this.moduleNameGetter(stream);                
                var originalContents = reader.ReadToEnd();
                var newContents = originalContents.Replace("define([", string.Format("define(\"{0}\", [", moduleName));
                
                if (originalContents != newContents)
                {
                    stream.UnloadStream();
                    File.WriteAllText(stream.OriginalSourcePath, newContents);
                    return PvcUtil.PathToStream(stream.OriginalSourcePath);
                }
                
                return stream;
            }
        }

        private bool IsAnonymousModule(PvcStream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                do
                {
                    var currentLine = reader.ReadLine();
                    if (currentLine != null && currentLine.StartsWith("define([", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                while (!reader.EndOfStream);
            }

            return false;
        }

        private static string DefaultGetModuleName(PvcStream stream)
        {
            return Path.GetFileNameWithoutExtension(stream.StreamName);
        }
    }
}
