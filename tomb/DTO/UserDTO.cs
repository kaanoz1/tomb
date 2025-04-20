using System.ComponentModel.DataAnnotations;
using tomb.Model;

namespace tomb.DTO
{

    public class UserQueryDTO
    {
        public required string Username { get; set; }

        public required string Name { get; set; }

        public required string? Surname { get; set; }

        public required byte[]? Image { get; set; } = null!;

        public List<string> Roles { get; set; } = [];
    }

    public class UserDTO
    {
        public required Guid Id { get; set; }

        public required string Username { get; set; }

        public required string Name { get; set; }

        public string? Surname { get; set; }

        public byte[]? Image { get; set; } = null!;

        public required DateTime CreatedAt { get; set; }
    }

    public class UserFetchDTO
    {
        public required Guid Id { get; set; }

        public required string Username { get; set; }

        public required string Name { get; set; }

        public string? Surname { get; set; }

        public byte[]? Image { get; set; } = null!;

        public required DateTime CreatedAt { get; set; }

        public required DateTime LastActive { get; set; }

        public required int TombCount { get; set; }
        public List<string> Roles { get; set; } = [];
    }

    public class UserOwnDTO : UserFetchDTO
    {
        public required string Email { get; set; }

        public required DateTime? EmailConfirmedFrom { get; set; }

    }

    public static class UserExtension
    {
        public static UserDTO ToUserDTO(this User user) => new() { CreatedAt = user.CreatedAt, Id = user.Id, Image = user.Image, Name = user.Name, Surname = user.Surname, Username = user.UserName };

        public static UserFetchDTO ToUserFetchDTO(this User user, List<string>? roles = null) => new() { CreatedAt = user.CreatedAt, Id = user.Id, Image = user.Image, Name = user.Name, Surname = user.Surname, Username = user.UserName, TombCount = user.Tombs.Count, LastActive = user.LastActive, Roles = roles ?? [] };
         
        public static UserOwnDTO ToUserOwnDTO(this User user, List<string>? roles = null) => new () { CreatedAt = user.CreatedAt, Id = user.Id, Image = user.Image, Name = user.Name, Surname = user.Surname, Username = user.UserName, TombCount = user.Tombs.Count, LastActive = user.LastActive, Email = user.Email, EmailConfirmedFrom = user.EmailVerified, Roles = roles ?? [] };

        public static UserQueryDTO ToUserQueryDTO(this User user, List<string>? roles = null) => new() { Image = user.Image, Name = user.Name, Surname = user.Surname, Username = user.UserName, Roles = roles ?? [] };

    }


}
