namespace UGem.Services.Category;

public class Request
{
    public class CreateCategoryRequest
    {
        public required Guid ParentId { get; set; }
        public required string Name { get; set; }
        
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public Guid Id  { get; set; }
    }
}