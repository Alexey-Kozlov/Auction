namespace SearchService.DTO;

public record SearchParamsDTO(
    string SearchTerm,
    string Seller,
    string Winner,
    string OrderBy,
    string FilterBy,
    int PageNumber = 1,
    int PageSize = 5
);