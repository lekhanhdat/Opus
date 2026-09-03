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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRBackupJobReportDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string AttachementName { get; set; }
        /// <summary>
        ///  type == 0, attachement is in database.
        ///  type == 1, attachement is in file system.
        /// </summary>
        [DataMember]
        public int AttachementType { get; set; }
        [DataMember]
        public Byte[] Attachement { get; set; } // only defined if type=0
        [DataMember]
        public string AttachementLocation { get; set; }    // only defined if type==1
        [DataMember]
        public PRBackupJobDto Job { get; set; }
        [DataMember]
        public int Remark1 { get; set; }
        [DataMember]
        public long Remark2 { get; set; }//Long
        [DataMember]
        public long Remark3 { get; set; } //Long
        [DataMember]
        public string Remark4 { get; set; }
        [DataMember]
        public string Remark5 { get; set; }

    }
}
