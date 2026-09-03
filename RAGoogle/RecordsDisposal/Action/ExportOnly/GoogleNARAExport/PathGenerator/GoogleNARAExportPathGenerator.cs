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

using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using System.Collections.Concurrent;
using System.Text;

namespace RAGoogle
{
    public class GoogleNARAExportPathGenerator(string physicalDeviceDtoPath, string driveName) : GoogleExportPathGeneratorBase
    {
        private readonly string _files = "Files";

        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> parentIdAndFolderNameDictionary = new([new KeyValuePair<string, ConcurrentDictionary<string, string>>(driveName, new())]);
        private ConcurrentDictionary<string, ConcurrentBag<string>> parentIdAndFileNameDictionary = new();

        public override GoogleExportInfo GenerateGoogleItemExportInfo(GoogleItemPathGeneratorInfo itemInfo)
        {
            var (relativePathParent, folderName) =
                CreateGoogleFolderStructure(itemInfo.GoogleItem.Path, itemInfo.GoogleItem.ParentIds);
            var exportFileName = HandleDuplicateFileName(itemInfo.GoogleItem.ParentId, itemInfo.GoogleItem);
            var (driveName, directory) = HandleFolderPath(itemInfo, itemInfo.NodeLevel, relativePathParent);
            var pathDrive = SecurityUtils.SafeCombinePath(itemInfo.JobId, driveName);
            GoogleExportInfo info = new GoogleExportInfo
            {
                JobID = itemInfo.JobId,
                FolderPath = SecurityUtils.SafeCombinePath(pathDrive, _files, directory),
                PhysicalDevicePath = physicalDeviceDtoPath,
                GoogleItem = itemInfo.GoogleItem,
                ContentFilePath = exportFileName,
                FolderName = folderName
            };
            return info;
        }
        private (string, string) HandleFolderPath(GoogleItemPathGeneratorInfo googleInfo, NodeLevel nodeLevel, string relativePathParent)
        {
            string driveName;
            string relativePath;

            if (nodeLevel == NodeLevel.GoogleMyDrive)
            {
                relativePath = relativePathParent.Substring(googleInfo.GoogleItem.MemberEmail.Length + 1);
                driveName = googleInfo.GoogleItem.MemberEmail;
            }
            else
            {
                relativePath = relativePathParent.Substring(googleInfo.GoogleItem.DriveName.Length + 1);
                driveName = googleInfo.GoogleItem.DriveName;
            }

            var directory = Path.GetDirectoryName(relativePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;

            return (driveName, directory);
        }

        private (string, string) CreateGoogleFolderStructure(string relativePath, string parentIds)
        {
            string folderNameOfParentFile = driveName;
            var tempPath = relativePath.IndexOf('/') > 0
                ? relativePath.Substring(0, relativePath.LastIndexOf('/'))
                : relativePath;
            List<string> folderNames = tempPath.Split('/').ToList();
            List<string> folderIds = parentIds.Split('/').ToList();

            if (folderNames.Count == 1)
            {
                return (relativePath, driveName);
            }


            StringBuilder stringBuilder = new();
            stringBuilder.Append(driveName + '/');
            int index = 0;
            while (true)
            {
                if (!parentIdAndFolderNameDictionary.TryGetValue(index == 0 ? driveName : folderIds[index],
                        out var subFolders))
                {
                    parentIdAndFolderNameDictionary.TryAdd(folderIds[index],
                        new ConcurrentDictionary<string, string>());
                    subFolders = parentIdAndFolderNameDictionary[folderIds[index]];
                };

                if (index == folderNames.Count - 1)
                {
                    break;
                }

                var currentSubFolderId = folderIds[index + 1];
                var currentSubFolderName = folderNames[index + 1];


                if (subFolders!.TryGetValue(currentSubFolderId, out var folderName))
                {
                    stringBuilder.Append(folderName + '/');
                    folderNameOfParentFile = folderName;
                }
                else
                {
                    int newSubFolderIndex = 0;

                    while (true)
                    {
                        var newFolderName = newSubFolderIndex == 0 ? currentSubFolderName : currentSubFolderName + $"_{newSubFolderIndex}";
                        var succeed = AddNewFolderName(subFolders, newFolderName, currentSubFolderId);
                        if (succeed)
                        {
                            stringBuilder.Append(newFolderName + '/');
                            folderNameOfParentFile = newFolderName;
                            break;
                        }

                        newSubFolderIndex++;
                    }
                }


                index++;
            }

            return (stringBuilder.ToString(), folderNameOfParentFile);
        }

        private bool AddNewFolderName(ConcurrentDictionary<string, string> subFolders, string currentFolderName, string currentFolderId)
        {
            if (!subFolders.Values.Contains(currentFolderName))
            {
                return subFolders.TryAdd(currentFolderId, currentFolderName);
            }

            return false;
        }

        private string HandleDuplicateFileName(string parentId, DownloadedFileInfo info)
        {
            string exportPath;
            if (!parentIdAndFileNameDictionary.ContainsKey(parentId))
            {
                parentIdAndFileNameDictionary.TryAdd(parentId, new ConcurrentBag<string>());
            }

            var fileNameConcurrentBag = parentIdAndFileNameDictionary[parentId];
            int duplicateNumber = 0;

            while (true)
            {
                string fileName = duplicateNumber == 0 ? info.FormattedFileVersionName : info.FormattedFileVersionName + $"_{duplicateNumber}";

                var (succeed, newFileName) = HandleFileName(fileNameConcurrentBag!, fileName, info);
                if (succeed)
                {
                    exportPath = newFileName;
                    break;
                }

                duplicateNumber++;
            }

            return exportPath;
        }

        private (bool, string) HandleFileName(ConcurrentBag<string> fileNameConcurrentBag, string fileName, DownloadedFileInfo info)
        {
            if (!fileNameConcurrentBag.Contains(fileName))
            {
                fileNameConcurrentBag.Add(fileName);
                var exportPath = fileName + info.DownloadFileExtension;
                return (true, exportPath);
            }
            return (false, string.Empty);
        }
    }
}
