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




namespace AvePoint.Media.Service.DomainModel
{
    using System.Data.SQLite;
    #region using directives

    using System.Reflection;
    using System.Text;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Common;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    "ADO-44845",
    true)]

    #endregion CodeReview

    public abstract class SqlBuilderBase
        : ISqlBuilder
    {
        public StringBuilder Build(SqlBuildInfo info)
        {
            var level = info.FilterInfo.Level;
            return Invoker.CallMethod(this, "BuildQueryFor" + level.ToString(), info) as StringBuilder;
        }

        public StringBuilder Build(SqlBuildInfo info, NodeLevel level)
        {
            return Invoker.CallMethod(this, "BuildQueryFor" + level.ToString(), info) as StringBuilder;
        }

        public virtual StringBuilder BuildQueryForItem(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            // Extra parentheses are intentional due to many 'or' condition to ensure correct logic when additional AND conditions are appended. Do not remove.
            sql.Append(" from " + IndexConstants.TableNameGranularBody + " where ((COL_TYPE = 'I' or COL_TYPE = 'U' or COL_TYPE = 'A')");
            //if (filter.RuleType == FilterRuleType.Name || filter.RuleType == FilterRuleType.Title)
            //if (IsUseFullNameMatch(info))
            //{
                
            //}
            //else
            //{
                sql.Append($" and {GetItemTitle()} like @TEXT or (substr(COL_NAME,instr(COL_NAME, ':')+1) like @TEXT and COL_TYPE = 'A')" +
                    $" or substr(COL_NAME,1,instr(COL_NAME, ':')-1) in (select substr(COL_NAME,1,instr(COL_NAME, ':')-1) from {IndexConstants.TableNameGranularBody} where COL_TYPE='A') and (COL_PARENT_PATH_MD5,substr(COL_NAME,1,instr(COL_NAME, ':')-1)) in (select COL_PARENT_PATH_MD5,substr(COL_NAME,1,instr(COL_NAME, ':')-1) from {IndexConstants.TableNameGranularBody} where {GetItemTitle()} like @TEXT and COL_TYPE = 'I'))");
            //}
            //SQLiteFunction.RegisterFunction(typeof(ItemTitleFunction));
            //else if (filter.RuleType == FilterRuleType.Attribute)
            //    sql.Append(" and COL_ATTRIBUTES like @TEXT");
            return sql;
        }
        public StringBuilder BuildQueryForFS(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularBody + " where (COL_TYPE = 'D')");
            sql.Append($" and COL_NAME like @TEXT");
            return sql;
        }
        public virtual StringBuilder BuildQueryForDocument(SqlBuildInfo info)
        {
            return BuildQueryForDocAndVersion(info);
        }
        public virtual StringBuilder BuildQueryForDocumentVersion(SqlBuildInfo info)
        {
            return BuildQueryForDocAndVersion(info);
        }
        private StringBuilder BuildQueryForDocAndVersion(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularBody + " where (COL_TYPE = 'D' or COL_TYPE = 'V')");
            if (IsUseFullNameMatch(info))
            {
                sql.Append($" and (CASE WHEN instr(COL_NAME, ':') > 0 THEN substr(COL_NAME, 1, instr(COL_NAME, ':') - 1) ELSE COL_NAME END) = @TEXT");
            }
            else
            {
                sql.Append($" and (CASE WHEN instr(COL_NAME, ':') > 0 THEN substr(COL_NAME, 1, instr(COL_NAME, ':') - 1) ELSE COL_NAME END) like @TEXT");
            }
            //SQLiteFunction.RegisterFunction(typeof(DocumentNameFunction));
            return sql;
        }
        public virtual StringBuilder BuildQueryForAttachment(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularBody + " where COL_TYPE = 'A'");
            sql.Append(" and ATTACHMENTNAME(COL_NAME) like @TEXT");
            SQLiteFunction.RegisterFunction(typeof(AttachmentNameFunction));
            return sql;
        }

        public virtual StringBuilder BuildQueryForFolder(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'F'");
            //if (filter.RuleType == FilterRuleType.Name || filter.RuleType == FilterRuleType.Title)
                sql.Append($" and {GetContainerName()} like @TEXT");
            //SQLiteFunction.RegisterFunction(typeof(ContainerNameFunction));
            return sql;
        }

        public virtual StringBuilder BuildQueryForList(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'L'");
            //if (filter.RuleType == FilterRuleType.Name || filter.RuleType == FilterRuleType.Title)
                sql.Append($" and {GetContainerName()} like @TEXT");
            //SQLiteFunction.RegisterFunction(typeof(ContainerNameFunction));
            return sql;
        }

        public virtual StringBuilder BuildQueryForSite(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'W'");
            //if (filter.RuleType == FilterRuleType.Name || filter.RuleType == FilterRuleType.Title)
                sql.Append(" and substr(COL_ATTRIBUTES,instr(COL_ATTRIBUTES,':')+1,instr(COL_ATTRIBUTES,X'13')-instr(COL_ATTRIBUTES,':')-1) like @TEXT");
            //SQLiteFunction.RegisterFunction(typeof(SiteTitleFunction));
            //else if (filter.RuleType == FilterRuleType.Url)
            //    sql.Append(" and COMBINEURL(COL_SITE_URL,COL_NAME) like @TEXT");
            return sql;
        }
        public virtual StringBuilder BuildQueryForSiteCollection(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'E'");
            //if (filter.RuleType == FilterRuleType.Name || filter.RuleType == FilterRuleType.Title)
            sql.Append($" and {GetSiteCollectionName()} like @TEXT");
            //SQLiteFunction.RegisterFunction(typeof(ContainerNameFunction));
            //else if (filter.RuleType == FilterRuleType.Url)
            //    sql.Append(" and COMBINEURL(COL_SITE_URL,COL_NAME) like @TEXT");
            return sql;
        }
        #region google
        public virtual StringBuilder BuildQueryForGoogleDriveDocument(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append(" from " + IndexConstants.TableNameGDriveItem + " where (COL_TYPE = '20')");
            sql.Append($" and COL_NAME like @TEXT");
            return sql;
        }

        public virtual StringBuilder BuildQueryCountForGoogleDriveDocument(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append("Select count(*) from " + IndexConstants.TableNameGDriveItem + " where (COL_TYPE = '20')");
            sql.Append($" and COL_NAME like @TEXT");
            return sql;
        }

        #endregion
        private string GetSiteCollectionName()
        {
            return $"(CASE WHEN instr(COL_NAME, '\\') > 0 THEN substr(COL_NAME, length(COL_NAME) - instr(reverse(COL_NAME),'\\') + 1) WHEN instr(COL_NAME, '/') > 0 THEN substr(COL_NAME, length(COL_NAME) - instr(reverse(COL_NAME),'/') + 1) ELSE COL_NAME END)";
        }
        protected string GetContainerName()
        {
            string result= $"substr(COL_NAME, length(COL_NAME) -instr(reverse(COL_NAME),'\\')+2)";
            return result;
        }
        private string GetItemTitle()
        {
            return $"(CASE WHEN instr(COL_ATTRIBUTES, 'Title:') > 0 THEN substr(substr(COL_ATTRIBUTES, 7),1,instr(substr(COL_ATTRIBUTES, 7),X'13')) WHEN instr(COL_ATTRIBUTES, 'Title'+X'12') > 0 THEN substr(substr(COL_ATTRIBUTES, 'Title'+X'12'+7),1,instr(substr(COL_ATTRIBUTES, 'Title'+X'12'+7),X'13')-1) ELSE COL_ATTRIBUTES END)";
        }
        private bool IsUseFullNameMatch(SqlBuildInfo info)
        {
            var filter = info.FilterInfo;
            if (filter != null && !string.IsNullOrEmpty(filter.FilterName))
            {
                return filter.FilterName.StartsWith('\"') && filter.FilterName.EndsWith('\"');
            }
            return false;
        }

        public StringBuilder BuildQueryCount(SqlBuildInfo info)
        {
            var level = info.FilterInfo.Level;
            return Invoker.CallMethod(this, "BuildQueryCountFor" + level.ToString(), info) as StringBuilder;
        }

        public virtual StringBuilder BuildQueryCountForItem(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            // Extra parentheses are intentional due to many 'or' condition to ensure correct logic when additional AND conditions are appended. Do not remove.
            sql.Append("Select count(*)  from " + IndexConstants.TableNameGranularBody + " where ((COL_TYPE = 'I' or COL_TYPE = 'U' or COL_TYPE = 'A')");
            sql.Append($" and {GetItemTitle()} like @TEXT or (substr(COL_NAME,instr(COL_NAME, ':')+1) like @TEXT and COL_TYPE = 'A')" +
                $" or substr(COL_NAME,1,instr(COL_NAME, ':')-1) in (select substr(COL_NAME,1,instr(COL_NAME, ':')-1) from {IndexConstants.TableNameGranularBody} where COL_TYPE='A') and (COL_PARENT_PATH_MD5,substr(COL_NAME,1,instr(COL_NAME, ':')-1)) in (select COL_PARENT_PATH_MD5,substr(COL_NAME,1,instr(COL_NAME, ':')-1) from {IndexConstants.TableNameGranularBody} where {GetItemTitle()} like @TEXT and COL_TYPE = 'I'))");
            return sql;
        }

        public virtual StringBuilder BuildQueryCountForSiteCollection(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append(info.SelectSection);
            sql.Append("Select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'E'");
            sql.Append($" and {GetSiteCollectionName()} like @TEXT");
            return sql;
        }

        private StringBuilder BuildQueryCountForDocAndVersion(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append("Select count(*) from " + IndexConstants.TableNameGranularBody + " where (COL_TYPE = 'D' or COL_TYPE = 'V')");
            if (IsUseFullNameMatch(info))
            {
                sql.Append($" and (CASE WHEN instr(COL_NAME, ':') > 0 THEN substr(COL_NAME, 1, instr(COL_NAME, ':') - 1) ELSE COL_NAME END) = @TEXT");
            }
            else
            {
                sql.Append($" and (CASE WHEN instr(COL_NAME, ':') > 0 THEN substr(COL_NAME, 1, instr(COL_NAME, ':') - 1) ELSE COL_NAME END) like @TEXT");
            }
            return sql;
        }

        public virtual StringBuilder BuildQueryCountForDocument(SqlBuildInfo info)
        {
            return BuildQueryCountForDocAndVersion(info);
        }
        public virtual StringBuilder BuildQueryCountForDocumentVersion(SqlBuildInfo info)
        {
            return BuildQueryCountForDocAndVersion(info);
        }

        public virtual StringBuilder BuildQueryCountForFolder(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append("Select count(*) from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'F'");
            sql.Append($" and {GetContainerName()} like @TEXT");
            return sql;
        }
    
        public virtual StringBuilder BuildQueryCountForList(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append("Select count(*) from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'L'");
            sql.Append($" and {GetContainerName()} like @TEXT");
            return sql;
        }

        public virtual StringBuilder BuildQueryCountForSite(SqlBuildInfo info)
        {
            var sql = new StringBuilder();
            var filter = info.FilterInfo;
            sql.Append("Select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameGranularHead + " where COL_TYPE = 'W'");
            sql.Append(" and substr(COL_ATTRIBUTES,instr(COL_ATTRIBUTES,':')+1,instr(COL_ATTRIBUTES,X'13')-instr(COL_ATTRIBUTES,':')-1) like @TEXT");
            return sql;
        }

    }
}