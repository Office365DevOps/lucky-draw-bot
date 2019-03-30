using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuckyDrawBot.Models;
using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Services
{
    public interface IActivityBuilder
    {
        Activity CreateMainActivity(Competition competition);
        Activity CreateResultActivity(Competition competition);
    }

    public class ActivityBuilder : IActivityBuilder
    {
        private readonly BotSettings _botSettings;

        public ActivityBuilder(BotSettings botSettings)
        {
            _botSettings = botSettings;
        }

        public Activity CreateMainActivity(Competition competition)
        {
            var isInitial = competition.Competitors.Count <= 0;

            var activity = Activity.CreateMessageActivity() as Activity;
            if (isInitial)
            {
                activity.From = new ChannelAccount(_botSettings.Id, "bot name");
                activity.Conversation = new ConversationAccount(id: competition.ChannelId);
            }

            activity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = competition.Gift,
                        Subtitle = competition.Description,
                        Text = GenerateCompetitorsText(competition.Competitors),
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://github.com/tony-xia/microsoft-teams-templates/raw/master/images/steak.jpg"
                            }
                        },
                        Buttons = new List<CardAction>
                        {
                            new CardAction
                            {
                                Title = "I am in",
                                Type = "invoke",
                                Value = new InvokeActionData { Type = InvokeActionType.Join, CompetitionId = competition.Id }
                            }
                        }
                    }
                }
            };
            return activity;
        }

        private string GenerateCompetitorsText(IList<Competitor> competitors)
        {
            switch (competitors.Count)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return string.Format("{0} joined this lucky draw", competitors[0].Name);
                case 2:
                    return string.Format("{0} and {1} joined this lucky draw", competitors[0].Name, competitors[1].Name);
                case 3:
                    return string.Format("{0}, {1} and {2} joined this lucky draw", competitors[0].Name, competitors[1].Name, competitors[2].Name);
                default:
                    return string.Format("{0}, {1} and {2} others joined this lucky draw", competitors[0].Name, competitors[1].Name, competitors.Count - 2);
            }
        }

        public Activity CreateResultActivity(Competition competition)
        {
            var winners = competition.Competitors.Where(c => competition.WinnerAadObjectIds.Contains(c.AadObjectId)).ToList();
            var activity = Activity.CreateMessageActivity() as Activity;
            activity.From = new ChannelAccount(_botSettings.Id, "bot name");
            activity.Conversation = new ConversationAccount(id: competition.ChannelId);
            activity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "Our winners are: " + string.Join(", ", winners.Select(w => w.Name)),
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://serverpress.com/wp-content/uploads/2015/12/congrats-gif-2.gif"
                            }
                        }
                    }
                }
            };
            return activity;
        }
    }
}