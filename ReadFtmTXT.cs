using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ReadFtmTXT
{
    public static void Read(string Path)
    {
        var filestream = new System.IO.FileStream(Path,
                              System.IO.FileMode.Open,
                              System.IO.FileAccess.Read,
                              System.IO.FileShare.ReadWrite);
        var file = new System.IO.StreamReader(filestream, System.Text.Encoding.UTF8, true, 128);

        bool Found_Tracks_Header = false;

        bool Pattern_Read = false;
        int PatternNo = 0;

        for (int i = 0; i < Master.Measures.Length; i++)
        {
            Master.Measures[i] = new List<SequenceLine[]>();
        }

        List<SequenceLine>[] EntireMeasure;

        EntireMeasure = new List<SequenceLine>[Constants.Sound_Channels];
		for(int i = 0; i < EntireMeasure.Length; i++)
		{
			EntireMeasure[i] = new List<SequenceLine>();
		}

        string lineOfText;
        while ((lineOfText = file.ReadLine()) != null)
        {
            if (lineOfText == "# Tracks")
            {
                Found_Tracks_Header = true;
                continue;
            }
            else if (lineOfText.Contains("TRACK"))
            {
                string[] Splitted = lineOfText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                Master.Speed = int.Parse(Splitted[2]);
            }
            else if (Found_Tracks_Header && !Pattern_Read)
            {
                if (lineOfText.Contains("PATTERN "))
                {
                    int Length = "PATTERN ".Length;
                    PatternNo = MyConvert.HexToDec(lineOfText.Substring(Length, lineOfText.Length - Length));

                    Pattern_Read = true;
                    continue;
                }
            }
            else if (Pattern_Read)
            {
                if (!string.IsNullOrEmpty(lineOfText))
                {
                    string[] Splitted_ = lineOfText.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] Splitted = new string[Splitted_.Length - 1];

                    Array.Copy(Splitted_, 1, Splitted, 0, Splitted.Length);
                    Splitted_ = null;

                    int SoundChannel = 0;
                    foreach (string Entry in Splitted)
                    {
                        string[] Section = Entry.Split(' ');
                        string Note = "", Instrument = "", Volume = "";
                        List<Effect> l_effect = new List<Effect>();
                        for (int i = 0; i < Section.Length; i++)
                        {
                            string Index = Section[i];
                            switch (i)
                            {
                                case 0:
                                    Note = Index.Contains(".") ? "" : Index;
                                    break;
                                case 1:
                                    Instrument = Index.Contains(".") ? "" : Index;
                                    break;
                                case 2:
                                    Volume = Index.Contains(".") ? "" : Index;
                                    break;
                                default:
                                    string EffectName, Argument;
                                    if (Section[i] != "...")
                                    {
                                        EffectName = Index[0].ToString();
                                        Argument = Index.Substring(1);
                                        l_effect.Add(new Effect(EffectName, Argument));

                                    }
                                    break;
                            }
                        }
                        EntireMeasure[SoundChannel++].Add(new SequenceLine(Note, Instrument, Volume, l_effect));
                    }
                }
                else
                {
                    int sc = 0;
                    foreach (List<SequenceLine> s in EntireMeasure)
                    {
                        Master.Measures[sc++].Add(s.ToArray());
                    }
                    EntireMeasure = new List<SequenceLine>[Constants.Sound_Channels];
					for(int i = 0; i < EntireMeasure.Length; i++)
					{
						EntireMeasure[i] = new List<SequenceLine>();
					}
                    Pattern_Read = false;
                }
            }
        }
    }
}
