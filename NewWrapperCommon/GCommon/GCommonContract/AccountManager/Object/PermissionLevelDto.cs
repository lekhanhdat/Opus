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
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionLevelDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<PermissionGroup> PermGroups { get; set; }
        [DataMember]
        public PermissionLevelType Type { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PermissionLevelDto)) return false;
            PermissionLevelDto dto = obj as PermissionLevelDto;
            return this.Id == dto.Id &&
                this.Name == dto.Name &&
                this.Description == dto.Description;
            //this.PermissionGroup == dto.PermissionGroup;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    //特别注意添加 [Flags]  他能使 数据库中的数据 例如：9在转换时变为SystemPermission | Default
    public enum PermissionLevelType : int
    {
        [EnumMember]
        Default = 1 << 0,
        [EnumMember]
        Customized = 1 << 1,
        [EnumMember]
        TenantPermission = 1 << 2,
        [EnumMember]
        SystemPermission = 1 << 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("permissionGroup")]
    public class PermissionGroup
    {
        [DataMember]
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "checked")]
        public bool Checked { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "full")]
        public bool Full { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "view")]
        public bool View { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "write")]
        public bool Write { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "control")]
        public bool Control { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "subGroups")]
        public List<PermissionGroup> SubGroups { get; set; }

        public void AddSubGroups(PermissionGroup group)
        {
            if (SubGroups == null)
            {
                SubGroups = new List<PermissionGroup>();
            }
            SubGroups.Add(group);
        }
        public void RemoveSubGroup(PermissionGroup group)
        {
            if (SubGroups != null)
            {
                SubGroups.Remove(group);
            }
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("perGroup")]
    public class PerGroup
    {

        [DataMember]
        [XmlAttribute(AttributeName = "checked")]
        public bool Checked { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [DataMember]
        [XmlAttribute(AttributeName = "subGroups")]
        public List<PerGroup> SubGroups { get; set; }

        public void AddSubGroups(PerGroup group)
        {
            if (SubGroups == null)
            {
                SubGroups = new List<PerGroup>();
            }
            SubGroups.Add(group);
        }
        public void RemoveSubGroup(PerGroup group)
        {
            if (SubGroups != null)
            {
                SubGroups.Remove(group);
            }
        }
    }
}

