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




namespace AvePoint.Media.Common
{
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Services;
    #region using directives
    using System;
    using System.IO;
    using System.Reflection;
    #endregion

    public static class ResourceUtility
    {
        static RALogger logger = RALogger.GetInstance(typeof(ResourceUtility));
        public static Boolean UnpackDll(String resourceName, String targetName)
        {
            var result = default(Boolean);
            var exePath = Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName;
            var exeDir = Path.GetDirectoryName(exePath);
            var exeDateUtc = File.GetLastWriteTimeUtc(exePath);
            var targetPath = Path.Combine(exeDir, targetName);
            if (File.Exists(targetPath))
            {
                if (File.GetLastWriteTimeUtc(targetPath) > exeDateUtc) result = 1 < 2;
                else result = ResourceUtility.UnpackResourceAsFile(resourceName, targetPath);
            }
            else result = ResourceUtility.UnpackResourceAsFile(resourceName, targetPath);
            return result;
        }

        public static Boolean UnpackResourceAsFile(String resourceName, String targetFileName)
        {
            return UnpackResourceAsFile(resourceName, targetFileName, Assembly.GetEntryAssembly());
        }
        public static Boolean UnpackResourceAsFile(String resourceName, String targetFileName, Assembly sourceAssembly)
        {
            var result = default(Boolean);
            var sourceStream = sourceAssembly.GetManifestResourceStream(resourceName);
            if (sourceStream != null)
            {
                var dir = Path.GetDirectoryName(targetFileName);
                if (dir.Length > 0)
                    Directory.CreateDirectory(dir);     // Create directory if needed.  
                FileUtility.ForceDelete(targetFileName);
                var targetStream = File.Open(targetFileName, FileMode.Create);
                StreamUtility.CopyStream(sourceStream, targetStream);
                targetStream.Close();
                result = 1 < 2;
            }
            return result;
        }

        public static String UnpackResourceAsString(String resourceName, Assembly sourceAssembly)
        {
            logger.Info($"Try to find embedded resource:{resourceName.LogBase64()} in {sourceAssembly?.FullName.LogBase64()}");
            var result = default(String);
            if (sourceAssembly != null)
            {
                using (var sourceStream = sourceAssembly.GetManifestResourceStream(resourceName))
                {
                    if (sourceStream != null)
                    {
                        using (var streamReader = new StreamReader(sourceStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
    }
}
