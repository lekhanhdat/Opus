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
using AvePoint.RA.CommonUtil;
using Util.MSAzure;

namespace ExchangeUtility.Graph.Teams
{
    /// <summary>
    /// 对Teams messages进行处理的中间件抽象类
    /// </summary>
    public abstract class TeamsMessageMiddleware
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsMessageMiddleware));
        public TeamsMessageMiddleware Next { get; set; }

        public abstract void Invoke(TeamsMessageContext context);
        protected bool IsMatchRegion(string url, TeamsMessageContext context)
        {
            try
            {
                if (context.Environment is AzureEnvironment.Germany)
                    return url.ToLower().Contains("https://graph.microsoft.de");
                else if (context.IsGovernmentEnvironment)
                    return url.ToLower().Contains("https://graph.microsoft.us");
                else
                    return true;
            }
            catch (System.Exception ex)
            {
                logger.Warn("An exception occurs when checking if region match [{0}].", ex);
                return true;
            }
        }
    }
}