namespace UGem.Services.Category;

public class Request
{
    public class CreateCategoryRequest
    {
        public Guid? ParentId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public Guid Id  { get; set; }
    }
}