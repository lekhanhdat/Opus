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
using System.Text;
using Azure.Data.Tables;
using System.Globalization;

namespace AvePoint.RA.DB.Core
{
    public class AzureTableQueryConditionBuilder : IDisposable
    {
        private StringBuilder _QueryString = new StringBuilder();
        private bool _AppendOperationOption = false;

        private static string _PartitionKey = "PartitionKey";
        private static string _RowKey = "RowKey";
        public AzureTableQueryConditionBuilder()
        { }

        public AzureTableQueryConditionBuilder(string partitionKey)
        {
            AppendPartitionKey(partitionKey);
        }

        public AzureTableQueryConditionBuilder(string partitionKey, string rowKey)
        {
            AppendPartitionKey(partitionKey);
            AppendRowKey(rowKey);
        }

        public string AppendAndQuery(string propertyName, string operation, object propertyValue, AzureDataType type = AzureDataType.String)
        {
            string tempCondition = GenerateFilterCondition(propertyName, operation, propertyValue);
            if (!NeedAppendOperation())
            {
                ResetAppendOperationOption();
                _QueryString = new StringBuilder(tempCondition);
            }
            else
            {
                _QueryString = new StringBuilder(CombineFilters(_QueryString.ToString(), AzureTableOperators.And, tempCondition));
            }
            return _QueryString.ToString();
        }

        public string AppendOrQuery(string propertyName, string operation, object propertyValue, AzureDataType type = AzureDataType.String)
        {
            string tempCondition = GenerateFilterCondition(propertyName, operation, propertyValue);
            if (!NeedAppendOperation())
            {
                ResetAppendOperationOption();
                _QueryString = new StringBuilder(tempCondition);
            }
            else
            {
                _QueryString = new StringBuilder(CombineFilters(_QueryString.ToString(), AzureTableOperators.Or, tempCondition));
            }
            return _QueryString.ToString();
        }

        private string AppendPartitionKey(string partitionKey)
        {
            string partitionCondition = GenerateFilterCondition(_PartitionKey, AzureQueryComparisons.Equal, partitionKey);
            if (!NeedAppendOperation())
            {
                ResetAppendOperationOption();
                _QueryString = new StringBuilder(partitionCondition);
            }
            else
            {
                _QueryString = new StringBuilder(CombineFilters(_QueryString.ToString(), AzureTableOperators.And, partitionCondition));
            }
            return _QueryString.ToString();
        }

        private string AppendRowKey(string rowKey)
        {
            string rowKeyCondition = GenerateFilterCondition(_RowKey, AzureQueryComparisons.Equal, rowKey);
            if (!NeedAppendOperation())
            {
                ResetAppendOperationOption();
                _QueryString = new StringBuilder(rowKeyCondition);
            }
            else
            {
                _QueryString = new StringBuilder(CombineFilters(_QueryString.ToString(), AzureTableOperators.And, rowKeyCondition));
            }
            return _QueryString.ToString();
        }

        public static string CombineAndQueries(string query1, string qeury2)
        {
            return CombineFilters(query1, AzureTableOperators.And, qeury2);
        }

        public static string CombineOrQueries(string query1, string qeury2)
        {
            return CombineFilters(query1, AzureTableOperators.Or, qeury2);
        }

        public static string CreateTemperaryQuery(string propertyName, string operation, object propertyValue, AzureDataType type = AzureDataType.String)
        {
            return GenerateFilterCondition(propertyName, operation, propertyValue);
        }

        private void ResetAppendOperationOption()
        {
            _AppendOperationOption = true;
        }

