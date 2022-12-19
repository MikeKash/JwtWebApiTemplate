using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JwtWebApiTemplate.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [JsonIgnore]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonIgnore]
        public DateTime TokenCreated { get; set; }
        [JsonIgnore]
        public DateTime TokenExpires { get; set; }
    }
}
