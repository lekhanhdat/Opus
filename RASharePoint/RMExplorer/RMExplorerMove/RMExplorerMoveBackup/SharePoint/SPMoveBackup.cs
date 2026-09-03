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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPMoveBackup : IExplorerMoveBackup
    {
        private RALogger logger = RALogger.GetInstance(typeof(SPMoveBackup));

        internal string exportPath = string.Empty;
        internal string exportFileName = string.Empty;
        private bool isCurrentVersion = false;
        private AveSPFolder parentFolder;
        private IAveFile aveDoc;
        private bool IsFirstItem;
        private int fileVersion;
        private AveObjectModelFactory aveObjectModelFactory = null;
        private IAveORecords mRecord = null;

        private IAveORecords Record
        {
            get
            {
                if (mRecord == null)
                {
                    mRecord = aveObjectModelFactory.CreateRecords();
                }
                return mRecord;
            }
        }

        public SPMoveBackup(AveObjectModelFactory modelFactory, AveSPFolder mParentFolder, IAveFile mAveDoc, int mFileVersion, bool currentVersion = true, string tempPath = "")
        {
            parentFolder = mParentFolder;
            aveDoc = mAveDoc;
            aveObjectModelFactory = modelFactory;
            fileVersion = mFileVersion;
            isCurrentVersion = currentVersion;
            if (string.IsNullOrEmpty(tempPath))
            {
                exportPath = ExportTempFileLocation.GenerateTempFilePath(mAveDoc.Name);
            }
            else
            {
                exportPath = tempPath;
            }
            exportFileName = mAveDoc.Name;
        }
        public SPMoveBackup(SourceRecord record, string tempPath = "")
        {
            string userName = record.UserName;
            var user = new AveBPOSAccountInfo() { Domain = "", UserName = userName, Password = CspCommunicationWrapper.UnWrapKeyToSecureString(record.Password) };
            var aveSPSite = new AveSPSite(record.SiteUrl, AveContextKind.ClientObjectModel, user, null);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(record.SiteUrl, user, AveContextKind.ClientObjectModel);
            var aveSite = aveObjectModelFactory.CreateSite(record.SiteUrl);
            var aveWeb = aveSite.OpenWeb(record.WebId);
            var aveSPWeb = new AveSPWeb(aveSPSite, record.WebId, aveWeb.Name);
            var aveList = aveWeb.GetList(record.ListId);
            var aveSPList = new AveSPList(aveSPWeb, record.ListId, aveList.RootFolder.ServerRelativeUrl, true);
            aveDoc = aveWeb.GetFile(record.ItemId, record.DirPath);
            fileVersion = aveDoc.UIVersion;
            parentFolder = InitSPFolderNode(aveSPList, aveDoc.ParentFolder, aveList.RootFolder.UniqueId);
            if (string.IsNullOrEmpty(tempPath))
            {
                exportPath = ExportTempFileLocation.GenerateTempFilePath(record.LeafName);
            }
            else
            {
                exportPath = tempPath;
            }
            exportFileName = record.LeafName;
        }

        public void MoveBackup()
        {
            try
            {
                using (RAFileSender fileSender = new RAFileSender(exportPath))
                {
                    var fileSenderWrapper = new FileSendWrapper(fileSender);
                    using (var exportStream = new WrapperBackupStreamV1(fileSenderWrapper))
                    {
                        SPExport export = new SPExport(parentFolder as AveSPFolder, aveDoc, IsFirstItem, fileVersion);
                        export.ExportSPFile(exportStream);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in export file to fs, reason : " + ex.ToString());
                throw;
            }
        }

        public Guid GetSourceTermId(string columnName)
        {
            Guid termId = Guid.Empty;
            try
            {
                if (aveDoc.Item.Fields.ContainsField(columnName))
                {
                    var termObj = aveDoc.Item[columnName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termId = new Guid(valueString[1]);
                        }
                        else
                        {
                            logger.Info($"{aveDoc.Url} invalid term format:{valueString}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting source termid, file url:{0}, error:{1}", aveDoc?.Url, e.ToString());
            }
            return termId;
        }

        public void Delete()
        {
            TempFileDeletion.DeleteTempFile(this.exportPath);
            if (isCurrentVersion)
            {
                try
                {
                    SPDeletion.DeleteSPFile(aveDoc);
                }
                catch (Exception ex)
                {
                    logger.Debug("delete file failed, will retry with internal way, reason : {0}.", ex.ToString());
                    //Client Record API do not impl the IsRecord interface
                    if (CommonUtil.IsRecord(aveDoc.Item))
                    {
                        logger.Info("Current file is delcare file and UndeclareItemAsRecord and delete.FileName:{0}.", aveDoc.Name);
                        Record.UndeclareItemAsRecord(aveDoc.Item);
                        SPDeletion.DeleteSPFile(aveDoc);
                        logger.Info("Delete declare file success.File name:{0}", aveDoc.Name);
                    }
                    else if (aveDoc.Item.Fields.ContainsField("Retention label"))
                    {
                        logger.Info("Current file is label file and Records remove label and delete.FileName:{0}.", aveDoc.Name);
                        //aveDoc.Item.SetComplianceTag(string.Empty, false, false, false, false);
                        var complianceInfo = aveDoc.Item.GetComplianceInfo(false);
                        if (!complianceInfo.TagPolicyRecord && complianceInfo.TagPolicyHold)
                        {
                            aveDoc.Item.LockRecordItem();
                        }
                        aveDoc.Item.SetComplianceTagOnBulkItems(string.Empty);
                        aveDoc = aveDoc.ParentFolder.ParentWeb.GetFile(aveDoc.UniqueId, aveDoc.ServerRelativeUrl);
                        SPDeletion.DeleteSPFile(aveDoc);
                        logger.Info("Delete label file success.File name:{0}", aveDoc.Name);
                    }
                    else
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
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
                var a = folder.Item["Version"].ToString();
                aveSPFolder = new AveSPFolder(InitSPFolderNode(aveSPList, folder.ParentFolder, rootFolderId), folder.Name, folder.UniqueId, folder.Item.ID, 1);
            }
            return aveSPFolder;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }


    }
}
