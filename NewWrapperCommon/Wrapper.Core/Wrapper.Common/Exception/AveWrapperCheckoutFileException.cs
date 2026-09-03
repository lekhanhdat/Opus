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


namespace AvePoint.Wrapper.Common
{
    using System;

    [Serializable]
    public class AveWrapperCheckoutFileException : AveWrapperException
    {
        public Guid SiteId { get; set; }
        public Guid WebId { get; set; }
        public Guid FileId { get; set; }
        public int CheckoutUserId { get; set; }
        public string CheckoutUserLoginName { get; set; }
        public string CheckoutUserDisplayName { get; set; }
        public CheckoutFileErrorType ErrorType { get; set; }

        public AveWrapperCheckoutFileException() { }
        public AveWrapperCheckoutFileException(string message) : base(message) { }
        public AveWrapperCheckoutFileException(string message, Exception inner) : base(message, inner) { }
        public AveWrapperCheckoutFileException(AveWrapperErrorCode errorCode, string message) : base(errorCode, message) { }
        public AveWrapperCheckoutFileException(AveInternalResourceKey key, params object[] args) : base(key, args) { }


    }

    public enum CheckoutFileErrorType
    {
        CheckoutUserNotExist,
        CheckoutUserNotPermission,
        CheckoutFileStreamError
    }
}
