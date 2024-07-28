using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace GoogleDriveConsole
{
	public class MyDriveService
	{
		private DriveService _driveService;

		public Action<IDownloadProgress> DownloadProgressChanged;

		public MyDriveService(FileStream credentialJsonFs)
		{
			GoogleCredential credential = GoogleCredential.FromStream(credentialJsonFs).CreateScoped(new[] { DriveService.Scope.Drive });

			_driveService = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "_applicationName"
			});
			DownloadProgressChanged = progress => { };
		}

		public IEnumerable<Google.Apis.Drive.v3.Data.File> GetFilesByFolderName(string folderName)
		{
			var folders = GetFilesImpl($" trashed = false and mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' ");
			foreach (var folder in folders)
			{
				var result = GetFilesImpl($" trashed = false and mimeType != 'application/vnd.google-apps.folder' and parents in '{folder.Id}' ");
				foreach (var file in result)
					yield return file;
			}
		}

		private IEnumerable<Google.Apis.Drive.v3.Data.File> GetFilesImpl(string Q)
		{
			FilesResource.ListRequest req = _driveService.Files.List();
			req.PageSize = 1000;
			req.Q = Q;
			req.Fields = "nextPageToken, files(id, name,parents,mimeType,size,capabilities,modifiedTime,webViewLink,webContentLink)";
			FileList fileFeedList = req.Execute();

			while (fileFeedList != null)
			{
				foreach (Google.Apis.Drive.v3.Data.File file in fileFeedList.Files)
					yield return file;

				if (fileFeedList.NextPageToken == null)
					break;

				req.PageToken = fileFeedList.NextPageToken;
				fileFeedList = req.Execute();
			}
		}

		public async Task<Google.Apis.Download.IDownloadProgress> DownloadAsync(string Id, FileStream saveFileStream)
		{
			var req = _driveService.Files.Get(Id);
			req.MediaDownloader.ProgressChanged += DownloadProgressChanged;
			return await req.DownloadAsync(saveFileStream);
		}

		public async Task<Google.Apis.Download.IDownloadProgress> DownloadAsync(string Id, FileStream saveFileStream, int chunkSize)
		{
			var req = _driveService.Files.Get(Id);
			req.MediaDownloader.ProgressChanged += DownloadProgressChanged;
			req.MediaDownloader.ChunkSize = chunkSize;
			return await req.DownloadAsync(saveFileStream);
		}
	}
}
