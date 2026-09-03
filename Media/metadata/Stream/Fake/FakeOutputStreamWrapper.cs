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

namespace AvePoint.Metadata
{
    using System.IO;
    public class FakeOutputStreamWrapper : IOutputStreamWrapper
    {
        private string dir;

        public FakeOutputStreamWrapper(string dir)
        {
            this.dir = dir;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }

        public void WriteContent(byte[] buffer, int offset, int count)
        {
            InnerWrite("content.txt", buffer, offset, count);
        }

        public void WriteHead(string xml)
        {
            InnerWrite("head.txt", xml);
        }

        public void WriteMetadata(byte[] buffer, int offset, int count)
        {
            InnerWrite("metadata.txt", buffer, offset, count);
        }

        public void WriteTail(string xml)
        {
            InnerWrite("tail.txt", xml);
        }

        public void WriteTail(string xml, bool isOK)
        {
            WriteTail(xml);
        }

        public void InnerWrite(string fileName, string body)
        {
            File.AppendAllText(Path.Combine(dir, fileName), body);
        }

        public void InnerWrite(string fileName, byte[] buffer, int offset, int count)
        {
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Append);
            stream.Write(buffer, offset, count);
        }

    }
}