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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityImportResult
    {
        [DataMember]
        public string guid { get; set; }

        //0 : success 
        //1 : same skip
        //2 : format Error 
        //3 : older
        //4 : Conflict
        //5 : newer
        //6 : insertError
        [DataMember]
        public int status { get; set; }

        [DataMember]
        public string fileName { get; set; }

        [DataMember]
        public string profileName { get; set; }

        [DataMember]
        public DataEncryptionProfile dataEncryptionProfile { get; set; }

        //用来保存数据库中的名字(在导入后改变过profile的name)
        [DataMember]
        public string profileNameInDB { get; set; }

        //user object info
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public string CreatedUserId { get; set; }

        [DataMember]
        public bool IsImport { get; set; }

        [DataMember]
        public Exception ValidatedException { get; set; }

        

    }

    public enum SecutityImportStatusEnum
    {
        Successful = 0,
        Skip = 1,
        FormatError = 2,
        Older = 3,
        Confilict = 4,
        Newer = 5,
        InserttError = 6
    }

}
