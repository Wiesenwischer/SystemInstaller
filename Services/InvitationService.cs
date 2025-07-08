using SystemInstaller.Infrastructure.Adapters;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Web.Services
{
    public class InvitationService
    {
        private readonly LegacyInvitationServiceAdapter _adapter;

        public InvitationService(LegacyInvitationServiceAdapter adapter)
        {
            _adapter = adapter;
        }
        
        public async Task<UserInvitation> CreateInvitationAsync(
            Guid tenantId, 
            string email, 
            string firstName, 
            string lastName, 
            string role, 
            string invitedByUserId)
        {
            return await _adapter.CreateInvitationAsync(tenantId, email, firstName, lastName, role, invitedByUserId);
        }
        
        public async Task<UserInvitation?> GetInvitationByTokenAsync(string token)
        {
            return await _adapter.GetInvitationByTokenAsync(token);
        }
        
        public async Task<bool> AcceptInvitationAsync(string token, string userId)
        {
            return await _adapter.AcceptInvitationAsync(token, userId);
        }
        
        public async Task<List<UserInvitation>> GetPendingInvitationsAsync(Guid tenantId)
        {
            return await _adapter.GetPendingInvitationsAsync(tenantId);
        }
        
        public async Task<bool> CancelInvitationAsync(Guid invitationId)
        {
            return await _adapter.CancelInvitationAsync(invitationId);
        }
        
        public async Task<bool> ResendInvitationAsync(Guid invitationId)
        {
            return await _adapter.ResendInvitationAsync(invitationId);
        }
    }
}
