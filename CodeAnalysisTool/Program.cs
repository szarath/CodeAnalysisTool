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
        public List<string> CheckIndentation(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<string> indentationIssues = new List<string>();

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                string indentationPattern = @"^\s*";
                Match match = Regex.Match(line, indentationPattern);

                if (match.Success && match.Length % 4 != 0)
                {
                    indentationIssues.Add($"Line {lineNumber + 1}: Indentation issue - {line}");
                }
            }

            return indentationIssues;
        }


        public List<string> CheckVariableNaming(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<string> variableNamingIssues = new List<string>();

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                // Define your variable naming pattern (e.g., camelCase)
                string variablePattern = @"\b[a-z][a-zA-Z0-9]*\b";
                MatchCollection matches = Regex.Matches(line, variablePattern);

                foreach (Match match in matches)
                {
                    // Check if the variable name doesn't match the expected pattern
                    if (!IsCamelCase(match.Value))
                    {
                        variableNamingIssues.Add($"Line {lineNumber + 1}: Variable naming issue - {match.Value}");
                    }
                }
            }

            return variableNamingIssues;
        }

        public List<string> CheckMethodNaming(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<string> methodNamingIssues = new List<string>();

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                // Define your method naming pattern (e.g., PascalCase)
                string methodPattern = @"(?<=\s)([A-Z][a-zA-Z0-9]*)\(";
                MatchCollection matches = Regex.Matches(line, methodPattern);

                foreach (Match match in matches)
                {
                    // Check if the method name doesn't match the expected pattern
                    if (!IsPascalCase(match.Groups[1].Value))
                    {
                        methodNamingIssues.Add($"Line {lineNumber + 1}: Method naming issue - {match.Groups[1].Value}");
                    }
                }
            }

            return methodNamingIssues;
        }


        // Add more methods for other code quality checks as needed
        public void AddIssuesToWordDocument(Body body, string filePath, List<string> issues)
        {
            AddToWordDocument(body, $"Issues in file: {filePath}");

            for (int i = 0; i < issues.Count; i++)
            {
                AddToWordDocument(body, $"Issue {i + 1} - {issues[i]}");
            }
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
