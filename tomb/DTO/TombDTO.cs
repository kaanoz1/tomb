using tomb.Model;

namespace tomb.DTO
{
    public class TombOwnDTO
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required string? Description { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
    }

    public class TombOwnerDTO : TombOwnDTO
    {
        public required UserDTO Owner { get; set; }
    }

    public static class TombExtension
    {
        public static TombOwnDTO ToTombOwnDTO(this Tomb tomb) => new()
        {
            Id = tomb.Id,
            Name = tomb.Name,
            Description = tomb.Description,
            CreatedAt = tomb.CreatedAt,
            Latitude = tomb.Latitude,
            Longitude = tomb.Longitude
        };
        public static TombOwnerDTO ToTombOwnerDTO(this Tomb tomb) => new() { CreatedAt = tomb.CreatedAt, Description = tomb.Description, Id = tomb.Id, Latitude = tomb.Latitude, Longitude = tomb.Longitude, Name = tomb.Name, Owner = tomb.User.ToUserDTO() }; 
    }



}
