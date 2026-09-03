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
using System.IO;
using System.Reflection;
using System.Text;
using AutoInstallation.Contract;
using AutoInstallation.Records.App.Resources;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class IOUtility
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void DeleteDirectory(DirectoryInfo info)
        {
            if (info.Name.ToLower() != "logs" && info.Name.ToLower() != "audit")
            {
                foreach (var fo in info.GetDirectories()) DeleteDirectory(fo);
                foreach (var fi in info.GetFiles())
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(LOGRESX.COMMONLOG_DELETEFILEERROR, fi.FullName, ex.ToString());
                    }

                try
                {
                    info.Delete();
                }
                catch (Exception ex)
                {
                    logger.Warn(LOGRESX.COMMONLOG_DELETEFILEERROR, info.FullName, ex.ToString());
                }
            }
        }

        public static void Save(string filePath, Stream stream)
        {
            var fi = new FileInfo(filePath);
            var folder = fi.Directory;
            if (!folder.Exists) folder.Create();
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            var fs = new FileStream(fi.FullName, FileMode.OpenOrCreate);
            fs.Write(bytes, 0, bytes.Length);
            stream.Dispose();
            fs.Dispose();
        }

        public static string WriteReport(string directory, string name, List<ReportItem> previewItems)
        {
            //previewItems.Insert(0, new ReportItem {Key = Resource.COMMON_REPORT_TITLE + ":"});
            var filePath = Path.Combine(directory, name + "(" + DateTime.Now.ToString("yyyyMMddhhmmss") + ").txt");
            var folder = new DirectoryInfo(directory);
            if (!folder.Exists) folder.Create();
            try
            {
                var sb = new StringBuilder();
                foreach (var v in previewItems)
                {
                    if (sb.Length != 0) sb.Append("\r\n");
                    sb.Append(string.Format("{0,-50}{1}", v.Key, v.Value));
                    //sb.Append(v.Key);
                    //sb.Append("    ");
                    //sb.Append(v.Value);
                }

                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                }

                //using (StreamWriter sw = new StreamWriter(fullPath, false, Encoding.Default)) { sw.WriteLine("aaa"); }
            }
            catch (Exception ex)
            {
                filePath = string.Empty;
                var sb = new StringBuilder();
                foreach (var v in previewItems)
                {
                    sb.Append("<");
                    sb.Append(v.Key);
                    sb.Append(":");
                    sb.Append(v.Value);
                    sb.Append(">");
                }

                logger.Warn(LOGRESX.COMMONUTILITYLOG_WRITEREPORTFAILED, filePath, sb.ToString(), ex.ToString());
            }

            return filePath;
        }
    }
}