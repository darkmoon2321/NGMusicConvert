public struct Effect
{
    public string Effect_Prefix { get; private set; }
    public int Argument { get; private set; }

    public Effect(string Effect_Prefix, string Argument)
    {
        this.Effect_Prefix = Effect_Prefix;
        this.Argument = MyConvert.HexToDec(Argument);
    }
}