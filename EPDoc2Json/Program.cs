using Newtonsoft.Json;
using System;
using System.IO;

namespace EPDoc2Json
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var file = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\Doc\group-heating-and-cooling-coils.tex";
            var dir = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\Doc\";
            var files = Directory.GetFiles(dir, "*.tex");
            foreach (var f in files)
            {
                var jsonFile = f.Replace(".tex", ".json");
                var sectionObj = TexHelper.ReadTexAsObj(f);
                SaveAsJson(jsonFile, sectionObj);
            }
           

            Console.Read();
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