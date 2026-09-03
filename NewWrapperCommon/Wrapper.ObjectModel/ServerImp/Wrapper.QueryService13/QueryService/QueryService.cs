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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Linq;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    public class AveQueryServiceProvider
    {
        private static IAveQueryService m_Instance;
        private static object syncRoot;

        static AveQueryServiceProvider()
        {
            syncRoot = new object();
        }

        public static T Instance<T>(object arg) where T : IAveQueryService
        {
            lock (syncRoot)
            {
                m_Instance = AveQueryServiceFactory.CreateQueryService(arg);
                return (T)m_Instance;
            }
        }
    }

    internal class AveQueryServiceFactory
    {
        internal static IAveQueryService CreateQueryService(object arg)
        {
            if (WrapperRuntime.CurrentContext.Opimized)
            {
                var queryService = new AveQueryService();
                queryService.InitQuerySession(arg);
                return queryService;
            }
            else
            {
                return null;
            }
        }

        internal static IAveQuerySessionSchema CreateQuerySessionSchema(SPSVersion schema, AveQueryWorker queryWorker)
        {
            IAveQuerySessionSchema querySessionSchema = null;
            if (schema == SPSVersion.SP1)
            {
                querySessionSchema = new AveQuerySessionSchemaForSP1(queryWorker);
            }
            else
            {
                throw new ArgumentException();
            }
            return querySessionSchema;
        }
    }

    internal partial class AveQueryService : AveQueryServiceBase, IAveQueryService, IAveCommonQueryService
    {
        private const int DEFAULT_TIMEOUT = 180;
        private const int DocList = 1;
        protected IAveQuerySessionSchema mQuerySessionSchema;//RTM,SP1
        private SPSVersion SPSchema
        {
            [QueryReview("2012/05/17", "Long Liang")]
            get
            {
                return SPSVersion.SP1;
            }
        }
        internal AveQueryService()
        {
            mQueryWorker = new AveQueryWorker();
            mQuerySessionSchema = AveQueryServiceFactory.CreateQuerySessionSchema(this.SPSchema, mQueryWorker);
        }

        #region Private Methods

        #region MetadataServiceApplication

        private bool GetPermissions(AveTaxonomyRights rightsToMatch, bool exactMatch, AveTaxonomyRights aceRights)
        {
            bool flag = false;
            if (exactMatch && aceRights == rightsToMatch)
            {
                flag = true;
            }
            else if (!exactMatch && (aceRights & rightsToMatch) == rightsToMatch)
            {
                flag = true;
            }
            //if (flag)
            //{
            //    acl2.Add(ace.PrincipalName, aceRights, ace.DenyRightsMask);
            //}
            return flag;
        }

        private List<int> GetIds(SqlDataReader reader)
        {
            List<int> ids = new List<int>();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                if (!ids.Contains(id))
                {
                    ids.Add(id);
                }
            }
            return ids;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取Groups信息，有API实现，效率考虑
        /// </summary>
        /// <param name="groupIds"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        private List<AveMetadataGroupInfo> GetGroups(List<int> groupIds, Guid defaultPartitionId)
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
            command.Parameters["@GroupIdList"].Value = GetListString<int>(groupIds);
            command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
            command.Parameters["@PartitionId"].Value = defaultPartitionId; ;

            Dictionary<int, AveMetadataGroupInfo> groupDic = new Dictionary<int, AveMetadataGroupInfo>();

            using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetGroups"))
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

                        if (pName.StartsWith("c:0+.w|s", StringComparison.OrdinalIgnoreCase))
                        {
                            pName = pName.Substring(7);
                            pName = AveObjectModelFactory.CreateObjectModelFactory("", null).CreatePeopleEditor().GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(pName));
                        }
                        else if (pName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase) || pName.StartsWith("c:0-.f|", StringComparison.OrdinalIgnoreCase))
                        {
                            pName = pName.Substring(7);
                        }
                        else if (pName.StartsWith("SiteCollectionId:", StringComparison.OrdinalIgnoreCase))
                        {
                            //可查看Microsoft.SharePoint.Taxonomy.Internal.Security.DeserializePermission(IDataReader dataReader, SPAcl<TaxonomyRights> acl, List<Guid> siteCollectionIds)源代码
                            Guid item = new Guid(pName.Substring("SiteCollectionId:".Length));
                            group.Sites.Add(item);
                            continue;
                        }

                        if (GetPermissions(AveTaxonomyRights.GroupManager | AveTaxonomyRights.EditTerm | AveTaxonomyRights.AddTermSetEditPermissions | AveTaxonomyRights.EditGroup | AveTaxonomyRights.EditTermSet, false, (AveTaxonomyRights)mask))
                        {
                            if (group.GroupManagers == null)
                            {
                                group.GroupManagers = new List<AveAceInfo>();
                            }
                            group.GroupManagers.Add(new AveAceInfo() { DisplayName = pName, GrantRightsMask = mask, PrincipalName = pName, DenyRightsMask = (ulong)AveTaxonomyRights.None });
                        }
                        if (GetPermissions(AveTaxonomyRights.Contributor | AveTaxonomyRights.EditTerm | AveTaxonomyRights.EditTermSet, false, (AveTaxonomyRights)mask))
                        {
                            if (group.Contributors == null)
                            {
                                group.Contributors = new List<AveAceInfo>();
                            }
                            group.Contributors.Add(new AveAceInfo() { DisplayName = pName, GrantRightsMask = mask, PrincipalName = pName, DenyRightsMask = (ulong)AveTaxonomyRights.None });
                        }
                    }
                }
            }
            return groupList;
        }

        private string GetListString<T>(ICollection<T> list)
        {
            return GetListString<T>(list, '\\');
        }

        private string GetListString<T>(ICollection<T> list, char separator)
        {
            if (list == null || list.Count == 0) return null;
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (T local in list)
            {
                if (!flag) builder.Append(separator);
                flag = false;
                builder.Append(local.ToString());
            }
            return builder.ToString();
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 获取TermSetId，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <returns></returns>
        private int GetTermSetId(Guid termSetId, Guid partitionId)
        {
            int termsetId = -1;
            string commandText = @"SELECT TOP 1 es.Id FROM ECMTermSet es WITH(NOLOCK) WHERE es.PartitionId=@PartitionId and es.UniqueId=@UniqueId";
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

        /// <summary>
        /// 获取changeItem到Root Term的直线型tree结构
        /// </summary>
        /// <param name="item"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermChangeItem GetChangeItemTree(AveTermChangeItem item, Guid defaultPartitionId, int defaultLanguage)
        {
            string path = string.Empty;
            AveTermInfo termInfo = GetTerm(item.TermSetId.Value, item.ObjectId, defaultPartitionId, defaultLanguage);
            if (item.ChangeType == AveTermChangeItem.ChangedOperationType.Add)
            {
                if (termInfo.IsPinned)
                {
                    path = GetTermPath(defaultPartitionId, item.ObjectId, 0, GetTermSetId(item.TermSetId.Value, defaultPartitionId));
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
            bool isRoot = true;
            //若path为string.Empty的话，说明这个term的parent被删除了
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string[] itemObjectIds = path.Split('\\');
                    foreach (string id in itemObjectIds)
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        AveTermChangeItem changeItem = null;
                        int termObjectId = Convert.ToInt32(id);
                        if (item.ObjectId != termObjectId)
                        {
                            changeItem = new AveTermChangeItem();
                            changeItem.ChangeType = AveTermChangeItem.ChangedOperationType.FakeAsParent;
                            changeItem.ChangeTime = item.ChangeTime;
                            changeItem.ItemType = AveTermChangeItem.ChangedItemType.Term;
                            changeItem.ObjectId = termObjectId;
                            changeItem.TermSetId = item.TermSetId;
                            changeItem.GroupId = item.GroupId;
                            changeItem.ChangeData = string.Empty;
                            AveTermInfo info = GetTerm(item.TermSetId.Value, termObjectId, defaultPartitionId, defaultLanguage);
                            changeItem.Name = info.Name;
                            changeItem.Id = info.Id;
                        }
                        else
                        {
                            AveTermInfo info = GetTerm(item.TermSetId.Value, termObjectId, defaultPartitionId, defaultLanguage);
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
            }
            return root;
        }

        #region << 获取Term详细信息 >>
        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取Term详细信息,效率考虑，有API实现
        /// </summary>
        private List<AveTermInfo> GetTerms(List<int> termIds, int termSetId, bool isAllTersInSet, Guid defaultPartitionId, int defaultLanguage, Guid termSetUniqueId)
        {
            List<AveTermInfo> termList = new List<AveTermInfo>();
            if (!isAllTersInSet && (termIds == null || termIds.Count == 0))
            {
                return termList;
            }

            if (termIds == null)
            {
                return termList;
            }

            foreach (int id in termIds)
            {
                //Sql
                string cmdText = @"SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId
From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId AND etsm.TermSetId = ets.Id
left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id
WHERE ets.UniqueId = @TermSetUniqueId AND et.Id = @Id AND etsm.PartitionId = @PartitionId";

                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@TermSetUniqueId", termSetUniqueId);
                mQueryWorker.AddParameter("@Id", id);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                AveTermInfo termInfo = new AveTermInfo();
                int pinSourceTermSetId = 0;
                int parentTermId = 0;
                //1 Term Basic
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
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
                        termInfo.ParentTermSetId = termSetUniqueId;
                        if (!reader.IsDBNull(11))
                        {
                            termInfo.CustomSortOrder = reader.GetString(11);
                        }
                        if (!reader.IsDBNull(7))
                        {
                            termInfo.MergedTermIds = reader.GetString(7).Split('\\').Where(s => AveTypeHelper.IsGuid(s)).Select(s => new Guid(s)).ToList();
                        }
                    }
                }
                termInfo.IsReused = GetTermIsReusedProperty(id, defaultPartitionId);

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
                GetTermLabels(termInfo, id, defaultPartitionId, defaultLanguage);

                //3：Term Description
                GetTermDescription(termInfo, id, defaultPartitionId);

                //4: Get Property
                GetTermProperty(termInfo, id, termSetId, defaultPartitionId, defaultLanguage);

                termList.Add(termInfo);
            }
            return termList;
        }
        /// <summary>
        /// 获取Parent Term UniqueId
        /// </summary>
        private Guid GetParentTermUniqueId(int parentTermId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT et.UniqueId From ECMTermSetMembership etsm WITH(NOLOCK) left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id
WHERE etsm.TermId = @Id AND etsm.PartitionId = @PartitionId";
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
        /// <summary>
        /// 获取Term的Pin Source TermSet UniqueId
        /// </summary>
        private Guid GetTermPinSourceTermSetUniqueId(int pinSourceTermSetId, Guid defaultPartitionId)
        {
            Guid pinSourceTermSetUniqueId = Guid.Empty;
            string cmdText = @"select top 1 ts.UniqueId from ECMTermSet ts WITH(NOLOCK) left join ECMTermSetMembership tsms WITH(NOLOCK) on ts.PartitionId=tsms.PartitionId
where tsms.TermSetId = ts.Id and tsms.TermSetId = @TermSetId and tsms.PartitionId = @PartitionId";
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
        /// <summary>
        /// 获取Term Reused属性
        /// </summary>
        private bool GetTermIsReusedProperty(int termId, Guid defaultPartitionId)
        {
            string cmdText = @"Select COUNT(*) from ECMTerm et WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on et.PartitionId=etsm.PartitionId
where et.Id = @Id and etsm.TermId = @Id and etsm.PartitionId = @PartitionId";
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
        #endregion << 获取Term详细信息 >>


        private void GetTermProperty(AveTermInfo termInfo, int id, int termSetId, Guid defaultPartitionId, int defaultLanguage)
        {
            #region customProperty

            string cmdText = @"
select Property.PropertyName,Property.PropertyValue from dbo.ECMTerm as Term With(NOLOCK) 
inner join dbo.ECMTermProperty as Property With(NOLOCK) on Term.PartitionId = Property.PartitionId and Property.TermId = Term.Id and Property.TermSetId = 0 
where Term.Id = @ID";
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", id);

            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
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

            #endregion

            #region LocalProperty

            cmdText = @"
select Property.PropertyName,Property.PropertyValue from dbo.ECMTerm as Term With(NOLOCK) 
inner join dbo.ECMTermProperty as Property With(NOLOCK) on Term.PartitionId = Property.PartitionId and Property.TermId = Term.Id and Property.TermSetId = @TermSetId
where Term.Id = @ID";

            mQueryWorker.AddParameter("@TermSetId", termSetId);

            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
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

            #endregion
        }

        #region << 获取TermLabel >>
        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取TermLabel，效率考虑，有API实现
        /// </summary>
        private void GetTermLabels(AveTermInfo termInfo, int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            //Sql
            string cmdText = @"
Select etl.TermId, etl.LCID, etl.Label, etl.IsDefault  from ECMTermLabel etl WITH(NOLOCK) where etl.PartitionId =@PartitionId and etl.TermId=@Id";
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
                    AveLableInfo lInfo = new AveLableInfo();
                    lInfo.Language = reader.GetInt32(1);
                    lInfo.Value = reader.GetString(2);
                    lInfo.IsDefaultForLanguage = reader.GetBoolean(3);
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
        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取TermLabel,效率考虑，有API实现
        /// </summary>
        private void GetTermDescription(AveTermInfo termInfo, int termId, Guid defaultPartitionId)
        {
            //Sql
            string cmdText = @"
Select etd.TermId, etd.LCID, etd.Description from ECMTermDescription etd WITH(NOLOCK) where etd.PartitionId=@PartitionId and etd.TermId=@Id";
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
                    int lcid = reader.GetInt32(1);
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

        //private List<AveTermInfo> GetTerms(List<int> termIds, int termSetId, bool isAllTersInSet, Guid defaultPartitionId, int defaultLanguage)
        //{
        //    List<AveTermInfo> termList = new List<AveTermInfo>();
        //    if (!isAllTersInSet && (termIds == null || termIds.Count == 0))
        //    {
        //        return termList;
        //    }
        //    mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
        //    mQueryWorker.ClearParameters();
        //    mQueryWorker.Command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
        //    mQueryWorker.Command.Parameters["@PartitionId"].Value = defaultPartitionId;

        //    if (isAllTersInSet)
        //    {
        //        mQueryWorker.Command.Parameters.Add(new SqlParameter("@@IsAllTermsInTermset", SqlDbType.Bit));
        //        mQueryWorker.Command.Parameters["@@IsAllTermsInTermset"].Value = 1;
        //    }
        //    else
        //    {
        //        mQueryWorker.Command.Parameters.Add(new SqlParameter("@TermIdList", SqlDbType.VarChar, 0x7fffffff));
        //        mQueryWorker.Command.Parameters["@TermIdList"].Value = GetListString<int>(termIds);
        //    }

        //    using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetTerms"))
        //    {
        //        Dictionary<int, AveTermInfo> terms = new Dictionary<int, AveTermInfo>();
        //        #region Stored Procedure Fields
        //        // et.Id
        //        //,PartitionId
        //        //,CreatedTime
        //        //,LastModifiedTime
        //        //,Owner
        //        //,UniqueId
        //        //,LastUsedTime
        //        //,UseCount
        //        //,IsDeprecated
        //        //,IsDeleted
        //        //,MergedIdList
        //        #endregion
        //        while (reader.Read())
        //        {
        //            if (reader.FieldCount == 11)
        //            {
        //                AveTermInfo term = new AveTermInfo();
        //                term.Id = reader.GetGuid(5);
        //                term.Owner = reader.GetString(4);
        //                term.IsDeprecated = reader.GetBoolean(8);
        //                terms[reader.GetInt32(0)] = term;
        //                termList.Add(term);
        //            }
        //        }

        //        #region Stored Procedure Fields
        //        //TermId
        //        //,LCID
        //        //,Label
        //        //,IsDefault  
        //        #endregion
        //        if (reader.NextResult())
        //        {
        //            while (reader.Read())
        //            {
        //                int id = reader.GetInt32(0);
        //                AveTermInfo term = terms[id];
        //                if (term.Labels == null)
        //                {
        //                    term.Labels = new List<AveLableInfo>();
        //                }

        //                AveLableInfo lInfo = new AveLableInfo();

        //                lInfo.Language = reader.GetInt32(1);
        //                lInfo.Value = reader.GetString(2);
        //                lInfo.IsDefaultForLanguage = reader.GetBoolean(3);
        //                if (lInfo.IsDefaultForLanguage && lInfo.Language == defaultLanguage)
        //                {
        //                    term.Name = lInfo.Value;
        //                    term.TermName = lInfo.Value;
        //                }
        //                term.Labels.Add(lInfo);
        //            }
        //        }

        //        #region Stored Procedure Fields
        //        //TermId
        //        //,LCID
        //        //,Description  
        //        #endregion
        //        if (reader.NextResult())
        //        {
        //            while (reader.Read())
        //            {
        //                int id = reader.GetInt32(0);
        //                AveTermInfo term = terms[id];
        //                term.Description = reader.GetString(2);
        //                int lcid = reader.GetInt32(1);
        //                foreach (AveLableInfo l in term.Labels)
        //                {
        //                    if (l.Language == lcid)
        //                    {
        //                        l.Description = reader.GetString(2);
        //                        break;
        //                    }
        //                }
        //            }
        //        }

        //        #region Stored Procedure Fields
        //        //TermId
        //        //,PropertyName
        //        //,PropertyValue              
        //        #endregion
        //        if (reader.NextResult())
        //        {
        //            while (reader.Read())
        //            {
        //                int id = reader.GetInt32(0);
        //                AveTermInfo term = terms[id];

        //            }
        //        }

        //        #region Stored Procedure Fields
        //        //TermSetId
        //        //,TermId
        //        //,Path AS IdPath
        //        //,ParentTermId
        //        //,AvailableForTagging
        //        //,CustomSortOrder
        //        //,IsSource
        //        //,CASE tm.ParentTermId WHEN 0 THEN N''
        //        //    ELSE dbo.fn_ECM_GetPathInLabels(@PartitionId, tm.Path, @LCID, @DefaultLcid) 
        //        //    END AS Path
        //        //,CASE @IncludeIdPath WHEN 0 THEN NULL
        //        //    ELSE dbo.fn_ECM_GetPathInGuids(@PartitionId, tm.Path,tm.TermSetId) 
        //        //    END AS FullGuidPath
        //        //,pt.UniqueId AS ParentTermGuid
        //        //,ts.Name AS TermSetName
        //        //,ts.UniqueId AS TermSetGuid
        //        //,ts.Type
        //        #endregion
        //        if (reader.NextResult())
        //        {
        //            while (reader.Read())
        //            {
        //                int id = reader.GetInt32(1);
        //                AveTermInfo term = terms[id];
        //                if (termSetId != reader.GetInt32(0))
        //                {
        //                    continue;
        //                }

        //                int parentId = reader.GetInt32(3);
        //                term.IsRoot = reader.GetInt32(3) == 0;
        //                term.IsAvailableForTagging = reader.GetBoolean(4);
        //                if (reader.FieldCount > 12 && !reader.IsDBNull(12))
        //                {
        //                    term.IsKeyword = reader.GetInt32(12) == 1;
        //                }
        //                term.IsSourceTerm = reader.GetBoolean(6);
        //                if (!reader.IsDBNull(5))
        //                {
        //                    term.CustomSortOrder = reader.GetString(5);
        //                }
        //            }
        //        }

        //        #region Stored Procedure Fields
        //        // childTerm.ParentTermId AS ParentTermId
        //        //,childTerm.TermSetId 
        //        //,childTerm.TermId As ChildTermId
        //        //if (reader.NextResult())
        //        //{
        //        //    while (reader.Read())
        //        //    {
        //        //        int id = reader.GetInt32(0);
        //        //        AveTermInfo term = terms[id];
        //        //        int setId = termSetMap[id];
        //        //        if (setId != reader.GetInt32(1) && !term.IsSourceTerm)
        //        //        {
        //        //            term.SourceTermId = reader.GetGuid(2);
        //        //        }
        //        //    }
        //        //}
        //        #endregion
        //    }
        //    return termList;
        //}

        #region << 获取TermSet详细信息 >>
        [QueryReview("2012/05/17", "Long Liang", Changed = true, Comment = "Add batch query")]
        /// <summary>
        /// 获取TermSet详细信息,效率考虑，有API实现
        /// </summary>
        /// <param name="termSetIds"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private List<AveTermSetInfo> GetTermSets(List<int> termSetIds, Guid defaultPartitionId, int defaultLanguage)
        {
            List<AveTermSetInfo> termSetList = new List<AveTermSetInfo>();
            Dictionary<int, AveTermSetInfo> termSetDic = new Dictionary<int, AveTermSetInfo>();
            if (termSetIds == null || termSetIds.Count == 0)
            {
                return termSetList;
            }

            const string cmdText = @"Select et.Id, et.PartitionId, et.CreatedTime, et.LastModifiedTime, et.Owner, et.CustomSortOrder, et.UniqueId, et.Name, et.Description,
                               et.Type, et.IsOpen, et.AvailableForTagging, et.Stakeholders, et.Contact, et.GroupId from ECMTermSet et WITH(NOLOCK) where 
                               et.Id in ";
            int count = termSetIds.Count;

            for (int index = 0; index < count;)
            //foreach (int setId in termSetIds)
            {
                #region Get batch query collection
                int remain = count - index;
                if (remain > 100)
                {
                    remain = 100;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("( ");
                for (int offset = 0; offset < remain; offset++)
                {
                    builder.Append(termSetIds[offset + index]);
                    if (offset == remain - 1)
                    {
                        builder.Append(" ");
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                }
                builder.Append(" )");
                index += remain;
                #endregion

                mQueryWorker.Command.CommandType = CommandType.Text;
                //mQueryWorker.ClearParameters();
                //mQueryWorker.AddParameter("@SetId", setId);
                //mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText + builder.ToString()))
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        AveTermSetInfo termSetInfo = new AveTermSetInfo();
                        termSetInfo.Id = reader.GetGuid(6);
                        termSetInfo.Name = reader.GetString(7);
                        termSetInfo.PartitionId = defaultPartitionId;
                        //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                        int tmpLength = 0;
                        string tmpName = termSetInfo.Name;
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
                            string holders = reader.GetString(12);
                            if (!String.IsNullOrEmpty(holders))
                            {
                                termSetInfo.Stakeholders = new List<string>();
                                foreach (string h in holders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    termSetInfo.Stakeholders.Add(FindRealUserGroupName(h));
                                }
                            }
                        }
                        termSetDic.Add(id, termSetInfo);
                        termSetList.Add(termSetInfo);
                    }
                }

                //for 13 read Custom Property
                const string propertyCmdText = @"
select Property.PropertyName,Property.PropertyValue from dbo.ECMTermSet as TermSet With(NOLOCK) 
inner join dbo.ECMTermProperty as Property With(NOLOCK) on TermSet.PartitionId = Property.PartitionId and Property.TermId = 0 and TermSet.Id = Property.TermSetId 
where TermSet.Id = @ID";

                foreach (var valuePair in termSetDic)
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@ID", valuePair.Key);
                    using (var reader = mQueryWorker.ExecuteReader(propertyCmdText))
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
            return termSetList;
        }
        #endregion << 获取TermSet详细信息 >>

        #region << 替换UserGroup数据库存储的模式到正常模式 >>
        /// <summary>
        /// 替换数据库存储的模式到正常模式
        /// </summary>
        /// <param name="name">数据库模式</param>
        /// <returns>正常模式</returns>
        private string FindRealUserGroupName(string name)
        {
            string retName = name;
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

        //private List<AveTermSetInfo> GetTermSets(List<int> termSetIds, Guid defaultPartitionId, int defaultLanguage)
        //{
        //    List<AveTermSetInfo> termSetList = new List<AveTermSetInfo>();
        //    if (termSetIds == null || termSetIds.Count == 0)
        //    {
        //        return termSetList;
        //    }
        //    mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
        //    mQueryWorker.ClearParameters();
        //    mQueryWorker.Command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
        //    mQueryWorker.Command.Parameters["@PartitionId"].Value = defaultPartitionId;
        //    mQueryWorker.Command.Parameters.Add(new SqlParameter("@TermSetIdList", SqlDbType.VarChar, 0x7fffffff));
        //    mQueryWorker.Command.Parameters["@TermSetIdList"].Value = GetListString<int>(termSetIds);

        //    using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetTermSets"))
        //    {
        //        #region Stored Procedure Fields
        //        //ts.Id
        //        //,PartitionId
        //        //,CreatedTime
        //        //,LastModifiedTime
        //        //,Owner
        //        //,CustomSortOrder
        //        //,UniqueId
        //        //,Name
        //        //,Description
        //        //,Type
        //        //,IsOpen
        //        //,AvailableForTagging
        //        //,Stakeholders
        //        //,Contact
        //        //,GroupId
        //        #endregion
        //        Dictionary<int, AveTermSetInfo> setDic = new Dictionary<int, AveTermSetInfo>();

        //        while (reader.Read())
        //        {
        //            int id = reader.GetInt32(0);
        //            AveTermSetInfo termSetInfo = new AveTermSetInfo();
        //            termSetInfo.Id = reader.GetGuid(6);
        //            termSetInfo.Name = reader.GetString(7);
        //            //if (termSetInfo.Name.StartsWith(this.DefaultLanguage + "|", StringComparison.OrdinalIgnoreCase))
        //            //{
        //            //    termSetInfo.Name = termSetInfo.Name.Substring(termSetInfo.Name.IndexOf("|") + 1);
        //            //}
        //            //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
        //            int tmpLength = 0;
        //            string tmpName = termSetInfo.Name;
        //            tmpLength = tmpName.IndexOf(defaultLanguage + "|", StringComparison.OrdinalIgnoreCase);
        //            if (tmpLength >= 0)
        //            {
        //                tmpName = tmpName.Substring(tmpLength + defaultLanguage.ToString().Length + 1);
        //                tmpLength = tmpName.IndexOf(";", StringComparison.OrdinalIgnoreCase);
        //                if (tmpLength >= 0)
        //                {
        //                    tmpName = tmpName.Substring(0, tmpLength);
        //                }
        //            }
        //            termSetInfo.Name = tmpName;
        //            termSetInfo.Description = reader.GetString(8);
        //            termSetInfo.Type = reader.GetByte(9);
        //            termSetInfo.Owner = reader.GetString(4);
        //            if (!reader.IsDBNull(5))
        //            {
        //                termSetInfo.CustomSortOrder = reader.GetString(5);
        //            }
        //            termSetInfo.IsAvailableForTagging = reader.GetBoolean(11);
        //            termSetInfo.IsOpenForTermCreation = reader.GetBoolean(10);
        //            if (!reader.IsDBNull(13))
        //            {
        //                termSetInfo.Contact = reader.GetString(13);
        //            }
        //            if (!reader.IsDBNull(12))
        //            {
        //                string holders = reader.GetString(12);
        //                if (!String.IsNullOrEmpty(holders))
        //                {
        //                    termSetInfo.Stakeholders = new List<string>();
        //                    foreach (string h in holders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        //                    {
        //                        termSetInfo.Stakeholders.Add(h);
        //                    }
        //                }
        //            }
        //            termSetList.Add(termSetInfo);
        //            setDic[id] = termSetInfo;
        //        }
        //    }

        //    return termSetList;
        //}

        #endregion

        #region RBSUtility
        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 更新Pool中的信息，无API实现
        /// </summary>
        /// <param name="poolId"></param>
        /// <param name="canStoreNewBlobs"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        private void CreatePool(byte[] poolId, bool canStoreNewBlobs, short blobStoreId, int collectionId)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_sp_add_pool]";
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", poolId);
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@client_version", 0);
                    cmd.Parameters.Add("@pool_id", SqlDbType.Int);
                    cmd.Parameters["@pool_id"].Direction = ParameterDirection.Output;
                    object x = cmd.ExecuteScalar();
                    int poolIndex = (int)cmd.Parameters["@pool_id"].Value;

                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@BlobStoreId", blobStoreId);
                    mQueryWorker.AddParameter("@StorePoolId", poolId);
                    mQueryWorker.AddParameter("@PoolId", poolIndex);
                    mQueryWorker.AddParameter("@CanStoreNewBlobs", canStoreNewBlobs);
                    mQueryWorker.AddParameter("@CloseTime", DateTime.Now);
                    string commandText = @"UPDATE [mssqlrbs_resources].[rbs_internal_pools] 
SET [can_store_new_blobs]=@CanStoreNewBlobs,[close_time]=@CloseTime 
WHERE [blob_store_id]=@BlobStoreId AND [store_pool_id]=@StorePoolId AND [pool_id]=@PoolId";
                    mQueryWorker.ExecuteNonQuery(commandText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 向数据库中写入Blob信息，无API实现
        /// </summary>
        /// <param name="stubinfo"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        private long WriteBlobInformationToDB(AveRBSStubInfo stubinfo, short blobStoreId, int collectionId)
        {
            long blobNum = -1;
            long blobSize = stubinfo.DataLength;
            if (blobSize == 0)
                return -1;

            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_sp_register_blob]";
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", stubinfo.StorePoolId);
                    cmd.Parameters.AddWithValue("@store_blob_id", stubinfo.StoreBlobId);
                    cmd.Parameters.AddWithValue("@create_time", DateTime.Now.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@length", blobSize);
                    cmd.Parameters.AddWithValue("@client_version", 0);

                    cmd.Parameters.Add("@blob_number", SqlDbType.BigInt);
                    cmd.Parameters["@blob_number"].Direction = ParameterDirection.Output;

                    object x = cmd.ExecuteScalar();
                    blobNum = (long)cmd.Parameters["@blob_number"].Value;
                }
            }
            catch (SqlException ex)
            {
                //由于可能在插入STUB的过程中破坏mssqlrbs_resources.rbs_internal_blobs的unique index 'rbs_internal_blobs_ix_orphan_scan'，因此，如果出现这样的错误
                //我们应该获取已存在的这条STUB的Blob_Number并利用它生成一个RbsId返回给调用者，这样，将会出现有两个或者多个DocStreams中的记录拥有同一个
                //RBS Stub的情况，也就是有多个DocStreams中的记录有着相同的RbsId。
                if (ex.ToString().Contains(@"Cannot insert duplicate key row in object 'mssqlrbs_resources.rbs_internal_blobs' with unique index 'rbs_internal_blobs_ix_orphan_scan'.") || ex.Number == 50000)
                {
                    return GetBlobNumber(stubinfo, blobStoreId);
                }
                else
                    throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return blobNum;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取Blob Number，无API实现
        /// </summary>
        /// <param name="stubInfo"></param>
        /// <param name="blobStoreId"></param>
        /// <returns></returns>
        private long GetBlobNumber(AveRBSStubInfo stubInfo, short blobStoreId)
        {
            long blobNum = -1;
            string cmdStr = @"SELECT blob_number FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH(NOLOCK)
WHERE blob_store_id=@blob_store_id AND store_pool_id=@store_pool_id AND store_blob_id=@store_blob_id";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@blob_store_id", blobStoreId);
                mQueryWorker.AddParameter("@store_pool_id", stubInfo.StorePoolId);
                mQueryWorker.AddParameter("@store_blob_id", stubInfo.StoreBlobId);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdStr))
                {
                    if (dr.Read())
                        blobNum = dr.GetInt64(0);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex).ToString());
            }
            catch (AveQueryException queryException)
            {
                Console.WriteLine(queryException.ToString());
            }
            catch (Exception e)
            {//log here
                Console.WriteLine(new AveQueryException(e.Message, e).ToString());
            }
            return blobNum;
        }

        /// <summary>
        /// 生成Blob Number，无API实现
        /// </summary>
        /// <param name="rbs_id"></param>
        /// <returns></returns>
        private long GenerateBlobNumber(byte[] rbs_id)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_number]";
                    cmd.Parameters.AddWithValue("@blob_id", rbs_id);
                    cmd.Parameters.Add(new SqlParameter("@blob_num", SqlDbType.BigInt));
                    cmd.Parameters["@blob_num"].Direction = ParameterDirection.ReturnValue;

                    object x = cmd.ExecuteScalar();
                    return (long)(cmd.Parameters["@blob_num"].Value);
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 通过BlobNumber生成RbsId的函数,无API实现
        /// </summary>
        /// <param name="blob_num">需要转换的BlobNumber</param>
        /// <returns>生成的RbsId</returns>
        private byte[] GenerateRbsId(long blob_num, int collectionId)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_id]";
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_number", blob_num);
                    cmd.Parameters.Add("@blob_id", SqlDbType.VarBinary, 64);
                    cmd.Parameters["@blob_id"].Direction = ParameterDirection.ReturnValue;

                    object x = cmd.ExecuteScalar();
                    return (byte[])cmd.Parameters["@blob_id"].Value;
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        #endregion

        #region UtilityProcess

        private static string SpecialCharacters(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                str = str.Replace("%", "[%]");
                str = str.Replace("_", "[_]");
                return str;
            }
            return str;
        }

        private string GetUserInfoFilter(string userSearchInfo)
        {
            return "(tp_Title = @displayName) or (tp_Login = @loginName or tp_Login like '%|'+@fuzzyName) or (tp_Email = @emailAddress)";
        }

        // 在SQL查询语句的like语句中，如果匹配的name含有特定字符'%'和'_'，则需要转为'[%]'和'[_]'，以区别于通配符。
        // 但是在使用'='时，则不能进行转化。否则会导致匹配不出user/group。
        // fuzzyName作为isExact = true时，查找 like '%|fuzzyName'时使用，匹配claim user。所以需要转化字符。
        [QueryReview("2012/12/11", "Austin Han", true, "Use union all instead of union to improve the performance")]
        private SqlCommand GetNativeCommand(string userSearchInfo, bool isExact, AveAccountSearchFlag mFlag, string siteId)
        {
            SqlCommand command = null;
            string groupFilter = string.Empty, userInfoFilter = string.Empty;
            string fuzzyName = string.Empty;
            //if (!isExact)
            //{
            userSearchInfo = SpecialCharacters(userSearchInfo);
            groupFilter = "(Title like '%'+@displayName+'%')";
            userInfoFilter = "(tp_Title like '%'+@displayName+'%') or (tp_Login like '%'+@loginName+'%') or (tp_Email like '%'+@emailAddress+'%')";
            //}
            //else
            //{
            //    fuzzyName = SpecialCharacters(userSearchInfo);
            //    groupFilter = "(Title = @displayName)";
            //    userInfoFilter = GetUserInfoFilter(userSearchInfo);
            //}
            StringBuilder sqlText = new StringBuilder();
            if ((mFlag & AveAccountSearchFlag.IncludeSharePointGroup) != AveAccountSearchFlag.None)
            {
                sqlText.AppendFormat("select distinct top 201 title as loginName, title as displayName, '' as eMail, '2' as type from groups with (nolock) where (@siteId is null or SiteId = @siteId) and ({0})", groupFilter);
            }
            if ((mFlag & AveAccountSearchFlag.IncludeSharePointUser) != AveAccountSearchFlag.None)
            {
                if (sqlText.Length != 0)
                {
                    sqlText.Append(" union all ");//Add union all to improve performance. Added by Austin Han
                }
                sqlText.AppendFormat("select distinct top 201 tp_login as loginName, tp_title as displayName, tp_Email as eMail, (case when tp_DomainGroup=1 then '1' else '0' end) as type from UserInfo with (nolock) where (@siteId is null or tp_SiteID = @siteId) and tp_Deleted=0 and (tp_IsActive=1 or tp_Login='SHAREPOINT\\system') and ({0})", userInfoFilter);
            }
            sqlText.Append(" order by loginName");
            command = mQueryWorker.Connection.CreateCommand();
            command.CommandText = sqlText.ToString();
            command.Parameters.AddWithValue("@displayName", userSearchInfo);
            command.Parameters.AddWithValue("@loginName", userSearchInfo);
            command.Parameters.AddWithValue("@emailAddress", userSearchInfo);
            command.Parameters.AddWithValue("@fuzzyName", fuzzyName);
            command.Parameters.Add("@siteId", SqlDbType.UniqueIdentifier).Value = (!string.IsNullOrEmpty(siteId)) ? ((object)new Guid(siteId)) : ((object)DBNull.Value);
            return command;
        }

        #endregion

        #endregion

        #region MetadataServiceApplication

        /// <summary>
        /// 获取MMS的default Language，无API实现
        /// 同时返回AveTermStoreInfo信息
        /// </summary>
        /// <param name="termStoreInfo"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public int GetLanguage(ref AveTermStoreInfo termStoreInfo, Guid defaultPartitionId)
        {
            termStoreInfo = new AveTermStoreInfo();
            int defaultLanguage = 0;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
                mQueryWorker.Command.Parameters["@PartitionId"].Value = defaultPartitionId;
                mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetSessionData"))
                {
                    while (reader.Read())
                    {
                        if (reader.GetBoolean(2))
                        {
                            defaultLanguage = reader.GetInt32(1);
                            break;
                        }
                    }
                    if (reader.NextResult())
                    { }

                    //PrincipalName
                    //,Rights
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            string pName = reader.GetString(0);
                            ulong mask = (ulong)reader.GetInt64(1);
                            if (pName.StartsWith("c:0+.w|s", StringComparison.OrdinalIgnoreCase))
                            {
                                pName = pName.Substring(7);
                                pName = AveObjectModelFactory.CreateObjectModelFactory("", null).CreatePeopleEditor().GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(pName));
                            }
                            else if (pName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase) || pName.StartsWith("c:0-.f|", StringComparison.OrdinalIgnoreCase))
                            {
                                pName = pName.Substring(7);
                            }

                            if (GetPermissions(AveTaxonomyRights.ManageTermStore | AveTaxonomyRights.TermStoreAdministrator, false, (AveTaxonomyRights)mask))
                            {
                                termStoreInfo.TermStoreAdministrators.Add(new AveAceInfo() { DisplayName = pName, GrantRightsMask = mask, PrincipalName = pName, DenyRightsMask = (ulong)AveTaxonomyRights.None });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetLanguageError, ex);
            }
            return defaultLanguage;
        }

        /// <summary>
        /// ??MMS?default Language,?API??
        /// ????AveTermStoreInfo??
        /// </summary>
        /// <param name="termStoreInfo"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public int GetLanguage(Guid defaultPartitionId)
        {
            AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
            int defaultLanguage = 0;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
                mQueryWorker.Command.Parameters["@PartitionId"].Value = defaultPartitionId;
                mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetSessionData"))
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
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetLanguageError, ex);
            }
            return defaultLanguage;
        }

        public AveTermStoreInfo GetTermStoreInfo(Guid defaultPartitionId)
        {
            AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
                mQueryWorker.Command.Parameters["@PartitionId"].Value = defaultPartitionId;
                string commandText = @"SELECT PrincipalName,Rights FROM ECMPermission WITH(NOLOCK)
                                  WHERE
                                      PartitionId = @PartitionId and
                                      GroupId = 0";
                mQueryWorker.Command.CommandType = CommandType.Text;
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
                {
                    while (reader.Read())
                    {
                        string pName = reader.GetString(0);
                        ulong mask = (ulong)reader.GetInt64(1);
                        if (pName.StartsWith("c:0+.w|s", StringComparison.OrdinalIgnoreCase))
                        {
                            pName = pName.Substring(7);
                            pName = AveObjectModelFactory.CreateObjectModelFactory("", null).CreatePeopleEditor().GetAccountFromSid(AveDirectoryServiceUtility.ConvertStringSidToBytes(pName));
                        }
                        else if (pName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase) || pName.StartsWith("c:0-.f|", StringComparison.OrdinalIgnoreCase))
                        {
                            pName = pName.Substring(7);
                        }
                        if (GetPermissions(AveTaxonomyRights.ManageTermStore | AveTaxonomyRights.TermStoreAdministrator, false, (AveTaxonomyRights)mask))
                        {
                            termStoreInfo.TermStoreAdministrators.Add(new AveAceInfo() { DisplayName = pName, GrantRightsMask = mask, PrincipalName = pName, DenyRightsMask = (ulong)AveTaxonomyRights.None });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while getting MMS Term Store Info.Error Message{0}", ex);
            }
            return termStoreInfo;
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 获取Global Groups,效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId)
        {
            string command = @"SELECT 
       [Id]
  FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and ( [Type]=0 or [Type]=1)";
            List<int> ids = null;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                ids = GetIds(reader);
            }
            return GetGroups(ids, defaultPartitionId);
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 按照GroupId（Guid）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId)
        {
            string command = @"SELECT 
       [Id]
  FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and UniqueId=@UniqueId";
            List<int> ids = null;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", groupId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                ids = GetIds(reader);
            }
            return GetGroups(ids, defaultPartitionId)[0];
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 按照GroupName获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId)
        {
            string command = @"SELECT 
       [Id]
  FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and Name=@Name";
            List<int> ids = null;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Name", groupName);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                ids = GetIds(reader);
            }
            return GetGroups(ids, defaultPartitionId)[0];
        }

        /// <summary>
        /// 按照GroupId（Int）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId)
        {
            List<int> ids = new List<int>();
            ids.Add(groupId);

            List<AveMetadataGroupInfo> groups = GetGroups(ids, defaultPartitionId);
            if (groups != null && groups.Count > 0)
            {
                return GetGroups(ids, defaultPartitionId)[0];
            }
            return null;
        }

        /// <summary>
        /// 获取Local Groups，效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        [QueryReview("2012/04/26", "Qianwen Hu")]
        public List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId)
        {
            string command = @"SELECT 
       [Id]
      ,[PartitionId]
      ,[UniqueId]
      ,[Name]
      ,[Description]
      ,[LastModifiedTime]
      ,[CreatedTime]
      ,[Type]
  FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=2";
            List<int> ids = new List<int>();
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                ids = GetIds(reader);
            }
            return GetGroups(ids, defaultPartitionId);
        }
        /// <summary>
        /// 按照Id获取Group下的TermSet信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="partitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [QueryReview("2012/05/17", "Long Liang", true, " use index as join key and please add PartitionId in feature")]
        public List<AveTermSetInfo> GetTermSetIds(Guid groupId, Guid partitionId, int defaultLanguage)
        {
            //SELECT ets.UniqueId,ets.Name,ets.Type FROM ECMTermSet ets WITH(NOLOCK) INNER JOIN ECMGroup eg WITH(NOLOCK) ON  eg.UniqueId=@GroupId WHERE eg.Id=ets.GroupId";
            string commandText = @"SELECT ets.UniqueId,ets.Name,ets.Type 
FROM ECMTermSet ets WITH(NOLOCK) 
INNER JOIN ECMGroup eg WITH(NOLOCK) 
ON  ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId 
where eg.UniqueId=@GroupId and eg.PartitionId=@PartitionId"; //and eg.PartitionId=@PartitionId";
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@GroupId", groupId);
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            List<AveTermSetInfo> setInfo = new List<AveTermSetInfo>(); ;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
            {
                while (reader.Read())
                {
                    AveTermSetInfo info = new AveTermSetInfo();
                    Guid id = reader.GetGuid(0);
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
                    info.Id = id;
                    info.Name = name;
                    info.Type = reader.GetByte(2);
                    setInfo.Add(info);

                }
            }
            return setInfo;
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
            List<int> ids = new List<int>();
            ids.Add(termId);
            return GetTerms(ids, GetTermSetId(termSetId, defaultPartitionId), false, defaultPartitionId, defaultLanguage, termSetId)[0];
        }

        [QueryReview("2012/05/17", "Long Liang", true, "use index as query")]
        /// <summary>
        /// 按照Guid获取指定TermSet下指定的Terms信息（多值），效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId, int defaultLanguage)
        {
            //string cmdText = @"SELECT etsm.TermId FROm ECMTermSetMembership etsm WITH(NOLOCK), ECMTerm et WITH(NOLOCK) WHERE et.UniqueId=@TermId AND et.Id=etsm.ParentTermId AND etsm.PartitionId=@PartitionId";
            var intTermSetId = GetTermSetId(termSetId, defaultPartitionId);
            const string cmdText = @"
SELECT etsm.TermId 
From ECMTermSetMembership etsm WITH(NOLOCK), 
ECMTerm et WITH(NOLOCK) 
WHERE et.UniqueId=@TermId
AND et.PartitionId=@PartitionId
AND etsm.PartitionId=@PartitionId
AND etsm.ParentTermId=et.Id AND etsm.TermSetId = @TermSetId";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermId", termId);
            mQueryWorker.AddParameter("@TermSetId", intTermSetId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            List<int> ids = null;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                ids = GetIds(reader);
            }

            return GetTerms(ids, intTermSetId, false, defaultPartitionId, defaultLanguage, termSetId);
        }

        [QueryReview("2012/05/17", "Long Liang", true, "use index as query")]
        /// <summary>
        /// 按照Guid获取termSet下的全部Terms信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId, int defaultLanguage)
        {
            //string cmdText = @"SELECT etsm.TermId FROm ECMTermSetMembership etsm WITH(NOLOCK), ECMTermSet ets WITH(NOLOCK) WHERE ets.UniqueId=@SetId AND ets.Id=etsm.TermSetId AND etsm.ParentTermId=0 AND etsm.PartitionId=@PartitionId";
            const string cmdText = @"
SELECT etsm.TermId 
From ECMTermSetMembership etsm WITH(NOLOCK), 
ECMTermSet ets WITH(NOLOCK) 
WHERE ets.PartitionId = @PartitionId AND
ets.UniqueId=@SetId AND 
etsm.PartitionId=@PartitionId AND
etsm.TermSetId=ets.Id AND 
etsm.ParentTermId=0 ";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SetId", termSetId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            List<int> ids = null;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                ids = GetIds(reader);
            }

            return GetTerms(ids, GetTermSetId(termSetId, defaultPartitionId), false, defaultPartitionId, defaultLanguage, termSetId);
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 按照Guid获取多TermSet下的TermIds，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<Guid> GetTermIds(Guid termSetId, Guid defaultPartitionId)
        {
            int set = GetTermSetId(termSetId, defaultPartitionId);
            //string cmdText = @"SELECT DISTINCT et.UniqueId FROM ECMTermSetMembership etsm WITH(NOLOCK) INNER JOIN ECMTerm et WITH(NOLOCK) ON et.Id=etsm.TermId WHERE etsm.TermSetId=@ID";
            const string cmdText = @"
SELECT DISTINCT et.UniqueId FROM ECMTermSetMembership etsm WITH(NOLOCK) 
INNER JOIN ECMTerm et WITH(NOLOCK) 
ON et.PartitionId=etsm.PartitionId and et.Id=etsm.TermId 
WHERE etsm.PartitionId=@PartitionId and etsm.TermSetId=@ID";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ID", set);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            List<Guid> ids = new List<Guid>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    Guid id = reader.GetGuid(0);
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                }
            }

            return ids;
        }

        [QueryReview("2012/05/17", "Long Liang", true, "re-order the index")]
        /// <summary>
        /// 按照Guid获取特定Term下的TermIds，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<Guid> GetTermIds(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {
            int id = GetTermId(termId, defaultPartitionId);
            int setId = GetTermSetId(termSetId, defaultPartitionId);
            //string cmdText = @"SELECT et.UniqueId FROM ECMTermSetMembership etsm WITH(NOLOCK), ECMTerm et WITH(NOLOCK) WHERE etsm.TermSetId=@TermSetId AND etsm.ParentTermId=@TermId AND et.Id=etsm.TermId AND etsm.PartitionId=@PartitionId";
            const string cmdText = @"
