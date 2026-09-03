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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Publishing;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Report;

namespace AvePoint.ObjectModel.Server13
{
    class AveDocumentSerializer : IAveDocumentSerializer, IDisposable
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveDocumentSerializer));

        protected AveFileCollection mFileCollection;
        protected AveFolder mFolder;
        protected AveWeb mWeb;
        protected AveSite mSite;
        protected IReport mReport;

        internal IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }

        public AveDocumentSerializer(AveFileCollection fileCollection)
        {
            mFileCollection = fileCollection;
            mFolder = mFileCollection.Folder as AveFolder;
            mWeb = mFileCollection.Web as AveWeb;
            mSite = mWeb.Site as AveSite;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public AveRestoreResult SetObjectData(AveDocumentInfo info, IAveRestoreStream receiver, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<IAveListItem> holdItems, System.Collections.Hashtable HTMetaInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.SetObjectData"))
            {

                AveItem aveItem = info.AveItem as AveItem;
                AveRestoreResult result = AveRestoreResult.Normal;

                List<SPListItem> spholdItems = new List<SPListItem>();
                if (holdItems != null)
                {
                    foreach (IAveListItem item in holdItems)
                    {
                        spholdItems.Add((item as AveListItem).ListItem);
                    }
                }

                #region If the document is View
                if (info.IsView)
                {
                    RestoreView(info, aveItem, allDocData, receiver);
                }
                #endregion

                RestoreGhostPage(info, aveItem, allDocData, receiver);

                PreRestoreDocument(info, receiver, aveItem, allDocData, allUserData);
                result = RealRestoreDocument(info, aveItem, receiver, allDocData, allUserData, spholdItems, HTMetaInfo);
                PostRestoreDocument(info, aveItem, allUserData);

                return result;

            }


        }

        private void RestoreView(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allDocData, IAveRestoreStream receiver)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.RestoreView"))
            {

                AveView view = new AveView(aveItem.mList);
                string sourceFileName = null;
                foreach (AveViewInfo viewInfo in info.AveView.Vinfos)
                {

                    if (viewInfo.IsPersonal && viewInfo.UserID <= 0)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperReportResource.Wrapper_Report_RestorePersonalViewError, viewInfo.Title, viewInfo.UserID);
                        mReport.AddDetail(new AveWrapperReportDto(viewInfo.Title, viewInfo.Title, AveReportObjectType.PersonalView, AveStatus.Skipped, AveReportResource.Wrapper_Report_RestorePersonalViewError, viewInfo.Title, viewInfo.UserID));
                        info.AveView.Views[viewInfo.Id] = Guid.Empty;
                        continue;
                    }
                    SPView destView = null;
                    try
                    {
                        //bool isNewCreatedView = false;
                        destView = view.RestoreView(info, viewInfo.Title, viewInfo.LeafName, viewInfo.UserID, viewInfo.IsPersonal, (int)viewInfo.ViewType, viewInfo.Id, viewInfo.IsDefaultView, viewInfo.IsMobileView, viewInfo.IsDefaultMobileView, viewInfo.Hidden);
                        //info.IsNewCreatedView = isNewCreatedView;
                    }
                    catch (AveUnauthorizedAccessException e)
                    {
                        logger.Log(AveLogLevel.WARN, "Permission issue occurred while restoring view. View LeafName: {0}. User: {1}", viewInfo.LeafName, e.UserLoginName);
                        mReport.AddDetail(new AveWrapperReportDto(viewInfo.Title, info.Name, AveReportObjectType.PersonalView, AveStatus.Failed, AveReportResource.Wrapper_Report_UserPermissionNotEnough, e.UserLoginName, aveItem.mList.ParentWeb.Url));
                        info.AveView.Views[viewInfo.Id] = Guid.Empty;
                        continue;
                    }
                    if (viewInfo.Title == "RssView")
                    {
                        if (aveItem.mList != null)
                        {
                            info.AveView.RestoreRssView = true;
                        }
                    }
                    info.MappingManager.SiteMappingManager.AddViewGuidMapping(viewInfo.Id, destView.ID);
                    if (!viewInfo.IsPersonal)
                    {
                        sourceFileName = viewInfo.LeafName;
                    }
                }
                if (!string.IsNullOrEmpty(info.AveView.ViewUrl))
                {
                    aveItem.mSPFile = mWeb.Web.GetFile(info.AveView.ViewUrl);
                    #region If the document is new created view
                    if (info.IsNewCreated || info.IsOverWrite)
                    {
                        aveItem.RestoreWebPart(info);
                        if (sourceFileName != null && !sourceFileName.Equals(aveItem.mSPFile.Name))
                        {
                            string newUrl = aveItem.mSPFile.ParentFolder.Url + "/" + sourceFileName;
                            try
                            {
                                aveItem.mSPFile.MoveTo(newUrl);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("Rename the view file failed, old URL: {0}, new URL: {1}, error: {2}", aveItem.mSPFile.Url, newUrl, ex.ToString());
                            }
                        }
                        if (info.HasStream)
                        {
                            try
                            {
                                Stream stream = new AveSPFileStream(receiver);
                                aveItem.mSPFile.SaveBinary(stream);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SaveFileDataError, e.ToString());
                            }
                        }
                        DateTime timeLastModified = allDocData.ContainsKey("TimeLastModified") ? (DateTime)allDocData["TimeLastModified"] : DateTime.MinValue;
                        if (timeLastModified != DateTime.MinValue)
                        {
                            //mSite.QueryService.UpdateViewLastModifiedTimeByNative(info, aveItem.File, timeLastModified);                            
                            info.GUID = aveItem.mSPFile.UniqueId;
                            info.Level = (int)aveItem.mSPFile.Level;
                            aveItem.SetDocData("TimeLastModified", timeLastModified);
                        }
                        //还原view完毕，不需要进行reload
                        aveItem.UpdateDataByNative(true, false, false);
                        //还原view完毕，不需要skip，返回normal类型的result，否则会返回omit类型的result
                        throw new AveRestoreException(AveRestoreResult.Normal, AveRestoreResult.Normal.ToString());
                    }
                    #endregion
                    info.RestoringItem.NeedSkipped = true;
                }
                else
                {
                    info.RestoringItem.NeedSkipped = true;
                }
                //return AveRestoreResult.Omit;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());

            }

        }


        private void RestoreGhostPage(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allDocData, IAveRestoreStream receiver)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.RestoreGhostPage"))
            {

                #region If the document is ghost page
                if (info.IsGhostPage && (info.OriginalRowId < 0 || aveItem.mSPList == null ||
                    aveItem.mSPList != null &&
                    (aveItem.mSPList.BaseTemplate == SPListTemplateType.ListTemplateCatalog
                    || aveItem.mSPList.BaseTemplate == SPListTemplateType.WebTemplateCatalog
                    || aveItem.mSPList.BaseTemplate == SPListTemplateType.SolutionCatalog
                    || aveItem.mSPList.BaseTemplate == SPListTemplateType.ThemeCatalog
                    || aveItem.mSPList.BaseTemplate == SPListTemplateType.WebPartCatalog
                    || aveItem.mSPList.BaseTemplate == SPListTemplateType.MasterPageCatalog)))
                {
                    aveItem.mSPFile = aveItem.AddGhostedPage(info.Name, info.SetupPath, receiver, false, info.GhostPageOption, info);
                    if (info.OriginalVersion == aveItem.mSPFile.UIVersion)
                    {
                        if (allDocData.ContainsKey("MetaInfo"))
                        {
                            byte[] bts = (byte[])allDocData["MetaInfo"];
                            aveItem.RestoreMetaInfo(aveItem.mSPFile, bts);
                        }
                        //mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.OriginalVersion);


                        aveItem.UpdateAlldocsPropertiesByNative(info, aveItem);


                        #region replace the content ---Has been deleted by Austin. This piece of code has been moved to the front of adding file using SharePoint API---
                        //if (info.HasStream
                        //    && (info.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                        //    || info.Name.EndsWith(".master", StringComparison.OrdinalIgnoreCase)))
                        //{
                        //    if (ChangeContent(aveItem.mSite, aveItem.File, info))
                        //    {
                        //        aveItem.ReloadFile();
                        //    }
                        //}
                        #endregion
                        try
                        {
                            if (aveItem.mSPList != null && aveItem.mSPFile.Item != null)
                            {
                                if (aveItem.mSPFile.UIVersion % 512 > 0 && aveItem.mSPList.EnableMinorVersions == false)
                                {
                                    aveItem.mSPList.EnableMinorVersions = true;
                                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                                    aveItem.mSPList.Update();
                                }
                                aveItem.InitBySPFile(aveItem.mSPFile);
                                aveItem.SetReport(Report);
                                aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                            }
                            else
                            {
                                aveItem.RestoreWebPart(info);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateFieldsError, e.ToString());
                        }
                        if (info.IsOrignialCheckOut && info.CheckoutUserId > 0)
                        {
                            mSite.QueryService.ChangeCheckoutUserIDForAllVersion(info.SiteId, info.ParentId, aveItem.mSPFile.UniqueId, info.CheckoutUserId);
                        }
                        //TODO 是否需要重新reload Item
                        aveItem.UpdateDataByNative(true, true);

                        throw new AveRestoreException(AveRestoreResult.Normal, AveRestoreResult.Omit.ToString());
                    }
                }
                #endregion

            }

        }

        protected virtual void PreRestoreDocument(AveDocumentInfo info, IAveRestoreStream receiver, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            PreRestoreDocumentWithConflictType(info,receiver,aveItem,allDocData,allUserData);
        }


        /// <summary>
        /// Do some checking&action by Setting
        /// </summary>
        /// <param name="info"></param>
        /// <param name="aveItem"></param>
        /// <param name="allDocData"></param>
        /// <returns></returns>
        private void PreRestoreDocumentWithConflictType(AveDocumentInfo info, IAveRestoreStream receiver, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.PreRestoreDocument"))
            {

                info.IsThumbnails = IsThumbnails();
                aveItem.IsWelcomePage = false;
                aveItem.CheckConflictState(info.RestoringItem, info.SiteId, info.ParentId);
                if (info.SettingInfo.IsProcessSolutionStatus)
                {
                    if (!Guid.Empty.Equals(info.SolutionId))
                    {
                        DeactiveSolution(aveItem, info.SolutionId, ref info.ActivatedWebSolutionFeatureIDs);
                    }
                }
                aveItem.GetSOIntegrationUtilForRestore(receiver);
                info.IsStubData = aveItem.mList == null ? false : aveItem.mList.SOIntegrationUtil.StorageInfo.IsBackupLinkForArchivedData;
                info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion);
                int version = -1;
                switch (info.RestoringItem.ConflictType)
                {
                    case ConflictType.None:
                        {

                            break;
                        }
                    case ConflictType.Document:
                        {
                            if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
                            {
                                HandleOneVersionCheckOutFile(info, aveItem);
                            }
                            aveItem.mSPFile = GetFile(info.Name);
                            if (aveItem.mSPFile != null)
                            {
                                version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, aveItem.mSPFile.UniqueId);
                            }
                            if (info.RestoreOption == AveRestoreMode.Default && !info.RestoringItem.IsNewItem)
                            {
                                info.RestoringItem.NeedSkipped = true;
                                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                            }
                            //现在的问题是RowId冲突，但是Version不冲突这时应该继续还原
                            if (info.OriginalVersion != version && !info.RestoringItem.IsNewItem)
                            {
                                HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                                break;
                            }
                            else if (info.RestoreOption == AveRestoreMode.OverWrite)
                            {
                                HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                            }
                            else if (info.RestoreOption == AveRestoreMode.OverWriteByModifiedTime)
                            {
                                if (allDocData.ContainsKey("BiggestVersionModified") && allDocData.ContainsKey("Level") && !aveItem.OverwriteByModifiedTime(info, allDocData["BiggestVersionModified"], allDocData["Level"]))
                                {
                                    info.RestoringItem.NeedSkipped = true;
                                    //return AveRestoreResult.Omit;
                                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                }
                                HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                            }
                            else if (info.RestoreOption == AveRestoreMode.AppendANewVersion)
                            {

                                HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                                //to do 现在在外围，需要移进来，要考虑如何把新的Item Name返回给外围 1_.000:1024
                            }
                            else if (info.RestoreOption == AveRestoreMode.Append)
                            {
                                HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                                //info.RestoringItem.NeedSkipped = true;
                                //throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString()); //to do 异常类型
                            }
                            break;
                        }
                    case ConflictType.RecycleBin:
                        {
                            if (info.RestoringItem.IsIncludingRecycleBinData)
                            {
                                if (info.RestoreOption == AveRestoreMode.Default)
                                {
                                    info.RestoringItem.NeedSkipped = true;
                                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                }
                            }

                            mSite.QueryService.RemoveItemInRecycleBin(mWeb.Site, info.ParentId, info.Name);
                            break;
                        }
                    case ConflictType.Both:
                        {
                            mSite.QueryService.RemoveItemInRecycleBin(mWeb.Site, info.ParentId, info.Name);
                            HandleConflictWithDocument(info, aveItem, allDocData, allUserData);
                            break;
                        }
                    default:
                        {
                            info.RestoringItem.NeedSkipped = true;
                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());//to do 异常类型
                        }
                }

            }

        }
        private void HandleOneVersionCheckOutFile(AveDocumentInfo info, AveItem aveItem)
        {
            aveItem.mSPFile = GetFile(info.Name);
            if (!aveItem.mSPFile.Exists)
            {
                try
                {
                    info.IsCheckOut = mSite.QueryService.IsCheckOutFile(null, info.SiteId, info.ParentId, info.Name);
                    if (info.IsCheckOut)
                    {
                        //只有一个version的checkout文件的case满足这种条件，改成直接用API takeover
                        //aveItem.mSPFile = aveItem.LoadCheckOutFile(mWeb.Web, mFolder.Folder.ServerRelativeUrl, info.Name);
                        (this.mFolder.ParentList as AveDocumentLibrary).TakeOverCheckedOutFile(mFolder.ServerRelativeUrl.TrimEnd('/') + "/" + info.Name);
                    }
                    else if (aveItem.mSPFile.Item != null && aveItem.mSPFile.Item.FileSystemObjectType == SPFileSystemObjectType.Folder)
                    {
                        //ADO-169631 源端与目的端file和folder冲突，content选择skip时
                        logger.Info("There is folder in the destination that has the same name as the document in the source, name: {0}.", info.Name);
                        throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Restore_DocumentTypeConflict, info.Name, aveItem.mSPFile.Item.Folder.ServerRelativeUrl);
                    }
                }
                catch (AveWrapperException)
                {
                    throw;
                }
                catch (Exception ce)
                {
                    logger.Warn("An error happened while reloading file, name: {1}. Error: {0}", ce.ToString(), info.Name);
                }

            }
            aveItem.InitBySPFile(aveItem.mSPFile);
        }
        private void HandleConflictWithDocument(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            CheckModifiedTimeOfConflictFile(info, aveItem, allUserData);
            if (info.SettingInfo.DELETE_ITEM)
            {
                SPFile tempFile = null;
                try
                {
                    tempFile = GetFile(info.Name);
                    if (!tempFile.Exists)
                    {
                        AveDocumentInfo destItemInfo = new AveDocumentInfo();
                        destItemInfo.IsCheckOut = mSite.QueryService.IsCheckOutFile(destItemInfo, mWeb.Site.ID, mFolder.Folder.UniqueId, info.Name);
                        if (destItemInfo.IsCheckOut && destItemInfo.CheckoutUserId != mWeb.Web.CurrentUser.ID)
                        {
                            //tempFile = mSite.GetCheckoutWeb(mSite.ID, mWeb.Web, mWeb.Web.SiteUsers.GetByID(destItemInfo.CheckoutUserId), destItemInfo.CheckOutFileUniqueID).GetFile(destItemInfo.CheckOutFileUniqueID);
                            //只有一个version的checkout文件的case满足这种条件，改成直接用API takeover                                    
                            (this.mFolder.ParentList as AveDocumentLibrary).TakeOverCheckedOutFile(mFolder.ServerRelativeUrl.TrimEnd('/') + "/" + info.Name);
                        }
                        else if (tempFile.Item != null && tempFile.Item.Folder != null && tempFile.Item.Folder.Exists)
                        {
                            throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Restore_DocumentTypeConflict, info.Name, tempFile.Item.Folder.ServerRelativeUrl);
                            //throw new ItemTypeConflictException(info.OriginalRowId, tempFile.Item.Folder.ServerRelativeUrl);
                        }
                    }
                    if (!info.IsThumbnails)//Don't delete thumbnail of Pictrue Library,Image Library 
                    {
                        //wikiPage应该走删除路线
                        //if (info.HasStream || (mFolder.ParentList != null && mFolder.ParentList.BaseTemplate == AveListTemplateType.WebPageLibrary))
                        //{
                        if (tempFile != null && aveItem.mParentFolder.ParentListId != Guid.Empty && !AveSPServerUtility.IsOrInSystemFormsFolder(aveItem.mParentFolder) && !AveSPServerUtility.IsFormsFile(tempFile) && !(aveItem.mList).IsReportTemplateList())
                        {
                            info.RestoringItem.OverwriteAllVersion = true;
                            info.OldDocId = tempFile.UniqueId;
                            bool movedSuccess = false;
                            if (info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER)
                            {
                                movedSuccess = aveItem.MoveToConflictFolder(aveItem.mSPList, aveItem.mParentFolder, tempFile.Item, true);
                            }
                            if (!movedSuccess)
                            {
                                if (tempFile.Item != null)
                                {
                                    aveItem.UnLockItem(tempFile.Item);
                                }
                                if (WrapperRuntime.CurrentContext.IsMoss && aveItem.Web.RootFolder.WelcomePage.Equals(tempFile.Url, StringComparison.OrdinalIgnoreCase)
                                    && AvePublishing.IsPublishingWeb(aveItem.Web))
                                {
                                    //此处使用SetWelcomePage时，有时会导致之后创建子web时抛异常，经过调试通过修改RootFolder.WelcomePage的方式可以避免该问题。
                                    //AvePublishing.SetWelcomePage(mAveSPFolder.ParentList.ParentWeb.SPWeb, "AveDefault.aspx", false);
                                    try
                                    {
                                        SPFolder folder = ((AveFolder)aveItem.Web.RootFolder).Folder;
                                        folder.WelcomePage = "";
                                        folder.Update();
                                        aveItem.IsWelcomePage = true;
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateRootFolderError, e.ToString());
                                    }
                                }

                                bool needSetAlertsDirty = aveItem.IsItemHasAlerts(tempFile.Item);

                                if (info.KeepDestItemRowId)
                                {
                                    info.DestItemRowId = tempFile.Item.ID;
                                    info.DestItemUniqueId = tempFile.UniqueId;
                                }
                                if (info.RestoringItem.OverWriteBlob)
                                {
                                    RemoveWorkflowInstance(tempFile);
                                    tempFile.Delete();
                                }
                                else
                                {
                                    using (new AveEventReceiverUtility((info.AveItem as AveItem).mList != null && (info.AveItem as AveItem).mList.IsConnectorList == true))
                                    {                                  
                                        RemoveWorkflowInstance(tempFile);
                                        tempFile.Delete();
                                    }
                                }

                                //ADO-42263:删除file之后，如果这个file上有alert，需要调用下面方法更新SPWeb的Alerts对象
                                if (needSetAlertsDirty)
                                {
                                    AveAssemblyUtility.InvokeMethod(mWeb.Web.Alerts, "SetAlertsDirty", new Type[] { }, new object[] { });
                                }
                            }
                        }
                    }
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
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.PreRestoreDocError, e.ToString());
                    try
                    {
                        ProcessMasterPage(tempFile, info);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, "An exception occurred while doing process MasterPage. Exception: {0}", ex.ToString());
                    }
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.FileDeletedFailed, tempFile.ServerRelativeUrl, e.ToString());
                    try
                    {
                        if (mSite.QueryService.IsCheckOutFile(null, info.SiteId, info.ParentId, info.Name))
                        {
                            // if the document is checking out by another user,then it can not be delete,we must change checkout userID temporary 
                            //if (tempFile == null || !tempFile.Exists)
                            //{
                            //    tempFile = aveItem.LoadCheckOutFileByNative(mWeb.Web, aveItem.mParentFolder.ServerRelativeUrl, info.Name);
                            //}
                            //mSite.QueryService.ChangeCheckoutUserIDForAllVersion(info, tempFile.UniqueId, tempFile.Web.CurrentUser.ID);

                            tempFile.CheckIn("");

                            bool needSetAlertsDirty = aveItem.IsItemHasAlerts(tempFile.Item);
                            using (new AveEventReceiverUtility((info.AveItem as AveItem).mList != null && (info.AveItem as AveItem).mList.IsConnectorList == true))
                            {
                                RemoveWorkflowInstance(tempFile);
                                tempFile.Delete();
                            }
                            //ADO-42263:删除file之后，如果这个file上有alert，需要调用下面方法更新SPWeb的Alerts对象
                            if (needSetAlertsDirty)
                            {
                                AveAssemblyUtility.InvokeMethod(mWeb.Web.Alerts, "SetAlertsDirty", new Type[] { }, new object[] { });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.FileDeletedFailed, tempFile.ServerRelativeUrl, ex.ToString());
                        info.IsCheckOut = mSite.QueryService.IsCheckOutFile(null, info.SiteId, info.ParentId, info.Name);
                    }
                }
            }
            else
            {
                info.IsCheckOut = mSite.QueryService.IsCheckOutFile(null, info.SiteId, info.ParentId, info.Name);
            }
        }

        protected void ProcessMasterPage(SPFile tempFile, AveDocumentInfo info)
        {
            try
            {

                if (tempFile.ServerRelativeUrl.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
                {
                    string tempFileUrl = tempFile.ServerRelativeUrl;
                    string tempNewFileUrl = tempFileUrl.Replace(".master", "") + "_temp.master";
                    bool isMasterpage = false;
                    foreach (IAveWeb web in mWeb.Site.AllWebs)
                    {
                        PublishingWeb pWeb = null;
                        if (AvePoint.Common.AveEnv.IsPublishing)
                        {
                            pWeb = PublishingWeb.GetPublishingWeb((web as AveWeb).Web);
                        }
                        if (NeedBackUpMasterPageSetting(web, pWeb, tempFileUrl))
                        {
                            AveExtendMasterPageInfo masterSetting = new AveExtendMasterPageInfo();
                            BackUpMasterPageSetting(web, pWeb, masterSetting);
                            masterSetting.CurrentWebId = web.ID;
                            isMasterpage = true;
                            if (info.TempMasterSettings == null)
                            {
                                info.TempMasterSettings = new List<AveExtendMasterPageInfo>();
                            }
                            info.TempMasterSettings.Add(masterSetting);
                            info.TempFileUrl = tempNewFileUrl;
                        }
                        web.Dispose();
                    }
                    if (isMasterpage && !string.IsNullOrEmpty(tempNewFileUrl))
                    {
                        try
                        {
                            tempFile.MoveTo(tempNewFileUrl);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, "Can not move the master page to another file. Exception: {0}", ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while do process masterpage. exception:{0}", ex.ToString());
            }
        }

        private void CheckModifiedTimeOfConflictFile(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allUserData)
        {
            try
            {
                if (allUserData.ContainsKey("Modified") && aveItem.SkipIfSameModifiedTime(info, allUserData["Modified"]))
                {
                    info.RestoringItem.NeedSkipped = true;
                    throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, AveRestoreResult.SkipTheSameItem.ToString());
                }
                //ADO-101847:因为RealrestoreDocument中对于还原历史Version统一抛Failed。所以Skip冲突选项下，在这将历史Version Skip掉。
                if (!info.RestoringItem.OverWrite && (info.OriginalVersion <= info.RestoringItem.PublishingUIVersion || info.OriginalVersion <= info.RestoringItem.DraftUIVersion))
                {
                    info.RestoringItem.NeedSkipped = true;
                    throw new AveRestoreException(AveRestoreResult.Omit, "This version was skipped according to the content level conflict resolution of current job");
                }
            }
            catch (AveRestoreException)
            {
                throw;
            }
        }

        private bool NeedBackUpMasterPageSetting(IAveWeb web, PublishingWeb pWeb, string tempFileUrl)
        {
            bool webSettings = (string.Equals(tempFileUrl, web.MasterUrl, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(tempFileUrl, web.CustomMasterUrl, StringComparison.OrdinalIgnoreCase));

            bool pWebSettings = false;
            if (pWeb != null)
            {
                pWebSettings = (string.Equals(tempFileUrl, pWeb.CustomMasterUrl.Value, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(tempFileUrl, pWeb.MasterUrl.Value, StringComparison.OrdinalIgnoreCase));
            }

            return webSettings || pWebSettings;
        }

        private void BackUpMasterPageSetting(IAveWeb web, PublishingWeb pWeb, AveWebMasterPageInfo masterSetting)
        {
            masterSetting.CPageUrl = web.CustomMasterUrl;
            masterSetting.MPageUrl = web.MasterUrl;
            if (pWeb != null)
            {
                masterSetting.CInheriting = pWeb.CustomMasterUrl.IsInheriting;
                masterSetting.CPageUrl = pWeb.CustomMasterUrl.Value;
                masterSetting.MInheriting = pWeb.MasterUrl.IsInheriting;
                masterSetting.MPageUrl = pWeb.MasterUrl.Value;
            }
        }

        /// <summary>
        /// Restore Document Info, including version/field values/meta info,ect.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="aveItem"></param>
        /// <param name="receiver"></param>
        /// <param name="allDocData"></param>
        /// <param name="allUserData"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "dotx")]
        private AveRestoreResult RealRestoreDocument(AveDocumentInfo info, AveItem aveItem, IAveRestoreStream receiver, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<SPListItem> holdItems, Hashtable HTMetaInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.RealRestoreDocument"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                int intResult = 2;
                if (info.IsThumbnails && info.SettingInfo.MIG_STUB_PIC_THUMBNAILS)
                {
                    aveItem.RestoreMigStubThumbnails(receiver, (mWeb as AveWeb).Web, (mFolder as AveFolder).Folder, info.Name, info.IsOrignialCheckOut);
                    return AveRestoreResult.Normal;
                }
                int currentFileVersion = -1;

                intResult = aveItem.CreateANewFileOrVersion(receiver, mWeb.Web,
                                                aveItem.mSPList,
                                                mFolder.Folder,
                                                info.Name,
                                                info.OriginalVersion,
                                                info.IsOrignialCheckOut,
                                                info.CheckinComment,
                                                info.RestoringItem,
                                                info.IsGhostPage,
                                                info.SetupPath,
                                                holdItems,
                                                HTMetaInfo,
                                                info);
                aveItem.mAveItemRestoreResult = intResult;
                // add the out ref paramter filecurrentversion is for stub restore, if the file is new created the value will be -1 and if the file
                // is already exist then the value will be the currentversion 
                if (!info.IsNewCreated)
                {
                    currentFileVersion = aveItem.mSPFile.UIVersion;
                }

                if (aveItem.IsWelcomePage)
                {
                    try
                    {
                        IAveFolder folder = aveItem.Web.RootFolder;
                        folder.WelcomePage = aveItem.File.Url;
                        folder.Update();
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateRootFolderError, e.ToString());
                    }
                }
                if (info.RestoringItem.Skip(intResult) && aveItem.mList != null && aveItem.mList.BaseTemplate != AveListTemplateType.WebPageLibrary && !info.IsThumbnails)
                {
                    //return AveRestoreResult.Omit;
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }
                //mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.OriginalVersion);

                aveItem.UpdateAlldocsPropertiesByNative(info, aveItem);

                #region replace the content  ---Has been deleted by Austin. This piece of code has been moved to the front of adding file using SharePoint API---
                //if (info.HasStream)// && (info.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) || info.Name.EndsWith(".master", StringComparison.OrdinalIgnoreCase)))
                //{
                //    if (ChangeContent(aveItem.mSite, aveItem.File, info))
                //    {
                //        aveItem.ReloadFile();
                //    }
                //}
                #endregion

                if (!aveItem.mSPFile.InDocumentLibrary || aveItem.mSPFile.Item == null)
                {
                    aveItem.RestoreWebPart(info);
                    if (allDocData.ContainsKey("MetaInfo"))
                    {
                        byte[] bts = (byte[])allDocData["MetaInfo"];
                        aveItem.RestoreMetaInfo(aveItem.mSPFile, bts);
                    }
                    //真正还原之前，会过滤checkout version，此处不需要添加判断
                    if (info.IsOrignialCheckOut && info.CheckoutUserId > 0)
                    {
                        mSite.QueryService.ChangeCheckoutUserIDForAllVersion(info.SiteId, info.ParentId, aveItem.mSPFile.UniqueId, info.CheckoutUserId);
                    }
                    aveItem.UpdateDataByNative(true, false, false);
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

                //if (aveItem.mSPFile.Item.ID != info.DestItemRowId && aveItem.mSPFile.UniqueId != info.DestItemUniqueId)
                //{
                //    ChangeItemId(info, aveItem);
                //}

                if (intResult == 0)
                {
                    //originalVersion < destVersion              
                    //用数据库增加version，有些field需要添加进来
                    //This is not supported for now in SharePoint 2013
                    //UpdateANativeCreatedFileOrVersion(info, aveItem, receiver, allDocData, allUserData);

                    //暂时处理成skip exception ，以后考虑是否修改为AveWrapperNotSupportedException， 10,13目前逻辑一致，需要统一抛出的exception
                    throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_SingleDocumentVersionNotSupported);
                }
                else if (intResult == 1 || intResult == 2)
                {
                    //originalVersion >= destVersion   
                    UpdateANewCreatedFileOrVersion(info, aveItem, receiver, allDocData, currentFileVersion, intResult);
                }
                if (info.IsStubData)
                {
                    //    aveItem.RestoreStubDBInfo();
                    aveItem.RestoreConnectorStub(aveItem.mSPListItem.UniqueId, info.OriginalVersion, intResult);
                }
                aveItem.RestoreItemConnectorInfo(receiver,info, info.OriginalVersion, intResult);
                return result;

            }

        }

        private void UpdateANewCreatedFileOrVersion(AveDocumentInfo info, AveItem aveItem, IAveRestoreStream receiver, Dictionary<string, object> allDocData, int currentFileVersion, int result)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.UpdateANewCreatedFileOrVersion"))
            {

                info.IsOrignialCheckOut = (aveItem.mSPFile.CheckOutType != SPFile.SPCheckOutType.None);

                if (result == 2 && info.HasStream && info.InternalVersion > 0)
                {
                    if (info.SettingInfo.DESTSTUB_CONTENT)
                    {
                        //convert dest stub docflag to content
                        //mSite.QueryService.SetDocFlagAsContent(info, info.OriginalVersion);
                    }
                    aveItem.RestoreContentByNative(receiver);
                    aveItem.ReloadFile();
                    if (aveItem.mSPList != null)
                    {
                        aveItem.ListItem = mFolder.ParentList.GetItemById(aveItem.ListItem.ID);
                    }
                    aveItem.mSPListItem = aveItem.mSPFile.Item;
                    //同步重新赋值
                    aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                
                }
             
                if (allDocData.ContainsKey("MetaInfo"))
                {
                    byte[] bts = (byte[])allDocData["MetaInfo"];
                    aveItem.RestoreMetaInfo(aveItem.mSPListItem, bts);
                }
                //originalVersion == destVersion
                //通过测试发现，该处涉及到sharepoint API的一个"BUG",在执行完更新ModerationStatus之后，
                //如果某个column值在item.Propertes["colName"]和item["colName"]都存在的情况下，如果通过给item["colName"]赋值想要更新该column的值，
                //使用systemupdate之后，在item.Propertes["colName"]中的值还是旧的值，当再次使用systemupdate，item["colName"]也会被更新成旧的值，
                //在此处的处理方式为将更新moderationStatus的过程放在更新field value之后。
                //if (aveItem.mSPListItem != null)
                //{
                //    UpdateModerationInfomation(info, aveItem);
                //}
                if (info.CheckinComment != null && !aveItem.mSPFile.CheckInComment.Equals(info.CheckinComment))
                {
                    aveItem.SetDocData("CheckInComment", info.CheckinComment);
                }
                //if (mAveSPItem.ParentFolder.ParentList.SPList.BaseTemplate == SPListTemplateType.WebPageLibrary)
                //{
                //    ProcessSpecialField(fieldData);
                //}
                aveItem.SetReport(Report);
                // if it's notfile,execute this statements , otherwise skip 
                if (info.OriginalRowId > 0)
                {
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                }    
            }
        }

        private void PostRestoreDocument(AveDocumentInfo info, AveItem aveItem, Dictionary<string, object> allUserData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.PostRestoreDocument"))
            {

                if (info.NeedUpdateStatusByNative)
                {
                    //if (info.IsOrignialCheckOut)
                    //{
                    //mSite.QueryService.ChangeModerationStatusAndDraftOwnerIdByNative(info, aveItem.File, info.ModerationStatus);
                    //    aveItem.SetDocData("DraftOwnerId", null);
                    //}
                    //else
                    //{
                    //    mSite.QueryService.ChangeModerationStatusByNative(info, aveItem.File.UniqueId, info.ModerationStatus);
                    //}
                    aveItem.SetUserData("tp_ModerationStatus", info.ModerationStatus);
                    if (info.DraftOwnerId > 0)
                    {
                        aveItem.SetDocData("DraftOwnerId", info.DraftOwnerId);
                        aveItem.SetUserData("tp_DraftOwnerId", info.DraftOwnerId);
                    }
                }

                if (info.IsNewCreated && info.SettingInfo.KEEP_ITEM_TPGUID && allUserData.ContainsKey("#tp_GUID"))// keep tp_guid
                {
                    Guid tp_Guid = new Guid(allUserData["#tp_GUID"].ToString());
                    //mSite.QueryService.ChangeItemTPGuidByNative(info, info.SiteId, mFolder.Folder.UniqueId, aveItem.mSPFile.UniqueId, tp_Guid);
                    aveItem.SetUserData("tp_GUID", tp_Guid);
                }
                #region Removed! Change Level By Native
                //if (aveItem.mSPFile.Level != (SPFileLevel)info.OriginalLevel || info.DraftOwnerId > 0)
                //{

                //    //如果源端Level是255，但目的端的却不是255，这时不应该去改Level，因为文件时Check out时，上一个draft那个version是在alldocs表中
                //    //但Level为2时，上一个draft version就不在alldocs里面
                //    //强制改Level，会导致文件在页面中显示不出来
                //    //在大部分模块中，会去删除文件，不会出现这种Level是255，却当前文件不是255，除了Archiver
                //    int level = info.OriginalLevel;
                //    if (info.OriginalLevel == 255)
                //    {
                //        level = (byte)aveItem.mSPFile.Level;
                //    }
                //    mSite.QueryService.ChangeLevelByNative(info, aveItem.File.Item, aveItem.mSPFile.UIVersion, level, info.DraftOwnerId);
                //    aveItem.mSPFile = mWeb.Web.GetFile(aveItem.mSPFile.UniqueId);
                //}
                #endregion
                if (info.IsOrignialCheckOut && info.CheckoutUserId > 0 && (info.RestoringItem.TargetTable != RestoreTargetTable.AllDocVersions || info.IsNewCreated))
                {
                    if (this.mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                    {
                        mSite.QueryService.ChangeCheckoutUserIDForAllVersion(info.SiteId, info.ParentId, aveItem.mSPFile.UniqueId, info.CheckoutUserId);
                        aveItem.ReloadFile();
                        try
                        {
                            aveItem.InitBySPFile(aveItem.mSPFile, false);//reload to get checkout file object
                        }
                        catch (Exception ex)
                        {
                            logger.Info("Init file error: {0}", ex);
                        }
                    }
                    else
                    {
                        //为了平时开发使用，后期需要去掉或者完善log
                        throw new Exception("Failed to restore Checkout file version.");
                    }
                }

                if (info.SettingInfo.IsProcessSolutionStatus)
                {
                    #region SolutionItem
                    if (info.SolutionId != Guid.Empty && (aveItem.mList != null && aveItem.mList.BaseTemplate == AveListTemplateType.SolutionCatalog))
                    {
                        try
                        {
                            if (allUserData.ContainsKey("#SolutionStatus"))
                            {
                                int status = (int)allUserData["#SolutionStatus"];
                                SPUserSolutionCollection solutionColl = mWeb.Web.Site.Solutions;
                                SPUserSolution solution = solutionColl[info.SolutionId];
                                if (solution == null && status == 1)
                                {
                                    solution = solutionColl.Add(aveItem.mSPFile.Item.ID);
                                    //AveAssemblyUtility.InvokeMethod(solutionColl, typeof(SPUserSolutionCollection),
                                    //                                "EnsureSiteCollectionFeaturesActivated", new object[] { solution });
                                    if (solution.Status == SPUserSolutionStatus.Activated)
                                    {
                                        mWeb.Site.ReloadSite();//需要reload site，否则FeatureDefinitions取不到。ADO-155720
                                        mWeb.ReloadWeb();//更新Feature的状态
                                        using (SPSite site = mWeb.Web.Site)
                                        {
                                            SPFeatureDefinitionCollection featureDefinitions = site.FeatureDefinitions;
                                            foreach (SPFeatureDefinition featureDefinition in featureDefinitions)
                                            {
                                                if (featureDefinition.SolutionId == info.SolutionId)
                                                {
                                                    if (featureDefinition.Scope == SPFeatureScope.Site)
                                                    {
                                                        try
                                                        {
                                                            site.Features.Add(featureDefinition.Id, false, SPFeatureDefinitionScope.Site);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            logger.Debug("Add site feature {0} error. {1}", featureDefinition.DisplayName, ex);
                                                        }
                                                    }
                                                    else if (featureDefinition.Scope == SPFeatureScope.Web && ((info.ActivatedWebFeatureIDs != null && info.ActivatedWebFeatureIDs.Contains(featureDefinition.Id)) || (info.ActivatedWebSolutionFeatureIDs != null && info.ActivatedWebSolutionFeatureIDs.Contains(featureDefinition.Id))))
                                                    {
                                                        try
                                                        {
                                                            mWeb.Web.Features.Add(featureDefinition.Id, false, SPFeatureDefinitionScope.Site);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            logger.Debug("Add web feature {0} error. {1}", featureDefinition.DisplayName, ex);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (solution != null && status != 1)
                                {
                                    solutionColl.Remove(solution);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.SolutionHandleFailed, e.ToString());
                        }
                    }
                    #endregion
                }
                aveItem.UpdateDataByNative(true, true);


            }

        }

        protected bool IsThumbnails()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.IsThumbnails"))
            {

                bool isTumb = false;
                try
                {
                    List<int> targetListTemplates = new List<int> { 109, 851, 2100 };//109 refer to Picture Library,851 refer to images Library,2100 refer to slide Library

                    List<string> targetFolders = new List<string> { "_w", "_t" };//Hidden folder where thumbnails file placed
                    if (targetFolders.Contains(mFolder.Name)) //here we shouldn't use IgnoreCase
                    {
                        if (targetListTemplates.Contains(Convert.ToInt32(mFolder.ParentList.BaseTemplate)))
                        {
                            isTumb = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.JudgeThumbnailsError, e.ToString());
                    isTumb = false;
                }
                return isTumb;

            }

        }

        protected SPFile GetFile(string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.GetFile"))
            {

                string folderPath = mFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name;
                return mFolder.Folder.ParentWeb.GetFile(folderPath);

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Deactive is spelt correctly.")]
        protected void DeactiveSolution(AveItem aveItem, Guid solutionId, ref List<Guid> SolutionFeaturesId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.DeactiveSolution"))
            {

                SolutionFeaturesId = new List<Guid>();
                foreach (IAveFeature feature in mWeb.Features)
                {
                    if (feature.Definition.SolutionId == solutionId && !SolutionFeaturesId.Contains(feature.DefinitionId))
                    {
                        SolutionFeaturesId.Add(feature.DefinitionId);
                    }
                }
                if (aveItem.mList != null && aveItem.mList.BaseTemplate == AveListTemplateType.SolutionCatalog)
                {
                    SPUserSolutionCollection solutionColl = (mWeb.Site as AveSite).Site.Solutions;
                    SPUserSolution solution = solutionColl[solutionId];
                    if (solution != null)
                    {
                        solutionColl.Remove(solution);
                    }
                }

            }

        }

        private void ChangeItemId(AveDocumentInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDocumentSerializer.ChangeItemId"))
            {

                if (info.IsNewCreated && (aveItem.mSPFile.Item != null) && (aveItem.mSPFile.Item.ID != info.OriginalRowId) && info.NeedChangeItemId)
                {
                    int moveItemIdResult = mSite.QueryService.ChangeItemId(info.SiteId, aveItem.mList.ID, info.ParentId, aveItem.mSPFile.UniqueId, aveItem.mSPFile.Item.ID, info.OriginalRowId);
                    if (moveItemIdResult == 0)
                    {
                        aveItem.mSPFile = mWeb.Web.GetFile(aveItem.mSPFile.UniqueId);
                        aveItem.InitBySPFile(aveItem.mSPFile);
                    }
                }

            }

        }

        internal void RemoveWorkflowInstance(SPFile file)
        {
            if (file.Item != null)
            {
                AveListItem listItem = new AveListItem(this.mFolder.ParentList as AveList, file.Item);
                listItem.RemoveItemWorkflowInstance();
            }
        }

        public void Dispose()
        {
            if (mReport != null)
                mReport.Dispose();

        }
    }
}
