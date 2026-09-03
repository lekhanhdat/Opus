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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    [DataContract]
    public class ExplorerQueryV3Dto
    {
        /// <summary>
        /// advanced query option
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public ExplorerQueryOptionV3 QueryOption { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public ExplorerPagingInfo PagingInfo { get; set; }
    }
    [DataContract]
    public class ExplorerQueryOptionV3
    {
        [DataMember]
        public List<ExplorerSearchOptionV3> Values { get; set; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryOrderColumn OrderColumn { set; get; }
    }

    /// <summary>
    /// New model for advanced search which unite the search and filter in V2 together
    /// </summary>
    [DataContract]
    public class ExplorerSearchOptionV3
    {
        /// <summary>
        /// Search string formatted with JSON
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// and/or logic among columns
        /// </summary>
        [DataMember]
        public ExplorerSearchKeyOperationLogic ColumnsLogic { get; set; }

        /// <summary>
        /// contains/equals
        /// </summary>
        [DataMember]
        public ExplorerSearchColumnOperationLogic ColumnOperationLogic { get; set; }
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExplorerQueryColumn Column { get; set; }
    }
}
