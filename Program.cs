﻿using System.Collections.Generic;
using System.Windows.Forms;

namespace NinjaGaidenMusicConverty
{
    class Program
    {
        static void Main(string[] args)
        {
            //string Path = @"/home/david/Desktop/Working with ROMs/fceux-2.2.3-win32/ROMs/Ninja Gaiden/sound/expl_final.txt";
            string Path = "";
            Master.ROM_Offset = 0x1F38A;
            Master.Final_Loop_Measure = 0xff;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                Path = openFileDialog.FileName;
            }
            else return;
            OpenFileDialog romFileDialog = new OpenFileDialog();
            romFileDialog.Filter = "ROM files (*.nes)|*.nes|All files (*.*)|*.*";
            romFileDialog.RestoreDirectory = true;
            if (romFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                Master.ROM_Path = romFileDialog.FileName;
            }
            else return;
            
            ReadFtmTXT.Read(Path);
            ConvertToNinjaGaiden ng = new ConvertToNinjaGaiden();
            ng.Convert();
            Master.WriteFiles();
        }
    }
}
