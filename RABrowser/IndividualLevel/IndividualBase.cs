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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class IndividualBase : IDisposable
    {

        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveObjectModelFactory mObjectModel = null;
        private string mFarmId = string.Empty;
        private AveSqlConnection mSqlConn = new AveSqlConnection();
        protected string mSiteUrl = string.Empty;


        public IAveBrowserQuery Query { get; set; }

        public AveObjectModelFactory ObjectModel
        {
            get
            {
                return mObjectModel;
            }
        }

        public string FarmId
        {
            get
            {
                return mFarmId;
            }
        }

        public IndividualBase(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
        {
            mSiteUrl = siteUrl;
            mObjectModel = objectModel;
            if (!string.IsNullOrEmpty(sqlConnString))
            {
                var constr = new SqlConnectionStringBuilder(sqlConnString)
                {
                    Pooling = false
                };
                mSqlConn.Open(constr.ConnectionString);
            }

            using (Query = objectModel.CreateBrowserQuery(siteUrl)) { }
            mFarmId = AveEnv.AgentFarmId;
        }

        public virtual NodeExtensionDto FillNodeExtension(NodeExtensionDto extensionNode, object nodeDto)
        {
            return extensionNode;
        }

        public IAveSite GetSite(string url)
        {
            return ObjectModel.CreateSite(url);
        }

        public void Dispose()
        {
            if (mSqlConn != null)
            {
                mSqlConn.Dispose();
                mSqlConn = null;
            }
        }
    }

    internal class SPTreeNodeDtoComparer : IComparer<SPTreeNodeDto>
    {
        public int Compare(SPTreeNodeDto x, SPTreeNodeDto y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
        }
    }
}
