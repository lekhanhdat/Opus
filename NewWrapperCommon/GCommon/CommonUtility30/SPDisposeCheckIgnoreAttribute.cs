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




//--------------------------------------------------------------------------------
// This file is a "Sample" as part of the MICROSOFT SDK SAMPLES FOR SHAREPOINT
// PRODUCTS AND TECHNOLOGIES
//
// (c) 2008 Microsoft Corporation.  All rights reserved.  
//
// This source code is intended only as a supplement to Microsoft
// Development Tools and/or on-line documentation.  See these other
// materials for detailed information regarding Microsoft code samples.
// 
// THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//--------------------------------------------------------------------------------

using System;

namespace SPDisposeCheck
{

    public enum SPDisposeCheckID
    {
        // SPDisposeCheckIDs.
        _000 = 0,   //UNDEFINED
        _100 = 100, //Microsoft.SharePoint.SPList.BreakRoleInheritance() method
        _110 = 110, //Microsoft.SharePoint.SPSite new() operator
        _120 = 120, //Microsoft.SharePoint.SPSite.OpenWeb()
        _130 = 130, //Microsoft.SharePoint.SPSite.AllWebs[] indexer
        _140 = 140, //Microsoft.SharePoint.SPSite.RootWeb, LockIssue, Owner, and SecondaryContact properties
        _150 = 150, //Microsoft.SharePoint.SPSite.AllWebs.Add() method
        _160 = 160, //Microsoft.SharePoint.SPWeb.GetLimitedWebPartManager() method
        _170 = 170, //Microsoft.SharePoint.SPWeb.ParentWeb property
        _180 = 180, //Microsoft.SharePoint.SPWeb.Webs property
        _190 = 190, //Microsoft.SharePoint.SPWeb.Webs.Add() method
        _200 = 200, //Microsoft.SharePoint.SPWebCollection.Add() method 
        _210 = 210, //Microsoft.SharePoint.WebControls.SPControl GetContextSite() and GetContextWeb() methods
        _220 = 220, //Microsoft.SharePoint.SPContext Current.Site / SPContext.Site and SPContext.Current.Web / SPContext.Web properties
        _230 = 230, //Microsoft.SharePoint.Administration.SPSiteCollection[] indexer
        _240 = 240, //Microsoft.SharePoint.Administration.Add() method
        _300 = 300, //Microsoft.SharePoint.Publishing.GetPublishingWebs() method
        _310 = 310, //Microsoft.SharePoint.Publishing.PublishingWebCollection.Add() method
        _320 = 320, //Microsoft.SharePoint.Publishing.PublishingWeb.GetVariation() method 
        _400 = 400, //Microsoft.Office.Server.UserProfiles.PersonalSite property 
        _500 = 500, //Microsoft.SharePoint.Portal.SiteData.AreaManager.GetArea() method
        _999 = 999  //All
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly | AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
    public class SPDisposeCheckIgnoreAttribute : Attribute
    {
        public SPDisposeCheckIgnoreAttribute(SPDisposeCheckID Id, string Reason)
        {
            _id = Id;
            _reason = Reason;
        }

        protected SPDisposeCheckID _id;
        protected string _reason;

        public SPDisposeCheckID Id
        {
            get { return _id; }
            set { _id = Id; }
        }

        public string Reason
        {
            get { return _reason; }
            set { _reason = Reason; }
        }
    }

}