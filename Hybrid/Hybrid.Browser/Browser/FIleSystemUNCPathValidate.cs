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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class FileSystemUNCPathValidate : IBrowser
    {
        private const int ERROR_SUCCESS = 0;
        private const int DFS_INFO_LEVEL_3 = 3;

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetDfsGetClientInfo(
            string dfsEntryPath,
            string serverName,
            string shareName,
            int level,
            out IntPtr buffer);

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(FileSystemUNCPathValidate));

        public HybridBrowserType BrowserType => HybridBrowserType.FileSystemUNCPathValidate;

        public string Browse(string message)
        {
            try
            {
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var validateSucceedConnectionIds = new List<Guid>();
                var finalPaths = new Dictionary<Guid, string>();
                var pathTypes = new Dictionary<Guid, FileSystemPathType>();
                var args = SerializerHelper.DeserializeByJsonSerializer<FileSystemUNCPathValidateArgs>(message);
                var isEnabledJPMC = args.isEnabledJPMC;
                foreach (var pathPair in args.UNCPaths)
                {
                    var pathType = GetFileSystemPathType(pathPair.Value, isEnabledJPMC);
                    if (pathType == FileSystemPathType.Unknown)
                    {
                        if (isEnabledJPMC)
                        {
                            Logger.Warn($"Skip validate [{pathPair.Key}], path is neither UNC nor DFS. Path: {pathPair.Value}");
                        }
                        Logger.Warn($"Skip validate [{pathPair.Key}], path is not UNC. Path: {pathPair.Value}");
                        continue;
                    }

                    var effectivePath = pathPair.Value;
                    Logger.Info($"Start validate [{pathPair.Key}] path, type [{pathType}].");

                    if (Validation(effectivePath, pathType))
                    {
                        Logger.Info($"Succeed validate [{pathPair.Key}] path, type [{pathType}].");
                        validateSucceedConnectionIds.Add(pathPair.Key);
                        finalPaths[pathPair.Key] = effectivePath;
                        pathTypes[pathPair.Key] = pathType;
                    }
                }

                var data = new FileSystemValidateSucceedConnectionInfo
                {
                    TenantId = tenantId,
                    BatchId = args.BatchId,
                    ConnectionIds = validateSucceedConnectionIds
                };

                _ = Task.Run(() => HybridAgentApiClientUtil.Client.treeBrowserService.AddSucceedValidateConnectionIds(data)).Result;

                return SerializerHelper.SerializeByJsonSerializer(new ValidateResult
                {
                    Result = ValidateResultEnum.Succeed,
                    UNCPaths = finalPaths.Count == 0 ? null : finalPaths,
                    PathType = pathTypes.Count == 0 ? null : pathTypes
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while validate file system paths. Error: {e}");
                var result = new ValidateResult
                {
                    Result = ValidateResultEnum.Failed,
                    Message = e.Message
                };
                return SerializerHelper.SerializeByJsonSerializer(result);
            }
        }

        private bool Validation(string rootdir, FileSystemPathType pathType)
        {
            string tempFileForValidate = Guid.NewGuid().ToString() + "." + DateTime.Now.Ticks + "_Records.tmp";
            try
            {
                Logger.Info($"Begin to validate node file for [{pathType}] path.");
                var system = StorageUtil.OpenXSystem(rootdir);
                if (!system.DirectoryExists(new Media.Storage.StorageInfo()))
                {
                    return false;
                }

                byte[] bytes = new byte[1];
                bytes[0] = 0x00;
                var storageInfo = new Media.Storage.StorageInfo("", tempFileForValidate);
                using (Stream s = new MemoryStream(bytes))
                {
                    system.CommitStream(s, storageInfo);
                }

                bool hasException = false;
                try
                {
                    system.DeleteFile(storageInfo);
                }
                catch (Exception ex)
                {
                    hasException = true;
                    Logger.Warn($"Delete test file error: {ex}");
                }

                if (system.FileExists(storageInfo) || hasException)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Validate test error for [{pathType}] path.", e);
                return false;
            }

            return true;
        }

        private static FileSystemPathType GetFileSystemPathType(string path, bool isEnabledJPMC)
        {
            if (Win32Native.PathIsNetworkPath(path))
            {
                if (isEnabledJPMC)
                {
                    return IsDfsPath(path) ? FileSystemPathType.Dfs : FileSystemPathType.Unc;
                }
                return FileSystemPathType.Unc;
            }
            return FileSystemPathType.Unknown;
        }

        private static bool IsDfsPath(string path)
        {
            IntPtr buffer = IntPtr.Zero;
            try
            {
                var result = NetDfsGetClientInfo(path, null, null, DFS_INFO_LEVEL_3, out buffer);
                return result == ERROR_SUCCESS;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to detect DFS path by Windows API. Path: {path}, Error: {ex}");
                return false;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    NetApiBufferFree(buffer);
                }
            }
        }
    }
}
