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


namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    using AvePoint.GCommon.Contract.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRBackupLevel
    {
        [EnumMember]
        Database = 0,//不使用，由于media有使用，故暂不删除

        [EnumMember]
        [Description("Full Backup")]
        FullBackup = 1,

        [EnumMember]
        [Description("Incremental Backup")]
        IncrementalBackup = 2,

        [EnumMember]
        [Description("Differential Backup")]
        DifferentialBackup = 4,


        [EnumMember]
        [Description("None")]
        None = 8,//None为Database类型，由于0不能做Flag故改为8

        [EnumMember]
        [Description("Site Collection")]
        SiteCollection = 16,

        [EnumMember]
        Site = 32,

        [EnumMember]
        Folder = 64,

        [EnumMember]
        Item = 128,

        [EnumMember]
        [Description("Item Version")]
        ItemVersion = 256
    }
}
