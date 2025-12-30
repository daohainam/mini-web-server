using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http2Tests;

internal static class String2ByteExtentions
{
    public static byte[] ToByteArray(this string s)
    {
        byte[] byteArray = new byte[s.Length / 2];
        for (int i = 0; i < byteArray.Length; i++)
        {
            byteArray[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
        }
        return byteArray;
    }
}
