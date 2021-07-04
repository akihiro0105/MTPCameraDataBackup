using MediaDevices;
using System;
using System.IO;
using System.Linq;

namespace DataCopy
{
    class Program
    {
        private static string targetDevice = "ILCE-7C";
        private static string targetDirectory = @"F:\Camera";
        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            args.ToList().ForEach(a => { Console.WriteLine(a); });
            if (args.Length != 2 && args.Length != 0)
            {
                Console.WriteLine("Set device name and directory path");
                return;
            }
            if (args.Length == 2)
            {
                targetDevice = args[0];
                targetDirectory = args[1];
            }
            var devices = MediaDevice.GetDevices().ToList();
            var device = devices.Where(item => item.FriendlyName.Equals(targetDevice)).FirstOrDefault();
            if (device == null)
            {
                Console.WriteLine("Connect device");
                return;
            }
            Console.WriteLine(device.FriendlyName);
            device.Connect();

            var root = device.GetRootDirectory();
            var rootInfo = device.GetDirectoryInfo(root.FullName);
            var storage = device.GetDirectories(rootInfo.FullName).First();
            var storageInfo = device.GetDirectoryInfo(storage);
            device.GetDirectories(storageInfo.FullName).ToList().ForEach(directory =>
            {
                var directoryInfo = device.GetDirectoryInfo(directory);
                Console.WriteLine(directoryInfo.FullName);
                var targetDirectoryInfo = Directory.CreateDirectory(Path.Combine(targetDirectory, directoryInfo.Name));

                device.GetFiles(directoryInfo.FullName).ToList().ForEach(files =>
                {
                    var filesInfo = device.GetFileInfo(files);
                    Console.WriteLine(" : " + filesInfo.FullName);
                    var targetFileInfo = Path.Combine(targetDirectoryInfo.FullName, filesInfo.Name);

                    if (!File.Exists(targetFileInfo))
                    {
                        using (var file = new FileStream(targetFileInfo, FileMode.Create, FileAccess.Write))
                        {
                            using (var stream = filesInfo.OpenRead())
                            {
                                var data = new byte[1024];
                                while (true)
                                {
                                    var count = stream.Read(data, 0, data.Length);
                                    if (count == 0) break;
                                    file.Write(data, 0, count);
                                }
                            }
                        }
                    }
                });
            });

            device.Disconnect();
        }
    }
}
