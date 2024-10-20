﻿namespace SearchService.DTO;

public record SearchParamsDTO(
    string SearchTerm,
    string SearchAdv,
    string Seller,
    string Winner,
    string OrderBy,
    string FilterBy,
    string SessionId,
    int PageNumber = 1,
    int PageSize = 5
);