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
	// Token: 0x0200015F RID: 351
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("GanttSchemesDataSet")]
	[ToolboxItem(true)]
	[DesignerCategory("code")]
	[Serializable]
	public class GanttSchemesDataSet : DataSet
	{
		// Token: 0x060019AB RID: 6571 RVA: 0x00053148 File Offset: 0x00051348
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GanttSchemes, new string[]
			{
				"WGANTT_SCHEME_ORDER",
				"WGANTT_SCHEME_TYPE",
				"WGANTT_SCHEME_UID",
				"WGANTT_SCHEME_NAME"
			});
		}

		// Token: 0x060019AC RID: 6572 RVA: 0x00053190 File Offset: 0x00051390
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public GanttSchemesDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060019AD RID: 6573 RVA: 0x000531E4 File Offset: 0x000513E4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected GanttSchemesDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["GanttSchemes"] != null)
				{
					base.Tables.Add(new GanttSchemesDataSet.GanttSchemesDataTable(dataSet.Tables["GanttSchemes"]));
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

		// Token: 0x1700076D RID: 1901
		// (get) Token: 0x060019AE RID: 6574 RVA: 0x00053341 File Offset: 0x00051541
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GanttSchemesDataSet.GanttSchemesDataTable GanttSchemes
		{
			get
			{
				return this.tableGanttSchemes;
			}
		}

		// Token: 0x1700076E RID: 1902
		// (get) Token: 0x060019AF RID: 6575 RVA: 0x00053349 File Offset: 0x00051549
		// (set) Token: 0x060019B0 RID: 6576 RVA: 0x00053351 File Offset: 0x00051551
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

		// Token: 0x1700076F RID: 1903
		// (get) Token: 0x060019B1 RID: 6577 RVA: 0x0005335A File Offset: 0x0005155A
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17000770 RID: 1904
		// (get) Token: 0x060019B2 RID: 6578 RVA: 0x00053362 File Offset: 0x00051562
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

		// Token: 0x060019B3 RID: 6579 RVA: 0x0005336A File Offset: 0x0005156A
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x060019B4 RID: 6580 RVA: 0x00053380 File Offset: 0x00051580
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			GanttSchemesDataSet ganttSchemesDataSet = (GanttSchemesDataSet)base.Clone();
			ganttSchemesDataSet.InitVars();
			ganttSchemesDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return ganttSchemesDataSet;
		}

		// Token: 0x060019B5 RID: 6581 RVA: 0x000533AC File Offset: 0x000515AC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x060019B6 RID: 6582 RVA: 0x000533AF File Offset: 0x000515AF
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x000533B4 File Offset: 0x000515B4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["GanttSchemes"] != null)
				{
					base.Tables.Add(new GanttSchemesDataSet.GanttSchemesDataTable(dataSet.Tables["GanttSchemes"]));
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

		// Token: 0x060019B8 RID: 6584 RVA: 0x0005347C File Offset: 0x0005167C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x000534B0 File Offset: 0x000516B0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x060019BA RID: 6586 RVA: 0x000534B9 File Offset: 0x000516B9
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableGanttSchemes = (GanttSchemesDataSet.GanttSchemesDataTable)base.Tables["GanttSchemes"];
			if (initTable && this.tableGanttSchemes != null)
			{
				this.tableGanttSchemes.InitVars();
			}
		}

		// Token: 0x060019BB RID: 6587 RVA: 0x000534EC File Offset: 0x000516EC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "GanttSchemesDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/GanttSchemesDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableGanttSchemes = new GanttSchemesDataSet.GanttSchemesDataTable();
			base.Tables.Add(this.tableGanttSchemes);
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x00053544 File Offset: 0x00051744
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGanttSchemes()
		{
			return false;
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x00053547 File Offset: 0x00051747
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x00053558 File Offset: 0x00051758
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			GanttSchemesDataSet ganttSchemesDataSet = new GanttSchemesDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = ganttSchemesDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = ganttSchemesDataSet.GetSchemaSerializable();
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

		// Token: 0x0400059A RID: 1434
		private GanttSchemesDataSet.GanttSchemesDataTable tableGanttSchemes;

		// Token: 0x0400059B RID: 1435
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000160 RID: 352
		// (Invoke) Token: 0x060019C0 RID: 6592
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GanttSchemesRowChangeEventHandler(object sender, GanttSchemesDataSet.GanttSchemesRowChangeEvent e);

		// Token: 0x02000161 RID: 353
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GanttSchemesDataTable : DataTable, IEnumerable
		{
			// Token: 0x060019C3 RID: 6595 RVA: 0x000536A0 File Offset: 0x000518A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSchemesDataTable()
			{
				base.TableName = "GanttSchemes";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060019C4 RID: 6596 RVA: 0x000536C8 File Offset: 0x000518C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GanttSchemesDataTable(DataTable table)
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

			// Token: 0x060019C5 RID: 6597 RVA: 0x00053770 File Offset: 0x00051970
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GanttSchemesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000771 RID: 1905
			// (get) Token: 0x060019C6 RID: 6598 RVA: 0x00053780 File Offset: 0x00051980
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SCHEME_NAMEColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_NAME;
				}
			}

			// Token: 0x17000772 RID: 1906
			// (get) Token: 0x060019C7 RID: 6599 RVA: 0x00053788 File Offset: 0x00051988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_SCHEME_ORDERColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_ORDER;
				}
			}

			// Token: 0x17000773 RID: 1907
			// (get) Token: 0x060019C8 RID: 6600 RVA: 0x00053790 File Offset: 0x00051990
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_UID;
				}
			}

			// Token: 0x17000774 RID: 1908
			// (get) Token: 0x060019C9 RID: 6601 RVA: 0x00053798 File Offset: 0x00051998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SCHEME_TYPEColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_TYPE;
				}
			}

			// Token: 0x17000775 RID: 1909
			// (get) Token: 0x060019CA RID: 6602 RVA: 0x000537A0 File Offset: 0x000519A0
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

			// Token: 0x17000776 RID: 1910
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSchemesDataSet.GanttSchemesRow this[int index]
			{
				get
				{
					return (GanttSchemesDataSet.GanttSchemesRow)base.Rows[index];
				}
			}

			// Token: 0x14000125 RID: 293
			// (add) Token: 0x060019CC RID: 6604 RVA: 0x000537C0 File Offset: 0x000519C0
			// (remove) Token: 0x060019CD RID: 6605 RVA: 0x000537F8 File Offset: 0x000519F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSchemesDataSet.GanttSchemesRowChangeEventHandler GanttSchemesRowChanging;

			// Token: 0x14000126 RID: 294
			// (add) Token: 0x060019CE RID: 6606 RVA: 0x00053830 File Offset: 0x00051A30
			// (remove) Token: 0x060019CF RID: 6607 RVA: 0x00053868 File Offset: 0x00051A68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSchemesDataSet.GanttSchemesRowChangeEventHandler GanttSchemesRowChanged;

			// Token: 0x14000127 RID: 295
			// (add) Token: 0x060019D0 RID: 6608 RVA: 0x000538A0 File Offset: 0x00051AA0
			// (remove) Token: 0x060019D1 RID: 6609 RVA: 0x000538D8 File Offset: 0x00051AD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSchemesDataSet.GanttSchemesRowChangeEventHandler GanttSchemesRowDeleting;

			// Token: 0x14000128 RID: 296
			// (add) Token: 0x060019D2 RID: 6610 RVA: 0x00053910 File Offset: 0x00051B10
			// (remove) Token: 0x060019D3 RID: 6611 RVA: 0x00053948 File Offset: 0x00051B48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSchemesDataSet.GanttSchemesRowChangeEventHandler GanttSchemesRowDeleted;

			// Token: 0x060019D4 RID: 6612 RVA: 0x0005397D File Offset: 0x00051B7D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGanttSchemesRow(GanttSchemesDataSet.GanttSchemesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060019D5 RID: 6613 RVA: 0x0005398C File Offset: 0x00051B8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSchemesDataSet.GanttSchemesRow AddGanttSchemesRow(string WGANTT_SCHEME_NAME, int WGANTT_SCHEME_ORDER, Guid WGANTT_SCHEME_UID, int WGANTT_SCHEME_TYPE)
			{
				GanttSchemesDataSet.GanttSchemesRow ganttSchemesRow = (GanttSchemesDataSet.GanttSchemesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WGANTT_SCHEME_NAME,
					WGANTT_SCHEME_ORDER,
					WGANTT_SCHEME_UID,
					WGANTT_SCHEME_TYPE
				};
				ganttSchemesRow.ItemArray = itemArray;
				base.Rows.Add(ganttSchemesRow);
				return ganttSchemesRow;
			}

			// Token: 0x060019D6 RID: 6614 RVA: 0x000539E2 File Offset: 0x00051BE2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060019D7 RID: 6615 RVA: 0x000539F0 File Offset: 0x00051BF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				GanttSchemesDataSet.GanttSchemesDataTable ganttSchemesDataTable = (GanttSchemesDataSet.GanttSchemesDataTable)base.Clone();
				ganttSchemesDataTable.InitVars();
				return ganttSchemesDataTable;
			}

			// Token: 0x060019D8 RID: 6616 RVA: 0x00053A10 File Offset: 0x00051C10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new GanttSchemesDataSet.GanttSchemesDataTable();
			}

			// Token: 0x060019D9 RID: 6617 RVA: 0x00053A18 File Offset: 0x00051C18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWGANTT_SCHEME_NAME = base.Columns["WGANTT_SCHEME_NAME"];
				this.columnWGANTT_SCHEME_ORDER = base.Columns["WGANTT_SCHEME_ORDER"];
				this.columnWGANTT_SCHEME_UID = base.Columns["WGANTT_SCHEME_UID"];
				this.columnWGANTT_SCHEME_TYPE = base.Columns["WGANTT_SCHEME_TYPE"];
			}

			// Token: 0x060019DA RID: 6618 RVA: 0x00053A80 File Offset: 0x00051C80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWGANTT_SCHEME_NAME = new DataColumn("WGANTT_SCHEME_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_NAME);
				this.columnWGANTT_SCHEME_ORDER = new DataColumn("WGANTT_SCHEME_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_ORDER);
				this.columnWGANTT_SCHEME_UID = new DataColumn("WGANTT_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_UID);
				this.columnWGANTT_SCHEME_TYPE = new DataColumn("WGANTT_SCHEME_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_TYPE);
			}

			// Token: 0x060019DB RID: 6619 RVA: 0x00053B41 File Offset: 0x00051D41
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSchemesDataSet.GanttSchemesRow NewGanttSchemesRow()
			{
				return (GanttSchemesDataSet.GanttSchemesRow)base.NewRow();
			}

			// Token: 0x060019DC RID: 6620 RVA: 0x00053B4E File Offset: 0x00051D4E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new GanttSchemesDataSet.GanttSchemesRow(builder);
			}

			// Token: 0x060019DD RID: 6621 RVA: 0x00053B56 File Offset: 0x00051D56
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(GanttSchemesDataSet.GanttSchemesRow);
			}

			// Token: 0x060019DE RID: 6622 RVA: 0x00053B62 File Offset: 0x00051D62
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GanttSchemesRowChanged != null)
				{
					this.GanttSchemesRowChanged(this, new GanttSchemesDataSet.GanttSchemesRowChangeEvent((GanttSchemesDataSet.GanttSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060019DF RID: 6623 RVA: 0x00053B95 File Offset: 0x00051D95
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GanttSchemesRowChanging != null)
				{
					this.GanttSchemesRowChanging(this, new GanttSchemesDataSet.GanttSchemesRowChangeEvent((GanttSchemesDataSet.GanttSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060019E0 RID: 6624 RVA: 0x00053BC8 File Offset: 0x00051DC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GanttSchemesRowDeleted != null)
				{
					this.GanttSchemesRowDeleted(this, new GanttSchemesDataSet.GanttSchemesRowChangeEvent((GanttSchemesDataSet.GanttSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060019E1 RID: 6625 RVA: 0x00053BFB File Offset: 0x00051DFB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GanttSchemesRowDeleting != null)
				{
					this.GanttSchemesRowDeleting(this, new GanttSchemesDataSet.GanttSchemesRowChangeEvent((GanttSchemesDataSet.GanttSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060019E2 RID: 6626 RVA: 0x00053C2E File Offset: 0x00051E2E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGanttSchemesRow(GanttSchemesDataSet.GanttSchemesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060019E3 RID: 6627 RVA: 0x00053C3C File Offset: 0x00051E3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				GanttSchemesDataSet ganttSchemesDataSet = new GanttSchemesDataSet();
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
				xmlSchemaAttribute.FixedValue = ganttSchemesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GanttSchemesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = ganttSchemesDataSet.GetSchemaSerializable();
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

			// Token: 0x0400059C RID: 1436
			private DataColumn columnWGANTT_SCHEME_NAME;

			// Token: 0x0400059D RID: 1437
			private DataColumn columnWGANTT_SCHEME_ORDER;

			// Token: 0x0400059E RID: 1438
			private DataColumn columnWGANTT_SCHEME_UID;

			// Token: 0x0400059F RID: 1439
			private DataColumn columnWGANTT_SCHEME_TYPE;
		}

		// Token: 0x02000162 RID: 354
		public class GanttSchemesRow : DataRow
		{
			// Token: 0x060019E4 RID: 6628 RVA: 0x00053E34 File Offset: 0x00052034
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GanttSchemesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGanttSchemes = (GanttSchemesDataSet.GanttSchemesDataTable)base.Table;
			}

			// Token: 0x17000777 RID: 1911
			// (get) Token: 0x060019E5 RID: 6629 RVA: 0x00053E50 File Offset: 0x00052050
			// (set) Token: 0x060019E6 RID: 6630 RVA: 0x00053E94 File Offset: 0x00052094
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WGANTT_SCHEME_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableGanttSchemes.WGANTT_SCHEME_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_NAME' in table 'GanttSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSchemes.WGANTT_SCHEME_NAMEColumn] = value;
				}
			}

			// Token: 0x17000778 RID: 1912
			// (get) Token: 0x060019E7 RID: 6631 RVA: 0x00053EA8 File Offset: 0x000520A8
			// (set) Token: 0x060019E8 RID: 6632 RVA: 0x00053EEC File Offset: 0x000520EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_SCHEME_ORDER
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSchemes.WGANTT_SCHEME_ORDERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_ORDER' in table 'GanttSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSchemes.WGANTT_SCHEME_ORDERColumn] = value;
				}
			}

			// Token: 0x17000779 RID: 1913
			// (get) Token: 0x060019E9 RID: 6633 RVA: 0x00053F08 File Offset: 0x00052108
			// (set) Token: 0x060019EA RID: 6634 RVA: 0x00053F4C File Offset: 0x0005214C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WGANTT_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableGanttSchemes.WGANTT_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_UID' in table 'GanttSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSchemes.WGANTT_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x1700077A RID: 1914
			// (get) Token: 0x060019EB RID: 6635 RVA: 0x00053F68 File Offset: 0x00052168
			// (set) Token: 0x060019EC RID: 6636 RVA: 0x00053FAC File Offset: 0x000521AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGANTT_SCHEME_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSchemes.WGANTT_SCHEME_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_TYPE' in table 'GanttSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSchemes.WGANTT_SCHEME_TYPEColumn] = value;
				}
			}

			// Token: 0x060019ED RID: 6637 RVA: 0x00053FC5 File Offset: 0x000521C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_SCHEME_NAMENull()
			{
				return base.IsNull(this.tableGanttSchemes.WGANTT_SCHEME_NAMEColumn);
			}

			// Token: 0x060019EE RID: 6638 RVA: 0x00053FD8 File Offset: 0x000521D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_SCHEME_NAMENull()
			{
				base[this.tableGanttSchemes.WGANTT_SCHEME_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060019EF RID: 6639 RVA: 0x00053FF0 File Offset: 0x000521F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_SCHEME_ORDERNull()
			{
				return base.IsNull(this.tableGanttSchemes.WGANTT_SCHEME_ORDERColumn);
			}

			// Token: 0x060019F0 RID: 6640 RVA: 0x00054003 File Offset: 0x00052203
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_SCHEME_ORDERNull()
			{
				base[this.tableGanttSchemes.WGANTT_SCHEME_ORDERColumn] = Convert.DBNull;
			}

			// Token: 0x060019F1 RID: 6641 RVA: 0x0005401B File Offset: 0x0005221B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableGanttSchemes.WGANTT_SCHEME_UIDColumn);
			}

			// Token: 0x060019F2 RID: 6642 RVA: 0x0005402E File Offset: 0x0005222E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_SCHEME_UIDNull()
			{
				base[this.tableGanttSchemes.WGANTT_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060019F3 RID: 6643 RVA: 0x00054046 File Offset: 0x00052246
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGANTT_SCHEME_TYPENull()
			{
				return base.IsNull(this.tableGanttSchemes.WGANTT_SCHEME_TYPEColumn);
			}

			// Token: 0x060019F4 RID: 6644 RVA: 0x00054059 File Offset: 0x00052259
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_SCHEME_TYPENull()
			{
				base[this.tableGanttSchemes.WGANTT_SCHEME_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x040005A4 RID: 1444
			private GanttSchemesDataSet.GanttSchemesDataTable tableGanttSchemes;
		}

		// Token: 0x02000163 RID: 355
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GanttSchemesRowChangeEvent : EventArgs
		{
			// Token: 0x060019F5 RID: 6645 RVA: 0x00054071 File Offset: 0x00052271
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSchemesRowChangeEvent(GanttSchemesDataSet.GanttSchemesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700077B RID: 1915
			// (get) Token: 0x060019F6 RID: 6646 RVA: 0x00054087 File Offset: 0x00052287
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSchemesDataSet.GanttSchemesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700077C RID: 1916
			// (get) Token: 0x060019F7 RID: 6647 RVA: 0x0005408F File Offset: 0x0005228F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040005A5 RID: 1445
			private GanttSchemesDataSet.GanttSchemesRow eventRow;

			// Token: 0x040005A6 RID: 1446
			private DataRowAction eventAction;
		}
	}
}
