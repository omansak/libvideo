// Code from YoutubeExplode (LGPL https://github.com/Tyrrrz/YoutubeExplode/blob/master/License.txt)
using System.Text;

namespace VideoLibrary.CipherOperations
{
    internal class SwapCipherOperation : ICipherOperation
    {
        private readonly int _index;

        public SwapCipherOperation(int index)
        {
            _index = index;
        }

        public string Decipher(string input)
        {
            var sb = new StringBuilder(input)
            {
                [0] = input[_index],
                [_index] = input[0]
            };
            return sb.ToString();
        }
    }
}
