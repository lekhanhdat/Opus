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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Restore
{
    internal class AveWorkflowRestoreCore
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public SPWFAssociationProc WFAssociationProcessor = null;
        public SPWFAssociationProc WFAssociationProcessor13Model = null;
        public SPWFAssociationProc WFAssociationProcessorProject = null;
        public SPWFInstanceProc WFInstanceProcessor = null;
        public SPWFInstanceProc WFInstanceProcessor13Model = null;
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
            : this(null, null, null, null) { }

        public AveWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
        {
            WFAssociationProcessor = assoProcessor == null ? SPWFAssociationProc.CreateInstance(SPWFProcessorType.API) : assoProcessor;
            WFAssociationProcessor13Model = assoProcessor13Model == null ? SPWFAssociationProc.CreateInstance(SPWFProcessorType.API13Model) : assoProcessor13Model;
            WFInstanceProcessor = instanceProcessor == null ? SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native) : instanceProcessor;
            WFInstanceProcessor13Model = instanceProcessor13Model == null ? SPWFInstanceProc.CreateInstance(SPWFProcessorType.Native13Model) : instanceProcessor13Model;
            WFAssociationProcessor.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessor13Model.CustomProcessors = SPWorkflowProcessorRuntime.CustomAssociationProcessors;
            WFAssociationProcessorProject = SPWFAssociationProc.CreateInstance(SPWFProcessorType.Project);
            SPWorkflowFileContentProc.CustomContentProcessors = SPWorkflowProcessorRuntime.CustomTemplateContentProcessors;

        }

        public virtual void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object parent) { }

        protected void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object parent, SPWFAssociationParentType parentType)
        {
            switch (plantForm)
            {
                case SPWFInternalPlatform.WF2010PlatformType:
                    WFAssociationProcessor.ParentObject = parent;
                    WFAssociationProcessor.ParentObjectType = parentType;
                    break;
                case SPWFInternalPlatform.WF2013PlatformType:
                    WFAssociationProcessor13Model.ParentObject = parent;
                    WFAssociationProcessor13Model.ParentObjectType = parentType;
                    break;
                case SPWFInternalPlatform.WFProjectPlatformType:
                    WFAssociationProcessorProject.ParentObject = parent;
                    WFAssociationProcessorProject.ParentObjectType = parentType;
                    break;
                default:
                    log.Log(AveLogLevel.WARN, "Unsppurt workflow platform.");
                    break;
            }
        }

        public virtual void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object parent) { }

        internal void RestoreSchedule(SPWFInstanceUnit assoUnit, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {
#endif
                InstanceProcCreationParam param = null;
                try
                {
                    param = new InstanceProcCreationParam();
                    param.ParentItem = item;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
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
                    throw new AveWrapperWorkflowException(WrapperWorkflowResource.RestoreWFInError, procException);
                }
                catch (Exception e)
                {
                    throw new AveWrapperWorkflowException(WrapperWorkflowResource.RestoreWFInUnexpectError, e);
                }
                finally
                {
                    if (param?.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal void RestoreSchedule(SPWFInstanceUnit assoUnit, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {
#endif
                try
                {

                    InstanceProcCreationParam param = new InstanceProcCreationParam();
                    param.ParentWeb = web;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
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
#if PerformanceLog
            }
#endif
        }

        internal void RestoreInstance(SPWFInstanceUnit assoUnit, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {
#endif
                InstanceProcCreationParam param = null;
                try
                {
                    switch (assoUnit.WFInternalPlatform)
                    {
                        case SPWFInternalPlatform.WF2010PlatformType:

                            param = new InstanceProcCreationParam();
                            param.ParentItem = item;
                            //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
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
                    throw new AveWrapperWorkflowException(WrapperWorkflowResource.RestoreWFInError, procException);
                }
                catch (Exception e)
                {
                    throw new AveWrapperWorkflowException(WrapperWorkflowResource.RestoreWFInUnexpectError, e);
                }
                finally
                {
                    if (param?.QueryService != null)
                    {
                        //将连接释放
                        param.QueryService.Dispose();
                        param.QueryService = null;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal void RestoreInstance(SPWFInstanceUnit assoUnit, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreInstance"))
            {
#endif
                try
                {

                    InstanceProcCreationParam param = new InstanceProcCreationParam();
                    param.ParentWeb = web;
                    //param.Conn = new AveSqlConnection(item.ParentList.ParentWeb.Site.ContentDatabase.DatabaseConnectionString).Connection;
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
#if PerformanceLog
            }
#endif
        }

        protected void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object parent, SPWFAssociationParentType parentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWorkflowRestoreCore.RestoreWorkflowAssociation"))
            {
#endif
                try
                {
                    SetWorkflowParentTypeAndObject(assoUnit.WFInternalPlatform, parent, parentType);
                    switch (assoUnit.WFInternalPlatform)
                    {
                        case SPWFInternalPlatform.WF2010PlatformType:
                            WFAssociationProcessor.Restore(assoUnit, ForceUpdate);
                            break;
                        case SPWFInternalPlatform.WF2013PlatformType:
                            WFAssociationProcessor13Model.Restore(assoUnit, ForceUpdate);
                            break;
                        case SPWFInternalPlatform.WFProjectPlatformType:
                            WFAssociationProcessorProject.Restore(assoUnit, ForceUpdate);
                            break;
                        default:
                            log.Log(AveLogLevel.WARN, "Unsppurt workflow platform.");
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

#if PerformanceLog
            }
#endif
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
    }

    internal class AveWebWorkflowRestoreCore : AveWorkflowRestoreCore
    {
        public AveWebWorkflowRestoreCore()
            : base()
        { }
        public AveWebWorkflowRestoreCore(SPWFAssociationProc assoProcessor, SPWFAssociationProc assoProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
            : base(assoProcessor, assoProcessor13Model, instanceProcessor, instanceProcessor13Model)
        { }

        public override void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object web)
        {
            SetWorkflowParentTypeAndObject(plantForm, web, SPWFAssociationParentType.Web);
        }


        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object web)
        {
            RestoreWorkflowAssociation(assoUnit, web, SPWFAssociationParentType.Web);
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
        public override void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object webContentType)
        {
            SetWorkflowParentTypeAndObject(plantForm, webContentType, SPWFAssociationParentType.WebContentType);
        }

        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object contentType)
        {
            RestoreWorkflowAssociation(assoUnit, contentType, SPWFAssociationParentType.WebContentType);
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
        public override void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object list)
        {
            SetWorkflowParentTypeAndObject(plantForm, list, SPWFAssociationParentType.List);
        }
        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object list)
        {
            RestoreWorkflowAssociation(assoUnit, list, SPWFAssociationParentType.List);
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
        public override void SetWorkflowParentTypeAndObject(SPWFInternalPlatform plantForm, object contentType)
        {
            SetWorkflowParentTypeAndObject(plantForm, contentType, SPWFAssociationParentType.ListContentType);
        }
        public override void RestoreWorkflowAssociation(SPWFAssociationUnit assoUnit, object contentType)
        {
            RestoreWorkflowAssociation(assoUnit, contentType, SPWFAssociationParentType.ListContentType);
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
