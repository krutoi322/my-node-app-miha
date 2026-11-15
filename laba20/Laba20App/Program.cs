using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        HttpListener server = new HttpListener();
        server.Prefixes.Add("http://localhost:3000/");
        server.Start();
        Console.WriteLine("Сервер запущен на порту 3000");
        while (true)
        {
            var context = await server.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }
    static async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        string path = request.Url.AbsolutePath;
        string method = request.HttpMethod;
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int statusCode = 200;
        try
        {
            if (method == "GET" && path == "/")
            {
                response.ContentType = "text/html; charset=utf-8";
                string html = $"<h1>Лабораторная работа №20</h1><p>Группа: 478</p><p>Метод: {method}</p><p>URL: {path}</p>";
                await WriteResponse(response, html);
            }
            else if (method == "GET" && path == "/about")
            {
                response.ContentType = "text/html; charset=utf-8";
                string html = $"<h1>О студенте</h1><p>Группа: 478</p><p>Студент: Семенюк Михаил</p>";
                await WriteResponse(response, html);
            }
            else if (method == "GET" && path == "/headers")
            {
                response.ContentType = "application/json; charset=utf-8";
                var headersDict = new Dictionary<string, string>();
                foreach (string key in request.Headers.AllKeys)
                {
                    headersDict[key.ToLower()] = request.Headers[key];
                }
                var result = new
                {
                    headers = headersDict,
                    method = method,
                    url = path,
                    httpVersion = request.ProtocolVersion.ToString()
                };
                await WriteResponse(response, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else if (method == "GET" && path == "/headers/set")
            {
                response.Headers.Add("X-Powered-By", "C# HttpListener");
                response.Headers.Add("X-Group", "478");
                response.Headers.Add("Cache-Control", "no-cache");
                response.ContentType = "text/html; charset=utf-8";
                await WriteResponse(response, "<h1>Заголовки установлены</h1>");
            }
            else if (method == "GET" && path == "/headers/check")
            {
                response.ContentType = "application/json; charset=utf-8";
                response.Headers.Set("X-Group", "478");
                var result = new
                {
                    hasContentType = response.ContentType != null,
                    hasXGroup = response.Headers["X-Group"] != null,
                    hasUnknown = response.Headers["X-Unknown"] != null
                };
                string jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                await WriteResponse(response, jsonResponse);
            }
            else if (method == "POST" && (path == "/echo" || path == "/form"))
            {
                if (request.ContentLength64 > 1024 * 1024)
                {
                    response.StatusCode = 413;
                    response.ContentType = "text/plain; charset=utf-8";
                    await WriteResponse(response, "413 Payload Too Large");
                    statusCode = 413;
                    return;
                }
                string body = "";
                var encoding = request.ContentEncoding ?? Encoding.UTF8;
                using (var reader = new StreamReader(request.InputStream, encoding))
                {
                    body = await reader.ReadToEndAsync();
                }
                Console.WriteLine($"[LOG] Получено тело размером: {Encoding.UTF8.GetByteCount(body)} байт");
                string contentType = request.ContentType ?? "";
                if (path == "/echo")
                {
                    if (contentType.Contains("application/json"))
                    {
                        response.ContentType = "application/json; charset=utf-8";
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                        if (dict != null)
                        {
                            dict["group"] = "478";
                            await WriteResponse(response, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                    else if (contentType.Contains("text/plain"))
                    {
                        response.ContentType = "text/plain; charset=utf-8";
                        await WriteResponse(response, body);
                    }
                    else
                    {
                        response.ContentType = contentType;
                        await WriteResponse(response, body);
                    }
                }
                else if (path == "/form")
                {
                    response.ContentType = "application/json; charset=utf-8";
                    var formData = ParseFormData(body);
                    await WriteResponse(response, JsonSerializer.Serialize(formData, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            else
            {
                response.StatusCode = 404;
                response.ContentType = "text/html; charset=utf-8";
                await WriteResponse(response, $"<h1>404 Not Found</h1><p>Путь {path} не найден</p>");
                statusCode = 404;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[{time}] {method} {path} {statusCode}");
            response.OutputStream.Close();
        }
    }
    static async Task WriteResponse(HttpListenerResponse response, string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }
    static Dictionary<string, string> ParseFormData(string body)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(body)) return result;
        var pairs = body.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                result[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
            }
        }
        return result;
    }
}