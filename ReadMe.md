# NGMusicConvert
Tool for converting Famitracker data to Ninja Gaiden format

Original program created by FCandChill.  This program accepts text files containing Famitracker data for
conversion into a format that can be understood by the Ninja Gaiden music engine.  In Famitracker,
select "File->Export text" and save the text file containing your data.  Then open Program.cs and 
modify the file name and path to match your data file.  The program output will be created in the same
folder as the executable, with file name output.txt.

To compile on linux you must first have Mono installed:

sudo apt install mono-devel

Or follow the directions at: https://www.mono-project.com/download/stable/#download-lin

To compile NGMusicConvert from terminal using Mono:

mcs Program.cs Constants.cs Master.cs MyConvert.cs SequenceLine.cs Effect.cs ReadFtmTXT.cs ConvertToNinjaGaiden.cs
