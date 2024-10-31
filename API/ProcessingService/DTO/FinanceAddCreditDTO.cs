namespace ProcessingService.DTO;
public record FinanceAddCreditDTO
(
     string UserLogin,
     int Amount,
     string SessionId
);