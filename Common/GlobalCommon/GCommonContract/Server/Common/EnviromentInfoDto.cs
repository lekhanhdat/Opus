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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EnviromentInfoDto
    {
        [DataMember]
        public EnviromentZoneType Zone { set; get; }

        [DataMember]
        public Dictionary<string, NavigationInfo> Navigations { set; get; }

        [DataMember]
        public AccountLanguageType SupportEmailLanguage { set; get; }

        [DataMember]
        public string TollFree { set; get; }

        [DataMember]
        public string SalesEmail { set; get; }

        [DataMember]
        public bool IsBuyNow { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NavigationInfo
    {
        [DataMember]
        public string USNav { set; get; }
        [DataMember]
        public string ChinaNav { set; get; }
        [DataMember]
        public string JPNav { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EnviromentZoneType
    {
        [EnumMember]
        Global,
        [EnumMember]
        China,
        [EnumMember]
        Gov,
    }
}
