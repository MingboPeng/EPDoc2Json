using Newtonsoft.Json;
using System;
using System.IO;

namespace EPDoc2Json
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var file = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\Doc\group-heating-and-cooling-coils.tex";
            var sectionObj = TexHelper.ReadTexAsObj(file);

            var jsonFile = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\DocJson\group-heating-and-cooling-coils.json";
            SaveAsJson(jsonFile, sectionObj);

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