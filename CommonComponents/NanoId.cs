using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public class NanoId
    {
        //private Random random = new Random(); //Random() may be faster than cryptoservice, but higher chance of repeating values
        private static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

        public static string Gen_Id(string prefix, int randomBaseLen)
        {
            string possibleCharacters = "abcdefghjknopqrstuxyz123456789";

            prefix = prefix.ToLower();

            var rndPart = new char[randomBaseLen];
            byte[] num = new byte[1];

            for (int i = 0; i < rndPart.Length; i++)
            {
                random.GetBytes(num);
                var charIdx = (num[0] % possibleCharacters.Length); //Random number modulo charater range length produces zero-based character range

                rndPart[i] = possibleCharacters[charIdx];
            }

            return prefix + new String(rndPart);
        }
    }
}
