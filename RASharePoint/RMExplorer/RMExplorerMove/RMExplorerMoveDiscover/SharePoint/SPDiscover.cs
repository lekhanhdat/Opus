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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPDiscover : IMoveDiscover
    {
        private RALogger logger = RALogger.GetInstance(typeof(SPDiscover));
        private PCContainer<SourceBase> pcContainer = null;
        private SourceRecord record = null;
        private IAveFile aveDoc;

        private bool calculateTotalCount = false;
        private int calculateLevel = 0;
        public long TotalCount { get; private set; } = 0;

        public SPDiscover(SourceRecord mRecord, PCContainer<SourceBase> mPCContainer)
        {
            record = mRecord;
            pcContainer = mPCContainer;
        }

        //这个构造函数只给统计total size 的 使用，正常功能不要使用
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mRecord">原端页面选择的对象</param>
        /// <param name="level">统计total size 的时候discover 到那层</param>
        public SPDiscover(SourceRecord mRecord, int level)
        {
            calculateTotalCount = true;
            calculateLevel = level;
            record = mRecord;
        }

        public async System.Threading.Tasks.Task StartDiscoverAsync()
        {
            logger.Info("Start to get remote site collection:" + record.AveSiteId);
            RemoteSiteCollection site = new RMExplorerUtility().GetRemoteSiteCollectionByListUrl(record.SiteUrl);
            var user = await PoolUserUtil.GetBPOSInfoAsync(site);
            var aveSPSite = new AveSPSite(record.SiteUrl, AveContextKind.ClientObjectModel, user, null);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(record.SiteUrl, user, AveContextKind.ClientObjectModel);
            var aveSite = aveObjectModelFactory.CreateSite(record.SiteUrl);
            var aveWeb = aveSite.OpenWeb(record.WebId);
            var aveSPWeb = new AveSPWeb(aveSPSite, record.WebId, aveWeb.Name);
            var aveList = aveWeb.GetList(record.ListId);
            var aveSPList = new AveSPList(aveSPWeb, record.ListId, aveList.RootFolder.ServerRelativeUrl, true);
            aveDoc = aveWeb.GetFile(record.ItemId, record.DirPath);
            if (!aveDoc.Exists)
            {
                logger.Error(string.Format("File : {0} is not exist in source, skip it.", record.DirPath));
                throw new Exception(I18NString.SourceNotExists);
            }
            if (IsCheckoutFile(aveDoc))
            {
                if (aveDoc.Item != null && !CommonUtil.IsRecord(aveDoc.Item))
                {
                    logger.Error(string.Format("File : {0} is checked out, skip it.", record.DirPath));
                    throw new Exception(I18NString.FileIsCheckOut);
                }
                else
                {
                    logger.Debug(string.Format("File : {0} is declared in source,", record.Id));
                }
            }
            //Must Use aveSPList.SPList.RootFolder.UniqueId, because if you use ListId get aveList, wrapper do not have aveList.RootFolder.UniqueId value
            var parentFolder = InitSPFolderNode(aveSPList, aveDoc.ParentFolder, aveSPList.SPList.RootFolder.UniqueId);
            bool firstItemVersion = false;
            if(calculateTotalCount)
            {
                if(calculateLevel > 0)
                {
                    TotalCount += aveDoc.Versions.Count;
                }
                TotalCount++;//Add current version here
                return;
            }
            foreach (var version in aveDoc.Versions)
            {
                bool isFirstItemVersion = false;
                if (!firstItemVersion)
                {
                    isFirstItemVersion = true;
                    firstItemVersion = true;
                }
                int uiVersion = CommonUtil.ConvertVersionLabelToUIVersion(version.VersionLabel);
                var backup = new SPMoveBackup(aveObjectModelFactory, parentFolder, aveDoc, uiVersion, false);
                var source = new SPSource(backup, record, (int)GCommon.Contract.Tree.Object.NodeLevel.ItemVersion, 1, record.FullPath + ":" + version.VersionLabel, backup.exportPath, backup.exportFileName, isFirstItemVersion);
                pcContainer.Produce(source);
            }
            bool firstItem = false;
            if (!firstItemVersion)
            {
                firstItemVersion = true;
                firstItem = true;
            }
            var moveBackup = new SPMoveBackup(aveObjectModelFactory, parentFolder, aveDoc, aveDoc.UIVersion, true);
            var currentVersionSource = new SPSource(moveBackup, record, record.NodeType, 0, record.FullPath, moveBackup.exportPath, moveBackup.exportFileName, firstItem);
            pcContainer.Produce(currentVersionSource);
        }

        public bool IsCheckoutFile(IAveFile file)
        {
            bool checkOutFile = false;
            //Online 环境中，目前没有一个可用API 判断文件是check out 的，所以只能通过获取column value 来判断。 check out 文件的column ： {[CheckoutUser, 13;#nmg]}；  非check out文件{[CheckedOutUserId, 1;#]}
            try
            {
                if (file.Item != null)
                {
                    var values = file.Item.FieldValues;
                    string checkoutUser = values.ContainsKey("CheckoutUser") ? values["CheckoutUser"].ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(checkoutUser))
                    {
                        string separator = ";#";
                        int index = checkoutUser.IndexOf(separator);
                        if (index > 0)
                        {
                            var checkoutUserName = checkoutUser.Substring(index);
                            if(!string.IsNullOrEmpty(checkoutUser))
                            {
                                checkOutFile = true;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                logger.Debug(" Can not get Check Out User. Reason : {0}", e.ToString());
            }
            return checkOutFile;
        }

        private AveSPFolder InitSPFolderNode(AveSPList aveSPList, IAveFolder folder, Guid rootFolderId)
        {
            AveSPFolder aveSPFolder = null;
            if (folder.UniqueId == rootFolderId)
            {
                aveSPFolder = new AveSPFolder(aveSPList);
            }
            else
            {
                string uiVersion = folder.Item["Version"].ToString();
                int version = Convert.ToInt32(uiVersion.Split('.')[0]) * 512 + Convert.ToInt32(uiVersion.Split('.')[1]);
                aveSPFolder = new AveSPFolder(InitSPFolderNode(aveSPList, folder.ParentFolder, rootFolderId), folder.Name, folder.UniqueId, folder.Item.ID, version);
            }
            return aveSPFolder;
        }
    }
}
