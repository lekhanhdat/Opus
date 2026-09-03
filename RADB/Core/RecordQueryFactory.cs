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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    public static class RecordQueryFactory
    {
        #region collect data table
        /// <summary>
        /// add this for build start with query....
        /// </summary>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        internal static string GetFolderRowKeyContidion(string rowKey)
        {
            char lastChar = rowKey.TrimEnd('|').Last();
            Char afterLastChar = (char)(lastChar + 1);
            string tempKey = rowKey.Substring(0, rowKey.Length - 2);
            return tempKey + afterLastChar + "|";
        }
        internal static string CreatePartitionKeyQuery(string partitionKey)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
            return builder.ToString();
        }
        internal static string CreateInActiveSiteCollectionQuery(List<Guid> siteCollectionIds)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            foreach (var siteCollectionId in siteCollectionIds)
            {
                builder.AppendAndQuery("PartitionKey", AzureQueryComparisons.NotEqual, siteCollectionId.ToString(), AzureDataType.String);
            }
            return builder.ToString();
        }
        internal static string CreateGetAllPhysicalLibaryQuery()
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery("IsPhysicalLibrary", AzureQueryComparisons.Equal, 1, AzureDataType.Int);
            return builder.ToString();
        }
        internal static string CreateNonePhysicalQuery()
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery("IsPhysicalLibrary", AzureQueryComparisons.Equal, 0, AzureDataType.Int);
            return builder.ToString();
        }

        internal static string CreateRemoveFolderObjectQuery(Guid siteCollectionID, string rowkey, Guid listId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            var conditionKey = GetFolderRowKeyContidion(rowkey);
            var query = AzureTableQueryConditionBuilder.CreateTemperaryQuery("PartitionKey", AzureQueryComparisons.Equal, siteCollectionID.ToString(), AzureDataType.String);
            builder.AppendAndQuery("ListID", AzureQueryComparisons.Equal, listId, AzureDataType.Guid);
            builder.AppendAndQuery("RowKey", AzureQueryComparisons.GreaterThanOrEqual, rowkey);
            builder.AppendAndQuery("RowKey", AzureQueryComparisons.LessThan, conditionKey);
            var condition = AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), query);
            return condition;
        }
        internal static string CreateRemoveObjectQuery(Guid sitecollectionId, Guid webId, Guid listId, Guid folderId, int itemId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            //string ManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
            var query = AzureTableQueryConditionBuilder.CreateTemperaryQuery("PartitionKey", AzureQueryComparisons.Equal, sitecollectionId.ToString(), AzureDataType.String);
            if (webId != Guid.Empty)
            {
                builder.AppendAndQuery("WebID", AzureQueryComparisons.Equal, webId, AzureDataType.Guid);
            }
            if (listId != Guid.Empty)
            {
                builder.AppendAndQuery("ListID", AzureQueryComparisons.Equal, listId, AzureDataType.Guid);
            }
            if (folderId != Guid.Empty)
            {
                builder.AppendAndQuery("FolderID", AzureQueryComparisons.Equal, folderId, AzureDataType.Guid);
            }
            if (itemId != 0)
            {
                builder.AppendAndQuery("ItemRowId", AzureQueryComparisons.Equal, itemId, AzureDataType.Int);
            }
            var condition = AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), query);
            return condition;

        }
        #endregion
        #region site collection table
        #endregion
        #region Site Size

        internal static string CreateGetBCSDataObjectQuery()
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery("TermID", AzureQueryComparisons.NotEqual, Guid.Empty, AzureDataType.Guid);
            //builder.AppendAndQuery("IsInActive", AzureQueryComparisons.Equal, 0, AzureDataType.Int);
            return builder.ToString();
        }

        internal static string CreateGetSiteByIDQuery(Guid sitecollectionId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            //string ManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
            var query = AzureTableQueryConditionBuilder.CreateTemperaryQuery("PartitionKey", AzureQueryComparisons.Equal, sitecollectionId.ToString(), AzureDataType.String);
            builder.AppendAndQuery("NodeType", AzureQueryComparisons.Equal, (int)NodeLevel.SiteCollection, AzureDataType.Int);
            var condition = AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), query);
            return condition;
        }
        #endregion
    }
}
