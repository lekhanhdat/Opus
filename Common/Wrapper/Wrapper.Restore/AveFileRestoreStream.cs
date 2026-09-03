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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveFileRestoreStream : IAveRestoreStream, IDisposable
    {

        private FileStream mStream;

        public AveFileRestoreStream(FileStream fs)
        {
            mStream = fs;
        }

        public void Dispose()
        {
            mStream.Close();
        }

        public void Seek(long posit)
        {
            mStream.Seek(posit, SeekOrigin.Begin);
        }

        public long GetPosit()
        {
            return mStream.Position;
        }

        #region IAveFileReceiver Members

        public byte[] DataBuffer
        {
            get { throw new Exception("This method needs implementation "); }
        }
        public string ReadTail()
        {
            throw new Exception("This method needs implementation ");
        }

        public string ReadHead()
        {
            throw new Exception("This method needs implementation ");
        }

        public void Reset()
        {
            throw new Exception("This method needs implementation ");
        }

        public AveMetadata ReadMetadata()
        {
            throw new Exception("This method needs implementation ");
        }

        public AveMetadata TryReadMetadata(AveMetadataType metadataName)
        {
            throw new Exception("This method needs implementation ");
        }

        public long ContentLength
        {
            get
            {
                throw new Exception("This method needs implementation");
            }
        }

        public int ReadContent(byte[] buffer, int offset, int length)
        {
            throw new Exception("This method needs implementation ");
        }

        public List<AveMetadata> TryReadMetadataList(AveMetadataType metadataName)
        {
            throw new NotImplementedException();
        }

        #endregion


        public long CurrentNodeTransferedSize
        {
            get { throw new NotImplementedException(); }
        }
    }
}
