/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AvePoint.GCommon;
using Azure.Storage.Blobs.Specialized;
using Newtonsoft.Json;
using RAArchiverCommon;

namespace HSMAzureCommon
{
    public delegate void UpdateItemDelegate(int count);
    public class QuickXorHash : HashAlgorithm
    {
        private const int BitsInLastCell = 32;
        private const byte Shift = 11;
        private const int Threshold = 600;
        private const byte WidthInBits = 160;

        private UInt64[] _data;
        private Int64 _lengthSoFar;
        private int _shiftSoFar;

        public QuickXorHash()
        {
            this.Initialize();
        }

        public override sealed void Initialize()
        {
            this._data = new ulong[(QuickXorHash.WidthInBits - 1) / 64 + 1];
            this._shiftSoFar = 0;
            this._lengthSoFar = 0;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            unchecked
            {
                int currentShift = this._shiftSoFar;

                // The bitvector where we'll start xoring
                int vectorArrayIndex = currentShift / 64;

                // The position within the bit vector at which we begin xoring
                int vectorOffset = currentShift % 64;
                int iterations = Math.Min(cbSize, QuickXorHash.WidthInBits);

                for (int i = 0; i < iterations; i++)
                {
                    bool isLastCell = vectorArrayIndex == this._data.Length - 1;
                    int bitsInVectorCell = isLastCell ? QuickXorHash.BitsInLastCell : 64;

                    // There's at least 2 bitvectors before we reach the end of the array
                    if (vectorOffset <= bitsInVectorCell - 8)
                    {
                        for (int j = ibStart + i; j < cbSize + ibStart; j += QuickXorHash.WidthInBits)
                        {
                            this._data[vectorArrayIndex] ^= (ulong)array[j] << vectorOffset;
                        }
                    }
                    else
                    {
                        int index1 = vectorArrayIndex;
                        int index2 = isLastCell ? 0 : (vectorArrayIndex + 1);
                        byte low = (byte)(bitsInVectorCell - vectorOffset);

                        byte xoredByte = 0;
                        for (int j = ibStart + i; j < cbSize + ibStart; j += QuickXorHash.WidthInBits)
                        {
                            xoredByte ^= array[j];
                        }
                        this._data[index1] ^= (ulong)xoredByte << vectorOffset;
                        this._data[index2] ^= (ulong)xoredByte >> low;
                    }
                    vectorOffset += QuickXorHash.Shift;
                    while (vectorOffset >= bitsInVectorCell)
                    {
                        vectorArrayIndex = isLastCell ? 0 : vectorArrayIndex + 1;
                        vectorOffset -= bitsInVectorCell;
                    }
                }

                // Update the starting position in a circular shift pattern
                this._shiftSoFar = (this._shiftSoFar + QuickXorHash.Shift * (cbSize % QuickXorHash.WidthInBits)) % QuickXorHash.WidthInBits;
            }

            this._lengthSoFar += cbSize;
        }

        protected override byte[] HashFinal()
        {
            // Create a byte array big enough to hold all our data
            byte[] rgb = new byte[(QuickXorHash.WidthInBits - 1) / 8 + 1];

            // Block copy all our bitvectors to this byte array
            for (Int32 i = 0; i < this._data.Length - 1; i++)
            {
                Buffer.BlockCopy(
                BitConverter.GetBytes(this._data[i]), 0,
                rgb, i * 8,
                8);
            }

            Buffer.BlockCopy(
            BitConverter.GetBytes(this._data[this._data.Length - 1]), 0,
            rgb, (this._data.Length - 1) * 8,
            rgb.Length - (this._data.Length - 1) * 8);

            // XOR the file length with the least significant bits
            // Note that GetBytes is architecture-dependent, so care should
            // be taken with porting. The expected value is 8-bytes in length in little-endian format
            var lengthBytes = BitConverter.GetBytes(this._lengthSoFar);
            System.Diagnostics.Debug.Assert(lengthBytes.Length == 8);
            for (int i = 0; i < lengthBytes.Length; i++)
            {
                rgb[(QuickXorHash.WidthInBits / 8) - lengthBytes.Length + i] ^= lengthBytes[i];
            }

            return rgb;
        }

        public override int HashSize
        {
            get
            {
                return QuickXorHash.WidthInBits;
            }
        }
    }

    public class FileHash
    {
        public string MD5Hash { get; set; }
        public string Checksum { get; set; }
        public string IV { get; set; }
        public bool LargeFile { get; set; }
    }

    public class AzureCommonWrapper
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        int PreObjectsProcessed = 0;

