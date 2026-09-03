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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.Workflow;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    internal class AveWorkflowRestoreCore : IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public SPWFAssociationProc WFAssociationProcessor = null;
        public SPWFAssociationProc WFAssociationProcessor13Model = null;
        public SPWFInstanceProc WFInstanceProcessor = null;
        public SPWFInstanceProc WFInstanceProcessor13Model = null;
        public SPExportedNintexWorkflowAssociation WFAssociationProcessorExportedNintex = null;

        internal SPWFAssociationParentType ParentType = SPWFAssociationParentType.Invalid;

        private bool mForceUpdate = true;
        internal bool ForceUpdate
        {
            get
            { return mForceUpdate; }
            set
            { mForceUpdate = value; }
        }





        public AveWorkflowRestoreCore()
            : this(null, null, null, null)
        { }

        public AveWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
        {
            WFAssociationProcessor = assoProcessor == null ? SPWFAssociationProc.CreateInstance(SPWFProcessorType.API) : assoProcessor;
            WFAssociationProcessor13Model = assoProcessor13Model == null ? SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model) : assoProcessor13Model;
            WFInstanceProcessor = instanceProcessor == null ? SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native) : instanceProcessor;
            WFInstanceProcessor13Model = instanceProcessor13Model == null ? SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native13Model) : instanceProcessor13Model;
            WFAssociationProcessor.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessor13Model.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessorExportedNintex = new SPExportedNintexWorkflowAssociation();
            SPWorkflowFileContentProc.CustomContentProcessors = SPWorkflowProcessorRuntime.CustomTemplateContentProcessors;

        }

        public virtual void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object parent, WFAveSPObjectCache spObjectCache, bool isPostAction) { }

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks")]
        internal void RestoreInstance(SPWFInstanceUnit assoUnit, IAveListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {

                InstanceProcCreationParam param = null;
                try
                {
                    switch (assoUnit.WFInternalPlatform)
                    {
                        case SPWFInternalPlatform.WF2010PlatformType:

                            param = new InstanceProcCreationParam();
                            param.ParentItem = item;
                            //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
                            //restore时param.QueryService只会通过该种方式new出来QueryService，在RestoreInstance后会dispose,所以该QueryService暂时不会有问题
                            param.QueryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(item.ParentList.ParentWeb.Site);
                            param.ProcType = SPWFProcessorType.Native;
                            param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                            param.AssociationProc = WFAssociationProcessor;
                            param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                            param.Overwrite = true;
                            param.Append = false;
                            WFInstanceProcessor.ParentItem = new AveWFParentItem();
                            WFInstanceProcessor.ParentItem.ParentItemType = WFParentItemType.ListItem;
                            WFInstanceProcessor.SetInstanceProcParameters(param);
                            WFInstanceProcessor.Restore(assoUnit);
                            break;
                        case SPWFInternalPlatform.WF2013PlatformType:
                            param = new InstanceProcCreationParam();
                            param.ParentItem = item;
                            //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
                            //restore时param.QueryService只会通过该种方式new出来QueryService，在RestoreInstance后会dispose,所以该QueryService暂时不会有问题
                            param.QueryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(item.ParentList.ParentWeb.Site);
                            param.ProcType = SPWFProcessorType.Native13Model;
                            param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                            param.AssociationProc = WFAssociationProcessor13Model;
                            param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                            param.Overwrite = true;
                            param.Append = false;
                            WFInstanceProcessor13Model.ParentItem = new AveWFParentItem();
                            WFInstanceProcessor13Model.ParentItem.ParentItemType = WFParentItemType.ListItem;
                            WFInstanceProcessor13Model.SetInstanceProcParameters(param);
                            WFInstanceProcessor13Model.Restore(assoUnit);
                            break;
                        default:
                            break;
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    if (procException.ErrorCode == SPWFProcessorErrorCode.InstanceIsRunningException)
                    {
                        throw new AveWrapperWorkflowException(WrapperWorkflowResource.SkipRunningWFInError, procException);
                    }
                    throw new AveWrapperWorkflowException(procException, AveInternalResourceKey.Wrapper_Exception_Workflow_RestoreWorkflowInstanceError);
                }
                catch (Exception e)
                {
                    throw new AveWrapperWorkflowException(e, AveInternalResourceKey.Wrapper_Exception_Workflow_RestoreWorkflowInstanceError);
                }
                finally
                {
                    if (param.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100013:CheckExistingExceptionHandlingBlocks")]
        internal void RestoreSchedule(SPWFInstanceUnit assoUnit, IAveListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {

                InstanceProcCreationParam param = null;
                try
                {
                    param = new InstanceProcCreationParam();
                    param.ParentItem = item;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
                    //restore时param.QueryService只会通过该种方式new出来QueryService，在RestoreSchedule后会dispose,所以该QueryService暂时不会有问题
                    param.QueryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(item.ParentList.ParentWeb.Site);
                    param.ProcType = SPWFProcessorType.Native;
                    param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                    param.AssociationProc = WFAssociationProcessor;
                    param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                    param.Overwrite = true;
                    param.Append = false;
                    WFInstanceProcessor.ParentItem = new AveWFParentItem();
                    WFInstanceProcessor.ParentItem.ParentItemType = WFParentItemType.ListItem;
                    WFInstanceProcessor.SetInstanceProcParameters(param);
                    WFInstanceProcessor.RestoreSchedule(assoUnit);

                }
                catch (SPWFProcessorException procException)
                {
                    throw new AveWrapperWorkflowException(procException, AveInternalResourceKey.Wrapper_Exception_Workflow_RestoreWorkflowInstanceError);
                }
                catch (Exception e)
                {
                    throw new AveWrapperWorkflowException(e, AveInternalResourceKey.Wrapper_Exception_Workflow_RestoreWorkflowInstanceError);
                }
                finally
                {
                    if (param.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }

            }

        }

        internal void RestoreSchedule(SPWFInstanceUnit assoUnit, IAveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {
                InstanceProcCreationParam param = new InstanceProcCreationParam();
                try
                {
                    param.ParentWeb = web;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
                    //restore时param.QueryService只会通过该种方式new出来QueryService，在还原完Schedule后会dispose,所以该QueryService暂时不会有问题
                    param.QueryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(web.Site);
                    param.ProcType = SPWFProcessorType.Native;
                    param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                    param.AssociationProc = WFAssociationProcessor;
                    param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                    param.Overwrite = true;
                    param.Append = false;
                    WFInstanceProcessor.ParentItem = new AveWFParentItem();
                    WFInstanceProcessor.ParentItem.ParentItemType = WFParentItemType.Web;
                    WFInstanceProcessor.SetInstanceProcParameters(param);
                    WFInstanceProcessor.RestoreSchedule(assoUnit);

                }
                catch (SPWFProcessorException procException)
                {
                    string errMsg = (procException.ProcInnerException == null) ? procException.ErrorCodeString : procException.ProcInnerException.Message;
                    log.Log(AveLogLevel.INFO, errMsg, procException);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreInstanceError, e);
                }
                finally
                {
                    if (param.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }

            }

        }

        internal void RestoreInstance(SPWFInstanceUnit assoUnit, IAveWeb web)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {

                InstanceProcCreationParam param = new InstanceProcCreationParam();
                try
                {
                    param.ParentWeb = web;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
                    //restore时param.QueryService只会通过该种方式new出来QueryService，在RestoreInstance后会dispose,所以该QueryService暂时不会有问题
                    param.QueryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(web.Site);
                    param.ProcType = SPWFProcessorType.Native;
                    param.CustomProcessors = SPWorkflowProcessorRuntime.CustomInstanceProcessors;
                    param.AssociationProc = WFAssociationProcessor;
                    param.WebLevelFieldProcessor = new SPFieldProcessor(SPFieldProcessorScope.Web);
                    param.Overwrite = true;
                    param.Append = false;
                    WFInstanceProcessor.ParentItem = new AveWFParentItem();
                    WFInstanceProcessor.ParentItem.ParentItemType = WFParentItemType.Web;
                    WFInstanceProcessor.SetInstanceProcParameters(param);
                    WFInstanceProcessor.Restore(assoUnit);

                }
                catch (SPWFProcessorException procException)
                {
                    string errMsg = (procException.ProcInnerException == null) ? procException.ErrorCodeString : procException.ProcInnerException.Message;
                    log.Log(AveLogLevel.INFO, errMsg, procException);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreInstanceError, e);
                }
                finally
                {
                    if (param.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }

            }

        }

        protected void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object parent, SPWFAssociationParentType parentType, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreWorkflowAssociation"))
            {

                try
                {
                    switch (assoUnit.WFInternalPlatform)
                    {
                        case SPWFInternalPlatform.WF2010PlatformType:
                            WFAssociationProcessor.ParentObject = parent;
                            WFAssociationProcessor.ParentObjectType = parentType;
                            WFAssociationProcessor.AveSPObjectCache = spObjectCache;
                            WFAssociationProcessor.Restore(assoUnit, ForceUpdate, isPostAction);
                            break;
                        case SPWFInternalPlatform.WF2013PlatformType:
                            WFAssociationProcessor13Model.ParentObject = parent;
                            WFAssociationProcessor13Model.ParentObjectType = parentType;
                            WFAssociationProcessor13Model.AveSPObjectCache = spObjectCache;
                            WFAssociationProcessor13Model.Restore(assoUnit, ForceUpdate, isPostAction);
                            break;
                        case SPWFInternalPlatform.WFExportedNintex:
                            WFAssociationProcessorExportedNintex.ParentObject = parent;
                            WFAssociationProcessorExportedNintex.ParentObjectType = parentType;
                            WFAssociationProcessorExportedNintex.AveSPObjectCache = spObjectCache;
                            WFAssociationProcessorExportedNintex.Restore(assoUnit, ForceUpdate, isPostAction);
                            break;
                        default:
                            log.Log(AveLogLevel.WARN, "Do not support workflow platform.");
                            break;
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while restoring workflow definition.", procException);
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveWrapperWorkflowException(WrapperWorkflowResource.WFDeUnexpectError, e);
                }


            }

        }

        internal void ExecutePostAction()
        {
            if (!SPWorkflowProcessorRuntime.ProcessAssociation)
            {
                return;
            }
            try
            {
                if (!SPWorkflowProcessorRuntime.ProcessInstance)
                {
                    SPWorkflowProcessorRuntime.ExecutePostAction(WFAssociationProcessor, WFAssociationProcessor13Model, null, null);
                    return;
                }
                SPWorkflowProcessorRuntime.ExecutePostAction(WFAssociationProcessor, WFAssociationProcessor13Model, WFInstanceProcessor, WFInstanceProcessor13Model);
            }
            catch (SPWFProcessorException procException)
            {
                string errMsg = (procException.ProcInnerException == null) ? procException.ErrorCodeString : procException.ProcInnerException.Message;
                string msg = string.Format("An error occurred while executing workflow post action. detail:{0}", errMsg);
                log.Log(AveLogLevel.INFO, msg, procException);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An unknown error occurred while executing workflow post action.", e);
            }
        }

        public void Dispose()
        {
            if (WFAssociationProcessor != null)
            {
                WFAssociationProcessor.Dispose();
                WFAssociationProcessor = null;
            }
            if (WFAssociationProcessor13Model != null)
            {
                WFAssociationProcessor13Model.Dispose();
                WFAssociationProcessor13Model = null;
            }
            if (WFInstanceProcessor != null)
            {
                WFInstanceProcessor.Dispose();
                WFInstanceProcessor = null;
            }
            if (WFInstanceProcessor13Model != null)
            {
                WFInstanceProcessor13Model.Dispose();
                WFInstanceProcessor13Model = null;
            }
            if (WFAssociationProcessorExportedNintex != null)
            {
                WFAssociationProcessorExportedNintex.Dispose();
                WFAssociationProcessorExportedNintex = null;
            }
        }
    }

    internal class AveWebWorkflowRestoreCore : AveWorkflowRestoreCore
    {
        public AveWebWorkflowRestoreCore()
            : base()
        { }
        public AveWebWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
            : base(assoProcessor, assoProcessor13Model, instanceProcessor, instanceProcessor13Model)
        { }
        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object web, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            RestoreWorkflowAssociation(assoUnit, web, SPWFAssociationParentType.Web, spObjectCache, isPostAction);
        }
    }

    internal class AveWebContentTypeWorkflowRestoreCore : AveWorkflowRestoreCore
    {
        public AveWebContentTypeWorkflowRestoreCore()
            : base()
        { }
        public AveWebContentTypeWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
            : base(assoProcessor, assoProcessor13Model, instanceProcessor, instanceProcessor13Model)
        { }
        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object contentType, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            RestoreWorkflowAssociation(assoUnit, contentType, SPWFAssociationParentType.WebContentType, spObjectCache, isPostAction);
        }
    }

    internal class AveListWorkflowRestoreCore : AveWorkflowRestoreCore
    {
        public AveListWorkflowRestoreCore()
            : base()
        { }
        public AveListWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
            : base(assoProcessor, assoProcessor13Model, instanceProcessor, instanceProcessor13Model)
        { }

        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object list, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            RestoreWorkflowAssociation(assoUnit, list, SPWFAssociationParentType.List, spObjectCache, isPostAction);
        }
    }

    internal class AveListContentTypeWorkflowRestoreCore : AveWorkflowRestoreCore
    {
        public AveListContentTypeWorkflowRestoreCore()
            : base()
        { }
        public AveListContentTypeWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
            : base(assoProcessor, assoProcessor13Model, instanceProcessor, instanceProcessor13Model)
        { }

        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object contentType, WFAveSPObjectCache spObjectCache, bool isPostAction)
        {
            RestoreWorkflowAssociation(assoUnit, contentType, SPWFAssociationParentType.ListContentType, spObjectCache, isPostAction);
        }
    }

    internal static class AveWorkflowRestoreCoreFactory
    {
        internal static AveWorkflowRestoreCore GetWorkflowRestoreCore(SPWFAssociationParentType parentType, SPWFAssociationProc assoProc, SPWFAssociationProc assoProc13Model, SPWFInstanceProc instanceProc, SPWFInstanceProc instanceProc13Model)
        {
            AveWorkflowRestoreCore restoreCore;
            switch (parentType)
            {
                case SPWFAssociationParentType.List:
                    restoreCore = new AveListWorkflowRestoreCore(assoProc, assoProc13Model, instanceProc, instanceProc13Model);
                    break;
                case SPWFAssociationParentType.ListContentType:
                    restoreCore = new AveListContentTypeWorkflowRestoreCore(assoProc, assoProc13Model, instanceProc, instanceProc13Model);
                    break;
                case SPWFAssociationParentType.Web:
                    restoreCore = new AveWebWorkflowRestoreCore(assoProc, assoProc13Model, instanceProc, instanceProc13Model);
                    break;
                case SPWFAssociationParentType.WebContentType:
                    restoreCore = new AveWebContentTypeWorkflowRestoreCore(assoProc, assoProc13Model, instanceProc, instanceProc13Model);
                    break;
                default:
                    throw new AveException("Invalid parent type.");
            }
            restoreCore.ParentType = parentType;
            return restoreCore;
        }
    }
}
