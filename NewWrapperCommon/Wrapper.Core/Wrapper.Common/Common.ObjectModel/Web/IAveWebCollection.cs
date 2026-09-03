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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace AvePoint.Wrapper.Common
{
    public interface IAveWebCollection : ICollection, IEnumerable<IAveWeb>, IEnumerable
    {
        IAveWeb Add(AveWebCreationInformation webCreationInfo);
        IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, IAveWebTemplate WebTemplate, bool useUniquePermissions, bool bConvertIfThere);
        IAveWeb Add(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere);
        IAveWeb this[string name] { get; }
        IAveWeb this[Guid webId] { get; }
        IAveWeb this[int index] { get; }
        string WebUrlFromPageUrl(string pageUrl);
        XmlNode GetWeb(string siteDirUrl);
    }

    public class AveWebCreationInformation
    {
        private string mdescription;
        private int mlanguage;
        private string mtitle;
        private string murl;
        private bool museSamePermissionsAsParentSite;
        private string mwebTemplate;

        public string Description
        {
            get
            {
                return this.mdescription;
            }
            set
            {
                this.mdescription = value;
            }
        }

        public int Language
        {
            get
            {
                return this.mlanguage;
            }
            set
            {
                this.mlanguage = value;
            }
        }

        public string Title
        {
            get
            {
                return this.mtitle;
            }
            set
            {
                this.mtitle = value;
            }
        }

        public string Url
        {
            get
            {
                return this.murl;
            }
            set
            {
                this.murl = value;
            }
        }

        public bool UseSamePermissionsAsParentSite
        {
            get
            {
                return this.museSamePermissionsAsParentSite;
            }
            set
            {
                this.museSamePermissionsAsParentSite = value;
            }
        }

        public string WebTemplate
        {
            get
            {
                return this.mwebTemplate;
            }
            set
            {
                this.mwebTemplate = value;
            }
        }

    }
}
