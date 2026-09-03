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
	// Token: 0x0200015A RID: 346
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("FiscalYearDataSet")]
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class FiscalYearDataSet : DataSet
	{
		// Token: 0x06001964 RID: 6500 RVA: 0x000522BC File Offset: 0x000504BC
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.FiscalYears, new string[]
			{
				"WFISCAL_YEAR",
				"WFISCAL_PERIOD_FINISH_DATE",
				"WFISCAL_PERIOD_START_DATE"
			});
		}

		// Token: 0x06001965 RID: 6501 RVA: 0x000522FC File Offset: 0x000504FC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public FiscalYearDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x00052350 File Offset: 0x00050550
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected FiscalYearDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["FiscalYears"] != null)
				{
					base.Tables.Add(new FiscalYearDataSet.FiscalYearsDataTable(dataSet.Tables["FiscalYears"]));
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

		// Token: 0x1700075F RID: 1887
		// (get) Token: 0x06001967 RID: 6503 RVA: 0x000524AD File Offset: 0x000506AD
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public FiscalYearDataSet.FiscalYearsDataTable FiscalYears
		{
			get
			{
				return this.tableFiscalYears;
			}
		}

		// Token: 0x17000760 RID: 1888
		// (get) Token: 0x06001968 RID: 6504 RVA: 0x000524B5 File Offset: 0x000506B5
		// (set) Token: 0x06001969 RID: 6505 RVA: 0x000524BD File Offset: 0x000506BD
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

		// Token: 0x17000761 RID: 1889
		// (get) Token: 0x0600196A RID: 6506 RVA: 0x000524C6 File Offset: 0x000506C6
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

		// Token: 0x17000762 RID: 1890
		// (get) Token: 0x0600196B RID: 6507 RVA: 0x000524CE File Offset: 0x000506CE
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

		// Token: 0x0600196C RID: 6508 RVA: 0x000524D6 File Offset: 0x000506D6
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x000524EC File Offset: 0x000506EC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			FiscalYearDataSet fiscalYearDataSet = (FiscalYearDataSet)base.Clone();
			fiscalYearDataSet.InitVars();
			fiscalYearDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return fiscalYearDataSet;
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x00052518 File Offset: 0x00050718
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x0005251B File Offset: 0x0005071B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x00052520 File Offset: 0x00050720
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["FiscalYears"] != null)
				{
					base.Tables.Add(new FiscalYearDataSet.FiscalYearsDataTable(dataSet.Tables["FiscalYears"]));
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

		// Token: 0x06001971 RID: 6513 RVA: 0x000525E8 File Offset: 0x000507E8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x0005261C File Offset: 0x0005081C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x00052625 File Offset: 0x00050825
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableFiscalYears = (FiscalYearDataSet.FiscalYearsDataTable)base.Tables["FiscalYears"];
			if (initTable && this.tableFiscalYears != null)
			{
				this.tableFiscalYears.InitVars();
			}
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x00052658 File Offset: 0x00050858
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "FiscalYearDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/FiscalYear/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableFiscalYears = new FiscalYearDataSet.FiscalYearsDataTable();
			base.Tables.Add(this.tableFiscalYears);
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x000526B0 File Offset: 0x000508B0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeFiscalYears()
		{
			return false;
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x000526B3 File Offset: 0x000508B3
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x000526C4 File Offset: 0x000508C4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			FiscalYearDataSet fiscalYearDataSet = new FiscalYearDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = fiscalYearDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = fiscalYearDataSet.GetSchemaSerializable();
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

		// Token: 0x0400058E RID: 1422
		private FiscalYearDataSet.FiscalYearsDataTable tableFiscalYears;

		// Token: 0x0400058F RID: 1423
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200015B RID: 347
		// (Invoke) Token: 0x06001979 RID: 6521
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void FiscalYearsRowChangeEventHandler(object sender, FiscalYearDataSet.FiscalYearsRowChangeEvent e);

		// Token: 0x0200015C RID: 348
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class FiscalYearsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600197C RID: 6524 RVA: 0x0005280C File Offset: 0x00050A0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalYearsDataTable()
			{
				base.TableName = "FiscalYears";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600197D RID: 6525 RVA: 0x00052834 File Offset: 0x00050A34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal FiscalYearsDataTable(DataTable table)
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

			// Token: 0x0600197E RID: 6526 RVA: 0x000528DC File Offset: 0x00050ADC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected FiscalYearsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17000763 RID: 1891
			// (get) Token: 0x0600197F RID: 6527 RVA: 0x000528EC File Offset: 0x00050AEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_YEARColumn
			{
				get
				{
					return this.columnWFISCAL_YEAR;
				}
			}

			// Token: 0x17000764 RID: 1892
			// (get) Token: 0x06001980 RID: 6528 RVA: 0x000528F4 File Offset: 0x00050AF4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_PERIOD_START_DATEColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_START_DATE;
				}
			}

			// Token: 0x17000765 RID: 1893
			// (get) Token: 0x06001981 RID: 6529 RVA: 0x000528FC File Offset: 0x00050AFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_PERIOD_FINISH_DATEColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_FINISH_DATE;
				}
			}

			// Token: 0x17000766 RID: 1894
			// (get) Token: 0x06001982 RID: 6530 RVA: 0x00052904 File Offset: 0x00050B04
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

			// Token: 0x17000767 RID: 1895
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalYearDataSet.FiscalYearsRow this[int index]
			{
				get
				{
					return (FiscalYearDataSet.FiscalYearsRow)base.Rows[index];
				}
			}

			// Token: 0x14000121 RID: 289
			// (add) Token: 0x06001984 RID: 6532 RVA: 0x00052924 File Offset: 0x00050B24
			// (remove) Token: 0x06001985 RID: 6533 RVA: 0x0005295C File Offset: 0x00050B5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalYearDataSet.FiscalYearsRowChangeEventHandler FiscalYearsRowChanging;

			// Token: 0x14000122 RID: 290
			// (add) Token: 0x06001986 RID: 6534 RVA: 0x00052994 File Offset: 0x00050B94
			// (remove) Token: 0x06001987 RID: 6535 RVA: 0x000529CC File Offset: 0x00050BCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalYearDataSet.FiscalYearsRowChangeEventHandler FiscalYearsRowChanged;

			// Token: 0x14000123 RID: 291
			// (add) Token: 0x06001988 RID: 6536 RVA: 0x00052A04 File Offset: 0x00050C04
			// (remove) Token: 0x06001989 RID: 6537 RVA: 0x00052A3C File Offset: 0x00050C3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalYearDataSet.FiscalYearsRowChangeEventHandler FiscalYearsRowDeleting;

			// Token: 0x14000124 RID: 292
			// (add) Token: 0x0600198A RID: 6538 RVA: 0x00052A74 File Offset: 0x00050C74
			// (remove) Token: 0x0600198B RID: 6539 RVA: 0x00052AAC File Offset: 0x00050CAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalYearDataSet.FiscalYearsRowChangeEventHandler FiscalYearsRowDeleted;

			// Token: 0x0600198C RID: 6540 RVA: 0x00052AE1 File Offset: 0x00050CE1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddFiscalYearsRow(FiscalYearDataSet.FiscalYearsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600198D RID: 6541 RVA: 0x00052AF0 File Offset: 0x00050CF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalYearDataSet.FiscalYearsRow AddFiscalYearsRow(int WFISCAL_YEAR, DateTime WFISCAL_PERIOD_START_DATE, DateTime WFISCAL_PERIOD_FINISH_DATE)
			{
				FiscalYearDataSet.FiscalYearsRow fiscalYearsRow = (FiscalYearDataSet.FiscalYearsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WFISCAL_YEAR,
					WFISCAL_PERIOD_START_DATE,
					WFISCAL_PERIOD_FINISH_DATE
				};
				fiscalYearsRow.ItemArray = itemArray;
				base.Rows.Add(fiscalYearsRow);
				return fiscalYearsRow;
			}

			// Token: 0x0600198E RID: 6542 RVA: 0x00052B44 File Offset: 0x00050D44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalYearDataSet.FiscalYearsRow FindByWFISCAL_YEAR(int WFISCAL_YEAR)
			{
				return (FiscalYearDataSet.FiscalYearsRow)base.Rows.Find(new object[]
				{
					WFISCAL_YEAR
				});
			}

			// Token: 0x0600198F RID: 6543 RVA: 0x00052B72 File Offset: 0x00050D72
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001990 RID: 6544 RVA: 0x00052B80 File Offset: 0x00050D80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				FiscalYearDataSet.FiscalYearsDataTable fiscalYearsDataTable = (FiscalYearDataSet.FiscalYearsDataTable)base.Clone();
				fiscalYearsDataTable.InitVars();
				return fiscalYearsDataTable;
			}

			// Token: 0x06001991 RID: 6545 RVA: 0x00052BA0 File Offset: 0x00050DA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new FiscalYearDataSet.FiscalYearsDataTable();
			}

			// Token: 0x06001992 RID: 6546 RVA: 0x00052BA8 File Offset: 0x00050DA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWFISCAL_YEAR = base.Columns["WFISCAL_YEAR"];
				this.columnWFISCAL_PERIOD_START_DATE = base.Columns["WFISCAL_PERIOD_START_DATE"];
				this.columnWFISCAL_PERIOD_FINISH_DATE = base.Columns["WFISCAL_PERIOD_FINISH_DATE"];
			}

			// Token: 0x06001993 RID: 6547 RVA: 0x00052BF8 File Offset: 0x00050DF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWFISCAL_YEAR = new DataColumn("WFISCAL_YEAR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_YEAR);
				this.columnWFISCAL_PERIOD_START_DATE = new DataColumn("WFISCAL_PERIOD_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_START_DATE);
				this.columnWFISCAL_PERIOD_FINISH_DATE = new DataColumn("WFISCAL_PERIOD_FINISH_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_FINISH_DATE);
				base.Constraints.Add(new UniqueConstraint("Constraint2", new DataColumn[]
				{
					this.columnWFISCAL_YEAR
				}, true));
				this.columnWFISCAL_YEAR.AllowDBNull = false;
				this.columnWFISCAL_YEAR.Unique = true;
			}

			// Token: 0x06001994 RID: 6548 RVA: 0x00052CCB File Offset: 0x00050ECB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalYearDataSet.FiscalYearsRow NewFiscalYearsRow()
			{
				return (FiscalYearDataSet.FiscalYearsRow)base.NewRow();
			}

			// Token: 0x06001995 RID: 6549 RVA: 0x00052CD8 File Offset: 0x00050ED8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new FiscalYearDataSet.FiscalYearsRow(builder);
			}

			// Token: 0x06001996 RID: 6550 RVA: 0x00052CE0 File Offset: 0x00050EE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(FiscalYearDataSet.FiscalYearsRow);
			}

			// Token: 0x06001997 RID: 6551 RVA: 0x00052CEC File Offset: 0x00050EEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.FiscalYearsRowChanged != null)
				{
					this.FiscalYearsRowChanged(this, new FiscalYearDataSet.FiscalYearsRowChangeEvent((FiscalYearDataSet.FiscalYearsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001998 RID: 6552 RVA: 0x00052D1F File Offset: 0x00050F1F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.FiscalYearsRowChanging != null)
				{
					this.FiscalYearsRowChanging(this, new FiscalYearDataSet.FiscalYearsRowChangeEvent((FiscalYearDataSet.FiscalYearsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001999 RID: 6553 RVA: 0x00052D52 File Offset: 0x00050F52
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.FiscalYearsRowDeleted != null)
				{
					this.FiscalYearsRowDeleted(this, new FiscalYearDataSet.FiscalYearsRowChangeEvent((FiscalYearDataSet.FiscalYearsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600199A RID: 6554 RVA: 0x00052D85 File Offset: 0x00050F85
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.FiscalYearsRowDeleting != null)
				{
					this.FiscalYearsRowDeleting(this, new FiscalYearDataSet.FiscalYearsRowChangeEvent((FiscalYearDataSet.FiscalYearsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600199B RID: 6555 RVA: 0x00052DB8 File Offset: 0x00050FB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveFiscalYearsRow(FiscalYearDataSet.FiscalYearsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600199C RID: 6556 RVA: 0x00052DC8 File Offset: 0x00050FC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				FiscalYearDataSet fiscalYearDataSet = new FiscalYearDataSet();
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
				xmlSchemaAttribute.FixedValue = fiscalYearDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "FiscalYearsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = fiscalYearDataSet.GetSchemaSerializable();
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

			// Token: 0x04000590 RID: 1424
			private DataColumn columnWFISCAL_YEAR;

			// Token: 0x04000591 RID: 1425
			private DataColumn columnWFISCAL_PERIOD_START_DATE;

			// Token: 0x04000592 RID: 1426
			private DataColumn columnWFISCAL_PERIOD_FINISH_DATE;
		}

		// Token: 0x0200015D RID: 349
		public class FiscalYearsRow : DataRow
		{
			// Token: 0x0600199D RID: 6557 RVA: 0x00052FC0 File Offset: 0x000511C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal FiscalYearsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableFiscalYears = (FiscalYearDataSet.FiscalYearsDataTable)base.Table;
			}

			// Token: 0x17000768 RID: 1896
			// (get) Token: 0x0600199E RID: 6558 RVA: 0x00052FDA File Offset: 0x000511DA
			// (set) Token: 0x0600199F RID: 6559 RVA: 0x00052FF2 File Offset: 0x000511F2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WFISCAL_YEAR
			{
				get
				{
					return (int)base[this.tableFiscalYears.WFISCAL_YEARColumn];
				}
				set
				{
					base[this.tableFiscalYears.WFISCAL_YEARColumn] = value;
				}
			}

			// Token: 0x17000769 RID: 1897
			// (get) Token: 0x060019A0 RID: 6560 RVA: 0x0005300C File Offset: 0x0005120C
			// (set) Token: 0x060019A1 RID: 6561 RVA: 0x00053050 File Offset: 0x00051250
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WFISCAL_PERIOD_START_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableFiscalYears.WFISCAL_PERIOD_START_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFISCAL_PERIOD_START_DATE' in table 'FiscalYears' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableFiscalYears.WFISCAL_PERIOD_START_DATEColumn] = value;
				}
			}

			// Token: 0x1700076A RID: 1898
			// (get) Token: 0x060019A2 RID: 6562 RVA: 0x0005306C File Offset: 0x0005126C
			// (set) Token: 0x060019A3 RID: 6563 RVA: 0x000530B0 File Offset: 0x000512B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WFISCAL_PERIOD_FINISH_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableFiscalYears.WFISCAL_PERIOD_FINISH_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFISCAL_PERIOD_FINISH_DATE' in table 'FiscalYears' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableFiscalYears.WFISCAL_PERIOD_FINISH_DATEColumn] = value;
				}
			}

			// Token: 0x060019A4 RID: 6564 RVA: 0x000530C9 File Offset: 0x000512C9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWFISCAL_PERIOD_START_DATENull()
			{
				return base.IsNull(this.tableFiscalYears.WFISCAL_PERIOD_START_DATEColumn);
			}

			// Token: 0x060019A5 RID: 6565 RVA: 0x000530DC File Offset: 0x000512DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFISCAL_PERIOD_START_DATENull()
			{
				base[this.tableFiscalYears.WFISCAL_PERIOD_START_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x060019A6 RID: 6566 RVA: 0x000530F4 File Offset: 0x000512F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWFISCAL_PERIOD_FINISH_DATENull()
			{
				return base.IsNull(this.tableFiscalYears.WFISCAL_PERIOD_FINISH_DATEColumn);
			}

			// Token: 0x060019A7 RID: 6567 RVA: 0x00053107 File Offset: 0x00051307
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFISCAL_PERIOD_FINISH_DATENull()
			{
				base[this.tableFiscalYears.WFISCAL_PERIOD_FINISH_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x04000597 RID: 1431
			private FiscalYearDataSet.FiscalYearsDataTable tableFiscalYears;
		}

		// Token: 0x0200015E RID: 350
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class FiscalYearsRowChangeEvent : EventArgs
		{
			// Token: 0x060019A8 RID: 6568 RVA: 0x0005311F File Offset: 0x0005131F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalYearsRowChangeEvent(FiscalYearDataSet.FiscalYearsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700076B RID: 1899
			// (get) Token: 0x060019A9 RID: 6569 RVA: 0x00053135 File Offset: 0x00051335
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalYearDataSet.FiscalYearsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700076C RID: 1900
			// (get) Token: 0x060019AA RID: 6570 RVA: 0x0005313D File Offset: 0x0005133D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04000598 RID: 1432
			private FiscalYearDataSet.FiscalYearsRow eventRow;

			// Token: 0x04000599 RID: 1433
			private DataRowAction eventAction;
		}
	}
}
