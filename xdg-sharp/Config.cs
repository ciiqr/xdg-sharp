// 
// Functions to configure Basic Settings
// 

using System;

namespace xdg
{
    public static class Config
    {
        public static bool Debug = false;

        // TODO: Change to properties

        public static string language = "C";
        public static string windowmanager = null;
        public static string icon_theme = "hicolor";
        public static int icon_size = 48;
        public static int cache_time = 5;
        public static bool root_mode = false;

        public static void SetWindowManager(string wm)
        {
            windowmanager = wm;
        }
        public static void SetIconTheme(string theme)
        {
            icon_theme = theme;
//            xdg.IconTheme.themes = []; // TODO: Implement
        }
        public static void SetIconSize(int size)
        {
            icon_size = size;
        }
        public static void SetCacheTime(int time)
        {
            cache_time = time;
        }
        public static void SetLocale(string lang)
        {
            // TODO: Implement

//            lang = locale.normalize(lang);
//            locale.setlocale(locale.LC_ALL, lang);
//            xdg.Locale.update(lang);
        }
        public static void SetRootMode(bool boolean)
        {
            root_mode = boolean;
        }

    }
}

