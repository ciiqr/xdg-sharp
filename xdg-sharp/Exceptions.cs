//
// Exception Classes for the xdg package
// 

using System;

namespace xdg
{
    class Exception: System.Exception
    {
        public static bool Debug = false; // TODO: Move to config
        public Exception(string message) : base(message) { }
    }
    class ValidationError: Exception
    {
        public string file;

        public ValidationError(string message, string file): base(String.Format("ValidationError in file '{0}': {1} ", file, message))
        {
            this.file = file;
        }
    }
    class ParsingError: Exception
    {
        public string file;

        public ParsingError(string message, string file): base(String.Format("ParsingError in file '{0}', {1}", file, message))
        {
            this.file = file;
        }
    }
    class NoKeyError: Exception
    {
        public string key;
        public string group;

        public NoKeyError(string key, string group, string file): base(String.Format("No key '{0}' in group {1} of file {2}", key, group, file))
        {
            this.key = key;
            this.group = group;
        }
    }
    class DuplicateKeyError: Exception
    {
        public string key;
        public string group;

        public DuplicateKeyError(string key, string group, string file): base(String.Format("Duplicate key '{0}' in group {1} of file {2}", key, group, file))
        {
            this.key = key;
            this.group = group;
        }
    }
    class NoGroupError: Exception
    {
        public string group;

        public NoGroupError(string group, string file): base(String.Format("No group: {0} in file {1}", group, file))
        {
            this.group = group;
        }
    }
    class DuplicateGroupError: Exception
    {
        public string group;

        public DuplicateGroupError(string group, string file): base(String.Format("Duplicate group: {0} in file {1}", group, file))
        {
            this.group = group;
        }
    }
    class NoThemeError: Exception
    {
        public string theme;

        public NoThemeError(string theme): base(String.Format("No such icon-theme: {0}", theme))
        {
            this.theme = theme;
        }
    }
}