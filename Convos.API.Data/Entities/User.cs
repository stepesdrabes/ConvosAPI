using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Convos.API.Data.Entities;

public class User
{
    [Key] public string Id { get; set; }
    public string Username { get; set; }
    public string? ImageName { get; set; }
    public DateTime RegisteredAt { get; set; }
    [JsonIgnore] public string PasswordHash { get; set; }
}