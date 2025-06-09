namespace SearchService.DTO;

public class SearchParamsDTO
{
    public string SearchTerm { get; set; }
    public string SearchAdv { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public string OrderBy { get; set; }
    public string FilterBy { get; set; }
    public string SessionId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
}
