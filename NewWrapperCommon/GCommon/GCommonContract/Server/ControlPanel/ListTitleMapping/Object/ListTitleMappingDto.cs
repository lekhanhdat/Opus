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


using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Collections.Generic;
using ListTitleNameSpace = AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
using System;
using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ListTitleMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListTitleMappingDataContract : IProfileContent
    {
        public ListTitleMappingDataContract()
        {
            this.listTitleMappings = new List<ListTitleMappingDto>();
        }

        [DataMember]
        public List<ListTitleMappingDto> listTitleMappings { get; set; }

        [DataMember]
        public long modifiedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListTitleMappingDto
    {
        public ListTitleMappingDto()
        {
            this.ListConditions = new List<ListTitleNameSpace.ColumnFilter>();
            this.SiteConditions = new List<ListTitleNameSpace.ColumnFilter>();
            this.MappingValues = new List<ListTitleMappingValue>();
        }

        [DataMember]
        public List<ListTitleNameSpace.ColumnFilter> ListConditions { get; set; }
        [DataMember]
        public List<ListTitleNameSpace.ColumnFilter> SiteConditions { get; set; }
        [DataMember]
        public List<ListTitleMappingValue> MappingValues { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListTitleMappingValue : INotifyPropertyChanged
    {
        private string srcGroupName;
        [DataMember]
        public string SrcGroupName
        {
            get { return srcGroupName; }
            set
            {
                srcGroupName = value;
                NotifyPropertyChanged("SrcGroupName");
            }
        }

        private string desGroupName;
        [DataMember]
        public string DesGroupName
        {
            get { return desGroupName; }
            set
            {
                desGroupName = value;
                NotifyPropertyChanged("DesGroupName");
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
