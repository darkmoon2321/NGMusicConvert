using System.Collections.Generic;
using System.IO;
using System;

public static class Master
{
    public static List<SequenceLine[]>[] Measures = new List<SequenceLine[]>[Constants.Sound_Channels];
    public static int Speed;
	public static string convertByteToHexString(byte toConvert){
        int i=0;
        byte[] letters = new byte[2];
        string s = "";
        for(i=0;i<2;i++)
        {
            letters[i] = (byte)(toConvert & 0xF);
            letters[i] = (byte)((letters[i] < 10) ? letters[i]+48 : letters[i] + 55);
            toConvert >>= 4;
        }
        for(i=1;i>=0;i--)
        {
            s += (char)(letters[i]);
        }
        return s;
    }
    public static List<List<byte>>[] Byte_Data = new List<List<byte>>[Constants.Sound_Channels];
    public static void WriteFiles()
    {
        string FileName = "output.txt";
        File.Delete(FileName);
        string Output = "";
        for(int Channel = 0; Channel < Byte_Data.Length; Channel++)
        {
            int MeasureNo = 0;
            
            foreach(List<byte> Byte in Byte_Data[Channel])
            {
                if (Byte.Count > 0)
                {
                    Output += string.Format("Soundchannel- {0} Measure- {1}.bin", Channel.ToString("X2"), MeasureNo.ToString("X2"));
                    Output += "\n";
                    foreach(byte SingleByte in Byte)
                    {
                        Output += (convertByteToHexString(SingleByte) + ' ');
                    }
                    Output += ("\n\n");
                }
                MeasureNo++;
            }
        }
        File.WriteAllText("output.txt",Output);
    }
}