        private bool NeedAppendOperation()
        {
            return _AppendOperationOption;
        }
        private static string GenerateFilterCondition(string propertyName, string operation, object propertyValue)
        {        
            if (null == propertyName || null == propertyValue)
            {
                throw new ArgumentNullException("propertyValue or propertyValue");
            }
            string text = null;
            var type = propertyValue.GetType();

            if (type == typeof(String))
            {
                var propertyValueString = propertyValue.ToString();
                text = string.Format(CultureInfo.InvariantCulture, "'{0}'", propertyValueString.Replace("'", "''"));
            }
            else if (type == typeof(Guid)) 
            {
                var propertyValueGuid = (Guid)propertyValue;
                var propertyValueString = propertyValueGuid.ToString();
                text = string.Format(CultureInfo.InvariantCulture, "guid'{0}'", propertyValueString);
            }
            else if(type == typeof(Boolean))
            {
                var propertyValueBool = (bool)propertyValue;
                text = propertyValueBool ? "true" : "false";
            }
            else if (type == typeof(DateTime))
            {
                DateTimeOffset propertyValueDate = (DateTime)propertyValue;
                var propertyValueForDate = propertyValueDate.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
                text = string.Format(CultureInfo.InvariantCulture, "datetime'{0}'", propertyValueForDate);
            }
            else if (type == typeof(Int32))
            {
                var propertyValueInt = (int)propertyValue;
                text = Convert.ToString(propertyValueInt, CultureInfo.InvariantCulture);
            }
            else if (type == typeof(Int64))
            {
                var propertyValueLong = (long)propertyValue;
                var propertyValueForLong = Convert.ToString(propertyValueLong, CultureInfo.InvariantCulture);
                text = string.Format(CultureInfo.InvariantCulture, "{0}L", propertyValueForLong);
            }
            else if (type == typeof(Double))
            {
                var propertyValueDouble = (double)propertyValue;
                var propertyValueForDouble = Convert.ToString(propertyValueDouble, CultureInfo.InvariantCulture);
                text = (int.TryParse(propertyValueForDouble, out var _) ? string.Format(CultureInfo.InvariantCulture, "{0}.0", propertyValueForDouble) : propertyValueForDouble);
            }
            else if (type == typeof(byte[]))
            {
                var propertyValueByte = (byte[])propertyValue;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in propertyValueByte)
                {
                    stringBuilder.AppendFormat("{0:x2}", b);
                }
                var propertyValueString = stringBuilder.ToString();
                text = string.Format(CultureInfo.InvariantCulture, "X'{0}'", propertyValueString);

            }

            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", propertyName, operation, text);
        }

        private static string CombineFilters(string filterA, string operatorString, string filterB)
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}) {1} ({2})", filterA, operatorString, filterB);
        }

        public override string ToString()
        {
            return _QueryString.ToString();
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_QueryString.ToString()))
            {
                _QueryString = null;
            }
        }
    }

    public static class AzureTableOperators
    {
        public const string And = "and";
        public const string Not = "not";
        public const string Or = "or";
    }

    public static class AzureQueryComparisons
    {
        public const string Equal = "eq";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "ge";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "le";
        public const string NotEqual = "ne";
    }

    public enum AzureDataType
    {
        Binary,
        Bool,
        Date,
        Double,
        Guid,
        Int,
        Long,
        String,
    }

    internal class ArchiverTableEntityProperty
    {
        internal static string ScanItemID = "ScanItemID";
        internal static string ScopeID = "ScopeID";
        internal static string ScanJobID = "ScanJobID";
        internal static string NodeID = "NodeID";
        internal static string ParentID = "ParentID";
        internal static string LeafName = "LeafName";
        internal static string Path = "Path";
        internal static string ScanTime = "ScanTime";
        internal static string UIVersion = "UIVersion";
        internal static string LibRowID = "LibRowID";
        internal static string NodeType = "NodeType";
        internal static string SPNodeLevel = "SPNodeLevel";
        internal static string CacheNodeType = "CacheNodeType";
        internal static string Level = "Level";
        internal static string ArchiveLevel = "ArchiveLevel";
        internal static string Status = "Status";
        internal static string RuleID = "RuleID";
        internal static string ExpireTime = "ExpireTime";
        internal static string LastModifiedTime = "LastModifiedTime";
        internal static string KeepDataStatus = "KeepDataStatus";
        internal static string Property = "Property";
        internal static string SourceFlag = "SourceFlag";
        internal static string ExportToRECO = "ExportToRECO";
        internal static string RowKey = "RowKey";
        internal static string PartitionKey = "PartitionKey";
        internal static string MailBoxGroupId = "MailBoxGroupId";
        internal static string MailBoxId = "MailBoxID";
        internal static string ArchivedTime = "ArchivedTime";
        internal static string MoveToApprovalTable = "MovedToApprovalTable";
        internal static string ListID = "ListId";
    }
}
