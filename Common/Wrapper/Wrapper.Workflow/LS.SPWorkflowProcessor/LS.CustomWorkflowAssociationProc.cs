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
using System.IO;
using System.Reflection;
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    public delegate void CustomAssociationProcBackupDelegate(IAveList parentList, SPWFAssociationUnit parentAsso);
    public delegate void CustomAssociationProcRestoreDelegate(IAveList parentList, SPWFAssociationUnit parentAsso, IAveWorkflowAssociation spAsso);
    
    public interface ICustomWorkflowAssociationProc
    {
        void BackupCustomWorkflowData(SPWFAssociationUnit parentAsso);
        void RestoreCustomWorkflowData(SPWFAssociationUnit parentAsso);
    }

    public class CustomWorkflowAssociationProc
    {

        public event CustomAssociationProcBackupDelegate mBackupProc;
        public event CustomAssociationProcRestoreDelegate mRestoreProc;

        private List<ICustomWorkflowAssociationProc> mCustomProcs;
        public List<ICustomWorkflowAssociationProc> CustomProcessors
        {
            get { return mCustomProcs; }
        }

        public CustomWorkflowAssociationProc(List<ICustomWorkflowAssociationProc> procs)
        {
            mCustomProcs = procs;
        }

        public void FireBackupCustomWorkflowDataEvent(SPWFAssociationUnit parentAsso)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireBackupCustomWorkflowDataEvent");
            if (CustomProcessors == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomAssociationProc);
                return;
            }
            try
            {
                #region Event
                //foreach (ICustomWorkflowAssociationProc procInterface in mCustomProcs)
                //{
                //    mBackupProc += new CustomAssociationProcBackupDelegate(procInterface.BackupCustomWorkflowData);
                //}
                //if (mBackupProc != null)
                //    mBackupProc(parentList, parentAsso);
                //foreach (ICustomWorkflowAssociationProc procInterface in mCustomProcs)
                //{
                //    mBackupProc -= new CustomAssociationProcBackupDelegate(procInterface.BackupCustomWorkflowData);
                //}
                #endregion

                foreach (ICustomWorkflowAssociationProc procInterface in CustomProcessors)
                {
                    procInterface.BackupCustomWorkflowData(parentAsso);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AssociationCustomDataBackupException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCustomDataBackupError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireBackupCustomWorkflowDataEvent");
            }
        }

        public void FireRestoreCustomWorkflowDataEvent(SPWFAssociationUnit parentAsso)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "FireRestoreCustomWorkflowDataEvent");
            if (CustomProcessors == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomAssociationProc);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireRestoreCustomWorkflowDataEvent");
                return;
            }
            if (parentAsso.SerializableData.mSerializableCustomData == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NoCustomData);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireRestoreCustomWorkflowDataEvent");
                return;
            }
            try
            {
                foreach (ICustomWorkflowAssociationProc procInterface in CustomProcessors)
                {
                    procInterface.RestoreCustomWorkflowData(parentAsso);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.AssociationCustomDataRestoreException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCustomDataRestoreError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "FireRestoreCustomWorkflowDataEvent");
            }
        }
    }
}
