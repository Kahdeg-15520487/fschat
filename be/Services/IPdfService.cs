namespace be.Services
{
    public interface IPdfService
    {
        Task<Stream> GeneratePdfFromChat(Guid groupId);
    }
}
