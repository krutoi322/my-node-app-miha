using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string dataPath = @"C:\Users\mihaz\my-node-app\laba14\data_17.txt";
        string resultPath = @"C:\Users\mihaz\my-node-app\laba14\processed_17.txt";

        if (!File.Exists(dataPath))
        {
            GenerateFile(dataPath, 100000);
        }

        FileInfo fi = new FileInfo(dataPath);
        Console.WriteLine($"Обработка: {dataPath}");
        Console.WriteLine($"Размер файла: {FormatSize(fi.Length)}");

        var sw = Stopwatch.StartNew();
        long sum = 0;
        int count = 0;
        int min = 1001;  
        int max = 0;      
        var allNumbers = new List<int>();

        long totalLines = CountLines(dataPath);
        long nextProgress = totalLines / 10;
        long processed = 0;

        using (var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
        using (var reader = new StreamReader(fs, Encoding.UTF8))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                if (parts.Length < 2) continue;

                int number = int.Parse(parts[1].Trim());

                sum += number;
                count++;
                if (number < min) min = number;
                if (number > max) max = number;
                allNumbers.Add(number);

                processed++;

                if (nextProgress > 0 && processed % nextProgress == 0)
                {
                    int percent = (int)(processed * 100 / totalLines);
                    Console.WriteLine($"Прогресс: {percent}% ({processed:N0} строк обработано)");
                }
            }
        }

        allNumbers.Sort();
        double median;
        int n = allNumbers.Count;

        if (n == 0)
            median = 0;
        else if (n % 2 == 1)
            median = allNumbers[n / 2];
        else
            median = (allNumbers[n / 2 - 1] + allNumbers[n / 2]) / 2.0;

        double average = 0;
        if (count > 0)
            average = (double)sum / count;

        sw.Stop();

        using (var writer = new StreamWriter(resultPath, false, Encoding.UTF8))
        {
            writer.WriteLine($"Всего строк: {count:N0}");
            writer.WriteLine($"Сумма чисел: {sum:N0}");
            writer.WriteLine($"Среднее значение: {average:F2}");
            writer.WriteLine($"Максимальное число: {max}");
            writer.WriteLine($"Минимальное число: {min}");
            writer.WriteLine($"Медиана: {median:F2}");
        }
        Console.WriteLine("Обработка завершена");
        Console.WriteLine("Результаты:");
        Console.WriteLine($"Всего строк: {count:N0}");
        Console.WriteLine($"Сумма чисел: {sum:N0}");
        Console.WriteLine($"Среднее значение: {average:F2}");
        Console.WriteLine($"Максимальное число: {max}");
        Console.WriteLine($"Минимальное число: {min}");
        Console.WriteLine($"Медиана: {median:F2}");
        Console.WriteLine($"Результаты сохранены в: {resultPath}");
        Console.WriteLine($"Время выполнения: {sw.Elapsed.TotalSeconds:F2} сек");
    }
    static void GenerateFile(string path, int lines)
    {
        var rnd = new Random();

        using (var writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            for (int i = 1; i <= lines; i++)
            {
                int number = rnd.Next(1, 1001);
                writer.WriteLine($"{i}, {number}, Вариант 17");
            }
        }
    }
    static long CountLines(string path)
    {
        long count = 0;

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
        using (var reader = new StreamReader(fs, Encoding.UTF8))
        {
            while (reader.ReadLine() != null)
                count++;
        }

        return count;
    }
    static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} байт";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} КБ";
        return $"{bytes / (1024.0 * 1024.0):F2} МБ";
    }
}