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
/* FormDigestFactory.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using AvePoint.Wrapper.Common;
using System.Net;

namespace AvePoint.Wrapper.Backup
{
    /// <summary>
    /// Object factory for FormDigest instances.
    /// </summary>
    class FormDigestFactory
    {
        #region Private Members
        private static FormDigestFactory mFormDigestFactory;
        #endregion

        #region Constructor
        protected FormDigestFactory()
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the current instance of the factory.
        /// </summary>
        public static FormDigestFactory Current
        {
            get { return mFormDigestFactory ?? (mFormDigestFactory = new FormDigestFactory()); }
        }

        /// <summary>
        /// Gets a new <see cref="NintexFormsClient.FormDigest" /> for the specified version, endpoint URL, and credentials.
        /// </summary>
        /// <param name="contextKind">The SharePoint version for the service endpoint.</param>
        /// <param name="webUrl">The URL of the SharePoint site.</param>
        /// <param name="credentials">The credentials to use.</param>
        /// <returns>A <see cref="NintexFormsClient.FormDigest" /> implementation for the specified version, 
        /// endpoint URL, and credentials; otherwise, a null reference.</returns>
        public FormDigestBase CreateFormDigestInstance(AveContextKind contextKind, string webUrl, ICredentials credentials)
        {
            switch (contextKind)
            {
                case AveContextKind.Server13ObjectModel:
                case AveContextKind.Server16ObjectModel:
                    return new FormDigest2013(webUrl, credentials);
                case AveContextKind.ServerObjectModel:
                case AveContextKind.Server10ObjectModel:
                    return new FormDigest2010(webUrl, credentials);
                default:
                    return null;
            }
        }

        #endregion
    }
}
