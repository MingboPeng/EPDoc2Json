using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EPDoc2Json
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\Doc\group-heating-and-cooling-coils.tex";
            var sectionObj = TexHelper.ReadTexAsObj(file);

            var jsonFile = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\DocJson\group-heating-and-cooling-coils.json";
            SaveAsJson(jsonFile, sectionObj);
            
            Console.Read();
        }
       
        static void SaveAsJson(string JsonFilePath, object SerialiableObj)
        {
            using (StreamWriter json = File.CreateText(JsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(json, SerialiableObj);
            }
        }
    }

    
    
}
