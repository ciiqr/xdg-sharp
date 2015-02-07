using System;
using System.IO;

namespace xdg
{
    public static class Utils
    {
        public static void makedirs(string path, Mono.Unix.Native.FilePermissions permissions=Mono.Unix.Native.FilePermissions.ALLPERMS)
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
    }
}

