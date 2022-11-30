using System.Web.Helpers;
using Convos.API.Data;
using Convos.API.Data.Entities;
using Convos.API.Data.Models.Auth;
using Convos.API.Data.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Convos.API.Core.Services;

public class UserService
{
    private readonly DatabaseContext _context;
    private readonly FileService _fileService;

    public UserService(DatabaseContext context, FileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<List<User>> GetAllUsers() =>
        await _context.Users.OrderByDescending(x => x.RegisteredAt).ToListAsync();

    public async Task<User?> GetUserById(string id) => await _context.Users.FindAsync(id);

    public async Task<User?> GetUserByUsername(string username) =>
        await _context.Users.FirstOrDefaultAsync(user => user.Username == username.ToLower());

    public async Task<User> CreateUser(RegisterModel model)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = model.Username.ToLower(),
            PasswordHash = Crypto.HashPassword(model.Password),
            RegisteredAt = DateTime.Now
        };

        if (model.ImageFile != null) user.ImageName = await _fileService.UploadFile(model.ImageFile);

        await _context.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateUser(User user, UpdateUserModel model)
    {
        user.Username = model.Username ?? user.Username;

        if (model.ImageFile != null)
        {
            if (user.ImageName != null) _fileService.DeleteFile(user.ImageName);
            user.ImageName = await _fileService.UploadFile(model.ImageFile);
        }

        _context.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }
}