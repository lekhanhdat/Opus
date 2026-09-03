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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        void TraceInformation(Int32 eventId, String format, params Object[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
// ReSharper disable MethodOverloadWithOptionalParameter
        void TraceInformation(String format, params Object[] data);
// ReSharper restore MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void TraceInformation(String message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        void TraceWarning(Int32 eventId, String format, params Object[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
// ReSharper disable MethodOverloadWithOptionalParameter
        void TraceWarning(String format, params Object[] data);
// ReSharper restore MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void TraceWarning(String message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        void TraceError(Int32 eventId, String format, params Object[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
// ReSharper disable MethodOverloadWithOptionalParameter
        void TraceError(String format, params Object[] data);
// ReSharper restore MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void TraceError(String message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        void TraceVerbose(Int32 eventId, String format, params Object[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
// ReSharper disable MethodOverloadWithOptionalParameter
        void TraceVerbose(String format, params Object[] data);
// ReSharper restore MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void TraceVerbose(String message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        void TraceCritical(Int32 eventId, String format, params Object[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
// ReSharper disable MethodOverloadWithOptionalParameter
        void TraceCritical(String format, params Object[] data);
// ReSharper restore MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void TraceCritical(String message);
    }
}
