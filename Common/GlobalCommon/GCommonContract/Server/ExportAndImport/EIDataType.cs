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




namespace AvePoint.GCommon.Contract.Server.ExportAndImport
{
    #region == using directives ==
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EIDataType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        [Description("Granular Backup")]
        GranularBackup = 1,

        [EnumMember]
        [Description("Platform Recovery")]
        PlatformRecovery = 2,

        [EnumMember]
        [Description("Archiver Import Data")]
        Archive = 3,

        [EnumMember]
        [Description("Extender And Archiver Stub And Blob Data")]
        StubAndBlob = 4,

        [EnumMember]
        [Description("Connector Stub And Blob Data")]
        ConnectorStubAndBlob = 5,

        [EnumMember]
        [Description("Archiver Scan Data")]
        ArchiveVerify = 6,

        [EnumMember]
        [Description("Third Party Stub Data")]
        ThirdPartyStub = 7,

        [EnumMember]
        [Description("DocAve V6 EBS Stub Data")]
        ExtenderEBSStub = 8,

        [EnumMember]
        [Description("Solution Data")]
        SolutionData = 9
    }
}
