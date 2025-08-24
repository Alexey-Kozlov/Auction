namespace FinanceService.DTO;

public class PagedParamsDTO
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string OrderBy { get; set; }
}