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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Restore;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    internal class AveDocumentSerializerV1 : AveDocumentSerializer
    {
        public AveDocumentSerializerV1(AveFileCollection fileCollection) : base(fileCollection)
        {
        }

        protected override void PreRestoreDocument(AveDocumentInfo info, IAveRestoreStream receiver, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.PreRestoreDocument"))
            {
                ProcessSolutionStatus(info, aveItem);
                info.IsStubData = IsStubData(receiver, aveItem);
                aveItem.IsWelcomePage = false;
                aveItem.CheckConflictState(info.RestoringItem, info.SiteId, info.ParentId);
                info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion, info.RestoreOption);

                HandleRecycleBinConflict(info);

                HandleTargetTable(info, aveItem);

                HandleDocumentConflict(info, aveItem, allDocData, allUserData);
            }
        }

        private void HandleDocumentConflict(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            if (info.RestoringItem.ConflictWithDocument)
            {
                HandleModifiedTimeComparision(info, aveItem, allDocData, allUserData);

                HandleDocumentConflictByRestoreMode(info);

                if (NeedDeleteFile(info))
                {
                    info.RestoringItem.OverwriteAllVersion = true;
                    DeleteFile(info, aveItem);
                }
            }
        }

        private static void HandleDocumentConflictByRestoreMode(AveDocumentInfo info)
        {
            switch (info.RestoreOption)
            {
                case AveRestoreMode.Default:
                    if (!info.RestoringItem.IsNewItem)
                    {
                        info.RestoringItem.NeedSkipped = true;
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                    break;
            }
        }

        private bool NeedDeleteFile(AveDocumentInfo info)
        {
            if (!info.SettingInfo.DELETE_ITEM)
            {
                info.IsCheckOut = IsCheckOutFile(mSite, info);
                return false;
            }

            if (IsThumbnails())
            {
                //Don't delete thumbnail of Pictrue Library,Image Library 
                return false;
            }
            return true;
        }

        private void DeleteFile(AveDocumentInfo info, AveItem aveItem)
        {
            SPFile tempFile = null;
            try
            {
                tempFile = LoadSPFile(info);
                HandleItemTypeConflict(info, tempFile);

                bool moveSuccess = HandleMoveConflictFileAction(info, aveItem, tempFile);

                if (moveSuccess)
                {
                    return;
                }

                if (info.KeepDestItemRowId && tempFile != null && tempFile.Item != null)
                {
                    info.DestItemRowId = tempFile.Item.ID;
                    info.DestItemUniqueId = tempFile.UniqueId;
                }

                UnlockItem(aveItem, tempFile);
                PreDeleteWelcomePage(aveItem, tempFile);

                DeleteConflictFile(info, aveItem, tempFile, false);
            }
            catch (AveWrapperException)
            {
                throw;
            }
            catch (AveWarningException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.PreRestoreDocError, e);
                if (tempFile != null)
                {
                    ProcessMasterPage(tempFile, info);
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.FileDeletedFailed, tempFile.ServerRelativeUrl, e);
                    if (IsCheckOutFile(mSite, info))
                    {
                        tempFile.CheckIn("");
                        DeleteConflictFile(info, aveItem, tempFile, info.RestoringItem.OverWriteBlob);
                    }
                }
            }
        }

        private void DeleteConflictFile(AveDocumentInfo info,AveItem aveItem, SPFile tempFile, bool overWriteBlob)
        {
            bool hasAlert = aveItem.IsItemHasAlerts(tempFile.Item);

            DeleteConflictFileInternal(info,tempFile,overWriteBlob);

            //ADO-42263:删除file之后，如果这个file上有alert，需要调用下面方法更新SPWeb的Alerts对象
            if (hasAlert)
            {
                AveAssemblyUtility.InvokeMethod(mWeb.Web.Alerts, "SetAlertsDirty", new Type[] { });
            }
        }

        private void DeleteConflictFileInternal(AveDocumentInfo info, SPFile tempFile, bool overWriteBlob)
        {
            if (overWriteBlob)
            {
                RemoveWorkflowInstance(tempFile);
                tempFile.Delete();
            }
            else
            {
                AveItem item = info.AveItem as AveItem;
                bool enableEventReceiver = item != null && item.mList != null && item.mList.IsConnectorList == true;
                using (new AveEventReceiverUtility(enableEventReceiver))
                {
                    RemoveWorkflowInstance(tempFile);
                    tempFile.Delete();
                }
            }
        }

        private static void UnlockItem(AveItem aveItem, SPFile tempFile)
        {
            if (tempFile.ParentFolder != null && tempFile.ParentFolder.ParentListId != Guid.Empty && tempFile.Item != null)
            {
                aveItem.UnLockItem(tempFile.Item);
            }
        }

        private void PreDeleteWelcomePage(AveItem aveItem, SPFile tempFile)
        {
            if (WrapperRuntime.CurrentContext.IsMoss && aveItem.Web.RootFolder.WelcomePage.Equals(tempFile.Url, StringComparison.OrdinalIgnoreCase)
                && AvePublishing.IsPublishingWeb(aveItem.Web))
            {
                //此处使用SetWelcomePage时，有时会导致之后创建子web时抛异常，经过调试通过修改RootFolder.WelcomePage的方式可以避免该问题。
                //AvePublishing.SetWelcomePage(mAveSPFolder.ParentList.ParentWeb.SPWeb, "AveDefault.aspx", false);
                try
                {
                    SPFolder folder = ((AveFolder) aveItem.Web.RootFolder).Folder;
                    folder.WelcomePage = "";
                    folder.Update();
                    aveItem.IsWelcomePage = true;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateRootFolderError, e.ToString());
                }
            }
        }

        private static bool HandleMoveConflictFileAction(AveDocumentInfo info, AveItem aveItem, SPFile tempFile)
        {
            bool moveSuccess = false;
            if (info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER)
            {
                moveSuccess = aveItem.MoveToConflictFolder(aveItem.mSPList, aveItem.mParentFolder, tempFile.Item, true);

            }
            return moveSuccess;
        }

        private void HandleItemTypeConflict(AveDocumentInfo info, SPFile tempFile)
        {
            if (tempFile.Exists)
            {
                return;
            }
            if (tempFile.Item != null && tempFile.Item.Folder != null && tempFile.Item.Folder.Exists)
            {
                throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Restore_DocumentTypeConflict, info.Name, tempFile.Item.Folder.ServerRelativeUrl);
            }
        }

        private bool IsCheckOutFile(AveSite site, AveDocumentInfo info)
        {
           return site.QueryService.IsCheckOutFile(null, info.SiteId, info.ParentId, info.Name);
        }

        private SPFile LoadSPFile(AveDocumentInfo info)
        {
            SPFile file = GetFile(info.Name);
            if (!file.Exists)
            {
                try
                {
                    info.IsCheckOut = IsCheckOutFile(mSite,info);
                    if (info.IsCheckOut)
                    {
                        //只有一个version的checkout文件的case满足这种条件，改成直接用API takeover
                        //aveItem.mSPFile = aveItem.LoadCheckOutFile(mWeb.Web, mFolder.Folder.ServerRelativeUrl, info.Name);
                        (this.mFolder.ParentList as AveDocumentLibrary).TakeOverCheckedOutFile(mFolder.ServerRelativeUrl.TrimEnd('/') + "/" + info.Name);
                    }
                }
                catch (Exception ce)
                {
                    logger.Warn("An error happened while reloading file, name: {1}. Error: {0}", ce.ToString(), info.Name);
                }
            }
            return file;
        }

        private void HandleModifiedTimeComparision(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            object modifiedTime;
            if (allUserData.TryGetValue("Modified", out modifiedTime) && aveItem.SkipIfSameModifiedTime(info, modifiedTime))
            {
                info.RestoringItem.NeedSkipped = true;
                throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, AveRestoreResult.SkipTheSameItem.ToString());
            }
            object level;
            if (allDocData.TryGetValue("BiggestVersionModified", out modifiedTime)
                && allDocData.TryGetValue("Level", out level)
                && !aveItem.OverwriteByModifiedTime(info, modifiedTime, level)) //add for overwrite by modifiedTime
            {
                info.RestoringItem.NeedSkipped = true;
                throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, AveRestoreResult.SkipTheSameItem.ToString());
            }
        }

        private void HandleTargetTable(AveDocumentInfo info,AveItem aveItem)
        {
            if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
            {
                if (info.RestoringItem.ConflictWithDocument)
                {
                    var tempSPFile = LoadSPFile(info);
                    HandleItemTypeConflict(info, tempSPFile);
                    aveItem.InitBySPFile(tempSPFile);
                }
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
        }

        private void HandleRecycleBinConflict(AveBaseItemInfo info)
        {
            if (info.RestoringItem.ConflilctFromRecycleBin)
            {
                //skip, include recycle bin的情况下直接skip掉当前还原的document
                if (info.RestoreOption == AveRestoreMode.Default && info.RestoringItem.IsIncludingRecycleBinData)
                {
                    throw new AveRestoreException(AveRestoreResult.SkipRecycleBinData, string.Empty);
                }
                mSite.QueryService.RemoveItemInRecycleBin(mWeb.Site, info.ParentId, info.Name);
            }
        }

        private static bool IsStubData(IAveRestoreStream receiver, AveItem aveItem)
        {
            aveItem.GetSOIntegrationUtilForRestore(receiver);
            return aveItem.mList == null ? false : aveItem.mList.SOIntegrationUtil.StorageInfo.IsBackupLinkForArchivedData;
        }

        private void ProcessSolutionStatus(AveDocumentInfo info, AveItem aveItem)
        {
            if (info.SettingInfo.IsProcessSolutionStatus)
            {
                if (!Guid.Empty.Equals(info.SolutionId))
                {
                    DeactiveSolution(aveItem, info.SolutionId, ref info.ActivatedWebSolutionFeatureIDs);
                }
            }
        }
    }
}
