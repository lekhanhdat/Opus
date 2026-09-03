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

using System.IO;

namespace AvePoint.Metadata;
public interface IOutputStreamWrapper
{
    void WriteHead(string xml);
    void WriteMetadata(byte[] buffer, int offset, int count);
    void WriteContent(byte[] buffer, int offset, int count);
    void WriteTail(string xml);
    void WriteTail(string xml, bool isOK);
}

internal static class IOutputStreamWrapperExtension
{
    public static void CopyFrom(this IOutputStreamWrapper toStream, Stream fromStream, long startIndex, long length)
    {
        fromStream.Position = startIndex;
        const int BUFFER_SIZE = 64 * 1024;
        byte[] buffer = new byte[BUFFER_SIZE];
        var readBytes = 0;
        var currentReadBytes = -1;
        while ((currentReadBytes = fromStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
        {
            readBytes += currentReadBytes;
            if (readBytes >= length)
            {
                currentReadBytes -= (int)(readBytes - length);
                toStream.WriteMetadata(buffer, 0, currentReadBytes);
                break;
            }
            toStream.WriteMetadata(buffer, 0, currentReadBytes);
        }
    }
}