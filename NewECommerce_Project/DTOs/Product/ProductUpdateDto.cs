namespace NewECommerce_Project.DTOs.Product
{
    public class ProductUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public int Stock {  get; set; }
    }

}
