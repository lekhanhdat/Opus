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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmAlternateAccessMappingOperation : CAOperation
    {
        [DataMember]
        public List<AlternateUrl> Collection { get; set; }
        [DataMember]
        public OperationType Operation { get; set; }
        [DataMember]
        public string ResourceName { get; set; }
        [DataMember]
        public string UrlProtocolHostAndPort { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ZoneOption
    {
        [EnumMember]
        Default,
        [EnumMember]
        Intranet,
        [EnumMember]
        Internet,
        [EnumMember]
        Custom,
        [EnumMember]
        Extranet
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AlternateUrl
    {
        [DataMember]
        public String WebAppName { get; set; }
        [DataMember]
        public String InternalURL { get; set; }
        [DataMember]
        public String CurrentURL { get; set; }
        [DataMember]
        public ZoneOption Zone { get; set; }
        [DataMember]
        public String PublicURLForZone { get; set; }
        [DataMember]
        public bool CollectionCanBeDeleted { get; set; }
        [DataMember]
        public bool URLCanBeDeleted { get; set; }

        public override bool Equals(object obj)
        {
            AlternateUrl url = obj as AlternateUrl;
            if (this.InternalURL.Equals(url.InternalURL) && this.Zone.ToString().Equals(url.Zone.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.InternalURL.GetHashCode();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationType
    {
        [EnumMember]
        AddNewCollection,
        [EnumMember]
        AddInternalUrl,
        [EnumMember]
        EditInterUrl,
        [EnumMember]
        EditPublicUrl,
        [EnumMember]
        DeleteInternalUrl,
        [EnumMember]
        DeleteCollection
    }
}
