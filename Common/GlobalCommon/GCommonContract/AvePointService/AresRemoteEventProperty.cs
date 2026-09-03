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
namespace AvePoint.GCommon.Contract.AvePointService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Message sent by AvePoint service when some action trigger some event receiver that registed by AvePoint service
    /// This object instance is serialized to String type by Json
    /// Maybe implement more properties later
    /// </summary>
    public class AresRemoteEventProperty
    {
        /// <summary>
        /// SPRemoteEventProperties instance is sent by SharePoint when some action trigger the event receiver that registed by AvePoint serivce.
        /// This property is SPRemoteEventProperties instance serialized by AvePoint using Json
        /// </summary>
        public string Properties { get; set; }

        /// <summary>
        /// An extension that stored some extra information needed by each module
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// data repository that is filled in service 
        /// </summary>
        public Dictionary<String, Object> ServiceData { get; set; }
    }
}
