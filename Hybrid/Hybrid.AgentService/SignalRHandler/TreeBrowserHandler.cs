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
using AvePoint.Hybrid.AgentService.ServiceEndpoint;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.CommonUtil;
using HybirdProxy.EndpointHandler;
using System.Reflection;

namespace AvePoint.Hybrid.AgentService.Handler
{
    public class TreeBrowserHandler : EndpointHandlerBase<STreeBrowserExecute, TreeBrowserArgs, BrowserResult>
    {

        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override BrowserResult Process(TreeBrowserArgs param)
        {

            logger.Info("Receive message, browse url address " + param.RootDir + ", type : " + param.Type);

            BrowserResult result =null;
            var browserService = (IEPBrowserService)WindsorManager.GetService("AvePoint.Hybrid.AgentService.ServiceEndpoint.EPBrowserService", typeof(IEPBrowserService));
            if (param.Type == (int)TreeBrowserType.Validation)
            {
                result = browserService.Validate(param);
            }
            else if (param.Type == (int)TreeBrowserType.Browser)
            {
                result = browserService.ListNode(param);
            }

            logger.Info("Finish to process broser request.  " + result.Result + ", message : " + result.Message);

            return result;
        }
    }
}
