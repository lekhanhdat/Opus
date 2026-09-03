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
using System.Xml.XPath;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveFont : AveClientObject, IAveFont
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveFont));

        public AveFont() { }

        public AveFont(Dictionary<string, object> fontProp)
        {
            base.DataCache.AddPropertyies(fontProp);
        }

        public Dictionary<string, IAveThemeFont> FontSlots
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, IAveThemeFont>>("FontSlots");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public string PreviewSlot1
        {
            get
            {
                return base.DataCache.GetProperty<string>("PreviewSlot1");
            }
        }

        public string PreviewSlot2
        {
            get
            {
                return base.DataCache.GetProperty<string>("PreviewSlot2");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public IAveThemeFont GetFont(string slot)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IAveFont> GetFontSchemesFromFolder(IAveSite site, string strThemeFolder)
        {
            string themeGalleryUrl = AveThmxTheme.GetThemeGalleryUrl(site);
            string folderUrl = string.IsNullOrEmpty(strThemeFolder) ? themeGalleryUrl : (themeGalleryUrl + "/" + strThemeFolder);
            return GetFontSchemesFromFolder(site.RootWeb, folderUrl).AsReadOnly();
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "spfont is a part of value")]
        private List<IAveFont> GetFontSchemesFromFolder(IAveWeb themesWeb, string folderUrl)
        {
            List<IAveFont> list = null;
            IAveFolder folder = themesWeb.GetFolder(folderUrl);
            if (!folder.Exists)
            {
                throw new ArgumentException("Unable to find an SPFolder at folderUrl: " + folderUrl);
            }
            list = new List<IAveFont>(folder.Files.Count);
            foreach (IAveFile file in folder.Files)
            {
                if (file.ServerRelativeUrl.EndsWith(".spfont", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        IAveFont item = Open(file, true);
                        list.Add(item);
                        continue;
                    }
                    catch (Exception exception)
                    {
                        mLogger.Debug("Failed to open font scheme file at: {0}, message: {1}.", new object[] { file.ServerRelativeUrl, exception.Message });
                        continue;
                    }
                }
            }
            return list;
        }

        public IAveFont Open(IAveFile file)
        {
            return Open(file, false);
        }

        public IAveFont Open(IAveFile file, bool readPublishedVersion)
        {
            IAveFont font = null;
            if ((file != null) && file.Exists)
            {
                Stream stream = file.OpenBinaryStream();
                Dictionary<string, object> fontProp = new Dictionary<string, object>();
                fontProp["ServerRelativeUrl"] = file.ServerRelativeUrl;
                Initialize(stream, fontProp);
                font = new AveFont(fontProp);
            }
            return font;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "latin is a part of xpath and dml is a part of prefix")]
        private void Initialize(Stream stream, Dictionary<string, object> fontProp)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlReader reader = XmlReader.Create(stream);
                xmlDocument.Load(reader);
                XPathNavigator navigator = xmlDocument.CreateNavigator();
                XmlNamespaceManager namespaces = new XmlNamespaceManager(navigator.NameTable);
                namespaces.AddNamespace("dml", "http://schemas.microsoft.com/sharepoint/");
                XPathNavigator navigator1 = navigator.SelectSingleNode("./dml:fontScheme/@name", namespaces);
                if (navigator1 == null)
                {
                    throw new XmlException("Error when parsing font XML: name attribute must be defined on the fontScheme tag.");
                }
                fontProp["Name"] = navigator1.Value;
                XPathNavigator navigator2 = navigator.SelectSingleNode("./dml:fontScheme/@previewSlot1", namespaces);
                if (navigator2 == null)
                {
                    throw new XmlException("Error when parsing font XML: previewSlot1 attribute must be defined on the fontScheme tag.");
                }
                fontProp["PreviewSlot1"] = navigator2.Value;
                XPathNavigator navigator3 = navigator.SelectSingleNode("./dml:fontScheme/@previewSlot2", namespaces);
                if (navigator3 == null)
                {
                    throw new XmlException("Error when parsing font XML: previewSlot2 attribute must be defined on the fontScheme tag.");
                }
                fontProp["PreviewSlot2"] = navigator3.Value;
                XPathNodeIterator iterator = navigator.Select("./dml:fontScheme/dml:fontSlots/dml:fontSlot", namespaces);
                Dictionary<string, IAveThemeFont> fontSlots = new Dictionary<string, IAveThemeFont>(iterator.Count);
                foreach (XPathNavigator navigator4 in iterator)
                {
                    string currentLanguageFont = string.Empty;
                    foreach (XPathNavigator n1 in navigator4.Select("./dml:latin", namespaces))
                    {
                        currentLanguageFont = n1.GetAttribute("typeface", string.Empty);
                    }
                    fontSlots.Add(navigator4.SelectSingleNode("./@name", namespaces).Value, new AveThemeFont(currentLanguageFont));
                }
                fontProp["FontSlots"] = fontSlots;
            }
            catch (XmlException exception)
            {
                mLogger.Debug("Invalid font scheme XML");
                throw new Exception("Invalid font scheme XML.", exception);
            }
        }
    }
}
