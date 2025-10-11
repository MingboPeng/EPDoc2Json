using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            var strings = text.Substring(firstSp).Split("\\label");

            var name = strings[0];
            name = name.Substring(1, name.Length - 2);

            //var label = strings[1].Trim(chars);

            return CleanString(name.Trim());
        }


        static string CleanString(string text)
        {
            var removes = new[]
            {
                @"\\hyperref\[.*?\]",
                @"\\begin{.*?}.*",
                @"\\end{.*?}.*",
                @"\\item",
                @"\\protect\s*",
                @"\\hyperlink",


                @"\\emph",
                @"\\{1,2}\(", // remove \\( \(
                @"\\{1,2}\)", // remove \\) \)
                @"{\[\]}",
                @"\\tightlist",
                @"\\#",
                "\"",
                @"\\toprule",
                @"\\midrule",
                @"\\endfirsthead",
                @"\\endhead",
                @"\\centering",
                @"\\includegraphics.*", // anything starts with \includegraphics
   
                @"\\caption{.*",
                @".*\\tabularnewline",
                @"\\bottomrule",
                @"~?\\ref",
                @"\\setlength.*", // anything starts with \setlenght
                @"\\si",

            };

            var replacements = new Dictionary<string, string>
            {
                // General text replacements
                { @"\\text.{2}{", "{" },
                { @"\\%", "%" },
                { @"\\textgreater{}", ">" },
                { @"\\textless{}", "<" },
                { @"{\[}", "[" },
                { @"{\]}", "]" },
                { @"\$?\^\\circ\$?", "°" },
                { @"\^o", "°" },
                { @"\?\?\s?C(elsius)?", "°C" },
                { @"\\?celsius", "°C" },
                { @"\?\?F", "°F" },
                { @"\\?fahrenheit", "°F" },
                { @"\$\\?times\$?", "×" },
                { @"\\[,;]" , " "},
                { @"\\cdot" , "⋅"},
                { @"\\ldots", "…" },
                { @"---", "—" },
                { @"--", "–" },
                
                // Greek letters and symbols
                { @"\\Delta" , "Δ"},
                { @"\\rho", "ρ" },
                { @"\\alpha", "α" },
                { @"\\beta", "β" },
                { @"\\gamma", "γ" },
                { @"\\mu", "μ" },
                { @"\\sigma", "σ" },
                { @"\\omega", "ω" },
                { @"\\Omega", "Ω" },
                { @"\\tau", "τ" },
                { @"\\phi", "φ" },
                { @"\\Phi", "Φ" },

                // Braces
                { @"(\\{)|({\\ )", "{" },
                { @"(\\})|(}\\)", "}" },

            };

            var replacedText = text;

            foreach (var item in removes)
            {
                replacedText = Regex.Replace(replacedText, item, string.Empty, RegexOptions.IgnoreCase);
            }

            foreach (var item in replacements)
            {
                replacedText = Regex.Replace(replacedText, item.Key, item.Value, RegexOptions.IgnoreCase);
            }

            // Iterative replacements for nested or complex patterns
            string previous;
            do
            {
                previous = replacedText;
                // Superscripts: m^{3} -> m³
                replacedText = Regex.Replace(replacedText, @"\^\{(-?\d+)\}", m => ToSuperscript(m.Groups[1].Value));
                // Subscripts: H_{2}O -> H₂O
                replacedText = Regex.Replace(replacedText, @"_\{(-?\d+)\}", m => ToSubscript(m.Groups[1].Value));
                // Simple fractions: \frac{a}{b} -> a/b
                replacedText = Regex.Replace(replacedText, @"\\frac\{(.*?)\}\{(.*?)\}", "$1/$2");
                // Remove remaining braces around single characters/words
                replacedText = Regex.Replace(replacedText, @"\{([a-zA-Z0-9\.]+)\}", "$1");
            } while (previous != replacedText);

            // Final cleanup
            replacedText = replacedText.Replace("$", "") // remove math mode delimiters
                                     .Replace("~", "") // replace tilde with space
                                     .Trim();

            // Remove some known larger blocks that are hard with single-line regex
            replacedText = Regex.Replace(replacedText, @"\\def\\labelenumi.*?\}", "", RegexOptions.Singleline);

            return replacedText.Trim();
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
                    if (!string.IsNullOrEmpty(cleaned))
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
                    if (!string.IsNullOrEmpty(cleaned))
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


            //Console.WriteLine("        -{0}", paragraphName);

            curIndex++;
            while (curIndex < TotalCount)
            {
                currenLine = AllLines[curIndex];

                //break when hits next subsubsection or subsection
                if (currenLine.StartsWith("\\subsubsection") ||
                    currenLine.StartsWith("\\subsection") ||
                    currenLine.StartsWith("\\paragraph"))
                {
                    //Console.WriteLine("");
                    curIndex--;
                    break;
                }

                //find the content of paragraph 
                var cleaned = CleanString(currenLine);
                if (!string.IsNullOrEmpty(cleaned))
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

        private static string ToSuperscript(string input)
        {
            var superscriptMap = new Dictionary<char, char>
            {
                { '0', '⁰' }, { '1', '¹' }, { '2', '²' }, { '3', '³' }, { '4', '⁴' },
                { '5', '⁵' }, { '6', '⁶' }, { '7', '⁷' }, { '8', '⁸' }, { '9', '⁹' },
                { '-', '⁻' }
            };
            return new string(input.Select(c => superscriptMap.ContainsKey(c) ? superscriptMap[c] : c).ToArray());
        }

        private static string ToSubscript(string input)
        {
            var subscriptMap = new Dictionary<char, char>
            {
                { '0', '₀' }, { '1', '₁' }, { '2', '₂' }, { '3', '₃' }, { '4', '₄' },
                { '5', '₅' }, { '6', '₆' }, { '7', '₇' }, { '8', '₈' }, { '9', '₉' },
                { '-', '₋' }
            };
            return new string(input.Select(c => subscriptMap.ContainsKey(c) ? subscriptMap[c] : c).ToArray());
        }
    }
}
