namespace GoogleDriveConsole
{
	internal class Program
	{

		static async Task Main(string[] args)
		{
			using FileStream fs = new FileStream("myJson.json", FileMode.Open, FileAccess.Read);
			MyDriveService myService = new MyDriveService(fs);

			var files = myService.GetFilesByFolderName("測試共享資料夾").ToList();
			Console.WriteLine($"files Count = {files.Count}");
			foreach (var file in files)
			{
				Console.WriteLine($"{file.Name} start");
				using var fStream = new FileStream(file.Name, FileMode.OpenOrCreate);
				var downloadProcess = await myService.DownloadAsync(file.Id, fStream);
				Console.WriteLine($"{file.Name} {downloadProcess.Status}");
			}
			Console.WriteLine("完成");
			Console.ReadLine();
		}
	}
}
