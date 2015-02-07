// 
// Complete implementation of the XDG Desktop Entry Specification Version 0.9.4
// http://standards.freedesktop.org/desktop-entry-spec/
// 
// Not supported:
// - Encoding: Legacy Mixed
// - Does not check exec parameters
// - Does not check URL"s
// - Does not completly validate deprecated/kde items
// - Does not completly check categories
// 

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace xdg
{
    public class DesktopEntry: IniFile
    {
        public static string DEFAULT_GROUP = "Desktop Entry";

        public string type;
        public string name;

        public DesktopEntry(string filename=null)
        {
            // Create a new DesktopEntry
            // 
            // If filename exists, it will be parsed as a desktop entry file. If not,
            // or if filename is None, a blank DesktopEntry is created.
            // 
            this.content = new Dictionary<string, Dictionary<string, string>>();
            if (filename != null)
            {
                if (System.IO.File.Exists(filename))
                    this.Parse(filename);
                else
                    this.reuse(filename);
            }
        }

        override public string ToString()
        {
            return this.GetName();
        }

        public void Parse(string file)
        {
            // Parse a desktop entry file.
            base.Parse(file, new List<string>(){"Desktop Entry", "KDE Desktop Entry"});
        }

        #region standard keys
        public string GetType_() // NOTE: Unfortunatly objects have a GetType method already...
        {
            return this.Get<string>("Type");
        }
        public string GetVersionString()
        {
            return this.Get<string>("Version");
        }
        public string GetName()
        {
            return this.Get<string>("Name", locale:true);
        }
        public string GetGenericName()
        {
            return this.Get<string>("GenericName", locale:true);
        }
        public bool GetNoDisplay()
        {
            return this.Get<bool>("NoDisplay");
        }
        public string GetComment()
        {
            return this.Get<string>("Comment", locale:true);
        }
        public string GetIcon()
        {
            return this.Get<string>("Icon", locale:true);
        }
        public bool GetHidden()
        {
            return this.Get<bool>("Hidden");
        }
        public List<string> GetOnlyShowIn()
        {
            return this.GetAsList<string>("OnlyShowIn");
        }
        public List<string> GetNotShowIn()
        {
            return this.GetAsList<string>("NotShowIn");
        }
        public string GetTryExec()
        {
            return this.Get<string>("TryExec");
        }
        public string GetExec()
        {
            return this.Get<string>("Exec");
        }
        public string GetPath()
        {
            return this.Get<string>("Path");
        }
        public bool GetTerminal()
        {
            return this.Get<bool>("Terminal");
        }
        public List<string> GetMimeTypes()
        {
            return this.GetAsList<string>("MimeType");
        }
        public List<string> GetCategories()
        {
            return this.GetAsList<string>("Categories");
        }
        public bool GetStartupNotify()
        {
            return this.Get<bool>("StartupNotify");
        }
        public string GetStartupWMClass()
        {
            return this.Get<string>("StartupWMClass");
        }
        public string GetURL()
        {
            return this.Get<string>("URL");
        }
        #endregion standard keys

        #region kde keys
        public List<string> GetServiceTypes()
        {
            return this.GetAsList<string>("ServiceTypes");
        }
        public string GetDocPath()
        {
            return this.Get<string>("DocPath");
        }
        public List<string> GetKeywords()
        {
            return this.GetAsList<string>("Keywords", locale:true);
        }
        public string GetInitialPreference()
        {
            return this.Get<string>("InitialPreference");
        }
        public string GetDev()
        {
            return this.Get<string>("Dev");
        }
        public string GetFSType()
        {
            return this.Get<string>("FSType");
        }
        public string GetMountPoint()
        {
            return this.Get<string>("MountPoint");
        }
        public bool GetReadonly()
        {
            return this.Get<bool>("ReadOnly");
        }
        public string GetUnmountIcon()
        {
            return this.Get<string>("UnmountIcon", locale:true);
        }
        #endregion kde keys

        #region deprecated keys
        public string GetMiniIcon()
        {
            return this.Get<string>("MiniIcon", locale:true);
        }
        public string GetTerminalOptions()
        {
            return this.Get<string>("TerminalOptions");
        }
        public string GetDefaultApp()
        {
            return this.Get<string>("DefaultApp");
        }
        public List<string> GetProtocols()
        {
            return this.GetAsList<string>("Protocols");
        }
        public List<string> GetExtensions()
        {
            return this.GetAsList<string>("Extensions");
        }
        public string GetBinaryPattern()
        {
            return this.Get<string>("BinaryPattern");
        }
        public string GetMapNotify()
        {
            return this.Get<string>("MapNotify");
        }
        public string GetEncoding()
        {
            return this.Get<string>("Encoding");
        }
        public string GetSwallowTitle()
        {
            return this.Get<string>("SwallowTitle", locale:true);
        }
        public string GetSwallowExec()
        {
            return this.Get<string>("SwallowExec");
        }
        public List<string> GetSortOrder()
        {
            return this.GetAsList<string>("SortOrder");
        }
        public Regex GetFilePattern()
        {
            return this.Get<Regex>("FilePattern");
        }
        public List<string> GetActions()
        {
            return this.GetAsList<string>("Actions");
        }
        #endregion deprecated keys

        #region desktop entry edit
        public void reuse(string filename) // NOTE: Was called 'new' in python
        {
            // Make this instance into a new desktop entry.
            // 
            // If filename has a .desktop extension, Type is set to Application. If it
            // has a .directory extension, Type is Directory.
            // 
            string type;
            if (Path.GetExtension(filename) == ".desktop")
                type = "Application";
            else if (Path.GetExtension(filename) == ".directory")
                type = "Directory";
            else
                throw new ParsingError("Unknown extension", filename);

            this.content = new Dictionary<string, Dictionary<string, string>>();
            this.addGroup(DEFAULT_GROUP);
            this.Set("Type", type);
            this.filename = filename;
        }
        #endregion desktop entry edit

        #region validation stuff
        override public void checkExtras()
        {
            // header
            if (DEFAULT_GROUP == "KDE Desktop Entry")
                this.warnings.Add("[KDE Desktop Entry]-Header is deprecated");

            // file extension
            if (this.fileExtension == ".kdelnk")
                this.warnings.Add("File extension .kdelnk is deprecated");
            else if (this.fileExtension != ".desktop" && this.fileExtension != ".directory")
                this.warnings.Add("Unknown File extension");

            // Type
            try
            {
                this.type = this.content[DEFAULT_GROUP]["Type"];
            }
            catch (KeyNotFoundException)
            {
                this.errors.Add("Key 'Type' is missing");
            }

            // Name
            try
            {
                this.name = this.content[DEFAULT_GROUP]["Name"];
            }
            catch (KeyNotFoundException)
            {
                this.errors.Add("Key 'Name' is missing");
            }
        }
        override public void checkGroup(string group)
        {
            // check if group header is valid
            if (group == DEFAULT_GROUP
                || Regex.IsMatch("^Desktop Action [a-zA-Z0-9\\-]+$", group)
                || (Regex.IsMatch("^X-", group) && group.IsASCII()))
            {
                if (this.content[group].ContainsKey("OnlyShowIn") &&
                    this.content[group].ContainsKey("NotShowIn"))
                    this.errors.Add("Group may either have OnlyShowIn or NotShowIn, but not both");
            }
            else
                this.errors.Add("Invalid Group name: " + group);
        }
        override public void checkKey(string key, string value, string group)
        {
            // standard keys     
            if (key == "Type")
            {
                if (value == "ServiceType" || value == "Service" || value == "FSDevice")
                    this.warnings.Add(String.Format("Type={0} is a KDE extension", key));
                else if (value == "MimeType")
                    this.warnings.Add("Type=MimeType is deprecated");
                else if (!(value == "Application" || value == "Link" || value == "Directory"))
                    this.errors.Add(String.Format("Value of key 'Type' must be Application, Link or Directory, but is '{0}'", value));

                if (this.fileExtension == ".directory" && !(value == "Directory"))
                    this.warnings.Add(String.Format("File extension is .directory, but Type is '{0}'", value));
                else if (this.fileExtension == ".desktop" && value == "Directory")
                    this.warnings.Add("Files with Type=Directory should have the extension .directory");

                if (value == "Application")
                if (!this.content[group].ContainsKey("Exec"))
                    this.warnings.Add("Type=Application needs 'Exec' key");
                if (value == "Link")
                if (!this.content[group].ContainsKey("URL"))
                    this.warnings.Add("Type=Link needs 'URL' key");
            }
            else if (key == "Version")
            {
                this.checkValue(key, value);
            }
            else if (Regex.IsMatch("^Name" + xdg.Locale.regex + "$", key))
            {
                ; // locale string
            }
            else if (Regex.IsMatch("^GenericName" + xdg.Locale.regex + "$", key))
            {
                ; // locale string
            }
            else if (key == "NoDisplay")
            {
                this.checkValue(key, value, type = "boolean");
            }
            else if (Regex.IsMatch("^Comment" + xdg.Locale.regex + "$", key))
            {
                ; // locale string
            }

            else if (Regex.IsMatch("^Icon" + xdg.Locale.regex + "$", key))
                this.checkValue(key, value);
            else if (key == "Hidden")
                this.checkValue(key, value, type = "boolean");
            else if (key == "OnlyShowIn")
            {
                this.checkValue(key, value, list: true);
                this.checkOnlyShowIn(value);
            }
            else if (key == "NotShowIn")
            {
                this.checkValue(key, value, list: true);
                this.checkOnlyShowIn(value);
            }
            else if (key == "TryExec")
            {
                this.checkValue(key, value);
                this.checkType(key, "Application");
            }
            else if (key == "Exec")
            {
                this.checkValue(key, value);
                this.checkType(key, "Application");
            }
            else if (key == "Path")
            {
                this.checkValue(key, value);
                this.checkType(key, "Application");
            }
            else if (key == "Terminal")
            {
                this.checkValue(key, value, type = "boolean");
                this.checkType(key, "Application");
            }
            else if (key == "Actions")
            {
                this.checkValue(key, value, list: true);
                this.checkType(key, "Application");
            }
            else if (key == "MimeType")
            {
                this.checkValue(key, value, list: true);
                this.checkType(key, "Application");
            }
            else if (key == "Categories")
            {
                this.checkValue(key, value);
                this.checkType(key, "Application");
                this.checkCategories(value);
            }
            else if (Regex.IsMatch("^Keywords" + xdg.Locale.regex + "$", key))
            {
                this.checkValue(key, value, type = "localestring", list: true);
                this.checkType(key, "Application");
            }
            else if (key == "StartupNotify")
            {
                this.checkValue(key, value, type = "boolean");
                this.checkType(key, "Application");
            }
            else if (key == "StartupWMClass")
            {
                this.checkType(key, "Application");
            }
            else if (key == "URL")
            {
                this.checkValue(key, value);
                this.checkType(key, "URL");
            }

            // kde extensions
            else if (key == "ServiceTypes")
            {
                this.checkValue(key, value, list: true);
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "DocPath")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "InitialPreference")
            {
                this.checkValue(key, value, type = "numeric");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "Dev")
            {
                this.checkValue(key, value);
                this.checkType(key, "FSDevice");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "FSType")
            {
                this.checkValue(key, value);
                this.checkType(key, "FSDevice");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "MountPoint")
            {
                this.checkValue(key, value);
                this.checkType(key, "FSDevice");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (key == "ReadOnly")
            {
                this.checkValue(key, value, type = "boolean");
                this.checkType(key, "FSDevice");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }
            else if (Regex.IsMatch("^UnmountIcon" + xdg.Locale.regex + "$", key))
            {
                this.checkValue(key, value);
                this.checkType(key, "FSDevice");
                this.warnings.Add(String.Format("Key '{0}' is a KDE extension", key));
            }

            // deprecated keys
            else if (key == "Encoding")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (Regex.IsMatch("^MiniIcon" + xdg.Locale.regex + "$", key))
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "TerminalOptions")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "DefaultApp")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "Protocols")
            {
                this.checkValue(key, value, list: true);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "Extensions")
            {
                this.checkValue(key, value, list: true);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "BinaryPattern")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "MapNotify")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (Regex.IsMatch("^SwallowTitle" + xdg.Locale.regex + "$", key))
            {
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "SwallowExec")
            {
                this.checkValue(key, value);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "FilePattern")
            {
                this.checkValue(key, value, type = "regex", list: true);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }
            else if (key == "SortOrder")
            {
                this.checkValue(key, value, list: true);
                this.warnings.Add(String.Format("Key '{0}' is deprecated", key));
            }

            // "X-" extensions
            else if (Regex.IsMatch("^X-[a-zA-Z0-9-]+", key))
            {
                ;
            }

            else
            {
                this.errors.Add(String.Format("Invalid key: {0}", key));
            }
        }
        public void checkType(string key, string type)
        {
            if (this.GetType_() != type)
                this.errors.Add(String.Format("Key '{0}' only allowed in Type={1}", key, type));
        }
        public void checkOnlyShowIn(string value)
        {
            List<string> values = this.getList(value);
            var valid = new String[] {"GNOME", "KDE", "LXDE", "MATE", "Razor", "ROX", "TDE", "Unity", "XFCE", "Old"};
            foreach (var item in values)
            {
                if ((Array.IndexOf(valid, item) == -1) && (item.Substring(0, 2) != "X-"))
                    this.errors.Add(String.Format("'{0}' is not a registered OnlyShowIn value", item));
            }
        }
        public void checkCategories(string value)
        {
            var values = this.getList(value);

            var main = new String[] {"AudioVideo", "Audio", "Video", "Development", "Education", "Game", "Graphics", "Network", "Office", "Science", "Settings", "System", "Utility"};

            // TODO: Determine if this is exactly what is mean by "if not any(item in main for item in values):"
            bool allValuesInMain = true;
            foreach (var item in values)
            {
                if (Array.IndexOf(main, item) == -1)
                {
                    allValuesInMain = false;
                    break;
                }
            }

            if (allValuesInMain)
                this.errors.Add("Missing main category");

            var additional = new String[] {"Building", "Debugger", "IDE", "GUIDesigner", "Profiling", "RevisionControl", "Translation", "Calendar", "ContactManagement", "Database", "Dictionary", "Chart", "Email", "Finance", "FlowChart", "PDA", "ProjectManagement", "Presentation", "Spreadsheet", "WordProcessor", "2DGraphics", "VectorGraphics", "RasterGraphics", "3DGraphics", "Scanning", "OCR", "Photography", "Publishing", "Viewer", "TextTools", "DesktopSettings", "HardwareSettings", "Printing", "PackageManager", "Dialup", "InstantMessaging", "Chat", "IRCClient", "Feed", "FileTransfer", "HamRadio", "News", "P2P", "RemoteAccess", "Telephony", "TelephonyTools", "VideoConference", "WebBrowser", "WebDevelopment", "Midi", "Mixer", "Sequencer", "Tuner", "TV", "AudioVideoEditing", "Player", "Recorder", "DiscBurning", "ActionGame", "AdventureGame", "ArcadeGame", "BoardGame", "BlocksGame", "CardGame", "KidsGame", "LogicGame", "RolePlaying", "Shooter", "Simulation", "SportsGame", "StrategyGame", "Art", "Construction", "Music", "Languages", "ArtificialIntelligence", "Astronomy", "Biology", "Chemistry", "ComputerScience", "DataVisualization", "Economy", "Electricity", "Geography", "Geology", "Geoscience", "History", "Humanities", "ImageProcessing", "Literature", "Maps", "Math", "NumericalAnalysis", "MedicalSoftware", "Physics", "Robotics", "Spirituality", "Sports", "ParallelComputing", "Amusement", "Archiving", "Compression", "Electronics", "Emulator", "Engineering", "FileTools", "FileManager", "TerminalEmulator", "Filesystem", "Monitor", "Security", "Accessibility", "Calculator", "Clock", "TextEditor", "Documentation", "Adult", "Core", "KDE", "GNOME", "XFCE", "GTK", "Qt", "Motif", "Java", "ConsoleOnly"};

            // Merge arrays
            var allcategories = new String[additional.Length + main.Length];
            Array.Copy(additional, allcategories, additional.Length); // TODO: Determine if this is right because stack overflow answer I got it from was wrong
            Array.Copy(main, 0, allcategories, additional.Length, main.Length);

            foreach (var item in values)
            {
                if ((Array.IndexOf(allcategories, item) == -1) && (!item.StartsWith("X-")))
                    this.errors.Add(String.Format("'{0}' is not a registered Category", item));
            }
        }
        #endregion validation stuff
    }
}