SELECT et.UniqueId 
FROM ECMTermSetMembership etsm WITH(NOLOCK), 
ECMTerm et WITH(NOLOCK) 
WHERE etsm.PartitionId=@PartitionId 
AND etsm.TermSetId=@TermSetId 
AND etsm.ParentTermId=@TermId 
AND et.Id=etsm.TermId 
";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermId", id);
            mQueryWorker.AddParameter("@TermSetId", setId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            List<Guid> ids = new List<Guid>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    Guid term = reader.GetGuid(0);
                    if (!ids.Contains(term))
                    {
                        ids.Add(term);
                    }
                }
            }
            return ids;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 判断是否是SiteCollection下的Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId)
        {
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            mQueryWorker.AddParameter("@GroupId", groupId);

            try
            {
                string txt = "SELECT Type FROM ECMGroup WITH(NOLOCK) WHERE PartitionId = @PartitionId AND UniqueId=@UniqueId";
                int type = (int)mQueryWorker.ExecuteScalar(txt);
                if (type == 2)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.IsSiteCollectionGroupError, ex);
            }
            return false;
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 通过GroupId获取SiteCollectionId，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<Guid> GetSiteCollectionIdList(Guid groupId, Guid defaultPartitionId)
        {
            //string txt = "SELECT ep.PrincipalName FROM ECMPermission ep WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) WHERE eg.UniqueId=@UniqueId AND ep.GroupId=eg.Id AND eg.PartitionId=@PartitionId";
            const string txt = @"
SELECT ep.PrincipalName 
FROM ECMPermission ep WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) 
WHERE eg.PartitionId=@PartitionId 
AND eg.UniqueId=@UniqueId 
AND ep.PartitionId = @PartitionId
AND ep.GroupId=eg.Id 
";
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            mQueryWorker.AddParameter("@GroupId", groupId);
            List<Guid> siteList = new List<Guid>();

            try
            {
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(txt))
                {
                    while (reader.Read())
                    {
                        string pName = reader.GetString(0);
                        if (pName.StartsWith("SiteCollectionId:", StringComparison.OrdinalIgnoreCase))
                        {
                            //可查看Microsoft.SharePoint.Taxonomy.Internal.Security.DeserializePermission(IDataReader dataReader, SPAcl<TaxonomyRights> acl, List<Guid> siteCollectionIds)源代码
                            Guid item = new Guid(pName.Substring("SiteCollectionId:".Length));
                            siteList.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetSiteCollectionIdError, ex);
            }

            return siteList;
        }

        /// <summary>
        /// 获取特定Group+TermSet下的Changes为Incremental处理，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="changedItemType"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChanges(Nullable<int> groupId, Nullable<int> termSetId, DateTime sinceTime, Nullable<int> changedItemType, Guid defaultPartitionId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
            SqlCommand command = mQueryWorker.Command;
            command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
            command.Parameters["@PartitionId"].Value = defaultPartitionId;

            command.Parameters.Add(new SqlParameter("@SinceTime", SqlDbType.DateTime));
            command.Parameters["@SinceTime"].Value = sinceTime;
            if (groupId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@GroupId", SqlDbType.Int));
                command.Parameters["@GroupId"].Value = groupId.Value;
            }
            if (termSetId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@TermSetId", SqlDbType.Int));
                command.Parameters["@TermSetId"].Value = termSetId.Value;
            }
            if (changedItemType.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@ChangedItemType", SqlDbType.Int));
                command.Parameters["@ChangedItemType"].Value = (int)changedItemType.Value;
            }

            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetChanges"))
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

                //List<int> ids = new List<int>();
                while (reader.Read())
                {
                    AveTermChangeItem item = new AveTermChangeItem();

                    if (!reader.IsDBNull(4))
                    {
                        int obj = reader.GetInt32(4);
                        //if (ids.Contains(obj))
                        //{
                        //    continue;
                        //}
                        //ids.Add(obj);

                        item.ObjectId = obj;
                    }

                    if (!reader.IsDBNull(3))
                    {
                        item.Id = reader.GetGuid(3);
                    }

                    //item.ItemId = reader.GetInt32(4);

                    item.ItemType = (AveTermChangeItem.ChangedItemType)reader.GetInt32(5);
                    item.ChangeType = (AveTermChangeItem.ChangedOperationType)reader.GetInt32(6);
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
            return items;
        }

        /// <summary>
        /// 获取对应term的Path，为了得到term到root term的tree结构
        /// </summary>
        /// <param name="partitionId"></param>
        /// <param name="termIntId"></param>
        /// <returns></returns>
        public string GetTermPath(Guid partitionId, int termIntId, int isSource = 1, int termSetId = 0)
        {
            string path = string.Empty;
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
            return path;
        }

        private string GetChangeItemPath(AveTermChangeItem item, AveTermInfo info, Guid defaultPartitionId)
        {
            string path = item.Path;
            if (string.IsNullOrEmpty(path))
            {
                if (item.ChangeType == AveTermChangeItem.ChangedOperationType.Delete)
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(item.ChangeData);
                        XmlElement ptId = (XmlElement)doc.GetElementsByTagName("PTId")[0];
                        int pTermObjectId = Convert.ToInt32(ptId.InnerXml);
                        if (pTermObjectId != 0)
                        {
                            path = GetTermPath(defaultPartitionId, pTermObjectId, info.IsSourceTerm ? 1 : 0, GetTermSetId(item.TermSetId.Value, defaultPartitionId)) + "\\" + item.ObjectId;
                        }
                        else
                        {
                            path = item.ObjectId.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while getting term parent objectId. Term UniqueId:{0}, Error:{1}", item.Id, ex.ToString());
                    }
                }
                else
                {
                    path = GetTermPath(defaultPartitionId, item.ObjectId, info.IsSourceTerm ? 1 : 0, GetTermSetId(item.TermSetId.Value, defaultPartitionId));

                }
            }
            return path;
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 获取term上的Changes为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of sql statement. ")]
        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            if (sinceTime.HasValue)
            {
                //string cmdText = @"SELECT TOP 1 et.Id FROM ECMTerm et WITH(NOLOCK) WHERE et.UniqueId=@TermId";
                string cmdText = @"
SELECT TOP 1 et.Id FROM ECMTerm et WITH(NOLOCK) WHERE et.PartitionId=@PartitionId And et.UniqueId=@TermId";

                mQueryWorker.Command.CommandType = CommandType.Text;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@TermId", termId);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                int termIntId = (int)mQueryWorker.ExecuteScalar(cmdText);

                //                cmdText = @"declare @SetId int
                //
                //set @SetId = ( SELECT ID FROM ECMTermSet WITH(NOLOCK) WHERE UniqueId=@UniqueId)
                //
                //SELECT  GroupUniqueId,
                //        TermSetUniqueId,
                //        ObjectUniqueId,
                //        ObjectId,
                //        ObjectType,
                //        ChangeType,
                //        ChangeTime,
                //        ChangeData,
                //        ModifiedBy,
                //        ecmTermSet.Path FROM ECMChangeLog ecmLog WITH(NOLOCK)
                //        INNER JOIN ECMTermSetMembership ecmTermSet WITH(NOLOCK) ON ecmTermSet.TermId=ecmLog.ObjectId AND ecmTermSet.TermSetId=@SetId 
                //        WHERE ecmLog.TermSetUniqueId=@UniqueId AND ecmLog.ObjectType=1 AND ecmTermSet.ParentTermId=@TermId AND ecmLog.ChangeTime>@SinceTime";

                cmdText = @"declare @SetId int
set @SetId = ( SELECT ID FROM ECMTermSet WITH(NOLOCK) WHERE PartitionId=@PartitionId And UniqueId=@UniqueId)

SELECT  GroupUniqueId,
        TermSetUniqueId,
        ObjectUniqueId,
        ObjectId,
        ObjectType,
        ChangeType,
        ChangeTime,
        ChangeData,
        ModifiedBy,
        ecmTermSet.Path FROM ECMChangeLog ecmLog WITH(NOLOCK)
        INNER JOIN ECMTermSetMembership ecmTermSet WITH(NOLOCK) 
        ON ecmTermSet.PartitionId=ecmlog.PartitionId AND ecmTermSet.TermId=ecmLog.ObjectId AND ecmTermSet.TermSetId=@SetId 
        WHERE ecmlog.PartitionId=@PartitionId
        AND ecmLog.ChangeTime>@SinceTime 
        AND ecmLog.TermSetUniqueId=@UniqueId 
        AND ecmLog.ObjectType=1 
        AND ecmTermSet.PartitionId = @PartitionId
        AND ecmTermSet.ParentTermId=@TermId 
";
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.CommandType = CommandType.Text;
                SqlCommand command = mQueryWorker.Command;
                command.Parameters.Add(new SqlParameter("@UniqueId", SqlDbType.UniqueIdentifier));
                command.Parameters["@UniqueId"].Value = termSetId;
                command.Parameters.Add(new SqlParameter("@TermId", SqlDbType.Int));
                command.Parameters["@TermId"].Value = termIntId;
                mQueryWorker.AddParameter("@SinceTime", sinceTime.Value);
                mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    List<int> ids = new List<int>();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(3);
                        if (ids.Contains(id))
                        {
                            continue;
                        }
                        ids.Add(id);
                        AveTermChangeItem item = new AveTermChangeItem();
                        item.Id = reader.GetGuid(2);
                        //item.ItemId = reader.GetInt32(3);

                        item.ItemType = (AveTermChangeItem.ChangedItemType)reader.GetInt32(4);
                        item.ChangeType = (AveTermChangeItem.ChangedOperationType)reader.GetInt32(5);
                        item.ChangeTime = reader.GetDateTime(6);
                        item.PartitionId = defaultPartitionId;
                        if (!reader.IsDBNull(1))
                        {
                            item.TermSetId = reader.GetGuid(2);
                        }
                        item.GroupId = reader.GetGuid(0);
                        item.Path = reader.GetString(9);
                        items.Add(item);
                    }
                }

                //string end = "\\" + termIntId;
                //string middle = "\\" + termIntId + "\\";
                //string start = termIntId + "\\";

                //for (int i = items.Count - 1; i >= 0; i--)
                //{
                //    AveTermChangeItem item = items[i];

                //    if (item.Path.StartsWith(start) || item.Path.IndexOf(middle) != -1 || item.Path.EndsWith(end))
                //    {
                //        continue;
                //    }
                //    items.RemoveAt(i);
                //}

            }
            else
            {
                List<Guid> terms = GetTermIds(termSetId, termId, defaultPartitionId);
                foreach (Guid term in terms)
                {
                    AveTermChangeItem item = new AveTermChangeItem();
                    item.TermSetId = termSetId;
                    item.ItemType = (AveTermChangeItem.ChangedItemType)1;
                    item.ChangeType = 0;
                    item.Id = term;
                    //item.ItemId = t.TermId;                    
                    items.Add(item);
                }
            }
            foreach (AveTermChangeItem item in items)
            {
                AveTermInfo termInfo = GetTerm(termSetId, GetTermId(item.Id, defaultPartitionId), defaultPartitionId, defaultLanguage);
                item.Name = termInfo.Name;
                item.IsPinned = termInfo.IsPinned;
                item.IsReused = termInfo.IsReused;
                item.IsRoot = termInfo.IsRoot;
                item.IsSourceTerm = termInfo.IsSourceTerm;
                item.PinSourceTermSetId = termInfo.PinSourceTermSetId;
                item.Path = GetTermPath(defaultPartitionId, GetTermId(termId, defaultPartitionId), termInfo.IsSourceTerm ? 1 : 0);
            }
            return items;
        }
        /// <summary>
        /// 获取termset下所有的source terms,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetTermSetChildren(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT et.Id,et.UniqueId,etsm.Path,etsm.ParentTermId,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId AND etsm.TermSetId = ets.Id
