using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class BaseTest
    {
        protected ITestOutputHelper Output { get; }
        protected TestContext TestContext { get; }

        public BaseTest(ITestOutputHelper output)
        {
            Output = output;
            TestContext = new TestContext(Output);
        }

        public ServerFixture CreateServerFixture(ServerFixtureConfiguration serverConfiguration)
        {
            return new ServerFixture(serverConfiguration, new TestContext(Output));
        }
    }
}
