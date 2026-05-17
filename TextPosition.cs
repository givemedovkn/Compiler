namespace Compiler
{
    public struct TextPosition
    {

        private uint _lineNumber;
        private byte _charNumber;

        public TextPosition(uint lineNumber = 0, byte charNumber = 0)
        {
            _lineNumber = lineNumber;
            _charNumber = charNumber;
        }
        public uint LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                _lineNumber = value;
            }
        }
        public byte CharNumber
        {
            get
            {
                return _charNumber;
            }
            set
            {
                _charNumber = value;
            }
        }
    }
}
