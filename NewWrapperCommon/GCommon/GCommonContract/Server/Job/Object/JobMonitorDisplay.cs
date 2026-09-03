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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{

    public class DisplayConstnats
    {
        public const string Column = "Column";
        public const string Columns = "Columns";
        public const string Name = "Name";
        public const string I18N = "I18N";
        
        public const string Type = "Type";
        public const string Seq = "Seq";
        public const string IsDisplayed = "IsDisplayed";
        public const string IsSortable = "IsSortable";
        public const string IsFilterable = "IsFilterable";
        public const string Module = "Module";
        public const string Modules = "Modules";
        public const string JobMonitorDisplay = "JobMonitorDisplay";
        public const string Converter = "Converter";
    }

    [DataContract]
    [XmlRoot(DisplayConstnats.JobMonitorDisplay)]
    public class JobMonitorDisplay
    {
        private List<Module> modules;

        [DataMember]
        [XmlArray(ElementName = DisplayConstnats.Modules)]
        public List<Module> Modules
        { 
            get
            {
                if (modules == null)
                {
                    modules = new List<Module>();
                }
                return modules;
            }
            set
            {
                modules = value;
            }
        }
    }

    [DataContract]
    [XmlRoot(DisplayConstnats.Module)]
    public class Module
    {
        [DataMember]
        [XmlAttribute(DisplayConstnats.Name)]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.Type)]
        public int Type { get; set; }

        [DataMember]
        [XmlIgnoreAttribute]
        public int TotalLength { get; set; }

        private List<Column> cols;

        private List<BaseJobDto> values;

        [DataMember]
        [XmlIgnoreAttribute]
        public List<BaseJobDto> Values
        {
            get
            {
                if (values == null)
                {
                    values = new List<BaseJobDto>();
                }
                return values;
            }
            set
            {
                values = value;
            }
        }

        [DataMember]
        [XmlArray(ElementName=DisplayConstnats.Columns)]
        public List<Column> Columns 
        { 
            get
            {
                if (cols == null)
                {
                    cols = new List<Column>();
                }
                return cols;
            }
            set
            {
                cols = value;
            }
        }

        public Module ShallowCopy()
        {
            Module m = new Module();
            m.Name = this.Name;
            m.Type = this.Type;
            m.cols = this.cols;
           
            return m;
        }
    }

    [DataContract]
    [XmlRoot(DisplayConstnats.Column)]
    public class Column
    {
        [DataMember]
        [XmlAttribute(DisplayConstnats.Name)]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.I18N)]
        public string I18N { get; set; }

        [DataMember]
        [XmlIgnoreAttribute]
        public string DispName { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.Seq)]
        public int Seq { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.IsDisplayed)]
        public bool IsDisplayed { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.IsSortable)]
        public bool IsSortable { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.IsFilterable)]
        public bool IsFilterable { get; set; }

        [DataMember]
        [XmlAttribute(DisplayConstnats.Converter)]
        public string Converter { get; set; }

        //[DataMember]
        //[XmlIgnoreAttribute]
        //public Type ValueType { get; set; }

        /*
        [DataMember]
        [XmlIgnoreAttribute]
        public object Value {get; set;}
         */ 

        public Column ShallowCopy()
        {
            Column col = new Column();
            col.Name = this.Name;
            col.Seq = this.Seq;
            col.IsDisplayed = this.IsDisplayed;
            col.IsSortable = this.IsSortable;
            col.IsFilterable = this.IsFilterable;
            col.I18N = this.I18N;
            col.Converter = this.Converter;
            //col.Value = this.Value;
            return col;
        }

        public static int CompareBySeq(Column c1, Column c2)
        {
            return c1.Seq - c2.Seq;
        }

    }
}

