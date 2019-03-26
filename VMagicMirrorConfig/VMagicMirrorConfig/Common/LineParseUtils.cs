using System;

namespace Baku.VMagicMirrorConfig
{
    static class LineParseUtils
    {
        public static bool TryReadIntParam(string line, string name, out int result)
        {
            string key = name + ":";
            if (
                line.StartsWith(key) &&
                int.TryParse(line.Substring(key.Length), out result)
                )
            {
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }

        public static bool TryReadIntParam(string line, string name, Action<int> actOnParsed)
        {
            if (TryReadIntParam(line, name, out int result))
            {
                actOnParsed(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryReadBoolParam(string line, string name, out bool result)
        {
            string key = name + ":";
            if (
                line.StartsWith(key) &&
                bool.TryParse(line.Substring(key.Length), out result)
                )
            {
                return true;
            }
            else
            {
                result = false;
                return false;
            }
        }

        public static bool TryReadBoolParam(string line, string name, Action<bool> act)
        {
            if (TryReadBoolParam(line, name, out bool result))
            {
                act(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TryReadStringParam(string line, string name, out string result)
        {
            string key = name + ":";
            if (line.StartsWith(key))
            {
                result = line.Substring(key.Length);
                return true;
            }
            else
            {
                result = "";
                return false;
            }
        }

        public static bool TryReadStringParam(string line, string name, Action<string> act)
        {
            if (TryReadStringParam(line, name, out string result))
            {
                act(result);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
