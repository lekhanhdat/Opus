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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveColor : AveClientObject, IAveColor
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveColor));

        public AveColor() { }

        public AveColor(Dictionary<string, object> colorProp)
        {
            base.DataCache.AddPropertyies(colorProp);
        }

        public string AccessibleDescription
        {
            get { return string.Empty; }
        }

        public Dictionary<string, IAveThemeColor> Colors
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, IAveThemeColor>>("Colors");
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

        public string PreviewSlot3
        {
            get
            {
                return base.DataCache.GetProperty<string>("PreviewSlot3");
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IAveColor> GetColorPalettesFromFolder(IAveSite site, string strThemeFolder, bool recursive)
        {
            string themeGalleryUrl = AveThmxTheme.GetThemeGalleryUrl(site);
            string folderUrl = string.IsNullOrEmpty(strThemeFolder) ? themeGalleryUrl : (themeGalleryUrl + "/" + strThemeFolder);
            return GetColorPalettesFromFolder(site.RootWeb, folderUrl, recursive).AsReadOnly();
        }

        private List<IAveColor> GetColorPalettesFromFolder(IAveWeb themesWeb, string folderUrl, bool recursive)
        {
            List<IAveColor> colorPalettes = null;
            IAveFolder colorPalettesFolder = themesWeb.GetFolder(folderUrl);
            if (!colorPalettesFolder.Exists)
            {
                throw new ArgumentException("Unable to find an SPFolder at folderUrl: " + folderUrl);
            }
            colorPalettes = new List<IAveColor>(colorPalettesFolder.Files.Count);
            AddColorPalettes(colorPalettesFolder, colorPalettes, recursive);
            return colorPalettes;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "spcolor is a part of value")]
        private void AddColorPalettes(IAveFolder colorPalettesFolder, List<IAveColor> colorPalettes, bool recursive)
        {
            foreach (IAveFile file in colorPalettesFolder.Files)
            {
                if (file.ServerRelativeUrl.EndsWith(".spcolor", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        IAveColor item = Open(file, true);
                        colorPalettes.Add(item);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Debug("Failed to open color palette file at: {0}, message: {1}.", new object[] { file.ServerRelativeUrl, ex.Message });
                        continue;
                    }
                }
            }
            if (recursive)
            {
                foreach (IAveFolder folder in colorPalettesFolder.SubFolders)
                {
                    AddColorPalettes(folder, colorPalettes, true);
                }
            }
        }

        public IAveColor Open(IAveFile file)
        {
            return Open(file, false);
        }

        public IAveColor Open(IAveFile file, bool readPublishedVersion)
        {
            AveColor color = null;
            if ((file != null) && file.Exists)
            {
                Stream stream = file.OpenBinaryStream();
                if (stream == null)
                {
                    return null;
                }
                Dictionary<string, object> colorProp = new Dictionary<string, object>();
                colorProp["ServerRelativeUrl"] = file.ServerRelativeUrl;
                Initialize(stream, colorProp);
                color = new AveColor(colorProp);
            }
            return color;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "dml is a part of keys")]
        private void Initialize(Stream stream, Dictionary<string, object> colorProp)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlReader reader = XmlReader.Create(stream);
                xmlDocument.Load(reader);
                XPathNavigator navigator = xmlDocument.CreateNavigator();
                XmlNamespaceManager namespaces = new XmlNamespaceManager(navigator.NameTable);
                namespaces.AddNamespace("dml", "http://schemas.microsoft.com/sharepoint/");
                XPathNavigator navigator1 = navigator.SelectSingleNode("./dml:colorPalette/@isInverted", namespaces);
                //if (navigator1 == null)
                //{
                //    this.IsInverted = false;
                //}
                //else
                //{
                //    this.IsInverted = navigator1.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                //}
                XPathNavigator navigator2 = navigator.SelectSingleNode("./dml:colorPalette/@previewSlot1", namespaces);
                if (navigator2 == null)
                {
                    throw new XmlException("Error when parsing color XML: previewSlot1 attribute must be defined on the colorPalette tag.");
                }
                colorProp["PreviewSlot1"] = navigator2.Value;
                XPathNavigator navigator3 = navigator.SelectSingleNode("./dml:colorPalette/@previewSlot2", namespaces);
                if (navigator3 == null)
                {
                    throw new XmlException("Error when parsing color XML: previewSlot2 attribute must be defined on the colorPalette tag.");
                }
                colorProp["PreviewSlot2"] = navigator3.Value;
                XPathNavigator navigator4 = navigator.SelectSingleNode("./dml:colorPalette/@previewSlot3", namespaces);
                if (navigator4 == null)
                {
                    throw new XmlException("Error when parsing color XML: previewSlot3 attribute must be defined on the colorPalette tag.");
                }
                colorProp["PreviewSlot3"] = navigator4.Value;
                XPathNodeIterator iterator = navigator.Select("./dml:colorPalette/dml:color", namespaces);
                Dictionary<string, IAveThemeColor> colors = new Dictionary<string, IAveThemeColor>(iterator.Count);
                foreach (XPathNavigator navigator5 in iterator)
                {
                    string key = navigator5.SelectSingleNode("./@name", namespaces).Value;
                    string str2 = navigator5.SelectSingleNode("./@value", namespaces).Value;
                    if (colors.ContainsKey(key))
                    {
                        mLogger.Debug("A color node with name \"{0}\" already exists in this SPColor file", new object[] { key });
                        throw new Exception("Invalid color scheme XML.");
                    }
                    colors.Add(key, new AveThemeColor(str2));
                }
                colorProp["Colors"] = colors;
            }
            catch (XmlException exception)
            {
                mLogger.Debug("Invalid color scheme XML");
                throw new Exception("Invalid color scheme XML.", exception);
            }
        }

    }
}
