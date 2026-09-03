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
    #region using directives
    using System;
    using System.IO;
    #endregion

    public static class StreamUtility
    {
        public static void CopyFromFile(String fromFilePath, Stream toStream)
        {
            using (Stream fromStream = File.OpenRead(fromFilePath))
                CopyStream(fromStream, toStream);
        }

        public static void CopyToFile(Stream fromStream, String toFilePath)
        {
            using (Stream toStream = File.Create(toFilePath))
                CopyStream(fromStream, toStream);
        }

        /// <summary>
        /// CopyStream simply copies 'fromStream' to 'toStream'
        /// </summary>
        public static Int32 CopyStream(Stream fromStream, Stream toStream)
        {
            var buffer = new byte[8192];
            var totalBytes = 0;
            for (; ; )
            {
                var count = fromStream.Read(buffer, 0, buffer.Length);
                if (count == 0) break;
                toStream.Write(buffer, 0, count);
                totalBytes += count;
            }
            return totalBytes;
        }
    }
}
