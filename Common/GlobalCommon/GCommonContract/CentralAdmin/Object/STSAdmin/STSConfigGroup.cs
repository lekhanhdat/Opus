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




using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin
{
    [DataContract]
    public class STSConfigGroup
    {
        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public string STSType { set; get; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public bool Required { set; get; }

        [DataMember]
        public string Tip { set; get; }

        [DataMember]
        public string IsSpecial { set; get; }

        [DataMember]
        public string ParamSelection { set; get; }

        [DataMember]
        public string ParamDescription { set; get; }

        [DataMember]
        public string ParamNameOnly { set; get; }

        [DataMember]
        public bool ParamIsSend { set; get; }

        [DataMember]
        public string ParamBelongTo { set; get; }

        [DataMember]
        public string ParamNameSelect { set; get; }

        [DataMember]
        public int Encrypted { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public string UserInfoId { set; get; }

    }
}
