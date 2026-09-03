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




namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.CommonFilter;
    using Cloud.Sdk.Data.Aos;

    #endregion using directives

    #region Code Review

    [AveCodeReview(
    "2012/7/25",
    "dwxue@avepoint.com",
    "jbli@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion Code Review

    public abstract class AdvancedSearchServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IAdvancedSearchService
        where TParameter : class, IAdvancedSearchInfo, new()
        where TResult : class, IAdvancedSearchResult, new()
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public Boolean HasExceedLimitCount { get; set; }

        public String FarmName { get; set; }

        public String FarmId { get; set; }

        public List<TreeNode> ListModeResult { get; set; }

        public List<TreeNode> ResultNodes { get; set; }

        public List<TreeNode> FinallyResultTree { get; set; }
        public GranularAdvancedSearchInfo SearchInfo { get; set; }

        public List<TreeNode> Search(IAdvancedSearchInfo searchInfo)
        {
            var info = searchInfo as TParameter;
            return this.InternalSearch(info, null);
        }

        private List<TreeNode> InternalSearch(TParameter searchInfo, ArchiverRestoreOrderBy orderBy)
        {
            List<TreeNode> result = new List<TreeNode>();
            try
            {
                result = Search(searchInfo, orderBy);
            }
            catch (Exception e)
            {
                this.ProcessException(e);
                throw;
            }
            return result;
        }

        public abstract List<TreeNode> Search(TParameter searchInfo, ArchiverRestoreOrderBy orderBy);

        public abstract void ProcessException(Exception e);

        public abstract void Dispose();
    }
}