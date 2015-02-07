// 
// Base Class for DesktopEntry, IconTheme and IconData
// 

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ContentGroupDictionaryType = System.Collections.Generic.Dictionary<string, string>;
using ContentDictionaryType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

// TODO: Switch all to string.IsNullOrEmpty()  Done?
using System.Text;

namespace xdg
{
    public class Point: Tuple<int, int>
    {
        public Point(int i1, int i2): base(i1, i2) {}
    }

    public class IniFile
    {
        public List<string> warnings;
        public List<string> errors;

        public string defaultGroup = "";
        public string fileExtension = "";
        public string filename = "";
        public bool tainted = false;
        public ContentDictionaryType content;

        public IniFile(string filename=null)
        {
            this.content = new ContentDictionaryType();
            if (!String.IsNullOrEmpty(filename))
                this.Parse(filename);
        }

        #region Equality
        public override bool Equals(Object obj) 
        {
            return obj is IniFile && this == (IniFile)obj;
        }
        public override int GetHashCode() 
        {
            return content.GetHashCode();
        }
        public static bool operator ==(IniFile thisObj, IniFile other)
        {
            return thisObj.content == other.content;
        }
        public static bool operator !=(IniFile thisObj, IniFile other)
        {
            return !(thisObj == other);
        }
        #endregion Equality

        public void Parse(string filename, List<string> headers=null)
        {
            // Parse an INI file.
            // 
            // headers -- list of headers the parser will try to select as a default header
            // 
            // for performance reasons
            StreamReader fd;
            var content = this.content;

            if (!File.Exists(filename))
                throw new ParsingError("File not found", filename);

            try
            {
                // The content should be UTF-8, but legacy files can have other
                // encodings, including mixed encodings in one file. We don"t attempt
                // to decode them, but we silence the errors.
                fd = new StreamReader(filename, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true); // TODO: Determine if this replaces invalidly encoded characters
            }
            catch (Exception)
            {
                if (Exception.Debug)
                    throw;
                else
                    return;
            }

            // parse file
            string currentGroup = null;
            var line = fd.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                // empty line
                if (String.IsNullOrEmpty(line))
                    continue;
                // comment
                else if (line[0] == '#')
                    continue;
                // new group
                else if (line[0] == '[')
                {
                    currentGroup = line.TrimStart('[').TrimEnd(']');
                    if (Exception.Debug && this.hasGroup(currentGroup))
                        throw new DuplicateGroupError(currentGroup, filename);
                    else
                        content[currentGroup] = new ContentGroupDictionaryType();
                }
                // key
                else
                {
                    string key, value;
                    try
                    {
                        var splitLine = line.Split("=".ToCharArray(), 1);
                        key = splitLine[0];
                        value = splitLine[1];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ParsingError("Invalid line: " + line, filename);
                    }

                    key = key.Trim(); // Spaces before/after "=" should be ignored
                    try
                    {
                        if (Exception.Debug && this.hasKey(key, currentGroup))
                            throw new DuplicateKeyError(key, currentGroup, filename);
                        else
                            content[currentGroup][key] = value.Trim();
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(KeyNotFoundException))
                            throw new ParsingError("Parsing error on key, group missing", filename);
                        else
                            throw;
                    }
                }
            }
            fd.Close();

            this.filename = filename;
            this.tainted = false;

