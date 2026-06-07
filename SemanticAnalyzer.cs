internal class SemanticAnalyzer
{
    private static Dictionary<string, DataType> _symbolTable = 
        new Dictionary<string, DataType>();

    public static void AddVariable(string name, DataType type, TextPosition pos)
    {
        if (_symbolTable.ContainsKey(name))
        {
            InputOutput.Error(pos, 101);
        }
        else
        {
            _symbolTable[name] = type;
        }
    }

    public static DataType GetVariableType(string name, TextPosition pos)
    {
        if (_symbolTable.TryGetValue(name, out DataType type))
        {
            return type;
        }

        InputOutput.Error(pos, 102);
        return DataType.Unknown;
    }
}

