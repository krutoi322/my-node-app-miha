using System;
using System.Linq;

namespace laba17
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return 0;
            }
            string command = args[0];

            switch (command)
            {
                case "greet":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Ошибка: укажите имя для приветствия");
                        return 1;
                    }
                    Console.WriteLine($"Привет, {args[1]}! Добро пожаловать в CLI-приложение группы №478");
                    return 0;

                case "info":
                    Console.WriteLine("Группа: №478");
                    Console.WriteLine("Студент: Семенюк Михаил");
                    Console.WriteLine("Лабораторная работа №17");
                    Console.WriteLine($"Дата: {DateTime.Now:yyyy-MM-dd}");
                    return 0;

                case "--version":
                case "-V":
                    Console.WriteLine("1.0.0");
                    return 0;

                case "generate":
                    return HandleGenerate(args);

                case "init":
                    return HandleInit(args);

                case "--help":
                case "-h":
                    ShowHelp();
                    return 0;

                default:
                    Console.WriteLine($"Ошибка: неизвестная команда \"{command}\"");
                    Console.WriteLine("Для справки используйте: my-cli --help");
                    return 1;
            }
        }

        static int HandleGenerate(string[] args)
        {
            if (args.Contains("--help") || args.Contains("-h"))
            {
                Console.WriteLine("Использование: my-cli generate [опции]");
                Console.WriteLine("  --type <type>    Тип отчёта (html, pdf, json, csv)");
                Console.WriteLine("  --output <path>  Путь для сохранения");
                Console.WriteLine("  --dry-run        Показать что будет сделано");
                Console.WriteLine("  --verbose        Подробный вывод");
                return 0;
            }

            string type = "html";
            string output = "./output.html";
            bool verbose = false;
            bool dryRun = false;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--type" && i + 1 < args.Length) type = args[i + 1];
                else if (args[i] == "--output" && i + 1 < args.Length) output = args[i + 1];
                else if (args[i] == "--verbose") verbose = true;
                else if (args[i] == "--dry-run") dryRun = true;
            }

            if (type != "html" && type != "pdf" && type != "json" && type != "csv")
            {
                Console.WriteLine($"Ошибка: недопустимый тип отчёта \"{type}\".");
                Console.WriteLine("Допустимые значения: html, pdf, json, csv");
                return 1;
            }

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] Будет сгенерирован отчёт типа: {type}");
                Console.WriteLine($"[DRY-RUN] Файл будет сохранён в: {output}");
                Console.WriteLine("[DRY-RUN] Действия не выполнены (режим проверки)");
                return 0;
            }

            if (verbose)
            {
                Console.WriteLine("[VERBOSE] Запуск генерации отчёта...");
                Console.WriteLine($"[VERBOSE] Тип отчёта: {type}");
                Console.WriteLine($"[VERBOSE] Путь сохранения: {output}");
            }
            Console.WriteLine($"Отчёт успешно сгенерирован: {output}");
            return 0;
        }

        static int HandleInit(string[] args)
        {
            bool noInteractive = args.Contains("--no-interactive");
            string name = "", type = "";
            bool git = false;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--name" && i + 1 < args.Length) name = args[i + 1];
                else if (args[i] == "--type" && i + 1 < args.Length) type = args[i + 1];
                else if (args[i] == "--git") git = true;
            }

            if (noInteractive)
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
                {
                    if (Console.IsInputRedirected && string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Ошибка: недостаточно данных. Укажите --type");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: в неинтерактивном режиме необходимо указать --name и --type");
                    }
                    return 1;
                }
            }
            else
            {
                if (Console.IsInputRedirected)
                {
                    name = Console.ReadLine() ?? "";
                    Console.WriteLine("Ошибка: недостаточно данных. Укажите --type");
                    return 1;
                }

                if (string.IsNullOrEmpty(name))
                {
                    Console.Write("Введите название проекта: ");
                    name = Console.ReadLine() ?? "";
                }

                if (string.IsNullOrEmpty(type))
                {
                    Console.WriteLine("Выберите тип проекта:");
                    Console.WriteLine("  1) Web-приложение");
                    Console.WriteLine("  2) CLI-утилита");
                    Console.WriteLine("  3) Библиотека");
                    Console.WriteLine("  4) Микросервис");
                    Console.Write("Ваш выбор (1-4): ");

                    string choice = Console.ReadLine() ?? "";
                    type = choice switch
                    {
                        "1" => "Web-приложение",
                        "2" => "CLI-утилита",
                        "3" => "Библиотека",
                        "4" => "Микросервис",
                        _ => "CLI-утилита"
                    };
                }

                if (!git)
                {
                    Console.Write("Использовать Git? (y/n): ");
                    if (Console.ReadLine()?.ToLower() == "y") git = true;
                }

                Console.Write("Введите токен доступа: ");
                string password = "";
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();
            }

            Console.WriteLine($"✓ Проект \"{name}\" успешно инициализирован!");
            Console.WriteLine($"Тип: {type}");
            Console.WriteLine($"Git: {(git ? "да" : "нет")}");
            return 0;
        }

        static void ShowHelp()
        {
            Console.WriteLine("Использование: my-cli <команда> [аргументы]");
            Console.WriteLine();
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("  greet <имя>   - поприветствовать пользователя");
            Console.WriteLine("  info          - вывести информацию о группе");
            Console.WriteLine("  generate      - сгенерировать отчёт");
            Console.WriteLine("  init          - инициализировать проект");
        }
    }
}