left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id 
WHERE ets.UniqueId = @UniqueId AND etsm.PartitionId = @PartitionId AND etsm.IsSource = 1";
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
        /// <summary>
        /// 获取termset的parent(group),为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取group相关属性,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveMetadataGroupInfo GetTermGroupInfo(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT eg.Id,eg.UniqueId,eg.Name,eg.Description,eg.Type FROM ECMTermSet ets WITH(NOLOCK) INNER JOIN ECMGroup eg WITH(NOLOCK) ON ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId 
where ets.UniqueId=@UniqueId and ets.PartitionId=@PartitionId";
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
        /// <summary>
        /// 获取term的parent term(可能为termset),为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="parentTermId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="isRoot"></param>
        /// <param name="isSourceTerm"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取term的parent term的相关属性,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="parentTermUniqueId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermInfo GetParentTermInfo(Guid termSetId, Guid parentTermUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId 
WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.IsSource = 1 AND
et.UniqueId = @ParentTermUniqueId AND etsm.PartitionId = @PartitionId AND
ets.UniqueId = @TermSetId";

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
        /// <summary>
        /// 获取termset相关属性,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermSetInfo GetTermSetInfo(Guid termSetUniqueId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT eg.UniqueId,ets.Name,ets.Type FROM ECMTermSet ets WITH(NOLOCK) INNER JOIN ECMGroup eg WITH(NOLOCK) ON ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId WHERE
