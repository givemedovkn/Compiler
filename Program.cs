using System;
internal class Program
{
    private static void Main(string[] args)
    {
        const string inputPath = "source.txt";

        // Инициализируем файловый менеджер ввода-вывода
        InputOutput.Init(inputPath);

        // Запускаем синтаксический разбор
        SyntaxAnalyzer.Parse();

        // Вычитываем оставшиеся до конца файлы (если парсер завершил работу раньше)
        while (!InputOutput.IsEof)
        {
            LexicalAnalyzer.NextSym();
        }

    }
}