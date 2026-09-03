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



namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Util;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_1 },
    "ADO-26069",
    false)]
    [AveCodeReview(
   "2012/8/9",
   "rongbiao.sun@avepoint.com",
   "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_LOG_1 },
    null,
    true)]
    #endregion

    class TSMClient : IDisposable
    {
        static bool setUped;
        static object locker = new object();
        IntPtr instance;
        StorageLogger logger;
        ThrowException throwEx;
        TSMRetry tsmRetry;
        TSMConfiguration config;
        TSMOpenParameter openParameter;
        private bool isWriting;
        private bool isOperating;
        string ValidateHLFileName = "DocAve6_ValidateFile_HL";
        string ValidateLLFileName = "DocAve6_ValidateFile_LL";
        bool IsSingleSession;
        bool isValidate;

        public TSMClient()
        {
            this.throwEx = new ThrowException(ThrowTSMException);
            this.instance = TSMApi.GetInstance(this.throwEx);
            this.logger = StorageLogger.GetInstance(this.GetType());
            this.tsmRetry = new TSMRetry(true, 6, 30);
            this.openParameter = new TSMOpenParameter();
            this.config = new TSMConfiguration();
        }

        private void ThrowTSMException(string msg)
        {
            throw new TSMIOException(msg);
        }

        public void SetUp(TSMNodeInfo nodeInfo)
        {
            if (!setUped)
            {
                lock (locker)
                {
                    if (!setUped)
                    {
                        try
                        {
                            var commOpt = this.config.CheckCommOpt(nodeInfo);
                            CleanUpNoUseData(nodeInfo);
                            this.isValidate = nodeInfo.IsValidate;
                            logger.Debug("common option:" + commOpt);
                            var isCleanUpConflicted = false;
                            try
                            {
                                TSMApi.CleanUp(this.instance);
                            }
                            catch (TSMIOException e)
                            {
                                Trace.TraceWarning(e.Message);
                                isCleanUpConflicted = true;
                            }
                            if (!isCleanUpConflicted)
                            {
                                TSMApi.SetUp(this.instance, nodeInfo.CommDsmiDir, nodeInfo.CommDsmiLogDir, nodeInfo.CommDsmiLogName, nodeInfo.CommConfigFile);
                            }
                            setUped = true;
                        }
                        catch (TSMIOException e)
                        {
                            logger.Error("Error when setup TSM api: {0}", e);
                            try
                            {
                                TSMApi.CleanUp(this.instance);
                            }
                            catch (System.Exception ex)
                            {
                                Trace.TraceWarning(ex.Message);
                            }
                            setUped = false;
                            throw;
                        }
                    }
                }
            }
        }

        public string CreateConfigFile(TSMNodeInfo nodeInfo)
        {
            lock (this.config)
            {
                var nodeOpt = this.config.CheckNodeOpt(nodeInfo);
                logger.Debug("node option:" + nodeOpt);
                return nodeOpt;
            }
        }

        public TSMSession OpenSession(TSMNodeInfo nodeInfo)
        {
            try
            {
                logger.Debug("Begin open a TSM api session");
                CreateConfigFile(nodeInfo);
                var handle = default(UInt32);
                this.IsSingleSession = nodeInfo.IsSingleSession;
                if (this.IsSingleSession)
                {
                    lock (locker)
                    {
                        while (this.isWriting)
                        {
                            Thread.Sleep(10);
                        }
                        this.isOperating = true;
                        handle = TSMApi.OpenSession(this.instance, nodeInfo.ConfigFile, nodeInfo.Filespace, nodeInfo.Capacity, nodeInfo.Occupancy, nodeInfo.SizeEstimate, nodeInfo.Password);
                    }
                }
                else
                {
                    handle = TSMApi.OpenSession(this.instance, nodeInfo.ConfigFile, nodeInfo.Filespace, nodeInfo.Capacity, nodeInfo.Occupancy, nodeInfo.SizeEstimate, nodeInfo.Password);
                }
                var session = new TSMSession();
                session.Handle = handle;
                session.State = 0;
                logger.Debug("End open a TSM api session, handle : {0}.", handle);
                return session;
            }
            catch (TSMIOException e)
            {
                logger.Error("Error open session: {0}", e);
                if (this.isValidate)
                {
                    throw;
                }
                return this.tsmRetry.Retry(true, e, delegate
                {
                    CreateConfigFile(nodeInfo);
                    var handle = default(UInt32);
                    if (this.IsSingleSession)
                    {
                        lock (locker)
                        {
                            while (this.isWriting)
                            {
                                Thread.Sleep(10);
                            }
                            this.isOperating = true;
                            handle = TSMApi.OpenSession(this.instance, nodeInfo.ConfigFile, nodeInfo.Filespace, nodeInfo.Capacity, nodeInfo.Occupancy, nodeInfo.SizeEstimate, nodeInfo.Password);
                            this.isOperating = false;
                        }
                    }
                    else
                    {
                        handle = TSMApi.OpenSession(this.instance, nodeInfo.ConfigFile, nodeInfo.Filespace, nodeInfo.Capacity, nodeInfo.Occupancy, nodeInfo.SizeEstimate, nodeInfo.Password);
                    }
                    var session = new TSMSession();
                    session.Handle = handle;
                    session.State = 0;
                    return session;
                }) as TSMSession;
            }
            finally
            {
                this.isOperating = false;
            }
        }

        public void BeginWrite(TSMSession session, string hlName, string llName)
        {
            try
            {
                this.openParameter.HighName = hlName;
                this.openParameter.LowName = llName;
                if (this.IsSingleSession)
                {
                    lock (locker)
                    {
                        while (this.isOperating)
                        {
                            Thread.Sleep(10);
                        }
                        TSMApi.BeginWrite(this.instance, session.Handle, hlName, llName, DSMObjType.DSM_FILE);
                        this.isWriting = true;
                        session.State = 2;
                    }
                }
                else
                {
                    TSMApi.BeginWrite(this.instance, session.Handle, hlName, llName, DSMObjType.DSM_FILE);
                    session.State = 2;
                }
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when begin write, HighName = {0}, LowName = {1}, details :{2}.", hlName, llName, e);
                if (e.Message.Contains("ANS0238E"))
                {
                    logger.Warn("The session state is {0}", session.State);
                    try
                    {
                        TSMApi.EndWrite(instance, session.Handle);
                        session.State = 0;
                    }
                    catch (TSMIOException ex)
                    {
                        logger.Warn("Error when end the write, error message is {0}", ex);
                    }
                }
                this.tsmRetry.Retry(true, e, delegate
                {
                    if (this.IsSingleSession)
                    {
                        lock (locker)
                        {
                            while (this.isOperating)
                            {
                                Thread.Sleep(10);
                            }
                            TSMApi.BeginWrite(this.instance, session.Handle, hlName, llName, DSMObjType.DSM_FILE);
                            this.isWriting = true;
                            session.State = 2;
                        }
                    }
                    else
                    {
                        TSMApi.BeginWrite(this.instance, session.Handle, hlName, llName, DSMObjType.DSM_FILE);
                        session.State = 2;
                    }
                    return null;
                });
            }
        }

        public void CreateDirectory(TSMSession session, string highName, string lowName)
        {
            try
            {
                this.openParameter.HighName = highName;
                this.openParameter.LowName = lowName;
                TSMApi.BeginWrite(this.instance, session.Handle, highName, lowName, DSMObjType.DSM_DIRECTORY);
                TSMApi.EndWrite(this.instance, session.Handle);
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when begin write, HighName = {0}, LowName = {1},details {2}", highName, lowName, e);
                this.tsmRetry.Retry(true, e, delegate
                {
                    TSMApi.BeginWrite(this.instance, session.Handle, highName, lowName, DSMObjType.DSM_DIRECTORY);
                    TSMApi.EndWrite(this.instance, session.Handle);
                    return null;
                });
            }
        }

        public bool CreateValidateFile(TSMSession session)
        {
            var isSuccessful = false;
            try
            {
                TSMApi.BeginWrite(this.instance, session.Handle, TSMUtil.AddDelimiter(this.ValidateHLFileName), TSMUtil.AddDelimiter(this.ValidateLLFileName), DSMObjType.DSM_FILE);
                var info = new Byte[1024];
                TSMApi.Write(this.instance, session.Handle, info, 0, info.Length);
                TSMApi.EndWrite(this.instance, session.Handle);
                isSuccessful = true;
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when creating validate file, details {0}.", e);
            }
            return isSuccessful;
        }
        public void Write(TSMSession session, byte[] buf, Int32 off, Int32 len)

        {
            try
            {
                TSMApi.Write(this.instance, session.Handle, buf, off, len);
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when write date to TSM, details {0}.", e);
                throw;
            }
        }

        public void EndWrite(TSMSession session)
        {
            try
            {
                TSMApi.EndWrite(this.instance, session.Handle);
                session.State = 0;
                this.isWriting = false;
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when end write, details {0}.", e);
                this.tsmRetry.Retry(true, e, delegate
                {
                    TSMApi.EndWrite(this.instance, session.Handle);
                    session.State = 0;
                    return null;
                });
            }
            finally
            {
                this.isWriting = false;
            }
        }

        public void BeginRead(TSMSession session, string hlName, string llName, Int64 off, Int64 len)
        {
            try
            {
                this.openParameter.Position = off;
                this.openParameter.PartTotalLen = len;
                this.openParameter.HighName = hlName;
                this.openParameter.LowName = llName;
                TSMApi.BeginRead(this.instance, session.Handle, hlName, llName, off, len);
                session.State = 4;
            }
            catch (TSMIOException e)
            {
                logger.Error(e.Message, e);
                this.tsmRetry.Retry(true, e, delegate
                {
                    ReOpen(FileMode.Open, session.Handle);
                    TSMApi.BeginRead(this.instance, session.Handle, hlName, llName, off, len);
                    return null;
                });
            }
        }

        public Int32 Read(TSMSession session, byte[] buf, Int32 off, Int32 len)
        {
            var result = -1;
            try
            {
                result = TSMApi.Read(this.instance, session.Handle, buf, off, len);
            }
            catch (TSMIOException e)
            {
                logger.Warn(e.Message, e);
                result = (Int32)this.tsmRetry.Retry(true, e, delegate
                {
                    ReOpen(FileMode.Open, session.Handle);
                    TSMApi.BeginRead(this.instance, session.Handle, this.openParameter.HighName, this.openParameter.LowName, this.openParameter.Position, this.openParameter.PartTotalLen);
                    return TSMApi.Read(this.instance, session.Handle, buf, off, len);
                });
            }

            if (result > 0)
            {
                this.openParameter.Position += result;
                this.openParameter.PartTotalLen -= result;
            }
            return result;
        }

        public UInt32 GetLength(TSMSession session, string highName, string lowName)
        {
            var result = default(UInt32);
            try
            {
                return TSMApi.GetLength(this.instance, session.Handle, highName, lowName);
            }
            catch (TSMIOException e)
            {
                logger.Error("Error when get data length. HL_Name : {0} , LL_Name : {1}, details : {2}", highName, lowName, e);
                result = (UInt32)this.tsmRetry.Retry(true, e, delegate
                {
                    return TSMApi.GetLength(this.instance, session.Handle, highName, lowName);
                });
            }
            return result;
        }

        public void EndRead(TSMSession session)
        {
            try
            {
                TSMApi.EndRead(this.instance, session.Handle);
                session.State = 0;
            }
            catch (TSMIOException e)
            {
                logger.Error("Error occurred when end read from TSM, details {0}. ", e);
                this.tsmRetry.Retry(true, e, delegate
                {
                    TSMApi.EndRead(this.instance, session.Handle);
                    return null;
                });
            }
        }

        public Int64 DeleteObject(TSMSession session, string hlName, string llName, DSMObjType objType)
        {
            var size = default(Int64);
            try
            {
                if (this.IsSingleSession)
                {
                    lock (locker)
                    {
                        while (this.isWriting)
                        {
                            Thread.Sleep(10);
                        }
                        this.isOperating = true;
                        size = TSMApi.DeleteObjects(this.instance, session.Handle, hlName, llName, objType);
                    }
                }
                else
                {
                    size = TSMApi.DeleteObjects(this.instance, session.Handle, hlName, llName, objType);
                }
                logger.Info("Delete object succeed. High name : {0}, low name : {1}.", hlName, llName);
                return size;
            }
            catch (TSMIOException e)
            {
                logger.Error("Delete object failed. High name : {0}, low name : {1}, details {2}.", hlName, llName, e);
                return (Int64)this.tsmRetry.Retry(true, e, delegate
                {
                    var tSize = default(Int64);
                    if (this.IsSingleSession)
                    {
                        lock (locker)
                        {
                            while (this.isWriting)
                            {
                                Thread.Sleep(10);
                            }
                            this.isOperating = true;
                            tSize = TSMApi.DeleteObjects(this.instance, session.Handle, hlName, llName, objType);
                            this.isOperating = false;
                        }
                    }
                    else
                    {
                        tSize = TSMApi.DeleteObjects(this.instance, session.Handle, hlName, llName, objType);
                    }
                    logger.Info("Delete object succeed. High name : {0}, low name : {1}.", hlName, llName);
                    return tSize;
                });
            }
            finally
            {
                this.isOperating = false;
            }
        }

        public bool CheckObject(TSMSession session, string hlName, string llName, DSMObjType objType)
        {
            var result = false;
            try
            {
                if (this.IsSingleSession)
                {
                    lock (locker)
                    {
                        while (this.isWriting)
                        {
                            Thread.Sleep(10);
                        }
                        this.isOperating = true;
                        result = TSMApi.CheckObject(this.instance, session.Handle, hlName, llName, objType);
                    }
                }
                else
                {
                    result = TSMApi.CheckObject(this.instance, session.Handle, hlName, llName, objType);
                }
            }
            catch (TSMIOException e)
            {
                logger.Error("Error occurred when check object, high name : {0}, low name : {1}, details {2}.", hlName, llName, e);
                result = (bool)this.tsmRetry.Retry(true, e, delegate
                {
                    if (this.IsSingleSession)
                    {
                        lock (locker)
                        {
                            while (this.isWriting)
                            {
                                Thread.Sleep(10);
                            }
                            this.isOperating = true;
                            result = TSMApi.CheckObject(this.instance, session.Handle, hlName, llName, objType);
                            this.isOperating = false;
                            return result;
                        }
                    }
                    else
                    {
                        return TSMApi.CheckObject(this.instance, session.Handle, hlName, llName, objType);
                    }
                });
            }
            finally
            {
                this.isOperating = false;
            }
            return result;
        }

        public string[] ListObject(TSMSession session, string hlName, string lowName, bool isLowName)
        {
            var nameList = default(string[]);
            var size = default(Int32);
            if (isLowName)
            {
                size = TSMApi.GetObjectNameSize(this.instance, session.Handle, hlName, lowName);
            }
            else
            {
                //如果是查询DIR，调用查找HLName方法
                size = TSMApi.getObjectNameSizeForTool(this.instance, session.Handle, hlName, lowName);
            }
            var sbNames = new StringBuilder(size);
            TSMApi.GetObjectNames(this.instance, session.Handle, sbNames, size);
            if (sbNames.ToString().Contains("&&"))
            {
                var stringSeparators = new string[] { "&&" };
                nameList = sbNames.ToString().Split(stringSeparators, StringSplitOptions.None);
            }
            else if (!String.IsNullOrEmpty(sbNames.ToString()))
            {
                nameList = new string[1];
                nameList[0] = sbNames.ToString();
            }
            return nameList;
        }

        /// <summary>
        /// List All Directories In TSM When It Match
        /// </summary>
        /// <param name="session">The current communication session with TSM</param>
        /// <param name="hlName">The high level name</param>
        /// <param name="lowName">The low level name</param>
        /// <returns>The list of directories</returns>
        public List<XDirectoryInfo> ListDirectory(TSMSession session, string hlName, string lowName)
        {
            var objType = DSMObjType.DSM_DIRECTORY;
            var items = TSMApi.ListItems(this.instance, session.Handle, hlName, lowName, objType, DSMObjState.DSM_STATE_ACTIVE);
            //List<DSMObjectItem> items = TSMApi.ListItems(instance, session.Handle, hlName, lowName, objType, DSMObjState.DSM_STATE_ACTIVE);
            if (items != null)
            {
                var rs = new List<XDirectoryInfo>();
                foreach (var item in items)
                {
                    rs.Add(new TSMDirectoryInfo(item.HighName.TrimStart('\\').TrimEnd('\\'), item.LowName.TrimStart('\\').TrimEnd('\\')));
                }
                return rs;
            }
            return null;
        }

        /// <summary>
        /// List All Files In TSM When It Match
        /// </summary>
        /// <param name="session">The current communication session with TSM</param>
        /// <param name="hlName">The high level name</param>
        /// <param name="lowName">The low level name</param>
        /// <returns>The list of files</returns>
        public List<XFileInfo> ListFile(TSMSession session, string hlName, string lowName)
        {
            var rs = new List<XFileInfo>();
            lock (session)
            {
                var objType = DSMObjType.DSM_FILE;

                var items = TSMApi.ListItems(instance, session.Handle, hlName, lowName, objType, DSMObjState.DSM_STATE_ACTIVE);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        rs.Add(new TSMFileInfo(item.HighName.TrimStart('\\').TrimEnd('\\'), item.LowName.TrimStart('\\').TrimEnd('\\'), item.Size));
                    }
                }
            }
            return rs;
        }

        public void CloseSession(TSMSession session)
        {
            try
            {
                logger.Debug("Begin close TSM session, handle : {0}", session.Handle);
                TSMApi.CloseSession(this.instance, session.Handle);
            }
            catch (TSMIOException e)
            {
                logger.Error("Error occurred when close TSM session, details : {0}", e);
                this.tsmRetry.Retry(true, e, delegate
                {
                    TSMApi.CloseSession(this.instance, session.Handle);
                    return null;
                });
            }
        }

        public void Close()
        {
            TSMApi.ReleaseInstance(this.instance);
        }

        public void ReOpen(FileMode mode, UInt32 handle)
        {
            //read
            if (mode == FileMode.Open)
            {
                try
                {
                    TSMApi.EndRead(this.instance, handle);
                }
                catch (TSMIOException e)
                {
                    logger.Warn(e.Message);
                }
            }
        }

        public void Dispose()
        {
            logger.Info("TSM client disposed.");
        }

        public void CleanUpNoUseData(TSMNodeInfo mNodeInfo)
        {
            this.config.CleanUpNoUseData(mNodeInfo);
        }

        public void CleanUpValidateData(TSMNodeInfo mNodeInfo)
        {
            this.config.CleanUpValidateData(mNodeInfo);
        }

        public Int32 GetObjectNameSizeWithDate(TSMSession session, String hlName, String llName)
        {
            try
            {

                //var size = TSMApi.GetObjectNameSizeWithDate(this.instance, session.Handle, hlName, llName, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
                var size = TSMApi.getObjectNameSizeForTool(this.instance, session.Handle, hlName, llName);
                logger.Info("GetObjectNameSizeWithDate succeed. High name : {0}.", hlName);
                return size;
            }
            catch (TSMIOException e)
            {
                logger.Error("GetObjectNameSizeWithDate failed. High name : {0}.", hlName);
                return (Int32)this.tsmRetry.Retry(true, e, delegate
                {
                    var tSize = default(Int32);
                    tSize = TSMApi.GetObjectNameSizeWithDate(this.instance, session.Handle, hlName, llName, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
                    logger.Info("GetObjectNameSizeWithDate succeed. High name : {0}.", hlName);
                    return tSize;
                });
            }
            finally
            {
                this.isOperating = false;
            }
        }

        public Int32 GetObjectNameSize(TSMSession session, String hlName, String llName)
        {
            try
            {

                var size = TSMApi.GetObjectNameSize(this.instance, session.Handle, hlName, llName);
                logger.Info("GetObjectNameSize succeed. High name : {0}.", hlName);
                return size;
            }
            catch (TSMIOException e)
            {
                logger.Error("GetObjectNameSizet failed. High name : {0}.", hlName);
                return (Int32)this.tsmRetry.Retry(true, e, delegate
                {
                    var tSize = default(Int32);
                    tSize = TSMApi.GetObjectNameSize(this.instance, session.Handle, hlName, llName);
                    logger.Info("GetObjectNameSize succeed. High name : {0}.", hlName);
                    return tSize;
                });
            }
            finally
            {
                this.isOperating = false;
            }
        }

        public String[] GetObjectNames(TSMSession session, StringBuilder names, Int32 size)
        {
            String[] lowNameList = new string[] { };
            try
            {
                TSMApi.GetObjectNames(this.instance, session.Handle, names, size);
                logger.Info("GetObjectNameSize succeed.");
            }
            catch (TSMIOException e)
            {
                logger.Error("GetObjectNameSize failed");
                this.tsmRetry.Retry(true, e, () => this.GetObjectNames(session, names, size));
            }
            if (names.ToString().Contains("&&"))
            {
                var stringSeparators = new String[] { "&&" };
                lowNameList = names.ToString().Split(stringSeparators, StringSplitOptions.None);
            }
            else if (!String.IsNullOrEmpty(names.ToString()))
            {
                lowNameList = new String[1];
                lowNameList[0] = names.ToString();
            }
            return lowNameList;
        }
    }
}
