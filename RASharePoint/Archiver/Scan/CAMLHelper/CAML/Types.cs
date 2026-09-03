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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.CAMLHelper
{
    /// <summary>
    /// Class that exposes enumerations of values used by the <see cref="T:SharePointStu.CAMLHelper.CAML.CAMLManager"/> object.
    /// </summary>
    public class Types
    {
        #region Constructor

        /// <summary>
        /// public constructor.
        /// </summary>
        public Types()
        {
        }

        #endregion Constructor

        #region Public Enumerators

        /// <summary>
        /// Available field types.
        /// </summary>
        public enum FieldTypes
        {
            /// <summary>
            /// Text field.
            /// </summary>
            Text,
            /// <summary>
            /// Numeric field.
            /// </summary>
            Number,
            /// <summary>
            /// YesNo field.
            /// </summary>
            YesNo,
            /// <summary>
            /// Computed field.
            /// </summary>
            Computed,
            /// <summary>
            /// User field.
            /// </summary>
            User,
            /// <summary>
            /// DateTime field.
            /// </summary>
            DateTime,
            /// <summary>
            /// Counter field.
            /// </summary>
            Counter,
            /// <summary>
            /// Attachment field.
            /// </summary>
            Attachments,
            /// <summary>
            /// Lookup field.
            /// </summary>
            Lookup,
            /// <summary>
            /// File field.
            /// </summary>
            File,
            /// <summary>
            /// MetaData Service field
            /// </summary>
            MMSData,
            /// <summary>
            /// ID field
            /// </summary>
            Integer
        }

        /// <summary>
        /// Available query types.
        /// </summary>
        public enum QueryTypes
        {
            /// <summary>
            /// Equals.
            /// </summary>
            Eq,
            /// <summary>
            /// Does not equal.
            /// </summary>
            Neq,
            /// <summary>
            /// Greater than.
            /// </summary>
            Gt,
            /// <summary>
            /// Greater than or equal to.
            /// </summary>
            Geq,
            /// <summary>
            /// Less than.
            /// </summary>
            Lt,
            /// <summary>
            /// Less than or equal to.
            /// </summary>
            Leq,
            /// <summary>
            /// Begins with.
            /// </summary>
            BeginsWith,
            /// <summary>
            /// Contains.
            /// </summary>
            Contains,
            /// <summary>
            /// (startTime < value < endTime)
            /// </summary>
            FromTo,
            /// <summary>
            /// startTime <= value < endTime
            /// </summary>
            FromTo_1_0,
            /// <summary>
            /// startTime < value <= endTime
            /// </summary>
            FromTo_0_1,
            /// <summary>
            /// endTime <= value <= startTime
            /// </summary>
            FromTo_1_1,
            /// <summary>
            /// Is null.
            /// </summary>
            IsNull,
            /// <summary>
            /// Is not null.
            /// </summary>
            IsNotNull,
            /// <summary>
            /// More than two comparision
            /// </summary>
            In
        }

        /// <summary>
        /// Available join types.
        /// </summary>
        public enum JoinTypes
        {
            /// <summary>
            /// And.
            /// </summary>
            And,
            /// <summary>
            /// Or.
            /// </summary>
            Or
        }

        /// <summary>
        /// Available field reference types.
        /// </summary>
        public enum FieldRefTypes
        {
            /// <summary>
            /// Use the name of the field as the reference type.
            /// </summary>
            Name,
            /// <summary>
            /// Use the identifier of the field as the reference type.
            /// </summary>
            ID
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ScopeTypes
        {
            /// <summary>
            /// 显示指定文件夾下的item及子文件夾
            /// </summary>
            Default,
            /// <summary>
            /// 只显示指定文件夾下的item
            /// </summary>
            FilesOnly,
            /// <summary>
            /// 显示所有item,不显示文件夾
            /// </summary>
            Recursive,
            /// <summary>
            /// 显示所有item和所有子文件夾 
            /// </summary>
            RecursiveAll
        }

        #endregion Public Enumerators
    }
}
