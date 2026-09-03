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
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveForm : AveClientObject, IAveForm
    {
        public AveForm()
        {

        }

        #region IAveForm Members

        public Guid ID
        {
            get {
                return base.DataCache.GetProperty<Guid>("ID");
            }
        }

        public string TemplateName
        {
            get {
                return base.DataCache.GetProperty<string>("TemplateName");
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return default(string);
            }
        }

        #endregion

        public string ToolbarTemplateName
        {
            get { return base.DataCache.GetProperty<string>("ToolbarTemplateName"); }
        }

        public AvePAGETYPE Type
        {
            get { return base.DataCache.GetProperty<AvePAGETYPE>("Type"); }
        }
    }
}
