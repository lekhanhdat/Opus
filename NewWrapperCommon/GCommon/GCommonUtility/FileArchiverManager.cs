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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace AvePoint.GCommon
{
    public class FileArchiverManager
    {
        private string baseDirectory;
        private string baseFileName;
        private int maxRollBackups;
        private int maxFileSize;

        public FileArchiverManager(string baseDirectory, string baseFileName, int maxRollBackups, int maxFileSize)
        {
            this.baseDirectory = baseDirectory;
            this.baseFileName = baseFileName;
            this.maxRollBackups = maxRollBackups;
            this.maxFileSize = maxFileSize;
        }

        public void PutFile(string fileFullPath, bool autoDelete = true)
        {
            string baseFileFullPath = Path.Combine(baseDirectory, baseFileName);
            if (File.Exists(baseFileFullPath))
            {
                RollingTheFile(baseFileFullPath);
            }

            string now = AvePoint.GCommon.Utility.AveDateTimeUtility.ConvertToTypeForCommon000(DateTime.Now);
            FileInfo fi = new FileInfo(fileFullPath);
            string dir = fi.Directory.FullName;
            string fileName = fi.Name;
            string newFileFullPath = Path.Combine(dir, now + "_" + fileName);
            File.Move(fileFullPath, newFileFullPath);
            ZipFile.CreateFromDirectory(baseDirectory, dir, CompressionLevel.Optimal, false, Encoding.UTF8);

            if (autoDelete)
            {
                File.Delete(newFileFullPath);
            }
            else
            {
                File.Move(newFileFullPath, fileFullPath);
            }
        }

        private void RollingTheFile(string fileFullPath)
        {
            FileInfo fi = new FileInfo(fileFullPath);
            if (fi.Length < maxFileSize) return;
            RenameFile(fileFullPath, fileFullPath, 1);
        }

        private void RenameFile(string currentFileFullPath, string baseFileFullPath, int postFixNumber)
        {
            if (File.Exists(baseFileFullPath + "." + postFixNumber))
            {
                RenameFile(baseFileFullPath + "." + postFixNumber, baseFileFullPath, postFixNumber + 1);
                File.Move(currentFileFullPath, baseFileFullPath + "." + postFixNumber);
            }
            else
            {
                if (postFixNumber - 1 == maxRollBackups)
                {
                    File.Delete(baseFileFullPath + "." + maxRollBackups);
                }
                else
                {
                    File.Move(currentFileFullPath, baseFileFullPath + "." + postFixNumber);
                }
            }
        }

        public static void Test()
        {
            FileArchiverManager archiverManager = new FileArchiverManager(@"C:\temp", "test.zip", 3, 1024 * 64);
            for (int i = 0; i < int.MaxValue; i++)
            {
                archiverManager.PutFile(@"C:\temp\log.log", false);
                archiverManager.PutFile(@"C:\temp\DocAve.log", false);
                archiverManager.PutFile(@"C:\temp\DocAve.log", false);
            }
        }
    }
}
