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
    #region directives
    using Merged18NResources.MediaServiceDomainModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    public class ExchangeStatisticsCalculateResult
        : StatisticsCalculateResultBase
        , IStatisticsCalculateResult
    {
        public Dictionary<NodeLevel, RestoreStatistics> ExchangeResultStatistics { get; set; }
        public override string ToString()
        {
            return string.Format(MediaServiceDomainModelResource.StatisticsCalculateResultBaseToStringResultInfos, Environment.NewLine,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineMailbox].TotalCount,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineMailbox].TotalSize,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineFolder].TotalCount,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineFolder].TotalSize,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineItem].TotalCount,
                 ExchangeResultStatistics[NodeLevel.ExchangeOnlineItem].TotalSize);
        }
    }

}
