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

using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace ConsoleApp1 
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Укажите команду: system, resources или network");
                return;
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

                case "system":
                    ShowSystemInfo();
                    break;
                case "resources":
                    ShowResources();
                    break;
                case "network":
                    ShowNetwork();
                    break;
                default:
                    Console.WriteLine("Неизвестная команда");
                    break;
            }
        }
        static void ShowSystemInfo()
        {
            Console.WriteLine("Информация о системе (группа №478)");

            string platform = "Неизвестная платформа";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platform = "win32";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platform = "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platform = "darwin";

            Console.WriteLine($"Платформа: {platform}");
            Console.WriteLine($"Тип ОС: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Архитектура: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Версия ОС: {Environment.OSVersion.Version}");
            Console.WriteLine($"Имя хоста: {Environment.MachineName}");

            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            Console.WriteLine($"Время работы: {uptime.Hours} ч {uptime.Minutes} мин {uptime.Seconds} сек");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Console.WriteLine("Вы работаете в Windows");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Console.WriteLine("Вы работаете в Linux");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Console.WriteLine("Вы работаете в macOS");
            else Console.WriteLine("Неизвестная платформа");
        }
        static void ShowResources()
        {
            Console.WriteLine("Информация о процессоре");
            Console.WriteLine($"Количество логических ядер: {Environment.ProcessorCount}");

            try
            {
                ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in cpuSearcher.Get())
                {
                    Console.WriteLine($"Модель процессора: {obj["Name"]}");
                    Console.WriteLine($"Средняя частота: {obj["MaxClockSpeed"]} МГц");
                    break;
                }
            }
            catch
            {
                Console.WriteLine("Модель процессора: Недоступно (требуется Windows)");
            }

            Console.WriteLine("\nИнформация о памяти");
            try
            {
                ManagementObjectSearcher memSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in memSearcher.Get())
                {
                    ulong totalMemory = (ulong)obj["TotalVisibleMemorySize"] * 1024;
                    ulong freeMemory = (ulong)obj["FreePhysicalMemory"] * 1024;
                    ulong usedMemory = totalMemory - freeMemory;

                    double totalGB = Math.Round((double)totalMemory / (1024 * 1024 * 1024), 2);
                    double freeGB = Math.Round((double)freeMemory / (1024 * 1024 * 1024), 2);
                    double usedGB = Math.Round((double)usedMemory / (1024 * 1024 * 1024), 2);
                    double usedPercent = Math.Round(((double)usedMemory / totalMemory) * 100, 1);

                    Console.WriteLine($"Общий объём: {totalGB} ГБ");
                    Console.WriteLine($"Свободно: {freeGB} ГБ");
                    Console.WriteLine($"Использовано: {usedGB} ГБ ({usedPercent}%)");
                    Console.WriteLine("Группа: №478");

                    double freePercent = ((double)freeMemory / totalMemory) * 100;
                    if (freePercent < 20)
                        Console.WriteLine("Мало свободной памяти");
                    else
                        Console.WriteLine("Память в норме");
                }
            }
            catch
            {
                Console.WriteLine("Информация о памяти недоступна.");
            }

            Console.WriteLine("\nСредняя загрузка системы");
            Console.WriteLine("Недоступна на Windows");
        }
        static void ShowNetwork()
        {
            Console.WriteLine("Сетевые интерфейсы");
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            int count = 0;
            string mainInterface = "Не найден";

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                string ipv4 = "Нет";
                string mac = "Нет";
                bool isInternal = true;

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipv4 = ip.Address.ToString();
                        if (!ipv4.StartsWith("192.168.") && !ipv4.StartsWith("10.") && !ipv4.StartsWith("172."))
                        {
                            isInternal = false;
                            if (mainInterface == "Не найден") mainInterface = $"{ni.Name} ({ipv4})";
                        }
                    }
                }

                string physicalAddress = ni.GetPhysicalAddress().ToString();
                if (physicalAddress.Length >= 8)
                {
                    mac = physicalAddress.Substring(0, 8) + "**:**:**";
                }

                Console.WriteLine($"\nИнтерфейс: {ni.Name}");
                Console.WriteLine($"IPv4: {ipv4}");
                Console.WriteLine($"MAC: {mac}");
                Console.WriteLine($"Внутренний: {(isInternal ? "да" : "нет")}");
                count++;
            }

            Console.WriteLine($"\nВсего интерфейсов: {count}");
            Console.WriteLine($"Основной интерфейс: {mainInterface}");

            Console.WriteLine("\nИнформация о пользователе");
            Console.WriteLine($"Имя пользователя: {Environment.UserName}");
            Console.WriteLine($"Домашняя директория: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
            Console.WriteLine("Группа: №478");
            Console.WriteLine($"Проверка root: {(Environment.UserName == "root" ? "да" : "нет")}");
 (system, resources, network)
        }
    }
}