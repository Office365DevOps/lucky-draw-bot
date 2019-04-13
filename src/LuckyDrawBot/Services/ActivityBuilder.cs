using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ILocalizationFactory _localizationFactory;

        public ActivityBuilder(BotSettings botSettings, ILocalizationFactory localizationFactory)
        {
            _botSettings = botSettings;
            _localizationFactory = localizationFactory;
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

            var localization = _localizationFactory.Create(competition.Locale);
            var plannedDrawTimeString = competition.PlannedDrawTime.ToOffset(TimeSpan.FromHours(competition.OffsetHours))
                                                                   .ToString("f", CultureInfo.GetCultureInfo(competition.Locale));
            activity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = competition.Gift,
                        Subtitle = localization["MainActivity.Description", competition.WinnerCount, plannedDrawTimeString],
                        Text = GenerateCompetitorsText(competition),
                        Images = string.IsNullOrEmpty(competition.GiftImageUrl) ? null : new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = competition.GiftImageUrl
                            }
                        },
                        Buttons = competition.IsCompleted ? null : new List<CardAction>
                        {
                            new CardAction
                            {
                                Title = localization["MainActivity.JoinButton"],
                                Type = "invoke",
                                Value = new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = competition.Id }
                            }
                        }
                    }
                }
            };
            return activity;
        }

        private string GenerateCompetitorsText(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);

            var competitors = competition.Competitors.OrderByDescending(c => c.JoinTime).ToList();
            switch (competitors.Count)
            {
                case 0:
                    return localization["MainActivity.NoCompetitor"];
                case 1:
                    return localization["MainActivity.OneCompetitor", competitors[0].Name];
                case 2:
                    return localization["MainActivity.TwoCompetitors", competitors[0].Name, competitors[1].Name];
                case 3:
                    return localization["MainActivity.ThreeCompetitors", competitors[0].Name, competitors[1].Name, competitors[2].Name];
                default:
                    return localization["MainActivity.FourOrMoreCompetitors", competitors[0].Name, competitors[1].Name, competitors.Count - 2];
            }
        }

        public Activity CreateResultActivity(Competition competition)
        {
            var winners = competition.Competitors.Where(c => competition.WinnerAadObjectIds.Contains(c.AadObjectId)).ToList();

            var localization = _localizationFactory.Create(competition.Locale);
            HeroCard contentCard;
            if (winners.Count <= 0)
            {
                contentCard = new HeroCard()
                {
                    Title = localization["ResultActivity.NoWinnerTitle"],
                    Subtitle = localization["ResultActivity.NoWinnerSubtitle", competition.Gift],
                    Images = new List<CardImage>()
                    {
                        new CardImage()
                        {
                            Url = localization["ResultActivity.NoWinnerImageUrl"]
                        }
                    }
                };
            }
            else
            {
                contentCard = new HeroCard()
                {
                    Title = localization["ResultActivity.WinnersTitle", string.Join(", ", winners.Select(w => w.Name))],
                    Subtitle = localization["ResultActivity.WinnersSubtitle", competition.Gift],
                    Images = new List<CardImage>()
                    {
                        new CardImage()
                        {
                            Url = localization["ResultActivity.WinnersImageUrl"]
                        }
                    }
                };
            }

            var activity = Activity.CreateMessageActivity() as Activity;
            activity.From = new ChannelAccount(_botSettings.Id, "bot name");
            activity.Conversation = new ConversationAccount(id: competition.ChannelId);
            activity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = contentCard
                }
            };
            return activity;
        }
    }
}