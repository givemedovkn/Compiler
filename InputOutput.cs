namespace Compiler
{
    public class InputOutput
    {
        private const byte ErrMax = 9;

        private static char _ch;
        private static TextPosition _positionNow;
        private static List<Err> _err;
        
        private static string _line;
        private static byte _lastInLine;
        private static StreamReader _file;
        private static uint _errCount;

        private static readonly Dictionary<byte, string> _errorTable = new Dictionary<byte, string>
        {
            { 1, "Недопустимый символ" },
            { 2, "Незавершенная строковая константа (ожидался символ ')" },
            { 3, "Ожидалось число" },
            { 4, "Идентификатор превышает допустимую длину" },
            { 5, "Деление на ноль в константном выражении" },
            { 6, "Ожидалась точка '.' после END" }
        };

        public static char Ch
        {
            get
            {
                return _ch;
            }
            set
            {
                _ch = value;
            }
        }

        public static TextPosition PositionNow
        {
            get
            {
                return _positionNow;
            }
            set
            {
                _positionNow = value;
            }
        }

        public static List<Err> ErrList
        {
            get
            {
                return _err;
            }
        }

        public static void Init(string sourcePath)
        {
            _file = new StreamReader(sourcePath);
            _err = new List<Err>();

            _positionNow.LineNumber = 0;
            _positionNow.CharNumber = 0;
            _lastInLine = 0;
            _errCount = 0;

            ReadNextLine();

            if (_line != null && _line.Length > 0)
            {
                _ch = _line[0];
            }
        }
      
        public static void NextCh()
        {
            if (_positionNow.CharNumber == _lastInLine)
            {
                if (_positionNow.LineNumber >= 0)
                {
                    ListThisLine();
                    if (_err.Count > 0)
                    {
                        ListErrors();
                    }
                }

                ReadNextLine();
                _positionNow.LineNumber = _positionNow.LineNumber + 1;
                _positionNow.CharNumber = 0;
            }
            else
            {
                _positionNow.CharNumber = (byte)(_positionNow.CharNumber + 1);
            }

            _ch = _line[_positionNow.CharNumber];
        }

        private static void ListThisLine()
        {
            Console.WriteLine($"{_positionNow.LineNumber}: {_line.TrimEnd('\n', '\r')}");
        }

        private static void ReadNextLine()
        {
            if (!_file.EndOfStream)
            {
                _line = _file.ReadLine() + " ";
                _lastInLine = (byte)(_line.Length - 1);
            }
            else
            {
                End();
            }
        }

        private static void End()
        {
            Console.WriteLine($"\nКомпиляция завершена: ошибок — {_errCount}!");
            _file.Close();
            _line = null;
            _ch = '\0';
        }

        private static void ListErrors()
        {
            int prefixLength = $"{_positionNow.LineNumber}: ".Length;

            foreach (Err item in _err)
            {
                _errCount = _errCount + 1;
                string errorPrefix = "**";
                if (_errCount < 10)
                {
                    errorPrefix = errorPrefix + "0";
                }
                errorPrefix = errorPrefix + $"{_errCount}**";

                int targetSpaceCount = prefixLength + item.ErrorPosition.CharNumber;

                while (errorPrefix.Length < targetSpaceCount)
                {
                    errorPrefix = errorPrefix + " ";
                }

                string errorMsg = _errorTable.ContainsKey(item.ErrorCode)
                    ? _errorTable[item.ErrorCode]
                    : "Неизвестная ошибка";

                errorPrefix = errorPrefix + $"^ ошибка код {item.ErrorCode}: {errorMsg}";
                Console.WriteLine(errorPrefix);
            }
            _err.Clear();
        }

        public static void Error(byte errorCode, TextPosition position)
        {
            if (_err.Count <= ErrMax)
            {
                Err e = new Err(position, errorCode);
                _err.Add(e);
            }
        }
    }
}