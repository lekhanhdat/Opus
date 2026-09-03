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
    public class AveListFlags
    {
        public const long ORDERED_LIST = 0x0000000000000001;
        public const long UNDELETEABLE_LIST = 0x0000000000000004;
        public const long ATTACHMENTENABLE_LIST = 0x0000000000000008;

        public const long CATALOG_LIST = 0x0000000000000010;  
        public const long ASSOCIATED_WITH_MEETINGS_WORKSPACESITE_LIST = 0x0000000000000020;
        public const long VERSIONENABLE_LIST = 0x0000000000000080;

        public const long IMPLEMENT_INFRASTRUCTURE_LIST = 0x0000000000000100; 
        public const long MODERATIONENABLE_LIST =         0x0000000000000400;
        public const long ALLOWMULTIPLE_RESPONSES_LIST =  0x0000000000000800;

        public const long ONLY_BE_INSTANTIATIN_ROOTSITE_LIST = 0x0000000000004000;

        public const long NEED_CHECKOUT_BEFORE_MODIFY_LIST =   0x0000000000040000;
        public const long SUPPORT_CREATEDMINOR_VERSIONS_LIST = 0x0000000000080000;

        public const long ITEMS_VISIBLE_TO_ANYONE_LIST =  0x0000000002000000; 
        public const long WORKFLOWS_ASSOCIATEDWITH_LIST = 0x0000000004000000;

        public const long CREATION_FOLDER_BE_BLOCKED_LIST = 0x0000000020000000;  

        public const long RSSFEED_SYNDICATION_DISABLE_LIST = 0x0000004000000000;

        /// <summary>
        /// If this list is a survey,it will allow multiple responses for agiven user,
        /// rather than restricting users to single response.
        /// this flag Must be ignored for lists thar are not surveys.
        /// </summary>
        public static bool IsAllowMultipleResponses(long value)
        {
            return (value & ALLOWMULTIPLE_RESPONSES_LIST) != 0;
        }


        #region /*************************** Function of those bit is not clear **************************************/

        public const long PUBLIC_LIST = 0x0000000000000002;

        public const long SEND_ALERTS_WHEN_ASSIGNEDTO_USER_LIST = 0x0000000000000040; 

        public const long FILLOUTFORM_WHEN_USER_ISDENIED_LIST = 0x0000000000000200;

        public const long USES_FIELD_VALUE_WHEN_PRESENT_DATA_LIST = 0x0000000000001000; 
        public const long MUST_NOT_BE_SERIALIZED_AS_SITE_TEMPLATE_LIST = 0x0000000000002000;
        public const long ROOT_DOCUMENT_BE_SERIALIZED_LIST = 0x0000000000008000;

        public const long VIAEMAIL_INSERTION_ENABLE_LIST = 0x0000000000010000;
        public const long PRIVATE_LIST = 0x0000000000020000;

        public const long REQUIRES_EDITITEMRIGHT_TO_SEEMINORVERSION_LIST = 0x0000000000100000;
        public const long REQUIRES_APPROVEITEMSRIGHT_TO_SEEMINORVERSION_LIST = 0x0000000000200000;
        public const long DISPLAY_USER_INTERFACE_LIST = 0x0000000000400000;
        public const long HAS_SCHEMA_CUSTOMIZED_LIST = 0x0000000000800000;

        public const long DOCUMENT_GENERATE_THUMBNAIL_FILES_LIST = 0x0000000001000000; 
        public const long MUST_NOT_AUTO_EXPLORE_LIST = 0x0000000008000000;

        public const long CAN_OPEN_IN_BROWSER_LIST = 0x0000000010000000;  
        public const long DISALLOW_ADVANCE_VIEW_FUNCTIONALITY_LIST = 0x0000000040000000;
        public const long SPECIFIE_ORDERING_AVAILABLE_ON_PERBASIS_LIST = 0x0000000080000000;

        public const long MUSTNOT_BE_EXPLORED_AS_MIGRATION_PACKAGE_LSIT = 0x0000000100000000; 
        public const long HAVE_SCHEMA_CACHED_IN_MEMORY_LIST = 0x0000000200000000;
        public const long MUSTNOT_BE_PROCESSED_BY_SEARCHCRAWLER_LIST = 0x0000000800000000;

        public const long DATA_MUST_INCLUED_WHEN_SAVEAS_TEMPLATE_LIST = 0x0000001000000000; 
        public const long CONTENTTYPE_MANIPULATION_DISABLE_LIST = 0x0000002000000000;
        public const long IRM_ENABLE_LIST = 0x0000008000000000;

        public const long EXPIRATION_OF_IRM_ENABLE_LIST = 0x0000010000000000;
        public const long UNREGISTER_IRM_BE_BLOCKED_LIST = 0x0000020000000000;
        #endregion

        #region
        /// <summary>
        /// This List is a catalog(e.g. Web Part gallery, Master Page gallery, etc.).
        /// </summary>
        public static bool IsCataLogList(long value)
        {
            return (value & CATALOG_LIST) != 0;
        }

        /// <summary>
        /// this list is an "ordered list"(e.g a links list), and supports ordering and reordering of its items
        /// if the items in the can be ordered ,then the list is an "ordered list". 
        /// </summary>
        public static bool IsOrderedList(long value)
        {
            return (value & ORDERED_LIST) != 0;
        }

        /// <summary>
        /// this list is "undeletable"(e.g it is crucial to the functioning of the containing site or site collection);
        /// </summary>
        public static bool IsUndeletableList(long value)
        {
            return (value & UNDELETEABLE_LIST) != 0;
        }

        /// <summary>
        /// attachments on List Items are disabled.This bit must be set if the List is a Document Library.
        /// </summary>
        public static bool IsAttachmentsEnable(long value)
        {
            return (value & ATTACHMENTENABLE_LIST) != 0;
        }

        /// <summary>
        /// this list is associated with a site using the meetings workspace site template,and contains data scoped to each instance of a recurring meeting.
        /// </summary>
        public static bool IsAssociatedWithMeetingsWorkSpaceSite(long value)
        {
            return (value & ASSOCIATED_WITH_MEETINGS_WORKSPACESITE_LIST) != 0;
        }


        public static bool IsVersionEnable(long value)
        {
            return (value & VERSIONENABLE_LIST) != 0;
        }

        /// <summary>
        /// This list must be hidden from enumeration functions.This is intended for Lists implementing infrastucture for an application.
        /// e.g. Master Page Gallery,WorkSpacace Pages
        /// </summary>
        public static bool IsImplementInfrastructureList(long value)
        {
            return (value & IMPLEMENT_INFRASTRUCTURE_LIST) != 0;
        }

        /// <summary>
        /// This list has moderation enabled,requiring an approval process when content is created or modified.
        /// </summary>
        public static bool IsModerationEnable(long value)
        {
            return (value & MODERATIONENABLE_LIST) != 0;
        }        

        /// <summary>
        /// This list server template for this list can only be instantiated in the root site of a given site collection
        /// e.g. site template Gallery,solution Gallery
        /// </summary>
        public static bool IsOnlyBeInstantiatInRootSiteList(long value)
        {
            return (value & ONLY_BE_INSTANTIATIN_ROOTSITE_LIST) != 0;
        }

        /// <summary>
        /// This document library requires the user to check out documents before modifying them
        /// </summary>
        public static bool IsNeedCheckOutBeforeModifyList(long value)
        {
            return (value & NEED_CHECKOUT_BEFORE_MODIFY_LIST) != 0;
        }

        /// <summary>
        /// This list supports creation of minor versions on item revisions
        /// </summary>
        public static bool IsSupportCreatedMinorVersionsList(long value)
        {
            return (value & SUPPORT_CREATEDMINOR_VERSIONS_LIST) != 0;
        }

        /// <summary>
        /// List Items in this list are visible to anyone who has access to the list iteself,this is userful for shared resources such as
        /// the master page gallery,where onew page may be used throughout a site collection in scopes with varying permissions.
        /// </summary>
        public static bool IsItemsVisibleToAnyOneList(long value)
        {
            return (value & ITEMS_VISIBLE_TO_ANYONE_LIST) != 0;
        }

        /// <summary>
        ///  This list currently has workflows associated with it
        /// </summary>
        public static bool IsWorkflowsAssociatedWithList(long value)
        {
            return (value & WORKFLOWS_ASSOCIATEDWITH_LIST) != 0;
        }

        /// <summary>
        /// Creation of Folders must be blocked in this list
        /// </summary>
        public static bool IsCreationFolderBeBlockedList(long value)
        {
            return (value & CREATION_FOLDER_BE_BLOCKED_LIST) != 0;
        }

        /// <summary>
        /// RSS feed syndication is disabled for this list
        /// </summary>
        public static bool IsRSSFeedSyndicationDisableList(long value)
        {
            return (value & RSSFEED_SYNDICATION_DISABLE_LIST) != 0;
        }

        #endregion

        #region /************* Peroperty of the bit whose function is not clear ***************************/

        /// <summary>
        /// This List is a public list. This bit must be ignored.
        /// </summary>
        public static bool IsPublicList(long value)
        {
            return (value & PUBLIC_LIST) != 0;
        }

        /// <summary>
        /// This List must send alerts when a List Item is assigned to a User.
        /// </summary>
        public static bool IsAlertWhenAssignToUser(long value)
        {
            return (value & SEND_ALERTS_WHEN_ASSIGNEDTO_USER_LIST) != 0;
        }

        /// <summary>
        /// This List must send alerts when a List Item is assigned to a User.
        /// </summary>
        public static bool IsFillOutFormWhenUserIsDeniList(long value)
        {
            return (value & FILLOUTFORM_WHEN_USER_ISDENIED_LIST) != 0;
        }

        /// <summary>
        /// This List uses the value of each Field‘s ForcedDisplay attribute when,presenting data from that Field. 
        /// This is commonly used in anonymous,surveys to display common placeholder text wherever the,respondent‘s name would normally appear.
        /// </summary>
        public static bool IsUseFieldWhenPresentDatalist(long value)
        {
            return (value & USES_FIELD_VALUE_WHEN_PRESENT_DATA_LIST) != 0;
        }

        /// <summary>
        /// The List Server Template for this List can only be instantiated in the,Root Site of a given Site Collection.
        /// </summary>
        public static bool IsMustNotBeSerializedAsTemplate(long value)
        {
            return (value & MUST_NOT_BE_SERIALIZED_AS_SITE_TEMPLATE_LIST) != 0;
        }

        /// <summary>
        /// When a List Server Template is being created for this List, Documents,in the root of the List can also be serialized.
        /// </summary>
        public static bool IsRootDocumentBeSerializedList(long value)
        {
            return (value & ROOT_DOCUMENT_BE_SERIALIZED_LIST) != 0;
        }

        /// <summary>
        /// Insertion of List Items via email is enabled for this List.
        /// </summary>
        public static bool IsViaEmailInsertionEnableList(long value)
        {
            return (value & VIAEMAIL_INSERTION_ENABLE_LIST) != 0;
        }

        /// <summary>
        /// This is a private List. When a List Server Template based on this List is created, 
        /// the new List can be given an ACL so that only its,owner and administrators can access the List.
        /// </summary>
        public static bool IsPrivateList(long value)
        {
            return (value & PRIVATE_LIST) != 0;
        }

        /// <summary>
        /// This List requires Users have the EditListItems right to see minor versions of Documents.
        /// </summary>
        public static bool IsRequeiresEditItemRightToSeeMinorversionList(long value)
        {
            return (value & REQUIRES_EDITITEMRIGHT_TO_SEEMINORVERSION_LIST) != 0;
        }

        /// <summary>
        /// This List requires Users have the ApproveItems right to see minor versions of Documents.
        /// </summary>
        public static bool IsRequiresApproveItemRightToSeeMinorVersionList(long value)
        {
            return (value & REQUIRES_APPROVEITEMSRIGHT_TO_SEEMINORVERSION_LIST) != 0;
        }


        /// <summary>
        /// The WFE displays a user interface for manipulating multiple Content Types (for example, a List that contains both announcements and tasks).
        /// </summary>
        public static bool IsDisplayUserInterfaceList(long value)
        {
            return (value & DISPLAY_USER_INTERFACE_LIST) != 0;
        }


        /// <summary>
        /// This List has had its schema customized from the version that exists in the on-disk schema file that was used to create it.
        /// </summary>
        public static bool IsHasSchemaCustomizedList(long value)
        {
            return (value & HAS_SCHEMA_CUSTOMIZED_LIST) != 0;
        }

        /// <summary>
        /// Document parsers in this List generate thumbnail files corresponding to Documents saved to this List. 
        /// This bit MUST be ignored for Lists which are not Document Libraries.
        /// </summary>
        public static bool IsDocumentGenerateThumbnailFilesList(long value)
        {
            return (value & DOCUMENT_GENERATE_THUMBNAIL_FILES_LIST) != 0;
        }



        /// <summary>
        /// This List must not be automatically exported when exporting a List that references it. 
        /// Exporting is an implementation-specific capability of Windows SharePoint Services.
        /// </summary>
        public static bool IsMustNotAutoExploreList(long value)
        {
            return (value & MUST_NOT_AUTO_EXPLORE_LIST) != 0;
        }

        /// <summary>
        /// Applications generating server transformations of List Items in this List can choose to open the List Item in a browser 
        /// rather than in a separate client-side application. 
        /// Server transformations are performed by server-side Document viewers that can allow clients to view Documents 
        /// without additional client software. Server transformations are an implementation-specific capability of Windows SharePoint Services.
        /// </summary>
        public static bool IsCanOpenInBrowserList(long value)
        {
            return (value & CAN_OPEN_IN_BROWSER_LIST) != 0;
        }

        /// <summary>
        /// This List disallows advanced View functionality, such as the datasheet View and Views involving Web Part to Web Part connections.
        /// </summary>
        public static bool IsDisallowAdvanceViewFunctionalityList(long value)
        {
            return (value & DISALLOW_ADVANCE_VIEW_FUNCTIONALITY_LIST) != 0;
        }

        /// <summary>
        /// This List specifies custom sorting orders for the list of Content Types available on a per-Folder basis.
        /// </summary>
        public static bool IsSpecifieOrderAvailableOnPerBasisList(long value)
        {
            return (value & SPECIFIE_ORDERING_AVAILABLE_ON_PERBASIS_LIST) != 0;
        }

        /// <summary>
        /// This List MUST NOT be exported as part of a migration package.
        /// Migration packages are an implementation-specific capability of Windows SharePoint Services.
        /// </summary>
        public static bool IsMustNotBeExploredAsMigrationPackageList(long value)
        {
            return (value & MUSTNOT_BE_EXPLORED_AS_MIGRATION_PACKAGE_LSIT) != 0;
        }

        /// <summary>
        /// This List can have its schema cached in memory when possible, rather than retrieving the schema every time the List is accessed.
        /// </summary>
        public static bool IsHaveSchemaCachedInMemoryList(long value)
        {
            return (value & HAVE_SCHEMA_CACHED_IN_MEMORY_LIST) != 0;
        }

        /// <summary>
        /// This List MUST NOT be processed by a search crawler.
        /// </summary>
        public static bool IsMustNotBeProcessedBySearch_List(long value)
        {
            return (value & MUSTNOT_BE_PROCESSED_BY_SEARCHCRAWLER_LIST) != 0;
        }

        /// <summary>
        /// Data from this List MUST always be included when it is saved as a List Server Template, even if not otherwise requested.
        /// </summary>
        public static bool IsDataMustIncludedWhenSaveAsTemplateList(long value)
        {
            return (value & DATA_MUST_INCLUED_WHEN_SAVEAS_TEMPLATE_LIST) != 0;
        }

        /// <summary>
        /// Content Type manipulation is disabled on this List.
        /// </summary>
        public static bool IsCotentTypeManipulationDisableList(long value)
        {
            return (value & CONTENTTYPE_MANIPULATION_DISABLE_LIST) != 0;
        }

        /// <summary>
        /// Information Rights Management (IRM) is enabled for this Document Library.
        /// </summary>
        public static bool IsIRMEnableList(long value)
        {
            return (value & IRM_ENABLE_LIST) != 0;
        }

        /// <summary>
        /// Expiration of IRM rights is enabled for this Document Library. Setting this bit requires the IRM enabled bit also be set.
        /// </summary>
        public static bool IsExpirationOfIRMEnableList(long value)
        {
            return (value & EXPIRATION_OF_IRM_ENABLE_LIST) != 0;
        }

        /// <summary>
        /// Documents that do not have a registered IRM protector will be blocked from this Document Library
        /// </summary>
        public static bool IsUnregisterIRMBeBlockedList(long value)
        {
            return (value & UNREGISTER_IRM_BE_BLOCKED_LIST) != 0;
        }

        #endregion
    }

}
