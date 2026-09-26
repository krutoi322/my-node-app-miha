using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string filePath = @"C:\Users\mihaz\my-node-app\laba14\student_17.txt";
        int userLines = 0;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Создан файл {filePath}");
        userLines++;
        Console.WriteLine("Содержимое файла: ");

        Console.WriteLine("ФИО: ");
        string FIO = Console.ReadLine();
        sb.AppendLine($"Студент: {FIO}");
        userLines++;

        Console.WriteLine("Группа: ");
        string gr = Console.ReadLine();
        sb.AppendLine($"Группа: {gr}");
        userLines++;

        Console.WriteLine("Вариант: ");
        string varik = Console.ReadLine();
        sb.AppendLine($"Вариант: {varik}");
        userLines++;

        sb.AppendLine($"Дата: {DateTime.Now:yyyy-MM-dd HH:mm}");
        userLines++;

        Console.WriteLine("Любимые книги: ");    
        sb.AppendLine("Любимые книги: ");

        Console.Write("1. ");
        string book1 = Console.ReadLine();
        sb.AppendLine($"1. {book1}");
        userLines++;

        Console.Write("2. ");
        string book2 = Console.ReadLine();
        sb.AppendLine($"2. {book2}");
        userLines++;

        Console.Write("3. ");
        string book3 = Console.ReadLine();
        sb.AppendLine($"3. {book3}");
        userLines++;

        Console.Write("4. ");
        string book4 = Console.ReadLine();
        sb.AppendLine($"4. {book4}");
        userLines++;

        Console.Write("5. ");
        string book5 = Console.ReadLine();
        sb.AppendLine($"5. {book5}");
        userLines++;

        sb.AppendLine($"Количество записей: {userLines}");
        File.WriteAllText(filePath, sb.ToString());
        Console.WriteLine($"Создан файл {filePath}");
        

        Console.WriteLine();
        Console.WriteLine("Содержимое файла:");
        Console.WriteLine(File.ReadAllText(filePath));
    }
}