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
    /// Class that defines a single ViewField element of a CAML query.
    /// </summary>
    public class ViewField : BaseElement
    {
        #region Constructors

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.ViewField"/> object.
        /// </summary>
        /// <param name="field">The value of the field identifier to set the view field rule with.</param>
        /// <param name="fieldRefType">The <see cref="T:SharePointStu.CAMLHelper.CAML.Types.FieldRefTypes"/> object type of the field to be viewed.</param>
        public ViewField(string field, Types.FieldRefTypes fieldRefType)
            : base(field, fieldRefType)
        {
        }

        /// <summary>
        /// Initialise the <see cref="T:SharePointStu.CAMLHelper.CAML.ViewField"/> object using a field reference type of name.
        /// </summary>
        /// <param name="field">The value of the field identifier to set the view field rule with.</param>
        public ViewField(string field)
            : base(field, Types.FieldRefTypes.Name)
        {
        }

        #endregion Constructors
    }
}
