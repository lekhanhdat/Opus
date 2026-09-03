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


namespace AvePoint.Item.Restore
{
    using AvePoint.GCommon;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.Wrapper.Common;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    delegate void RestoreItemMethod(RestoreContentDto aveItemDto, IAveRestoreStream restoreStream);

    class CacheQueueConsumer
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private BlockingQueue<object> cacheQueue;       //this queue will be disposed out of this class
        private EventWaitHandle nonContentLevelFinished;
        private EventWaitHandle allFinished;
        private readonly int MAX_THREAD_COUNT;
        private bool needRestartRestoreThread = false;
        
        private AveAppendableTaskExecutor taskExecutor;
        private RestoreItemMethod restoreMethod;

        private bool needModeChange;
        public ThreadMode ThreadMode { get; private set; }

        public CacheQueueConsumer(BlockingQueue<object> cacheQueue, int maxThreadCount, RestoreItemMethod restoreMethod)
        {
            this.ThreadMode = ThreadMode.MultiThread;
            this.MAX_THREAD_COUNT = maxThreadCount;
            this.cacheQueue = cacheQueue;
            this.restoreMethod = restoreMethod;
            this.nonContentLevelFinished = new ManualResetEvent(false);
            this.allFinished = new ManualResetEvent(false);
            this.taskExecutor = new AveAppendableTaskExecutor(maxThreadCount);
        }

        public void StartRestoreFromCache()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    this.taskExecutor.StartExecute();
                    while (true)
                    {
                        object cachedObj = this.cacheQueue.Dequeue();
                        if (!ProcessCachedObj(cachedObj))
                        {
                            break;
                        }
                    }
                    this.needRestartRestoreThread = false;
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred when restore item from cache, error detail : {0}", e.ToString());
                    this.needRestartRestoreThread = true;
                }
                finally
                {
                    this.allFinished.Set();
                }
            });
        }

        public bool ProcessCachedObj(object cachedObj)
        {
            if (cachedObj == null)
            {
                this.taskExecutor.WaitForAllTasks();
                this.nonContentLevelFinished.Set();
                //exit and release resources
                return false;
            }
            else if (cachedObj is NonItemLevelSignal)  //Non-Item Level Object
            {
                this.taskExecutor.WaitForAllTasks();
                RefreshThreadMode();
                this.nonContentLevelFinished.Set();
                return true;
            }
            else if (cachedObj is RestorePauseSignal)
            {
                this.taskExecutor.WaitForAllTasks();                
                this.nonContentLevelFinished.Set();
                return true;
            }
            else if (cachedObj is AveStreamSegments)     //Item Level Cache Stream
            {
                this.taskExecutor.AddTask(() => { RestoreFromCacheStream(cachedObj as AveStreamSegments); });
                return true;
            }
            else
            {
                //This line should never be hit
                mLog.Error("An error occurred when getting cache object from cache queue, unknown object is found, object type : {0}.", cachedObj.GetType().ToString());
                return true;
            }
        }

        public void WaitForAllCachedItemsBeRestored()
        {
            //if (WaitHandle.WaitAny(new WaitHandle[] { this.nonContentLevelFinished }, WAIT_TIMEOUT) == WaitHandle.WaitTimeout)
            //{
            //    mLog.Error("Time out when waiting for all cached items be restored.");
            //}
            this.nonContentLevelFinished.WaitOne();
            this.nonContentLevelFinished.Reset();
        }

        public void WaitForAllTasks()
        {
            this.taskExecutor.WaitForAllTasks();
        }

        public void SwitchThreadMode(ThreadMode mode)
        {
            if (this.ThreadMode != mode)
            {
                this.ThreadMode = mode;
                needModeChange = true;
            }
            RefreshThreadMode();
        }

        private void RefreshThreadMode()
        {
            if (needModeChange)
            {
                mLog.Debug("Thread mode changed, current mode : {0}", this.ThreadMode.ToString());
                needModeChange = false;
                if (this.ThreadMode == ThreadMode.SingleThread)
                {
                    this.taskExecutor.ResetThreadThreshold(1);
                }
                else
                {
                    this.taskExecutor.ResetThreadThreshold(this.MAX_THREAD_COUNT);
                }
            }
        }

        public Func<IInputStreamWrapper,IAveRestoreStream> InitRestoreStream;

        /// <summary>
        /// Restore the all versions and attachments of an item
        /// </summary>
        /// <param name="cachedStream"></param>
        /// <param name="context"></param>
        public void RestoreFromCacheStream(AveStreamSegments streamSegments)
        {
            try
            {
                mLog.Debug("Start restore item from cache stream");
                CacheFileReceiver fileReceiver = new CacheFileReceiver(streamSegments);
                IAveRestoreStream restoreStream;
                if (InitRestoreStream == null)
                {
                    restoreStream = new WrapperRestoreStreamV2(fileReceiver);
                }
                else
                {
                    restoreStream = InitRestoreStream(fileReceiver);
                }
                
                RestoreContentDto aveItemDto = null;
                while ((aveItemDto = fileReceiver.GetNextItemDto()) != null)
                {
                    RestoreItem(aveItemDto, restoreStream, streamSegments.Stream);
                }
                if (!(streamSegments.Stream as AveCoordinatedStream).IsEndOfStream)
                {
                    mLog.Warn("Error occured when reading item cache stream, the item is skipped or the content is not successfully restored.");
                }
                mLog.Debug("Finished restore item from cache stream");
            }
            catch (Exception e)
            {
                mLog.Error("An error occured when restore item from cache stream, message: {0}, stacktrace: {1}", e.Message, e.StackTrace);
            }
            finally
            {
                if (streamSegments.Stream != null)
                {
                    streamSegments.Stream.Dispose();
                }
            }
        }

        /// <summary>
        /// the method is used to restore one version/one attachment from the cache stream of an item
        /// </summary>
        /// <param name="restoreItem"></param>
        /// <param name="restoreStream"></param>
        /// <param name="cachedStream"></param>
        private void RestoreItem(RestoreContentDto aveItemDto, IAveRestoreStream restoreStream, Stream cachedStream)
        {
            //long position = cachedStream.Position;
            this.restoreMethod(aveItemDto, restoreStream);
            //bool isSkipped = (position + AveWrapperConstants.HEADER_SIZE + metadataAndContentLength + [tail length] + [header lengths]) != cachedStream.Position;
            ////if the item/file is skipped or the stream is not correctly read, set the pointer to the item next to current item
            //if (isSkipped)
            //{
            //    cachedStream.Seek(Convert.ToInt64(metadataAndContentLength + AveWrapperConstants.HEADER_SIZE), SeekOrigin.Current);
            //}
            restoreStream.Reset();
        }

        public void WaitForAll()
        {
            while (true)
            {
                this.allFinished.WaitOne();
                if (this.needRestartRestoreThread)
                {
                    StartRestoreFromCache();
                    this.allFinished.Reset();
                }
                else
                {
                    mLog.Debug("Restore thread has exited.");
                    break;
                }
            }
        }

        public void Close()
        {
            if (this.nonContentLevelFinished != null)
            {
                this.nonContentLevelFinished.Close();
            }
            if (this.allFinished != null)
            {
                this.allFinished.Close();
            }
            if (this.taskExecutor != null)
            {
                this.taskExecutor.Dispose();
            }
        }
    }

    enum ThreadMode
    {
        SingleThread,
        MultiThread
    }
}
