using System.ComponentModel.DataAnnotations;

namespace GamingPlatform.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? name { get; set; }
        public string pseudo {  get; set; }
        public string? email { get; set; }

        [DataType(DataType.Date)]
        public DateTime visitDate { get; set; }
        public string? gender { get; set; }
    }
}
