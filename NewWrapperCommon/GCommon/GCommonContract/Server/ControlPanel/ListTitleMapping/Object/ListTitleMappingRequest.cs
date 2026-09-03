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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ContentTypeMapping.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ListTitleMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListTitleMappingRequest
    {
        /// <summary>
        /// used by get action
        /// </summary>
        [DataMember]
        public String profileId { get; set; }

        /// <summary>
        /// used by save action
        /// </summary>
        [DataMember]
        public ProfileDto profile { get; set; }

        /// <summary>
        /// used by delete action
        /// </summary>
        [DataMember]
        public List<String> profileIds { get; set; }

        /// <summary>
        /// used by download action
        /// </summary>
        [DataMember]
        public DownLoadRequest downLoadRequest { get; set; }

        [DataMember]
        public UploadRequest uploadRequest { get; set; }
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class UploadRequest
    //{
    //     <summary>
    //     upload mapping name
    //     </summary>
    //    [DataMember]
    //    public String mappingName { get; set; }

    //     <summary>
    //     upload bytes
    //     </summary>
    //    [DataMember]
    //    public byte[] importBytes { get; set; }
    //}

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class DownLoadRequest
    //{
    //    /// <summary>
    //    /// down load from database
    //    /// </summary>
    //    [DataMember]
    //    public String mappingId { get; set; }

    //    /// <summary>
    //    /// down load from Gui
    //    /// </summary>
    //    [DataMember]
    //    public ProfileDto mapping { get; set; }
    //}
}
