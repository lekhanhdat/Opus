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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServicesOfAllServersOperation : CAOperation
    {         
        [DataMember]
        public List<ServicesOfOneServer> Services { get; set; }
        [DataMember]
        public FarmInformation FarmInfo { set; get; }

        [DataMember]
        public string SelectedServerId { get; set; }
        [DataMember]
        public string SelectedServiceId { get; set; }
        [DataMember]
        public ServiceAction SelectedServiceAction { get; set; }
        [DataMember]
        public bool RemoveServer { get; set; }
        [DataMember]
        public Service ServiceAfterAction { get; set; }
      
    }
    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServicesOfOneServer
    {
        /// <summary>
        /// For GUI
        /// </summary>
        public string ServerStatusStr { get; set; }

        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string ServerId { get; set; }
        [DataMember]
        public string ServerStatus { get; set; }
        [DataMember]
        public List<string> SharePointProductsInstalled { get; set; }
        [DataMember]
        public List<Service> Services { get; set; }
        [DataMember]
        public List<string> ServicesRunning { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Service
    {
        /// <summary>
        /// For GUI
        /// </summary>
        public string StatusStr { get; set; }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public ServiceAction AvailableAction { get; set; }
        [DataMember]
        public bool Configurable { get; set; }
        

        /// <summary>
        /// add by Zhang Hailong
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayName;
        }

    }

     [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ServiceAction
    {
        [EnumMember]
        NoAction,
        [EnumMember]
        Start,
        [EnumMember]
        Stop
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmInformation
    {
        [DataMember]
        public string ConfigurationDatabaseVersion { get; set; }
        [DataMember]
        public string ConfigurationDatabaseServer { get; set; }
        [DataMember]
        public string ConfigurationDatabaseName { get; set; } 
    } 
}
