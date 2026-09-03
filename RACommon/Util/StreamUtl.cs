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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class StreamUtl
    {
        public static MemoryStream CopyStreamToMemory(Stream inputStream)
        {
            MemoryStream ret = new MemoryStream();
            const int BUFFER_SIZE = 1024;
            byte[] buf = new byte[BUFFER_SIZE];

            int bytesread = 0;
            while ((bytesread = inputStream.Read(buf, 0, BUFFER_SIZE)) > 0)
                ret.Write(buf, 0, bytesread);

            ret.Position = 0;
            return ret;
        }

        public static byte[] ReadFile(string filePath)
        {
            using FileStream fs = new(filePath, FileMode.OpenOrCreate, FileAccess.Read);
            var buffer = new byte[1024];
            using var ms = new MemoryStream();
            int read;
            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }
}
