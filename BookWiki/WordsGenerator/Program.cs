using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace WordsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = @"D:\words.txt";

            var text = File.ReadAllText(filePath);
            var lower = text.ToLower();

            var words = lower.Split(' ', '\n', '\r', '.', ',', '!', '?', '-', ':', ';', '\'', '"');

            var differentWords = words.Distinct();

            var code = new List<string>();

            code.Add("private string[] _words = new []");
            code.Add("{");
            foreach (var word in differentWords.Where(x => string.IsNullOrWhiteSpace(x) == false))
            {
                code.Add($"\"{word}\",");
            }
            code.Add("};");

            File.WriteAllLines(@"D:\words2.txt", code);
        }
    }
}
