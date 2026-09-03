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
/* FormSaveInfo.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Represents metadata associated with saving or publishing a form.
    /// </summary>
    [DataContract]
    public class FormSaveInfo
    {
        /// <summary>
        /// The URL for the form.
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// The current version of the form.
        /// </summary>
        [DataMember]
        public string Version { get; set; }
    }
}
