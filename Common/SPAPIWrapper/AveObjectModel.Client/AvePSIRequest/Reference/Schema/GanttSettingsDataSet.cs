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
	// Token: 0x02000164 RID: 356
	[XmlRoot("GanttSettingsDataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[Serializable]
	public class GanttSettingsDataSet : DataSet
	{
		// Token: 0x060019F8 RID: 6648 RVA: 0x00054098 File Offset: 0x00052298
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GanttSettings, new string[]
			{
				"WGANTT_BAR_TYPE",
				"WGANTT_END_COLOR",
				"WGANTT_BAR_COLOR",
				"WGANTT_END_SHAPE",
				"WGANTT_START_SHAPE",
				"WGANTT_BAR_PATTERN",
				"WGANTT_SCHEME_UID",
				"WGANTT_SHOW",
				"WGANTT_STYLE_ID",
				"WGANTT_START_COLOR"
			});
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x00054110 File Offset: 0x00052310
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public GanttSettingsDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060019FA RID: 6650 RVA: 0x00054164 File Offset: 0x00052364
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected GanttSettingsDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["GanttSettings"] != null)
				{
					base.Tables.Add(new GanttSettingsDataSet.GanttSettingsDataTable(dataSet.Tables["GanttSettings"]));
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

		// Token: 0x1700077D RID: 1917
		// (get) Token: 0x060019FB RID: 6651 RVA: 0x000542C1 File Offset: 0x000524C1
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GanttSettingsDataSet.GanttSettingsDataTable GanttSettings
		{
			get
			{
				return this.tableGanttSettings;
			}
		}

		// Token: 0x1700077E RID: 1918
		// (get) Token: 0x060019FC RID: 6652 RVA: 0x000542C9 File Offset: 0x000524C9
		// (set) Token: 0x060019FD RID: 6653 RVA: 0x000542D1 File Offset: 0x000524D1
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
		[Browsable(true)]
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

		// Token: 0x1700077F RID: 1919
		// (get) Token: 0x060019FE RID: 6654 RVA: 0x000542DA File Offset: 0x000524DA
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

		// Token: 0x17000780 RID: 1920
		// (get) Token: 0x060019FF RID: 6655 RVA: 0x000542E2 File Offset: 0x000524E2
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06001A00 RID: 6656 RVA: 0x000542EA File Offset: 0x000524EA
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06001A01 RID: 6657 RVA: 0x00054300 File Offset: 0x00052500
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			GanttSettingsDataSet ganttSettingsDataSet = (GanttSettingsDataSet)base.Clone();
			ganttSettingsDataSet.InitVars();
			ganttSettingsDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return ganttSettingsDataSet;
		}

		// Token: 0x06001A02 RID: 6658 RVA: 0x0005432C File Offset: 0x0005252C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06001A03 RID: 6659 RVA: 0x0005432F File Offset: 0x0005252F
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x00054334 File Offset: 0x00052534
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["GanttSettings"] != null)
				{
					base.Tables.Add(new GanttSettingsDataSet.GanttSettingsDataTable(dataSet.Tables["GanttSettings"]));
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

		// Token: 0x06001A05 RID: 6661 RVA: 0x000543FC File Offset: 0x000525FC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x00054430 File Offset: 0x00052630
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x00054439 File Offset: 0x00052639
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableGanttSettings = (GanttSettingsDataSet.GanttSettingsDataTable)base.Tables["GanttSettings"];
			if (initTable && this.tableGanttSettings != null)
			{
				this.tableGanttSettings.InitVars();
			}
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x0005446C File Offset: 0x0005266C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "GanttSettingsDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/GanttSettingsDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableGanttSettings = new GanttSettingsDataSet.GanttSettingsDataTable();
			base.Tables.Add(this.tableGanttSettings);
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x000544C4 File Offset: 0x000526C4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGanttSettings()
		{
			return false;
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x000544C7 File Offset: 0x000526C7
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x000544D8 File Offset: 0x000526D8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			GanttSettingsDataSet ganttSettingsDataSet = new GanttSettingsDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = ganttSettingsDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = ganttSettingsDataSet.GetSchemaSerializable();
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

		// Token: 0x040005A7 RID: 1447
		private GanttSettingsDataSet.GanttSettingsDataTable tableGanttSettings;

		// Token: 0x040005A8 RID: 1448
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000165 RID: 357
		// (Invoke) Token: 0x06001A0D RID: 6669
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GanttSettingsRowChangeEventHandler(object sender, GanttSettingsDataSet.GanttSettingsRowChangeEvent e);

		// Token: 0x02000166 RID: 358
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GanttSettingsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001A10 RID: 6672 RVA: 0x00054620 File Offset: 0x00052820
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSettingsDataTable()
			{
				base.TableName = "GanttSettings";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001A11 RID: 6673 RVA: 0x00054648 File Offset: 0x00052848
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GanttSettingsDataTable(DataTable table)
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

			// Token: 0x06001A12 RID: 6674 RVA: 0x000546F0 File Offset: 0x000528F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GanttSettingsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000781 RID: 1921
			// (get) Token: 0x06001A13 RID: 6675 RVA: 0x00054700 File Offset: 0x00052900
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_UID;
				}
			}

			// Token: 0x17000782 RID: 1922
			// (get) Token: 0x06001A14 RID: 6676 RVA: 0x00054708 File Offset: 0x00052908
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_STYLE_IDColumn
			{
				get
				{
					return this.columnWGANTT_STYLE_ID;
				}
			}

			// Token: 0x17000783 RID: 1923
			// (get) Token: 0x06001A15 RID: 6677 RVA: 0x00054710 File Offset: 0x00052910
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SHOWColumn
			{
				get
				{
					return this.columnWGANTT_SHOW;
				}
			}

			// Token: 0x17000784 RID: 1924
			// (get) Token: 0x06001A16 RID: 6678 RVA: 0x00054718 File Offset: 0x00052918
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_BAR_TYPEColumn
			{
				get
				{
					return this.columnWGANTT_BAR_TYPE;
				}
			}

			// Token: 0x17000785 RID: 1925
			// (get) Token: 0x06001A17 RID: 6679 RVA: 0x00054720 File Offset: 0x00052920
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_BAR_PATTERNColumn
			{
				get
				{
					return this.columnWGANTT_BAR_PATTERN;
				}
			}

			// Token: 0x17000786 RID: 1926
			// (get) Token: 0x06001A18 RID: 6680 RVA: 0x00054728 File Offset: 0x00052928
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_BAR_COLORColumn
			{
				get
				{
					return this.columnWGANTT_BAR_COLOR;
				}
			}

			// Token: 0x17000787 RID: 1927
			// (get) Token: 0x06001A19 RID: 6681 RVA: 0x00054730 File Offset: 0x00052930
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_START_SHAPEColumn
			{
				get
				{
					return this.columnWGANTT_START_SHAPE;
				}
			}

			// Token: 0x17000788 RID: 1928
			// (get) Token: 0x06001A1A RID: 6682 RVA: 0x00054738 File Offset: 0x00052938
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_START_COLORColumn
			{
				get
				{
					return this.columnWGANTT_START_COLOR;
				}
			}

			// Token: 0x17000789 RID: 1929
			// (get) Token: 0x06001A1B RID: 6683 RVA: 0x00054740 File Offset: 0x00052940
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGANTT_END_SHAPEColumn
			{
				get
				{
					return this.columnWGANTT_END_SHAPE;
				}
			}

			// Token: 0x1700078A RID: 1930
			// (get) Token: 0x06001A1C RID: 6684 RVA: 0x00054748 File Offset: 0x00052948
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_END_COLORColumn
			{
				get
				{
					return this.columnWGANTT_END_COLOR;
				}
			}

			// Token: 0x1700078B RID: 1931
			// (get) Token: 0x06001A1D RID: 6685 RVA: 0x00054750 File Offset: 0x00052950
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x1700078C RID: 1932
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSettingsDataSet.GanttSettingsRow this[int index]
			{
				get
				{
					return (GanttSettingsDataSet.GanttSettingsRow)base.Rows[index];
				}
			}

			// Token: 0x14000129 RID: 297
			// (add) Token: 0x06001A1F RID: 6687 RVA: 0x00054770 File Offset: 0x00052970
			// (remove) Token: 0x06001A20 RID: 6688 RVA: 0x000547A8 File Offset: 0x000529A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSettingsDataSet.GanttSettingsRowChangeEventHandler GanttSettingsRowChanging;

			// Token: 0x1400012A RID: 298
			// (add) Token: 0x06001A21 RID: 6689 RVA: 0x000547E0 File Offset: 0x000529E0
			// (remove) Token: 0x06001A22 RID: 6690 RVA: 0x00054818 File Offset: 0x00052A18
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSettingsDataSet.GanttSettingsRowChangeEventHandler GanttSettingsRowChanged;

			// Token: 0x1400012B RID: 299
			// (add) Token: 0x06001A23 RID: 6691 RVA: 0x00054850 File Offset: 0x00052A50
			// (remove) Token: 0x06001A24 RID: 6692 RVA: 0x00054888 File Offset: 0x00052A88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSettingsDataSet.GanttSettingsRowChangeEventHandler GanttSettingsRowDeleting;

			// Token: 0x1400012C RID: 300
			// (add) Token: 0x06001A25 RID: 6693 RVA: 0x000548C0 File Offset: 0x00052AC0
			// (remove) Token: 0x06001A26 RID: 6694 RVA: 0x000548F8 File Offset: 0x00052AF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GanttSettingsDataSet.GanttSettingsRowChangeEventHandler GanttSettingsRowDeleted;

			// Token: 0x06001A27 RID: 6695 RVA: 0x0005492D File Offset: 0x00052B2D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGanttSettingsRow(GanttSettingsDataSet.GanttSettingsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001A28 RID: 6696 RVA: 0x0005493C File Offset: 0x00052B3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSettingsDataSet.GanttSettingsRow AddGanttSettingsRow(Guid WGANTT_SCHEME_UID, int WGANTT_STYLE_ID, int WGANTT_SHOW, int WGANTT_BAR_TYPE, int WGANTT_BAR_PATTERN, int WGANTT_BAR_COLOR, int WGANTT_START_SHAPE, int WGANTT_START_COLOR, int WGANTT_END_SHAPE, int WGANTT_END_COLOR)
			{
				GanttSettingsDataSet.GanttSettingsRow ganttSettingsRow = (GanttSettingsDataSet.GanttSettingsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WGANTT_SCHEME_UID,
					WGANTT_STYLE_ID,
					WGANTT_SHOW,
					WGANTT_BAR_TYPE,
					WGANTT_BAR_PATTERN,
					WGANTT_BAR_COLOR,
					WGANTT_START_SHAPE,
					WGANTT_START_COLOR,
					WGANTT_END_SHAPE,
					WGANTT_END_COLOR
				};
				ganttSettingsRow.ItemArray = itemArray;
				base.Rows.Add(ganttSettingsRow);
				return ganttSettingsRow;
			}

			// Token: 0x06001A29 RID: 6697 RVA: 0x000549D5 File Offset: 0x00052BD5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001A2A RID: 6698 RVA: 0x000549E4 File Offset: 0x00052BE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				GanttSettingsDataSet.GanttSettingsDataTable ganttSettingsDataTable = (GanttSettingsDataSet.GanttSettingsDataTable)base.Clone();
				ganttSettingsDataTable.InitVars();
				return ganttSettingsDataTable;
			}

			// Token: 0x06001A2B RID: 6699 RVA: 0x00054A04 File Offset: 0x00052C04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new GanttSettingsDataSet.GanttSettingsDataTable();
			}

			// Token: 0x06001A2C RID: 6700 RVA: 0x00054A0C File Offset: 0x00052C0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWGANTT_SCHEME_UID = base.Columns["WGANTT_SCHEME_UID"];
				this.columnWGANTT_STYLE_ID = base.Columns["WGANTT_STYLE_ID"];
				this.columnWGANTT_SHOW = base.Columns["WGANTT_SHOW"];
				this.columnWGANTT_BAR_TYPE = base.Columns["WGANTT_BAR_TYPE"];
				this.columnWGANTT_BAR_PATTERN = base.Columns["WGANTT_BAR_PATTERN"];
				this.columnWGANTT_BAR_COLOR = base.Columns["WGANTT_BAR_COLOR"];
				this.columnWGANTT_START_SHAPE = base.Columns["WGANTT_START_SHAPE"];
				this.columnWGANTT_START_COLOR = base.Columns["WGANTT_START_COLOR"];
				this.columnWGANTT_END_SHAPE = base.Columns["WGANTT_END_SHAPE"];
				this.columnWGANTT_END_COLOR = base.Columns["WGANTT_END_COLOR"];
			}

			// Token: 0x06001A2D RID: 6701 RVA: 0x00054AF8 File Offset: 0x00052CF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWGANTT_SCHEME_UID = new DataColumn("WGANTT_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_UID);
				this.columnWGANTT_STYLE_ID = new DataColumn("WGANTT_STYLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_STYLE_ID);
				this.columnWGANTT_SHOW = new DataColumn("WGANTT_SHOW", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SHOW);
				this.columnWGANTT_BAR_TYPE = new DataColumn("WGANTT_BAR_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_BAR_TYPE);
				this.columnWGANTT_BAR_PATTERN = new DataColumn("WGANTT_BAR_PATTERN", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_BAR_PATTERN);
				this.columnWGANTT_BAR_COLOR = new DataColumn("WGANTT_BAR_COLOR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_BAR_COLOR);
				this.columnWGANTT_START_SHAPE = new DataColumn("WGANTT_START_SHAPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_START_SHAPE);
				this.columnWGANTT_START_COLOR = new DataColumn("WGANTT_START_COLOR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_START_COLOR);
				this.columnWGANTT_END_SHAPE = new DataColumn("WGANTT_END_SHAPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_END_SHAPE);
				this.columnWGANTT_END_COLOR = new DataColumn("WGANTT_END_COLOR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_END_COLOR);
			}

			// Token: 0x06001A2E RID: 6702 RVA: 0x00054CC7 File Offset: 0x00052EC7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GanttSettingsDataSet.GanttSettingsRow NewGanttSettingsRow()
			{
				return (GanttSettingsDataSet.GanttSettingsRow)base.NewRow();
			}

			// Token: 0x06001A2F RID: 6703 RVA: 0x00054CD4 File Offset: 0x00052ED4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new GanttSettingsDataSet.GanttSettingsRow(builder);
			}

			// Token: 0x06001A30 RID: 6704 RVA: 0x00054CDC File Offset: 0x00052EDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(GanttSettingsDataSet.GanttSettingsRow);
			}

			// Token: 0x06001A31 RID: 6705 RVA: 0x00054CE8 File Offset: 0x00052EE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GanttSettingsRowChanged != null)
				{
					this.GanttSettingsRowChanged(this, new GanttSettingsDataSet.GanttSettingsRowChangeEvent((GanttSettingsDataSet.GanttSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A32 RID: 6706 RVA: 0x00054D1B File Offset: 0x00052F1B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GanttSettingsRowChanging != null)
				{
					this.GanttSettingsRowChanging(this, new GanttSettingsDataSet.GanttSettingsRowChangeEvent((GanttSettingsDataSet.GanttSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A33 RID: 6707 RVA: 0x00054D4E File Offset: 0x00052F4E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GanttSettingsRowDeleted != null)
				{
					this.GanttSettingsRowDeleted(this, new GanttSettingsDataSet.GanttSettingsRowChangeEvent((GanttSettingsDataSet.GanttSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A34 RID: 6708 RVA: 0x00054D81 File Offset: 0x00052F81
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GanttSettingsRowDeleting != null)
				{
					this.GanttSettingsRowDeleting(this, new GanttSettingsDataSet.GanttSettingsRowChangeEvent((GanttSettingsDataSet.GanttSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A35 RID: 6709 RVA: 0x00054DB4 File Offset: 0x00052FB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGanttSettingsRow(GanttSettingsDataSet.GanttSettingsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001A36 RID: 6710 RVA: 0x00054DC4 File Offset: 0x00052FC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				GanttSettingsDataSet ganttSettingsDataSet = new GanttSettingsDataSet();
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
				xmlSchemaAttribute.FixedValue = ganttSettingsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GanttSettingsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = ganttSettingsDataSet.GetSchemaSerializable();
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

			// Token: 0x040005A9 RID: 1449
			private DataColumn columnWGANTT_SCHEME_UID;

			// Token: 0x040005AA RID: 1450
			private DataColumn columnWGANTT_STYLE_ID;

			// Token: 0x040005AB RID: 1451
			private DataColumn columnWGANTT_SHOW;

			// Token: 0x040005AC RID: 1452
			private DataColumn columnWGANTT_BAR_TYPE;

			// Token: 0x040005AD RID: 1453
			private DataColumn columnWGANTT_BAR_PATTERN;

			// Token: 0x040005AE RID: 1454
			private DataColumn columnWGANTT_BAR_COLOR;

			// Token: 0x040005AF RID: 1455
			private DataColumn columnWGANTT_START_SHAPE;

			// Token: 0x040005B0 RID: 1456
			private DataColumn columnWGANTT_START_COLOR;

			// Token: 0x040005B1 RID: 1457
			private DataColumn columnWGANTT_END_SHAPE;

			// Token: 0x040005B2 RID: 1458
			private DataColumn columnWGANTT_END_COLOR;
		}

		// Token: 0x02000167 RID: 359
		public class GanttSettingsRow : DataRow
		{
			// Token: 0x06001A37 RID: 6711 RVA: 0x00054FBC File Offset: 0x000531BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GanttSettingsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGanttSettings = (GanttSettingsDataSet.GanttSettingsDataTable)base.Table;
			}

			// Token: 0x1700078D RID: 1933
			// (get) Token: 0x06001A38 RID: 6712 RVA: 0x00054FD8 File Offset: 0x000531D8
			// (set) Token: 0x06001A39 RID: 6713 RVA: 0x0005501C File Offset: 0x0005321C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WGANTT_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableGanttSettings.WGANTT_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_UID' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x1700078E RID: 1934
			// (get) Token: 0x06001A3A RID: 6714 RVA: 0x00055038 File Offset: 0x00053238
			// (set) Token: 0x06001A3B RID: 6715 RVA: 0x0005507C File Offset: 0x0005327C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_STYLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_STYLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_STYLE_ID' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_STYLE_IDColumn] = value;
				}
			}

			// Token: 0x1700078F RID: 1935
			// (get) Token: 0x06001A3C RID: 6716 RVA: 0x00055098 File Offset: 0x00053298
			// (set) Token: 0x06001A3D RID: 6717 RVA: 0x000550DC File Offset: 0x000532DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_SHOW
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_SHOWColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SHOW' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_SHOWColumn] = value;
				}
			}

			// Token: 0x17000790 RID: 1936
			// (get) Token: 0x06001A3E RID: 6718 RVA: 0x000550F8 File Offset: 0x000532F8
			// (set) Token: 0x06001A3F RID: 6719 RVA: 0x0005513C File Offset: 0x0005333C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_BAR_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_BAR_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_BAR_TYPE' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_BAR_TYPEColumn] = value;
				}
			}

			// Token: 0x17000791 RID: 1937
			// (get) Token: 0x06001A40 RID: 6720 RVA: 0x00055158 File Offset: 0x00053358
			// (set) Token: 0x06001A41 RID: 6721 RVA: 0x0005519C File Offset: 0x0005339C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGANTT_BAR_PATTERN
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_BAR_PATTERNColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_BAR_PATTERN' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_BAR_PATTERNColumn] = value;
				}
			}

			// Token: 0x17000792 RID: 1938
			// (get) Token: 0x06001A42 RID: 6722 RVA: 0x000551B8 File Offset: 0x000533B8
			// (set) Token: 0x06001A43 RID: 6723 RVA: 0x000551FC File Offset: 0x000533FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGANTT_BAR_COLOR
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_BAR_COLORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_BAR_COLOR' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_BAR_COLORColumn] = value;
				}
			}

			// Token: 0x17000793 RID: 1939
			// (get) Token: 0x06001A44 RID: 6724 RVA: 0x00055218 File Offset: 0x00053418
			// (set) Token: 0x06001A45 RID: 6725 RVA: 0x0005525C File Offset: 0x0005345C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGANTT_START_SHAPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_START_SHAPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_START_SHAPE' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_START_SHAPEColumn] = value;
				}
			}

			// Token: 0x17000794 RID: 1940
			// (get) Token: 0x06001A46 RID: 6726 RVA: 0x00055278 File Offset: 0x00053478
			// (set) Token: 0x06001A47 RID: 6727 RVA: 0x000552BC File Offset: 0x000534BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGANTT_START_COLOR
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_START_COLORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_START_COLOR' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_START_COLORColumn] = value;
				}
			}

			// Token: 0x17000795 RID: 1941
			// (get) Token: 0x06001A48 RID: 6728 RVA: 0x000552D8 File Offset: 0x000534D8
			// (set) Token: 0x06001A49 RID: 6729 RVA: 0x0005531C File Offset: 0x0005351C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_END_SHAPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_END_SHAPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_END_SHAPE' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_END_SHAPEColumn] = value;
				}
			}

			// Token: 0x17000796 RID: 1942
			// (get) Token: 0x06001A4A RID: 6730 RVA: 0x00055338 File Offset: 0x00053538
			// (set) Token: 0x06001A4B RID: 6731 RVA: 0x0005537C File Offset: 0x0005357C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGANTT_END_COLOR
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGanttSettings.WGANTT_END_COLORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_END_COLOR' in table 'GanttSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGanttSettings.WGANTT_END_COLORColumn] = value;
				}
			}

			// Token: 0x06001A4C RID: 6732 RVA: 0x00055395 File Offset: 0x00053595
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGANTT_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_SCHEME_UIDColumn);
			}

			// Token: 0x06001A4D RID: 6733 RVA: 0x000553A8 File Offset: 0x000535A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_SCHEME_UIDNull()
			{
				base[this.tableGanttSettings.WGANTT_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06001A4E RID: 6734 RVA: 0x000553C0 File Offset: 0x000535C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_STYLE_IDNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_STYLE_IDColumn);
			}

			// Token: 0x06001A4F RID: 6735 RVA: 0x000553D3 File Offset: 0x000535D3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_STYLE_IDNull()
			{
				base[this.tableGanttSettings.WGANTT_STYLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x06001A50 RID: 6736 RVA: 0x000553EB File Offset: 0x000535EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_SHOWNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_SHOWColumn);
			}

			// Token: 0x06001A51 RID: 6737 RVA: 0x000553FE File Offset: 0x000535FE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_SHOWNull()
			{
				base[this.tableGanttSettings.WGANTT_SHOWColumn] = Convert.DBNull;
			}

			// Token: 0x06001A52 RID: 6738 RVA: 0x00055416 File Offset: 0x00053616
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_BAR_TYPENull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_BAR_TYPEColumn);
			}

			// Token: 0x06001A53 RID: 6739 RVA: 0x00055429 File Offset: 0x00053629
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_BAR_TYPENull()
			{
				base[this.tableGanttSettings.WGANTT_BAR_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x06001A54 RID: 6740 RVA: 0x00055441 File Offset: 0x00053641
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_BAR_PATTERNNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_BAR_PATTERNColumn);
			}

			// Token: 0x06001A55 RID: 6741 RVA: 0x00055454 File Offset: 0x00053654
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_BAR_PATTERNNull()
			{
				base[this.tableGanttSettings.WGANTT_BAR_PATTERNColumn] = Convert.DBNull;
			}

			// Token: 0x06001A56 RID: 6742 RVA: 0x0005546C File Offset: 0x0005366C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_BAR_COLORNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_BAR_COLORColumn);
			}

			// Token: 0x06001A57 RID: 6743 RVA: 0x0005547F File Offset: 0x0005367F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_BAR_COLORNull()
			{
				base[this.tableGanttSettings.WGANTT_BAR_COLORColumn] = Convert.DBNull;
			}

			// Token: 0x06001A58 RID: 6744 RVA: 0x00055497 File Offset: 0x00053697
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_START_SHAPENull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_START_SHAPEColumn);
			}

			// Token: 0x06001A59 RID: 6745 RVA: 0x000554AA File Offset: 0x000536AA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_START_SHAPENull()
			{
				base[this.tableGanttSettings.WGANTT_START_SHAPEColumn] = Convert.DBNull;
			}

			// Token: 0x06001A5A RID: 6746 RVA: 0x000554C2 File Offset: 0x000536C2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_START_COLORNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_START_COLORColumn);
			}

			// Token: 0x06001A5B RID: 6747 RVA: 0x000554D5 File Offset: 0x000536D5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_START_COLORNull()
			{
				base[this.tableGanttSettings.WGANTT_START_COLORColumn] = Convert.DBNull;
			}

			// Token: 0x06001A5C RID: 6748 RVA: 0x000554ED File Offset: 0x000536ED
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGANTT_END_SHAPENull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_END_SHAPEColumn);
			}

			// Token: 0x06001A5D RID: 6749 RVA: 0x00055500 File Offset: 0x00053700
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_END_SHAPENull()
			{
				base[this.tableGanttSettings.WGANTT_END_SHAPEColumn] = Convert.DBNull;
			}

			// Token: 0x06001A5E RID: 6750 RVA: 0x00055518 File Offset: 0x00053718
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGANTT_END_COLORNull()
			{
				return base.IsNull(this.tableGanttSettings.WGANTT_END_COLORColumn);
			}

			// Token: 0x06001A5F RID: 6751 RVA: 0x0005552B File Offset: 0x0005372B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGANTT_END_COLORNull()
			{
				base[this.tableGanttSettings.WGANTT_END_COLORColumn] = Convert.DBNull;
			}

			// Token: 0x040005B7 RID: 1463
			private GanttSettingsDataSet.GanttSettingsDataTable tableGanttSettings;
		}

		// Token: 0x02000168 RID: 360
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GanttSettingsRowChangeEvent : EventArgs
		{
			// Token: 0x06001A60 RID: 6752 RVA: 0x00055543 File Offset: 0x00053743
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSettingsRowChangeEvent(GanttSettingsDataSet.GanttSettingsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000797 RID: 1943
			// (get) Token: 0x06001A61 RID: 6753 RVA: 0x00055559 File Offset: 0x00053759
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GanttSettingsDataSet.GanttSettingsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17000798 RID: 1944
			// (get) Token: 0x06001A62 RID: 6754 RVA: 0x00055561 File Offset: 0x00053761
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040005B8 RID: 1464
			private GanttSettingsDataSet.GanttSettingsRow eventRow;

			// Token: 0x040005B9 RID: 1465
			private DataRowAction eventAction;
		}
	}
}
