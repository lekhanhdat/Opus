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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;

    #endregion

    /// <summary>
    /// The root interface of the trace utility of the MicroKernel
    /// </summary>
    public interface ITraceSource
    {
        void TraceInformation(Int32 eventId, String format, params Object[] data);
        void TraceInformation(String format, params Object[] data);
        void TraceInformation(String message);

        void TraceWarning(Int32 eventId, String format, params Object[] data);
        void TraceWarning(String format, params Object[] data);
        void TraceWarning(String message);

        void TraceError(Int32 eventId, String format, params Object[] data);
        void TraceError(String format, params Object[] data);
        void TraceError(String message);

        void TraceVerbose(Int32 eventId, String format, params Object[] data);
        void TraceVerbose(String format, params Object[] data);
        void TraceVerbose(String message);

        void TraceCritical(Int32 eventId, String format, params Object[] data);
        void TraceCritical(String format, params Object[] data);
        void TraceCritical(String message);
    }
}
