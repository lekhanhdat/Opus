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
using System.IO;
using System.Xml;
using AvePoint.Common;
using System.Globalization;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveUserMapping
    {
        private Dictionary<string, string> mUserMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> UserMapping
        {
            get { return mUserMapping; }
        }

        private Dictionary<string, string> mDomainMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> DomainMapping
        {
            get { return mDomainMapping; }
        }

        private string mDefaultUserLogin = string.Empty;
        public string DefaultUserLogin
        {
            get { return mDefaultUserLogin; }
        }

        public void LoadMappings(string filePath)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filePath);

            LoadMappings(xDoc);
        }

        public void LoadMappings(XmlDocument xDoc)
        {
            XmlElement root = xDoc.DocumentElement;
            foreach (XmlElement xe in root.ChildElements())
            {
                if (xe.Name == "UserMappings")
                {
                    LoadUserMapping(xe);
                }
                else if (xe.Name == "DomainMappings")
                {
                    LoadDomainMapping(xe);
                }
            }
        }

        public void Init()
        {
            XmlDocument xdoc = new XmlDocument();
            string mPath = AveEnv.AgentDataFolder + "\\SP2010\\SP2010ItemConfiguration.cfg";
            xdoc.Load(mPath);
            XmlElement root = xdoc.DocumentElement;
            foreach (XmlElement xe in root.ChildElements())
            {
                if (xe.Name == "ListMappings")
                {

                }
                else if (xe.Name == "UserMappings")
                {
                    LoadUserMapping(xe);
                }
                else if (xe.Name == "DomainMappings")
                {
                    LoadDomainMapping(xe);
                }
            }
        }
        private void LoadDomainMapping(XmlElement xe)
        {
            XmlNode rootNode = (XmlNode)xe;

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string source = node.Attributes["SourceDomain"].Value;
                string destination = node.Attributes["DestinationDomain"].Value;
                if (!mDomainMapping.ContainsKey(source))
                {
                    mDomainMapping.Add(source, destination);
                }
            }
        }
        private void LoadUserMapping(XmlElement xe)
        {
            XmlNode rootNode = (XmlNode)xe;

            if (rootNode.Attributes["defaultUser"] != null && rootNode.Attributes["defaultUser"].Value != string.Empty)
            {
                mDefaultUserLogin = rootNode.Attributes["defaultUser"].Value;
            }

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string source = node.Attributes["SourceUser"].Value;
                string destination = node.Attributes["DestinationUser"].Value;
                if (!mUserMapping.ContainsKey(source))
                {
                    mUserMapping.Add(source, destination);
                }
            }
        }
        public string GetMappedName(string oldName, ref bool _isDefault)
        {
            string rs = oldName;
            string name = oldName.ToLower(CultureInfo.InvariantCulture);
            string domain = string.Empty;
            if (name.IndexOf("/",StringComparison.OrdinalIgnoreCase) >= 0)
            {
                domain = name.Substring(0, name.IndexOf("/",StringComparison.OrdinalIgnoreCase));
            }

            if (mUserMapping.Count != 0 && mUserMapping.ContainsKey(name))
            {
                rs = (string)mUserMapping[name];
                return rs;
            }
            if (mDomainMapping.Count != 0 && mDomainMapping.ContainsKey(name))
            {
                rs = name.Replace(domain, (string)mDomainMapping[domain]);
                return rs;
            }
            if (mDefaultUserLogin != string.Empty)
            {
                _isDefault = true;
                return mDefaultUserLogin;
            }
            return rs;
        }
    }
}
