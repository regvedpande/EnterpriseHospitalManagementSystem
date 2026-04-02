using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IAiAssistantService
    {
        Task<AiAssistantPageViewModel> BuildAsync(
            AiAssistantRole role,
            string userId,
            string? userName,
            string? prompt,
            CancellationToken ct = default);
    }
}
