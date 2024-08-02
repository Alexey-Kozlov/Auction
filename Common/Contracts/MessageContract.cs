namespace Common.Contracts;

public record MessageContract
(
      Guid Id,
      string Message,
      MessageType MessageType,
      Guid CorrelationId
);

public enum MessageType
{
      Ошибка,
      Предупреждение,
      Сообщение,
      НедостаточноДенег

}