            // check header
            if (headers != null)
            {
                bool setDefaultGroup = false;
                foreach (var header in headers)
                {
                    if (content.ContainsKey(header))
                    {
                        this.defaultGroup = header;
                        setDefaultGroup = true;
                        break;
                    }
                }
                if (!setDefaultGroup)
                    throw new ParsingError(String.Format("[{0}]-Header missing", headers[0]), filename);
            }
        }

        #region stuff to access the keys
        public type Get<type>(string key, string group=null, bool locale=false)
        {
            string val = null;
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;

            // return key (with locale)
            if (this.content.ContainsKey(group) && (this.content[group].ContainsKey(key)))
            {
                if (locale)
                    val = this.content[group][this.__addLocale(key, group)];
                else
                    val = this.content[group][key];
            }
            else
            {
                if (Exception.Debug)
                {
                    if (!this.content.ContainsKey(group))
                        throw new NoGroupError(group, this.filename);
                    else if (this.content[group].ContainsKey(key))
                        throw new NoKeyError(key, group, this.filename);
                }
                else
                    val = "";
            }

            var typ = typeof(type);

            object result;
            if (typ == typeof(bool))
                result = this.getBoolean(val);
            else if (typ == typeof(int))
            {
                try
                {
                    result = int.Parse(val);
                }
                catch (Exception)
                {
                    result = 0;
                }
            }
            else if (typ == typeof(double)) // TODO: Determine if this is the best...
            {
                try
                {
                    result = double.Parse(val);
                }
                catch (Exception)
                {
                    result = 0.0;
                }
            }
            else if (typ == typeof(Regex))
                result = new Regex(val);
            else if (typ == typeof(Point))
            {
                try
                {
                    var splitVal = val.Split(",".ToCharArray());
                    string x = splitVal[0];
                    string y = splitVal[1];

                    result = new Point(int.Parse(x), int.Parse(y));
                }
                catch (Exception)
                {
                    result = new Point(0, 0);
                }
            }
             else
                 result = default(type);

            return (type)result;
        }
        public List<type> GetAsList<type>(string key, string group=null, bool locale=false)
        {
            string value = null;
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;

            // return key (with locale)
            if (this.content.ContainsKey(group) && (this.content[group].ContainsKey(key)))
            {
                if (locale)
                    value = this.content[group][this.__addLocale(key, group)];
                else
                    value = this.content[group][key];
            }
            else
            {
                if (Exception.Debug)
                {
                    if (!this.content.ContainsKey(group))
                        throw new NoGroupError(group, this.filename);
                    else if (this.content[group].ContainsKey(key))
                        throw new NoKeyError(key, group, this.filename);
                }
                else
                    value = "";
            }

            var values = this.getList(value);
            var result = new List<type>();
            var typ = typeof(type);
            foreach (var val in values)
            {
                object valCheck;
                if (typ == typeof(bool))
                    valCheck = this.getBoolean(val);
                else if (typ == typeof(int))
                {
                    try
                    {
                        valCheck = int.Parse(val);
                    }
                    catch (Exception)
                    {
                        valCheck = 0;
                    }
                }
                else if (typ == typeof(double))
                {
                    try
                    {
                        valCheck = double.Parse(val);
                    }
                    catch (Exception)
                    {
                        valCheck = 0.0;
                    }
                }
                else if (typ == typeof(Regex))
                    valCheck = new Regex(val);
                else if (typ == typeof(Point))
                {
                    try
                    {
                        var splitVal = val.Split(",".ToCharArray());
                        string x = splitVal[0];
                        string y = splitVal[1];

                        valCheck = new Point(int.Parse(x), int.Parse(y));
                    }
                    catch (Exception)
                    {
                        valCheck = new Point(0, 0);
                    }
                }
                else
                    valCheck = default(type);

                result.Add((type)valCheck);
            }

            return result;
        }

        #endregion stuff to access the keys

        #region subget
        public List<string> getList(string value) // TODO: Change to an array
        {
            List<string> list;
            if (Regex.IsMatch(@"(?<!\\)\;", value))
                list = new List<string>(Regex.Split(@"(?<!\\);", value));
            else if (Regex.IsMatch(@"(?<!\\)\|", value))
                list = new List<string>(Regex.Split(@"(?<!\\)\|", value));
            else if (Regex.IsMatch(@"(?<!\\),", value))
                list = new List<string>(Regex.Split(@"(?<!\\),", value));
            else
                list = new List<string>{value};

            if (list[list.Count - 1] == "")
                list.RemoveAt(list.Count - 1);
            return list;
        }

        public bool getBoolean(string boolean)
        {
            if (boolean == "1" || boolean.ToLower() == "true")
                return true;
            else if (boolean == "0" || boolean.ToLower() == "false")
                return false; // TODO: If this is all we're doing then that seems a bit odd to duplicate
            return false;
        }
        #endregion subget

        public string __addLocale(string key, string group=null)
        {
            // add locale to key according the current lc_messages
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;

            foreach (var lang in xdg.Locale.langs)
            {
                var langkey = String.Format("{0}[{1}]", key, lang);
                if (this.content[group].ContainsKey(langkey))
                    return langkey;
            }

            return key;
        }

        // start validation stuff
        public void validate(string report="All")
        {
            // Validate the contents, raising ``ValidationError`` if there
            // is anything amiss.
            // 
            // report can be "All" / "Warnings" / "Errors"
            // 

            this.warnings = new List<string>();
            this.errors = new List<string>();

            // Get file extension
            this.fileExtension = Path.GetExtension(this.filename);

            // overwrite this for own checkings
            this.checkExtras();

            // check all keys
            foreach (var group in this.content)
            {
                this.checkGroup(group.Key);
                foreach (var item in group.Value)
                {
                    this.checkKey(item.Key, item.Value, group.Key);
                    // check if value is empty
                    if (item.Value == "")
                        this.warnings.Add(String.Format("Value of Key '{0}' is empty", item.Key));
                }
            }

            // raise Warnings / Errors
            var msg = "";
            var sb = new StringBuilder();

            if (report == "All" || report == "Warnings") // TODO: Change to Enum
                foreach (var line in this.warnings)
                    sb.Append("\n- " + line);

            if (report == "All" || report == "Errors")
            {
                foreach (var line in this.errors)
                    sb.Append("\n- " + line);
            }

            msg = sb.ToString();
            if (!String.IsNullOrEmpty(msg))
                throw new ValidationError(msg, this.filename);
        }

        // check if group header is valid
        virtual public void checkGroup(string group)
        {
            return;
        }

        // check if key is valid
        virtual public void checkKey(string key, string value, string group)
        {
            return;
        }

        // check random stuff
        public void checkValue(string key, string value, string type="string", bool list=false)
        {
            List<string> values;
            if (list)
                values = this.getList(value);
            else
                values = new List<string> {value};

            foreach (var val in values)
            {
                int code = 0;
                if (type == "string")
                    code = this.checkString(val);

                if (type == "localestring")
                    continue;
                else if (type == "boolean")
                    code = this.checkBoolean(val);
                else if (type == "numeric")
                    code = this.checkNumber(val);
                else if (type == "integer")
                    code = this.checkInteger(val);
                else if (type == "regex")
                    code = this.checkRegex(val);
                else if (type == "point")
                    code = this.checkPoint(val);

                if (code == 1)
                    this.errors.Add(String.Format("'{0}' is not a valid {1}", val, type));
                else if (code == 2)
                    this.warnings.Add(String.Format("Value of key '{0}' is deprecated", key));
            }
        }

        virtual public void checkExtras()
        {
            return;
        }

        public int checkBoolean(string value) // TODO: Change return to enum (CheckStatus { Valid=0, Invalid, Deprecated})
        {
            // 1 or 0 : deprecated
            if (value == "1" || value == "0")
                return 2;
            // true or false: ok
            else if (!(value == "true" || value == "false")) // TODO: I think this is wrong, if these values are okay, then why 
                return 1;
            return 0;
        }

        public int checkNumber(string value)
        {
            // TODO: Change to TryParse, 
            try
            {
                double.Parse(value);
            }
            catch (Exception)
            {
                return 1;
            }
            return 0;
        }

        public int checkInteger(string value)
        {
            // int() ValueError
            try
            {
                int.Parse(value);
            }
            catch (Exception)
            {
                return 1;
            }
            return 0;
        }
        public int checkPoint(string value)
        {
            if (!Regex.IsMatch("^[0-9]+,[0-9]+$", value))
                return 1;
            return 0;
        }

        public int checkString(string value)
        {
            return (value.IsASCII()) ? 0 : 1;
        }

        public int checkRegex(string value)
        {
            try
            {
                new Regex(value);
            }
            catch (Exception)
            {
                return 1;
            }
            return 0;
        }

        // write support
        public void write(string filename=null, bool trusted=false)
        {
            if (String.IsNullOrEmpty(filename) && String.IsNullOrEmpty(this.filename))
                throw new ParsingError("File not found", "");

            if (!String.IsNullOrEmpty(filename))
                this.filename = filename;
            else
                filename = this.filename;

            var dirname = filename;
            if (dirname[dirname.Length - 1] != Path.DirectorySeparatorChar)
                dirname += Path.DirectorySeparatorChar;
                
            if ((!String.IsNullOrEmpty(filename)) && (!Directory.Exists(dirname)))
                Utils.makedirs(dirname);
            using (var fp = new StreamWriter(filename, append: false, encoding: System.Text.Encoding.UTF8))
            {
                // An executable bit signifies that the desktop file is
                // trusted, but then the file can be executed. Add hashbang to
                // make sure that the file is opened by something that
                // understands desktop files.
                if (trusted)
                    fp.Write("#!/usr/bin/env xdg-open\n");

                if (String.IsNullOrEmpty(this.filename))
                {
                    fp.Write(String.Format("[{0}]\n", this.defaultGroup));
                    foreach (var item in this.content[this.defaultGroup])
                    {
                        var key = item.Key;
                        var value = item.Value;
                        fp.Write(String.Format("{0}={1}\n", key, value));
                        fp.Write("\n");
                    }
                }
                foreach (var item in this.content)
                {
                    var name = item.Key;
                    var group = item.Value;

                    if (name != this.defaultGroup)
                    {
                        fp.Write(String.Format("[{0}]\n", name));
                        foreach (var item2 in group)
                        {
                            var key = item2.Key;
                            var value = item2.Value;
                            fp.Write(String.Format("{0}={1}\n", key, value));
                        }
                        fp.Write("\n");
                    }
                }
            }

            // Add executable bits to the file to show that it"s trusted.
            if (trusted)
            {
                Mono.Unix.Native.Stat stats;
                Mono.Unix.Native.Syscall.stat(filename, out stats);
                var mode = stats.st_mode | Mono.Unix.Native.FilePermissions.S_IXUSR | Mono.Unix.Native.FilePermissions.S_IXGRP | Mono.Unix.Native.FilePermissions.S_IXOTH;
                Mono.Unix.Native.Syscall.chmod(filename, mode);
            }

            this.tainted = false;
        }

        public void Set(string key, string value, string group=null, bool locale=false)
        {
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;

            if (locale && xdg.Locale.langs.Length > 0)
                key = key + "[" + xdg.Locale.langs[0] + "]";

            try
            {
                this.content[group][key] = value;
            }
            catch (KeyNotFoundException)
            {
                throw new NoGroupError(group, this.filename);
            }

            this.tainted = (value == this.Get<string>(key, group));
        }

        public void addGroup(string group)
        {
            if (this.hasGroup(group))
            if (Exception.Debug)
                throw new DuplicateGroupError(group, this.filename);
            else
            {
                this.content[group] = new ContentGroupDictionaryType();
                this.tainted = true;
            }
        }

        public bool removeGroup(string group)
        {
            var existed = this.content.ContainsKey(group);
            if (existed)
            {
                this.content.Remove(group);
                this.tainted = true;
            }
            else
            {
                if (Exception.Debug)
                    throw new NoGroupError(group, this.filename);
            }
            return existed;
        }

        public string removeKey(string key, string group=null, bool locales=true)
        {
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;
            var lastKey = "";
            try
            {
                if (locales)
                {
                    foreach (var name in this.content[group].Keys)
                    {
                        if (Regex.IsMatch("^" + key + xdg.Locale.regex + "$", name) && name != key)
                            this.content[group].Remove(name);
                    }
                }
                lastKey = group;
                var value = this.content[group][key];
                this.content[group].Remove(key);
                this.tainted = true;
                return value;
            }
            catch (KeyNotFoundException)
            {
                if (Exception.Debug)
                {
                    if (lastKey == group)
                        throw new NoGroupError(group, this.filename);
                    else
                        throw new NoKeyError(key, group, this.filename);
                }
                else
                    return "";
            }
            }

        // misc
        public IEnumerable<string> groups()
        {
            return this.content.Keys;
        }

        public bool hasGroup(string group)
        {
            return this.content.ContainsKey(group);
        }

        public bool hasKey(string key, string group=null)
        {
            // set default group
            if (String.IsNullOrEmpty(group))
                group = this.defaultGroup;

            return this.content[group].ContainsKey(key);
        }

        public string getFileName()
        {
            return this.filename;
        }
    }

}

