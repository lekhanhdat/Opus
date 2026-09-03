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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMLocalNodeDao : BaseDao<RMLocalNode>, IRMLocalNodeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMLocalNodeDao));

        private const string TABLE_NAME = "RMLocalNodes";


        public void BatchAdd(IEnumerable<OnPremiseSPLocalNode> nodes)
        {
            ThrowUtil.ThrowIfNull(nodes, "nodes");
            if (nodes.Count() == 0)
            {
                return;
            }
            logger.Debug("Add Local Nodes Total: {0}", nodes.Count());
            using (new PerformanceScope("Batch Add Local Nodes"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(nodes))
                {
                    table.TableName = tableName;
                    BatchAdd(table, tableName);
                }
            }
        }

        public int DeleteNodesByIDs(IEnumerable<string> ids)
        {
            ThrowUtil.ThrowIfNull(ids, nameof(ids));
            int result = 0;
            DatabaseUtility.BatchOperation<string>(ids, (batchIds) =>
            {
                ExecuteWithRetry(context =>
                {
                    List<SqlParameter> paras = null;
                    string sql = $"Delete From {GetFullTableName()} Where Id in {DatabaseUtility.BuildInClause(batchIds, out paras)}";
                    result += context.Database.ExecuteSqlCommand(sql, paras.ToArray());
                });
            });
            return result;
        }

        public int UpdateNodes(IEnumerable<OnPremiseSPLocalNode> nodes)
        {
            var idAndNodeMap = nodes.ToDictionary(n => n.Id);
            int result = 0;
            DatabaseUtility.BatchOperation<string>(idAndNodeMap.Keys, (batchIDs) =>
            {
                batchIDs = DatabaseUtility.EscapeSqlParam(batchIDs);
                ExecuteWithRetry(context =>
                {
                    foreach (var domain in context.RMLocalNodes.Where(m => batchIDs.Contains(m.Id)))
                    {
                        CopyToDoamin(idAndNodeMap[domain.Id], domain);
                    }
                    result += context.SaveChanges();
                });
            });
            return result;
        }

        public List<SPTreeNodeDto> GetAllNodes()
        {
            return FindAll().Select(ConvertToDto).ToList();
        }


        public Task<List<RMLocalNode>> GetAllNodesByLevelAsync(NodeLevel level)
        {

            return FindListAsync(item => item.NodeLevel == (int)level);
        }

        public async Task<List<OnPremiseSPLocalNode>> GetPageNodesByParentIdAsync(int pageIndex, int total, string parentId)
        {
             return ((await FindPageListWithOrderAsync(pageIndex, total, true, item => item.Url, item => item.ParentId == parentId)).Item1)
                .ConvertAll(ConvertToGlobaLocalNode);
        }

        public RMLocalNode GetById(string id)
        {
            return Find(item => item.Id == id);
        }

        public RMLocalNode GetByUrl(string url)
        {
            return Find(item => item.Url == url);
        }

        public Task<List<RMLocalNode>> GetByIdsAsync(List<string> ids)
        {
            return FindListAsync(item => ids.Contains(item.Id));
        }

        public Task<List<RMLocalNode>> GetByParentIdAsync(string parentId)
        {
            return FindListAsync(item => item.ParentId == parentId);
        }

        public Task<List<RMLocalNode>> GetSitesByFarmIdAsync(string farmId)
        {
            return FindListAsync(item => item.FarmId == farmId && (int)NodeLevel.SiteCollection == item.NodeLevel);
        }
        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[{TABLE_NAME}]";
        }

        public int SyncCount() 
        {
            return CountAll();
        }

        #region Convertor
        private SPTreeNodeDto ConvertToDto(RMLocalNode domain)
        {
            var spVersion = 0;
            int.TryParse(domain.SPVersion, out spVersion);

            return new SPTreeNodeDto()
            {
                ID = domain.Id,
                SPObjectId = domain.ObjectId,
                ParentId = domain.ParentId,
                FarmID = domain.FarmId,
                Name = domain.Name,
                Description = domain.Description,
                Url = domain.Url,
                Level = (NodeLevel)domain.NodeLevel,
                SPVersion = spVersion,
                Type = (NodeType)domain.SiteCollectionType,
                SPType = SPType.Moss
            };
        }



        private void CopyToDoamin(OnPremiseSPLocalNode nodeDto, RMLocalNode domain)
        {
            //domain.ObjectId = nodeDto.SPObjectId;
            //domain.ParentId = nodeDto.ParentId;
            //domain.FarmId = nodeDto.FarmID;
            domain.Name = nodeDto.Name;
            domain.Description = nodeDto.Description;
            domain.Url = nodeDto.Url;
            domain.NodeLevel = nodeDto.NodeLevel;
            domain.SPVersion = nodeDto.SPVersion.ToString();
            domain.SiteCollectionType = nodeDto.SiteCollectionType;
            domain.ModifiedDate = DateTime.UtcNow.Ticks;
        }

        private OnPremiseSPLocalNode ConvertToGlobaLocalNode(RMLocalNode domain)
        {
            return new OnPremiseSPLocalNode
            {
                Id = domain.Id,
                ObjectId = domain.ObjectId,
                ParentId = domain.ParentId,
                FarmId = domain.FarmId,
                Name = domain.Name,
                Description = domain.Description,
                Url = domain.Url,
                NodeLevel = domain.NodeLevel,
                SPVersion = domain.SPVersion,
                SiteCollectionType = domain.SiteCollectionType
            };
        }

        private DataTable ConvertToDataTable(IEnumerable<OnPremiseSPLocalNode> nodes)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("ObjectId", typeof(String));
            table.Columns.Add("ParentId", typeof(String));
            table.Columns.Add("FarmId", typeof(String));
            table.Columns.Add("Url", typeof(String));
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("Description", typeof(String));
            table.Columns.Add("NodeLevel", typeof(Int32));
            table.Columns.Add("SiteCollectionType", typeof(Int32));
            table.Columns.Add("SPVersion", typeof(String));
            table.Columns.Add("CreateTime", typeof(Int64));
            table.Columns.Add("ModifiedDate", typeof(Int64));
            foreach (var node in nodes)
            {
                var row = table.NewRow();
                row["Id"] = node.Id;
                row["ObjectId"] = node.ObjectId;
                row["ParentId"] = node.ParentId;
                row["FarmId"] = node.FarmId;
                row["Url"] = node.Url;
                row["Name"] = node.Name;
                row["Description"] = node.Description;
                row["NodeLevel"] = node.NodeLevel;
                row["SiteCollectionType"] = node.SiteCollectionType;
                row["SPVersion"] = node.SPVersion.ToString();
                row["CreateTime"] = DateTime.UtcNow.Ticks;
                row["ModifiedDate"] = DateTime.UtcNow.Ticks;
                table.Rows.Add(row);
            }

            return table;
        }

        

        #endregion
    }
}
