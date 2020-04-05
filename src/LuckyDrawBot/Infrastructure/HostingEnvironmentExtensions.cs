namespace Microsoft.AspNetCore.Hosting
{
    public static class HostingEnvironmentExtensions
    {
        public static bool IsTest(this IHostingEnvironment hostingEnvironment)
        {
            return hostingEnvironment.IsEnvironment("Test");
        }
    }
}