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
	// Token: 0x02000155 RID: 341
	[XmlRoot("FiscalPeriodDataSet")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[Serializable]
	public class FiscalPeriodDataSet : DataSet
	{
		// Token: 0x06001915 RID: 6421 RVA: 0x00051270 File Offset: 0x0004F470
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.FiscalPeriods, new string[]
			{
				"WFISCAL_PERIOD_NAME",
				"WFISCAL_PERIOD_UID",
				"WFISCAL_YEAR",
				"WFISCAL_PERIOD_FINISH_DATE",
				"WFISCAL_PERIOD_START_DATE",
				"WFISCAL_QUARTER",
				"WFISCAL_MONTH"
			});
		}

		// Token: 0x06001916 RID: 6422 RVA: 0x000512D0 File Offset: 0x0004F4D0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public FiscalPeriodDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06001917 RID: 6423 RVA: 0x00051324 File Offset: 0x0004F524
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected FiscalPeriodDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["FiscalPeriods"] != null)
				{
					base.Tables.Add(new FiscalPeriodDataSet.FiscalPeriodsDataTable(dataSet.Tables["FiscalPeriods"]));
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

		// Token: 0x17000749 RID: 1865
		// (get) Token: 0x06001918 RID: 6424 RVA: 0x00051481 File Offset: 0x0004F681
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public FiscalPeriodDataSet.FiscalPeriodsDataTable FiscalPeriods
		{
			get
			{
				return this.tableFiscalPeriods;
			}
		}

		// Token: 0x1700074A RID: 1866
		// (get) Token: 0x06001919 RID: 6425 RVA: 0x00051489 File Offset: 0x0004F689
		// (set) Token: 0x0600191A RID: 6426 RVA: 0x00051491 File Offset: 0x0004F691
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(true)]
		[DebuggerNonUserCode]
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

		// Token: 0x1700074B RID: 1867
		// (get) Token: 0x0600191B RID: 6427 RVA: 0x0005149A File Offset: 0x0004F69A
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

		// Token: 0x1700074C RID: 1868
		// (get) Token: 0x0600191C RID: 6428 RVA: 0x000514A2 File Offset: 0x0004F6A2
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x0600191D RID: 6429 RVA: 0x000514AA File Offset: 0x0004F6AA
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600191E RID: 6430 RVA: 0x000514C0 File Offset: 0x0004F6C0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			FiscalPeriodDataSet fiscalPeriodDataSet = (FiscalPeriodDataSet)base.Clone();
			fiscalPeriodDataSet.InitVars();
			fiscalPeriodDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return fiscalPeriodDataSet;
		}

		// Token: 0x0600191F RID: 6431 RVA: 0x000514EC File Offset: 0x0004F6EC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06001920 RID: 6432 RVA: 0x000514EF File Offset: 0x0004F6EF
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001921 RID: 6433 RVA: 0x000514F4 File Offset: 0x0004F6F4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["FiscalPeriods"] != null)
				{
					base.Tables.Add(new FiscalPeriodDataSet.FiscalPeriodsDataTable(dataSet.Tables["FiscalPeriods"]));
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

		// Token: 0x06001922 RID: 6434 RVA: 0x000515BC File Offset: 0x0004F7BC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001923 RID: 6435 RVA: 0x000515F0 File Offset: 0x0004F7F0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001924 RID: 6436 RVA: 0x000515F9 File Offset: 0x0004F7F9
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableFiscalPeriods = (FiscalPeriodDataSet.FiscalPeriodsDataTable)base.Tables["FiscalPeriods"];
			if (initTable && this.tableFiscalPeriods != null)
			{
				this.tableFiscalPeriods.InitVars();
			}
		}

		// Token: 0x06001925 RID: 6437 RVA: 0x0005162C File Offset: 0x0004F82C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "FiscalPeriodDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/FiscalPeriod/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableFiscalPeriods = new FiscalPeriodDataSet.FiscalPeriodsDataTable();
			base.Tables.Add(this.tableFiscalPeriods);
		}

		// Token: 0x06001926 RID: 6438 RVA: 0x00051684 File Offset: 0x0004F884
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeFiscalPeriods()
		{
			return false;
		}

		// Token: 0x06001927 RID: 6439 RVA: 0x00051687 File Offset: 0x0004F887
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001928 RID: 6440 RVA: 0x00051698 File Offset: 0x0004F898
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			FiscalPeriodDataSet fiscalPeriodDataSet = new FiscalPeriodDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = fiscalPeriodDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = fiscalPeriodDataSet.GetSchemaSerializable();
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

		// Token: 0x0400057E RID: 1406
		private FiscalPeriodDataSet.FiscalPeriodsDataTable tableFiscalPeriods;

		// Token: 0x0400057F RID: 1407
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000156 RID: 342
		// (Invoke) Token: 0x0600192A RID: 6442
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void FiscalPeriodsRowChangeEventHandler(object sender, FiscalPeriodDataSet.FiscalPeriodsRowChangeEvent e);

		// Token: 0x02000157 RID: 343
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class FiscalPeriodsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600192D RID: 6445 RVA: 0x000517E0 File Offset: 0x0004F9E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalPeriodsDataTable()
			{
				base.TableName = "FiscalPeriods";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600192E RID: 6446 RVA: 0x00051808 File Offset: 0x0004FA08
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal FiscalPeriodsDataTable(DataTable table)
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

			// Token: 0x0600192F RID: 6447 RVA: 0x000518B0 File Offset: 0x0004FAB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected FiscalPeriodsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700074D RID: 1869
			// (get) Token: 0x06001930 RID: 6448 RVA: 0x000518C0 File Offset: 0x0004FAC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WFISCAL_PERIOD_UIDColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_UID;
				}
			}

			// Token: 0x1700074E RID: 1870
			// (get) Token: 0x06001931 RID: 6449 RVA: 0x000518C8 File Offset: 0x0004FAC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_MONTHColumn
			{
				get
				{
					return this.columnWFISCAL_MONTH;
				}
			}

			// Token: 0x1700074F RID: 1871
			// (get) Token: 0x06001932 RID: 6450 RVA: 0x000518D0 File Offset: 0x0004FAD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_QUARTERColumn
			{
				get
				{
					return this.columnWFISCAL_QUARTER;
				}
			}

			// Token: 0x17000750 RID: 1872
			// (get) Token: 0x06001933 RID: 6451 RVA: 0x000518D8 File Offset: 0x0004FAD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_YEARColumn
			{
				get
				{
					return this.columnWFISCAL_YEAR;
				}
			}

			// Token: 0x17000751 RID: 1873
			// (get) Token: 0x06001934 RID: 6452 RVA: 0x000518E0 File Offset: 0x0004FAE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WFISCAL_PERIOD_START_DATEColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_START_DATE;
				}
			}

			// Token: 0x17000752 RID: 1874
			// (get) Token: 0x06001935 RID: 6453 RVA: 0x000518E8 File Offset: 0x0004FAE8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFISCAL_PERIOD_FINISH_DATEColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_FINISH_DATE;
				}
			}

			// Token: 0x17000753 RID: 1875
			// (get) Token: 0x06001936 RID: 6454 RVA: 0x000518F0 File Offset: 0x0004FAF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WFISCAL_PERIOD_NAMEColumn
			{
				get
				{
					return this.columnWFISCAL_PERIOD_NAME;
				}
			}

			// Token: 0x17000754 RID: 1876
			// (get) Token: 0x06001937 RID: 6455 RVA: 0x000518F8 File Offset: 0x0004FAF8
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

			// Token: 0x17000755 RID: 1877
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalPeriodDataSet.FiscalPeriodsRow this[int index]
			{
				get
				{
					return (FiscalPeriodDataSet.FiscalPeriodsRow)base.Rows[index];
				}
			}

			// Token: 0x1400011D RID: 285
			// (add) Token: 0x06001939 RID: 6457 RVA: 0x00051918 File Offset: 0x0004FB18
			// (remove) Token: 0x0600193A RID: 6458 RVA: 0x00051950 File Offset: 0x0004FB50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalPeriodDataSet.FiscalPeriodsRowChangeEventHandler FiscalPeriodsRowChanging;

			// Token: 0x1400011E RID: 286
			// (add) Token: 0x0600193B RID: 6459 RVA: 0x00051988 File Offset: 0x0004FB88
			// (remove) Token: 0x0600193C RID: 6460 RVA: 0x000519C0 File Offset: 0x0004FBC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalPeriodDataSet.FiscalPeriodsRowChangeEventHandler FiscalPeriodsRowChanged;

			// Token: 0x1400011F RID: 287
			// (add) Token: 0x0600193D RID: 6461 RVA: 0x000519F8 File Offset: 0x0004FBF8
			// (remove) Token: 0x0600193E RID: 6462 RVA: 0x00051A30 File Offset: 0x0004FC30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalPeriodDataSet.FiscalPeriodsRowChangeEventHandler FiscalPeriodsRowDeleting;

			// Token: 0x14000120 RID: 288
			// (add) Token: 0x0600193F RID: 6463 RVA: 0x00051A68 File Offset: 0x0004FC68
			// (remove) Token: 0x06001940 RID: 6464 RVA: 0x00051AA0 File Offset: 0x0004FCA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event FiscalPeriodDataSet.FiscalPeriodsRowChangeEventHandler FiscalPeriodsRowDeleted;

			// Token: 0x06001941 RID: 6465 RVA: 0x00051AD5 File Offset: 0x0004FCD5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddFiscalPeriodsRow(FiscalPeriodDataSet.FiscalPeriodsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001942 RID: 6466 RVA: 0x00051AE4 File Offset: 0x0004FCE4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalPeriodDataSet.FiscalPeriodsRow AddFiscalPeriodsRow(Guid WFISCAL_PERIOD_UID, int WFISCAL_MONTH, int WFISCAL_QUARTER, int WFISCAL_YEAR, DateTime WFISCAL_PERIOD_START_DATE, DateTime WFISCAL_PERIOD_FINISH_DATE, string WFISCAL_PERIOD_NAME)
			{
				FiscalPeriodDataSet.FiscalPeriodsRow fiscalPeriodsRow = (FiscalPeriodDataSet.FiscalPeriodsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WFISCAL_PERIOD_UID,
					WFISCAL_MONTH,
					WFISCAL_QUARTER,
					WFISCAL_YEAR,
					WFISCAL_PERIOD_START_DATE,
					WFISCAL_PERIOD_FINISH_DATE,
					WFISCAL_PERIOD_NAME
				};
				fiscalPeriodsRow.ItemArray = itemArray;
				base.Rows.Add(fiscalPeriodsRow);
				return fiscalPeriodsRow;
			}

			// Token: 0x06001943 RID: 6467 RVA: 0x00051B58 File Offset: 0x0004FD58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalPeriodDataSet.FiscalPeriodsRow FindByWFISCAL_PERIOD_UID(Guid WFISCAL_PERIOD_UID)
			{
				return (FiscalPeriodDataSet.FiscalPeriodsRow)base.Rows.Find(new object[]
				{
					WFISCAL_PERIOD_UID
				});
			}

			// Token: 0x06001944 RID: 6468 RVA: 0x00051B86 File Offset: 0x0004FD86
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001945 RID: 6469 RVA: 0x00051B94 File Offset: 0x0004FD94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				FiscalPeriodDataSet.FiscalPeriodsDataTable fiscalPeriodsDataTable = (FiscalPeriodDataSet.FiscalPeriodsDataTable)base.Clone();
				fiscalPeriodsDataTable.InitVars();
				return fiscalPeriodsDataTable;
			}

			// Token: 0x06001946 RID: 6470 RVA: 0x00051BB4 File Offset: 0x0004FDB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new FiscalPeriodDataSet.FiscalPeriodsDataTable();
			}

			// Token: 0x06001947 RID: 6471 RVA: 0x00051BBC File Offset: 0x0004FDBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWFISCAL_PERIOD_UID = base.Columns["WFISCAL_PERIOD_UID"];
				this.columnWFISCAL_MONTH = base.Columns["WFISCAL_MONTH"];
				this.columnWFISCAL_QUARTER = base.Columns["WFISCAL_QUARTER"];
				this.columnWFISCAL_YEAR = base.Columns["WFISCAL_YEAR"];
				this.columnWFISCAL_PERIOD_START_DATE = base.Columns["WFISCAL_PERIOD_START_DATE"];
				this.columnWFISCAL_PERIOD_FINISH_DATE = base.Columns["WFISCAL_PERIOD_FINISH_DATE"];
				this.columnWFISCAL_PERIOD_NAME = base.Columns["WFISCAL_PERIOD_NAME"];
			}

			// Token: 0x06001948 RID: 6472 RVA: 0x00051C64 File Offset: 0x0004FE64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWFISCAL_PERIOD_UID = new DataColumn("WFISCAL_PERIOD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_UID);
				this.columnWFISCAL_MONTH = new DataColumn("WFISCAL_MONTH", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_MONTH);
				this.columnWFISCAL_QUARTER = new DataColumn("WFISCAL_QUARTER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_QUARTER);
				this.columnWFISCAL_YEAR = new DataColumn("WFISCAL_YEAR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_YEAR);
				this.columnWFISCAL_PERIOD_START_DATE = new DataColumn("WFISCAL_PERIOD_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_START_DATE);
				this.columnWFISCAL_PERIOD_FINISH_DATE = new DataColumn("WFISCAL_PERIOD_FINISH_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_FINISH_DATE);
				this.columnWFISCAL_PERIOD_NAME = new DataColumn("WFISCAL_PERIOD_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWFISCAL_PERIOD_NAME);
				base.Constraints.Add(new UniqueConstraint("Constraint2", new DataColumn[]
				{
					this.columnWFISCAL_PERIOD_UID
				}, true));
				this.columnWFISCAL_PERIOD_UID.AllowDBNull = false;
				this.columnWFISCAL_PERIOD_UID.Unique = true;
				this.columnWFISCAL_MONTH.AllowDBNull = false;
				this.columnWFISCAL_QUARTER.AllowDBNull = false;
				this.columnWFISCAL_YEAR.AllowDBNull = false;
				this.columnWFISCAL_PERIOD_START_DATE.AllowDBNull = false;
				this.columnWFISCAL_PERIOD_FINISH_DATE.AllowDBNull = false;
				this.columnWFISCAL_PERIOD_NAME.AllowDBNull = false;
			}

			// Token: 0x06001949 RID: 6473 RVA: 0x00051E33 File Offset: 0x00050033
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalPeriodDataSet.FiscalPeriodsRow NewFiscalPeriodsRow()
			{
				return (FiscalPeriodDataSet.FiscalPeriodsRow)base.NewRow();
			}

			// Token: 0x0600194A RID: 6474 RVA: 0x00051E40 File Offset: 0x00050040
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new FiscalPeriodDataSet.FiscalPeriodsRow(builder);
			}

			// Token: 0x0600194B RID: 6475 RVA: 0x00051E48 File Offset: 0x00050048
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(FiscalPeriodDataSet.FiscalPeriodsRow);
			}

			// Token: 0x0600194C RID: 6476 RVA: 0x00051E54 File Offset: 0x00050054
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.FiscalPeriodsRowChanged != null)
				{
					this.FiscalPeriodsRowChanged(this, new FiscalPeriodDataSet.FiscalPeriodsRowChangeEvent((FiscalPeriodDataSet.FiscalPeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600194D RID: 6477 RVA: 0x00051E87 File Offset: 0x00050087
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.FiscalPeriodsRowChanging != null)
				{
					this.FiscalPeriodsRowChanging(this, new FiscalPeriodDataSet.FiscalPeriodsRowChangeEvent((FiscalPeriodDataSet.FiscalPeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600194E RID: 6478 RVA: 0x00051EBA File Offset: 0x000500BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.FiscalPeriodsRowDeleted != null)
				{
					this.FiscalPeriodsRowDeleted(this, new FiscalPeriodDataSet.FiscalPeriodsRowChangeEvent((FiscalPeriodDataSet.FiscalPeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600194F RID: 6479 RVA: 0x00051EED File Offset: 0x000500ED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.FiscalPeriodsRowDeleting != null)
				{
					this.FiscalPeriodsRowDeleting(this, new FiscalPeriodDataSet.FiscalPeriodsRowChangeEvent((FiscalPeriodDataSet.FiscalPeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001950 RID: 6480 RVA: 0x00051F20 File Offset: 0x00050120
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveFiscalPeriodsRow(FiscalPeriodDataSet.FiscalPeriodsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001951 RID: 6481 RVA: 0x00051F30 File Offset: 0x00050130
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				FiscalPeriodDataSet fiscalPeriodDataSet = new FiscalPeriodDataSet();
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
				xmlSchemaAttribute.FixedValue = fiscalPeriodDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "FiscalPeriodsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = fiscalPeriodDataSet.GetSchemaSerializable();
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

			// Token: 0x04000580 RID: 1408
			private DataColumn columnWFISCAL_PERIOD_UID;

			// Token: 0x04000581 RID: 1409
			private DataColumn columnWFISCAL_MONTH;

			// Token: 0x04000582 RID: 1410
			private DataColumn columnWFISCAL_QUARTER;

			// Token: 0x04000583 RID: 1411
			private DataColumn columnWFISCAL_YEAR;

			// Token: 0x04000584 RID: 1412
			private DataColumn columnWFISCAL_PERIOD_START_DATE;

			// Token: 0x04000585 RID: 1413
			private DataColumn columnWFISCAL_PERIOD_FINISH_DATE;

			// Token: 0x04000586 RID: 1414
			private DataColumn columnWFISCAL_PERIOD_NAME;
		}

		// Token: 0x02000158 RID: 344
		public class FiscalPeriodsRow : DataRow
		{
			// Token: 0x06001952 RID: 6482 RVA: 0x00052128 File Offset: 0x00050328
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal FiscalPeriodsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableFiscalPeriods = (FiscalPeriodDataSet.FiscalPeriodsDataTable)base.Table;
			}

			// Token: 0x17000756 RID: 1878
			// (get) Token: 0x06001953 RID: 6483 RVA: 0x00052142 File Offset: 0x00050342
			// (set) Token: 0x06001954 RID: 6484 RVA: 0x0005215A File Offset: 0x0005035A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WFISCAL_PERIOD_UID
			{
				get
				{
					return (Guid)base[this.tableFiscalPeriods.WFISCAL_PERIOD_UIDColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_PERIOD_UIDColumn] = value;
				}
			}

			// Token: 0x17000757 RID: 1879
			// (get) Token: 0x06001955 RID: 6485 RVA: 0x00052173 File Offset: 0x00050373
			// (set) Token: 0x06001956 RID: 6486 RVA: 0x0005218B File Offset: 0x0005038B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WFISCAL_MONTH
			{
				get
				{
					return (int)base[this.tableFiscalPeriods.WFISCAL_MONTHColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_MONTHColumn] = value;
				}
			}

			// Token: 0x17000758 RID: 1880
			// (get) Token: 0x06001957 RID: 6487 RVA: 0x000521A4 File Offset: 0x000503A4
			// (set) Token: 0x06001958 RID: 6488 RVA: 0x000521BC File Offset: 0x000503BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WFISCAL_QUARTER
			{
				get
				{
					return (int)base[this.tableFiscalPeriods.WFISCAL_QUARTERColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_QUARTERColumn] = value;
				}
			}

			// Token: 0x17000759 RID: 1881
			// (get) Token: 0x06001959 RID: 6489 RVA: 0x000521D5 File Offset: 0x000503D5
			// (set) Token: 0x0600195A RID: 6490 RVA: 0x000521ED File Offset: 0x000503ED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WFISCAL_YEAR
			{
				get
				{
					return (int)base[this.tableFiscalPeriods.WFISCAL_YEARColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_YEARColumn] = value;
				}
			}

			// Token: 0x1700075A RID: 1882
			// (get) Token: 0x0600195B RID: 6491 RVA: 0x00052206 File Offset: 0x00050406
			// (set) Token: 0x0600195C RID: 6492 RVA: 0x0005221E File Offset: 0x0005041E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime WFISCAL_PERIOD_START_DATE
			{
				get
				{
					return (DateTime)base[this.tableFiscalPeriods.WFISCAL_PERIOD_START_DATEColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_PERIOD_START_DATEColumn] = value;
				}
			}

			// Token: 0x1700075B RID: 1883
			// (get) Token: 0x0600195D RID: 6493 RVA: 0x00052237 File Offset: 0x00050437
			// (set) Token: 0x0600195E RID: 6494 RVA: 0x0005224F File Offset: 0x0005044F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WFISCAL_PERIOD_FINISH_DATE
			{
				get
				{
					return (DateTime)base[this.tableFiscalPeriods.WFISCAL_PERIOD_FINISH_DATEColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_PERIOD_FINISH_DATEColumn] = value;
				}
			}

			// Token: 0x1700075C RID: 1884
			// (get) Token: 0x0600195F RID: 6495 RVA: 0x00052268 File Offset: 0x00050468
			// (set) Token: 0x06001960 RID: 6496 RVA: 0x00052280 File Offset: 0x00050480
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WFISCAL_PERIOD_NAME
			{
				get
				{
					return (string)base[this.tableFiscalPeriods.WFISCAL_PERIOD_NAMEColumn];
				}
				set
				{
					base[this.tableFiscalPeriods.WFISCAL_PERIOD_NAMEColumn] = value;
				}
			}

			// Token: 0x0400058B RID: 1419
			private FiscalPeriodDataSet.FiscalPeriodsDataTable tableFiscalPeriods;
		}

		// Token: 0x02000159 RID: 345
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class FiscalPeriodsRowChangeEvent : EventArgs
		{
			// Token: 0x06001961 RID: 6497 RVA: 0x00052294 File Offset: 0x00050494
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public FiscalPeriodsRowChangeEvent(FiscalPeriodDataSet.FiscalPeriodsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700075D RID: 1885
			// (get) Token: 0x06001962 RID: 6498 RVA: 0x000522AA File Offset: 0x000504AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public FiscalPeriodDataSet.FiscalPeriodsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700075E RID: 1886
			// (get) Token: 0x06001963 RID: 6499 RVA: 0x000522B2 File Offset: 0x000504B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400058C RID: 1420
			private FiscalPeriodDataSet.FiscalPeriodsRow eventRow;

			// Token: 0x0400058D RID: 1421
			private DataRowAction eventAction;
		}
	}
}
