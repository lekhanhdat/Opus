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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object
{
    [XmlRoot(ElementName = "BackupPlanSetting")]
    public class EOBackupPlanSetting
    {
        [XmlAttribute]
        public int CompressionType { get; set; }

        [XmlAttribute]
        public int DataSecurity { get; set; }

        [XmlAttribute]
        public bool FullTextIndex { get; set; }

        [XmlAttribute]
        public bool LockSiteCollection { get; set; }

        [XmlAttribute]
        public string FilterPolicyId { get; set; }

        [XmlAttribute]
        public string NotificationProfileId { get; set; }

        [XmlAttribute]
        public bool IncludeUserProfile { get; set; }

        [XmlAttribute]
        public string SecurityProfileGuid { get; set; }

        [XmlAttribute]
        public bool IncludeVersions { get; set; }

        [XmlAttribute]
        public Dictionary<string, double> DeviceInvalidDataPercent { get; set; }

        [XmlAttribute]
        public int DataCheckState { get; set; }

        [XmlAttribute]
        public string DataMissingRecord { get; set; }
    }

    public enum DataCheckState
    {
        None = 0,
        Successful = 1,
        Failed = 2,
        RealFullBackup = 3,
        SubJobFailed = 4,
        ForceFullBackup = 5,
    }
}
