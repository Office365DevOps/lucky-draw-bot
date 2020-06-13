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

        public HelpCommandHandler HelpCommand => _services.GetRequiredService<HelpCommandHandler>();
        public UnknownCommandHandler UnknownCommand => _services.GetRequiredService<UnknownCommandHandler>();
        public CreateCompetitionCommandHandler CreateCompetitionCommand => _services.GetRequiredService<CreateCompetitionCommandHandler>();
        public CreateDraftCompetitionCommandHandler CreateDraftCompetitionCommand => _services.GetRequiredService<CreateDraftCompetitionCommandHandler>();

        public ActionJoinCompetitionHandler ActionJoinCompetition => _services.GetRequiredService<ActionJoinCompetitionHandler>();
        public ActionViewCompetitionDetailHandler ActionViewCompetitionDetail => _services.GetRequiredService<ActionViewCompetitionDetailHandler>();
        public ActionEditDraftCompetitionHandler ActionEditDraftCompetition => _services.GetRequiredService<ActionEditDraftCompetitionHandler>();
        public ActionSaveDraftCompetitionHandler ActionSaveDraftCompetition => _services.GetRequiredService<ActionSaveDraftCompetitionHandler>();
        public ActionActivateCompetitionHandler ActionActivateCompetition => _services.GetRequiredService<ActionActivateCompetitionHandler>();
    }
}
