using System;
using System.Text;
using System.Security.Cryptography;

namespace brCodeUlt
{
    using static Utility;

    class Program
    {
        static void Main()
        {
            while (true)
            {
                try
                {
                    Console.Write("Encode or decode? (e/d) ");
                    string input = Console.ReadLine();

                    if (input == "e" || input == "encode")
                    {
                        Console.Write("Text: ");
                        string textToEncode = Console.ReadLine();
                        Console.Write("Password: ");
                        string password = Console.ReadLine();
                        byte[] encodedBrCode = BRCodeUlt.EncodeText(textToEncode, password);
                        Console.Write("Full save path: ");
                        string savePath = Console.ReadLine();
                        File.WriteAllBytes(savePath, encodedBrCode);
                    }
                    else if (input == "d" || input == "decode")
                    {
                        Console.Write("Full path to save encoded file: ");
                        string fileToDecode = Console.ReadLine();
                        byte[] bytes = File.ReadAllBytes(fileToDecode);
                        Console.Write("password: ");
                        string password = Console.ReadLine();
                        string decodedText = BRCodeUlt.DecodeText(bytes, password);
                        Console.WriteLine(decodedText + " ");
                    }
                    else
                    {
                        Console.WriteLine("I don't understand that");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    class Utility
    {
        public static Random random = new Random();

        public static string HashText(string text, string salt, SHA512 hasher)
        {
            byte[] textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(text, salt));
            byte[] hashedBytes = hasher.ComputeHash(textWithSaltBytes);
            hasher.Clear();
            return Convert.ToBase64String(hashedBytes);
        }
    }

    class BRCodeUlt
    {
        static int size = 16;

        public static string DecodeText(byte[] bytes, string password)
        {
            Console.WriteLine("Decrypting...");

            string returnValue = "";

            List<byte> newBytes = new();
            newBytes = bytes.ToList();

            int counter = 0;

            string hashedPassword = HashText(password, "!@BARD#$" + password + "!@CODE#$", SHA512.Create());

            char[] hashedPasswordArray = hashedPassword.ToCharArray();

            if (password != "")
            {
                for (int j = 0; j < 100000; j++)
                {
                    for (int i = 0; i < hashedPasswordArray.Length; i++)
                    {
                        hashedPasswordArray[i] = ((char)Convert.ToByte((Convert.ToInt32(hashedPasswordArray[i]) +
                            hashedPasswordArray[counter % hashedPasswordArray.Length]) % 256));

                        counter++;
                    }

                    counter = 0;

                    hashedPassword = HashText(hashedPassword, "!@BARD#$" + hashedPassword + "!@CODE#$", SHA512.Create());
                    hashedPasswordArray = hashedPassword.ToCharArray();
                }
            }

            hashedPassword = new string(hashedPasswordArray);

            if (password != "")
            {
                for (int i = 0; i < newBytes.Count; i++)
                {
                    int tempInt = Convert.ToInt32(newBytes[i]) -
                        hashedPassword[counter % hashedPassword.Length];

                    if (tempInt < 0)
                    {
                        tempInt = 256 + tempInt;
                    }

                    newBytes[i] = Convert.ToByte(tempInt);

                    counter++;
                }
            }

            byte[] newBytesList = newBytes.ToArray();

            int length = newBytesList.Length;

            for (int i = 0; i < length / 8; i++)
            {
                string binary = "";

                byte[] currentOctet = newBytesList[(i * 8)..((i + 1) * 8)];

                foreach (var v in currentOctet)
                {
                    binary += DecimalToBinary(Convert.ToInt32(Convert.ToString(v, 2), 2), 8);
                }

                returnValue += Decode(binary);
            }

            if ((int)returnValue[returnValue.Length - 1] == 3)
            {
                returnValue = returnValue[0..(returnValue.Length - 1)];
            }

            string recordString = returnValue;

            if ((int)recordString[0] == 23)
            {
                string[] records = recordString.Split((char)23);

                int offsetInt = recordString.Length;

                string _return = records[2][0..(Convert.ToInt32(records[1]))];

                return _return;
            }

            return returnValue;
        }
        
        public static byte[] EncodeText(string input, string password)
        {
            Console.WriteLine("Encrypting...");

            int length = input.Length;

            string tempStr = input;

            string startRecord = "";
            string endRecord = "";

            if (input.Length <= 1014)
            {
                int sectorLength = random.Next(1, 1014 - input.Length);

                int startRecordNum = sectorLength;
                int endRecordNum = (1014 - input.Length - sectorLength);

                startRecord = (char)23 + input.Length.ToString() + (char)23;

                int recordLength = startRecord.Length + endRecord.Length;

                List<char> set1 = new List<char>();
                List<char> set2 = new List<char>();

                for (int i = 0; i < startRecordNum; i++)
                {
                    int b = random.Next(32, 256);
                    set1.Add((char)b);
                }

                for (int i = 0; i < endRecordNum; i++)
                {
                    int b = random.Next(32, 256);
                    set2.Add((char)b);
                }

                string newText = input + new string(set1.ToArray()) + new string(set2.ToArray());

                tempStr = newText;
            }

            string text = tempStr;

            text = startRecord + text;

            List<byte> returnValue = new();

            List<string> twins = new();

            int index = 0;

            if (text.Length % 2 != 0)
                index = text.Length / 2 + 1;
            else
                index = text.Length / 2;

            for (int i = 0; i < index; i++)
            {
                if (text.Length % 2 != 0 && text.Length < 2)
                {
                    twins.Add(text + (char)03);
                }
                else
                {
                    twins.Add(text[0..2]);
                    text = text[2..];
                }
            }

            foreach (var v in twins)
            {
                returnValue.AddRange(Encode(v));
            }

            int counter = 0;

            string hashedPassword = HashText(password, "!@BARD#$" + password + "!@CODE#$", SHA512.Create());

            char[] hashedPasswordArray = hashedPassword.ToCharArray();

            if (password != "")
            {
                for (int j = 0; j < 100000; j++)
                {
                    for (int i = 0; i < hashedPasswordArray.Length; i++)
                    {
                        hashedPasswordArray[i] = ((char)Convert.ToByte((Convert.ToInt32(hashedPasswordArray[i]) +
                            hashedPasswordArray[counter % hashedPasswordArray.Length]) % 256));

                        counter++;
                    }

                    counter = 0;

                    hashedPassword = HashText(hashedPassword, "!@BARD#$" + hashedPassword + "!@CODE#$", SHA512.Create());
                    hashedPasswordArray = hashedPassword.ToCharArray();
                }
            }

            hashedPassword = new string(hashedPasswordArray);

            if (password != "")
            {
                for (int i = 0; i < returnValue.Count; i++)
                {
                    returnValue[i] = (Convert.ToByte((Convert.ToInt32(returnValue[i]) +
                        hashedPassword[counter % hashedPassword.Length]) % 256));

                    counter++;
                }
            }

            return returnValue.ToArray();
        }

        public static string Decode(string binary)
        {
            List<int> nullByteList = new();
            List<int> bytes = new();

            bytes.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 0, 4, 2, 4, 8), 2)));
            bytes.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 2, 4, 2, 4, 8), 2)));
            bytes.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 4, 4, 2, 4, 8), 2)));
            bytes.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 6, 4, 2, 4, 8), 2)));

            nullByteList.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 0, 0, 2, 4, 8), 2)));
            nullByteList.Add(Convert.ToChar(Convert.ToInt32(GetSector(binary, 2, 0, 2, 4, 8), 2)));

            string blue = GetSector(binary, 4, 0, 2, 2, 8);

            string lime = GetSector(binary, 4, 2, 2, 2, 8);

            string purple = GetSector(binary, 6, 2, 2, 2, 8);

            string orange = GetSector(binary, 6, 0, 2, 1, 8);

            string magenta = GetSector(binary, 6, 1, 2, 1, 8);

            bytes[Convert.ToInt32(orange, 2)] -= Convert.ToInt32(purple, 2);

            for (int i = 0; i < bytes.Count; i++)
            {
                bytes[i] -= (((Convert.ToInt32(magenta, 2) + i) % 4) + 1) * Convert.ToInt32(lime, 2);
            }

            for (int i = 0; i < bytes.Count; i++)
            {
                if (bytes[i] < 0)
                {
                    bytes[i] = 256 + bytes[i];
                }
            }

            List<int> newByteList = new();

            for (int i = 0; i < bytes.Count; i++)
            {
                if (bytes[i] != (((nullByteList[0]) + Convert.ToInt32(blue, 2))) % 256 &&
                    bytes[i] != (((nullByteList[1]) + Convert.ToInt32(blue, 2))) % 256)
                    newByteList.Add(bytes[i]);
            }

            string returnValue = "";

            foreach (var v in newByteList)
            {
                returnValue += (char)v;
            }

            return returnValue;
        }

        public static byte[] Encode(string text)
        {
            if (text.Length != 2)
                return null;

            string binary = "";

            for (int i = 0; i < 64; i++)
            {
                binary += "0";
            }

            List<int> bytes = new();

            int blue = random.Next(0, 16);

            int[] order = new int[] { 1, 2, 3, 4 };

            order.Shuffle();

            List<int> nullByteList = new();

            List<int> newByteList = new();

            int counter = 0;

            for (int i = 0; i < text.Length; i++)
            {
                bytes.Add((char)text[i]);
            }

            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] > text.Length)
                {
                    int nullByte = Convert.ToInt32(GetRandomNullByte(8), 2);

                    nullByteList.Add(nullByte);

                    nullByte += blue;

                    newByteList.Add(nullByte);
                }
                else
                {
                    newByteList.Add(bytes[counter]);
                    counter++;
                }
            }

            int lime = random.Next(0, 16);
            int magenta = random.Next(0, 4);

            for (int i = 0; i < newByteList.Count; i++)
            {
                newByteList[i] += (((magenta + i) % 4) + 1) * lime;
            }

            int orange = random.Next(0, text.Length);
            int purple = random.Next(0, 16);

            newByteList[orange] += purple;

            for (int i = 0; i < newByteList.Count; i++)
            {
                newByteList[i] = newByteList[i] % 256;
            }

            SetSector(ref binary, 0, 4, 2, 4, 8, DecimalToBinary(newByteList[0], 8));
            SetSector(ref binary, 2, 4, 2, 4, 8, DecimalToBinary(newByteList[1], 8));
            SetSector(ref binary, 4, 4, 2, 4, 8, DecimalToBinary(newByteList[2], 8));
            SetSector(ref binary, 6, 4, 2, 4, 8, DecimalToBinary(newByteList[3], 8));

            SetSector(ref binary, 0, 0, 2, 4, 8, DecimalToBinary(nullByteList[0], 8));
            SetSector(ref binary, 2, 0, 2, 4, 8, DecimalToBinary(nullByteList[1], 8));

            SetSector(ref binary, 4, 0, 2, 2, 8, DecimalToBinary(blue, 4));

            SetSector(ref binary, 4, 2, 2, 2, 8, DecimalToBinary(lime, 4));

            SetSector(ref binary, 6, 2, 2, 2, 8, DecimalToBinary(purple, 4));

            SetSector(ref binary, 6, 0, 2, 1, 8, DecimalToBinary(orange, 2));

            SetSector(ref binary, 6, 1, 2, 1, 8, DecimalToBinary(magenta, 2));

            List<byte> returnByteList = new();

            for (int i = 0; i < 8; i++)
            {
                returnByteList.Add(Convert.ToByte(Convert.ToInt32(binary[0..8], 2)));
                binary = binary[8..];
            }

            return returnByteList.ToArray();
        }

        private static string GetRandomNullByte(int length)
        {
            Random random = new Random();

            string result = "";

            int randomInt = random.Next(198, 246);

            return DecimalToBinary(randomInt, 8);
        }

        private static string DecimalToBinary(int number, int digits)
        {
            string tempStr = Convert.ToString(number, 2);

            while (tempStr.Length < digits)
            {
                tempStr = "0" + tempStr;
            }

            return tempStr;
        }

        public static void PrintCharArray(string text)
        {
            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine(text[(i * 8)..(i * 8 + 8)]);
            }
        }

        private static void ChangeCharInString(ref string text, int position, char newValue)
        {
            char[] chars = text.ToCharArray();

            chars[position] = newValue;

            text = new string(chars);
        }

        private static void SetSector(ref string binary, int xPos, int yPos, int xDim, int yDim, int binarySideLength, string newBinary)
        {
            string value = "";
            int counter = 0;

            int start = (yPos * (binarySideLength)) + xPos;

            for (int i = 0; i < yDim; i++)
            {
                for (int j = 0; j < xDim; j++)
                {
                    ChangeCharInString(ref binary, start + (i * (binarySideLength)) + j, newBinary[counter]);
                    counter++;
                }
            }
        }

        private static string GetSector(string binary, int xPos, int yPos, int xDim, int yDim, int binarySideLength)
        {
            string value = "";

            int start = (yPos * (binarySideLength)) + xPos;

            for (int i = 0; i < yDim; i++)
            {
                for (int j = 0; j < xDim; j++)
                {
                    int pos = start + (i * (binarySideLength)) + j;

                    value += binary[pos];
                }
            }

            return value;
        }
    }

    static class ExtensionMethods
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        public static string Offset(this string text, int amount)
        {
            char[] offsetString = text.ToCharArray();

            for (int i = 0; i < text.Length; i++)
            {
                int charInt = (Convert.ToInt32(text[i]) + amount);
                if (charInt < 0)
                    charInt = 256 + charInt;

                charInt = charInt % 256;

                offsetString[i] = (char)charInt;
            }

            return new string(offsetString);
        }
    }
}
