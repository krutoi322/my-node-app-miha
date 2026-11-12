using System.Text.Encodings.Web;
using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:3000");
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

builder.Services.AddResponseCompression();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.RejectionStatusCode = 429; 
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers["X-RateLimit-Limit"] = "100";
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("{\"error\":\"Слишком много запросов\",\"status\":429}", token);
    };
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync("{\"error\":\"Внутренняя ошибка сервера\",\"status\":500}");
    }
});
app.UseResponseCompression();
app.Use(async (context, next) =>
{
    var startTime = DateTime.Now;
    var stopwatch = Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    Console.WriteLine($"[{startTime:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} - {stopwatch.ElapsedMilliseconds}ms");
});
app.UseRateLimiter();

app.MapGet("/", () =>
{
    var html = $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""utf-8"">
    <title>Лабораторная работа №16</title>
</head>
<body>
    <h1>Лабораторная работа №16</h1>
    <p>Группа: №478</p>
    <p>Дата и время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
    <p>Доброго времени суток</p>
    
    <h3>Список доступных маршрутов:</h3>
    <ul>
        <li>GET /</li>
        <li>GET /about</li>
        <li>GET /contacts</li>
        <li>GET /api/books</li>
        <li>GET /api/books/1</li>
        <li>GET /api/books/search?author=Толстой</li>
    </ul>
</body>
</html>";
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapGet("/about", () =>
{
    var html = "<h1>О разработчике</h1><p>Студент: Семенюк Михаил</p>";
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapGet("/contacts", () =>
{
    var html = "<h1>Контакты</h1><p>Email: mihazhelezo@gmail.com</p>";
    return Results.Content(html, "text/html; charset=utf-8");
});

var books = new List<Book>();
var nextBookId = 1; 

app.MapGet("/api/books", () =>
{
    return Results.Ok(books);
});

app.MapGet("/api/books/search", (string? author) =>
{
    if (string.IsNullOrWhiteSpace(author))
    {
        return Results.BadRequest(new { error = "Укажите автора", status = 400 });
    }

    var foundBooks = books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(foundBooks);
});

app.MapGet("/api/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book == null)
    {
        return Results.NotFound(new { error = "Не найдена книга", status = 404 });
    }
    return Results.Ok(book);
});

app.MapPost("/api/books", (BookRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author) || request.Year <= 0)
    {
        return Results.BadRequest(new { error = "Неправильно заполнены данные", status = 400 });
    }

    var newBook = new Book
    {
        Id = nextBookId,
        Title = request.Title,
        Author = request.Author,
        Year = request.Year.Value
    };

    nextBookId++; 
    books.Add(newBook);

    return Results.Created($"/api/books/{newBook.Id}", newBook);
});

app.MapPut("/api/books/{id:int}", (int id, BookRequest request) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book == null)
    {
        return Results.NotFound(new { error = "Не найдена книга", status = 404 });
    }

    if (!string.IsNullOrWhiteSpace(request.Title))
    {
        book.Title = request.Title;
    }
    if (!string.IsNullOrWhiteSpace(request.Author))
    {
        book.Author = request.Author;
    }
    if (request.Year.HasValue && request.Year > 0)
    {
        book.Year = request.Year.Value;
    }

    return Results.Ok(book);
});

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book == null)
    {
        return Results.NotFound(new { error = "Не найдена книга", status = 404 });
    }

    books.Remove(book);
    return Results.Ok(new { message = "Книга удалена", id = id });
});

app.MapGet("/error", () =>
{
    throw new Exception("Тестовая ошибка");
});

app.MapGet("/async-error", async () =>
{
    await Task.Delay(100); 
    throw new Exception("Тестовая асинхронная ошибка");
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Сервер запущен: http://localhost:3000");
});

app.Run();
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int Year { get; set; }
}
public class BookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int? Year { get; set; }
}