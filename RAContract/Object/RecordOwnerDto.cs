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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class RecordOwnerDto
    {
        public int LnkId { get; set; }
        public string ObjectId { set; get; }

        public string UserPrincipalName { get; set; }

        public string DisplayName { get; set; }
        public string TenantId { get; set; }
        public AccountType Type { get; set; }
    }
    [DataContract]
    public class AOSUserDto
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string DisplayName { get; set; }

        [IgnoreDataMember]
        public string DisplayName_Lower { 
            get 
            {
                return DisplayName?.ToLower();
            } 
        }
        [DataMember]
        public AccountType InviteType { get; set; }
        [DataMember]
        public int RMUserId { get; set; }
        //AADAcount
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string SurName { get; set; }
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public string TenantId { get; set; }
    }


    public class RecordOwnerGroupDto
    {
        public RecordOwnerGroupDto()
        {
            this.Owners = new List<RecordOwnerDto>();
            this.OwnerIds = new SortedSet<string>();
        }

        public int SPSettingId { get; set; }

        public Guid ScopeId { get; set; }

        public Guid SiteGroupId { get; set; }

        public Guid SiteId { get; set; }

        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public Guid FolderId { get; set; }
        public Guid MailBoxId { get; set; }

        public bool MailToOwner { get; set; }

        public List<RecordOwnerDto> Owners { get; set; }

        public SortedSet<string> OwnerIds { get; set; }

        public void AddOwner(RecordOwnerDto owner)
        {
            if (this.OwnerIds.Add(owner.ObjectId))
            {
                this.Owners.Add(owner);
            }
        }

        public void AddOwnerRange(IEnumerable<RecordOwnerDto> owners)
        {
            if (owners == null)
            {
                return;
            }
            foreach (var item in owners)
            {
                AddOwner(item);
            }
        }

        public string GetGroupKey()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var id in OwnerIds)
            {
                sb.Append(id.Replace('-', ' '));
            }
            return sb.ToString();
        }
    }

    public class ManualRuleInfo
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Criteria { get; set; }
        public string EXOCriteria { get; set; }
        public string PhysicalCriteria { get; set; }
        public string FSCriteria { get; set; }
        public string SPLocalCriteria { get; set; }
        public string OneDriveCriteria { get; set; }
        public string AzureFileCriteria { get; set; }
        public bool IsSendEmailToOwner { get; set; }
        public bool EXOIsSendEmailToOwner { get; set; }
        public bool PhysicalIsSendEmailToOwner { get; set; }
        public bool FSIsSendEmailToOwner { get; set; }
        public bool SPLocalIsSendEmailToOwner { get; set; }
        public bool OneDriveIsSendEmailToOwner { get; set; }
        public bool AzureFileIsSendEmailToOwner { get; set; }
        public List<UserInfo> Users { get; set; }
        public List<UserInfo> EXOUsers { get; set; }
        public List<UserInfo> PhysicalUsers { get; set; }
        public List<UserInfo> FSUsers { get; set; }
        public List<UserInfo> SPLocalUsers { get; set; }
        public List<UserInfo> OneDriveUsers { get; set; }
        public List<UserInfo> AzureFileUsers { get; set; }
        public string WorkflowId { get; set; }
        public string PhyWorkflowId { get; set; }
        public string EXOWorkflowId { get; set; }
        public string FSWorkflowId { get; set; }
        public string SPLocalWorkflowId { get; set; }
        public string OneDriveWorkflowId { get; set; }
        public string AzureFileWorkflowId { get; set; }
        public string DisposalClass { get; set; }
    }

    public enum RecordOwnerSettingType
    {
        SharePoint = 0,
        ExchangeOnline = 1,
        PhysicalRecord = 2,
        FileSystem = 3,
        SharePointOnPremise = 4,
        OneDrive = 5,
        AzureFileShare = 6,
        Box = 7,
        GoogleDrive = 8,
        AISharePointOnline = 10,
        AIOneDrive = 11,
        Teams = 12,
        AITeams = 13,
        AIGoogleDrive = 14,
    }
}
