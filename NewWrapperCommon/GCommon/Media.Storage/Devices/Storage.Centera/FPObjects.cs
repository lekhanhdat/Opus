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



namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.IO;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Util;
    using System.Web;
    using System.Diagnostics;
    #endregion

    class FPObject : IDisposable
    {
        protected ulong objRef { get; set; }
        public ulong Ref
        {
            get { return objRef; }
        }

        public void Dispose()
        {
            Close();
        }
        public virtual void Close() { }
    }

    class FPPool : FPObject
    {

        Dictionary<string, FPClip> mClips = new Dictionary<string, FPClip>();

        public FPPool(string poolAddress)
        {
            this.objRef = Pool.Open(poolAddress);
            //Pool.SetOption(this.objRef, FPOption.BUFFERSIZE, 1024 * 1000);
        }
        public FPPool(ulong poolRef)
        {
            this.objRef = poolRef;
            //Pool.SetOption(this.objRef, FPOption.BUFFERSIZE, 1024 * 1000);
        }
        private AveLogger logger = AveLogger.GetInstance(typeof(FPPool));
        public FPClip CreateClip(string clipName)
        {
            ulong clipRef = Clip.Create(objRef, clipName);
            logger.Debug(string.Format("Clip Created, ID : {0}, Name: {1}", clipRef, clipName));
            FPClip newClip = new FPClip(clipRef);
            return newClip;
        }

        public FPClip OpenClip(string clipId)
        {
            ulong clipRef = Clip.Open(objRef, clipId, FPClipOpenMode.FP_OPEN_FLAT);
            FPClip clip = new FPClip(clipRef);
            return clip;
        }

        private FPTag FetchTag(string clipId, string tagName)
        {

            string preClipId = clipId;
            while (!string.IsNullOrEmpty(preClipId))
            {
                FPClip clip = OpenClip(preClipId);
                FPTag tag = clip.FetchTag(tagName);
                if (tag != null)
                {
                    return tag;
                }
                preClipId = clip.GetClipMeta("PREVIOUS_CLIP_ID");
            }
            return null;
        }

        public void RemoveClip(string clipId)
        {
            FPClip clip = null;
            if (mClips.ContainsKey(clipId))
            {
                clip = mClips[clipId];
                clip.Close();
                mClips.Remove(clipId);
            }
        }

        public override void Close()
        {
            FPClip clip = null;
            try
            {
                foreach (string clipId in mClips.Keys)
                {
                    clip = mClips[clipId];
                    clip.Close();
                }
                mClips = new Dictionary<string, FPClip>();
                if (this.objRef != 0)
                {
                    Pool.Close(objRef);
                    this.objRef = 0;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        public FPPoolInfo GetInfo()
        {
            return Pool.GetPoolInfo(objRef);
        }
        public Boolean GetPermisson(FPOption CapabilityName, FPOption inCapabilityAttributeName)
        {
            if (Pool.GetPoolPermisson(objRef, CapabilityName.ToString(), inCapabilityAttributeName.ToString()) == "true")
            {
                return true;
            }
            return false;
        }
        public static void RegisterApplication(string appName, string appVersion)
        {
            Pool.RegisterApplication(appName, appVersion);
        }

        /// <summary>
        /// 这里需要做成可配置的
        /// </summary>
        /// <param name="option"></param>
        /// <param name="value"></param>
        public static void SetGlobalOption(FPOption option, int value)
        {
            Pool.SetGlobalOption(option, value);
        }

        public int GetOption(FPOption option)
        {
            return Pool.GetOption(this.objRef, option);
        }

        public void SetOption(FPOption option, int optionValue)
        {
            Pool.SetOption(this.objRef, option, optionValue);
        }

        public void DeleteClip(string clipId)
        {
            Clip.Delete(objRef, clipId);
        }

        public bool ExistsClip(string clipId)
        {
            int rs = Clip.Exists(objRef, clipId);
            if (rs == 1)
            {
                return true;
            }
            return false;
        }


    }

    class FPClip : FPObject
    {

        public const int MAX_TAG_NUMBER = 10;
        public const string PREVIOUS_CLIP_ID = "PREVIOUS_CLIP_ID";
        bool writed;
        bool notifiedWrite;
        string clipId;
        //FPTag topTag;
        bool newCreated;
        public bool FPObjCacheEnable { get; set; }
        Dictionary<string, FPTag> tagList = new Dictionary<string, FPTag>();

        public bool NewCreated
        {
            get { return newCreated; }
            set { this.newCreated = value; }
        }


        private AveLogger logger = AveLogger.GetInstance(typeof(FPClip));



        public FPTag GetTopTag()
        {
            ulong tagRef = Clip.GetTopTag(this.objRef);
            //logger.Debug(string.Format("Tag Top, Opened : {0}", tagRef));
            FPTag topTag = new FPTag(tagRef, this, 0);
            return topTag;
        }

        public FPTag FetchNext()
        {
            ulong tagRef = Clip.FetchNext(this.objRef);
            if (tagRef > 0)
            {
                FPTag tag = new FPTag(tagRef, this, 0);
                return tag;
            }
            return null;
        }

        public ulong GetTotalSize()
        {
            if (this.objRef != 0)
            {
                return Clip.GetTotalSize(this.objRef);
            }
            else
            {
                throw new Exception("clip can not be null");
            }
        }
        public void SetRetentionPeriod(UInt64 retentionSecs)
        {
            if (retentionSecs > 0)
            {
                if (this.objRef != 0)
                {
                    Clip.SetRetentionPeriod(this.objRef, retentionSecs);
                }
                else
                    throw new Exception("clip can not be null");
            }
        }

        public UInt64 GetRetentionPeriod()
        {
            var result = Clip.FPClip_GetRetentionPeriod(this.objRef);
            return result;
        }
        //public FPTag nextTag()
        //{


        //    ulong tagRef = Clip.FetchNext(this.objRef);
        //    if (tagRef == 0)
        //    {
        //        return null;
        //    }
        //    topTag = new FPTag(tagRef, this, 0);
        //    //return tag;

        //    return topTag;

        //}
        public bool Writed { get { return this.writed; } }
        public bool NotifiedWrite { get { return this.notifiedWrite; } set { this.notifiedWrite = value; } }

        public string ClipId
        {
            get { return this.clipId; }
            set { this.clipId = value; }
        }

        public FPClip(ulong clipRef)
        {
            this.objRef = clipRef;
        }

        public void RawRead(FPStream stream)
        {
            Clip.RawRead(this.objRef, stream.Ref);
        }

        public override void Close()
        {
            try
            {
                if (FPObjCacheEnable)
                {
                    foreach (string key in tagList.Keys)
                    {
                        try
                        {
                            tagList[key].Close();
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceWarning(ex.Message);
                        }
                    }
                    tagList.Clear();
                    tagList = null;
                }
                //tagList = new Dictionary<string, FPTag>();
                //if (topTag != null)
                //{
                //    logger.Debug(string.Format("Tap Top, Closed : {0}", topTag.Ref));
                //    topTag.Close();
                //    topTag = null;
                //}
                if (this.objRef != 0)
                {
                    Clip.Close(this.objRef);
                    logger.Debug(string.Format("Clip, Closed : {0}", objRef));
                    this.objRef = 0;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        //public FPTag GetTopTag()
        //{
        //    ulong tagRef = Clip.GetTopTag(this.objRef);
        //    FPTag tag = new FPTag(tagRef, this);
        //    return tag;
        //}

        public FPTag FetchTag(string tagName)
        {
            FPTag tag;
            while ((tag = FetchNext()) != null)
            {
                if (tagName.Equals(tag.Name))
                {
                    break;
                }
                tag.Dispose();
                tag = null;
            }
            return tag;
        }

        private object locker = new object();

        public FPTag OpenTag(string tagName)
        {
            //if (FPObjCacheEnable)
            //{
            //    if (tagList.ContainsKey(tagName))
            //    {
            //        return tagList[tagName];
            //    }
            //    else
            //    {
            //        FPTag tag;
            //        while ((tag = FetchNext()) != null)
            //        {
            //            lock (locker)
            //            {
            //                tagList[tag.Name] = tag;
            //            }
            //            if (tagName.Equals(tag.Name))
            //            {
            //                break;
            //            }
            //        }
            //        return tagList[tagName];
            //    }
            //}
            //else
            //{
            FPTag tag;
            if ((tag = FetchTag(tagName)) == null)
            {
                FPTag top = GetTopTag();
                if (tagName.Equals(top.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return top;
                }
                top.Dispose();
                tag = FetchTag(tagName);
            }
            return tag;
            //}


            //tagName = HttpUtility.UrlEncode(tagName).Replace("+", "%20").Replace("/", "%2F");
            //if (tagList.ContainsKey(tagName)) 
            //{
            //    if (tagList[tagName].Exist())
            //    {
            //        return tagList[tagName];
            //    }
            //}
            //FPTag tag = TopTag.GetFirstChild();
            //if (tag != null && tag.Exist())
            //{
            //    if (tagName.Equals(tag.Name))
            //        {
            //            return tag;
            //        }
            //    FPTag temp;
            //    while ((temp = tag.GetSibling()) != null && temp.Exist())
            //    {
            //        if (tagName.Equals(temp.Name))
            //        {
            //            tag.Close();
            //            tag = null;
            //            return temp;
            //        }
            //        tag.Close();
            //        tag = null;
            //        tag = temp;
            //    }
            //}
        }

        private Dictionary<string, FPTag> mNewTags = new Dictionary<string, FPTag>();

        public FPTag CreateTag(string tagName, int sequence)
        {
            FPTag topTag = GetTopTag();
            FPTag tag = topTag.Create(tagName, sequence);
            topTag.Close();
            return tag;
        }

        private int sequence;
        public int Sequence { get { return sequence; } }
        public FPTag CreateTag(string tagName)
        {
            FPTag topTag = GetTopTag();
            FPTag tag = topTag.Create(tagName, sequence++);
            topTag.Close();
            return tag;
        }

        public string Write()
        {
            this.clipId = Clip.Write(this.objRef);
            this.writed = true;
            return this.clipId;
        }

        public string GetClipMeta(string meta)
        {
            return Clip.GetAttribute(this.objRef, meta);
        }

        public bool UpdateClipMeta(string key, string value)
        {
            try
            {
                Clip.SetAttribute(this.objRef, key, value);
                return true;
            }
            catch (FPException e)
            {
                logger.Error("UpdateClipMeta failed" + e.ToString());
                return false;
            }
        }
    }

    class FPTag : FPObject
    {
        FPClip clip;
        string name;
        int sequence;
        long length = -1;
        public const string ORIGINAL_NAME = "ORIGINAL_NAME";
        object obj = new object();
        private static AveLogger logger = AveLogger.GetInstance(typeof(FPTag));

        public bool Exist()
        {
            if (this.objRef == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void CheckState()
        {
            if (this.objRef == 0 && !string.IsNullOrEmpty(this.name))
            {
                this.objRef = this.clip.OpenTag(this.name).objRef;
            }
        }

        public FPClip Clip
        {
            get { return clip; }
        }

        public int Sequence
        {
            get { return this.sequence; }
        }

        public string Name
        {
            get
            {
                lock (obj)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        name = Tag.GetName8(objRef);
                    }
                    return name;
                }
            }
            set
            {
                this.name = value;
            }
        }

        public long Length
        {
            get
            {
                if (this.length == -1)
                {
                    length = Tag.GetBlobSize(this.objRef);
                }
                return this.length;
            }
        }

        public FPTag(ulong tagRef, FPClip clip, int sequence)
        {
            this.objRef = tagRef;
            this.clip = clip;
            this.sequence = sequence;
        }

        public string GetTagMeta(string meta)
        {
            return Tag.GetAttribute(this.objRef, meta);
        }

        public bool UpdateTagMeta(string key, string value)
        {
            try
            {
                Tag.SetAttribute(this.objRef, key, value);
                return true;
            }
            catch (FPException e)
            {
                logger.Warn("UpdateTagMeta failed" + e.ToString());
                return false;
            }
        }
        //public FPTag(ulong tagRef)
        //{
        //    this.objRef = tagRef;
        //}

        public FPTag Create(string tagName, int sequence)
        {
            //tagName = HttpUtility.UrlEncode(tagName).Replace("+", "%20").Replace("/", "%2F");
            ulong tagRef = Tag.Create(objRef, tagName);
            logger.Debug(string.Format("Tag, Created : {0}, Name : {1}", tagRef, tagName));
            FPTag tag = new FPTag(tagRef, clip, sequence);
            return tag;
        }

        public void BlobWrite(FPStream stream)
        {
            Tag.BlobWrite(this.objRef, stream.Ref, 0);
        }

        public void BlobWritePartial(FPStream stream, long option, long sequenceId)
        {
            Tag.BlobWritePartial(objRef, stream.Ref, option, sequenceId);
        }

        public void BlobRead(FPStream stream)
        {
            Tag.BlobRead(objRef, stream.Ref, 0);
        }

        public void BlobReadPartial(FPStream stream, long offset, long partLen, long option)
        {
            Tag.BlobReadPartial(objRef, stream.Ref, offset, partLen, option);
        }

        public FPTag GetSibling()
        {
            FPTag tag = new FPTag(Tag.GetSibling(objRef), clip, -1);
            return tag;
        }

        public FPTag GetPreSibling()
        {
            FPTag tag = new FPTag(Tag.GetPrevSibling(objRef), clip, -1);
            return tag;
        }

        public FPTag GetFirstChild()
        {
            FPTag tag = new FPTag(Tag.GetFirstChild(objRef), clip, -1);
            return tag;
        }

        public FPTag GetParent()
        {
            FPTag tag = new FPTag(Tag.GetParent(objRef), clip, -1);
            return tag;
        }

        public override void Close()
        {
            try
            {
                if (this.objRef != 0)
                {
                    Tag.Close(objRef);
                    //logger.Debug(string.Format("Tag, Closed : {0}", objRef));
                    this.objRef = 0;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }

        }

        public string GetNextTagName()
        {
            int start = Name.LastIndexOf('_');
            int end = Name.IndexOf(".", StringComparison.CurrentCultureIgnoreCase);
            string extention = Name.Substring(end);
            int number = int.Parse(Name.Substring(start + 1, (end - start - 1)));
            int next = number + 1;
            string newName = Name.Substring(0, start + 1) + next + extention;
            return newName;
        }
    }

    class FPStream : FPObject
    {

        protected bool readAble { get; set; }
        protected bool writeAble { get; set; }

        public bool CanRead
        {
            get { return readAble; }
        }

        public virtual void BeginRead() { }

        public bool CanWrite
        {
            get { return writeAble; }
        }

        public FPStream()
        {
        }

        public FPStream(ulong streamHandel)
        {

        }

        public FPStream(string file, int bufLen)
        {

        }

        public FPStream(string filePath, FPStreamPerm perm)
        {
            this.objRef = Stream.CreateFileForOutput(filePath, perm);
        }

        public virtual void Write(byte[] buffer, int offset, int length)
        {

        }

        public virtual int Read(byte[] buffer, int offset, int length)
        {
            return 0;
        }

        public virtual void Commit()
        {
        }
        private AveLogger logger = AveLogger.GetInstance(typeof(FPStream));
        public virtual void Close()
        {
            if (this.objRef != 0)
            {
                Stream.Close(objRef);
                logger.Debug(string.Format("Stream, Closed : {0}", this.objRef));
                this.objRef = 0;
            }
        }
    }

    class FPInputStream : FPStream
    {
        private unsafe FPStreamInfo* theInfo;
        private System.IO.Stream userStream;
        private FPAsyncStreamCallBacks callBacks;
        private FPAsyncCallback completeProc;
        private FPAsyncCallback setMarkerProc;
        private FPAsyncCallback resetMarkProc;
        private FPAsyncCallback closeProc;
        private IntPtr userData;

        private FPTag tag;
        private Thread readThread;
        private bool start;
        private static AveLogger logger = AveLogger.GetInstance(typeof(FPInputStream));

        /// <summary>
        /// FPInputStream, 
        /// 1. Create BufferStream
        /// 2. Call bo a Blob Read
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="offset"></param>
        /// <param name="partLen"></param>
        public FPInputStream(FPTag tag, long offset, long partLen)
        {
            try
            {
                //if (partLen < 0 || offset + partLen > tag.Length)
                //{
                //    throw new InvalidDataException("Invalid Params : PartLen - " + partLen + " , Offset - " + offset + " , FileSize - " + tag.Length);
                //}
                if (partLen <= 0)
                {
                    partLen = tag.Length;
                }
                if (offset + partLen >= tag.Length)
                {
                    partLen = tag.Length - offset;
                }
                this.tag = tag;
                callBacks = new FPAsyncStreamCallBacks();
                //prepareProc = new FPAsyncCallback(callBacks.PrepareBufferProc);
                completeProc = new FPAsyncCallback(callBacks.CompletedProc);
                setMarkerProc = new FPAsyncCallback(callBacks.SetMark);
                resetMarkProc = new FPAsyncCallback(callBacks.ResetMark);
                closeProc = new FPAsyncCallback(callBacks.CloseProc);
                userData = (IntPtr)this.GetHashCode();

                readAble = true;
                writeAble = false;

                this.objRef = Stream.CreateGenericStream(null, completeProc, setMarkerProc, resetMarkProc, closeProc, userData);
                callBacks.StreamRef = this.objRef;
                userStream = callBacks.Stream;
                unsafe
                {
                    theInfo = Stream.GetInfo(this.objRef);
                    theInfo->AtEOF = 0;
                    theInfo->StreamLen = partLen;
                    theInfo->TransferLen = 0;
                    theInfo->StreamPos = 0;
                    theInfo->MarkerPos = 0;
                    //theInfo->Buffer = Marshal.AllocHGlobal(1024 * 64).ToPointer();
                    theInfo->ReadFlag = (byte)0;
                }

                readThread = new Thread(
                    delegate()
                    {
                        try
                        {
                            tag.BlobReadPartial(this, offset, partLen, 0);
                        }
                        catch (Exception t)
                        {
                            logger.Error(string.Format(t.Message + "TagName:{0}, Offset:{1}, PartLen:{2}, userStream is NULL? : {3}", this.tag.Name, offset, partLen, (this.userStream == null ? true : false)), t);
                        }
                    }
                    );
                readThread.Name = "CenteraReadThread_" + tag.Name + "@" + DateTime.Now.Ticks;
                //readThread.Start();
            }
            catch (Exception t)
            {
                logger.Error(t.Message, t);
                throw;
            }
        }

        public override void BeginRead()
        {

        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            if (length <= 0)
            {
                return 0;
            }
            if (!start)
            {
                start = true;
                readThread.Start();
            }
            int readLen = userStream.Read(buffer, offset, length);
            return readLen;
        }

        public override void Close()
        {
            try
            {
                if (start)
                {
                    if (readThread.IsAlive)
                    {
                        readThread.Join();
                    }
                }
                readThread = null;
                if (userStream != null)
                {
                    userStream.Close();
                    userStream = null;
                }

                base.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

    }

    class FPOutputStream : FPStream
    {
        unsafe FPStreamInfo* theInfo;
        System.IO.Stream userStream;
        FPAsyncStreamCallBacks callBacks;
        FPAsyncCallback prepareProc;
        FPAsyncCallback completeProc;
        FPAsyncCallback setMarkerProc;
        FPAsyncCallback resetMarkProc;
        FPAsyncCallback closeProc;
        IntPtr userData;

        FPTag tag;
        Thread writeThread;
        bool hasException;

        bool start;
        private AveLogger logger = AveLogger.GetInstance(typeof(FPOutputStream));
        /// <summary>
        /// for write to centera
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="mode"></param>
        /// <param name="streamLen"></param>
        public FPOutputStream(FPTag tag, long streamLen)
        {
            this.tag = tag;
            callBacks = new FPAsyncStreamCallBacks();
            prepareProc = new FPAsyncCallback(callBacks.PrepareBufferProc);
            completeProc = new FPAsyncCallback(callBacks.CompletedProc);
            setMarkerProc = new FPAsyncCallback(callBacks.SetMark);
            resetMarkProc = new FPAsyncCallback(callBacks.ResetMark);
            closeProc = new FPAsyncCallback(callBacks.CloseProc);
            userData = (IntPtr)this.GetHashCode();

            readAble = false;
            writeAble = true;

            writeThread = new Thread(
                delegate()
                {
                    try
                    {
                        tag.BlobWrite(this);
                    }
                    catch (System.Exception ex)
                    {
                        logger.Error(ex.ToString());
                        hasException = true;
                    }
                });

            this.objRef = Stream.CreateGenericStream(prepareProc, completeProc, setMarkerProc, resetMarkProc, closeProc, userData);
            logger.Debug(string.Format("Stream, Created : {0}", this.objRef));
            callBacks.StreamRef = this.objRef;
            userStream = callBacks.Stream;

            unsafe
            {
                theInfo = Stream.GetInfo(this.objRef);
                theInfo->AtEOF = 0;
                theInfo->StreamLen = streamLen;
                theInfo->TransferLen = 0;
                theInfo->StreamPos = 0;
                theInfo->MarkerPos = 0;
                theInfo->Buffer = null;
                theInfo->ReadFlag = (byte)1;
            }
        }

        public override void Write(byte[] buffer, int offset, int length)
        {
            userStream.Write(buffer, offset, length);
            if (!start)
            {
                writeThread.Start();
                start = true;
            }
            if (hasException)
            {
                throw new Exception("write file failed, name:" + tag.Name);
            }
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            throw new NotSupportedException();
        }

        public override void Commit()
        {
            userStream.Flush();
            if (writeThread.IsAlive)
                writeThread.Join();
            userStream.Close();
            userStream = null;
            if (hasException)
            {
                throw new Exception("write file failed, name:" + tag.Name);
            }
        }

        public override void Close()
        {
            try
            {
                if (userStream != null)
                {
                    userStream.Close();
                    userStream = null;
                }
                if (writeThread.IsAlive)
                {
                    writeThread.Abort();
                }
                base.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }
    }

    class FPKnownException : XException
    {
        FPErrorInfo errorInfo;
        public string ErrorInformation { get; set; }
        public FPKnownException(FPErrorInfo errorInfo, string errorInformation)
            : base(errorInfo.ToString())
        {
            this.errorInfo = errorInfo;
            this.ErrorInformation = errorInformation;
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();
            message.Append("error code : " + errorInfo.Error);
            message.Append("error text : " + errorInfo.ErrorString);
            message.Append("system error code : " + errorInfo.SystemError);
            message.Append("error trace : " + errorInfo.Trace);
            message.Append("information : " + EMCErrorInformation.GetInformation(errorInfo.Error, errorInfo.SystemError));
            return message.ToString();
        }
    }
    class FPException : XException
    {
        FPErrorInfo errorInfo;

        public FPException(FPErrorInfo errorInfo)
            : base(errorInfo.ToString())
        {
            this.errorInfo = errorInfo;
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();
            message.Append("error code : " + errorInfo.Error);
            message.Append("error text : " + errorInfo.ErrorString);
            message.Append("system error code : " + errorInfo.SystemError);
            message.Append("error trace : " + errorInfo.Trace);
            message.Append("information : " + EMCErrorInformation.GetInformation(errorInfo.Error, errorInfo.SystemError));
            return message.ToString();
        }
    }

    #region StructLayout
    [StructLayout(LayoutKind.Sequential)]
    struct FPErrorInfo
    {
        /// <summary>
        /// The last FPLibrary error that occurred on the current thread
        /// </summary>
        private int error;
        public int Error { get { return this.error; } set { this.error = value; } }

        /// <summary>
        /// The last system error that occurred on this thread
        /// </summary>
        private int systemError;
        public int SystemError { get { return systemError; } set { this.systemError = value; } }

        /// <summary>
        /// The fuction trace for the last error that occurred
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        private string trace;
        public string Trace { get { return this.trace; } set { this.trace = value; } }

        /// <summary>
        /// The message associated with the FPLibrary error
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        private string message;
        public string Message { get { return this.message; } set { this.message = value; } }

        /// <summary>
        /// The error string associated with the FPLibrary error
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        private string errorString;
        public string ErrorString { get { return this.errorString; } set { this.errorString = value; } }

        /// <summary>
        /// 
        /// </summary>
        private ushort errorClass;
        public ushort ErrorClass { get { return this.errorClass; } set { this.errorClass = value; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FPPoolInfo
    {
        /*
        /// <summary>
        /// The current version of this structure.
        /// </summary>
        public int poolInfoVersion;

        /// <summary>
        /// The total capacity of the pool, in bytes
        /// </summary>
        public ulong capacity;

        /// <summary>
        /// The total free usable space of the pool, in bytes
        /// </summary>
        public ulong freeSpace;

        /// <summary>
        /// The cluster identifier of the pool
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string clusterID;

        /// <summary>
        /// The name of the cluster
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string clusterName;

        /// <summary>
        /// The version of the pool server software.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string version;

        /// <summary>
        /// A comma-separated list of the replication cluster's node (with access role) addresses as specified when replication was enabled;
        /// empty if replica cluster not identified or configured.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string replicaAddress;
         * */
        /// <summary>
        /// The current version of this structure.
        /// </summary>
        private int poolInfoVersion;

        public int PoolInfoVersion { set { this.poolInfoVersion = value; } get { return this.poolInfoVersion; } }

        /// <summary>
        /// The total capacity of the pool, in bytes
        /// </summary>
        private ulong capacity;
        public ulong Capcity { get { return capacity; } }

        /// <summary>
        /// The total free usable space of the pool, in bytes
        /// </summary>
        private ulong freeSpace;
        public ulong FreeSpace { get { return freeSpace; } }

        /// <summary>
        /// The cluster identifier of the pool
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string clusterID;
        public string ClusterID { get { return clusterID; } }

        /// <summary>
        /// The name of the cluster
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string clusterName;
        public string ClusterName { get { return clusterName; } }

        /// <summary>
        /// The version of the pool server software.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string version;
        public string Version { get { return version; } }

        /// <summary>
        /// A comma-separated list of the replication cluster's node (with access role) addresses as specified when replication was enabled;
        /// empty if replica cluster not identified or configured.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string replicaAddress;
        public string ReplicaAddress { get { return replicaAddress; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct FPStreamInfo
    {
        /// <summary>
        /// The current version of FPStreamInfo
        /// </summary>
        private short version;
        public short Version { get { return version; } }

        /// <summary>
        /// Application-specific data, untouched by the SDK
        /// </summary>
        private void* userData;
        public void* UserData { get { return userData; } }

        /// <summary>
        /// The current position of the stream
        /// </summary>
        private long streamPos;
        public long StreamPos { get { return this.streamPos; } set { this.streamPos = value; } }
        /// <summary>
        /// The position of the stream marker;
        /// </summary>
        private long markerPos;
        public long MarkerPos { get { return this.markerPos; } set { this.markerPos = value; } }

        /// <summary>
        /// The length of the stream in bytes, if known, else -1.
        /// </summary>
        private long streamLen;
        public long StreamLen { get { return streamLen; } set { this.streamLen = value; } }

        /// <summary>
        /// True if the end of stream has been reached.
        /// </summary>
        private byte atEOF;
        public byte AtEOF { get { return atEOF; } set { this.atEOF = value; } }

        /// <summary>
        /// Read/Write indicator, true on FPTag_BlobWrite(), false on FPTag_BlobRead().
        /// </summary>
        private byte readFlag;
        public byte ReadFlag { get { return readFlag; } set { this.readFlag = value; } }

        /// <summary>
        /// The data buffer supplied by the application.
        /// </summary>
        private void* buffer;
        public void* Buffer { get { return this.buffer; } set { this.buffer = value; } }

        /// <summary>
        /// The number of bytes to be transferred or actually transferred.
        /// </summary>
        private long transferLen;
        public long TransferLen { get { return this.transferLen; } set { this.transferLen = value; } }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate long FPAsyncCallback(ref FPStreamInfo info);

    class FPPipeStream : System.IO.Stream
    {
        private readonly Queue<byte> ioBuffer = new Queue<byte>();
        private bool flushed;
        private long maxBufferLength = 1024 * 1024 * 5;
        private bool bufferLastReaded;

        public long MaxBufferLength
        {
            get { return maxBufferLength; }
            set { this.maxBufferLength = value; }
        }

        public bool BufferLastReaded
        {
            get { return bufferLastReaded; }
            set
            {
                this.bufferLastReaded = value;
                if (!bufferLastReaded)
                {
                    lock (ioBuffer)
                    {
                        Monitor.Pulse(ioBuffer);
                    }
                }
            }
        }

        public new void Dispose()
        {
            ioBuffer.Clear();
        }

        public override void Close()
        {
            base.Close();
            ioBuffer.Clear();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            flushed = true;
            lock (ioBuffer)
            {
                Monitor.Pulse(ioBuffer);
            }
        }

        public override long Length
        {
            get { return ioBuffer.Count; }
        }

        public override long Position
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private bool ReadAvaliable(int count)
        {
            bool isAvaliable = (Length >= count || flushed) && (Length >= (count + 1) || !bufferLastReaded);
            return isAvaliable;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset > 0)
            {
                throw new NotSupportedException("Offsets with value of non-zero are not supported");
            }
            if (buffer == null)
            {
                throw new ArgumentException("Read buffer is null");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length. ");
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            }
            if (BufferLastReaded && count > maxBufferLength)
            {
                throw new ArgumentException(String.Format("count({0}) > mMaxBufferLength({1})", count, maxBufferLength));
            }
            if (count == 0)
            {
                return 0;
            }
            int readLen = 0;
            lock (ioBuffer)
            {
                while (!ReadAvaliable(count))
                {
                    Monitor.Wait(ioBuffer);
                }
                for (; readLen < count && Length > 0; readLen++)
                {
                    buffer[readLen] = ioBuffer.Dequeue();
                }
                Monitor.Pulse(ioBuffer);
            }
            return readLen;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentException("Buffer is null");
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length. ");
            }
            if (count == 0)
            {
                return;
            }
            lock (ioBuffer)
            {
                if (Length >= maxBufferLength)
                {
                    Monitor.Wait(ioBuffer);
                }
                flushed = false;
                for (int i = offset; i < offset + count; i++)
                {
                    ioBuffer.Enqueue(buffer[i]);
                }
                Monitor.Pulse(ioBuffer);
            }
        }
    }

    class FPAsyncStreamCallBacks
    {
        private int bufferSize = 64 * 1024;
        private byte[] localBuffer;
        private System.IO.Stream userStream;
        private ulong fPStreamHandle;

        public FPAsyncStreamCallBacks() : this(new FPPipeStream(), 0, 64 * 1024) { }

        public FPAsyncStreamCallBacks(System.IO.Stream stream, ulong streamHandle) : this(stream, streamHandle, 1024 * 64) { }

        public FPAsyncStreamCallBacks(System.IO.Stream stream, ulong streamHandle, int bufferSize)
        {
            this.userStream = stream;
            this.fPStreamHandle = streamHandle;
            this.bufferSize = bufferSize;
            this.localBuffer = new byte[bufferSize];
        }

        public ulong StreamRef
        {
            set { this.fPStreamHandle = value; }
        }

        public System.IO.Stream Stream
        {
            get { return userStream; }
        }

        private AveLogger logger = AveLogger.GetInstance(typeof(FPAsyncStreamCallBacks));

        unsafe public long PrepareBufferProc(ref FPStreamInfo info)
        {
            try
            {
                if (info.ReadFlag == (byte)1)
                {
                    if (info.Buffer == null)
                    {
                        info.Buffer = Marshal.AllocHGlobal(bufferSize).ToPointer();
                    }

                    int dataSize = userStream.Read(localBuffer, 0, bufferSize);

                    Marshal.Copy(localBuffer, 0, (IntPtr)info.Buffer, dataSize);

                    info.StreamPos += dataSize;
                    info.TransferLen = dataSize;

                    if (info.StreamLen == -1)
                    {
                        if (dataSize < bufferSize)
                        {
                            info.AtEOF = 1;
                        }
                    }
                    else if (info.StreamLen <= info.StreamPos)
                    {
                        info.AtEOF = 1;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                logger.Error(e.Message + string.Format("userStream is NULL ? {1}", (userStream == null ? true : false)), e);
                throw;
            }
        }

        public long CompletedProc(ref FPStreamInfo info)
        {
            try
            {
                if (info.ReadFlag == (byte)0)
                {
                    if (info.TransferLen > 0)
                    {
                        int copyLen = 0;
                        if (info.TransferLen > bufferSize)
                        {
                            bufferSize = (int)info.TransferLen;
                            localBuffer = new byte[bufferSize];
                            copyLen = bufferSize;
                        }
                        else
                        {
                            copyLen = (int)info.TransferLen;
                        }
                        unsafe
                        {
                            Marshal.Copy((IntPtr)info.Buffer, localBuffer, 0, copyLen);
                        }
                        userStream.Write(localBuffer, 0, copyLen);
                        info.StreamPos += info.TransferLen;
                    }
                    else
                    {
                        userStream.Flush();
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                logger.Error(e.Message + string.Format("userStream is NULL ? {1}", (userStream == null ? true : false)), e);
                throw;
            }
        }

        public long SetMark(ref FPStreamInfo info)
        {
            info.MarkerPos = info.StreamPos;
            return 0;
        }

        public long ResetMark(ref FPStreamInfo info)
        {
            info.StreamPos = info.MarkerPos;
            logger.Error("some error in emc service");
            throw new Exception("some error in emc service");
            //return 0;
        }

        public long CloseProc(ref FPStreamInfo info)
        {
            unsafe
            {
                Marshal.FreeHGlobal((IntPtr)info.Buffer);
                info.Buffer = null;
            }
            return 0;
        }
    }

    #endregion
}
