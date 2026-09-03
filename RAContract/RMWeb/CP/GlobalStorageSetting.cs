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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Contract.CodeView;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    [RACodeReview("Allen Yin")]
    public class GlobalStorageSetting
    {
        public int Id { get; set; }
        public List<StoragePolicy> AllStoragePolicy { get; set; }
        public List<ExportLocation> AllExportLocation { get; set; }
        public List<SecurityProfile> AllSecurityProfile { get; set; }
        //public List<ProcessingPool> AllProcessingPool { get; set; }

        public StoragePolicy CurrentStoragePolicy { get; set; }
        public bool CurStoragePolicyRemoved { get; set; }

        public ExportLocation CurrentExportLocation { get; set; }
        public bool CurExportLocationRemoved { get; set; }

        public SecurityProfile CurrentSecurityProfile { get; set; }
        public bool CurSecurityProfileRemoved { get; set; }

        //public ProcessingPool CurrentProcessingPool { get; set; }
        //public bool CurProcessingPoolRemoved { get; set; }

        public bool UseCompression { get; set; }
        public bool UseEncryption { get; set; } = true;
        public int CompressionSpeed { get; set; }
        public DataSecurity CompressionMethod { get; set; }
        public DataSecurity EncryptionMethod { get; set; }
        public GSSExceptionType GSSExceptionType { get; set; }
        public string ExceptionMsg { get; set; }
        public string Extentions { get; set; }
    }
    public class StoragePolicy
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class ExportLocation
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
    public class SecurityProfile
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
    //public class ProcessingPool
    //{
    //    public string ID { get; set; }
    //    public string Name { get; set; }
    //}
    public enum GSSExceptionType
    {
        DocAveConnFailed = 1
    }

    public enum StorageType
    {
        Amazon = 401,
        S3Compatible = 601,
        Wasabi = 602,
        Box = 9,
        Dropbox = 407,
        FTP = 1,
        AzureBlob = 403,
        NetApp_Alta_Vault = 510,
        Rackspace = 402,
        SFTP = 12,
    }
}
