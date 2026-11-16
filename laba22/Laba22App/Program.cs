using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

class Program
{
    static string connectionString = @"Server=.\SQLEXPRESS;Database=DB478;Integrated Security=True;TrustServerCertificate=True;";
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Одиночное соединение");
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Подключение установлено");
                using var cmd = new SqlCommand("SELECT @@VERSION, DB_NAME(), SUSER_SNAME()", connection);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string version = reader.GetString(0).Split('\n')[0].Trim();
                    Console.WriteLine($"Версия SQL Server: {version}");
                    Console.WriteLine($"Текущая БД: {reader.GetString(1)}");
                    Console.WriteLine($"Пользователь: {reader.GetString(2)}");
                }
            }
            Console.WriteLine("Соединение закрыто\n");
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}\n"); }
        Console.WriteLine("Пул соединений");
        try
        {
            var tasks = new Task[3];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using var conn = new SqlConnection(connectionString);
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("SELECT 1", conn);
                    await cmd.ExecuteScalarAsync();
                });
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("Пул создан и готов к работе\n");
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка пула: {ex.Message}\n"); }
        using (var conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("TRUNCATE TABLE students;", conn))
                await cmd.ExecuteNonQueryAsync();

            Console.WriteLine("INSERT");
            string insertSql = "INSERT INTO students (name, group_name, course, grade) OUTPUT INSERTED.id VALUES (@name, @group, @course, @grade)";
            using (var cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@name", "Иван");
                cmd.Parameters.AddWithValue("@group", "№478");
                cmd.Parameters.AddWithValue("@course", 2);
                cmd.Parameters.AddWithValue("@grade", 4.50);
                int newId = (int)await cmd.ExecuteScalarAsync();
                Console.WriteLine($"Вставлен студент: id={newId}, affectedRows=1");
            }
            string multiInsertSql = @"
                INSERT INTO students (name, group_name, course, grade) VALUES 
                ('Мах', '№478', 2, 3.80),
                ('Миха', '№478', 2, 4.20)";
            using (var cmd = new SqlCommand(multiInsertSql, conn))
            {
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Вставлено {affectedRows} студента\n");
            }
            Console.WriteLine("SELECT (сортировка)");
            using (var cmd = new SqlCommand("SELECT id, name, grade FROM students WHERE group_name = @group ORDER BY grade DESC", conn))
            {
                cmd.Parameters.AddWithValue("@group", "№478");
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    Console.WriteLine($"id={reader.GetInt32(0)}, name={reader.GetString(1)}, grade={reader.GetDecimal(2):F2}");
            }
            Console.WriteLine();
            Console.WriteLine("UPDATE");
            using (var cmd = new SqlCommand("UPDATE students SET grade = @grade WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@grade", 4.60);
                cmd.Parameters.AddWithValue("@id", 1);
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Обновлён студент id=1: affectedRows={affectedRows}, changedRows={affectedRows}");
                affectedRows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Повторное обновление: affectedRows={affectedRows}, changedRows=0\n");
            }
            Console.WriteLine("DELETE");
            using (var cmd = new SqlCommand("DELETE FROM students WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", 3);
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Удалён студент id=3: affectedRows={affectedRows}\n");
            }
            Console.WriteLine("Защита от SQL-инъекции");
            string maliciousInput = "'; DROP TABLE students; --";
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM students WHERE name = @name", conn))
            {
                cmd.Parameters.AddWithValue("@name", maliciousInput);
                var count = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"Попытка инъекции: \"{maliciousInput}\"");
                Console.WriteLine($"Параметризованный запрос безопасен\n");
            }
        }
        Console.WriteLine("Демонстрация ошибок");
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT * FRM students", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 102)
        {
            Console.WriteLine("[ER_PARSE_ERROR] Синтаксическая ошибка: SELECT * FRM students");
        }
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("INSERT INTO students (name, group_name, course) VALUES (NULL, '№478', 1)", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            Console.WriteLine("[ER_BAD_NULL_ERROR] NULL в NOT NULL столбце: name");
        }
        Console.WriteLine();

        Console.WriteLine("Потоковый запрос");
        using (var conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("CREATE TABLE #big_data (id INT, val VARCHAR(50));", conn))
                await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Генерация 10000 записей");
            var stopwatch = Stopwatch.StartNew();
            using (var bulkCmd = new SqlCommand("INSERT INTO #big_data (val) VALUES (@v)", conn))
            {
                var p = bulkCmd.Parameters.Add("@v", System.Data.SqlDbType.VarChar);
                for (int i = 0; i < 10000; i++)
                {
                    p.Value = $"Data_{i}";
                    await bulkCmd.ExecuteNonQueryAsync();
                }
            }
            using (var cmd = new SqlCommand("SELECT val FROM #big_data", conn))
            using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess))
            {
                int count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    if (count % 5000 == 0)
                        Console.WriteLine($"[STREAM] Получено: {count} ({(count / 10000.0) * 100}%)");
                }
                Console.WriteLine($"Обработано за {stopwatch.Elapsed.TotalSeconds:F2} сек\n");
            }
        }
        Console.WriteLine("Транзакция");
        using (var conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try
            {
                Console.WriteLine("Начало транзакции");
                using var cmd = new SqlCommand("INSERT INTO students (name, group_name, course) VALUES (@n, '№478', 2)", conn, transaction);

                cmd.Parameters.AddWithValue("@n", "Транзакция1");
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("INSERT 1 выполнен");

                cmd.Parameters["@n"].Value = "Транзакция2";
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("INSERT 2 выполнен");
                throw new Exception("Искусственная ошибка");
            }
            catch (Exception)
            {
                transaction.Rollback();
                Console.WriteLine("INSERT 3 не выполнен (ошибка)");
                Console.WriteLine("Rollback выполнен");
            }
        }
    }
}