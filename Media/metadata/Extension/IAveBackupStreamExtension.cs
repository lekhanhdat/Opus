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

namespace AvePoint.Metadata;
using System;
using System.IO;

public static class IAveBackupStreamExtension
{
    public static Stream ToStream(this IAveBackupStream self)
    {
        return new BackupStreamWrapper(self);
    }


    class BackupStreamWrapper : Stream
    {
        private IAveBackupStream inner;

        public BackupStreamWrapper(IAveBackupStream inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new System.NotImplementedException();

        public override long Position
        {
            get => this.position;
            set => throw new System.NotImplementedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }
        private long position = 0;
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.inner.WriteContent(buffer, offset, count);
            this.position += count;
        }
    }
}