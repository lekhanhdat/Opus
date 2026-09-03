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





using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmConfigureManagedAccountsOperation : CAOperation
    {
        [DataMember]
        public List<CAManagedAccount> ManagedAccounts { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAManagedAccount 
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public bool AutomaticChange { get; set; }

        [DataMember]
        public bool CanChangePassword { get; set; }

        [DataMember]
        public CASPSchedule ChangeSchedule { get; set; }

        [DataMember]
        public string ChangeScheduleText { get; set; }

        [DataMember]
        public long PasswordLastChange { get; set; }

        [DataMember]
        public long PasswordNextChange { get; set; }

        [DataMember]
        public int DaysBeforeChangeToEmail { get; set; }

        [DataMember]
        public int DaysBeforeExpiryToChange { get; set; }

        [DataMember]
        public bool EnableEmailBeforePasswordChange { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int MinPasswordLen { get; set; }

        [DataMember]
        public long PasswordExpiration { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Result { get; set; }

        #region for Update

        [DataMember]
        public bool ChangePassword { get; set; }

        [DataMember]
        public bool GeneratePassword { get; set; }

        [DataMember]
        public bool NewPassword { get; set; }

        [DataMember]
        public bool ExistingPassword { get; set; }

        #endregion
    }
}
