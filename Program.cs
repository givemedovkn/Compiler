using System;
using Compiler;

internal class Program
{
    private static void Main(string[] args)
    {
        string testFilePath = "source.txt";

        
        InputOutput.Init(testFilePath);

        char currentCh = ' ';
        TextPosition currentPos = new TextPosition();
        while (InputOutput.Ch != '\0')
        {
            currentCh = InputOutput.Ch;
            currentPos = InputOutput.PositionNow;
            if (currentCh == '@')
            {
                InputOutput.Error(1, currentPos);
            }
            if (currentCh == '\'' && currentPos.LineNumber == 2)
            {
                InputOutput.Error(2, currentPos);
            }
            if (currentCh == 'd' && currentPos.LineNumber == 3 && currentPos.CharNumber == 2)
            {
                InputOutput.Error(6, currentPos);
            }
            InputOutput.NextCh();
        }
    }
}