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
using AvePoint.Common;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using HSMAzureCommon;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMCommon
{
    public class HSMStubInfo
    {
    }

    public class HSMListInfo: HSMStubInfo
    {
        public IAveList ListObject;
    }

    public class HSMFileInfo : HSMStubInfo
    {
        public IAveFile FileObject;
        public List<AveRoleAssignmentInfo> RoleAssignment;
        public AveUserList UserList;
        public AveGroupList GroupList;
        public AveSPDocumentMetadataDto MetadataDto;
        public string PathMD5;
        public string StubId;
        public string FileServerRelatedUrl;
        public ArchiverBasicIndex ArchiverFileIndex;
    }

    public class HSMManifestFileInfo : HSMStubInfo
    {
        public string FileServerRelatedUrl;
        public string PathMD5;
        public string StubId;
        public string SpId;
        public Guid WebId;
        public long DocumentSize;
        public string FileName;
        public string DocumentAccessUrl;
        public string SiteUrl;
        public string FolderPath;
        public Guid ListId;
        public int BaseTemplate;
        public bool Hidden;
        public bool IsCatalog;
        public int RowId;
        public int AuthorId;
        public int ModifiedId;
        public string RuleName;
        public string StubTemplateId;
        public DateTime? CreatedTime;
        public DateTime? ModifiedTime;
        public string Author;
        public string Editor;
        public Dictionary<string, object>? ColumnValues;
        public List<AveRoleAssignmentInfo>? RoleAssignments;
        public string VersionString;
        public long TotalSize;
        public AveSPFolder ParentFolder;
    }

    public class HSMLocalInfo
    {
        public string LocalBasePath;
        public string DataContainerName;
        public string MetadataContainerName;
        public string QueueReportContainerName;
        public string ContainerId;

        public string MetadataContainerPath
        {
            get { return Path.Combine(LocalBasePath, MetadataContainerName); }
        }

        public string DataContainerPath
        {
            get { return Path.Combine(LocalBasePath, DataContainerName); }
        }

        public static HSMLocalInfo CreateNew(string tenantGroupId, string jobId)
        {
            HSMLocalInfo info = new HSMLocalInfo();

            info.ContainerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            string jobid = jobId.Replace("_", "-").ToLower();
            info.LocalBasePath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, tenantGroupId, jobId);
            info.DataContainerName = "m-" + info.ContainerId;
            info.MetadataContainerName = "s-" + info.ContainerId;
            info.QueueReportContainerName = "q-" + jobid + "-" + info.ContainerId;
            return info;
        }
    }

    public class HSMFileMapping
    {
        public Guid ID;
        public int RowID;
        public Guid FileNewID;
        public string FileUrl;
        public long Size;
        public long TotalSize;
        public string MD5;
        public Guid ListID;
        public string RuleID;
        public string ContainerId;
        public StubExportStauts Status;
        public int AuthorID;
        public string AuthorEmail;
        public int ModifiedID;
        public string ModifiedEmail;
        public string CreateTime;
        public string ModifiedTime;
        public int VersionCount;
        public long ModifiedTimeTicks;
        public long TimeLastModifiedTicks;
        public bool IsManifestStub;
        public string StubId;

        public ARRestoreFileInfo Conver2RestoreFileInfo(string subJobId)
        {
            ARRestoreFileInfo restoreFileInfo = new ARRestoreFileInfo();
            restoreFileInfo.id = ID.ToString();
            restoreFileInfo.rowid = RowID;
            restoreFileInfo.serverRelativeUrl = FileUrl;
            restoreFileInfo.size = Size;
            restoreFileInfo.subjobid = subJobId;
            restoreFileInfo.MD5 = MD5;
            restoreFileInfo.TotalSize = TotalSize;
            restoreFileInfo.AuthorID = AuthorID;
            restoreFileInfo.AuthorEmail = AuthorEmail;
            restoreFileInfo.ModifiedID = ModifiedID;
            restoreFileInfo.ModifiedEmail = ModifiedEmail;
            restoreFileInfo.CreateTime = CreateTime;
            restoreFileInfo.ModifiedTime = ModifiedTime;
            restoreFileInfo.VersionCount = VersionCount;
            restoreFileInfo.ModifiedTimeTicks = ModifiedTimeTicks;
            restoreFileInfo.TimeLastModifiedTicks = TimeLastModifiedTicks;
            restoreFileInfo.IsManifestStub = IsManifestStub;
            restoreFileInfo.StubId = StubId;
            return restoreFileInfo;
        }
    }

    public enum StubExportStauts
    {
        //export status
        Successful = 0,
        Failed = 1,

        //when delete file, we need verified.
        Verified = 2,
    }

    public class CreateLinkFileReportDto
    {
        public string FileUrl;
        public JobDetailsStatus Status;
        public long Size;
        public string Md5;
        public string Message;
    }

    public class StubFileDto
    {
        public string BackupFileId;
        public string StubRealId;
        public JobDetailsStatus Status;
        public string StubTypeStr;
        public string IndexRecordId;
        public string SiteUrl; // old stub location 
        public bool IsSkipUpdateIndex;
    }

    public class MigrationRestoreFileDto : CreateLinkFileReportDto
    {
        public string NodeType;
        public string Path; // different from SourceUrl (FileUrl)
        public long StartTime; // for report
        public List<MigrationRestoreVersionDto> VersionsReportDtos;
    }

    public class MigrationRestoreVersionDto : CreateLinkFileReportDto
    {
        public string Version;
        public string Name;

        // for statistics
        public char Type;
        public string BackUpJobId;
        public string StorageId;
        public string RowKey;
        public long ArchiveTime;
        public long StartTime; // for report
    }
}
