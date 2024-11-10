using Google.Apis.Download;
using Microsoft.Extensions.Configuration;

namespace GoogleDriveConsole
{
    internal class Program
    {
        private const int chunkSize = 5 * 1024 * 1024;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
            IConfiguration _configuration = builder.Build();
            AppSettingDto? setting = _configuration.Get<AppSettingDto>();
            if (setting == null)
            {
                Console.WriteLine("setting配置錯誤");
                Console.ReadLine();
                return;
            }

            MyDriveService myService = InitMyDriveService(setting.GoogleCredentialFile);

            Directory.CreateDirectory(setting.FileSaveFolder);
            var files = myService.GetFilesByFolderName(setting.GoogleFolderName).ToList();
            Console.WriteLine($"files Count = {files.Count}");
            foreach (var file in files)
            {
                Console.WriteLine($"{file.Name} start");
                string savePath = Path.Combine(setting.FileSaveFolder, file.Name);
                using var fStream = new FileStream(savePath, FileMode.OpenOrCreate);
                var downloadProcess = await myService.DownloadAsync(file.Id, fStream, chunkSize);
                Console.WriteLine($"{file.Name} {downloadProcess.Status}");
            }
            Console.WriteLine("完成");
            Console.ReadLine();
        }

        private static MyDriveService InitMyDriveService(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            MyDriveService myService = new MyDriveService(fs);
            myService.DownloadProgressChanged = (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        Console.WriteLine($"Downloaded {progress.BytesDownloaded / 1024} KB");
                        break;
                    case DownloadStatus.Completed:
                        Console.WriteLine("Download complete.");
                        break;
                    case DownloadStatus.Failed:
                        Console.WriteLine("Download failed.");
                        break;
                }
            };
            return myService;
        }
    }
}
