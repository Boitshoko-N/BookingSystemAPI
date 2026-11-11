using System.Security.Cryptography;

namespace BookingSystemAPI.Services
{
    public interface IBookingNumberGenerator
    {
        string Generate();
    }

    public class BookingNumberGenerator : IBookingNumberGenerator
    {
        // No 0, O, 1, I or L, so numbers can't be misread over the phone.
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int Length = 6;

        public string Generate()
        {
            var characters = new char[Length];
            for (int i = 0; i < Length; i++)
            {
                characters[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }
            return "BK-" + new string(characters);
        }
    }
}