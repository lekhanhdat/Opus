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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    #endregion

    public class IndexConstants
    {
        // Granular 表名 & 索引名
        public const string TableNameGranularHead = "TB_HEAD_INDEX";
        public const string TableNameGranularBody = "TB_BODY_INDEX";
        public const string TableNameGranularAgent = "TB_AGENT_INDEX";
        public const string TableNameGranularSiteMaster = "TB_SITE_MASTER_INDEX";
        public const string TableNameGranularJobInfo = "TB_JOB_INFO";
        public const string TableNameGranularJobProperties = "TB_JOB_PROPERTIES";

        public const string IndexNameGranularHeadJobid = "IDX_HEAD_JOBID";
        public const string IndexNameGranularBodyJobid = "IDX_BODY_JOBID";
        public const string IndexNameGranularHeadPathMD5Hash = "IDX_HEAD_PATH_MD5_HASH";
        public const string IndexNameGranularBodyPathMD5Hash = "IDX_BODY_PATH_MD5_HASH";
        public const string IndexNameGranularHeadParentPathMD5Hash = "IDX_HEAD_PARENT_PATH_MD5_HASH";
        public const string IndexNameGranularBodyParentPathMD5Hash = "IDX_BODY_PARENT_PATH_MD5_HASH";

        //Platform 表名
        public const string TableNamePlatformHead = "TB_HEAD_INDEX";
        public const string TableNamePlatformBody = "TB_BODY_INDEX";
        public const string TableNamePlatformJobInfo = "TB_JOB_INFO";
        public const string TableNamePlatformAgentIndex = "TB_AGENT_INDEX";
        public const string TableNamePlatformSiteMaster = "TB_SITE_MASTER_INDEX";
        public const string TableNamePlatformJobProperties = "TB_JOB_PROPERTIES";
        public const string TableNamePlatformFolder = "FOLDER";
        public const string TableNamePlatformFile = "FILE";
        public const string TableNamePlatformBlob = "SPOBJECT";

        public const string IndexNamePlatformHeadParentPathMD5 = "IDX_TB_HEAD_INDEX_PARENTPATHMD5";
        public const string IndexNamePlatformHeadLeafName = "IDX_TB_HEAD_INDEX_LEAF_NAME";
        public const string IndexNamePlatformBodyParentPathMD5 = "IDX_TB_BODY_INDEX_PARENTPATHMD5";
        public const string IndexNamePlatformBodyLeafName = "IDX_TB_BODY_INDEX_LEAF_NAME";
        public const string IndexNamePlatformFileParentGuid = "IDX_FILE_PARENTGUID";
        public const string IndexNamePlatformFolderParentGuid = "IDX_FOLDER_PARENTGUID";

        //Extension Archive 表名
        public const string TableNameArchiveHead = "TB_HEAD_INDEX";
        public const string TableNameArchiveBody = "TB_BODY_INDEX";
        public const string TableNameArchiveSiteMaster = "TB_SITE_MASTER_INDEX";
        public const string TableNameArchiveSiteInfo = "TB_SITE_INFO";
        public const string TableNameArchiveIndexInfo = "TB_MASTER_INDEX_INFO";
        public const string TableNameArchiveJobInfo = "TB_JOB_INFO";
        public const string TableNameArchiveSiteConfiguration = "TB_SITE_CONFIGURATION";
        public const string TableNameCommonSiteMaster = "TB_COMMON_SITE_MASTER_INDEX";

        //Extension Vault 表名
        public const string TableNameVaultHead = "TB_HEAD_INDEX";
        public const string TableNameVaultBody = "TB_BODY_INDEX";
        public const string TableNameVaultSiteMaster = "TB_SITE_MASTER_INDEX";
        public const string TableNameVaultSiteInfo = "TB_SITE_INFO";
        public const string TableNameVaultJobInfo = "TB_JOB_INFO";
        public const string TableNameVaultSiteConfiguration = "TB_SITE_CONFIGURATION";

        //Solution tableName
        public const string TableNameSolutionHead = "TB_HEAD_INDEX";
        public const string TableNameSolutionJobInfo = "TB_JOB_INFO";

        //General Table Name
        public const string TableNameGeneralItem = "TB_ITEM_INDEX";
        public const string TableNameGeneralSiteMaster = "TB_SITE_MASTER_INDEX";
        public const string TableNameGeneralJobInfo = "TB_JOB_INFO";

        //ExchangeOnline Table Name
        public const string TableNameExchangeAgent = "TB_AGENT_INDEX";
        public const string TableNameExchangeContainer = "TB_CONTAINER_INDEX";
        public const string TableNameExchangeItem = "TB_ITEM_INDEX";
        public const string TableNameExchangeSiteMaster = "TB_MASTER_INDEX";
        public const string TableNameExchangeJobInfo = "TB_JOB_INFO";
        public const string TableNameExchangeDataMd5 = "TB_DATAMD5_INDEX";
        public const string TableNameExchangePlanner = "TB_PLANNER_INDEX";

        //Google Drive Table Name
        public const string TableNameGDriveAgent = "TB_AGENT_INDEX";
        public const string TableNameGDriveMaster = "TB_MASTER_INDEX";
        public const string TableNameGDriveContainer = "TB_CONTAINER_INDEX";
        public const string TableNameGDriveItem = "TB_ITEM_INDEX";

        public const string SiteType = "E";
        public const string WebType = "W";
        public const string FolderType = "F";
        public const string ListType = "L";
        public const string DocumentType = "D";
        public const string ListItemType = "I";
        public const string ListItemAttachmentType = "A";
    }
}