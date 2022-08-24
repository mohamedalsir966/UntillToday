using Azure.Identity;
using Cxunicorn.Common.Extensions.Authentication;
using Cxunicorn.Common.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Cxunicorn.Common.Services.Graph
{
    public class GraphService : IGraphService
    {
        public GraphServiceClient GraphDeligatedClient { get; set; }
        public GraphServiceClient GraphApplicationClient { get; set; }
        private readonly Guid TeamsProductSkuId = new("57ff2da0-773e-42df-b2af-ffb7a2317929");
        public GraphService(GraphServiceClient? applicationServiceClient = null, string? accessToken = null)
        {
            if (applicationServiceClient != null)
                this.GraphApplicationClient = applicationServiceClient;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                GraphDeligatedClient = new GraphServiceClient(
                          new DelegateAuthenticationProvider(
                              requestMessage =>
                              {
                                  //requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                  // Append the access token to the request.
                                  requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                                  // Get event times in the current time zone.
                                  requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                                  return Task.CompletedTask;
                              }));
            }
        }
        public async Task<IEnumerable<Guid>> GetAllMembersIdInOrgAsync()
        {
            var users = await GraphApplicationClient.Users
                .Request()
                .Filter("userType eq 'Member'")
                .Select(x => x.Id)
                .Top(500)
                .WithMaxRetry(5)
                .GetAsync();

            var usersCollection = new List<User>();
            usersCollection.AddRange(users.CurrentPage);
            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();
                usersCollection.AddRange(users.CurrentPage);
            }
            return usersCollection.Select(x => Guid.Parse(x.Id));

        }

        public async Task<IEnumerable<User>> GetAllMembersInOrgAsync()
        {
            var users = await GraphApplicationClient.Users
                .Request()
                .Filter("userType eq 'Member'")
                .Top(500)
                .WithMaxRetry(5)
                .GetAsync();

            var usersCollection = new List<User>();
            usersCollection.AddRange(users.CurrentPage);
            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();
                usersCollection.AddRange(users.CurrentPage);
            }
            return usersCollection;
        }

        public static async Task<string> CreateDeligatedTokenAsync(AuthenticationOptions graphOptions)
        {
            using var httpClient = new HttpClient();
            var url = "https://login.microsoftonline.com/" + graphOptions.AzureAdTenantId + "/oauth2/v2.0/token";
            List<KeyValuePair<string, string>> authBody = new()
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", graphOptions.AzureAdClientId),
                new KeyValuePair<string, string>("client_secret", graphOptions.AzureAdClientSecret),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                new KeyValuePair<string, string>("userName", graphOptions.ServiceAccountEmail),
                new KeyValuePair<string, string>("password", graphOptions.ServiceAccountPassword)
            };
            var formBody = new FormUrlEncodedContent(authBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = formBody
            };
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var strResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(strResponse);
            return tokenResponse.AccessToken;
        }

        public async Task<IEnumerable<(Guid AadId, string ConversationReference, string upn, string name)>> InstallAndGetConversationIdAsync(IEnumerable<Guid> userAadIds, string teamsAppId)
        {
            var proactiveInstallApplication = new ProactiveInstallation(GraphApplicationClient);

            List<(Guid AadId, string ConversationReference, string upn, string name)> userConvReference = new();
            foreach (var aadId in userAadIds)
            {
                try
                {
                    var result = await proactiveInstallApplication.InstallAppAndGetConversationIdAsync(teamsAppId, aadId);
                    var user = await GraphDeligatedClient.Users[Convert.ToString(aadId)].Request().Select("UserPrincipalName,displayName").WithMaxRetry(5).GetAsync();
                    userConvReference.Add((aadId, result, user.UserPrincipalName, user.DisplayName));
                }
                catch (Exception) { }
            }
            return userConvReference;
        }
        public async Task<IEnumerable<Guid>> GetDistinctMemberIdsForTeamsAndGroupsAsync(IEnumerable<Guid> groupIds)
        {
            List<User> graphUsers = new();
            foreach (var groupId in groupIds)
            {
                var members = await GraphApplicationClient
                                    .Groups[Convert.ToString(groupId)]
                                    .TransitiveMembers
                                    .Request()
                                    .Select(x => x.Id)
                                    .Top(500)
                                    .WithMaxRetry(5)
                                    .GetAsync();

                graphUsers.AddRange(members.OfType<User>().ToList()!);
                while (members.NextPageRequest != null)
                {
                    members = await members.NextPageRequest.GetAsync();
                    graphUsers.AddRange(members.OfType<User>().ToList()!);
                }
            }
            return graphUsers.Select(x => Guid.Parse(x.Id)).Distinct();
        }
        public async Task<IEnumerable<(string base64, string userName, string userJob, string userAadId)>> GetUsersInfoAndPhotos(IEnumerable<string> userAadIds)
        {
            List<(string base64, string userName, string userJob, string userAadId)> users = new();
            foreach (var userAadId in userAadIds)
            {
                var base64 = "";
                var user = await GraphDeligatedClient.Users[userAadId].Request().GetAsync();
                try
                {
                    Stream photoresponse = await GraphDeligatedClient.Users[userAadId].Photo.Content.Request().GetAsync();
                    if (photoresponse != null)
                    {
                        byte[] bytes;
                        using (var memoryStream = new MemoryStream())
                        {
                            photoresponse.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                        }
                        base64 = Convert.ToBase64String(bytes);
                    }
                }
                catch (Exception)
                { }
                users.Add(new(base64, user.DisplayName, user.JobTitle, user.Id));
            };
            return users;
        }
        public async Task SendMailAsync(string fromEmail, Message message)
        {
            await GraphApplicationClient.Users[fromEmail]
                .SendMail(message, true)
                .Request()
                .WithMaxRetry(5)
                .PostAsync();
        }


        public async Task<User> GetUserAsync(string emailNickname)
        {
            try
            {
                var org = (await GraphApplicationClient.Organization.Request().GetAsync()).FirstOrDefault();

                var emailFormatted = $"{emailNickname}@{org?.VerifiedDomains?.FirstOrDefault()?.Name}";

                var user = await GraphApplicationClient
                    .Users
                    .Request()
                    .Filter("startswith(userPrincipalName, '" + emailFormatted + "')")
                    .GetAsync();

                return user.CurrentPage.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<User> CreateUserAsync(string firstName, string lastName, string domainName,
            string? emailNickname = null,
            string? email = null,
            string? password = null,
            string? jobTitle = null,
            string? phone = null,
            bool RequireNonAlphanumeric = true,
            bool RequireDigit = true,
            bool RequireLowercase = true,
            bool RequireUppercase = true,
            int RequiredLength = 10)
        {
            if (string.IsNullOrWhiteSpace(password))
                password = PasswordHelper.GeneratePassword(
                    RequireNonAlphanumeric,
                    RequireDigit,
                    RequireLowercase,
                    RequireUppercase,
                    RequiredLength);

            var org = (await GraphApplicationClient.Organization.Request().GetAsync()).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(emailNickname))
                throw new Exception("Email or EmailNickname should be passed.");

            if (org == null)
                throw new Exception("No org found.");

            if (string.IsNullOrWhiteSpace(email))
            {
                if (!org.VerifiedDomains.Any())
                    throw new Exception("No verified domain found.");
            }

            string? emailFormatted = null;
            if (string.IsNullOrWhiteSpace(emailNickname))
                emailFormatted = email;
            else if (string.IsNullOrWhiteSpace(email))
                emailFormatted = $"{emailNickname}@{domainName}";

            var user = new User
            {
                AccountEnabled = true,
                DisplayName = firstName + " " + lastName,
                MailNickname = emailFormatted?.Split("@")[0],
                UserPrincipalName = emailFormatted,
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                },
                UsageLocation = org?.CountryLetterCode,
                GivenName = firstName,
                Surname = lastName,
                JobTitle = jobTitle,
                MobilePhone = phone,
            };

            user.Id = (await GraphApplicationClient.Users.Request().WithMaxRetry(5).AddAsync(user)).Id;
            return user;
        }

        public async Task AssignTeamsLicenseAsync(string userAadId)
        {
            var isLicensesAvailable = await IsTeamsLicensesAvailableAsync();
            if (!isLicensesAvailable.isAvailable)
                throw new Exception("No License available");

            var addLicenses = new List<AssignedLicense>()
                {
                    new AssignedLicense
                    {
                        SkuId = isLicensesAvailable.skuId
                    }
                };

            var removeLicenses = new List<Guid>() { };

            await GraphApplicationClient.Users[userAadId]
                .AssignLicense(addLicenses, removeLicenses)
                .Request()
                .WithMaxRetry(5)
                .PostAsync();
        }

        public async Task<IGraphServiceSubscribedSkusCollectionPage> GetAvailableLicensesAsync(Guid? skuId = null)
        {
            if (skuId == null || skuId == Guid.Empty)
            {
                return await GraphApplicationClient.SubscribedSkus
                    .Request()
                    .GetAsync();
            }

            return await GraphApplicationClient.SubscribedSkus
                .Request()
                .Filter($"skuId eq '{skuId}'")
                .WithMaxRetry(5)
                .GetAsync();
        }

        public async Task<bool> IsLicenseAvailableAsync(Guid skuId)
        {
            var licenses = await GetAvailableLicensesAsync();
            var ab = licenses.ToList();
            var license = licenses.Where(x => x.SkuId == skuId).FirstOrDefault();
            if (license == null)
                return false;

            if (license.CapabilityStatus != "Enabled")
                return false;

            if (license.PrepaidUnits.Enabled.GetValueOrDefault() == 0)
                return false;

            if (license.PrepaidUnits.Enabled.GetValueOrDefault() >= license.ConsumedUnits)
                return false;

            return true;
        }

        public async Task<(bool isAvailable, Guid? skuId)> IsTeamsLicensesAvailableAsync()
        {
            var licenses = await GetAvailableLicensesAsync();
            var license = licenses.Where(x => x.ServicePlans.Any(x => x.ServicePlanId == TeamsProductSkuId && x.ProvisioningStatus == Convert.ToString(ProvisioningResult.Success))).FirstOrDefault();
            if (license == null)
                return (false, null);

            if (license.CapabilityStatus != "Enabled")
                return (false, null);

            if (license.PrepaidUnits.Enabled.GetValueOrDefault() == 0)
                return (false, null);

            if (license.ConsumedUnits >= license.PrepaidUnits.Enabled.GetValueOrDefault())
                return (false, null);

            return (true, license.SkuId);
        }

        public async Task<ServicePrincipal?> GetServicePrincipal(string appId)
        {
            var servicePrincipals = await GraphApplicationClient.ServicePrincipals
            .Request()
            .Filter($"appId eq '{appId}'")
            .WithMaxRetry(5)
            .GetAsync();

            return servicePrincipals.CurrentPage.FirstOrDefault();
        }

        public async Task<IEnumerable<AppRoleAssignment>?> GetAppRoleAssignments(string userAadId, string resourceId)
        {
            var appRoleAssignments = await GraphApplicationClient
                    .Users[userAadId]
                    .AppRoleAssignments
                    .Request()
                    .Filter($"resourceId eq {resourceId}")
                    .WithMaxRetry(5)
                    .GetAsync();

            return appRoleAssignments.CurrentPage.ToList();
        }

        public async Task<IEnumerable<AppRole>?> GetAppRoles(string appObjectId)
        {
            var application = await GraphApplicationClient
                .Applications[appObjectId]
                .Request()
                .Select("appRoles")
                .WithMaxRetry(5)
                .GetAsync();

            if (application == null)
                return null;

            return application.AppRoles;
        }

        public async Task AssignRole(string userAadId, string appObjectId, string appRoleId)
        {
            var appRoleAssignment = new AppRoleAssignment
            {
                PrincipalId = Guid.Parse(userAadId),
                ResourceId = Guid.Parse(appObjectId),
                AppRoleId = Guid.Parse(appRoleId)
            };

            await GraphApplicationClient
                 .Users[userAadId]
                 .AppRoleAssignments
                 .Request()
                 .WithMaxRetry(5)
                 .AddAsync(appRoleAssignment);
        }

        public async Task RemoveAssignedRole(string servicePrincipalId, string appRoleAssignmentId)
        {
            await GraphApplicationClient
                .ServicePrincipals[servicePrincipalId]
                .AppRoleAssignments[appRoleAssignmentId]
                .Request()
                .WithMaxRetry(5)
                .DeleteAsync();
        }

        public async Task<string?> GetVerifiedDomainName()
        {
            var res = await GraphApplicationClient
                .Organization
                .Request()
                .WithMaxRetry(5)
                .GetAsync();

            var org = res.FirstOrDefault();

            if (org == null)
                throw new Exception("No org found.");

            if (!org.VerifiedDomains.Any())
                throw new Exception("No verified domain found.");


            return org.VerifiedDomains?.FirstOrDefault()?.Name;
        }

        public async Task<List<Presence>> GetUsersPresenceAsync(IEnumerable<string> userPool)
        {
            var presence = await GraphDeligatedClient.Communications
                             .GetPresencesByUserId(userPool)
                             .Request()
                             .Top(500)
                             .WithMaxRetry(5)
                             .PostAsync();

            var presences = new List<Presence>();
            presences.AddRange(presence.CurrentPage);
            while (presence.NextPageRequest != null)
            {
                presence = await presence.NextPageRequest.PostAsync();
                presences.AddRange(presence.CurrentPage);
            }

            return presences;
        }

        public async Task DeleteUser(string appObjectId)
        {
            await GraphApplicationClient.Users[appObjectId]
                             .Request()
                             .DeleteAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid aadObjectId)
        {
            try
            {
                var user = await GraphApplicationClient
                    .Users[aadObjectId.ToString()]
                    .Request()
                    .GetAsync();

                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> CreateTeamAsync(string name)
        {

            var team = new Team
            {
                DisplayName = name,
            //    Description = "My Sample Team’s Description",
                Members = new TeamMembersCollectionPage()
    {
        new AadUserConversationMember
        {
            Roles = new List<String>()
            {
                "owner"
            },
            AdditionalData = new Dictionary<string, object>()
            {
                {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('" + "38ca7058-5879-4859-8988-1600c917369b" + "')"}
            }
        }
    },
                AdditionalData = new Dictionary<string, object>()
    {
        {"template@odata.bind", "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"}
    }
            };


            var newTeam = await GraphApplicationClient.Teams
                .Request()
                .AddAsync(team);

            return newTeam.Id;
        }

        public async Task<string> CreateChannelAsync(string name, string teamId)
        {
            var channel = new Channel
            {
                MembershipType = ChannelMembershipType.Private,
                DisplayName = name,
              //  Description = "This is my first private channels",
                Members = new ChannelMembersCollectionPage()
    {
        new AadUserConversationMember
        {
            Roles = new List<String>()
            {
                "owner"
            },
            AdditionalData = new Dictionary<string, object>()
            {
                 {"user@odata.bind", "https://graph.microsoft.com/v1.0/users('" + "38ca7058-5879-4859-8988-1600c917369b" + "')"}
            }
        }
    }
            };


            await GraphApplicationClient.Teams[teamId].Channels
                .Request()
                .AddAsync(channel);

            return channel.Id;

        }

        public async Task<string> AddToTeamAndChannel(string teamName, string channelName, Guid aaId)
        {
            //get the team Id
            var groups = await GraphApplicationClient.Groups
                                         .Request()
                                         .Filter("resourceProvisioningOptions / Any(x: x eq 'Team') and startswith(displayName, '" + teamName + "')")
                                         .Select("id")
                                         .GetAsync();

            string teamId;
            string channelId;
            if (!groups.CurrentPage.Any())
            {
                // no existing team, so create one
                teamId = await CreateTeamAsync(teamName);

                //now create a private channel for it
                channelId = await CreateChannelAsync(channelName, teamId);
            }

            else
            {
                //the team is there now get the channel
                teamId = groups.FirstOrDefault()!.Id;

                var channel = await GraphApplicationClient.Teams[teamId].Channels
                                                 .Request()
                                                 .Filter("startswith(displayName, '" + channelName + "')")
                                                 .Select("id")
                                                 .GetAsync();
                if (!channel.Any())
                    channelId = await CreateChannelAsync(channelName, teamId);
                else
                {
                    channelId = channel.FirstOrDefault()!.Id;
                }

            }

            await AddMemberAsync(teamId, channelId, aaId);
            return channelId;

        }

        public async Task AddMemberAsync(string teamId, string channelId, Guid adId)
        {
            var conversationMember = new AadUserConversationMember
            {
                Roles = new List<String>()
                              {
                         
                                  },
                AdditionalData = new Dictionary<string, object>()
                             {
                         {"user@odata.bind", $"https://graph.microsoft.com/v1.0/users('" + adId + "')"}

                              }
            };

            await GraphApplicationClient.Teams[teamId].Members
                .Request()
                .AddAsync(conversationMember);

            await GraphApplicationClient.Teams[teamId].Channels[channelId].Members
                .Request()
                .AddAsync(conversationMember);
        }

        public async Task UpdateUserAsync(string adId, string? firstName, string? lastName,
            string? jobTitle = null,
            string? phone = null)
        {

            var user = new User();

            if(firstName!=null)
                user.GivenName = firstName;
            if (lastName != null)
                user.Surname = lastName;
            if(jobTitle!=null)
                user.JobTitle = jobTitle;
            if(phone !=null)
                user.MobilePhone = phone;

            await GraphApplicationClient.Users[adId]
                .Request()
                .UpdateAsync(user);
        }
    }
}
