namespace ProcessingService.DTO;

public record SessionDTO(
    string SessionId
);
public record RestoreSnapShotDTO(
    string SessionId,
    string SnapShotId
);