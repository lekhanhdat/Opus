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


namespace AvePoint.GCommon.Contract.Tree.Object
{
    #region == using directives ==
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    #endregion
    [DataContract]
    [XmlRootAttribute("ExchangeOnlineTreeNodeDto")]
    public class ExchangeOnlineTreeNodeDto : AveTreeNodeDto<ExchangeOnlineTreeNodeDto>
    {
        public override bool Equals(object obj)
        {
            if (!(obj is ExchangeOnlineTreeNodeDto))
            {
                return false;
            }
            var node = obj as ExchangeOnlineTreeNodeDto;
            if (node.Level == NodeLevel.ExchangeOnlineFolders || node.Level == NodeLevel.ExchangeOnlineItems)
            {
                return ID == node.ID && Name == node.Name && FullPath == node.FullPath;
            }
            else
            {
                return Name == node.Name && FullPath == node.FullPath;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("O365TenantId")]
        public string O365TenantId { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public string ObjectId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("FarmName")]
        public String FarmName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("InternalFolderPath")]
        public String InternalFolderPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("EmailAddress")]
        public String EmailAddress { get; set; }

        /// <summary> 记录folder下所有foldercount，由agent browse folder时赋值，agent计算folder level的process时用 </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SubFolderCount")]
        public int SubFolderCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("Sender")]
        public String Sender { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("DisplayTo")]
        public String DisplayTo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SendDate")]
        public long SendDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("HasAttachment")]
        public Boolean HasAttachment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("Category")]
        public String Category { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //[XmlAttribute("ServiceUrl")]
        //public String ServiceUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("GroupName")]
        public String GroupName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SiteCollectionUrl")]
        public String SiteCollectionUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("MailboxType")]
        public MailboxType MailboxType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("IsNullClassificationSetting")]
        public bool IsNullClassificationSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("SkipRemoveContentAndDestroyAction")]     
        public bool SkipRemoveContentAndDestroyAction { get; set; }
        
        [IgnoreDataMember]
        [XmlIgnore]
        public bool UsingModernApp { get; set; }
    }
}
