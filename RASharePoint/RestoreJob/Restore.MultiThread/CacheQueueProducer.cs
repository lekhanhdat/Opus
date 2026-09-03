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

namespace AvePoint.Item.Restore
{
    using AvePoint.GCommon;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Common.Utility;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    class CacheQueueProducer
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private CacheQueueConsumer cacheQueueConsumer;

        private BlockingQueue<object> cacheQueue;       //this queue will be disposed out of this class        
        private AveStreamSegments streamSegments;
        private string preItemName;
        private IFileReceiver realFileReceiver;
        private bool isFirstAttachment = true;

        public CacheQueueProducer(CacheQueueConsumer cacheQueueConsumer, BlockingQueue<object> cacheQueue, IFileReceiver realFileReceiver)
        {
            this.cacheQueueConsumer = cacheQueueConsumer;
            this.cacheQueue = cacheQueue;
            this.realFileReceiver = realFileReceiver;
        }

        public void ProduceCacheObj(RestoreContentDto item)
        {
            if (isFirstAttachment && item.Type == AveConstants.TYPE_ATTACHMENTS)
            {
                RefreshCacheStream();
                //if it is end of the job, put null to tell consumer to end after tasks finished,
                //if non item level object reached, put an instance of NonItemLevelSignal into cache queue
                this.cacheQueue.Enqueue(new RestorePauseSignal());
                WaitForConsumer();
                //wait all items are restored before start restoring attachment                
                this.preItemName = null;
                CompareAndUpdatePreviousItemName(item.Name);
                isFirstAttachment = false;
            }
            else if (!CompareAndUpdatePreviousItemName(item.Name))
            {
                RefreshCacheStream();
            }
            WriteContentLevelObjToCacheStream(item);
        }

        public void PutNonItemLevelSignalToCacheQueue(bool isEndOfJob)
        {
            isFirstAttachment = true;
            RefreshCacheStream();
            //if it is end of the job, put null to tell consumer to end after tasks finished,
            //if non item level object reached, put an instance of NonItemLevelSignal into cache queue
            this.cacheQueue.Enqueue(isEndOfJob ? null : new NonItemLevelSignal());
            WaitForConsumer();
        }

        private void WriteContentLevelObjToCacheStream(RestoreContentDto item)
        {
            mLog.Debug("Start to write restore object to cache, leaf name : {0}, type : {1}", item.Name, item.Type);
            CacheWriter cacheFileWriter = new CacheWriter(realFileReceiver, item, streamSegments);
            cacheFileWriter.WriteRestoreContent();
            cacheFileWriter.WriteMetadata();
            if (item.Type != AveConstants.TYPE_LISTITEM)
            {
                cacheFileWriter.WriteContent();
            }
            cacheFileWriter.WriteFileTail();
        }

        private void RefreshCacheStream()
        {
            if (this.streamSegments != null && this.streamSegments.Stream.Length != 0)
            {
                PutCacheStreamToCacheQueue();
            }
            this.streamSegments = new AveStreamSegments(new AveCoordinatedStream("CQP"));
        }

        private void PutCacheStreamToCacheQueue()
        {
            try
            {
                this.streamSegments.Stream.Flush();
                this.streamSegments.Stream.Position = 0;
                this.cacheQueue.Enqueue(this.streamSegments);
            }
            catch (Exception e)
            {
                mLog.Error("error occurred when flush stream: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Check if current item name is the same as the previous one, if not, update previous item name
        /// </summary>
        /// <param name="curItemName">current item name</param>
        /// <returns>true if the previous item name is the same as current item name, otherwise false</returns>
        private bool CompareAndUpdatePreviousItemName(string curItemName)
        {
            int urlSlashPos = curItemName.IndexOf('/');
            while (urlSlashPos != -1)
            {
                curItemName = curItemName.Substring(urlSlashPos + 1);
                urlSlashPos = curItemName.IndexOf('/');
            }
            int attachmentColonPos = curItemName.IndexOf(':');
            if (attachmentColonPos != -1)
            {
                curItemName = curItemName.Substring(0, attachmentColonPos);
            }
            if (!curItemName.Equals(this.preItemName))
            {
                this.preItemName = curItemName;
                return false;
            }
            return true;
        }

        private void WaitForConsumer()
        {
            this.cacheQueueConsumer.WaitForAllCachedItemsBeRestored();
        }

        public void Close()
        {
            if (this.streamSegments != null)
            {
                this.streamSegments.Stream.Dispose();
            }
        }
    }

    internal class NonItemLevelSignal { }

    internal class RestorePauseSignal { }
}
