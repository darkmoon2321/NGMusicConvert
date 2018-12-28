using System;
public static class MyConvert
{
    public static int HexToDec(string s) => s == "" ? Constants.NULL : Convert.ToInt32(s, 16);
}