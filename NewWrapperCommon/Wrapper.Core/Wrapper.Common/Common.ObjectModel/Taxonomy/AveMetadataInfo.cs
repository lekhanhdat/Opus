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
    [DataContract]
    public class AveTermStoreInfo
    {
        [DataMember]
        public int DefaultLanguage;
        [DataMember]
        public List<AveMetadataGroupInfo> Groups = new List<AveMetadataGroupInfo>();
        [DataMember]
        public Guid Id;
        [DataMember]
        public string Name;
        [DataMember]
        public List<AveAceInfo> TermStoreAdministrators = new List<AveAceInfo>();
        [DataMember]
        public int WorkingLanguage;

        //for replicator, support cache function
        [DataMember]
        public DateTime LastAccessTime = DateTime.MinValue;
        [DataMember]
        public Guid UniqueId = Guid.Empty;
        [DataMember]
        public AveTermChangeItem.ChangedOperationType OperationType;
        [DataMember]
        public Guid PartitionId;
        [DataMember]
        public bool IsMetadataParition;
        [DataMember]
        public string SiteUrl;
    }

    [DataContract]
    public class AveMetadataGroupInfo
    {
        // Properties
        //internal override SPAcl<TaxonomyRights> Acl { get; }
        [DataMember]
        public List<AveAceInfo> Contributors = new List<AveAceInfo>();
        [DataMember]
        public string Description;
        [DataMember]
        public List<AveAceInfo> GroupManagers = new List<AveAceInfo>();
        [DataMember]
        public bool IsSiteCollectionGroup;
        [DataMember]
        public bool IsSystemGroup;
        [DataMember]
        public Guid Id;
        [DataMember]
        public string Name;
        //internal override Group ParentGroup { get; }
        //internal SharedGroup SharedGroup { get; }
        //public List<Guid> SiteCollectionAccessIds { get; }
        [DataMember]
        public List<AveTermSetInfo> TermSets = new List<AveTermSetInfo>();
        //internal int TermSetsCount { get; }
        [DataMember]
        public List<Guid> Sites = new List<Guid>();
        [DataMember]
        public AveTermChangeItem.ChangedOperationType OperationType;
        [DataMember]
        public List<string> SiteCollectionReadOnlyAccessUrls=new List<string>();  //for SharePoint 2013
        [DataMember]
        public Guid PartitionId;
        [DataMember]        
		public bool IsMetadataPartition;
    }

    [DataContract]
    public class AveTermSetInfo
    {
        //// Properties
        //internal override List<int> ChildTermIdList { get; }
        [DataMember]
        public string Contact;
        //public override string CustomSortOrder { get; set; }
        [DataMember]
        public string Description;
        //public Group Group { get; }
        [DataMember]
        public bool IsAvailableForTagging;
        [DataMember]
        public bool IsOpenForTermCreation;
        [DataMember]
        public Guid Id;
        [DataMember]
        public string Name;
        //internal string NameInCurrentLcid { get; }
        //internal Dictionary<int, string> Names { get; }
        [DataMember]
        public string Owner;
        //internal override SharedTermSet SharedTermSet { get; }
        [DataMember]
        public List<string> Stakeholders = new List<string>();
        [DataMember]
        public List<AveTermInfo> Terms = new List<AveTermInfo>();
        [DataMember]
        public string CustomSortOrder;
        [DataMember]
        public byte Type;
        [DataMember]
        public Guid ParentId;
        [DataMember]
        public AveTermChangeItem.ChangedOperationType OperationType;
        [DataMember]
        public Dictionary<string, string> CustomProperties;
        //internal int TermsCount { get; }
        //internal TermSetType Type { get; }
        [DataMember]
        public Guid PartitionId;
        [DataMember]
        public bool IsMetadataParition;
    }

    [DataContract]
    public class AveTermInfo
    {
        [DataMember]
        public string TermName;
        [DataMember]
        public bool IsKeyword;
        [DataMember]
        public bool IsRoot;
        [DataMember]
        public string SourceTermName;
        [DataMember]
        public Guid SourceTermId;
        [DataMember]
        public Guid ParentTermSetId;
        [DataMember]
        public Guid ParentTermId;
        // Properties
        //internal override List<int> ChildTermIdList { get; }
        //internal int ClientId { get; set; }
        //public ReadOnlyDictionary<string, string> CustomProperties { get; }
        //public override string CustomSortOrder { get; set; }
        //internal string IdForSearch { get; }
        [DataMember]
        public bool IsAvailableForTagging;
        [DataMember]
        public bool IsDeprecated;
        //public bool IsKeyword { get; }
        public bool IsReused;
        //public bool IsRoot { get; }
        #region for 13
        [DataMember]
        public bool IsPinned;
        [DataMember]
        public Guid PinSourceTermSetId;
        #endregion
        [DataMember]
        public bool IsSourceTerm;
        //internal override string ItemKey { get; }
        [DataMember]
        public List<AveLableInfo> Labels = new List<AveLableInfo>();
        //internal string LeafIdForSearch { get; }
        //internal SharedTermMembership Membership { get; }
        //public ReadOnlyCollection<Guid> MergedTermIds { get; }\
        [DataMember]
        public Guid Id;
        [DataMember]
        public string Name;
        [DataMember]
        public Dictionary<int, string> Description = new Dictionary<int, string>();
        [DataMember]
        public string Owner;
        //public Term Parent { get; }
        //public TermCollection ReusedTerms { get; }
        //internal SharedTerm SharedTerm { get; }
        //internal override SharedTermSet SharedTermSet { get; }
        //public Term SourceTerm { get; }
        [DataMember]
        public List<AveTermInfo> Terms = new List<AveTermInfo>();
        //public int TermsCount { get; }
        //public TermSet TermSet { get; }
        //public TermSetCollection TermSets { get; }
        [DataMember]
        public string CustomSortOrder { get; set; }
        [DataMember]
        public AveTermChangeItem.ChangedOperationType OperationType;
        public Dictionary<string, string> CustomProperties;
        public Dictionary<string, string> LocalCustomProperties;
        [DataMember]
        public Guid PartitionId;
        [DataMember]
        public bool IsMetadataParition;
        /// <summary>
        /// Add in DocAve6.6
        /// </summary>
        [DataMember]
        public List<Guid> MergedTermIds; 


    }

    [DataContract]
    public class AveLableInfo
    {
        // Properties
        [DataMember]
        public bool IsDefaultForLanguage;
        [DataMember]
        public int Language;
        //public Term Term { get; }
        [DataMember]
        public string Value;

        [DataMember]
        public string Description;
    }

    [DataContract]
    public class AveAceInfo
    {
        [DataMember]
        public string DisplayName;
        [DataMember]
        public string PrincipalName;
        [DataMember]
        public ulong DenyRightsMask;
        [DataMember]
        public ulong GrantRightsMask;
    }

    [DataContract]
    public class AveManagedMetadataServiceApplicationInfo
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string DatabaseName;
        [DataMember]
        public string DatabaseServer;
        [DataMember]
        public bool UseWindowsAuthentication;
        [DataMember]
        public string SqlAuthenticationUserName;
        [DataMember]
        public string SqlAuthenticationUserPassword;
        [DataMember]
        public string FailoverDatabaseServer;
        [DataMember]
        public string ContentTypeHub;
        [DataMember]
        public AveIisWebServiceApplicationPoolInfo ApplicationPool;
        [DataMember]
        public bool IsErrorReportEnabled;
    }

    [DataContract]
    public class AveIisWebServiceApplicationPoolInfo
    {
        [DataMember]
        public string Name;
    }

    [DataContract]
    public class AveTermChangeItem
    {
        [DataMember]
        public DateTime ChangeTime;
        [DataMember]
        public Guid Id;
        [DataMember]
        public byte TermSetType;

        [DataMember]
        public ChangedOperationType ChangeType;

        [DataMember]
        public ChangedItemType ItemType;

        [DataMember]
        public int ObjectId;

        [DataMember]
        public Nullable<Guid> TermSetId;
        [DataMember]
        public Guid GroupId;

        [DataMember]
        public string Name;

        [DataMember]
        public string ChangeData;

        [DataMember]
        public Guid PartitionId;
        [DataMember]
        public bool IsMetadataPartition;

        public string Path;

        public string FriendlyChangeData;

        public bool IsNewAdd = false;

        public bool IsSourceTerm = false;

        public bool IsRoot = false;

        public bool IsPinned = false;

        public bool IsReused = false;

        public Guid PinSourceTermSetId = Guid.Empty;

        public bool IsGlobalGroup = true;

        public Guid ParentTermId = Guid.Empty;

        private List<AveTermChangeItem> subTerms;
        public List<AveTermChangeItem> SubTerms
        {
            get 
            {
                if (subTerms == null)
                {
                    subTerms = new List<AveTermChangeItem>();
                }
                return subTerms;
            }
            set
            {
                subTerms = value;
            }
        }

        public enum ChangedItemType
        {
            Unknown,
            Term,
            TermSet,
            Group,
            TermStore,
            Site
        }

        public enum ChangedOperationType
        {
            Unknown,
            Add,
            Edit,
            Delete,
            Move,
            Copy,
            PathChange,
            Merge,
            Import,
            Restore,
            FakeAsParent,
        }
    }

    [DataContract]
    public class ServiceSetting 
    {
        [DataMember]
        public Guid PartitionId;
        [DataMember]
        public string settingsXml;
    }

    /// <summary>
    /// Mapping the table SubscriptionId of SharePoint_Config DB
    /// </summary>
    [DataContract]
    public class AveSiteMapVisible
    {
        [DataMember]
        public Guid SiteId;
        [DataMember]
        public Guid ApplicationId;
        [DataMember]
        public Guid DatabaseId;
        [DataMember]
        public string Path;
        public Int64 Version;
        //[DataMember]
        //public List<AveSiteMapVisible> ManagedSites;
    }

    [DataContract]
    public enum AvePartitionOptions
    {
        UnPartitioned,
        UniquePartitionPerSubscription
    }
}
