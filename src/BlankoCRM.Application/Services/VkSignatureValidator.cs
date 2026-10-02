using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Application.Services
{
    /// <summary>
    /// Реализация проверки подписи VK Mini App (HMAC-SHA256 + Base64 URL-safe).
    /// </summary>
    public class VkSignatureValidator : IVkSignatureValidator
    {
        private readonly byte[] _signingKey;

        public VkSignatureValidator(IConfiguration configuration)
        {
            var appSecret = configuration["VkApp:Secret"]
                ?? throw new InvalidOperationException("VkApp:Secret is not configured.");
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
            _signingKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("VK Mini Apps"));
        }

        /// <inheritdoc />
        public bool Validate(IReadOnlyDictionary<string, string> vkParams, string sign)
        {
            if (string.IsNullOrWhiteSpace(sign))
            {
                return false;
            }

            var ordered = vkParams
                .Where(p => p.Key.StartsWith("vk_", StringComparison.Ordinal))
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}={p.Value}");

            var queryString = string.Join("&", ordered);

            using var hmac = new HMACSHA256(_signingKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));

            var computed = Convert.ToBase64String(hash)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(sign));
        }
    }
}
