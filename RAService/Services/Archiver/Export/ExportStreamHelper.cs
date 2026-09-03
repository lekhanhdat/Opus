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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Service.ControlPanel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver.Export
{
    public class ExportStreamHelper
    {
        public static string GetStringFromStream(Stream stream)
        {
            if (stream == null)
                return string.Empty;

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        public static void UploadFileToStorage(string unZipFolderPath, string zipFolderPath, string fileName, Func<string, Stream, bool> uploadFuc)
        {
            DeleteFile(zipFolderPath);
            ZipFile.CreateFromDirectory(unZipFolderPath, zipFolderPath);
            using (var stream = new FileStream(zipFolderPath, FileMode.Open, FileAccess.Read))
            {
                uploadFuc(fileName, stream);
            }
            DeleteFile(zipFolderPath);
            DeleteDirectory(unZipFolderPath);
        }
    }
}
