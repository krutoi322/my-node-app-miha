using System;
using System.Collections.Generic;
using System.Linq;

public class EventEmitter
{
    protected class Listener
    {
        public Delegate Action { get; set; }
        public bool IsOnce { get; set; }
    }
    private Dictionary<string, List<Listener>> _events = new Dictionary<string, List<Listener>>();

    public void On(string eventName, Delegate listener)
    {
        if (!_events.ContainsKey(eventName))
            _events[eventName] = new List<Listener>();
        _events[eventName].Add(new Listener { Action = listener, IsOnce = false });
    }
    public void AddListener(string eventName, Delegate listener) => On(eventName, listener);

    public void Once(string eventName, Delegate listener)
    {
        if (!_events.ContainsKey(eventName))
            _events[eventName] = new List<Listener>();
        _events[eventName].Add(new Listener { Action = listener, IsOnce = true });
    }
    public bool Emit(string eventName, params object[] args)
    {
        if (!_events.ContainsKey(eventName) || _events[eventName].Count == 0)
        {
            if (eventName == "error")
            {
                throw new Exception(args.Length > 0 ? args[0].ToString() : "Неизвестная ошибка");
            }
            return false;
        }
        var listenersToInvoke = _events[eventName].ToList();
        foreach (var listener in listenersToInvoke)
        {
            listener.Action?.DynamicInvoke(args);
            if (listener.IsOnce)
            {
                _events[eventName].Remove(listener);
            }
        }
        return true;
    }
    public int ListenerCount(string eventName)
    {
        return _events.ContainsKey(eventName) ? _events[eventName].Count : 0;
    }
    public List<Delegate> Listeners(string eventName)
    {
        return _events.ContainsKey(eventName) ? _events[eventName].Select(l => l.Action).ToList() : new List<Delegate>();
    }
    public void RemoveListener(string eventName, Delegate listener)
    {
        if (_events.ContainsKey(eventName))
        {
            var toRemove = _events[eventName].FirstOrDefault(l => l.Action == listener);
            if (toRemove != null)
                _events[eventName].Remove(toRemove);
        }
    }
    public void RemoveAllListeners(string eventName)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName].Clear();
        }
    }
}
public class DatabaseConnection : EventEmitter
{
    public void Connect()
    {
        Console.WriteLine("[EVENT] Подключение к БД");
        Emit("connect");
    }
    public void Query(string sql, string result)
    {
        Console.WriteLine($"[EVENT] Выполнение запроса: {sql}");
        Emit("query", sql, result);
    }
    public void Close()
    {
        Console.WriteLine("[EVENT] Закрытие соединения");
        Emit("close");
    }
    public void Error(string message)
    {
        Emit("error", message);
    }
}
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Демонстрация EventEmitter");
        var emitter1 = new EventEmitter();
        emitter1.On("greet", new Action<string>(name => Console.WriteLine($"[greet] Привет, {name}")));
        emitter1.On("info", new Action(() => Console.WriteLine("[info] Группа: №478")));
        emitter1.On("bye", new Action(() => Console.WriteLine("[bye] До свидания")));
        emitter1.Emit("greet", "Иван");
        emitter1.Emit("info");
        emitter1.Emit("bye");
        Console.WriteLine($"Событие \"unknown\" без слушателей: {emitter1.Emit("unknown").ToString().ToLower()}");
        Console.WriteLine($"Событие \"greet\" со слушателями: {emitter1.Emit("greet", "Иван").ToString().ToLower()}");
        Console.WriteLine();
        Console.WriteLine("Сравнение on и once");

        var emitter2 = new EventEmitter();
        int onCount = 0;
        int onceCount = 0;
        emitter2.On("tick", new Action<int>(n => { onCount++; Console.WriteLine($"[tick #{n}] on-слушатель"); }));
        emitter2.Once("tick", new Action<int>(n => { onceCount++; Console.WriteLine($"[tick #{n}] once-слушатель"); }));
        emitter2.Emit("tick", 1);
        emitter2.Emit("tick", 2);
        emitter2.Emit("tick", 3);
        Console.WriteLine($"on-слушатель вызван: {onCount} раза");
        Console.WriteLine($"once-слушатель вызван: {onceCount} раз");
        Console.WriteLine();
        Console.WriteLine("Управление подписками");
        Action dummy1 = () => { };
        Action dummy2 = () => { };
        Action dummy3 = () => { };
        emitter2.On("test", dummy1);
        emitter2.On("test", dummy2);
        emitter2.On("test", dummy3);
        Console.WriteLine($"Слушателей до удаления: {emitter2.ListenerCount("test")}");
        emitter2.RemoveListener("test", dummy2);
        Console.WriteLine($"Слушателей после удаления одного: {emitter2.ListenerCount("test")}");
        emitter2.RemoveAllListeners("test");
        Console.WriteLine($"Слушателей после removeAllListeners: {emitter2.ListenerCount("test")}");
        Console.WriteLine();
        Console.WriteLine("Порядок вызова");

        var emitter3 = new EventEmitter();
        emitter3.On("order", new Action(() => Console.WriteLine("1. Первый слушатель (группа №478)")));
        emitter3.On("order", new Action(() => Console.WriteLine("2. Второй слушатель")));
        emitter3.On("order", new Action(() => Console.WriteLine("3. Третий слушатель")));
        emitter3.Emit("order");
        Console.WriteLine();
        Console.WriteLine("DatabaseConnection (группа №478)");

        var db = new DatabaseConnection();
        db.On("connect", new Action(() => Console.WriteLine("[EVENT] Соединение установлено")));
        db.On("query", new Action<string, string>((sql, res) => Console.WriteLine($"[EVENT] Результат: {res}")));
        db.On("close", new Action(() => Console.WriteLine("[EVENT] Соединение закрыто")));
        db.Connect();
        db.Query("SELECT * FROM students", "50 записей");
        db.Query("INSERT INTO students", "1 запись добавлена");
        db.Close();
        Console.WriteLine();
        Console.WriteLine("Демонстрация ошибки");
        db.On("error", new Action<string>(err => Console.WriteLine($"[EVENT] Ошибка: {err}")));
        db.Error("Connection timeout");
        Console.WriteLine("(без слушателя error — процесс упал бы с исключением)");
    }
}