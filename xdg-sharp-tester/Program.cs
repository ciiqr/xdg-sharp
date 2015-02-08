using System;

namespace xdgsharptester
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var entry = new xdg.DesktopEntry("/home/william/.local/share/applications/chrome-fdmmgilgnpjigdojojpjoooidkmcomcm-Default.desktop");

            Console.WriteLine(entry.GetName() ?? "null");
            // Console.WriteLine(entry.getFileName());
            Console.WriteLine(entry.GetIcon() ?? "null");

//            Console.WriteLine(xdg.BaseDirectory.SaveConfigPath("quick-launch"));
//            Console.WriteLine(xdg.BaseDirectory.SaveCachePath("quick-launch"));
//
//            Console.WriteLine(xdg.BaseDirectory.xdg_cache_home);
//            Console.WriteLine(xdg.BaseDirectory.xdg_cache_home);

//            Console.WriteLine((int)Mono.Unix.Native.FilePermissions.S_IXOTH);

//            if ("/var"["/var".Length -1] != System.IO.Path.DirectorySeparatorChar)
//            Console.WriteLine(System.IO.Path.GetDirectoryName("/var"));
//            Console.WriteLine(System.IO.Path.GetDirectoryName("/var/"));
//            Console.WriteLine(System.IO.Path.GetDirectoryName("/var/lib"));
//            Console.WriteLine(System.IO.Path.GetDirectoryName("/var/lib/"));

//            MergeArrays();
        }

        public static void MergeArrays()
        {
            var main = new String[] {"AudioVideo", "Audio", "Video"};
            var additional = new String[] {"Building", "Debugger", "IDE", "GUIDesigner", "Profiling"};

            // Merge arrays
            var allcategories = new String[additional.Length + main.Length];
            Array.Copy(additional, allcategories, additional.Length); // TODO: Determine if this is right because stack overflow answer I got it from was wrong
            Array.Copy(main, 0, allcategories, additional.Length, main.Length);
            foreach (var item in allcategories)
                Console.WriteLine(item);
        }
    }
}
