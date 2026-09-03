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
	// Token: 0x020002C9 RID: 713
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("ProjectImpactDataSet")]
	[Serializable]
	public class ProjectImpactDataSet : DataSet
	{
		// Token: 0x060041A9 RID: 16809 RVA: 0x000D00F8 File Offset: 0x000CE2F8
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ProjectImpactValues, new string[]
			{
				"DRIVER_UID",
				"PROJ_UID",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.LookupTableValues, new string[]
			{
				"LT_VALUE_TEXT",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ImpactStatements, new string[]
			{
				"DRIVER_UID",
				"PROJECT_IMPACT_CF_UID",
				"DESCRIPTION",
				"LT_STRUCT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Drivers, new string[]
			{
				"DRIVER_UID",
				"DRIVER_DESCRIPTION",
				"DRIVER_NAME"
			});
		}

		// Token: 0x060041AA RID: 16810 RVA: 0x000D01B8 File Offset: 0x000CE3B8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ProjectImpactDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x060041AB RID: 16811 RVA: 0x000D020C File Offset: 0x000CE40C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected ProjectImpactDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["ProjectImpactValues"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.ProjectImpactValuesDataTable(dataSet.Tables["ProjectImpactValues"]));
				}
				if (dataSet.Tables["ImpactStatements"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.ImpactStatementsDataTable(dataSet.Tables["ImpactStatements"]));
				}
				if (dataSet.Tables["Drivers"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.DriversDataTable(dataSet.Tables["Drivers"]));
				}
				if (dataSet.Tables["LookupTableValues"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.LookupTableValuesDataTable(dataSet.Tables["LookupTableValues"]));
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

		// Token: 0x1700141B RID: 5147
		// (get) Token: 0x060041AC RID: 16812 RVA: 0x000D03FF File Offset: 0x000CE5FF
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ProjectImpactDataSet.ProjectImpactValuesDataTable ProjectImpactValues
		{
			get
			{
				return this.tableProjectImpactValues;
			}
		}

		// Token: 0x1700141C RID: 5148
		// (get) Token: 0x060041AD RID: 16813 RVA: 0x000D0407 File Offset: 0x000CE607
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ProjectImpactDataSet.ImpactStatementsDataTable ImpactStatements
		{
			get
			{
				return this.tableImpactStatements;
			}
		}

		// Token: 0x1700141D RID: 5149
		// (get) Token: 0x060041AE RID: 16814 RVA: 0x000D040F File Offset: 0x000CE60F
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DebuggerNonUserCode]
		public ProjectImpactDataSet.DriversDataTable Drivers
		{
			get
			{
				return this.tableDrivers;
			}
		}

		// Token: 0x1700141E RID: 5150
		// (get) Token: 0x060041AF RID: 16815 RVA: 0x000D0417 File Offset: 0x000CE617
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ProjectImpactDataSet.LookupTableValuesDataTable LookupTableValues
		{
			get
			{
				return this.tableLookupTableValues;
			}
		}

		// Token: 0x1700141F RID: 5151
		// (get) Token: 0x060041B0 RID: 16816 RVA: 0x000D041F File Offset: 0x000CE61F
		// (set) Token: 0x060041B1 RID: 16817 RVA: 0x000D0427 File Offset: 0x000CE627
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
		[Browsable(true)]
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

		// Token: 0x17001420 RID: 5152
		// (get) Token: 0x060041B2 RID: 16818 RVA: 0x000D0430 File Offset: 0x000CE630
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x17001421 RID: 5153
		// (get) Token: 0x060041B3 RID: 16819 RVA: 0x000D0438 File Offset: 0x000CE638
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x060041B4 RID: 16820 RVA: 0x000D0440 File Offset: 0x000CE640
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x060041B5 RID: 16821 RVA: 0x000D0454 File Offset: 0x000CE654
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			ProjectImpactDataSet projectImpactDataSet = (ProjectImpactDataSet)base.Clone();
			projectImpactDataSet.InitVars();
			projectImpactDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return projectImpactDataSet;
		}

		// Token: 0x060041B6 RID: 16822 RVA: 0x000D0480 File Offset: 0x000CE680
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x060041B7 RID: 16823 RVA: 0x000D0483 File Offset: 0x000CE683
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x060041B8 RID: 16824 RVA: 0x000D0488 File Offset: 0x000CE688
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["ProjectImpactValues"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.ProjectImpactValuesDataTable(dataSet.Tables["ProjectImpactValues"]));
				}
				if (dataSet.Tables["ImpactStatements"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.ImpactStatementsDataTable(dataSet.Tables["ImpactStatements"]));
				}
				if (dataSet.Tables["Drivers"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.DriversDataTable(dataSet.Tables["Drivers"]));
				}
				if (dataSet.Tables["LookupTableValues"] != null)
				{
					base.Tables.Add(new ProjectImpactDataSet.LookupTableValuesDataTable(dataSet.Tables["LookupTableValues"]));
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

		// Token: 0x060041B9 RID: 16825 RVA: 0x000D05E4 File Offset: 0x000CE7E4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x060041BA RID: 16826 RVA: 0x000D0618 File Offset: 0x000CE818
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x060041BB RID: 16827 RVA: 0x000D0624 File Offset: 0x000CE824
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableProjectImpactValues = (ProjectImpactDataSet.ProjectImpactValuesDataTable)base.Tables["ProjectImpactValues"];
			if (initTable && this.tableProjectImpactValues != null)
			{
				this.tableProjectImpactValues.InitVars();
			}
			this.tableImpactStatements = (ProjectImpactDataSet.ImpactStatementsDataTable)base.Tables["ImpactStatements"];
			if (initTable && this.tableImpactStatements != null)
			{
				this.tableImpactStatements.InitVars();
			}
			this.tableDrivers = (ProjectImpactDataSet.DriversDataTable)base.Tables["Drivers"];
			if (initTable && this.tableDrivers != null)
			{
				this.tableDrivers.InitVars();
			}
			this.tableLookupTableValues = (ProjectImpactDataSet.LookupTableValuesDataTable)base.Tables["LookupTableValues"];
			if (initTable && this.tableLookupTableValues != null)
			{
				this.tableLookupTableValues.InitVars();
			}
		}

		// Token: 0x060041BC RID: 16828 RVA: 0x000D06F8 File Offset: 0x000CE8F8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "ProjectImpactDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/ProjectImpactDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableProjectImpactValues = new ProjectImpactDataSet.ProjectImpactValuesDataTable();
			base.Tables.Add(this.tableProjectImpactValues);
			this.tableImpactStatements = new ProjectImpactDataSet.ImpactStatementsDataTable();
			base.Tables.Add(this.tableImpactStatements);
			this.tableDrivers = new ProjectImpactDataSet.DriversDataTable();
			base.Tables.Add(this.tableDrivers);
			this.tableLookupTableValues = new ProjectImpactDataSet.LookupTableValuesDataTable();
			base.Tables.Add(this.tableLookupTableValues);
		}

		// Token: 0x060041BD RID: 16829 RVA: 0x000D07A4 File Offset: 0x000CE9A4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeProjectImpactValues()
		{
			return false;
		}

		// Token: 0x060041BE RID: 16830 RVA: 0x000D07A7 File Offset: 0x000CE9A7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeImpactStatements()
		{
			return false;
		}

		// Token: 0x060041BF RID: 16831 RVA: 0x000D07AA File Offset: 0x000CE9AA
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeDrivers()
		{
			return false;
		}

		// Token: 0x060041C0 RID: 16832 RVA: 0x000D07AD File Offset: 0x000CE9AD
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeLookupTableValues()
		{
			return false;
		}

		// Token: 0x060041C1 RID: 16833 RVA: 0x000D07B0 File Offset: 0x000CE9B0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060041C2 RID: 16834 RVA: 0x000D07C4 File Offset: 0x000CE9C4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			ProjectImpactDataSet projectImpactDataSet = new ProjectImpactDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = projectImpactDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = projectImpactDataSet.GetSchemaSerializable();
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

		// Token: 0x04000DA9 RID: 3497
		private ProjectImpactDataSet.ProjectImpactValuesDataTable tableProjectImpactValues;

		// Token: 0x04000DAA RID: 3498
		private ProjectImpactDataSet.ImpactStatementsDataTable tableImpactStatements;

		// Token: 0x04000DAB RID: 3499
		private ProjectImpactDataSet.DriversDataTable tableDrivers;

		// Token: 0x04000DAC RID: 3500
		private ProjectImpactDataSet.LookupTableValuesDataTable tableLookupTableValues;

		// Token: 0x04000DAD RID: 3501
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x020002CA RID: 714
		// (Invoke) Token: 0x060041C4 RID: 16836
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ProjectImpactValuesRowChangeEventHandler(object sender, ProjectImpactDataSet.ProjectImpactValuesRowChangeEvent e);

		// Token: 0x020002CB RID: 715
		// (Invoke) Token: 0x060041C8 RID: 16840
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ImpactStatementsRowChangeEventHandler(object sender, ProjectImpactDataSet.ImpactStatementsRowChangeEvent e);

		// Token: 0x020002CC RID: 716
		// (Invoke) Token: 0x060041CC RID: 16844
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void DriversRowChangeEventHandler(object sender, ProjectImpactDataSet.DriversRowChangeEvent e);

		// Token: 0x020002CD RID: 717
		// (Invoke) Token: 0x060041D0 RID: 16848
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void LookupTableValuesRowChangeEventHandler(object sender, ProjectImpactDataSet.LookupTableValuesRowChangeEvent e);

		// Token: 0x020002CE RID: 718
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ProjectImpactValuesDataTable : DataTable, IEnumerable
		{
			// Token: 0x060041D3 RID: 16851 RVA: 0x000D090C File Offset: 0x000CEB0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactValuesDataTable()
			{
				base.TableName = "ProjectImpactValues";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060041D4 RID: 16852 RVA: 0x000D0934 File Offset: 0x000CEB34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ProjectImpactValuesDataTable(DataTable table)
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

			// Token: 0x060041D5 RID: 16853 RVA: 0x000D09DC File Offset: 0x000CEBDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected ProjectImpactValuesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001422 RID: 5154
			// (get) Token: 0x060041D6 RID: 16854 RVA: 0x000D09EC File Offset: 0x000CEBEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJ_UIDColumn
			{
				get
				{
					return this.columnPROJ_UID;
				}
			}

			// Token: 0x17001423 RID: 5155
			// (get) Token: 0x060041D7 RID: 16855 RVA: 0x000D09F4 File Offset: 0x000CEBF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x17001424 RID: 5156
			// (get) Token: 0x060041D8 RID: 16856 RVA: 0x000D09FC File Offset: 0x000CEBFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17001425 RID: 5157
			// (get) Token: 0x060041D9 RID: 16857 RVA: 0x000D0A04 File Offset: 0x000CEC04
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

			// Token: 0x17001426 RID: 5158
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ProjectImpactValuesRow this[int index]
			{
				get
				{
					return (ProjectImpactDataSet.ProjectImpactValuesRow)base.Rows[index];
				}
			}

			// Token: 0x14000269 RID: 617
			// (add) Token: 0x060041DB RID: 16859 RVA: 0x000D0A24 File Offset: 0x000CEC24
			// (remove) Token: 0x060041DC RID: 16860 RVA: 0x000D0A5C File Offset: 0x000CEC5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ProjectImpactValuesRowChangeEventHandler ProjectImpactValuesRowChanging;

			// Token: 0x1400026A RID: 618
			// (add) Token: 0x060041DD RID: 16861 RVA: 0x000D0A94 File Offset: 0x000CEC94
			// (remove) Token: 0x060041DE RID: 16862 RVA: 0x000D0ACC File Offset: 0x000CECCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ProjectImpactValuesRowChangeEventHandler ProjectImpactValuesRowChanged;

			// Token: 0x1400026B RID: 619
			// (add) Token: 0x060041DF RID: 16863 RVA: 0x000D0B04 File Offset: 0x000CED04
			// (remove) Token: 0x060041E0 RID: 16864 RVA: 0x000D0B3C File Offset: 0x000CED3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ProjectImpactValuesRowChangeEventHandler ProjectImpactValuesRowDeleting;

			// Token: 0x1400026C RID: 620
			// (add) Token: 0x060041E1 RID: 16865 RVA: 0x000D0B74 File Offset: 0x000CED74
			// (remove) Token: 0x060041E2 RID: 16866 RVA: 0x000D0BAC File Offset: 0x000CEDAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ProjectImpactValuesRowChangeEventHandler ProjectImpactValuesRowDeleted;

			// Token: 0x060041E3 RID: 16867 RVA: 0x000D0BE1 File Offset: 0x000CEDE1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddProjectImpactValuesRow(ProjectImpactDataSet.ProjectImpactValuesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060041E4 RID: 16868 RVA: 0x000D0BF0 File Offset: 0x000CEDF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.ProjectImpactValuesRow AddProjectImpactValuesRow(Guid PROJ_UID, Guid DRIVER_UID, Guid LT_STRUCT_UID)
			{
				ProjectImpactDataSet.ProjectImpactValuesRow projectImpactValuesRow = (ProjectImpactDataSet.ProjectImpactValuesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					PROJ_UID,
					DRIVER_UID,
					LT_STRUCT_UID
				};
				projectImpactValuesRow.ItemArray = itemArray;
				base.Rows.Add(projectImpactValuesRow);
				return projectImpactValuesRow;
			}

			// Token: 0x060041E5 RID: 16869 RVA: 0x000D0C44 File Offset: 0x000CEE44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ProjectImpactValuesRow FindByPROJ_UIDDRIVER_UID(Guid PROJ_UID, Guid DRIVER_UID)
			{
				return (ProjectImpactDataSet.ProjectImpactValuesRow)base.Rows.Find(new object[]
				{
					PROJ_UID,
					DRIVER_UID
				});
			}

			// Token: 0x060041E6 RID: 16870 RVA: 0x000D0C7B File Offset: 0x000CEE7B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060041E7 RID: 16871 RVA: 0x000D0C88 File Offset: 0x000CEE88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ProjectImpactDataSet.ProjectImpactValuesDataTable projectImpactValuesDataTable = (ProjectImpactDataSet.ProjectImpactValuesDataTable)base.Clone();
				projectImpactValuesDataTable.InitVars();
				return projectImpactValuesDataTable;
			}

			// Token: 0x060041E8 RID: 16872 RVA: 0x000D0CA8 File Offset: 0x000CEEA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new ProjectImpactDataSet.ProjectImpactValuesDataTable();
			}

			// Token: 0x060041E9 RID: 16873 RVA: 0x000D0CB0 File Offset: 0x000CEEB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnPROJ_UID = base.Columns["PROJ_UID"];
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
			}

			// Token: 0x060041EA RID: 16874 RVA: 0x000D0D00 File Offset: 0x000CEF00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnPROJ_UID = new DataColumn("PROJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJ_UID);
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnPROJ_UID,
					this.columnDRIVER_UID
				}, true));
				this.columnPROJ_UID.AllowDBNull = false;
				this.columnDRIVER_UID.AllowDBNull = false;
			}

			// Token: 0x060041EB RID: 16875 RVA: 0x000D0DDC File Offset: 0x000CEFDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ProjectImpactValuesRow NewProjectImpactValuesRow()
			{
				return (ProjectImpactDataSet.ProjectImpactValuesRow)base.NewRow();
			}

			// Token: 0x060041EC RID: 16876 RVA: 0x000D0DE9 File Offset: 0x000CEFE9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ProjectImpactDataSet.ProjectImpactValuesRow(builder);
			}

			// Token: 0x060041ED RID: 16877 RVA: 0x000D0DF1 File Offset: 0x000CEFF1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(ProjectImpactDataSet.ProjectImpactValuesRow);
			}

			// Token: 0x060041EE RID: 16878 RVA: 0x000D0DFD File Offset: 0x000CEFFD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ProjectImpactValuesRowChanged != null)
				{
					this.ProjectImpactValuesRowChanged(this, new ProjectImpactDataSet.ProjectImpactValuesRowChangeEvent((ProjectImpactDataSet.ProjectImpactValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060041EF RID: 16879 RVA: 0x000D0E30 File Offset: 0x000CF030
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ProjectImpactValuesRowChanging != null)
				{
					this.ProjectImpactValuesRowChanging(this, new ProjectImpactDataSet.ProjectImpactValuesRowChangeEvent((ProjectImpactDataSet.ProjectImpactValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060041F0 RID: 16880 RVA: 0x000D0E63 File Offset: 0x000CF063
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ProjectImpactValuesRowDeleted != null)
				{
					this.ProjectImpactValuesRowDeleted(this, new ProjectImpactDataSet.ProjectImpactValuesRowChangeEvent((ProjectImpactDataSet.ProjectImpactValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060041F1 RID: 16881 RVA: 0x000D0E96 File Offset: 0x000CF096
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ProjectImpactValuesRowDeleting != null)
				{
					this.ProjectImpactValuesRowDeleting(this, new ProjectImpactDataSet.ProjectImpactValuesRowChangeEvent((ProjectImpactDataSet.ProjectImpactValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060041F2 RID: 16882 RVA: 0x000D0EC9 File Offset: 0x000CF0C9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveProjectImpactValuesRow(ProjectImpactDataSet.ProjectImpactValuesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060041F3 RID: 16883 RVA: 0x000D0ED8 File Offset: 0x000CF0D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ProjectImpactDataSet projectImpactDataSet = new ProjectImpactDataSet();
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
				xmlSchemaAttribute.FixedValue = projectImpactDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ProjectImpactValuesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = projectImpactDataSet.GetSchemaSerializable();
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

			// Token: 0x04000DAE RID: 3502
			private DataColumn columnPROJ_UID;

			// Token: 0x04000DAF RID: 3503
			private DataColumn columnDRIVER_UID;

			// Token: 0x04000DB0 RID: 3504
			private DataColumn columnLT_STRUCT_UID;
		}

		// Token: 0x020002CF RID: 719
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ImpactStatementsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060041F4 RID: 16884 RVA: 0x000D10D0 File Offset: 0x000CF2D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ImpactStatementsDataTable()
			{
				base.TableName = "ImpactStatements";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060041F5 RID: 16885 RVA: 0x000D10F8 File Offset: 0x000CF2F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ImpactStatementsDataTable(DataTable table)
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

			// Token: 0x060041F6 RID: 16886 RVA: 0x000D11A0 File Offset: 0x000CF3A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ImpactStatementsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001427 RID: 5159
			// (get) Token: 0x060041F7 RID: 16887 RVA: 0x000D11B0 File Offset: 0x000CF3B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x17001428 RID: 5160
			// (get) Token: 0x060041F8 RID: 16888 RVA: 0x000D11B8 File Offset: 0x000CF3B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn PROJECT_IMPACT_CF_UIDColumn
			{
				get
				{
					return this.columnPROJECT_IMPACT_CF_UID;
				}
			}

			// Token: 0x17001429 RID: 5161
			// (get) Token: 0x060041F9 RID: 16889 RVA: 0x000D11C0 File Offset: 0x000CF3C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x1700142A RID: 5162
			// (get) Token: 0x060041FA RID: 16890 RVA: 0x000D11C8 File Offset: 0x000CF3C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DESCRIPTIONColumn
			{
				get
				{
					return this.columnDESCRIPTION;
				}
			}

			// Token: 0x1700142B RID: 5163
			// (get) Token: 0x060041FB RID: 16891 RVA: 0x000D11D0 File Offset: 0x000CF3D0
			[Browsable(false)]
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x1700142C RID: 5164
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.ImpactStatementsRow this[int index]
			{
				get
				{
					return (ProjectImpactDataSet.ImpactStatementsRow)base.Rows[index];
				}
			}

			// Token: 0x1400026D RID: 621
			// (add) Token: 0x060041FD RID: 16893 RVA: 0x000D11F0 File Offset: 0x000CF3F0
			// (remove) Token: 0x060041FE RID: 16894 RVA: 0x000D1228 File Offset: 0x000CF428
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ImpactStatementsRowChangeEventHandler ImpactStatementsRowChanging;

			// Token: 0x1400026E RID: 622
			// (add) Token: 0x060041FF RID: 16895 RVA: 0x000D1260 File Offset: 0x000CF460
			// (remove) Token: 0x06004200 RID: 16896 RVA: 0x000D1298 File Offset: 0x000CF498
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ImpactStatementsRowChangeEventHandler ImpactStatementsRowChanged;

			// Token: 0x1400026F RID: 623
			// (add) Token: 0x06004201 RID: 16897 RVA: 0x000D12D0 File Offset: 0x000CF4D0
			// (remove) Token: 0x06004202 RID: 16898 RVA: 0x000D1308 File Offset: 0x000CF508
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ImpactStatementsRowChangeEventHandler ImpactStatementsRowDeleting;

			// Token: 0x14000270 RID: 624
			// (add) Token: 0x06004203 RID: 16899 RVA: 0x000D1340 File Offset: 0x000CF540
			// (remove) Token: 0x06004204 RID: 16900 RVA: 0x000D1378 File Offset: 0x000CF578
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.ImpactStatementsRowChangeEventHandler ImpactStatementsRowDeleted;

			// Token: 0x06004205 RID: 16901 RVA: 0x000D13AD File Offset: 0x000CF5AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddImpactStatementsRow(ProjectImpactDataSet.ImpactStatementsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06004206 RID: 16902 RVA: 0x000D13BC File Offset: 0x000CF5BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ImpactStatementsRow AddImpactStatementsRow(Guid DRIVER_UID, Guid PROJECT_IMPACT_CF_UID, Guid LT_STRUCT_UID, string DESCRIPTION)
			{
				ProjectImpactDataSet.ImpactStatementsRow impactStatementsRow = (ProjectImpactDataSet.ImpactStatementsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					DRIVER_UID,
					PROJECT_IMPACT_CF_UID,
					LT_STRUCT_UID,
					DESCRIPTION
				};
				impactStatementsRow.ItemArray = itemArray;
				base.Rows.Add(impactStatementsRow);
				return impactStatementsRow;
			}

			// Token: 0x06004207 RID: 16903 RVA: 0x000D1414 File Offset: 0x000CF614
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.ImpactStatementsRow FindByDRIVER_UIDPROJECT_IMPACT_CF_UIDLT_STRUCT_UID(Guid DRIVER_UID, Guid PROJECT_IMPACT_CF_UID, Guid LT_STRUCT_UID)
			{
				return (ProjectImpactDataSet.ImpactStatementsRow)base.Rows.Find(new object[]
				{
					DRIVER_UID,
					PROJECT_IMPACT_CF_UID,
					LT_STRUCT_UID
				});
			}

			// Token: 0x06004208 RID: 16904 RVA: 0x000D1454 File Offset: 0x000CF654
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06004209 RID: 16905 RVA: 0x000D1464 File Offset: 0x000CF664
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ProjectImpactDataSet.ImpactStatementsDataTable impactStatementsDataTable = (ProjectImpactDataSet.ImpactStatementsDataTable)base.Clone();
				impactStatementsDataTable.InitVars();
				return impactStatementsDataTable;
			}

			// Token: 0x0600420A RID: 16906 RVA: 0x000D1484 File Offset: 0x000CF684
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new ProjectImpactDataSet.ImpactStatementsDataTable();
			}

			// Token: 0x0600420B RID: 16907 RVA: 0x000D148C File Offset: 0x000CF68C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnPROJECT_IMPACT_CF_UID = base.Columns["PROJECT_IMPACT_CF_UID"];
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnDESCRIPTION = base.Columns["DESCRIPTION"];
			}

			// Token: 0x0600420C RID: 16908 RVA: 0x000D14F4 File Offset: 0x000CF6F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnPROJECT_IMPACT_CF_UID = new DataColumn("PROJECT_IMPACT_CF_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnPROJECT_IMPACT_CF_UID);
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnDESCRIPTION = new DataColumn("DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDESCRIPTION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnDRIVER_UID,
					this.columnPROJECT_IMPACT_CF_UID,
					this.columnLT_STRUCT_UID
				}, true));
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDRIVER_UID.ReadOnly = true;
				this.columnPROJECT_IMPACT_CF_UID.AllowDBNull = false;
				this.columnPROJECT_IMPACT_CF_UID.ReadOnly = true;
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnDESCRIPTION.AllowDBNull = false;
				this.columnDESCRIPTION.ReadOnly = true;
				this.columnDESCRIPTION.MaxLength = 1000;
			}

			// Token: 0x0600420D RID: 16909 RVA: 0x000D165E File Offset: 0x000CF85E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ImpactStatementsRow NewImpactStatementsRow()
			{
				return (ProjectImpactDataSet.ImpactStatementsRow)base.NewRow();
			}

			// Token: 0x0600420E RID: 16910 RVA: 0x000D166B File Offset: 0x000CF86B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ProjectImpactDataSet.ImpactStatementsRow(builder);
			}

			// Token: 0x0600420F RID: 16911 RVA: 0x000D1673 File Offset: 0x000CF873
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ProjectImpactDataSet.ImpactStatementsRow);
			}

			// Token: 0x06004210 RID: 16912 RVA: 0x000D167F File Offset: 0x000CF87F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ImpactStatementsRowChanged != null)
				{
					this.ImpactStatementsRowChanged(this, new ProjectImpactDataSet.ImpactStatementsRowChangeEvent((ProjectImpactDataSet.ImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004211 RID: 16913 RVA: 0x000D16B2 File Offset: 0x000CF8B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ImpactStatementsRowChanging != null)
				{
					this.ImpactStatementsRowChanging(this, new ProjectImpactDataSet.ImpactStatementsRowChangeEvent((ProjectImpactDataSet.ImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004212 RID: 16914 RVA: 0x000D16E5 File Offset: 0x000CF8E5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ImpactStatementsRowDeleted != null)
				{
					this.ImpactStatementsRowDeleted(this, new ProjectImpactDataSet.ImpactStatementsRowChangeEvent((ProjectImpactDataSet.ImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004213 RID: 16915 RVA: 0x000D1718 File Offset: 0x000CF918
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ImpactStatementsRowDeleting != null)
				{
					this.ImpactStatementsRowDeleting(this, new ProjectImpactDataSet.ImpactStatementsRowChangeEvent((ProjectImpactDataSet.ImpactStatementsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004214 RID: 16916 RVA: 0x000D174B File Offset: 0x000CF94B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveImpactStatementsRow(ProjectImpactDataSet.ImpactStatementsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06004215 RID: 16917 RVA: 0x000D175C File Offset: 0x000CF95C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ProjectImpactDataSet projectImpactDataSet = new ProjectImpactDataSet();
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
				xmlSchemaAttribute.FixedValue = projectImpactDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ImpactStatementsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = projectImpactDataSet.GetSchemaSerializable();
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

			// Token: 0x04000DB5 RID: 3509
			private DataColumn columnDRIVER_UID;

			// Token: 0x04000DB6 RID: 3510
			private DataColumn columnPROJECT_IMPACT_CF_UID;

			// Token: 0x04000DB7 RID: 3511
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x04000DB8 RID: 3512
			private DataColumn columnDESCRIPTION;
		}

		// Token: 0x020002D0 RID: 720
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class DriversDataTable : DataTable, IEnumerable
		{
			// Token: 0x06004216 RID: 16918 RVA: 0x000D1954 File Offset: 0x000CFB54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriversDataTable()
			{
				base.TableName = "Drivers";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06004217 RID: 16919 RVA: 0x000D197C File Offset: 0x000CFB7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal DriversDataTable(DataTable table)
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

			// Token: 0x06004218 RID: 16920 RVA: 0x000D1A24 File Offset: 0x000CFC24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected DriversDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700142D RID: 5165
			// (get) Token: 0x06004219 RID: 16921 RVA: 0x000D1A34 File Offset: 0x000CFC34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DRIVER_UIDColumn
			{
				get
				{
					return this.columnDRIVER_UID;
				}
			}

			// Token: 0x1700142E RID: 5166
			// (get) Token: 0x0600421A RID: 16922 RVA: 0x000D1A3C File Offset: 0x000CFC3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_NAMEColumn
			{
				get
				{
					return this.columnDRIVER_NAME;
				}
			}

			// Token: 0x1700142F RID: 5167
			// (get) Token: 0x0600421B RID: 16923 RVA: 0x000D1A44 File Offset: 0x000CFC44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DRIVER_DESCRIPTIONColumn
			{
				get
				{
					return this.columnDRIVER_DESCRIPTION;
				}
			}

			// Token: 0x17001430 RID: 5168
			// (get) Token: 0x0600421C RID: 16924 RVA: 0x000D1A4C File Offset: 0x000CFC4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			[Browsable(false)]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17001431 RID: 5169
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.DriversRow this[int index]
			{
				get
				{
					return (ProjectImpactDataSet.DriversRow)base.Rows[index];
				}
			}

			// Token: 0x14000271 RID: 625
			// (add) Token: 0x0600421E RID: 16926 RVA: 0x000D1A6C File Offset: 0x000CFC6C
			// (remove) Token: 0x0600421F RID: 16927 RVA: 0x000D1AA4 File Offset: 0x000CFCA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.DriversRowChangeEventHandler DriversRowChanging;

			// Token: 0x14000272 RID: 626
			// (add) Token: 0x06004220 RID: 16928 RVA: 0x000D1ADC File Offset: 0x000CFCDC
			// (remove) Token: 0x06004221 RID: 16929 RVA: 0x000D1B14 File Offset: 0x000CFD14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.DriversRowChangeEventHandler DriversRowChanged;

			// Token: 0x14000273 RID: 627
			// (add) Token: 0x06004222 RID: 16930 RVA: 0x000D1B4C File Offset: 0x000CFD4C
			// (remove) Token: 0x06004223 RID: 16931 RVA: 0x000D1B84 File Offset: 0x000CFD84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.DriversRowChangeEventHandler DriversRowDeleting;

			// Token: 0x14000274 RID: 628
			// (add) Token: 0x06004224 RID: 16932 RVA: 0x000D1BBC File Offset: 0x000CFDBC
			// (remove) Token: 0x06004225 RID: 16933 RVA: 0x000D1BF4 File Offset: 0x000CFDF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.DriversRowChangeEventHandler DriversRowDeleted;

			// Token: 0x06004226 RID: 16934 RVA: 0x000D1C29 File Offset: 0x000CFE29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddDriversRow(ProjectImpactDataSet.DriversRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06004227 RID: 16935 RVA: 0x000D1C38 File Offset: 0x000CFE38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.DriversRow AddDriversRow(Guid DRIVER_UID, string DRIVER_NAME, string DRIVER_DESCRIPTION)
			{
				ProjectImpactDataSet.DriversRow driversRow = (ProjectImpactDataSet.DriversRow)base.NewRow();
				object[] itemArray = new object[]
				{
					DRIVER_UID,
					DRIVER_NAME,
					DRIVER_DESCRIPTION
				};
				driversRow.ItemArray = itemArray;
				base.Rows.Add(driversRow);
				return driversRow;
			}

			// Token: 0x06004228 RID: 16936 RVA: 0x000D1C80 File Offset: 0x000CFE80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.DriversRow FindByDRIVER_UID(Guid DRIVER_UID)
			{
				return (ProjectImpactDataSet.DriversRow)base.Rows.Find(new object[]
				{
					DRIVER_UID
				});
			}

			// Token: 0x06004229 RID: 16937 RVA: 0x000D1CAE File Offset: 0x000CFEAE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600422A RID: 16938 RVA: 0x000D1CBC File Offset: 0x000CFEBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ProjectImpactDataSet.DriversDataTable driversDataTable = (ProjectImpactDataSet.DriversDataTable)base.Clone();
				driversDataTable.InitVars();
				return driversDataTable;
			}

			// Token: 0x0600422B RID: 16939 RVA: 0x000D1CDC File Offset: 0x000CFEDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ProjectImpactDataSet.DriversDataTable();
			}

			// Token: 0x0600422C RID: 16940 RVA: 0x000D1CE4 File Offset: 0x000CFEE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDRIVER_UID = base.Columns["DRIVER_UID"];
				this.columnDRIVER_NAME = base.Columns["DRIVER_NAME"];
				this.columnDRIVER_DESCRIPTION = base.Columns["DRIVER_DESCRIPTION"];
			}

			// Token: 0x0600422D RID: 16941 RVA: 0x000D1D34 File Offset: 0x000CFF34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDRIVER_UID = new DataColumn("DRIVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_UID);
				this.columnDRIVER_NAME = new DataColumn("DRIVER_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_NAME);
				this.columnDRIVER_DESCRIPTION = new DataColumn("DRIVER_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDRIVER_DESCRIPTION);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnDRIVER_UID
				}, true));
				this.columnDRIVER_UID.AllowDBNull = false;
				this.columnDRIVER_UID.ReadOnly = true;
				this.columnDRIVER_UID.Unique = true;
				this.columnDRIVER_NAME.AllowDBNull = false;
				this.columnDRIVER_NAME.ReadOnly = true;
				this.columnDRIVER_NAME.MaxLength = 255;
				this.columnDRIVER_DESCRIPTION.ReadOnly = true;
				this.columnDRIVER_DESCRIPTION.MaxLength = 1000;
			}

			// Token: 0x0600422E RID: 16942 RVA: 0x000D1E57 File Offset: 0x000D0057
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.DriversRow NewDriversRow()
			{
				return (ProjectImpactDataSet.DriversRow)base.NewRow();
			}

			// Token: 0x0600422F RID: 16943 RVA: 0x000D1E64 File Offset: 0x000D0064
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ProjectImpactDataSet.DriversRow(builder);
			}

			// Token: 0x06004230 RID: 16944 RVA: 0x000D1E6C File Offset: 0x000D006C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(ProjectImpactDataSet.DriversRow);
			}

			// Token: 0x06004231 RID: 16945 RVA: 0x000D1E78 File Offset: 0x000D0078
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.DriversRowChanged != null)
				{
					this.DriversRowChanged(this, new ProjectImpactDataSet.DriversRowChangeEvent((ProjectImpactDataSet.DriversRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004232 RID: 16946 RVA: 0x000D1EAB File Offset: 0x000D00AB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.DriversRowChanging != null)
				{
					this.DriversRowChanging(this, new ProjectImpactDataSet.DriversRowChangeEvent((ProjectImpactDataSet.DriversRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004233 RID: 16947 RVA: 0x000D1EDE File Offset: 0x000D00DE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.DriversRowDeleted != null)
				{
					this.DriversRowDeleted(this, new ProjectImpactDataSet.DriversRowChangeEvent((ProjectImpactDataSet.DriversRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004234 RID: 16948 RVA: 0x000D1F11 File Offset: 0x000D0111
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.DriversRowDeleting != null)
				{
					this.DriversRowDeleting(this, new ProjectImpactDataSet.DriversRowChangeEvent((ProjectImpactDataSet.DriversRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004235 RID: 16949 RVA: 0x000D1F44 File Offset: 0x000D0144
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveDriversRow(ProjectImpactDataSet.DriversRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06004236 RID: 16950 RVA: 0x000D1F54 File Offset: 0x000D0154
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ProjectImpactDataSet projectImpactDataSet = new ProjectImpactDataSet();
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
				xmlSchemaAttribute.FixedValue = projectImpactDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "DriversDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = projectImpactDataSet.GetSchemaSerializable();
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

			// Token: 0x04000DBD RID: 3517
			private DataColumn columnDRIVER_UID;

			// Token: 0x04000DBE RID: 3518
			private DataColumn columnDRIVER_NAME;

			// Token: 0x04000DBF RID: 3519
			private DataColumn columnDRIVER_DESCRIPTION;
		}

		// Token: 0x020002D1 RID: 721
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class LookupTableValuesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06004237 RID: 16951 RVA: 0x000D214C File Offset: 0x000D034C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public LookupTableValuesDataTable()
			{
				base.TableName = "LookupTableValues";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06004238 RID: 16952 RVA: 0x000D2174 File Offset: 0x000D0374
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal LookupTableValuesDataTable(DataTable table)
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

			// Token: 0x06004239 RID: 16953 RVA: 0x000D221C File Offset: 0x000D041C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected LookupTableValuesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001432 RID: 5170
			// (get) Token: 0x0600423A RID: 16954 RVA: 0x000D222C File Offset: 0x000D042C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn LT_STRUCT_UIDColumn
			{
				get
				{
					return this.columnLT_STRUCT_UID;
				}
			}

			// Token: 0x17001433 RID: 5171
			// (get) Token: 0x0600423B RID: 16955 RVA: 0x000D2234 File Offset: 0x000D0434
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn LT_VALUE_TEXTColumn
			{
				get
				{
					return this.columnLT_VALUE_TEXT;
				}
			}

			// Token: 0x17001434 RID: 5172
			// (get) Token: 0x0600423C RID: 16956 RVA: 0x000D223C File Offset: 0x000D043C
			[Browsable(false)]
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17001435 RID: 5173
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.LookupTableValuesRow this[int index]
			{
				get
				{
					return (ProjectImpactDataSet.LookupTableValuesRow)base.Rows[index];
				}
			}

			// Token: 0x14000275 RID: 629
			// (add) Token: 0x0600423E RID: 16958 RVA: 0x000D225C File Offset: 0x000D045C
			// (remove) Token: 0x0600423F RID: 16959 RVA: 0x000D2294 File Offset: 0x000D0494
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.LookupTableValuesRowChangeEventHandler LookupTableValuesRowChanging;

			// Token: 0x14000276 RID: 630
			// (add) Token: 0x06004240 RID: 16960 RVA: 0x000D22CC File Offset: 0x000D04CC
			// (remove) Token: 0x06004241 RID: 16961 RVA: 0x000D2304 File Offset: 0x000D0504
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.LookupTableValuesRowChangeEventHandler LookupTableValuesRowChanged;

			// Token: 0x14000277 RID: 631
			// (add) Token: 0x06004242 RID: 16962 RVA: 0x000D233C File Offset: 0x000D053C
			// (remove) Token: 0x06004243 RID: 16963 RVA: 0x000D2374 File Offset: 0x000D0574
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.LookupTableValuesRowChangeEventHandler LookupTableValuesRowDeleting;

			// Token: 0x14000278 RID: 632
			// (add) Token: 0x06004244 RID: 16964 RVA: 0x000D23AC File Offset: 0x000D05AC
			// (remove) Token: 0x06004245 RID: 16965 RVA: 0x000D23E4 File Offset: 0x000D05E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ProjectImpactDataSet.LookupTableValuesRowChangeEventHandler LookupTableValuesRowDeleted;

			// Token: 0x06004246 RID: 16966 RVA: 0x000D2419 File Offset: 0x000D0619
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddLookupTableValuesRow(ProjectImpactDataSet.LookupTableValuesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06004247 RID: 16967 RVA: 0x000D2428 File Offset: 0x000D0628
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.LookupTableValuesRow AddLookupTableValuesRow(Guid LT_STRUCT_UID, string LT_VALUE_TEXT)
			{
				ProjectImpactDataSet.LookupTableValuesRow lookupTableValuesRow = (ProjectImpactDataSet.LookupTableValuesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					LT_STRUCT_UID,
					LT_VALUE_TEXT
				};
				lookupTableValuesRow.ItemArray = itemArray;
				base.Rows.Add(lookupTableValuesRow);
				return lookupTableValuesRow;
			}

			// Token: 0x06004248 RID: 16968 RVA: 0x000D246B File Offset: 0x000D066B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06004249 RID: 16969 RVA: 0x000D2478 File Offset: 0x000D0678
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ProjectImpactDataSet.LookupTableValuesDataTable lookupTableValuesDataTable = (ProjectImpactDataSet.LookupTableValuesDataTable)base.Clone();
				lookupTableValuesDataTable.InitVars();
				return lookupTableValuesDataTable;
			}

			// Token: 0x0600424A RID: 16970 RVA: 0x000D2498 File Offset: 0x000D0698
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ProjectImpactDataSet.LookupTableValuesDataTable();
			}

			// Token: 0x0600424B RID: 16971 RVA: 0x000D249F File Offset: 0x000D069F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnLT_STRUCT_UID = base.Columns["LT_STRUCT_UID"];
				this.columnLT_VALUE_TEXT = base.Columns["LT_VALUE_TEXT"];
			}

			// Token: 0x0600424C RID: 16972 RVA: 0x000D24D0 File Offset: 0x000D06D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnLT_STRUCT_UID = new DataColumn("LT_STRUCT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnLT_STRUCT_UID);
				this.columnLT_VALUE_TEXT = new DataColumn("LT_VALUE_TEXT", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnLT_VALUE_TEXT);
				this.columnLT_STRUCT_UID.AllowDBNull = false;
				this.columnLT_STRUCT_UID.ReadOnly = true;
				this.columnLT_VALUE_TEXT.ReadOnly = true;
				this.columnLT_VALUE_TEXT.MaxLength = 255;
			}

			// Token: 0x0600424D RID: 16973 RVA: 0x000D256B File Offset: 0x000D076B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.LookupTableValuesRow NewLookupTableValuesRow()
			{
				return (ProjectImpactDataSet.LookupTableValuesRow)base.NewRow();
			}

			// Token: 0x0600424E RID: 16974 RVA: 0x000D2578 File Offset: 0x000D0778
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ProjectImpactDataSet.LookupTableValuesRow(builder);
			}

			// Token: 0x0600424F RID: 16975 RVA: 0x000D2580 File Offset: 0x000D0780
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(ProjectImpactDataSet.LookupTableValuesRow);
			}

			// Token: 0x06004250 RID: 16976 RVA: 0x000D258C File Offset: 0x000D078C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.LookupTableValuesRowChanged != null)
				{
					this.LookupTableValuesRowChanged(this, new ProjectImpactDataSet.LookupTableValuesRowChangeEvent((ProjectImpactDataSet.LookupTableValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004251 RID: 16977 RVA: 0x000D25BF File Offset: 0x000D07BF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.LookupTableValuesRowChanging != null)
				{
					this.LookupTableValuesRowChanging(this, new ProjectImpactDataSet.LookupTableValuesRowChangeEvent((ProjectImpactDataSet.LookupTableValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004252 RID: 16978 RVA: 0x000D25F2 File Offset: 0x000D07F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.LookupTableValuesRowDeleted != null)
				{
					this.LookupTableValuesRowDeleted(this, new ProjectImpactDataSet.LookupTableValuesRowChangeEvent((ProjectImpactDataSet.LookupTableValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004253 RID: 16979 RVA: 0x000D2625 File Offset: 0x000D0825
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.LookupTableValuesRowDeleting != null)
				{
					this.LookupTableValuesRowDeleting(this, new ProjectImpactDataSet.LookupTableValuesRowChangeEvent((ProjectImpactDataSet.LookupTableValuesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06004254 RID: 16980 RVA: 0x000D2658 File Offset: 0x000D0858
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveLookupTableValuesRow(ProjectImpactDataSet.LookupTableValuesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06004255 RID: 16981 RVA: 0x000D2668 File Offset: 0x000D0868
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ProjectImpactDataSet projectImpactDataSet = new ProjectImpactDataSet();
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
				xmlSchemaAttribute.FixedValue = projectImpactDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "LookupTableValuesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = projectImpactDataSet.GetSchemaSerializable();
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

			// Token: 0x04000DC4 RID: 3524
			private DataColumn columnLT_STRUCT_UID;

			// Token: 0x04000DC5 RID: 3525
			private DataColumn columnLT_VALUE_TEXT;
		}

		// Token: 0x020002D2 RID: 722
		public class ProjectImpactValuesRow : DataRow
		{
			// Token: 0x06004256 RID: 16982 RVA: 0x000D2860 File Offset: 0x000D0A60
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ProjectImpactValuesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableProjectImpactValues = (ProjectImpactDataSet.ProjectImpactValuesDataTable)base.Table;
			}

			// Token: 0x17001436 RID: 5174
			// (get) Token: 0x06004257 RID: 16983 RVA: 0x000D287A File Offset: 0x000D0A7A
			// (set) Token: 0x06004258 RID: 16984 RVA: 0x000D2892 File Offset: 0x000D0A92
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJ_UID
			{
				get
				{
					return (Guid)base[this.tableProjectImpactValues.PROJ_UIDColumn];
				}
				set
				{
					base[this.tableProjectImpactValues.PROJ_UIDColumn] = value;
				}
			}

			// Token: 0x17001437 RID: 5175
			// (get) Token: 0x06004259 RID: 16985 RVA: 0x000D28AB File Offset: 0x000D0AAB
			// (set) Token: 0x0600425A RID: 16986 RVA: 0x000D28C3 File Offset: 0x000D0AC3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableProjectImpactValues.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableProjectImpactValues.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x17001438 RID: 5176
			// (get) Token: 0x0600425B RID: 16987 RVA: 0x000D28DC File Offset: 0x000D0ADC
			// (set) Token: 0x0600425C RID: 16988 RVA: 0x000D2920 File Offset: 0x000D0B20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid LT_STRUCT_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableProjectImpactValues.LT_STRUCT_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LT_STRUCT_UID' in table 'ProjectImpactValues' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableProjectImpactValues.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x0600425D RID: 16989 RVA: 0x000D2939 File Offset: 0x000D0B39
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLT_STRUCT_UIDNull()
			{
				return base.IsNull(this.tableProjectImpactValues.LT_STRUCT_UIDColumn);
			}

			// Token: 0x0600425E RID: 16990 RVA: 0x000D294C File Offset: 0x000D0B4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLT_STRUCT_UIDNull()
			{
				base[this.tableProjectImpactValues.LT_STRUCT_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x04000DCA RID: 3530
			private ProjectImpactDataSet.ProjectImpactValuesDataTable tableProjectImpactValues;
		}

		// Token: 0x020002D3 RID: 723
		public class ImpactStatementsRow : DataRow
		{
			// Token: 0x0600425F RID: 16991 RVA: 0x000D2964 File Offset: 0x000D0B64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ImpactStatementsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableImpactStatements = (ProjectImpactDataSet.ImpactStatementsDataTable)base.Table;
			}

			// Token: 0x17001439 RID: 5177
			// (get) Token: 0x06004260 RID: 16992 RVA: 0x000D297E File Offset: 0x000D0B7E
			// (set) Token: 0x06004261 RID: 16993 RVA: 0x000D2996 File Offset: 0x000D0B96
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableImpactStatements.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableImpactStatements.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x1700143A RID: 5178
			// (get) Token: 0x06004262 RID: 16994 RVA: 0x000D29AF File Offset: 0x000D0BAF
			// (set) Token: 0x06004263 RID: 16995 RVA: 0x000D29C7 File Offset: 0x000D0BC7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid PROJECT_IMPACT_CF_UID
			{
				get
				{
					return (Guid)base[this.tableImpactStatements.PROJECT_IMPACT_CF_UIDColumn];
				}
				set
				{
					base[this.tableImpactStatements.PROJECT_IMPACT_CF_UIDColumn] = value;
				}
			}

			// Token: 0x1700143B RID: 5179
			// (get) Token: 0x06004264 RID: 16996 RVA: 0x000D29E0 File Offset: 0x000D0BE0
			// (set) Token: 0x06004265 RID: 16997 RVA: 0x000D29F8 File Offset: 0x000D0BF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableImpactStatements.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableImpactStatements.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x1700143C RID: 5180
			// (get) Token: 0x06004266 RID: 16998 RVA: 0x000D2A11 File Offset: 0x000D0C11
			// (set) Token: 0x06004267 RID: 16999 RVA: 0x000D2A29 File Offset: 0x000D0C29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string DESCRIPTION
			{
				get
				{
					return (string)base[this.tableImpactStatements.DESCRIPTIONColumn];
				}
				set
				{
					base[this.tableImpactStatements.DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x04000DCB RID: 3531
			private ProjectImpactDataSet.ImpactStatementsDataTable tableImpactStatements;
		}

		// Token: 0x020002D4 RID: 724
		public class DriversRow : DataRow
		{
			// Token: 0x06004268 RID: 17000 RVA: 0x000D2A3D File Offset: 0x000D0C3D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal DriversRow(DataRowBuilder rb) : base(rb)
			{
				this.tableDrivers = (ProjectImpactDataSet.DriversDataTable)base.Table;
			}

			// Token: 0x1700143D RID: 5181
			// (get) Token: 0x06004269 RID: 17001 RVA: 0x000D2A57 File Offset: 0x000D0C57
			// (set) Token: 0x0600426A RID: 17002 RVA: 0x000D2A6F File Offset: 0x000D0C6F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DRIVER_UID
			{
				get
				{
					return (Guid)base[this.tableDrivers.DRIVER_UIDColumn];
				}
				set
				{
					base[this.tableDrivers.DRIVER_UIDColumn] = value;
				}
			}

			// Token: 0x1700143E RID: 5182
			// (get) Token: 0x0600426B RID: 17003 RVA: 0x000D2A88 File Offset: 0x000D0C88
			// (set) Token: 0x0600426C RID: 17004 RVA: 0x000D2AA0 File Offset: 0x000D0CA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string DRIVER_NAME
			{
				get
				{
					return (string)base[this.tableDrivers.DRIVER_NAMEColumn];
				}
				set
				{
					base[this.tableDrivers.DRIVER_NAMEColumn] = value;
				}
			}

			// Token: 0x1700143F RID: 5183
			// (get) Token: 0x0600426D RID: 17005 RVA: 0x000D2AB4 File Offset: 0x000D0CB4
			// (set) Token: 0x0600426E RID: 17006 RVA: 0x000D2AF8 File Offset: 0x000D0CF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DRIVER_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableDrivers.DRIVER_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DRIVER_DESCRIPTION' in table 'Drivers' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableDrivers.DRIVER_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x0600426F RID: 17007 RVA: 0x000D2B0C File Offset: 0x000D0D0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsDRIVER_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableDrivers.DRIVER_DESCRIPTIONColumn);
			}

			// Token: 0x06004270 RID: 17008 RVA: 0x000D2B1F File Offset: 0x000D0D1F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDRIVER_DESCRIPTIONNull()
			{
				base[this.tableDrivers.DRIVER_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x04000DCC RID: 3532
			private ProjectImpactDataSet.DriversDataTable tableDrivers;
		}

		// Token: 0x020002D5 RID: 725
		public class LookupTableValuesRow : DataRow
		{
			// Token: 0x06004271 RID: 17009 RVA: 0x000D2B37 File Offset: 0x000D0D37
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal LookupTableValuesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableLookupTableValues = (ProjectImpactDataSet.LookupTableValuesDataTable)base.Table;
			}

			// Token: 0x17001440 RID: 5184
			// (get) Token: 0x06004272 RID: 17010 RVA: 0x000D2B51 File Offset: 0x000D0D51
			// (set) Token: 0x06004273 RID: 17011 RVA: 0x000D2B69 File Offset: 0x000D0D69
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid LT_STRUCT_UID
			{
				get
				{
					return (Guid)base[this.tableLookupTableValues.LT_STRUCT_UIDColumn];
				}
				set
				{
					base[this.tableLookupTableValues.LT_STRUCT_UIDColumn] = value;
				}
			}

			// Token: 0x17001441 RID: 5185
			// (get) Token: 0x06004274 RID: 17012 RVA: 0x000D2B84 File Offset: 0x000D0D84
			// (set) Token: 0x06004275 RID: 17013 RVA: 0x000D2BC8 File Offset: 0x000D0DC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string LT_VALUE_TEXT
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableLookupTableValues.LT_VALUE_TEXTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'LT_VALUE_TEXT' in table 'LookupTableValues' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableLookupTableValues.LT_VALUE_TEXTColumn] = value;
				}
			}

			// Token: 0x06004276 RID: 17014 RVA: 0x000D2BDC File Offset: 0x000D0DDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsLT_VALUE_TEXTNull()
			{
				return base.IsNull(this.tableLookupTableValues.LT_VALUE_TEXTColumn);
			}

			// Token: 0x06004277 RID: 17015 RVA: 0x000D2BEF File Offset: 0x000D0DEF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetLT_VALUE_TEXTNull()
			{
				base[this.tableLookupTableValues.LT_VALUE_TEXTColumn] = Convert.DBNull;
			}

			// Token: 0x04000DCD RID: 3533
			private ProjectImpactDataSet.LookupTableValuesDataTable tableLookupTableValues;
		}

		// Token: 0x020002D6 RID: 726
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ProjectImpactValuesRowChangeEvent : EventArgs
		{
			// Token: 0x06004278 RID: 17016 RVA: 0x000D2C07 File Offset: 0x000D0E07
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactValuesRowChangeEvent(ProjectImpactDataSet.ProjectImpactValuesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001442 RID: 5186
			// (get) Token: 0x06004279 RID: 17017 RVA: 0x000D2C1D File Offset: 0x000D0E1D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ProjectImpactValuesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001443 RID: 5187
			// (get) Token: 0x0600427A RID: 17018 RVA: 0x000D2C25 File Offset: 0x000D0E25
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000DCE RID: 3534
			private ProjectImpactDataSet.ProjectImpactValuesRow eventRow;

			// Token: 0x04000DCF RID: 3535
			private DataRowAction eventAction;
		}

		// Token: 0x020002D7 RID: 727
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ImpactStatementsRowChangeEvent : EventArgs
		{
			// Token: 0x0600427B RID: 17019 RVA: 0x000D2C2D File Offset: 0x000D0E2D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ImpactStatementsRowChangeEvent(ProjectImpactDataSet.ImpactStatementsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001444 RID: 5188
			// (get) Token: 0x0600427C RID: 17020 RVA: 0x000D2C43 File Offset: 0x000D0E43
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.ImpactStatementsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001445 RID: 5189
			// (get) Token: 0x0600427D RID: 17021 RVA: 0x000D2C4B File Offset: 0x000D0E4B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000DD0 RID: 3536
			private ProjectImpactDataSet.ImpactStatementsRow eventRow;

			// Token: 0x04000DD1 RID: 3537
			private DataRowAction eventAction;
		}

		// Token: 0x020002D8 RID: 728
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class DriversRowChangeEvent : EventArgs
		{
			// Token: 0x0600427E RID: 17022 RVA: 0x000D2C53 File Offset: 0x000D0E53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DriversRowChangeEvent(ProjectImpactDataSet.DriversRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001446 RID: 5190
			// (get) Token: 0x0600427F RID: 17023 RVA: 0x000D2C69 File Offset: 0x000D0E69
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ProjectImpactDataSet.DriversRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001447 RID: 5191
			// (get) Token: 0x06004280 RID: 17024 RVA: 0x000D2C71 File Offset: 0x000D0E71
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000DD2 RID: 3538
			private ProjectImpactDataSet.DriversRow eventRow;

			// Token: 0x04000DD3 RID: 3539
			private DataRowAction eventAction;
		}

		// Token: 0x020002D9 RID: 729
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class LookupTableValuesRowChangeEvent : EventArgs
		{
			// Token: 0x06004281 RID: 17025 RVA: 0x000D2C79 File Offset: 0x000D0E79
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public LookupTableValuesRowChangeEvent(ProjectImpactDataSet.LookupTableValuesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001448 RID: 5192
			// (get) Token: 0x06004282 RID: 17026 RVA: 0x000D2C8F File Offset: 0x000D0E8F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ProjectImpactDataSet.LookupTableValuesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001449 RID: 5193
			// (get) Token: 0x06004283 RID: 17027 RVA: 0x000D2C97 File Offset: 0x000D0E97
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000DD4 RID: 3540
			private ProjectImpactDataSet.LookupTableValuesRow eventRow;

			// Token: 0x04000DD5 RID: 3541
			private DataRowAction eventAction;
		}
	}
}
