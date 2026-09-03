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
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    public class ConnectorTreeMessage : SPTreeMessage
    {
        /// <summary>
        /// if is contentlibrary, value is ConnectorConstants.LIST_BASETYPE_FSDL
        /// if is medialibrary, value is ConnectorConstants.LIST_BASETYPE_VDL
        /// </summary>
        [DataMember]
        public ConnectorLibType ConnectorLibraryType { get; set; }

        /// <summary>
        /// 所有已经配置过managedPath的library的path信息,Key为SPObjectId, Value为对应的Path
        /// </summary>
        [DataMember]
        public Dictionary<string, string> ConnectorLibraryPathInfos { get; set; }

    }
}
