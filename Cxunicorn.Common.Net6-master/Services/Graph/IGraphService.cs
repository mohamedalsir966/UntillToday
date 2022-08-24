using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Graph
{
    public interface IGraphService
    {
        public GraphServiceClient GraphDeligatedClient { get; set; }
        public GraphServiceClient GraphApplicationClient { get; set; }
        Task<IEnumerable<Guid>> GetAllMembersIdInOrgAsync();
        Task<IEnumerable<User>> GetAllMembersInOrgAsync();
        Task<IEnumerable<(Guid AadId, string ConversationReference, string upn, string name)>> InstallAndGetConversationIdAsync(IEnumerable<Guid> userAadIds, string teamsAppId);
        Task<IEnumerable<Guid>> GetDistinctMemberIdsForTeamsAndGroupsAsync(IEnumerable<Guid> groupIds);
        Task<IEnumerable<(string base64, string userName, string userJob, string userAadId)>> GetUsersInfoAndPhotos(IEnumerable<string> userAadIds);
        Task SendMailAsync(string fromEmail, Message message);
        Task AssignTeamsLicenseAsync(string userAadId);
        Task<User?> GetUserAsync(string emailNickname);
        Task<User> CreateUserAsync(string firstName, string lastName, string domainName,
              string? emailNickname = null,
              string? email = null,
              string? password = null,
              string? jobTitle = null,
              string? phone = null,
              bool RequireNonAlphanumeric = true,
              bool RequireDigit = true,
              bool RequireLowercase = true,
              bool RequireUppercase = true,
              int RequiredLength = 10);
        Task<IGraphServiceSubscribedSkusCollectionPage> GetAvailableLicensesAsync(Guid? skuId = null);
        Task<bool> IsLicenseAvailableAsync(Guid skuId);
        Task<(bool isAvailable, Guid? skuId)> IsTeamsLicensesAvailableAsync();

        // appId = clientId
        Task<ServicePrincipal?> GetServicePrincipal(string appId);
        // ServicePrincipal.Id = resourceId
        Task<IEnumerable<AppRoleAssignment>?> GetAppRoleAssignments(string userAadId, string resourceId);

        // appObjectId != clientId
        Task<IEnumerable<AppRole>?> GetAppRoles(string appObjectId);
        Task AssignRole(string userAadId,string appObjectId, string appRoleId);
        Task RemoveAssignedRole(string servicePrincipalId, string appRoleAssignmentId);
        Task<string?> GetVerifiedDomainName();
        Task<List<Presence>> GetUsersPresenceAsync(IEnumerable<string> userPool);
        Task DeleteUser(string appObjectId);
        Task<User?> GetUserByIdAsync(Guid aadObjectId);
        Task<string> CreateTeamAsync(string name);
        Task<string> CreateChannelAsync(string name, string teamId);
        Task<string> AddToTeamAndChannel(string teamName, string channelName, Guid aaId);
        Task AddMemberAsync(string teamId, string channelId, Guid adId);
        Task UpdateUserAsync(string adId, string? firstName, string? lastName,
           string? jobTitle = null,
           string? phone = null);

    }
}
