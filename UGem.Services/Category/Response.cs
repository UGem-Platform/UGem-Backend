namespace UGem.Service.Category;

public class Response
{
    public class GetCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
    }
}