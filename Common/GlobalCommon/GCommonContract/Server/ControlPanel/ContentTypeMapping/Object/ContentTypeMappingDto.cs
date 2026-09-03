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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using ContentTypeNameSpace = AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ContentTypeMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypeMappingDataContract : IProfileContent
    {
        public ContentTypeMappingDataContract()
        {
            this.contentMappings = new List<ContentTypeMappingDto>();
        }

        [DataMember]
        public List<ContentTypeMappingDto> contentMappings { get; set; }

        [DataMember]
        public long modifiedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypeMappingDto
    {
        public ContentTypeMappingDto()
        {
            this.ListConditions = new List<ContentTypeNameSpace.ColumnFilter>();
            this.SiteConditions = new List<ContentTypeNameSpace.ColumnFilter>();
            this.MappingValues = new List<MappingValue>();
        }

        [DataMember]
        public List<ContentTypeNameSpace.ColumnFilter> ListConditions { get; set; }
        [DataMember]
        public List<ContentTypeNameSpace.ColumnFilter> SiteConditions { get; set; }
        [DataMember]
        public List<MappingValue> MappingValues { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingValue : INotifyPropertyChanged
    {
        private string sourceGroupName;
        [DataMember]
        public string SourceGroupName
        {
            get { return sourceGroupName; }
            set
            {
                sourceGroupName = value;
                NotifyPropertyChanged("SourceGroupName");
            }
        }

        private string destinationGroupName;
        [DataMember]
        public string DestinationGroupName
        {
            get { return destinationGroupName; }
            set
            {
                destinationGroupName = value;
                NotifyPropertyChanged("DestinationGroupName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  
    }
}
