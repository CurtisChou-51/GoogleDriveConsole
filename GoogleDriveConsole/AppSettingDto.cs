namespace GoogleDriveConsole
{
    public class AppSettingDto
    {
        public string GoogleCredentialFile { get; set; } = string.Empty;

        public string[] GoogleFolderPath { get; set; } = [];

        public string FileSaveFolder { get; set; } = string.Empty;
    }
}
