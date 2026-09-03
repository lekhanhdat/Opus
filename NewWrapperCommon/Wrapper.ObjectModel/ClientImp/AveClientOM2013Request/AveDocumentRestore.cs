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
using System.IO;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.Application;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Resource.Client;
using AveClientRequest.Common;
using System.Xml;
using System.Reflection;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2013DocumentRestore : AveDocumentRestore, IDisposable
    {
        protected AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 为unittest添加构造函数
        /// </summary>
        public Ave2013DocumentRestore() { }

        public Ave2013DocumentRestore(AveClientOM2013Request request, Site site, object obj, AveClientContext conText, string serverVersion, IReport report)
            : base(request, site, obj, conText, serverVersion, report)
        {
        }

        protected override void PrepareRestoreContext(AveDocumentInfo docInfo, Stream fileStream)
        {
            base.PrepareRestoreContext(docInfo, fileStream);
            InitItemRestoreContext();
        }

        protected virtual void InitItemRestoreContext()
        {
            mItemRestore = mParentList != null && mRowId > 0 ? new Ave2013ListItemRestore(base.mRequest as AveClientOM2013Request, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, mObj) : null;
        }

        public override bool SkipRestoreTopicFile(string fileName, IAveWeb mParentWeb, List mParentList)
        {
            if (mParentWeb != null && mParentList != null)
            {
                if (mParentWeb.WebTemplate.Equals(AveCommunitiesConstants.CommunityTemplateName) && mParentList.Title.Equals("Site Pages") && fileName.Equals("Topic.aspx"))//过滤掉Site Pages中的Topic.aspx
                {
                    return true;
                }
            }
            return false;
        }

        protected override Microsoft.SharePoint.Client.File AddFile(string serverRelativeUrl, Stream stream, string etag, bool overwriteIfExists, AveClientOMRequest.SaveBinaryCheckMode checkMode, ClientRuntimeContext context, object obj)
        {
            Microsoft.SharePoint.Client.File file = null;
            string fileType = Path.GetExtension(mFileRelativeUrl);
            if (mSpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase) || stream.Length < WrapperConfiguration.BPOS_S.UploadLimit)
            {
                FileCreationInformation fileCreationInfo = new FileCreationInformation();
                fileCreationInfo.ContentStream = stream;
                fileCreationInfo.Url = serverRelativeUrl;
                fileCreationInfo.Overwrite = overwriteIfExists;
                file = AddFileByAPI(mParentFolder.Files, fileCreationInfo);
            }
            else
            {
                if (mContext.HasPendingRequest)
                {
                    mContext.ExecuteQuery();
                }
                Microsoft.SharePoint.Client.File.SaveBinaryDirect(mContext, mFileRelativeUrl, mFileStream, true);
                ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(mParentWeb, "ObjectData") as ClientObjectData;
                objData.MethodReturnObjects.Clear();
                file = GetFileByAPI();
            }

            if (mIsWelcomePageChanged)
            {
                string fileUrl = this.GetRelativeUrl(serverRelativeUrl);
                mIsWelcomePageChanged = false;
                mParentWeb.RootFolder.WelcomePage = fileUrl;
                mParentWeb.RootFolder.Update();
            }

            return file;
        }

        protected virtual Microsoft.SharePoint.Client.File GetFileByAPI()
        {
            return mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);
        }

        public virtual BaseDocumentRestore CreateDocumentObject(AveDocumentInfo info, Stream fileStream)
        {
            BaseDocumentRestore itemRestore;
            if (info.IsView)
            {
                itemRestore = new AveViewRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else if (info.OriginalRowId <= 0)
            {
                itemRestore = new AveSystemFileRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else if (info.ParentLibraryIsMasterPageGallery)
            {
                itemRestore = new AveMasterPageDocumentRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else if (IsPageLibrary(info))
            {
                itemRestore = new AvePageFileRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else if (info.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                itemRestore = new AveXmlFileRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else if ((info.AveItem.Folder.ParentList != null && info.AveItem.Folder.ParentList.IsOneDriveLibrary)
                || WrapperConfiguration.KeepVersionSettingDuringRestore)
            {
                //也可以用此方法还原普通的Document，不开关Version
                itemRestore = new AveOneDriveDocumentRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            else
            {
                itemRestore = new AveOrdinaryFileRestore(mContext as AveClientContext, mRequest as AveClientOM2013Request, mObj, info, fileStream);
            }
            itemRestore.SetReport(mReport);
            return itemRestore;
        }

        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream)
        {
            //return base.RestoreDocument(info, fileStream);
            //Need Test, and change WebPart restore logic.
            PrepareRestoreContext(info, fileStream);
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            try
            {
                if (info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER && info.SettingInfo.DELETE_ITEM)
                {
                    MoveToConflictFolder();
                }
                restoreResult = CreateDocumentObject(info, fileStream).Restore();
                if (info.SettingInfo.MOVE_SOURCE_TO_CONFLICT_FOLDER && info.SettingInfo.DELETE_ITEM)
                {
                    MoveToConflictFolder();
                }
                HandleSolution(info, restoreResult);
                return restoreResult;
            }
            catch (Exception ex)
            {
                restoreResult["Exception"] = string.Format("Restore document:{0}\\{1} failed:{2}.\r\n", info.ParentFolderRelativeUrl, info.Name, ex.ToString());
                restoreResult["ExceptionMessage"] = ex.Message;
                return restoreResult;
            }
        }

        protected bool IsPageLibrary(AveDocumentInfo info)
        {
            IAveWeb webCache = info.DocData.ContainsKey("AveWebObject") ? (IAveWeb)info.DocData["AveWebObject"] : null;
            if (webCache != null &&
                webCache.Lists[info.ListId].BaseTemplate == AveListTemplateType.PagesLibrary ||
                webCache.Lists[info.ListId].BaseTemplate == AveListTemplateType.WebPageLibrary)
            {
                return true;
            }
            return false;
        }

        protected void HandleSolution(AveDocumentInfo info, Dictionary<string, object> restoreResult)
        {
            object status;
            if (!info.DocData.TryGetValue("SolutionStatus", out status))
            {
                return;
            }
            if (status == null || (int)status != 1)
            {
                return;
            }
            object id = 0;
            if (!restoreResult.TryGetValue("RowId", out id))
            {
                return;
            }
            this.mRequest.OperateSolution("ACT", mContext.Url, info.ParentWebRelativeUrl, (int)id);
        }

        protected override void SetEditorReadOnly(bool readOnly) { } //if SP2013,Set editor field will throw exception.
        protected override void UpdateEditor(ListItem item, Dictionary<string, object> documentProperties) { }//if SP2103,Author and Editor should be update together. 
    }
}
