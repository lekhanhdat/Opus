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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    public class EmsAutoSupportLogDto
    {
        public string appVersion { set; get; }
        public bool autoSupport { set; get; }
        public string category { set; get; }
        public string computerName { set; get; }
        public string eventDescription { set; get; }
        public int eventId { set; get; }
        public string eventSource { set; get; }
        public int logLevel { set; get; }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ems-autosupport-log is unmodifiable as the cause of being referenced.")]
        public XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<ems-autosupport-log/>");
            XmlNode xe = doc.CreateElement("app-version");
            xe.InnerText = this.appVersion;
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("auto-support");
            xe.InnerText = this.autoSupport.ToString();
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("category");
            xe.InnerText = this.category;
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("computer-name");
            xe.InnerText = this.computerName;
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("event-description");
            xe.InnerText = this.eventDescription;
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("event-id");
            xe.InnerText = this.eventId.ToString();
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("event-source");
            xe.InnerText = this.eventSource;
            doc.DocumentElement.AppendChild(xe);

            xe = doc.CreateElement("log-level");
            xe.InnerText = this.logLevel.ToString();
            doc.DocumentElement.AppendChild(xe);

            return doc;
        }
    }
}
