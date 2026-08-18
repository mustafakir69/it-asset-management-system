using System.Globalization;
using System.Text;

namespace TakipProgrami.Api.Helpers;

public static class UsernameRules
{
    public const int MaxLength = 100;

    public static string? FromFullName(string? fullName)
    {
        var username = Normalize(fullName);
        return username is not null && username.Contains('.') ? username : null;
    }

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        var separatorPending = false;

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            var normalizedCharacter = character switch
            {
                'ı' or 'İ' => 'i',
                'ç' or 'Ç' => 'c',
                'ğ' or 'Ğ' => 'g',
                'ö' or 'Ö' => 'o',
                'ş' or 'Ş' => 's',
                'ü' or 'Ü' => 'u',
                _ => char.ToLowerInvariant(character)
            };

            var isAsciiLetterOrDigit = normalizedCharacter is >= 'a' and <= 'z'
                or >= '0' and <= '9';
            if (!isAsciiLetterOrDigit)
            {
                separatorPending = result.Length > 0;
                continue;
            }

            if (separatorPending && result[^1] != '.') result.Append('.');
            result.Append(normalizedCharacter);
            separatorPending = false;
        }

        var normalized = result.ToString().Trim('.');
        return normalized.Length == 0 ? null : normalized;
    }

    public static string WithSuffix(string baseUsername, int suffix)
    {
        var suffixText = suffix <= 1 ? string.Empty : suffix.ToString(CultureInfo.InvariantCulture);
        var maximumBaseLength = MaxLength - suffixText.Length;
        var shortenedBase = baseUsername.Length <= maximumBaseLength
            ? baseUsername
            : baseUsername[..maximumBaseLength].TrimEnd('.');
        return $"{shortenedBase}{suffixText}";
    }

    public static string FirstAvailable(string baseUsername, IReadOnlySet<string> usedUsernames)
    {
        for (var suffix = 1; suffix < int.MaxValue; suffix++)
        {
            var candidate = WithSuffix(baseUsername, suffix);
            if (!usedUsernames.Contains(candidate)) return candidate;
        }

        throw new InvalidOperationException("Kullanıcı adı oluşturulamadı.");
    }
}
