// 
// The freedesktop.org Base Directory specification provides a way for
// applications to locate shared data and configuration:
// 
//     http://standards.freedesktop.org/basedir-spec/
// 
// (based on version 0.6)
// 

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace xdg
{
    public static class BaseDirectory
    {
        public static string _home;
        public static string xdg_data_home;
        public static string[] xdg_data_dirs;
        public static string xdg_config_home;
        public static string[] xdg_config_dirs;
        public static string xdg_cache_home;

        static BaseDirectory()
        {
            _home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            xdg_data_home = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(_home, ".local", "share");
            xdg_config_home = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(_home, ".config");
            xdg_cache_home = Environment.GetEnvironmentVariable("XDG_CACHE_HOME") ?? Path.Combine(_home, ".cache");

            var tempDataDirs = new List<string>();
            tempDataDirs.Add(xdg_data_home);
            tempDataDirs.AddRange((Environment.GetEnvironmentVariable("XDG_DATA_DIRS") ?? "/usr/local/share:/usr/share").Split(Path.PathSeparator));

            var tempConfigDirs = new List<string>();
            tempConfigDirs.Add(xdg_config_home);
            tempConfigDirs.AddRange((Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS") ?? "/etc/xdg").Split(Path.PathSeparator));

            // Remove empty paths
            tempDataDirs  .RemoveAll((p) => p.Length == 0); // TODO: What would happen if this was an async delegate
            tempConfigDirs.RemoveAll((p) => p.Length == 0);

            xdg_data_dirs = tempDataDirs.ToArray();
            xdg_config_dirs = tempConfigDirs.ToArray();
        }

        // TODO: Move to it's own class
        private static void makedirs(string path, Mono.Unix.Native.FilePermissions permissions=Mono.Unix.Native.FilePermissions.ALLPERMS)
        {
            string[] pathParts = path.Split(Path.PathSeparator);

            for (int i = 0; i < pathParts.Length; i++)
            {
                if (i > 0)
                    pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);

                if (!Directory.Exists(pathParts[i]))
                    Mono.Unix.Native.Syscall.mkdir(pathParts[i], permissions);
            }
        }

        public static string SaveConfigPath(params string[] resources)
        {
            // Ensure ``$XDG_CONFIG_HOME/<resource>/`` exists, and return its path.
            // "resource" should normally be the name of your application. Use this
            // when saving configuration settings.
            // 
            var resource = Path.Combine(resources);
            Debug.Assert(!resource.StartsWith("/"));
            var path = Path.Combine(xdg_config_home, resource);
            if (!Directory.Exists(path))
                makedirs(path, Mono.Unix.Native.FilePermissions.S_IRWXU);
            return path;
        }
        public static string SaveDataPath(params string[] resources)
        {
            // Ensure ``$XDG_DATA_HOME/<resource>/`` exists, and return its path.
            // "resource" should normally be the name of your application or a shared
            // resource. Use this when saving or updating application data.
            // 
            var resource = Path.Combine(resources);
            Debug.Assert(!resource.StartsWith("/"));
            var path = Path.Combine(xdg_data_home, resource);
            if (!Directory.Exists(path))
                makedirs(path);
            return path;
        }
        public static string SaveCachePath(params string[] resources)
        {
            // Ensure ``$XDG_CACHE_HOME/<resource>/`` exists, and return its path.
            // "resource" should normally be the name of your application or a shared
            // resource.
            var resource = Path.Combine(resources);
            Debug.Assert(!resource.StartsWith("/"));
            var path = Path.Combine(xdg_cache_home, resource);
            if (!Directory.Exists(path))
                makedirs(path);
            return path;
        }
        public static IEnumerable<string> LoadConfigPaths(params string[] resources)
        {
            // Returns an iterator which gives each directory named "resource" in the
            // configuration search path. Information provided by earlier directories should
            // take precedence over later ones, and the user-specific config dir comes
            // first.
            var resource = Path.Combine(resources);
            foreach (var config_dir in xdg_config_dirs)
            {
                var path = Path.Combine(config_dir, resource);
                if (System.IO.Directory.Exists(path))
                    yield return path;
            }
        }
        public static string LoadFirstConfig(params string[] resources)
        {
            // Returns the first result from load_config_paths, or None if there is nothing
            // to load.
            foreach (var x in LoadConfigPaths(resources))
                return x;
            return null;
        }
        public static IEnumerable<string> LoadDataPaths(params string[] resources)
        {
            // Returns an iterator which gives each directory named "resource" in the
            // application data search path. Information provided by earlier directories
            // should take precedence over later ones.
            var resource = Path.Combine(resources);
            foreach (var data_dir in xdg_data_dirs)
            {
                var path = Path.Combine(data_dir, resource);
                if (System.IO.Directory.Exists(path))
                    yield return path;
            }
        }
        public static string GetRuntimeDir(bool strict=true)
        {
            // Returns the value of $XDG_RUNTIME_DIR, a directory path.
            // 
            // This directory is intended for "user-specific non-essential runtime files
            // and other file objects (such as sockets, named pipes, ...)", and
            // "communication and synchronization purposes".
            // 
            // As of late 2012, only quite new systems set $XDG_RUNTIME_DIR. If it is not
            // set, with ``strict=True`` (the default), a KeyError is raised. With 
            // ``strict=False``, PyXDG will create a fallback under /tmp for the current
            // user. This fallback does *not* provide the same guarantees as the
            // specification requires for the runtime directory.
            // 
            // The strict default is deliberately conservative, so that application
            // developers can make a conscious decision to allow the fallback.
            // 
            var value = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");

            if (value == null)
            {
                if (strict)
                    throw new xdg.Exception("GetRuntimeDir: strict mode enabled, XDG_RUNTIME_DIR was null");

                value = "/tmp/pyxdg-runtime-dir-fallback-" + Environment.UserName;

                // int status = 
                Mono.Unix.Native.Syscall.mkdir(value, Mono.Unix.Native.FilePermissions.S_IRWXU);

                // TODO: Figure out how to make a directory and get the status as a Mono.Unix.Native.Errno
//                if (status == Mono.Unix.Native.Errno.EEXIST)
                // Already exists - set 700 permissions again.;
                Mono.Unix.Native.Syscall.chmod(value, Mono.Unix.Native.FilePermissions.S_IRWXU);
            }
            return value;
        }
    }
}

