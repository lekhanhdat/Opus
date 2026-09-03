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
using System.Threading;
using AvePoint.GCommon.FileTransfer;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.GCommon;
using System.Reflection;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.GCommon.Contract.CodeReview;
using System.IO;
using AvePoint.Common;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAArchiverCommon;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/11/2",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
    "ADO-53910",
    false
    )]
    internal class ResponseHandle : IFileSenderResponseWorker, IDisposable
    {
        //private ArchiverDeletion mDeletion;
        private MessageProcessor mProcessor;
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public ResponseHandle(ScheduleConfiguration config)
        {
            //mDeletion = new ArchiverDeletion(config);
            mProcessor = new MessageProcessor(config);
        }

        public int ProcessMessage(string message)
        {
            throw new NotImplementedException(LOGRESOURCE.StorageOptimization13_SOARResponseHandleProcessMessageNotImplementedException);
        }

        public void SaveXmlHeader(string message)
        {
            try
            {
                mProcessor.StoreResponseMessage(message);
            }
            catch (Exception ex)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARResponseHandleSaveXmlHeader, ex.ToString());
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                mProcessor.WaitingForResponseHandleCompleted(Int32.MaxValue);
                if (mProcessor != null)
                {
                    mProcessor.Dispose();
                    mProcessor = null;
                }
                //if (mDeletion != null)
                //{
                //    mDeletion.Dispose();
                //    mDeletion = null;
                //}
            }
            catch (Exception e)
            {
                mLog.Info("Dispose Error: {0}", e.ToString());
            }
        }
    }

    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/2/24",
    "ruiheng.liu@AvePoint.com",
    "Dong.xie@AvePoint.com",
    new string[]
    {
        CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
        CodeReviewConstants.CHECK_LIST_ID_EH_1,
        CodeReviewConstants.CHECK_LIST_ID_EH_2,
        CodeReviewConstants.CHECK_LIST_ID_DB_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_10,
        CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
    }, 
    "ADO-25950",
    false
    )]

    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2012/8/7",
      "ruiheng.liu@AvePoint.com",
      "yanlong.gu@AvePoint.com",
      new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
      "ADO-44684",
      false
      )]
    internal class MessageProcessor : IDisposable
    {
        #region Private fields
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ArchiverDeletion mDeletion;
        private Thread readerThread;
        private readonly int maxCacheSize = 100000;//ADO-104240 把缓存大小改成100000，防止读取过多message ，导致内存过大
        private PCContainer<string> mMessageContainer = null;
        StreamWriter streamWriter = null;
        bool writeHeadToLocal = false;
        string folderPath = string.Empty;
        string filePath = string.Empty;
        long tempFileHeadCount = 0;

        private readonly ScheduleConfiguration config;
        #endregion

        #region constructor
        //public MessageProcessor(ArchiverDeletion deletion,ScheduleConfiguration config)
        public MessageProcessor(ScheduleConfiguration config)
        {
            this.config = config;
            mDeletion = new ArchiverDeletion(config);
            maxCacheSize = config.BackgroundSettings.MaxDeletionCacheSize;
            mMessageContainer = new PCContainer<string>(maxCacheSize);
            mMessageContainer.StartProduce();
            StartInternalThread();
            folderPath = Path.Combine(config.ArchiveTemp, config.JobId);
            filePath = Path.Combine(folderPath, config.JobId + ".tmp");
        }
        #endregion

        #region Methords
        /// <summary>
        /// store response message from media,此方法会立即返回
        /// </summary>
        public void StoreResponseMessage(string message)
        {
            if (message.Equals("End", StringComparison.OrdinalIgnoreCase))//
            {
                //ADO-104240必须先释放StreamWriter。如果EndProduce()在前面，在Processor()方法中，StreamWriter 没有释放，就去实例化StreamReader 的情况
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                }
                mMessageContainer.EndProduce();
                mLog.Info(string.Format("Temp file head Count is: {0}", tempFileHeadCount.ToString()));
                return;
            }
            //ADO-104240如果PCContainer中数量 大于等于缓存最大值，则之后的Header 都需要写在本地缓存中，
            //hasDeletionFile是一个控制的阀值，如果执行过写本地操作，之后都会写在本地
            if (writeHeadToLocal || mMessageContainer.Count >= maxCacheSize)
            {
                if (!writeHeadToLocal)
                {
                    writeHeadToLocal = true;
                    mLog.Info(string.Format("Begin to write to local file The flag is : {0}", writeHeadToLocal.ToString()));
                    InitStreamWriter();
                }
                tempFileHeadCount++;
                streamWriter.WriteLine(message);
            }
            else
            {
                mMessageContainer.Produce(message);
            }
        }

        /// <summary>
        /// 等待Delete线程退出
        /// </summary>
        public void WaitingForResponseHandleCompleted(int timeOut)
        {
            if (readerThread != null)
            {
                readerThread.Join(timeOut);
            }
        }

        public void Dispose()
        {
            if(mDeletion != null)
            {
                mDeletion.Dispose();
            }
            if (mMessageContainer != null)
            {
                mMessageContainer.Dispose();
            }
            //如果用到streamWriter ，逻辑上不会出现在这里还没释放的情况
            if (streamWriter != null)
            {
                streamWriter.Dispose();
            }
        }

        private void StartInternalThread()
        {
            string currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            readerThread = new Thread(new ThreadStart(Processor));
            readerThread.Name = currentThreadId + "_MessageProcessor";
            readerThread.IsBackground = true;
            readerThread.Start();
        }

        private void Processor()
        {
            long tempFileHeadCount = 0;
            try
            {
                using (IMultiDeleteController deleteController = new MultiDeleteController(config, config.BackgroundSettings.TotalMultiDeleteThreadNumber, config.BackgroundSettings.EnableMultiDelete))
                {
                    while (true)
                    {
                        if (NeedStopCurrentJob())
                        {
                            return;
                        }
                        string message = mMessageContainer.Consume();
                        if (string.IsNullOrEmpty(message))
                        {
                            //ADO-104240 change return to break ,因为缓存中还可能有Header ，需要去缓存中处理
                            break;
                        }
                        if (config.jobtype == Contract.JobMonitor.JobType.ArchiverByHSMXml && config.currentRule.KeepDataOption == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove)
                        {
                            mLog.Info("this is ArchiverByHSMXml job,no need to delete document.and this is not leave stub action");
                        }
                        else
                        {
                            var deletionNode = new DeletionNode(message);
                            deleteController.Process(deletionNode, mDeletion); 
                        }
                        //mDeletion.HandleResponseMessage(deletionNode);
                    }
                    //ADO-104240 开始检查缓存文件，从缓存文件中进行删除操作
                    if (writeHeadToLocal && File.Exists(filePath))
                    {
                        mLog.Info("Begin to read from local file");
                        using (StreamReader streamReader = new StreamReader(filePath))
                        {
                            while (streamReader.Peek() > 0)
                            {
                                if (NeedStopCurrentJob())
                                {
                                    return;
                                }
                                tempFileHeadCount++;

                                var deletionNode = new DeletionNode(streamReader.ReadLine());
                                //mDeletion.HandleResponseMessage(deletionNode);
                                deleteController.Process(deletionNode, mDeletion);
                            }
                        }
                        File.Delete(filePath);
                        mLog.Info(string.Format("Delete the temp file for deletion, Temp head count is: {0}", tempFileHeadCount.ToString()));
                    }

                    deleteController.WaitForFinish();
                }
            }
            catch (Exception e)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARResponseHandleProcessor, e.ToString()); 
            }
            finally
            {
                //config.soArchiverQueryWorkerForDel.DeleteAndMoveItems(config);
            }
        }

        private void InitStreamWriter()
        {
            if (!Directory.Exists(folderPath))
            {
                mLog.Info("Begin Create temp folder for Deletion");
                Directory.CreateDirectory(folderPath);
            }
            streamWriter = new StreamWriter(filePath);
        }

        /// <summary>
        /// 检查是否需要退出
        /// </summary>
        /// <returns></returns>
        private bool NeedStopCurrentJob()
        {
            try
            {
                using (new CheckJobStopScope()) { }
            }
            catch (JobStopException)
            {
                return true;
            }
            return false;
        }
        #endregion

    }

}