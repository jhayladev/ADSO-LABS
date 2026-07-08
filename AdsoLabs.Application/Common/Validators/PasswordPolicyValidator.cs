namespace AdsoLabs.Application.Common.Validators;

public static class PasswordPolicyValidator
{
    private const int LongitudMinima = 8;

    public static (bool EsValido, string? Error) Validar(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < LongitudMinima)
            return (false, $"La contraseña debe tener al menos {LongitudMinima} caracteres.");

        if (!password.Any(char.IsUpper))
            return (false, "La contraseña debe incluir al menos una letra mayúscula.");

        if (!password.Any(char.IsDigit))
            return (false, "La contraseña debe incluir al menos un número.");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return (false, "La contraseña debe incluir al menos un carácter especial.");

        return (true, null);
    }
}
