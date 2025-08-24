namespace ProcessingService.DTO;

public record SessionDTO();
public record RestoreSnapShotDTO(
    DateTime RestoreDate,
    bool ResetLog
);