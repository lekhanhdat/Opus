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
    class AveLabel : AveClientObject, IAveLabel
    {
        private AveTerm m_AveTerm;
        private IAveRequest m_Request;

        public AveLabel(IAveRequest m_Request,string lableName, int lcid, bool isDefault, AveTerm term)
        {
            this.m_Request = m_Request;
            this.m_AveTerm = term;
            base.DataCache.AddChangedProperty("Value", lableName);
            base.DataCache.AddChangedProperty("Language", lcid);
            base.DataCache.AddChangedProperty("IsDefaultForLanguage", isDefault);
        }

        public AveLabel(IAveRequest m_Request, AveTerm m_AveTerm, Dictionary<string, object> labelProperties)
        {
            this.m_Request = m_Request;
            this.m_AveTerm = m_AveTerm;
            base.DataCache.AddPropertyies(labelProperties);
        }
        #region IAveLabel Members

        public bool IsDefaultForLanguage
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsDefaultForLanguage");
            }
        }

        public int Language
        {
            get
            {
                return base.DataCache.GetProperty<int>("Language");
            }
        }

        public string Value
        {
            get
            {

                return base.DataCache.GetProperty<string>("Value");
            }
        }

        #endregion
    }
}
