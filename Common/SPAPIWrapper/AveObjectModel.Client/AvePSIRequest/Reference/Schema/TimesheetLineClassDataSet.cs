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
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Office.Project.Server.Schema
{
	// Token: 0x02000748 RID: 1864
	[DesignerCategory("code")]
	[XmlRoot("TimesheetLineClassDataSet")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[Serializable]
	public class TimesheetLineClassDataSet : DataSet
	{
		// Token: 0x0600B425 RID: 46117 RVA: 0x00231B44 File Offset: 0x0022FD44
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Departments, new string[]
			{
				"TS_LINE_CLASS_DEPARTMENT_UID",
				"TS_LINE_CLASS_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.LineClasses, new string[]
			{
				"TS_LINE_CLASS_ORGANIZATION",
				"TS_LINE_CLASS_MULTILINE",
				"TS_LINE_CLASS_NAME",
				"TS_LINE_CLASS_IS_EDITABLE",
				"TS_LINE_CLASS_DESC",
				"TS_LINE_CLASS_IS_DISABLED",
				"TS_LINE_CLASS_TYPE",
				"MOD_DATE",
				"TS_LINE_CLASS_UID",
				"TS_LINE_CLASS_ALWAYS_DISPLAY",
				"TS_LINE_CLASS_NEED_APPROVAL"
			});
		}

		// Token: 0x0600B426 RID: 46118 RVA: 0x00231BE8 File Offset: 0x0022FDE8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public TimesheetLineClassDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600B427 RID: 46119 RVA: 0x00231C3C File Offset: 0x0022FE3C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected TimesheetLineClassDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
		{
			if (base.IsBinarySerialized(info, context))
			{
				this.InitVars(false);
				CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
				this.Tables.CollectionChanged += value;
				this.Relations.CollectionChanged += value;
				return;
			}
			string s = (string)info.GetValue("XmlSchema", typeof(string));
			if (base.DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
			{
				DataSet dataSet = new DataSet();
				dataSet.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
				if (dataSet.Tables["LineClasses"] != null)
				{
					base.Tables.Add(new TimesheetLineClassDataSet.LineClassesDataTable(dataSet.Tables["LineClasses"]));
				}
				if (dataSet.Tables["Departments"] != null)
				{
					base.Tables.Add(new TimesheetLineClassDataSet.DepartmentsDataTable(dataSet.Tables["Departments"]));
				}
				base.DataSetName = dataSet.DataSetName;
				base.Prefix = dataSet.Prefix;
				base.Namespace = dataSet.Namespace;
				base.Locale = dataSet.Locale;
				base.CaseSensitive = dataSet.CaseSensitive;
				base.EnforceConstraints = dataSet.EnforceConstraints;
				base.Merge(dataSet, false, MissingSchemaAction.Add);
				this.InitVars();
			}
			else
			{
				base.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
			}
			base.GetSerializationData(info, context);
			CollectionChangeEventHandler value2 = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value2;
			this.Relations.CollectionChanged += value2;
		}

		// Token: 0x170036F9 RID: 14073
		// (get) Token: 0x0600B428 RID: 46120 RVA: 0x00231DCB File Offset: 0x0022FFCB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public TimesheetLineClassDataSet.LineClassesDataTable LineClasses
		{
			get
			{
				return this.tableLineClasses;
			}
		}

		// Token: 0x170036FA RID: 14074
		// (get) Token: 0x0600B429 RID: 46121 RVA: 0x00231DD3 File Offset: 0x0022FFD3
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public TimesheetLineClassDataSet.DepartmentsDataTable Departments
		{
			get
			{
				return this.tableDepartments;
			}
		}

		// Token: 0x170036FB RID: 14075
		// (get) Token: 0x0600B42A RID: 46122 RVA: 0x00231DDB File Offset: 0x0022FFDB
		// (set) Token: 0x0600B42B RID: 46123 RVA: 0x00231DE3 File Offset: 0x0022FFE3
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override SchemaSerializationMode SchemaSerializationMode
		{
			get
			{
				return this._schemaSerializationMode;
			}
			set
			{
				this._schemaSerializationMode = value;
			}
		}

		// Token: 0x170036FC RID: 14076
		// (get) Token: 0x0600B42C RID: 46124 RVA: 0x00231DEC File Offset: 0x0022FFEC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x170036FD RID: 14077
		// (get) Token: 0x0600B42D RID: 46125 RVA: 0x00231DF4 File Offset: 0x0022FFF4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x0600B42E RID: 46126 RVA: 0x00231DFC File Offset: 0x0022FFFC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600B42F RID: 46127 RVA: 0x00231E10 File Offset: 0x00230010
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			TimesheetLineClassDataSet timesheetLineClassDataSet = (TimesheetLineClassDataSet)base.Clone();
			timesheetLineClassDataSet.InitVars();
			timesheetLineClassDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return timesheetLineClassDataSet;
		}

		// Token: 0x0600B430 RID: 46128 RVA: 0x00231E3C File Offset: 0x0023003C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600B431 RID: 46129 RVA: 0x00231E3F File Offset: 0x0023003F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600B432 RID: 46130 RVA: 0x00231E44 File Offset: 0x00230044
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["LineClasses"] != null)
				{
					base.Tables.Add(new TimesheetLineClassDataSet.LineClassesDataTable(dataSet.Tables["LineClasses"]));
				}
				if (dataSet.Tables["Departments"] != null)
				{
					base.Tables.Add(new TimesheetLineClassDataSet.DepartmentsDataTable(dataSet.Tables["Departments"]));
				}
				base.DataSetName = dataSet.DataSetName;
				base.Prefix = dataSet.Prefix;
				base.Namespace = dataSet.Namespace;
				base.Locale = dataSet.Locale;
				base.CaseSensitive = dataSet.CaseSensitive;
				base.EnforceConstraints = dataSet.EnforceConstraints;
				base.Merge(dataSet, false, MissingSchemaAction.Add);
				this.InitVars();
				return;
			}
			base.ReadXml(reader);
			this.InitVars();
		}

		// Token: 0x0600B433 RID: 46131 RVA: 0x00231F3C File Offset: 0x0023013C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600B434 RID: 46132 RVA: 0x00231F70 File Offset: 0x00230170
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600B435 RID: 46133 RVA: 0x00231F7C File Offset: 0x0023017C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableLineClasses = (TimesheetLineClassDataSet.LineClassesDataTable)base.Tables["LineClasses"];
			if (initTable && this.tableLineClasses != null)
			{
				this.tableLineClasses.InitVars();
			}
			this.tableDepartments = (TimesheetLineClassDataSet.DepartmentsDataTable)base.Tables["Departments"];
			if (initTable && this.tableDepartments != null)
			{
				this.tableDepartments.InitVars();
			}
		}

		// Token: 0x0600B436 RID: 46134 RVA: 0x00231FEC File Offset: 0x002301EC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "TimesheetLineClassDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/TimesheetLineClassDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableLineClasses = new TimesheetLineClassDataSet.LineClassesDataTable();
			base.Tables.Add(this.tableLineClasses);
			this.tableDepartments = new TimesheetLineClassDataSet.DepartmentsDataTable();
			base.Tables.Add(this.tableDepartments);
		}

		// Token: 0x0600B437 RID: 46135 RVA: 0x00232060 File Offset: 0x00230260
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeLineClasses()
		{
			return false;
		}

		// Token: 0x0600B438 RID: 46136 RVA: 0x00232063 File Offset: 0x00230263
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeDepartments()
		{
			return false;
		}

		// Token: 0x0600B439 RID: 46137 RVA: 0x00232066 File Offset: 0x00230266
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600B43A RID: 46138 RVA: 0x00232078 File Offset: 0x00230278
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			TimesheetLineClassDataSet timesheetLineClassDataSet = new TimesheetLineClassDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = timesheetLineClassDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = timesheetLineClassDataSet.GetSchemaSerializable();
			if (xs.Contains(schemaSerializable.TargetNamespace))
			{
				MemoryStream memoryStream = new MemoryStream();
				MemoryStream memoryStream2 = new MemoryStream();
				try
				{
					schemaSerializable.Write(memoryStream);
					foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
					{
						XmlSchema xmlSchema = (XmlSchema)obj;
						memoryStream2.SetLength(0L);
						xmlSchema.Write(memoryStream2);
						if (memoryStream.Length == memoryStream2.Length)
						{
							memoryStream.Position = 0L;
							memoryStream2.Position = 0L;
							while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
							{
							}
							if (memoryStream.Position == memoryStream.Length)
							{
								return xmlSchemaComplexType;
							}
						}
					}
				}
				finally
				{
					if (memoryStream != null)
					{
						memoryStream.Close();
					}
					if (memoryStream2 != null)
					{
						memoryStream2.Close();
					}
				}
			}
			xs.Add(schemaSerializable);
			return xmlSchemaComplexType;
		}

		// Token: 0x04002452 RID: 9298
		private TimesheetLineClassDataSet.LineClassesDataTable tableLineClasses;

		// Token: 0x04002453 RID: 9299
		private TimesheetLineClassDataSet.DepartmentsDataTable tableDepartments;

		// Token: 0x04002454 RID: 9300
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000749 RID: 1865
		// (Invoke) Token: 0x0600B43C RID: 46140
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void LineClassesRowChangeEventHandler(object sender, TimesheetLineClassDataSet.LineClassesRowChangeEvent e);

		// Token: 0x0200074A RID: 1866
		// (Invoke) Token: 0x0600B440 RID: 46144
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DepartmentsRowChangeEventHandler(object sender, TimesheetLineClassDataSet.DepartmentsRowChangeEvent e);

		// Token: 0x0200074B RID: 1867
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class LineClassesDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B443 RID: 46147 RVA: 0x002321C0 File Offset: 0x002303C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public LineClassesDataTable()
			{
				base.TableName = "LineClasses";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B444 RID: 46148 RVA: 0x002321E8 File Offset: 0x002303E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal LineClassesDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600B445 RID: 46149 RVA: 0x00232290 File Offset: 0x00230490
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected LineClassesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170036FE RID: 14078
			// (get) Token: 0x0600B446 RID: 46150 RVA: 0x002322A0 File Offset: 0x002304A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TS_LINE_CLASS_UIDColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_UID;
				}
			}

			// Token: 0x170036FF RID: 14079
			// (get) Token: 0x0600B447 RID: 46151 RVA: 0x002322A8 File Offset: 0x002304A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_NAMEColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_NAME;
				}
			}

			// Token: 0x17003700 RID: 14080
			// (get) Token: 0x0600B448 RID: 46152 RVA: 0x002322B0 File Offset: 0x002304B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_IS_EDITABLEColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_IS_EDITABLE;
				}
			}

			// Token: 0x17003701 RID: 14081
			// (get) Token: 0x0600B449 RID: 46153 RVA: 0x002322B8 File Offset: 0x002304B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_DESCColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_DESC;
				}
			}

			// Token: 0x17003702 RID: 14082
			// (get) Token: 0x0600B44A RID: 46154 RVA: 0x002322C0 File Offset: 0x002304C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TS_LINE_CLASS_IS_DISABLEDColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_IS_DISABLED;
				}
			}

			// Token: 0x17003703 RID: 14083
			// (get) Token: 0x0600B44B RID: 46155 RVA: 0x002322C8 File Offset: 0x002304C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_ALWAYS_DISPLAYColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_ALWAYS_DISPLAY;
				}
			}

			// Token: 0x17003704 RID: 14084
			// (get) Token: 0x0600B44C RID: 46156 RVA: 0x002322D0 File Offset: 0x002304D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_TYPEColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_TYPE;
				}
			}

			// Token: 0x17003705 RID: 14085
			// (get) Token: 0x0600B44D RID: 46157 RVA: 0x002322D8 File Offset: 0x002304D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TS_LINE_CLASS_NEED_APPROVALColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_NEED_APPROVAL;
				}
			}

			// Token: 0x17003706 RID: 14086
			// (get) Token: 0x0600B44E RID: 46158 RVA: 0x002322E0 File Offset: 0x002304E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_ORGANIZATIONColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_ORGANIZATION;
				}
			}

			// Token: 0x17003707 RID: 14087
			// (get) Token: 0x0600B44F RID: 46159 RVA: 0x002322E8 File Offset: 0x002304E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x17003708 RID: 14088
			// (get) Token: 0x0600B450 RID: 46160 RVA: 0x002322F0 File Offset: 0x002304F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TS_LINE_CLASS_MULTILINEColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_MULTILINE;
				}
			}

			// Token: 0x17003709 RID: 14089
			// (get) Token: 0x0600B451 RID: 46161 RVA: 0x002322F8 File Offset: 0x002304F8
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x1700370A RID: 14090
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.LineClassesRow this[int index]
			{
				get
				{
					return (TimesheetLineClassDataSet.LineClassesRow)base.Rows[index];
				}
			}

			// Token: 0x14000661 RID: 1633
			// (add) Token: 0x0600B453 RID: 46163 RVA: 0x00232318 File Offset: 0x00230518
			// (remove) Token: 0x0600B454 RID: 46164 RVA: 0x00232350 File Offset: 0x00230550
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.LineClassesRowChangeEventHandler LineClassesRowChanging;

			// Token: 0x14000662 RID: 1634
			// (add) Token: 0x0600B455 RID: 46165 RVA: 0x00232388 File Offset: 0x00230588
			// (remove) Token: 0x0600B456 RID: 46166 RVA: 0x002323C0 File Offset: 0x002305C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.LineClassesRowChangeEventHandler LineClassesRowChanged;

			// Token: 0x14000663 RID: 1635
			// (add) Token: 0x0600B457 RID: 46167 RVA: 0x002323F8 File Offset: 0x002305F8
			// (remove) Token: 0x0600B458 RID: 46168 RVA: 0x00232430 File Offset: 0x00230630
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.LineClassesRowChangeEventHandler LineClassesRowDeleting;

			// Token: 0x14000664 RID: 1636
			// (add) Token: 0x0600B459 RID: 46169 RVA: 0x00232468 File Offset: 0x00230668
			// (remove) Token: 0x0600B45A RID: 46170 RVA: 0x002324A0 File Offset: 0x002306A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.LineClassesRowChangeEventHandler LineClassesRowDeleted;

			// Token: 0x0600B45B RID: 46171 RVA: 0x002324D5 File Offset: 0x002306D5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddLineClassesRow(TimesheetLineClassDataSet.LineClassesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B45C RID: 46172 RVA: 0x002324E4 File Offset: 0x002306E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.LineClassesRow AddLineClassesRow(Guid TS_LINE_CLASS_UID, string TS_LINE_CLASS_NAME, bool TS_LINE_CLASS_IS_EDITABLE, string TS_LINE_CLASS_DESC, bool TS_LINE_CLASS_IS_DISABLED, bool TS_LINE_CLASS_ALWAYS_DISPLAY, byte TS_LINE_CLASS_TYPE, bool TS_LINE_CLASS_NEED_APPROVAL, string TS_LINE_CLASS_ORGANIZATION, DateTime MOD_DATE, bool TS_LINE_CLASS_MULTILINE)
			{
				TimesheetLineClassDataSet.LineClassesRow lineClassesRow = (TimesheetLineClassDataSet.LineClassesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					TS_LINE_CLASS_UID,
					TS_LINE_CLASS_NAME,
					TS_LINE_CLASS_IS_EDITABLE,
					TS_LINE_CLASS_DESC,
					TS_LINE_CLASS_IS_DISABLED,
					TS_LINE_CLASS_ALWAYS_DISPLAY,
					TS_LINE_CLASS_TYPE,
					TS_LINE_CLASS_NEED_APPROVAL,
					TS_LINE_CLASS_ORGANIZATION,
					MOD_DATE,
					TS_LINE_CLASS_MULTILINE
				};
				lineClassesRow.ItemArray = itemArray;
				base.Rows.Add(lineClassesRow);
				return lineClassesRow;
			}

			// Token: 0x0600B45D RID: 46173 RVA: 0x0023257C File Offset: 0x0023077C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.LineClassesRow FindByTS_LINE_CLASS_UID(Guid TS_LINE_CLASS_UID)
			{
				return (TimesheetLineClassDataSet.LineClassesRow)base.Rows.Find(new object[]
				{
					TS_LINE_CLASS_UID
				});
			}

			// Token: 0x0600B45E RID: 46174 RVA: 0x002325AA File Offset: 0x002307AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B45F RID: 46175 RVA: 0x002325B8 File Offset: 0x002307B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				TimesheetLineClassDataSet.LineClassesDataTable lineClassesDataTable = (TimesheetLineClassDataSet.LineClassesDataTable)base.Clone();
				lineClassesDataTable.InitVars();
				return lineClassesDataTable;
			}

			// Token: 0x0600B460 RID: 46176 RVA: 0x002325D8 File Offset: 0x002307D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new TimesheetLineClassDataSet.LineClassesDataTable();
			}

			// Token: 0x0600B461 RID: 46177 RVA: 0x002325E0 File Offset: 0x002307E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnTS_LINE_CLASS_UID = base.Columns["TS_LINE_CLASS_UID"];
				this.columnTS_LINE_CLASS_NAME = base.Columns["TS_LINE_CLASS_NAME"];
				this.columnTS_LINE_CLASS_IS_EDITABLE = base.Columns["TS_LINE_CLASS_IS_EDITABLE"];
				this.columnTS_LINE_CLASS_DESC = base.Columns["TS_LINE_CLASS_DESC"];
				this.columnTS_LINE_CLASS_IS_DISABLED = base.Columns["TS_LINE_CLASS_IS_DISABLED"];
				this.columnTS_LINE_CLASS_ALWAYS_DISPLAY = base.Columns["TS_LINE_CLASS_ALWAYS_DISPLAY"];
				this.columnTS_LINE_CLASS_TYPE = base.Columns["TS_LINE_CLASS_TYPE"];
				this.columnTS_LINE_CLASS_NEED_APPROVAL = base.Columns["TS_LINE_CLASS_NEED_APPROVAL"];
				this.columnTS_LINE_CLASS_ORGANIZATION = base.Columns["TS_LINE_CLASS_ORGANIZATION"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnTS_LINE_CLASS_MULTILINE = base.Columns["TS_LINE_CLASS_MULTILINE"];
			}

			// Token: 0x0600B462 RID: 46178 RVA: 0x002326E0 File Offset: 0x002308E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnTS_LINE_CLASS_UID = new DataColumn("TS_LINE_CLASS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_UID);
				this.columnTS_LINE_CLASS_NAME = new DataColumn("TS_LINE_CLASS_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_NAME);
				this.columnTS_LINE_CLASS_IS_EDITABLE = new DataColumn("TS_LINE_CLASS_IS_EDITABLE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_IS_EDITABLE);
				this.columnTS_LINE_CLASS_DESC = new DataColumn("TS_LINE_CLASS_DESC", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_DESC);
				this.columnTS_LINE_CLASS_IS_DISABLED = new DataColumn("TS_LINE_CLASS_IS_DISABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_IS_DISABLED);
				this.columnTS_LINE_CLASS_ALWAYS_DISPLAY = new DataColumn("TS_LINE_CLASS_ALWAYS_DISPLAY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_ALWAYS_DISPLAY);
				this.columnTS_LINE_CLASS_TYPE = new DataColumn("TS_LINE_CLASS_TYPE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_TYPE);
				this.columnTS_LINE_CLASS_NEED_APPROVAL = new DataColumn("TS_LINE_CLASS_NEED_APPROVAL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_NEED_APPROVAL);
				this.columnTS_LINE_CLASS_ORGANIZATION = new DataColumn("TS_LINE_CLASS_ORGANIZATION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_ORGANIZATION);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnTS_LINE_CLASS_MULTILINE = new DataColumn("TS_LINE_CLASS_MULTILINE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_MULTILINE);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnTS_LINE_CLASS_UID
				}, true));
				this.columnTS_LINE_CLASS_UID.AllowDBNull = false;
				this.columnTS_LINE_CLASS_UID.Unique = true;
				this.columnTS_LINE_CLASS_ORGANIZATION.AllowDBNull = false;
			}

			// Token: 0x0600B463 RID: 46179 RVA: 0x00232927 File Offset: 0x00230B27
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.LineClassesRow NewLineClassesRow()
			{
				return (TimesheetLineClassDataSet.LineClassesRow)base.NewRow();
			}

			// Token: 0x0600B464 RID: 46180 RVA: 0x00232934 File Offset: 0x00230B34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new TimesheetLineClassDataSet.LineClassesRow(builder);
			}

			// Token: 0x0600B465 RID: 46181 RVA: 0x0023293C File Offset: 0x00230B3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(TimesheetLineClassDataSet.LineClassesRow);
			}

			// Token: 0x0600B466 RID: 46182 RVA: 0x00232948 File Offset: 0x00230B48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.LineClassesRowChanged != null)
				{
					this.LineClassesRowChanged(this, new TimesheetLineClassDataSet.LineClassesRowChangeEvent((TimesheetLineClassDataSet.LineClassesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B467 RID: 46183 RVA: 0x0023297B File Offset: 0x00230B7B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.LineClassesRowChanging != null)
				{
					this.LineClassesRowChanging(this, new TimesheetLineClassDataSet.LineClassesRowChangeEvent((TimesheetLineClassDataSet.LineClassesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B468 RID: 46184 RVA: 0x002329AE File Offset: 0x00230BAE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.LineClassesRowDeleted != null)
				{
					this.LineClassesRowDeleted(this, new TimesheetLineClassDataSet.LineClassesRowChangeEvent((TimesheetLineClassDataSet.LineClassesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B469 RID: 46185 RVA: 0x002329E1 File Offset: 0x00230BE1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.LineClassesRowDeleting != null)
				{
					this.LineClassesRowDeleting(this, new TimesheetLineClassDataSet.LineClassesRowChangeEvent((TimesheetLineClassDataSet.LineClassesRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B46A RID: 46186 RVA: 0x00232A14 File Offset: 0x00230C14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveLineClassesRow(TimesheetLineClassDataSet.LineClassesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B46B RID: 46187 RVA: 0x00232A24 File Offset: 0x00230C24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				TimesheetLineClassDataSet timesheetLineClassDataSet = new TimesheetLineClassDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = timesheetLineClassDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "LineClassesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = timesheetLineClassDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002455 RID: 9301
			private DataColumn columnTS_LINE_CLASS_UID;

			// Token: 0x04002456 RID: 9302
			private DataColumn columnTS_LINE_CLASS_NAME;

			// Token: 0x04002457 RID: 9303
			private DataColumn columnTS_LINE_CLASS_IS_EDITABLE;

			// Token: 0x04002458 RID: 9304
			private DataColumn columnTS_LINE_CLASS_DESC;

			// Token: 0x04002459 RID: 9305
			private DataColumn columnTS_LINE_CLASS_IS_DISABLED;

			// Token: 0x0400245A RID: 9306
			private DataColumn columnTS_LINE_CLASS_ALWAYS_DISPLAY;

			// Token: 0x0400245B RID: 9307
			private DataColumn columnTS_LINE_CLASS_TYPE;

			// Token: 0x0400245C RID: 9308
			private DataColumn columnTS_LINE_CLASS_NEED_APPROVAL;

			// Token: 0x0400245D RID: 9309
			private DataColumn columnTS_LINE_CLASS_ORGANIZATION;

			// Token: 0x0400245E RID: 9310
			private DataColumn columnMOD_DATE;

			// Token: 0x0400245F RID: 9311
			private DataColumn columnTS_LINE_CLASS_MULTILINE;
		}

		// Token: 0x0200074C RID: 1868
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DepartmentsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B46C RID: 46188 RVA: 0x00232C1C File Offset: 0x00230E1C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DepartmentsDataTable()
			{
				base.TableName = "Departments";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B46D RID: 46189 RVA: 0x00232C44 File Offset: 0x00230E44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DepartmentsDataTable(DataTable table)
			{
				base.TableName = table.TableName;
				if (table.CaseSensitive != table.DataSet.CaseSensitive)
				{
					base.CaseSensitive = table.CaseSensitive;
				}
				if (table.Locale.ToString() != table.DataSet.Locale.ToString())
				{
					base.Locale = table.Locale;
				}
				if (table.Namespace != table.DataSet.Namespace)
				{
					base.Namespace = table.Namespace;
				}
				base.Prefix = table.Prefix;
				base.MinimumCapacity = table.MinimumCapacity;
			}

			// Token: 0x0600B46E RID: 46190 RVA: 0x00232CEC File Offset: 0x00230EEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected DepartmentsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700370B RID: 14091
			// (get) Token: 0x0600B46F RID: 46191 RVA: 0x00232CFC File Offset: 0x00230EFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TS_LINE_CLASS_UIDColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_UID;
				}
			}

			// Token: 0x1700370C RID: 14092
			// (get) Token: 0x0600B470 RID: 46192 RVA: 0x00232D04 File Offset: 0x00230F04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn TS_LINE_CLASS_DEPARTMENT_UIDColumn
			{
				get
				{
					return this.columnTS_LINE_CLASS_DEPARTMENT_UID;
				}
			}

			// Token: 0x1700370D RID: 14093
			// (get) Token: 0x0600B471 RID: 46193 RVA: 0x00232D0C File Offset: 0x00230F0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x1700370E RID: 14094
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.DepartmentsRow this[int index]
			{
				get
				{
					return (TimesheetLineClassDataSet.DepartmentsRow)base.Rows[index];
				}
			}

			// Token: 0x14000665 RID: 1637
			// (add) Token: 0x0600B473 RID: 46195 RVA: 0x00232D2C File Offset: 0x00230F2C
			// (remove) Token: 0x0600B474 RID: 46196 RVA: 0x00232D64 File Offset: 0x00230F64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.DepartmentsRowChangeEventHandler DepartmentsRowChanging;

			// Token: 0x14000666 RID: 1638
			// (add) Token: 0x0600B475 RID: 46197 RVA: 0x00232D9C File Offset: 0x00230F9C
			// (remove) Token: 0x0600B476 RID: 46198 RVA: 0x00232DD4 File Offset: 0x00230FD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.DepartmentsRowChangeEventHandler DepartmentsRowChanged;

			// Token: 0x14000667 RID: 1639
			// (add) Token: 0x0600B477 RID: 46199 RVA: 0x00232E0C File Offset: 0x0023100C
			// (remove) Token: 0x0600B478 RID: 46200 RVA: 0x00232E44 File Offset: 0x00231044
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.DepartmentsRowChangeEventHandler DepartmentsRowDeleting;

			// Token: 0x14000668 RID: 1640
			// (add) Token: 0x0600B479 RID: 46201 RVA: 0x00232E7C File Offset: 0x0023107C
			// (remove) Token: 0x0600B47A RID: 46202 RVA: 0x00232EB4 File Offset: 0x002310B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimesheetLineClassDataSet.DepartmentsRowChangeEventHandler DepartmentsRowDeleted;

			// Token: 0x0600B47B RID: 46203 RVA: 0x00232EE9 File Offset: 0x002310E9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddDepartmentsRow(TimesheetLineClassDataSet.DepartmentsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B47C RID: 46204 RVA: 0x00232EF8 File Offset: 0x002310F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimesheetLineClassDataSet.DepartmentsRow AddDepartmentsRow(Guid TS_LINE_CLASS_UID, Guid TS_LINE_CLASS_DEPARTMENT_UID)
			{
				TimesheetLineClassDataSet.DepartmentsRow departmentsRow = (TimesheetLineClassDataSet.DepartmentsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					TS_LINE_CLASS_UID,
					TS_LINE_CLASS_DEPARTMENT_UID
				};
				departmentsRow.ItemArray = itemArray;
				base.Rows.Add(departmentsRow);
				return departmentsRow;
			}

			// Token: 0x0600B47D RID: 46205 RVA: 0x00232F40 File Offset: 0x00231140
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B47E RID: 46206 RVA: 0x00232F50 File Offset: 0x00231150
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				TimesheetLineClassDataSet.DepartmentsDataTable departmentsDataTable = (TimesheetLineClassDataSet.DepartmentsDataTable)base.Clone();
				departmentsDataTable.InitVars();
				return departmentsDataTable;
			}

			// Token: 0x0600B47F RID: 46207 RVA: 0x00232F70 File Offset: 0x00231170
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new TimesheetLineClassDataSet.DepartmentsDataTable();
			}

			// Token: 0x0600B480 RID: 46208 RVA: 0x00232F77 File Offset: 0x00231177
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnTS_LINE_CLASS_UID = base.Columns["TS_LINE_CLASS_UID"];
				this.columnTS_LINE_CLASS_DEPARTMENT_UID = base.Columns["TS_LINE_CLASS_DEPARTMENT_UID"];
			}

			// Token: 0x0600B481 RID: 46209 RVA: 0x00232FA8 File Offset: 0x002311A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnTS_LINE_CLASS_UID = new DataColumn("TS_LINE_CLASS_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_UID);
				this.columnTS_LINE_CLASS_DEPARTMENT_UID = new DataColumn("TS_LINE_CLASS_DEPARTMENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnTS_LINE_CLASS_DEPARTMENT_UID);
				this.columnTS_LINE_CLASS_UID.AllowDBNull = false;
				this.columnTS_LINE_CLASS_DEPARTMENT_UID.AllowDBNull = false;
			}

			// Token: 0x0600B482 RID: 46210 RVA: 0x00233027 File Offset: 0x00231227
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimesheetLineClassDataSet.DepartmentsRow NewDepartmentsRow()
			{
				return (TimesheetLineClassDataSet.DepartmentsRow)base.NewRow();
			}

			// Token: 0x0600B483 RID: 46211 RVA: 0x00233034 File Offset: 0x00231234
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new TimesheetLineClassDataSet.DepartmentsRow(builder);
			}

			// Token: 0x0600B484 RID: 46212 RVA: 0x0023303C File Offset: 0x0023123C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(TimesheetLineClassDataSet.DepartmentsRow);
			}

			// Token: 0x0600B485 RID: 46213 RVA: 0x00233048 File Offset: 0x00231248
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DepartmentsRowChanged != null)
				{
					this.DepartmentsRowChanged(this, new TimesheetLineClassDataSet.DepartmentsRowChangeEvent((TimesheetLineClassDataSet.DepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B486 RID: 46214 RVA: 0x0023307B File Offset: 0x0023127B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DepartmentsRowChanging != null)
				{
					this.DepartmentsRowChanging(this, new TimesheetLineClassDataSet.DepartmentsRowChangeEvent((TimesheetLineClassDataSet.DepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B487 RID: 46215 RVA: 0x002330AE File Offset: 0x002312AE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DepartmentsRowDeleted != null)
				{
					this.DepartmentsRowDeleted(this, new TimesheetLineClassDataSet.DepartmentsRowChangeEvent((TimesheetLineClassDataSet.DepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B488 RID: 46216 RVA: 0x002330E1 File Offset: 0x002312E1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DepartmentsRowDeleting != null)
				{
					this.DepartmentsRowDeleting(this, new TimesheetLineClassDataSet.DepartmentsRowChangeEvent((TimesheetLineClassDataSet.DepartmentsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B489 RID: 46217 RVA: 0x00233114 File Offset: 0x00231314
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveDepartmentsRow(TimesheetLineClassDataSet.DepartmentsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B48A RID: 46218 RVA: 0x00233124 File Offset: 0x00231324
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				TimesheetLineClassDataSet timesheetLineClassDataSet = new TimesheetLineClassDataSet();
				XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
				xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
				xmlSchemaAny.MinOccurs = 0m;
				xmlSchemaAny.MaxOccurs = decimal.MaxValue;
				xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny);
				XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
				xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
				xmlSchemaAny2.MinOccurs = 1m;
				xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
				xmlSchemaSequence.Items.Add(xmlSchemaAny2);
				XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
				xmlSchemaAttribute.Name = "namespace";
				xmlSchemaAttribute.FixedValue = timesheetLineClassDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DepartmentsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = timesheetLineClassDataSet.GetSchemaSerializable();
				if (xs.Contains(schemaSerializable.TargetNamespace))
				{
					MemoryStream memoryStream = new MemoryStream();
					MemoryStream memoryStream2 = new MemoryStream();
					try
					{
						schemaSerializable.Write(memoryStream);
						foreach (object obj in xs.Schemas(schemaSerializable.TargetNamespace))
						{
							XmlSchema xmlSchema = (XmlSchema)obj;
							memoryStream2.SetLength(0L);
							xmlSchema.Write(memoryStream2);
							if (memoryStream.Length == memoryStream2.Length)
							{
								memoryStream.Position = 0L;
								memoryStream2.Position = 0L;
								while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
								{
								}
								if (memoryStream.Position == memoryStream.Length)
								{
									return xmlSchemaComplexType;
								}
							}
						}
					}
					finally
					{
						if (memoryStream != null)
						{
							memoryStream.Close();
						}
						if (memoryStream2 != null)
						{
							memoryStream2.Close();
						}
					}
				}
				xs.Add(schemaSerializable);
				return xmlSchemaComplexType;
			}

			// Token: 0x04002464 RID: 9316
			private DataColumn columnTS_LINE_CLASS_UID;

			// Token: 0x04002465 RID: 9317
			private DataColumn columnTS_LINE_CLASS_DEPARTMENT_UID;
		}

		// Token: 0x0200074D RID: 1869
		public class LineClassesRow : DataRow
		{
			// Token: 0x0600B48B RID: 46219 RVA: 0x0023331C File Offset: 0x0023151C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal LineClassesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableLineClasses = (TimesheetLineClassDataSet.LineClassesDataTable)base.Table;
			}

			// Token: 0x1700370F RID: 14095
			// (get) Token: 0x0600B48C RID: 46220 RVA: 0x00233336 File Offset: 0x00231536
			// (set) Token: 0x0600B48D RID: 46221 RVA: 0x0023334E File Offset: 0x0023154E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid TS_LINE_CLASS_UID
			{
				get
				{
					return (Guid)base[this.tableLineClasses.TS_LINE_CLASS_UIDColumn];
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_UIDColumn] = value;
				}
			}

			// Token: 0x17003710 RID: 14096
			// (get) Token: 0x0600B48E RID: 46222 RVA: 0x00233368 File Offset: 0x00231568
			// (set) Token: 0x0600B48F RID: 46223 RVA: 0x002333AC File Offset: 0x002315AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string TS_LINE_CLASS_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableLineClasses.TS_LINE_CLASS_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_NAME' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_NAMEColumn] = value;
				}
			}

			// Token: 0x17003711 RID: 14097
			// (get) Token: 0x0600B490 RID: 46224 RVA: 0x002333C0 File Offset: 0x002315C0
			// (set) Token: 0x0600B491 RID: 46225 RVA: 0x00233404 File Offset: 0x00231604
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool TS_LINE_CLASS_IS_EDITABLE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableLineClasses.TS_LINE_CLASS_IS_EDITABLEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_IS_EDITABLE' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_IS_EDITABLEColumn] = value;
				}
			}

			// Token: 0x17003712 RID: 14098
			// (get) Token: 0x0600B492 RID: 46226 RVA: 0x00233420 File Offset: 0x00231620
			// (set) Token: 0x0600B493 RID: 46227 RVA: 0x00233464 File Offset: 0x00231664
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string TS_LINE_CLASS_DESC
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableLineClasses.TS_LINE_CLASS_DESCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_DESC' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_DESCColumn] = value;
				}
			}

			// Token: 0x17003713 RID: 14099
			// (get) Token: 0x0600B494 RID: 46228 RVA: 0x00233478 File Offset: 0x00231678
			// (set) Token: 0x0600B495 RID: 46229 RVA: 0x002334BC File Offset: 0x002316BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool TS_LINE_CLASS_IS_DISABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableLineClasses.TS_LINE_CLASS_IS_DISABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_IS_DISABLED' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_IS_DISABLEDColumn] = value;
				}
			}

			// Token: 0x17003714 RID: 14100
			// (get) Token: 0x0600B496 RID: 46230 RVA: 0x002334D8 File Offset: 0x002316D8
			// (set) Token: 0x0600B497 RID: 46231 RVA: 0x0023351C File Offset: 0x0023171C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool TS_LINE_CLASS_ALWAYS_DISPLAY
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableLineClasses.TS_LINE_CLASS_ALWAYS_DISPLAYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_ALWAYS_DISPLAY' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_ALWAYS_DISPLAYColumn] = value;
				}
			}

			// Token: 0x17003715 RID: 14101
			// (get) Token: 0x0600B498 RID: 46232 RVA: 0x00233538 File Offset: 0x00231738
			// (set) Token: 0x0600B499 RID: 46233 RVA: 0x0023357C File Offset: 0x0023177C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte TS_LINE_CLASS_TYPE
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableLineClasses.TS_LINE_CLASS_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_TYPE' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_TYPEColumn] = value;
				}
			}

			// Token: 0x17003716 RID: 14102
			// (get) Token: 0x0600B49A RID: 46234 RVA: 0x00233598 File Offset: 0x00231798
			// (set) Token: 0x0600B49B RID: 46235 RVA: 0x002335DC File Offset: 0x002317DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool TS_LINE_CLASS_NEED_APPROVAL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableLineClasses.TS_LINE_CLASS_NEED_APPROVALColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_NEED_APPROVAL' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_NEED_APPROVALColumn] = value;
				}
			}

			// Token: 0x17003717 RID: 14103
			// (get) Token: 0x0600B49C RID: 46236 RVA: 0x002335F5 File Offset: 0x002317F5
			// (set) Token: 0x0600B49D RID: 46237 RVA: 0x0023360D File Offset: 0x0023180D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string TS_LINE_CLASS_ORGANIZATION
			{
				get
				{
					return (string)base[this.tableLineClasses.TS_LINE_CLASS_ORGANIZATIONColumn];
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_ORGANIZATIONColumn] = value;
				}
			}

			// Token: 0x17003718 RID: 14104
			// (get) Token: 0x0600B49E RID: 46238 RVA: 0x00233624 File Offset: 0x00231824
			// (set) Token: 0x0600B49F RID: 46239 RVA: 0x00233668 File Offset: 0x00231868
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableLineClasses.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17003719 RID: 14105
			// (get) Token: 0x0600B4A0 RID: 46240 RVA: 0x00233684 File Offset: 0x00231884
			// (set) Token: 0x0600B4A1 RID: 46241 RVA: 0x002336C8 File Offset: 0x002318C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool TS_LINE_CLASS_MULTILINE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableLineClasses.TS_LINE_CLASS_MULTILINEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TS_LINE_CLASS_MULTILINE' in table 'LineClasses' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLineClasses.TS_LINE_CLASS_MULTILINEColumn] = value;
				}
			}

			// Token: 0x0600B4A2 RID: 46242 RVA: 0x002336E1 File Offset: 0x002318E1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTS_LINE_CLASS_NAMENull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_NAMEColumn);
			}

			// Token: 0x0600B4A3 RID: 46243 RVA: 0x002336F4 File Offset: 0x002318F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTS_LINE_CLASS_NAMENull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4A4 RID: 46244 RVA: 0x0023370C File Offset: 0x0023190C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTS_LINE_CLASS_IS_EDITABLENull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_IS_EDITABLEColumn);
			}

			// Token: 0x0600B4A5 RID: 46245 RVA: 0x0023371F File Offset: 0x0023191F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTS_LINE_CLASS_IS_EDITABLENull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_IS_EDITABLEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4A6 RID: 46246 RVA: 0x00233737 File Offset: 0x00231937
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTS_LINE_CLASS_DESCNull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_DESCColumn);
			}

			// Token: 0x0600B4A7 RID: 46247 RVA: 0x0023374A File Offset: 0x0023194A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetTS_LINE_CLASS_DESCNull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_DESCColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4A8 RID: 46248 RVA: 0x00233762 File Offset: 0x00231962
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTS_LINE_CLASS_IS_DISABLEDNull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_IS_DISABLEDColumn);
			}

			// Token: 0x0600B4A9 RID: 46249 RVA: 0x00233775 File Offset: 0x00231975
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetTS_LINE_CLASS_IS_DISABLEDNull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_IS_DISABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4AA RID: 46250 RVA: 0x0023378D File Offset: 0x0023198D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTS_LINE_CLASS_ALWAYS_DISPLAYNull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_ALWAYS_DISPLAYColumn);
			}

			// Token: 0x0600B4AB RID: 46251 RVA: 0x002337A0 File Offset: 0x002319A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetTS_LINE_CLASS_ALWAYS_DISPLAYNull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_ALWAYS_DISPLAYColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4AC RID: 46252 RVA: 0x002337B8 File Offset: 0x002319B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTS_LINE_CLASS_TYPENull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_TYPEColumn);
			}

			// Token: 0x0600B4AD RID: 46253 RVA: 0x002337CB File Offset: 0x002319CB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetTS_LINE_CLASS_TYPENull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4AE RID: 46254 RVA: 0x002337E3 File Offset: 0x002319E3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTS_LINE_CLASS_NEED_APPROVALNull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_NEED_APPROVALColumn);
			}

			// Token: 0x0600B4AF RID: 46255 RVA: 0x002337F6 File Offset: 0x002319F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTS_LINE_CLASS_NEED_APPROVALNull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_NEED_APPROVALColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4B0 RID: 46256 RVA: 0x0023380E File Offset: 0x00231A0E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableLineClasses.MOD_DATEColumn);
			}

			// Token: 0x0600B4B1 RID: 46257 RVA: 0x00233821 File Offset: 0x00231A21
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableLineClasses.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B4B2 RID: 46258 RVA: 0x00233839 File Offset: 0x00231A39
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsTS_LINE_CLASS_MULTILINENull()
			{
				return base.IsNull(this.tableLineClasses.TS_LINE_CLASS_MULTILINEColumn);
			}

			// Token: 0x0600B4B3 RID: 46259 RVA: 0x0023384C File Offset: 0x00231A4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetTS_LINE_CLASS_MULTILINENull()
			{
				base[this.tableLineClasses.TS_LINE_CLASS_MULTILINEColumn] = Convert.DBNull;
			}

			// Token: 0x0400246A RID: 9322
			private TimesheetLineClassDataSet.LineClassesDataTable tableLineClasses;
		}

		// Token: 0x0200074E RID: 1870
		public class DepartmentsRow : DataRow
		{
			// Token: 0x0600B4B4 RID: 46260 RVA: 0x00233864 File Offset: 0x00231A64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DepartmentsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDepartments = (TimesheetLineClassDataSet.DepartmentsDataTable)base.Table;
			}

			// Token: 0x1700371A RID: 14106
			// (get) Token: 0x0600B4B5 RID: 46261 RVA: 0x0023387E File Offset: 0x00231A7E
			// (set) Token: 0x0600B4B6 RID: 46262 RVA: 0x00233896 File Offset: 0x00231A96
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid TS_LINE_CLASS_UID
			{
				get
				{
					return (Guid)base[this.tableDepartments.TS_LINE_CLASS_UIDColumn];
				}
				set
				{
					base[this.tableDepartments.TS_LINE_CLASS_UIDColumn] = value;
				}
			}

			// Token: 0x1700371B RID: 14107
			// (get) Token: 0x0600B4B7 RID: 46263 RVA: 0x002338AF File Offset: 0x00231AAF
			// (set) Token: 0x0600B4B8 RID: 46264 RVA: 0x002338C7 File Offset: 0x00231AC7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid TS_LINE_CLASS_DEPARTMENT_UID
			{
				get
				{
					return (Guid)base[this.tableDepartments.TS_LINE_CLASS_DEPARTMENT_UIDColumn];
				}
				set
				{
					base[this.tableDepartments.TS_LINE_CLASS_DEPARTMENT_UIDColumn] = value;
				}
			}

			// Token: 0x0400246B RID: 9323
			private TimesheetLineClassDataSet.DepartmentsDataTable tableDepartments;
		}

		// Token: 0x0200074F RID: 1871
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class LineClassesRowChangeEvent : EventArgs
		{
			// Token: 0x0600B4B9 RID: 46265 RVA: 0x002338E0 File Offset: 0x00231AE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public LineClassesRowChangeEvent(TimesheetLineClassDataSet.LineClassesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700371C RID: 14108
			// (get) Token: 0x0600B4BA RID: 46266 RVA: 0x002338F6 File Offset: 0x00231AF6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimesheetLineClassDataSet.LineClassesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700371D RID: 14109
			// (get) Token: 0x0600B4BB RID: 46267 RVA: 0x002338FE File Offset: 0x00231AFE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400246C RID: 9324
			private TimesheetLineClassDataSet.LineClassesRow eventRow;

			// Token: 0x0400246D RID: 9325
			private DataRowAction eventAction;
		}

		// Token: 0x02000750 RID: 1872
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DepartmentsRowChangeEvent : EventArgs
		{
			// Token: 0x0600B4BC RID: 46268 RVA: 0x00233906 File Offset: 0x00231B06
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DepartmentsRowChangeEvent(TimesheetLineClassDataSet.DepartmentsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700371E RID: 14110
			// (get) Token: 0x0600B4BD RID: 46269 RVA: 0x0023391C File Offset: 0x00231B1C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimesheetLineClassDataSet.DepartmentsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700371F RID: 14111
			// (get) Token: 0x0600B4BE RID: 46270 RVA: 0x00233924 File Offset: 0x00231B24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400246E RID: 9326
			private TimesheetLineClassDataSet.DepartmentsRow eventRow;

			// Token: 0x0400246F RID: 9327
			private DataRowAction eventAction;
		}
	}
}
