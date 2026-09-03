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
    public class AzureUploadSetting
    {
        public AzureLocationInfo AzureSetting { get; set; }

        public string ExportLocation { get; set; }

        public string SourceContainerName { get; set; }
      
        public string MainfestContainerName { get; set; }
      
        public string QueueContainName { get; set; }
     
        public int LifeTime { get; set; }
     
        public int BlobRequestOptionsClientTimeoutHour { get; set; }
    
        public int BlobRequestOptionsClientTimeoutMinute { get; set; }
  
        public int BlobRequestOptionsServerTimeoutHour { get; set; }
       
        public int BlobRequestOptionsServerTimeoutMinute { get; set; }

        public Guid SyncFileName { get; set; }

        public bool IsEncryption { get; set; }
    }

    public class AzureLocationInfo
    {

        public string AccessPoint { get; set; }

        public string AccountName { get; set; }

        public string AccountKey { get; set; }
    }

    public class AzureResult
    {
 
        public Boolean AzureIused { get; set; }

        public string AzureContainerSourceUri { get; set; }
   
        public string AzureContainerManifestUri { get; set; }
    
        public string AzureQueueReportUri { get; set; }
      
        public string AzureSourceContainerName { get; set; }
       
        public string AzureManifestContainerName { get; set; }
       
        public string AzureQueueReportContainerName { get; set; }
        
        public string ErrorMessage { get; set; }
    }

    public class AuzreDownLoadSetting
    {
  
        public AzureLocationInfo AzureSetting { get; set; }
        public string MainfestContainerName { get; set; }
        public int LifeTime { get; set; }
        public string ExportLocation { get; set; }
        public FileDownloadType FileDonwloadType { get; set; }
        public int BlobRequestOptionsClientTimeoutHour { get; set; }
        public int BlobRequestOptionsClientTimeoutMinute { get; set; }
        public int BlobRequestOptionsServerTimeoutHour { get; set; }
        public int BlobRequestOptionsServerTimeoutMinute { get; set; }
        public bool NeedDelete { get; set; }
        public bool IsEncryption { get; set; }


    }

    public enum FileDownloadType
    {
   
        None = 0,
        
        XML,
       
        Logs,
       
        Warn,
       
        Err
    }
}
