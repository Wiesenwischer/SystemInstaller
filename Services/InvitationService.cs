using Microsoft.EntityFrameworkCore;
using SystemInstaller.Web.Data;
using System.Net.Mail;
using System.Net;

namespace SystemInstaller.Web.Services
{
    public class InvitationService
    {
        private readonly SystemInstallerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvitationService> _logger;
        
        public InvitationService(
            SystemInstallerDbContext context, 
            IConfiguration configuration,
            ILogger<InvitationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<UserInvitation> CreateInvitationAsync(
            Guid tenantId, 
            string email, 
            string firstName, 
            string lastName, 
            string role, 
            string invitedByUserId)
        {
            // Check if user is already invited or member
            var existingInvitation = await _context.UserInvitations
                .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Email == email && !i.IsUsed);
            
            if (existingInvitation != null)
            {
                throw new InvalidOperationException("Eine Einladung für diese E-Mail-Adresse existiert bereits.");
            }
            
            var existingUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.Email == email);
            
            if (existingUser != null)
            {
                throw new InvalidOperationException("Benutzer ist bereits Mitglied dieses Tenants.");
            }
            
            var invitation = new UserInvitation
            {
                TenantId = tenantId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                InvitedByUserId = invitedByUserId,
                InvitationToken = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            
            _context.UserInvitations.Add(invitation);
            await _context.SaveChangesAsync();
            
            // Send invitation email
            await SendInvitationEmailAsync(invitation);
            
            return invitation;
        }
        
        public async Task<UserInvitation?> GetInvitationByTokenAsync(string token)
        {
            return await _context.UserInvitations
                .Include(i => i.Tenant)
                .FirstOrDefaultAsync(i => i.InvitationToken == token && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
        }
        
        public async Task<bool> AcceptInvitationAsync(string token, string userId)
        {
            var invitation = await GetInvitationByTokenAsync(token);
            if (invitation == null) return false;
            
            // Check if user is already member
            var existingUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == invitation.TenantId && tu.UserId == userId);
            
            if (existingUser != null)
            {
                // Mark invitation as used
                invitation.IsUsed = true;
                invitation.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            
            // Add user to tenant
            var tenantUser = new TenantUser
            {
                TenantId = invitation.TenantId,
                UserId = userId,
                Email = invitation.Email,
                FirstName = invitation.FirstName,
                LastName = invitation.LastName,
                Role = invitation.Role
            };
            
            _context.TenantUsers.Add(tenantUser);
            
            // Mark invitation as used
            invitation.IsUsed = true;
            invitation.UsedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<List<UserInvitation>> GetPendingInvitationsAsync(Guid tenantId)
        {
            return await _context.UserInvitations
                .Where(i => i.TenantId == tenantId && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<bool> CancelInvitationAsync(Guid invitationId)
        {
            var invitation = await _context.UserInvitations.FindAsync(invitationId);
            if (invitation == null || invitation.IsUsed) return false;
            
            _context.UserInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ResendInvitationAsync(Guid invitationId)
        {
            var invitation = await _context.UserInvitations
                .Include(i => i.Tenant)
                .FirstOrDefaultAsync(i => i.Id == invitationId && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
            
            if (invitation == null) return false;
            
            // Extend expiration
            invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            
            // Resend email
            await SendInvitationEmailAsync(invitation);
            return true;
        }
        
        private async Task SendInvitationEmailAsync(UserInvitation invitation)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(invitation.TenantId);
                if (tenant == null) return;
                
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:8081";
                var invitationUrl = $"{baseUrl}/invitation/accept/{invitation.InvitationToken}";
                
                var subject = $"Einladung zu {tenant.Name} - SystemInstaller";
                var body = $@"
                    <html>
                    <body>
                        <h2>Einladung zu {tenant.Name}</h2>
                        <p>Hallo {invitation.FirstName} {invitation.LastName},</p>
                        <p>Sie wurden zu dem Tenant <strong>{tenant.Name}</strong> im SystemInstaller eingeladen.</p>
                        <p>Ihre Rolle: <strong>{invitation.Role}</strong></p>
                        <p>Klicken Sie auf den folgenden Link, um die Einladung anzunehmen:</p>
                        <p><a href=""{invitationUrl}"">Einladung annehmen</a></p>
                        <p>Diese Einladung läuft am {invitation.ExpiresAt:dd.MM.yyyy HH:mm} ab.</p>
                        <p>Mit freundlichen Grüßen,<br/>Das SystemInstaller Team</p>
                    </body>
                    </html>
                ";
                
                await SendEmailAsync(invitation.Email, subject, body);
                
                _logger.LogInformation($"Invitation email sent to {invitation.Email} for tenant {tenant.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send invitation email to {invitation.Email}");
            }
        }
        
        private async Task SendEmailAsync(string to, string subject, string body)
        {
            // In a real application, you would configure SMTP settings
            // For now, we'll just log the email
            _logger.LogInformation($"EMAIL TO: {to}\nSUBJECT: {subject}\nBODY: {body}");
            
            // TODO: Implement actual email sending
            // Example with SMTP:
            /*
            var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
            {
                Port = int.Parse(_configuration["Email:Port"]),
                Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
                EnableSsl = true,
            };
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromAddress"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            
            mailMessage.To.Add(to);
            
            await smtpClient.SendMailAsync(mailMessage);
            */
            
            await Task.CompletedTask;
        }
    }
}
