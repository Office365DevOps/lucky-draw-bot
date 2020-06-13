using System;
using Microsoft.Extensions.DependencyInjection;

namespace LuckyDrawBot.Handlers
{
    public class BotHandlers
    {
        private readonly IServiceProvider _services;

        public BotHandlers(IServiceProvider services)
        {
            _services = services;
        }

        public NonTeamsChannelHandler NonTeamsChannel => _services.GetRequiredService<NonTeamsChannelHandler>();
        public AddedToNewChannelHandler AddedToNewChannel => _services.GetRequiredService<AddedToNewChannelHandler>();

        public CommandHelpHandler CommandHelp => _services.GetRequiredService<CommandHelpHandler>();
        public CommandCreateCompetitionHandler CommandCreateCompetition => _services.GetRequiredService<CommandCreateCompetitionHandler>();
        public CommandCreateDraftCompetitionHandler CommandCreateDraftCompetition => _services.GetRequiredService<CommandCreateDraftCompetitionHandler>();
        public UnknownCommandHandler UnknownCommand => _services.GetRequiredService<UnknownCommandHandler>();

        public ActionJoinCompetitionHandler ActionJoinCompetition => _services.GetRequiredService<ActionJoinCompetitionHandler>();
        public ActionViewCompetitionDetailHandler ActionViewCompetitionDetail => _services.GetRequiredService<ActionViewCompetitionDetailHandler>();
        public ActionEditDraftCompetitionHandler ActionEditDraftCompetition => _services.GetRequiredService<ActionEditDraftCompetitionHandler>();
        public ActionSaveDraftCompetitionHandler ActionSaveDraftCompetition => _services.GetRequiredService<ActionSaveDraftCompetitionHandler>();
        public ActionActivateCompetitionHandler ActionActivateCompetition => _services.GetRequiredService<ActionActivateCompetitionHandler>();
    }
}
