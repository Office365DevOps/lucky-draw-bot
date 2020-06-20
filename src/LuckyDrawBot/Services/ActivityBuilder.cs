using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using AdaptiveCards;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Models;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace LuckyDrawBot.Services
{
    public interface IActivityBuilder
    {
        Activity CreateMainActivity(Competition competition);
        Activity CreateResultActivity(Competition competition);
        TaskModuleResponse CreateCompetitionDetailTaskInfoResponse(Competition competition);
        TaskModuleResponse CreateEditNotAllowedTaskInfoResponse(Competition competition);
        TaskModuleResponse CreateDraftCompetitionEditTaskInfoResponse(Competition competition, string errorMessage, string currentLocale);
        Attachment CreateComposeEditForm(string gift, int winnerCount, string giftImageUrl, DateTimeOffset localPlannedDrawTime, string errorMessage, string currentLocale);
        Attachment CreatePreviewCard(CompetitionEditForm editForm, DateTimeOffset localPlannedDrawTime, string locale);
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
            if (competition.Status == CompetitionStatus.Draft)
            {
                return CreateMainActivityForDraft(competition);
            }

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
            var viewDetailAction = new CardAction
            {
                Title = localization["MainActivity.ViewDetailButton"],
                Type = "invoke",
                Value = new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.ViewDetail, CompetitionId = competition.Id }
            };
            var joinAction = new CardAction
            {
                Title = localization["MainActivity.JoinButton"],
                Type = "invoke",
                Value = new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = competition.Id }
            };

            var isCompleted = (competition.Status == CompetitionStatus.Completed);
            var subtitle = isCompleted
                           ? localization["MainActivity.DescriptionAfterCompletion", competition.WinnerCount, plannedDrawTimeString]
                           : localization["MainActivity.Description", competition.WinnerCount, plannedDrawTimeString];
            activity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = competition.Gift,
                        Subtitle = subtitle,
                        Text = GenerateCompetitorsText(competition),
                        Images = string.IsNullOrEmpty(competition.GiftImageUrl) ? null : new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = competition.GiftImageUrl
                            }
                        },
                        Buttons = isCompleted ? new List<CardAction> { viewDetailAction } : new List<CardAction> { joinAction, viewDetailAction }
                    }
                }
            };
            return activity;
        }

        public Activity CreateMainActivityForDraft(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);
            var editDraftAction = new CardAction
            {
                Title = localization["MainActivity.Draft.EditButton"],
                Type = "invoke",
                Value = new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.EditDraft, CompetitionId = competition.Id }
            };

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
                        Title = localization["MainActivity.Draft.Title", competition.CreatorName],
                        Subtitle = localization["MainActivity.Draft.Subtitle"],
                        Buttons = new List<CardAction> { editDraftAction }
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

            var conversationId = competition.ChannelId;
            var isGroupChat = string.IsNullOrEmpty(competition.TeamId);
            if (!isGroupChat)
            {
                conversationId += ";messageid=" + competition.MainActivityId;
            }
            var activity = Activity.CreateMessageActivity() as Activity;
            activity.From = new ChannelAccount(_botSettings.Id, "bot name");
            activity.Conversation = new ConversationAccount(id: conversationId);
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

        public TaskModuleResponse CreateCompetitionDetailTaskInfoResponse(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);

            List<AdaptiveElement> body;
            if (competition.Competitors.Any())
            {
                body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = localization["CompetitionDetail.Competitors"],
                        Size = AdaptiveTextSize.Large
                    },
                };
                foreach (var competitor in competition.Competitors)
                {
                    var isWinner = competition.WinnerAadObjectIds.Contains(competitor.AadObjectId);
                    var namePrefix = isWinner ? "🎁 " : string.Empty;
                    body.Add(new AdaptiveTextBlock { Text = namePrefix + competitor.Name });
                }
            }
            else
            {
                body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = localization["CompetitionDetail.NoCompetitorJoined"],
                        Size = AdaptiveTextSize.Medium
                    },
                };
            }

            var taskInfo = new TaskModuleContinueResponse
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo
                {
                    Title = string.Empty,
                    Height = "medium",
                    Width = "small",
                    Card = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.0")
                        {
                            Body = body
                        }
                    }
                }
            };
            return new TaskModuleResponse { Task = taskInfo };
        }

        public TaskModuleResponse CreateEditNotAllowedTaskInfoResponse(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo
                    {
                        Title = string.Empty,
                        Height = "small",
                        Width = "medium",
                        Card = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = new AdaptiveCard("1.0")
                            {
                                Body = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = localization["EditCompetition.NotAllowed"],
                                        Size = AdaptiveTextSize.Large
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        public TaskModuleResponse CreateDraftCompetitionEditTaskInfoResponse(Competition competition, string errorMessage, string currentLocale)
        {
            var localization = _localizationFactory.Create(competition.Locale);
            var localPlannedDrawTime = competition.PlannedDrawTime.ToOffset(TimeSpan.FromHours(competition.OffsetHours));

            var body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.Gift.Label"],
                },
                new AdaptiveTextInput
                {
                    Id = "gift",
                    Placeholder = localization["EditCompetition.Form.Gift.Placeholder"],
                    Value = competition.Gift,
                    IsMultiline = false
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.WinnerCount.Label"],
                },
                new AdaptiveNumberInput
                {
                    Id = "winnerCount",
                    Placeholder = localization["EditCompetition.Form.WinnerCount.Placeholder"],
                    Value = competition.WinnerCount,
                    Min = 1,
                    Max = 10000
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.PlannedDrawTime.Label"],
                },
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "1",
                            Items = new List<AdaptiveElement>
                            {
                                // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
                                // DateInput does not post back the value in non-English situation.
                                // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
                                (currentLocale != null && currentLocale.StartsWith("en"))
                                    ? new AdaptiveDateInput
                                    {
                                        Id = "plannedDrawTimeLocalDate",
                                        Value = localPlannedDrawTime.ToString("yyyy-MM-dd")
                                    }
                                    : new AdaptiveTextInput
                                    {
                                        Id = "plannedDrawTimeLocalDate",
                                        Placeholder = localization["EditCompetition.Form.PlannedDrawTimeLocalDate.Placeholder"],
                                        Value = localPlannedDrawTime.ToString("yyyy-MM-dd")
                                    } as AdaptiveElement
                            }
                        },
                        new AdaptiveColumn
                        {
                            Width = "1",
                            Items = new List<AdaptiveElement>
                            {
                                // Similar to the above BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
                                // TimeInput does not send back the correct value for non-English culture, when user selects 4:30 of afternoon, it sends back "04:30".
                                // Workaround: use TextInput instead and validate user's input against "HH:mm" format
                                (currentLocale != null && currentLocale.StartsWith("en"))
                                    ? new AdaptiveTimeInput
                                    {
                                        Id = "plannedDrawTimeLocalTime",
                                        Value = localPlannedDrawTime.ToString("HH:mm")
                                    }
                                    : new AdaptiveTextInput
                                    {
                                        Id = "plannedDrawTimeLocalTime",
                                        Placeholder = localization["EditCompetition.Form.PlannedDrawTimeLocalTime.Placeholder"],
                                        Value = localPlannedDrawTime.ToString("HH:mm")
                                    } as AdaptiveElement

                            }
                        }
                    }
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.GiftImageUrl.Label"],
                },
                new AdaptiveTextInput
                {
                    Id = "giftImageUrl",
                    Style = AdaptiveTextInputStyle.Url,
                    Placeholder = localization["EditCompetition.Form.GiftImageUrl.Placeholder"],
                    Value = competition.GiftImageUrl
                }
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                body.Insert(0, new AdaptiveTextBlock
                {
                    Text = errorMessage,
                    Color = AdaptiveTextColor.Attention
                });
            }

            var actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = localization["EditCompetition.SaveDraftButton"],
                    Data = new InvokeActionData { UserAction = InvokeActionType.SaveDraft, CompetitionId = competition.Id } 
                },
                new AdaptiveSubmitAction
                {
                    Title = localization["EditCompetition.ActivateCompetition"],
                    Data = new InvokeActionData { UserAction = InvokeActionType.ActivateCompetition, CompetitionId = competition.Id } 
                }
            };

            var taskInfo = new TaskModuleContinueResponse
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo
                {
                    Title = string.Empty,
                    Card = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = new AdaptiveCard("1.0")
                        {
                            Body = body,
                            Actions = actions
                        }
                    }
                }
            };
            return new TaskModuleResponse { Task = taskInfo };
        }

        public Attachment CreateComposeEditForm(string gift, int winnerCount, string giftImageUrl, DateTimeOffset localPlannedDrawTime, string errorMessage, string currentLocale)
        {
            var localization = _localizationFactory.Create(currentLocale);

            var body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.Gift.Label"],
                },
                new AdaptiveTextInput
                {
                    Id = "gift",
                    Placeholder = localization["EditCompetition.Form.Gift.Placeholder"],
                    Value = gift,
                    IsMultiline = false
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.WinnerCount.Label"],
                },
                new AdaptiveNumberInput
                {
                    Id = "winnerCount",
                    Placeholder = localization["EditCompetition.Form.WinnerCount.Placeholder"],
                    Value = winnerCount,
                    Min = 1,
                    Max = 10000
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.PlannedDrawTime.Label"],
                },
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "1",
                            Items = new List<AdaptiveElement>
                            {
                                // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
                                // DateInput does not post back the value in non-English situation.
                                // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
                                (currentLocale != null && currentLocale.StartsWith("en"))
                                    ? new AdaptiveDateInput
                                    {
                                        Id = "plannedDrawTimeLocalDate",
                                        Value = localPlannedDrawTime.ToString("yyyy-MM-dd")
                                    }
                                    : new AdaptiveTextInput
                                    {
                                        Id = "plannedDrawTimeLocalDate",
                                        Placeholder = localization["EditCompetition.Form.PlannedDrawTimeLocalDate.Placeholder"],
                                        Value = localPlannedDrawTime.ToString("yyyy-MM-dd")
                                    } as AdaptiveElement
                            }
                        },
                        new AdaptiveColumn
                        {
                            Width = "1",
                            Items = new List<AdaptiveElement>
                            {
                                // Similar to the above BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
                                // TimeInput does not send back the correct value for non-English culture, when user selects 4:30 of afternoon, it sends back "04:30".
                                // Workaround: use TextInput instead and validate user's input against "HH:mm" format
                                (currentLocale != null && currentLocale.StartsWith("en"))
                                    ? new AdaptiveTimeInput
                                    {
                                        Id = "plannedDrawTimeLocalTime",
                                        Value = localPlannedDrawTime.ToString("HH:mm")
                                    }
                                    : new AdaptiveTextInput
                                    {
                                        Id = "plannedDrawTimeLocalTime",
                                        Placeholder = localization["EditCompetition.Form.PlannedDrawTimeLocalTime.Placeholder"],
                                        Value = localPlannedDrawTime.ToString("HH:mm")
                                    } as AdaptiveElement

                            }
                        }
                    }
                },
                new AdaptiveTextBlock
                {
                    Text = localization["EditCompetition.Form.GiftImageUrl.Label"],
                },
                new AdaptiveTextInput
                {
                    Id = "giftImageUrl",
                    Style = AdaptiveTextInputStyle.Url,
                    Placeholder = localization["EditCompetition.Form.GiftImageUrl.Placeholder"],
                    Value = giftImageUrl
                }
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                body.Insert(0, new AdaptiveTextBlock
                {
                    Text = errorMessage,
                    Color = AdaptiveTextColor.Attention
                });
            }

            var actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = localization["EditCompetition.PreviewButton"],
                    Data = new ComposeActionData { UserAction = ComposeActionType.Preview } 
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = new AdaptiveCard("1.0")
                {
                    Body = body,
                    Actions = actions
                }
            };
        }

        public Attachment CreatePreviewCard(CompetitionEditForm editForm, DateTimeOffset localPlannedDrawTime, string locale)
        {
            var localization = _localizationFactory.Create(locale);
            var plannedDrawTimeString = localPlannedDrawTime.ToString("f", CultureInfo.GetCultureInfo(locale));
            var winnerCount = int.Parse(editForm.WinnerCount);

            var elements = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock { Text = editForm.Gift, Size = AdaptiveTextSize.Large },
                new AdaptiveTextBlock { Text = localization["MainActivity.Description", winnerCount, plannedDrawTimeString], Size = AdaptiveTextSize.Default },
                new AdaptiveTextBlock { Text = localization["MainActivity.NoCompetitor"], Size = AdaptiveTextSize.Default },
            };

            if (!string.IsNullOrEmpty(editForm.GiftImageUrl))
            {
                elements.Add(new AdaptiveImage { UrlString = editForm.GiftImageUrl });
            }
            elements.Add(new AdaptiveTextBlock { Id = "LuckyDrawData", IsVisible = false, Text = JsonSerializer.Serialize(editForm) });

            var card = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = new AdaptiveCard("1.0")
                {
                    Body = elements,
                    Height = AdaptiveHeight.Auto,
                    Actions = new List<AdaptiveAction>()
                    {
                        new AdaptiveSubmitAction
                        {
                            Type = AdaptiveSubmitAction.TypeName,
                            Title = localization["MainActivity.JoinButton"]
                        },
                        new AdaptiveSubmitAction
                        {
                            Type = AdaptiveSubmitAction.TypeName,
                            Title = localization["MainActivity.ViewDetailButton"]
                        },
                    }
                }
            };

            return card;
        }
    }
}