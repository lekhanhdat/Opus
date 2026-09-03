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
    /// <summary>
    /// 所有的Info都是为了创建基本的site，web，list而使用，至于其他的属性应该归纳与setting范围。
    /// Title和Description都属于setting部分，所以这两个属性有部分还是重合了。
    /// </summary>
    [DataContract]
    public class AveWebInfo
    {
        [DataMember]
        public string Url;
        /// <summary>
        /// Name == ServerRelativeUrl
        /// </summary>
        [DataMember]
        public string Name;
        [DataMember]
        public string Title;
        [DataMember]
        public string Description;
        [DataMember]
        public uint LCID;
        [DataMember]
        public string WebTemplate;
        [DataMember]
        public Guid OldWebId;
        [DataMember]
        public string LookupFieldsXml;
        [DataMember]
        public bool IsRootWeb;
        [DataMember]
        public bool HasUniqueRoleDefinitions;
        //public string MasterUrl;
        //public string CustomMasterUrl;
        //public AveWebSettingInfo WebSettingInfo;
        [DataMember]
        public AveWebInfo parentWebInfo;
        [DataMember]
        public int WorkingLanguage;

        #region add for SP App
        [DataMember]
        public bool IsAppWeb;
        [DataMember]
        public Guid AppInstanceId;
        #endregion

        public string SrcUrl;
        public long Size;
    }

    [DataContract]
    public class AveWebSettingInfo
    {
        //public Guid ID = Guid.Empty;// [uniqueidentifier] NOT NULL,
        //public Guid SiteId = Guid.Empty;// [uniqueidentifier] NOT NUL;
        //public string FullUrl = string.Empty;// [nvarchar](256) NOT NULL;
        //public Guid ParentWebId = Guid.Empty;// [uniqueidentifier] NULL;
        [DataMember]
        public AveRestorableProperty<bool> TreeViewEnabled;
        [DataMember]
        public AveRestorableProperty<bool> SyndicationEnabled;
        [DataMember]
        public AveRestorableProperty<bool> ParserEnabled;
        [DataMember]
        public AveRestorableProperty<bool> PresenceEnabled;
        [DataMember]
        public AveRestorableProperty<int> ASPXPageIndexMode;
        [DataMember]
        public AveRestorableProperty<short> ProductVersion;//[smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<short> TemplateVersion;// [smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Guid> FirstUniqueAncestorWebId;// [uniqueidentifier] NOT NULL,
        [DataMember]
        public AveRestorableProperty<int> Author;// [int] NOT NULL,
        //public string Title;//] [nvarchar](255) NULL,
        [DataMember]
        public AveRestorableProperty<DateTime> TimeCreated;//] [datetime] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Dictionary<string, Dictionary<string, Dictionary<Guid, string>>>> NavigationWebAndPage = new AveRestorableProperty<Dictionary<string, Dictionary<string, Dictionary<Guid, string>>>>();
        [DataMember]
        public AveRestorableProperty<Dictionary<string, string>> NavNodeIdUrlMapping = new Dictionary<string, string>();
        [DataMember]
        public AveRestorableProperty<int> CachedNavDirty;//] [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> CachedNav;//] [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> CachedInheritedNav;//] [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<string> CachedNavScope;//] [nvarchar](max) NULL,
        [DataMember]
        public AveRestorableProperty<int> CachedDataVersion;//] [int] NOT NULL,
        //public string Description;// [nvarchar;//](max) NULL,
        [DataMember]
        public AveRestorableProperty<Guid> ScopeId;// [uniqueidentifier;//] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<Guid>> SecurityProvider = new Nullable<Guid>();//] [uniqueidentifier] NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> MetaInfo;//] [varbinary;//](max) NULL,
        [DataMember]
        public AveRestorableProperty<int> MetaInfoVersion;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<DateTime> LastMetadataChange;// [datetime] NOT NULL,
        [DataMember]
        public AveRestorableProperty<int> NavStructNextEid;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<Guid>> NavParentWebId = new Nullable<Guid>(); // [uniqueidentifier] NULL,
        [DataMember]
        public AveRestorableProperty<int> NextWebGroupId;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<string> DefTheme;// [nvarchar](64) NULL,
        [DataMember]
        public AveRestorableProperty<string> AlternateCSSUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> CustomizedCss;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> CustomJSUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> AlternateHeaderUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> DailyUsageData;// [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<int> DailyUsageDataVersion;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> MonthlyUsageData;// [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<int> MonthlyUsageDataVersion;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<short> DayLastAccessed;// [smallint] NOT NULL,
        //public int WebTemplate;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<int> Language;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<int> Locale;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<short> TimeZone;// [smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> Time24 = new Nullable<bool>();// [bit] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> CalendarType = new Nullable<short>();// [smallint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> AdjustHijriDays = new Nullable<short>();// [smallint] NULL,
        [DataMember]
        public AveRestorableProperty<short> MeetingCount;// [smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<short> ProvisionConfig;// [smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<int> Flags;// [int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<short> Collation;// [smallint] NOT NULL,

        #region access requests settings
        [DataMember]
        public AveRestorableProperty<string> RequestAccessEmail;// [nvarchar](255) NULL,
        [DataMember]
        public AveRestorableProperty<bool> MembersCanShare;
        [DataMember]
        public AveRestorableProperty<bool> AllowMembersEditMembership;
        [DataMember]
        public AveRestorableProperty<bool> UseAccessRequestDefault;
        [DataMember]
        public AveRestorableProperty<string> AccessRequestSiteDescription;//inherit site has it's own value

        #endregion

        [DataMember]
        public AveRestorableProperty<string> MasterUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> CustomMasterUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> SiteLogoUrl;// [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> SiteLogoDescription;// [nvarchar](255) NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> AuditFlags = new Nullable<int>();// [int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> InheritAuditFlags = new Nullable<int>();// [int] NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> Ancestry;// [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<byte>> AltCalendarType = new Nullable<byte>();// [tinyint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<byte>> CalendarViewOptions = new Nullable<byte>();// [tinyint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> WorkDays = new Nullable<short>();// [smallint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> WorkDayStartHour = new Nullable<short>();// [smallint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> WorkDayEndHour = new Nullable<short>();//] [smallint] NULL,
        //public AveRestorableProperty<Nullable<byte>> UIVersion = new Nullable<byte>();// [tinyint] NULL,
        [DataMember]
        public AveRestorableProperty<short> ClientTag; // add for version B
        [DataMember]
        public AveRestorableProperty<bool> AllowMUI;// [bit] NOT NULL
        [DataMember]
        public AveRestorableProperty<uint> LocaleId;

        #region -- add for web master page setting
        [DataMember]
        public AveRestorableProperty<bool> CInheriting;
        [DataMember]
        public AveRestorableProperty<string> CPageUrl;
        [DataMember]
        public AveRestorableProperty<bool> MInheriting;
        [DataMember]
        public AveRestorableProperty<string> MPageUrl;
        [DataMember]
        public AveRestorableProperty<bool> InheritAlertCss;
        [DataMember]
        public AveRestorableProperty<string> InheritAlertCssUrl;
        #endregion

        /***** add for B ********/
        [DataMember]
        public AveRestorableProperty<bool> IsMultilingual;
        [DataMember]
        public AveRestorableProperty<bool> OverwriteTranslationsOnChange;
        [DataMember]
        public AveRestorableProperty<string> ThemedCssUrl;
        [DataMember]
        public AveRestorableProperty<string> ThemedCssFolderUrl;
        [DataMember]
        public AveRestorableProperty<string> ThemedTemplate;//use for restoring subsite theme
        [DataMember]
        public AveRestorableProperty<string> ThemedTitle;
        [DataMember]
        public AveRestorableProperty<string> ThemedFontUrl;
        [DataMember]
        public AveRestorableProperty<string> ThemedColorUrl;
        [DataMember]
        public AveRestorableProperty<string> ThemedImageUrl;
        [DataMember]
        public AveRestorableProperty<byte[]> ThemedImageContent;
        [DataMember]
        public AveRestorableProperty<string> ThemedMasterPageUrl;
        [DataMember]
        public AveRestorableProperty<bool> InheritsThemedCssFolderUrl;
        [DataMember]
        public AveRestorableProperty<string> ServerRelativeUrl;
        [DataMember]
        public AveRestorableProperty<bool> ExcludeFromOfflineClient;
        [DataMember]
        public AveRestorableProperty<string> Title; //[nvarchar 255] null
        [DataMember]
        public AveRestorableProperty<string> Description; //[nvarchar(max)] null
        [DataMember]
        public AveRestorableProperty<bool> HasUniqueRoleAssignments;
        [DataMember]
        public AveRestorableProperty<string> Theme;
        [DataMember]
        public AveRestorableProperty<bool> QuickLaunchEnabled;
        [DataMember]
        public AveRestorableProperty<bool> UserSharedNav;
        [DataMember]
        public AveRestorableProperty<bool> AllowUnsafeUpdate;
        [DataMember]
        public AveRestorableProperty<string> WelcomePage;
        [DataMember]
        public AveRestorableProperty<bool> UiversionConfigurationEnable;
        [DataMember]
        public AveRestorableProperty<int> Uiversion;
        [DataMember]
        public AveRestorableProperty<int> AnonymousState;
        [DataMember]
        public AveRestorableProperty<AveWebThemeInfo> WebTheme;
        [DataMember]
        public AveRestorableProperty<List<int>> SupportedUICultures;

        [DataMember]
        public AveRestorableProperty<DateTime> LastItemModifiedDate;
        [DataMember]
        public AveRestorableProperty<int> SettingTypes;
        [DataMember]
        public AveRestorableProperty<string> AuditLogReportStorageLocation;
        /// <summary>
        /// add for 3.17.1 NFR
        /// </summary>
        [DataMember]
        public AveRestorableProperty<Dictionary<string,string>> TitleResourceInfo;
        /// <summary>
        /// add for 3.17.1 NFR
        /// </summary>
        [DataMember]
        public AveRestorableProperty<Dictionary<string, string>> DescriptionResourceInfo;
        #region Modern look and feel
        //Theme
        public AveRestorableProperty<AveModernThemeInfo> ModernThemeInfo;
        //Header
        public AveRestorableProperty<int> HeaderEmphasis;
        public AveRestorableProperty<int> HeaderLayout;
        //Navigation
        public AveRestorableProperty<bool> MegaMenuEnabled;
        //Footer
        public AveRestorableProperty<bool> FooterEnabled;
        #endregion
    }

    public class AveWebsTableColumnValue
    {
        public const short AdjustHijriDays = 0;
        public const short AlternateCalendarType = 0;
        public const short CalendarType = 1;
        public const short WorkDayStartHour = 480;
        public const short WorkDayEndHour = 1020;
        public const short WorkDays = 62;
    }

    [Serializable]
    public class AveWebMasterPageInfo
    {
        public bool Inheriting;
        public string PageUrl;
        public bool CInheriting;
        public string CPageUrl;
        public bool MInheriting;
        public string MPageUrl;
    }

    [Serializable]
    public class AveExtendMasterPageInfo : AveWebMasterPageInfo
    {
        public Guid CurrentWebId;
    }

    [DataContract]
    public class AveWebThemeInfo
    {
        public AveWebThemeInfo() { }
        [DataMember]
        public string ThemeName;
        [DataMember]
        public bool InheritsThemedCssFolderUrl;
        [DataMember]
        public string DarkColor1;
        [DataMember]
        public string DarkColor2;
        [DataMember]
        public string LightColor1;
        [DataMember]
        public string LightColor2;
        [DataMember]
        public string AccentColor1;
        [DataMember]
        public string AccentColor2;
        [DataMember]
        public string AccentColor3;
        [DataMember]
        public string AccentColor4;
        [DataMember]
        public string AccentColor5;
        [DataMember]
        public string AccentColor6;
        [DataMember]
        public string HyperlinkColor;
        [DataMember]
        public string FollowedHyperlinkColor;
        [DataMember]
        public string MajorFont;
        [DataMember]
        public string MinorFont;
    }

    [DataContract]
    public class AveModernThemeInfo
    {
        public AveModernThemeInfo() { }
        [DataMember]
        public string ThemedCssFolderUrl;
        [DataMember]
        public string ThemedFontUrl;
        [DataMember]
        public string ThemedColorUrl;
        [DataMember]
        public string ThemedImageUrl;
        [DataMember]
        public byte[] ThemedFontContent;
        [DataMember]
        public byte[] ThemedColorContent;
        [DataMember]
        public byte[] ThemedImageContent;
    }
}
