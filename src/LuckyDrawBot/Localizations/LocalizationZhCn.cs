using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Localizations
{
    public class LocalizationZhCn : BaseLocalization
    {
        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["Help.Message"] = "您可以发送这样一条消息来创建一个抽奖：<b>@luckydraw secret gift, 1h</b><br/>还想了解更多？这个是发送给抽奖机器人的消息格式：<br/>@luckydraw [奖品名字], [奖品数目], [抽奖时间], [奖品图片的URL网址]<br>",
            ["MainActivity.Description"] = "共有 {0} 个奖品. 抽奖时间：{1}",
            ["MainActivity.JoinButton"] = "我要参加",
            ["MainActivity.NoCompetitor"] = "",
            ["MainActivity.OneCompetitor"] = "{0} 参加了抽奖",
            ["MainActivity.TwoCompetitors"] = "{0} 和 {1} 参加了抽奖",
            ["MainActivity.ThreeCompetitors"] = "{0}, {1} 和 {2} 参加了抽奖",
            ["MainActivity.FourOrMoreCompetitors"] = "{0}, {1} 和另外 {2} 个人参加了抽奖",
            ["ResultActivity.NoWinner"] = "没有人参与此次抽奖",
            ["ResultActivity.NoWinnerImageUrl"] = "https://media.tenor.co/images/5d6e7c144fb4ef5985652ea6d7219965/raw",
            ["ResultActivity.Winners"] = "中奖者: {0}",
            ["ResultActivity.WinnersImageUrl"] = "https://serverpress.com/wp-content/uploads/2015/12/congrats-gif-2.gif",
        };

        public LocalizationZhCn() : base(Strings)
        {
        }
    }
}