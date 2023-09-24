using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CodeAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CodeAnalysisTool <FolderPath>");
                return;
            }

            string folderPath = args[0];

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder not found.");
                return;
            }

            CodeAnalyzer analyzer = new CodeAnalyzer();
            List<string> csFiles = GetCsFiles(folderPath);
            bool hasIssues = false;

            foreach (string filePath in csFiles)
            {
                Console.WriteLine($"Analyzing file: {filePath}");
                bool fileHasIssues = analyzer.CheckIndentation(filePath);
                if (fileHasIssues)
                {
                    Console.WriteLine($"File {filePath} has indentation issues.");
                    hasIssues = true;
                }
            }

            if (!hasIssues)
            {
                Console.WriteLine("No indentation issues found in any files.");
            }
        }

        static List<string> GetCsFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories).ToList();
        }
    }

    class CodeAnalyzer
    {
        public bool CheckIndentation(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            bool hasIndentationIssues = false;

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                string indentationPattern = @"^\s*";
                Match match = Regex.Match(line, indentationPattern);

                if (match.Success && match.Length % 4 != 0)
                {
                    Console.WriteLine($"Indentation issue in file {filePath}, line {lineNumber + 1}: {line}");
                    hasIndentationIssues = true;
                }
            }

            return hasIndentationIssues;
        }
    }
}
