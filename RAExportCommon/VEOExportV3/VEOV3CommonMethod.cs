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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.Wrapper.Backup;
using RAExportCommon.VEOExportV2;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static RAExportCommon.RecordVEOClassV3;
using ZipUtil = AvePoint.GCommon.ZipUtil;

namespace RAExportCommon
{
    public static class VEOV3CommonMethod
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static IExportSettingsDao ExportSettingsDao => PlatformWindsorManager.GetService<IExportSettingsDao>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();


        public static string AddValueNewLine(string content)
        {
            return string.Format("{0}{1}{0}", "\r\n", content);
        }

        public static bool HasVEOV3Permission()
        {
            return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.VEOV3) && TenantService.IsNewOpusTenant();
        }

        public static bool HasUpgradedVEOV3()
        {
            return RMKeyValueDao.HasUpgradeVEOV3() && HasVEOV3Permission();
        }

        public static XmlSerializerNamespaces AddXMLNS(RDFTemplate template)
        {
            var xmlns = new XmlSerializerNamespaces();
            switch (template)
            {
                case RDFTemplate.AglsFromVERS2:
                    xmlns.Add("dcterms", "http://purl.org/dc/terms/");
                    xmlns.Add("aglsterms", "http://www.agls.gov.au/agls/terms/");
                    xmlns.Add("versterms", "http://www.prov.vic.gov.au/vers/terms/");
                    xmlns.Add("dcam", "http://purl.org/dcam/");
                    break;
                case RDFTemplate.Agent:
                case RDFTemplate.Record:
                case RDFTemplate.Business:
                case RDFTemplate.Mandate:
                case RDFTemplate.Relationship:
                    xmlns.Add("anzs5478", "http://www.prov.vic.gov.au/ANZS5478");
                    break;
                default:
                    break;
            }
            return xmlns;
        }

        public static CacheSettingDto GenerateCacheSettings(string jobId)
        {
            string mainFolder = jobId.Contains('_') ? jobId.Remove(jobId.IndexOf('_', StringComparison.OrdinalIgnoreCase)) : jobId;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), "ArchiverCache", "VEO", mainFolder),
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            return new CacheSettingDto
            {
                Extension = new CacheSettingExtension { Path = new List<PathMap>() { new PathMap() { DiskInfo = disk } } },
                LimitFreeSpace = 1024 * 1024 * 1024,//1 GB
            };
        }

        public static void CleanCache(string systemCachePath)
        {
            if (Directory.Exists(systemCachePath))
            {
                Directory.Delete(systemCachePath, true);
                return;
            }
            if (File.Exists(systemCachePath))
            {
                File.Delete(systemCachePath);
            }
        }

        public static SignatureBlock BuildVEOSignature(byte[] content)
        {
            var mSignatureBlock = new SignatureBlock();
            mSignatureBlock.Version = VEOV3CommonString.VEO_VERSION;
            mSignatureBlock.SignatureAlgorithm = VEOV3CommonString.ALGORITHMID_SHA512WITHRSA;
            mSignatureBlock.SignatureDateTime = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
            mSignatureBlock.Signer = VEOV3CommonString.SIGNER;
            byte[] bytes = SHA512WithRSASignature.Signature(content);
            mSignatureBlock.Signature = AddValueNewLine(Convert.ToBase64String(bytes));
            mSignatureBlock.CertificateChain = new string[1];
            mSignatureBlock.CertificateChain[0] = AddValueNewLine(Convert.ToBase64String(AveCertificateOperation.ExportCertificateWithCertFormat()));
            return mSignatureBlock;
        }

        public static void ExportVEOReadmeFile(string destinationPath)
        {
            try
            {
                string content = VEOV3CommonString.VEO_README_CONTENT;
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    content += Environment.NewLine;
                }
                byte[] contentBytes = Encoding.GetEncoding(1252).GetBytes(content);
                mLog.Info($"Begin export {VEOV3CommonString.VEOReadme}, content length [{content.Length}], content bytes length[{contentBytes.Length}].");
                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(contentBytes, 0, contentBytes.Length);
                    fileStream.Flush();
                    mLog.Info($"Export {VEOV3CommonString.VEOReadme} into cache succeed, length [{fileStream.Length}].");
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occurred while export {VEOV3CommonString.VEOReadme} into cache. Error: {ex.Message}.");
            }
        }

        public static ArchiverSetting GetExportArchiverSetting(SourceFlag sourceFlag)
        {
            ArchiverSetting setting = new ArchiverSetting();
            var exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.VEO, (int)sourceFlag);
            if (exportSetting != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(exportSetting.ArchiverSetting);
                setting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                setting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
            }
            else
            {
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", VEOV3CommonString.VEOV3TemplateZipFile);
                var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", Path.GetFileNameWithoutExtension(VEOV3CommonString.VEOV3TemplateZipFile));
                AvePoint.GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(sr.ReadToEnd());
                        setting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                        setting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
                    }
                }
            }
            return setting;
        }

        public static string GetWebappUrl(AveSPSite aveSite)
        {
            Uri webAppUri = new Uri(aveSite.SPSite.Url);
            string webAppUrl;
            string siteUrl = aveSite.SPSite.Url;
            int lengh = 0;
            if (siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                lengh = "https://".Length;
            }
            else
            {
                lengh = "http://".Length;
            }
            int indexOfSlash = siteUrl.IndexOf("/", lengh, StringComparison.OrdinalIgnoreCase);
            webAppUrl = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppUrl = siteUrl.Substring(0, indexOfSlash);
            }
            webAppUri = new Uri(webAppUrl);
            return webAppUri.AbsoluteUri.Trim('/');
        }

        public static bool IsNotContainValue(object? obj)
        {
            if (obj == null) return true;
            if (obj is IEnumerable<string> list) return !list.Any(d => !string.IsNullOrWhiteSpace(d));
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && (p.GetIndexParameters().Length == 0) && !p.Name.Equals("parsetype", StringComparison.OrdinalIgnoreCase));

            bool hasData = properties.Any(p =>
            {
                var value = p.GetValue(obj);

                if (value == null) return false;
                if (p.PropertyType == typeof(string)) return !string.IsNullOrWhiteSpace(value?.ToString());
                if (value is IEnumerable<string> list) return list.Any(d => !string.IsNullOrWhiteSpace(d));
                //if (value is IEnumerable<object> objectList) return objectList.Any(child => !IsNotContainValue(child));
                return !IsNotContainValue(value);
            });

            return !hasData;
        }

        public static dynamic? FilteredValidValue(object? obj)
        {
            if (obj == null) return null;
            if (obj is IEnumerable<string> stringList)
            {
                if (stringList.Any(d => !string.IsNullOrWhiteSpace(d))) return obj;
                else return null;
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && (p.GetIndexParameters().Length == 0) && !p.Name.Equals("parsetype", StringComparison.OrdinalIgnoreCase));

            foreach (var p in properties)
            {
                var value = p.GetValue(obj);
                if (value == null) continue;
                if (p.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(value?.ToString())) return obj;
                if (value is IEnumerable<string> stringListP)
                {
                    if (stringListP.Any(d => !string.IsNullOrWhiteSpace(d))) return obj;
                    else continue;
                }
                if (FilteredValidValue(value) != null) return obj;
            }
            return null;
        }

        public static double AutoFitSizeUnit(long size, string unit = "KB")
        {
            double tempSize = size * 1.0;
            if (Enum.TryParse(typeof(SizeUnit), unit, true, out var result))
            {
                SizeUnit sizeUnit = (SizeUnit)result;
                switch (sizeUnit)
                {
                    case SizeUnit.B:
                        return tempSize / (long)SizeUnit.B;
                    case SizeUnit.KB:
                        return tempSize / (long)SizeUnit.KB;
                    case SizeUnit.MB:
                        return tempSize / (long)SizeUnit.MB;
                    case SizeUnit.GB:
                        return tempSize / (long)SizeUnit.GB;
                    case SizeUnit.TB:
                        return tempSize / (long)SizeUnit.TB;
                    case SizeUnit.PB:
                        return tempSize / (long)SizeUnit.PB;
                    case SizeUnit.EB:
                        return tempSize / (long)SizeUnit.EB;
                    default:
                        return tempSize / (long)SizeUnit.B;
                }
            }
            else
            {
                return tempSize / (long)SizeUnit.KB;
            }
        }
        public enum SizeUnit : ulong
        {
            [Description("B / Bytes")]
            B = 1,
            [Description("Bytes")]
            Bytes = 1,

            [Description("KB / Kilobytes")]
            KB = 1024,
            [Description("Kilobytes")]
            Kilobytes = 1024,

            [Description("MB / Megabytes")]
            MB = 1024L * 1024,
            [Description("Megabytes")]
            Megabytes = 1024L * 1024,

            [Description("GB / Gigabytes")]
            GB = 1024L * 1024 * 1024,
            [Description("Gigabytes")]
            Gigabytes = 1024L * 1024 * 1024,

            [Description("TB / Terabytes")]
            TB = 1024L * 1024 * 1024 * 1024,
            [Description("Terabytes")]
            Terabytes = 1024L * 1024 * 1024 * 1024,

            [Description("PB / Petabytes")]
            PB = 1024L * 1024 * 1024 * 1024 * 1024,
            [Description("Petabytes")]
            Petabytes = 1024L * 1024 * 1024 * 1024 * 1024,

            [Description("EB / Exabytes")]
            EB = 1024L * 1024 * 1024 * 1024 * 1024 * 1024,
            [Description("Exabytes")]
            Exabytes = 1024L * 1024 * 1024 * 1024 * 1024 * 1024,
        }

        public static string ComputeHashAsBase64(string filePath, string hashAlgorithm = VEOV3CommonString.ALGORITHM_SHA512)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return string.Empty;
            }
            using (var hash = GetHashAlgorithm(hashAlgorithm))
            {
                if (hash == null) return null;
                var fileStream = ExchangeUtils.GetEXOItemLocalMSGFileStream(filePath);
                byte[] hashBytes = hash.ComputeHash(fileStream);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static string ComputeHashAsBase64(Stream fileStream, string hashAlgorithm = VEOV3CommonString.ALGORITHM_SHA512)
        {  
            using (var hash = GetHashAlgorithm(hashAlgorithm))
            {
                if (hash == null) return null;
                byte[] hashBytes = hash.ComputeHash(fileStream);
                return Convert.ToBase64String(hashBytes);
            }
        }
        private static HashAlgorithm GetHashAlgorithm(string algorithm)
        {
            return algorithm.ToUpper() switch
            {
                "MD5" => MD5.Create(),
                "SHA1" or "SHA-1" => SHA1.Create(),
                "SHA256" or "SHA-256" => SHA256.Create(),
                "SHA384" or "SHA-384" => SHA384.Create(),
                "SHA512" or "SHA-512" => SHA512.Create(),
                _ => throw new ArgumentException("Unsupported hash algorithm", nameof(algorithm))
            };
        }
        public static long CalculateGBSizeUnit(long fileSize)
        {
            return fileSize * (int)SizeUnit.GB;

        }

        public static void CreateVEOZipWithPassword(string folderPath, string outputZipFile, string encryptKey)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptKey))
                {
                    ZipUtil.ZipFolder(folderPath, outputZipFile);
                }
                else
                {
                    ZipUtil.ZipFolder(folderPath, outputZipFile, encryptKey, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"zip the directory {folderPath} failed, maybe the path is too long, try to zip with alphaFS. {e.ToString()}");
                ZipUtil.ZipFolderForLongPath(folderPath, outputZipFile, encryptKey, Encoding.UTF8);
            }
        }
    }
}
