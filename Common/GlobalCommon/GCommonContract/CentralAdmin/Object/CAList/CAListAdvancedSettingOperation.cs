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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListAdvancedSettingOperation : CAOperation
    {
        [DataMember]
        public bool IsMeettingTemplate { get; set; }

        [DataMember]
        public int ListTemplateType { get; set; }

        /// <summary>
        /// 当从SP取出来的ListTemplateType值超出了契约中定义
        /// 的枚举范围之后需要使用此值存储从SP取出来的值
        /// </summary>
        [DataMember]
        public int ListTemplateTypeIntValue { get; set; }

        [DataMember]
        public BaseType BaseType { get; set; }

        [DataMember]
        public bool IsLibrary { get; set; }

        /// <summary>
        ///     is content type enable avisoble
        /// </summary>
        [DataMember]
        public bool IsContentTypesVisible { get; set; }

        // Summary:
        //     Share List Items Across All Meetings (Series Items) , used in meeting template site
        [DataMember]
        public bool ISSeriesItems { get; set; }

        [DataMember]
        public bool CanCreateItem { get; set; }

        // Summary:
        //     Offline Client Availability
        [DataMember]
        public bool ExcludeFromOfflineClient { get; set; }

        [DataMember]
        public bool IsSiteAssetsLibrary { get; set; }

        // Summary:
        //     Datasheet
        [DataMember]
        public bool DisableGridEditing { get; set; }

        // Summary:
        //     Dialogs Content Types 
        [DataMember]
        public bool NavigateForFormsPages { get; set; }

        // Summary:
        //     Content Types 
        [DataMember]
        public bool ContentTypesEnabled { get; set; }

        // Summary:
        //     Folders 
        [DataMember]
        public bool EnableFolderCreation { get; set; }

        // Summary:
        //     Document Template 
        [DataMember]
        public string DocumentTemplateUrl { get; set; }

        // Summary:
        //     Opening Documents in the Browser 
        //     0  : Open in the client application 
        //     1  : Open in the browser 
        //     2  : Use the server default (Open in the browser)
        [DataMember]
        public int DefaultItemOpenUseListSetting { get; set; }

        // Summary:
        //     Custom Send To Destination 
        [DataMember]
        public string SendToLocationName { get; set; }
        [DataMember]
        public string SendToLocationUrl { get; set; }

        [DataMember]
        public bool EventHandlersEnabled { get; set; }

        [DataMember]
        public string EventSinkAssembly { get; set; }

        [DataMember]
        public string EventSinkClass { get; set; }

        [DataMember]
        public string EventSinkData { get; set; }


        // Summary:
        //     A 32-bit integer that indicates the Read security setting. Possible values
        //     include the following: 1 - All users have Read access to all items. 2 - Users
        //     have Read access only to items that they create.
        //     
        //      if the value is 0, the list have no such settings

        [DataMember]
        public int ReadSecurity { get; set; }

        // Summary:
        //     A 32-bit integer that specifies the Write security setting. Possible values
        //     include the following: 1 — All users can modify all items. 2 — Users can
        //     modify only items that they create. 4 — Users cannot modify any list item.
        //     
        //      if the value is 0, the list have no such settings
        [DataMember]
        public int WriteSecurity { get; set; }

        [DataMember]
        public bool EnableAssignToEmail { get; set; }

        [DataMember]
        public bool ISOutEmailSetting { get; set; }

        // Summary:
        //   Attachments 
        [DataMember]
        public bool EnableAttachments { get; set; }

        // Summary:
        //   Search
        [DataMember]
        public bool NoCrawl { get; set; }

        [DataMember]
        public bool IsDefaultView { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public bool IndexNonDefaultViews { get; set; }

        [DataMember]
        public bool QuickEdit { get; set; }

        [DataMember]
        public bool ReIndex { get; set; }

        [DataMember]
        public bool AutomaticIndexManagement { get; set; }

        [DataMember]
        public int ListExperience { get; set; }

        [DataMember]
        public Visible Visible { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Visible
    {
        [DataMember]
        public bool ContentTypeSectionVisible { get; set; }

        [DataMember]
        public bool ItemLevelSecurityPanelVisible { get; set; }

        [DataMember]
        public bool SendToSectionVisible { get; set; }

        [DataMember]
        public bool TasksIssuesEmailSettingsSectionVisible { get; set; }

        [DataMember]
        public bool OpenDocumentSectionVisible { get; set; }

        [DataMember]
        public bool FolderCreationSectionVisible { get; set; }

        [DataMember]
        public bool AllowCrawlSectionVisible { get; set; }

        [DataMember]
        public bool AllowNonDefaultSectionVisible { get; set; }

        [DataMember]
        public bool AllowReindexBTNVisible { get; set; }

        [DataMember]
        public bool QuickEditSectionVisible { get; set; }

        [DataMember]
        public bool AutomaticIndexManagementSectionVisible { get; set; }

        [DataMember]
        public bool ListExperienceSectionVisible { get; set; }

        [DataMember]
        public bool AllowSyncSectionVisible { get; set; }

        [DataMember]
        public bool AttachmentLibrarySectionVisible { get; set; }

        [DataMember]
        public bool DocumentTemplateSectionVisible { get; set; }

        [DataMember]
        public bool DialogSectionVisible { get; set; }

        [DataMember]
        public bool AttachmentSectionVisible { get; set; }

        [DataMember]
        public bool GridEditSectionVisible { get; set; }
    }
}
