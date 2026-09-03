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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Native13NinTexWorkflowEntity
{
    [Serializable, Obfuscation]
    public class Message
    {
        // Properties
        public bool AllowLazyApprove { get; set; }
        public bool AttachFile { get; set; }
        public MessageAttachmentCollection Attachments { get; set; }
        public string Body { get; set; }
        public UserCollection CcList { get; set; }
        public UserCollection BccList { get; set; }
        public DeliveryMechanism DeliveryType { get; set; }
        public bool ExcludeHeaderAndFooter { get; set; }
        public User From { get; set; }
        public bool IsHtmlMessage { get; set; }
        public bool IsUsingGroupMessage { get; set; }
        public MessageOptions Options { get; set; }
        public MessageImportance Priority { get; set; }
        public string Subject { get; set; }


        // Nested Types
        public enum eWorkDays
        {
            Friday = 2,
            Monday = 0x20,
            Saturday = 1,
            Sunday = 0x40,
            Thursday = 4,
            Tuesday = 0x10,
            Wednesday = 8
        }
    }


}
