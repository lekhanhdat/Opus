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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveForm : IAveForm
    {
        private SPForm mForm;

        public AveForm(SPForm form)
        {
            mForm = form;
        }

        #region IAveForm Members

        public Guid ID
        {
            get { return mForm.ID; }
        }

        public string TemplateName
        {
            get { return mForm.TemplateName; }
        }

        public string Url
        {
            get { return mForm.Url; }
        }

        public string ServerRelativeUrl
        {
            get { return mForm.ServerRelativeUrl; }
        }

        #endregion

        public string ToolbarTemplateName
        {
            get { return mForm.ToolbarTemplateName; }
        }

        public AvePAGETYPE Type
        {
            get { return (AvePAGETYPE)mForm.Type; }
        }
    }
}
