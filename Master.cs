using System.Collections.Generic;
using System.IO;
using System;

public static class Master
{
    public static List<SequenceLine[]>[] Measures = new List<SequenceLine[]>[Constants.Sound_Channels];
    public static List<int> Measure_Lengths;
    public static List<List<byte>> Instrument_Envelopes;
    public static List<List<byte>> Macros;
    public static List<List<byte>> Envelope_Volumes;
    public static List<List<byte>> Envelope_Lengths;
    public static List<byte> Instrument_Envelope_Conversions;
    public static List<byte> Instrument_Initial_Volumes;
    public static List<byte> ROM_Data;
    public static string ROM_Path;
    public static List<byte>[] Measure_Order = new List<byte>[Constants.Sound_Channels];
    public static int Measure_Length;
    public static int Speed;
    public static List<List<byte>>[] Byte_Data = new List<List<byte>>[Constants.Sound_Channels];
    public static List<byte> Linear_Data;
    public static int [] Measure_Loop_Pointers = new int[Constants.Sound_Channels];
    public static int [] Linear_Channel_Pointers = new int[Constants.Sound_Channels];
    public static List<byte>[] Final_Note = new List<byte>[Constants.Sound_Channels]; 
    public static int ROM_Offset;
    public static byte Final_Loop_Measure;
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
    public static byte getByteFromText(string text,int pos){
        byte result = 0;
        byte digit;
        for(int i=0;i<2;i++){
		    digit = (byte)text[pos++];
            digit = (byte)((digit >= 65) ? digit - 55 : digit - 48);
            if(digit > 0xf) return 0xff;
            result <<= 4;
            result |= digit;
        }
        return result;
    }
    public static byte getByteFromDecimalText(string text,ref int pos){
        byte result = 0;
        byte digit;
        while((pos < text.Length) && (text[pos] == ' ')) pos++;
        for(int i=0;i<2;i++){
            if(pos >= text.Length) return result;
		    digit = (byte)text[pos++];
            if((digit > 57) || (digit < 48)) return result;
            digit -= 48;
            result *= 10;
            result += digit;
        }
        return result;
    }
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
        
        Output = "";
        
        for(int i = 0;i<Linear_Data.Count;i++) Output += (convertByteToHexString(Linear_Data[i]) + ' ');
        File.WriteAllText("linear_output.txt",Output);
    }
}
