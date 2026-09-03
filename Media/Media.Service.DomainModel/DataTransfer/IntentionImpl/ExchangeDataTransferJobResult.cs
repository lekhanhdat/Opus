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
    #region using directives

    using AvePoint.GCommon.Contract.Media.Object;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    public class ExchangeDataTransferJobResult
        : DataTransferJobResultBase,
        IDataTransferJobResult
    {
        public List<String> FinishedCycles { get; set; }

        public List<String> FailedCycles { get; set; }

        public ExchangeDataTransferResult ToExchangeDataTransferResult()
        {
            return new ExchangeDataTransferResult()
            {
                FinishedCycles = this.FinishedCycles,
                FailedCycles = this.FailedCycles,
                Message = this.Message,
            };
        }
    }
}
