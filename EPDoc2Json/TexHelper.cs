using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EPDoc2Json
{
    static class TexHelper
    {
        public static dynamic ReadTexAsObj(string TexFile)
        {
            var lines = File.ReadAllLines(TexFile, System.Text.Encoding.ASCII).ToList();
            dynamic section = new ExpandoObject();

            //sub-sections
            var subSections = new List<ExpandoObject>();


            var curIndex = 0;
            var total = lines.Count;

            while (curIndex < total)
            {
                string current = lines[curIndex];

                if (current.StartsWith("\\subsection"))
                {

                    var subsection = LoopSubsection(ref curIndex, total, ref lines);

                    subSections.Add(subsection);

                }

                curIndex++;
            }

            section.subsection = subSections;

            return section;



        }


        static string CleanHeader(string text)
        {

            //   \subsection{Coil:Cooling:Water}\label{coilcoolingwater}
            //   \subsubsection{Inputs}\label{inputs-35}
            //   \paragraph{ Field: Name}\label{ field - name - 34}

            var firstSp = text.IndexOf('{');
            var strings = text.Substring(firstSp).Split(new char[] { '\\' });

            var name = strings[0];
            name = name.Substring(1, name.Length - 2);

            //var label = strings[1].Trim(chars);

            return CleanString(name.Trim());
        }

        static string CleanString(string text)
        {
            return new System.Text.StringBuilder(text)
              .Replace("\\begin{lstlisting}", "")
              .Replace("\\end{lstlisting}", "")
              .Replace("\\begin{itemize}", "")
              .Replace("\\item", "")
              .Replace("\\end{itemize}", "")
              .Replace("\\textit", "")
              .Replace("\\textbf", "")
              .Replace("\\emph", "")
              .Replace("\\(", "")
              .Replace("\\)", "")
              .Replace("\\%", "%")
              .Replace("\\textgreater{}", ">")
              .Replace("\\textless{}", "<")
              .Replace("{[}", "[")
              .Replace("{]}", "]")
              .Replace("{[]}", "")
              .Replace("\\begin{enumerate}", "")
              .Replace("\\def\\labelenumi{\\arabic{enumi})}", "")
              .Replace("\\tightlist", "")
              .Replace("\\end{enumerate}", "")
              .Replace("\\#", "")
              .ToString();
        }

        static dynamic LoopSubsection(ref int CurrentIndex, int TotalCount, ref List<string> AllLines)
        {
            var curIndex = CurrentIndex;
            var currenLine = AllLines[curIndex];

            dynamic subsection = new ExpandoObject();
            var subName = CleanHeader(currenLine);
            subsection.Name = subName;
            var note = new List<string>();
            var subsubsections = new List<dynamic>();
            var paragraph = new List<dynamic>();

            Console.WriteLine("    -{0}", subName);
            curIndex++;

            while (curIndex < TotalCount)
            {
                currenLine = AllLines[curIndex];
                //break when hits next subsection
                if (currenLine.StartsWith("\\subsection"))
                {
                    Console.WriteLine("");
                    curIndex--;
                    break;
                }

                //find the items in subsection
                if (currenLine.StartsWith("\\subsubsection"))
                {

                    var subsubsection = LoopSubsubsection(ref curIndex, TotalCount, ref AllLines);
                    subsubsections.Add(subsubsection);

                }
                else if (currenLine.StartsWith("\\paragraph"))
                {
                    var para = LoopParagraph(ref curIndex, TotalCount, ref AllLines);
                    paragraph.Add(para);

                }
                else
                {
                    var cleaned = CleanString(currenLine);
                    if (note.Any() || !string.IsNullOrEmpty(cleaned))
                    {
                        note.Add(cleaned);
                    }
                }


                curIndex++;

            }

            if (subsubsections.Any())
                subsection.Subsubsections = subsubsections;

            if (note.Any())
                subsection.Note = note;

            if (paragraph.Any())
                subsection.Paragraph = paragraph;

            return subsection;
        }

        static dynamic LoopSubsubsection(ref int curIndex, int TotalCount, ref List<string> AllLines)
        {
            var currenLine = AllLines[curIndex];

            dynamic subsubsection = new ExpandoObject();
            var subName = CleanHeader(currenLine);
            subsubsection.Name = subName;
            var paragraph = new List<dynamic>();
            var note = new List<string>();

            Console.WriteLine("    -{0}", subName);
            curIndex++;

            while (curIndex < TotalCount)
            {
                currenLine = AllLines[curIndex];


                //break when hits next subsubsection or subsection
                if (currenLine.StartsWith("\\subsubsection") ||
                    currenLine.StartsWith("\\subsection"))
                {
                    Console.WriteLine("");
                    curIndex--;
                    break;
                }

                //find the items in subsubsection
                if (currenLine.StartsWith("\\paragraph"))
                {

                    var para = LoopParagraph(ref curIndex, TotalCount, ref AllLines);
                    paragraph.Add(para);

                }
                else
                {

                    var cleaned = CleanString(currenLine);
                    if (note.Any() || !string.IsNullOrEmpty(cleaned))
                    {
                        note.Add(cleaned);
                    }
                }

                curIndex++;


            }

            if (paragraph.Any())
                subsubsection.Paragraph = paragraph;

            if (note.Any())
                subsubsection.Note = note;

            return subsubsection;
        }

        static dynamic LoopParagraph(ref int curIndex, int TotalCount, ref List<string> AllLines)
        {
            var currenLine = AllLines[curIndex];

            dynamic paragraph = new ExpandoObject();
            var paragraphName = CleanHeader(currenLine);
            paragraph.Name = paragraphName;
            var note = new List<string>();


            Console.WriteLine("        -{0}", paragraphName);

            curIndex++;
            while (curIndex < TotalCount)
            {
                currenLine = AllLines[curIndex];

                //break when hits next subsubsection or subsection
                if (currenLine.StartsWith("\\subsubsection") ||
                    currenLine.StartsWith("\\subsection") ||
                    currenLine.StartsWith("\\paragraph"))
                {
                    Console.WriteLine("");
                    curIndex--;
                    break;
                }

                //find the content of paragraph 
                var cleaned = CleanString(currenLine);
                if (note.Any() || !string.IsNullOrEmpty(cleaned))
                {
                    note.Add(cleaned);
                }

                //Console.WriteLine("            -{0}", checkedContent);

                curIndex++;

            }

            if (note.Any())
                paragraph.Note = note;

            return paragraph;
        }
    }
}
