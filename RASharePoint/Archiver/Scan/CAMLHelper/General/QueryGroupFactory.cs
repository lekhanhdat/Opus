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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Global;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AvePoint.RA.SharePoint.Archiver.CAMLHelper
{
    public class QueryOptions
    {
        public List<Rule> Rules { get; set; }
        public IAveList List { get; set; }
        public IAveFieldCollection Fields { get; set; }
        //public string RMColumnName { get; set; }
        //public int WssId { get; set; }
        public DateTime DueStart { get; set; }
        public DateTime DueEnd { get; set; }
    }

    public class QueryGroupFactory
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RuleItemCollection mCheckerCollections;
        private IAveFieldCollection mFields;
        private DateTime mTimePoint;
        private QueryGroup mCamlGroup = new QueryGroup();
        private QueryGroupFactoryType mGroupFactoryType;



        private RegionalSettings mRegionalSettings;
        private IAveTimeZone mSPWebTimeZone;
        private IAveList mList;
        private string mRMColumnName;
        private List<int> mWssIds;


        public QueryGroupFactory(
            QueryGroupFactoryType groupFactoryType,
            RuleItemCollection collection,
            IAveFieldCollection listFields,
            DateTime timePoint)
        {
            this.mGroupFactoryType = groupFactoryType;
            this.mCheckerCollections = collection;
            this.mFields = listFields;
            this.mTimePoint = timePoint;
        }


        public QueryGroupFactory(
            QueryGroupFactoryType groupFactoryType,
            RuleItemCollection collection,
            IAveFieldCollection listFields,
            IAveTimeZone timeZone,
            RegionalSettings regionalSettings,
            DateTime timePoint,
            string internalFieldName,
            List<int> wssIds)
        {
            this.mGroupFactoryType = groupFactoryType;
            this.mCheckerCollections = collection;
            this.mSPWebTimeZone = timeZone;
            this.mRegionalSettings = regionalSettings;
            this.mList = listFields.List;
            this.mFields = listFields;
            this.mTimePoint = timePoint;
            this.mRMColumnName = internalFieldName;
            this.mWssIds = wssIds;
        }

        public QueryGroupFactory(
            QueryGroupFactoryType groupFactoryType,
            RuleItemCollection collection,
            IAveFieldCollection listFields,
            IAveTimeZone timeZone,
            RegionalSettings regionalSettings,
            DateTime timePoint,
            string internalFieldName)
        {
            this.mGroupFactoryType = groupFactoryType;
            this.mCheckerCollections = collection;
            this.mSPWebTimeZone = timeZone;
            this.mRegionalSettings = regionalSettings;
            this.mList = listFields.List;
            this.mFields = listFields;
            this.mTimePoint = timePoint;
            this.mRMColumnName = internalFieldName;
        }

        public QueryGroup GetQueryGroupByRuleCheckerCollection(bool includeRecord = true)
        {
            //  string listBaseType = ((int)mList.BaseTemplate).ToString();
            if (mCheckerCollections != null && !mCheckerCollections.HasUnCamlQueryableCondition)
            {
                //accordWithParentListTypeIDCondition = true 代表有ParentListTypeID条件的Filter，并且Parent List符合条件
                //bool accordWithParentListTypeIDCondition = false;
                bool needAddCommonCondition = false;
                foreach (var checker in mCheckerCollections.Rules)
                {
                    QueryGroup filtersGroup = new QueryGroup();
                    //filtersGroup.AddCondition(GetParentUrlCondition(parentUrl));
                    //nextNeedSkip = true, 代表之前有 不符合的ParentListTypeID条件 或者 条件中的Field在List里不存在 或者 其他查不到数据的条件
                    bool nextNeedSkip = false;
                    bool preCombineModeIsAnd = true;

                    foreach (var filter in checker.RuleFilters)
                    {
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
                    AddAdditionalCondition(includeRecord);
                }

            }
            else
            {
                //just for content due report
                mLog.Info("Has uncamlqueryable condition, will not use sp query to discover.");
                AddAdditionalCondition(includeRecord);
            }

            return mCamlGroup;
        }

        private void AddAdditionalCondition(bool includeRecord)
        {
            if (mGroupFactoryType == QueryGroupFactoryType.DisposalScan)
            {
                AddCommonQueryCondition();
            }
            if (mGroupFactoryType == QueryGroupFactoryType.DueReportScan)
            {
                AddCommonQueryCondition();
                if (!includeRecord)
                {
                    AddDeclaredQueryCondition();
                }
            }
        }

        private void AddCommonQueryCondition()
        {
            mCamlGroup.Conditions.Add(QueryConditionFactory.GetTaxonomyQueryCondition(mRMColumnName, mWssIds.ToArray(), Types.JoinTypes.And));
            if (mTimePoint != DateTime.MinValue)
            {
                mCamlGroup.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               SPBuiltInFieldName.CreatedTime,
               Types.FieldTypes.DateTime,
               Types.QueryTypes.Lt,
               ConvertCamlQueryDateTimeString(mTimePoint),
               true));
            }
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

        #region For Inherit parent term
        public QueryGroup GetQueryGroupByRuleCheckerCollection4UnClassification(bool includeRecord = true)
        {
            if (mCheckerCollections != null && !mCheckerCollections.HasUnCamlQueryableCondition)
            {
                bool needAddCommonCondition = false;
                foreach (var checker in mCheckerCollections.Rules)
                {
                    QueryGroup filtersGroup = new QueryGroup();
                    bool nextNeedSkip = false;
                    bool preCombineModeIsAnd = true;

                    foreach (var filter in checker.RuleFilters)
                    {
                        bool isAndCombinMode = filter.CombineMode == ArchiverFilterCombineMode.And;
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
                            if (!ProcessFilter(filter, out camlFilter))
                            {
                                if (preCombineModeIsAnd)
                                {
                                    filtersGroup.Conditions.Clear();
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
                    AddAdditionalIsNullCondition(includeRecord);
                }

            }
            else
            {
                //just for content due report
                mLog.Info("Has uncamlqueryable condition, will not use sp query to discover.");
                AddAdditionalIsNullCondition(includeRecord);
            }

            return mCamlGroup;
        }

        private void AddAdditionalIsNullCondition(bool includeRecord)
        {
            if (mGroupFactoryType == QueryGroupFactoryType.DisposalScan)
            {
                AddIsNullQueryCondition();
            }
            if (mGroupFactoryType == QueryGroupFactoryType.DueReportScan)
            {
                AddIsNullQueryCondition();
                if (!includeRecord)
                {
                    AddDeclaredQueryCondition();
                }
            }
        }

        private void AddIsNullQueryCondition()
        {
            mCamlGroup.Conditions.Add(QueryConditionFactory.GetTaxonomyQueryNullCondition(mRMColumnName, Types.JoinTypes.And));
            if (mTimePoint != DateTime.MinValue)
            {
                mCamlGroup.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               SPBuiltInFieldName.CreatedTime,
               Types.FieldTypes.DateTime,
               Types.QueryTypes.Lt,
               ConvertCamlQueryDateTimeString(mTimePoint),
               true));
            }
        }
        #endregion

        private bool ProcessFilter(ArchiverRuleFilter filter, out QueryCondition camlFilter)
        {
            bool result = true;
            camlFilter = new QueryCondition();
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
                            IAveField findedField = null;
                            for (int i = 0; i < mFields.Count; i++)
                            {
                                if (filter.RuleName.ToLower().Equals(mFields[i]?.Title?.ToLower()))
                                {
                                    findedField = mFields[i];
                                    break;
                                }
                            }

                            if (findedField == null)
                            {
                                throw new ArgumentException(string.Format("Field Not Exist {0}", filter.RuleName));
                            }
                            queryOption.Field = findedField.InternalName;
                        }
                        catch (Exception e)
                        {
                            mLog.Warn($"Process filter error: {e}");
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
                                long filesize;
                                if (!long.TryParse(filter.Value1, out filesize))
                                {
                                    throw new InvalidRuleValueException(filter.FilterCretia());
                                }
                                if (filter.Value1Unit == PolicyValueUnit.KB)
                                {
                                    filesize = filesize * 1024;
                                }
                                else if (filter.Value1Unit == PolicyValueUnit.MB)
                                {
                                    filesize = filesize * 1024 * 1024;
                                }
                                else if (filter.Value1Unit == PolicyValueUnit.GB)
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
                        case ArchiverFilterRuleType.LastAccessedTime:
                        case ArchiverFilterRuleType.LastActiveTime:
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
                        case ArchiverFilterRuleType.RetentionLabel:
                            queryOption.Field = SPColumnConstants.SP_ComplianceTag;
                            break;
                        case ArchiverFilterRuleType.SensitiveLabel:
                        case ArchiverFilterRuleType.SensitiveLabelFullName:
                            queryOption.Field = SPColumnConstants.Sensitive_Label_Display_Name;
                            break;
                        case ArchiverFilterRuleType.ParentLibraryName:
                        case ArchiverFilterRuleType.ParentLibraryBoolean:
                        case ArchiverFilterRuleType.ParentLibraryDateTime:
                        case ArchiverFilterRuleType.ParentLibraryNumber:
                        case ArchiverFilterRuleType.ParentLibraryText:
                        case ArchiverFilterRuleType.ParentSiteCollectionBoolean:
                        case ArchiverFilterRuleType.ParentSiteCollectionDateTime:
                        case ArchiverFilterRuleType.ParentSiteCollectionNumber:
                        case ArchiverFilterRuleType.ParentSiteCollectionText:
                        case ArchiverFilterRuleType.PropertyBagBoolean:
                        case ArchiverFilterRuleType.PropertyBagDateTime:
                        case ArchiverFilterRuleType.PropertyBagNumber:
                        case ArchiverFilterRuleType.PropertyBagText:
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
                        else if (filter.Value1.Equals("no", StringComparison.OrdinalIgnoreCase))
                        {
                            query.Value1 = "0";
                        }
                        else
                        {
                            query.QueryType = Types.QueryTypes.IsNull;
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

                    try
                    {
                        if (filter.Value1Unit == PolicyValueUnit.Days)
                        {
                            tempDt = mTimePoint.AddDays(-num);
                        }
                        else if (filter.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            tempDt = mTimePoint.AddDays(-num * 7);
                        }
                        else if (filter.Value1Unit == PolicyValueUnit.Months)
                        {
                            tempDt = mTimePoint.AddMonths(-num);
                        }
                        else if (filter.Value1Unit == PolicyValueUnit.Years)
                        {
                            tempDt = mTimePoint.AddYears(-num);
                        }
                        else
                        {
                            throw new InvalidRuleUnitException(filter.FilterCretia());
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        mLog.Warn($"The filter policy no.{filter.SequenceNo} of rule name: [{filter.RuleName}] has time value less than min datetime. Force using min datetime");
                        tempDt = DateTime.MinValue.AddDays(1); // avoid exception from converting to negative time zone
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    query.Value1 = ConvertCamlQueryDateTimeString(tempDt);
                    query.QueryType = Types.QueryTypes.Lt;
                    break;

                case ArchiverFilterCondition.ListIn:
                    query.QueryType = Types.QueryTypes.In;
                    query.FieldType = Types.FieldTypes.Text;
                    query.StringValues = filter.Value1.Split(";", StringSplitOptions.RemoveEmptyEntries);
                    break;
                case ArchiverFilterCondition.IsEmpty:
                    query.QueryType = Types.QueryTypes.IsNull;
                    query.FieldType = Types.FieldTypes.Text;
                    break;
                case ArchiverFilterCondition.IsNotEmpty:
                    query.QueryType = Types.QueryTypes.IsNotNull;
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

    public class APIDateTimeFormat
    {
        public const string DATETYPEForAPI003 = "yyyy-MM-dd HH:mm";
    }

    #region Custom Exception for Invalid Rule
    [Serializable]
    public class UnSupportRuleConditionException : Exception
    {
        public UnSupportRuleConditionException(string filterCretia)
            : base(string.Format("The rule condition type is not supported in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    [Serializable]
    public class UnSupportRuleTypeException : Exception
    {
        public UnSupportRuleTypeException(string filterCretia)
            : base(string.Format("The field is not supported in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    [Serializable]
    public class InvalidRuleUnitException : Exception
    {
        public InvalidRuleUnitException(string filterCretia)
            : base(string.Format("The Value unit is Invalid in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }

    [Serializable]
    public class InvalidRuleValueException : Exception
    {
        public InvalidRuleValueException(string filterCretia)
            : base(string.Format("The Value is Invalid in CAML query. FilterCretia: {0}.", filterCretia))
        {
        }
    }
    #endregion


    public enum QueryGroupFactoryType { 
    
        ArchiverScan = 0,
        DisposalScan = 1,
        DueReportScan = 2
    }
}
