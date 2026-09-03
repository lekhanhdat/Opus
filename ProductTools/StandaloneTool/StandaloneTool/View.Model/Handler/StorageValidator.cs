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
using AvePoint.Media.ClassicStorage.Util;
using AvePoint.RA.CommonUtil;
using Renci.SshNet;
using StandaloneTool.Common;
using StandaloneTool.Model.Common;
using StandaloneTool.Model.StorageInfo;
using StandaloneTool.Model.Verify;
using Storage;
using Storage.SFTP;
using System.IO;
using XConst = AvePoint.Media.StorageApi.XConst;

namespace StandaloneTool.View.Model.Handler
{
    public static class StorageValidator
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(StorageValidator));

        public static bool ValidateAzureInfo(AzureStorageInfo info, bool isCheckAveStorage = false)
        {
            try
            {
                var physicalDeviceDto = GenerateAzure(info);
                var physicalDevice = XFactory.InstanceSystem(physicalDeviceDto.BuildXRI());

                if (!physicalDevice.DirectoryExists(new StorageInfo { LowName = string.Empty })) return false;

                if (physicalDevice.Validate().SystemHealth == XSystemHealth.ConnectedFailed) return false;

                physicalDevice.Open();

                if (!isCheckAveStorage)
                {
                    GlobalInfo.IsSkipAPData = false;
                    GlobalInfo.TargetStorage = physicalDevice;
                    GlobalInfo.TargetStorageType = StorageDeviceType.CloudAzure;
                    GlobalInfo.ExportLocation = physicalDevice.SystemPath;
                    return true;
                }

                GlobalInfo.AvepointMappingStorage = physicalDevice;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Check Azure info failed. Error: {0}.", ex);
                return false;
            }
        }

        public static bool IsPortFormatCorrect(string port)
        {
            var checker = new StringVerification();
            return checker.ValidatePort(port);
        }

        private static PhysicalDeviceDto GenerateAzure(AzureStorageInfo info)
        {
            return new()
            {
                Id = Guid.NewGuid().ToString(),
                Type = 403,
                ConnectionString = CreateAzureBlobStorageConnectionString(info),
                Usage = PhysicalDeviceUsage.All,
                DeviceMode = (int)PhysicalDeviceStatus.Online,
                ModifyTime = DateTime.Now.Ticks,
            };
        }
        private static string Encode(string value) => value.IsNullOrEmpty() ? value : value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");

        public static bool ValidateSftpInfo(SftpStorageInfo info, ref VerifyResult verifyResult)
        {
            try
            {
                verifyResult = VerifyResult.SFTPIPError;
                if (!string.IsNullOrWhiteSpace(info.RootFolder))
                {
                    info.RootFolder = $"\\{info.RootFolder}";
                }

                var cloneInfo = info.Clone();
                cloneInfo.RootFolder = string.Empty;

                var physicalDeviceDto = GenterateSftp(cloneInfo);
                var physicalDevice = XFactory.InstanceSystem(physicalDeviceDto.BuildXRI());

                if (!physicalDevice.DirectoryExists(new StorageInfo { HighName = info.RootFolder, LowName = string.Empty }))
                {
                    verifyResult = VerifyResult.SFTPFolderPathInvalid;
                    return false;
                }

                physicalDeviceDto = GenterateSftp(info);
                physicalDevice = XFactory.InstanceSystem(physicalDeviceDto.BuildXRI());
                if (physicalDevice.Validate().SystemHealth == XSystemHealth.ConnectedFailed) return false;
                physicalDevice.Open();
                GlobalInfo.TargetStorage = physicalDevice;
                GlobalInfo.TargetStorageType = StorageDeviceType.SFTP;
                GlobalInfo.ExportLocation = string.IsNullOrEmpty(info.RootFolder) ? GetSftpWorkingDirectory(info) : info.RootFolder.TrimStart('\\');
                verifyResult = VerifyResult.Success;
                return true;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                verifyResult = VerifyResult.SFTPAuthenticationException;
                logger.Error("Check SFTP info failed. Error: {0}.", ex);
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("Check SFTP info failed. Error: {0}.", ex);
                return false;
            }
        }


        private static PhysicalDeviceDto GenterateSftp(SftpStorageInfo info)
        {
            return new()
            {
                Id = Guid.NewGuid().ToString(),
                Type = 0,
                ConnectionString = CreateSftpStorageConnectionBuilder(info).ToString(),
                Usage = PhysicalDeviceUsage.All,
                DeviceMode = (int)PhysicalDeviceStatus.Online,
                ModifyTime = DateTime.Now.Ticks
            };
        }

        private static XRI CreateSftpStorageConnectionBuilder(SftpStorageInfo info)
        {
            var builder = new XRI { VIM = StorageName.SFTP };
            if (!string.IsNullOrEmpty(info.Password))
            {
                builder.Params.Add(XRIParameterKeys.PASSWORD_KEY, info.Password);
            }
            else
            {
                builder.Params.Add(SFTPXRIParameterKeys.SFTP_PRIVATE_KEY_NAME, info.PrivateKeyFile);
                builder.Params.Add(SFTPXRIParameterKeys.SFTP_PRIVATE_KEY_PASSWORD_NAME, info.PrivateKeyFilePassword);
            }
            builder.Params.Add(SFTPXRIParameterKeys.SFTP_HOST, info.Host);
            builder.Params.Add(SFTPXRIParameterKeys.SFTP_PORT, info.Port);
            builder.Params.Add(XRIParameterKeys.USERNAME_KEY, info.Username);
            builder.Params.Add(SFTPXRIParameterKeys.SFTP_RootFolder, info.RootFolder);
            builder.Params.Add(XRIParameterKeys.CREATE_IF_NOT_EXISTS, "true");
            builder.Params.Add(XRIParameterKeys.ADVANCED_KEY, "false");
            builder.Params.Add(XRIParameterKeys.RETRY_COUNT, "0");
            return builder;
        }

        private static string CreateAzureBlobStorageConnectionString(AzureStorageInfo info)
        {
            return $"{XConst.MEDIASTORAGE_PROTOCOL}{StorageName.Azure}?accesspoint={Encode(info.AccessPoint)}&containername={Encode(info.ContainerName)}&secret={Encode(info.AccountKey)}&name={Encode(info.AccountName)}&isvalidate=true&advanced=false&creation=false";
        }

        private static string GetSftpWorkingDirectory(SftpStorageInfo info)
        {
            ConnectionInfo connectionInfo;
            var workingDirectory = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(info.Password))
                {
                    using (var privateKeyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(info.PrivateKeyFile)))
                    {
                        var privateKey = new PrivateKeyFile(privateKeyStream, info.PrivateKeyFilePassword);
                        var authMethod = new PrivateKeyAuthenticationMethod(info.Username, privateKey);
                        connectionInfo = new ConnectionInfo(info.Host, int.Parse(info.Port), info.Username, authMethod);
                    }
                }
                else
                {
                    var authMethod = new PasswordAuthenticationMethod(info.Username, info.Password);
                    connectionInfo = new ConnectionInfo(info.Host, int.Parse(info.Port), info.Username, authMethod);
                }

                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    workingDirectory = sftpClient.WorkingDirectory;
                    sftpClient.Disconnect();
                    return workingDirectory;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connection SFTP server: {ex.Message}");
                return workingDirectory;
            }
        }
    }
}
