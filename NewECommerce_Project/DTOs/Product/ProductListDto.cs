namespace NewECommerce_Project.DTOs.Product
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } 
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }  = string.Empty;
        public int Stock {  get; set; }
    }

}
