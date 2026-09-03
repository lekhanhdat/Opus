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



namespace AvePoint.Media.Storage.FTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net.Sockets;
    using System.IO;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Diagnostics;
    using System.Net;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/29",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_2 },
    "ADO-26069",
    true)]
    #endregion

    class FTPStream : XStream
    {
        FtpClient ftpClient;
        FileMode streamMode;
        Stream input;
        Stream output;
        StorageLogger logger = new StorageLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public FTPStream(FtpClient ftpClient, StorageInfo info, AbstractXSystem sys)
            : base(sys)
        {
            this.ftpClient = ftpClient;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
        }

        public void InitStream(FileMode mode)
        {
            streamMode = mode;
            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.OpenOrCreate:
                    InitWriteStream(this.Info);
                    break;
                case FileMode.Open:
                    InitReadStream(this.Info);
                    break;
                case FileMode.Append:
                    InitAppendWriteStream(this.Info);
                    break;
                case FileMode.Truncate:
                default:
                    throw new Exception("Unsupported access type.");
            }
        }
        public void InitAppendWriteStream(StorageInfo info)
        {
            try
            {
                logger.Info("append stream for file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
                if (!ftpClient.CheckFile(PathUtil.CombinePath(info.HighName, info.LowName)))
                {
                    if (!ftpClient.CheckDirectory(info.HighName))
                    {
                        ftpClient.MakeDirectory(info.HighName);
                    }
                }
                CloseOutputStream();
                output = this.ftpClient.GetAppendStream(PathUtil.CombinePath(info.HighName, info.LowName));
                if (output == null)
                {
                    logger.Error("Init ftp append stream error {0}", this.Info);
                    throw new Exception("Init ftp append stream error ");
                }
            }
            catch (WebException e)
            {
                logger.Error("FtpStream initAppendWriteStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                throw;
            }
            catch (Exception e)
            {
                logger.Error("Init ftp append stream error : {0}", e.ToString());
                throw;
            }
        }
        public void InitReadStream(StorageInfo info)
        {
            try
            {
                logger.Info("open stream for file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
                if (!ftpClient.CheckFile(PathUtil.CombinePath(info.HighName, info.LowName)))
                {
                    throw new Exception("storage node can not be found " + this.Info);
                }
                CloseInputStream();
                input = ftpClient.GetDownloadStream(PathUtil.CombinePath(info.HighName, info.LowName), info.Offset);
                if (input == null)
                {
                    logger.Error(string.Format("Init ftp download stream error {0}", this.Info));
                    throw new Exception("Init ftp download stream error ");
                }
            }
            catch (WebException e)
            {
                logger.Error("FtpStream initReadStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public void InitWriteStream(StorageInfo info)
        {
            try
            {
                logger.Info("create stream for file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
                if (!ftpClient.CheckFile(PathUtil.CombinePath(info.HighName, info.LowName)))
                {
                    if (!ftpClient.CheckDirectory(info.HighName))
                    {
                        ftpClient.MakeDirectory(info.HighName);
                    }
                }
                else
                {
                    ftpClient.DeleteFile(info.HighPlusLowName);
                }
                CloseOutputStream();
                output = this.ftpClient.GetUploadStream(PathUtil.CombinePath(info.HighName, info.LowName));
                if (output == null)
                {
                    logger.Error(string.Format("Init ftp upload stream error {0}", this.Info));
                    throw new Exception("Init ftp upload stream error ");
                }
            }
            catch (WebException e)
            {
                logger.Error("FtpStream initWriteStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }
        public override bool CanRead
        {
            get { return input.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return output.CanWrite; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return this.Info.Length; }
        }

        public override long Position
        {
            get
            {
                return this.ReadLength;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            Int64 startTicks = DateTime.UtcNow.Ticks;
            Int32 readLen = 0;
            try
            {
                readLen = input.Read(buffer, offset, count);
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
                return readLen;
            }
            catch (Exception e)
            {
                logger.Error("Read file {0} failed, error message: {1}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e.ToString());
                throw;
            }
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                lock (output)
                {
                    Int64 startTicks = DateTime.UtcNow.Ticks;
                    output.Write(buffer, offset, count);
                    System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                    System.IncreaseTotalWriteBytes(count);
                }
            }
            catch (Exception e)
            {
                logger.Error("Write file {0} failed, error message: {1}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e.Message);
                throw;
            }
        }
        public void CloseOutputStream()
        {
            if (output != null)
            {
                output.Close();
                output = null;
            }
        }
        public void CloseInputStream()
        {
            try
            {
                if (input != null)
                {
                    input.Close();
                    input = null;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
        }

        public override void Close()
        {
            switch (streamMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Append:
                    CloseOutputStream();
                    break;
                case FileMode.Open:
                    CloseInputStream();
                    break;
                case FileMode.Truncate:
                default:
                    throw new Exception("Unsupported access type.");
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
