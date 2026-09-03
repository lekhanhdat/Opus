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
using AvePoint.GCommon.Contract.Tree.Object;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Tree;

[DataContract]
[XmlRootAttribute("GoogleDriveTreeNodeDto")]
public class GoogleDriveTreeNodeDto : AveTreeNodeDto<GoogleDriveTreeNodeDto>
{
    [DataMember]
    [XmlAttribute("objectId")]
    public string ObjectId { get; set; }

    [IgnoreDataMember]
    [XmlIgnore]
    public string ContainerId { get; set; }

    [IgnoreDataMember]
    [XmlIgnore]
    public string NodeId { get; set; }

    [DataMember]
    [XmlAttribute("tenantId")]
    public string TenantId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute("SkipRemoveContentAndDestroyAction")]
    public bool SkipRemoveContentAndDestroyAction { get; set; }
    public PropertyState Property { get; set; }
    public SecurityState Security { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute("PredictionModeType")]
    public int PredictionModeType { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [XmlAttribute("IsNodeProcessFromGControl")]
    public bool IsNodeProcessFromGControl { get; set; }

    public override bool Equals(object obj)
    {
        if (!(obj is GoogleDriveTreeNodeDto))
        {
            return false;
        }
        var node = obj as GoogleDriveTreeNodeDto;
        return Name == node.Name && ID == node.ID;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
