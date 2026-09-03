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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class EndUserRequest
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public EndUserRequest()
        {
        }

        internal RelativeDataArchiverContract GetEndUserArchiverContract(RMRelatedItemInfo info, string ruleId, string metaData)
        {
            mLog.Info("start get end user archive job for contract");
            RelativeDataArchiverContract endUserContract = null;
            try
            {
                endUserContract = GetEndUserArchiveContract(info, ruleId, metaData);
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Error in archiving the related item. Exception: {0}", ex.Message));
            }
            return endUserContract;
        }

        private RelativeDataArchiverContract GetEndUserArchiveContract(RMRelatedItemInfo info, string ruleId, string metaData)
        {
            mLog.Info("get archive contract");
            RelativeDataArchiverContract endUserContract = GetEndUserArchiveBackupContract(info, ruleId, metaData);
            switch (info.level)
            {
                case SORelativeDataArchiverNodeLevel.Document:
                    {
                        endUserContract.NodeLevel = (int)ArchiveLevel.Document;
                        endUserContract.FullPath = info.url;
                        break;
                    }
                case SORelativeDataArchiverNodeLevel.Item:
                    {
                        endUserContract.NodeLevel = (int)ArchiveLevel.Item;
                        endUserContract.FullPath = info.url;
                        break;
                    }
                //case SOEndUserArchiverNodeLevel.Multifiles:
                //    {
                //        endUserContract.NodeLevel = (int)ArchiveLevel.List;
                //        endUserContract.FullPath = SOContextObject.SOList.FullPath;
                //        break;
                //    }
                default:
                    mLog.Error(string.Format("Get contract error: {0}", info.level.ToString()));
                    break;
            }
            return endUserContract;
        }

        private RelativeDataArchiverContract GetEndUserArchiveBackupContract(RMRelatedItemInfo info, string ruleId, string metaData)
        {
            RelativeDataArchiverContract endUserContract = new RelativeDataArchiverContract();
            //Regist farm name is hard code 
            endUserContract.FarmName = "Remote Farm 2013";
            endUserContract.SiteId = info.SiteId.ToString();
            endUserContract.SiteUrl = info.SiteUrl;
            endUserContract.RuleId = ruleId;
            endUserContract.MetaData = GetRequestMetadata(info, metaData);
            //Agent 直接另起进程完成Job，不需要Agent Address
            //endUserContract.AgentAddress = AveEnv.AgentAddress;
            endUserContract.NodeId = string.Empty;
            endUserContract.NodeName = string.Empty;
            return endUserContract;
        }

        private string GetRequestMetadata(RMRelatedItemInfo info, string metaData)
        {
            string metadata = string.Empty;
            #region Build XML Tree
            SORelativeDataArchiveBackupRequest endUserArchiveBackupRequest = null;
            switch (info.level)
            {
                case SORelativeDataArchiverNodeLevel.Item:
                case SORelativeDataArchiverNodeLevel.Document:
                    {
                        endUserArchiveBackupRequest = GetItemArchiveRequest(info, metaData);
                        break;
                    }
                default:
                    break;
            }
            #endregion
            metadata = SerializerHelper.SerializeToXmlString<SORelativeDataArchiveBackupRequest>(endUserArchiveBackupRequest);
            return metadata;
        }

        private SORelativeDataArchiveBackupRequest GetItemArchiveRequest(RMRelatedItemInfo info, string metaData)
        {
            SORelativeDataArchiveBackupRequest endUserArchiveBackupRequest = new SORelativeDataArchiveBackupRequest()
            {
                SiteCollectionId = info.SiteId.ToString(),
                SiteCollectionUrl = info.SiteUrl,
                WebId = info.WebId.ToString(),
                ListId = info.ListId.ToString(),
                FolderId = info.FolderId.ToString(),
                LeafName = info.name,
                ItemId = info.id.ToString(),
                DocLibRowId = info.DocLibRowId,
                Path = AveUrlUtility.GetServerRelativeUrl(info.url),
                ParentFolderIsRootFolder = info.ParentFolderIsRootFolder,
                CurrentLevel = info.level.ToString(),
                //ItemLastModifiedTime = item.Web.RegionalSettings.TimeZone.LocalTimeToUTC(((DateTime)item["Modified"]))
                WebServerRelatedUrl = info.WebServerRelativeUrl,
                ListUrl = info.ListUrl,
                FolderUrl = info.FolderUrl
            };
            return endUserArchiveBackupRequest;
        }
    }

    public enum ArchiveLevel
    {
        Site = 100,
        Web = 200,
        List = 300,
        Folder = 400,
        Item = 500,
        Document = -3
    }
}
