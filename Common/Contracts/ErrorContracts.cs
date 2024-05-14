namespace Contracts;

public record ErrorContract
(
      Guid Id,
      string ErrorMessage,
      Guid CorrelationId
);

