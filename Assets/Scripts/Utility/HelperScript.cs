using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class HelperScript
{
    public static string GenerateRandom(int length)
    {
        System.Random r = new System.Random();
        return new string(Enumerable.Repeat(StringPattern.RANDOM_CHARS, length).Select(s => s[r.Next(s.Length)]).ToArray());
    }


    public static string Sha256FromString(string toEncrypt)
    {
        var msg = Encoding.UTF8.GetBytes(toEncrypt);
        SHA256Managed hashString = new SHA256Managed();

        string hex = "";
        var hashValue = hashString.ComputeHash(msg);
        foreach (byte x in hashValue)
        {
            hex += string.Format("{0:x2}", x);

        }
        return hex;
    }


}
