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
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
namespace LS.SPWorkflowProcessor
{
    public class SPContentTypeProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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

        private Dictionary<string, string> allContentTypeIdMaping = new Dictionary<string, string>();
        public Dictionary<string, string> AllContentTypeIdMaping
        {
            get { return allContentTypeIdMaping; }
        }


        #region ************************Backup  Region************************
        public List<SPContentTypeUnit> BackupContentTypes(IAveContentTypeCollection cts)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupContentTypes");
            List<SPContentTypeUnit> units = new List<SPContentTypeUnit>();
            foreach (IAveContentType ct in cts)
            {
                try
                {
                    logger.Debug("Begin to backup ContentType: {0}, {1}.", ct.Name, ct.ID);
                    SPContentTypeUnit unit = GenerateInheritanceTree(ct);
                    if (unit != null)
                    {
                        units.Add(unit); 
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while backup BackupContentTypes, Error: {0}.", e);
                }
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
            SPContentTypeUnit unit = null;
            try
            {
                if (currentCT == null)
                {
                    logger.Warn("currentCT is null, do not backup it.");
                    return unit;
                }
                unit = new SPContentTypeUnit(currentCT);
                ResetLevelAndIndex(currentCT.ParentWeb.ID, unit);
                if (AveBuiltInContentTypeId.Contains(currentCT.ID))
                    unit.mParentUnit = null;
                else
                    unit.mParentUnit = InheritanceTreeNode(currentCT.Parent);
                logger.Debug("Backup ContentType for workflow task, ContentTypeInfo: {0}, {1}, {2}.", currentCT.Name, currentCT.ID, currentCT.Scope);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while backup contentType, Error: {0}.", e);
            }
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
            try
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
                    if (leafUnit == null)
                    {
                        continue;
                    }
                    try
                    {
                        List<SPContentTypeUnit> allUnits = leafUnit.GetAllUnits(true);
                        foreach (SPContentTypeUnit curUnit in allUnits)
                        {
                            if (curUnit == null)
                            {
                                continue;
                            }
                            SetParentCTCollection(parentList, curUnit);
                            RestoreContentTypeUnit(curUnit);
                        }
                        if (leafUnit.SerializableData.mId != leafUnit.SerializableData.mNewId)
                            ContentTypeIdMaping.Add(leafUnit.SerializableData.mId.ToUpperEx(2, leafUnit.SerializableData.mId.Length - 2), leafUnit.SerializableData.mNewId.ToUpperEx(2, leafUnit.SerializableData.mNewId.Length - 2));
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.CT_RestoreUnknownException, leafUnit.SerializableData.mId, e.Message);
                        logger.Warn("An exception occurred while restore list content types. exception:{0}", e.ToString());
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreListContentTypes");
                        //throw new SPWFProcessorException(SPWFProcessorErrorCode.ContentTypeRestoreError, e, leafUnit.SerializableData.mId);
                    }
                }
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreListContentTypes");
            }
            catch (Exception e)
            {
                logger.Warn("An exception occurred while restore all list content types. exception: {0}.", e);
            }
        }

        private void SetParentCTCollection(IAveList parentList, SPContentTypeUnit unit)
        {
            if (unit.SerializableData.mParentScope == SPContentTypeScope.List)
                unit.mSPContentTypeCollection = parentList.ContentTypes;
            else if (unit.SerializableData.mParentScope == SPContentTypeScope.Web)
            {
                //parentList.ParentWeb不应该Dispose，但是它的parentWeb应该dispose
                bool isFirstWeb = true;
                IAveWeb parentWeb = parentList.ParentWeb;
                for (int i = 0; i < unit.SerializableData.mLevel; i++)
                {
                    if (parentWeb.ParentWeb != null)
                    {
                        if (!isFirstWeb)
                        {
                            parentWeb.Dispose();
                            isFirstWeb = false;
                        }
                        parentWeb = parentWeb.ParentWeb;
                    }
                }
                unit.mSPContentTypeCollection = parentWeb.ContentTypes;
                if (!isFirstWeb)
                {
                    parentWeb.Dispose();
                }

                IAveContentTypeId srcId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
                if (AveBuiltInContentTypeId.Contains(srcId))
                    return;

                IAveContentType dstCT = unit.mSPContentTypeCollection[unit.SerializableData.mName];
                if (dstCT == null)
                {
                    while (parentWeb.ParentWeb != null)
                    {
                        var tempWeb = parentWeb.ParentWeb;
                        if (!isFirstWeb)
                        {
                            parentWeb.Dispose();
                        }
                        dstCT = tempWeb.ContentTypes[unit.SerializableData.mName];
                        if (dstCT != null)
                        {
                            unit.mSPContentTypeCollection = tempWeb.ContentTypes;
                            //肯定不是ParentList.ParentWeb,所以可以直接dispose
                            tempWeb.Dispose();
                            break;
                        }
                        parentWeb = tempWeb;
                    }
                }
            }
        }

        private void RestoreContentTypeUnit(SPContentTypeUnit unit)
        {
            CTConflictAction conflictAction = HandleContentTypeConflict(unit);
            IAveContentType ct = null;
            var srcContentTypeId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
            switch (conflictAction)
            {
                case CTConflictAction.CreateNew:
                case CTConflictAction.Rename:
                    IAveContentType parentCT = unit.mParentUnit.mSPContentTypeCollection[unit.mParentUnit.SerializableData.mName];
                    if (parentCT == null)
                    {
                        parentCT = unit.mParentUnit.mSPContentTypeCollection.Web.AvailableContentTypes[unit.mParentUnit.SerializableData.mName];
                    }
                    //尽量keep contenTypeId
                    try
                    {
                        ct = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentType(srcContentTypeId, unit.mSPContentTypeCollection, unit.SerializableData.mName);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while creating  contentType by id {0},{1},Error:{2}", unit.SerializableData.mName, unit.SerializableData.mId, e);
                        ct = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentType(parentCT, unit.mSPContentTypeCollection, unit.SerializableData.mName);
                    }
                    //client API中只有调Add后才会真正添加ContentType，所以需要先调Add,再取contentType上的属性，否则会出错
                    ct=unit.mSPContentTypeCollection.Add(ct);
                    //var srcContentTypeId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
                    if (ct.ID != srcContentTypeId)
                    {
                        AddContentTypeToMapping(srcContentTypeId,ct.ID);
                        SPWorkflowProcessorRuntime.MappingManager.WebMappingManager.AddWebLevelCTIdMapping(unit.SerializableData.mId, ct.ID, false);
                    }
                    string temp = ct.Name;
                    ct = null;
                    ct = unit.mSPContentTypeCollection[temp];
                    break;
                case CTConflictAction.Update:
                    ct = unit.mSPContentTypeCollection[unit.SerializableData.mName];
                    //var srcContentTypeId = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateContentTypeId(unit.SerializableData.mId);
                    if (ct.ID != srcContentTypeId)
                    {
                        AddContentTypeToMapping(srcContentTypeId, ct.ID);
                        SPWorkflowProcessorRuntime.MappingManager.WebMappingManager.AddWebLevelCTIdMapping(unit.SerializableData.mId, ct.ID, false);
                    }
                    break;
                case CTConflictAction.Builtin:
                    return;
                default:
                    throw new Exception("Not supported");
            }

            unit.SetSPObjectPropByUnit(ct);
            unit.SerializableData.mNewId = ct.ID.ToString();
        }

        private void AddContentTypeToMapping(IAveContentTypeId src, IAveContentTypeId dest)
        {
            const string formatStr = "\"{0}\"";
            if (src != dest)
            {
                var sourceId = string.Format(formatStr, src);
                var destId = string.Format(formatStr, dest);
                AllContentTypeIdMaping.AddEx(sourceId, destId);
                logger.Debug("WorkflowTaskContentTypeMapping:{0},{1}",sourceId,destId);
            }
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

            if (sId.Equals(dId) || sId.IsChildOf(dId))
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
