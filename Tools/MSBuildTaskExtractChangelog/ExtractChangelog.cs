using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildReadTask
{
    public class ExtractChangelog : Task
    {
        [Required]
        public string FilePath { get; set; }

        [Output]
        public string ExtractedText { get; private set; }

        public override bool Execute()
        {
            try
            {
                var textToParse = File.ReadAllText(FilePath);
                var regex = new Regex("(?sm)^(## \\d{1,2}\\.\\d{1,2}\\.\\d{1,2}(?:-beta\\.\\d{1,2})? \\(\\d{4}-\\d{2}-\\d{2}\\))(.+?)(?=\n## \\d{1,2}\\.\\d{1,2}\\.\\d{1,2}(?:-beta\\.\\d{1,2})? \\(\\d{4}-\\d{2}-\\d{2}\\))");
                var matches = regex.Matches(textToParse);
                ExtractedText = matches[0].Groups[0].Value;
                File.WriteAllText("./ExtractedChangelog.md", ExtractedText);
            }
            catch (Exception ex)
            {
                Log.LogError($"The changelog could not be extracted because: {ex.Message}\nAborting the build.");
                return false;
            }

            return true;
        }
    }
}
