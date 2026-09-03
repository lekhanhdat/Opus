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
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 所有的Info都是为了创建基本的site，web，list而使用，至于其他的属性应该归纳与setting范围。
    /// Title和Description都属于setting部分，所以这两个属性有部分还是重合了。
    /// </summary>
    [DataContract]
    [Serializable]
    public class AveSiteInfo
    {
        [DataMember]
        public Guid UniqueId;
        [DataMember]
        public string ServerRelativeUrl;
        [DataMember]
        public bool IsHostheader;
        [DataMember]
        public string WebAppUrl;
        [DataMember]
        public string Url;
        [DataMember]
        public string Title;
        [DataMember]
        public string Description;
        [DataMember]
        public uint LCID;
        [DataMember]
        public string WebTemplate;
        [DataMember]
        public string OwnerLogin;
        [DataMember]
        public string OwnerName;
        [DataMember]
        public string OwnerEmail;
        [DataMember]
        public string SecondaryContactLogin;
        [DataMember]
        public string SecondaryContactName;
        [DataMember]
        public string SecondaryContactEmail;
        [DataMember]
        public int CompatibilityLevel;  //Archiver Site Collection 需要这个属性
        [DataMember]
        public long StorageMaximumLevel;
        [DataMember]
        public double UserCodeMaximumLevel;
        [DataMember]
        public Dictionary<Guid, string> AllWebTemplates;
        [DataMember]
        public List<string> Prefixes = new List<string>();
    }

    [DataContract]
    [Serializable]
    public class AveCreateSiteInfo
    {
        [DataMember]
        public string UserName;
        [DataMember]
        public string Password;
        [DataMember]
        public string AdminUrl;
        [DataMember]
        public long StorageQuota;
        [DataMember]
        public double ResourceQuota;
        [DataMember]
        public ushort EventCategory;
        [DataMember]
        public string CustomerId;
        [DataMember]
        public string AosApiUrl;
        [DataMember]
        public string ClientId;
        [DataMember]
        public string SiteOwnerUPN;
    }

    [DataContract]
    [Serializable]
    public class AveGroupTeamInfo
    {
        [DataMember]
        public bool IsConnectedToTeams { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public List<AveChannelInfo> Channels { get; set; }
    }

    [DataContract]
    [Serializable]
    public class AveChannelInfo
    {
        [DataMember]
        public bool IsPrivateChannel;
        [DataMember]
        public string Id;
        [DataMember]
        public string DisplayName;
        [DataMember]
        public string RelatedSiteUrl;
        [DataMember]
        public string FileFolderUrl;
    }

    [DataContract]
    public class AveSiteSettingInfo
    {
        [DataMember]
        public string TaxonomyHiddenList;

        [DataMember]
        public AveRestorableProperty<Nullable<Guid>> Id = new Nullable<Guid>(); //[uniqueidentifier] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> NextUserOrGroupId = new Nullable<int>(); //[int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> OwnerID = new Nullable<int>(); //[int] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> SecondaryContactID = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> Subscribed = new Nullable<bool>(); //[bit] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<DateTime>> TimeCreated = new Nullable<DateTime>(); //[datetime] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> UsersCount = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> BWUsed = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> DiskUsed = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> SecondStageDiskUsed = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> QuotaTemplateID = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> DiskQuota = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> UserQuota = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> DiskWarning = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<DateTime>> DiskWarned = new Nullable<DateTime>(); //[datetime] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<double>> CurrentResourceUsage = new Nullable<double>(); //[float] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<double>> AverageResourceUsage = new Nullable<double>(); //[float] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<double>> ResourceUsageWarning = new Nullable<double>(); //[float] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<double>> ResourceUsageMaximum = new Nullable<double>(); //[float] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> BitFlags = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> SecurityVersion = new Nullable<long>(); //[bigint] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<DateTime>> CertificationDate = new Nullable<DateTime>(); //[datetime] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<short>> DeadWebNotifyCount = new Nullable<short>(); //[smallint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<string> PortalURL; // [nvarchar](260) NULL,
        [DataMember]
        public AveRestorableProperty<string> PortalName; // [nvarchar](255) NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<DateTime>> LastContentChange = new Nullable<DateTime>(); //[datetime] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<DateTime>> LastSecurityChange = new Nullable<DateTime>(); //[datetime] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> AuditFlags;// = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> InheritAuditFlags;// = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<Guid>> UserInfoListId = new Nullable<Guid>(); //[uniqueidentifier] NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<int>> UserIsActiveFieldRowOrdinal = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public AveRestorableProperty<string> UserIsActiveFieldColumnName; //[nvarchar](64) NULL,
        [DataMember]
        public AveRestorableProperty<string> UserAccountDirectoryPath; //[nvarchar](512) NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<Guid>> RootWebId = new Nullable<Guid>(); // [uniqueidentifier] NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> HashKey; // [binary](16) NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> DomainGroupMapVersion = new Nullable<long>(); // [bigint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<Nullable<long>> DomainGroupMapCacheVersion = new Nullable<long>(); // [bigint] NOT NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> DomainGroupMapCache; // [varbinary](max) NULL,
        [DataMember]
        public AveRestorableProperty<string> HostHeader; // [nvarchar](128) NULL,
        [DataMember]
        public AveRestorableProperty<byte[]> SubscriptionId; // [varbinary](16) NULL
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> SyndicationEnabled = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> UseAuditFlagCache = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<int>> AuditLogTrimmingRetention = new Nullable<int>();
        [DataMember]
        public AveRestorableProperty<string> AuditLogTrimmingCallout;
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> UiversionConfigurationEnable = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> TrimAuditLog = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> AllowDesigner = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> AllowMasterPageEditing = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> AllowRevertFromTemplate = new Nullable<bool>();
        [DataMember]
        public AveRestorableProperty<string> SiteNavigationSettingInfo;
        [DataMember]
        public AveRestorableProperty<List<Guid>> SolutionIdCollection;
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> ShowURLStructure = new Nullable<bool>();
        #region Variations Settings
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> UserConfiguredEnableAutoSpawn;
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> StopAutoSpawnAfterDelete;
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> UpdateWebParts;
        [DataMember]
        public AveRestorableProperty<Nullable<bool>> SendNotificationEmail;
        #endregion
    }

    public class SecurityTrimObject
    {
        public SecurityTrimObject()
        {
            this.TrimmedProperties = new Dictionary<string, object>();
            this.Children = new List<SecurityTrimObject>();//may contains indirectly trimmed properties
        }

        public string Name { get; set; }
        public string ServerRelativeUrl { get; set; }
        public Guid Id { get; set; }
        public int ItemId { get; set; }
        public SecurityTrimLevel Level { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> TrimmedProperties { get; set; }//directly trimmed properties
        public SecurityTrimObject Parent { get; set; }
        public List<SecurityTrimObject> Children { get; set; }

        public override string ToString()
        {
            return TextNode("");
        }

        public SecurityTrimObject GetWeb(string webRelativeUrl, string siteUrl)
        {
            if (this.Level != SecurityTrimLevel.Site)
            {
                return null;
            }
            SecurityTrimObject web = this.Children.Find(w => w.Level == SecurityTrimLevel.Web && w.ServerRelativeUrl.Equals(webRelativeUrl, StringComparison.OrdinalIgnoreCase));
            if (web == null)
            {
                web = new SecurityTrimObject() { Level = SecurityTrimLevel.Web, ServerRelativeUrl = webRelativeUrl, Name = new Uri(new Uri(siteUrl), webRelativeUrl).ToString() };
                this.Children.Add(web);
            }
            return web;
        }

        public SecurityTrimObject GetList(Guid listId, string title)
        {
            if (this.Level != SecurityTrimLevel.Web)
            {
                return null;
            }
            SecurityTrimObject list = this.Children.Find(l => l.Level == SecurityTrimLevel.List && l.Id.Equals(listId));
            if (list == null)
            {
                list = new SecurityTrimObject() { Level = SecurityTrimLevel.List, Name = title, Id = listId };
                this.Children.Add(list);
            }
            else
            {
                if (!string.IsNullOrEmpty(title))
                {
                    list.Name = title;
                }
            }
            return list;
        }

        public SecurityTrimObject GetFolder(string serverRelativeUrl, string name)
        {
            if (this.Level != SecurityTrimLevel.Web && this.Level != SecurityTrimLevel.List)
            {
                return null;
            }
            SecurityTrimObject folder = this.Children.Find(f => f.Level == SecurityTrimLevel.Folder && f.ServerRelativeUrl.Equals(serverRelativeUrl, StringComparison.OrdinalIgnoreCase));
            if (folder == null)
            {
                folder = new SecurityTrimObject() { Level = SecurityTrimLevel.Folder, Name = name, ServerRelativeUrl = serverRelativeUrl };
                this.Children.Add(folder);
            }
            return folder;
        }

        public SecurityTrimObject GetFile(string serverRelativeUrl, string name)
        {
            if (this.Level != SecurityTrimLevel.Web && this.Level != SecurityTrimLevel.Folder)
            {
                return null;
            }
            SecurityTrimObject file = this.Children.Find(f => f.Level == SecurityTrimLevel.Document && f.ServerRelativeUrl.Equals(serverRelativeUrl, StringComparison.OrdinalIgnoreCase));
            if (file == null)
            {
                file = new SecurityTrimObject() { Level = SecurityTrimLevel.Document, Name = name, ServerRelativeUrl = serverRelativeUrl };
                this.Children.Add(file);
            }
            return file;
        }

        public SecurityTrimObject GetListItem(int itemId, string name)
        {
            if (this.Level != SecurityTrimLevel.List && this.Level != SecurityTrimLevel.Folder)
            {
                return null;
            }
            SecurityTrimObject listItem = this.Children.Find(i => i.Level == SecurityTrimLevel.ListItem && i.ItemId == itemId);
            if (listItem == null)
            {
                listItem = new SecurityTrimObject() { Level = SecurityTrimLevel.ListItem, Name = name, ItemId = itemId };
                this.Children.Add(listItem);
            }
            return listItem;
        }

        private string TextNode(string prefix)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(prefix + string.Format("SecurityTrimming Level: {0}, Name: {1}, Type: {2}, Id : {3}, ServerRelativeUrl: {4} \r\n", this.Level.ToString(), this.Name, this.Type == null ? string.Empty : this.Type, this.Id, this.ServerRelativeUrl == null ? string.Empty : this.ServerRelativeUrl));
            foreach (KeyValuePair<string, object> property in this.TrimmedProperties)
            {
                builder.Append(prefix + string.Format("\tTrimmed Property Name: {0}, Trimmed Reason: {1} \r\n", property.Key, property.Value.ToString()));
            }
            foreach (SecurityTrimObject trimObj in this.Children)
            {
                if (trimObj.Level - this.Level == 1)
                {
                    builder.Append(prefix + string.Format("\tSecurityTrimming Level: {0}, Name: {1}, Type: {2}, Id : {3}, ServerRelativeUrl: {4} \r\n", trimObj.Level.ToString(), trimObj.Name, trimObj.Type == null ? string.Empty : trimObj.Type, trimObj.Id, trimObj.ServerRelativeUrl == null ? string.Empty : trimObj.ServerRelativeUrl));
                    foreach (KeyValuePair<string, object> property in trimObj.TrimmedProperties)
                    {
                        builder.Append(prefix + string.Format("\t\tTrimmed Property Name: {0}, Trimmed Reason: {1} \r\n", property.Key, property.Value.ToString()));
                    }
                }
                else
                {
                    builder.Append(trimObj.TextNode(prefix + "\t"));
                }
            }
            return builder.ToString();
        }
    }
}
