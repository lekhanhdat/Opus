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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSMAzureCommon
{
    public class AzureQueueMessage
    {
       
        public string Event { get; set; }
        
        public string JobId { get; set; }
       
        public string Time { get; set; }
       
        public string SiteId { get; set; }
      
        public string WebId { get; set; }
       
        public string DBId { get; set; }
       
        public string FarmId { get; set; }
        
        public string ServerId { get; set; }
        
        public string CorrelationId { get; set; }
       
        public string FilesCreated { get; set; }
        
        public string BytesProcessed { get; set; }
        
        public string TotalObjects { get; set; }
       
        public string TotalErrors { get; set; }
        
        public string TotalWarnings { get; set; }

        public string LastObjectId { get; set; }

        public string ObjectType { get; set; }

        public string Url { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string FileName { get; set; }

        public string IV { get; set; }

        public string Content { get; set; }

        public string ErrorCode { get; set; }
    }
}
