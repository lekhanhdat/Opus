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



namespace System
{
    #region using directives
    using System.Linq;

    #endregion

    public static class TypeExtensions
    {
        /// <summary>
        /// To get the attributes of a object
        /// </summary>
        /// <typeparam name="T">the attribute type</typeparam>
        /// <param name="value">an type object</param>
        /// <returns> To get the type attributes</returns>
        public static T[] GetAttributes<T>(this Type value) where T : Attribute
        {
            var attributes = from attribute in value.GetCustomAttributes(typeof(T), true)
                             let strongTypeAttribute = attribute as T
                             orderby strongTypeAttribute descending
                             select strongTypeAttribute;
            return attributes.ToArray();
        }

        /// <summary>
        /// To get the attribute of a object
        /// </summary>
        /// <typeparam name="T">the attribute type</typeparam>
        /// <param name="value">an type object</param>
        /// <returns> To get the type attribute</returns>
        public static T GetAttribute<T>(this Type value) where T : Attribute
        {
            return value.GetAttributes<T>().Length == 0
                ? default(T)
                : value.GetAttributes<T>()[0];
        }
    }
}