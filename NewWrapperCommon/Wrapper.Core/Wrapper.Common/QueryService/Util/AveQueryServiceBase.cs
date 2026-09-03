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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Common
{
    internal abstract class AveQueryServiceBase : IAveQueryService
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveQueryServiceBase));
        

        protected AveQueryWorker mQueryWorker;

        //private static Dictionary<string, SPSVersion> mSchemaTable = new Dictionary<string, SPSVersion>();

        protected void ExceptionHandlingScope(string performanceScope, Action run)
        {
            if (string.IsNullOrEmpty(performanceScope)) throw new ArgumentNullException("performanceScope");
            if (run == null) throw new ArgumentNullException("run");
            using (new AvePerformanceScope(performanceScope))
            {
                try
                {
                    run();
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
        }


        internal virtual void InitQuerySession(object param)
        {
            var connString = string.Empty;
            if (param is string)
            {
                connString = param.ToString();
            }
            else
            {
                connString = (string)AveAssemblyUtility.GetPropertyValue(param, "DatabaseConnectionString");
            }
            if (mQueryWorker == null)
            {
                mQueryWorker = new AveQueryWorker();
            }
            mQueryWorker.Open(connString);
            if (WrapperConfiguration.IsMonitorEnable && mQueryWorker.Command != null)
            {
                AveQueryMonitor.RegisterConnection(mQueryWorker);
            }
        }

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

        [System.Diagnostics.Conditional("DEBUG")]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        internal class QueryReviewAttribute : Attribute
        {
            public string Date { get; set; }
            public string ReviewerFullName { get; set; }
            public bool Changed { get; set; }
            public string Comment { get; set; }
            public string CustomReviewId { set; get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="date">YYYY/MM/DD: 表示此次Code Review的具体日期</param>
            /// <param name="reviewerFullName">Full Name, e.g. Sid You或者Xihe You, 不要写xhyou: 表示此次Code Review的Reviewer</param>
            /// <param name="changed">是否发现问题并更改</param>
            /// <param name="comment">Comment</param>
            public QueryReviewAttribute(string date, string reviewerFullName, bool changed, string comment)
            {
                this.Date = date;
                this.ReviewerFullName = reviewerFullName;
                this.Changed = changed;
                this.Comment = comment;
            }



            public QueryReviewAttribute(string customReviewId)
            {
                this.CustomReviewId = customReviewId;
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="date">YYYY/MM/DD: 表示此次Code Review的具体日期</param>
            /// <param name="reviewerFullName">Full Name, e.g. Sid You或者Xihe You, 不要写xhyou: 表示此次Code Review的Reviewer</param>
            public QueryReviewAttribute(string date, string reviewerFullName)
                : this(date, reviewerFullName, false, null) { }
        }
    }
}
