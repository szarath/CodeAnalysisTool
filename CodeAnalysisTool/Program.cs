using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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
            string outputFileName = "CodeAnalysisResults.docx";
            string outputFilePath = Path.Combine(folderPath, outputFileName);

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder not found.");
                return;
            }

            CodeAnalyzer analyzer = new CodeAnalyzer();
            List<string> csFiles = GetCsFiles(folderPath);
            bool hasIssues = false;

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(outputFilePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                foreach (string filePath in csFiles)
                {
                    Console.WriteLine($"Analyzing file: {filePath}");
                    bool fileHasIssues = false;

                    fileHasIssues |= analyzer.CheckIndentation(filePath);
                    fileHasIssues |= analyzer.CheckVariableNaming(filePath);
                    fileHasIssues |= analyzer.CheckMethodNaming(filePath);

                    if (fileHasIssues)
                    {
                        Console.WriteLine($"File {filePath} has code quality issues.");
                        hasIssues = true;
                        analyzer.AddIssuesToWordDocument(body, filePath);
                    }
                }

                if (!hasIssues)
                {
                    Console.WriteLine("No code quality issues found in any files.");
                    analyzer.AddToWordDocument(body, "No code quality issues found in any files.");
                }
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

                // Check for indentation of 4 spaces per level
                if (match.Success && match.Length % 4 != 0)
                {
                    Console.WriteLine($"Indentation issue in file {filePath}, line {lineNumber + 1}: {line}");
                    hasIndentationIssues = true;
                }
            }

            return hasIndentationIssues;
        }

        public bool CheckVariableNaming(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            bool hasVariableNamingIssues = false;

            foreach (string line in lines)
            {
                // Define your variable naming pattern (e.g., camelCase)
                string variablePattern = @"\b[a-z][a-zA-Z0-9]*\b";
                MatchCollection matches = Regex.Matches(line, variablePattern);

                foreach (Match match in matches)
                {
                    // Check if the variable name doesn't match the expected pattern
                    if (!IsCamelCase(match.Value))
                    {
                        Console.WriteLine($"Variable naming issue in file {filePath}: {match.Value}");
                        hasVariableNamingIssues = true;
                    }
                }
            }

            return hasVariableNamingIssues;
        }

        public bool CheckMethodNaming(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            bool hasMethodNamingIssues = false;

            foreach (string line in lines)
            {
                // Define your method naming pattern (e.g., PascalCase)
                string methodPattern = @"(?<=\s)([A-Z][a-zA-Z0-9]*)\(";
                MatchCollection matches = Regex.Matches(line, methodPattern);

                foreach (Match match in matches)
                {
                    // Check if the method name doesn't match the expected pattern
                    if (!IsPascalCase(match.Groups[1].Value))
                    {
                        Console.WriteLine($"Method naming issue in file {filePath}: {match.Groups[1].Value}");
                        hasMethodNamingIssues = true;
                    }
                }
            }

            return hasMethodNamingIssues;
        }

        // Add more methods for other code quality checks as needed
        public void AddIssuesToWordDocument(Body body, string filePath)
        {
            // Example: Adding issues to the Word document with line numbers
            AddToWordDocument(body, $"Issues in file: {filePath}");
            // You can add more issue details here, including line numbers.
            AddToWordDocument(body, "Issue 1 - Line 12: Indentation issue");
            AddToWordDocument(body, "Issue 2 - Line 25: Variable naming issue");
            AddToWordDocument(body, "Issue 3 - Line 40: Method naming issue");
        }

        public void AddToWordDocument(Body body, string text)
        {
            Paragraph paragraph = body.AppendChild(new Paragraph());
            Run run = paragraph.AppendChild(new Run());
            Text t = run.AppendChild(new Text(text));
        }
        // Helper methods for naming conventions
        private bool IsCamelCase(string input)
        {
            return Regex.IsMatch(input, "^[a-z][a-zA-Z0-9]*$");
        }

        private bool IsPascalCase(string input)
        {
            return Regex.IsMatch(input, "^[A-Z][a-zA-Z0-9]*$");
        }
    }

}
