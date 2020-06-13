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

        public HelpCommandHandler HelpCommand => _services.GetRequiredService<HelpCommandHandler>();

        public AddedToNewChannelHandler AddedToNewChannel => _services.GetRequiredService<AddedToNewChannelHandler>();

        public UnknownCommandHandler UnknownCommand => _services.GetRequiredService<UnknownCommandHandler>();

        public CreateDraftCompetitionCommandHandler CreateDraftCompetition => _services.GetRequiredService<CreateDraftCompetitionCommandHandler>();

        public CreateCompetitionCommandHandler CreateCompetitionCommand => _services.GetRequiredService<CreateCompetitionCommandHandler>();
    }
}
