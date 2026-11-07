using System.Text.Encodings.Web;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:3000");
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
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

app.Use(async (context, next) =>
{
    var start = DateTime.Now;
    var sw = Stopwatch.StartNew();

    await next();

    sw.Stop();
    Console.WriteLine($"[{start:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} - {sw.ElapsedMilliseconds}ms");
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/protected"))
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync("{\"error\":\"Требуется заголовок Authorization\",\"status\":401}");
            return;
        }
    }
    await next();
});

app.MapGet("/", () =>
{
    var html = $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""utf-8"">
</head>
<body>
    <h1>Лабораторная работа №15</h1>
    <p>Группа: №478</p>
    <p>Дата и время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
    <p>Доброго времени суток</p>
</body>
</html>";
    return Results.Content(html, "text/html; charset=utf-8");
});

var users = new List<User>();
var userId = 1;

app.MapGet("/api/users", () => Results.Ok(users));

app.MapPost("/api/users", (UserInput input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Group))
        return Results.BadRequest(new { error = "Неправильно заполнены данные", status = 400 });

    var user = new User { Id = userId++, Name = input.Name, Group = input.Group };
    users.Add(user);
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapPut("/api/users/{id:int}", (int id, UserInput input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Group))
        return Results.BadRequest(new { error = "Не заполнены поля", status = 400 });

    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { error = $"Пользователь с id={id} не найден", status = 404 });

    user.Name = input.Name;
    user.Group = input.Group;
    return Results.Ok(user);
});

app.MapDelete("/api/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { error = $"Пользователь с id={id} не найден", status = 404 });

    users.Remove(user);
    return Results.Ok(new { message = "Пользователь удалён", id });
});

app.MapGet("/protected", (HttpContext ctx) =>
    Results.Ok(new { message = "Доступ разрешён", auth = ctx.Request.Headers["Authorization"].ToString() }));

app.MapGet("/error", () =>
{
    throw new Exception("Тестовая ошибка");
});

app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("Сервер запущен: http://localhost:3000"));
app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
}
public record UserInput(string? Name, string? Group);