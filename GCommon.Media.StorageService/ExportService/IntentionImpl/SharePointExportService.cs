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

using System.Text;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Site = System.Security.Policy.Site;

namespace AvePoint.GCommon.Media.StorageService;

internal class SharePointExportService : ExportServiceBase
{
    private const char SLASH = '/';
    private const char BACKSLASH = '\\';
    protected override ExportDataFileResult ExportDataFile(Stream stream, ExportInfo exportInfo, long length = 0)
    {
        ExportDataFileResult result = new ();
        stream.Position = 0;
        #region Check if folder exist

        exportInfo.FolderName = exportInfo.FolderName.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
        var folders = exportInfo.FolderName.Split("\\").ToList();
        var jobId = folders[0];
        var fileName = exportInfo.FileName;
        folders.RemoveAll(folder => folder == jobId);
        var currentFolder = exportInfo.JobFolder;
        foreach (var folder in folders)
        {
            try
            {
                var sharePointFolder = currentFolder.Folders[folder];
                currentFolder = sharePointFolder;
            }
            catch
            {
                var newFolder = currentFolder.Folders.Add(folder);
                currentFolder = newFolder;
            }
        }

        string fileServerRelativeUrl = currentFolder.ServerRelativeUrl + $"/{fileName}";
        #endregion
        SharePointExportService.AddFile(exportInfo.ParentWebUrl, fileServerRelativeUrl, stream, true);
        result.FileSize = length;
        result.FileName = Path.Combine(exportInfo.FolderName, exportInfo.FileName);
        return result;
    }
}