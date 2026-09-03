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
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    public enum AveDateTimeFieldFormatType
    {
        DateOnly,
        DateTime
    }

    public enum AveChoiceFormatType
    {
        Dropdown,
        RadioButtons,
    }

    public enum AveModerationStatusType
    {
        Approved,
        Denied,
        Pending,
        Draft,
        Scheduled
    }

    public enum AveAppViewsPolicy
    {
        Unknown = 0,
        Disabled = 1,
        NotDisabled = 2
    }

    public enum AveSharingDomainRestrictionModes
    {
        None = 0,
        AllowList = 1,
        BlockList = 2
    }
    
    public enum AveRestrictedToRegion
    {
        NoRestriction = 0,
        BlockMoveOnly = 1,
        BlockFull = 2,
        Unknown = 3
    }    

    public enum AveFlowsPolicy
    {
        Unknown = 0,
        Disabled = 1,
        NotDisabled = 2
    }
    
    public enum AveCompanyWideSharingLinksPolicy
    {
        Unknown = 0,
        Disabled = 1,
        NotDisabled = 2
    }
    
    public enum AveSPOConditionalAccessPolicyType
    {
        Unknown = 0,
        Disabled = 1,
        Enabled = 2,
    }


    public enum AveDenyAddAndCustomizePagesStatus
    {
        Unknown = 0,
        Disabled = 1,
        Enabled = 2,
    }

    public enum AvePWAEnabledStatus
    {
        Unknown = 0,
        Disabled = 1,
        Enabled = 2,
    }

    public enum AveSandboxedCodeActivationCapabilities
    {
        Unknown = 0,
        Check = 1,
        Disabled = 2,
        Enabled = 3,
    }

    public enum AveSharingCapabilities
    {
        Disabled = 0,
        ExternalUserSharingOnly = 1,
        ExternalUserAndGuestSharing = 2,
        ExistingExternalUserSharingOnly = 3
    }

    [Serializable]
    public enum AveRelationshipDeleteBehavior
    {
        None,
        Cascade,
        Restrict
    }

    public enum AveNumberFormatTypes
    {
        Automatic = -1,
        FiveDecimals = 5,
        FourDecimals = 4,
        NoDecimal = 0,
        OneDecimal = 1,
        ThreeDecimals = 3,
        TwoDecimals = 2
    }

    public enum AveRichTextMode
    {
        Compatible,
        FullHtml,
        HtmlAsXml,
        ThemeHtml
    }

    public enum AveCalendarType
    {
        ChineseLunar = 15,
        Gregorian = 1,
        GregorianArabic = 10,
        GregorianMEFrench = 9,
        GregorianXLITEnglish = 11,
        GregorianXLITFrench = 12,
        Hebrew = 8,
        Hijri = 6,
        Japan = 3,
        Korea = 5,
        KoreaJapanLunar = 14,
        None = 0,
        SakaEra = 0x10,
        Taiwan = 4,
        Thai = 7
    }

    public enum AvePersonalizationScope
    {
        User,
        Shared
    }

    public enum AveNodeTypes
    {
        // Summary:
        //     Specifies no node types.
        None = 0,
        //
        // Summary:
        //     Specifies any type of Microsoft.SharePoint.SPWeb site.
        Area = 1,
        //
        // Summary:
        //     Specifies a List item in the Pages list.
        Page = 2,
        //
        // Summary:
        //     Specifies a Microsoft SharePoint Foundation list (SPList).
        List = 4,
        //
        // Summary:
        //     Specifies a Microsoft SharePoint Foundation list item (SPListItem).
        ListItem = 8,
        //
        // Summary:
        //     Specifies a CMS Page Layout.
        PageLayout = 16,
        //
        // Summary:
        //     Specifies a navigation heading.
        Heading = 32,
        //
        // Summary:
        //     Specifies an authored link that references a page.
        AuthoredLinkToPage = 64,
        //
        // Summary:
        //     Specifies an authored link that references a Web site or area.
        AuthoredLinkToWeb = 128,
        //
        // Summary:
        //     Specifies a generic authored link.
        AuthoredLinkPlain = 256,
        //
        // Summary:
        //     Specifies any type of authored link.
        AuthoredLink = 448,
        //
        // Summary:
        //     Specifies a combination of Area, Page, Heading and AuthoredLink. Navigation
        //     uses this value to determine which node types to return by default.
        Default = 483,
        //
        // Summary:
        //     Specifies a custom node type that may be useful for extensibility purposes.
        Custom = 512,
        //
        // Summary:
        //     Specifies all node types, including Area, Page, List, ListItem, PageLayout,
        //     Heading, AuthoredLink, and Custom.
        All = 1023,
        //
        Error = 1024,
    }

    public enum AveOpenBinaryOptions
    {
        None = 0,
        SkipVirusScan = 4,
        Unprotected = 2
    }

    public enum AveAuditMaskType
    {
        All = -1,
        CheckIn = 2,
        CheckOut = 1,
        ChildDelete = 0x40,
        Copy = 0x800,
        Delete = 8,
        Move = 0x1000,
        None = 0,
        ProfileChange = 0x20,
        SchemaChange = 0x80,
        Search = 0x2000,
        SecurityChange = 0x100,
        Undelete = 0x200,
        Update = 0x10,
        View = 4,
        Workflow = 0x400
    }

    [Serializable]
    public enum AveEventReceiverType
    {
        ContextEvent = 0x7ffe,
        EmailReceived = 0x4e20,
        FieldAdded = 0x2775,
        FieldAdding = 0x65,
        FieldDeleted = 0x2777,
        FieldDeleting = 0x67,
        FieldUpdated = 0x2776,
        FieldUpdating = 0x66,
        InvalidReceiver = -1,
        ItemAdded = 0x2711,
        ItemAdding = 1,
        ItemAttachmentAdded = 0x2717,
        ItemAttachmentAdding = 7,
        ItemAttachmentDeleted = 0x2718,
        ItemAttachmentDeleting = 8,
        ItemCheckedIn = 0x2714,
        ItemCheckedOut = 0x2715,
        ItemCheckingIn = 4,
        ItemCheckingOut = 5,
        ItemDeleted = 0x2713,
        ItemDeleting = 3,
        ItemFileConverted = 0x271a,
        ItemFileMoved = 0x2719,
        ItemFileMoving = 9,
        ItemUncheckedOut = 0x2716,
        ItemUncheckingOut = 6,
        ItemUpdated = 0x2712,
        ItemUpdating = 2,
        ItemVersionDeleted = 0x271b,
        ListAdded = 0x2778,
        ListAdding = 0x68,
        ListDeleted = 0x2779,
        ListDeleting = 0x69,
        SiteDeleted = 0x27d9,
        SiteDeleting = 0xc9,
        WebAdding = 0xcc,
        WebDeleted = 0x27da,
        WebDeleting = 0xca,
        WebMoved = 0x27db,
        WebMoving = 0xcb,
        WebProvisioned = 0x27dc,
        WorkflowCompleted = 0x2907,
        WorkflowPostponed = 0x2906,
        WorkflowStarted = 0x2905,
        WorkflowStarting = 0x1f5,
        GroupAdded = 0x283d,
        GroupAdding = 0x12d,
        GroupDeleted = 0x283f,
        GroupDeleting = 0x12f,
        GroupUpdated = 0x283e,
        GroupUpdating = 0x12e,
        GroupUserAdded = 0x2840,
        GroupUserAdding = 0x130,
        GroupUserDeleted = 0x2841,
        GroupUserDeleting = 0x131,
        InheritanceBreaking = 0x137,
        InheritanceBroken = 0x2847,
        InheritanceReset = 0x2848,
        InheritanceResetting = 0x138,
        RoleAssignmentAdded = 0x2845,
        RoleAssignmentAdding = 0x135,
        RoleAssignmentDeleted = 0x2846,
        RoleAssignmentDeleting = 0x136,
        RoleDefinitionAdded = 0x2842,
        RoleDefinitionAdding = 0x132,
        RoleDefinitionDeleted = 0x2844,
        RoleDefinitionDeleting = 0x134,
        RoleDefinitionUpdated = 0x2843,
        RoleDefinitionUpdating = 0x133
    }

    public enum AveAlertStatus
    {
        On,
        Off,
        Error
    }

    public sealed class AveBuiltInFieldId
    {
        // Fields
        public static readonly Guid _Author = new Guid("{246d0907-637c-46b7-9aa0-0bb914daa832}");
        public static readonly Guid _Category = new Guid("{0fc9cace-c5c2-465d-ae88-b67f2964ca93}");
        public static readonly Guid _CheckinComment = new Guid("{58014f77-5463-437b-ab67-eec79532da67}");
        public static readonly Guid _Comments = new Guid("{52578fc3-1f01-4f4d-b016-94ccbcf428cf}");
        public static readonly Guid _Contributor = new Guid("{370b7779-0344-4b9f-8f2d-dc1c62eae801}");
        public static readonly Guid _CopySource = new Guid("{6b4e226d-3d88-4a36-808d-a129bf52bccf}");
        public static readonly Guid _Coverage = new Guid("{3b1d59c0-26b1-4de6-abbd-3edb4e2c6eca}");
        public static readonly Guid _DCDateCreated = new Guid("{9f8b4ee0-84b7-42c6-a094-5cbde2115eb9}");
        public static readonly Guid _DCDateModified = new Guid("{810dbd02-bbf5-4c67-b1ce-5ad7c5a512b2}");
        public static readonly Guid _EditMenuTableEnd = new Guid("{2ea78cef-1bf9-4019-960a-02c41636cb47}");
        public static readonly Guid _EditMenuTableStart = new Guid("{3c6303be-e21f-4366-80d7-d6d0a3b22c7a}");
        public static readonly Guid _EditMenuTableStart2 = new Guid("{1344423c-c7f9-4134-88e4-ad842e2d723c}");
        public static readonly Guid _EndDate = new Guid("{8a121252-85a9-443d-8217-a1b57020fadf}");
        public static readonly Guid _Format = new Guid("{36111fdd-2c65-41ac-b7ef-48b9b8da4526}");
        public static readonly Guid _HasCopyDestinations = new Guid("{26d0756c-986a-48a7-af35-bf18ab85ff4a}");
        public static readonly Guid _Identifier = new Guid("{3c76805f-ad45-483a-9c85-7ac24506ce1a}");
        public static readonly Guid _IsCurrentVersion = new Guid("{c101c3e7-122d-4d4d-bc34-58e94a38c816}");
        public static readonly Guid _LastPrinted = new Guid("{b835f7c6-88a0-45d5-80c9-7ab4b2888b2b}");
        public static readonly Guid _Level = new Guid("{43bdd51b-3c5b-4e78-90a8-fb2087f71e70}");
        public static readonly Guid _ModerationComments = new Guid("{34ad21eb-75bd-4544-8c73-0e08330291fe}");
        public static readonly Guid _ModerationStatus = new Guid("{fdc3b2ed-5bf2-4835-a4bc-b885f3396a61}");
        public static readonly Guid _Photo = new Guid("{1020c8a0-837a-4f1b-baa1-e35aff6da169}");
        public static readonly Guid _Publisher = new Guid("{2eedd0ae-4281-4b77-99be-68f8b3ad8a7a}");
        public static readonly Guid _Relation = new Guid("{5e75c854-6e9d-405d-b6c1-f8725bae5822}");
        public static readonly Guid _ResourceType = new Guid("{edecec70-f6e2-4c3c-a4c7-f61a515dfaa9}");
        public static readonly Guid _Revision = new Guid("{16b4ab96-0ce5-4c82-a836-f3117e8996ff}");
        public static readonly Guid _RightsManagement = new Guid("{ada3f0cb-6f95-4588-bb08-d97cc0623522}");
        public static readonly Guid _SharedFileIndex = new Guid("{034998e9-bf1c-4288-bbbd-00eacfc64410}");
        public static readonly Guid _Source = new Guid("{b0a3c1db-faf1-48f0-9be1-47d2fc8cb5d6}");
        public static readonly Guid _SourceUrl = new Guid("{c63a459d-54ba-4ab7-933a-dcf1c6fadec2}");
        public static readonly Guid _Status = new Guid("{1dab9b48-2d1a-47b3-878c-8e84f0d211ba}");
        public static readonly Guid _UIVersion = new Guid("{7841bf41-43d0-4434-9f50-a673baef7631}");
        public static readonly Guid _UIVersionString = new Guid("{dce8262a-3ae9-45aa-aab4-83bd75fb738a}");
        public static readonly Guid _Version = new Guid("{78be84b9-d70c-447b-8275-8dcd768b6f92}");
        public static readonly Guid ActualWork = new Guid("{b0b3407e-1c33-40ed-a37c-2430b7a5d081}");
        public static readonly Guid AdminTaskAction = new Guid("{7b016ee5-70aa-4abb-8aa3-01795b4efe6f}");
        public static readonly Guid AdminTaskDescription = new Guid("{93490584-b6a8-4996-aa00-ead5f59aae0d}");
        public static readonly Guid AdminTaskOrder = new Guid("{cf935cc2-a00c-4ad3-bca1-0865ab15afc1}");
        public static readonly Guid AllowEditing = new Guid("{7266b59c-030b-4ca3-bc09-bb8e76ad969b}");
        public static readonly Guid AlternateThumbnailUrl = new Guid("{f39d44af-d3f3-4ae6-b43f-ac7330b5e9bd}");
        public static readonly Guid Anniversary = new Guid("{9d76802c-13c4-484a-9872-d7f9641c4672}");
        public static readonly Guid AssignedTo = new Guid("{53101f38-dd2e-458c-b245-0c236cc13d1a}");
        public static readonly Guid AssistantNumber = new Guid("{f55de332-074e-4e71-a71a-b90abfad51ae}");
        public static readonly Guid AssistantsName = new Guid("{2aea194d-e399-4f05-95af-94f87b1f2687}");
        public static readonly Guid AssociatedListId = new Guid("{b75067a2-e23b-499f-aa07-4ceb6c79e0b3}");
        public static readonly Guid Attachments = new Guid("{67df98f4-9dec-48ff-a553-29bece9c5bf4}");
        public static readonly Guid AttendeeStatus = new Guid("{3329f39d-70ed-4858-b8c8-c5237634bf08}");
        public static readonly Guid Author = new Guid("{1df5e554-ec7e-46a6-901d-d85a3881cb18}");
        public static readonly Guid BaseAssociationGuid = new Guid("{e9359d15-261b-48f6-a302-01419a68d4de}");
        public static readonly Guid BaseName = new Guid("{7615464b-559e-4302-b8e2-8f440b913101}");
        public static readonly Guid BillingInformation = new Guid("{4f03f66b-fb1e-4ed2-ab8e-f6ed3fe14844}");
        public static readonly Guid Birthday = new Guid("{c4c7d925-bc1b-4f37-826d-ac49b4fb1bc1}");
        public static readonly Guid Body = new Guid("{7662cd2c-f069-4dba-9e35-082cf976e170}");
        public static readonly Guid BodyAndMore = new Guid("{c7e9537e-bde4-4923-a100-adbd9e0a0a0d}");
        public static readonly Guid BodyWasExpanded = new Guid("{af82aa75-3039-4573-84a8-73ffdfd22733}");
        public static readonly Guid Break = new Guid("{9b12fb06-254e-43b3-bfc8-8eea422ebc9f}");
        public static readonly Guid Business2Number = new Guid("{6547d03a-76d3-4d74-9d34-f51b837c0879}");
        public static readonly Guid CallBack = new Guid("{274b7e21-284a-4c49-bec6-f1f2cb6fc344}");
        public static readonly Guid CallbackNumber = new Guid("{344e9657-b17f-4344-a834-ff7c056bcc5e}");
        public static readonly Guid CallTime = new Guid("{63fc6806-db53-4d0d-b18b-eaf90e96ddf5}");
        public static readonly Guid CarNumber = new Guid("{92a011a9-fd1b-42e0-b6fa-afcfee1928fa}");
        public static readonly Guid Categories = new Guid("{9ebcd900-9d05-46c8-8f4d-e46e87328844}");
        public static readonly Guid Category = new Guid("{6df9bd52-550e-4a30-bc31-a4366832a87d}");
        public static readonly Guid CellPhone = new Guid("{2a464df1-44c1-4851-949d-fcd270f0ccf2}");
        public static readonly Guid CheckoutUser = new Guid("{3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b}");
        public static readonly Guid ChildrensNames = new Guid("{6440b402-8ec5-4d7a-83f4-afccb556b5cc}");
        public static readonly Guid Combine = new Guid("{e52012a0-51eb-4c0c-8dfb-9b8a0ebedcb6}");
        public static readonly Guid Comment = new Guid("{6df9bd52-550e-4a30-bc31-a4366832a87f}");
        public static readonly Guid Comments = new Guid("{9da97a8a-1da5-4a77-98d3-4bc10456e700}");
        public static readonly Guid Company = new Guid("{038d1503-4629-40f6-adaf-b47d1ab2d4fe}");
        public static readonly Guid CompanyNumber = new Guid("{27cb1283-bda2-4ae8-bcff-71725b674dbb}");
        public static readonly Guid CompanyPhonetic = new Guid("{034aae88-6e9a-4e41-bc8a-09b6c15fcdf4}");
        public static readonly Guid Completed = new Guid("{35363960-d998-4aad-b7e8-058dfe2c669e}");
        public static readonly Guid ComputerNetworkName = new Guid("{86a78395-c8ad-429e-abff-be09417b523e}");
        public static readonly Guid Confidential = new Guid("{9b0e6471-c5c5-42ef-9ade-63170bf28819}");
        public static readonly Guid Confirmations = new Guid("{ef7465d3-5d54-487b-b081-ade80acae88e}");
        public static readonly Guid ConfirmedTo = new Guid("{1b89212c-1c67-487a-8c14-4d30bf4ef223}");
        public static readonly Guid ConnectionType = new Guid("{939dfb93-3107-44c6-a98f-dd88dca3f8cf}");
        public static readonly Guid ContactInfo = new Guid("{e1a85174-b8d0-4962-9ce6-758f8b612725}");
        public static readonly Guid Content = new Guid("{7650d41a-fa26-4c72-a641-af4e93dc7053}");
        public static readonly Guid ContentType = new Guid("{c042a256-787d-4a6f-8a8a-cf6ab767f12d}");
        public static readonly Guid ContentTypeId = new Guid("{03e45e84-1992-4d42-9116-26f756012634}");
        public static readonly Guid CorrectBodyToShow = new Guid("{b0204f69-2253-43d2-99ad-c0df00031b66}");
        public static readonly Guid Created = new Guid("{8c06beca-0777-48f7-91c7-6da68bc07b69}");
        public static readonly Guid Created_x0020_By = new Guid("{4dd7e525-8d6b-4cb4-9d3e-44ee25f973eb}");
        public static readonly Guid Created_x0020_Date = new Guid("{998b5cff-4a35-47a7-92f3-3914aa6aa4a2}");
        public static readonly Guid CustomerID = new Guid("{81368791-7cbc-4230-981a-a7669ade9801}");
        public static readonly Guid Data = new Guid("{38269294-165e-448a-a6b9-f0e09688f3f9}");
        public static readonly Guid Date = new Guid("{2139e5cc-6c75-4a65-b84c-00fe93027db3}");
        public static readonly Guid DateCompleted = new Guid("{24bfa3c2-e6a0-4651-80e9-3db44bf52147}");
        public static readonly Guid DayOfWeek = new Guid("{61fc45dd-b33d-4679-8646-be9e6584fadd}");
        public static readonly Guid DecisionStatus = new Guid("{ac3a1092-34ad-42b2-8d47-a79d01d9f516}");
        public static readonly Guid Deleted = new Guid("{4ed6dfdf-86a8-4894-bd1b-4fa28042be53}");
        public static readonly Guid Department = new Guid("{05fdf852-4b64-4096-9b2b-d2a62a86bc59}");
        public static readonly Guid Description = new Guid("{3f155110-a6a2-4d70-926c-94648101f0e8}");
        public static readonly Guid Detail = new Guid("{6529a881-d745-4117-a552-3dcc7110e9b8}");
        public static readonly Guid DiscussionLastUpdated = new Guid("{59956c56-30dd-4cb1-bf12-ef693b42679c}");
        public static readonly Guid DiscussionTitle = new Guid("{c5abfdc7-3435-4183-9207-3d1146895cf8}");
        public static readonly Guid DiscussionTitleLookup = new Guid("{f0218b98-d0d6-4fc1-b15b-aabeb89f32a9}");
        public static readonly Guid DLC_Description = new Guid("{2fd53156-ff9d-4cc3-b0ac-fe8a7bc82283}");
        public static readonly Guid DLC_Duration = new Guid("{80289bac-fd36-4848-b67a-bc8b5b621ec2}");
        public static readonly Guid DocIcon = new Guid("{081c6e4c-5c14-4f20-b23e-1a71ceb6a67c}");
        public static readonly Guid DueDate = new Guid("{c1e86ea6-7603-493c-ab5d-db4bbfe8f96a}");
        public static readonly Guid Duration = new Guid("{4d54445d-1c84-4a6d-b8db-a51ded4e1acc}");
        public static readonly Guid Edit = new Guid("{503f1caa-358e-4918-9094-4a2cdc4bc034}");
        public static readonly Guid Editor = new Guid("{d31655d1-1d5b-4511-95a1-7a09e9b75bf2}");
        public static readonly Guid EMail = new Guid("{fce16b4c-fe53-4793-aaab-b4892e736d15}");
        public static readonly Guid Email2 = new Guid("{e232d6c8-9f49-4be2-bb28-b90570bcf167}");
        public static readonly Guid Email3 = new Guid("{8bd27dbd-29a0-4ccd-bcb4-03fe70c538b1}");
        public static readonly Guid EmailBody = new Guid("{8cbb9252-1035-4156-9c35-f54e9056c65a}");
        public static readonly Guid EmailCalendarDateStamp = new Guid("{32f182ba-284e-4a87-93c3-936a6585af39}");
        public static readonly Guid EmailCalendarSequence = new Guid("{7a0cb12b-c70c-4f99-99f1-a232783a87d7}");
        public static readonly Guid EmailCalendarUid = new Guid("{f4e00567-8a9d-451b-82d4-a4447f9bd9a5}");
        public static readonly Guid EmailCc = new Guid("{a6af6df4-feb5-4dbf-bef6-d81230d4a071}");
        public static readonly Guid EmailFrom = new Guid("{e7cb6f60-f676-4b1d-89a3-975b6bc78cad}");
        public static readonly Guid EmailHeaders = new Guid("{e6985df4-cf66-4313-bcda-d89744d3b02f}");
        public static readonly Guid EmailReferences = new Guid("{124527a9-fc10-48ff-8d44-960a7db405f8}");
        public static readonly Guid EmailSender = new Guid("{4ce600fb-a927-4911-bfc1-11076b76b522}");
        public static readonly Guid EmailSubject = new Guid("{072e9bb6-a643-44ce-b6fb-8b574a792556}");
        public static readonly Guid EmailTo = new Guid("{caa2cb1e-a124-4068-9496-14feef1a901f}");
        public static readonly Guid EncodedAbsThumbnailUrl = new Guid("{b9e6f3ae-5632-4b13-b636-9d1a2bd67120}");
        public static readonly Guid EncodedAbsUrl = new Guid("{7177cfc7-f399-4d4d-905d-37dd51bc90bf}");
        public static readonly Guid EncodedAbsWebImgUrl = new Guid("{a1ca0063-779f-49f9-999c-a4a2e3645b07}");
        public static readonly Guid End = new Guid("{04b29608-b1e8-4ff9-90d5-5328096dd5ac}");
        public static readonly Guid EndDate = new Guid("{2684f9f2-54be-429f-ba06-76754fc056bf}");
        public static readonly Guid Event = new Guid("{20a1a5b1-fddf-4420-ac68-9701490e09af}");
        public static readonly Guid EventCanceled = new Guid("{b8bbe503-bb22-4237-8d9e-0587756a2176}");
        public static readonly Guid EventType = new Guid("{5d1d4e76-091a-4e03-ae83-6a59847731c0}");
        public static readonly Guid Expires = new Guid("{6a09e75b-8d17-4698-94a8-371eda1af1ac}");
        public static readonly Guid ExtendedProperties = new Guid("{1c5518e2-1e99-49fe-bfc6-1a8de3ba16e2}");
        public static readonly Guid Facilities = new Guid("{a4e7b3e1-1b0a-4ffa-8426-c94d4cb8cc57}");
        public static readonly Guid fAllDayEvent = new Guid("{7d95d1f4-f5fd-4a70-90cd-b35abc9b5bc8}");
        public static readonly Guid File_x0020_Size = new Guid("{8fca95c0-9b7d-456f-8dae-b41ee2728b85}");
        public static readonly Guid File_x0020_Type = new Guid("{39360f11-34cf-4356-9945-25c44e68dade}");
        public static readonly Guid FileDirRef = new Guid("{56605df6-8fa1-47e4-a04c-5b384d59609f}");
        public static readonly Guid FileLeafRef = new Guid("{8553196d-ec8d-4564-9861-3dbe931050c8}");
        public static readonly Guid FileRef = new Guid("{94f89715-e097-4e8b-ba79-ea02aa8b7adb}");
        public static readonly Guid FileSizeDisplay = new Guid("{78a07ba4-bda8-4357-9e0f-580d64487583}");
        public static readonly Guid FileType = new Guid("{c53a03f3-f930-4ef2-b166-e0f2210c13c0}");
        public static readonly Guid FirstName = new Guid("{4a722dd4-d406-4356-93f9-2550b8f50dd0}");
        public static readonly Guid FirstNamePhonetic = new Guid("{ea8f7ca9-2a0e-4a89-b8bf-c51a6af62c73}");
        public static readonly Guid FolderChildCount = new Guid("{960ff01f-2b6d-4f1b-9c3f-e19ad8927341}");
        public static readonly Guid FormData = new Guid("{78eae64a-f5f2-49af-b416-3247b76f46a1}");
        public static readonly Guid FormURN = new Guid("{17ca3a22-fdfe-46eb-99b5-9646baed3f16}");
        public static readonly Guid fRecurrence = new Guid("{f2e63656-135e-4f1c-8fc2-ccbe74071901}");
        public static readonly Guid FreeBusy = new Guid("{393003f9-6ccb-4ea9-9623-704aa4748dec}");
        public static readonly Guid From = new Guid("{4cd541b9-c8ee-468f-bee6-33f3b9baa722}");
        public static readonly Guid FSObjType = new Guid("{30bb605f-5bae-48fe-b4e3-1f81d9772af9}");
        public static readonly Guid FTPSite = new Guid("{d733736e-4204-4812-9565-191567b27e33}");
        public static readonly Guid FullBody = new Guid("{9c4be348-663a-4172-a38a-9714b2634c17}");
        public static readonly Guid FullName = new Guid("{475c2610-c157-4b91-9e2d-6855031b3538}");
        public static readonly Guid GbwCategory = new Guid("{7fc04acf-6b4f-418c-8dc5-ecfb0085bb51}");
        public static readonly Guid GbwLocation = new Guid("{afaa4198-9797-4e45-9825-8f7e7b0f5dd5}");
        public static readonly Guid Gender = new Guid("{23550288-91b5-4e7f-81f9-1a92661c4838}");
        public static readonly Guid GoFromHome = new Guid("{6570d35e-7f0a-4123-93c9-f53ffa5810d3}");
        public static readonly Guid GoingHome = new Guid("{2ead592e-f05c-41a2-9817-e06dac25bc19}");
        public static readonly Guid GovernmentIDNumber = new Guid("{da31d3c9-f9da-4c35-88d4-60aafa4c3f19}");
        public static readonly Guid Group = new Guid("{c86a2f7f-7680-4a0b-8907-39c4f4855a35}");
        public static readonly Guid GUID = new Guid("{ae069f25-3ac2-4256-b9c3-15dbc15da0e0}");
        public static readonly Guid HasCustomEmailBody = new Guid("{47f68c3b-8930-406f-bde2-4a8c669ee87c}");
        public static readonly Guid HealthReportCategory = new Guid("{a63505f2-f42c-4d94-b03b-78ba2c73d40e}");
        public static readonly Guid HealthReportExplanation = new Guid("{b4c8faec-5d60-49ee-a5fb-6165f5c3e6a9}");
        public static readonly Guid HealthReportRemedy = new Guid("{8aa22caa-8000-44c9-b343-a7705bbed863}");
        public static readonly Guid HealthReportServers = new Guid("{84a318aa-9035-4529-98b9-e08bb20a5da0}");
        public static readonly Guid HealthReportServices = new Guid("{e2b0b450-6795-4b86-86b7-3c21ab1797fb}");
        public static readonly Guid HealthReportSeverity = new Guid("{505423c5-f085-48b9-9432-12073d643ba5}");
        public static readonly Guid HealthReportSeverityIcon = new Guid("{89efcbd9-9796-41f0-b569-65325f1882dc}");
        public static readonly Guid HealthRuleAutoRepairEnabled = new Guid("{1e41a55e-ef71-4740-b65a-d11e24c1d00d}");
        public static readonly Guid HealthRuleCheckEnabled = new Guid("{7b2b1712-a73d-4ad7-a9d0-662f0291713d}");
        public static readonly Guid HealthRuleReportLink = new Guid("{cf4ff575-f1f5-4c5b-b595-54bbcccd0c62}");
        public static readonly Guid HealthRuleSchedule = new Guid("{26761ba3-729d-4bfc-9658-77b55e01f8d5}");
        public static readonly Guid HealthRuleScope = new Guid("{e59f08c9-fa34-4f94-a00a-f6458b1d3c56}");
        public static readonly Guid HealthRuleService = new Guid("{2d6e61d0-be31-460c-ab8b-77d8b369f517}");
        public static readonly Guid HealthRuleType = new Guid("{7dd0a092-8704-4ed2-8253-ac309150ac59}");
        public static readonly Guid HealthRuleVersion = new Guid("{6b6b1455-09ee-43b7-beea-4dc97456de2f}");
        public static readonly Guid Hobbies = new Guid("{203fa378-6eb8-4ed9-a4f9-221a4c1fbf46}");
        public static readonly Guid HolidayDate = new Guid("{335e22c3-b8a4-4234-9790-7a03eeb7b0d4}");
        public static readonly Guid HolidayNightWork = new Guid("{dc9100ec-251d-4e81-a6cb-d967a065ba24}");
        public static readonly Guid HolidayWork = new Guid("{b5a7350f-2716-46ca-9c42-66bb39d042ec}");
        public static readonly Guid Home2Number = new Guid("{8c5a385d-2fff-42da-a4c5-f6a904f2e491}");
        public static readonly Guid HomeAddressCity = new Guid("{5aeabc56-57c6-4861-bc12-bd72c30fc6bd}");
        public static readonly Guid HomeAddressCountry = new Guid("{897ecfd7-4293-4782-b463-bd68440a5fed}");
        public static readonly Guid HomeAddressPostalCode = new Guid("{c0e4b4c6-6245-4846-8561-b8c6c01fefc1}");
        public static readonly Guid HomeAddressStateOrProvince = new Guid("{f5b36006-69b0-418c-bd4a-f25ca7e096bb}");
        public static readonly Guid HomeAddressStreet = new Guid("{8c66e340-0985-4d68-af03-3050ece4862b}");
        public static readonly Guid HomeFaxNumber = new Guid("{c189a857-e6b0-488f-83a0-f4ee0a3ad01e}");
        public static readonly Guid HomePhone = new Guid("{2ab923eb-9880-4b47-9965-ebf93ae15487}");
        public static readonly Guid HTML_x0020_File_x0020_Type = new Guid("{0c5e0085-eb30-494b-9cdd-ece1d3c649a2}");
        public static readonly Guid IconOverlay = new Guid("{b77cdbcf-5dce-4937-85a7-9fc202705c91}");
        public static readonly Guid ID = new Guid("{1d22ea11-1e32-424e-89ab-9fedbadb6ce1}");
        public static readonly Guid IMAddress = new Guid("{4cbd96f7-09c6-4b5e-ad42-1cbe123de63a}");
        public static readonly Guid ImageCreateDate = new Guid("{a5d2f824-bc53-422e-87fd-765939d863a5}");
        public static readonly Guid ImageHeight = new Guid("{1944c034-d61b-42af-aa84-647f2e74ca70}");
        public static readonly Guid ImageSize = new Guid("{922551b8-c7e0-46a6-b7e3-3cf02917f68a}");
        public static readonly Guid ImageWidth = new Guid("{7e68a0f9-af76-404c-9613-6f82bc6dc28c}");
        public static readonly Guid IMEComment1 = new Guid("{d2433b20-3f02-4432-817d-369f104a2dcd}");
        public static readonly Guid IMEComment2 = new Guid("{e2c93917-cf32-4b29-be5c-d71f1bac7714}");
        public static readonly Guid IMEComment3 = new Guid("{7c52f61a-e1e0-4341-9e2f-9b36cddfdd7c}");
        public static readonly Guid IMEDisplay = new Guid("{90244050-709c-4837-9316-93863fbd3da6}");
        public static readonly Guid IMEPos = new Guid("{f3cdbcfd-f456-45f4-9000-b6f34bb95d84}");
        public static readonly Guid IMEUrl = new Guid("{84b0fe85-6b16-40c3-8507-e56c5bbc482e}");
        public static readonly Guid In = new Guid("{ee394fd4-4c11-4d8e-baff-83270c1921aa}");
        public static readonly Guid Indentation = new Guid("{26c4f53e-733a-4202-814b-377492b6c841}");
        public static readonly Guid IndentLevel = new Guid("{68227570-72dd-4816-b6b6-4b81ff99a393}");
        public static readonly Guid Initials = new Guid("{7a282f86-69d9-40ff-ae1c-c746cf21256b}");
        public static readonly Guid InstanceID = new Guid("{50a54da4-1528-4e67-954a-e2d24f1e9efb}");
        public static readonly Guid IsActive = new Guid("{af5036db-36f4-46c8-bde7-a677bd0ef280}");
        public static readonly Guid ISDNNumber = new Guid("{a579062a-6c1d-4ad3-9d5e-035f9f2c1882}");
        public static readonly Guid IsNonWorkingDay = new Guid("{baf7091c-01fb-4831-a975-08254f87f234}");
        public static readonly Guid IsRootPost = new Guid("{bd2216c1-a2f3-48c0-b21c-dc297d0cc658}");
        public static readonly Guid IsSiteAdmin = new Guid("{9ba260b2-85a1-4a32-ad7a-63eaceffe6b4}");
        public static readonly Guid IssueStatus = new Guid("{3f277a5c-c7ae-4bbe-9d44-0456fb548f94}");
        public static readonly Guid Item = new Guid("{92b8e9d0-a11b-418f-bf1c-c44aaa73075d}");
        public static readonly Guid ItemChildCount = new Guid("{b824e17e-a1b3-426e-aecf-f0184d900485}");
        public static readonly Guid JobTitle = new Guid("{c4e0f350-52cc-4ede-904c-dd71a3d11f7d}");
        public static readonly Guid Keywords = new Guid("{b66e9b50-a28e-469b-b1a0-af0e45486874}");
        public static readonly Guid Language = new Guid("{d81529e8-384c-4ca6-9c43-c86a256e6a44}");
        public static readonly Guid Last_x0020_Modified = new Guid("{173f76c8-aebd-446a-9bc9-769a2bd2c18f}");
        public static readonly Guid LastNamePhonetic = new Guid("{fdc8216d-dabf-441d-8ac0-f6c626fbdc24}");
        public static readonly Guid Late = new Guid("{df7f27a4-d87b-4a97-947b-13d1d4f7e6de}");
        public static readonly Guid LeaveEarly = new Guid("{a2a86efe-c28e-4dde-ab56-0afa31664bbc}");
        public static readonly Guid LessLink = new Guid("{076193bd-865b-4de7-9633-1f12069a6fff}");
        public static readonly Guid LimitedBody = new Guid("{61b97279-cbc0-4aa9-a362-f1ff249c1706}");
        public static readonly Guid LinkDiscussionTitle = new Guid("{46045bc4-283a-4826-b3dd-7a78d790b266}");
        public static readonly Guid LinkDiscussionTitle2 = new Guid("{b4e31c47-f962-4f9f-9132-eb555a1a026c}");
        public static readonly Guid LinkDiscussionTitleNoMenu = new Guid("{3ac9353f-613f-42bd-98e1-530e9fd1cbf6}");
        public static readonly Guid LinkFilename = new Guid("{5cc6dc79-3710-4374-b433-61cb4a686c12}");
        public static readonly Guid LinkFilenameNoMenu = new Guid("{9d30f126-ba48-446b-b8f9-83745f322ebe}");
        public static readonly Guid LinkIssueIDNoMenu = new Guid("{03f89857-27c9-4b58-aaab-620647deda9b}");
        public static readonly Guid LinkTitle = new Guid("{82642ec8-ef9b-478f-acf9-31f7d45fbc31}");
        public static readonly Guid LinkTitleNoMenu = new Guid("{bc91a437-52e7-49e1-8c4e-4698904b2b6d}");
        public static readonly Guid List = new Guid("{f44e428b-61c8-4100-a911-a3a635f43bb5}");
        public static readonly Guid ListType = new Guid("{81dde544-1e25-4765-b5fd-ba613198d850}");
        public static readonly Guid Location = new Guid("{288f5f32-8462-4175-8f09-dd7ba29359a9}");
        public static readonly Guid ManagersName = new Guid("{ba934502-d68d-4960-a54b-51e15fef5fd3}");
        public static readonly Guid MasterSeriesItemID = new Guid("{9b2bed84-7769-40e3-9b1d-7954a4053834}");
        public static readonly Guid MessageBody = new Guid("{fbba993f-afee-4e00-b9be-36bc660dcdd1}");
        public static readonly Guid MessageId = new Guid("{2ef29342-2f5f-4052-90d3-8192e0705e51}");
        public static readonly Guid MetaInfo = new Guid("{687c7f94-686a-42d3-9b67-2782eac4b4f8}");
        public static readonly Guid MiddleName = new Guid("{418c8d29-6f2e-44c3-8955-2cd7ec3e2151}");
        public static readonly Guid Mileage = new Guid("{3126c2f1-063e-4892-828f-0696ec6e105f}");
        public static readonly Guid MobileContent = new Guid("{53a2a512-d395-4852-8714-d4c27e7585f3}");
        public static readonly Guid MobilePhone = new Guid("{bf03d3ca-aa6e-4845-809a-b4378b37ce08}");
        public static readonly Guid Modified = new Guid("{28cf69c5-fa48-462a-b5cd-27b6f9d2bd5f}");
        public static readonly Guid Modified_x0020_By = new Guid("{822c78e3-1ea9-4943-b449-57863ad33ca9}");
        public static readonly Guid MoreLink = new Guid("{fb6c2494-1b14-49b0-a7ca-0506d6e85a62}");
        public static readonly Guid MyEditor = new Guid("{078b9dba-eb8c-4ec5-bfdd-8d220a3fcc5d}");
        public static readonly Guid Name = new Guid("{bfc6f32c-668c-43c4-a903-847cca2f9b3c}");
        public static readonly Guid NameOrTitle = new Guid("{76d1cc87-56de-432c-8a2a-16e5ba5331b3}");
        public static readonly Guid Nickname = new Guid("{6b0a2cd7-a7f9-41ca-b932-f3bebb603793}");
        public static readonly Guid NightWork = new Guid("{aaa68c08-6276-4337-9bce-b9cd852c7328}");
        public static readonly Guid NoCodeVisibility = new Guid("{a05a8639-088a-4aea-b8a9-afc888971c81}");
        public static readonly Guid Notes = new Guid("{e241f186-9b94-415c-9f66-255ce7f86235}");
        public static readonly Guid NumberOfVacation = new Guid("{44e16d52-da1b-4e72-8bdb-89a3b77ec8b0}");
        public static readonly Guid Occurred = new Guid("{5602dc33-a60a-4dec-bd23-d18dfcef861d}");
        public static readonly Guid Office = new Guid("{26169ab2-4bd2-4870-b077-10f49c8a5822}");
        public static readonly Guid OffsiteParticipant = new Guid("{16b6952f-3ce6-45e0-8f4e-42dac6e12441}");
        public static readonly Guid OffsiteParticipantReason = new Guid("{4a799ba5-f449-4796-b43e-aa5186c3c414}");
        public static readonly Guid ol_Department = new Guid("{c814b2cf-84c6-4f56-b4a4-c766938a97c5}");
        public static readonly Guid ol_EventAddress = new Guid("{493896da-0a4f-46ec-a68e-9cfd1a5fc19b}");
        public static readonly Guid Oof = new Guid("{63c1c608-df6f-4cfa-bcab-fdbf9c223e31}");
        public static readonly Guid Order = new Guid("{ca4addac-796f-4b23-b093-d2a3f65c0774}");
        public static readonly Guid OrganizationalIDNumber = new Guid("{0850ae15-19dd-431f-9c2f-3aff3ae292ce}");
        public static readonly Guid OtherAddressCity = new Guid("{90fa9a8e-aac0-4828-9cb4-78f98416affa}");
        public static readonly Guid OtherAddressCountry = new Guid("{3c0e9e00-8fcc-479f-9d8d-3447cda34c5b}");
        public static readonly Guid OtherAddressPostalCode = new Guid("{0557c3f8-60c4-4dfb-b5ba-bf3c4e4386b1}");
        public static readonly Guid OtherAddressStateOrProvince = new Guid("{f45883bc-8733-4b77-ab5d-43613986aa12}");
        public static readonly Guid OtherAddressStreet = new Guid("{dff5dfc2-e2b7-4a19-bde7-76dabc90a3d2}");
        public static readonly Guid OtherFaxNumber = new Guid("{aad15eb6-d7fd-47b8-abd4-adc0fe33a6ba}");
        public static readonly Guid OtherNumber = new Guid("{96e02495-f428-48bc-9f13-06d98ba58c34}");
        public static readonly Guid Out = new Guid("{fde05b9b-52bf-43dc-9b96-bb35fa7aa05d}");
        public static readonly Guid Outcome = new Guid("{dcde7b1f-918b-4ed5-819f-9798f8abac37}");
        public static readonly Guid Overbook = new Guid("{d8cd5bcf-3768-4d6c-a8aa-fefa3c793d8d}");
        public static readonly Guid Overtime = new Guid("{35d79e8b-3701-4659-9c27-c070ed3c2bfa}");
        public static readonly Guid owshiddenversion = new Guid("{d4e44a66-ee3a-4d02-88c9-4ec5ff3f4cd5}");
        public static readonly Guid PagerNumber = new Guid("{f79bf074-daf7-4c06-a314-15b287fdf4c9}");
        public static readonly Guid ParentFolderId = new Guid("{a9ec25bf-5a22-4658-bd19-484e52efbe1a}");
        public static readonly Guid ParentLeafName = new Guid("{774eab3a-855f-4a34-99da-69dc21043bec}");
        public static readonly Guid ParentVersionString = new Guid("{bc1a8efb-0f4c-49f8-a38f-7fe22af3d3e0}");
        public static readonly Guid Participants = new Guid("{453c2d71-c41e-46bc-97c1-a5a9535053a3}");
        public static readonly Guid ParticipantsPicker = new Guid("{8137f7ad-9170-4c1d-a17b-4ca7f557bc88}");
        public static readonly Guid PendingModTime = new Guid("{4d2444c2-0e97-476c-a2a3-e9e4a9c73009}");
        public static readonly Guid PercentComplete = new Guid("{d2311440-1ed6-46ea-b46d-daa643dc3886}");
        public static readonly Guid PermMask = new Guid("{ba3c27ee-4791-4867-8821-ff99000bac98}");
        public static readonly Guid PersonalWebsite = new Guid("{5aa071d9-3254-40fb-82df-5cedeff0c41e}");
        public static readonly Guid PersonImage = new Guid("{adfe65ee-74bb-4771-bec5-d691d9a6a14e}");
        public static readonly Guid PersonViewMinimal = new Guid("{b4ab471e-0262-462a-8b3f-c1dfc9e2d5fd}");
        public static readonly Guid Picture = new Guid("{d9339777-b964-489a-bf09-2ac3c3fe5f0d}");
        public static readonly Guid PostCategory = new Guid("{38bea83b-350a-1a6e-f34a-93a6af31338b}");
        public static readonly Guid Predecessors = new Guid("{c3a92d97-2b77-4a25-9698-3ab54874bc6f}");
        public static readonly Guid Preview = new Guid("{bd716b26-546d-43f2-b229-62699581fa9f}");
        public static readonly Guid PreviewExists = new Guid("{3ca8efcd-96e8-414f-ba90-4c8c4a8bfef8}");
        public static readonly Guid PreviewOnForm = new Guid("{8c0d0aac-9b76-4951-927a-2490abe13c0b}");
        public static readonly Guid PrimaryNumber = new Guid("{d69bcc0e-57c3-4f3b-bbc5-b090edf21f0f}");
        public static readonly Guid Priority = new Guid("{a8eb573e-9e11-481a-a8c9-1104a54b2fbd}");
        public static readonly Guid Profession = new Guid("{f0753a13-44b1-4269-82af-5c34c57b0c67}");
        public static readonly Guid ProgId = new Guid("{c5c4b81c-f1d9-4b43-a6a2-090df32ebb68}");
        public static readonly Guid PublishedDate = new Guid("{b1b53d80-23d6-e31b-b235-3a286b9f10ea}");
        public static readonly Guid Purpose = new Guid("{8ee23f39-e2d1-4b46-8945-42386b24829d}");
        public static readonly Guid QuotedTextWasExpanded = new Guid("{e393d344-2e8c-425b-a8c3-89ac3144c9a2}");
        public static readonly Guid RadioNumber = new Guid("{d1aede4f-1352-48d9-81e2-b10097c359c1}");
        public static readonly Guid RecurrenceData = new Guid("{d12572d0-0a1e-4438-89b5-4d0430be7603}");
        public static readonly Guid RecurrenceID = new Guid("{dfcc8fff-7c4c-45d6-94ed-14ce0719efef}");
        public static readonly Guid ReferredBy = new Guid("{9b4cc5a9-1119-43e4-b2a8-412c4031f92b}");
        public static readonly Guid RelatedIssues = new Guid("{875fab27-6e95-463b-a4a6-82544f1027fb}");
        public static readonly Guid RelevantMessages = new Guid("{9161f6cb-a8e6-47b8-9d24-89415de691f7}");
        public static readonly Guid RepairDocument = new Guid("{5d36727b-bcb2-47d2-a231-1f0bc63b7439}");
        public static readonly Guid ReplyNoGif = new Guid("{87cda0e2-fc57-4eec-a696-b0de2f61f361}");
        public static readonly Guid RequiredField = new Guid("{de1baa4b-2117-473b-aa0c-4d824034142d}");
        public static readonly Guid Resolved = new Guid("{a6fd2bb9-c701-4168-99cc-242e42f7671a}");
        public static readonly Guid ResolvedBy = new Guid("{b4fa187b-eb65-478e-8bc6-93b0da320f03}");
        public static readonly Guid ResolvedDate = new Guid("{c4995c71-4c5c-4e9f-afc1-a9033f2bfde5}");
        public static readonly Guid RestrictContentTypeId = new Guid("{8b02a33c-accd-4b73-bcae-6932c7aab812}");
        public static readonly Guid Role = new Guid("{eeaeaaf1-4110-465b-905e-df1073a7e0e6}");
        public static readonly Guid RulesUrl = new Guid("{ad97fbac-70af-4860-a078-5ee704946f93}");
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<Guid, bool> s_dict = null;
        public static readonly Guid ScheduledWork = new Guid("{3bdf7bd3-f229-419e-8e12-3dfecb49ed38}");
        public static readonly Guid ScopeId = new Guid("{dddd2420-b270-4735-93b5-92b713d0944d}");
        public static readonly Guid SelectedFlag = new Guid("{7ebf72ca-a307-4c18-9e5b-9d89e1dae74f}");
        public static readonly Guid SelectFilename = new Guid("{5f47e085-2150-41dc-b661-442f3027f552}");
        public static readonly Guid SelectTitle = new Guid("{b1f7969b-ea65-42e1-8b54-b588292635f2}");
        public static readonly Guid SendEmailNotification = new Guid("{cb2413f2-7de9-4afc-8587-1ca3f563f624}");
        public static readonly Guid ServerUrl = new Guid("{105f76ce-724a-4bba-aece-f81f2fce58f5}");
        public static readonly Guid Service = new Guid("{48b4a73e-8853-44ac-83a8-3a4bd59ce9ec}");
        public static readonly Guid ShortComment = new Guid("{691b9a4b-512e-4341-b3f1-68914130d5b2}");
        public static readonly Guid ShortestThreadIndex = new Guid("{4753e73b-5b5d-4bbc-8e09-c9683b0d40a7}");
        public static readonly Guid ShortestThreadIndexId = new Guid("{2bec4782-695f-406d-9e50-f1d39a2b8eb6}");
        public static readonly Guid ShortestThreadIndexIdLookup = new Guid("{8ffccefe-998b-4896-a6df-32d566f69141}");
        public static readonly Guid ShowCombineView = new Guid("{086f2b30-460c-4251-b75a-da88a5b205c1}");
        public static readonly Guid ShowRepairView = new Guid("{11851948-b05e-41be-9d9f-bc3bf55d1de3}");
        public static readonly Guid SipAddress = new Guid("{829c275d-8744-4d9b-a42f-53f53eb60559}");
        public static readonly Guid SortBehavior = new Guid("{423874f8-c300-4bfb-b7a1-42e2159e3b19}");
        public static readonly Guid SpouseName = new Guid("{f590b1de-8e28-4c17-91bc-bf4096024b7e}");
        public static readonly Guid Start = new Guid("{05e6336c-d22e-478e-9414-366762883b3f}");
        public static readonly Guid StartDate = new Guid("{64cd368d-2f95-4bfc-a1f9-8d4324ecb007}");
        public static readonly Guid StatusBar = new Guid("{f90bce56-87dc-4d73-bfcb-03fcaf670500}");
        public static readonly Guid Subject = new Guid("{76a81629-44d4-4ce1-8d4d-6d7ebcd885fc}");
        public static readonly Guid Suffix = new Guid("{d886eba3-d018-4103-a322-d5780127ef8a}");
        public static readonly Guid SurveyTitle = new Guid("{e6f528fb-2e22-483d-9c80-f2536acdc6de}");
        public static readonly Guid SystemTask = new Guid("{af0a3d4b-3ceb-449e-9bf4-51103f2032e3}");
        public static readonly Guid TaskCompanies = new Guid("{3914f98e-6d99-4218-9ba3-af7370b9e7bc}");
        public static readonly Guid TaskDueDate = new Guid("{cd21b4c2-6841-4f9e-a23a-738a65f99889}");
        public static readonly Guid TaskGroup = new Guid("{50d8f08c-8e99-4948-97bf-2be41fa34a0d}");
        public static readonly Guid TaskStatus = new Guid("{c15b34c3-ce7d-490a-b133-3f4de8801b76}");
        public static readonly Guid TaskType = new Guid("{8d96aa48-9dff-46cf-8538-84c747ffa877}");
        public static readonly Guid TelexNumber = new Guid("{e7be7f3c-c436-481d-8865-669e5146f53c}");
        public static readonly Guid TemplateUrl = new Guid("{4b1bf6c6-4f39-45ac-acd5-16fe7a214e5e}");
        public static readonly Guid ThreadIndex = new Guid("{cef73bf1-edf6-4dd9-9098-a07d83984700}");
        public static readonly Guid Threading = new Guid("{58ca6516-51cd-41fb-a908-dd2a4aeea8bc}");
        public static readonly Guid ThreadingControls = new Guid("{c55a4674-640b-4bae-8738-ce0439e6f6d4}");
        public static readonly Guid ThreadTopic = new Guid("{769b99d9-d361-4948-b687-f01332391629}");
        public static readonly Guid Thumbnail = new Guid("{ac7bb138-02dc-40eb-b07a-84c15575b6e9}");
        public static readonly Guid ThumbnailExists = new Guid("{1f43cd21-53c5-44c5-8675-b8bb86083244}");
        public static readonly Guid ThumbnailOnForm = new Guid("{9941082a-4160-46a1-a5b2-03394bfdf7ee}");
        public static readonly Guid TimeZone = new Guid("{6cc1c612-748a-48d8-88f2-944f477f301b}");
        public static readonly Guid Title = new Guid("{fa564e0f-0c70-4ab9-b863-0177e6ddd247}");
        public static readonly Guid ToggleQuotedText = new Guid("{e451420d-4e62-43e3-af83-010d36e353a2}");
        public static readonly Guid TotalWork = new Guid("{f3c4a259-19a2-44b8-ab3d-e9145d07d538}");
        public static readonly Guid TrimmedBody = new Guid("{6d0f8993-5050-41f3-be6c-18902d282357}");
        public static readonly Guid TTYTDDNumber = new Guid("{f54697f1-0357-4c5a-a711-0cb654bc73e4}");
        public static readonly Guid UID = new Guid("{63055d04-01b5-48f3-9e1e-e564e7c6b23b}");
        public static readonly Guid UIVersion = new Guid("{8e334549-c2bd-4110-9f61-672971be6504}");
        public static readonly Guid UniqueId = new Guid("{4b7403de-8d94-43e8-9f0f-137a3e298126}");
        public static readonly Guid Until = new Guid("{fe3344ab-b468-471f-8fa5-9b506c7d1557}");
        public static readonly Guid URL = new Guid("{c29e077d-f466-4d8e-8bbe-72b66c5f205c}");
        public static readonly Guid URLNoMenu = new Guid("{aeaf07ee-d2fb-448b-a7a3-cf7e062d6c2a}");
        public static readonly Guid URLwMenu = new Guid("{2a9ab6d3-268a-4c1c-9897-e5f018f87e64}");
        public static readonly Guid User = new Guid("{5928ff1f-daa1-406c-b4a9-190485a448cb}");
        public static readonly Guid UserField1 = new Guid("{566656f5-17b3-4291-98a5-5074aadf77b3}");
        public static readonly Guid UserField2 = new Guid("{182d1b9e-1718-4e11-b279-38f7ed0a20d6}");
        public static readonly Guid UserField3 = new Guid("{a03eb53e-f123-4af9-9355-f92bd75c00b3}");
        public static readonly Guid UserField4 = new Guid("{adefa4ca-14c3-4694-b531-f51b706efe9d}");
        public static readonly Guid UserName = new Guid("{211a8cfc-93b7-4173-9254-0bfe2d1643da}");
        public static readonly Guid V3Comments = new Guid("{6df9bd52-550e-4a30-bc31-a4366832a87e}");
        public static readonly Guid V4CallTo = new Guid("{7111aa1b-e7ae-4b69-acaf-db669b76e03a}");
        public static readonly Guid V4HolidayDate = new Guid("{492b1ac0-c594-4013-a2b6-ea70f5a8a506}");
        public static readonly Guid V4SendTo = new Guid("{e0f298a5-7e3e-4895-9ff8-90d88ec4526d}");
        public static readonly Guid Vacation = new Guid("{dfd58778-bf8e-4769-8265-09ac03159eed}");
        public static readonly Guid VirusStatus = new Guid("{4a389cb9-54dd-4287-a71a-90ff362028bc}");
        public static readonly Guid WebPage = new Guid("{a71affd2-dcc7-4529-81bc-2fe593154a5f}");
        public static readonly Guid WhatsNew = new Guid("{cf68a174-123b-413e-9ec1-b43e3a3175d7}");
        public static readonly Guid Whereabout = new Guid("{e2a07293-596a-4c59-9089-5c4f9339077f}");
        public static readonly Guid wic_SystemCopyright = new Guid("{f08ab41d-9a03-49ae-9413-6cd284a15625}");
        public static readonly Guid WikiField = new Guid("{c33527b4-d920-4587-b791-45024d00068a}");
        public static readonly Guid WorkAddress = new Guid("{fc2e188e-ba91-48c9-9dd3-16431afddd50}");
        public static readonly Guid WorkCity = new Guid("{6ca7bd7f-b490-402e-af1b-2813cf087b1e}");
        public static readonly Guid WorkCountry = new Guid("{3f3a5c85-9d5a-4663-b925-8b68a678ea3a}");
        public static readonly Guid WorkFax = new Guid("{9d1cacc8-f452-4bc1-a751-050595ad96e1}");
        public static readonly Guid WorkflowAssociation = new Guid("{8d426880-8d96-459b-ae48-e8b3836d8b9d}");
        public static readonly Guid WorkflowDisplayName = new Guid("{5263cd09-a770-4549-b012-d9f3df3d8df6}");
        public static readonly Guid WorkflowInstance = new Guid("{de21c770-a12b-4f88-af4b-aeebd897c8c2}");
        public static readonly Guid WorkflowInstanceID = new Guid("{de8beacf-5505-47cd-80a6-aa44e7ffe2f4}");
        public static readonly Guid WorkflowItemId = new Guid("{8e234c69-02b0-42d9-8046-d5f49bf0174f}");
        public static readonly Guid WorkflowLink = new Guid("{58ddda52-c2a3-4650-9178-3bbc1f6e36da}");
        public static readonly Guid WorkflowListId = new Guid("{1bfee788-69b7-4765-b109-d4d9c31d1ac1}");
        public static readonly Guid WorkflowName = new Guid("{e506d6ca-c2da-4164-b858-306f1c41c9ec}");
        public static readonly Guid WorkflowOutcome = new Guid("{18e1c6fa-ae37-4102-890a-cfb0974ef494}");
        public static readonly Guid WorkflowTemplate = new Guid("{bfb1589e-2016-4b98-ae62-e91979c3224f}");
        public static readonly Guid WorkflowVersion = new Guid("{f1e020bc-ba26-443f-bf2f-b68715017bbc}");
        public static readonly Guid WorkPhone = new Guid("{fd630629-c165-4513-b43c-fdb16b86a14d}");
        public static readonly Guid Workspace = new Guid("{881eac4a-55a5-48b6-a28e-8329d7486120}");
        public static readonly Guid WorkspaceLink = new Guid("{08fc65f9-48eb-4e99-bd61-5946c439e691}");
        public static readonly Guid WorkState = new Guid("{ceac61d3-dda9-468b-b276-f4a6bb93f14f}");
        public static readonly Guid WorkZip = new Guid("{9a631556-3dac-49db-8d2f-fb033b0fdc24}");
        public static readonly Guid xd_ProgID = new Guid("{cd1ecb9f-dd4e-4f29-ab9e-e9ff40048d64}");
        public static readonly Guid xd_Signature = new Guid("{fbf29b2d-cae5-49aa-8e0a-29955b540122}");
        public static readonly Guid XMLTZone = new Guid("{c4b72ed6-45aa-4422-bff1-2b6750d30819}");
        public static readonly Guid XomlUrl = new Guid("{566da236-762b-4a76-ad1f-b08b3c703fce}");
        public static readonly Guid XSLStyleBaseView = new Guid("{4630e6ac-e543-4667-935a-2cc665e9b755}");
        public static readonly Guid XSLStyleCategory = new Guid("{dfffbbfb-0cc3-4ce7-8cb3-a2958fb726a1}");
        public static readonly Guid XSLStyleIconUrl = new Guid("{3dfb3e11-9ccd-4404-b44a-a71f6399ea56}");
        public static readonly Guid XSLStyleRequiredFields = new Guid("{acb9088a-a171-4b99-aa7a-10388586bc74}");
        public static readonly Guid XSLStyleWPType = new Guid("{4499086f-9ac1-41df-86c3-d8c1f8fc769a}");

        // Methods
        private AveBuiltInFieldId()
        { }
        static AveBuiltInFieldId()
        {
            s_dict = new Dictionary<Guid, bool>(0x19c);
            s_dict[Home2Number] = true;
            s_dict[AdminTaskOrder] = true;
            s_dict[PendingModTime] = true;
            s_dict[ImageHeight] = true;
            s_dict[SipAddress] = true;
            s_dict[HomeAddressStateOrProvince] = true;
            s_dict[Combine] = true;
            s_dict[MyEditor] = true;
            s_dict[Attachments] = true;
            s_dict[PersonalWebsite] = true;
            s_dict[Suffix] = true;
            s_dict[CarNumber] = true;
            s_dict[ShowCombineView] = true;
            s_dict[DayOfWeek] = true;
            s_dict[DLC_Duration] = true;
            s_dict[AdminTaskAction] = true;
            s_dict[CallTime] = true;
            s_dict[LinkDiscussionTitle] = true;
            s_dict[WorkCountry] = true;
            s_dict[Indentation] = true;
            s_dict[TaskGroup] = true;
            s_dict[ConnectionType] = true;
            s_dict[Author] = true;
            s_dict[xd_ProgID] = true;
            s_dict[Initials] = true;
            s_dict[WorkspaceLink] = true;
            s_dict[SpouseName] = true;
            s_dict[ThumbnailExists] = true;
            s_dict[SelectedFlag] = true;
            s_dict[XSLStyleRequiredFields] = true;
            s_dict[ContactInfo] = true;
            s_dict[_ModerationStatus] = true;
            s_dict[FileType] = true;
            s_dict[HealthReportExplanation] = true;
            s_dict[_Source] = true;
            s_dict[wic_SystemCopyright] = true;
            s_dict[IsRootPost] = true;
            s_dict[HomePhone] = true;
            s_dict[TotalWork] = true;
            s_dict[Break] = true;
            s_dict[WorkZip] = true;
            s_dict[ShortComment] = true;
            s_dict[_Comments] = true;
            s_dict[EmailHeaders] = true;
            s_dict[Hobbies] = true;
            s_dict[PersonImage] = true;
            s_dict[UserField2] = true;
            s_dict[FirstName] = true;
            s_dict[TaskCompanies] = true;
            s_dict[ResolvedBy] = true;
            s_dict[Modified] = true;
            s_dict[MobileContent] = true;
            s_dict[WorkflowVersion] = true;
            s_dict[IconOverlay] = true;
            s_dict[AdminTaskDescription] = true;
            s_dict[_DCDateCreated] = true;
            s_dict[WorkState] = true;
            s_dict[UserField4] = true;
            s_dict[IMEComment3] = true;
            s_dict[FTPSite] = true;
            s_dict[Group] = true;
            s_dict[AssistantNumber] = true;
            s_dict[Mileage] = true;
            s_dict[PreviewExists] = true;
            s_dict[EmailBody] = true;
            s_dict[FileDirRef] = true;
            s_dict[OffsiteParticipant] = true;
            s_dict[V4HolidayDate] = true;
            s_dict[V4SendTo] = true;
            s_dict[JobTitle] = true;
            s_dict[EncodedAbsThumbnailUrl] = true;
            s_dict[ExtendedProperties] = true;
            s_dict[XSLStyleCategory] = true;
            s_dict[MiddleName] = true;
            s_dict[Description] = true;
            s_dict[_SharedFileIndex] = true;
            s_dict[LimitedBody] = true;
            s_dict[UIVersion] = true;
            s_dict[Workspace] = true;
            s_dict[_ModerationComments] = true;
            s_dict[ol_EventAddress] = true;
            s_dict[HealthRuleReportLink] = true;
            s_dict[Nickname] = true;
            s_dict[OffsiteParticipantReason] = true;
            s_dict[ResolvedDate] = true;
            s_dict[PersonViewMinimal] = true;
            s_dict[PrimaryNumber] = true;
            s_dict[Created_x0020_Date] = true;
            s_dict[WhatsNew] = true;
            s_dict[_Category] = true;
            s_dict[Role] = true;
            s_dict[UserField3] = true;
            s_dict[Overtime] = true;
            s_dict[VirusStatus] = true;
            s_dict[AlternateThumbnailUrl] = true;
            s_dict[Facilities] = true;
            s_dict[Item] = true;
            s_dict[fAllDayEvent] = true;
            s_dict[IMEComment1] = true;
            s_dict[_Revision] = true;
            s_dict[Late] = true;
            s_dict[ConfirmedTo] = true;
            s_dict[Editor] = true;
            s_dict[FormURN] = true;
            s_dict[HomeFaxNumber] = true;
            s_dict[HealthReportCategory] = true;
            s_dict[Purpose] = true;
            s_dict[EMail] = true;
            s_dict[GUID] = true;
            s_dict[ID] = true;
            s_dict[DocIcon] = true;
            s_dict[EncodedAbsUrl] = true;
            s_dict[Confidential] = true;
            s_dict[TTYTDDNumber] = true;
            s_dict[RequiredField] = true;
            s_dict[TaskType] = true;
            s_dict[ParentFolderId] = true;
            s_dict[HomeAddressCity] = true;
            s_dict[Start] = true;
            s_dict[LeaveEarly] = true;
            s_dict[HealthReportSeverity] = true;
            s_dict[HolidayNightWork] = true;
            s_dict[SurveyTitle] = true;
            s_dict[_EditMenuTableStart2] = true;
            s_dict[EventCanceled] = true;
            s_dict[ComputerNetworkName] = true;
            s_dict[Location] = true;
            s_dict[_Author] = true;
            s_dict[Edit] = true;
            s_dict[QuotedTextWasExpanded] = true;
            s_dict[CustomerID] = true;
            s_dict[WorkflowInstanceID] = true;
            s_dict[TemplateUrl] = true;
            s_dict[Priority] = true;
            s_dict[owshiddenversion] = true;
            s_dict[Categories] = true;
            s_dict[GovernmentIDNumber] = true;
            s_dict[_EditMenuTableStart] = true;
            s_dict[ShortestThreadIndexId] = true;
            s_dict[Subject] = true;
            s_dict[CellPhone] = true;
            s_dict[ISDNNumber] = true;
            s_dict[Keywords] = true;
            s_dict[Out] = true;
            s_dict[Thumbnail] = true;
            s_dict[Picture] = true;
            s_dict[Email3] = true;
            s_dict[Date] = true;
            s_dict[RelatedIssues] = true;
            s_dict[WorkflowTemplate] = true;
            s_dict[Category] = true;
            s_dict[V3Comments] = true;
            s_dict[ShortestThreadIndexIdLookup] = true;
            s_dict[EmailTo] = true;
            s_dict[WorkflowInstance] = true;
            s_dict[DueDate] = true;
            s_dict[_Format] = true;
            s_dict[NightWork] = true;
            s_dict[HealthRuleSchedule] = true;
            s_dict[UniqueId] = true;
            s_dict[User] = true;
            s_dict[_SourceUrl] = true;
            s_dict[MoreLink] = true;
            s_dict[WorkAddress] = true;
            s_dict[DLC_Description] = true;
            s_dict[ThreadTopic] = true;
            s_dict[GbwLocation] = true;
            s_dict[ServerUrl] = true;
            s_dict[RecurrenceData] = true;
            s_dict[IMEDisplay] = true;
            s_dict[ToggleQuotedText] = true;
            s_dict[Service] = true;
            s_dict[_Photo] = true;
            s_dict[GoingHome] = true;
            s_dict[WorkflowLink] = true;
            s_dict[OrganizationalIDNumber] = true;
            s_dict[ParentLeafName] = true;
            s_dict[HomeAddressStreet] = true;
            s_dict[WorkFax] = true;
            s_dict[URLwMenu] = true;
            s_dict[EmailSender] = true;
            s_dict[WorkflowAssociation] = true;
            s_dict[OtherAddressStateOrProvince] = true;
            s_dict[XMLTZone] = true;
            s_dict[ManagersName] = true;
            s_dict[ListType] = true;
            s_dict[Comments] = true;
            s_dict[FormData] = true;
            s_dict[XSLStyleWPType] = true;
            s_dict[Content] = true;
            s_dict[FirstNamePhonetic] = true;
            s_dict[UID] = true;
            s_dict[OtherAddressPostalCode] = true;
            s_dict[CompanyNumber] = true;
            s_dict[Until] = true;
            s_dict[_CopySource] = true;
            s_dict[LinkFilenameNoMenu] = true;
            s_dict[SortBehavior] = true;
            s_dict[PermMask] = true;
            s_dict[EventType] = true;
            s_dict[LastNamePhonetic] = true;
            s_dict[PercentComplete] = true;
            s_dict[_DCDateModified] = true;
            s_dict[Predecessors] = true;
            s_dict[_UIVersion] = true;
            s_dict[RecurrenceID] = true;
            s_dict[From] = true;
            s_dict[_Contributor] = true;
            s_dict[xd_Signature] = true;
            s_dict[Birthday] = true;
            s_dict[LinkFilename] = true;
            s_dict[IssueStatus] = true;
            s_dict[HTML_x0020_File_x0020_Type] = true;
            s_dict[FreeBusy] = true;
            s_dict[LinkIssueIDNoMenu] = true;
            s_dict[_UIVersionString] = true;
            s_dict[IMEPos] = true;
            s_dict[URLNoMenu] = true;
            s_dict[OtherAddressStreet] = true;
            s_dict[NumberOfVacation] = true;
            s_dict[WorkflowOutcome] = true;
            s_dict[Preview] = true;
            s_dict[BaseName] = true;
            s_dict[XomlUrl] = true;
            s_dict[ThreadIndex] = true;
            s_dict[HealthRuleScope] = true;
            s_dict[ShowRepairView] = true;
            s_dict[XSLStyleIconUrl] = true;
            s_dict[Profession] = true;
            s_dict[IMEUrl] = true;
            s_dict[MessageId] = true;
            s_dict[GbwCategory] = true;
            s_dict[UserName] = true;
            s_dict[EmailCalendarUid] = true;
            s_dict[Deleted] = true;
            s_dict[V4CallTo] = true;
            s_dict[_RightsManagement] = true;
            s_dict[RulesUrl] = true;
            s_dict[InstanceID] = true;
            s_dict[Company] = true;
            s_dict[HealthRuleCheckEnabled] = true;
            s_dict[ShortestThreadIndex] = true;
            s_dict[_Status] = true;
            s_dict[WorkflowListId] = true;
            s_dict[WorkflowName] = true;
            s_dict[_EditMenuTableEnd] = true;
            s_dict[UserField1] = true;
            s_dict[AllowEditing] = true;
            s_dict[Data] = true;
            s_dict[DiscussionLastUpdated] = true;
            s_dict[RepairDocument] = true;
            s_dict[WorkflowDisplayName] = true;
            s_dict[OtherAddressCity] = true;
            s_dict[MetaInfo] = true;
            s_dict[FileSizeDisplay] = true;
            s_dict[AssignedTo] = true;
            s_dict[Order] = true;
            s_dict[File_x0020_Size] = true;
            s_dict[ProgId] = true;
            s_dict[ImageCreateDate] = true;
            s_dict[HealthRuleType] = true;
            s_dict[HealthRuleService] = true;
            s_dict[EmailCalendarSequence] = true;
            s_dict[IMAddress] = true;
            s_dict[HealthRuleVersion] = true;
            s_dict[BaseAssociationGuid] = true;
            s_dict[Last_x0020_Modified] = true;
            s_dict[StartDate] = true;
            s_dict[EmailReferences] = true;
            s_dict[CheckoutUser] = true;
            s_dict[RelevantMessages] = true;
            s_dict[CompanyPhonetic] = true;
            s_dict[IsActive] = true;
            s_dict[ReferredBy] = true;
            s_dict[File_x0020_Type] = true;
            s_dict[URL] = true;
            s_dict[NameOrTitle] = true;
            s_dict[IndentLevel] = true;
            s_dict[ReplyNoGif] = true;
            s_dict[AssistantsName] = true;
            s_dict[TaskStatus] = true;
            s_dict[EmailCalendarDateStamp] = true;
            s_dict[LinkTitleNoMenu] = true;
            s_dict[ChildrensNames] = true;
            s_dict[ImageSize] = true;
            s_dict[ScheduledWork] = true;
            s_dict[IMEComment2] = true;
            s_dict[CallBack] = true;
            s_dict[_Coverage] = true;
            s_dict[ContentTypeId] = true;
            s_dict[StatusBar] = true;
            s_dict[Modified_x0020_By] = true;
            s_dict[WebPage] = true;
            s_dict[PostCategory] = true;
            s_dict[DiscussionTitle] = true;
            s_dict[PreviewOnForm] = true;
            s_dict[FileLeafRef] = true;
            s_dict[HasCustomEmailBody] = true;
            s_dict[EmailFrom] = true;
            s_dict[Whereabout] = true;
            s_dict[BodyWasExpanded] = true;
            s_dict[_EndDate] = true;
            s_dict[ActualWork] = true;
            s_dict[Name] = true;
            s_dict[ol_Department] = true;
            s_dict[HealthReportSeverityIcon] = true;
            s_dict[Title] = true;
            s_dict[_HasCopyDestinations] = true;
            s_dict[List] = true;
            s_dict[FileRef] = true;
            s_dict[HealthReportServices] = true;
            s_dict[In] = true;
            s_dict[Department] = true;
            s_dict[OtherAddressCountry] = true;
            s_dict[_Identifier] = true;
            s_dict[Completed] = true;
            s_dict[PublishedDate] = true;
            s_dict[Business2Number] = true;
            s_dict[OtherFaxNumber] = true;
            s_dict[ThreadingControls] = true;
            s_dict[_Version] = true;
            s_dict[ContentType] = true;
            s_dict[EncodedAbsWebImgUrl] = true;
            s_dict[SelectTitle] = true;
            s_dict[BillingInformation] = true;
            s_dict[LinkTitle] = true;
            s_dict[FSObjType] = true;
            s_dict[fRecurrence] = true;
            s_dict[Confirmations] = true;
            s_dict[DateCompleted] = true;
            s_dict[SendEmailNotification] = true;
            s_dict[WorkPhone] = true;
            s_dict[FullBody] = true;
            s_dict[Event] = true;
            s_dict[Oof] = true;
            s_dict[Participants] = true;
            s_dict[Expires] = true;
            s_dict[AttendeeStatus] = true;
            s_dict[_Publisher] = true;
            s_dict[DiscussionTitleLookup] = true;
            s_dict[_LastPrinted] = true;
            s_dict[HealthReportServers] = true;
            s_dict[HealthRuleAutoRepairEnabled] = true;
            s_dict[Body] = true;
            s_dict[ParticipantsPicker] = true;
            s_dict[SystemTask] = true;
            s_dict[Threading] = true;
            s_dict[Notes] = true;
            s_dict[_Relation] = true;
            s_dict[Anniversary] = true;
            s_dict[HolidayWork] = true;
            s_dict[RadioNumber] = true;
            s_dict[HolidayDate] = true;
            s_dict[_ResourceType] = true;
            s_dict[XSLStyleBaseView] = true;
            s_dict[RestrictContentTypeId] = true;
            s_dict[TimeZone] = true;
            s_dict[ThumbnailOnForm] = true;
            s_dict[Detail] = true;
            s_dict[CorrectBodyToShow] = true;
            s_dict[EndDate] = true;
            s_dict[TelexNumber] = true;
            s_dict[DecisionStatus] = true;
            s_dict[PagerNumber] = true;
            s_dict[Duration] = true;
            s_dict[_IsCurrentVersion] = true;
            s_dict[EmailSubject] = true;
            s_dict[WikiField] = true;
            s_dict[Created_x0020_By] = true;
            s_dict[MobilePhone] = true;
            s_dict[Office] = true;
            s_dict[FolderChildCount] = true;
            s_dict[WorkCity] = true;
            s_dict[Overbook] = true;
            s_dict[Resolved] = true;
            s_dict[FullName] = true;
            s_dict[HealthReportRemedy] = true;
            s_dict[TaskDueDate] = true;
            s_dict[HomeAddressPostalCode] = true;
            s_dict[HomeAddressCountry] = true;
            s_dict[WorkflowItemId] = true;
            s_dict[Comment] = true;
            s_dict[_CheckinComment] = true;
            s_dict[Gender] = true;
            s_dict[LessLink] = true;
            s_dict[ParentVersionString] = true;
            s_dict[IsNonWorkingDay] = true;
            s_dict[ScopeId] = true;
            s_dict[MasterSeriesItemID] = true;
            s_dict[TrimmedBody] = true;
            s_dict[End] = true;
            s_dict[ItemChildCount] = true;
            s_dict[Language] = true;
            s_dict[Created] = true;
            s_dict[AssociatedListId] = true;
            s_dict[IsSiteAdmin] = true;
            s_dict[OtherNumber] = true;
            s_dict[MessageBody] = true;
            s_dict[Vacation] = true;
            s_dict[LinkDiscussionTitle2] = true;
            s_dict[Occurred] = true;
            s_dict[SelectFilename] = true;
            s_dict[ImageWidth] = true;
            s_dict[Outcome] = true;
            s_dict[GoFromHome] = true;
            s_dict[EmailCc] = true;
            s_dict[Email2] = true;
            s_dict[BodyAndMore] = true;
            s_dict[_Level] = true;
            s_dict[LinkDiscussionTitleNoMenu] = true;
            s_dict[NoCodeVisibility] = true;
            s_dict[CallbackNumber] = true;
        }

        private static object lockObject = new object();
        public static bool Contains(Guid fid)
        {
            bool flag = false;
            s_dict.TryGetValue(fid, out flag);
            return flag;
        }
    }

    public enum AveFieldUserSelectionMode
    {
        PeopleOnly,
        PeopleAndGroups
    }

    public enum AveRecycleBinItemState
    {
        None,
        FirstStageRecycleBin,
        SecondStageRecycleBin
    }

    public enum AveRecycleBinItemType
    {
        None = 0,
        File = 1,
        FileVersion = 2,
        ListItem = 3,
        List = 4,
        Folder = 5,
        FolderWithLists = 6,
        Attachment = 7,
        ListItemVersion = 8,
        CascadeParent = 9,
        Web = 10,
    }

    public enum AveEventType
    {
        Add = 1,
        All = -1,
        Delete = 4,
        Discussion = 0xff0,
        Modify = 2
    }

    public enum AveAlertFrequency
    {
        Immediate = 0,
        Daily = 1,
        Weekly = 2,
    }

    public enum AveAlertDeliveryChannels
    {
        Email = 1,
        Sms = 2
    }

    public enum AveListItemType
    {
        GenericItem = 0,
        DicussionTopic = 1,
        DicussionReply = 2,
        ExceptionEvent = 3,
    }

    public enum AveFeatureDefinitionScope
    {
        None = 0,
        Farm = 1,
        Site = 2,
        WebApplication = 4,
        Web = 3
    }

    public enum AveFeatureScope
    {
        Farm = 0,
        ScopeInvalid = -1,
        Site = 2,
        Web = 3,
        WebApplication = 1
    }

    // [SubsetCallableType, ClientCallableType(Name="PageType")]
    public enum AvePAGETYPE
    {
        Invalid = -1,
        DefaultView = 0,
        NormalView = 1,
        DialogView = 2,
        View = 3,
        DisplayForm = 4,
        DisplayFormDialog = 5,
        EditForm = 6,
        EditFormDialog = 7,
        NewForm = 8,
        NewFormDialog = 9,
        SolutionForm = 10,
        PAGE_MAXITEMS = 11
    }

    public enum AvePAGETYPE_FOR_LOCAL
    {
        //[ClientCallable(Name = "DefaultView")]
        PAGE_DEFAULTVIEW = 0,
        //[ClientCallable(Name = "DialogView")]
        PAGE_DIALOGVIEW = 2,
        //[ClientCallable(Name = "DisplayForm")]
        PAGE_DISPLAYFORM = 4,
        //[ClientCallable(Name = "DisplayFormDialog")]
        PAGE_DISPLAYFORMDIALOG = 5,
        //[ClientCallable(Name = "EditForm")]
        PAGE_EDITFORM = 6,
        //[ClientCallable(Name = "EditFormDialog")]
        PAGE_EDITFORMDIALOG = 7,
        //[ClientCallable(Name = "Invalid")]
        PAGE_INVALID = -1,
        PAGE_MAXITEMS = 11,
        //[ClientCallable(Name = "NewForm")]
        PAGE_NEWFORM = 8,
        //[ClientCallable(Name = "NewFormDialog")]
        PAGE_NEWFORMDIALOG = 9,
        //[ClientCallable(Name = "NormalView")]
        PAGE_NORMALVIEW = 1,
        //[ClientCallable(Name = "SolutionForm")]
        PAGE_SOLUTIONFORM = 10,
        //[ClientCallable(Name = "View")]
        PAGE_VIEW = 3
    }

    public enum AveOfficialFileAction
    {
        Copy,
        Move,
        Link
    }

    public enum AvePolicyFeatureState
    {
        Hidden,
        Visible
    }

    public enum AveCompositeIndexableStatus
    {
        NotCompositeIndexable,
        AsLastFieldInIndex,
        AsAnyFieldInIndex
    }

    public enum AveAlertType
    {
        List,
        Item,
        Custom
    }

    public enum AveIdentityType
    {
        LocalSystem,
        LocalService,
        NetworkService,
        SpecificUser
    }

    public enum AveServerRole
    {
        Invalid,
        WebFrontEnd,
        Application,
        SingleServer
    }

    public enum AveCheckOutStatus
    {
        None,
        ShortTerm,
        LongTerm,
        LongTermOffline
    }

    public enum AveStorage
    {
        None,
        Personal,
        Shared
    }

    public enum AveClaimProviderOperationOptions
    {
        AllZones = 4,
        None = 0,
        OverrideContextRules = 1,
        OverrideDefaultFlag = 2,
        WindowsRequired = 8
    }

    public enum AveEventProcessingOptions
    {
        AllowResumeChange = 0x10,
        None = 0,
        SkipAboutToChange = 1,
        SkipChangeCanceled = 4,
        SkipChangeCanceledEmail = 8,
        SkipHasChanged = 2
    }

    public enum AveRunningJobStatus
    {
        Scheduled,
        Initialized,
        Succeeded,
        Failed,
        Retry,
        Aborted,
        Pausing,
        Paused
    }

    public enum AveObjectStatus
    {
        Online,
        Disabled,
        Offline,
        Unprovisioning,
        Provisioning,
        Upgrading,
    }

    public enum AveQuiesceMode
    {
        Normal,
        Quiescing,
        Quiesced,
    }

    public enum AveFormTemplateState
    {
        Normal,
        Uploading,
        Quiescing,
        Quiesced,
        PendingConversion,
        Converting,
        Error,
        UploadFailed,
        Removing,
        Upgrading
    }

    public enum AveVirtualServerState
    {
        NeedExtend = 2,
        NeedUpgrade = 4,
        NotAdministrable = 8,
        NotNTFS = 0x10,
        Ready = 1
    }

    public enum AveUrlZone
    {
        Default = 0,
        Intranet = 1,
        Internet = 2,
        Custom = 3,
        Extranet = 4
    }

    public enum AveListCategoryType
    {
        None,
        Libraries,
        Communications,
        Tracking,
        CustomLists
    }

    public enum AveQuickLaunchHeading
    {
        Discussions = 0x3ee,
        Documents = 0x3ec,
        Lists = 0x3eb,
        PeopleAndGroups = 0x403,
        Pictures = 0x3ed,
        Recent = 0x409,
        Sites = 0x402,
        Survey = 0x3ef
    }

    public enum AveChromeType
    {
        WebPart,
        Full
    }

    public enum AvePrincipalType
    {
        All = 15,
        DistributionList = 2,
        None = 0,
        SecurityGroup = 4,
        SharePointGroup = 8,
        User = 1
    }

    public enum AveColleagueGroupType
    {
        // Summary:
        //     Indicates that the colleague was added by the user.
        UserSpecified = 0,
        //
        // Summary:
        //     Indicates that the colleague is in the same organization as the user.
        General = 2,
        //
        // Summary:
        //     Indicates that the colleague is a user's peer.
        Peer = 5,
    }

    public enum AveMembershipSource
    {
        // Summary:
        //     The source of the membership is a distribution list (DL).
        DistributionList = 0,
        //
        // Summary:
        //     The source of the membership is a SharePoint site.
        SharePointSite = 1,
        //
        // Summary:
        //     The source of the membership is a custom membership that a user created.
        Other = 2,
    }

    public enum AveMembershipGroupType
    {
        // Summary:
        //     Membership group is a custom membership group.
        UserSpecified = 0,
        //
        // Summary:
        //     Membership group is a Distribution List (DL).
        DistributionList = 7,
        //
        // Summary:
        //     Membership group is a SharePoint site.
        SharePointSite = 8,
    }

    public enum AvePrivacy
    {
        // Summary:
        //     Privacy level gives visibility of users' profile properties, and other My
        //     Site content, to everyone.
        Public = 1,
        //
        // Summary:
        //     Privacy level limits the visibility of users' profile properties, and other
        //     My Site content, to my colleagues.
        Contacts = 2,
        //
        // Summary:
        //     Privacy level limits the visibility of users' profile properties, and other
        //     My Site content, to my workgroup.
        Organization = 4,
        //
        // Summary:
        //     Privacy level limits the visibility of users' profile properties, and other
        //     My Site content, to my manager and me.
        Manager = 8,
        //
        // Summary:
        //     Privacy level limits the visibility of users' profile properties, and other
        //     My Site content, to me only.
        Private = 16,
        //
        // Summary:
        //     Privacy level is not set.
        NotSet = 1073741824,
    }

    // Summary:
    //     Defines the privacy policy for whatever a user is applying to.
    public enum AvePrivacyPolicy
    {
        // Summary:
        //     Makes it a requirement that the user fill in a value.
        Mandatory = 1,
        //
        // Summary:
        //     Opt-in to provide a privacy policy value for a property.
        OptIn = 2,
        //
        // Summary:
        //     Opt-out from providing a privacy policy value for a property.
        OptOut = 4,
        //
        // Summary:
        //     Turns off the feature and hides all related user interface.
        Disabled = 8,
    }

    // Summary:
    //     Defines the type of separator character used to separate multiple values
    //     for a property.
    public enum AveMultiValueSeparator
    {
        // Summary:
        //     The separator character for multiple values is a comma.
        Comma = 0,
        //
        // Summary:
        //     The separator character for multiple values is a semicolon.
        Semicolon = 1,
        //
        // Summary:
        //     The separator character for multiple values is a new line character.
        Newline = 2,
        //
        // Summary:
        //     The separator character for multiple values is an unknown character.
        Unknown = 255,
    }

    // Summary:
    //     Defines the type of quick link group.
    public enum AveQuickLinkGroupType
    {
        // Summary:
        //     The quick link group is a custom type associated with a specific link.
        UserSpecified = 0,
        //
        // Summary:
        //     The quick link group type is a best bet associated with a specific link.
        BestBet = 1,
        //
        // Summary:
        //     The quick link group is a general type associated with a specific link.
        General = 2,
        //
        DocumentLibrary = 3,
        //
        AssetLibrary = 4,
        //
        ProcessRepository = 6,
    }

    public enum AveWeekOfMonth
    {
        First,
        Second,
        Third,
        Fourth,
        Last
    }

    public enum AveIisWebServiceApplicationPoolProvisioningOptions
    {
        None,
        UpdateOnly
    }

    public enum AveSolutionDeploymentState
    {
        NotDeployed,
        GlobalDeployed,
        WebApplicationDeployed,
        GlobalAndWebApplicationDeployed
    }

    public enum AveSolutionOperationResult
    {
        NoOperationPerformed,
        RetractionSucceeded,
        DeploymentSucceeded,
        RetractionWarningsOccurred,
        DeploymentWarningsOccurred,
        DeploymentFailedCabExtraction,
        DeploymentSolutionValidationFailed,
        DeploymentFailedFileCopy,
        DeploymentFailedFeatureInstall,
        RetractionFailedCouldNotRemoveFile,
        RetractionFailedCouldNotRemoveFeature,
        DeploymentFailedCallout
    }

    public enum AveSolutionDeploymentJobType
    {
        Deploy,
        Retract,
        Upgrade
    }

    public enum AveViewFlags : long
    {
        AggregationView = 0x400L,
        Calendar = 0x80000L,
        Chart = 0x20000L,
        ClientModified = 2L,
        Contributor = 0x4000L,
        Default = 0x100000L,
        DefaultMobile = 0x1000000L,
        DefaultViewForContentType = 0x10000000L,
        FailIfEmpty = 0x40L,
        FileDialog = 0x100L,
        FileDialogTemplates = 0x200L,
        FilesOnly = 0x200000L,
        FreeForm = 0x80L,
        Gantt = 0x4000000L,
        Grid = 0x800L,
        Hidden = 8L,
        HideUnapproved = 0x20000000L,
        Html = 1L,
        IncludeRootFolder = 0x8000000L,
        IncludeVersions = 0x2000000L,
        LockWeb = 0x10L,
        Mobile = 0x800000L,
        Moderator = 0x8000L,
        None = 0L,
        Ordered = 0x400000L,
        Personal = 0x40000L,
        ReadOnly = 0x20L,
        RecurrenceRowset = 0x2000L,
        Recursive = 0x1000L,
        RequiresClientIntegration = 0x40000000L,
        TabularView = 4L,
        Threaded = 0x10000L,
        Unknown = 0x80000000L
    }

    public enum AveWebAnonymousState
    {
        Disabled,
        Enabled,
        On
    }

    public enum AveTriState
    {
        False,
        True,
        NA
    }

    public enum AveAnonymousPolicy
    {
        None,
        DenyWrite,
        DenyAll
    }

    public enum AvePolicyRoleType
    {
        None,
        DenyAll,
        DenyWrite,
        FullRead,
        FullControl
    }

    public enum AveScopeCompilationState
    {
        Empty,
        Invalid,
        QueryExpanded,
        NeedsCompile,
        Compiled,
        NeedsRecompile
    }

    public enum AveScopeCompilationType
    {
        ConditionalCompile,
        AlwaysCompile
    }

    public enum AveScopeRuleFilterBehavior
    {
        Include,
        Require,
        Exclude
    }

    public enum AveScopeRuleType
    {
        AllContent,
        Url,
        PropertyQuery
    }

    public enum AveUrlScopeRuleType
    {
        Folder,
        HostName,
        Domain
    }

    public enum AveManagedDataType
    {
        Unsupported,
        Text,
        Integer,
        Decimal,
        DateTime,
        YesNo,
        Binary
    }

    public enum AveUserSolutionStatus
    {
        Deactivated,
        Activated,
        Disabled
    }

    public enum AveMode
    {
        Add,
        Edit,
        Delete
    }

    public enum AveChildType
    {
        BestBets,
        Boosts,
        Demotions,
        FeaturedContent
    }

    public enum AveSynonymExpansionType
    {
        OneWay = 0,
        TwoWay = 1
    }

    public enum AveSortDirection
    {
        Ascending,
        Descending
    }

    public enum AveExpressionTypes : byte
    {
        AND = 0,
        MATCH = 3,
        NOT = 1,
        OR = 2
    }

    public enum AveSiteHitRuleBehavior
    {
        SimultaneousRequests,
        DelayBetweenRequests
    }

    public enum AvePromotionMode
    {
        B,
        D
    }

    public enum AveLockType
    {
        Exclusive,
        Shared,
        None
    }

    public enum AveORole
    {
        None,
        Index,
        Query,
        IndexQuery
    }

    public enum AveDeploymentObjectType
    {
        File = 5,
        Folder = 2,
        Invalid = 0x63,
        List = 3,
        ListItem = 4,
        Site = 0,
        Web = 1
    }

    public enum AveAuditEventType
    {
        AuditMaskChange = 14,
        CheckIn = 2,
        CheckOut = 1,
        ChildDelete = 7,
        ChildMove = 0x10,
        Copy = 12,
        Custom = 100,
        Delete = 4,
        EventsDeleted = 50,
        FileFragmentWrite = 0x11,
        Move = 13,
        ProfileChange = 6,
        SchemaChange = 8,
        Search = 15,
        SecGroupCreate = 30,
        SecGroupDelete = 0x1f,
        SecGroupMemberAdd = 0x20,
        SecGroupMemberDel = 0x21,
        SecRoleBindBreakInherit = 40,
        SecRoleBindInherit = 0x27,
        SecRoleBindUpdate = 0x26,
        SecRoleDefBreakInherit = 0x25,
        SecRoleDefCreate = 0x22,
        SecRoleDefDelete = 0x23,
        SecRoleDefModify = 0x24,
        Undelete = 10,
        Update = 5,
        View = 3,
        Workflow = 11
    }

    public enum AveAPIType
    {
        Unknown = 0,
        Server = 1,
        BPOS_D = 2,
        BPOS_S = 3
    }

    public enum AveCrawlStatus
    {
        CrawlCompleting = 13,
        CrawlingFull = 1,
        CrawlingIncremental = 6,
        CrawlPausing = 9,
        CrawlResuming = 11,
        CrawlStarting = 12,
        CrawlStopping = 8,
        Idle = 0,
        Paused = 2,
        ProcessingNotifications = 7,
        Recovering = 4,
        ShuttingDown = 5,
        Throttled = 3
    }

    public enum AveRankingUpdateType
    {
        FullUpdate,
        ClickDistanceUpdate,
        QueryIndependentRankRefresh
    }

    public enum AveDaysOfMonth
    {
        Day1 = 1,
        Day10 = 0x200,
        Day11 = 0x400,
        Day12 = 0x800,
        Day13 = 0x1000,
        Day14 = 0x2000,
        Day15 = 0x4000,
        Day16 = 0x8000,
        Day17 = 0x10000,
        Day18 = 0x20000,
        Day19 = 0x40000,
        Day2 = 2,
        Day20 = 0x80000,
        Day21 = 0x100000,
        Day22 = 0x200000,
        Day23 = 0x400000,
        Day24 = 0x800000,
        Day25 = 0x1000000,
        Day26 = 0x2000000,
        Day27 = 0x4000000,
        Day28 = 0x8000000,
        Day29 = 0x10000000,
        Day3 = 4,
        Day30 = 0x20000000,
        Day31 = 0x40000000,
        Day4 = 8,
        Day5 = 0x10,
        Day6 = 0x20,
        Day7 = 0x40,
        Day8 = 0x80,
        Day9 = 0x100,
        Everyday = 0x7fffffff
    }

    public enum AveMonthsOfYear
    {
        AllMonths = 0xfff,
        April = 8,
        August = 0x80,
        December = 0x800,
        February = 2,
        January = 1,
        July = 0x40,
        June = 0x20,
        March = 4,
        May = 0x10,
        November = 0x400,
        October = 0x200,
        September = 0x100
    }

    public enum AveMessageType
    {
        Success,
        Warning,
        Error
    }

    public enum AveCrawlLogFilterProperty
    {
        UrlLogTime,
        MessageType,
        ContentSourceId,
        MessageId,
        Url,
        HostName,
        StartAt,
        TotalEntries,
        CatalogType
    }

    public enum AveCatalogType
    {
        PortalContent,
        ProfileContent
    }

    public enum AveDaysOfWeek
    {
        Everyday = 0x7f,
        Friday = 0x20,
        Monday = 2,
        Saturday = 0x40,
        Sunday = 1,
        Thursday = 0x10,
        Tuesday = 4,
        Wednesday = 8,
        Weekdays = 0x3e,
        Weekends = 0x41
    }

    /// <summary>
    /// 这个枚举的顺序和SP2013里的顺序是不一致的
    /// 使用是要注意用AveTypeHelper.ParseEnum<T>(object value)
    /// 转换成需要的值
    /// </summary>
    public enum AveContentSourceType
    {
        Business,
        O12Business,
        CustomRepository,
        Custom,
        Exchange,
        File,
        LotusNotes,
        SharePoint,
        Web,
        TopicPages,
        PushedContent
    }

    [Flags]
    public enum AveTaxonomyRights : ulong
    {
        AddManageTermStorePermissions = 0x20L,
        AddTermSetEditPermissions = 8L,
        All = 0xfffL,
        Contributor = 0x40L,
        EditGroup = 4L,
        EditTerm = 1L,
        EditTermSet = 2L,
        GroupManager = 0x80L,
        ManageTermStore = 0x10L,
        None = 0L,
        TermStoreAdministrator = 0x100L
    }

    public enum AveSearchProvider
    {
        Default,
        SharepointSearch,
        FASTSearch
    }

    public enum AveSearchServiceApplicationType
    {
        Regular,
        ExtendedConnector
    }

    public enum AveOCrawlRuleType
    {
        InclusionRule,
        ExclusionRule
    }

    public enum AvePropagationStatus
    {
        NoPropagation,
        Idle,
        Propagating,
        WaitingForInitialization,
        QueryComponentNotResponding
    }

    public enum AveScopesCompilationState
    {
        Idle,
        Compiling
    }

    public enum AveScopesCompilationScheduleType
    {
        None,
        Automatic,
        Custom
    }

    public enum AveAdminCompoentSummaryState
    {
        Initializing,
        Online,
        NotResponding
    }

    public enum AveCrawlComponentSummaryState
    {
        Initializing,
        Online,
        NotResponding,
        Recovering,
        Disabled,
        Deleting
    }

    public enum AveQueryComponentSummaryState
    {
        SetupTopology,
        Initializing,
        Online,
        NotResponding,
        Disabled,
        Recovering,
        Deleting
    }

    public enum AveOLocationType
    {
        Custom = 0xff,
        FASTSearch = 5,
        LocalSharepoint = 0,
        OpenSearch = 2
    }

    [Obsolete("Use AveOManagedDataType instead, this enum may remove later.")]
    public enum AveOManagedType
    {
        Unsupported,
        Text,
        Integer,
        Decimal,
        DateTime,
        YesNo,
        Binary,
        Double
    }

    public enum AveOManagedDataType
    {
        Unsupported,
        Text,
        Integer,
        Decimal,
        DateTime,
        YesNo,
        Binary,
        Double
    }

    public enum AveOFederationAuthType
    {
        [EnumMember]
        Anonymous = 0,
        [EnumMember]
        ApplicationPoolIdentity = 5,
        [EnumMember]
        Custom = 0x10,
        [EnumMember]
        Kerberos = 7,
        [EnumMember]
        LocalNTAuth = 1,
        [EnumMember]
        PerUserBasicAuth = 15,
        [EnumMember]
        PerUserCookie = 9,
        [EnumMember]
        PerUserCustom = 0x11,
        [EnumMember]
        PerUserDigest = 14,
        [EnumMember]
        PerUserFormsAuthentication = 8,
        [EnumMember]
        PerUserNTLM = 12,
        [EnumMember]
        SingleAccountBasicAuth = 2,
        [EnumMember]
        SingleAccountCookie = 4,
        [EnumMember]
        SingleAccountDigest = 13,
        [EnumMember]
        SingleAccountFormsAuthentication = 3,
        [EnumMember]
        SingleAccountNTLM = 11,
        [EnumMember]
        SSO = 10
    }

    public enum AveManagedType
    {
        Boolean = 3,
        Datetime = 6,
        Decimal = 5,
        Float = 4,
        Integer = 2,
        Text = 1
    }

    public enum AveWorkflowStatus
    {
        Completed = 5,
        ErrorOccurred = 3,
        ErrorOccurredRetrying = 7,
        FailedOnStart = 1,
        FailedOnStartRetrying = 6,
        InProgress = 2,
        Max = 15,
        NotStarted = 0,
        StoppedByUser = 4,
        ViewQueryOverflow = 8
    }
    public enum AveIterationGranularity
    {
        Item,
        List,
        Web,
        SiteCollection,
        WebApplication,
        Service
    }

    public enum AveQueryThrottleOption
    {
        Default,
        Override,
        Strict
    }

    public enum AveULSTraceLevel
    {
        High = 20,
        Medium = 50,
        Monitorable = 15,
        Unexpected = 10,
        Verbose = 100,
        VerboseEx = 200
    }

    public enum ViewUrlKind
    {
        SiteQuotaView,
        DataBaseView
    }

    public enum AveDirtyItemOp
    {
        None,
        Add,
        Delete,
        Change
    }

    public enum AveVirusCheckStatus
    {
        Clean,
        Infected,
        InfectedCleanable,
        Cleaned,
        CleanFailed,
        Deleted,
        Timeout
    }

    public enum AveSharePointCrawlBehavior
    {
        CrawlVirtualServers,
        CrawlSites
    }

    public enum AveServiceApplicationType
    {
        UserProfileService,
        BDCService,
        ManagedMetadataService,
        ManagedMetadataServiceApplication,
        ManagedMetadataServiceApplicationUtilities,
        PartionSettings,
    }

    [DataContract]
    public enum AveMethodInstanceType
    {
        [EnumMember]
        AccessChecker = 7,
        [EnumMember]
        AssociationNavigator = 13,
        [EnumMember]
        Associator = 14,
        [EnumMember]
        BinarySecurityDescriptorAccessor = 0x11,
        [EnumMember]
        BulkAssociatedIdEnumerator = 0x16,
        [EnumMember]
        BulkAssociationNavigator = 0x17,
        [EnumMember]
        BulkIdEnumerator = 0x18,
        [EnumMember]
        BulkSpecificFinder = 20,
        [EnumMember]
        ChangedIdEnumerator = 11,
        [EnumMember]
        Creator = 8,
        [EnumMember]
        DeletedIdEnumerator = 12,
        [EnumMember]
        Deleter = 10,
        [EnumMember]
        Disassociator = 15,
        [EnumMember]
        Finder = 1,
        [EnumMember]
        GenericInvoker = 4,
        [EnumMember]
        IdEnumerator = 5,
        [EnumMember]
        Scalar = 6,
        [EnumMember]
        SpecificFinder = 2,
        [EnumMember]
        StreamAccessor = 0x10,
        [EnumMember]
        Updater = 9
    }

    [Flags]
    public enum AveObjectTypes
    {
        All = 0x1fff,
        Anniversary = 4,
        Colleague = 0x40,
        Custom = 0x400,
        DLMembership = 8,
        MultiValueProperty = 2,
        None = 0,
        OrganizationMembership = 0x1000,
        OrganizationProfile = 0x800,
        PersonalizationSite = 0x80,
        QuickLink = 0x20,
        SingleValueProperty = 1,
        SiteMembership = 0x10,
        UserProfile = 0x100,
        WebLog = 0x200
    }

    [Flags]
    public enum AveOrganizationMembershipType
    {
        Leader = 2,
        Member = 1
    }

    #region add for SP2013
    public enum AveProjectPolicyOption
    {
        NotHavePolicy,
        CanHavePolicy,
        MustHavePolicy
    }

    public enum AveSPDateTimeFieldFriendlyFormatType
    {
        Unspecified,
        Disabled,
        Relative
    }

    public enum AveSearchComponentType
    {
        Unspecified,
        AdminComponent,
        IndexComponent,
        QueryProcessingComponent,
        CrawlComponent,
        ContentProcessingComponent,
        AnalyticsProcessingComponent
    }

    public enum AveTermSetItemType
    {
        Term,
        TermSet
    }

    public enum AveStandardNavigationSource
    {
        Unknown = 0,
        PortalProvider = 1,
        TaxonomyProvider = 2,
        InheritFromParentWeb = 3,
    }

    public enum AveScriptSafeExternalEmbedding
    {
        None = 0,
        AllowedDomains = 1,
        All = 2,
    }

    public enum AveChangeTypes
    {
        Add = 1,
        All = 15,
        Delete = 4,
        Metadata = 8,
        Modify = 2,
        None = 0
    }




    #endregion

    public enum AveAuditItemType
    {
        Document = 1,
        Folder = 5,
        List = 4,
        ListItem = 3,
        Site = 7,
        Web = 6
    }

    public enum AveAuditEventSource
    {
        SharePoint,
        ObjectModel
    }

    public enum AveAuditLocationType
    {
        ClientLocation = 1,
        Invalid = -1,
        Url = 0
    }

    public enum EBSType07
    {
        AvepointStub,
        AvepointBlob,
        FSDL,
    }

    public enum AveDateFormat
    {
        DateTime,
        DateOnly,
        TimeOnly,
        ISO8601,
        MonthDayOnly,
        MonthYearOnly,
        LongDate,
        UnknownFormat
    }

    public enum AveWebConfigModificationType
    {
        EnsureChildNode,
        EnsureAttribute,
        EnsureSection
    }

    public enum AveOriginalIssuerType
    {
        ClaimProvider = 0x63,
        Forms = 0x66,
        SecurityTokenService = 0x73,
        TrustedProvider = 0x74,
        Unknown = 0,
        Windows = 0x77
    }

    #region Add for Change Log

    public enum AveChangeType
    {
        Add = 1,
        AssignmentAdd = 11,
        AssignmentDelete = 12,
        Delete = 3,
        ListContentTypeAdd = 0x13,
        ListContentTypeDelete = 20,
        MemberAdd = 13,
        MemberDelete = 14,
        MoveAway = 5,
        MoveInto = 6,
        Navigation = 0x10,
        NoChange = 0,
        Rename = 4,
        Restore = 7,
        RoleAdd = 8,
        RoleDelete = 9,
        RoleUpdate = 10,
        ScopeAdd = 0x11,
        ScopeDelete = 0x12,
        SystemUpdate = 15,
        Update = 2
    }

    public enum AveCollectionScope
    {
        ContentDB = 0,
        Site = 1,
        Web = 2,
        List = 3
    }

    #endregion

    #region Add for Social Feed
    public enum AveOSocialFeedSortOrder
    {
        ByModifiedTime = 0,
        ByCreatedTime = 1,
    }

    [Flags]
    public enum AveOSocialThreadAttributes : long
    {
        None = 0,
        IsDigest = 1,
        CanReply = 2,
        CanLock = 4,
        IsLocked = 8,
        ReplyLimitReached = 16,
    }

    [Flags]
    public enum AveOSocialPostAttributes
    {
        None = 0,
        CanLike = 1,
        CanDelete = 2,
        UseAuthorImage = 4,
        UseSmallImage = 8,
        CanFollowUp = 16,
    }

    public enum AveOSocialAttachmentKind
    {
        Image = 0,
        Video = 1,
        Document = 2,
    }

    public enum AveOSocialActorType
    {
        User = 0,
        Document = 1,
        Site = 2,
        Tag = 3,
    }

    [Flags]
    public enum AveOSocialActorTypes
    {
        All = 15,
        Documents = 2,
        ExcludeContentWithoutFeeds = 0x10000000,
        None = 0,
        Sites = 4,
        Tags = 8,
        Users = 1
    }

    public enum AveOSocialStatusCode
    {
        OK = 0,
        InvalidRequest = 1,
        AccessDenied = 2,
        ItemNotFound = 3,
        InvalidOperation = 4,
        ItemNotModified = 5,
        InternalError = 6,
        CacheReadError = 7,
        CacheUpdateError = 8,
        PersonalSiteNotFound = 9,
        FailedToCreatePersonalSite = 10,
        NotAuthorizedToCreatePersonalSite = 11,
        CannotCreatePersonalSite = 12,
        LimitReached = 13,
        AttachmentError = 14,
        PartialData = 15,
        FeatureDisabled = 16,
    }

    public enum AveOSocialDataOverlayType
    {
        Link,
        Actors
    }

    public enum AveOSocialFollowResult
    {
        OK,
        AlreadyFollowing,
        LimitReached,
        InternalError
    }

    public enum AveOSocialThreadType
    {
        Normal,
        LikeReference,
        ReplyReference,
        MentionReference,
        TagReference
    }

    public enum AveOSocialPostType
    {
        Root,
        Reply
    }

    public enum AveOSocialDataItemType
    {
        User,
        Document,
        Site,
        Tag,
        Link
    }

    #endregion

    #region Add for SP Apps
    public enum AveAppInstanceStatus
    {
        Canceling = 7,
        Disabled = 12,
        Disabling = 11,
        Initialized = 9,
        Installed = 5,
        Installing = 1,
        InvalidStatus = 0,
        Uninstalling = 4,
        UpgradeCanceling = 10,
        Upgrading = 8
    }

    public enum AveAppSource
    {
        InvalidSource,
        Marketplace,
        CorporateCatalog,
        DeveloperSite,
        ObjectModel,
        RemoteObjectModel,
        SiteCollectionCorporateCatalog
    }

    public enum AveAppPrincipalPermissionKind
    {
        None,
        Guest,
        Read,
        Write,
        Manage,
        FullControl
    }

    #endregion

    /// <summary>
    /// Wrapper Native Api Permission
    /// </summary>
    public enum WrapperNativeApiPermission
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// only API
        /// </summary>
        Api = 1,

        /// <summary>
        /// Internal API + API
        /// </summary>
        ApiNative = 2,

        /// <summary>
        /// Native reader + API + API native
        /// </summary>
        NativeRead = 4,

        /// <summary>
        /// has enough permission
        /// </summary>
        FullControl = 0xFFFF,
    }

    /// <summary>
    /// adding for IAvePortalNavigation
    /// </summary>
    public enum AveAutomaticSortingMethod
    {
        Title,
        CreatedDate,
        LastModifiedDate
    }

    /// <summary>
    /// Adding for IAvePortalNavigation
    /// </summary>
    public enum AveOrderingMethod
    {
        Automatic,
        ManualWithAutomaticPageSorting,
        Manual
    }

    // Summary:
    //     Used to specify the mode of a SPDataSource.
    public enum AveDataSourceMode
    {
        List = 0,
        ListOfLists = 1,
        CrossList = 2,
        Webs = 3,
        ListItem = 4,
    }

    public enum AveUserCustomActionScope
    {
        Unknown = 0,
        Site = 2,
        Web = 3,
        List = 4
    }

    public enum AveUserCustomActionRegistrationType
    {
        None = 0,
        List = 1,
        ContentType = 2,
        ProgId = 3,
        FileType = 4
    }

    public enum AveChangedItemType
    {
        Unknown,
        Term,
        TermSet,
        Group,
        TermStore,
        Site
    }

    public enum AveChangedOperationType
    {
        Add = 1,
        Copy = 5,
        Delete = 3,
        Edit = 2,
        Import = 8,
        Merge = 7,
        Move = 4,
        PathChange = 6,
        Restore = 9,
        Unknown = 0
    }


}
