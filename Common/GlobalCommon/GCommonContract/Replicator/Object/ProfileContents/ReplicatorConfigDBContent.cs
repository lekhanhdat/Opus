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





using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.ProfileContents
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorConfigDBContent : IProfileContent
    {
        [DataMember]
        [XmlAttribute("databaseServer")]
        public string DatabaseServer { get; set; }

        [DataMember]
        [XmlAttribute("databaseName")]
        public string DatabaseName { get; set; }

        [DataMember]
        public DatabaseCredentials DbCredentials { get; set; }

        [DataMember]
        [XmlAttribute("deleteUnusedDB")]
        public bool DeleteUnusedDB { get; set; }

        [DataMember]
        [XmlAttribute("failoverDatabaseServer")]
        public string FailoverDatabaseServer { get; set; }

        [DataMember]
        [XmlAttribute("isConnectionString")]
        public bool IsConnectionString { get; set; }

        [DataMember]
        [XmlAttribute("connectionString")]
        public string ConnectionString { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseCredentials
    {
        [DataMember]
        [XmlAttribute("authentication")]
        public DatabaseAuthentication Authentication { get; set; }

        [DataMember]
        [XmlAttribute("account")]
        public string Account { get; set; }

        [DataMember]
        [XmlAttribute("password")]
        public string Password { get; set; }
    }
}
