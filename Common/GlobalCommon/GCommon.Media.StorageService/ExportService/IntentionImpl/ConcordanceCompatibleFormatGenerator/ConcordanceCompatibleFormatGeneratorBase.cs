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

    #endregion using directives

    internal abstract class ConcordanceCompatibleFormatGeneratorBase
        : IConcordanceCompatibleFormatGenerator
    {
        protected abstract Char Comma { get; }

        protected abstract Char Quote { get; }

        protected abstract Char NewLine { get; }
        public static IConcordanceCompatibleFormatGenerator GetCompatibleFormatGenerator(ExportFormat exportFormat)
        {
            return Activator.CreateInstance(Type.GetType(typeof(ConcordanceCompatibleFormatGeneratorBase).Namespace + "." + exportFormat.ToString() + "ConcordanceCompatibleFormatGenerator")) as IConcordanceCompatibleFormatGenerator;
        }

        public virtual String GenerateHeaderLine(MetaData metaData)
        {
            return this.Generate(metaData, isHeaderLine: true);
        }

        public virtual String GenerateDataLine(MetaData metaData)
        {
            return this.Generate(metaData, isHeaderLine: false);
        }

        private String Generate(MetaData metaData, Boolean isHeaderLine)
        {
            var metaDataInfo = new StringBuilder();
            var properties = metaData.GetType().GetProperties().ToList()
                     .FindAll(item => item.GetAttribute<ConcordanceMetaDataAttribute>() != null);
            for (int i = 0; i < properties.Count; i++)
            {
                var attribute = properties[i].GetAttribute<ConcordanceMetaDataAttribute>();
                var stringVaule = isHeaderLine ? attribute.ColumnName : this.GetValue(properties[i], metaData);
                if (i < properties.Count - 1)
                    metaDataInfo.AppendFormat("{0}{1}{0}{2}", this.Quote, stringVaule, this.Comma);
                else metaDataInfo.AppendFormat("{0}{1}{0}{2}", this.Quote, stringVaule, Environment.NewLine);
            }
            return metaDataInfo.ToString();
        }

        private String GetValue(PropertyInfo property, Object instance)
        {
            var result = default(String);
            var propertyValue = property.FastGetValue(instance) ?? String.Empty;
            if (propertyValue is IEnumerable<MetaDataItemInfo>)
            {
                var metaDataItemInfoSet = propertyValue as IEnumerable<MetaDataItemInfo>;
                var stringBuilder = new StringBuilder();
                metaDataItemInfoSet.ForEach<MetaDataItemInfo>(metaDataItem =>
                stringBuilder.AppendFormat("{0}={1}{2}", metaDataItem.Name, metaDataItem.Value, this.NewLine));
                result = stringBuilder.ToString();
            }
            else if (propertyValue is List<String>)
            {
                var stringBuilder = new StringBuilder();
                var attachmentsList = propertyValue as List<String>;
                for (int i = 0; i < attachmentsList.Count; i++)
                {
                    if (i < attachmentsList.Count)
                        stringBuilder.AppendFormat("{0}{1}", attachmentsList[i], this.NewLine);
                    else
                        stringBuilder.AppendFormat("{0}", attachmentsList[i]);
                }
                result = stringBuilder.ToString();
            }
            else result = propertyValue != null ? propertyValue.ToString() : String.Empty;
            return result;
        }
    }
}