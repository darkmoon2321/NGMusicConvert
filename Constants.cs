using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Constants
{
    public const int NULL = -1;
    public const int b_NULL = 0xFF;
    public const int Sound_Channels = 5;
    public static readonly byte [] Music_Channels = new byte[] {0x04, 0x05, 0x02, 0x07, 0x06};
    public static readonly byte [,] Volume_Table = new byte[,] {
 	{0, 0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0, 	0},
 	{0, 1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1},
 	{0, 1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	2},
 	{0, 1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	1, 	2, 	2, 	2, 	2, 	2, 	3},
 	{0, 1, 	1, 	1, 	1, 	1, 	1, 	1, 	2, 	2, 	2, 	2, 	3, 	3, 	3, 	4},
 	{0, 1, 	1, 	1, 	1, 	1, 	2, 	2, 	2, 	3, 	3, 	3, 	4, 	4, 	4, 	5},
 	{0, 1, 	1, 	1, 	1, 	2, 	2, 	2, 	3, 	3, 	4, 	4, 	4, 	5, 	5, 	6},
 	{0, 1, 	1, 	1, 	1, 	2, 	2, 	3, 	3, 	4, 	4, 	5, 	5, 	6, 	6, 	7},
 	{0, 1, 	1, 	1, 	2, 	2, 	3, 	3, 	4, 	4, 	5, 	5, 	6, 	6, 	7, 	8},
 	{0, 1, 	1, 	1, 	2, 	3, 	3, 	4, 	4, 	5, 	6, 	6, 	7, 	7, 	8, 	9},
 	{0, 1, 	1, 	2, 	2, 	3, 	4, 	4, 	5, 	6, 	6, 	7, 	8, 	8, 	9, 	0xA},
 	{0, 1, 	1, 	2, 	2, 	3, 	4, 	5, 	5, 	6, 	7, 	8, 	8, 	9, 	0xA, 	0xB},
 	{0, 1, 	1, 	2, 	3, 	4, 	4, 	5, 	6, 	7, 	8, 	8, 	9, 	0xA, 	0xB, 	0xC},
 	{0, 1, 	1, 	2, 	3, 	4, 	5, 	6, 	6, 	7, 	8, 	9, 	0xA, 	0xB, 	0xC, 	0xD},
 	{0, 1, 	1, 	2, 	3, 	4, 	5, 	6, 	7, 	8, 	9, 	0xA, 	0xB, 	0xC, 	0xD, 	0xE},
 	{0, 1, 	2, 	3, 	4, 	5, 	6, 	7, 	8, 	9, 	0xA, 	0xB, 	0xC, 	0xD, 	0xE, 	0xF}
    };
    
    public enum Channel
    {
        Square1,
        Square2,
        Triangle,
        Noise,
        DPCM
    }
}