        public Dictionary<string, bool> UploadAzureFilesStatusDic = new Dictionary<string, bool>();
        //public Dictionary<string, FileHash> UploadFileHashDic = [];

        public event UpdateItemDelegate itemChanged;

        private int chunkSize = 1024 * 1024;

        public static long LimitFileSize = (long)250 * 1024 * 1024; // 250MB according to RECO-34471

        private readonly int length = 260;

        private static AzureCommonWrapper wrapper;
        public static AzureCommonWrapper Instance
        {
            get
            {
                if (wrapper == null)
                {
                    wrapper = new AzureCommonWrapper();
                }
                return wrapper;
            }
        }

        internal AzureResult UploadToAzure(AzureUploadSetting azureInfo)
        {
            AzureResult result = new AzureResult();
            Uri url = new Uri(azureInfo.AzureSetting.AccessPoint);
            bool useHttps = url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? true : false;
            string endPointSuffixm = url.DnsSafeHost;
            endPointSuffixm = endPointSuffixm.Substring(endPointSuffixm.IndexOf(".", StringComparison.OrdinalIgnoreCase) + 1);
            string accountKey = azureInfo.AzureSetting.AccountKey;
            using (AzureBlobManager manager = new AzureBlobManager(useHttps, endPointSuffixm, azureInfo.AzureSetting.AccountName, accountKey))
            {
                manager.SetBlobRequestOptions(azureInfo.BlobRequestOptionsServerTimeoutHour, azureInfo.BlobRequestOptionsServerTimeoutMinute, azureInfo.BlobRequestOptionsClientTimeoutHour, azureInfo.BlobRequestOptionsClientTimeoutMinute);
                if (manager.LoginBlob())
                {
                    if (manager.UploadMutipleFilesToAzure(azureInfo.ExportLocation, azureInfo.IsEncryption))
                    {
                        WinAzure azure = manager.GetImportToken(azureInfo.SourceContainerName, azureInfo.MainfestContainerName, azureInfo.QueueContainName, azureInfo.LifeTime);
                        result.AzureIused = azure.AzureIused;
                        result.AzureContainerManifestUri = azure.AzureContainerManifestUri;
                        result.AzureContainerSourceUri = azure.AzureContainerSourceUri;
                        result.AzureManifestContainerName = azure.AzureManifestContainerName;
                        result.AzureQueueReportContainerName = azure.AzureQueueReportContainerName;
                        result.AzureQueueReportUri = azure.AzureQueueReportUri;
                        result.AzureSourceContainerName = azure.AzureSourceContainerName;
                    }
                }
            }
            return result;
        }

        public AzureResult GetAzureContainerToken(AzureUploadSetting azureInfo, AzureBlobManager manager)
        {
            AzureResult result = new AzureResult();
            mLog.Info("begin get container token,thread id {0}.", Thread.CurrentThread.ManagedThreadId);
            try
            {
                manager.SetBlobRequestOptions(azureInfo.BlobRequestOptionsServerTimeoutHour, azureInfo.BlobRequestOptionsServerTimeoutMinute, azureInfo.BlobRequestOptionsClientTimeoutHour, azureInfo.BlobRequestOptionsClientTimeoutMinute);
                manager.SetQueueRequestOptions(azureInfo.BlobRequestOptionsServerTimeoutHour, azureInfo.BlobRequestOptionsServerTimeoutMinute);
                WinAzure azure = manager.GetImportToken(azureInfo.SourceContainerName, azureInfo.MainfestContainerName, azureInfo.QueueContainName, azureInfo.LifeTime);
                result.AzureIused = azure.AzureIused;
                result.AzureContainerManifestUri = azure.AzureContainerManifestUri;
                result.AzureContainerSourceUri = azure.AzureContainerSourceUri;
                result.AzureManifestContainerName = azure.AzureManifestContainerName;
                result.AzureQueueReportContainerName = azure.AzureQueueReportContainerName;
                result.AzureQueueReportUri = azure.AzureQueueReportUri;
                result.AzureSourceContainerName = azure.AzureSourceContainerName;
            }
            catch (Exception e)
            {
                mLog.Info("An error occurred get container token.Exception:{0}", e.ToString());
                result.AzureIused = false;
                result.ErrorMessage = e.ToString();
            }
            mLog.Info("End get container token.");
            return result;

        }

