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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOScope : AveClientObject, IAveOScope
    {
        private IAveRequest m_Request;

        public AveOScope(IDictionary<string, object> scopeProp, IAveRequest m_Request)
        {
            base.DataCache.AddPropertyies(scopeProp);
            this.m_Request = m_Request;
        }
        public IAveOScopeRuleCollection Rules
        {
            get { throw new NotImplementedException(); }
        }

        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("ID");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsShared
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsShared");
            }
        }

        public AveScopeCompilationState CompilationState
        {
            get
            {
                if (!base.DataCache.GetPropertyWithoutChange("CompilationState").ToString().Equals("Ready"))
                {
                    return AveScopeCompilationState.Empty;
                }
                return AveScopeCompilationState.Compiled;
            }
        }

        public int Count
        {
            get
            {
                return base.DataCache.GetProperty<int>("Count");
            }
        }

        public bool DisplayInAdminUI
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DisplayInAdminUI"))
                {
                    return true;
                }
                return base.DataCache.GetProperty<bool>("DisplayInAdminUI");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string LastModifiedBy
        {
            get
            {
                return base.DataCache.GetProperty<string>("LastModifiedBy");
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }


        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string AlternateResultsPage
        {
            get
            {
                return base.DataCache.GetProperty<string>("AlternateResultsPage");
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
