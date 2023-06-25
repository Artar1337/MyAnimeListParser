using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MyAnimeListParser
{
    class Program
    {
        private static IEnumerable<string> HtmlAgilityPackParse(string doc, int limit)
        {
            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(doc);

            List<string> tables = new List<string>();

            foreach (HtmlNode table in htmlSnippet.DocumentNode.SelectNodes("//table"))
            {
                HtmlAttribute att = table.Attributes["data-items"];
                tables.Add(att.Value);
            }
            if (tables.Count > 1)
                return null;

            string source = tables[0];
            string substring = "anime_title&quot;:";
            var indices = new List<int>();

            int index = source.IndexOf(substring, 0);
            while (index > -1)
            {
                indices.Add(index);
                index = source.IndexOf(substring, index + substring.Length);
            }

            string endSubstring = ",&quot;anime_title_eng";
            int parseLength = 256;
            var titles = new List<string>();
            foreach(var titleIndex in indices)
            {
                string tmp = source.Substring(titleIndex + substring.Length, parseLength);
                int endIndex = tmp.IndexOf(endSubstring, 0);
                titles.Add(tmp.Substring(0, endIndex).Replace("&quot;", "\""));
            }

            Random r = new Random();
            titles = titles.OrderBy(x => r.NextDouble()).ToList();
            if (limit > 0 && limit < titles.Count) 
            {
                titles = titles.Take(limit).ToList();
            }
            return titles;
        }

        private static void ParseFile()
        {
            Console.WriteLine("Введите имя файла (должен лежать в папке bin/debug)");
            string fileName = Directory.GetCurrentDirectory() + "/" + Console.ReadLine();
            Console.WriteLine("Введите размер выборки из тайтлов");
            int value = -1;
            try
            {
                value = Int32.Parse(Console.ReadLine());
                if (value < 1)
                    throw new Exception();
            }
            catch
            {
                Console.WriteLine("Ограничение введено неверно, так что просто будем возвращать все");
            }
            if (File.Exists(fileName))
            {
                var content = HtmlAgilityPackParse(File.ReadAllText(fileName), value);
                if (content != null)
                {
                    Console.WriteLine("Смотрите одно из следующих аниме:");
                    foreach (var element in content)
                    {
                        Console.WriteLine(element);
                    }
                }
                else
                {
                    Console.WriteLine("Что-то пошло не так!");
                }
            }
            else
            {
                Console.WriteLine($"Файла {fileName} не существует!");
            }
            Console.ReadKey();
        }

        private static string GetHtmlPageText(string path)
        {
            WebClient client = new WebClient();
            try
            {
                string s = client.DownloadString(path);
                return s;
            }
            catch
            {
                
            }
            return null;
        }

        private static void ParseLink()
        {
            Console.WriteLine("Введите никнейм на MyAnimeList");
            string nickname = Console.ReadLine();
            Console.WriteLine("Введите размер выборки из тайтлов");
            int value = -1;
            try
            {
                value = Int32.Parse(Console.ReadLine());
                if (value < 1)
                    throw new Exception();
            }
            catch
            {
                Console.WriteLine("Ограничение введено неверно, так что просто будем возвращать все");
            }

            string file = GetHtmlPageText($"https://myanimelist.net/animelist/{nickname}?status=6");
            if (file != null)
            {
                var content = HtmlAgilityPackParse(file, value);
                if (content != null)
                {
                    Console.WriteLine("Смотрите одно из следующих аниме:");
                    foreach (var element in content)
                    {
                        Console.WriteLine(element);
                    }
                }
                else
                {
                    Console.WriteLine("Что-то пошло не так!");
                }
            }
            else
            {
                Console.WriteLine($"Что-то пошло не так! Возможно, что юзера {nickname} не существует!");
            }
            Console.ReadKey();
        }

        static void Main()
        {
            ParseLink();
        }
    }
}