ets.UniqueId = @UniqueId AND ets.PartitionId = @PartitionId";
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
        /// <summary>
        /// 获取source term相关属性,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private AveTermInfo GetSourceTermInfo(Guid termSetId, int termId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId 
WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.IsSource = 1 AND
et.Id = @Id and etsm.TermId = @Id and etsm.PartitionId = @PartitionId";

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
        /// <summary>
        /// 获取在指定的termset中获取term的parent term相关属性,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        private Guid GetParentTermUniqueIdForTermSet(Guid termSetId, int parentTermId, Guid partitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT et.UniqueId From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.TermId = @Id AND 
etsm.PartitionId = @PartitionId AND ets.UniqueId = @TermSetId";
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

        [QueryReview("2012/05/17", "Long Liang", true, " re-order index")]
        /// <summary>
        /// 获取指定Guid的TermId，效率考虑，有API实现.
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public int GetTermId(Guid termUniqueId, Guid defaultPartitionId)
        {
            string cmdText = @"SELECT TOP 1 et.Id FROM ECMTerm et WITH(NOLOCK) WHERE et.PartitionId =@PartitionId AND et.UniqueId=@TermUniqueId";
            int termId = -1;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermUniqueId", termUniqueId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                if (reader.Read())
                {
                    termId = reader.GetInt32(0);
                }
            }
            return termId;
        }

        public int? GetGroupId(Guid groupId, Guid partitionId)
        {
            string cmdText = @"SELECT TOP 1 Id FROM ECMGroup WITH(NOLOCK) WHERE PartitionId = @PartitionId and UniqueId=@UniqueId";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            mQueryWorker.AddParameter("@UniqueId", groupId);

            var result = mQueryWorker.ExecuteScalar(cmdText);
            return result is DBNull ? (int?)null : (int?)result;
        }
        /// <summary>
        /// 获取指定Guid的GroupId，效率考虑，有API实现. TODO需要修改接口，添加PartitionId
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int? GetGroupId(Guid groupId)
        {
            string cmdText = @"SELECT TOP 1 Id FROM ECMGroup WITH(NOLOCK) WHERE UniqueId=@UniqueId";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", groupId);

            var result = mQueryWorker.ExecuteScalar(cmdText);
            return result is DBNull ? (int?)null : (int?)result;
        }

        /// <summary>
        /// 按照Global或者Local获取GroupIds信息，效率考虑，有API实现. TODO需要添加partitionId
        /// </summary>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetGroupIds(bool isGlobal)
        {
            string command = @"SELECT 
       [UniqueId],[Name]
  FROM [ECMGroup] WITH(NOLOCK) WHERE ";
            if (isGlobal)
            {
                command += "[Type]=0 or [Type]=1";
            }
            else
            {
                command += "[Type]=2";
            }
            Dictionary<Guid, string> ids = new Dictionary<Guid, string>();
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                while (reader.Read())
                {
                    Guid id = reader.GetGuid(0);
                    ids[id] = reader.GetString(1);
                }
            }
            return ids;
        }

        public Dictionary<Guid, string> GetGroupIds(bool isGlobal, Guid partitionId)
        {
            string command = @"SELECT 
       [UniqueId],[Name]
  FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and ";
            if (isGlobal)
            {
                command += "([Type]=0 or [Type]=1)";
            }
            else
            {
                command += "[Type]=2";
            }
            Dictionary<Guid, string> ids = new Dictionary<Guid, string>();
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PartitionId", partitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(command))
            {
                while (reader.Read())
                {
                    Guid id = reader.GetGuid(0);
                    ids[id] = reader.GetString(1);
                }
            }
            return ids;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 判断contentType是否published，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public bool IsPublished(string contentTypeId, Guid defaultPartitionId)
        {
            string cmdText = "SELECT COUNT(Id) FROM [ECMPackage] WITH(NOLOCK) WHERE [PartitionId]=@PartitionId AND Id=@Id AND [Type]=@Type AND IsPublished=1";
            mQueryWorker.ClearParameters();
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            mQueryWorker.AddParameter("@Type", new Guid("B4AD3A44-D934-4C91-8D1F-463ACEADE443"));
            mQueryWorker.AddParameter("@Id", contentTypeId);

            int count = (int)mQueryWorker.ExecuteScalar(cmdText);
            return count > 0;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 判断contentType是否unpublished，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public bool IsUnPublished(string contentTypeId, Guid defaultPartitionId)
        {
            string cmdText = "SELECT COUNT(Id) FROM [ECMPackage] WITH(NOLOCK) WHERE [PartitionId]=@PartitionId AND Id=@Id AND [Type]=@Type AND IsPublished=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            mQueryWorker.AddParameter("@Type", new Guid("B4AD3A44-D934-4C91-8D1F-463ACEADE443"));
            mQueryWorker.AddParameter("@Id", contentTypeId);

            int count = (int)mQueryWorker.ExecuteScalar(cmdText);
            return count > 0;
        }

        /// <summary>
        /// 获取TermStore，效率考虑，有API实现.
        /// </summary>
        /// <param name="defaultpartitionId"></param>
        /// <returns></returns>
        public string GetTermStore(Guid defaultpartitionId)
        {
            try
            {
                SqlCommand command = mQueryWorker.Command;
                mQueryWorker.ClearParameters();
                mQueryWorker.Command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@PartitionId", SqlDbType.UniqueIdentifier));
                command.Parameters["@PartitionId"].Value = defaultpartitionId;
                using (SqlDataReader reader = mQueryWorker.ExecuteReader("proc_ECM_GetServiceSettings"))
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            return string.Empty;
        }

        /// <summary>
        /// ChangedOperationType的优先级为Delete > Add > Edit > FakeAsParent
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="targetType"></param>
        /// <returns>返回结果为true说明targetType的优先级高于baseType</returns>
        private bool CompareChangedOperationType(AveTermChangeItem.ChangedOperationType baseType, AveTermChangeItem.ChangedOperationType targetType)
        {
            bool result = false;
            if (targetType == AveTermChangeItem.ChangedOperationType.Delete)
            {
                result = true;
            }
            if (baseType == AveTermChangeItem.ChangedOperationType.FakeAsParent && targetType == AveTermChangeItem.ChangedOperationType.Edit)
            {
                result = true;
            }
            return result;
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
            //item.TermSetId
            //item.GroupId
            item.ChangeData = string.Empty;
        }

        /// <summary>
        /// 获取我们想要目标对象的Guid
        /// </summary>
        /// <param name="item">实际操作的对象信息</param>
        /// <param name="itemType"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private Guid GetTargetId(AveTermChangeItem item, AveTermChangeItem.ChangedItemType itemType)
        {
            Guid targetId = Guid.Empty;
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
        /// 合并获得到的所有变化,每个对象只取一个最重要的操作,也会删除一些不需要做的对象操作信息
        /// </summary>
        /// <param name="changes">所有的变化</param>
        /// <param name="itemType">我们想要获取的对象变化的item type</param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        private List<AveTermChangeItem> MergeChanges(List<AveTermChangeItem> changes, AveTermChangeItem.ChangedItemType itemType)
        {
            List<AveTermChangeItem> result = new List<AveTermChangeItem>();
            Dictionary<Guid, AveTermChangeItem> tempResult = new Dictionary<Guid, AveTermChangeItem>();

            foreach (AveTermChangeItem changeItem in changes)
            {
                Guid targetId = GetTargetId(changeItem, itemType);
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

            foreach (AveTermChangeItem item in tempResult.Values)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 获取MMS下store中的Group改变为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {
            if (sinceTime.HasValue)
            {
                List<AveTermChangeItem> temp = GetChanges(null, null, sinceTime.Value, null, defaultPartitionId);
                List<AveTermChangeItem> changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.Group);
                foreach (AveTermChangeItem changeItem in changes)
                {
                    if (changeItem.ChangeType.Equals(AveTermChangeItem.ChangedOperationType.Delete))
                        continue;
                    changeItem.Name = GetGroup(changeItem.GroupId, defaultPartitionId).Name;
                    var groupId = GetGroupId(changeItem.GroupId);
                    changeItem.ObjectId = groupId.HasValue ? (int)groupId : -1;
                    changeItem.PartitionId = defaultPartitionId;
                }
                return changes;
            }
            else
            {
                List<AveTermChangeItem> items = new List<AveTermChangeItem>();
                Dictionary<Guid, string> groups = GetGroupIds(isGlobal);
                foreach (KeyValuePair<Guid, string> id in groups)
                {
                    AveTermChangeItem item = new AveTermChangeItem();
                    item.Id = id.Key;
                    item.PartitionId = defaultPartitionId;
                    item.GroupId = id.Key;
                    item.ItemType = (AveTermChangeItem.ChangedItemType)3;
                    item.ChangeType = 0;
                    item.Name = id.Value;
                    items.Add(item);
                }
                return items;
            }
        }
        /// <summary>
        /// ??MMS?store??Group???Incremental??,????,?API??
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInStoreForTenant(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {
            Dictionary<Guid, string> groups = GetGroupIds(isGlobal, defaultPartitionId);
            if (sinceTime.HasValue)
            {
                List<AveTermChangeItem> temp = GetChanges(null, null, sinceTime.Value, null, defaultPartitionId);
                List<AveTermChangeItem> returnChanges = new List<AveTermChangeItem>();
                List<AveTermChangeItem> changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.Group);
                foreach (AveTermChangeItem changeItem in changes)
                {
                    if (changeItem.ChangeType.Equals(AveTermChangeItem.ChangedOperationType.Delete))
                        continue;
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
            else
            {
                List<AveTermChangeItem> items = new List<AveTermChangeItem>();
                foreach (KeyValuePair<Guid, string> id in groups)
                {
                    AveTermChangeItem item = new AveTermChangeItem();
                    item.Id = id.Key;
                    item.GroupId = id.Key;
                    item.ItemType = (AveTermChangeItem.ChangedItemType)3;
                    item.ChangeType = 0;
                    item.Name = id.Value;
                    item.PartitionId = defaultPartitionId;
                    items.Add(item);
                }
                return items;
            }
        }

        /// <summary>
        /// 获取MMS某个Group下的TermSet改变为Incremental处理,效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            if (sinceTime.HasValue)
            {
                List<AveTermChangeItem> temp = GetChanges(GetGroupId(groupId), null, sinceTime.Value, null, defaultPartitionId);
                List<AveTermChangeItem> changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.TermSet);
                foreach (AveTermChangeItem changeItem in changes)
                {
                    if (changeItem.ChangeType.Equals(AveTermChangeItem.ChangedOperationType.Delete))
                        continue;
                    AveTermSetInfo info = GetTermSet(changeItem.TermSetId.Value, defaultPartitionId, defaultLanguage);
                    changeItem.Name = info.Name;
                    changeItem.TermSetType = info.Type;
                    changeItem.ObjectId = GetTermSetId(changeItem.TermSetId.Value, defaultPartitionId);
                    changeItem.PartitionId = defaultPartitionId;
                }
                return changes;
            }
            else
            {
                List<AveTermChangeItem> items = new List<AveTermChangeItem>();
                List<AveTermSetInfo> sets = GetTermSetIds(groupId, defaultPartitionId, defaultLanguage);
                foreach (AveTermSetInfo t in sets)
                {
                    AveTermChangeItem item = new AveTermChangeItem();

                    item.TermSetId = t.Id;
                    item.ItemType = (AveTermChangeItem.ChangedItemType)2;
                    item.ChangeType = 0;
                    item.Id = t.Id;
                    item.Name = t.Name;
                    item.GroupId = groupId;
                    item.TermSetType = t.Type;
                    item.PartitionId = defaultPartitionId;
                    items.Add(item);
                }
                return items;
            }
        }

        /// <summary>
        /// 获取TermSet中的Term改变为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage)
        {
            if (sinceTime.HasValue)
            {
                List<AveTermChangeItem> results = new List<AveTermChangeItem>();
                List<AveTermChangeItem> temp = GetChanges(null, GetTermSetId(termSetId, defaultPartitionId), sinceTime.Value, 1, defaultPartitionId);
                List<AveTermChangeItem> changes = MergeChanges(temp, AveTermChangeItem.ChangedItemType.Term);
                foreach (AveTermChangeItem item in changes)
                {
                    AveTermChangeItem changeItemTree = GetChangeItemTree(item, defaultPartitionId, defaultLanguage);
                    if (changeItemTree == null)
                    {
                        continue;
                    }
                    AddToResults(results, changeItemTree);
                }
                return results;
            }
            else
            {
                List<AveTermChangeItem> items = new List<AveTermChangeItem>();
                List<AveTermInfo> terms = GetTermsInTermSet(termSetId, defaultPartitionId, defaultLanguage);
                foreach (AveTermInfo termInfo in terms)
                {
                    AveTermChangeItem item = new AveTermChangeItem();

                    item.TermSetId = termSetId;
                    item.ItemType = (AveTermChangeItem.ChangedItemType)1;
                    item.ChangeType = 0;
                    item.Id = termInfo.Id;
                    item.Name = termInfo.Name;
                    //item.ItemId = t.TermId;
                    item.IsPinned = termInfo.IsPinned;
                    item.IsReused = termInfo.IsReused;
                    item.IsRoot = termInfo.IsRoot;
                    item.IsSourceTerm = termInfo.IsSourceTerm;
                    item.PinSourceTermSetId = termInfo.PinSourceTermSetId;
                    item.Path = GetTermPath(defaultPartitionId, GetTermId(termInfo.Id, defaultPartitionId), termInfo.IsSourceTerm ? 1 : 0);
                    items.Add(item);
                }
                return items;
            }
        }

        public void AddToResults(List<AveTermChangeItem> results, AveTermChangeItem changeItem)
        {
            if (results.Count == 0)
            {
                results.Add(changeItem);
                return;
            }
            AveTermChangeItem keyItem = null;
            foreach (AveTermChangeItem item in results)
            {
                if (item.Id == changeItem.Id)
                {
                    keyItem = item;
                    break;
                }
            }
            if (keyItem == null)
            {
                results.Add(changeItem);
                return;
            }
            if (changeItem.SubTerms.Count > 0)
            {
                AveTermChangeItem tempKeyItem = keyItem.SubTerms.Find(item => changeItem.SubTerms[0].Id == item.Id);
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

        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId)
        {
            return GetMetaDataChanges(groupId, sinceTime, toTime, defaultPartitionId, true);
        }

        [QueryReview("2012/05/17", "Long Liang")]
        private List<AveTermChangeItem> GetMetaDataChanges(Guid id, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId, bool isGroup)
        {
            string commandText = string.Empty;

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();

            #region sql command
            if (!sinceTime.HasValue)
            {
                commandText = @"SELECT * FROM ECMChangeLog WITH(NOLOCK)
                where PartitionId = @PartitionId 
                AND ChangeTime <= @ChangeTimeTo";

                mQueryWorker.AddParameter("@ChangeTimeTo", toTime);
            }
            else if (!toTime.HasValue)
            {
                commandText = @"SELECT * FROM ECMChangeLog WITH(NOLOCK) 
                where PartitionId = @PartitionId 
                AND ChangeTime >= @ChangeTimeFrom";

                mQueryWorker.AddParameter("@ChangeTimeFrom", sinceTime);
            }
            else
            {
                commandText = @"SELECT * FROM ECMChangeLog WITH(NOLOCK)
                where 
                PartitionId = @PartitionId 
                AND ChangeTime >= @ChangeTimeFrom 
                AND ChangeTime <= @ChangeTimeTo";

                mQueryWorker.AddParameter("@ChangeTimeFrom", sinceTime);
                mQueryWorker.AddParameter("@ChangeTimeTo", toTime);
            }
            #endregion

            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            if (isGroup)
            {
                commandText += " AND GroupUniqueId = @GroupUniqueId";
                mQueryWorker.AddParameter("@GroupUniqueId", id);
            }
            else
            {
                commandText += " AND TermSetUniqueId = @TermSetUniqueId";
                mQueryWorker.AddParameter("@TermSetUniqueId", id);
            }


            List<AveTermChangeItem> items = new List<AveTermChangeItem>();

            #region execute query
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
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
                    AveTermChangeItem item = new AveTermChangeItem();

                    if (!reader.IsDBNull(4))
                    {
                        item.ObjectId = reader.GetInt32(4);
                    }

                    if (!reader.IsDBNull(3))
                    {
                        item.Id = reader.GetGuid(3);
                    }

                    //item.ItemId = reader.GetInt32(4);

                    item.ItemType = (AveTermChangeItem.ChangedItemType)reader.GetInt32(5);
                    item.ChangeType = (AveTermChangeItem.ChangedOperationType)reader.GetInt32(6);
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
            #endregion

            return items;
        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId)
        {
            return GetMetaDataChanges(termSetId, sinceTime, toTime, defaultPartitionId, false);
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 获取Group下的TermSets通过Id，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId, int defaultLanguage)
        {
            //string commandText = @"SELECT DISTINCT es.Id FROM ECMTermSet es WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) WHERE es.GroupId = eg.Id AND eg.UniqueId=@GroupId";
            const string commandText = @"
SELECT DISTINCT es.Id 
FROM ECMTermSet es WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) 
WHERE eg.PartitionId=@PartitionId AND eg.UniqueId=@GroupId AND es.PartitionId=@PartitionId AND es.GroupId = eg.Id  
";
            List<int> termSetIds = null;
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@GroupId", groupId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
            {
                termSetIds = GetIds(reader);
            }

            return GetTermSets(termSetIds, defaultPartitionId, defaultLanguage);
        }

        /// <summary>
        /// 获取特定的TermSet通过Id，效率考虑，有API实现
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId, int defaultLanguage)
        {
            List<int> ids = new List<int>();
            ids.Add(setId);

            List<AveTermSetInfo> termSets = GetTermSets(ids, defaultPartitionId, defaultLanguage);
            if (termSets == null || termSets.Count == 0)
            {
                return null;
            }
            else
            {
                return termSets[0];
            }
        }

        [QueryReview("2012/05/17", "Long Liang", true, " add partition id")]
        /// <summary>
        /// 获取特定的TermSet通过Guid，效率考虑，有API实现
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId, int defaultLanguage)
        {
            const string cmdText = @"Select et.Id, et.PartitionId, et.CreatedTime, et.LastModifiedTime, et.Owner, et.CustomSortOrder, et.UniqueId, et.Name, et.Description,
                               et.Type, et.IsOpen, et.AvailableForTagging, et.Stakeholders, et.Contact, et.GroupId from ECMTermSet et WITH(NOLOCK) where 
                               et.PartitionId=@PartitionId AND et.UniqueId=@UniqueId";
            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UniqueId", setId);
            mQueryWorker.AddParameter("@PartitionId", defaultPartitionId);

            AveTermSetInfo termSetInfo = null;
            int termSetId = 0;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                if (reader.Read())
                {
                    termSetId = reader.GetInt32(0);
                    termSetInfo = new AveTermSetInfo();
                    termSetInfo.Id = reader.GetGuid(6);
                    termSetInfo.Name = reader.GetString(7);
                    termSetInfo.PartitionId = defaultPartitionId;
                    //处理多语言情况下名字情况例如：1033|CC Team;1041|CC Team
                    int tmpLength = 0;
                    string tmpName = termSetInfo.Name;
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
                        string holders = reader.GetString(12);
                        if (!String.IsNullOrEmpty(holders))
                        {
                            termSetInfo.Stakeholders = new List<string>();
                            foreach (string h in holders.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                termSetInfo.Stakeholders.Add(FindRealUserGroupName(h));
                            }
                        }
                    }
                }
            }
            if (termSetInfo != null)
            {
                //for 13 read Custom Property
                const string propertyCmdText = @"
select Property.PropertyName,Property.PropertyValue from dbo.ECMTermSet as TermSet With(NOLOCK) 
inner join dbo.ECMTermProperty as Property With(NOLOCK) on TermSet.PartitionId = Property.PartitionId and Property.TermId = 0 and TermSet.Id = Property.TermSetId 
where TermSet.Id = @ID";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", termSetId);
                using (var reader = mQueryWorker.ExecuteReader(propertyCmdText))
                {
                    if (reader.HasRows)
                    {
                        termSetInfo.CustomProperties = new Dictionary<string, string>();
                    }

                    while (reader.Read())
                    {
                        termSetInfo.CustomProperties.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
            return termSetInfo;
        }

        [QueryReview("2012/05/17", "Long Liang", true, " re-order the index")]
        /// <summary>
        /// 通过MetadataServiceApplication获取不到Term中的GetDefaultLabel方法,只能通过SQL实现
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public string GetTermDefaultLabel(int termId, Guid defaultPartitionId, int defaultLanguage)
        {
            string cmdText = @"SELECT [ECMTermLabel].Label 
                    FROM [ECMTermLabel] WITH(NOLOCK)
                    WHERE 
                    [ECMTermLabel].PartitionId = @DefaultPartitionId
                    AND
                    [ECMTermLabel].TermId = @TermId 
                    AND 
                    [ECMTermLabel].LCID = @DefaultLanguage 
                    AND 
                    [ECMTermLabel].IsDefault = 1 
                    ; 
                    ";

            mQueryWorker.Command.CommandType = CommandType.Text;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TermId", termId);
            mQueryWorker.AddParameter("@DefaultLanguage", defaultLanguage);
            mQueryWorker.AddParameter("@DefaultPartitionId", defaultPartitionId);

            object label = mQueryWorker.ExecuteScalar(cmdText);

            if (label != null)
            {
                return label.ToString();
            }
            else
            {
                return null;
            }
        }

        public List<ServiceSetting> GetPartitionServiceSettings()
        {
            List<ServiceSetting> result = new List<ServiceSetting>();
            string cmdText = @"SELECT [ECMServiceSettings].PartitionId,[ECMServiceSettings].Settings 
                    FROM [ECMServiceSettings] WITH(NOLOCK)
                    where [ECMServiceSettings].PartitionId <> '00000000-0000-0000-0000-000000000000';
                    ";
            mQueryWorker.Command.CommandType = CommandType.Text;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
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
            return result;
        }

        public List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId)
        {
            List<AveSiteMapVisible> result = new List<AveSiteMapVisible>();
            try
            {
                mQueryWorker.AddParameter("@SubscriptionId", defaultPartitionId);
                mQueryWorker.Command.CommandType = CommandType.Text;
                string cmdText = @"SELECT Id,ApplicationId,DatabaseId,[Path],[Version] FROM
        dbo.SiteMap WITH (NOLOCK) where SubscriptionId = @SubscriptionId and DeleteTransactionId = 0x";
                using (var dReader = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dReader.HasRows)
                    {
                        while (dReader.Read())
                        {
                            AveSiteMapVisible managedSite = new AveSiteMapVisible();
                            managedSite.SiteId = dReader.GetGuid(0);
                            managedSite.ApplicationId = dReader.GetGuid(1);
                            managedSite.DatabaseId = dReader.GetGuid(2);
                            managedSite.Path = dReader.GetString(3);
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
                logger.Warn("An error occurred when call the method GetTenancyAdminSiteId,error:{0}", ex.ToString());
            }
            return result;
        }

        #endregion

        #region Replicator

        /// <summary>
        /// 获取一个ContentDB中的所有Webs信息,效率考虑，有API实现.
        /// </summary>
        /// <returns></returns>
        public IAveQueryDataReader GetAllWebs()
        {
            return mQuerySessionSchema.GetAllWebs();
        }

        /// <summary>
        /// 获取一个Web下所有的list的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        public IAveQueryDataReader GetAllListsInWeb(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            return mQuerySessionSchema.GetAllListsInWeb(siteId, webId, includeRecycleBin);
        }

        /// <summary>
        /// 获取一个ContentDB中的所有EventReceivers信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="assemblyFullName"></param>
        /// <returns></returns>
        public IAveQueryDataReader GetAllEventReceivers(string assemblyFullName)
        {
            return mQuerySessionSchema.GetAllEventReceivers(assemblyFullName);
        }

        /// <summary>
        /// 根据scripts更新EventReceiver信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="scripts"></param>
        public void Commit(List<string> scripts)
        {
            try
            {
                mQueryWorker.Command.Transaction = mQueryWorker.BeginTransaction();
                int count = 0;
                foreach (var str in scripts)
                {
                    mQueryWorker.ExecuteNonQuery(str);
                    count++;
                    if (count > 1000)
                    {
                        mQueryWorker.Command.Transaction.Commit();
                        mQueryWorker.Command.Transaction = mQueryWorker.BeginTransaction();
                        count = 0;
                    }
                }
                mQueryWorker.Command.Transaction.Commit();
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取新创建的Web信息，效率考虑，有API实现
        /// </summary>
        /// <param name="newWebs"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sBuilder"></param>
        public void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder)
        {
            mQuerySessionSchema.GetNewWebsByContentDB(newWebs, startTime, endTime, sBuilder);
        }
        public void GetAllNewWebsInContentDB(Dictionary<Guid, List<Guid>> newWebs, DateTime startTime, DateTime endTime)
        {
            string cmdText = @"Select e.WebId, e.SiteId From EventCache as e with(nolock), AllWebs as w with(nolock) Where e.ObjectType=4 And e.EventType=4096
                                             And e.WebId=w.Id And w.DeleteTransactionId = 0x And e.EventTime Between @StartTime And @EndTime;";
            try
            {
                mQueryWorker.AddParameter("@StartTime", startTime);
                mQueryWorker.AddParameter("@EndTime", endTime);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        Guid webId = reader.GetGuid(0);
                        Guid siteId = reader.GetGuid(1);
                        List<Guid> webList;
                        if (!newWebs.TryGetValue(siteId, out webList))
                        {
                            webList = new List<Guid>();
                            newWebs[siteId] = webList;
                        }
                        webList.Add(webId);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取一个ContentDB下的所有Web(非RootWeb)的ID信息
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="allWebs"></param>
        public void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs)
        {
            mQuerySessionSchema.GetAllWebsByContentDB(dataBase, allWebs);
        }
        public void GetAllSubWebsInContentDB(IAveContentDatabase dataBase, Dictionary<Guid, List<Guid>> allWebs)
        {
            string cmdText = @"Select ID, SiteId, ParentWebId From AllWebs with (nolock) where DeleteTransactionId = 0x;";
            try
            {
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(2))
                        {
                            var siteId = reader.GetGuid(1);
                            List<Guid> webList;
                            if (!allWebs.TryGetValue(siteId, out webList))
                            {
                                webList = new List<Guid>();
                                allWebs[siteId] = webList;
                            }
                            webList.Add(reader.GetGuid(0));
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

        }

        /// <summary>
        /// 将EventReceivers信息写入数据库中
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="siteId"></param>
        /// <param name="assemblyFullName"></param>
        /// <param name="eventHandlerClassNames"></param>
        public void WebDelAndMoveEventHandler(Guid webId, Guid siteId, string assemblyFullName, string eventHandlerClassNames)
        {
            try
            {
                string cmdText = "EXEC proc_InsertEventReceiver '{0}',N'','{1}','{2}','{2}',1,NULL,NULL,NULL,0,{3},10000,NULL,N'{4}',N'{5}',NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL";
                string delProc = string.Format(cmdText, Guid.NewGuid().ToString(), siteId.ToString(), webId.ToString(), 10202, assemblyFullName, eventHandlerClassNames);
                string movProc = string.Format(cmdText, Guid.NewGuid().ToString(), siteId.ToString(), webId.ToString(), 10203, assemblyFullName, eventHandlerClassNames);
                mQueryWorker.ExecuteNonQuery(delProc);
                mQueryWorker.ExecuteNonQuery(movProc);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        #endregion

        #region UtilityProcess

        /// <summary>
        /// 获取User信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="userSearchInfo"></param>
        /// <param name="flag"></param>
        /// <param name="siteId"></param>
        /// <param name="isExact"></param>
        /// <returns></returns>
        public List<AveUserDetail> GetUserDetailByNative(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact)
        {
            List<AveUserDetail> userDetails = new List<AveUserDetail>();
            try
            {
                using (SqlCommand command = GetNativeCommand(userSearchInfo, isExact, flag, siteId))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string loginName = reader.GetString(0);
                            bool isGroup = reader.GetString(3).Equals("2") ? true : false;
                            if (isGroup)
                            {
                                AveUserDetail detail = new AveUserDetail();
                                detail.LoginName = loginName;
                                detail.DisplayName = reader.GetString(1);
                                detail.Email = reader.GetString(2);
                                detail.AccountType = AveAccountType.SharePointGroup;
                                userDetails.Add(detail);
                            }
                            else
                            {
                                bool isDomainGroup = reader.GetString(3).Equals("1") ? true : false;
                                AveUserDetail detail = new AveUserDetail();
                                detail.LoginName = loginName;
                                detail.DisplayName = reader.GetString(1);
                                detail.Email = reader.GetString(2);
                                detail.AccountType = AveAccountType.SharePointUser;
                                userDetails.Add(detail);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetUserInfoError, ex);
            }
            return userDetails;
        }

        #endregion

        #region Central Admin

        /// <summary>
        /// 获取SQL 服务器所在机器的HostName,API方式有缺陷，有API实现.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public string GetDBServerName()
        {
            try
            {
                string cmdText = "SELECT SERVERPROPERTY('servername')";
                return mQueryWorker.ExecuteScalar(cmdText) as string;
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取数据库所在磁盘的剩余空间和该数据库占用空间和可用空间,无API实现.
        /// </summary>
        /// <param name="usedSize"></param>
        /// <param name="freeSize"></param>
        /// <param name="diskFreesize"></param>
        public void GetDBSize(out double usedSize, out double freeSize, out double diskFreesize)//MB
        {
            usedSize = freeSize = diskFreesize = 0;
            StringBuilder errorMessage = new StringBuilder();
            try
            {
                SqlCommand cmd = mQueryWorker.CreateCommand();
                cmd.CommandTimeout = 0;

                #region freeDiskSpace
                try
                {
                    cmd.CommandText = "exec sp_helpfile";
                    List<string> logicalDrives = new List<string>();
                    using (SqlDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            string logicalDrive = sr.GetString(2).ToLower(CultureInfo.InvariantCulture);
                            logicalDrive = logicalDrive.Substring(0, 1);
                            if (!logicalDrives.Contains(logicalDrive))
                            {
                                logicalDrives.Add(logicalDrive);
                            }
                        }
                    }
                    cmd.CommandText = "exec master..xp_FixedDrives";
                    using (SqlDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            string logicalDrive = sr.GetString(0).ToLower(CultureInfo.InvariantCulture);
                            int freeSpace = sr.GetInt32(1);
                            if (logicalDrives.Contains(logicalDrive))
                            {
                                diskFreesize += freeSpace;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    errorMessage.AppendFormat("GetDBSizeFreeSpaceWarn:{0}.", new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex));
                }
                catch (AveQueryException queryexception)
                {
                    errorMessage.AppendFormat("GetDBSizeFreeSpaceWarn:{0}.", queryexception.ToString());
                }
                catch (Exception e)
                {
                    errorMessage.AppendFormat("GetDBSizeFreeSpaceWarn:{0}.", new AveQueryException(e.Message, e).ToString());
                    //logger.Warn(LOGRESX.CASiteCollectionChangeContentDatabaseGetDBSizeFreeSpaceWarn, ex.ToString());
                }
                #endregion

                #region Database Size

                try
                {
                    cmd.CommandText = "exec sp_SpaceUsed";
                    using (SqlDataReader sr = cmd.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            double totalMB = Convert.ToDouble(sr.GetString(1).Split(' ')[0], CultureInfo.InvariantCulture);
                            freeSize = Convert.ToDouble(sr.GetString(2).Split(' ')[0], CultureInfo.InvariantCulture);
                            usedSize = totalMB - freeSize;
                            break;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    errorMessage.AppendFormat("GetDBSizeDBSizeWarn:{0}.", new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex));
                }
                catch (AveQueryException queryexception)
                {
                    errorMessage.AppendFormat("GetDBSizeDBSizeWarn:{0}.", queryexception.ToString());
                }
                catch (Exception e)
                {
                    errorMessage.AppendFormat("GetDBSizeDBSizeWarn:{0}.", new AveQueryException(e.Message, e).ToString());
                    //logger.Warn(LOGRESX.CASiteCollectionChangeContentDatabaseGetDBSizeFreeSpaceWarn, ex.ToString());
                }
                #endregion
            }
            catch (SqlException ex)
            {
                errorMessage.AppendFormat("GetDBSizeDBInfoWarn:{0}.", new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex));
            }
            catch (AveQueryException queryexception)
            {
                errorMessage.AppendFormat("GetDBSizeDBInfoWarn:{0}.", queryexception.ToString());
            }
            catch (Exception e)
            {
                errorMessage.AppendFormat("GetDBSizeDBInfoWarn:{0}.", new AveQueryException(e.Message, e).ToString());
                //logger.Warn(LOGRESX.CASiteCollectionChangeContentDatabaseGetDBSizeFreeSpaceWarn, ex.ToString());
            }
            if (!string.IsNullOrEmpty(errorMessage.ToString()))
            {
                throw new AveQueryException("Get DB size error.", new Exception(errorMessage.ToString()));
            }
        }

        /// <summary>
        /// 获取Orphan Sites,无API实现
        /// </summary>
        /// <param name="siteIdFilter"></param>
        /// <param name="appUrl"></param>
        /// <param name="appSuffix"></param>
        /// <returns></returns>
        public IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix)
        {
            return mQuerySessionSchema.GetOrphanSite(siteIdFilter, appUrl, appSuffix);
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取一个WebApp下所有SiteCollection的Id,效率考虑，有API实现.
        /// </summary>
        /// <param name="webAppId"></param>
        /// <returns></returns>
        public string GetSiteIds(Guid webAppId)
        {
            string cmdText = "select id from sitemap with (nolock) where ApplicationId= @ApplicationId";
            StringBuilder siteIds = new StringBuilder();
            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@ApplicationId", webAppId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            siteIds.AppendFormat("'{0}',", reader.GetGuid(0).ToString());
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            if (siteIds.Length == 0)
            {
                return string.Empty;
            }
            siteIds.Length = siteIds.Length - 1;
            return string.Format("and s.id not in ({0})", siteIds.ToString());
        }

        [QueryReview("2012/05/17", "Long Liang", true, "should have check the group member permission")]
        /// <summary>
        /// 获取无权限的Users和Groups,无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <param name="searchUsers"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public IAveQueryDataReader GetSiteNoPermissionAccounts(Guid siteId, Guid scopeId, List<string> searchUsers)
        {
            //            string cmdstring =
            //        @" with noPermissionUserAndGroup(siteid, GroupName, LoginName) as (
            //	         select  tp_SiteID, null , tp_Login 
            //	         from UserInfo with (nolock) 
            //	         where tp_isActive = 1 AND tp_siteAdmin = 0 and tp_SiteID = @siteId
            //	         and tp_id not in
            //	             (
            //		            SELECT DISTINCT UserInfo.tp_ID FROM UserInfo with (nolock) INNER JOIN
            //		            RoleAssignment with (nolock) ON UserInfo.tp_SiteID = RoleAssignment.siteId AND UserInfo.tp_ID = principalId
            //                    where  UserInfo.tp_SiteID = @siteId and RoleAssignment.ScopeId=@scopeId
            //		             union 
            //		             select distinct GroupMembership.MemberId from GroupMembership with (nolock) left join Groups with (nolock) 
            //                    on GroupMembership.GroupId = Groups.ID and GroupMembership.SiteId = Groups.SiteId
            //                     where  GroupMembership.SiteId = @siteId
            //	             )
            //            union
            //		    SELECT  siteid, Title, Title
            //		    FROM    Groups with (nolock)  where groups.SiteId = @siteId and
            //		    Groups.id not in
            //		        (
            //			        SELECT DISTINCT Groups.ID FROM  Groups with (nolock)  inner JOIN
            //			        RoleAssignment with (nolock) ON groups.siteid = RoleAssignment.siteid AND groups.id = principalId
            //                    where  Groups.SiteId = @siteId and RoleAssignment.ScopeId=@scopeId
            //		        )
            //            )
            //            select * from noPermissionUserAndGroup WITH(NOLOCK) ";
            string cmdstring = @"
WITH  noPermissionUserAndGroup(SiteId, GroupName, LoginName) AS 
 (
 
     SELECT  tp_SiteID, null , tp_Login 
     From UserInfo with (nolock) 
     Where tp_isActive = 1 AND tp_siteAdmin = 0 and tp_SiteID = @siteId
     and tp_id not in
         (
         
            SELECT DISTINCT UserInfo.tp_ID 
            FROM UserInfo with(nolock)
            inner join
            RoleAssignment with(nolock)
            on UserInfo.tp_SiteID=RoleAssignment.SiteId 
            and UserInfo.tp_ID = RoleAssignment.PrincipalId
            Where RoleAssignment.SiteId=@siteId and RoleAssignment.ScopeId=@scopeId and UserInfo.tp_SiteID=@siteId

            union
            select distinct GroupMembership.MemberId from GroupMembership with(nolock)
            inner join RoleAssignment with(nolock)
            on GroupMembership.SiteId = RoleAssignment.SiteId and GroupMembership.GroupId=RoleAssignment.PrincipalId
            where GroupMembership.SiteId=@siteId and RoleAssignment.SiteId=@siteId and RoleAssignment.ScopeId=@scopeId
         )
    union
    SELECT  SiteId, Title, Title
    FROM    Groups with (nolock)  where groups.SiteId = @siteId and
    Groups.id not in
        (
            SELECT DISTINCT Groups.ID FROM  Groups with (nolock)  inner JOIN
            RoleAssignment with (nolock) ON groups.SiteId = RoleAssignment.SiteId AND groups.id = principalId
            where  Groups.SiteId = @siteId and RoleAssignment.ScopeId=@scopeId
        )
)
 

select * from noPermissionUserAndGroup WITH(NOLOCK)
";
            try
            {
                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.Parameters.AddWithValue("@siteId", siteId);
                    command.Parameters.AddWithValue("@scopeId", scopeId);
                    if (searchUsers != null && searchUsers.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder("LoginName in (");
                        string format = "N" + "'{0}'," + "N" + "'i:0#.w|{0}'," + "N" + "'c:0+.w|{0}',";
                        foreach (string user in searchUsers)
                        {
                            sb.Append(string.Format(format, user.Replace("'", "''")));
                        }
                        sb.Length--;
                        sb.Append(")");
                        cmdstring = string.Format("{0} where {1}", cmdstring, sb);
                    }
                    command.CommandText = cmdstring;
                    return new AveQueryDataReader(command.ExecuteReader());
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

        }

        [QueryReview("2012/05/17", "Long Liang", true, " re-order the index")]
        /// <summary>
        /// 查询Web下最顶端Navigation关联的Document信息
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        public string GetDocNameFromDB(Guid siteId, Guid webId)
        {
            string docName = String.Empty;
            //SqlConnection conn = null;
            SqlDataReader reader = null;
            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    //可能会有性能问题
                    cmd.CommandText = "select docid from NavNodes with (nolock) where SiteId=@siteID and webID=@webID and EidParent < 0 and url is null";
                    cmd.Parameters.AddWithValue("@siteID", siteId);
                    cmd.Parameters.AddWithValue("@webID", webId);
                    //conn.Open();
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            Guid docID = reader.GetGuid(0);
                            reader.Close();
                            reader.Dispose();
                            cmd.CommandText = "Select leafname from alldocs with (nolock) where id=@ID and SiteId=@siteID";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@siteID", siteId);
                            cmd.Parameters.AddWithValue("@ID", docID);
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    docName = reader.GetString(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            return docName;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 删除Orphan Site，无API实现.
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="itemId"></param>
        public void RecycleOrphanSiteInDB(IAveContentDatabase dataBase, string itemId)
        {
            var errorMessage = default(String);

            bool siteIsAlreadyInDeletion = false;
            using (SqlCommand commandSelect = mQueryWorker.CreateCommand())
            {
                commandSelect.Parameters.Add(new SqlParameter { DbType = DbType.String, Value = itemId, ParameterName = "@SiteId" });
                commandSelect.CommandText = @"select Id 
                            from SiteDeletion with (nolock) 
                            where SiteId=@SiteId";

                using (var reader = commandSelect.ExecuteReader())
                {
                    siteIsAlreadyInDeletion = reader.Read();
                }
            }

            if (!siteIsAlreadyInDeletion)
            {
                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.Parameters.Add(new SqlParameter { DbType = DbType.String, Value = itemId, ParameterName = "@SiteId" });
                    using (var trans = mQueryWorker.BeginTransaction())
                    {
                        try
                        {
                            command.Transaction = trans;
                            string strSqlCmdInsert = String.Format(
                                CultureInfo.InvariantCulture,
                                @"INSERT INTO [{0}].[dbo].[SiteDeletion] 
                            ([SiteId],[InDeletion],[Restorable],[DeleteIsForMigration])
                            VALUES (@SiteId,0,0,1)", new Object[] { dataBase.Name });
                            command.CommandText = strSqlCmdInsert;
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();

                            string strSqlCmdUpdate =
                                String.Format(
                                CultureInfo.InvariantCulture,
                                @"UPDATE [{0}].[dbo].[AllSites]
                               SET [Deleted] = 1
                               WHERE Id=@SiteId", new Object[] { dataBase.Name });
                            command.CommandText = strSqlCmdUpdate;
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();

                            trans.Commit();
                        }
                        catch (Exception e)
                        {
                            trans.Rollback();
                            errorMessage += e.Message + Environment.NewLine;
                        }
                    }
                }
            }
        }

        [Obsolete("not used any more, will be removed later, use GetAllPageInWeb instead.")]
        public List<string> GetAllPageOfWeb(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 效率考虑，暂无API实现，可以用于获取当前web下所有page,不包括subsite中page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetAllPageInWeb(Guid siteId, Guid webId, bool isCurrentVersion = true)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@WebId", webId},
                {"@SiteId", siteId}
            };
            var commandText = @"
        select Id,DirName,LeafName,UIVersion,UIVersionString,IsCurrentVersion
        from AllDocs with(nolock) 
        where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and leafname like '%.aspx' and Type=0";

            return GetAllPageInternal(parameters, commandText, isCurrentVersion);
        }

        private Dictionary<Guid, string> GetAllPageInternal(IDictionary<string, object> parameters, string queryString, bool isCurrentVersion)
        {
            var result = new Dictionary<Guid, string>();
            try
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameters(parameters);
                using (var reader = mQueryWorker.ExecuteReader(queryString))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        if (!result.ContainsKey(id))
                        {
                            if (isCurrentVersion)
                            {
                                bool pageIsCurrentVersion = reader.GetBoolean(5);
                                if (!pageIsCurrentVersion)
                                {
                                    continue;
                                }
                                else
                                {
                                    string dirName = reader.GetString(1);
                                    string leafName = reader.GetString(2);
                                    string pageUrl = string.Format("{0}/{1}", dirName, leafName);
                                    result.Add(id, pageUrl);
                                }
                            }
                            else// last pulbished version ,maybe is current version
                            {
                                string pageUIVersionString = reader.GetString(4);
                                if (pageUIVersionString.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
                                {
                                    string dirName = reader.GetString(1);
                                    string leafName = reader.GetString(2);
                                    if (!reader.GetBoolean(5))
                                    {
                                        leafName += "?PageVersion=" + reader.GetInt32(3);

                                    }
                                    string pageUrl = string.Format("{0}/{1}", dirName, leafName);
                                    result.Add(id, pageUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                logger.Error("Get All Page Internal Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error("Get All Page Internal Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e.Message, e);
            }
            return result;
        }

        /// <summary>
        /// 递归调用，可能存在效率问题，需要考虑是否可以使用
        /// public Dictionary<Guid, string> GetAllPage(Guid siteId, string parentUrl)
        /// 这个方法替代
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public List<string> GetAllPage(Guid siteId, Guid parentId)
        {
            List<string> result = new List<string>();
            List<Guid> ids = new List<Guid>();

            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"
select Id,DirName,LeafName,Type from AllDocs with(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and IsCurrentVersion=1 and (ExtensionForFile ='aspx' or Type=1)";
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@ParentId", parentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            int type = Convert.ToInt32(reader[3]);
                            if (type == 1)
                            {
                                ids.Add(reader.GetGuid(0));
                            }
                            else
                            {
                                result.Add(string.Format("{0}/{1}", reader.GetString(1), reader.GetString(2)));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("Get All Page Error:{0}. Reason:{1}.", e.Message, e);
                        }
                    }
                }
            }

            foreach (Guid subId in ids)
            {
                result.AddRange(GetAllPage(siteId, subId));
            }
            return result;
        }

        /// <summary>
        /// 效率考虑，暂无API实现，可以用户获取list或folder下的所有page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentUrl">example /sites/de/subEnglish/Shared Documents,如果传空值或者"/",则会返回site collection下所有的Page</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetAllPage(Guid siteId, string parentUrl)
        {
            Dictionary<Guid, string> result = new Dictionary<Guid, string>();

            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    string parent = string.Format("{0}%", parentUrl.Trim('/'));
                    cmd.CommandText = AveDiscoverQueryString.GetAllPagesUnderFolder_Select_AllDocs;
                    cmd.Parameters.AddWithValue("@parentUrl", parent);
                    cmd.Parameters.AddWithValue("@SiteId", siteId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid id = reader.GetGuid(0);
                            //去重
                            if (!result.ContainsKey(id))
                            {
                                result.Add(id, string.Format("{0}/{1}", reader.GetString(1), reader.GetString(2)));
                            }
                        }

                    }
                }
            }
            catch (SqlException e)
            {
                logger.Error("Get All Page Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error("Get All Page Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e.Message, e);
            }
            return result;
        }

        /// <summary>
        /// 为site添加webpart TODO: improve it in different way.
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webPartKey"></param>
        /// <param name="webpartNameTemp"></param>
        /// <returns></returns>
        public IAveQueryDataReader WebAddWebPartMessageHandler(Guid siteId, string webPartKey, string webpartNameTemp)
        {
            string cmdText = @"SELECT ISNULL(nvarchar8,''),ISNULL(nvarchar9,''),ISNULL(nvarchar11,'') FROM AllUserData WITH (NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ContentType=@WebPartKey AND nvarchar7=@WebPartName";
            try
            {
                using (SqlCommand mCommand = mQueryWorker.CreateCommand())
                {
                    mCommand.CommandText = cmdText;
                    mCommand.Parameters.AddWithValue("@SiteId", siteId);
                    mCommand.Parameters.AddWithValue("@WebPartKey", webPartKey);
                    mCommand.Parameters.AddWithValue("@WebPartName", webpartNameTemp);
                    return new AveQueryDataReader(mCommand.ExecuteReader());
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取某一scope下的重复文件信息，无API实现
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="webIds"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="fileNamePattern"></param>
        /// <param name="includeFileExtensions"></param>
        /// <param name="requestedListTemplate"></param>
        /// <returns></returns>
        public IAveQueryDataReader SearchDuplicateFiles(List<string> siteIds, List<string> webIds, List<string> excludeFileNames, string fileNamePattern, List<string> includeFileExtensions, bool searchFile, bool searchAttachment)
        {
            string cmdText = GetDuplicateFileQuery(siteIds, webIds, searchFile, searchAttachment, includeFileExtensions, excludeFileNames, fileNamePattern);
            try
            {
                using (SqlCommand mCommand = mQueryWorker.CreateCommand())
                {
                    mCommand.CommandText = cmdText;
                    return new AveQueryDataReader(mCommand.ExecuteReader());
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }


        //select two views (docs, lists)
        /// <summary>
        /// TODO: improve it in different way.
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="webIds"></param>
        /// <param name="searchFile"></param>
        /// <param name="searchAttachment"></param>
        /// <param name="includeFileExtensions"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="fileNamePattern"></param>
        /// <returns></returns>
        private string GetDuplicateFileQuery(List<string> siteIds, List<string> webIds, bool searchFile, bool searchAttachment, List<string> includeFileExtensions, List<string> excludeFileNames, string fileNamePattern)
        {
            string text = @"SELECT D.LeafName as leafName, D.DirName as dirName, D.SiteId as siteId, D.WebId as webId, 
            D.ListId as listId, D.Id as docId, L.tp_BaseType as listType, D.UIVersionString as versionStr, IsNull(D.Size, 0) as fileSize, D.[TimeLastModified] as modifiedTime,
            S.HostHeader as hostHeader 
            FROM Docs D WITH(NOLOCK)
            inner join Lists L with (nolock) on D.listid = L.tp_id  and D.webId = L.tp_webId 
            inner join Sites S with (nolock) on D.SiteId = S.Id 
            where D.ListId is not null AND D.Size is not null AND D.IsCurrentVersion = 1 AND D.Type = 0 AND (L.tp_Flags & 256 = 0)";
            string siteFilter = GetCondByCommaSeparatedList(siteIds);
            if (!string.IsNullOrEmpty(siteFilter))
            {
                text = text + " and D.SiteId in (" + siteFilter + ")";
            }
            string webFilter = GetCondByCommaSeparatedList(webIds);
            if (!string.IsNullOrEmpty(webFilter))
            {
                text = text + " and D.WebId in (" + webFilter + ")";
            }
            if (searchFile && !searchAttachment)
            {
                text += " and (L.tp_BaseType = 1)";
            }
            else if (!searchFile && searchAttachment)
            {
                text += " and not (L.tp_BaseType in (1,4))";
            }
            string condByCommaSeparatedList = GetCondByCommaSeparatedList(includeFileExtensions);
            if (!string.IsNullOrEmpty(condByCommaSeparatedList))
            {
                text = text + " and (IsNull(d.Extension,'') in (" + condByCommaSeparatedList.ToLower(CultureInfo.InvariantCulture) + "))";
            }

            GetExcludeFileNamesCond(excludeFileNames, ref text);
            if (!string.IsNullOrEmpty(fileNamePattern) && fileNamePattern.Trim() != string.Empty)
            {
                text = text + " and (d.LeafName like N'%" + fileNamePattern.Trim() + "%')";
            }
            return text;
        }
        private void GetExcludeFileNamesCond(List<string> excludeFileNames, ref string sSQL)
        {
            List<string> clearList = new List<string>();
            List<string> fuzzyList = new List<string>();
            foreach (string excludeName in excludeFileNames)
            {
                if (excludeName.IndexOf("*", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    AddIfNotExists(ref fuzzyList, excludeName);
                }
                else
                {
                    AddIfNotExists(ref clearList, excludeName);
                }
            }
            if (fuzzyList.Count > 0)
            {
                foreach (string current in fuzzyList)
                {
                    if ((current.StartsWith("*", StringComparison.OrdinalIgnoreCase) || current.EndsWith("*", StringComparison.OrdinalIgnoreCase)) && current.IndexOf("**", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        string currentStr = current.Replace("*", "%");
                        sSQL = sSQL + " and (not (lower(d.LeafName) like '" + currentStr.ToLower(CultureInfo.InvariantCulture) + "'))";
                    }
                }
            }
            if (clearList.Count > 0)
            {
                string text = string.Empty;
                foreach (string current2 in clearList)
                {
                    text += string.Format("N'{0}',", current2.Trim().ToLower(CultureInfo.InvariantCulture));
                }
                text = text.Trim(',');
                if (!string.IsNullOrEmpty(text))
                {
                    sSQL = sSQL + " and (not (lower(d.LeafName) in (" + text + ")))";
                }
            }
        }

        private string GetCondByCommaSeparatedList(List<string> includeFileExtensions)
        {
            string text = string.Empty;
            foreach (string name in includeFileExtensions)
            {
                text += string.Format("'{0}',", name);
            }
            return text.Trim(',');
        }

        private void AddIfNotExists(ref List<string> filenames, string value)
        {
            string item = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            if (filenames != null && !filenames.Contains(item))
            {
                filenames.Add(item);
            }
        }
        #endregion

        #region others

        /// <summary>
        /// 获得所有connector stub的大小,无API实现. TODO: improve it in different way.
        /// </summary>
        /// <returns></returns>
        public ulong GetConnectorDataSize()
        {
            ulong size = 0;
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = @"
                with RBS_doc(id)
                as(
                select Id from DocStreams(nolock) where Content is null and RbsId is not null
                )
                SELECT coalesce(sum(cast(coalesce(size,0) as bigint)),0) as size FROM dbo.AllDocs(nolock) 
                WHERE ((DocFlags&65536)=65536 or id in (select * from RBS_doc with(nolock) )) AND type = 0";
                    size = ulong.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return size;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (mQueryWorker != null)
            {
                mQueryWorker.Dispose();
                mQueryWorker = null;
            }
        }

        #endregion

        #region Migration

        public bool CheckDatabaseServerRole(string userName, ServerRole sRole, byte[] sid)
        {
            bool checkStatus = false;
            try
            {
                string commandText = @"
                                    SELECT * FROM sys.server_role_members rm WITH(NOLOCK)
                                    JOIN sys.server_principals Roles WITH(NOLOCK)
                                    ON rm.role_principal_id = Roles.principal_id
                                    JOIN sys.server_principals Logins WITH(NOLOCK)
                                    ON rm.member_principal_id = Logins.principal_id
                                    WHERE Roles.type='R' AND (Logins.type='U' OR Logins.type = 'G')
                                    AND Roles.name = @RoleName
                                    AND (Logins.name IN ('@LoginNames') OR Logins.sid = @Sid)";
                mQueryWorker.AddParameter("@RoleName", sRole.ToString());
                commandText = commandText.Replace("@LoginNames", userName);
                mQueryWorker.AddParameter("@Sid", sid);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
                {
                    if (reader.Read())
                    {
                        checkStatus = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error happened when check database server role.Reason:{0}.", ex.ToString());
            }
            return checkStatus;
        }

        public bool CheckDatabaseRole(string logins, DatabaseRole dbRole, byte[] sid)
        {
            bool checkStatus = false;
            try
            {
                if (logins.IndexOf(',') < 0)
                {
                    string commandText = @"
                                    SELECT IS_ROLEMEMBER( @RoleName, (SELECT sys.sysusers.name 
                                    FROM sys.sysusers WITH(NOLOCK)
                                    INNER JOIN master.sys.syslogins WITH(NOLOCK) ON sys.sysusers.sid = syslogins.sid
                                    WHERE loginname  IN ('@LoginName')))";
                    mQueryWorker.AddParameter("@RoleName", dbRole.ToString());
                    commandText = commandText.Replace("@LoginName", logins);
                    mQueryWorker.AddParameter("@Sid", sid); // no use
                    checkStatus = (int)mQueryWorker.ExecuteScalar(commandText) == 1;
                }
                else
                {
                    var groupNames = new List<String>();
                    string commandTextForSearchLoginName = @"
                                    SELECT sys.sysusers.name
                                    FROM sys.sysusers WITH(NOLOCK)
                                    INNER JOIN master.sys.syslogins WITH(NOLOCK) ON sys.sysusers.sid = syslogins.sid
                                    WHERE loginname  IN ('@LoginNames')";
                    commandTextForSearchLoginName = commandTextForSearchLoginName.Replace("@LoginNames", logins);
                    mQueryWorker.AddParameter("@Sid", sid);
                    using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandTextForSearchLoginName))
                    {

                        while (reader.Read())
                        {
                            groupNames.Add(Convert.ToString(reader[0]));
                        }
                    }
                    string commandText = @"SELECT IS_ROLEMEMBER(@RoleName,@UserName)";
                    mQueryWorker.AddParameter("@RoleName", dbRole.ToString());
                    foreach (String groupName in groupNames)
                    {
                        mQueryWorker.AddParameter("@UserName", groupName);
                        if ((int)mQueryWorker.ExecuteScalar(commandText) == 1)
                        {
                            checkStatus = true;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error happened when check database role.Reason:{0}.", ex.ToString());
            }
            return checkStatus;
        }

        #endregion

        #region GA+

        public Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo()
        {
            return mQuerySessionSchema.GetSitesStorageInfo();
        }

        public void DeleteOrphanSiteInDB(IAveContentDatabase dataBase, string itemId)
        {
            RecycleOrphanSiteInDB(dataBase, itemId);
        }

        #endregion

        private enum DataType
        {
            None,
            Content,
            Stub,
        }
    }

    internal enum SPSVersion
    {
        None = 0,
        RTM = 1,
        SP1 = 2
    }
}

