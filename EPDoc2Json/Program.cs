using Newtonsoft.Json;
using System;
using System.IO;

namespace EPDoc2Json
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var dir = @"..\..\..\..\Doc\";
            var texfiles = Directory.GetFiles(dir, "*.tex");
            
            foreach (var TexFilePath in texfiles)
            {
              ProcessSingleFile(TexFilePath);
            }

            Console.Read();
        }

        private static void ProcessSingleFile(string TexFilePath)
        {
            var sectionObj = TexHelper.ReadTexAsObj(TexFilePath);
            // Output into DocJson/*.json
            FileInfo f = new FileInfo(TexFilePath);
            var JsonFilePath = Path.Combine(f.Directory.Parent.FullName, "DocJson", Path.ChangeExtension(f.Name, ".json"));
            Console.WriteLine(TexFilePath, JsonFilePath);

            SaveAsJson(JsonFilePath, sectionObj);
        }

        private static void SaveAsJson(string JsonFilePath, object SerialiableObj)
        {
            using (StreamWriter json = File.CreateText(JsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(json, SerialiableObj);
            }
        }
    }
}