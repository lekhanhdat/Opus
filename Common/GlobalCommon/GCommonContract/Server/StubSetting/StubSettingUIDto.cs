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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.StubSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.StubSetting
{
    [DataContract]
    public class StubSettingUIDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int StubType { get; set; }
        [DataMember]
        public string StubContent { get; set; }
        [DataMember]
        public int StubCustomizeTags { get; set; }
        [DataMember]
        public bool IsDeclareStubAsRecords { get; set; }
        [DataMember]
        public string LastModifiedTime { get; set; }
        [DataMember]
        public bool IsEnabledRetention { get; set; }
        [DataMember]
        public int RetentionValue { get; set; }
        [DataMember]
        public int RetentionUnit { get; set; } // DateUnit
    }
    [DataContract]
    public class StubSettingResult : CommonSettingResultForPage
    {
        [DataMember]
        public List<StubSettingUIDto> StubSettingUIDtosList { get; set; }
    }

    public enum StubCustomizeTag
    {
        All = -1,
        None = 0,
        FileName = 1,
        FilePath = 2,
        Archivedtime = 4,
        Rulename = 8,
        RestoreLink = 16,
        ExternalLink = 32,
    }


}
