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
    using System.Collections.Generic;
    using System.Diagnostics;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion

    ///<Summary>
    /// Allow user to map the default request key to another one
    ///</Summary>
    public class CoreServiceLocatorKeyMapService : ICoreServiceLocatorKeyMapService
    {
        /// <summary>
        /// The mapped key cache dictionary
        /// </summary>
        public Dictionary<String, String> KeyMapDictionary { get; set; }

        #region ICoreServiceLocatorKeyMapService Members

        /// <summary>
        /// Map the default key to another
        /// </summary>
        /// <param name="requestObjectId">the original request key</param>
        /// <returns>the mapped key</returns>
        public virtual String MapKey(String requestObjectId)
        {
            var result = default(String);
            if (this.KeyMapDictionary != null)
                this.KeyMapDictionary.TryGetValue(requestObjectId, out result);

            return result ?? requestObjectId;
        }

        #endregion
    }
}
