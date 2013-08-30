using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;

namespace Core
{
    public static class ModuleManager
    {

        public static void LoadModuleAsync(string moduleName, Action<Module> startModule)
        {
            var uri = new Uri(moduleName + ".xap", UriKind.Relative);
            var client = new WebClient();
            client.OpenReadCompleted += (sender, args) =>
            {
                var stream = args.Result;

                OnLoadModuleAsyncComplete(moduleName, stream, startModule);
            };
            client.OpenReadAsync(uri);
        }

        public static void OnLoadModuleAsyncComplete(string moduleName, Stream stream, Action<Module> startModule)
        {
            var manifest = LoadManifest(stream);

            var root = XDocument.Parse(manifest).Root;
            XNamespace ns = @"http://schemas.microsoft.com/client/2007/deployment";

            var parts = root.Elements(ns + "Deployment.Parts")
                            .Elements(ns + "AssemblyPart");

            Assembly asmWithStartModule = null;

            //handle assembly parts
            foreach (XElement elm in parts)
            {
                var source = elm.Attribute("Source").Value;
                var asmPart = new AssemblyPart();
                var streamInfo = Application.GetResourceStream(
                     new StreamResourceInfo(stream, "application/binary"),
                     new Uri(source, UriKind.Relative));
                var asm = asmPart.Load(streamInfo.Stream);

                if (source == moduleName + ".dll")
                {
                    asmWithStartModule = asm;
                }
            }

            //handle external parts

            if (asmWithStartModule != null)
            {
                var mtype = asmWithStartModule.GetTypes().Single(typeof(Module).IsAssignableFrom);
                var module = (Module)Activator.CreateInstance(mtype);
                startModule(module);
            }
        }

        public static string LoadManifest(Stream stream)
        {
            var manifest = new StreamReader(Application.GetResourceStream(
                                                new StreamResourceInfo(stream, null),
                                                new Uri("AppManifest.xaml", UriKind.Relative)).Stream).ReadToEnd();

            return manifest;
        }

    }
}
