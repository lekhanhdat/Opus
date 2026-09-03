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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Worker.Impl
{
    public class AdvanceSearchWorker: CommonBrowserWorker
    {

        private IAveSecurity mSecurity = null;

        public AdvanceSearchWorker(SharePointBrowserContract contractFromGUI, AveObjectModelFactory objectModel, BrowserType browserType)
            : base(contractFromGUI, objectModel, browserType)
        {
            mSecurity = mObjectModel.CreateSecurity();
        }

        internal override SharePointBrowserContract DispatchBrowseRequest()
        {
            base.DispatchBrowseRequest();

            var resultNodes = mContractFromGUI.ChildenNodes;
            if(mContractFromGUI.FilterPolicy != null)
            {
                var policies = mContractFromGUI.FilterPolicy.FItems.Select(filter => (AvePoint.GCommon.Contract.CommonFilter.FilterPolicy)filter).ToList();
                var expressions = mContractFromGUI.FilterPolicy.AndOrExpression;
                var filterEngine = new FilterEngine(policies, expressions);
                mContractFromGUI.ChildenNodes = resultNodes.Where((dto) =>
                {
                    try
                    {
                        return filterEngine.IsQualified(new TreeNodeInfo()
                        {
                            Name = dto.DisplayName,
                            Url = dto.Url
                        });
                    }
                    catch(Exception e)
                    {
                        if (e is PropertyNotAssignedException)
                        {
                            Logger.Error("A property was not assigned while filtering an advanced-search node. Exception:{0}", e.ToString());
                        }
                        return false;
                    }
                }).ToList();
            }

            return mContractFromGUI;
        }
    }
}
