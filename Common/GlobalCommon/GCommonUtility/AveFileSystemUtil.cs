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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.IO;
    using System.Text;
    #endregion

    /// <summary>
    /// Identify some file system functions
    /// </summary>
    public class AveFileSystemUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="netShareLocation"></param>
        /// <returns></returns>
        public static bool IsLocalShareLocation(string netShareLocation)
        {
            if (String.IsNullOrEmpty(netShareLocation))
            {
                throw new ArgumentNullException("NetShare Location is empty.");
            }
            if (!netShareLocation.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            var tempNetShareLocation = netShareLocation.Substring(2);
            String[] tempNetShareParts = tempNetShareLocation.Split('\\');
            if (tempNetShareParts.Length < 2)
            {
                throw new ArgumentException(string.Format("NetShare Location:{0} is not valid.", netShareLocation));
            }
            String computer = tempNetShareParts[0];
            if (computer.Length == 0)
            {
                throw new ArgumentException(string.Format("NetShare Location:{0} is not valid.", netShareLocation));
            }

            return AveNetworkingUtil.IsLocalAddress(computer);
        }

        private static long GetCurrentDirectorySize(string dirName)
        {
            long totalSize = 0;
            string[] files = Directory.GetFiles(dirName);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                totalSize += fi.Length;
            }
            return totalSize;
        }

        public static long GetDirectorySize(string dirName, bool includeSubDirectorys)
        {
            long totalSize = 0;
            totalSize = GetCurrentDirectorySize(dirName);
            if (includeSubDirectorys)
            {
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string dir in dirs)
                {
                    totalSize += GetDirectorySize(dir, includeSubDirectorys);
                }
            }
            return totalSize;
        }

        /// <summary>
        /// get disk space
        /// </summary>
        /// <param name="lpDirectoryName">directory name</param>
        /// <param name="freeBytesAvailableToCaller">free bytes the caller can use (disk quota)</param>
        /// <param name="totalBytesAvailableToCaller">total bytes the caller can use (disk quota)</param>
        /// <param name="freeBytesAvailableOnDisk">total free bytes of disk</param>
        /// <returns></returns>
        public static bool GetDiskSpace(string lpDirectoryName, out ulong freeBytesAvailableToCaller, out ulong totalBytesAvailableToCaller, out ulong freeBytesAvailableOnDisk)
        {
            return Win32Native.GetDiskFreeSpaceEx(lpDirectoryName, out freeBytesAvailableToCaller, out totalBytesAvailableToCaller, out freeBytesAvailableOnDisk);
        }
        
        /// <summary>
        /// Get 8.3 Path Name
        /// </summary>
        /// <param name="longName">Full Path Name</param>
        /// <returns></returns>
        public static string GetShortPathName(string longName)
        {
            StringBuilder shortNameBuffer = new StringBuilder(256);
            int bufferSize = shortNameBuffer.Capacity;

            int result = Win32Native.GetShortPathName(longName, shortNameBuffer, bufferSize);

            return shortNameBuffer.ToString();
        }
    }
}
