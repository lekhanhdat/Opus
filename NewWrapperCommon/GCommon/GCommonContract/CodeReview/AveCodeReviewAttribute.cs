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



namespace AvePoint.GCommon.Contract.CodeReview
{
    #region using directives
    using System;
    using System.Diagnostics;
    #endregion

    [Conditional("DEBUG")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AveCodeReviewAttribute : Attribute
    {
        public string Date { get; set; }
        public string ReviewerEmail { get; set; }
        public string IssueOwnerEmail { get; set; }
        public string[] CheckList { get; set; }
        public string JiraId { get; set; }
        public bool IssueFixed { get; set; }
        public string[] IssueList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date">YYYY/MM/DD: 表示此次Code Review的具体日期</param>
        /// <param name="reviewerEmail">Email Address, e.g. Alex.Wang@avepoint.com: 表示此次Code Review的Reviewer</param>
        /// <param name="issueOwnerEmail">Email Address, e.g. Alex.Wang@avepoint.com: 表示解决此次Code Review中所发现问题的人</param>
        /// <param name="checkList">refer to the class of "CodeReviewConstants": 表示该类在此次Code Review中都检查了Check List中的哪些Check Point</param>
        /// <param name="jiraId">ADO-XXXXX: 用于标记此次Code Review是否发现了问题 (有JIRA ID代表发现了问题, 没有JIRA ID代表没有发现问题), 如果发现了, 则在JIRA中记录详细的问题列表 (类名, 问题检查点). 如果此次Code Review没有发现任何问题, 该项请设置成null. 另外, IITS会为大家创建每个Code Review周期的JIRA, 如果不知道具体JIRA ID的可以随时联系IITS Coordinator.</param>
        /// <param name="issueFixed">true/false: 如果此次Code Review发现了问题, 且该问题没有解决, 那么该项设置成False, 待该问题解决后设置成True. 如果此次Code Review未发现问题, 那么该项设置成True即可.</param>
        /// <param name="issueList">refer to the class of "CodeReviewConstants": 表示该类在此次Code Review中都检查出了Check List中的哪些Check Point. 如果此次Code Review没有发现任何问题, 该项请设置成null</param>
        public AveCodeReviewAttribute(string date, string reviewerEmail, string issueOwnerEmail, string[] checkList, string jiraId, bool issueFixed, string[] issueList)
            : this(date, reviewerEmail, issueOwnerEmail, checkList, jiraId, issueFixed)
        {
            this.IssueList = issueList;
        }

        /// <summary>
        /// 该构造方法已经废弃, 建议请使用最新的带有7个参数的构造方法
        /// </summary>
        [Obsolete]
        public AveCodeReviewAttribute(string date, string reviewerEmail, string issueOwnerEmail, string[] checkList, string jiraId, bool issueFixed)
        {
            this.Date = date;
            this.ReviewerEmail = reviewerEmail;
            this.IssueOwnerEmail = issueOwnerEmail;
            this.CheckList = checkList;
            this.JiraId = jiraId;
            this.IssueFixed = issueFixed;
        }

    }
}
