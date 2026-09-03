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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Explorer.Model;

namespace AvePoint.RA.RAExchange.Disposal.Action;

public class ExchangeGraphMoveItemUtil
{
    public bool KeepClassification { get; set; }
    
    public bool DeleteSourceItem { get; set; }
    
    public string ErrorMessage { get; set; } = string.Empty;
    
    public string DestUrl { get; set; } = string.Empty;
    
    public JobDetailsStatus Status { get; set; } = JobDetailsStatus.Successful;
    
    public bool Skip  { get; set; }
    
    public string ExportPath  { get; set; } = string.Empty;
    
    public string ItemName { get; set; } = string.Empty;
    
    public string MsgFileName { get; set; } = string.Empty;
    
    public string TermId { get; set; } = string.Empty;
    
    public Guid DesOldRecordId { get; set; } = Guid.Empty;

    public EXOMoveItemImport Importer = null;
    
    public Record DesRecord = null;
}