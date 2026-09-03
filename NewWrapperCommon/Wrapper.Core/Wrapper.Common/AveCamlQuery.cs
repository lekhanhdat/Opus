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
using System.Xml;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Common
{
    public class AveCamlQuery
    {
        private bool mdatesInUtc;
        private string mfolderServerRelativeUrl;
        private IAveListItemCollectionPosition mlistItemCollectionPosition;
        private string mviewXml;
        private string mviewFieldsXml;
        private string mqueryXml;
        private string mqueryOptionXml;

        public bool DatesInUtc
        {
            get
            {
                return this.mdatesInUtc;
            }
            set
            {
                this.mdatesInUtc = value;
            }
        }

        public string FolderServerRelativeUrl
        {
            get
            {
                return this.mfolderServerRelativeUrl;
            }
            set
            {
                this.mfolderServerRelativeUrl = value;
            }
        }


        public IAveListItemCollectionPosition ListItemCollectionPosition
        {
            get
            {
                return this.mlistItemCollectionPosition;
            }
            set
            {
                this.mlistItemCollectionPosition = value;

            }
        }

        public string ViewXml
        {
            get
            {
                return this.mviewXml;
            }
            set
            {
                this.mviewXml = value;
            }
        }
        public string ViewFieldsXml
        {
            get
            {
                return this.mviewFieldsXml;
            }
            set
            {
                this.mviewFieldsXml = value;
            }
        }
        public string QueryXml
        {
            get
            {
                return this.mqueryXml;
            }
            set
            {
                this.mqueryXml = value;
            }
        }
        public string QueryOptionXml
        {
            get
            {
                return this.mqueryOptionXml;
            }
            set
            {
                this.mqueryOptionXml = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in xml Query,Obj")]
        public static AveCamlQuery CreateAllFoldersQuery()
        {
            AveCamlQuery camlQuery = new AveCamlQuery();
            camlQuery.ViewXml = "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where></Query></View>";
            return camlQuery;
        }
        public static AveCamlQuery CreateAllItemsQuery()
        {
            AveCamlQuery camlQuery = new AveCamlQuery();
            camlQuery.ViewXml = "<View Scope=\"RecursiveAll\"><Query></Query></View>";
            camlQuery.ViewFieldsXml = "<ViewFields></ViewFields>";
            camlQuery.QueryXml = "<Query></Query>";
            camlQuery.QueryOptionXml = "<QueryOptions><Folder></Folder></QueryOptions>";
            camlQuery.FolderServerRelativeUrl = string.Empty;
            return camlQuery;
        }
        public static AveCamlQuery CreateAllItemsQuery(bool datesInUtc)
        {
            AveCamlQuery camlQuery = new AveCamlQuery();
            camlQuery.ViewXml = "<View Scope=\"RecursiveAll\"><Query></Query></View>";
            camlQuery.ViewFieldsXml = "<ViewFields></ViewFields>";
            camlQuery.QueryXml = "<Query></Query>";
            camlQuery.QueryOptionXml = "<QueryOptions><Folder></Folder></QueryOptions>";
            camlQuery.FolderServerRelativeUrl = string.Empty;
            camlQuery.DatesInUtc = datesInUtc;
            return camlQuery;
        }
        public static AveCamlQuery CreateAllItemsQuery(int rowLimit, params string[] viewFields)
        {
            AveCamlQuery camlQuery = new AveCamlQuery();
            if (rowLimit <= 0)
            {
                throw new ArgumentOutOfRangeException("rowLimit");
            }
            if (viewFields == null)
            {
                throw new ArgumentNullException("viewFields");
            }
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(output, settings);
            writer.WriteStartElement("View");
            writer.WriteAttributeString("Scope", "RecursiveAll");
            if (viewFields.Length > 0)
            {
                writer.WriteStartElement("ViewFields");
                foreach (string str in viewFields)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        writer.WriteStartElement("FieldRef");
                        writer.WriteAttributeString("Name", str);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
            writer.WriteElementString("RowLimit", rowLimit.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.Close();
            camlQuery.ViewXml = output.ToString();
            return camlQuery;
        }
        public string[] ToStringArray()
        {
            string[] returnInfo = new string[7];
            returnInfo[0] = this.ViewFieldsXml;
            returnInfo[1] = this.QueryXml;
            returnInfo[2] = this.QueryOptionXml;
            returnInfo[3] = this.ViewXml;
            returnInfo[4] = this.FolderServerRelativeUrl;
            if (this.ListItemCollectionPosition != null)
            {
                returnInfo[5] = this.ListItemCollectionPosition.PagingInfo;
            }
            else
            {
                returnInfo[5] = string.Empty;
            }
            returnInfo[6] = this.DatesInUtc.ToString();
            return returnInfo;
        }
    }

    public class AveItemCollectionPosition:IAveListItemCollectionPosition
    {
        private string mpagingInfo;

        public string PagingInfo
        {
            get
            {
                return this.mpagingInfo;
            }
            set
            {
                this.mpagingInfo = value;
            }
        }
    }

}
