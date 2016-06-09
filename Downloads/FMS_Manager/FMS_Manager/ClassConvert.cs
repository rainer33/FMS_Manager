using System;
using System.Collections.Generic;
using System.Text;

namespace FMS_Manager
{
    #region stringFormat 샘플
    /*
    enum Color {Yellow = 1, Blue, Green};
    static DateTime thisDate = DateTime.Now;
    public static void Main() 
    {
        string s = "";
    // Format a negative integer or floating-point number in various ways.
        Console.WriteLine("Standard Numeric Format Specifiers");
        s = String.Format(
            "(C) Currency: . . . . . . . . {0:C}\n" +
            "(D) Decimal:. . . . . . . . . {0:D}\n" +
            "(E) Scientific: . . . . . . . {1:E}\n" +
            "(F) Fixed point:. . . . . . . {1:F}\n" +
            "(G) General:. . . . . . . . . {0:G}\n" +
            "    (default):. . . . . . . . {0} (default = 'G')\n" +
            "(N) Number: . . . . . . . . . {0:N}\n" +
            "(P) Percent:. . . . . . . . . {1:P}\n" +
            "(R) Round-trip: . . . . . . . {1:R}\n" +
            "(X) Hexadecimal:. . . . . . . {0:X}\n",
            -123, -123.45f); 
        Console.WriteLine(s);

    // Format the current date in various ways.
        Console.WriteLine("Standard DateTime Format Specifiers");
        s = String.Format(
            "(d) Short date: . . . . . . . {0:d}\n" +
            "(D) Long date:. . . . . . . . {0:D}\n" +
            "(t) Short time: . . . . . . . {0:t}\n" +
            "(T) Long time:. . . . . . . . {0:T}\n" +
            "(f) Full date/short time: . . {0:f}\n" +
            "(F) Full date/long time:. . . {0:F}\n" +
            "(g) General date/short time:. {0:g}\n" +
            "(G) General date/long time: . {0:G}\n" +
            "    (default):. . . . . . . . {0} (default = 'G')\n" +
            "(M) Month:. . . . . . . . . . {0:M}\n" +
            "(R) RFC1123:. . . . . . . . . {0:R}\n" +
            "(s) Sortable: . . . . . . . . {0:s}\n" +
            "(u) Universal sortable: . . . {0:u} (invariant)\n" +
            "(U) Universal sortable: . . . {0:U}\n" +
            "(Y) Year: . . . . . . . . . . {0:Y}\n", 
            thisDate);
        Console.WriteLine(s);

    // Format a Color enumeration value in various ways.
        Console.WriteLine("Standard Enumeration Format Specifiers");
        s = String.Format(
            "(G) General:. . . . . . . . . {0:G}\n" +
            "    (default):. . . . . . . . {0} (default = 'G')\n" +
            "(F) Flags:. . . . . . . . . . {0:F} (flags or integer)\n" +
            "(D) Decimal number: . . . . . {0:D}\n" +
            "(X) Hexadecimal:. . . . . . . {0:X}\n", 
            Color.Green);       
        Console.WriteLine(s);
        }
    }
    Standard Numeric Format Specifiers
    (C) Currency: . . . . . . . . ($123.00)
    (D) Decimal:. . . . . . . . . -123
    (E) Scientific: . . . . . . . -1.234500E+002
    (F) Fixed point:. . . . . . . -123.45
    (G) General:. . . . . . . . . -123
        (default):. . . . . . . . -123 (default = 'G')
    (N) Number: . . . . . . . . . -123.00
    (P) Percent:. . . . . . . . . -12,345.00 %
    (R) Round-trip: . . . . . . . -123.45
    (X) Hexadecimal:. . . . . . . FFFFFF85

    Standard DateTime Format Specifiers
    (d) Short date: . . . . . . . 6/26/2004
    (D) Long date:. . . . . . . . Saturday, June 26, 2004
    (t) Short time: . . . . . . . 8:11 PM
    (T) Long time:. . . . . . . . 8:11:04 PM
    (f) Full date/short time: . . Saturday, June 26, 2004 8:11 PM
    (F) Full date/long time:. . . Saturday, June 26, 2004 8:11:04 PM
    (g) General date/short time:. 6/26/2004 8:11 PM
    (G) General date/long time: . 6/26/2004 8:11:04 PM
        (default):. . . . . . . . 6/26/2004 8:11:04 PM (default = 'G')
    (M) Month:. . . . . . . . . . June 26
    (R) RFC1123:. . . . . . . . . Sat, 26 Jun 2004 20:11:04 GMT
    (s) Sortable: . . . . . . . . 2004-06-26T20:11:04
    (u) Universal sortable: . . . 2004-06-26 20:11:04Z (invariant)
    (U) Universal sortable: . . . Sunday, June 27, 2004 3:11:04 AM
    (Y) Year: . . . . . . . . . . June, 2004

    Standard Enumeration Format Specifiers
    (G) General:. . . . . . . . . Green
        (default):. . . . . . . . Green (default = 'G')
    (F) Flags:. . . . . . . . . . Green (flags or integer)
    (D) Decimal number: . . . . . 3
    (X) Hexadecimal:. . . . . . . 00000003
    */
    #endregion
    public class ClassConvert
    {
        #region 시리얼통신 부분
        public ulong hexToDec(byte[] arr) //0x00  0x39  0x02  0xA4
        {
            string strHex = BitConverter.ToString(arr, 0, arr.Length).Replace("-", ""); // "003902A4"
            ulong lDecimal = UInt64.Parse(strHex, System.Globalization.NumberStyles.HexNumber); //003902A4 -> 3736228
            return lDecimal;
        }

