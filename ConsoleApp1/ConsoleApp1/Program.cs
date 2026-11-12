using System;
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
        }
    }
}