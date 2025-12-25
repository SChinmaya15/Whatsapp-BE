using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonRequired]
        public string Email { get; set; } = string.Empty;
        
        [BsonRequired]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string? TenantId { get; set; }
        
        public string Role { get; set; } = "User"; // User, Admin, etc.
        
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}

