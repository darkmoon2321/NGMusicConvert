using System.Collections.Generic;

namespace NinjaGaidenMusicConverty
{
    class Program
    {
        static void Main(string[] args)
        {
            string Path = @"/home/david/Desktop/Working with ROMs/fceux-2.2.3-win32/ROMs/Ninja Gaiden/sound/expl_v8.txt";

            ReadFtmTXT.Read(Path);
            ConvertToNinjaGaiden ng = new ConvertToNinjaGaiden();
            ng.Convert();
            Master.WriteFiles();
        }
    }
}
