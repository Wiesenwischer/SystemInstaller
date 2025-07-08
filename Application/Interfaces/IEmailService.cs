namespace SystemInstaller.Application.Interfaces;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string email, string tenantName, string invitationToken);
    Task SendWelcomeEmailAsync(string email, string tenantName);
}
