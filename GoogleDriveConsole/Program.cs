using Microsoft.Extensions.Configuration;

namespace GoogleDriveConsole
{
	internal class Program
	{

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

			using FileStream fs = new FileStream(setting.GoogleCredentialFile, FileMode.Open, FileAccess.Read);
			MyDriveService myService = new MyDriveService(fs);

			Directory.CreateDirectory(setting.FileSaveFolder);
			var files = myService.GetFilesByFolderName(setting.GoogleFolderName).ToList();
			Console.WriteLine($"files Count = {files.Count}");
			foreach (var file in files)
			{
				Console.WriteLine($"{file.Name} start");
				string savePath = Path.Combine(setting.FileSaveFolder, file.Name);
				using var fStream = new FileStream(savePath, FileMode.OpenOrCreate);
				var downloadProcess = await myService.DownloadAsync(file.Id, fStream);
				Console.WriteLine($"{file.Name} {downloadProcess.Status}");
			}
			Console.WriteLine("完成");
			Console.ReadLine();
		}
	}
}
