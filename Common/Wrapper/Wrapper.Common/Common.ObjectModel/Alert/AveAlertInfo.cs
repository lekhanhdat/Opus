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
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    [DataContract]
    public class AveAlertInfo
    {
        [DataMember]
        public Guid Id;

        [DataMember]
        public int UserId;

        [DataMember]
        public string UserLogin;

        [DataMember]
        public string UserEmail;

        [DataMember]
        public string SiteUrl;

        [DataMember]
        public string WebUrl;

        [DataMember]
        public string WebTitle;

        [DataMember]
        public int WebLanguage;

        [DataMember]
        public string ListUrl;

        [DataMember]
        public string ListTitle;

        [DataMember]
        public int ListBaseType;

        [DataMember]
        public int ListServerTemplate;

        [DataMember]
        public string AlertTitle;

        [DataMember]
        public int AlertType;

        [DataMember]
        public string AlertTemplateName;

        [DataMember]
        public string Filter;

        [DataMember]
        public AveAlertStatus Status;

        [DataMember]
        public Guid ItemDocId;

        [DataMember]
        public AveAlertDeliveryChannels DeliveryChannel;

        [DataMember]
        public AveEventType EventType;

        [DataMember]
        public AveAlertFrequency NotifyFreq;

        [DataMember]
        public DateTime NotifyTime;

    }
}
