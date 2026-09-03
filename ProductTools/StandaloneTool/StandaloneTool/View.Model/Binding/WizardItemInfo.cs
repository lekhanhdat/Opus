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
using AvePoint.Deployment.CommonGUI;
using StandaloneTool.Model;
using StandaloneTool.View.Model.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandaloneTool.View.Model.Binding
{
    public class WizardItemInfo : PageWizardItem
    {

        #region WizardOperator

        private WizardOperator mWizardOperator;

        public WizardOperator WizardOperator
        {
            get
            {
                if (mWizardOperator == null)
                {
                    mWizardOperator = new WizardOperator(this);
                }
                return mWizardOperator;
            }
            set
            {
                mWizardOperator = value;
                OnPropertyChanged("WizardOperator");
            }
        }

        #endregion

        #region PageLocation

        private string pageLocation;

        public string PageLocation
        {
            get { return pageLocation; }
            set
            {
                pageLocation = value;
                OnPropertyChanged("PageLocation");
            }
        }

        #endregion

        #region PageFeatures

        private PageFeatures pageFeatures;

        public PageFeatures PageFeatures
        {
            get { return pageFeatures; }
            set
            {
                pageFeatures = value;
                OnPropertyChanged("PageFeatures");
            }
        }

        #endregion
    }
}
