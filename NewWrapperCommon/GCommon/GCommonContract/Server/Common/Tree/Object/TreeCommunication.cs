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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Tree.Object
{
    [DataContract]
    public class TreeCommunication
    {
        [DataMember]
        public int TreeType { get; set; }

        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string AgentId { get; set; }

        [DataMember]
        public int Option { get; set; }

        [DataMember]
        public string SessionTreeName { get; set; }

        [DataMember]
        public int PagePosition { get; set; }

        [DataMember]
        public int CmdTag { get; set; }

        [DataMember]
        public List<String> patterns { get; set; }

        [DataMember]
        public string FarmID { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public bool IsAdminSearch { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public List<SPTreeNodeDto> AveTreeNodeDtoList { get; set; }

        [DataMember]
        public SPTreeNodeDto Node { get; set; }

        [DataMember]
        public List<SCSearchResultTreeNodeDto> SCSearchResultTreeNodeList { get; set; }

        [DataMember]
        public int ClickResult { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("NodeId:").Append(NodeId).Append("\t");
            builder.Append("AgentId:").Append(AgentId).Append("\t");
            builder.Append("Option:").Append(Option).Append("\t");
            builder.Append("SessionTreeName:").Append(SessionTreeName).Append("\t");
            builder.Append("CmdTag:").Append(CmdTag).Append("\t");
            builder.Append("FarmName:").Append(FarmName).Append("\t");
            builder.Append("Type:").Append(Type).Append("\t");
            builder.Append("IsAdminSearch:").Append(IsAdminSearch).Append("\t");
            builder.Append("JobId:").Append(JobId).Append("\t");
            builder.Append("patterns:").Append(patterns).Append("\t");
            return builder.ToString();
        }
    }
}
