using System.Globalization;
using System.Text;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Генерирует уникальный slug организации по имени с суффиксом от Id.
/// </summary>
public static class OrganizationSlugGenerator
{
    private const int MaxLength = 100;

    public static string Generate(string name, Guid organizationId)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var c in name.Trim().ToLowerInvariant())
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (char.IsAsciiLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (category is UnicodeCategory.SpaceSeparator
                or UnicodeCategory.DashPunctuation
                or UnicodeCategory.ConnectorPunctuation)
            {
                if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }
        }

        var baseSlug = builder.ToString().Trim('-');
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "org";
        }

        var suffix = organizationId.ToString("N");
        if (baseSlug.Length > MaxLength - suffix.Length - 1)
        {
            baseSlug = baseSlug[..(MaxLength - suffix.Length - 1)].TrimEnd('-');
        }

        return $"{baseSlug}-{suffix}";
    }
}
