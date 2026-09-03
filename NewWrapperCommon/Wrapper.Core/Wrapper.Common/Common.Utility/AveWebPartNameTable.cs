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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Common
{
    public class AveWebPartNameTable : XmlNameTable
    {
        //it is thread safe
        private static string[] predefinedStrings = new string[150];
        private static Hashtable _table = new Hashtable();
        private static AveWebPartNameTable m_table = new AveWebPartNameTable();        

        // Methods
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        private AveWebPartNameTable()
        {
            AddPredefinedString(0, "http://schemas.microsoft.com/WebPart/v2");
            AddPredefinedString(1, "Dir");
            AddPredefinedString(2, "Description");
            AddPredefinedString(3, "Encoding");
            AddPredefinedString(4, "Title");
            AddPredefinedString(5, "WebPart");
            AddPredefinedString(6, "IsIncluded");
            AddPredefinedString(7, "Zone");
            AddPredefinedString(8, "ZoneID");
            AddPredefinedString(9, "PartOrder");
            AddPredefinedString(10, "NumberLimit");
            AddPredefinedString(11, "FrameState");
            AddPredefinedString(12, "Height");
            AddPredefinedString(13, "Width");
            AddPredefinedString(14, "Toolbar");
            AddPredefinedString(15, "ContentLink");
            AddPredefinedString(0x10, "DisplayName");
            AddPredefinedString(0x11, "DataFields");
            AddPredefinedString(0x12, "DataQuery");
            AddPredefinedString(0x13, "XSLLink");
            AddPredefinedString(20, "XSL");
            AddPredefinedString(0x15, "AllowRemove");
            AddPredefinedString(0x16, "AllowMinimize");
            AddPredefinedString(0x17, "IsVisible");
            AddPredefinedString(0x18, "Namespace");
            AddPredefinedString(0x19, "ViewFlag");
            AddPredefinedString(0x1a, "DetailLink");
            AddPredefinedString(0x1b, "HelpLink");
            AddPredefinedString(0x1c, "PartStorage");
            AddPredefinedString(0x1d, null);
            AddPredefinedString(30, null);
            AddPredefinedString(0x1f, "PartImageSmall");
            AddPredefinedString(0x20, "PartImageLarge");
            AddPredefinedString(0x21, "Assembly");
            AddPredefinedString(0x22, "TypeName");
            AddPredefinedString(0x23, null);
            AddPredefinedString(0x24, null);
            AddPredefinedString(0x25, "FrameType");
            AddPredefinedString(0x26, "Connections");
            AddPredefinedString(0x27, "MissingAssembly");
            AddPredefinedString(40, "Name");
            AddPredefinedString(0x29, "");
            AddPredefinedString(0x2a, "xmlns");
            AddPredefinedString(0x2b, "AllowZoneChange");
            AddPredefinedString(0x2c, "ParamBindings");
            AddPredefinedString(0x2d, "FireInitialRow");
            AddPredefinedString(0x2e, null);
            AddPredefinedString(0x2f, "ImageLink");
            AddPredefinedString(0x30, null);
            AddPredefinedString(0x31, null);
            AddPredefinedString(50, "PostData");
            AddPredefinedString(0x33, "Tags");
            AddPredefinedString(0x34, "TagIndexes");
            AddPredefinedString(0x35, "RenderTags");
            AddPredefinedString(0x36, "RenderTagIndexes");
            AddPredefinedString(0x37, "LastUpdated");
            AddPredefinedString(0x38, "RefreshInterval");
            AddPredefinedString(0x39, "LastCached");
            AddPredefinedString(0x3a, null);
            AddPredefinedString(0x3b, "Content");
            AddPredefinedString(60, "ConnectionID");
            AddPredefinedString(0x3d, "http://www.w3.org/2001/XMLSchema");
            AddPredefinedString(0x3e, "http://www.w3.org/2001/XMLSchema-instance");
            AddPredefinedString(0x3f, "Normal");
            AddPredefinedString(0x40, "Minimized");
            AddPredefinedString(0x41, "Default");
            AddPredefinedString(0x42, "LeftToRight");
            AddPredefinedString(0x43, "RightToLeft");
            AddPredefinedString(0x44, "None");
            AddPredefinedString(0x45, "Standard");
            AddPredefinedString(70, "TitleBarOnly");
            AddPredefinedString(0x47, "true");
            AddPredefinedString(0x48, "false");
            AddPredefinedString(0x49, "xsi");
            AddPredefinedString(0x4a, "xsd");
            AddPredefinedString(0x4b, "NoDefaultStyle");
            AddPredefinedString(0x4c, "VerticalAlignment");
            AddPredefinedString(0x4d, "HorizontalAlignment");
            AddPredefinedString(0x4e, "BackgroundColor");
            AddPredefinedString(0x4f, "IsIncludedFilter");
            AddPredefinedString(80, "XML");
            AddPredefinedString(0x51, "XMLLink");
            AddPredefinedString(0x52, "HeaderCaption");
            AddPredefinedString(0x53, "HeaderTitle");
            AddPredefinedString(0x54, "HeaderDescription");
            AddPredefinedString(0x55, "Image");
            AddPredefinedString(0x56, "ContentHasToken");
            AddPredefinedString(0x57, "ExportControlledProperties");
            AddPredefinedString(0x58, "SourceType");
            AddPredefinedString(0x59, "Fields");
            AddPredefinedString(90, "http://schemas.microsoft.com/WebPart/v2/ContentEditor");
            AddPredefinedString(0x5b, "http://schemas.microsoft.com/WebPart/v2/PageViewer");
            AddPredefinedString(0x5c, "http://schemas.microsoft.com/WebPart/v2/Image");
            AddPredefinedString(0x5d, "http://schemas.microsoft.com/WebPart/v2/Xml");
            AddPredefinedString(0x5e, "http://schemas.microsoft.com/WebPart/v2/DataView");
            AddPredefinedString(0x5f, "http://schemas.microsoft.com/WebPart/v2/ListForm");
            AddPredefinedString(0x60, "http://schemas.microsoft.com/WebPart/v2/ListView");
            AddPredefinedString(0x61, null);
            AddPredefinedString(0x62, "http://schemas.microsoft.com/WebPart/v2/TitleBar");
            AddPredefinedString(0x63, "http://schemas.microsoft.com/WebPart/v2/SimpleForm");
            AddPredefinedString(100, "http://schemas.microsoft.com/WebPart/v2/Members");
            AddPredefinedString(0x65, "CacheDataStorage");
            AddPredefinedString(0x66, "CacheDataTimeout");
            AddPredefinedString(0x67, "CacheXslStorage");
            AddPredefinedString(0x68, "AlternativeText");
            AddPredefinedString(0x69, "DataSourceBindings");
            AddPredefinedString(0x6a, "Template");
            AddPredefinedString(0x6b, "http://schemas.microsoft.com/WebPart/v3");
            AddPredefinedString(0x6c, "ID");
            AddPredefinedString(0x6d, "AttachedPropertiesShared");
            AddPredefinedString(110, "AttachedPropertiesUser");
            AddPredefinedString(0x6f, "AllowConnect");
            AddPredefinedString(0x70, "AllowEdit");
            AddPredefinedString(0x71, "AllowHide");
            AddPredefinedString(0x72, "HelpMode");
            AddPredefinedString(0x73, "http://schemas.microsoft.com/WebPart/v2/UserTasks");
            AddPredefinedString(0x74, "http://schemas.microsoft.com/WebPart/v2/UserDocs");
            AddPredefinedString(0x75, "http://schemas.microsoft.com/WebPart/v2/Aggregation");
            AddPredefinedString(0x76, "QuerySiteCollection");
            AddPredefinedString(0x77, "MaxItemsShown");
            AddPredefinedString(120, "QueryLastModifiedBy");
            AddPredefinedString(0x79, "QueryCreatedBy");
            AddPredefinedString(0x7a, "QueryCheckedOutBy");
            AddPredefinedString(0x7b, "DisplayFolderColumn");
            AddPredefinedString(0x7c, "DisplayItemLinkColumn");
            AddPredefinedString(0x7d, "TitleUrl");
            AddPredefinedString(0x7e, "DisplayType");
            AddPredefinedString(0x7f, "MembershipGroupId");
            AddPredefinedString(0x80, "AllowClose");
            AddPredefinedString(0x81, "AuthorizationFilter");
            AddPredefinedString(130, "CatalogIconImageUrl");
            AddPredefinedString(0x83, "ChromeState");
            AddPredefinedString(0x84, "ChromeType");
            AddPredefinedString(0x85, "Direction");
            AddPredefinedString(0x86, "ExportMode");
            AddPredefinedString(0x87, "HelpUrl");
            AddPredefinedString(0x88, "Hidden");
            AddPredefinedString(0x89, "ImportErrorMessage");
            AddPredefinedString(0x8a, "IsClosed");
            AddPredefinedString(0x8b, "TitleIconImageUrl");
            AddPredefinedString(140, "ZoneIndex");
            AddPredefinedString(0x8d, "PersonalizableProperties");
            AddPredefinedString(0x8e, "NonPersonalizableProperties");
            AddPredefinedString(0x8f, "IPersonalizableProperties");
            AddPredefinedString(0x90, "AttachedProperties");
            AddPredefinedString(0x91, "LinkMap");
            AddPredefinedString(0x92, "Unknown");
            AddPredefinedString(0x93, "ViewContentTypeId");
            AddPredefinedString(0x94, "CssStyleSheet");
            AddPredefinedString(0x95, "ListName");
        }

        public override string Add(string array)
        {
            string str = this.Get(array);
            if (str != null)
            {
                return str;
            }
            lock (_table)
            {
                str = this.Get(array);
                if (str != null)
                {
                    return str;
                }
                _table[array] = new StringEntry(array);
                return array;
            }
        }

        public override string Add(char[] array, int offset, int length)
        {
            return this.Add(new string(array, offset, length));
        }

        private static void AddPredefinedString(ushort us, string s)
        {
            if (s != null)
            {
                predefinedStrings[us] = s;
                _table[s] = new StringEntry(s, us);
            }
        }

        public override string Get(string array)
        {
            StringEntry entry = (StringEntry)_table[array];
            if (entry == null)
            {
                return null;
            }
            return entry._s;
        }

        public override string Get(char[] array, int offset, int length)
        {
            return this.Get(new string(array, offset, length));
        }

        public static AveWebPartNameTable GlobalNameTable()
        {
            return m_table;
        }

        public string LookupPredefinedString(ushort us)
        {
            if (us >= predefinedStrings.Length)
            {
                return "";
            }
            return predefinedStrings[us];
        }

        // Nested Types
        public class StringEntry
        {
            // Fields
            public readonly ushort _predefinedConstant;
            public readonly string _s;

            // Methods
            public StringEntry(string s)
            {
                this._s = s;
                this._predefinedConstant = 0xffff;
            }

            public StringEntry(string s, ushort predefinedConstant)
                : this(s)
            {
                this._predefinedConstant = predefinedConstant;
            }
        }
    }
}
