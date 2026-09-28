using System.Security.Cryptography;

namespace EasyMemories.Services;

public static class JoinCodeGenerator
{
    // Sin caracteres ambiguos (0/O, 1/I/L) para que sea legible si alguien lo escribe a mano.
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public static string Generate(int length = 8)
    {
        Span<char> buffer = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(buffer);
    }
}