        public Boolean DeleteAzureContainer(AzureUploadSetting azureInfo, List<string> BlobcontainNameList, List<string> QueueContainNameList)
        {
            mLog.Info("begin Delete those containers.Thread:{0}", Thread.CurrentThread.ManagedThreadId);

            Uri url = new Uri(azureInfo.AzureSetting.AccessPoint);
            try
            {
                bool useHttps = url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? true : false;
                string endPointSuffixm = url.DnsSafeHost;
                endPointSuffixm = endPointSuffixm.Substring(endPointSuffixm.IndexOf(".", StringComparison.OrdinalIgnoreCase) + 1);
                string accountKey = azureInfo.AzureSetting.AccountKey;
                using (AzureBlobManager manager = new AzureBlobManager(useHttps, endPointSuffixm, azureInfo.AzureSetting.AccountName, accountKey))
                {
                    manager.SetBlobRequestOptions(azureInfo.BlobRequestOptionsServerTimeoutHour, azureInfo.BlobRequestOptionsServerTimeoutMinute, azureInfo.BlobRequestOptionsClientTimeoutHour, azureInfo.BlobRequestOptionsClientTimeoutMinute);
                    manager.SetQueueRequestOptions(azureInfo.BlobRequestOptionsServerTimeoutHour, azureInfo.BlobRequestOptionsServerTimeoutMinute);
                    if (manager.LoginBlob())
                    {
                        foreach (string containerName in BlobcontainNameList)
                        {
                            try
                            {
                                manager.DeleteBlobContainer(containerName);

                            }
                            catch (Exception ex)
                            {
                                mLog.Warn(" delete blob container {0} is failed,exception:{1}", containerName, ex.ToString());
                            }
                        }
                        foreach (string containerName in QueueContainNameList)
                        {
                            try
                            {
                                manager.DeleteQueueContainer(containerName);

                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("delete queue container {0} is failed,exception:{1}", containerName, ex.ToString());
                            }
                        }
                        return true;
                    }
                    else
                    {
                        mLog.Warn(" login Azure Failed");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Info("An error occurred while delete container from Azure.Thread Id:{0}.Exception:", Thread.CurrentThread.ManagedThreadId, e.ToString());
                return false;
            }
            finally
            {
                mLog.Info("end Delete those containers.Thread:{0}", Thread.CurrentThread.ManagedThreadId);

            }
        }

        public Boolean VerifyAndDownloadAzureFile(AuzreDownLoadSetting azureInfo, AzureBlobManager manager)
        {
            bool result = false;
            try
            {
                if (manager.VerifyAzureData(azureInfo.MainfestContainerName, (DownloadFileType)azureInfo.FileDonwloadType))
                {
                    result = manager.DownloadMutipleFiles(azureInfo.ExportLocation, azureInfo.MainfestContainerName, (DownloadFileType)azureInfo.FileDonwloadType, azureInfo.IsEncryption, azureInfo.NeedDelete);
                }
            }
            catch (Exception e)
            {
                mLog.Info("An error occurred while Check file on the Azure.Thread Id:{0}, Exception:{1}", Thread.CurrentThread.ManagedThreadId, e.ToString());
                result = false;
            }
            finally
            {
                mLog.Info("end Check file on the Azure,FileType:{0}, result:{1},Thread:{2}", azureInfo.ExportLocation, result, Thread.CurrentThread.ManagedThreadId);
            }
            return result;
        }

        public void WaitJobFinishedAction(string msg)
        {
            try
            {
                AzureQueueMessage queueMessage = JsonUtility.DeserializerFromJson<AzureQueueMessage>(msg);
                int ObjectsProcessed = 0;
                int.TryParse(queueMessage.FilesCreated, out ObjectsProcessed);
                if (ObjectsProcessed > PreObjectsProcessed)
                {
                    itemChanged(ObjectsProcessed - PreObjectsProcessed);
                    //ProgressCouner.IncJobProgress(ObjectsProcessed - PreObjectsProcessed);
                    PreObjectsProcessed = ObjectsProcessed;
                }
            }
            catch (Exception e)
            {
                mLog.Debug("An error occurred while De-serialize the queue message,exception:{0}", e);
            }
        }

        #region Support Encryption API

        public void EncryptContainerFile(string filePath, byte[] key, byte[] IV, out string md5Hash, out string checksum, BlockBlobClient blob = null)
        {
            byte[] bytesEncrypted = new byte[] { };
            byte[] bytesToBeEncrypted = new byte[] { };
            checksum = string.Empty;
            md5Hash = string.Empty;
            try
            {
                if (blob != null)
                {
                    EncryptContentAndUpload(filePath, key, IV, blob, out md5Hash, out checksum);
                    var metadataList = new Dictionary<string, string>
                    {
                        {"IV",  Convert.ToBase64String(IV)}
                    };

                    //if (!string.IsNullOrEmpty(md5Hash))
                    //{
                    //    blob.Properties.ContentMD5 = md5Hash;
                    //    blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentHash = Convert.FromBase64String(md5Hash) }).Wait();
                    //}

                    var task = blob.SetMetadataAsync(metadataList);
                    if (task != null)
                    {
                        task.Wait();
                        if (task.IsCompleted && task.Exception != null)
                        {
                            throw task.Exception;
                        }
                    }
                    mLog.Info("Encrypt file contents {0}", filePath);
                }
                else
                {
                    if (Path.GetExtension(filePath).Contains(".dat"))
                    {
                        AzureCommonWrapper.Instance.AES_EncryptContent(filePath, key, IV);
                        mLog.Info("Encrypt file contents {0}", Path.GetFileName(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Debug("Encrypt file contents failed {0}", ex.ToString());
            }
            finally
            {
                if (bytesEncrypted != null)
                {
                    bytesEncrypted = null;
                }
                if (bytesToBeEncrypted != null)
                {
                    bytesToBeEncrypted = null;
                }
            }
        }

        //public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] key, byte[] IV)
        //{

        //    // Set your salt here, change it to meet your flavor:
        //    // The salt bytes must be at least 8 bytes.
        //    Byte[] Cryptograph = null; // 加密后的密文  
        //    Aes Aes = Aes.Create();
        //    Aes.Mode = CipherMode.CBC;
        //    try
        //    {
        //        // 开辟一块内存流  
        //        using (MemoryStream Memory = new MemoryStream())
        //        {
        //            // 把内存流对象包装成加密流对象  
        //            using (CryptoStream Encryptor = new CryptoStream(Memory,
        //             Aes.CreateEncryptor(key, IV),
        //             CryptoStreamMode.Write))
        //            {
        //                // 明文数据写入加密流  
        //                Encryptor.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
        //                Encryptor.FlushFinalBlock();

        //                Cryptograph = Memory.ToArray();
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        mLog.Debug("AES_Encrypt file failed {0}", ex.ToString());
        //        Cryptograph = null;
        //    }
        //    return Cryptograph;
        //}

        public void AES_EncryptContent(string decryptedFileName, byte[] key, byte[] IV)
        {
            Byte[] Cryptograph = null; // 加密后的密文  
            byte[] chunkData = null;
            string tempFilePath = string.Format("{0}{1}", decryptedFileName, ".temp");
            //string tempFilePath = string.Format("{0}{1}{2}", AveEnv.AgentTempFolder, Guid.NewGuid(), ".temp");
            bool isSuccessfulEncrypt = false;
            try
            {
                using (Aes Aes = Aes.Create())
                {
                    Aes.Mode = CipherMode.CBC;
                    using (FileStream Memory = new FileStream(tempFilePath, FileMode.Create))
                    {
                        using (CryptoStream Encryptor = new CryptoStream(Memory, Aes.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                        {
                            using (FileStream fsInput = System.IO.File.OpenRead(decryptedFileName))
                            {
                                for (long i = 0; i < fsInput.Length; i += chunkSize)
                                {
                                    int readCount = chunkSize;
                                    if (fsInput.Length - i < chunkSize)
                                    {
                                        chunkData = new byte[fsInput.Length - i];
                                        readCount = (int)(fsInput.Length - i);
                                    }
                                    else
                                    {
                                        chunkData = new byte[chunkSize];
                                    }
                                    int bytesRead = fsInput.Read(chunkData, 0, readCount);
                                    Encryptor.Write(chunkData, 0, bytesRead);
                                    chunkData = null;
                                }
                            }
                            Encryptor.FlushFinalBlock();
                        }
                    }
                }
                isSuccessfulEncrypt = true;
            }
            catch (Exception ex)
            {
                mLog.Error("AES_EncryptContent {0} Error :{1}", decryptedFileName, ex.ToString());
            }
            finally
            {
                if (isSuccessfulEncrypt)
                {
                    File.Copy(tempFilePath, decryptedFileName, true);
                }
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private void EncryptContentAndUpload(string filePath, byte[] key, byte[] IV, BlockBlobClient blob, out string md5Hash, out string checksum)
        {
            checksum = string.Empty;
            md5Hash = string.Empty;
            Byte[] Cryptograph = null; // 加密后的密文  
           
            byte[] chunkData = null;
            var separator = Path.DirectorySeparatorChar;
            var tempSize = 0L;

            string tempFilePath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp.TrimEnd(separator), Guid.NewGuid() + ".temp");
            try
            {
                using (Aes Aes = Aes.Create())
                {
                    Aes.Mode = CipherMode.CBC;
                    using (FileStream Memory = new FileStream(tempFilePath, FileMode.Create))
                    {
                        using (CryptoStream Encryptor = new CryptoStream(Memory, Aes.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                        {
                            var fsInput = this.Open(filePath, FileMode.Open, FileAccess.Read);
                            tempSize = fsInput.Length;
                            if (tempSize >= LimitFileSize)
                            {
                                // handle request for large file ? timout, retry,....
                                checksum = GetChecksum(fsInput);
                                fsInput.Seek(0, SeekOrigin.Begin);
                            }
                            using (fsInput)
                            {
                                for (long i = 0; i < fsInput.Length; i += chunkSize)
                                {
                                    int readCount = chunkSize;
                                    if (fsInput.Length - i < chunkSize)
                                    {
                                        chunkData = new byte[fsInput.Length - i];
                                        readCount = (int)(fsInput.Length - i);
                                    }
                                    else
                                    {
                                        chunkData = new byte[chunkSize];
                                    }
                                    int bytesRead = fsInput.Read(chunkData, 0, readCount);
                                    Encryptor.Write(chunkData, 0, bytesRead);
                                    chunkData = null;
                                }
                            }
                            Encryptor.FlushFinalBlock();
                            Memory.Position = 0;
                            if (tempSize >= LimitFileSize)
                            {
                                md5Hash = GetMD5Value(Memory);
                                Memory.Seek(0, SeekOrigin.Begin);
                            }

                            var task = blob.UploadAsync(Memory);
                            if (task != null)
                            {
                                task.Wait();
                                if (task.IsCompleted && task.Exception != null)
                                {
                                    throw task.Exception;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("EncryptContentAndUpload {0} Failed : {1}", filePath, ex.ToString());
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

        }

        public string GetChecksum(Stream stream)
        {
            try
            {
                var qx = new QuickXorHash();
                var hashBytes = qx.ComputeHash(stream);
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occured while getting Quick Hash. EX: {ex}");
            }
            return string.Empty;
        }

        public string GetMD5Value(Stream stream)
        {
            try
            {
                //var test = false;
                //if (test)
                //{
                //    var oldHash1 = Convert.ToBase64String(HashAlgorithm.Create("MD5").ComputeHash(stream));
                //    stream.Seek(0, SeekOrigin.Begin);
                //    var oldHash2 = Convert.ToBase64String(MD5.Create().ComputeHash(stream));
                //    stream.Seek(0, SeekOrigin.Begin);
                //}
                return Convert.ToBase64String(MD5.HashData(stream));
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occured while getting MD5 Hash. EX: {ex}");
            }
            return string.Empty;
        }

        public string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return plaintext;
        }

        public Rfc2898DeriveBytes GenerateTempKey()
        {
            var saltBytes = new byte[16];
            RandomNumberGenerator.Create().GetBytes(saltBytes);
            using (var sha = SHA256.Create())
            {
                return new Rfc2898DeriveBytes(sha.ComputeHash(Encoding.UTF8.GetBytes("avepoint")), saltBytes, 100_000, HashAlgorithmName.SHA256);
            }
        }

        public byte[] CreateIV()
        {
            using (Aes Aes = Aes.Create())
            {
                Aes.GenerateIV();
                return Aes.IV;
            }
        }
        #endregion

        #region Support long path 
        public FileStream Open(string path, FileMode mode, FileAccess access)
        {
            path = AlphaUtility.FromNetShareToAlpha(path, length);
            if (path.Length >= length)
            {
                return new FileInfo(path).Open(FileMode.Open, FileAccess.Read);
            }
            else
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
        }
        #endregion

    }

    public class JsonUtility
    {
        public static string SerializerFromJson(object item)
        {

            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(item, setting);
        }

        public static T DeserializerFromJson<T>(string value)
        {

            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(value, setting);
        }
    }

    public class AlphaUtility
    {

        private static readonly string alphaHeader = @"\\?\UNC";

        public static string FromNetShareToAlpha(string path, int lengthLimit)
        {
            //In version 2.0 AlphaFS.dll need @"\\?\UNC"

            // path = ChangePath(path);

            //bool isLocal = CheckIsLocalOrUNC(path);

            if (path.Length >= lengthLimit)
            {
                return alphaHeader + "\\" + path;
            }
            else
            {
                return path;
            }

        }
    }
}

