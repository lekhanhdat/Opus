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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.PersonalSetting
{
    [DataContract]
    public class RMPersonalSettingBaseDto
    {
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Id { set; get; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Owner { get; set; }  // user id of account
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PersonalSettingType Type { get; set; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// If is defaut setting
        /// </summary>
        public bool IsDefault { get; set; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsBuiltIn { get; set; }
    }
    [DataContract]
    public class RMPersonalSettingDto : RMPersonalSettingBaseDto
    {
        /// <summary>
        /// json serialized string
        /// </summary>
        [DataMember]
        public string ContentStr { get; set; }
    }
    [DataContract]
    public class OfflineJobInfo
    {
        [DataMember]
        public string JobId { set; get; }
        [DataMember]
        public string StartTime { set; get; }
    }
    [DataContract]
    public class RMExplorerSearchCriteriaDto : RMPersonalSettingBaseDto
    {
        [DataMember]
        public bool IsOffline { set; get; }
        /// <summary>
        /// indicate if it is shared by others.
        /// </summary>
        [DataMember]
        public bool IsSharedBy { get; set; }
        [DataMember]
        public RMExplorerSearchCriteriaSetting Setting { get; set; }
        [DataMember]
        public List<OfflineJobInfo> OfflineJobs { set; get; }
        [DataMember]
        public bool HasRunningJob { set; get; }
        #region static method
        public static RMExplorerSearchCriteriaDto GetBuiltInSetting(PersonalSettingType type)
        {
            return new RMExplorerSearchCriteriaDto
            {
                Owner = TenantLocalValue.LogonUserId,
                Type = type,
                //Name = "Built-In-View",
                Name = RMPersonalSettingConst.Builtin_View_Name, //save I18n key instead of value for built-in view
                IsBuiltIn = true,
                IsDefault = true,
                Setting = new RMExplorerSearchCriteriaSetting
                {
                    //ContentStr = JsonConvert.SerializeObject(new ExplorerQueryOptionV2())
                },
            };
        }
        public static RMExplorerSearchCriteriaDto GetActiveOrArchivedCriteria(DSBInfo info)
        {
            if (info.Id == RMGlobalSearchDefautSettingId.Archived && info.ShowAll)
            {
                var dto = new RMExplorerSearchCriteriaDto
                {
                    Owner = TenantLocalValue.LogonUserId,
                    Id = RMGlobalSearchDefautSettingId.Archived,
                    Type = PersonalSettingType.GlobalSearchCriteria,
                    Setting = new RMExplorerSearchCriteriaSetting
                    {
                        IsAdvancedSearch = true,
                        AdvancedSearchs = new List<AdvancedSearchCriteria> {
                        new AdvancedSearchCriteria{
                            ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(true),
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.ContentArchived},
                                            //SavedColumnName = ""
                                        })
                        }
                        }
                    }
                };
                return dto;
            }
            else if(info.Id == RMGlobalSearchDefautSettingId.Active && info.ShowAll)
            {
                var dto = new RMExplorerSearchCriteriaDto
                {
                    Owner = TenantLocalValue.LogonUserId,
                    Id = RMGlobalSearchDefautSettingId.Active,
                    Type = PersonalSettingType.GlobalSearchCriteria,
                    Setting = new RMExplorerSearchCriteriaSetting
                    {
                        IsAdvancedSearch = true,
                        AdvancedSearchs = new List<AdvancedSearchCriteria>
                        {
                            new AdvancedSearchCriteria
                            {
                                ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(info.Id == RMGlobalSearchDefautSettingId.Active ? 
                                            new List<SourceFlag> { 
                                                SourceFlag.SharePoint,
                                                SourceFlag.Exchange, 
                                                SourceFlag.FileSystem, 
                                                SourceFlag.OneDrive, 
                                                SourceFlag.Physical, 
                                                SourceFlag.SharePointOnPrem,
                                                SourceFlag.AzureFileShare,
                                                SourceFlag.Box,
                                                SourceFlag.Google,
                                                SourceFlag.Teams,
                                            }:new List<SourceFlag> {(SourceFlag)info.Id}),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                            //SavedColumnName = ""
                                        })
                            },
                        }
                    },

                };
                return dto;
            }
            else
            {
                bool isPhysical = info.Id == (int)SourceFlag.Physical;
                bool isFileSystem = info.Id == (int)SourceFlag.FileSystem;
                var dto = new RMExplorerSearchCriteriaDto();
                if (isPhysical)
                {
                    dto = new RMExplorerSearchCriteriaDto
                    {
                        Owner = TenantLocalValue.LogonUserId,
                        Id = info.Id,
                        Type = PersonalSettingType.GlobalSearchCriteria,
                        Setting = new RMExplorerSearchCriteriaSetting
                        {
                            IsAdvancedSearch = true,
                            AdvancedSearchs = new List<AdvancedSearchCriteria>
                        {
                            new AdvancedSearchCriteria
                            {
                                ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(new List<SourceFlag> {(SourceFlag)info.Id}),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                        })
                            },
                            new AdvancedSearchCriteria
                            {
                                ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(new List<StatusValue>{ new StatusValue { Value = "1" },new StatusValue { Value = "6"},new StatusValue { Value = "7"} }),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn
                                            {
                                                Id = "eb4e9ab7-c939-425b-9e29-235236c9ce5b",
                                                IdsWithDuplicateName =new List<Guid>(){Guid.Parse("eb4e9ab7-c939-425b-9e29-235236c9ce5b") },
                                                Type = AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice,
                                            },
                                        }),
                            },
                        }
                        },

                    };
                }
                else if (isFileSystem)
                {
                    dto = new RMExplorerSearchCriteriaDto
                    {
                        Owner = TenantLocalValue.LogonUserId,
                        Id = info.Id,
                        Type = PersonalSettingType.GlobalSearchCriteria,
                        Setting = new RMExplorerSearchCriteriaSetting
                        {
                            IsAdvancedSearch = true,
                            AdvancedSearchs = new List<AdvancedSearchCriteria>
                        {
                            new AdvancedSearchCriteria
                            {
                                ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(new List<SourceFlag> {(SourceFlag)info.Id}),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                        })
                            },
                        }
                        },

                    };
                }
                else
                {
                    dto = new RMExplorerSearchCriteriaDto
                    {
                        Owner = TenantLocalValue.LogonUserId,
                        Id = info.Id,
                        Type = PersonalSettingType.GlobalSearchCriteria,
                        Setting = new RMExplorerSearchCriteriaSetting
                        {
                            IsAdvancedSearch = true,
                            AdvancedSearchs = new List<AdvancedSearchCriteria>
                        {
                            new AdvancedSearchCriteria
                            {
                                ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(new List<SourceFlag> {(SourceFlag)info.Id }),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                        })
                            },
                        }
                        },

                    };
                }
                return dto;
            }
        }
        /// <summary>
        /// get the default search setting for those physical object which return date is expired.
        /// note that, this setting will not be saved to database.
        /// </summary>
        /// <returns></returns>
        public static RMExplorerSearchCriteriaDto GetDefaultDelayedLoanSetting(AOSUserDto loanedBy = null)
        {
            var dto = new RMExplorerSearchCriteriaDto
            {
                Owner = TenantLocalValue.LogonUserId,
                Id = RMGlobalSearchDefautSettingId.DelayedLoan,
                Type = PersonalSettingType.GlobalSearchCriteria,
                Setting = new RMExplorerSearchCriteriaSetting
                {
                    IsAdvancedSearch = true,
                    AdvancedSearchs = new List<AdvancedSearchCriteria> {
                        new AdvancedSearchCriteria {
                            ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3
                                        {
                                            Value = JsonConvert.SerializeObject(new List<SourceFlag>{SourceFlag.Physical }),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag},
                                            //SavedColumnName = ""
                                        })
                        },
                        new AdvancedSearchCriteria {
                            ContentStr = JsonConvert.SerializeObject(
                                        new ExplorerSearchOptionV3 
                                        {
                                            Value = JsonConvert.SerializeObject(new DateInfo { Condition = DateCondition.BeforeNow }),
                                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Loan},
                                            //SavedColumnName = ""
                                        })
                        }
                    }
                },
            };
            if (loanedBy != null)
            {
                dto.Setting.AdvancedSearchs.Add(new AdvancedSearchCriteria
                {
                    ContentStr = JsonConvert.SerializeObject(
                                new ExplorerSearchOptionV3
                                {
                                    Value = JsonConvert.SerializeObject(new List<AOSUserDto> { loanedBy }),
                                    ColumnsLogic = ExplorerSearchKeyOperationLogic.AND,
                                    Column = new ExplorerQueryColumn
                                    {
                                        Id = DefaultColumnIDs.LoanedBy,
                                        Type = Contract.TemplateManagement.ColumnType.PeopleOrGroup
                                    },
                                    //SavedColumnName = ""
                                })
                });
            }
            return dto;
        }
        #endregion
    }

    public class StatusValue
    {
        public string Value { get; set; }
    }
    [DataContract]
    public class RMExplorerSearchCriteriaSetting
    {
        #region basic search
        /// <summary>
        /// term tree json string
        /// </summary>
        [DataMember]
        public string TermTreeStr { get; set; }

        /// <summary>
        /// fs treee json string
        /// </summary>
        [DataMember] 
        public string FSTreeStr { get; set; }

        /// <summary>
        /// spo tree json string
        /// </summary>
        [DataMember] 
        public string SPTreeStr { get; set; } 
        
        [DataMember] 
        public string TeamsTreeStr { get; set; }
        [DataMember]
        public string GoogleTreeStr { get; set; }
        /// <summary>
        /// setting json string
        /// </summary>
        [DataMember] 
        public string ContentStr { get; set; }

        #endregion
        /// <summary>
        /// saved columns
        /// </summary>
        [DataMember] 
        public string ColumnsStr { get; set; }
        [DataMember]
        public string ColumnSortSetting { get; set; }
        [DataMember]
        public bool IsAdvancedSearch { get; set; }
        /// <summary>
        /// advanced search
        /// </summary>
        [DataMember] 
        public List<AdvancedSearchCriteria> AdvancedSearchs { get; set; }
    }
    [DataContract]
    public class AdvancedSearchCriteria
    {
        [DataMember]
        public string TermTreeStr { get; set; }

        /// <summary>
        /// fs treee json string
        /// </summary>
        [DataMember] 
        public string FSTreeStr { get; set; }

        /// <summary>
        /// spo treee json string
        /// </summary>
        [DataMember] 
        public string SPTreeStr { get; set; }

        /// <summary>
        /// Teams tree json string
        /// </summary>
        [DataMember]
        public string TeamsTreeStr { get; set; }
        [DataMember]
        public string GoogleTreeStr { get; set; }

        /// <summary>
        /// setting json string
        /// </summary>
        [DataMember] 
        public string ContentStr { get; set; }
    }

    public class RMPersonalSettingSaveResult
    {
        /// <summary>
        /// id in database, if is equals 0, means failed to save
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// only valid if id equals 0.  
        /// </summary>
        public RMPersonalSettingSaveResultErrorCode ErrorCode { get; set; }
    }

    public enum RMPersonalSettingSaveResultErrorCode
    { 
        None = 0,
        SameName = 1,
        Other = 2,
        NoPermission = 3
    }

    [Serializable]
    public class SameNameException : Exception
    {
    }

    [Serializable]
    public class NoPermissionException : Exception
    {
    }

    public class RMPersonalSettingConst
    {
        public const string Builtin_View_Name = "RM_BCM_Search_Criteria_Builtin_View_Name";

        public const string TargetSetting_Name = "RM_HS_Criteria_View_Dialog_ViewNameTitle";
        public const string TargetSetting_IsDefault = "RM_BCM_Audit_Action_Search_Criteria_Is_Default";
        public const string TargetSetting_IsDefaultYes = "RM_JS_Common_Yes";
        public const string TargetSetting_IsDefaultNo = "RM_JS_Common_No";
        //public const string TargetSetting_Status = "RM_BCM_Audit_Action_Search_Criteria_Status";
        public const string TargetSetting_Content = "RM_BCM_Audit_Action_Search_Criteria_Content";
        public const string ShareToGroups = "RM_HS_SelectedGroupsTitle";
        public const string Audit_None = "RM_RC_Audit_None";
        
    }
    [DataContract]
    public class RMGlobalSearchSharedSettingDto : RMPersonalSettingSecurityGroupMappingBaseDto
    {
    }

    public class RMGlobalSearchDefautSettingId
    {
        public const int DelayedLoan = -1;
        public const int Active = 7;
        public const int Archived = 8;
    }

    public class RMPersonalSettingSecurityGroupMappingDto : RMPersonalSettingSecurityGroupMappingBaseDto
    {
        public string Owner { get; set; }
    }
    [DataContract]
    public class RMPersonalSettingSecurityGroupMappingBaseDto
    {
        /// <summary>
        /// personal setting id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public int Id { set; get; }

        /// <summary>
        /// a list of security group id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public List<int> SecurityGroups { get; set; }
    }

    public class RMPersonalSettingShareResult
    {
        public bool HasError { get; set; }
        public RMPersonalShareResultErrorCode? ErrorCode { get; set; }
    }

    public enum RMPersonalShareResultErrorCode
    {
        None = 0,
        InvalidParameter = 1,
        NoPermission = 2,
        Others = 3
    }

    public class TermInfo 
    {
        public Guid[] TermIds { get; set; }

        public string WithOutTerms { get; set; }
    }

    public class SiteInfo
    {
        public Guid Id { get; set; }

        public int Level { get; set; }
    }
    [DataContract]
    public class DSBInfo
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public bool ShowAll { get; set; }
    }

}
