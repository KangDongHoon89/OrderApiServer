using System.ComponentModel.DataAnnotations;

namespace OrderApiServer.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? OrderId { get; set; }

        [Required]
        public string? MenuItem {  get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; } = "대기 중";    // 기본 상태

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
