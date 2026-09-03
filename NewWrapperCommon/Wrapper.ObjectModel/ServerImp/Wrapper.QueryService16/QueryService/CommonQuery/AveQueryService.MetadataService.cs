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
namespace AvePoint.Wrapper.QueryService
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon;
    using System.Data.SqlClient;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using static AvePoint.Wrapper.QueryService.SP2016SelectQueryString;
    using AvePoint.Wrapper.Resource.QueryService;

    internal partial class AveQueryService:IAveMetadataServiceQueryService
    {
         #region private methods
 
         #region mms
         private bool HasPermission(AveTaxonomyRights rights, AveTaxonomyRights match, bool exactMatch)
         {
             bool hasPermission = false;
             if (rights == match)
             {
                 hasPermission = true;
             }
             else if (!exactMatch && (rights | match) == match)
             {
                 hasPermission = true;
             }
             return hasPermission;
         }

        private int GetTermStoreLanguage(AveTermStoreInfo termStoreInfo, Guid defaultPartitionId, bool getTermStoreInfo)
        {
            var defaultLanguage = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@PartitionId", SqlDbType.UniqueIdentifier, defaultPartitionId);
                using (var reader = mQueryWorker.ExecuteReader(TVF_ECMLanguage_PartitionId))
                {
                    while (reader.Read())
                    {
                        if (reader.GetBoolean(2))
                        {
                            defaultLanguage = reader.GetInt32(1);
                            break;
                        }
                    }
                }
                if(getTermStoreInfo)
                {
                    using (var reader = mQueryWorker.ExecuteReader(TVF_ECMPermission_PartitionIdGroupId))
                    {
                        while (reader.Read())
                        {
                            string pName = reader.GetString(0);
                            ulong mask = (ulong)reader.GetInt64(1);
                            pName = GetRealPrincipalName(pName);

                            if (HasPermission(AveTaxonomyRights.ManageTermStore | AveTaxonomyRights.TermStoreAdministrator, (AveTaxonomyRights)mask, false))
                            {
                                termStoreInfo.TermStoreAdministrators.Add(new AveAceInfo()
                                {
                                    DisplayName = pName,
                                    GrantRightsMask = mask,
                                    PrincipalName = pName,
                                    DenyRightsMask = (ulong)AveTaxonomyRights.None
                                });
                            }
                        }
                    }
                }
            });
            return defaultLanguage;
        }

        
        /// <summary>
        /// 获取Term Groups信息，有API实现，效率考虑
        /// </summary>
        /// <param name="groupIds"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns>保证返回集合，不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang")]
        private List<AveMetadataGroupInfo> GetTermGroups(List<int> groupIds, Guid defaultPartitionId)
        {
            List<AveMetadataGroupInfo> groupList = new List<AveMetadataGroupInfo>();
            if (groupIds == null || groupIds.Count == 0)
            {
                return groupList;
            }
            mQueryWorker.ClearParameters();
            SqlCommand command = mQueryWorker.Command;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@GroupIdList", SqlDbType.VarChar, 0x7fffffff));
            command.Parameters["@GroupIdList"].Value = GetListString<int>(groupIds, '\\');
            command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
            command.Parameters["@PartitionId"].Value = defaultPartitionId;

            Dictionary<int, AveMetadataGroupInfo> groupDic = new Dictionary<int, AveMetadataGroupInfo>();

            using (SqlDataReader reader = mQueryWorker.ExecuteReader(GetGlobalGroups_Select_proc_ECM_GetGroups))
            {
                #region Stored Procedure Fields
                //Id
                //,PartitionId
                //,UniqueId
                //,Name
                //,Description
                //,LastModifiedTime
                //,CreatedTime
                //,Type
                #endregion
                while (reader.Read())
                {
                    AveMetadataGroupInfo group = new AveMetadataGroupInfo();
                    group.Id = reader.GetGuid(2);
                    group.Name = reader.GetString(3);
                    group.Description = reader.GetString(4);
                    group.IsSystemGroup = reader.GetInt32(7) == 1;
                    group.IsSiteCollectionGroup = reader.GetInt32(7) == 2;
                    group.PartitionId = defaultPartitionId;
                    groupList.Add(group);
                    groupDic[int.Parse(reader.GetString(0))] = group;

                }

                #region Stored Procedure Fields
                //GroupId
                //,PrincipalName
                //,Rights
                #endregion
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        AveMetadataGroupInfo group = groupDic[id];
                        ulong mask = (ulong)reader.GetInt64(2);
                        string pName = reader.GetString(1);
                        pName = GetRealPrincipalName(pName);
                        if (pName.StartsWith("SiteCollectionId:", StringComparison.OrdinalIgnoreCase))
                        {
                            //可查看Microsoft.SharePoint.Taxonomy.Internal.Security.DeserializePermission(IDataReader dataReader, SPAcl<TaxonomyRights> acl, List<Guid> siteCollectionIds)源代码
                            Guid item = new Guid(pName.Substring("SiteCollectionId:".Length));
                            group.Sites.Add(item);
                            continue;
                        }
                        if (pName.StartsWith("SiteCollectionUrl:", StringComparison.OrdinalIgnoreCase))
                        {
                            string siteUrl = pName.Substring("SiteCollectionUrl:".Length);
                            group.SiteCollectionReadOnlyAccessUrls.Add(siteUrl);
                            continue;
                        }

                        var groupManagersRight = AveTaxonomyRights.GroupManager | AveTaxonomyRights.EditTerm | AveTaxonomyRights.AddTermSetEditPermissions | AveTaxonomyRights.EditGroup | AveTaxonomyRights.EditTermSet;
                        var groupManagerInfo = GetAceInfoByPermission(groupManagersRight, mask, pName, false);
                        if (groupManagerInfo != null)
                        {
                            if (group.GroupManagers == null)
                            {
                                group.GroupManagers = new List<AveAceInfo>();
                            }
                            group.GroupManagers.Add(groupManagerInfo);
                        }
                        var groupContributorRight= AveTaxonomyRights.Contributor | AveTaxonomyRights.EditTerm | AveTaxonomyRights.EditTermSet;
                        var contributorInfo = GetAceInfoByPermission(groupContributorRight, mask, pName, false);
                        if (contributorInfo != null)
                        {
                            if (group.Contributors == null)
                            {
                                group.Contributors = new List<AveAceInfo>();
                            }
                            group.Contributors.Add(contributorInfo);
                        }
                    }
                }
            }
            return groupList;
        }

        private AveAceInfo GetAceInfoByPermission(AveTaxonomyRights permissions, ulong mask,string pName, bool exactMatch)
        {
            if (HasPermission(permissions, (AveTaxonomyRights)mask, exactMatch))
            {
                return new AveAceInfo()
                {
                    DisplayName = pName,
                    GrantRightsMask = mask,
                    PrincipalName = pName,
                    DenyRightsMask = (ulong) AveTaxonomyRights.None
                };
            }
            return null;
        }

        /// <summary>
        /// 根据Group类型和PartitionId获取GroupId集合
        /// </summary>
        /// <param name="isGlobal"></param>
        /// <param name="partitionId">为null就是不用PartitionId查询</param>
        /// <returns></returns>
        private Dictionary<Guid, string> GetGroupIds(bool isGlobal, Guid? partitionId)
        {
            var ids = new Dictionary<Guid, string>();
            ExceptionHandlingScope(() =>
            {
               
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                 string command;
                if (partitionId.HasValue)
                {
                    command = isGlobal
                   ? GetGlobalTermGroupIdsWithPartitionId_Select_ECMGroup
                   : GetLocalTermGroupIdsWithPartitionId_Select_ECMGroup;
                    mQueryWorker.AddParameter("@PartitionId", partitionId);
                }
                else
                {
                    command = isGlobal
                    ? GetGlobalTermGroupIds_Select_ECMGroup
                    : GetLocalTermGroupIds_Select_ECMGroup;

                }
               
                using (var reader = mQueryWorker.ExecuteReader(command))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        ids[id] = reader.GetString(1);
                    }
                }
            });
            return ids;
        }

        #region term相关


        /// <summary>
        /// 根据term的UniqueId获取int的TermId，效率考虑，有API实现.
        /// </summary>
        /// <param name="termGuid"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " re-order index")]
        private int GetTermIdByGuid(Guid termGuid, Guid defaultPartitionId)
        {
            int termId = -1;
            string cmdText = GetTermIdByGuid_Select_ECMTerm;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermId", termGuid);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                if (reader.Read())
                {
                    termId = reader.GetInt32(0);
                }
            }
            return termId;
        }

        /// <summary>
        /// 根据Id集合获取指定的Term的详细信息,效率考虑，有API实现
        /// </summary>
        /// <param name="termIds"></param>
        /// <param name="termSetId"></param>
        /// <param name="isAllTersInSet">从逻辑看没什么实际含义</param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集合，不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang")]
        private List<AveTermInfo> GetTermsByIds(ICollection<int> termIds, int termSetId, bool isAllTersInSet, Guid defaultPartitionId, int defaultLanguage)
        {
            var termList = new List<AveTermInfo>();
            //todo:wbhu,isAllTersInSet这个参数有什么用？没看出来有实际意义,后续确认下
            if (!isAllTersInSet && (termIds == null || termIds.Count == 0))
            {
                return termList;
            }

            if (termIds == null)
            {
                return termList;
            }

            termList.AddRange(termIds.Select(id => GetTermById(id, termSetId, defaultPartitionId, defaultLanguage)));
            return termList;
        }

        /// <summary>
        /// get sub term ids under the specific term
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        private List<Guid> GetTermIdsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {
            List<Guid> ids = null;
            var termIdInt = GetTermIdByGuid(termId, defaultPartitionId);
            var termSetIdInt = GetTermSetIdByGuid(termSetId, defaultPartitionId);

            //todo:wbhu,这个query与GetTermsUniqueIdInTermSet_Select_ECMTermSetMembership_ECMTerm类似，考虑能不能用一个
            string cmdText = GetTermsUniqueIdInTerm_Select_ECMTermSetMembership_ECMTerm;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermId", termIdInt);
            mQueryWorker.AddParameter("@TermSetId", termSetIdInt);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                ids = GetGuidValues(reader, 0);
            }
            return ids;
        }

        /// <summary>
        /// get all properties of a term with specific term id
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermInfo GetTermById(int termId, int termSetId, Guid defaultPartitionId, int defaultLanguage)
        {
             string cmdText = GetTermById_Select_ECMTerm_ECMTermSetMembership;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermSetId", termSetId); 
            mQueryWorker.AddParameter("@Id", termId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            var termInfo = new AveTermInfo();
            int pinSourceTermSetId = 0;
            int parentTermId = 0;
            //1 Term Basic information
            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {

                    termInfo.Id = reader.GetGuid(3);
                    termInfo.Owner = reader.GetString(4);
                    termInfo.IsDeprecated = reader.GetBoolean(5);
                    termInfo.IsAvailableForTagging = reader.GetBoolean(10);
                    termInfo.IsSourceTerm = reader.GetBoolean(12);
                    termInfo.IsRoot = reader.GetInt32(9) == 0;
                    parentTermId = reader.GetInt32(9);
                    termInfo.IsPinned = (reader.IsDBNull(13) ? 0 : reader.GetInt32(13)) != 0;
                    pinSourceTermSetId = reader.IsDBNull(13) ? 0 : reader.GetInt32(13);
                    termInfo.PartitionId = defaultPartitionId;
                    termInfo.ParentTermSetId = reader.GetGuid(14);
                    if (!reader.IsDBNull(11))
                    {
                        termInfo.CustomSortOrder = reader.GetString(11);
                    }
                    if (!reader.IsDBNull(7))
                    {
                        termInfo.MergedTermIds = reader.GetString(7)
                            .Split('\\')
                            .Where(AveTypeHelper.IsGuid)
                            .Select(s => new Guid(s))
                            .ToList();
                    }
                }
            }
            termInfo.IsReused = GetTermIsReusedProperty(termId, defaultPartitionId);

            if (pinSourceTermSetId != 0)
            {
                Guid pinSourceTermSetUniqueId = GetTermPinSourceTermSetUniqueId(pinSourceTermSetId, defaultPartitionId);
                if (pinSourceTermSetUniqueId != Guid.Empty)
                {
                    termInfo.PinSourceTermSetId = pinSourceTermSetUniqueId;
                }
            }

            if (!termInfo.IsRoot)
            {
                termInfo.ParentTermId = GetParentTermUniqueId(parentTermId, defaultPartitionId, defaultLanguage);
            }

            //2：Term Label
            GetTermLabels(termInfo, termId, defaultPartitionId, defaultLanguage);

            //3：Term Description
            GetTermDescription(termInfo, termId, defaultPartitionId);

            //4: Get Property
            GetTermProperty(termInfo, termId, termSetId);

            return termInfo;
        }

        private Guid GetParentTermUniqueId(int parentTermId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetParentTermUniqueId_Select_ECMTermSetMembership;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", parentTermId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);

            Guid parentTermUniqueId = Guid.Empty;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    parentTermUniqueId = reader.GetGuid(0);
                }
            }
            return parentTermUniqueId;
        }

        private Guid GetTermPinSourceTermSetUniqueId(int pinSourceTermSetId, Guid defaultPartitionId)
        {
            Guid pinSourceTermSetUniqueId = Guid.Empty;
            string cmdText = GetTermPinSourceTermSetUniqueId_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermSetId", pinSourceTermSetId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    pinSourceTermSetUniqueId = reader.GetGuid(0);
                }
            }
            return pinSourceTermSetUniqueId;
        }

        private bool GetTermIsReusedProperty(int termId, Guid defaultPartitionId)
        {
            string cmdText = GetTermIsReusedProperty_Select_ECMTerm;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", termId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            int count = 0;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }
            if (count > 1)
            {
                return true;
            }
            return false;
        }

        #region << 获取TermLabel >>

        /// <summary>
        /// 获取TermLabel信息，效率考虑，有API实现
        /// 获取的信息存储在termInfo中
        /// </summary>
        /// <param name="termInfo"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        [QueryReview("2012/05/17", "Long Liang")]
        private void GetTermLabels(AveTermInfo termInfo, int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            //Sql
             string cmdText = GetTermLabels_Select_ECMTermLabel;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", termId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            //Exec
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    //Label
                    if (termInfo.Labels == null)
                    {
                        termInfo.Labels = new List<AveLableInfo>();
                    }
                    var lInfo = new AveLableInfo
                    {
                        Language = reader.GetInt32(1),
                        Value = reader.GetString(2),
                        IsDefaultForLanguage = reader.GetBoolean(3)
                    };
                    if (lInfo.IsDefaultForLanguage && lInfo.Language == defaultLanguage)
                    {
                        termInfo.Name = lInfo.Value;
                        termInfo.TermName = lInfo.Value;
                    }
                    termInfo.Labels.Add(lInfo);
                }
            }
        }
        #endregion << 获取TermLabel >>

        #region << 获取TermDescription >>
        /// <summary>
        /// 获取TermDescription,效率考虑，有API实现
        /// 获取的Description存储在termInfo中
        /// </summary>
        /// <param name="termInfo"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        [QueryReview("2012/05/17", "Long Liang")]
        private void GetTermDescription(AveTermInfo termInfo, int termId, Guid defaultPartitionId)
        {
            //Sql
             string cmdText = GetTermDescription_Select_ECMTermDescription;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", termId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            //Exec
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    //description lcid                   
                    var lcid = reader.GetInt32(1);
                    //description 
                    termInfo.Description[lcid] = reader.GetString(2);
                    
                    foreach (AveLableInfo l in termInfo.Labels)
                    {
                        if (l.Language == lcid)
                        {
                            l.Description = reader.GetString(2);
                            break;
                        }
                    }
                }
            }
        }
        #endregion << 获取TermDescription >>

        #region << 获取TermProperty >>

        /// <summary>
        /// get term properties ,include custom property and local property
        /// store information in termInfo
        /// </summary>
        /// <param name="termInfo"></param>
        /// <param name="id"></param>
        /// <param name="termSetId"></param>
        private void GetTermProperty(AveTermInfo termInfo, int id, int termSetId)
        {

            GetTermCustomProperty(termInfo, id);

            GetTermLocalProperty(termInfo, id, termSetId);

        }

        /// <summary>
        /// get term custom property
        /// store information in termInfo
        /// </summary>
        /// <param name="termInfo"></param>
        /// <param name="id"></param>
        private void GetTermCustomProperty(AveTermInfo termInfo, int id)
        {
             string cmdText = GetTermProperty_Select_ECMTerm_ECMTermProperty;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@TermSetId", 0);

            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    //Label
                    if (termInfo.CustomProperties == null)
                    {
                        termInfo.CustomProperties = new Dictionary<string, string>();
                    }
                    termInfo.CustomProperties.Add(reader.GetString(0), reader.GetString(1));
                }
            }
        }

        /// <summary>
        /// get term local property
        /// store information in termInfo
        /// </summary>
        /// <param name="termInfo"></param>
        /// <param name="id"></param>
        /// <param name="termSetId"></param>
        private void GetTermLocalProperty(AveTermInfo termInfo, int id, int termSetId)
        {
             string cmdText = GetTermProperty_Select_ECMTerm_ECMTermProperty;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@TermSetId", termSetId);

            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    //Label
                    if (termInfo.LocalCustomProperties == null)
                    {
                        termInfo.LocalCustomProperties = new Dictionary<string, string>();
                    }
                    termInfo.LocalCustomProperties.Add(reader.GetString(0), reader.GetString(1));
                }
            }
        }




        #endregion

        #endregion term相关

        #region termSet

        #region << 获取TermSet详细信息 >>


        /// <summary>
        /// 获取TermSet详细信息,效率考虑，有API实现
        /// </summary>
        /// <param name="termSetIds"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集合(可能为空)，但是不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang", Changed = true, Comment = "Add batch query")]
        private List<AveTermSetInfo> GetTermSetsByIds(List<int> termSetIds, Guid defaultPartitionId, int defaultLanguage)
        {
            var termSetList = new List<AveTermSetInfo>();
            Dictionary<int, AveTermSetInfo> termSetDic = new Dictionary<int, AveTermSetInfo>();
            if (termSetIds == null || termSetIds.Count == 0)
            {
                return termSetList;
            }
            var count = termSetIds.Count;
            for (var index = 0; index < count;)
            {
                #region Get batch query comamnd
                var remain = count - index;
                if (remain > 100)
                {
                    remain = 100;
                }
                var cmdText = GetTermSetsByIdCollection_Select_ECMTermSet(termSetIds, index, remain);
                index += remain;
                #endregion

                mQueryWorker.Command.CommandType = CommandType.Text;

                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        int termSetId = reader.GetInt32(0);
                        var termSetInfo = new AveTermSetInfo
                        {
                            Id = reader.GetGuid(6),
                            Name = reader.GetString(7),
                            PartitionId = defaultPartitionId
                        };
                        //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                        var tmpName = termSetInfo.Name;
                        var tmpLength = tmpName.IndexOf(defaultLanguage + "|", StringComparison.OrdinalIgnoreCase);
                        if (tmpLength >= 0)
                        {
                            tmpName = tmpName.Substring(tmpLength + defaultLanguage.ToString().Length + 1);
                            tmpLength = tmpName.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                            if (tmpLength >= 0)
                            {
                                tmpName = tmpName.Substring(0, tmpLength);
                            }
                        }
                        termSetInfo.Name = tmpName;
                        termSetInfo.Description = reader.GetString(8);
                        termSetInfo.Type = reader.GetByte(9);
                        termSetInfo.Owner = FindRealUserGroupName(reader.GetString(4));
                        if (!reader.IsDBNull(5))
                        {
                            termSetInfo.CustomSortOrder = reader.GetString(5);
                        }
                        termSetInfo.IsAvailableForTagging = reader.GetBoolean(11);
                        termSetInfo.IsOpenForTermCreation = reader.GetBoolean(10);
                        if (!reader.IsDBNull(13))
                        {
                            termSetInfo.Contact = reader.GetString(13);
                        }
                        if (!reader.IsDBNull(12))
                        {
                            var holders = reader.GetString(12);
                            if (!String.IsNullOrEmpty(holders))
                            {
                                termSetInfo.Stakeholders = new List<string>();
                                foreach (var h in holders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    termSetInfo.Stakeholders.Add(FindRealUserGroupName(h));
                                }
                            }
                        }
                        termSetList.Add(termSetInfo);
                        termSetDic.Add(termSetId, termSetInfo);
                    }
                }
            }
            SetTermSetsCustomProperty(termSetDic);
            return termSetList;
        }

        #region << 替换UserGroup数据库存储的模式到正常模式 >>

        /// <summary>
        /// 替换数据库存储的模式到正常模式
        /// </summary>
        /// <param name="name">数据库模式</param>
        /// <returns>正常模式</returns>
        //todo:wbhu,这个跟metadata service或query service关系都不大，考虑提到Common里作为一个公共方法
        private string FindRealUserGroupName(string name)
        {
            var retName = name;
            try
            {
                if (retName.StartsWith("c:0+.w|s", StringComparison.OrdinalIgnoreCase))
                {
                    retName = name.Substring(7);
                    retName = AveObjectModelFactory.CreateObjectModelFactory("", null).CreatePeopleEditor().GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(retName));
                }
                else if (retName.StartsWith("s-1-5-21", StringComparison.OrdinalIgnoreCase))
                {
                    retName = AveObjectModelFactory.CreateObjectModelFactory("", null).CreatePeopleEditor().GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(retName));
                }
                else if (retName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase) || retName.StartsWith("c:0-.f|", StringComparison.OrdinalIgnoreCase))
                {
                    retName = retName.Substring(7);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.FindRealUserGroupNameError, ex);
                retName = name;
            }
            return retName;
        }

        #endregion << 替换UserGroup数据库存储的模式到正常模式 >>

        #endregion << 获取TermSet详细信息 >>

        /// <summary>
        /// 获取TermSetId，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="partitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        private int GetTermSetIdByGuid(Guid termSetId, Guid partitionId)
        {
            int termsetId = -1;
            string commandText = GetTermSetIdByGuid_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", termSetId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
            {
                if (reader.Read())
                {
                    termsetId = reader.GetInt32(0);
                }
            }
            return termsetId;
        }

        #endregion termSet

        #region related to Incremental

        /// <summary>
        /// 查询特定时间范围内，特定scope上的Metadata change item.
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="toTime"></param>
        /// <param name="scopeId"></param>
        /// <param name="scopeType"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang")]
        private List<AveTermChangeItem> GetMetaDataChanges(Guid defaultPartitionId, DateTime? sinceTime, DateTime? toTime, Guid scopeId, AveTermChangeItem.ChangedItemType scopeType)
        {
            var items = new List<AveTermChangeItem>();
            ExceptionHandlingScope(() =>
            {
                Dictionary<string, object> parameters;
                var commandText = GetChangeMetadataWithCondition_Select_ECMChangeLog(sinceTime, toTime, scopeId, scopeType, defaultPartitionId, out parameters);
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                foreach (var parameter in parameters)
                {
                    mQueryWorker.AddParameter(parameter.Key, parameter.Value);
                }

                using (var reader = mQueryWorker.ExecuteReader(commandText))
                {
                    // N'SELECT '+ N' cl.PartitionId '+ N',cl.GroupUniqueId  ' + N',cl.TermSetUniqueId ' + N',cl.ObjectUniqueId ' 
                    //+ N',cl.ObjectId'+ N',cl.ObjectType'+N',cl.ChangeType'+ N',cl.ChangeTime'+ N',cl.ChangeData'+ N',cl.ModifiedBy' 
                    while (reader.Read())
                    {
                        var item = new AveTermChangeItem();

                        if (!reader.IsDBNull(4))
                        {
                            item.ObjectId = reader.GetInt32(4);
                        }

                        if (!reader.IsDBNull(3))
                        {
                            item.Id = reader.GetGuid(3);
                        }

                        item.ItemType = (AveTermChangeItem.ChangedItemType) reader.GetInt32(5);
                        item.ChangeType = (AveTermChangeItem.ChangedOperationType) reader.GetInt32(6);
                        item.ChangeTime = reader.GetDateTime(7);

                        if (!reader.IsDBNull(2))
                        {
                            item.TermSetId = reader.GetGuid(2);
                        }

                        if (!reader.IsDBNull(1))
                        {
                            item.GroupId = reader.GetGuid(1);
                        }

                        if (!reader.IsDBNull(8))
                        {
                            item.ChangeData = reader.GetString(8);
                        }

                        items.Add(item);
                    }
                }
            });

            return items;
        }

        /// <summary>
        /// 插入合并changeTerm的查询结果
        /// </summary>
        /// <param name="results"></param>
        /// <param name="changeItem"></param>
        private void AddToResults(List<AveTermChangeItem> results, AveTermChangeItem changeItem)
        {
            //if (results.Count == 0)
            //{
            //    results.Add(changeItem);
            //    return;
            //}
            var keyItem = results.FirstOrDefault(item => item.Id == changeItem.Id);
            if (keyItem == null)
            {
                results.Add(changeItem);
                return;
            }
            if (changeItem.SubTerms.Count > 0)
            {
                var tempKeyItem = keyItem.SubTerms.Find(item => changeItem.SubTerms[0].Id == item.Id);
                while (tempKeyItem != null)
                {
                    keyItem = tempKeyItem;
                    changeItem = changeItem.SubTerms[0];
                    if (changeItem.SubTerms.Count > 0)
                    {
                        tempKeyItem = tempKeyItem.SubTerms.Find(item => changeItem.SubTerms[0].Id == item.Id);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (CompareChangedOperationType(keyItem.ChangeType, changeItem.ChangeType))
            {
                keyItem.ChangeType = changeItem.ChangeType;
                if (changeItem.ChangeType == AveTermChangeItem.ChangedOperationType.Delete)
                {
                    keyItem.SubTerms = null;
                    return;
                }
            }
            if (changeItem.SubTerms.Count > 0)
            {
                keyItem.SubTerms.Add(changeItem.SubTerms[0]);
            }
        }

        /// <summary>
        /// 获取对应term的Path，为了得到term到root term的tree结构
        /// </summary>
        /// <param name="partitionId"></param>
        /// <param name="termIntId"></param>
        /// <returns>Path格式 1\2\3</returns>
        private string GetTermPath(Guid partitionId, int termIntId, int isSource = 1, int termSetId = 0)
        {
            var path = string.Empty;
            ExceptionHandlingScope(() =>
            {
                string cmdText = string.Empty;
                if (termSetId != 0)
                {
                    cmdText = @"SELECT TOP 1 Path FROM ECMTermSetMembership WITH(NOLOCK) WHERE PartitionId=@PartitionId And TermSetId=@TermSetId And TermId=@TermIntId And IsSource=@IsSource";
                }
                else
                {
                    cmdText = @"SELECT TOP 1 Path FROM ECMTermSetMembership WITH(NOLOCK) WHERE PartitionId=@PartitionId And TermId=@TermIntId And IsSource=@IsSource";
                }
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@TermIntId", termIntId);
                mQueryWorker.AddParameter("@PartitionId", partitionId);
                mQueryWorker.AddParameter("@IsSource", isSource);
                if (termSetId != 0)
                {
                    mQueryWorker.AddParameter("@TermSetId", termSetId);
                }
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (reader.Read())
                    {
                        path = reader.GetString(0);
                    }
                }
            });
            return path;
        }

        private string GetChangeItemPath(AveTermChangeItem item, AveTermInfo info, Guid defaultPartitionId)
        {
            var path = item.Path;
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (item.ChangeType == AveTermChangeItem.ChangedOperationType.Delete)
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(item.ChangeData);
                    var ptId = (XmlElement)doc.GetElementsByTagName("PTId")[0];
                    var pTermObjectId = Convert.ToInt32(ptId.InnerXml);
                    if (pTermObjectId != 0)
                    {
                        path = GetTermPath(defaultPartitionId, pTermObjectId, info.IsSourceTerm ? 1 : 0, GetTermSetIdByGuid(item.TermSetId.Value, defaultPartitionId)) + "\\" + item.ObjectId;
                    }
                    else
                    {
                        path = item.ObjectId.ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting term parent objectId. Term UniqueId:{0}, Error:{1}", item.Id, ex);
                }
            }
            else
            {
                path = GetTermPath(defaultPartitionId, item.ObjectId, info.IsSourceTerm ? 1 : 0, GetTermSetIdByGuid(item.TermSetId.Value, defaultPartitionId));
            }
            return path;
        }


        /// <summary>
        /// 获取changeItem到Root Term的直线型tree结构
        /// </summary>
        /// <param name="item"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermChangeItem GetChangeItemTree(AveTermChangeItem item, Guid defaultPartitionId, int defaultLanguage)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            string path = string.Empty;
            AveTermInfo termInfo = GetTerm(item.TermSetId.Value, item.ObjectId, defaultPartitionId, defaultLanguage);
            if (item.ChangeType == AveTermChangeItem.ChangedOperationType.Add)
            {
                if (termInfo.IsPinned)
                {
                    path = GetTermPath(defaultPartitionId, item.ObjectId, 0, GetTermSetIdByGuid(item.TermSetId.Value, defaultPartitionId));
                }
                else
                {
                    path = GetChangeItemPath(item, termInfo, defaultPartitionId);
                }
            }
            else
            {
                path = GetChangeItemPath(item, termInfo, defaultPartitionId);
            }
            AveTermChangeItem root = null;
            AveTermChangeItem tempTree = null;
            var isRoot = true;
            //若path为string.Empty的话，说明这个term的parent被删除了
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            try
            {
                var itemObjectIds = path.Split('\\');
                foreach (var id in itemObjectIds)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }
                    AveTermChangeItem changeItem;
                    var termObjectId = Convert.ToInt32(id);
                    if (item.ObjectId != termObjectId)
                    {
                        changeItem = new AveTermChangeItem
                        {
                            ChangeType = AveTermChangeItem.ChangedOperationType.FakeAsParent,
                            ChangeTime = item.ChangeTime,
                            ItemType = AveTermChangeItem.ChangedItemType.Term,
                            ObjectId = termObjectId,
                            TermSetId = item.TermSetId,
                            GroupId = item.GroupId,
                            ChangeData = string.Empty
                        };
                        var info = GetTerm(item.TermSetId.Value, termObjectId, defaultPartitionId, defaultLanguage);
                        changeItem.Name = info.Name;
                        changeItem.Id = info.Id;
                    }
                    else
                    {
                        var info = GetTerm(item.TermSetId.Value, termObjectId, defaultPartitionId, defaultLanguage);
                        changeItem = item;
                        changeItem.Name = info.Name;
                        changeItem.IsPinned = info.IsPinned;
                        changeItem.IsReused = info.IsReused;
                        changeItem.IsRoot = info.IsRoot;
                        changeItem.IsSourceTerm = info.IsSourceTerm;
                        changeItem.PinSourceTermSetId = info.PinSourceTermSetId;
                        changeItem.Path = GetTermPath(defaultPartitionId, GetTermId(item.Id, defaultPartitionId), info.IsSourceTerm ? 1 : 0);
                    }
                    if (isRoot)
                    {
                        tempTree = changeItem;
                        root = tempTree;
                        isRoot = false;
                        continue;
                    }
                    tempTree.SubTerms.Add(changeItem);
                    tempTree = changeItem;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while building the term tree. Term UniqueId:{0}, Error:{1}.", item.Id, ex);
            }
            return root;
        }

        /// <summary>
        /// 获取MMS下store中的Group改变，为Incremental处理，效率考虑，有API实现
        /// 如果sinceTime为空，则查询出所有Group
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="getGroupsByPartitionId">是否根据partition id查询TermGroup</param>
        /// <returns></returns>
        [Logic("wbhu", "方法内部没有实际的Query操作")]
        private List<AveTermChangeItem> GetChangesInStoreInternal(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId, bool getGroupsByPartitionId)
        {
            var groups = GetGroupIds(isGlobal, getGroupsByPartitionId ? defaultPartitionId : (Guid?)null);
            if (sinceTime.HasValue)
            {
                var temp = GetChanges(null, null, sinceTime.Value, null, defaultPartitionId);
                var changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.Group);
                List<AveTermChangeItem> returnChanges = new List<AveTermChangeItem>();
                foreach (var changeItem in changes.Where(changeItem => !changeItem.ChangeType.Equals(AveTermChangeItem.ChangedOperationType.Delete)))
                {
                    changeItem.Name = GetGroup(changeItem.GroupId, defaultPartitionId).Name;
                    var groupId = GetGroupId(changeItem.GroupId, defaultPartitionId);
                    changeItem.ObjectId = groupId.HasValue ? groupId.Value : -1;
                    changeItem.PartitionId = defaultPartitionId;
                    if (groups.ContainsKey(changeItem.GroupId))
                    {
                        returnChanges.Add(changeItem);
                    }
                }
                return returnChanges;
            }            
            return groups.Select(id => new AveTermChangeItem
            {
                Id = id.Key,
                GroupId = id.Key,
                ItemType = (AveTermChangeItem.ChangedItemType)3,
                ChangeType = 0,
                Name = id.Value,
                PartitionId = defaultPartitionId
            }).ToList();
        }

        /// <summary>
        /// ChangedOperationType的优先级为Delete > Add > Edit > FakeAsParent
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="targetType"></param>
        /// <returns>返回结果为true说明targetType的优先级高于baseType</returns>
        private bool CompareChangedOperationType(AveTermChangeItem.ChangedOperationType baseType, AveTermChangeItem.ChangedOperationType targetType)
        {
            return (targetType == AveTermChangeItem.ChangedOperationType.Delete)
                   || (baseType == AveTermChangeItem.ChangedOperationType.FakeAsParent && targetType == AveTermChangeItem.ChangedOperationType.Edit);
        }

        /// <summary>
        /// 合并获得到的所有变化,每个对象只取一个最重要的操作,也会删除一些不需要做的对象操作信息
        /// </summary>
        /// <param name="changes">所有的变化</param>
        /// <param name="itemType">我们想要获取的对象变化的item type</param>
        /// <returns></returns>
        private List<AveTermChangeItem> MergeChanges(IEnumerable<AveTermChangeItem> changes, AveTermChangeItem.ChangedItemType itemType)
        {
            var tempResult = new Dictionary<Guid, AveTermChangeItem>();

            foreach (var changeItem in changes)
            {
                var targetId = GetTargetId(changeItem, itemType);
                if (targetId == Guid.Empty)
                {
                    continue;
                }
                if (targetId == changeItem.Id)
                {
                    if (changeItem.ChangeType == AveTermChangeItem.ChangedOperationType.Add)
                    {
                        changeItem.IsNewAdd = true;
                    }
                }
                else
                {
                    UpdateChangeItemProperty(changeItem, itemType, targetId);
                }

                if (tempResult.ContainsKey(changeItem.Id))
                {
                    if (CompareChangedOperationType(tempResult[changeItem.Id].ChangeType, changeItem.ChangeType))
                    {
                        if (tempResult[changeItem.Id].IsNewAdd && changeItem.ChangeType == AveTermChangeItem.ChangedOperationType.Delete)
                        {
                            tempResult.Remove(changeItem.Id);
                            continue;
                        }
                        tempResult[changeItem.Id] = changeItem;
                    }
                }
                else
                {
                    tempResult.Add(changeItem.Id, changeItem);
                }

            }

            return tempResult.Values.ToList();
        }


        /// <summary>
        /// 获取我们想要目标对象的Guid
        /// </summary>
        /// <param name="item">实际操作的对象信息</param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private Guid GetTargetId(AveTermChangeItem item, AveTermChangeItem.ChangedItemType itemType)
        {
            var targetId = Guid.Empty;
            switch (itemType)
            {
                case AveTermChangeItem.ChangedItemType.Group:
                    targetId = item.GroupId;
                    break;
                case AveTermChangeItem.ChangedItemType.TermSet:
                    if (item.TermSetId.HasValue)
                    {
                        targetId = item.TermSetId.Value;
                    }
                    break;
                case AveTermChangeItem.ChangedItemType.Term:
                    targetId = item.Id;
                    break;
            }
            if (targetId == Guid.Empty)
            {
                logger.Warn("The object Guid is Empty!");
            }
            return targetId;
        }

        /// <summary>
        /// 当找到的对象不是实际修改的对象,需要做Parent的时候,将Parent对象信息更新到ChangeItem中
        /// </summary>
        /// <param name="item">实际修改的对象信息</param>
        /// <param name="itemType">对象的类型</param>
        /// <param name="targetId">我们想要目标对象的Guid</param>
        private void UpdateChangeItemProperty(AveTermChangeItem item, AveTermChangeItem.ChangedItemType itemType, Guid targetId)
        {
            item.Id = targetId;
            item.ItemType = itemType;
            item.ChangeType = AveTermChangeItem.ChangedOperationType.FakeAsParent;
            item.ChangeData = string.Empty;
        }

        #endregion related to Incremental

        #region publishing contentType

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="isPublished">0(Unpublished ContentType) or 1(Published ContentType)</param>
        /// <returns></returns>
        private bool IsPublishingContentTypeExist(string contentTypeId, Guid defaultPartitionId,int isPublished)
        {
            var count = 0;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetPublishingContentTypeCountById_Select_ECMPackage;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                mQueryWorker.AddParameter("@Type", new Guid("B4AD3A44-D934-4C91-8D1F-463ACEADE443"));
                mQueryWorker.AddParameter("@IsPublished", isPublished);
                mQueryWorker.AddParameter("@Id", contentTypeId);
                count = (int)mQueryWorker.ExecuteScalar(cmdText);
            });
            return count > 0;
        }

        #endregion publishing contentType

         #endregion mms

        #region sql method

        private List<int> GetIntValues(IDataReader reader, int columnIndex, bool distinct=true)
        {
            //todo:wbhu,挪到sql公共方法中
            var ids = new List<int>();
            while (reader.Read())
            {
                int id = reader.GetInt32(columnIndex);
                if (!distinct)
                {
                    ids.Add(id);
                }
                else
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }

        /// <summary>
        /// 获取指定index上的Guid value集合
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <param name="distinct">获取的集合是否去重</param>
        /// <returns></returns>
        private List<Guid> GetGuidValues(IDataReader reader, int columnIndex, bool distinct = true)
        {
            //todo:wbhu,挪到sql公共方法中
            var ids = new List<Guid>();
            while (reader.Read())
            {
                var id = reader.GetGuid(columnIndex);
                if (!distinct)
                {
                    ids.Add(id);
                }
                else
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }

        #endregion

        #region common methods

        /// <summary>
        /// Convert a collection to a string with the specific separator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private string GetListString<T>(ICollection<T> list, char separator)
        {
            if (list == null || list.Count == 0) return null;
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (T local in list)
            {
                if (!flag) builder.Append(separator);
                flag = false;
                builder.Append(local);
            }
            return builder.ToString();
        }

        private static string GetRealPrincipalName(string pName)
        {
            if (pName.StartsWith("c:0+.w|s", StringComparison.OrdinalIgnoreCase))
            {
                pName = pName.Substring(7);
                pName = AveObjectModelFactory.CreateObjectModelFactory("", null)
                    .CreatePeopleEditor()
                    .GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(pName));
            }
            else if (pName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase) || pName.StartsWith("c:0-.f|", StringComparison.OrdinalIgnoreCase))
            {
                pName = pName.Substring(7);
            }
            return pName;
        }

        #endregion

        #endregion

        #region Interface Implement

        /// <summary>
        /// 获取MMS的default Language,同时返回AveTermStoreInfo信息(只有TermStoreAdministrators信息)
        /// 出错return 0,正常return一个languageId
        /// </summary>
        /// <param name="termStoreInfo"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [Logic("wbhu")]
        //todo:wbhu,1.方法名称不准确，方法获取了TermStoreAdministrators信息和default language信息,2.ref 返回值不合理，从逻辑看用out更合理，改接口时要处理下
        public int GetLanguage(ref AveTermStoreInfo termStoreInfo, Guid defaultPartitionId)
        {
            termStoreInfo = new AveTermStoreInfo();
            int defaultLanguage = 0;

            try
            {
                defaultLanguage = GetTermStoreLanguage(termStoreInfo, defaultPartitionId, true);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetLanguageError, ex);
            }
            return defaultLanguage;
        }

        /// <summary>
        /// 获取MMS的default Language
        /// 出错return 0,正常return一个languageId
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        //todo:wbhu,方法名称不准确，获取什么的language?
        public int GetLanguage(Guid defaultPartitionId)
        {
            int defaultLanguage = 0;
            try
            {
                defaultLanguage = GetTermStoreLanguage(null, defaultPartitionId, false);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetLanguageError, ex);
            }
            return defaultLanguage;
        }

        /// <summary>
        /// 返回AveTermStoreInfo信息(只有TermStoreAdministrators信息)
        /// </summary>
        /// <param name="defaultPartitionId">Term store PartitionId</param>
        /// <returns>返回AveTermStoreInfo信息</returns>
        [Logic("wbhu")]
        public AveTermStoreInfo GetTermStoreInfo(Guid defaultPartitionId)
        {
            AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
            try
            {
                ExceptionHandlingScope(() =>
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@PartitionId", SqlDbType.UniqueIdentifier, defaultPartitionId);
                    string commandText = GetTermStoreInfo_Select_ECMPermission;
                    mQueryWorker.Command.CommandType = CommandType.Text;
                    using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
                    {
                        while (reader.Read())
                        {
                            string pName = reader.GetString(0);
                            ulong mask = (ulong) reader.GetInt64(1);
                            pName = GetRealPrincipalName(pName);
                            var termStoreAdministratorRight = AveTaxonomyRights.ManageTermStore | AveTaxonomyRights.TermStoreAdministrator;
                            var termStoreAdministratorInfo = GetAceInfoByPermission(termStoreAdministratorRight, mask, pName, false);
                            if (termStoreAdministratorInfo != null)
                            {
                                if (termStoreInfo.TermStoreAdministrators == null)
                                {
                                    termStoreInfo.TermStoreAdministrators = new List<AveAceInfo>();
                                }
                                termStoreInfo.TermStoreAdministrators.Add(termStoreAdministratorInfo);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting MMS Term Store Info.Error Message{0}", ex);
            }
            return termStoreInfo;
        }

        /// <summary>
        /// 获取Global Groups,效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId)
        {
            //todo:wbhu,需要review下，一个连表查询应该就可以替代当前的一个query+一个含有三个query的存储过程
            List<AveMetadataGroupInfo> groups = null;
            ExceptionHandlingScope(() =>
            {
                 string command = GetGlobalGroups_Select_ECMGroup;
                List<int> ids;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
                {
                    ids = GetIntValues(reader, 0);
                }
                groups = GetTermGroups(ids, defaultPartitionId);
            });
            return groups;
        }


        /// <summary>
        /// 按照GroupId（Guid）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId)
        {
            AveMetadataGroupInfo groupInfo = null;
            ExceptionHandlingScope(() =>
            {
                string command = GetGroupByGuid_Select_ECMGroup;
                List<int> ids = null;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@UniqueId", groupId);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
                {
                    ids = GetIntValues(reader, 0);
                }
                groupInfo = GetTermGroups(ids, defaultPartitionId).FirstOrDefault(); 
            });
            return groupInfo;
        }


        /// <summary>
        /// 按照GroupName获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId)
        {
            AveMetadataGroupInfo groupInfo = null;
            ExceptionHandlingScope(() =>
            {
                 string command = GetGroupByName_Select_ECMGroup;
                List<int> ids = null;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@Name", groupName);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
                {
                    ids = GetIntValues(reader, 0);
                }
                groupInfo = GetTermGroups(ids, defaultPartitionId).FirstOrDefault();
            });
            return groupInfo;
        }

        /// <summary>
        /// 按照GroupId（Int）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId)
        {
            AveMetadataGroupInfo groupInfo = null;
            ExceptionHandlingScope(() =>
            {
                var ids = new List<int> {groupId};
                groupInfo = GetTermGroups(ids, defaultPartitionId).FirstOrDefault();
            });
            return groupInfo;
        }

        /// <summary>
        /// 获取Local Groups，效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/04/26", "Qianwen Hu")]
        public List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId)
        {
            List<AveMetadataGroupInfo> groups = null;
            ExceptionHandlingScope(() =>
            {
                //todo:wbhu,这个query不合理，应该用defaultPartitionId做条件，而不是把所有local group查出来
                //todo:wbhu,考虑下能否替换GetTermGroups里的存储过程，查询次数和数据量可以有所减少
                string command = GetLocalGroups_Select_ECMGroup;
                List<int> ids;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
                {
                    ids = GetIntValues(reader, 0);
                }
                groups = GetTermGroups(ids, defaultPartitionId);
            });
            return groups;
        }

        /// <summary>
        /// 按照Guid获取指定TermSet下指定Term信息（唯一），效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            AveTermInfo termInfo = null;
            ExceptionHandlingScope(() =>
            {
                var ids = new List<int> {termId};
                var termSetIdInt = GetTermSetIdByGuid(termSetId, defaultPartitionId);
                termInfo = GetTermsByIds(ids, termSetIdInt, false, defaultPartitionId, defaultLanguage).FirstOrDefault();
            });
            return termInfo;
        }

        
        /// <summary>
        /// 按照Guid获取指定TermSet中,指定Term下的Terms信息（多值），效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang", true, "use index as query")]
        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId, int defaultLanguage)
        {
            List<AveTermInfo> terms = null;
            ExceptionHandlingScope(() =>
            {
                var intTermSetId = GetTermSetIdByGuid(termSetId, defaultPartitionId);

                 string cmdText = GetTermsIdInTerm_Select_ECMTermSetMembership_ECMTerm;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@TermId", termId);
                mQueryWorker.AddParameter("@TermSetId", intTermSetId);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                List<int> termIds = null;
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    termIds = GetIntValues(reader, 0);
                }

                terms = GetTermsByIds(termIds, intTermSetId, false, defaultPartitionId, defaultLanguage);
            });
            return terms;
        }

        /// <summary>
        /// 按照Guid获取termSet下的全部Terms信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang", true, "use index as query")]
        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId, int defaultLanguage)
        {

            List<AveTermInfo> terms = null;
            ExceptionHandlingScope(() =>
            {
                 string cmdText = GetTermsIdInTermSet_Select_ECMTermSetMembership_ECMTermSet;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SetId", termSetId);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                List<int> ids = null;
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    ids = GetIntValues(reader, 0);
                }
                var termSetIdInt = GetTermSetIdByGuid(termSetId, defaultPartitionId);
                terms = GetTermsByIds(ids, termSetIdInt, false, defaultPartitionId, defaultLanguage);
            });
            return terms;
        }

        /// <summary>
        /// 按照TermSet Guid获取TermSet下的Term的UniqueId集合，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public List<Guid> GetTermIds(Guid termSetId, Guid defaultPartitionId)
        {
            List<Guid> ids = null;
            ExceptionHandlingScope(() =>
            {
                var termSetIdInt = GetTermSetIdByGuid(termSetId, defaultPartitionId);

                 string cmdText = GetTermsUniqueIdInTermSet_Select_ECMTermSetMembership_ECMTerm;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", termSetIdInt);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);


                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    ids = GetGuidValues(reader, 0);
                }
            });
            return ids;
        }

        /// <summary>
        /// 按照Guid获取特定TermSet中指定Term下的Term UniqueId 集合，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns>返回一个集合,可能为空，不会返回null</returns>
        [QueryReview("2012/05/17", "Long Liang", true, "re-order the index")]
        public List<Guid> GetTermIds(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {
            List<Guid> ids = null;
            ExceptionHandlingScope(() =>
            {
                ids = GetTermIdsInTerm(termSetId, termId, defaultPartitionId);
            });
            return ids;
        }

        /// <summary>
        /// 判断是否是SiteCollection下的Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang")]
        public bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId)
        {
            var isSiteCollectionGroup = false;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                mQueryWorker.AddParameter("@GroupId", groupId);
                try
                {
                     string txt = GetTermGroupTypeByUniqueId_Select_ECMGroup;
                    var type = (int) mQueryWorker.ExecuteScalar(txt);
                    if (type == 2)
                    {
                        isSiteCollectionGroup = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.IsSiteCollectionGroupError, ex);
                }
            });
            return isSiteCollectionGroup;
        }

        /// <summary>
        /// 通过TermGroupId获取SiteCollectionId，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public List<Guid> GetSiteCollectionIdList(Guid groupId, Guid defaultPartitionId)
        {
            var siteList = new List<Guid>();
            ExceptionHandlingScope(() =>
            {
                 string txt = GetTermGroupPrincipalName_Select_ECMPermission_ECMGroup;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                //mQueryWorker.AddParameter("@GroupId", groupId);
                mQueryWorker.AddParameter("@UniqueId", groupId);
                try
                {
                    using (SqlDataReader reader = mQueryWorker.ExecuteReader(txt))
                    {
                        while (reader.Read())
                        {
                            var pName = reader.GetString(0);
                            if (pName.StartsWith("SiteCollectionId:", StringComparison.OrdinalIgnoreCase))
                            {
                                //可查看Microsoft.SharePoint.Taxonomy.Internal.Security.DeserializePermission(IDataReader dataReader, SPAcl<TaxonomyRights> acl, List<Guid> siteCollectionIds)源代码
                                var item = new Guid(pName.Substring("SiteCollectionId:".Length));
                                siteList.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetSiteCollectionIdError, ex);
                }
            });
            return siteList;
        }

        /// <summary>
        /// 从EMCChangeLog中获取特定Group+TermSet下的Changes为Incremental处理，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId">Allow null</param>
        /// <param name="termSetId">Allow null</param>
        /// <param name="sinceTime">Not null</param>
        /// <param name="changedItemType">Allow null</param>
        /// <param name="defaultPartitionId">Not null</param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChanges(int? groupId, int? termSetId, DateTime sinceTime, int? changedItemType, Guid defaultPartitionId)
        {
            var items = new List<AveTermChangeItem>();
            ExceptionHandlingScope(() =>
            {
                 string proc_ECM_GetChanges = GetMetadataServiceChanges_Select_proc_ECM_GetChanges;
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
                mQueryWorker.AddParameter("@PartitionId", SqlDbType.UniqueIdentifier, defaultPartitionId);
                mQueryWorker.AddParameter("@SinceTime", SqlDbType.DateTime, sinceTime);
                if (groupId.HasValue)
                {
                    mQueryWorker.AddParameter("@GroupId", SqlDbType.Int, groupId.Value);
                }
                if (termSetId.HasValue)
                {
                    mQueryWorker.AddParameter("@TermSetId", SqlDbType.Int, termSetId.Value);
                }
                if (changedItemType.HasValue)
                {
                    mQueryWorker.AddParameter("@ChangedItemType", SqlDbType.Int, changedItemType.Value);
                }

                using (var reader = mQueryWorker.ExecuteReader(proc_ECM_GetChanges))
                {
                    // N'SELECT '
                    //+ N' cl.PartitionId '
                    //+ N',cl.GroupUniqueId  ' 
                    //+ N',cl.TermSetUniqueId ' 
                    //+ N',cl.ObjectUniqueId ' 
                    //+ N',cl.ObjectId  ' 
                    //+ N',cl.ObjectType  ' 
                    //+ N',cl.ChangeType  ' 
                    //+ N',cl.ChangeTime  '     
                    //+ N',cl.ChangeData  ' 
                    //+ N',cl.ModifiedBy  ' 

                    while (reader.Read())
                    {
                        var item = new AveTermChangeItem();

                        if (!reader.IsDBNull(4))
                        {
                            item.ObjectId = reader.GetInt32(4);
                        }

                        if (!reader.IsDBNull(3))
                        {
                            item.Id = reader.GetGuid(3);
                        }

                        item.ItemType = (AveTermChangeItem.ChangedItemType) reader.GetInt32(5);
                        item.ChangeType = (AveTermChangeItem.ChangedOperationType) reader.GetInt32(6);
                        item.ChangeTime = reader.GetDateTime(7);
                        item.PartitionId = defaultPartitionId;
                        if (!reader.IsDBNull(2))
                        {
                            item.TermSetId = reader.GetGuid(2);
                        }

                        if (!reader.IsDBNull(1))
                        {
                            item.GroupId = reader.GetGuid(1);
                        }

                        if (!reader.IsDBNull(8))
                        {
                            item.ChangeData = reader.GetString(8);
                        }

                        items.Add(item);
                    }
                }
            });
            return items;
        }

        /// <summary>
        /// 获取term下change的terms,为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="sinceTime">如果为null，则查询term下所有term，不为null，按change log查询</param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [Logic("wbhu", "类功能不唯一，如果sinceTime为空，就会返回Term下所有的Term，考虑把这段逻辑挪出去,需要动接口")]
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of sql statement. ")]
        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            var items = new List<AveTermChangeItem>();
            ExceptionHandlingScope(() =>
            {
                if (sinceTime.HasValue)
                {
                    string GetTermIdCommand = GetTermIdByGuid_Select_ECMTerm;

                    mQueryWorker.Command.CommandType = CommandType.Text;
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@TermId", termId);
                    mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                    var termIntId = (int) mQueryWorker.ExecuteScalar(GetTermIdCommand);

                    string cmdText = GetChangeTermsInTerm_Select_ECMChangeLog_ECMTermSetMembership;
                    mQueryWorker.ClearParameters();
                    mQueryWorker.Command.CommandType = CommandType.Text;
                    mQueryWorker.AddParameter("@UniqueId", SqlDbType.UniqueIdentifier, termSetId);
                    mQueryWorker.AddParameter("@TermId", SqlDbType.Int, termIntId);
                    mQueryWorker.AddParameter("@SinceTime", sinceTime.Value);
                    mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                    using (var reader = mQueryWorker.ExecuteReader(cmdText))
                    {
                        var ids = new List<int>();
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(3);
                            if (ids.Contains(id))
                            {
                                continue;
                            }
                            ids.Add(id);
                            var item = new AveTermChangeItem
                            {
                                Id = reader.GetGuid(2),
                                ItemType = (AveTermChangeItem.ChangedItemType) reader.GetInt32(4),
                                ChangeType = (AveTermChangeItem.ChangedOperationType) reader.GetInt32(5),
                                ChangeTime = reader.GetDateTime(6),
                                PartitionId = defaultPartitionId,
                                GroupId = reader.GetGuid(0),
                                Path = reader.GetString(9),
                                TermSetId = reader.IsDBNull(1) ? (Guid?) null : reader.GetGuid(1)
                            };
                            items.Add(item);
                        }
                    }
                }
                else
                {
                    var terms = GetTermIds(termSetId, termId, defaultPartitionId);
                    items.AddRange(terms.Select(t =>
                        new AveTermChangeItem
                        {
                            TermSetId = termSetId,
                            ItemType = AveTermChangeItem.ChangedItemType.Term,
                            ChangeType = 0,
                            Id = t
                        }));
                }
                //todo:needReview,每个term都查一遍，只为取一个name,
                //todo:needReview GetTerm从逻辑上可能返回null,会造成空引用,已经加判断修改
                foreach (var item in items)
                {
                    var termInfo = GetTerm(termSetId, GetTermIdByGuid(item.Id, defaultPartitionId), defaultPartitionId, defaultLanguage);
                    if (termInfo == null)
                    {
                        logger.Warn("termInfo is null.Term Id:{0}, TermSet Id:{1}", item.Id, item.TermSetId);
                    }
                    else
                    {
                        item.Name = termInfo.Name;
                        item.IsPinned = termInfo.IsPinned;
                        item.IsReused = termInfo.IsReused;
                        item.IsRoot = termInfo.IsRoot;
                        item.IsSourceTerm = termInfo.IsSourceTerm;
                        item.PinSourceTermSetId = termInfo.PinSourceTermSetId;
                        item.Path = GetTermPath(defaultPartitionId, GetTermId(termId, defaultPartitionId), termInfo.IsSourceTerm ? 1 : 0);
                    }
                }
            });
            return items;
        }

        public List<AveTermChangeItem> GetTermSetChildren(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetTermSetChildren_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", termSetUniqueId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            List<Tuple<AveTermInfo, int, int, int>> termInfos = new List<Tuple<AveTermInfo, int, int, int>>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    int pinSourceTermSetId = 0;
                    int termId = 0;
                    int parentTermId = 0;
                    AveTermInfo termInfo = new AveTermInfo();
                    termId = reader.GetInt32(0);
                    termInfo.ParentTermSetId = reader.GetGuid(6);
                    termInfo.Id = reader.GetGuid(1);
                    termInfo.IsSourceTerm = reader.GetBoolean(4);
                    termInfo.IsRoot = reader.GetInt32(3) == 0;
                    parentTermId = reader.GetInt32(3);
                    termInfo.IsPinned = (reader.IsDBNull(5) ? 0 : reader.GetInt32(5)) != 0;
                    pinSourceTermSetId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                    termInfo.PartitionId = partitionId;
                    Tuple<AveTermInfo, int, int, int> temp = new Tuple<AveTermInfo, int, int, int>(termInfo, pinSourceTermSetId, termId, parentTermId);
                    termInfos.Add(temp);
                }
            }
            List<AveTermChangeItem> terms = new List<AveTermChangeItem>();
            foreach (var item in termInfos)
            {
                item.Item1.IsReused = GetTermIsReusedProperty(item.Item3, partitionId);

                if (item.Item2 != 0)
                {
                    Guid pinSourceTermSetUniqueId = GetTermPinSourceTermSetUniqueId(item.Item2, partitionId);
                    if (pinSourceTermSetUniqueId != Guid.Empty)
                    {
                        item.Item1.PinSourceTermSetId = pinSourceTermSetUniqueId;
                    }
                }

                if (!item.Item1.IsRoot)
                {
                    item.Item1.ParentTermId = GetParentTermUniqueIdForTermSet(termSetUniqueId, item.Item4, partitionId, defaultLanguage);
                }

                GetTermLabels(item.Item1, item.Item3, partitionId, defaultLanguage);

                if (item.Item1.ParentTermId != Guid.Empty)
                {
                    AveTermChangeItem term = new AveTermChangeItem();
                    term.Id = item.Item1.Id;
                    term.Name = item.Item1.Name;
                    term.IsPinned = item.Item1.IsPinned;
                    term.IsReused = item.Item1.IsReused;
                    term.IsRoot = item.Item1.IsRoot;
                    term.IsSourceTerm = item.Item1.IsSourceTerm;
                    term.PinSourceTermSetId = item.Item1.PinSourceTermSetId;
                    term.ParentTermId = item.Item1.ParentTermId;
                    term.TermSetId = item.Item1.ParentTermSetId;
                    term.PartitionId = item.Item1.PartitionId;
                    term.ItemType = AveTermChangeItem.ChangedItemType.Term;
                    term.ChangeType = 0;
                    term.Path = GetTermPath(partitionId, GetTermId(item.Item1.Id, partitionId), item.Item1.IsSourceTerm ? 1 : 0);
                    AveMetadataGroupInfo groupInfo = GetTermGroupInfo(term.ParentTermId, partitionId, defaultLanguage);
                    term.GroupId = groupInfo.Id;
                    if (groupInfo.IsSiteCollectionGroup)
                    {
                        term.IsGlobalGroup = false;
                    }
                    terms.Add(term);
                }
            }
            return terms;
        }

        public AveTermChangeItem GetTermSetParent(Guid termSetId, Guid partitionId, int defaultLanguage)
        {
            AveTermChangeItem item = new AveTermChangeItem();
            AveMetadataGroupInfo termGroupInfo = GetTermGroupInfo(termSetId, partitionId, defaultLanguage);
            item.Id = termGroupInfo.Id;
            item.GroupId = termGroupInfo.Id;
            item.ChangeType = 0;
            item.ItemType = AveTermChangeItem.ChangedItemType.Group;
            item.Name = termGroupInfo.Name;
            if (termGroupInfo.IsSiteCollectionGroup)
            {
                item.IsGlobalGroup = false;
            }
            return item;
        }

        private AveMetadataGroupInfo GetTermGroupInfo(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetTermGroupInfo_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", termSetUniqueId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            AveMetadataGroupInfo termGroupInfo = new AveMetadataGroupInfo();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    termGroupInfo.Id = reader.GetGuid(1);
                    termGroupInfo.Name = reader.GetString(2);
                    termGroupInfo.Description = reader.GetString(3);
                    int type = reader.GetInt32(4);
                    if (type == 1)
                    {
                        termGroupInfo.IsSystemGroup = true;
                        termGroupInfo.IsSiteCollectionGroup = false;
                    }
                    else if (type == 2)
                    {
                        termGroupInfo.IsSystemGroup = false;
                        termGroupInfo.IsSiteCollectionGroup = true;
                    }
                    else
                    {
                        termGroupInfo.IsSystemGroup = false;
                        termGroupInfo.IsSiteCollectionGroup = false;
                    }
                }
            }
            return termGroupInfo;
        }

        public AveTermChangeItem GetTermParent(Guid termSetId, Guid termId, Guid parentTermId, Guid partitionId, bool isRoot, bool isSourceTerm, int defaultLanguage)
        {
            AveTermChangeItem item = new AveTermChangeItem();
            if (isSourceTerm)
            {
                if (isRoot)
                {
                    AveTermSetInfo termSetInfo = GetTermSetInfo(termSetId, partitionId, defaultLanguage);
                    item.TermSetId = termSetInfo.Id;
                    item.ItemType = AveTermChangeItem.ChangedItemType.TermSet;
                    item.ChangeType = 0;
                    item.Id = termSetInfo.Id;
                    item.Name = termSetInfo.Name;
                    item.GroupId = termSetInfo.ParentId;
                    item.TermSetType = termSetInfo.Type;
                    item.PartitionId = partitionId;
                }
                else
                {
                    AveTermInfo termInfo = GetParentTermInfo(termSetId, parentTermId, partitionId, defaultLanguage);
                    item.ItemType = AveTermChangeItem.ChangedItemType.Term;
                    item.TermSetId = termInfo.ParentTermSetId;
                    item.Id = termInfo.Id;
                    item.ChangeType = 0;
                    item.Name = termInfo.Name;
                    item.IsPinned = termInfo.IsPinned;
                    item.IsReused = termInfo.IsReused;
                    item.IsRoot = termInfo.IsRoot;
                    item.IsSourceTerm = termInfo.IsSourceTerm;
                    item.PinSourceTermSetId = termInfo.PinSourceTermSetId;
                    item.ParentTermId = termInfo.ParentTermId;
                    item.PartitionId = partitionId;
                    item.Path = GetTermPath(partitionId, GetTermId(termInfo.Id, partitionId), termInfo.IsSourceTerm ? 1 : 0);
                    AveMetadataGroupInfo groupInfo = GetTermGroupInfo(termInfo.ParentTermSetId, partitionId, defaultLanguage);
                    item.GroupId = groupInfo.Id;
                    if (groupInfo.IsSiteCollectionGroup)
                    {
                        item.IsGlobalGroup = false;
                    }
                }
            }
            else
            {
                AveTermInfo termInfo = GetSourceTermInfo(termSetId, GetTermId(termId, partitionId), partitionId, defaultLanguage);
                item.ItemType = AveTermChangeItem.ChangedItemType.Term;
                item.TermSetId = termInfo.ParentTermSetId;
                item.Id = termInfo.Id;
                item.ChangeType = 0;
                item.Name = termInfo.Name;
                item.IsPinned = termInfo.IsPinned;
                item.IsReused = termInfo.IsReused;
                item.IsRoot = termInfo.IsRoot;
                item.IsSourceTerm = termInfo.IsSourceTerm;
                item.PinSourceTermSetId = termInfo.PinSourceTermSetId;
                item.ParentTermId = termInfo.ParentTermId;
                item.PartitionId = partitionId;
                item.Path = GetTermPath(partitionId, GetTermId(termInfo.Id, partitionId), termInfo.IsSourceTerm ? 1 : 0);
                AveMetadataGroupInfo groupInfo = GetTermGroupInfo(termInfo.ParentTermSetId, partitionId, defaultLanguage);
                item.GroupId = groupInfo.Id;
                if (groupInfo.IsSiteCollectionGroup)
                {
                    item.IsGlobalGroup = false;
                }
            }
            return item;
        }

        private AveTermInfo GetParentTermInfo(Guid termSetId, Guid parentTermUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetParentTermInfo_Select_ECMTermSet;

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ParentTermUniqueId", parentTermUniqueId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            mQueryWorker.AddParameter("@TermSetId", termSetId);

            AveTermInfo termInfo = new AveTermInfo();
            int pinSourceTermSetId = 0;
            int termId = 0;
            int parentTermId = 0;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    termId = reader.GetInt32(0);
                    termInfo.ParentTermSetId = reader.GetGuid(14);
                    termInfo.Id = reader.GetGuid(3);
                    termInfo.IsSourceTerm = reader.GetBoolean(12);
                    termInfo.IsRoot = reader.GetInt32(9) == 0;
                    parentTermId = reader.GetInt32(9);
                    termInfo.IsPinned = (reader.IsDBNull(13) ? 0 : reader.GetInt32(13)) != 0;
                    pinSourceTermSetId = reader.IsDBNull(13) ? 0 : reader.GetInt32(13);
                    termInfo.PartitionId = partitionId;
                }
            }
            termInfo.IsReused = GetTermIsReusedProperty(termId, partitionId);

            if (pinSourceTermSetId != 0)
            {
                Guid pinSourceTermSetUniqueId = GetTermPinSourceTermSetUniqueId(pinSourceTermSetId, partitionId);
                if (pinSourceTermSetUniqueId != Guid.Empty)
                {
                    termInfo.PinSourceTermSetId = pinSourceTermSetUniqueId;
                }
            }

            if (!termInfo.IsRoot)
            {
                termInfo.ParentTermId = GetParentTermUniqueIdForTermSet(termSetId, parentTermId, partitionId, defaultLanguage);
            }

            GetTermLabels(termInfo, termId, partitionId, defaultLanguage);

            return termInfo;
        }

        private AveTermSetInfo GetTermSetInfo(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetTermSetInfo_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", termSetUniqueId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            AveTermSetInfo termSetInfo = new AveTermSetInfo();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    string name = reader.GetString(1);
                    //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                    int tmpLength = 0;
                    string tmpName = name;
                    tmpLength = tmpName.IndexOf(defaultLanguage + "|", StringComparison.OrdinalIgnoreCase);
                    if (tmpLength >= 0)
                    {
                        tmpName = tmpName.Substring(tmpLength + defaultLanguage.ToString().Length + 1);
                        tmpLength = tmpName.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                        if (tmpLength >= 0)
                        {
                            tmpName = tmpName.Substring(0, tmpLength);
                        }
                    }
                    name = tmpName;
                    termSetInfo.ParentId = reader.GetGuid(0);
                    termSetInfo.Id = termSetUniqueId;
                    termSetInfo.Name = name;
                    termSetInfo.Type = reader.GetByte(2);
                }
            }
            return termSetInfo;
        }

        private AveTermInfo GetSourceTermInfo(Guid termSetId, int termId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetSourceTermInfo_Select_ECMTermSet;

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", termId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);

            AveTermInfo termInfo = new AveTermInfo();
            int pinSourceTermSetId = 0;
            int parentTermId = 0;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    termInfo.ParentTermSetId = reader.GetGuid(14);
                    termInfo.Id = reader.GetGuid(3);
                    termInfo.IsSourceTerm = reader.GetBoolean(12);
                    termInfo.IsRoot = reader.GetInt32(9) == 0;
                    parentTermId = reader.GetInt32(9);
                    termInfo.IsPinned = (reader.IsDBNull(13) ? 0 : reader.GetInt32(13)) != 0;
                    pinSourceTermSetId = reader.IsDBNull(13) ? 0 : reader.GetInt32(13);
                    termInfo.PartitionId = partitionId;
                }
            }
            termInfo.IsReused = GetTermIsReusedProperty(termId, partitionId);

            if (pinSourceTermSetId != 0)
            {
                Guid pinSourceTermSetUniqueId = GetTermPinSourceTermSetUniqueId(pinSourceTermSetId, partitionId);
                if (pinSourceTermSetUniqueId != Guid.Empty)
                {
                    termInfo.PinSourceTermSetId = pinSourceTermSetUniqueId;
                }
            }

            if (!termInfo.IsRoot)
            {
                termInfo.ParentTermId = GetParentTermUniqueIdForTermSet(termInfo.ParentTermSetId, parentTermId, partitionId, defaultLanguage);
            }

            GetTermLabels(termInfo, termId, partitionId, defaultLanguage);

            return termInfo;
        }

        private Guid GetParentTermUniqueIdForTermSet(Guid termSetId, int parentTermId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = GetParentTermUniqueIdForTermSet_Select_ECMTermSet;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", parentTermId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            mQueryWorker.AddParameter("@TermSetId", termSetId);

            Guid parentTermUniqueId = Guid.Empty;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    parentTermUniqueId = reader.GetGuid(0);
                }
            }
            return parentTermUniqueId;
        }


        /// <summary>
        /// 根据term的UniqueId获取int的TermId，效率考虑，有API实现.
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " re-order index")]
        public int GetTermId(Guid termId, Guid defaultPartitionId)
        {
            var termIdInt = 0;
            ExceptionHandlingScope(() =>
            {
                termIdInt = GetTermIdByGuid(termId, defaultPartitionId);
            });
            return termIdInt;
        }

        /// <summary>
        /// 按照Global或者Local获取GroupIds信息，效率考虑，有API实现. TODO需要添加partitionId
        /// </summary>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetGroupIds(bool isGlobal)
        {
            return GetGroupIds(isGlobal, null);
        }

        /// <summary>
        /// 获取指定Guid的GroupId，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int? GetGroupId(Guid groupId)
        {
            object result = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetTermGroupIdByUniqueId_Select_ECMGroup;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@UniqueId", groupId);
                result = mQueryWorker.ExecuteScalar(cmdText);
            });
            return result is DBNull ? (int?)null : (int?)result;
        }

        /// <summary>
        /// Get the group id by unique id in specific term store
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public int? GetGroupId(Guid groupId, Guid defaultPartitionId)
        {
            object result = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetTermGroupIdInStoreByUniqueId_Select_ECMGroup;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                mQueryWorker.AddParameter("@UniqueId", groupId);
                result = mQueryWorker.ExecuteScalar(cmdText);
            });
            return result is DBNull ? (int?)null : (int?)result;
        }


        /// <summary>
        /// 判断contentType是否published，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang")]
        public bool IsPublished(string contentTypeId, Guid defaultPartitionId)
        {
            return IsPublishingContentTypeExist(contentTypeId, defaultPartitionId, 1);
        }

        /// <summary>
        /// 判断contentType是否unpublished，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang")]
        public bool IsUnPublished(string contentTypeId, Guid defaultPartitionId)
        {
            return IsPublishingContentTypeExist(contentTypeId, defaultPartitionId, 0);
        }

        /// <summary>
        /// 获取TermStore setting xml，效率考虑，有API实现.
        /// </summary>
        /// <param name="defaultpartitionId"></param>
        /// <returns>如果TermStore不存在，返回String.Empty</returns>
        public string GetTermStore(Guid defaultpartitionId)
        {
            var settingXml = string.Empty;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.StoredProcedure);
                mQueryWorker.AddParameter("@PartitionId", SqlDbType.UniqueIdentifier, defaultpartitionId);
                using (var reader = mQueryWorker.ExecuteReader("proc_ECM_GetServiceSettings"))
                {
                    if (reader.Read())
                    {
                        settingXml = reader.GetString(0);
                    }
                }
            });
            return settingXml;
        }

        /// <summary>
        /// 获取MMS下store中的Group改变，为Incremental处理，效率考虑，有API实现
        /// 如果sinceTime为空，则查询出所有Group
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [Logic("wbhu", "方法内部没有实际的Query操作")]
        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {
            return GetChangesInStoreInternal(sinceTime, isGlobal, defaultPartitionId, false);
        }

        /// <summary>
        ///获取MMS下store中的Group改变，为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="isMetadataPartition">这里默认就是True，也就是说只有Tenant MMS调这个方法</param>
        /// <returns></returns>
        [Logic("wbhu", "方法内部没有实际的Query操作")]
        public List<AveTermChangeItem> GetChangesInStoreForTenant(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {
           return GetChangesInStoreInternal(sinceTime, isGlobal, defaultPartitionId, true);
        }

        /// <summary>
        /// 获取MMS某个Group下的TermSet改变为Incremental处理,效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [Logic("wbhu", "方法内部没有实际的Query操作")]
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            if (sinceTime.HasValue)
            {
                var temp = GetChanges(GetGroupId(groupId), null, sinceTime.Value, null, defaultPartitionId);
                var changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.TermSet);
                foreach (var changeItem in changes.Where(item => item.ChangeType != AveTermChangeItem.ChangedOperationType.Delete && item.TermSetId.HasValue))
                {
                    var info = GetTermSet(changeItem.TermSetId.Value, defaultPartitionId, defaultLanguage);
                    changeItem.Name = info.Name;
                    changeItem.TermSetType = info.Type;
                    changeItem.ObjectId = GetTermSetIdByGuid(changeItem.TermSetId.Value, defaultPartitionId);
                    changeItem.PartitionId = defaultPartitionId;
                }
                return changes;
            }
            var sets = GetTermSetIds(groupId, defaultPartitionId, defaultLanguage);
            return sets.Select(t => new AveTermChangeItem
            {
                TermSetId = t.Id,
                ItemType = (AveTermChangeItem.ChangedItemType) 2,
                ChangeType = 0,
                Id = t.Id,
                Name = t.Name,
                GroupId = groupId,
                TermSetType = t.Type,
                PartitionId = defaultPartitionId
            }).ToList();
        }

        /// <summary>
        /// 获取TermSet中的Term改变为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            var results = new List<AveTermChangeItem>();
            if (sinceTime.HasValue)
            {
                var temp = GetChanges(null, GetTermSetIdByGuid(termSetId, defaultPartitionId), sinceTime.Value, 1, defaultPartitionId);
                var changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.Term);
                foreach (var changeItemTree in changes.Select(item => GetChangeItemTree(item, defaultPartitionId, defaultLanguage))
                    .Where(changeItemTree => changeItemTree != null))
                {
                    AddToResults(results, changeItemTree);
                }
            }
            else
            {
                var terms = GetTermsInTermSet(termSetId, defaultPartitionId, defaultLanguage);
                results.AddRange(
                    terms.Select(t =>
                        new AveTermChangeItem
                        {
                            TermSetId = termSetId,
                            ItemType = (AveTermChangeItem.ChangedItemType) 1,
                            ChangeType = 0,
                            Id = t.Id,
                            Name = t.Name,
                            IsPinned = t.IsPinned,
                            IsReused = t.IsReused,
                            IsRoot = t.IsRoot,
                            IsSourceTerm = t.IsSourceTerm,
                            PinSourceTermSetId = t.PinSourceTermSetId,
                            Path = GetTermPath(defaultPartitionId, GetTermId(t.Id, defaultPartitionId), t.IsSourceTerm ? 1 : 0)
                        }));

            }
            return results;
        }

        /// <summary>
        /// 查询特定时间范围内，特定group上的Metadata change item.
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <param name="groupId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            return GetMetaDataChanges(defaultPartitionId, sinceTime, toTime, groupId, AveTermChangeItem.ChangedItemType.Group);
        }

        /// <summary>
        /// 查询特定时间范围内，特定TermSet上的Metadata change item.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="toTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            return GetMetaDataChanges(defaultPartitionId, sinceTime, toTime, termSetId, AveTermChangeItem.ChangedItemType.TermSet);
        }
        /// <summary>
        /// 按照Id获取Group下的TermSet信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="partitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " use index as join key and please add PartitionId in feature")]
        public List<AveTermSetInfo> GetTermSetIds(Guid groupId,Guid partitionId, int defaultLanguage)
        {
            var setInfo = new List<AveTermSetInfo>();
            ExceptionHandlingScope(() =>
            {
                var commandText = GetTermSetsInGroup_Select_ECMTermSet_ECMGroup;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@GroupId", groupId);
                mQueryWorker.AddParameter("@PartitionId", partitionId);
                using (var reader = mQueryWorker.ExecuteReader(commandText))
                {
                    while (reader.Read())
                    {
                        var info = new AveTermSetInfo();
                        var id = reader.GetGuid(0);
                        var name = reader.GetString(1);
                        //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                        var tmpName = name;
                        var tmpLength = tmpName.IndexOf(defaultLanguage + "|", StringComparison.OrdinalIgnoreCase);
                        if (tmpLength >= 0)
                        {
                            tmpName = tmpName.Substring(tmpLength + defaultLanguage.ToString().Length + 1);
                            tmpLength = tmpName.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                            if (tmpLength >= 0)
                            {
                                tmpName = tmpName.Substring(0, tmpLength);
                            }
                        }
                        name = tmpName;
                        info.Id = id;
                        info.Name = name;
                        info.Type = reader.GetByte(2);
                        setInfo.Add(info);

                    }
                }
            });
            return setInfo;
        }

        /// <summary>
        /// 通过Id获取Group下的TermSets，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        public List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId, int defaultLanguage)
        {
            List<AveTermSetInfo> termSets = null;
            ExceptionHandlingScope(() =>
            {
                const string commandText = GetTermSetIdsInGroup_Select_ECMTermSet_ECMGroup;
                List<int> termSetIds;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@GroupId", groupId);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
                using (var reader = mQueryWorker.ExecuteReader(commandText))
                {
                    termSetIds = GetIntValues(reader, 0);
                }

                termSets = GetTermSetsByIds(termSetIds, defaultPartitionId, defaultLanguage);
            });
            return termSets;
        }

        /// <summary>
        /// 通过TermSet Id 获取 term set的信息
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId, int defaultLanguage)
        {
            AveTermSetInfo termSetInfo = null;
            ExceptionHandlingScope(() =>
            {
                var ids = new List<int> {setId};
                termSetInfo = GetTermSetsByIds(ids, defaultPartitionId, defaultLanguage).FirstOrDefault();
            });
            return termSetInfo;
        }

        /// <summary>
        /// 通过TermSet UniqueId 获取 term set的信息
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId, int defaultLanguage)
        {
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", setId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            Dictionary<int, AveTermSetInfo> termSetDic = new Dictionary<int, AveTermSetInfo>();
            AveTermSetInfo termSetInfo = null;
            using (var reader = mQueryWorker.ExecuteReader(GetTermSet_Select_ECMTermSet))
            {
                if (reader.Read())
                {
                    termSetInfo = new AveTermSetInfo
                    {
                        Id = reader.GetGuid(6),
                        Name = reader.GetString(7),
                        PartitionId = defaultPartitionId
                    };
                    //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                    var tmpName = termSetInfo.Name;
                    var tmpLength = tmpName.IndexOf(defaultLanguage + "|", StringComparison.OrdinalIgnoreCase);
                    if (tmpLength >= 0)
                    {
                        tmpName = tmpName.Substring(tmpLength + defaultLanguage.ToString().Length + 1);
                        tmpLength = tmpName.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                        if (tmpLength >= 0)
                        {
                            tmpName = tmpName.Substring(0, tmpLength);
                        }
                    }
                    termSetInfo.Name = tmpName;
                    termSetInfo.Description = reader.GetString(8);
                    termSetInfo.Type = reader.GetByte(9);
                    termSetInfo.Owner = FindRealUserGroupName(reader.GetString(4));
                    if (!reader.IsDBNull(5))
                    {
                        termSetInfo.CustomSortOrder = reader.GetString(5);
                    }
                    termSetInfo.IsAvailableForTagging = reader.GetBoolean(11);
                    termSetInfo.IsOpenForTermCreation = reader.GetBoolean(10);
                    if (!reader.IsDBNull(13))
                    {
                        termSetInfo.Contact = reader.GetString(13);
                    }
                    if (!reader.IsDBNull(12))
                    {
                        var holders = reader.GetString(12);
                        if (!String.IsNullOrEmpty(holders))
                        {
                            termSetInfo.Stakeholders = new List<string>();
                            foreach (var h in holders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                termSetInfo.Stakeholders.Add(FindRealUserGroupName(h));
                            }
                        }
                    }
                    termSetDic.Add(reader.GetInt32(0), termSetInfo);
                }
            }
            SetTermSetsCustomProperty(termSetDic);
            return termSetInfo;
        }

        private void SetTermSetsCustomProperty(Dictionary<int, AveTermSetInfo> termSetDic)
        {
            foreach (var valuePair in termSetDic)
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", valuePair.Key);
                using (var reader = mQueryWorker.ExecuteReader(GetTermSetProperty_Select_ECMTermSet_ECMTermProperty))
                {
                    if (reader.HasRows)
                    {
                        valuePair.Value.CustomProperties = new Dictionary<string, string>();
                    }

                    while (reader.Read())
                    {
                        valuePair.Value.CustomProperties.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
        }

        /// <summary>
        /// 通过MetadataServiceApplication获取不到Term中的GetDefaultLabel方法,只能通过SQL实现
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " re-order the index")]
        public string GetTermDefaultLabel(int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            object label = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetTermDefaultLableByTermId_Select_ECMTermLabel;
                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@TermId", termId);
                mQueryWorker.AddParameter("@DefaultLanguage", defaultLanguage);
                mQueryWorker.AddParameter("@DefaultPartitionId", defaultPartitionId);
                label = mQueryWorker.ExecuteScalar(cmdText);
            });
            return label == null ? null : label.ToString();
        }

        /// <summary>
        /// 获取metadata service中所有partition 的setting信息
        /// </summary>
        /// <returns></returns>
        public List<ServiceSetting> GetPartitionServiceSettings()
        {
            var result = new List<ServiceSetting>();
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetMetadataServiceSettings_Select_ECMServiceSettings;
                mQueryWorker.Command.CommandType = CommandType.Text;
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result.Add(new ServiceSetting()
                            {
                                PartitionId = reader.GetGuid(0),
                                settingsXml = reader.GetString(1)
                            });
                        }
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 通过PartitionId从SharePoint_Config DB的SiteMappingVisible表查询
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId)
        {
            List<AveSiteMapVisible> result = new List<AveSiteMapVisible>();
            ExceptionHandlingScope(() =>
            {
                try
                {
                    mQueryWorker.AddParameter("@SubscriptionId", defaultPartitionId);
                    mQueryWorker.Command.CommandType = CommandType.Text;
                    var cmdText = GetTenantAdminSiteIdByPartitionId_Select_SiteMap;
                    using (var dReader = mQueryWorker.ExecuteReader(cmdText))
                    {
                        if (dReader.HasRows)
                        {
                            while (dReader.Read())
                            {
                                var managedSite = new AveSiteMapVisible
                                {
                                    SiteId = dReader.GetGuid(0),
                                    ApplicationId = dReader.GetGuid(1),
                                    DatabaseId = dReader.GetGuid(2),
                                    Path = dReader.GetString(3)
                                };
                                //从数据库中取出的是大头，这里需要反转
                                //也可以使用移位实现
                                var d = dReader.GetSqlBytes(4).Buffer.Reverse();
                                //BitConverter的Byte[]是小头
                                managedSite.Version = BitConverter.ToInt64(d.ToArray(), 0);
                                result.Add(managedSite);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred when call the method GetTenancyAdminSiteId,error:{0}", ex);
                }
            });
            return result;
        }

        #endregion

    }
}
