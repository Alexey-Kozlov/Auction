namespace ProcessingService.DTO;

public record RestoreSnapShotDTO(
    DateTime RestoreDate,
    bool ResetLog
);