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
    public class AddWebPartOperation:CAOperation
    {
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string Postion { get; set; }
        [DataMember]
        public List<ListAndLib> listAndLibCollection { get; set; }
        [DataMember]
        public List<WebPartSetting> WebParts { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string WebName { get; set; }
        [DataMember]
        public int WebPartType { get; set; }
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListAndLib
    {
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Desc { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebPartSetting
    {
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Desc { get; set; }
        [DataMember]
        public string Group { get; set; }
    }
}
