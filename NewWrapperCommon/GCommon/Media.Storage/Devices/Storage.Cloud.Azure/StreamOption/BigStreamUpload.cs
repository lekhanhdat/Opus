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
using System.Text;
using AvePoint.Media.Storage.Cloud.Common;
using System.Net;
using System.IO;
using AvePoint.Media.Storage.Util;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Contract.CodeReview;



namespace AvePoint.Media.Storage.Cloud.Azure
{
    class BigDBHttpUploadStream : HttpUploadStream
    {
        private AzureClient azureclient;
        private IChangeStream cs;
        //private long rangeMark;
        private long length;
        public override long Length { get { return length; } }
        //private HttpWebRequest request;
        private long begin = 0;
        private long end = 0;
        private long mark = 0;
        private long totalContentLength ;
        private long totalContentLengthTemp;
        private long total = 0;
        private long canWriteMark = 0;
        private long realContentLength = 0;

        public BigDBHttpUploadStream(HttpWebRequest request, AzureClient azureclient , IChangeStream cs)
            : base(request)
        {
            this.azureclient = azureclient;
            this.cs = cs;
            this.HttpWebRequest = request;
            totalContentLength = cs.GetTotalContentLength();
            realContentLength = cs.GetRealContentLength();
            totalContentLengthTemp = cs.GetTotalContentLength();
            begin = 0;
            end = 4194303;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {         
                mark += count; 
                canWriteMark = mark;
                if (mark > 4194304)
                {
                    try
                    {
                        if ((mark - count) != 4194304)
                        {
                            int tempMark = 4194304 - ((int)mark - count);
                            InnerStream.Write(buffer, 0, tempMark);
                            totalContentLengthTemp -= tempMark;
                            total += tempMark;
                            offset = tempMark;
                            count -= offset;
                        }
                        try
                        {
                            InnerStream.Close();
                            HttpWebResponse r = HttpWebRequest.GetResponse() as HttpWebResponse;
                            int code = (int)r.StatusCode;
                            HttpWebRequest.Abort();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e.Message, e);
                            throw ;
                        }
                            mark = count;
                            canWriteMark = mark;
                            if (totalContentLengthTemp <= 4194304)
                            {
                              //  type = 1;
                                int temp512Nmn = (int)totalContentLengthTemp / 512;
                                begin = end + 1;
                                if (totalContentLengthTemp % 512 != 0)
                                    end += (temp512Nmn + 1) * 512;
                                else
                                    end += temp512Nmn * 512;
                                HttpWebRequest = cs.ChangeHttpUploadStream(begin, end, 1);
                                InnerStream = new BufferedStream(HttpWebRequest.GetRequestStream(),  64 * 1024);
                            }
                            else
                            {
                                begin += 4194304;
                                end = begin + 4194304 - 1;
                                HttpWebRequest = cs.ChangeHttpUploadStream(begin, end, 1);
                                InnerStream = new BufferedStream(HttpWebRequest.GetRequestStream(), 64 * 1024);
                               // endWrite = 0;
                            }
                        }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message,e);
                        throw ;
                    }
                }
            InnerStream.Write(buffer, offset, count);
            totalContentLengthTemp -= count;
            total += count;
            byte[] buffers = null;
            if (total >= realContentLength)
            {
                    buffers = new byte[totalContentLengthTemp];
                    while (totalContentLengthTemp-- > 0)
                    {
                        buffers[totalContentLengthTemp] = (byte)'\0';
                    }
                   InnerStream.Write(buffers, 0, buffers.Length);
            }    
        }
    }

    #region CodeReview
    [AveCodeReview(
    "2012/7/27",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
     null,
     true)]
    #endregion
    class BlockBlobUploadStream : HttpUploadStream
    {

        private static int eachBlockSize = 4 * 1024 * 1024;
        private AbstractStreamOption asOption;

        private MemoryStream mStream = null;
        private byte[] blockContent = new byte[eachBlockSize];

        int blockId = 0;
        List<string> blockIdBase64s = new List<string>();

        public BlockBlobUploadStream(AbstractStreamOption asOption)
            : base(null)
        {
            this.asOption = asOption;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mStream == null)
            {
                mStream = new MemoryStream(eachBlockSize);
            }

            int freeCapactity = mStream.Capacity - (int)mStream.Position;

            while (count >= freeCapactity)
            {
                mStream.Write(buffer, offset, freeCapactity);
                count -= freeCapactity;
                offset += freeCapactity;
                CommitMemoryStream(mStream);
                freeCapactity = mStream.Capacity - (int)mStream.Position;
            }
            if (count > 0)
            {
                mStream.Write(buffer, offset, count);
            }

            if (mStream.Capacity - (int)mStream.Position == 0)
            {
                CommitMemoryStream(mStream);
            }

        }

        private void CommitMemoryStream(MemoryStream mStream)
        {
            if (mStream.Position > 0)
            {
                mStream.Position = 0;

                int readLen = mStream.Read(blockContent, 0, (int)mStream.Length);

                byte[] blockIdBytes = BitConverter.GetBytes(blockId);
                string blockIdBase64 = Convert.ToBase64String(blockIdBytes);
                asOption.Azureclient.PutBlock(asOption.FullURL, blockIdBase64, blockContent, 0, readLen);
                blockIdBase64s.Add(blockIdBase64);
                blockId++;
                mStream.SetLength(0);
                mStream.Position = 0;
            }
            //mStream.Close();
            //mStream = null;
        }

        public override StorageResult Commit(bool closeParent)
        {
            StorageResult rs = rs = new StorageResult();
            if (!this.IsCommited)
            {
                this.IsCommited = true;
                if (mStream != null)
                {
                    CommitMemoryStream(mStream);
                    mStream.Close();
                }
                asOption.Azureclient.PutBlockList(asOption.FullURL, blockIdBase64s);
                asOption.Azureclient.SetBlobMetadata(asOption.FullURL, this.Info.MetaInfos);
                rs.IsCommited = true;
            }
            return rs;
        }

        public override StorageResult Commit()
        {
            return Commit(true);
        }
    }
      
}
