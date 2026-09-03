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
using AvePoint.Media.Storage.Cloud.Common;
using System.IO;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.Media.Storage.Cloud.Amazon
{
    #region CodeReview
    [AveCodeReview(
    "2012/7/27",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
     new String[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion
    class AmazonMultipartUploadStream : HttpUploadStream
    {
        static Int32 eachPartSize = 5 * 1024 * 1024; //Each part must be at least 5 MB in size, except the last part.
        AmazonClient client;
        String fullURL;
        MemoryStream mStream = null;
        Byte[] partContent = new Byte[eachPartSize];
        String uploadId;
        Int32 partNumber = 0;
        Dictionary<Int32, String> eTags = new Dictionary<Int32, String>();

        public AmazonMultipartUploadStream(AmazonClient client, String fullURL, Dictionary<String, String> headers)
        {
            this.client = client;
            this.fullURL = fullURL;
            this.uploadId = client.InitiateMultipartUpload(fullURL, headers);
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (mStream == null)
            {
                mStream = new MemoryStream(eachPartSize);
            }
            Int32 freeCapactity = mStream.Capacity - (Int32)mStream.Position;
            while (count >= freeCapactity)
            {
                mStream.Write(buffer, offset, freeCapactity);
                count -= freeCapactity;
                offset += freeCapactity;
                CommitMemoryStream(mStream);
                freeCapactity = mStream.Capacity - (Int32)mStream.Position;
            }
            if (count > 0)
            {
                mStream.Write(buffer, offset, count);
            }
            if (mStream.Capacity - (Int32)mStream.Position == 0)
            {
                CommitMemoryStream(mStream);
            }
        }

        private void CommitMemoryStream(MemoryStream mStream)
        {
            if (mStream.Position > 0)
            {
                mStream.Position = 0;
                Int32 readLen = mStream.Read(partContent, 0, (Int32)mStream.Length);
                String eTag = this.client.UploadPart(fullURL + "?partNumber=" + (++partNumber) + "&uploadId=" + uploadId, partContent, 0, readLen);
                eTags.Add(partNumber, eTag);
                mStream.SetLength(0);
                mStream.Position = 0;
            }
        }

        public override StorageResult Commit(bool closeParent)
        {
            StorageResult storageResult = new StorageResult();
            if (!this.IsCommited)
            {
                this.IsCommited = true;
                if (mStream != null)
                {
                    CommitMemoryStream(mStream);
                    mStream.Close();
                }
                this.client.CompleteMultipartUpload(fullURL + "?uploadId=" + uploadId, this.eTags);
                storageResult.IsCommited = true;
            }
            return storageResult;
        }

        public override StorageResult Commit()
        {
            return Commit(true);
        }
    }
}
