using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeChatSignalRLab.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
        public string Fullname { get; set; }
    }
}
