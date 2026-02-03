namespace NewECommerce_Project.DTOs
{
    public class AllUserListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
    }
}
