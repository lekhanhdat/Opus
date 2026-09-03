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






namespace AvePoint.GCommon.Contract.SharePointBrowser.Object
{
    #region
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckUsersOperation
    {
        [DataMember]
        public string TreeNodeId { get; set; }

        [DataMember]
        public string TreeNodeUrl { get; set; }

        [DataMember]
        public NodeLevel TreeNodeLevel { get; set; }

        /// <summary>
        ///  this is used to identity the checkuser components, only used by manager, the agent should not change the value.
        /// </summary>
        [DataMember]
        public string Tag { get; set; }

        /// <summary>
        ///     the usernames that need to be checked
        /// </summary>
        [DataMember]
        public List<string> Contents { get; set; }

        [DataMember]
        public AccountSearchFlag Flag { get; set; }


        [DataMember]
        public AccountZone Zone { get; set; }

        /// <summary>
        ///     if WebApplications == null ||  WebApplications.Count == 0, the webapplication will be the TreeNodeId's parant webapplication
        /// </summary>
        [DataMember]
        public List<string> WebApplications { get; set; }

        /// <summary>
        ///     userd in LDAPLookup, if search the trust domain 
        ///     default value should be true;
        /// </summary>
        [DataMember]
        public bool IsSearchTrust { get; set; }

        /// <summary>
        ///     used in LDAPLookup
        ///  
        ///     if DomainFilter is empty : search all the domains;
        ///     else : search the domain in the DomainFilter list.
        ///     
        ///     default value : empty (null or DomainFilter.Count = 0)
        /// </summary>
        [DataMember]
        public List<string> DomainFilter { get; set; }

        /// <summary>
        ///     because of many type of user added in sharepoint, sometimes, we do not need to check out all types of the user, so use this parameters to filter the users.
        ///     SPMemberFlag is only used in SPMemberLookup, and it should not be  AccountSearchFlag.Node,
        ///     if SPMemberFlag = AccountSearchFlag.Node,  the value return will be:  AccountSearchFlag.IncludeADGroup | AccountSearchFlag.IncludeADUser | AccountSearchFlag.IncludeAllUsers 
        ///              | AccountSearchFlag.IncludeFormRole | AccountSearchFlag.IncludeFormUser | AccountSearchFlag.IncludeLocalGroup 
        ///             | AccountSearchFlag.IncludeLocalUser | AccountSearchFlag.IncludeSharePointSpecialUsers;
        ///    this is also the default value.
        /// </summary>
        [DataMember]
        public AccountSearchFlag SPMemberFlag { get; set; }

        [DataMember]
        public List<CheckUsersResult> Results { get; set; }

        [DataMember]
        public ReturnResult ReturnValue { get; set; }

        public CheckUsersOperation()
        {
            IsSearchTrust = true;
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckUsersResult
    {
        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public List<UserDetail> UserDetails { get; set; }

        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public bool IsRealRead { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserDetail
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string DomainId { get; set; }

        [DataMember]
        public string Id { get; set; }

        private string loginName;
        [DataMember]
        public string LoginName
        {
            get
            {
                return loginName == null ? null : loginName.Trim();
            }
            set
            {
                loginName = value;
            }
        }

        /// <summary>
        /// CBA认证中区分user的auth provider
        /// </summary>
        [DataMember]
        public string ProviderName { get; set; }

        /// <summary>
        /// CBA中SharePoint区分不同provider，不同user/group的type，不同认证的rule的标示
        /// </summary>
        [DataMember]
        public string Prefix { get; set; }

        private string spLoginName;
        [DataMember]
        public string SPLoginName
        {
            get { return string.IsNullOrEmpty(spLoginName) ? LoginName : spLoginName; }
            set { spLoginName = value; }
        }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string WorkPhone { get; set; }

        [DataMember]
        public AccountType AccountType { get; set; }

        [DataMember]
        public List<GroupDto> Groups { get; set; }

        [DataMember]
        public AccountStatus AccountState { get; set; }

        /// <summary>
        /// AccountManager用于区分User的属性; 用来区分user的source，AD User， Form User，Custome Provider[Rule]
        /// </summary>
        [DataMember]
        public string UserType { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is UserDetail)) return false;
            UserDetail detail = obj as UserDetail;
            return this.DisplayName == detail.DisplayName &&
                this.DomainId == detail.DomainId &&
                this.Id == detail.Id &&
                this.LoginName == detail.LoginName &&
                this.ProviderName == detail.ProviderName &&
                this.Prefix == detail.Prefix &&
                this.SPLoginName == detail.SPLoginName &&
                this.Email == detail.Email &&
                this.Title == detail.Title &&
                this.WorkPhone == detail.WorkPhone &&
                this.AccountType == detail.AccountType &&
                this.Groups == detail.Groups &&
                this.AccountState == detail.AccountState;
        }
        public override int GetHashCode()
        {
            if (!string.IsNullOrEmpty(LoginName))
            {
                return LoginName.ToLower().GetHashCode();
            }
            else
            {
                return 0;
            }
        }
        #region Added for GA+
        //Do not delete Department and Manager when merging met conflicting, just appent them at the last
        //CA also use this property in user check
        /// <summary>
        /// Department property in ActiveDirectory
        /// </summary>
        [DataMember]
        public string Department { get; set; }
        /// <summary>
        /// Manager property in ActiveDirectory
        /// </summary>
        [DataMember]
        public string ManagerSource { get; set; }
        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ADUser = 1,
        [EnumMember]
        ADGroup = 2,
        [EnumMember]
        SharePointUser = 4,
        [EnumMember]
        SharePointGroup = 8,
        [EnumMember]
        FormUser = 16,
        [EnumMember]
        FormRole = 32,
        [EnumMember]
        LocalUser = 64,
        [EnumMember]
        LocalGroup = 128,
        [EnumMember]
        AllUsers = 256,
        [EnumMember]
        ADFSUser = 512,
        [EnumMember]
        ADFSRole = 1024,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountStatus
    {
        // is not verify, the user may be any of the status
        [EnumMember]
        NoVerify = 0,

        [EnumMember]
        Actived = 1,

        // the user is disable
        [EnumMember]
        Deactived = 2,

        // the user is not exit or is  deleted.
        [EnumMember]
        Deleted = 4
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountSearchFlag
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        IncludeADUser = 1,
        [EnumMember]
        IncludeADGroup = 2,

        [EnumMember]
        IncludeSharePointUser = 4,
        [EnumMember]
        IncludeSharePointGroup = 8,

        [EnumMember]
        IncludeFormUser = 16,
        [EnumMember]
        IncludeFormRole = 32,

        [EnumMember]
        IncludeLocalUser = 64,
        [EnumMember]
        IncludeLocalGroup = 128,

        [EnumMember]
        IncludeADFSUser = 256,

        [EnumMember]
        IncludeAllUsers = 341,
        [EnumMember]
        IncludeAllGroups = 8362,

        // get the parent of the user or group
        [EnumMember]
        IncludeParentADGroup = 512,

        // include ad disabled users
        [EnumMember]
        IncludeADDisabledUsers = 1024,

        // include form disabled users
        [EnumMember]
        IncludeFormDisabledUsers = 2048,

        // include the special users in the sharepoint: nt authority\\local service;
        //nt authority\\authenticated users
        //sharepoint\\system
        //nt authority\\system
        [EnumMember]
        IncludeSharePointSpecialUsers = 4096,

        [EnumMember]
        IncludeADFSRole = 8192
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountZone
    {
        // Summary:
        //     Specifies the default zone used for requests unless another zone is specified.
        [EnumMember]
        Default = 0,
        //
        // Summary:
        //     Specifies an intranet zone.
        [EnumMember]
        Intranet = 1,
        //
        // Summary:
        //     Specifies an Internet zone.
        [EnumMember]
        Internet = 2,
        //
        // Summary:
        //     Specifies a custom zone.
        [EnumMember]
        Custom = 3,
        //
        // Summary:
        //     Specifies an extranet zone.
        [EnumMember]
        Extranet = 4,

        //
        // Summary:
        //  contains all the zones
        [EnumMember]
        All = 5
    }
}
