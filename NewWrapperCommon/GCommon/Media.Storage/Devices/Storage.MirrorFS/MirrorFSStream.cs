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

namespace AvePoint.Media.Storage.MirrorFS
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Util;
    using System.Diagnostics; 
    #endregion

    class MirrorFSStream : XStream
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(MirrorFSStream));
        private Dictionary<int, List<IXSystem>> innerSystems = new Dictionary<int,List<IXSystem>>();
        private FileMode mode = FileMode.Open;
        private int totalRead = 0;
        private XStream stream = null;
        private int i = 0;
        const string Temp_file_Location = @"..\temp\raid_temp";
        string tmpFilePath;
        const long MaxCacheStreamLength = 1024 * 1024 * 8;
        private Stream cacheStream;

        public MirrorFSStream(StorageInfo info, FileMode mode, MirrorFSSystem mirrorSys)
            : base(mirrorSys)
        {
            this.mode = mode;
            this.Info = info;
            innerSystems = mirrorSys.InnerSystems;
            this.URI.SdType = 6;
            this.URI.SysId = mirrorSys.SystemID;
            this.URI.SInfo = info.Clone();
        }

        public void InitStream()
        {
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    InitWriteStream();
                    break;
                case FileMode.Open:
                    stream = GetNextCanOpenStream();
                    break;
                default:
                    throw new Exception("Current mode is " + mode + " ,Unsupported access type.");
            }
        }

        private void InitWriteStream()
        {
            if (!Directory.Exists(Temp_file_Location))
            {
                Directory.CreateDirectory(Temp_file_Location);
            }
            tmpFilePath = PathUtil.CombinePath(Temp_file_Location, Guid.NewGuid().ToString());
            if (this.Info.Length > MaxCacheStreamLength || this.Info.Length <= 0)
            {
                this.cacheStream = new FileStream(tmpFilePath, FileMode.OpenOrCreate);
            }
            else
            {
                this.cacheStream = new MemoryStream();
            }
        }
        
        /// <summary>
        /// GetNextCanOpenStream
        /// </summary>
        /// <returns></returns>
        private XStream GetNextCanOpenStream()
        {
            bool isFindOut = false;
            foreach (List<IXSystem> group in innerSystems.Values)
            {
                foreach (IXSystem sys in group)
                {
                    try
                    {
                        if (sys.SystemHealth < XSystemHealth.Available)
                        {
                            sys.Validate();
                            if (sys.SystemHealth < XSystemHealth.Available)
                            {
                                continue;
                            }
                        }
                        XFileInfo fileInfo = sys.OpenFile(this.Info);
                        if (fileInfo == null || !fileInfo.Exists || fileInfo.FileSize < Info.Offset + Info.Length)
                        {
                            continue;
                        }
                        StorageInfo sInfo = this.Info.Clone();
                        sInfo.Offset = this.Info.Offset + totalRead;
                        sInfo.Length = this.Info.Length - totalRead;
                        stream = sys.OpenStream(sInfo, mode);
                        isFindOut = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(ex.Message);
                        if (mode != FileMode.Open)
                        {
                            throw;
                        }
                        else
                        {
                            i++;
                            stream = GetNextCanOpenStream();
                        }
                    }
                }
                if (isFindOut)
                {
                    break;
                }
            }
            if (stream == null)
            {
                throw new FileNotFoundException("Can not find the file, path is : " + Info.HighPlusLowName);
            }
            return stream;
        }


        /// <summary>
        /// Flush
        /// </summary>
        public override void Flush()
        {
            if (stream != null)
            {
                stream.Flush();
            }
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {   
                readLen = stream.Read(buffer, offset, count);
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
                totalRead = totalRead + readLen;
                return readLen;
            }
            catch (Exception t)
            {
                i++;
                stream = GetNextCanOpenStream();
                this.Read(buffer, offset, count);
                logger.Error(t.Message, t);
            }
            return readLen;
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            this.cacheStream.Write(buffer, offset, count);
            System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
            System.IncreaseTotalWriteBytes(count);
        }

        public override StorageResult Commit()
        {
            if (IsCommited)
            {
                throw new Exception("can't commit stream more than one time, file:" + this.Info.HighPlusLowName);
            }
            StorageResult rs = this.System.CommitStream(this.cacheStream, this.Info);
            this.IsCommited = true;
            return rs;
        }

        /// <summary>
        /// Close
        /// </summary>
        public override void Close()
        {
            if (!this.IsCommited)
            {
                switch (mode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Append:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                        Commit();
                        break;
                    case FileMode.Open:
                    default:
                        break;
                }
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (this.cacheStream != null)
            {
                this.cacheStream.Close();
            }
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }
        }

        /// <summary>
        /// GetURI
        /// </summary>
        /// <returns></returns>
        public override XURIResult GetURI()
        {
            return this.URI;
        }

        public override bool CanRead
        {
            get
            {
                if (stream == null)
                {
                    return cacheStream.CanRead;
                }
                else
                {
                    return stream.CanRead;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (stream == null)
                {
                    return cacheStream.CanSeek;
                }
                else
                {
                    return stream.CanSeek;
                }
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (stream == null)
                {
                    return cacheStream.CanWrite;
                }
                else
                {
                    return stream.CanWrite;
                }
            }
        }

        public override long Position
        {
            get
            {
                if (stream == null)
                {
                    return cacheStream.Position;
                }
                else
                {
                    return stream.Position;
                }
            }
            set
            {
                if (stream == null)
                {
                    cacheStream.Position = value;
                }
                else
                {
                    stream.Position = value;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(stream == null)
            {
                return cacheStream.Seek(offset, origin);
            }
            else
            {
                return stream.Seek(offset, origin);
            }
        }

        public override long Length
        {
            get
            {
                if (stream == null)
                {
                    if (this.Info.Length > 0)
                    {
                        return this.Info.Length;
                    }
                    return cacheStream.Length;
                }
                else
                {
                    if (this.Info.Length > 0)
                    {
                        return this.Info.Length;
                    }
                    return stream.Length;
                }
            }
        }
    }
}
