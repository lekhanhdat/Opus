using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.RACommonUtility.Common;
using Azure.Storage.Sas;
using Ionic.Zip;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

namespace RAMultiGeo.SyncCommonData
{
    public class AzureBlobStorageManager
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(AzureBlobStorageManager));
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageManager(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public async Task<string> ZipAndUploadFolder(string folderPath, string password, string tenantId, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
            }

            string zipFilePath = Path.Combine(folderPath, $"{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip");

            try
            {
                CreatePasswordProtectedZip(folderPath, zipFilePath, password);

                string blobName = $"{tenantId}/{Path.GetFileName(zipFilePath)}";
                var (realBlobName, _) = AzureUtil.UploadStorageBlobWithFolderContainName(_containerName, blobName, zipFilePath, "MultiGeo",_connectionString, true, true, CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage));
                
                return await GenerateSasUri(realBlobName);
            }
            finally
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        public async Task<string> DownloadAndExtractSQLiteFile(string downloadUrl, string extractFolderPath)
        {
            if (!Directory.Exists(extractFolderPath))
            {
                Directory.CreateDirectory(extractFolderPath);
            }

            string zipFilePath = Path.Combine(extractFolderPath, $"download_{DateTime.UtcNow:yyyyMMddHHmmss}.zip");

            try
            {
                await DownloadFileFromUrl(downloadUrl, zipFilePath);
                return ExtractSQLiteFromZip(zipFilePath, extractFolderPath);
            }
            finally
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        private async Task DownloadFileFromUrl(string downloadUrl, string destinationPath)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(downloadUrl);

            response.EnsureSuccessStatusCode();

            using var fileStream = File.Create(destinationPath);
            await response.Content.CopyToAsync(fileStream);
        }

        private string ExtractSQLiteFromZip(string zipFilePath, string extractFolderPath)
        {
            using var zip = ZipFile.Read(zipFilePath);

            var sqliteEntry = zip.Entries.FirstOrDefault(e => e.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
            if (sqliteEntry == null)
            {
                throw new FileNotFoundException("No SQLite (.db) file found in the zip archive.");
            }

            sqliteEntry.Extract(extractFolderPath, ExtractExistingFileAction.OverwriteSilently);

            return Path.Combine(extractFolderPath, sqliteEntry.FileName);
        }

        private void CreatePasswordProtectedZip(string folderPath, string zipFilePath, string password)
        {
            using var zip = new ZipFile();
            //zip.Password = password;
            //zip.Encryption = EncryptionAlgorithm.WinZipAes256;

            zip.AddDirectory(folderPath, Path.GetFileName(folderPath));
            zip.Save(zipFilePath);
        }

        private async Task<string> GenerateSasUri(string blobName)
        {
            string connectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
            var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
            AzureBlobStorage azureBlobStorage = new AzureBlobStorage(connectionString, containerName);
            if (await azureBlobStorage.CheckBlobExistAsync(blobName))
            {
                var sasUri = Util.MSAzure.StorageUtil.GenerateSasUriForRead(connectionString, containerName, blobName, TimeSpan.FromDays(7));
                Logger.Info("Finish Create File SAS");
                return sasUri;
            }
            else
            {
                throw new Exception($"Can not find blob, blobName:{blobName}.");
            }
        }
    }
}
