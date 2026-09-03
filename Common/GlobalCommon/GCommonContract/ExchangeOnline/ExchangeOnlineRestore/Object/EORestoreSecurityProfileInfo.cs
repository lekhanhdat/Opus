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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EORestoreSecurityProfileInfo
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public DataEncryptionProfile SecurityProfile { get; set; }

        /// <summary> false时，没找到对应Job的Security profile，即SecurityProfile为Null,需要用户Import Security file. </summary>
        [DataMember]
        public bool HasSecurityProfile { get; set; }

        [DataMember]
        public string SecurityProfileName { get; set; }

        [DataMember]
        public string SecurityProfileGUID { get; set; }
    }
}
