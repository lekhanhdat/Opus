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
using System.Xml;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
namespace LS.SPWorkflowProcessor
{
    public class SPContentTypeProcessor
    {
        private Guid mPreWebId;
        private int mIndex;
        private int mLevel;

        private Dictionary<string, string> mContentTypeIdMaping;
        public Dictionary<string, string> ContentTypeIdMaping
        {
            get
            {
                if (mContentTypeIdMaping == null)
                    mContentTypeIdMaping = new Dictionary<string, string>();
                return mContentTypeIdMaping;
            }
        }


        #region ************************Backup  Region************************
        public List<SPContentTypeUnit> BackupContentTypes(IAveContentTypeCollection cts)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupContentTypes");
            List<SPContentTypeUnit> units = new List<SPContentTypeUnit>();
            foreach (IAveContentType ct in cts)
            {
                SPContentTypeUnit unit = GenerateInheritanceTree(ct);
                units.Add(unit);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupContentTypes");
            return units;
        }

        private SPContentTypeUnit GenerateInheritanceTree(IAveContentType ct)
        {
            mPreWebId = Guid.Empty;
            mLevel = -1;
            mIndex = -1;
            return InheritanceTreeNode(ct);
        }

        private SPContentTypeUnit InheritanceTreeNode(IAveContentType currentCT)
        {
            SPContentTypeUnit unit = new SPContentTypeUnit(currentCT);
            ResetLevelAndIndex(currentCT.ParentWeb.ID, unit);
            if (AveBuiltInContentTypeId.Contains(currentCT.ID))
                unit.mParentUnit = null;
            else
                unit.mParentUnit = InheritanceTreeNode(currentCT.Parent);
            return unit;
        }

        private void ResetLevelAndIndex(Guid currentWebId, SPContentTypeUnit unit)
        {
            if (currentWebId != mPreWebId)
            {
                mPreWebId = currentWebId;
                mLevel++;
            }
            mIndex++;

            unit.SerializableData.mLevel = mLevel;
            unit.SerializableData.mIndex = mIndex;
        }
        #endregion


        #region ************************Restore Region************************
        public void RestoreListContentTypes(IAveList parentList, List<SPContentTypeUnit> ctUnits)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreListContentTypes");
            if (ctUnits == null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.CT_MissingContentTypes, parentList.Title);
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreListContentTypes");
                return;
            }
            foreach (SPContentTypeUnit leafUnit in ctUnits)
            {
                try
                {
                    List<SPContentTypeUnit> allUnits = leafUnit.GetAllUnits(true);
                    foreach (SPContentTypeUnit curUnit in allUnits)
                    {
                        SetParentCTCollection(parentList, curUnit);
                        RestoreContentTypeUnit(curUnit);
                    }
                    if (leafUnit.SerializableData.mId != leafUnit.SerializableData.mNewId)
                        ContentTypeIdMaping.Add(leafUnit.SerializableData.mId.ToUpperEx(2, leafUnit.SerializableData.mId.Length - 2), leafUnit.SerializableData.mNewId.ToUpperEx(2, leafUnit.SerializableData.mNewId.Length - 2));
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.CT_RestoreUnknownException, leafUnit.SerializableData.mId, e.Message);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreListContentTypes");
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ContentTypeRestoreError, e, leafUnit.SerializableData.mId);
                }
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreListContentTypes");
        }

        private void SetParentCTCollection(IAveList parentList, SPContentTypeUnit unit)
        {
            if (unit.SerializableData.mParentScope == SPContentTypeScope.List)
                unit.mSPContentTypeCollection = parentList.ContentTypes;
            else if (unit.SerializableData.mParentScope == SPContentTypeScope.Web)
            {
                IAveWeb parentWeb = parentList.ParentWeb;
                for (int i = 0; i < unit.SerializableData.mLevel; i++)
                {
                    if (parentWeb.ParentWeb != null)
                    {
                        parentWeb = parentWeb.ParentWeb;
                    }
                }
                unit.mSPContentTypeCollection = parentWeb.ContentTypes;

                IAveContentTypeId srcId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
                if (AveBuiltInContentTypeId.Contains(srcId))
                    return;

                IAveContentType dstCT = unit.mSPContentTypeCollection[unit.SerializableData.mName];
                if (dstCT == null)
                {
                    while (parentWeb.ParentWeb != null)
                    {
                        parentWeb = parentWeb.ParentWeb;
                        dstCT = parentWeb.ContentTypes[unit.SerializableData.mName];
                        if (dstCT != null)
                        {
                            unit.mSPContentTypeCollection = parentWeb.ContentTypes;
                            break;
                        }
                    }
                }
            }
        }

        private void RestoreContentTypeUnit(SPContentTypeUnit unit)
        {
            CTConflictAction conflictAction = HandleContentTypeConflict(unit);
            IAveContentType ct = null;
            switch (conflictAction)
            {
                case CTConflictAction.CreateNew:
                case CTConflictAction.Rename:
                    IAveContentType parentCT = unit.mParentUnit.mSPContentTypeCollection[unit.mParentUnit.SerializableData.mName];
                    ct = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentType(parentCT, unit.mSPContentTypeCollection, unit.SerializableData.mName);
                    unit.mSPContentTypeCollection.Add(ct);
                    string temp = ct.Name;
                    ct = null;
                    ct = unit.mSPContentTypeCollection[temp];
                    break;
                case CTConflictAction.Update:
                    ct = unit.mSPContentTypeCollection[unit.SerializableData.mName];
                    break;
                case CTConflictAction.Builtin:
                    return;
                default:
                    throw new Exception("Not supported");
            }

            unit.SetSPObjectPropByUnit(ct);
            unit.SerializableData.mNewId = ct.ID.ToString();
        }

        private string ctConflictProfix = ".LS.";
        private int ctConfilictIndex = 0;
        private CTConflictAction HandleContentTypeConflict(SPContentTypeUnit unit)
        {
            IAveContentTypeId srcId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
            if (AveBuiltInContentTypeId.Contains(srcId))
                return CTConflictAction.Builtin;

            IAveContentType dstCT = unit.mSPContentTypeCollection[unit.SerializableData.mName];
            if (dstCT == null)
                return CTConflictAction.CreateNew;
            if (AveBuiltInContentTypeId.Contains(dstCT.ID))
                return CTConflictAction.Builtin;
            if (dstCT.ID.CompareTo(srcId) == 0)
                return CTConflictAction.Update;

            IAveContentTypeId sId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
            IAveContentTypeId dId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(dstCT.ID.ToString());
            while (!AveBuiltInContentTypeId.Contains(sId))
            {
                sId = sId.Parent;
            }
            while (!AveBuiltInContentTypeId.Contains(dId))
            {
                dId = dId.Parent;
            }

            if (sId == dId || sId.IsChildOf(dId))
                return CTConflictAction.Update;
            else
            {
                unit.SerializableData.mName = unit.SerializableData.mOriginalName + ctConflictProfix + ctConfilictIndex;
                ctConfilictIndex++;
                return HandleContentTypeConflict(unit);
            }
        }

        private enum CTConflictAction
        {
            CreateNew,
            Update,
            Rename,
            Builtin,
        }
        #endregion
    }
}
