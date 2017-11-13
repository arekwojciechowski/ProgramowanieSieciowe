using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CRC
{
    public class CRC_algorytm
    {
        public static void Main()
        {
            
            
            Console.WriteLine("Podaj jakies dane: ");
            string tekst = Console.ReadLine();
            Console.WriteLine("Podane dane to: ", tekst);

            byte maska = 0;
            byte a = 0;
            byte b = 0;

            char[] litery = new char[] {'a', 'b', 'g', 'r'};

            foreach (var v in litery)
            {

                for(int j = 0; j<8; j++)
                {
                    maska = (byte) (1 << (j - 1));
                    b = (byte) ((v & maska) >> (j - 1));
                    a ^= b;
                }
            }

            Console.WriteLine("obliczona suma to: ");
            Console.WriteLine(a);
            
        }
    }
}