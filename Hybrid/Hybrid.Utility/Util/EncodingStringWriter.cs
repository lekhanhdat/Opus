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





namespace  AvePoint.Hybrid.Utility
{
    #region using directives
    using System;
    using System.IO;
    using System.Text;
    #endregion

    /// <summary>
    /// 继承StringWriter，并且重写Encoding，使StringWriter可以扩展Encoding方法。
    /// </summary>
    internal class EncodingStringWriter : StringWriter
    {
        Encoding encoding;
        /// <summary>
        /// StringWriter的Encoding方法
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }

        /// <summary>
        /// 构造函数，可以指定Encoding
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="sb"></param>
        /// <param name="formatProvider"></param>
        public EncodingStringWriter(Encoding encoding, StringBuilder sb, IFormatProvider formatProvider)
            : base(sb, formatProvider)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="formatProvider"></param>
        public EncodingStringWriter(StringBuilder sb, IFormatProvider formatProvider)
            : base(sb, formatProvider)
        {
            this.encoding = Encoding.UTF8;
        }

        /// <summary>
        /// 由于不能Overwrite Set方法，所以提供一个Method来更改Encoding方法
        /// </summary>
        /// <param name="encoding"></param>
        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }
    }
}
