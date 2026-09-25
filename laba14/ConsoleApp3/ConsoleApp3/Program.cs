using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        string filePath = @"C:\Users\mihaz\my-node-app\laba14\project_17";
        string filePath1 = @"C:\Users\mihaz\my-node-app\laba14\report_17.json";
        string root = args.Length > 0 ? args[0] : filePath;
        int filesCount = 0;
        int foldersCount = 0;
        var files = new List<FileInfo>();

        Scan(root, ref filesCount, ref foldersCount, files);

        Console.WriteLine($"Анализ директории: {root}");
        Console.WriteLine($"Папок: {foldersCount}");
        Console.WriteLine($"Файлов: {filesCount}");
        long totalSize = 0;
        foreach (var f in files)
            totalSize += f.Length;
        Console.WriteLine($"Общий размер: {FormatSize(totalSize)} ({totalSize} байт)");
        Console.WriteLine("Расширения файлов:");

        var extCount = new Dictionary<string, int>();
        var extSize = new Dictionary<string, long>();
        foreach (var f in files)
        {
            string ext = f.Extension;
            if (ext == "") ext = "(без расширения)";

            if (!extCount.ContainsKey(ext))
            {
                extCount[ext] = 0;
                extSize[ext] = 0;
            }
            extCount[ext]++;
            extSize[ext] += f.Length;
        }
        foreach (var pair in extCount)
        {
            Console.WriteLine($"{pair.Key}: {pair.Value} файлов ({FormatSize(extSize[pair.Key])})");
        }

        var sorted = new List<FileInfo>(files);
        sorted.Sort((a, b) => b.Length.CompareTo(a.Length));   

        Console.WriteLine("Топ 5  самых больших файлов:");
        for (int i = 0; i < sorted.Count && i < 5; i++)
        {
            var f = sorted[i];
            Console.WriteLine($"{i + 1}. {f.Name} ({FormatSize(f.Length)}) - {f.FullName}");
        }
        Console.WriteLine("Топ 5 самых маленьких файлов:");
        for (int i = 0; i < sorted.Count && i < 5; i++)
        {
            var f = sorted[sorted.Count - 1 - i];
            Console.WriteLine($"{i + 1}. {f.Name} ({FormatSize(f.Length)}) - {f.FullName}");
        }
        var variantFiles = new List<FileInfo>();
        foreach (var f in files)
        {
            if (f.Name.Contains("17"))
                variantFiles.Add(f);
        }

        Console.WriteLine("Файлы с номером варианта (17) в названии:");
        if (variantFiles.Count == 0)
            Console.WriteLine("  (не найдено)");
        else
            foreach (var f in variantFiles)
                Console.WriteLine($" {f.FullName}");

        var extList = new List<object>();
        foreach (var pair in extCount)
        {
            extList.Add(new
            {
                extension = pair.Key,
                count = pair.Value,
                size = extSize[pair.Key],
                sizeFormatted = FormatSize(extSize[pair.Key])
            });
        }

        var topBigList = new List<object>();
        for (int i = 0; i < sorted.Count && i < 5; i++)
        {
            var f = sorted[i];
            topBigList.Add(new
            {
                name = f.Name,
                path = f.FullName,
                size = f.Length,
                sizeFormatted = FormatSize(f.Length)
            });
        }

        var topSmallList = new List<object>();
        for (int i = 0; i < sorted.Count && i < 5; i++)
        {
            var f = sorted[sorted.Count - 1 - i];
            topSmallList.Add(new
            {
                name = f.Name,
                path = f.FullName,
                size = f.Length,
                sizeFormatted = FormatSize(f.Length)
            });
        }

        var variantList = new List<string>();
        foreach (var f in variantFiles)
            variantList.Add(f.FullName);

        var report = new
        {
            directory = root,
            folders = foldersCount,
            files = filesCount,
            totalSize = totalSize,
            totalSizeFormatted = FormatSize(totalSize),
            extensions = extList,
            top5Largest = topBigList,
            top5Smallest = topSmallList,
            variantFiles = variantList
        };
        var options = new JsonSerializerOptions {WriteIndented = true};
        string json = JsonSerializer.Serialize(report, options);
        File.WriteAllText(filePath1, json);

        Console.WriteLine($"Отчёт сохранён: {filePath1}");
    }
    static void Scan(string path, ref int filesCount, ref int foldersCount, List<FileInfo> files)
    {
        foldersCount++;
        foreach (string file in Directory.GetFiles(path))
        {
            files.Add(new FileInfo(file));
            filesCount++;
        }
        foreach (string dir in Directory.GetDirectories(path))
            Scan(dir, ref filesCount, ref foldersCount, files);
    }

    static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} байт";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} КБ";
        return $"{bytes / (1024.0 * 1024.0):F2} МБ";
    }
}