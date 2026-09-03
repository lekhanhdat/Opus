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




namespace Media.Extension
{
    #region using directives
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Service.DomainModel;
    using Media.Common;
    #endregion

    ///<Summary>
    /// extension of the System.String class
    ///</Summary>
    public static class StringExtension
    {
        /// <summary>
        /// Convert a string media data type to NodeLevelType
        /// </summary>
        /// <param name="value">the value is media data type string</param>
        /// <returns>the converted result</returns>
        public static NodeLevel ToNodeLevelByMediaDataTypeString(this String value)
        {
            return EnumConverter.ToEnum<MediaDataType>(value).GetAttribute<DataTypeMapAttribute>().NodeLevel;
        }

        /// <summary>
        /// Convert the SPObjectLevel string to a MediaDataType string
        /// </summary>
        /// <param name="value">the SPObject string</param>
        /// <returns>the converted MediaDataType string</returns>
        public static String ToMediaDataTypeStringBySPObjectLevelString(this String value)
        {
            return EnumConverter.ToEnum<MediaSPObjectLevel>(value).GetAttribute<SPObjectMapAttribute>().DataType.ToString();
        }

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            return Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(T)) as T;
        }
    }
}