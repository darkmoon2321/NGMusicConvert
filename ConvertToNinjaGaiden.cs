using System;
using System.Collections.Generic;

namespace NinjaGaidenMusicConverty
{
    public class ConvertToNinjaGaiden
    {
        private enum Soundchannel
        {
            Square1,
            Square2,
            Triangle,
            Noise,
            DPCM
        }

        public void Convert()
        {
            int SoundChannel = 0;

            foreach (List<SequenceLine[]> ll_seq in Master.Measures)
            {
                Master.Byte_Data[SoundChannel] = new List<List<byte>>();

                int MeasureNumber = 0;
                foreach (SequenceLine[] Measure in ll_seq)
                {
                    int Prev_Length = 0;
                    int Curr_Length = 0;
                    byte Current_Note = 0,
                        Prev_Volume = 0xF,
                        Current_Volume = 0xF;

                    string s_Current_Note = "";

                    List<Effect> Current_SFX = null;

                    List<byte> MeasureBytes = new List<byte>();
					int TickPointer = MeasureBytes.Count;
					bool IsBlank = true;
                    for (int i = 0; i < Measure.Length; i++)
                    {
                        Console.WriteLine(string.Format("Soundchannel: {0} Measure: {1} Line {2}", SoundChannel.ToString("X2"), MeasureNumber.ToString("X2"), i.ToString("X2")));
                        
                        if(Measure[i].Note == "===") break;

                        IsBlank = true;
                        if (SoundChannel != (int)Soundchannel.Noise)
                        {
                            Current_Note = ConvertNote(Measure[i].Note, out IsBlank);
                        }
                        else
                        {
                            Current_Note = ConvertNoiseNote(Measure[i].Note, out IsBlank);
                        }
                        
                        s_Current_Note = Measure[i].Note;
						
                        if (Measure[i].Volume != Constants.b_NULL)
                        {
                            Current_Volume = Measure[i].Volume;
                        }
                        else
                        {
                            Current_Volume = Prev_Volume;
                        }
                        Current_SFX = Measure[i].Effects;
                        
                        if (IsBlank)
                        {
                            Curr_Length++;
                            continue;
                        }
                        
                        //add note length code if length changes or if inserting the first line.
                        if ((Prev_Length != Curr_Length) && (Curr_Length != 0))
                        {
                            MeasureBytes.Insert(TickPointer,NoteLengthCode(Curr_Length));
                            Prev_Length = Curr_Length;
                        }
                        Curr_Length = 0;
                        TickPointer = MeasureBytes.Count;
                        
                        //add volume code if volume changes
                        if (Current_Volume != Prev_Volume)
                        {
                            foreach (byte b1 in GetVolume(Current_Volume))
                            {
                                MeasureBytes.Add(b1);
                            }
                        }
                        
                        if (Current_SFX != null)
                        {
                            foreach (Effect sfx in Current_SFX)
                            {
                                foreach (byte b1 in ProcessSFX(sfx))
                                {
                                    MeasureBytes.Add(b1);
                                }
                            }
                        }
                        MeasureBytes.Add(Current_Note);
                        Current_Note = 0;
                        //ignore empty volumes.
                        if (Current_Volume != Constants.b_NULL)
                        {
                            if (SoundChannel != (int)Constants.Channel.Noise)
                            {
                                Prev_Volume = Current_Volume;
                                Current_Volume = 0;
                            }
                        }
                        s_Current_Note = "";
                        Current_SFX = null;
                        Curr_Length++;
                    }
                    if((MeasureBytes.Count != 0) && IsBlank){
                        if ((Prev_Length != Curr_Length) && (Curr_Length != 0))
                        {
                            MeasureBytes.Insert(TickPointer,NoteLengthCode(Curr_Length));
                        }
                    }
                    
                    Master.Byte_Data[SoundChannel].Add(MeasureBytes);
                    MeasureNumber++;
                }

                SoundChannel++;
            }
        }

        private List<byte> ProcessSFX(Effect sfx)
        {
            List<byte> Bytes = new List<byte>();

            switch (sfx.Effect_Prefix)
            {
                case "4":
                    if (sfx.Argument != 0)
                    {
                        Bytes.Add(0xED); //enable vibrato.
                    }
                    else
                    {
                        Bytes.Add(0xEF); //disable vibrato.
                    }
                    break;
                case "V":
                    Bytes.Add(0xE2);
                    byte b_toAdd = 0;
                    switch (sfx.Argument)
                    {
                        case 0x0:
                            b_toAdd = 0x00;
                            break;
                        case 0x1:
                            b_toAdd = 0x40;
                            break;
                        case 0x2:
                            b_toAdd = 0x80;
                            break;
                        case 0x3:
                            b_toAdd = 0xC0;
                            break;
                        default:
                            throw new Exception();
                    }

                    Bytes.Add(b_toAdd);
                    break;
                case "R":
                    Bytes.Add(0xE4);
                    Bytes.Add(0xA6);
                    break;
                case "B":	//Loop condition ****.  May edit this later
					break;
                default:
                    throw new Exception();
            }

            return Bytes;
        }

