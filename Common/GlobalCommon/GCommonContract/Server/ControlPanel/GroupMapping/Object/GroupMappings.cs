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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.GroupMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GroupMappingContract : IProfileContent
    {
        [DataMember]
        public List<GroupMappingDto> GroupMappings { get; set; }

        [DataMember]
        public Int64 ModifiedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GroupMappingDto : INotifyPropertyChanged
    {
        [DataMember]
        public Boolean IsCheck { get; set; }
        //[DataMember]
        //public String SourceGroupName { get; set; }
        //[DataMember]
        //public String DestinationGroupName { get; set; }
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GroupMappingResponse
    {
        [DataMember]
        public Boolean MappingBeUsing { get; set; }

        [DataMember]
        public ProfileDto Profile { get; set; }

        [DataMember]
        public List<ProfileDto> ProfileList { get; set; }

        [DataMember]
        public List<CommonDetailInfoDto> CommonDetailInfoList { get; set; }

        //For unload & download
        [DataMember]
        public String ProfileExtension { get; set; }

        [DataMember]
        public ValidateResultType ResultType { get; set; }

        [DataMember]
        public byte[] DownLoadBytes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GroupMappingRequest
    {
        [DataMember]
        public String ProfileId { get; set; }

        [DataMember]
        public ProfileDto Profile { get; set; }

        [DataMember]
        public List<NameAndIdDto> NameAndIdList { get; set; }
    }
}
