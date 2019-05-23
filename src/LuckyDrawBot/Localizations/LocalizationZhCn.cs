using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Localizations
{
    public class LocalizationZhCn : BaseLocalization
    {
        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Help.Message"] = "您可以发送这样一条消息来创建一个抽奖：<b>@luckydraw 神秘礼物, 1, 23:59</b> 或者输入 <b>@luckydraw start</b>",
            ["InvalidCommand.Message"] = "对不起，我不太理解您说的。\r\n\r\n您可以尝试以下命令：\r\n\r\n* \"start\" 用于创建一个抽奖活动\r\n\r\n* \"help\" 用于显示帮助信息",
            ["InvalidCommand.WinnerCountLessThanOne"] = "奖品个数必须大于 0。",
            ["InvalidCommand.PlannedDrawTimeNotFuture"] = "抽奖时间必须是一个将来的时间。",
            ["MainActivity.Draft.EditButton"] = "编辑",
            ["MainActivity.Draft.Title"] = "{0} 正在发起抽奖活动",
            ["MainActivity.Draft.Subtitle"] = "时刻准备好哦 😁",
            ["MainActivity.Description"] = "共有 {0} 个奖品. 抽奖时间：{1}",
            ["MainActivity.JoinButton"] = "我要参加",
            ["MainActivity.ViewDetailButton"] = "参与人",
            ["MainActivity.NoCompetitor"] = "",
            ["MainActivity.OneCompetitor"] = "{0} 参加了抽奖",
            ["MainActivity.TwoCompetitors"] = "{0} 和 {1} 参加了抽奖",
            ["MainActivity.ThreeCompetitors"] = "{0}, {1} 和 {2} 参加了抽奖",
            ["MainActivity.FourOrMoreCompetitors"] = "{0}, {1} 和另外 {2} 个人参加了抽奖",
            ["ResultActivity.NoWinnerTitle"] = "没有人参与此次抽奖",
            ["ResultActivity.NoWinnerSubtitle"] = "奖品: {0}",
            ["ResultActivity.NoWinnerImageUrl"] = "https://media.tenor.co/images/5d6e7c144fb4ef5985652ea6d7219965/raw",
            ["ResultActivity.WinnersTitle"] = "中奖者: {0}",
            ["ResultActivity.WinnersSubtitle"] = "奖品: {0}",
            ["ResultActivity.WinnersImageUrl"] = "https://serverpress.com/wp-content/uploads/2015/12/congrats-gif-2.gif",
            ["CompetitionDetail.Competitors"] = "参与人",
            ["EditCompetition.NotAllowed"] = "只有发起此次抽奖的人才可以进行编辑。",
            ["EditCompetition.Form.Gift.Label"] = "奖品",
            ["EditCompetition.Form.Gift.Placeholder"] = "奖品名字",
            ["EditCompetition.Form.Gift.Invalid"] = "你必须输入奖品名字。",
            ["EditCompetition.Form.WinnerCount.Label"] = "奖品个数",
            ["EditCompetition.Form.WinnerCount.Placeholder"] = "数字必须大于 0",
            ["EditCompetition.Form.WinnerCount.Invalid"] = "奖品个数必须大于 0。",
            ["EditCompetition.Form.PlannedDrawTime.Label"] = "抽奖时间",
            ["EditCompetition.Form.PlannedDrawTimeLocalDate.Placeholder"] = "使用格式：YYYY-MM-DD  比如：2020-08-28",
            ["EditCompetition.Form.PlannedDrawTime.Invalid"] = "抽奖时间必须是一个将来的时间。",
            ["EditCompetition.Form.GiftImageUrl.Label"] = "奖品图片URL",
            ["EditCompetition.Form.GiftImageUrl.Placeholder"] = "https://www.abc.com/xyz.jpg",
            ["EditCompetition.Form.GiftImageUrl.Invalid"] = "奖品图片URL必须以'http://'或者'https://'开头。",
            ["EditCompetition.SaveDraftButton"] = "保存",
            ["EditCompetition.ActivateCompetition"] = "开放抽奖",
        };

        public LocalizationZhCn() : base(Strings)
        {
        }
    }
}