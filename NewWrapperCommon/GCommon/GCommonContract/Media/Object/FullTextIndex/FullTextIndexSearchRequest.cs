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



using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("Microsoft.Naming", "CA1712:DoNotPrefixEnumValuesWithTypeName", Scope = "type", Target = "AvePoint.GCommon.Contract.Media.Object.SearchSelection+ValueOperation")]
namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [KnownType("GetKnownTypes")]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexSearchRequest
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [DataMember]
        public String Keyword { get; set; }

        [DataMember]
        public SearchRequest SearchRequest { get; set; }

        [DataMember]
        public FullTextIndexJobType IndexType { get; set; }

        [DataMember]
        public String SortName { get; set; }

        [DataMember]
        public Boolean Reverse { get; set; }

        [DataMember]
        public Int32 OffSet { get; set; }

        [DataMember]
        public Int32 Length { get; set; }

        [DataMember]
        public string SearchId { get; set; }

        [DataMember]
        public bool IsFSArchiver { set; get; }
        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Full Text Index Search Request: ");
            stringBuilder.AppendFormat("Keyword: {0}, ", this.Keyword);
            stringBuilder.AppendFormat("Search Request: {0}, ", this.SearchRequest);
            stringBuilder.AppendFormat("Index Type: {0}, ", this.IndexType);
            stringBuilder.AppendFormat("Sort Name: {0}, ", this.SortName);
            stringBuilder.AppendFormat("Reverse: {0}", this.Reverse);
            stringBuilder.AppendFormat("Search ID:{0}", this.SearchId);
            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchRequest
    {
        [DataMember]
        public MatchingScopeType MatchingScope { get; set; }

        [DataMember]
        public DeleteIndexType DeleteIndex { get; set; }

        [DataMember]
        public List<IndexCrawlProfile> IndexProfiles { get; set; }

        [DataMember]
        public List<SearchScopes> SearchScopes { get; set; }

        [DataMember]
        public List<string> ScopeIds { get; set; }

        [DataMember]
        public List<SearchFilters> SearchFilters { get; set; }

        public SearchRequest()
        {
            IndexProfiles = new List<IndexCrawlProfile>();
            MatchingScope = MatchingScopeType.All;
            SearchScopes = new List<SearchScopes>();
            SearchFilters = new List<SearchFilters>();
        }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum MatchingScopeType
        {
            [EnumMember]
            MetaData = 1,
            [EnumMember]
            Content = 2,
            [EnumMember]
            All = MetaData | Content
        }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum DeleteIndexType
        {
            [EnumMember]
            Normal = 0,
            [EnumMember]
            Retention = 1,
        }

        public override String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("MatchingScope: ").Append(MatchingScope.ToString()).Append('\n');
            buf.Append("DeleteIndexType: ").Append(DeleteIndex.ToString()).Append('\n');
            buf.Append("IndexProfiles: ").Append(Arrays.ToString(this.IndexProfiles.ToArray())).Append('\n');
            buf.Append("SearchScopes: ").Append(Arrays.ToString(this.SearchScopes.ToArray())).Append('\n');
            buf.Append("SearchFilters: ").Append(Arrays.ToString(this.SearchFilters.ToArray())).Append('\n');
            return buf.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchFilters
    {
        [DataMember]
        public AveSharePointType SelectionSPType { get; set; }

        [DataMember]
        public List<SearchSelection> SelectionFilters { get; set; }

        public SearchFilters()
        {
            SelectionFilters = new List<SearchSelection>();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("SelectionSPType: ").Append(SelectionSPType.ToString()).Append(" ");
            buf.Append("SelectionFilter: ").Append(Arrays.ToString(this.SelectionFilters.ToArray())).Append('\n');
            return buf.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchScopes
    {
        [DataMember]
        public String SelectionFarmName { get; set; }

        [DataMember]
        public String SelectionFarmNameMD5 { get; set; }

        [DataMember]
        public List<SearchSelection> SelectionScopes { get; set; }

        public SearchScopes()
        {
            SelectionScopes = new List<SearchSelection>();
        }

        public override String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("SelectionFarmName: ").Append(SelectionFarmName).Append('\n');
            buf.Append("SelectionFarmNameMD5: ").Append(SelectionFarmNameMD5).Append('\n');
            buf.Append("SelectionScopes: ").Append(Arrays.ToString(this.SelectionScopes.ToArray())).Append('\n');
            return buf.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchSelection
    {
        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ValueType
        {
            [EnumMember]
            StringType,
            [EnumMember]
            TimeType,
            [EnumMember]
            NumberType,
            [EnumMember]
            ExpressionType
        }

        ///<summary>Sets how selections filter given multiple of selections within one field </summary>
        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ValueOperation
        {
            [EnumMember]
            ValueOperationOr,
            [EnumMember]
            ValueOperationAnd
        }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ConditionOperation
        {
            [EnumMember]
            Exactly,
            [EnumMember]
            Equals,
            [EnumMember]
            Match,
            [EnumMember]
            Contain,

            /// <summary>
            /// 大于等于 or (大于等于 and 小于)
            /// </summary>
            [EnumMember]
            GreaterOrEqualThan,
            /// <summary>
            /// 大于 or (大于 and 小于)
            /// </summary>
            [EnumMember]
            GreaterThan,
            /// <summary>
            /// 小于等于 or (大于等于 and 小于等于)
            /// </summary>
            [EnumMember]
            LessOrEqualThan,
            /// <summary>
            /// 小于 or (大于 and 小于)
            /// </summary>
            [EnumMember]
            LessThan,
            [EnumMember]
            FromTo,
            [EnumMember]
            Before,
            [EnumMember]
            After,
            [EnumMember]
            On,
            [EnumMember]
            WithIn,
            [EnumMember]
            OlderThan
        }

        [DataMember]
        public object RangeLowerValue { get; set; }

        [DataMember]
        public object RangeUpperValue { get; set; }

        [DataMember]
        public Int32 Order { get; set; }

        [DataMember]
        public String Value { get; set; }

        [DataMember]
        public String NotValue { get; set; }

        [DataMember]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "PrimitiveValue is unmodifiable as the cause of being referenced.")]
        public String PrimitiveValue { get; set; }

        ///<summary>Constructor </summary>
        ///<param name="fieldName"> field name </param>
        public SearchSelection(String fieldName)
        {
            FieldName = fieldName;

            SelectionValueTypeOperation = ValueType.StringType;
            SelectionOperation = ValueOperation.ValueOperationOr;
            SelectionConditionOperation = ConditionOperation.Equals;
        }

        [DataMember]
        public String FieldName { get; private set; }

        [DataMember]
        public ValueType SelectionValueTypeOperation { get; set; }

        [DataMember]
        public ValueOperation SelectionOperation { get; set; }

        [DataMember]
        public ConditionOperation SelectionConditionOperation { get; set; }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "PrimitiveValue is unmodifiable as the cause of being referenced.")]
        public override String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("name: ").Append(FieldName).Append(" ");
            buf.Append("value: " + Value).Append(" ");
            buf.Append("not: " + NotValue).Append(" ");
            buf.Append("lowerValue: " + RangeLowerValue).Append(" ");
            buf.Append("upperValue: " + RangeUpperValue).Append(" ");
            buf.Append("op: " + SelectionOperation).Append(" ");
            buf.Append("type:" + SelectionValueTypeOperation).Append(" ");
            buf.Append("condition:" + SelectionConditionOperation).Append(" ");
            buf.Append("order:" + Order).Append(" ");
            buf.Append("primitivevalue:" + PrimitiveValue);
            return buf.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverBackupSearchRequest : FullTextIndexSearchRequest
    {
        public override String ToString()
        {
            return base.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultBackupSearchRequest : ArchiverBackupSearchRequest
    {
        public override String ToString()
        {
            return base.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularBackupSearchRequest : FullTextIndexSearchRequest
    {
        [DataMember]
        public String FarmName { get; set; }

        public override String ToString()
        {
            return String.Format("Farm Name: {0}", this.FarmName);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IndexDeviceInfo
    {
        [DataMember]
        public String IndexPath { get; set; }

        [DataMember]
        public Int64 Size { get; set; }

        public override String ToString()
        {
            return String.Format("Index Path: {0}", this.IndexPath);
        }
    }
}