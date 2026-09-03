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



namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    #endregion

    internal class AutonomyCompatibleForamatGenerator
        : IAutonomyCompatibleForamatGenerator
    {
        public String SpaceChar { get { return " "; } }
        public String UnixNewLine { get { return "\n"; } }
        public String WindowsNewLine { get { return "\r\n"; } }

        public String Generate(MetaData metaData)
        {
            var stringBuilder = new StringBuilder();
            metaData.GetType().GetProperties().ForEach(property =>
            {
                var lineData = this.GetDataValue(property, metaData);
                stringBuilder.Append(lineData);
            });

            //we add two lines in the last because the sample idx file does this way
            stringBuilder.AppendFormat("{0}{1}{1}", "#DREENDDOC", UnixNewLine);

            return stringBuilder.ToString();
        }

        String GetDataValue(PropertyInfo property, Object metaData)
        {
            var result = String.Empty;
            var attribute = property.GetAttribute<AutonomyMetaDataAttribute>();
            if (attribute != null)
            {
                var propertyValue = property.FastGetValue(metaData);
                if (propertyValue is IEnumerable<MetaDataItemInfo>)
                {
                    if (!attribute.IsFiltered)
                    {
                        var filedDataBuilder = new StringBuilder();
                        var propertyRealValue = propertyValue as IEnumerable<MetaDataItemInfo>;
                        propertyRealValue.ForEach(item =>
                        {
                            filedDataBuilder.AppendFormat(
                                "{0}{1}{2}=\"{3}\"{4}",
                                attribute.FieldName,
                                SpaceChar,
                                item.Name,
                                item.Value,
                                UnixNewLine);
                        });
                        result = filedDataBuilder.ToString();
                    }
                }
                else
                {
                    if (!attribute.IsFiltered)
                    {
                        if (attribute.FieldName.EqualsIgnoreCase("#DREFIELD"))
                            result = "{0}{1}{2}=\"{3}\"{4}".FormatWith(
                               attribute.FieldName,
                               SpaceChar,
                               attribute.Name,
                               propertyValue.ToString(),
                               UnixNewLine);
                        else result = "{0}{1}{2}{3}".FormatWith(
                                attribute.FieldName,
                                SpaceChar,
                                propertyValue.ToString(),
                                UnixNewLine);
                    }
                }
            }
            return result;
        }
    }
}