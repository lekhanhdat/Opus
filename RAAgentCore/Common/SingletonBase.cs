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

namespace AvePoint.RA.FileSystem.Core
{
    /// <summary>
    /// Thread-safe singleton base class using Lazy&lt;T&gt; pattern.
    /// Provides better performance than lock-based double-check locking pattern.
    /// </summary>
    /// <typeparam name="T">The type of the singleton instance. Must have a parameterless constructor.</typeparam>
    public abstract class SingletonBase<T> where T : class, new()
    {
        private static readonly Lazy<T> _lazyInstance = new Lazy<T>(() => new T(), true);

        /// <summary>
        /// Gets the singleton instance. Thread-safe and lazy-initialized.
        /// </summary>
        public static T GetInstance()
        {
            return _lazyInstance.Value;
        }

        /// <summary>
        /// Checks if the singleton instance has been created.
        /// </summary>
        protected static bool IsInstanceCreated => _lazyInstance.IsValueCreated;

        /// <summary>
        /// Protected constructor to prevent direct instantiation.
        /// Only accessible by derived classes.
        /// </summary>
        protected SingletonBase()
        {
        }
    }
}
