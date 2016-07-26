using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LineRipper
{
    public class Utilities
    {
        static Utilities()
        {
            var invalidChars = new string(Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray());
            _invalidRegex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
        }

        private static readonly Regex _invalidRegex;

        public static string RemoveInvalidPathChars(string input)
        {
            return _invalidRegex.Replace(input, "");
        }
    }
}