        public string hexToString(int n)
        {
            byte[] bytes = BitConverter.GetBytes(n);
            string hexString = "";
            hexString += bytes[0].ToString("X2");
            return hexString;
        }

        public string byteToString(byte[] arr)
        {
            string result = "";
            for (int i = 0; i < arr.Length; i++)
            {
                result += arr[i].ToString();
                if (i != arr.Length - 1)
                    result += ",";
            }
            return result;
        }

        public System.Collections.BitArray bitToString(byte arr)
        {
            System.Collections.BitArray myBA = new System.Collections.BitArray(8);
            byte bb = (byte)0x01;
            for (int i = 0; i < 8; i++)
            {
                myBA[i] = (byte)(arr & bb) == bb;
                bb <<= 1;
            }
            return myBA;
        }

        public byte[] bitArrayToByteArray(System.Collections.BitArray bits)
        {
            const int BITSPERBYTE = 8;
            int bytesize = bits.Length / BITSPERBYTE;
            if (bits.Length % BITSPERBYTE > 0)
            {
                bytesize++;
            }
            byte[] bytes = new byte[bytesize];
            // Must init to good value, all zero bit byte has value zero
            // Lowest significant bit has a place value of 1, each position to
            // to the left doubles the value
            byte value = 0;
            byte significance = 1;
            // Remember where in the input/output arrays
            int bytepos = 0;
            int bitpos = 0;
            while (bitpos < bits.Length)
            {
                // If the bit is set add its value to the byte
                if (true == bits[bitpos])
                {
                    value += significance;
                }
                bitpos++;
                if (0 == bitpos % BITSPERBYTE)
                {
                    // A full byte has been processed, store it
                    // increase output buffer index and reset work values
                    bytes[bytepos] = value;
                    bytepos++;
                    value = 0;
                    significance = 1;
                }
                else
                {
                    // Another bit processed, next has doubled value
                    significance *= 2;
                }
            }
            return bytes;
        }
        //CRC체크
        public byte[] CRC16(int n, byte[] ptr)
        {
            byte[] crc = new byte[2];
            ushort crct = 0xffff;
            byte uctmp;
            for (int uitmp = 0; uitmp < n; uitmp++)
            {
                crct = (ushort)((crct & 0xff00) | (crct ^ (ushort)(ptr[uitmp] & 0xff)));
                for (uctmp = 0; uctmp < 8; uctmp++)
                {
                    if ((crct & 0x0001) > 0)
                        crct = (ushort)((crct >> 1) ^ 0xa001);
                    else
                        crct >>= 1;
                }
            }
            crc[0] = (byte)(crct & 0x00ff);
            crc[1] = (byte)((crct >> 8) & 0xff);
            return crc;
            //crct.ToString("X2");//String.Format("0x{0:X2}",crct);
        }

