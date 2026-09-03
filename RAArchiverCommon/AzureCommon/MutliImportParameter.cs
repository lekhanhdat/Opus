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
using AvePoint.Wrapper.Common;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSMAzureCommon
{
    public class MutliImportParameter
    {
        private IAveSite site;
        public IAveSite Site
        {
            get { return site; }
            set { site = value; }
        }

        private Guid webId;
        public Guid WebId
        {
            get { return webId; }
            set { webId = value; }
        }

        private Guid listId;
        public Guid ListId
        {
            get { return listId; }
            set { listId = value; }
        }

        private String manifestContainerDir;
        public String ManifestContainerDir
        {
            get { return manifestContainerDir; }
            set { manifestContainerDir = value; }
        }

        private String dataContainerDir;
        public String DataContainerDir
        {
            get { return dataContainerDir; }
            set { dataContainerDir = value; }
        }

        private WinAzure azureInfo;
        public WinAzure AzureInfo
        {
            get { return azureInfo; }
            set { azureInfo = value; }
        }

        private MigrationModuleType migrationModuleType;
        public MigrationModuleType MigrationModuleType
        {
            get { return migrationModuleType; }
            set { migrationModuleType = value; }
        }

        private bool isNeedCheckSourceFilesUploaded = false;
        public bool IsNeedCheckSourceFilesUploaded
        {
            get { return isNeedCheckSourceFilesUploaded; }
            set { isNeedCheckSourceFilesUploaded = value; }
        }

        private bool isEncryption;
        public bool IsEncryption
        {
            get { return isEncryption; }
            set { isEncryption = value; }
        }

        
		private bool isFreeContainer;
        public bool IsFreeContainer
        {
            get { return isFreeContainer; }
            set { isFreeContainer = value; }
        }

        private bool isReset;
        public bool IsReset
        {
            get { return isReset; }
            set { isReset = value; }
        }

        private FreeContainerParameters fcParameters;
        public FreeContainerParameters FCParameters
        {
            get { return fcParameters; }
            set { fcParameters = value; }
        }

        private List<ARRestoreFileInfo> currentRestoreFileIdsList;
        public List<ARRestoreFileInfo> CurrentRestoreFileIdsList
        {
            get { return currentRestoreFileIdsList; }
            set { currentRestoreFileIdsList = value; }
        }

        private int retryMigrationJobTime=15;
        public int RetryMigrationJobTime
        {
            get { return retryMigrationJobTime; }
            set { retryMigrationJobTime = value; }
        }

        public string JobId;

        #region Migration Restore Job that have original stub info

        private bool isOriginalSiteExist;

        public bool IsOriginalSiteExist
        {
            get { return isOriginalSiteExist; }
            set { isOriginalSiteExist = value; }
        }

        private IAveSite oriSite;
        public IAveSite OriSite
        {
            get { return oriSite; }
            set { oriSite = value; }
        }

        private Guid oriWebId;
        public Guid OriWebId
        {
            get { return oriWebId; }
            set { oriWebId = value; }
        }

        // original stub may be in different list, so we need original list id in each item to find it.
        //private Guid oriListId;
        //public Guid OriListId
        //{
        //    get { return oriListId; }
        //    set { oriListId = value; }
        //}
        #endregion

        // migration restore job large file hash info
        private Dictionary<string, FileHash> _uploadFileHashDic;
        public Dictionary<string, FileHash> UploadFileHashDic
        {
            get { return _uploadFileHashDic; }
            set { _uploadFileHashDic = value; }
        }
    }

    public class ARRestoreFileInfo
    {
        public string id;
        public int rowid;
        public string serverRelativeUrl;
        public string name;
        public long size;
        public string subjobid;
        public string MD5;
        public long TotalSize;
        public int AuthorID;
        public string AuthorEmail;
        public int ModifiedID;
        public string ModifiedEmail;
        public string CreateTime;
        public string ModifiedTime; //(DateTime)file.ListItemAllFields.FieldValues["Modified"];
        public long ModifiedTimeTicks;//(DateTime)file.ListItemAllFields.FieldValues["Modified"];

        public long TimeLastModifiedTicks;//file.TimeLastModified
        public int VersionCount;

        public bool IsManifestStub;
        public string StubId;
    }

    public class BulkDeclareAndDeleteFileInfo
    {
        public ARRestoreFileInfo mARRestoreFileInfo;
        public IAveListItem stubListItem;
        public int stubItemRowId;
        public bool hasErrorNode;
        public bool isSendedReport;
    }

    public enum MigrationModuleType
    {
        FileMigration = 0,
        LivelinkMigration,
        DocumentumMigration,
        SPMigration,
        LotusNotesMigration
    }

    public class BulkDeclareAndDeleteMigrationFileInfo
    {
        public ARMigrationRestoreFileInfo mARRestoreFileInfo;
        public IAveListItem restoredListItem;
        public int restoredItemRowId;
        public bool hasErrorNode;
        public bool isSendedReport;
    }

    public class ARMigrationRestoreFileInfo : ARRestoreFileInfo
    {
        public bool NeedDeclareRecord;
        public bool NeedDeleteStub;
        public string StubPath;

        // handle after report
        public char Type;
        public long ArchiveTime;
        //public string SrcUrl;
        public string StorageId;
        public string RowKey; // Id
        //public string ItemPathMd5;
        public string BackUpJobId;

        // for original stub
        public bool NeedDeleteOriStub;
        public string OriStubPath;
        public int OriStubRowId;
        public Guid OriParentListId;
        public string AveDocIdOriginal; // uniqueID of archived item (nodeGuid in index db)
    }
}
