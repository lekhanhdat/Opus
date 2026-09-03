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
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.CAML
{
    public class QueryGroup
    {
        private List<QueryGroup> mGroups;
        private List<QueryCondition> mConditions;
        private Types.JoinTypes mJoinType;

        public List<QueryGroup> Groups
        {
            get { return mGroups; }
            set { mGroups = value; }
        }

        public List<QueryCondition> Conditions
        {
            get { return mConditions; }
            set { mConditions = value; }
        }

        public Types.JoinTypes JoinType
        {
            get { return mJoinType; }
            set { mJoinType = value; }
        }

        /// <summary>
        /// 用于构造子QueryGroup的QueryGroup
        /// </summary>
        /// <param name="joinType">当前QueryGroup的JoinType</param>
        /// <param name="groups">为QueryGroup初始化子Group集合</param>
        /// <param name="conditions">为QueryGroup初始化子Conditions集合</param>
        public QueryGroup(Types.JoinTypes joinType = Types.JoinTypes.Or, List<QueryGroup> groups = null, List<QueryCondition> conditions = null)
        {
            mJoinType = joinType;

            if (groups == null)
            {
                mGroups = new List<QueryGroup>();
            }
            else
            {
                mGroups = groups;
            }

            if (conditions == null)
            {
                mConditions = new List<QueryCondition>();
            }
            else
            {
                mConditions = conditions;
            }
        }

        public void AddCondition(QueryCondition condition)
        {
            mConditions.Add(condition);
        }

        public void AddGroup(QueryGroup group)
        {
            if (group != null)
            {
                mGroups.Add(group);
            }
        }

        public string GetUnionCAML()
        {
            return GetUnionCAML(this);
        }

        private string GetUnionCAML(QueryGroup criteria)
        {
            Types.JoinTypes preJoinType = Types.JoinTypes.And;
            int groupCount, conditionCount;

            string groupsCaml = string.Empty;
            if (criteria.Groups != null && (groupCount = criteria.Groups.Count) > 0)
            {
                for (int ii = 0; ii < groupCount; ii++)
                {
                    var group = criteria.Groups[ii];
                    string caml = GetUnionCAML(group);
                    if (!string.IsNullOrEmpty(caml))
                    {
                        if (string.IsNullOrEmpty(groupsCaml))
                        {
                            groupsCaml = caml;
                        }
                        else
                        {
                            groupsCaml = string.Format("<{0}>{1}{2}</{0}>", preJoinType.ToString(), groupsCaml, caml);
                        }
                        preJoinType = group.JoinType;
                    }
                }
            }

            string conditionsCaml = string.Empty;
            if (criteria.Conditions != null && (conditionCount = criteria.Conditions.Count) > 0)
            {
                for (int ii = 0; ii < conditionCount; ii++)
                {
                    QueryCondition condition = criteria.Conditions[ii];
                    if (condition.Query != null)
                    {
                        string caml = condition.Query.GetCAML();
                        if (!string.IsNullOrEmpty(caml))
                        {
                            if (string.IsNullOrEmpty(conditionsCaml))
                            {
                                conditionsCaml = caml;
                            }
                            else
                            {
                                conditionsCaml = string.Format("<{0}>{1}{2}</{0}>", preJoinType.ToString(), conditionsCaml, caml);
                            }
                            preJoinType = condition.JoinType;
                        }
                    }
                }
            }

            string unionCaml = string.Empty;
            if (string.IsNullOrEmpty(conditionsCaml))
            {
                unionCaml = groupsCaml;
            }
            else if (!string.IsNullOrEmpty(groupsCaml))
            {
                unionCaml = string.Format("<{0}>{1}{2}</{0}>", preJoinType.ToString(), conditionsCaml, groupsCaml);
            }
            else
            {
                unionCaml = conditionsCaml;
            }

            return unionCaml;
        }

    }
}
