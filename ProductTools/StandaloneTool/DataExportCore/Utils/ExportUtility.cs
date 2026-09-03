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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Encryption;
using Castle.Windsor;
using DataExportCore.Cache;
using DataExportCore.Enum;
using Storage.SFTP;
using System.Security.Cryptography;
using System.Text;

namespace DataExportCore.Utils
{
    public class ExportUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ExportUtility));
        public const string MasterKeyProfileName = "DBSEEMasterKey";
        public const string IndexDeviceProfileName = "UsingIndexDevice";
        public const string AVEPOINT_STORAGE_ID = "6A040C17-AF8A-4F1F-96C1-7CEB2E23B1F3";
        private static readonly List<StorageDeviceType> SupportUploadStorageType = [StorageDeviceType.CloudAzure, StorageDeviceType.SFTP];

        public static void SetupConfiguration()
        {
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            WindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                Path.Combine(installPath, "Castle/ServiceCastle.config")));
            PlatformWindsorManager.SetUp(windsorContainer);
            StorageApiConfiguration.Setup();
        }

        private static RMAesEncryptorWrapper? _customAesEncryptorWrapper;

        public static RMAesEncryptorWrapper CustomAesEncryptorWrapper
        {
            get
            {
                if (_customAesEncryptorWrapper == null)
                {
                    if (GlobalCache.CustomPassword == null)
                    {
                        logger.Error($"CustomPassword is invalid");
                        throw new Exception("Password is invalid");
                    }
                    _customAesEncryptorWrapper = new(GenerateAesKey());
                    return _customAesEncryptorWrapper;
                }

                return _customAesEncryptorWrapper;
            }
        }

        public static bool IsNeedUploadAndDeleteCache()
        {
            return SupportUploadStorageType.Contains(GlobalCache.TargetStorageType);
        }

        private static byte[] GenerateAesKey()
        {
            try
            {
                return SHA256.HashData(Encoding.UTF8.GetBytes(GlobalCache.CustomPassword.ToPlainString()));
            }
            catch (Exception e)
            {
                logger.Error($"An error occurs while generating AesKey with custom password. Ex: {e}");
                throw;
            }
        }

        public static string BuildExportPath(string exportLocation, string name, string sitePath, NodeType level)
        {
            string exportPath = string.Empty;
            try
            {
                if (sitePath.StartsWith("http://"))
                {
                    exportPath = sitePath.Remove(0, "http://".Length);
                }
                else if (sitePath.StartsWith("https://"))
                {
                    exportPath = sitePath.Remove(0, "https://".Length);
                }
                switch (level)
                {
                    case NodeType.Site:
                        exportPath = new StringBuilder(exportPath).Append("\\").ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_");
                        break;
                    case NodeType.Web:
                        string webPath = name;
                        if (webPath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                        {
                            webPath = webPath.Remove(0, ".\\".Length);
                        }
                        var webSplitPath = webPath.TrimStart('/').Split('/');
                        webPath = string.Join("\\", webSplitPath.Select(_ => _.Replace("/", "_").Replace(":", "_").Replace(".", "_")).ToArray());
                        exportPath = Path.Combine(new StringBuilder(exportPath).Append("\\").ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_"), webPath);
                        break;
                    case NodeType.List:
                    case NodeType.Folder:
                        string listPath = name;
                        if (listPath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                        {
                            listPath = listPath.Remove(0, ".\\".Length);
                        }
                        var listSplitPath = listPath.TrimStart('/').Split('/');
                        listPath = string.Join("\\", listSplitPath.Select(_ => _.Replace("/", "_").Replace(":", "_").Replace(".", "_")).ToArray());
                        if (sitePath.StartsWith("http://"))
                        {
                            exportPath = sitePath.Remove(0, "http://".Length);
                        }
                        else if (sitePath.StartsWith("https://"))
                        {
                            exportPath = sitePath.Remove(0, "https://".Length);
                        }
                        exportPath = new StringBuilder(exportPath).Append("\\").Append(listPath).ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_");
                        break;
                    case NodeType.ExchangeOnlineMailbox:
                    case NodeType.Mail:
                        if (string.IsNullOrEmpty(name)) break;
                        exportPath = new StringBuilder(exportPath).Append("\\").Append(name).ToString();
                        break;
                    default:
                        return string.Empty;
                }
                logger.Info($"Finish to build export path, path[{exportPath}], level: [{level}]");
                return Path.Combine(exportLocation, ReplaceInvalidChar(exportPath, false));
            }
            catch (Exception e)
            {
                logger.Error($"An error occurs while build export path. Ex: {e}");
                return string.Empty;
            }
        }

        public static string ReplaceInvalidChar(string srcStr, bool isFile)
        {
            Char[] invalidCS = Path.GetInvalidFileNameChars();
            var sep = Path.DirectorySeparatorChar;
            foreach (char c in invalidCS)
            {
                srcStr = !(!isFile && c == sep) ? srcStr.Replace(c, '_') : srcStr;
            }
            return srcStr;
        }
        public static string BuildTargetUrl(string exportPath)
        {
            if (IsNeedUploadAndDeleteCache())
            {
                exportPath = exportPath.StartsWith(GlobalCache.ExportLocation) ? exportPath.Substring(GlobalCache.ExportLocation.Length) : exportPath;
            }
            return exportPath;
        }

        public static void AddUploadedSiteToReport(string siteUrl, string siteName, StorageDeviceType type, bool isError = false)
        {
            var destination = string.Empty;

            if (isError)
            {
                GlobalCache.SummaryReportDtos.Add(new SummaryReportDto
                {
                    Site_Collection = siteUrl,
                    Destination = string.Empty,
                    Status = ExportStatus.Failed.ToString(),
                    Comment = siteName // error
                });
                return;
            }

            try
            {
                siteName = siteName.Trim('\\');
                switch (type)
                {
                    case StorageDeviceType.CloudAzure:
                        destination = new Uri(Path.Combine(GlobalDeviceCache.GetDestinationDevice().SystemPath, siteName)).AbsoluteUri;
                        break;
                    case StorageDeviceType.SFTP:
                        var isRootFolder = !((Storage.AbstractXSystem)GlobalDeviceCache.GetDestinationDevice()).ConnectionString.Contains(SFTPXRIParameterKeys.SFTP_RootFolder, StringComparison.CurrentCulture);
                        destination = isRootFolder ? $"/{siteName}" : new Uri(Path.Combine(GlobalDeviceCache.GetDestinationDevice().SystemPath, siteName)).AbsolutePath.TrimStart('/').Replace('/', '\\');
                        break;
                    case StorageDeviceType.None:
                        destination = Path.Combine(GlobalCache.ExportLocation, siteName);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occured while building the destination path for summary report. Ex: {e}");
                destination = e.Message;
            }

            GlobalCache.SummaryReportDtos.Add(new SummaryReportDto
            {
                Site_Collection = siteUrl,
                Destination = destination,
                Status = ExportStatus.Successful.ToString(),
                Comment = string.Empty
            });
        }

        public static void AddUploadedTeamsToReport(string groupAddresss, string objectName, string exportPath, StorageDeviceType type, NodeType nodeType, ExportStatus status = ExportStatus.Successful)
        {
            var destination = string.Empty;

            if (status == ExportStatus.Failed || status == ExportStatus.Skipped)
            {
                GlobalCache.TeamsSummaryReportDtos.Add(new TeamsSummaryReportDto
                {
                    Teams = groupAddresss,
                    ObjectName = objectName,
                    Destination = string.Empty,
                    Status = status.ToString(),
                    //NodeType = GetNodeType(nodeType),
                    Comment = exportPath // error
                });
                return;
            }

            try
            {
                exportPath = exportPath.Trim('\\');
                switch (type)
                {
                    case StorageDeviceType.CloudAzure:
                        destination = new Uri(Path.Combine(GlobalDeviceCache.GetDestinationDevice().SystemPath, exportPath)).AbsoluteUri;
                        break;
                    case StorageDeviceType.SFTP:
                        var isRootFolder = !((Storage.AbstractXSystem)GlobalDeviceCache.GetDestinationDevice()).ConnectionString.Contains(SFTPXRIParameterKeys.SFTP_RootFolder, StringComparison.CurrentCulture);
                        destination = isRootFolder ? $"/{exportPath}" : new Uri(Path.Combine(GlobalDeviceCache.GetDestinationDevice().SystemPath, exportPath)).AbsolutePath.TrimStart('/').Replace('/', '\\');
                        break;
                    case StorageDeviceType.None:
                        destination = Path.Combine(GlobalCache.ExportLocation, exportPath);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occured while building the destination path for summary report. Ex: {e}");
                destination = e.Message;
            }

            GlobalCache.TeamsSummaryReportDtos.Add(new TeamsSummaryReportDto
            {
                Teams = groupAddresss,
                ObjectName = objectName,
                Destination = destination,
                Status = ExportStatus.Successful.ToString(),
                //NodeType = GetNodeType(nodeType),
                Comment = string.Empty
            });
        }

        private static string GetNodeType(NodeType nodeType) => nodeType switch
        {
            NodeType.Conversation => I18NEntity.GetString("SATool_ObjectLevel_Conversation"),
            NodeType.Site => I18NEntity.GetString("SATool_ObjectLevel_SiteCollection"),
            NodeType.ExchangeOnlineMailbox => I18NEntity.GetString("SATool_ObjectLevel_ExchangeMailBox"),
            _ => "Unknown",
        };
    }
}
