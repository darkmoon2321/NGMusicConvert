using System.Collections.Generic;
public class SequenceLine
{
    public string Note { get; private set; }
    public int Instrument { get; private set; }
    public byte Volume { get; private set; }
    public List<Effect> Effects { get; private set; }
    public SequenceLine(string Note, string Instrument, string Volume, List<Effect> Effects)
    {
        this.Note = Note;
        this.Volume = (byte)MyConvert.HexToDec(Volume);
        this.Instrument = MyConvert.HexToDec(Instrument);
        this.Effects = Effects;
    }
}