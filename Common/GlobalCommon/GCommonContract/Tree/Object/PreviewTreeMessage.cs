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
using AvePoint.GCommon.Contract.ContentManager.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    public class PreviewTreeMessage : AveTreeMessage
    {
        [DataMember]
        public List<SPTreeNodeDto> NodeList { get; set; }

        [DataMember]
        public SPTreeNodeDto Node { get; set; }

        [DataMember]
        public List<SPTreeNodeDto> SourceTree { get; set; }

        [DataMember]
        public string PreviewKey { get; set; }

        //标识browse的请求是从Content Manager功能的offline还是online发起的
        [DataMember]
        public OperationType FuncType { get; set; }

        [DataMember]
        public LocationInfoDto LocationInfoDto { get; set; }

        /// <summary>
        /// 传递content manager import preview 功能时的目的端farmid
        /// </summary>
        [DataMember]
        public string CMImportDestFarmId { get; set; }

        [DataMember]
        public bool IsPromoteSubSite { get; set; }
    }
}
