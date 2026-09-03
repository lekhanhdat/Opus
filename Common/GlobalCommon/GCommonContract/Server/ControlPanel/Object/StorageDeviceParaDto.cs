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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    public class StorageDeviceParaDto
    {
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlAttribute("useEncryption")]
        public bool UseEncryption { get; set; }
        [XmlAttribute("encryptionProfileId")]
        public string EncryptionProfileId { get; set; }
        public List<PhysicalDevicePropertyDto> PdPropertyDtos { get; set; }
        [XmlAttribute("IsSelectAllFarm")]
        public bool IsSelectAllFarm { get; set; }
        public List<string> FarmIds { get; set; }
        [XmlAttribute("BackupPhysicalDeviceId")]
        public string BackupPhysicalDeviceId { get; set; }
        [XmlAttribute("UseCompression")]
        public bool UseCompression { get; set; }
        [XmlAttribute("CompressionSpeed")]
        public int CompressionSpeed { get; set; }

        //[XmlAttribute("ScheduleInfo")]
        //public ScheduleDto Schedule { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Physical device id:{0}, properties:", this.BackupPhysicalDeviceId);
            stringBuilder.AppendLine();
            if (this.PdPropertyDtos != null)
            {
                foreach (var property in this.PdPropertyDtos)
                {
                    stringBuilder.Append(property);
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }
    }

    public class StorageDevicePropertyDto
    {
        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("value")]
        public string Value { get; set; }

        public override String ToString()
        {
            return String.Format("Physical device property:[key={0},value={1}]", this.Key, this.Value);
        }
    }
}
