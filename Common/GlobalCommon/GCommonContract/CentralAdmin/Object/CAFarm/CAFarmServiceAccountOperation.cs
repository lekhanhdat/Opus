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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmServiceAccountOperation : CAOperation
    {
        [DataMember]
        public List<ServiceComponent> Components { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceComponent : IComparable<ServiceComponent>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string CurrentAccount { get; set; }

        [DataMember]
        public List<string> AvailableAccounts { get; set; }

        [DataMember]
        public ComponentType TypeOfComponent { get; set; }

        #region IComparable<ServiceComponent> Members

        public int CompareTo(ServiceComponent other)
        {         
            if (other == null) return 1;         
            if (string.Compare(this.TypeOfComponent.ToString(), other.TypeOfComponent.ToString(), StringComparison.Ordinal) == 0)
            {
                return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            }
            else 
            {
                if (this.TypeOfComponent.ToString().Length > other.TypeOfComponent.ToString().Length)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ComponentType
    {

        [EnumMember]
        SelectOne,

        [EnumMember]
        Farm,

        [EnumMember]
        WindowsService,

        [EnumMember]
        WebApplicationPool,

        [EnumMember]
        ServiceApplicationPool

    }
}
