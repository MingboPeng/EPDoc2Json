﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace EPDoc2Json
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var dir = @"..\..\..\..\Doc\";
            //var texfiles = Directory.GetFiles(dir, "*.tex");

            var folder = $"./download";
            Directory.CreateDirectory(folder);
            var outputFolder = $"./outputs";
            Directory.CreateDirectory(outputFolder);

            foreach (var texfile in _texList)
            {
                var tex = DownloadTex(folder, texfile);
                ProcessSingleFile(outputFolder, tex);

            }

            Console.Read();
        }


        private static string[] _texList = {

            "group-air-distribution-equipment.tex",
            "group-air-distribution.tex",
            "group-condenser-equipment.tex",
            "group-coil-cooling-dx.tex",
            "group-controllers.tex",
            "group-design-objects.tex",
            "group-fans.tex",
            "group-heat-recovery.tex",
            "group-heating-and-cooling-coils.tex",
            "group-humidifiers-and-dehumidifiers.tex",
            "group-performance-curves.tex",
            "group-plant-condenser-loops.tex",
            "group-plant-equipment.tex",
            "group-pumps.tex",
            "group-radiative-convective-units.tex",
            "group-unitary-equipment.tex",
            "group-variable-refrigerant-flow-equipment.tex",
            "group-water-systems.tex",
            "group-water-heaters.tex",
            "group-zone-equipment.tex",
            "group-zone-forced-air-units.tex",
            "group-setpoint-managers.tex",

        };


        private static string DownloadTex(string folder, string fileName)
        {

            //group-heating-and-cooling-coils.tex
            var url = $"https://raw.githubusercontent.com/NREL/EnergyPlus/develop/doc/input-output-reference/src/overview/{fileName}";
           
            var saveAs = $"{folder}/{fileName}";
            Console.WriteLine($"Downloading {fileName}");
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(url, saveAs);
                Console.WriteLine($"Saved to {saveAs}");
            }

            return saveAs;

        }

        private static void ProcessSingleFile(string folder, string TexFilePath)
        {
            var sectionObj = TexHelper.ReadTexAsObj(TexFilePath);
            // Output into DocJson/*.json
            FileInfo f = new FileInfo(TexFilePath);
            var JsonFilePath = Path.Combine(folder, Path.ChangeExtension(f.Name, ".json"));
            Console.WriteLine(TexFilePath, JsonFilePath);

            SaveAsJson(JsonFilePath, sectionObj);

            Console.WriteLine($"Saved to {JsonFilePath}");
        }

        private static void SaveAsJson(string JsonFilePath, object SerialiableObj)
        {
            using (StreamWriter json = File.CreateText(JsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                // Pretty printing indented instead of one-line
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(json, SerialiableObj);
            }
        }
    }
}