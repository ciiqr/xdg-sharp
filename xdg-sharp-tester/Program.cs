using System;

namespace xdgsharptester
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Environment.UserName);
            Console.WriteLine(System.IO.Directory.Exists("/home/william"));
            Console.WriteLine(Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR_") ?? "null");
            Mono.Unix.Native.Syscall.mkdir("/home/william/etst", Mono.Unix.Native.FilePermissions.S_IRWXU);

            Console.WriteLine(xdg.BaseDirectory.SaveConfigPath("quick-launch"));
            Console.WriteLine(xdg.BaseDirectory.SaveCachePath("quick-launch"));
        }
    }
}
