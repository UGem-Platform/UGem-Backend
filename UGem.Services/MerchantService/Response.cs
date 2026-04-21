namespace UGem.Services.MerchantService;

public abstract class Response
{
    public class GetMerchantResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}