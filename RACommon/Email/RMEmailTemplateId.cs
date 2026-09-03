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

namespace AvePoint.RA.Common.Email
{
    public static class RMEmailTemplateId
    {
        public static readonly Guid LOAN_REQUEST_END_USER = new("AED805AD-AAD3-4238-BE51-18A629D27E9A");

        public static readonly Guid LOAN_REQUEST_RM = new("FEDCDDD8-6415-478A-BF3E-1E2F6D4E3892");

        public static readonly Guid LOAN_REQUEST_APPROVED = new("87B2759E-CBEE-42BB-AC79-8C993108E91B");

        public static readonly Guid LOAN_REQUEST_REJECT = new("61824ED2-EC98-47E9-A43E-982562F2BBD1");

        public static readonly Guid NEW_CREATION_REQUEST_ENDUSER = new("7F5CAABD-D7FA-4CA3-83D8-2C9FBAFFD3E9");

        public static readonly Guid NEW_CREATION_REQUEST_RM = new("60ABB721-46CB-4491-A5C8-28B939059876");

        public static readonly Guid CREATION_REQUEST_APPROVED = new("3FF42C82-3BB9-4F5F-98FA-4757DC1F550C");

        public static readonly Guid CREATE_REQUEST_REJECT = new("250D4D19-2B16-4432-84E3-F7DB83A22F49");

        public static readonly Guid WAITING_APPROVAL = new("E9A34229-0713-4F84-A5C3-65F46E616E4D");

        public static readonly Guid APPROVED = new("0C8C8ACA-1B74-47C6-8CEC-B65F39D7238B");

        public static readonly Guid REJECTED = new("C4A7095B-E732-4341-8B78-156FF5F9594B");

        public static readonly Guid ESCALATED = new("2E54FE0E-BB7D-425E-A4B8-DA68F8CEF816");

        public static readonly Guid MANUAL_APPROVAL = new("5E4C71E3-A9F9-4DF3-B517-6CD45518EB56");

        public static readonly Guid ML_MANUAL_APPROVAL = new("2C13FB99-4C44-4782-8AFF-40940B8C46F6");

        public static readonly Guid EXPORT_ZIP_PASSWORD = new("72D42C24-132A-4768-9D32-8A5DAFAFA083");

        public static readonly Guid JOB_NOTIFICATION = new("6F109E58-BB1E-4407-B88E-FCEE0599742E");
        public static readonly Guid BORROWER_NOTIFICATION = new("9F3C4C52-7D0E-4A2F-B6C8-3F8A9E2B1C74");

        public static readonly Guid HOLD_NOTIFICATION = new("B3F7A2D1-8E4C-4F6A-9D5B-2C1E3A4F5B6D");
        public static readonly Guid HOLD_MANAGER_NOTIFICATION = new("D4E5F6A7-B8C9-4D0E-9F1A-2B3C4D5E6F7A");
    }
}
