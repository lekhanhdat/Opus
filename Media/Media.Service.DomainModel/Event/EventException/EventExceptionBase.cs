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




namespace AvePoint.Media.Service.DomainModel.Event
{
    #region using directives
    using System;

    #endregion

    [Serializable]
    public abstract class EventExceptionBase : Exception, IEventExceptionBase
    {
        public EventExceptionBase() { }
        public EventExceptionBase(string message) : base(message) { }
        public EventExceptionBase(string message, Exception inner) : base(message, inner) { }
        protected EventExceptionBase(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public Exception InnerEventExcetion { get; set; }
        public Type TagetType { get; set; }
        public abstract Int32 EventId { get; }
        public abstract String EventSymbolicName { get; }
        public abstract String EventDescription { get; set; }
        public abstract String EventMessage { get; set; }
    }
}