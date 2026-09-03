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
using System.Xml.Serialization;


namespace AvePoint.GCommon.Contract.SharePointBrowser.Object
{
    [DataContract]
    public abstract class AbstractTreeNodeDto<T> where T : AbstractTreeNodeDto<T>
    {   
        public AbstractTreeNodeDto()
        {
            Depth = 1;
            CheckNumber = 0;
            CheckState = 0;
            Children = new List<T>();
        }
        [DataMember]
        [XmlAttribute]
        public String Id { get; set; }
        [DataMember]
        [XmlAttribute]
        public String ParentId { get; set; }
        [DataMember]
        [XmlAttribute]
        public String Name { get; set; }
        [DataMember]
        [XmlAttribute]
        public int Type{ get; set; }
        //[DataMember]
        [XmlIgnore]
        public T Parent{ get; set; }
        [DataMember]
        [XmlElement("TreeNode")]
        public List<T> Children { get; set; }
        //public List<T> Files = new List<T>()
        [DataMember]
        [XmlAttribute]
        public bool ChildrenLoaded{ get; set; }
        [DataMember]
        [XmlAttribute]
        public int Depth{ get; set; }
        [DataMember]
        [XmlAttribute]
        public int CheckNumber {get; set; }
        [DataMember]
        [XmlAttribute]
        public int CheckState{ get; set; }
        [DataMember]
        [XmlAttribute]
        public String Title{ get; set; }       
        [DataMember]
        [XmlAttribute]
        public int CurrentPageNum{ get; set; }

        //Added by hqin
        [DataMember]
        public string AgentId { get; set; }

        [DataMember]
        [XmlAttribute]
        public string FullPath { get; set; }

        public List<T> getChildren()
        {
            return Children;
        }
        public void setChildren(List<T> children)
        {
            this.Children = children;
        }
        public void AddChildren(T child)
        {
            this.Children.Add(child);
        }
    }
}
