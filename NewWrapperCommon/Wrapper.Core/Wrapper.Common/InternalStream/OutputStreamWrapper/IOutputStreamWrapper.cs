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

namespace AvePoint.Wrapper.Common
{
    public interface IOutputStreamWrapper
    {
        /// <summary>
        /// 写入Content数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void WriteContent(byte[] buffer, int offset, int count);
        /// <summary>
        /// 写入Metadata数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void WriteMetadata(byte[] buffer, int offset, int count);
        void WriteHead(string xml);
        void WriteTail(string xml);
        void WriteTail(string xml, bool isOK);
    }

    internal static class IOutputStreamWrapperExtension
    {
        public static void CopyFrom(this IOutputStreamWrapper toStream, System.IO.Stream fromStream)
        {
            const int BUFFER_SIZE = 64 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            var currentCount = -1;
            while ((currentCount = fromStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
            {
                toStream.WriteMetadata(buffer, 0, currentCount);
            }
        }
    }

}
