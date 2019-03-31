using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class TestContext
    {
        public ITestOutputHelper Output { get; }

        public TestContext(ITestOutputHelper output)
        {
            Output = output;
        }
    }
}