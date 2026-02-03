using System;
using System.ComponentModel.DataAnnotations;

namespace NewECommerce_Project.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }                      

        [Required, MaxLength(200)]
        public string Name { get; set; }              

        public string Description { get; set; }      

        [Required]
        public decimal Price { get; set; }           

        public string ImageUrl { get; set; }       

        [Required]
        public int CategoryId { get; set; }            
        public Category Category { get; set; }

        public int Stock { get; set; }               

        public bool IsActive { get; set; } = true;     

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
