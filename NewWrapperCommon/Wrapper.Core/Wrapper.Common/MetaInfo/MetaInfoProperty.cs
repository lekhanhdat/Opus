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
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public class MetaInfoProperty
    {
        // Fields
        private MetaInfoAccess m_Access;
        private Guid m_Id;
        private string m_Name;
        private string m_String;
        private string m_UpdateString;
        private MetaInfoValueType m_Type;
        private object m_Value;

        // Methods
        public MetaInfoProperty()
        {
            this.Access = MetaInfoAccess.ReadWrite;
            this.Type = MetaInfoValueType.String;
            this.Id = Guid.Empty;
        }       

        public MetaInfoProperty(XmlElement element)
        {
            this.Name = element.GetAttribute("Name");
            if (element.HasAttribute("Access"))
            {
                this.Access = (MetaInfoAccess) Enum.Parse(typeof(MetaInfoAccess), element.GetAttribute("Access"));
            }
            else
            {
                this.Access = MetaInfoAccess.ReadWrite;
            }
            if (element.HasAttribute("Type"))
            {
                this.Type = (MetaInfoValueType) Enum.Parse(typeof(MetaInfoValueType), element.GetAttribute("Type"));
            }
            else
            {
                this.Type = MetaInfoValueType.String;
            }
            if (element.HasAttribute("Id"))
            {
                this.Id = AvePoint.Common.Validator.IsGuid(element.GetAttribute("Id")) ? 
                          new Guid(element.GetAttribute("Id")) : 
                          Guid.Empty;
            }
            if (element.HasAttribute("Value"))
            {
                switch (this.Type)
                {
                    case MetaInfoValueType.Integer:
                        this.Value = int.Parse(element.GetAttribute("Value"), CultureInfo.InvariantCulture);
                        return;

                    case MetaInfoValueType.Time:
                    case MetaInfoValueType.FileSystemTime:
                        this.Value = DateTime.Parse(element.GetAttribute("Value"), CultureInfo.InvariantCulture);
                        return;

                    case MetaInfoValueType.Empty:
                        this.Value = string.Empty;
                        return;
                }
                this.Value = element.GetAttribute("Value");
            }
        }

        public MetaInfoProperty(string name, object value)
        {
            this.Access = MetaInfoAccess.ReadWrite;
            this.Id = Guid.Empty;
            this.Name = name;
            this.Type = MetaInfoHandler.ConvertObjectTypeToMataInfoType(value);
            this.Value = value;
        }

        public void Serialize(SerializationInfo info)
        {
            if (this.Access != MetaInfoAccess.NoAccess)
            {
                info.AddValue("Name", this.Name);
                info.AddValue("Type", this.Type.ToString());
                info.AddValue("Access", this.Access.ToString());
                info.AddValue("Value", this.Value);
                if (this.Id != Guid.Empty)
                {
                    info.AddValue("Id", this.Id);
                }
            }
        }

        // Properties
        public MetaInfoAccess Access
        {
            get
            {
                return this.m_Access;
            }
            set
            {
                this.m_Access = value;
            }
        }

        public Guid Id
        {
            get
            {
                return this.m_Id;
            }
            set
            {
                this.m_Id = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
            set
            {
                this.m_Name = value;
            }
        }

        public string TheString
        {
            get
            {
                if (this.m_String == null)
                {
                    this.m_String = string.Concat(new object[] { this.Name, ":", MetaInfoHandler.ConvertTypeToChar(this.Type), MetaInfoHandler.ConvertAccessToChar(this.Access), "|", this.Value, "\r\n" });
                }
                return this.m_String;
            }
            internal set
            {
                this.m_String = value;
            }
        }

        public string TheUpdateString
        {
            get
            {
                if (this.m_UpdateString == null)
                {
                    string value1 = null;
                    if (this.Type == MetaInfoValueType.Boolean)
                    {
                        value1 = this.Value.ToString().ToLower(CultureInfo.CurrentCulture);
                    }
                    else if (this.Type == MetaInfoValueType.String && !string.IsNullOrEmpty(this.Value as string))
                    {                        
                        value1 = (this.Value as string).Replace(@"\", @"\\").Replace(";", @"\;").Replace("=", @"\=");
                    }
                    this.m_UpdateString = string.Concat(new object[] { this.Name, ";", MetaInfoHandler.ConvertTypeToChar(this.Type), MetaInfoHandler.ConvertAccessToChar(this.Access), "|", value1, ";" });
                }
                return this.m_UpdateString;
            }
            internal set
            {
                this.m_UpdateString = value;
            }
        }

        public MetaInfoValueType Type
        {
            get
            {
                return this.m_Type;
            }
            set
            {
                this.m_Type = value;
            }
        }

        public object Value
        {
            get
            {
                return this.m_Value;
            }
            set
            {
                this.m_Value = value;
            }
        }

        public object ValueForWebProperties
        {
            get
            {
                if (this.Value != null)
                {
                    Type type = this.Value.GetType();
                    if (((type != typeof(string)) && (type != typeof(DateTime))) && ((type != typeof(int)) && (type != typeof(bool))))
                    {
                        return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { this.Value });
                    }
                    if ((type == typeof(string)) && (this.Type == MetaInfoValueType.Boolean))
                    {
                        return bool.Parse((string) this.Value);
                    }
                }
                return this.Value;
            }
        }
    }
}
