using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;

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

        public IEnumerable<File> GetFilesByFolderName(string folderName)
        {
            var topFolder = GetTopFolderByName(folderName);
            return topFolder == null ? [] : GetSubFiles(topFolder);
        }

        public IEnumerable<File> GetFilesByFolderPath(string[] folderPath)
        {
            var targetFolder = GetFolderByFolderPath(folderPath);
            return targetFolder == null ? [] : GetSubFiles(targetFolder);
        }

        private File? GetFolderByFolderPath(string[] folderPath)
        {
            string folderName = folderPath.First();
            var topFolder = GetTopFolderByName(folderName);
            if (topFolder == null)
                return null;

            int i = 1;
            var subFolder = topFolder;
            do
            {
                if (i == folderPath.Length)
                    return subFolder;
                subFolder = GetSubFolders(subFolder).FirstOrDefault(x => x.Name == folderPath[i]);
                i++;
            } while (subFolder != null);
            return subFolder;
        }

        /// <summary> 取得最上層資料夾 </summary>
        private File? GetTopFolderByName(string folderName)
        {
            return GetFilesImpl($" trashed = false and mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' ")
                .FirstOrDefault(x => x.Parents == null);
        }

        /// <summary> 取得資料夾下的檔案 </summary>
        private IEnumerable<File> GetSubFiles(File folder)
        {
            return GetFilesImpl($" trashed = false and mimeType != 'application/vnd.google-apps.folder' and parents in '{folder.Id}' ");
        }

        /// <summary> 取得資料夾下的資料夾 </summary>
        private IEnumerable<File> GetSubFolders(File folder)
        {
            return GetFilesImpl($" trashed = false and mimeType = 'application/vnd.google-apps.folder' and parents in '{folder.Id}' ");
        }

        private IEnumerable<File> GetFilesImpl(string Q)
        {
            FilesResource.ListRequest req = _driveService.Files.List();
            req.PageSize = 1000;
            req.Q = Q;
            req.Fields = "nextPageToken, files(id, name,parents,mimeType,size,capabilities,modifiedTime,webViewLink,webContentLink)";
            FileList fileFeedList = req.Execute();

            while (fileFeedList != null)
            {
                foreach (File file in fileFeedList.Files)
                    yield return file;

                if (fileFeedList.NextPageToken == null)
                    break;

                req.PageToken = fileFeedList.NextPageToken;
                fileFeedList = req.Execute();
            }
        }

        public async Task<IDownloadProgress> DownloadAsync(string Id, FileStream saveFileStream)
        {
            var req = _driveService.Files.Get(Id);
            req.MediaDownloader.ProgressChanged += DownloadProgressChanged;
            return await req.DownloadAsync(saveFileStream);
        }

        public async Task<IDownloadProgress> DownloadAsync(string Id, FileStream saveFileStream, int chunkSize)
        {
            var req = _driveService.Files.Get(Id);
            req.MediaDownloader.ProgressChanged += DownloadProgressChanged;
            req.MediaDownloader.ChunkSize = chunkSize;
            return await req.DownloadAsync(saveFileStream);
        }
    }
}
