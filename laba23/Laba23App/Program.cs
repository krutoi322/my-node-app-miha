using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

public class Student
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int Course { get; set; }
    public double Grade { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GroupInfo
{
    public ObjectId Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Curator { get; set; } = string.Empty;
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string connectionString = "mongodb://localhost:27017";
        string dbName = "db478";

        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);

        await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        Console.WriteLine("Подключение установлено");

        var buildInfo = await db.RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1));
        Console.WriteLine("Версия MongoDB: " + buildInfo["version"]);
        Console.WriteLine("База данных: " + dbName);

        var dbs = await client.ListDatabaseNamesAsync();
        Console.WriteLine("Список БД:");
        foreach (var name in await dbs.ToListAsync())
        {
            Console.WriteLine("- " + name);
        }
        var col = db.GetCollection<Student>("students");

        await col.DeleteManyAsync(_ => true);

        await col.Indexes.CreateOneAsync(new CreateIndexModel<Student>(
            Builders<Student>.IndexKeys.Ascending(s => s.GroupName)));

        Console.WriteLine("INSERT");
        var student1 = new Student { Name = "Некит", GroupName = "№478", Course = 2, Grade = 4.5 };
        await col.InsertOneAsync(student1);
        Console.WriteLine("Вставлен: insertedId=" + student1.Id);

        var studentsList = new List<Student>
        {
            new Student { Name = "Миха", GroupName = "№478", Course = 3, Grade = 8 },
            new Student { Name = "Мах", GroupName = "№478", Course = 4, Grade = 9 },
            new Student { Name = "Илюха", GroupName = "№478", Course = 1, Grade = 8 }
        };
        await col.InsertManyAsync(studentsList);
        Console.WriteLine("Вставлено " + studentsList.Count + " документа");

        Console.WriteLine("\nSELECT");
        var all = await col.Find(_ => true).ToListAsync();
        Console.WriteLine("Все документы (" + all.Count + "):");
        foreach (var s in all) Console.WriteLine("- " + s.Name);

        var filtered = await col.Find(s => s.GroupName == "№478").ToListAsync();
        Console.WriteLine("С фильтром по группе (" + filtered.Count + "):");
        foreach (var s in filtered) Console.WriteLine("- " + s.Name);

        var sorted = await col.Find(_ => true)
            .Sort(Builders<Student>.Sort.Descending(s => s.Grade))
            .Limit(2)
            .Skip(0)
            .ToListAsync();
        Console.WriteLine("Сортировка по grade (топ-2):");
        foreach (var s in sorted) Console.WriteLine("- " + s.Name + " (" + s.Grade + ")");

        var projected = await col.Find(_ => true)
            .Project<Student>(Builders<Student>.Projection.Include(s => s.Name).Include(s => s.Grade).Exclude(s => s.Id))
            .ToListAsync();
        Console.WriteLine("Проекция (только name, grade):");
        foreach (var s in projected) Console.WriteLine("- " + s.Name + " (" + s.Grade + ")");

        Console.WriteLine("\nUPDATE");
        var updateOne = await col.UpdateOneAsync(
            s => s.Name == "Некит",
            Builders<Student>.Update.Set(s => s.Grade, 4.8)
        );
        Console.WriteLine("updateOne: matchedCount=" + updateOne.MatchedCount + ", modifiedCount=" + updateOne.ModifiedCount);

        var updateMany = await col.UpdateManyAsync(
            s => s.GroupName == "№478",
            Builders<Student>.Update.Inc(s => s.Course, 1)
        );
        Console.WriteLine("updateMany: matchedCount=" + updateMany.MatchedCount + ", modifiedCount=" + updateMany.ModifiedCount);

        Console.WriteLine("\nDELETE");
        var deleteOne = await col.DeleteOneAsync(s => s.Name == "Ден");
        Console.WriteLine("deleteOne: deletedCount=" + deleteOne.DeletedCount);

        var deleteMany = await col.DeleteManyAsync(s => s.Grade < 4.0);
        Console.WriteLine("deleteMany: deletedCount=" + deleteMany.DeletedCount);

        Console.WriteLine("Курсоры");
        using (var cursor = await col.FindAsync(_ => true))
        {
            int count = 0;
            while (await cursor.MoveNextAsync())
            {
                foreach (var s in cursor.Current) count++;
            }
            Console.WriteLine("Итерация через курсор: получено " + count + " документов");
        }

        Console.WriteLine("\nАгрегации");
        var agg = await col.Aggregate()
            .Group(s => s.GroupName, g => new { Group = g.Key, Avg = g.Average(x => x.Grade) })
            .ToListAsync();
        foreach (var item in agg)
        {
            Console.WriteLine("Средний балл по группе " + item.Group + ": " + item.Avg);
        }
        var groupsCol = db.GetCollection<GroupInfo>("groups");
        await groupsCol.DeleteManyAsync(_ => true);
        await groupsCol.InsertOneAsync(new GroupInfo { GroupName = "№478", Curator = "Денищик Н.З." });

        var lookup = await col.Aggregate()
            .Lookup<Student, GroupInfo, BsonDocument>(
                groupsCol,
                s => s.GroupName,
                g => g.GroupName,
                a => a["joined_groups"])
            .ToListAsync();
        Console.WriteLine("$lookup (JOIN): " + lookup.FirstOrDefault());

        Console.WriteLine("\nИндексы");
        await col.Indexes.CreateOneAsync(new CreateIndexModel<Student>(
            Builders<Student>.IndexKeys.Ascending(s => s.GroupName).Descending(s => s.Grade)));
        Console.WriteLine("Создан составной индекс: group_name_1_grade_-1");

        var explainCommand = new BsonDocument
        {
            { "explain", new BsonDocument
                {
                    { "find", "students" },
                    { "filter", new BsonDocument("GroupName", "№478") }
                }
            },
            { "verbosity", "executionStats" }
        };
        var explainResult = await db.RunCommandAsync<BsonDocument>(explainCommand);
        var execTime = explainResult["executionStats"]["executionTimeMillis"];
        Console.WriteLine("explain() с индексом: executionTimeMillis=" + execTime);

        Console.WriteLine("\nТекстовый поиск");
        await col.Indexes.CreateOneAsync(new CreateIndexModel<Student>(
            Builders<Student>.IndexKeys.Text(s => s.Name)));

        var textSearch = await col.Find(Builders<Student>.Filter.Text("Некит")).ToListAsync();
        Console.WriteLine("Текстовый поиск по 'Некит': найдено " + textSearch.Count + " документов");

        Console.WriteLine("\nОшибка подключения");
        try
        {
            var badClient = new MongoClient("mongodb://localhost:9999");
            await badClient.GetDatabase(dbName).RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message.Split('.')[0]);
        }
    }
}