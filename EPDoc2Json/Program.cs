using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace EPDoc2Json
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"C:\Users\mingo\Documents\GitHub\EPDoc2Json\Doc\group-heating-and-cooling-coils.tex";
            ReadTexAsObj(file);

            Console.WriteLine("Hello World!");

            Console.Read();
        }
        static void ReadTexAsObj(string TexFile)
        {
            var lines = File.ReadAllLines(TexFile, System.Text.Encoding.ASCII).ToList();
            dynamic section = new ExpandoObject();
            
            //sub-sections
            var subSections = new List<ExpandoObject>();
            section.subsection = subSections;
            var subSectionCount = 0;

            var curIndex = 0;
            var total = lines.Count;
            
            while (curIndex < total) 
            {
                string current = lines[curIndex];

                if (current.StartsWith("\\subsection"))
                {
                    dynamic subsection = new ExpandoObject();
                    var name = CleanHeader(current);
                    Console.WriteLine(name);

                    subsection.Name = name;
                    subsection.Note = new List<string>();
                    
                    curIndex ++;

                    LoopSubsection(ref curIndex, total, ref lines, ref subsection);

                    subSections.Add(subsection);
                    subSectionCount++;


                }
                //else
                //{
                //    if (subSectionCount == 0)
                //    {
                //        continue;
                //    }

                //    if (current.StartsWith("\\begin{itemize}") ||
                //      current.StartsWith("\\item") ||
                //      current.StartsWith("\\end{itemize}"))
                //    {
                //        continue;
                //    }


                //    //var currentSub = subSections.Last();
                //    //currentSub

                //}

                curIndex++;
            }




        }
        static void ReadTex(string TexFile)
        {
            var lines = File.ReadAllLines(TexFile, System.Text.Encoding.GetEncoding(1252)).ToList();
            var c = lines.Count();
            var subSecs = new List<List<string>>();

            var subCount = 0;
            for (int i = 0; i < c; i++)
            {
                string current = lines[i];
                
                if (current.StartsWith("\\subsection"))
                {
                    subSecs.Add(new List<string>() { current });
                    subCount++;
                }
                else
                {
                    if (subCount == 0)
                    {
                        continue;
                    }

                    if (current.StartsWith("\\begin{itemize}") ||
                      current.StartsWith("\\item") ||
                      current.StartsWith("\\end{itemize}"))
                    {
                        continue;
                    }

                    var currentSub = subSecs.Last();
                    currentSub.Add(current);
                    
                }


            }
        }

        static void MakeSubsection(List<string> strings)
        {
            var subsec = strings;
            dynamic subObj = new ExpandoObject();

            subObj.SubSubSection = new List<ExpandoObject>();
            var curLine = 0;
            var total = subsec.Count;
            var isInSubSubSec = false;
            var subsubCount = 0;
            while (curLine < total)
            {
                var item = subsec[curLine];
                if (item.StartsWith("\\subsection"))
                {
                    subObj.Name = item;
                    isInSubSubSec = false;
                }

                if (item.StartsWith("\\subsubsection"))
                {
                    dynamic subsubObj = new ExpandoObject();
                    subsubObj.Paragraph = new List<ExpandoObject>();
                    subsubObj.Name = item;
                    subObj.SubSubSection.Add(subsubObj);
                    isInSubSubSec = true;
                    subsubCount++;
                }
                else
                {

                    if (isInSubSubSec)
                    {
                        dynamic subsubObj = subObj.SubSubSection[subsubCount - 1];

                        if (item.StartsWith("\\paragraph"))
                        {
                            dynamic paragragh = new ExpandoObject();
                            paragragh.Name = System.Text.RegularExpressions.Regex.Split(item.Replace("\\paragraph", ""), "label")[0].Trim(new char[] { '{', '\\', '}' });
                            paragragh.Note = new List<string>();
                            subsubObj.Paragraph.Add(paragragh);
                        }
                        else
                        {
                            var curPCount = subsubObj.Paragraph.Count;
                            if (curPCount > 0)
                            {
                                dynamic paragragh = subsubObj.Paragraph[curPCount - 1];
                                paragragh.Note.Add(item);

                            }


                        }
                    }
                }
                curLine++;
            }


        }

        static string CleanSubSection(string text)
        {
            //    \subsection{Coil:Cooling:Water}\label{coilcoolingwater}
            var strings = text.Substring(11).Split("\\label");
            var name = strings[0].Trim(new char[] { '{', '\\', '}' });
            var label = strings[1].Trim(new char[] { '{', '\\', '}' });

            return name;
        }

       
        static string CleanSubSubSection(string text)
        {
            //   \subsubsection{Inputs}\label{inputs-35}
            var strings = text.Substring(14).Split("\\label");
            var name = strings[0].Trim(new char[] { '{', '\\', '}' });
            var label = strings[1].Trim(new char[] { '{', '\\', '}' });

            return name;

        }

        static string CleanHeader(string text)
        {

            //   \subsection{Coil:Cooling:Water}\label{coilcoolingwater}
            //   \subsubsection{Inputs}\label{inputs-35}
            //   \paragraph{ Field: Name}\label{ field - name - 34}

            var firstSp = text.IndexOf('{');
            var chars = new char[] { '{', '\\', '}', ' ' };
            var strings = text.Substring(firstSp).Split("\\label");

            var name = strings[0].Trim(chars);
            var label = strings[1].Trim(chars);

            return name;
        }

        static void LoopSubsection(ref int CurrentIndex, int TotalCount, ref List<string> AllLines, ref dynamic SubsectionObj)
        {
            var curIndex = CurrentIndex;
            while (curIndex < TotalCount)
            {
                var subCurrent = AllLines[curIndex];
                //break when hits next subsection
                if (subCurrent.StartsWith("\\subsection"))
                {
                    Console.WriteLine("");
                    break;
                }

                //find the items in subsection
                if (subCurrent.StartsWith("\\subsubsection"))
                {
                    dynamic subsubsection = new ExpandoObject();
                    var subName = CleanHeader(subCurrent);
                    subsubsection.Name = subName;


                    Console.WriteLine("    -{0}", subName);
                    curIndex++;

                    LoopSubsubsection(ref curIndex, TotalCount, ref AllLines, ref subsubsection);

                    //SubsectionObj.Add(subsubsection);

                }
                else
                {
                    SubsectionObj.Note.Add(subCurrent);
                }

                
                curIndex++;



            }
        }

        static void LoopSubsubsection(ref int CurrentIndex, int TotalCount, ref List<string> AllLines, ref dynamic SubsectionObj)
        {
            var curIndex = CurrentIndex;
            while (curIndex < TotalCount)
            {
                var subsubCurrent = AllLines[curIndex];

                //break when hits next subsubsection or subsection
                if (subsubCurrent.StartsWith("\\subsubsection")||
                    subsubCurrent.StartsWith("\\subsection"))
                {
                    Console.WriteLine("");
                    break;
                }

                //find the items in subsubsection
                if (subsubCurrent.StartsWith("\\paragraph"))
                {
                    dynamic paragraph = new ExpandoObject();
                    var paragraphName = CleanHeader(subsubCurrent);
                    paragraph.Name = paragraphName;


                    Console.WriteLine("        -{0}", paragraphName);


                }
                
                curIndex++;



            }
        }

    }
    
}
