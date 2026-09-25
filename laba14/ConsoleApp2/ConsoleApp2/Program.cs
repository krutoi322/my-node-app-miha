using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = @"C:\Users\mihaz\my-node-app\laba14\project_17";

        if (Directory.Exists(filePath))
            Directory.Delete(filePath, recursive: true);

        string[] folders = {
            "src/modules",
            "src/components",
            "src/utils",
            "data/input",
            "data/output",
            "temp"
        };
        foreach (string f in folders)
            Directory.CreateDirectory(Path.Combine(filePath, f));

        WriteInfo(Path.Combine(filePath, "src", "modules"), "Модули приложения");
        WriteInfo(Path.Combine(filePath, "src", "components"), "Компоненты интерфейса");
        WriteInfo(Path.Combine(filePath, "src", "utils"), "Утилиты");
        WriteInfo(Path.Combine(filePath, "data", "input"), "Входные данные");
        WriteInfo(Path.Combine(filePath, "data", "output"), "Выходные данные");
        WriteInfo(Path.Combine(filePath, "temp"), "Временные данные");

        for (int i = 1; i <= 3; i++)
        {
            Directory.CreateDirectory(Path.Combine(filePath, "src", "components", i.ToString()));
        }
        Console.WriteLine("Дерево:");
        PrintTree(filePath, "");

        Directory.Move(Path.Combine(filePath, "temp"), Path.Combine(filePath, "data", "temp"));
        Directory.Move(Path.Combine(filePath, "data", "output"), Path.Combine(filePath, "data", "results"));
        Directory.Delete(Path.Combine(filePath, "data", "temp"), recursive: true);

        Console.WriteLine();
        Console.WriteLine("Обновлённое дерево:");
        PrintTree(filePath, "");

    }
    static void WriteInfo(string folder, string description)
    {
        File.WriteAllText(Path.Combine(folder, "info.txt"), description);
    }

    static void PrintTree(string path, string indent)
    {
        Console.WriteLine(indent + Path.GetFileName(path) + "/");
        foreach (string dir in Directory.GetDirectories(path))
            PrintTree(dir, indent + "    ");

        foreach (string file in Directory.GetFiles(path))
            Console.WriteLine(indent + "    " + Path.GetFileName(file));
    }
}