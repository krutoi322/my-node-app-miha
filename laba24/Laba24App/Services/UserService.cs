using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laba24App.Models;
using MongoDB.Driver;

namespace Laba24App.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }
        public async Task<List<User>> GetAllAsync() => await _users.Find(_ => true).ToListAsync();

        public async Task<User> GetByIdAsync(string id) => await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetByEmailAsync(string email) => await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task CreateAsync(User user)
        {
            Console.WriteLine($"[SERVICE] Создание пользователя: {user.Email}");
            await _users.InsertOneAsync(user);
            Console.WriteLine($"[SERVICE] Email отправлен: {user.Email}");
        }
        public async Task UpdateAsync(string id, User userIn) => await _users.ReplaceOneAsync(u => u.Id == id, userIn);
        public async Task DeleteAsync(string id) => await _users.DeleteOneAsync(u => u.Id == id);
    }
}