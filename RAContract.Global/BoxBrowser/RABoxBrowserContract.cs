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
using AvePoint.RA.Contract.Explorer;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.BoxBrowser
{
    public class RABoxBrowserContract
    {
        [DataMember]
        public SourceFlag Flag => SourceFlag.Box;

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string RealId { get; set; }

        [DataMember]
        public string ContainerId { get; set; }

        [DataMember]
        public RMNodeLevel Level { get; set; }

        [DataMember]
        public string LeafName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public IconStatus IconStatus { get; set; } = IconStatus.NoSet;

        [DataMember]
        public string OwnerId { get; set; }

        [DataMember]
        public RABoxBrowserContract Parent { get; set; }

        [DataMember]
        public bool HasParent => this.Parent != null;

        [DataMember]
        public string ConnectionId { get; set; } 

        [DataMember]
        public int PageIndex { get; set; } = 1;

        [DataMember(EmitDefaultValue = false)]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<RABoxBrowserContract> Children { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int ChildrenCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int CheckNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool Expanded { set; get; }
    }

    [DataContract]
    public enum RMNodeLevel
    {
        [EnumMember]
        Root = -2,
        [EnumMember]
        BoxConnectionGroup = 7100,
        [EnumMember]
        BoxConnection = 7101,
        [EnumMember]
        BoxUser = 7102,
        [EnumMember]
        BoxFolder = 7103,
        [EnumMember]
        BoxFile = 7104,
    }

    [DataContract]
    public enum IconStatus
    {
        NoSet = 0,
        Inhert = 1,
        Break = 2
    }
}
