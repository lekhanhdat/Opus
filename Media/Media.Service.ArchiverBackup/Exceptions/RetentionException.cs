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
using System.Runtime.Serialization;

namespace AvePoint.Media.Service.ArchiverBackup.Exceptions
{
    /// <summary>
    /// Exception thrown during archiver data retention operations
    /// </summary>
    [Serializable]
    public class RetentionException : Exception
    {
        /// <summary>
        /// Gets or sets the Job ID associated with the retention operation
        /// </summary>
        public string JobId { get; set; }
        
        /// <summary>
        /// Gets or sets the retention rule being executed
        /// </summary>
        public string RetentionRule { get; set; }
        
        public RetentionException()
        {
        }
        
        public RetentionException(string message) 
            : base(message)
        {
        }
        
        public RetentionException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
        
        protected RetentionException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            if (info != null)
            {
                JobId = info.GetString(nameof(JobId));
                RetentionRule = info.GetString(nameof(RetentionRule));
            }
        }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
                
            info.AddValue(nameof(JobId), JobId);
            info.AddValue(nameof(RetentionRule), RetentionRule);
            
            base.GetObjectData(info, context);
        }
    }
}
