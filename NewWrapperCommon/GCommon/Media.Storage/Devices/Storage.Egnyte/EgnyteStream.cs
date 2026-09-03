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


#region reference
using System;
using System.IO;
using System.Net;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Media.Storage.Util;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
#endregion

#region module
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteStream.#InitWriteStream()", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteStream.#InitWriteStream()", MessageId = "fs-content")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteStream.#InitReadStream()", MessageId = "pubapi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Egnyte.EgnyteStream.#InitReadStream()", MessageId = "fs-content")]
#endregion

namespace AvePoint.Media.Storage.Egnyte
{
    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    class EgnyteStream : XStream
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(EgnyteStream));
        FileMode fileMode;
        HttpWebRequest request;
        EgnyteSystem egnyteSystem;
        Stream inputStream;
        Stream outputStream;
        Int64 length;

        public override Int64 Length
        {
            get
            {
                return length;
            }
        }

        internal EgnyteStream(EgnyteSystem sys, StorageInfo storageInfo, FileMode mode)
            : base(sys)
        {
            this.fileMode = mode;
            this.Info = storageInfo;
            this.egnyteSystem = sys;
            if (this.fileMode == FileMode.Open)
            {
                this.InitReadStream();
            }
            else
            {
                this.InitWriteStream();
            }
            this.URI.SysId = sys.SystemID;
            this.URI.SInfo = storageInfo;
            this.URI.SdType = 409;
        }

        void InitWriteStream()
        {
            Thread.Sleep(1000);
            Info.path = PathUtil.CombinePath(this.egnyteSystem.SystemLocation, Info.HighName);
            var url = String.Format(StorageUrl.EgnyteStream, this.egnyteSystem.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(Info.path, Info.LowName)));
            request = EgnyteUtil.GenerateRequest("POST", url, this.egnyteSystem.OpenParameter.Token);
            this.inputStream = request.GetRequestStream();
            logger.Debug("Get request stream succeed.path = {0}", PathUtil.CombinePath(Info.path, Info.LowName));
        }

        void InitReadStream()
        {
            try
            {
                EgnyteUtil.Retry<Boolean>(delegate()
                {
                    this.Info.path = PathUtil.CombinePath(this.egnyteSystem.SystemLocation, this.Info.HighName);
                    var url = String.Format(StorageUrl.EgnyteStream, this.egnyteSystem.OpenParameter.Domain, EgnyteUtil.Encode(PathUtil.CombinePath(this.Info.path, this.Info.LowName)));
                    HttpWebRequest request = EgnyteUtil.GenerateRequest("GET", url, this.egnyteSystem.OpenParameter.Token);
                    //request.Headers.Add("entry_id", this.Info.ObjectId);
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        throw new Exception();
                    }
                    this.outputStream = response.GetResponseStream();
                    this.length = response.ContentLength;
                    //response.Close();
                    //request.Abort();
                    return true;
                },this.egnyteSystem.MaxRetryCount, this.egnyteSystem.RetryInterval);
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Get down stream failed.highName={0},lowName={1}.Message:{2}", Info.HighName, Info.LowName, e.Message));
                throw;
            }
        }

        public override void Close()
        {
            this.CloseStream();
            if (!IsCommited)
            {
                this.Commit();
            }
        }

        public override void Flush()
        {
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (this.fileMode == FileMode.Open)
            {
                throw new Exception();
            }
            try
            {
                this.inputStream.Write(buffer, offset, count);
                logger.Debug("Stream write succeed.");
            }
            catch (Exception e)
            {
                throw new RetryableException(String.Format("Write failed.Message:{0}", e.Message));
            }
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (this.fileMode != FileMode.Open)
            {
                throw new Exception();
            }
            try
            {
                return this.outputStream.Read(buffer, offset, count);
            }
            catch(Exception e)
            {
                throw new RetryableException(String.Format("Read failed.Message:{0}", e.Message));
            }
        }

        void CloseStream()
        {
            try
            {
                if (this.fileMode == FileMode.Open)
                {
                    if (this.outputStream != null)
                    {
                        this.outputStream.Close();
                        this.outputStream = null;
                    }
                }
                else
                {
                    if (this.inputStream != null)
                    {
                        this.inputStream.Close();
                        this.inputStream = null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Close stream error:" + ex.Message);
            }
        }

        void ExcuteRequest(HttpWebRequest request)
        {
            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        logger.Info("Upload succeed.");
                    }
                }
                logger.Debug("Commit succeed. IsCommit = {0}", IsCommited);
            }
            catch (WebException we)
            {
                using (HttpWebResponse response = we.Response as HttpWebResponse)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        logger.Debug(new StreamReader(stream).ReadToEnd());
                    }
                }
                logger.Info("Response headers = {0}", we.Response.Headers.ToString());
                logger.Error("Commit failed:" + we.Message);
                throw;
            }
        }

        public override StorageResult Commit()
        {
            this.CloseStream();
            StorageResult storageResult = new StorageResult();
            if (!IsCommited && this.fileMode != FileMode.Open)
            {
                ExcuteRequest(request);
                this.IsCommited = true;
            }
            storageResult.URI = this.URI;
            return storageResult;
        }

        public override Boolean CanRead
        {
            get { return outputStream.CanRead; }
        }

        public override Boolean CanSeek
        {
            get { return false; }
        }

        public override Boolean CanWrite
        {
            get { return inputStream.CanWrite; }
        }

        public override XURIResult GetURI()
        {
            return this.URI; 
        }
    }
}
