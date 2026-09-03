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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SharedServices.SearchService
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchServiceGeneralSettingsOperation : CAOperation
    {
        [DataMember]
        public List<CASearchServiceGeneralSetting> SearchServiceApplications { set; get; }

        public CASearchServiceGeneralSettingsOperation()
        {
            SearchServiceApplications = new List<CASearchServiceGeneralSetting>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchServiceGeneralSetting
    {
        [DataMember]
        public string ServiceApplicationId { get; set; }

        [DataMember]
        public string ServiceApplicationName { get; set; }

        [DataMember]
        public string DefaultAccessAccountName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public StatusSwitcher SearchAlertsStatus { get; set; }

        [DataMember]
        public StatusSwitcher QueryLoggingStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StatusSwitcher
    {
        [EnumMember]
        On,
        [EnumMember]
        Off,
    }
}
