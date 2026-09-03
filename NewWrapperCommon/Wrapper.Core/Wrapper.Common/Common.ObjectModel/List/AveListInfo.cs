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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 只需要创建List的基本信息，就可以。
    /// </summary>
    public class AveListInfo
    {
        public string Title;
        public string Description;
        public string Url;
        public string ServerRelativeUrl;

        public Guid Id;
        public string DocTemplateType;
        public int QuickLaunchOptions;

        public Guid TemplateFeatureId;
        public int BaseTemplate;
        public int BaseType;
        //public AveListSettingInfo ListSettingInfo;
        public string DataSourceXml;

        //add for filter list 
        public bool RootWebOnly;

        public string SrcUrl;
        public long Size;

        //Add for My Task of SP2013
        public string ListSchema;

        public bool IsNintexFormLibrary;
    }

    public class AveListSettingInfo
    {
        public AveRestorableProperty<long> PropertiesFlag;
        public AveRestorableProperty<bool> AllowContentTypes;
        public AveRestorableProperty<bool> AllowDeletion;
        public AveRestorableProperty<bool> AllowRssFeads;
        //public bool ApplicationList;
        //public bool AutoSaveEnabled;

        //public int BaseType;
        public AveRestorableProperty<bool> ContentTypesEnabled;
        public AveRestorableProperty<int> DefaultItemOpen;
        //public string Description;
        public AveRestorableProperty<bool> RequestAccessEnabled;
        public AveRestorableProperty<bool> EnableAssignToEmail;
        public AveRestorableProperty<bool> EnableAttachments;
        public AveRestorableProperty<bool> EnableDeployingList;
        public AveRestorableProperty<bool> EnableDeployWithDependentList;
        public AveRestorableProperty<bool> EnableFolderCreation;
        public AveRestorableProperty<bool> EnableMinorVersions;
        public AveRestorableProperty<bool> EnableModeration;
        public AveRestorableProperty<bool> EnablePeopleSelector;
        public AveRestorableProperty<bool> EnableResourceSelector;
        public AveRestorableProperty<bool> EnableSchemaCaching;
        public AveRestorableProperty<bool> EnableSyndication;
        public AveRestorableProperty<bool> EnableThrottling;
        public AveRestorableProperty<bool> EnableVersioning;
        public AveRestorableProperty<bool> EnforceDataValidation;
        public AveRestorableProperty<bool> ExcludeFromOfflineClient;
        public AveRestorableProperty<bool> ExcludeFromTemplate;
        public AveRestorableProperty<bool> ForceCheckout;
        public AveRestorableProperty<bool> HasUniqueRoleAssigntments;
        public AveRestorableProperty<bool> Hidden;
        public AveRestorableProperty<bool> IrmEnabled;
        public AveRestorableProperty<bool> IrmExpire;
        public AveRestorableProperty<bool> IrmReject;
        public AveRestorableProperty<bool> IsThrottled;
        public AveRestorableProperty<bool> OnQuickLaunch;
        public AveRestorableProperty<bool> NoCrawl;
        public AveRestorableProperty<bool> MultipleDataList;
        public AveRestorableProperty<bool> Ordered;

        public AveRestorableProperty<string> SendToLocationName;
        public AveRestorableProperty<string> SendToLocationUrl;
        public AveRestorableProperty<bool> IsSiteAssetsLibrary;
        public AveRestorableProperty<bool> DisableGridEditing;
        public AveRestorableProperty<bool> NavigateForFormsPages;
        public AveRestorableProperty<bool> EnableManagedIndexes;
        public AveRestorableProperty<ulong> AnonymousPermMask64;

        public AveRestorableProperty<string> ValidationMessage;
        //public string Title;
        public AveRestorableProperty<bool> ServerTemplateCanCreateFolders;
        public AveRestorableProperty<int> DraftVersionVisibility;
        //public Guid Id;
        public AveRestorableProperty<string> DefaultView;
        // public string ServerRelativeUrl;
        public AveRestorableProperty<string> ValidationFormula;
        //public long Flags;
        public AveRestorableProperty<AveListRootFolderInfo> RootFolderInfo;
        public AveRestorableProperty<bool> IsTaxonomyHiddenList;
        public AveRestorableProperty<bool> AllowMultiResponses;
        //add for list rating setting
        public AveRestorableProperty<bool> AllowRatingSetting;
        public AveRestorableProperty<int> RatingSettingType;
        //add for version B
        //public bool IsAttachmentLibrary;
        public AveRestorableProperty<bool> EnableAudienceSetting;
        public AveRestorableProperty<bool> EnableMetaPublish;
        //for document library Document Template in advance setting
        public AveRestorableProperty<string> DocumentTemplateUrl;
        //public string WebId; //[uniqueidentifier] NOT NULL,
        public AveRestorableProperty<Guid> Id; //[uniqueidentifier] NOT NULL,
        public AveRestorableProperty<string> Title; //[nvarchar](255) NOT NULL,
        public AveRestorableProperty<DateTime> Created; //[datetime] NOT NULL,
        public AveRestorableProperty<DateTime> LastSecurityChange; //[datetime] NOT NULL,
        public AveRestorableProperty<int> Version; //[int] NOT NULL,
        public AveRestorableProperty<Nullable<int>> Author = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<int> BaseType; //[int] NOT NULL,
        public AveRestorableProperty<Nullable<Guid>> FeatureId = new Nullable<Guid>(); //[uniqueidentifier] NULL,
        public AveRestorableProperty<int> ServerTemplate; //[int] NOT NULL,
        public AveRestorableProperty<Guid> RootFolder; //[uniqueidentifier] NOT NULL,
        public AveRestorableProperty<Nullable<Guid>> Template = new Nullable<Guid>(); //[uniqueidentifier] NULL,
        public AveRestorableProperty<string> ImageUrl; //[nvarchar](255) NOT NULL,
        public AveRestorableProperty<int> ReadSecurity; //[int] NOT NULL,
        public AveRestorableProperty<int> WriteSecurity; //[int] NOT NULL,
        public AveRestorableProperty<bool> Subscribed; //[bit] NOT NULL,
        public AveRestorableProperty<Nullable<int>> Direction = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<long> Flags; //[bigint] NOT NULL,
        public AveRestorableProperty<Nullable<int>> ThumbnailSize = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<Nullable<int>> WebImageWidth = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<Nullable<int>> WebImageHeight = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<string> Description; //[nvarchar](max) NOT NULL,
        // public string EmailInsertsFolder; //[nvarchar](255) NULL,
        // public string EmailInsertsLast    SyncTime; //[nvarchar](50) NULL,
        public AveRestorableProperty<string> EmailAlias; //[nvarchar](128) NULL,
        //public string DeleteTransactionId; //[varbinary](16) NOT NULL,
        public AveRestorableProperty<Guid> ScopeId; //[uniqueidentifier] NOT NULL,
        public AveRestorableProperty<bool> HasFGP; //[bit] NOT NULL,
        public AveRestorableProperty<bool> HasInternalFGP; //[bit] NOT NULL,
        public AveRestorableProperty<string> EventSinkAssembly; //[nvarchar](255) NULL,
        public AveRestorableProperty<string> EventSinkClass; //[nvarchar](255) NULL,
        public AveRestorableProperty<string> EventSinkData; //[nvarchar](255) NULL,
        //07DB中这个column是int类型，但是10和13这个column是tinyint，为了兼容07的数据，将其改成int
        public AveRestorableProperty<int> MaxRowOrdinal; //[tinyint] NOT NULL,
        public AveRestorableProperty<byte[]> Fields; //[dbo].[tCompressedString] NULL,
        public AveRestorableProperty<byte[]> ContentTypes; //[dbo].[tCompressedString] NULL,
        public AveRestorableProperty<Nullable<int>> AuditFlags = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<Nullable<int>> InheritAuditFlags = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<string> SendToLocation; //[nvarchar](512) NULL,
        public AveRestorableProperty<int> ListDataDirty; //[int] NOT NULL,
        public AveRestorableProperty<Nullable<Guid>> CacheParseId = new Nullable<Guid>(); //[uniqueidentifier] NULL,
        public AveRestorableProperty<Nullable<int>> MaxMajorVersionCount = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<Nullable<int>> MaxMajorwithMinorVersionCount = new Nullable<int>(); //[int] NULL,
        public AveRestorableProperty<Nullable<Guid>> DefaultWorkflowId = new Nullable<Guid>(); //[uniqueidentifier] NULL,
        public AveRestorableProperty<bool> NoThrottleListOperations; //[bit] NOT NULL,
        public AveRestorableProperty<int> ListSchemaVersion; //[int] NOT NULL
        //DOC-59469 add property for survey list
        public AveRestorableProperty<bool> ShowUser = true;// true is default value
        public AveRestorableProperty<string> RssViewField;
        public AveRestorableProperty<bool> EnterPriseKeyWordsEnable;
        public AveRestorableProperty<bool> ScheduledItemSetting;
        public AveRestorableProperty<bool> WorkflowsAssociated;
        public AveRestorableProperty<DateTime> LastModifiedTime;

        #region User Resource, Add in 6.4
        public AveRestorableProperty<AveUserResourceInfo> TitleResource;
        public AveRestorableProperty<AveUserResourceInfo> DescriptionResource;
        #endregion 

        public AveRestorableProperty<int> ListExperienceOptions;
        public AveRestorableProperty<AveComplianceTagInfo> ComplianceTag;
        //16 new add field
        public AveRestorableProperty<long> Flags2; //[bigint]  NULL,
        public AveRestorableProperty<bool> CrawlNonDefaultViews;

    }

    public class AveAllListsTableColumnValue
    {
        public const int  MaxMajorVersionCount = 0; //[int] NULL,
        public const int  MaxMajorwithMinorVersionCount = 0; //[int] NULL,
    }

    public class AveListMetaDateSettingInfo
    {
        public const string AssembleName = "Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string UpdateName = "TaxonomyItemUpdatedEventReceiver";
        public const string AddedName = "TaxonomyItemAddedAsyncEventReceiver";
    }

    public class AveListBrowserInfo
    {
        public Guid ID;
        public string Title;
        public string ServerRelativeUrl;
        public string Url;
        public string WebServerRelativeUrl;
        public string Name;
        public int BaseTemplate;
        public int BaseType;
        public bool Hidden;
        public bool HasUniqueRoleAssignments;
        public bool EnableFolderCreation;
        public string rootFolderName;
    }

    public enum AveRatingSettingType
    {
        None,
        Likes,
        Ratings
    }

    public class AveListRestoreBasicInfo
    {
        public RestoreObjectResult Status{get;set;}
    }
}
