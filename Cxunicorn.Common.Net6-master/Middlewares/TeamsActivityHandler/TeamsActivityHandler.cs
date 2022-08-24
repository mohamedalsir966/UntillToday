using Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.TeamsActivityHandler
{
    public class TeamsActivityHandler : Microsoft.Bot.Builder.Teams.TeamsActivityHandler
    {
        private readonly IUsersDataRepository _usersDataRepository;
        public TeamsActivityHandler(IUsersDataRepository usersDataRepository)
        {
            this._usersDataRepository = usersDataRepository;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            ConversationReference botConRef = turnContext.Activity.GetConversationReference();
            var currentMember = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            await _usersDataRepository.AddorUpdateConversationRefrenceAsync(botConRef, currentMember);
        }
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            ConversationReference botConRef = turnContext.Activity.GetConversationReference();
            var currentMember = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
            await _usersDataRepository.AddorUpdateConversationRefrenceAsync(botConRef, currentMember);
        }
        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            ConversationReference botConRef = turnContext.Activity.GetConversationReference();
            var currentMember = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);

            if (activity.Action.Equals(InstallationUpdateActionTypes.Add))
                await _usersDataRepository.AddorUpdateConversationRefrenceAsync(botConRef, currentMember);
            else if (activity.Action.Equals(InstallationUpdateActionTypes.Remove))
                await _usersDataRepository.DeleteConversationRefrenceAsync(botConRef, currentMember);

        }
    }
}
