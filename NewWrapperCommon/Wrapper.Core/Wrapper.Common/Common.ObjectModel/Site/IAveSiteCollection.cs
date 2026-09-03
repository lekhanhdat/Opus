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
using System.Text;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSiteCollection : IEnumerable<IAveSite>, ICollection
    {
        IAveSite this[int index] { get; }//
        IAveSite this[Guid id] { get; }
        IAveSite this[string strSiteName] { get; }
        int Count { get; }
        string[] Names { get; }

        IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail);
        IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail);
        IAveSite Add(string siteUrl, string title, string description, uint nLCID, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail, bool useHostHeaderAsSiteName);
        //Add for SP2013
        IAveSite Add(string siteUrl, string title, string description, uint nLCID, int compatibilityLevel, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail);
        IAveSite Add(string siteUrl, string title, string description, uint nLCID, int compatibilityLevel, string webTemplate, string ownerLogin, string ownerName, string ownerEmail, string secondaryContactLogin, string secondaryContactName, string secondaryContactEmail, bool useHostHeaderAsSiteName);

        void Backup(string siteUrl, string filePath, bool overWrite);
        void Restore(string strSiteUrl, string strFilename, bool bOverwrite);
        void Restore(string strSiteUrl, string strFilename, bool bOverwrite, bool hostHeaderAsSiteName);
    }
}
