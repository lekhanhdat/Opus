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






using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
namespace AvePoint.GCommon.Contract.Replicator.Object.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorAdvancedSetting
    {
        [DataMember]
        public bool Compression { get; set; }

        [DataMember]
        public int CompressionLevel { get; set; }

        [DataMember]
        public bool Encryption { get; set; }

        [DataMember]
        public string EncryptionId { get; set; }

        [DataMember]
        public int ConcurrentThreads { get; set; }

        [DataMember]
        public bool EnableByteLevel { get; set; }

        [DataMember]
        public string Notification { get; set; }

        [DataMember]
        public bool BackupSourceEnv { get; set; }

        [DataMember]
        public bool BackupDestEnv { get; set; }

        [DataMember]
        public bool DisableInformationRightsManagement { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [DataMember]
        public RestoreThreadOption RestoreThreadOption { get; set; }
        [DataMember]
        public bool SkipHiddenList { get; set; }
    }
}
