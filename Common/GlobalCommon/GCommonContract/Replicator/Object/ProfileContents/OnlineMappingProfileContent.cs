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



namespace AvePoint.GCommon.Contract.Replicator.Object.ProfileContents
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public sealed class OnlineMappingProfileContent : MappingProfileContentBase
    {
        [DataMember]
        public string FilterPolicyId { get; set; }

        [DataMember]
        public string UserMappingId { get; set; }

        [DataMember]
        public string DomainMappingId { get; set; }

        [DataMember]
        public string ColumnMappingId { get; set; }

        [DataMember]
        public string LanguageMappingId { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        [DataMember]
        public string NetworkControlId { get; set; }

        [DataMember]
        public int CompressionLevel { get; set; }

        [DataMember]
        public bool NeedCompressed { get; set; }

        [DataMember]
        public string SecurityProfileId { get; set; }

        [DataMember]
        public bool NeedEncrypted { get; set; }

        [DataMember]
        public bool BackupBeforeReplication { get; set; }

        [DataMember]
        public int ConcurrentThreads { get; set; }

        [DataMember]
        public bool EnableByteLevel { get; set; }

        [DataMember]
        public string ConflictionSubProfileId { get; set; }

        [DataMember]
        public bool IsCloseIRMSettings { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [DataMember]
        public bool IsUseOldIRMSetting { get; set; }

        [DataMember]
        public RestoreThreadOption RestoreThreadOption { get; set; }

        [DataMember]
        public SkipHiddenList SkipHiddenList { get; set; }
    }
}
