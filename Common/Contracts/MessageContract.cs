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

public interface IMessage
{
      string Data { get; }
}

public class Message : IMessage
{
      public string Data { get; init; }
      public Message(string message)
      {
            Data = message;
      }
}

