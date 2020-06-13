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

        public NonTeamsChannelHandler NonTeamsChannel
        {
            get
            {
                return _services.GetRequiredService<NonTeamsChannelHandler>();
            }
        }

        public HelpCommandHandler HelpCommand
        {
            get
            {
                return _services.GetRequiredService<HelpCommandHandler>();
            }
        }

        public AddedToNewChannelHandler AddedToNewChannel
        {
            get
            {
                return _services.GetRequiredService<AddedToNewChannelHandler>();
            }
        }

        public UnknownCommandHandler UnknownCommand
        {
            get
            {
                return _services.GetRequiredService<UnknownCommandHandler>();
            }
        }
    }
}
