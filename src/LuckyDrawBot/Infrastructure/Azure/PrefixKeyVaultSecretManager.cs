using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace LuckyDrawBot.Infrastructure.Azure
{
    public class PrefixKeyVaultSecretManager : IKeyVaultSecretManager
    {
        private const string CommonPrefix = "LuckyDrawBotCommon--";
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = $"{prefix}--";
        }

        public bool Load(SecretItem secret)
        {
            // Load a vault secret when its secret name starts with the
            // common prefix or application prefix. Other secrets won't be loaded.
            return secret.Identifier.Name.StartsWith(CommonPrefix) || secret.Identifier.Name.StartsWith(_prefix);
        }

        public string GetKey(SecretBundle secret)
        {
            // Remove the prefix from the secret name and replace two
            // dashes in any name with the KeyDelimiter, which is the
            // delimiter used in configuration (usually a colon). Azure
            // Key Vault doesn't allow a colon in secret names.
            var isCommonSecret = secret.SecretIdentifier.Name.StartsWith(CommonPrefix);
            var prefix = isCommonSecret ? CommonPrefix : _prefix;
            var secretName = secret.SecretIdentifier.Name.Substring(prefix.Length);
            return secretName.Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}