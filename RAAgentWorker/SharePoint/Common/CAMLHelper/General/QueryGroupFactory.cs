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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Reflection;
using System.Text;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.General
{
    public class QueryOptions
    {
        public Hybrid.AgentContract.Rule.RMRuleItemCollection CheckerCollections { get; set; }
        public IAveList List { get; set; }
        public IAveFieldCollection Fields { get; set; }
        public string RMColumnName { get; set; }
        public int WssId { get; set; }
        public DateTime DueStart { get; set; }
        public DateTime DueEnd { get; set; }
    }

    public class QueryGroupFactory
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Hybrid.AgentContract.Rule.RMRuleItemCollection mCheckerCollections;
        private RegionalSettings mRegionalSettings;
        private IAveTimeZone mSPWebTimeZone;
        private IAveList mList;
        private IAveFieldCollection mFields;
        private string mRMColumnName;
        private int mWssId;
        private DateTime mTimePoint;
        private QueryGroup mCamlGroup = new QueryGroup();

        public QueryGroupFactory(
            Hybrid.AgentContract.Rule.RMRuleItemCollection collection,
            IAveFieldCollection listFields,
            IAveTimeZone timeZone,
            RegionalSettings regionalSettings,
            DateTime timePoint,
            string internalFieldName,
            int wssId)
        {
            this.mCheckerCollections = collection;
            this.mSPWebTimeZone = timeZone;
            this.mRegionalSettings = regionalSettings;
            this.mList = listFields.List;
            this.mFields = listFields;
            this.mTimePoint = timePoint;
            this.mRMColumnName = internalFieldName;
            this.mWssId = wssId;
        }

        public QueryGroup GetQueryGroupByRuleCheckerCollection(bool includeRecord = true)
        {
            //  string listBaseType = ((int)mList.BaseTemplate).ToString();
            if (!mCheckerCollections.HasUnCamlQueryableCondition)
            {
                //accordWithParentListTypeIDCondition = true 代表有ParentListTypeID条件的Filter，并且Parent List符合条件
                //bool accordWithParentListTypeIDCondition = false;
                bool needAddCommonCondition = false;
                foreach (var checker in mCheckerCollections.Rules)
                {
                    QueryGroup filtersGroup = new QueryGroup();
                    //nextNeedSkip = true, 代表之前有 不符合的ParentListTypeID条件 或者 条件中的Field在List里不存在 或者 其他查不到数据的条件
                    bool nextNeedSkip = false;
                    bool preCombineModeIsAnd = true;

                    foreach (var filter in checker.RuleFilters)
                    {
                        if (filter.RuleType == ArchiverFilterRuleType.LastAccessedTime)
                        {
                            needAddCommonCondition = true;
                            continue;
                        }
                        bool isAndCombinMode = filter.CombineMode == ArchiverFilterCombineMode.And;
                        //nextNeedSkip=true时要跳过，Filter的CombineMode是Or的话，下一个Rule Filter就不用再跳过了
                        if (nextNeedSkip)
                        {
                            if (!isAndCombinMode)
                            {
                                nextNeedSkip = false;
                            }
                        }
                        else
                        {
                            QueryCondition camlFilter;
                            #region old logic 
                            //ParentListTypeID是唯一一个在Item级别（及其以下），不用等Query或者Check Rule，
                            //直接用Parent List就可以判断是否符合Rule的ArchiverFilterRuleType
                            //if (filter.RuleType == ArchiverFilterRuleType.ParentListTypeID)
                            //{
                            //    bool isEqualsCondition = filter.Condition == ArchiverFilterCondition.Equals;
                            //    bool isEqualsListType = filter.Value1 == listBaseType;
                            //    //判断是否符合ParentListTypeID条件
                            //    if ((isEqualsListType && isEqualsCondition) || (!isEqualsListType && !isEqualsCondition))
                            //    {
                            //        accordWithParentListTypeIDCondition = true;
                            //    }
                            //    //preCombineModeIsAnd = false时，此Filter可以被忽略，直接跳过即可；
                            //    //preCombineModeIsAnd = true时，由于当前所有的Filter加在一起是查询不到结果的，要清空之前所有的条件
                            //    else if (preCombineModeIsAnd)
                            //    {
                            //        filtersGroup.Conditions.Clear();
                            //        if (isAndCombinMode)
                            //        {
                            //            nextNeedSkip = true;
                            //        }
                            //    }
                            //}
                            //else
                            #endregion
                            if (!ProcessFilter(filter, out camlFilter))
                            {
                                //preCombineModeIsAnd = false时，此Filter可以被忽略，直接跳过即可；
                                //preCombineModeIsAnd = true时，由于当前所有的Filter加在一起是查询不到结果的，要清空之前所有的条件
                                if (preCombineModeIsAnd)
                                {
                                    filtersGroup.Conditions.Clear();
                                    //由于当前所有的Filter加在一起是查询不到结果的，所以当前Filter的CombinMode是And时，
                                    //下一个Filter加上之前的Filters也是查询不到结果的，下一个Filter不用处理，需要跳过
                                    if (isAndCombinMode)
                                    {
                                        nextNeedSkip = true;
                                    }
                                }
                            }
                            else
                            {
                                filtersGroup.AddCondition(camlFilter);
                            }
                        }

                        preCombineModeIsAnd = isAndCombinMode;
                    }

                    if (filtersGroup.Conditions.Count > 0)
                    {
                        mCamlGroup.AddGroup(filtersGroup);
                    }
                }

                if (mCamlGroup.Groups.Count > 0 || needAddCommonCondition)
                {
                    AddCommonQueryCondition();
                    if (!includeRecord)
                    {
                        AddDeclaredQueryCondition();
                    }
                }
            }
            else
            {
                AddCommonQueryCondition();
                if (!includeRecord)
                {
                    AddDeclaredQueryCondition();
                }
            }

            return mCamlGroup;
        }

        private void AddCommonQueryCondition()
        {
            mCamlGroup.Conditions.Add(QueryConditionFactory.GetTaxonomyQueryCondition(mRMColumnName, mWssId, Types.JoinTypes.And));
            mCamlGroup.Conditions.Add(new QueryCondition(
                Types.JoinTypes.And,
                Types.FieldRefTypes.Name,
                SPBuiltInFieldName.CreatedTime,
                Types.FieldTypes.DateTime,
                Types.QueryTypes.Lt,
                ConvertCamlQueryDateTimeString(mTimePoint),
                true));
        }

        private void AddDeclaredQueryCondition()
        {
            mCamlGroup.Conditions.Add(new QueryCondition(
                   Types.JoinTypes.And,
                   Types.FieldRefTypes.Name,
                   "_vti_ItemDeclaredRecord",
                   Types.FieldTypes.DateTime,
                   Types.QueryTypes.IsNull,
                   null,
                   true));

        }

        private bool ProcessFilter(ArchiverRuleFilter filter, out CAML.QueryCondition camlFilter)
        {
            bool result = true;
            camlFilter = new CAML.QueryCondition();
            Query queryOption = camlFilter.Query;

            camlFilter.JoinType = filter.CombineMode == ArchiverFilterCombineMode.And ? Types.JoinTypes.And : Types.JoinTypes.Or;

            try
            {
                ProcessFilterCondition(filter, ref queryOption);

                if (filter.RuleType == ArchiverFilterRuleType.TextColumn || filter.RuleType == ArchiverFilterRuleType.NumberColumn ||
                    filter.RuleType == ArchiverFilterRuleType.BooleanColumn || filter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    filter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || filter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
                {
                    if (filter.RuleName.StartsWith("[") && filter.RuleName.EndsWith("]"))
                    {
                        queryOption.Field = filter.RuleName.TrimStart('[').TrimEnd(']');
                    }
                    else
                    {
                        try
                        {
                            queryOption.Field = mFields[filter.RuleName].InternalName;
                        }
                        catch (Exception)
                        {
                            throw new UnSupportRuleTypeException(filter.FilterCretia());
                        }
                    }
                }
                else
                {
                    switch (filter.RuleType)
                    {
                        case ArchiverFilterRuleType.Title:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.Title);
                            queryOption.Field = SPBuiltInFieldName.Title;
                            break;
                        case ArchiverFilterRuleType.Name:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.Name);
                            queryOption.Field = SPBuiltInFieldName.Name;
                            break;
                        case ArchiverFilterRuleType.DocumentSize:
                            if (result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.DocumentSize))
                            {
                                queryOption.Field = SPBuiltInFieldName.DocumentSize;
                                int filesize;
                                if (!int.TryParse(filter.Value1, out filesize))
                                {
                                    throw new InvalidRuleValueException(filter.FilterCretia());
                                }
                                if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.KB)
                                {
                                    filesize = filesize * 1024;
                                }
                                else if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.MB)
                                {
                                    filesize = filesize * 1024 * 1024;
                                }
                                else if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.GB)
                                {
                                    filesize = filesize * 1024 * 1024 * 1024;
                                }
                                else
                                {
                                    throw new InvalidRuleUnitException(filter.FilterCretia());
                                }
                                queryOption.Value1 = filesize.ToString();
                            }
                            break;
                        case ArchiverFilterRuleType.ModifiedTime:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.ModifiedTime);
                            queryOption.Field = SPBuiltInFieldName.ModifiedTime;
                            break;
                        case ArchiverFilterRuleType.CreatedTime:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.CreatedTime);
                            queryOption.Field = SPBuiltInFieldName.CreatedTime;
                            break;
                        case ArchiverFilterRuleType.CreatedBy:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.CreatedBy);
                            queryOption.Field = SPBuiltInFieldName.CreatedBy;
                            break;
                        case ArchiverFilterRuleType.ModifiedBy:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.ModifiedBy);
                            queryOption.Field = SPBuiltInFieldName.ModifiedBy;
                            break;
                        case ArchiverFilterRuleType.ContentType:
                            result = mFields.ContainsFieldWithStaticName(SPBuiltInFieldName.ContentType);
                            queryOption.Field = SPBuiltInFieldName.ContentType;
                            break;
                        case ArchiverFilterRuleType.LastAccessedTime:
                            break;
                        #region Temporarily does not support
                        //case ArchiverFilterRuleType.ParentListTypeID:
                        //    break;
                        //case ArchiverFilterRuleType.LastAccessedTime:
                        //    break;
                        //case ArchiverFilterRuleType.Title:
                        //    break;
                        //case ArchiverFilterRuleType.Size:
                        //    break;
                        //case ArchiverFilterRuleType.KeepTheLatestVersion:
                        //    break;
                        //case ArchiverFilterRuleType.URL:
                        //    break;
                        //case ArchiverFilterRuleType.TextCustomProperty:
                        //    break;
                        //case ArchiverFilterRuleType.NumberCustomProperty:
                        //    break;
                        //case ArchiverFilterRuleType.BooleanCustomProperty:
                        //    break;
                        //case ArchiverFilterRuleType.DateTimeCustomProperty:
                        //    break;
                        //case ArchiverFilterRuleType.PrimaryAdministrator:
                        //    break;
                        //case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
                        //    break;
                        //case ArchiverFilterRuleType.ConversationContent:
                        //    break;
                        //case ArchiverFilterRuleType.Participant:
                        //    break;
                        //case ArchiverFilterRuleType.PostedBy:
                        //    break;
                        //case ArchiverFilterRuleType.RepliedBy:
                        //    break;
                        //case ArchiverFilterRuleType.LikedBy:
                        //    break;
                        //case ArchiverFilterRuleType.MentionedName:
                        //    break;
                        //case ArchiverFilterRuleType.Hashtag:
                        //    break;
                        #endregion
                        default:
                            throw new UnSupportRuleTypeException(filter.FilterCretia());
                    }
                }


            }
            catch (UnSupportRuleConditionException usrce)
            {
                mLog.Warn("An UnSupportRuleConditionException when ProcessFilter.Message:{0}.", usrce.ToString());
                result = false;
            }
            catch (UnSupportRuleTypeException usrte)
            {
                mLog.Warn("An UnSupportRuleTypeException when ProcessFilter.Message:{0}.", usrte.ToString());
                result = false;
            }
            catch (InvalidRuleUnitException irue)
            {
                mLog.Warn("An InvalidRuleUnitException when ProcessFilter.Message:{0}.", irue.ToString());
                result = false;
            }
            catch (InvalidRuleValueException irve)
            {
                mLog.Warn("An InvalidRuleValueException when ProcessFilter.Message:{0}.", irve.ToString());
                result = false;
            }
            catch (Exception ex)
            {
                mLog.Warn("An Exception when ProcessFilter.Message:{0}.", ex.ToString());
                throw;
            }

            return result;
        }

        private void ProcessFilterCondition(ArchiverRuleFilter filter, ref Query query)
        {
            if (filter.RuleType == ArchiverFilterRuleType.NumberColumn)
            {
                query.FieldType = Types.FieldTypes.Number;
            }
            switch (filter.Condition)
            {
                case ArchiverFilterCondition.Contains:
                    query.QueryType = Types.QueryTypes.Contains;
                    query.Value1 = filter.Value1;
                    break;
                case ArchiverFilterCondition.Equals:
                case (ArchiverFilterCondition)PolicyCondition.Equals:
                    query.QueryType = Types.QueryTypes.Eq;
                    if (filter.RuleType == ArchiverFilterRuleType.BooleanColumn)
                    {
                        query.FieldType = Types.FieldTypes.YesNo;
                        if (filter.Value1.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            query.Value1 = "true";
                        }
                        else
                        {
                            query.Value1 = "false";
                        }
                    }
                    else
                    {
                        query.Value1 = filter.Value1;
                    }
                    break;
                case ArchiverFilterCondition.DoesNotEqual:
                    query.QueryType = Types.QueryTypes.Neq;
                    if (filter.RuleType == ArchiverFilterRuleType.BooleanColumn)
                    {
                        query.FieldType = Types.FieldTypes.YesNo;
                        if (filter.Value1.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            query.Value1 = "true";
                        }
                        else
                        {
                            query.Value1 = "false";
                        }
                    }
                    else
                    {
                        query.Value1 = filter.Value1;
                    }
                    break;
                case ArchiverFilterCondition.GreaterThanOrEqualTo:
                    query.QueryType = Types.QueryTypes.Geq;
                    query.Value1 = filter.Value1;
                    break;
                case ArchiverFilterCondition.LessThanOrEqualTo:
                    query.QueryType = Types.QueryTypes.Leq;
                    query.Value1 = filter.Value1;
                    break;
                case ArchiverFilterCondition.FromTo:
                    query.FieldType = Types.FieldTypes.DateTime;
                    query.IncludeTimeValue = true;
                    query.QueryType = Types.QueryTypes.FromTo_1_1;
                    var fromDt = ConvertUtcDateTime(filter.Value1);
                    var toDt = ConvertUtcDateTime(filter.Value2);
                    // [REC-738] remove timepoint ref FromTo/Before
                    //if (fromDt > mTimePoint)
                    //{
                    //    throw new InvalidRuleValueException(filter.FilterCretia());
                    //}
                    //if (toDt > mTimePoint)
                    //{
                    //    toDt = mTimePoint;
                    //}
                    query.Value1 = ConvertCamlQueryDateTimeString(fromDt);
                    query.Value2 = ConvertCamlQueryDateTimeString(toDt);
                    break;
                case ArchiverFilterCondition.Before:
                    query.FieldType = Types.FieldTypes.DateTime;
                    query.IncludeTimeValue = true;
                    var ltDt = ConvertUtcDateTime(filter.Value1);
                    // [REC-738] remove timepoint ref FromTo/Before
                    //if (ltDt >= mTimePoint)
                    //{
                    //    ltDt = mTimePoint;
                    //    query.QueryType = Types.QueryTypes.Leq;
                    //}
                    //else
                    //{
                    //    query.QueryType = Types.QueryTypes.Lt;
                    //}
                    query.QueryType = Types.QueryTypes.Lt;
                    query.Value1 = ConvertCamlQueryDateTimeString(ltDt);
                    break;
                case ArchiverFilterCondition.OlderThan:
                    query.FieldType = Types.FieldTypes.DateTime;
                    query.IncludeTimeValue = true;
                    int num;
                    DateTime tempDt = DateTime.UtcNow;
                    if (!int.TryParse(filter.Value1, out num))
                    {
                        throw new InvalidRuleValueException(filter.FilterCretia());
                    }

                    if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Days)
                    {
                        tempDt = mTimePoint.AddDays(-num);
                    }
                    else if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Weeks)
                    {
                        tempDt = mTimePoint.AddDays(-num * 7);
                    }
                    else if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Months)
                    {
                        tempDt = mTimePoint.AddMonths(-num);
                    }
                    else if (filter.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Years)
                    {
                        tempDt = mTimePoint.AddYears(-num);
                    }
                    else
                    {
                        throw new InvalidRuleUnitException(filter.FilterCretia());
                    }
                    query.Value1 = ConvertCamlQueryDateTimeString(tempDt);
                    query.QueryType = Types.QueryTypes.Lt;
                    break;
                case ArchiverFilterCondition.IsEmpty:
                    query.QueryType = Types.QueryTypes.IsNull;
                    query.FieldType = Types.FieldTypes.Text;
                    break;
                //case ArchiverFilterCondition.OlderThanNow:
                //    query.FieldType = Types.FieldTypes.DateTime;
                //    query.IncludeTimeValue = true;
                //    query.Value1 = ConvertCamlQueryDateTimeString(DateTime.UtcNow);
                //    query.QueryType = Types.QueryTypes.Lt;
                //    break;
                default:
                    throw new UnSupportRuleConditionException(filter.FilterCretia());
            }
            if (query.Value1.Contains("&"))
            {
                query.Value1 = query.Value1.Replace("&", "&amp;");
            }
            if (query.Value1.Contains("<"))
            {
                query.Value1 = query.Value1.Replace("<", "&lt;");
            }

        }

        /// <param name="dtString">(AveDateTimeUtility.DATETYPEForAPI003) Format: yyyy-MM-dd HH:mm</param>
        private DateTime ConvertUtcDateTime(string dtString)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(dtString, APIDateTimeFormat.DATETYPEForAPI003, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            {
                dt = DateTime.Parse(dtString);
            }
            return dt;
        }

        private string ConvertCamlQueryDateTimeString(DateTime utcDt)
        {
            if (mRegionalSettings != null)
            {
                var rusult = mRegionalSettings.TimeZone.UTCToLocalTime(DateTime.SpecifyKind(utcDt, DateTimeKind.Unspecified));
                mRegionalSettings.Context.ExecuteQuery();
                return CreateISO8601DateTimeFromSystemDateTime(rusult.Value);
                //return CreateISO8601DateTimeFromSystemDateTime(mSPWebTimeZone.UTCToLocalTime(utcDt));
            }
            else
            {
                //TimeZoneInfo spWebTimezone = TimeZoneInfo.FindSystemTimeZoneById(mSPWebTimeZone.Description);
                //var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDt, spWebTimezone);
                //return CreateISO8601DateTimeFromSystemDateTime(localTime);
                return CreateISO8601DateTimeFromSystemDateTime(utcDt);
            }
        }

        private string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dtValue.Year.ToString("0000"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Month.ToString("00"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Day.ToString("00"));
            stringBuilder.Append("T");
            stringBuilder.Append(dtValue.Hour.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Minute.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Second.ToString("00"));
            stringBuilder.Append("Z");
            return stringBuilder.ToString();
        }
    }



    #region Custom Exception for Invalid Rule
    public class UnSupportRuleConditionException : Exception
    {
        public UnSupportRuleConditionException(string filterCretia)
            : base(string.Format("The rule condition type is not supported in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    public class UnSupportRuleTypeException : Exception
    {
        public UnSupportRuleTypeException(string filterCretia)
            : base(string.Format("The field is not supported in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    public class InvalidRuleUnitException : Exception
    {
        public InvalidRuleUnitException(string filterCretia)
            : base(string.Format("The Value unit is Invalid in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    public class InvalidRuleValueException : Exception
    {
        public InvalidRuleValueException(string filterCretia)
            : base(string.Format("The Value is Invalid in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }
    #endregion
}
