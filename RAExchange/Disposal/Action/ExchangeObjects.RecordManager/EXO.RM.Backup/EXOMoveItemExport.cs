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
using AvePoint.RA.Common;
using Microsoft.Exchange.WebServices.Data;
using RAExportCommon;
using System;
using System.Threading.Tasks;
using ExchangeBackupUtility.Graph;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    public class EXOMoveItemExport : IDisposable
    {      
        public string ExportEXOItem(string jobid, Item EXOItem, ExchangeService service)
        {
            using (var performance = new PerformanceScope("EXOMoveItemExport.ExportEXOItem", "", true))
            {
                return ExchangeUtils.GetEXOItemLocalMSGFilePath(jobid, EXOItem.Id.ToString(), service);
            }
        }       
        
        public async Task<string> ExportEXOItem(string jobid, IExchangeItem item)
        {
            using var performance = new PerformanceScope("EXOMoveItemExport.ExportEXOItem", "", true);
            return await ExchangeUtils.GetEXOItemLocalMSGFilePath(jobid, item);
        }       

        public void Dispose()
        {
        }
    }
}
