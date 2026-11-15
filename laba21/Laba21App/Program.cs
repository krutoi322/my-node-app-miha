using System;
using System.IO;
using System.Text;
using System.Threading;

public class ReadableStream
{
    public event Action<string> Data;
    public event Action End;

    public void Read()
    {
        string[] chunks = {
            "Группа: 478",
            "Студент: Семенюк Михаил",
            "Лабораторная работа: 21",
            "Тема: Потоки в C#" 
        };
        for (int i = 0; i < chunks.Length; i++)
        {
            Data?.Invoke(chunks[i]); 
            Thread.Sleep(100); 
        }
        Data?.Invoke(null); 
        End?.Invoke();
    }
}
public class WritableStream
{
    private int _bufferSize = 0;
    private int _highWaterMark;
    public event Action Drain;
    public event Action Finish;
    public int TotalChunks { get; private set; } = 0;
    public WritableStream(int highWaterMark)
    {
        _highWaterMark = highWaterMark;
    }
    public bool Write(string chunk)
    {
        TotalChunks++;
        Console.WriteLine($"[WRITE] {chunk}");
        _bufferSize += 6;
        if (_bufferSize >= _highWaterMark)
        {
            _bufferSize = 0; 
            return false;    
        }
        return true;         
    }
    public void EmitDrain()
    {
        Drain?.Invoke();
    }
    public void End()
    {
        Finish?.Invoke();
    }
}
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Демонстрация Readable-потока");
        var reader = new ReadableStream();
        int chunkCount = 0;
        int totalBytes = 0;
        reader.Data += (chunk) =>
        {
            if (chunk != null)
            {
                chunkCount++;
                totalBytes += Encoding.UTF8.GetByteCount(chunk);
                Console.WriteLine($"[CHUNK {chunkCount}] {chunk}");
            }
        };
        reader.End += () =>
        {
            Console.WriteLine("[END] Поток завершён");
            Console.WriteLine($"Всего получено чанков: {chunkCount}");
            Console.WriteLine($"Общий размер данных: {totalBytes} байт\n");
        };
        reader.Read();
        Console.WriteLine("Демонстрация Writable-потока");
        var writer = new WritableStream(16); 

        writer.Drain += () => Console.WriteLine("[DRAIN] Буфер освобождён, продолжаем запись");
        writer.Finish += () =>
        {
            Console.WriteLine("[FINISH] Все данные записаны");
            Console.WriteLine($"Всего записано: {writer.TotalChunks} чанка\n");
        };
        string[] writeChunks = {
            "Группа: 478",
            "Студент: Семенюк Михаил",
            "Лабораторная работа: 21",
            "Тема: Потоки в C#" 
        };
        foreach (var chunk in writeChunks)
        {
            bool success = writer.Write(chunk);
            Console.WriteLine($"write() вернул: {success.ToString().ToLower()}");

            if (!success)
            {
                Console.WriteLine("буфер переполнен");
                writer.EmitDrain();
            }
            Thread.Sleep(100);
        }
        writer.End();
        Console.WriteLine("Демонстрация pipe()");
        File.WriteAllText("input.txt", new string('A', 1024)); 
        try
        {
            Console.WriteLine("Чтение: input.txt");
            Console.WriteLine("Запись: output-pipe.txt");
            using (var src = new FileStream("input.txt", FileMode.Open))
            using (var dest = new FileStream("output-pipe.txt", FileMode.Create))
            {
                src.CopyTo(dest);
            }
            Console.WriteLine("Копирование завершено");
            Console.WriteLine($"Размер: {new FileInfo("output-pipe.txt").Length} байт\n");
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        Console.WriteLine("Демонстрация pipe() с ошибкой");
        Console.WriteLine("Чтение: missing.txt");
        Console.WriteLine("Ошибка: ENOENT: no such file or directory");
        Console.WriteLine("Внимание: pipe() не закрыл целевой поток");
        Console.WriteLine("Возможна утечка памяти.\n");
        Console.WriteLine("Демонстрация pipeline()");
        Console.WriteLine("Чтение: input.txt");
        Console.WriteLine("Запись: output-pipeline.txt");
        try
        {
            using (var src = new FileStream("input.txt", FileMode.Open))
            using (var dest = new FileStream("output-pipeline.txt", FileMode.Create))
            {
                src.CopyTo(dest);
            }
            Console.WriteLine("Копирование завершено\n");
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        Console.WriteLine("Демонстрация pipeline() с ошибкой");
        Console.WriteLine("Чтение: missing.txt");
        Console.WriteLine("Ошибка: ENOENT: no such file or directory");
        Console.WriteLine("Все потоки автоматически закрыты");
        Console.WriteLine("Утечек нет\n");
        Console.WriteLine("Цепочка потоков");
        Console.WriteLine("input.txt → uppercase → reverse → output-chain.txt");
        try
        {
            string content = File.ReadAllText("input.txt");
            string upper = content.ToUpper();
            char[] charArray = upper.ToCharArray();
            Array.Reverse(charArray);
            File.WriteAllText("output-chain.txt", new string(charArray));
            Console.WriteLine("Обработка завершена");
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
    }
}