        public int EXOR(int n, byte[] ptr)
        {
            int crc = 0;
            for (int a = 0; a < n; a++)
            {
                crc = crc ^ Convert.ToInt32((ptr[a]));
            }
            return crc;
        }


        public double Floating(byte[] msg1)
        {
            double[,] voltC = new double[2, 32];

            voltC[0, 0] = Math.Pow(2, 8);
            voltC[0, 1] = Math.Pow(2, 7);
            voltC[0, 2] = Math.Pow(2, 6);
            voltC[0, 3] = Math.Pow(2, 5);
            voltC[0, 4] = Math.Pow(2, 4);
            voltC[0, 5] = Math.Pow(2, 3);
            voltC[0, 6] = Math.Pow(2, 2);
            voltC[0, 7] = Math.Pow(2, 1);
            voltC[0, 8] = Math.Pow(2, 0);
            voltC[0, 9] = Math.Pow(2, -1);
            voltC[0, 10] = Math.Pow(2, -2);
            voltC[0, 11] = Math.Pow(2, -3);
            voltC[0, 12] = Math.Pow(2, -4);
            voltC[0, 13] = Math.Pow(2, -5);
            voltC[0, 14] = Math.Pow(2, -6);
            voltC[0, 15] = Math.Pow(2, -7);
            voltC[0, 16] = Math.Pow(2, -8);
            voltC[0, 17] = Math.Pow(2, -9);
            voltC[0, 18] = Math.Pow(2, -10);
            voltC[0, 19] = Math.Pow(2, -11);
            voltC[0, 20] = Math.Pow(2, -12);
            voltC[0, 21] = Math.Pow(2, -13);
            voltC[0, 22] = Math.Pow(2, -14);
            voltC[0, 23] = Math.Pow(2, -15);
            voltC[0, 24] = Math.Pow(2, -16);
            voltC[0, 25] = Math.Pow(2, -17);
            voltC[0, 26] = Math.Pow(2, -18);
            voltC[0, 27] = Math.Pow(2, -19);
            voltC[0, 28] = Math.Pow(2, -20);
            voltC[0, 29] = Math.Pow(2, -21);
            voltC[0, 30] = Math.Pow(2, -22);
            voltC[0, 31] = Math.Pow(2, -23);

            //msg1[0] = (byte)0x41;
            //msg1[1] = (byte)0x5C;
            //msg1[2] = (byte)0xA1;
            //msg1[3] = (byte)0x17;
            int a = Convert.ToInt32(msg1[0]);
            int b = Convert.ToInt32(msg1[1]);
            int c = Convert.ToInt32(msg1[2]);
            int d = Convert.ToInt32(msg1[3]);

            voltC[1, 0] = Convert.ToDouble(bitToString(msg1[0])[7]);
            voltC[1, 1] = Convert.ToDouble(bitToString(msg1[0])[6]);
            voltC[1, 2] = Convert.ToDouble(bitToString(msg1[0])[5]);
            voltC[1, 3] = Convert.ToDouble(bitToString(msg1[0])[4]);
            voltC[1, 4] = Convert.ToDouble(bitToString(msg1[0])[3]);
            voltC[1, 5] = Convert.ToDouble(bitToString(msg1[0])[2]);
            voltC[1, 6] = Convert.ToDouble(bitToString(msg1[0])[1]);
            voltC[1, 7] = Convert.ToDouble(bitToString(msg1[0])[0]);

            voltC[1, 8] = Convert.ToDouble(bitToString(msg1[1])[7]);
            voltC[1, 9] = Convert.ToDouble(bitToString(msg1[1])[6]);
            voltC[1, 10] = Convert.ToDouble(bitToString(msg1[1])[5]);
            voltC[1, 11] = Convert.ToDouble(bitToString(msg1[1])[4]);
            voltC[1, 12] = Convert.ToDouble(bitToString(msg1[1])[3]);
            voltC[1, 13] = Convert.ToDouble(bitToString(msg1[1])[2]);
            voltC[1, 14] = Convert.ToDouble(bitToString(msg1[1])[1]);
            voltC[1, 15] = Convert.ToDouble(bitToString(msg1[1])[0]);

            voltC[1, 16] = Convert.ToDouble(bitToString(msg1[2])[7]);
            voltC[1, 17] = Convert.ToDouble(bitToString(msg1[2])[6]);
            voltC[1, 18] = Convert.ToDouble(bitToString(msg1[2])[5]);
            voltC[1, 19] = Convert.ToDouble(bitToString(msg1[2])[4]);
            voltC[1, 20] = Convert.ToDouble(bitToString(msg1[2])[3]);
            voltC[1, 21] = Convert.ToDouble(bitToString(msg1[2])[2]);
            voltC[1, 22] = Convert.ToDouble(bitToString(msg1[2])[1]);
            voltC[1, 23] = Convert.ToDouble(bitToString(msg1[2])[0]);

            voltC[1, 24] = Convert.ToDouble(bitToString(msg1[3])[7]);
            voltC[1, 25] = Convert.ToDouble(bitToString(msg1[3])[6]);
            voltC[1, 26] = Convert.ToDouble(bitToString(msg1[3])[5]);
            voltC[1, 27] = Convert.ToDouble(bitToString(msg1[3])[4]);
            voltC[1, 28] = Convert.ToDouble(bitToString(msg1[3])[3]);
            voltC[1, 29] = Convert.ToDouble(bitToString(msg1[3])[2]);
            voltC[1, 30] = Convert.ToDouble(bitToString(msg1[3])[1]);
            voltC[1, 31] = Convert.ToDouble(bitToString(msg1[3])[0]);


            double bias = 0;
            double biasExp = 0;
            double unbiasExp = 0;
            double finalMulti = 0;
            double Mantissa = 0;
            double FullMantissa = 0;
            double currentVolt = 0;

            biasExp = (voltC[0, 1] * voltC[1, 1]) + (voltC[0, 2] * voltC[1, 2]) + (voltC[0, 3] * voltC[1, 3]) + (voltC[0, 4] * voltC[1, 4]) + (voltC[0, 5] * voltC[1, 5]) + (voltC[0, 6] * voltC[1, 6]) + (voltC[0, 7] * voltC[1, 7]) + (voltC[0, 8] * voltC[1, 8]);
            Mantissa = (voltC[0, 9] * voltC[1, 9]) + (voltC[0, 10] * voltC[1, 10]) + (voltC[0, 11] * voltC[1, 11]) + (voltC[0, 12] * voltC[1, 12]) + (voltC[0, 13] * voltC[1, 13]) + (voltC[0, 14] * voltC[1, 14]) + (voltC[0, 15] * voltC[1, 15]) + (voltC[0, 16] * voltC[1, 16]) + (voltC[0, 17] * voltC[1, 17]) + (voltC[0, 18] * voltC[1, 18]) + (voltC[0, 19] * voltC[1, 19]) + (voltC[0, 20] * voltC[1, 20]) + (voltC[0, 21] * voltC[1, 21]) + (voltC[0, 22] * voltC[1, 22]) + (voltC[0, 23] * voltC[1, 23]) + (voltC[0, 24] * voltC[1, 24]) + (voltC[0, 25] * voltC[1, 25]) + (voltC[0, 26] * voltC[1, 26]) + (voltC[0, 27] * voltC[1, 27]) + (voltC[0, 28] * voltC[1, 28]) + (voltC[0, 29] * voltC[1, 29]) + (voltC[0, 30] * voltC[1, 30]) + (voltC[0, 31] * voltC[1, 31]);

            if ((voltC[1, 1] + voltC[1, 2] + voltC[1, 3] + voltC[1, 4]) + (voltC[0, 5] * voltC[1, 5]) + (voltC[0, 6] * voltC[1, 6]) + (voltC[0, 7] * voltC[1, 7]) == 0)
            {
                bias = 126;
                FullMantissa = Mantissa;
            }
            else
            {
                bias = 127;
                FullMantissa = 1 + Mantissa;
            }
            unbiasExp = biasExp - bias;
            finalMulti = Math.Pow(2, unbiasExp);
            currentVolt = FullMantissa * finalMulti;
            if (voltC[1, 0] == 0)
            {
                return currentVolt;
            }
            else
            {
                double currentVolt2 = (currentVolt * -1);
                return currentVolt2;
            }
        }

        #endregion
    }
}
