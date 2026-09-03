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
using AvePoint.ObjectModel.ClientOM;
using AvePoint.ObjectModel.WebService;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using AveClientRequest.Common;
using ClientFile = Microsoft.SharePoint.Client.File;
using Microsoft365.Authentication;
using AvePoint.Wrapper.Resource;
using Microsoft365.SharePoint.Cache.Restore;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveAttachmentRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveAttachmentRestore));
        private string mWebRelativeUrl;
        private string mListTitle;
        private Guid mListId;
        private int mRowId;
        private int mDocLibRowId;
        private string mAttachmentLeafName;
        private int mAttachmentSize;
        private string mListRootFolderServerRelativeUrl;
        private Web mWeb;
        private List mList;
        private ListItem mListItem;
        private IAveRequest mRequest;
        private AveRestoreOption mRestoreOption;
        private Dictionary<string, object> mOriginalData;
        protected ClientContext mContext;
        protected ITokenProvider mTokenProvider;
        private const int LARGE_FILE_BLOCK_SIZE = 50 * 1024 * 1024;//50M

        public AveAttachmentRestore(IAveRequest request, ClientContext context, ITokenProvider tokenProvider)
        {
            mRequest = request;
            mContext = context;
            mTokenProvider = tokenProvider;
        }
        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mOriginalData = data;
            mWebRelativeUrl = data["WebUrl"] as string;
            mListTitle = data["ListTitle"] as string;
            mListId = (Guid)data["ListId"];
            mDocLibRowId = Convert.ToInt32(data["DoclibRowId"]);
            mRowId = Convert.ToInt32(data["DestRowId"]);
            mAttachmentLeafName = data["Name"] as string;
            mAttachmentSize = Convert.ToInt32(data["Size"]);
            mListRootFolderServerRelativeUrl = data["ListRootFolderServerRelativeUrl"] as string;
            mRestoreOption = (AveRestoreOption)data["RestoreOption"];
            mWeb = mContext.Site.OpenWeb(mWebRelativeUrl);
            mContext.Load(mWeb, w => w.Url);
            mList = mWeb.Lists.GetById(mListId);
            mListItem = mList.GetItemById(mRowId);
        }
        public Dictionary<string, object> RestoreAttachment(Dictionary<string, object> docData, Stream fileStream)
        {
            PrepareRestoreContext(docData);
            if(ItemRestoreCache.IsOverWriteFailItem(mListId.ToString(), mDocLibRowId.ToString()))
            {
                throw new Exception("RM_RS_FailOverwriteItem");
            }

            Attachment attachment = null;
            ListItemComplianceInfo complianceInfo = null;
            if (IsAttachmentExists(ref attachment))
            {
                if (mRestoreOption == AveRestoreOption.OverWrite)
                {
                    complianceInfo = GetComplianceTagIfItemIsNewCreate(mListItem);
                    DeleteComplianceTagIfItemIsNewCreate(complianceInfo);
                    attachment.DeleteObject();
                    mContext.ExecuteQuery();
                }
                else
                {
                    //Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                    //AveClientOM2013Request.AssembleAttachmentProperties(attachment, attachmentProperties);
                    throw new SkipException(WrapperRestoreReportResource.Wrapper_SkippedItemByIsSameItem);
                    //return attachmentProperties;
                }
            }
            else
            {
                complianceInfo = GetComplianceTagIfItemIsNewCreate(mListItem);
                DeleteComplianceTagIfItemIsNewCreate(complianceInfo);
            }
            Dictionary<string, object> res = null;
            if (fileStream.Length < WrapperConfiguration.WrapperConfigurationForBPOS.UploadLimit)
            {
                res = AddSmallAttachment(fileStream);
            }
            else if (fileStream.Length < LARGE_FILE_BLOCK_SIZE)
            {
                res = AddLargeFile(fileStream,true);
            }
            else
            {
                res = AddLargeFile(fileStream, false);
            }
            SetComplianceTagIfItemIsNewCreate(complianceInfo);
            return res;
        }

        private ListItemComplianceInfo GetComplianceTagIfItemIsNewCreate(ListItem listItem)
        {
            try
            {
                if(!ItemRestoreCache.IsNewCreateItem(mListId.ToString(), mRowId.ToString()))
                {
                    return null;
                }
                return mRequest.GetListItemComplianceInfo(mContext, listItem);
            }
            catch(Exception e)
            {
                mLogger.Error($"Fail get compliance tag,ex:{e}");
                throw;
            }
        }

        private void DeleteComplianceTagIfItemIsNewCreate(ListItemComplianceInfo complianceInfo)
        {
            if (ItemRestoreCache.IsNewCreateItem(mListId.ToString(), mRowId.ToString()) && !string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
            {
                try
                {
                    if (!complianceInfo.TagPolicyRecord && complianceInfo.TagPolicyHold && IsRecordTypeComplianceTag(complianceInfo.ComplianceTag) && WasOriginallyLocked())
                    {
                        mRequest.LockRecordItem(mWebRelativeUrl, mListRootFolderServerRelativeUrl, mRowId.ToString());
                    }
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { mRowId }, "");
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Fail delete retention label,error message:{ex.Message},web url:{mWebRelativeUrl},listUrl:{mListRootFolderServerRelativeUrl},rowId:{mRowId},error:{ex}");
                }
            }
        }

        protected bool IsRecordTypeComplianceTag(string complianceTagName)
        {
            try
            {
                var sitePropertyContext = SitePropertyCache.GetInstance();
                if ( sitePropertyContext.AvaliableComplianceTags == null)
                {
                    sitePropertyContext.InitAvaliableComplianceTags(mContext.Url, mContext);
                }
                var complianceTag = sitePropertyContext.AvaliableComplianceTags.FirstOrDefault(info => info.TagName == complianceTagName);
                if (complianceTag != null)
                {
                    if (complianceTag.BlockDelete && complianceTag.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLogger.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLogger.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}, ex:{ex}");
                throw;
            }
        }

        private bool WasOriginallyLocked()
        {
            if (mOriginalData?.TryGetValue("_vti_ItemHoldRecordStatus", out var status) != true || status == null || !int.TryParse(status.ToString(), out var value))
            {
                return false;
            }
            return ((long)value & 16L) != 0;
        }

        private void SetComplianceTagIfItemIsNewCreate(ListItemComplianceInfo complianceInfo)
        {
            if (ItemRestoreCache.IsNewCreateItem(mListId.ToString(), mRowId.ToString()) && !string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
            {
                try
                {
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { mRowId }, complianceInfo?.ComplianceTag);
                    if (WasOriginallyLocked() && IsRecordTypeComplianceTag(complianceInfo.ComplianceTag))
                    {
                        mRequest.LockRecordItem(mWebRelativeUrl, mListRootFolderServerRelativeUrl, mRowId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Fail set retention label,label:{complianceInfo.ComplianceTag},web url:{mWebRelativeUrl}, list url:{mListRootFolderServerRelativeUrl}, row id:{mRowId},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
        }

        private bool IsAttachmentExists(ref Attachment attachment)
        {
            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
            try
            {
                using (ehScope.StartScope())
                {
                    using (ehScope.StartTry())
                    {
                        attachment = mListItem.AttachmentFiles.GetByFileName(mAttachmentLeafName);
                        //modify for SAAS-23463 增加comment的load
                        mContext.Load(mListItem, i => i["Editor"], i => i["Modified"], i => i["_ModerationStatus"], i => i["_ModerationComments"]);
                        mContext.Load(attachment);
                    }
                    using (ehScope.StartCatch())
                    {
                        //modify for SAAS-23463
                        mContext.Load(mListItem, i => i["Editor"], i => i["Modified"], i => i["_ModerationStatus"], i => i["_ModerationComments"]);
                    }
                }
                mContext.ExecuteQuery();
            }
            catch(Exception e)
            {
                if (e.Message.Contains("Item does not exist. It may have been deleted by another user"))
                {
                    throw new Exception("RM_MA_WF_NoElements", e);
                }
                else
                {
                    throw;
                }
            }
            return !ehScope.HasException;
        }

        private Dictionary<string, object> AddSmallAttachment(Stream fileStream)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;                        
                     
            attachment = AddAttachment(fileStream);
            UpdateModifiedAndModeration();
            mContext.ExecuteQuery();
            AveClientOM2013Request.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private Dictionary<string, object> AddLargeFile(Stream fileStream,bool useRestApi)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            Attachment attachment = null;                                       
            attachment = AddAttachment(new MemoryStream(new byte[] { 0 }));
            mContext.ExecuteQuery();
            if (useRestApi)
            {
                FileRestProcessor.AddFileByRestApi(mContext, mTokenProvider, mWeb.Url, Guid.Empty, attachment.ServerRelativeUrl, fileStream, true);
            }
            else
            {
                FileCsomProcessor.UploadLargeFile(mContext, attachment.ServerRelativeUrl, fileStream, () =>
                   {
                       return mWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(attachment.ServerRelativeUrl));
                   });
            }
            UpdateModifiedAndModeration();
            mContext.ExecuteQuery();
            AveClientOM2013Request.AssembleAttachmentProperties(attachment, attachmentProperties);
            return attachmentProperties;
        }

        private void UpdateModifiedAndModeration() //SAAS-1996
        {
            ListItem tempListItem = new ListItem(this.mList.Context, new ObjectPathMethod(this.mList.Context, this.mList.Path, "GetItemById", new object[] { mRowId }));
            tempListItem["Modified"] = mListItem["Modified"];
            tempListItem["Editor"] = mListItem["Editor"];
            tempListItem["_ModerationStatus"] = mListItem["_ModerationStatus"];
            //add for SAAS-23463
            tempListItem["_ModerationComments"] = mListItem["_ModerationComments"];
            tempListItem.Update();
        }

        private Attachment AddAttachment(Stream stream)
        {            
            AttachmentCreationInformation attachmentCreationInfo = new AttachmentCreationInformation();
            attachmentCreationInfo.FileName = mAttachmentLeafName;
            attachmentCreationInfo.ContentStream = stream;
            Attachment attachment = mListItem.AttachmentFiles.Add(attachmentCreationInfo);
            mContext.Load(attachment);
            return attachment;
        }

        public void Dispose()
        {
        }
    }
}
