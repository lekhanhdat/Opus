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
using AvePoint.Common.FilterEngine;
using AvePoint.Common.FilterEngine.ObjectInfos;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public static class ObjectConverter
    {
        public static ObjectInfoBase CloneFilterObject(ObjectInfoBase obj, Dictionary<string, TimeSpan> offsets)
        {
            if (obj is FSFileInfo)
            {
                var file = obj as FSFileInfo;
                return new FSFileInfo
                {
                    Name = file.Name,
                    Size = file.Size,
                    Extension = file.Extension,
                    AccessTime = offsets.ContainsKey("Last Accessed Time") ? file.AccessTime.Add(-offsets["Last Accessed Time"]).AddMinutes(-1) : file.AccessTime,
                    Created = offsets.ContainsKey("Created Time") ? file.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : file.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? file.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : file.Modified,
                    Owner = file.Owner,
                    FilePath = file.FilePath
                };
            }
            else if (obj is FSFolderInfo)
            {
                var folder = obj as FSFolderInfo;
                return new FSFolderInfo
                {
                    Name = folder.Name,
                    AccessTime = offsets.ContainsKey("Last Accessed Time") ? folder.AccessTime.Add(-offsets["Last Accessed Time"]).AddMinutes(-1) : folder.AccessTime,
                    Created = offsets.ContainsKey("Created Time") ? folder.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : folder.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? folder.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : folder.Modified,
                    Owner = folder.Owner,
                };
            }
            else if (obj is AzureFileInfo)
            {
                var file = obj as AzureFileInfo;
                return new AzureFileInfo
                {
                    Title = file.Name,
                    Name = file.Name,
                    Size = file.Size,
                    AccessTime = offsets.ContainsKey("Last Accessed Time") ? file.AccessTime.Add(-offsets["Last Accessed Time"]).AddMinutes(-1) : file.AccessTime,
                    Created = offsets.ContainsKey("Created Time") ? file.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : file.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? file.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : file.Modified,
                    Path = file.Path
                };
            }
            else if (obj is DocumentInfo)
            {
                var file = obj as DocumentInfo;
                return new DocumentInfo
                {
                    Title = file.Name,
                    Name = file.Name,
                    Size = file.Size,
                    Created = offsets.ContainsKey("Created Time") ? file.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : file.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? file.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : file.Modified,
                    CreatedByLogonName = file.CreatedByLogonName,
                    ModifiedByLogonName = file.ModifiedByLogonName
                };
            }
            else if (obj is BoxItemInfo)
            {
                var file = obj as BoxItemInfo;
                return new BoxItemInfo
                {
                    Title = file.Name,
                    Name = file.Name,
                    Size = file.Size,
                    AccessTime = offsets.ContainsKey("Last Accessed Time") ? file.AccessTime.Add(-offsets["Last Accessed Time"]).AddMinutes(-1) : file.AccessTime,
                    Created = offsets.ContainsKey("Created Time") ? file.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : file.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? file.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : file.Modified,
                };
            }
            else if (obj is GoogleItemInfo)
            {
                var file = obj as GoogleItemInfo;
                return new GoogleItemInfo
                {
                    Title = file.Name,
                    Name = file.Name,
                    Size = file.Size,
                    Created = offsets.ContainsKey("Created Time") ? file.Created.Add(-offsets["Created Time"]).AddMinutes(-1) : file.Created,
                    Modified = offsets.ContainsKey("Modified Time") ? file.Modified.Add(-offsets["Modified Time"]).AddMinutes(-1) : file.Modified,
                };
            }
            else
            {
                return null;
            }
        }
        public static ObjectInfoBase ConvertXObject2FilterObject(StorageInfo xObj, string connectionPath = "")
        {
            if (xObj is XFileInfo)
            {
                XFileInfo file = xObj as XFileInfo;
                return ConverXFile2FilterObj(file, connectionPath);

            }
            else if (xObj is XDirectoryInfo)
            {
                XDirectoryInfo folder = xObj as XDirectoryInfo;
                return ConverXDir2FilterObj(folder);

            }
            return null;
        }

        private static ObjectInfoBase ConverXFile2FilterObj(XFileInfo file, string connectionPath = "")
        {
            FSFileInfo objectInfo = new FSFileInfo()
            {
                Name = Path.GetFileName(file.Name),
                Size = file.FileSize,
                Extension = Path.GetExtension(file.Name),
                AccessTime = file.LastAccessTimeUtc,
                Created = file.CreationTimeUtc,
                Modified = file.LastWriteTimeUtc,
                Owner = file.Owner,
                // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine  Neet to be verified
                FilePath = string.IsNullOrEmpty(connectionPath) ? file.FileFullPath : Path.Combine(connectionPath, file.HighPlusLowName),
            };
            return objectInfo;
        }
        private static ObjectInfoBase ConverXDir2FilterObj(XDirectoryInfo dir)
        {
            FSFolderInfo objectInfo = new FSFolderInfo()
            {
                Name = dir.Name,
                Created = dir.CreationTimeUtc,
                Modified = dir.LastWriteTimeUtc,
                Owner = dir.Owner,
                AccessTime = dir.LastAccessTimeUtc,
            };
            return objectInfo;
        }
    }
}
