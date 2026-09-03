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
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.FileTransfer;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.Common
{
    public sealed class AveCommonRestoreStream : WrapperRestoreStreamBase
    {
        public IFileReceiver FileReceiver
        {
            get;
            private set;
        }

        public AveCommonRestoreStream(IFileReceiver iReceiver)
            : base(new FileReceiverWrapper(iReceiver))
        {
            this.FileReceiver = iReceiver;
        }

        public override string ReadTail()
        {
            byte[] temp = new byte[1024];
            while (this.FileReceiver.ReadBytes(temp, temp.Length) != 0) ;
            return this.FileReceiver.GetFileTail();
        }

        public override string ReadHead()
        {
            Reset();
            return this.FileReceiver.GetNextFileHead();
        }

        protected override void InitInternalRestoreStream()
        {
            var buffer = new byte[16];
            int ret = stream.ReadMetadata(buffer, 0, buffer.Length);
            if (ret > 0)
            {
                var header = new HeaderV0(buffer);
                internalStream = new AveInternalRestoreStream(stream, header.ToV1Header());
            }
        }
    }
}
