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
//using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class FileTransferStream : FileStream
    {
        private string filePath { get; set; }

        private string fileFolder { get; set; }

        public FileTransferStream(string filePath ,string fileFolder, FileMode fileMode)
            : base(filePath, fileMode)
        {
            this.filePath = filePath;
            this.fileFolder = fileFolder;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(1000);
                try
                {
                    DeleteFolder(fileFolder);
                    DeleteFile(filePath);
                }
                catch (Exception ex)
                {
                }
            });
        }

        private void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string f in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(f))
                    {
                        FileInfo fi = new FileInfo(f);
                        File.Delete(f);
                    }
                    else
                    {
                        DeleteFolder(f);
                    }
                }
                Directory.Delete(dir);
            }
        }

        private void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }

}
