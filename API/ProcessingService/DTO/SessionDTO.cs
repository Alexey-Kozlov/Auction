namespace ProcessingService.DTO;

public record SessionDTO(
    string SessionId
);
public record RestoreSnapShotDTO(
    string SessionId,
    DateTime RestoreDate,
    bool ResetLog
);