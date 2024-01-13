using System;
using System.IO;
using System.Text;
using ClientTeamAssignment.Models;
using ClientTeamAssignment.Lz4;
using System.Text.RegularExpressions;

namespace ClientTeamAssignment.ViewModels
{
    static class Constants
    {
        public const string FILE_NAME = @"search.json.mozlz4";
        public const string PARTIAL_PATH = @"Mozilla\Firefox\Profiles";
        public const string DIRECTORY_PARTIAL_NAME = ".default-release";
    }
    internal class ReadableFormatViewModel
    {
        private ReadableFormatModel readableFormatModel;

        private string filePath = "";

        public ReadableFormatViewModel()
        {
            readableFormatModel = new ReadableFormatModel();

            GetFilePath();

            DecodeFile();

            GetDefaultSearchName();

            CreateTreeView(FileText);
        }

        public string DefaultSearchEngine
        {
            get => readableFormatModel.DefaultSearchEngine;
            set
            {
                readableFormatModel.DefaultSearchEngine = value;
            }
        }       
        
        public string FileText
        {
            get => readableFormatModel.FileText;
            set
            {
                readableFormatModel.FileText = value;
            }
        }        
        
        public string TreeViewText
        {
            get => readableFormatModel.FileText;
            set
            {
                readableFormatModel.FileText = value;
            }
        }

        private void GetFilePath()
        {
            try {
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.PARTIAL_PATH);
                string[] directories = Directory.GetDirectories(path);
                string subdir = directories[0];

                foreach (string directory in directories)
                {
                    if (directory.EndsWith(Constants.DIRECTORY_PARTIAL_NAME))
                    {
                        subdir = directory;
                    }
                }

                filePath = System.IO.Path.Combine(subdir, Constants.FILE_NAME);
            }

            catch(Exception ex)
            {
                Console.WriteLine($"Error - Getting File Path: {ex.Message}");
            }

        }

        private void DecodeFile()
        {
            try
            {
                byte[] decodedBytes = Lz4FileReader.ReadLz4File(filePath);
                string decodedText = System.Text.Encoding.UTF8.GetString(decodedBytes);
                FileText = decodedText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - Docoding file: {ex.Message}");
            }
        }

        private void CreateTreeView(string input)
        {
            bool withinQuotes = false;
            int bracketCount = 0;
            StringBuilder result = new StringBuilder();
            foreach (char c in input)
            {
                if (c == '"')
                {
                    withinQuotes = !withinQuotes;
                }
                if (!withinQuotes)
                {
                    if (c == ':')
                    {
                        result.Append(" : ");
                    }
                    else if (c == ',' || c == '{' || c == '}' || c == '[' || c == ']')
                    {
                        if (c == '{' || c == '[')
                        {
                            bracketCount++;
                        }
                        else if (c == '}' || c == ']')
                        {
                            bracketCount = Math.Max(bracketCount - 1, 0);
                        }
                        if(c != ',')
                        {
                            result.Append(c);
                        }
                        result.AppendLine();
                        result.Append(new string(' ', bracketCount * 2));
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
            result.Replace("\"", "");
            TreeViewText = result.ToString();
        }

        private void GetDefaultSearchName()
        {
            try
            {
                string appDefaultPattern = @"""appDefaultEngineId"":""([^""]+)""";
                string defaultPattern = @"""defaultEngineId"":""([^""]+)""";
                string engineId = FindStringAfterPattern(FileText, defaultPattern);

                if (engineId == null)
                {
                    engineId = FindStringAfterPattern(FileText, appDefaultPattern);
                }

                string engineNamePattern = $@"[^{{}}]*""id""[^{{}}]*:""{Regex.Escape(engineId)}""[^{{}}]*""_name""[^{{}}]*:""([^""]+)""[^{{}}]*";

                DefaultSearchEngine = FindStringAfterPattern(FileText, engineNamePattern);

            }

            catch {
                DefaultSearchEngine = "Could not find";
            }
  
        }

        private string FindStringAfterPattern(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }

}
