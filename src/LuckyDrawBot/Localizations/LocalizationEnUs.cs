using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Localizations
{
    public class LocalizationEnUs : BaseLocalization
    {
        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Help.Message"] = "Hi there, To start a lucky draw type something like <b>@luckydraw secret gift, 1, 23:59</b> or you can just type <b>@luckydraw start</b>.",
            ["InvalidCommand.Message"] = "I'm sorry I couldn't understand what you said.\r\n\r\nGet started with the following commands:\r\n\r\n* \"start\" to create a lucky draw\r\n\r\n* \"help\" to display help information",
            ["InvalidCommand.WinnerCountLessThanOne"] = "The number of prizes must be bigger than 0.",
            ["InvalidCommand.PlannedDrawTimeNotFuture"] = "The draw time must be future time.",
            ["MainActivity.Draft.EditButton"] = "Edit",
            ["MainActivity.Draft.Title"] = "{0} is starting a lucky draw",
            ["MainActivity.Draft.Subtitle"] = "Get ready for it 😁",
            ["MainActivity.Description"] = "{0} prize(s). Time: {1}",
            ["MainActivity.JoinButton"] = "I am in",
            ["MainActivity.ViewDetailButton"] = "Joined Users",
            ["MainActivity.NoCompetitor"] = "",
            ["MainActivity.OneCompetitor"] = "{0} joined this lucky draw",
            ["MainActivity.TwoCompetitors"] = "{0} and {1} joined this lucky draw",
            ["MainActivity.ThreeCompetitors"] = "{0}, {1} and {2} joined this lucky draw",
            ["MainActivity.FourOrMoreCompetitors"] = "{0}, {1} and {2} others joined this lucky draw",
            ["ResultActivity.NoWinnerTitle"] = "No one joined, no winner",
            ["ResultActivity.NoWinnerSubtitle"] = "Prize: {0}",
            ["ResultActivity.NoWinnerImageUrl"] = "https://media.tenor.co/images/5d6e7c144fb4ef5985652ea6d7219965/raw",
            ["ResultActivity.WinnersTitle"] = "Our winner(s): {0}",
            ["ResultActivity.WinnersSubtitle"] = "Prize: {0}",
            ["ResultActivity.WinnersImageUrl"] = "https://serverpress.com/wp-content/uploads/2015/12/congrats-gif-2.gif",
            ["CompetitionDetail.Competitors"] = "Joined Users",
            ["EditCompetition.NotAllowed"] = "Only the creator can edit.",
            ["EditCompetition.Form.Gift.Label"] = "Prize",
            ["EditCompetition.Form.Gift.Placeholder"] = "The name of prize",
            ["EditCompetition.Form.Gift.Invalid"] = "You must specify the prize.",
            ["EditCompetition.Form.WinnerCount.Label"] = "The number of prizes",
            ["EditCompetition.Form.WinnerCount.Placeholder"] = "The number should be bigger than 0",
            ["EditCompetition.Form.WinnerCount.Invalid"] = "The number of prizes must be bigger than 0.",
            ["EditCompetition.Form.PlannedDrawTime.Label"] = "Draw Time",
            ["EditCompetition.Form.PlannedDrawTimeLocalDate.Placeholder"] = "YYYY-MM-DD. For example, 2020-08-28",
            ["EditCompetition.Form.PlannedDrawTimeLocalTime.Placeholder"] = "HH:mm. For example, 16:30",
            ["EditCompetition.Form.PlannedDrawTime.Invalid"] = "The draw time must be future time.",
            ["EditCompetition.Form.GiftImageUrl.Label"] = "The URL of prize image",
            ["EditCompetition.Form.GiftImageUrl.Placeholder"] = "https://www.abc.com/xyz.jpg",
            ["EditCompetition.Form.GiftImageUrl.Invalid"] = "The URL of prize image must start with 'http://' or 'https://'.",
            ["EditCompetition.SaveDraftButton"] = "Save",
            ["EditCompetition.ActivateCompetition"] = "Start",
        };

        public LocalizationEnUs() : base(Strings)
        {
        }
    }
}