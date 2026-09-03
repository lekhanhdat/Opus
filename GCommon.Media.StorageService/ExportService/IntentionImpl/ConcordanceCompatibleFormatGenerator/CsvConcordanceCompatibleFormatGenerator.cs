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
    using System.Text;

    #endregion

    internal class CsvConcordanceCompatibleFormatGenerator
        : ConcordanceCompatibleFormatGeneratorBase
    {
        /// <summary>
        /// Comma ASCII code is 44
        /// </summary>
        protected override Char Comma { get { return '\u002C'; } }

        /// <summary>
        /// Quote ASCII code is 34
        /// </summary>
        protected override Char Quote { get { return '\u0022'; } }

        /// <summary>
        /// NewLine ASCII code is 13
        /// </summary>
        protected override Char NewLine { get { return '\u000D'; } }
        public override String GenerateHeaderLine(MetaData metaData)
        {
            return this.Generate(metaData, isHeaderLine: true);
        }

        public override String GenerateDataLine(MetaData metaData)
        {
            return this.Generate(metaData, isHeaderLine: false);
        }

        private string Generate(MetaData metaData, Boolean isHeaderLine)
        {
            var csvMetadataInfo = new StringBuilder();
            var properties = metaData.CsvMetadataInfo;
            for (Int32 i = 0; i < properties.Count; i++)
            {
                var origainalStringValue = isHeaderLine ? properties[i].Name : properties[i].Value;
                var stringValue = origainalStringValue.Contains("\"") ? origainalStringValue.Replace("\"", "\"\"") : origainalStringValue;
                if (i < properties.Count - 1)
                    csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", this.Quote, stringValue, this.Comma);
                else csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", this.Quote, stringValue, Environment.NewLine);
            }
            return csvMetadataInfo.ToString();
        }
    }
}