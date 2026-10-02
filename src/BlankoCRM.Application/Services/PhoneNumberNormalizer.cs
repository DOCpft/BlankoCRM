using System.Text;
using BlankoCRM.Core.Exceptions;

namespace BlankoCRM.Application.Services;

public static class PhoneNumberNormalizer
{
    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new StringBuilder(phone.Length);
        foreach (var character in phone)
        {
            if (character is >= '0' and <= '9')
            {
                digits.Append(character);
            }
            else if (!char.IsWhiteSpace(character) && character is not '+' and not '(' and not ')' and not '-' and not '.' and not '/')
            {
                throw new ValidationException("Телефон содержит недопустимые символы.");
            }
        }

        if (digits.Length == 0 || digits.Length > 20)
        {
            throw new ValidationException("Телефон должен содержать от 1 до 20 цифр.");
        }

        if (digits.Length == 11 && digits[0] == '8')
        {
            digits[0] = '7';
        }
        else if (digits.Length == 10)
        {
            digits.Insert(0, '7');
        }

        return digits.ToString();
    }
}
