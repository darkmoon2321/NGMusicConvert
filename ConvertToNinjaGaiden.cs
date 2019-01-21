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
            int j;
            foreach (List<SequenceLine[]> ll_seq in Master.Measures)
            {
                Master.Byte_Data[SoundChannel] = new List<List<byte>>();

                int MeasureNumber = 0;
                foreach (SequenceLine[] Measure in ll_seq)
                {
                    int Prev_Length = 0;
                    int Curr_Length = 0;
                    byte Current_Note = 0,
                        Prev_Note,
                        Prev_Volume = 0xF,
                        Current_Volume = 0xF;
                        
                    Prev_Note = (SoundChannel == 3)? (byte)0x10 : (byte)0x0C;
                    string s_Current_Note = "";

                    List<Effect> Current_SFX = null;

                    List<byte> MeasureBytes = new List<byte>();
					int TickPointer = MeasureBytes.Count;
					bool IsBlank = true;
                    for (int i = 0; i < Measure.Length; i++)
                    {
                        //Console.WriteLine(string.Format("Soundchannel: {0} Measure: {1} Line {2}", SoundChannel.ToString("X2"), MeasureNumber.ToString("X2"), i.ToString("X2")));
                        
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
                            List <byte> LengthList = NoteLengthCode(Curr_Length);
                            MeasureBytes.Insert(TickPointer,LengthList[0]);
                            if(MeasureBytes.Count == 1) MeasureBytes.Add(Prev_Note);
                            for(j=1;j<LengthList.Count;j++){
                                if(LengthList[j] != LengthList[j-1]) MeasureBytes.Add(LengthList[j]);
                                MeasureBytes.Add(Prev_Note);
                            }
                            Prev_Length = (LengthList.Count == 1) ? Curr_Length : LengthList[LengthList.Count-1];
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
                        if(SoundChannel == 4){ //Constants.Channel.DPCM){
                            if(Current_Note == 0x20){
                                MeasureBytes.Add(0xFB);
                                MeasureBytes.Add(0x02);
                            }
                            else if(Current_Note == 0x21){
                                MeasureBytes.Add(0xFB);
                                MeasureBytes.Add(0x01);
                            }
                            MeasureBytes.Add(0x0C);
                        }
                        else{
                            MeasureBytes.Add(Current_Note);
                        }
                        Prev_Note = Current_Note;
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
                            List <byte> LengthList = NoteLengthCode(Curr_Length);
                            MeasureBytes.Insert(TickPointer,LengthList[0]);
                            for(j=1;j<LengthList.Count;j++){
                                MeasureBytes.Add(LengthList[j]);
                                MeasureBytes.Add(Prev_Note);
                            }
                        }
                    }
                    Master.Final_Note[SoundChannel].Add(Prev_Note);
                    Master.Byte_Data[SoundChannel].Add(MeasureBytes);
                    MeasureNumber++;
                }
                SoundChannel++;
            }
            
            for(SoundChannel = 0;SoundChannel < Constants.Sound_Channels;SoundChannel++){
                byte Prev_Note = (byte)((SoundChannel == 3) ? 0x10 : 0x0C);
                int MeasureNum;
                Master.Linear_Channel_Pointers[SoundChannel] = Master.Linear_Data.Count;
                Master.Linear_Data.Add(0xE0);   //*****Set volume envelope
                Master.Linear_Data.Add(0x05);   //*****Change this later on to be dependent on instrument envelope setting
                for(int i = 0;i < Master.Measure_Order[SoundChannel].Count;i ++){
                    MeasureNum = Master.Measure_Order[SoundChannel][i];
                    if(i == Master.Final_Loop_Measure){
                        Master.Measure_Loop_Pointers[SoundChannel] = Master.Linear_Data.Count;
                    }
                    if(Master.Byte_Data[SoundChannel][MeasureNum].Count == 0){
                        List <byte> LengthList = NoteLengthCode(Master.Measure_Length);
                        for(j=0;j<LengthList.Count;j++){
                            Master.Linear_Data.Add(LengthList[j]);
                            Master.Linear_Data.Add(Prev_Note);
                        }
                    }
                    else{
                        for(j=0;j<Master.Byte_Data[SoundChannel][MeasureNum].Count;j++) Master.Linear_Data.Add(Master.Byte_Data[SoundChannel][MeasureNum][j]);
                        Prev_Note = Master.Final_Note[SoundChannel][MeasureNum];
                    }
                }
                FindLocalLoops((byte)SoundChannel, Master.Linear_Channel_Pointers[SoundChannel], Master.Measure_Loop_Pointers[SoundChannel]);
                FindLocalLoops((byte)SoundChannel, Master.Measure_Loop_Pointers[SoundChannel],Master.Linear_Data.Count);
                int Loop_Start, 
                Loop_Check,
                k,
                Max_Length,
                start;
                
                start = Master.Linear_Channel_Pointers[SoundChannel];
                
                List<bool> is_argument = FindValidLoopLocations(start,Master.Linear_Data.Count);
                bool Local_Loop;
                int Local_Loop_Start;
                for(Loop_Start = Master.Linear_Channel_Pointers[SoundChannel] ; Loop_Start < Master.Linear_Data.Count;Loop_Start++){
                    if(is_argument[Loop_Start - start]) continue;
                    Max_Length = 0;
                    for(Loop_Check = Loop_Start+5 ; Loop_Check < Master.Linear_Data.Count;Loop_Check++){  //Don't start looking until at least 5 bytes after start
                        if(is_argument[Loop_Check - start]) continue;
                        Local_Loop = false;
                        Local_Loop_Start = 0;
                        j = 0;
                        while(((Loop_Check + j) < Master.Linear_Data.Count) && ((Loop_Start + j) < Master.Linear_Data.Count) &&
                            (Master.Linear_Data[Loop_Start + j] != 0xE9) &&
                            (Master.Linear_Data[Loop_Start + j] == Master.Linear_Data[Loop_Check + j])){
                            if(!is_argument[Loop_Start - start + j]){
                                if(Master.Linear_Data[Loop_Start + j] == 0xEB){
                                    Local_Loop = true;
                                    Local_Loop_Start = j;
                                }
                                else if(Master.Linear_Data[Loop_Start + j] == 0xEC){
                                    if(Local_Loop){
                                        Local_Loop = false; //Get here if the pattern fully contains the local loop.
                                    }
                                    else{
                                        break; //Do not allow a pattern to contain the local loop terminator without also having it contain the local loop command
                                    }
                                }
                            }
                            j++;
                        }
                        if(Local_Loop) j = Local_Loop_Start;    //Don't allow a pattern to start a local loop without finishing it
                        while(is_argument[Loop_Start - start + j]) j--; //Ensure we don't end the pattern in the middle of a command with arguments.
                        if(Loop_Start < Master.Measure_Loop_Pointers[SoundChannel] && ((Loop_Start + j) >= Master.Measure_Loop_Pointers[SoundChannel])){    //Don't cross the final loop boundary
                            j = Master.Measure_Loop_Pointers[SoundChannel] - Loop_Start;
                        }
                        if(Loop_Check < Master.Measure_Loop_Pointers[SoundChannel] && ((Loop_Check + j) >= Master.Measure_Loop_Pointers[SoundChannel])){    //Don't cross the final loop boundary
                            j = Master.Measure_Loop_Pointers[SoundChannel] - Loop_Check;
                        }
                        if(j > Max_Length) Max_Length = j;
                    }
                    if(Max_Length >= 10){   //Don't bother making a pattern unless it is more than 10 bytes
                        List<byte> pattern = new List<byte>();
                        for(j=0;j<Max_Length;j++) pattern.Add(Master.Linear_Data[Loop_Start + j]);
                        int Pattern_Pointer = Master.Linear_Channel_Pointers[SoundChannel];     //Insert the pattern before the start of the channel's data.
                        Pattern_Pointer += (((Master.ROM_Offset - 0x10) & 0x3FFF) | 0x8000);
                        Pattern_Pointer += (Constants.Sound_Channels*3) + 1;
                        
                        for(j = Master.Linear_Data.Count - Max_Length; j>= Master.Linear_Channel_Pointers[SoundChannel];j--){
                            for(k=0;k<Max_Length;k++) if(Master.Linear_Data[j+k] != pattern[k]) break;
                            if(k>=Max_Length){
                                if((j < Master.Measure_Loop_Pointers[SoundChannel]) && ((j+k) >= Master.Measure_Loop_Pointers[SoundChannel])) continue; //Ensure we don't cross the final loop boundary
                                Master.Linear_Data.RemoveRange(j,Max_Length);
                                Master.Linear_Data.Insert(j,0xE9);
                                Master.Linear_Data.Insert(j+1,(byte)Pattern_Pointer);
                                Master.Linear_Data.Insert(j+2,(byte)(Pattern_Pointer>>8));
                                if(j < Master.Measure_Loop_Pointers[SoundChannel]) Master.Measure_Loop_Pointers[SoundChannel] -= (Max_Length - 3);
                            }
                        }
                        
                        for(j=0;j<pattern.Count;j++) Master.Linear_Data.Insert(Master.Linear_Channel_Pointers[SoundChannel] + j,pattern[j]);
                        Master.Linear_Data.Insert(Master.Linear_Channel_Pointers[SoundChannel] + j,0xEA);
                        Master.Linear_Channel_Pointers[SoundChannel] += pattern.Count + 1;
                        Master.Measure_Loop_Pointers[SoundChannel] += pattern.Count + 1;
                        start += pattern.Count + 1;
                        Loop_Start += pattern.Count + 1;
                        is_argument = FindValidLoopLocations(Master.Linear_Channel_Pointers[SoundChannel],Master.Linear_Data.Count);
                        
                    }
                }
                
                if(Master.Final_Loop_Measure == 0xff){
                    Master.Linear_Data.Add(0xFF); //Terminate the track if it does not Loop
                }
                else{
                    Master.Linear_Data.Add(0xE8);   //Add loop pointer command
                    int Final_Loop_Pointer = Master.Measure_Loop_Pointers[SoundChannel];
                    Final_Loop_Pointer += (((Master.ROM_Offset - 0x10) & 0x3FFF) | 0x8000);
                    Final_Loop_Pointer += (Constants.Sound_Channels*3) + 1;
                    Master.Linear_Data.Add((byte)Final_Loop_Pointer);
                    Master.Linear_Data.Add((byte)(Final_Loop_Pointer >> 8));
                }
            }
            for(j = 0;j< Constants.Sound_Channels;j++){
                Master.Linear_Data.Insert(j*3,Constants.Music_Channels[j]);
                int Channel_Pointer = Master.Linear_Channel_Pointers[j];
                Channel_Pointer += (((Master.ROM_Offset - 0x10) & 0x3FFF) | 0x8000);
                Channel_Pointer += (Constants.Sound_Channels*3) + 1;
                if(Master.ROM_Offset >= 0x1C010) Channel_Pointer |= 0x4000;
                Master.Linear_Data.Insert((j*3)+1,(byte)Channel_Pointer);
                Master.Linear_Data.Insert((j*3)+2,(byte)(Channel_Pointer >> 8));
            }
            Master.Linear_Data.Insert(j*3,0xFF);
        }
        
        private List<bool> FindValidLoopLocations(int start,int end){
            List<bool> is_argument = new List<bool>();
            int j;
            
            for(j = start ; j < end;j++){
                is_argument.Add(false);
                switch(Master.Linear_Data[j]){
                    case 0xE0: //envelope
                    case 0xE2: //pulse duty
                    case 0xE3: //Volume
                    case 0xE4: //Sweep
                    case 0xE5: //Detune
                    case 0xEB: //Local Loop
                    case 0xFE: //Fade Out
                    case 0xE1: //2 byte NOP
                    case 0xE6: //2 byte NOP
                    case 0xE7: //2 byte NOP
                    case 0xEE: //2 byte NOP
                    case 0xF1: //2 byte NOP
                    case 0xF2: //2 byte NOP
                    case 0xF3: //2 byte NOP
                    case 0xF4: //2 byte NOP
                    case 0xF5: //2 byte NOP
                    case 0xF6: //2 byte NOP
                    case 0xF7: //2 byte NOP
                    case 0xFC: //2 byte NOP
                    case 0xFD: //2 byte NOP
                    is_argument.Add(true);
                    j++;
                    break;
                    case 0xE8: //audio JMP
                    case 0xE9: //audio pattern
                    is_argument.Add(true);
                    is_argument.Add(true);
                    j+=2;
                    break;
                    case 0xF0:  //4 byte NOP
                    is_argument.Add(true);
                    is_argument.Add(true);
                    is_argument.Add(true);
                    j+=3;
                    break;
                    case 0xFA:  //DPCM (1 or 3 args depending on bank)
                    case 0xFB:  //DPCM (1 or 3 args depending on bank)
                    if(Master.ROM_Offset > 0x1C010){
                        is_argument.Add(true);
                        j++;
                    }
                    else{
                        is_argument.Add(true);
                        is_argument.Add(true);
                        is_argument.Add(true);
                        j+=3;
                    }
                    break;
                    default:    //all commands with no args.  Notes, tempos, EA, EC, ED, EF, F8, F9, FF
                    break;
                }
            }
            return is_argument;
        }
        private void FindLocalLoops(byte SoundChannel, int start,int end){
            int Loop_Start, 
                Loop_Length,
                Num_Loops,
                j,
                k,
                Max_Length,
                Max_Loops,
                Max_Total_Length;
            List<bool> is_argument = FindValidLoopLocations(start,end);
            
            //Console.WriteLine(Master.convertByteToHexString((byte)(Master.Linear_Data.Count>>8)) + Master.convertByteToHexString((byte)Master.Linear_Data.Count));
            for(Loop_Start = start ; Loop_Start < end;Loop_Start++){
                //Console.WriteLine(Master.convertByteToHexString((byte)(Loop_Start>>8)) + Master.convertByteToHexString((byte)Loop_Start));
                if(is_argument[Loop_Start - start]) continue;   //Don't search for loops that would start within a command's arguments
                Loop_Length = 1;
                Max_Total_Length = 0;
                Max_Length = 0;
                Max_Loops = 0;
                /*Console.WriteLine(Loop_Start);
                Console.WriteLine(Master.Linear_Data.Count);
                Console.WriteLine(is_argument.Count);
                if(Loop_Start == 0x8A){
                    Console.WriteLine("1st major local loop should be here.");
                }*/
                while(Loop_Length <= ((end - Loop_Start)>>1)){
                    Num_Loops = 0xFF;
                    if((Loop_Start - start + Loop_Length) >= is_argument.Count) break;
                    if(is_argument[Loop_Start - start + Loop_Length]){
                        Loop_Length++;
                        continue;
                    }
                    /*if(Loop_Start == 0x8A){
                        Console.WriteLine("Passed argument testing.");
                    }*/
                    for(j=0;j<Loop_Length;j++){
                        for(k=1;k<Num_Loops;k++){
                            if(((Loop_Start + j + (Loop_Length*k)) >= end) || 
                                (Master.Linear_Data[Loop_Start + j] != Master.Linear_Data[Loop_Start + j + (Loop_Length*k)])){
                                Num_Loops = k;
                                break;
                            }
                        }
                        if(Num_Loops < 2) break;
                    }
                    if((j>=Loop_Length) && ((Num_Loops*Loop_Length) > Max_Total_Length)){
                        Max_Total_Length = Num_Loops*Loop_Length;
                        Max_Length = Loop_Length;
                        Max_Loops = Num_Loops;
                        //string text_debug = Master.convertByteToHexString((byte)(Max_Total_Length>>8)) + Master.convertByteToHexString((byte)Max_Total_Length) +
                        //    " , loops: " + Master.convertByteToHexString((byte)Max_Loops) + ", Length " + Master.convertByteToHexString((byte)Max_Length);
                        //Console.WriteLine(text_debug);
                    }
                    Loop_Length++;
                }
                if(Max_Total_Length >= (Max_Length+4)){
                    Master.Linear_Data.RemoveRange(Loop_Start+Max_Length,Max_Total_Length - Max_Length);    //Create loop.  Remove repetitive data
                    Master.Linear_Data.Insert(Loop_Start+Max_Length,0xEC);  //Add Loop terminator
                    Master.Linear_Data.Insert(Loop_Start,0xEB); //Add Loop command
                    Master.Linear_Data.Insert(Loop_Start+1,(byte)(Max_Loops));  //Tell the command how many times to loop
                    if(Master.Measure_Loop_Pointers[SoundChannel] > Loop_Start){
                        Master.Measure_Loop_Pointers[SoundChannel] -= (Max_Total_Length - Max_Length - 3);
                    }
                    Loop_Start += Max_Length + 3;
                    end -= (Max_Total_Length - Max_Length - 3);
                    is_argument = FindValidLoopLocations(start,end);
                }
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
                    Master.Final_Loop_Measure = (byte)sfx.Argument;
                    break;
                case "W":   //DPCM playback speed override
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

        private List<byte> NoteLengthCode(int blank_spaces)
        {
            int TickAmount = (blank_spaces) * Master.Speed;
            List<byte> ReturnAmount = new List<byte>();

            int TickRemainder = TickAmount;
            int ClosestIndex;
            int MinDiff;
            int CurrentDiff;
            while(TickRemainder != 0){
                MinDiff = 0xffff;
                ClosestIndex = -1;
                int i;
                for(i=0;i<TickTable.Length;i++){
                    if((TickTable[i] <= TickRemainder)){
                        CurrentDiff = TickRemainder - TickTable[i];
                        if(CurrentDiff < MinDiff){
                            MinDiff = CurrentDiff;
                            ClosestIndex = i;
                        }
                    }
                }
                if(ClosestIndex >= 0){
                    ReturnAmount.Add((byte)(0x80 + ClosestIndex));
                    TickRemainder -= TickTable[ClosestIndex];
                }
                else{
                    throw new Exception("Unmatched Tick Count " + TickAmount);
                }
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
