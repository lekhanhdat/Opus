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




namespace AvePoint.GCommon.Media.StorageService
{
    #region directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AvePoint.Media.Storage;
    using AvePoint.Media.Storage.Util;

    #endregion directives

    public class CplGenerateService
        : ICplGenerateService
    {
        public void Generate(IXSystem exportDevice, ExportServiceInfo exportServiceInfo)
        {
            var directories = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var directory in directories)
            {
                if (directory.Contains("CPL"))
                {
                    var files = Directory.GetFiles(directory);
                    var buffer = new Byte[64 * 1024];
                    foreach (var file in files)
                    {
                        var cplFileStorageInfo = file.Contains("CommonCreateHyperlinks.cpl")
                            ? XConvert.FromNames(exportServiceInfo.JobId, "CommonCreateHyperlinks.cpl")
                            : XConvert.FromNames(exportServiceInfo.JobId, "CommonCreateConcordanceDB.cpl");
                        using (Stream stream = exportDevice.OpenStream(cplFileStorageInfo, FileMode.OpenOrCreate)
                            , fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            while (true)
                            {
                                int readLen = fileStream.Read(buffer, 0, buffer.Length);
                                if (readLen <= 0) break;
                                stream.Write(buffer, 0, readLen);
                            }
                            fileStream.Close();
                            stream.Close();
                        }
                    }
                }
            }
        }
    }
}