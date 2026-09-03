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
    using System.Runtime.Serialization;
    #endregion

    /*********************************************************************
     *       This class will be used as the top level entry of
     *       the docave6 communication system
     ********************************************************************/

    /*********************************************************************
     *    Please put the known type in alphabet order, thanks
     ********************************************************************/

    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = "http://www.avepoint.com")]
    public class CoreMessage
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String Extension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String AuthorizationKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Boolean IsExceptionOccurred { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String ExceptionDetails { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String ExceptionMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String ExceptionRawMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public CoreServiceInvocationContext InvocationContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string AllAccountProfilePwdCrc { get; set; }
    }
}