        private byte[] TickTable = new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x0A, 0x0C, 0x0E, 0x0F, 0x10, 0x12, 0x14, 0x15, 0x18, 0x1B,
            0x1C, 0x1E, 0x20, 0x24, 0x28, 0x2A, 0x30, 0x36, 0x38, 0x3C,
            0x40, 0x48, 0x50, 0x54, 0x60, 0x6C, 0x70, 0x80, 0x90, 0xC0,
            0xFF
        };

        private byte NoteLengthCode(int blank_spaces)
        {
            int TickAmount = (blank_spaces) * Master.Speed;
            byte ReturnAmount = 0;
            bool FoundTickAmount = false;

            for (int i = 0; i < TickTable.Length; i++)
            {
                if (TickTable[i] == TickAmount)
                {
                    FoundTickAmount = true;
                    ReturnAmount = (byte)(0x80 + i);
                    break;
                }
            }

            if (!FoundTickAmount)
            {
                throw new Exception("Unmatched Tick Count " + TickAmount);
            }

            return ReturnAmount;
        }


        private List<byte> GetVolume(int volume)
        {
            if (volume == 15)
            {
                volume--;
            }

            List<byte> ReturnMe = new List<byte>();
            if (volume != Constants.NULL)
            {
                ReturnMe.Add(0xE3);
                ReturnMe.Add((byte)(0x0F - (volume + 1)));
            }

            return ReturnMe;
        }

        private byte ConvertNoiseNote(string Note, out bool isBlank)
        {
            isBlank = false;

            byte FirstNibble = 0, LastNibble = 0;
            if (!string.IsNullOrEmpty(Note))
            {
                switch (Note)
                {
                    case "F-#": LastNibble = 0x00; break;
                    case "E-#": LastNibble = 0x01; break;
                    case "D-#": LastNibble = 0x02; break;
                    case "C-#": LastNibble = 0x03; break;
                    case "B-#": LastNibble = 0x04; break;
                    case "A-#": LastNibble = 0x05; break;
                    case "9-#": LastNibble = 0x06; break;
                    case "8-#": LastNibble = 0x07; break;
                    case "7-#": LastNibble = 0x08; break;
                    case "6-#": LastNibble = 0x09; break;
                    case "5-#": LastNibble = 0x0A; break;
                    case "4-#": LastNibble = 0x0B; break;
                    case "3-#": LastNibble = 0x0C; break;
                    case "2-#": LastNibble = 0x0D; break;
                    case "1-#": LastNibble = 0x0E; break;
                    case "0-#": LastNibble = 0x0F; break;
                    case "---": LastNibble = 0x10; break;
                    default: throw new Exception();
                }
            }
            else isBlank = true;

            return (byte)((FirstNibble << 4) + LastNibble);
        }

        private byte ConvertNote(string Note, out bool isBlank)
        {
            isBlank = false;

            byte FirstNibble = 0, LastNibble = 0;
            if (!string.IsNullOrEmpty(Note))
            {
                switch (Note.Substring(0, 2))
                {
                    case "C-": LastNibble = 0x00; break;
                    case "C#": LastNibble = 0x01; break;
                    case "D-": LastNibble = 0x02; break;
                    case "D#": LastNibble = 0x03; break;
                    case "E-": LastNibble = 0x04; break;
                    case "F-": LastNibble = 0x05; break;
                    case "F#": LastNibble = 0x06; break;
                    case "G-": LastNibble = 0x07; break;
                    case "G#": LastNibble = 0x08; break;
                    case "A-": LastNibble = 0x09; break;
                    case "A#": LastNibble = 0x0A; break;
                    case "B-": LastNibble = 0x0B; break;
                    case "--": LastNibble = 0x0C; break;
                    default: throw new Exception();
                }

                switch (Note.Substring(2, 1))
                {
                    case "-":
                    case "1": FirstNibble = 0x00; break;
                    case "2": FirstNibble = 0x01; break;
                    case "3": FirstNibble = 0x02; break;
                    case "4": FirstNibble = 0x03; break;
                    case "5": FirstNibble = 0x04; break;
                    case "6": FirstNibble = 0x05; break;
                    case "7": FirstNibble = 0x06; break;
                    case "8": FirstNibble = 0x07; break;
                    default: throw new Exception();
                }
            }
            else isBlank = true;

            return (byte)((FirstNibble << 4) + LastNibble);
        }
    }
}
