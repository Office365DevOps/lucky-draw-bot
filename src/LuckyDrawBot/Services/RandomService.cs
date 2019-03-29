using System;

namespace LuckyDrawBot.Services
{
    public interface IRandomService
    {
        int Next(int maxValue);
    }

    public class RandomService : IRandomService
    {
        private readonly Random _random = new Random();

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }
    }
}