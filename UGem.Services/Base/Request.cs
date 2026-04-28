namespace UGem.Services.Base;

public class Request
{
    public class PageRequest
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}