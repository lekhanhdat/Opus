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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Import
{
    internal class WrapperRecord
    {
        //rec, isUpdate, generateNewId, template, detail
        public WrapperRecord(Record record, bool isUpdate, bool generateNewId, TemplateDto template, JMImportPhysicalRecordsJobDetail detail, int rowNumber, PhysicalRecordActionAudit actionAudit, string barcode = "")
        {
            this.Record = record;
            this.IsUpdate = isUpdate;
            this.GenerateNewId = generateNewId;
            this.TemplateDto = template;
            this.Detail = detail;
            this.RowNumber = rowNumber;
            this.Barcode = barcode;
            this.ActionAudit = actionAudit;
        }

        public Record Record { get; set; }
        public bool IsUpdate { get; set; }
        public bool GenerateNewId { get; set; }
        public TemplateDto TemplateDto { get; set; }
        public JMImportPhysicalRecordsJobDetail Detail { get; set; }
        public int RowNumber { get; set; }

        public string Barcode { get; set; }
        
        public PhysicalRecordActionAudit ActionAudit { get; set; }
    }
}
