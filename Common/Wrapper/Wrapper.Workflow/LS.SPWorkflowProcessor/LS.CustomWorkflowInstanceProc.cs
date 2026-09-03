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
using System.Data.SqlClient;
using System.IO;

namespace LS.SPWorkflowProcessor
{
    public interface ICustomWorkflowInstanceProc
    {
        void BackupCustomWorkflowData(SPWorkflowSubItemUnit parentUnit);
        void RestoreCustomWorkflowData(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem);
        void ResetData(SPWFInstanceUnit parentUnit);
        void OnSPInstanceDeleted(Guid siteId, List<Guid> instanceId);
    }

    public class CustomWorkflowInstanceProc
    {
        

        private List<ICustomWorkflowInstanceProc> mCustomProcs;
        public List<ICustomWorkflowInstanceProc> CustomProcessors
        {
            get { return mCustomProcs; }
        }

        public CustomWorkflowInstanceProc(List<ICustomWorkflowInstanceProc> procs)
        {
            mCustomProcs = procs;
        }

        public void FireBackupCustomWorkflowDataEvent(SPWorkflowSubItemUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireBackupCustomWorkflowDataEvent");
            if (mCustomProcs == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomInstanceProc);
                return;
            }
            try
            {
                foreach (ICustomWorkflowInstanceProc procInterface in mCustomProcs)
                {
                    procInterface.BackupCustomWorkflowData(parentUnit);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.InstanceCustomDataBackupException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceCustomDataBackupError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireBackupCustomWorkflowDataEvent");
            }
        }

        public void FireRestoreCustomWorkflowDataEvent(SPWFInstanceUnit parentUnit, SPWorkflowSubItemUnit parentItem)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireRestoreCustomWorkflowDataEvent");
            if (mCustomProcs == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomInstanceProc);
                return;
            }
            try
            {
                foreach (ICustomWorkflowInstanceProc procInterface in mCustomProcs)
                {
                    procInterface.RestoreCustomWorkflowData(parentUnit, parentItem);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.InstanceCustomDataRestoreException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreCustomDataError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireRestoreCustomWorkflowDataEvent");
            }
        }

        public void FireResetData(SPWFInstanceUnit parentUnit)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireResetData");
            if (mCustomProcs == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomInstanceProc);
                return;
            }
            try
            {
                foreach (ICustomWorkflowInstanceProc procInterface in mCustomProcs)
                {
                    procInterface.ResetData(parentUnit);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.InstanceCustomDataRestoreException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreCustomDataError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireResetData");
            }
        }

        public void FireInstanceDeletedEvent(Guid siteId, List<Guid> instanceId)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireInstanceDeletedEvent");
            if (mCustomProcs == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomInstanceProc);
                return;
            }
            try
            {
                foreach (ICustomWorkflowInstanceProc procInterface in mCustomProcs)
                {
                    procInterface.OnSPInstanceDeleted(siteId, instanceId);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.InstanceCustomDataRestoreException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.InstanceRestoreCustomDataError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireInstanceDeletedEvent");
            }
        }
    }
}
