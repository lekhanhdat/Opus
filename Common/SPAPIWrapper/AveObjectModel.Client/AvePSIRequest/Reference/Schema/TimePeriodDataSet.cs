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
	// Token: 0x02000721 RID: 1825
	[ToolboxItem(true)]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[XmlRoot("TimePeriodDataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class TimePeriodDataSet : DataSet
	{
		// Token: 0x0600B090 RID: 45200 RVA: 0x00226858 File Offset: 0x00224A58
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.TimePeriods, new string[]
			{
				"WPRD_UID",
				"WPRD_FINISH_DATE",
				"WPRD_START_DATE",
				"WPRD_NAME",
				"WPRD_STATE_ENUM"
			});
		}

		// Token: 0x0600B091 RID: 45201 RVA: 0x002268A8 File Offset: 0x00224AA8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public TimePeriodDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600B092 RID: 45202 RVA: 0x002268FC File Offset: 0x00224AFC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected TimePeriodDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["TimePeriods"] != null)
				{
					base.Tables.Add(new TimePeriodDataSet.TimePeriodsDataTable(dataSet.Tables["TimePeriods"]));
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

		// Token: 0x170035D7 RID: 13783
		// (get) Token: 0x0600B093 RID: 45203 RVA: 0x00226A59 File Offset: 0x00224C59
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public TimePeriodDataSet.TimePeriodsDataTable TimePeriods
		{
			get
			{
				return this.tableTimePeriods;
			}
		}

		// Token: 0x170035D8 RID: 13784
		// (get) Token: 0x0600B094 RID: 45204 RVA: 0x00226A61 File Offset: 0x00224C61
		// (set) Token: 0x0600B095 RID: 45205 RVA: 0x00226A69 File Offset: 0x00224C69
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

		// Token: 0x170035D9 RID: 13785
		// (get) Token: 0x0600B096 RID: 45206 RVA: 0x00226A72 File Offset: 0x00224C72
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x170035DA RID: 13786
		// (get) Token: 0x0600B097 RID: 45207 RVA: 0x00226A7A File Offset: 0x00224C7A
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

		// Token: 0x0600B098 RID: 45208 RVA: 0x00226A82 File Offset: 0x00224C82
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600B099 RID: 45209 RVA: 0x00226A98 File Offset: 0x00224C98
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			TimePeriodDataSet timePeriodDataSet = (TimePeriodDataSet)base.Clone();
			timePeriodDataSet.InitVars();
			timePeriodDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return timePeriodDataSet;
		}

		// Token: 0x0600B09A RID: 45210 RVA: 0x00226AC4 File Offset: 0x00224CC4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600B09B RID: 45211 RVA: 0x00226AC7 File Offset: 0x00224CC7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600B09C RID: 45212 RVA: 0x00226ACC File Offset: 0x00224CCC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["TimePeriods"] != null)
				{
					base.Tables.Add(new TimePeriodDataSet.TimePeriodsDataTable(dataSet.Tables["TimePeriods"]));
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

		// Token: 0x0600B09D RID: 45213 RVA: 0x00226B94 File Offset: 0x00224D94
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600B09E RID: 45214 RVA: 0x00226BC8 File Offset: 0x00224DC8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600B09F RID: 45215 RVA: 0x00226BD1 File Offset: 0x00224DD1
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableTimePeriods = (TimePeriodDataSet.TimePeriodsDataTable)base.Tables["TimePeriods"];
			if (initTable && this.tableTimePeriods != null)
			{
				this.tableTimePeriods.InitVars();
			}
		}

		// Token: 0x0600B0A0 RID: 45216 RVA: 0x00226C04 File Offset: 0x00224E04
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "TimePeriodDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/TimePeriodDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableTimePeriods = new TimePeriodDataSet.TimePeriodsDataTable();
			base.Tables.Add(this.tableTimePeriods);
		}

		// Token: 0x0600B0A1 RID: 45217 RVA: 0x00226C5C File Offset: 0x00224E5C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeTimePeriods()
		{
			return false;
		}

		// Token: 0x0600B0A2 RID: 45218 RVA: 0x00226C5F File Offset: 0x00224E5F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600B0A3 RID: 45219 RVA: 0x00226C70 File Offset: 0x00224E70
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			TimePeriodDataSet timePeriodDataSet = new TimePeriodDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = timePeriodDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = timePeriodDataSet.GetSchemaSerializable();
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

		// Token: 0x0400238F RID: 9103
		private TimePeriodDataSet.TimePeriodsDataTable tableTimePeriods;

		// Token: 0x04002390 RID: 9104
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000722 RID: 1826
		// (Invoke) Token: 0x0600B0A5 RID: 45221
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void TimePeriodsRowChangeEventHandler(object sender, TimePeriodDataSet.TimePeriodsRowChangeEvent e);

		// Token: 0x02000723 RID: 1827
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class TimePeriodsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B0A8 RID: 45224 RVA: 0x00226DB8 File Offset: 0x00224FB8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimePeriodsDataTable()
			{
				base.TableName = "TimePeriods";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B0A9 RID: 45225 RVA: 0x00226DE0 File Offset: 0x00224FE0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal TimePeriodsDataTable(DataTable table)
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

			// Token: 0x0600B0AA RID: 45226 RVA: 0x00226E88 File Offset: 0x00225088
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected TimePeriodsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170035DB RID: 13787
			// (get) Token: 0x0600B0AB RID: 45227 RVA: 0x00226E98 File Offset: 0x00225098
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WPRD_UIDColumn
			{
				get
				{
					return this.columnWPRD_UID;
				}
			}

			// Token: 0x170035DC RID: 13788
			// (get) Token: 0x0600B0AC RID: 45228 RVA: 0x00226EA0 File Offset: 0x002250A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WPRD_START_DATEColumn
			{
				get
				{
					return this.columnWPRD_START_DATE;
				}
			}

			// Token: 0x170035DD RID: 13789
			// (get) Token: 0x0600B0AD RID: 45229 RVA: 0x00226EA8 File Offset: 0x002250A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WPRD_FINISH_DATEColumn
			{
				get
				{
					return this.columnWPRD_FINISH_DATE;
				}
			}

			// Token: 0x170035DE RID: 13790
			// (get) Token: 0x0600B0AE RID: 45230 RVA: 0x00226EB0 File Offset: 0x002250B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WPRD_NAMEColumn
			{
				get
				{
					return this.columnWPRD_NAME;
				}
			}

			// Token: 0x170035DF RID: 13791
			// (get) Token: 0x0600B0AF RID: 45231 RVA: 0x00226EB8 File Offset: 0x002250B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WPRD_STATE_ENUMColumn
			{
				get
				{
					return this.columnWPRD_STATE_ENUM;
				}
			}

			// Token: 0x170035E0 RID: 13792
			// (get) Token: 0x0600B0B0 RID: 45232 RVA: 0x00226EC0 File Offset: 0x002250C0
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

			// Token: 0x170035E1 RID: 13793
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimePeriodDataSet.TimePeriodsRow this[int index]
			{
				get
				{
					return (TimePeriodDataSet.TimePeriodsRow)base.Rows[index];
				}
			}

			// Token: 0x1400063D RID: 1597
			// (add) Token: 0x0600B0B2 RID: 45234 RVA: 0x00226EE0 File Offset: 0x002250E0
			// (remove) Token: 0x0600B0B3 RID: 45235 RVA: 0x00226F18 File Offset: 0x00225118
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimePeriodDataSet.TimePeriodsRowChangeEventHandler TimePeriodsRowChanging;

			// Token: 0x1400063E RID: 1598
			// (add) Token: 0x0600B0B4 RID: 45236 RVA: 0x00226F50 File Offset: 0x00225150
			// (remove) Token: 0x0600B0B5 RID: 45237 RVA: 0x00226F88 File Offset: 0x00225188
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimePeriodDataSet.TimePeriodsRowChangeEventHandler TimePeriodsRowChanged;

			// Token: 0x1400063F RID: 1599
			// (add) Token: 0x0600B0B6 RID: 45238 RVA: 0x00226FC0 File Offset: 0x002251C0
			// (remove) Token: 0x0600B0B7 RID: 45239 RVA: 0x00226FF8 File Offset: 0x002251F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimePeriodDataSet.TimePeriodsRowChangeEventHandler TimePeriodsRowDeleting;

			// Token: 0x14000640 RID: 1600
			// (add) Token: 0x0600B0B8 RID: 45240 RVA: 0x00227030 File Offset: 0x00225230
			// (remove) Token: 0x0600B0B9 RID: 45241 RVA: 0x00227068 File Offset: 0x00225268
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimePeriodDataSet.TimePeriodsRowChangeEventHandler TimePeriodsRowDeleted;

			// Token: 0x0600B0BA RID: 45242 RVA: 0x0022709D File Offset: 0x0022529D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddTimePeriodsRow(TimePeriodDataSet.TimePeriodsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B0BB RID: 45243 RVA: 0x002270AC File Offset: 0x002252AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimePeriodDataSet.TimePeriodsRow AddTimePeriodsRow(Guid WPRD_UID, DateTime WPRD_START_DATE, DateTime WPRD_FINISH_DATE, string WPRD_NAME, byte WPRD_STATE_ENUM)
			{
				TimePeriodDataSet.TimePeriodsRow timePeriodsRow = (TimePeriodDataSet.TimePeriodsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WPRD_UID,
					WPRD_START_DATE,
					WPRD_FINISH_DATE,
					WPRD_NAME,
					WPRD_STATE_ENUM
				};
				timePeriodsRow.ItemArray = itemArray;
				base.Rows.Add(timePeriodsRow);
				return timePeriodsRow;
			}

			// Token: 0x0600B0BC RID: 45244 RVA: 0x0022710C File Offset: 0x0022530C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimePeriodDataSet.TimePeriodsRow FindByWPRD_UID(Guid WPRD_UID)
			{
				return (TimePeriodDataSet.TimePeriodsRow)base.Rows.Find(new object[]
				{
					WPRD_UID
				});
			}

			// Token: 0x0600B0BD RID: 45245 RVA: 0x0022713A File Offset: 0x0022533A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B0BE RID: 45246 RVA: 0x00227148 File Offset: 0x00225348
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				TimePeriodDataSet.TimePeriodsDataTable timePeriodsDataTable = (TimePeriodDataSet.TimePeriodsDataTable)base.Clone();
				timePeriodsDataTable.InitVars();
				return timePeriodsDataTable;
			}

			// Token: 0x0600B0BF RID: 45247 RVA: 0x00227168 File Offset: 0x00225368
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new TimePeriodDataSet.TimePeriodsDataTable();
			}

			// Token: 0x0600B0C0 RID: 45248 RVA: 0x00227170 File Offset: 0x00225370
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWPRD_UID = base.Columns["WPRD_UID"];
				this.columnWPRD_START_DATE = base.Columns["WPRD_START_DATE"];
				this.columnWPRD_FINISH_DATE = base.Columns["WPRD_FINISH_DATE"];
				this.columnWPRD_NAME = base.Columns["WPRD_NAME"];
				this.columnWPRD_STATE_ENUM = base.Columns["WPRD_STATE_ENUM"];
			}

			// Token: 0x0600B0C1 RID: 45249 RVA: 0x002271EC File Offset: 0x002253EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWPRD_UID = new DataColumn("WPRD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWPRD_UID);
				this.columnWPRD_START_DATE = new DataColumn("WPRD_START_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWPRD_START_DATE);
				this.columnWPRD_FINISH_DATE = new DataColumn("WPRD_FINISH_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWPRD_FINISH_DATE);
				this.columnWPRD_NAME = new DataColumn("WPRD_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWPRD_NAME);
				this.columnWPRD_STATE_ENUM = new DataColumn("WPRD_STATE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWPRD_STATE_ENUM);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWPRD_UID
				}, true));
				this.columnWPRD_UID.AllowDBNull = false;
				this.columnWPRD_UID.Unique = true;
			}

			// Token: 0x0600B0C2 RID: 45250 RVA: 0x00227319 File Offset: 0x00225519
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimePeriodDataSet.TimePeriodsRow NewTimePeriodsRow()
			{
				return (TimePeriodDataSet.TimePeriodsRow)base.NewRow();
			}

			// Token: 0x0600B0C3 RID: 45251 RVA: 0x00227326 File Offset: 0x00225526
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new TimePeriodDataSet.TimePeriodsRow(builder);
			}

			// Token: 0x0600B0C4 RID: 45252 RVA: 0x0022732E File Offset: 0x0022552E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(TimePeriodDataSet.TimePeriodsRow);
			}

			// Token: 0x0600B0C5 RID: 45253 RVA: 0x0022733A File Offset: 0x0022553A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.TimePeriodsRowChanged != null)
				{
					this.TimePeriodsRowChanged(this, new TimePeriodDataSet.TimePeriodsRowChangeEvent((TimePeriodDataSet.TimePeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B0C6 RID: 45254 RVA: 0x0022736D File Offset: 0x0022556D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.TimePeriodsRowChanging != null)
				{
					this.TimePeriodsRowChanging(this, new TimePeriodDataSet.TimePeriodsRowChangeEvent((TimePeriodDataSet.TimePeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B0C7 RID: 45255 RVA: 0x002273A0 File Offset: 0x002255A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.TimePeriodsRowDeleted != null)
				{
					this.TimePeriodsRowDeleted(this, new TimePeriodDataSet.TimePeriodsRowChangeEvent((TimePeriodDataSet.TimePeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B0C8 RID: 45256 RVA: 0x002273D3 File Offset: 0x002255D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.TimePeriodsRowDeleting != null)
				{
					this.TimePeriodsRowDeleting(this, new TimePeriodDataSet.TimePeriodsRowChangeEvent((TimePeriodDataSet.TimePeriodsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B0C9 RID: 45257 RVA: 0x00227406 File Offset: 0x00225606
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveTimePeriodsRow(TimePeriodDataSet.TimePeriodsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B0CA RID: 45258 RVA: 0x00227414 File Offset: 0x00225614
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				TimePeriodDataSet timePeriodDataSet = new TimePeriodDataSet();
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
				xmlSchemaAttribute.FixedValue = timePeriodDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "TimePeriodsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = timePeriodDataSet.GetSchemaSerializable();
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

			// Token: 0x04002391 RID: 9105
			private DataColumn columnWPRD_UID;

			// Token: 0x04002392 RID: 9106
			private DataColumn columnWPRD_START_DATE;

			// Token: 0x04002393 RID: 9107
			private DataColumn columnWPRD_FINISH_DATE;

			// Token: 0x04002394 RID: 9108
			private DataColumn columnWPRD_NAME;

			// Token: 0x04002395 RID: 9109
			private DataColumn columnWPRD_STATE_ENUM;
		}

		// Token: 0x02000724 RID: 1828
		public class TimePeriodsRow : DataRow
		{
			// Token: 0x0600B0CB RID: 45259 RVA: 0x0022760C File Offset: 0x0022580C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal TimePeriodsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableTimePeriods = (TimePeriodDataSet.TimePeriodsDataTable)base.Table;
			}

			// Token: 0x170035E2 RID: 13794
			// (get) Token: 0x0600B0CC RID: 45260 RVA: 0x00227626 File Offset: 0x00225826
			// (set) Token: 0x0600B0CD RID: 45261 RVA: 0x0022763E File Offset: 0x0022583E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WPRD_UID
			{
				get
				{
					return (Guid)base[this.tableTimePeriods.WPRD_UIDColumn];
				}
				set
				{
					base[this.tableTimePeriods.WPRD_UIDColumn] = value;
				}
			}

			// Token: 0x170035E3 RID: 13795
			// (get) Token: 0x0600B0CE RID: 45262 RVA: 0x00227658 File Offset: 0x00225858
			// (set) Token: 0x0600B0CF RID: 45263 RVA: 0x0022769C File Offset: 0x0022589C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WPRD_START_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableTimePeriods.WPRD_START_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WPRD_START_DATE' in table 'TimePeriods' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimePeriods.WPRD_START_DATEColumn] = value;
				}
			}

			// Token: 0x170035E4 RID: 13796
			// (get) Token: 0x0600B0D0 RID: 45264 RVA: 0x002276B8 File Offset: 0x002258B8
			// (set) Token: 0x0600B0D1 RID: 45265 RVA: 0x002276FC File Offset: 0x002258FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime WPRD_FINISH_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableTimePeriods.WPRD_FINISH_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WPRD_FINISH_DATE' in table 'TimePeriods' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimePeriods.WPRD_FINISH_DATEColumn] = value;
				}
			}

			// Token: 0x170035E5 RID: 13797
			// (get) Token: 0x0600B0D2 RID: 45266 RVA: 0x00227718 File Offset: 0x00225918
			// (set) Token: 0x0600B0D3 RID: 45267 RVA: 0x0022775C File Offset: 0x0022595C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WPRD_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableTimePeriods.WPRD_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WPRD_NAME' in table 'TimePeriods' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimePeriods.WPRD_NAMEColumn] = value;
				}
			}

			// Token: 0x170035E6 RID: 13798
			// (get) Token: 0x0600B0D4 RID: 45268 RVA: 0x00227770 File Offset: 0x00225970
			// (set) Token: 0x0600B0D5 RID: 45269 RVA: 0x002277B4 File Offset: 0x002259B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WPRD_STATE_ENUM
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableTimePeriods.WPRD_STATE_ENUMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WPRD_STATE_ENUM' in table 'TimePeriods' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimePeriods.WPRD_STATE_ENUMColumn] = value;
				}
			}

			// Token: 0x0600B0D6 RID: 45270 RVA: 0x002277CD File Offset: 0x002259CD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWPRD_START_DATENull()
			{
				return base.IsNull(this.tableTimePeriods.WPRD_START_DATEColumn);
			}

			// Token: 0x0600B0D7 RID: 45271 RVA: 0x002277E0 File Offset: 0x002259E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWPRD_START_DATENull()
			{
				base[this.tableTimePeriods.WPRD_START_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B0D8 RID: 45272 RVA: 0x002277F8 File Offset: 0x002259F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWPRD_FINISH_DATENull()
			{
				return base.IsNull(this.tableTimePeriods.WPRD_FINISH_DATEColumn);
			}

			// Token: 0x0600B0D9 RID: 45273 RVA: 0x0022780B File Offset: 0x00225A0B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWPRD_FINISH_DATENull()
			{
				base[this.tableTimePeriods.WPRD_FINISH_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B0DA RID: 45274 RVA: 0x00227823 File Offset: 0x00225A23
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWPRD_NAMENull()
			{
				return base.IsNull(this.tableTimePeriods.WPRD_NAMEColumn);
			}

			// Token: 0x0600B0DB RID: 45275 RVA: 0x00227836 File Offset: 0x00225A36
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWPRD_NAMENull()
			{
				base[this.tableTimePeriods.WPRD_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B0DC RID: 45276 RVA: 0x0022784E File Offset: 0x00225A4E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWPRD_STATE_ENUMNull()
			{
				return base.IsNull(this.tableTimePeriods.WPRD_STATE_ENUMColumn);
			}

			// Token: 0x0600B0DD RID: 45277 RVA: 0x00227861 File Offset: 0x00225A61
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWPRD_STATE_ENUMNull()
			{
				base[this.tableTimePeriods.WPRD_STATE_ENUMColumn] = Convert.DBNull;
			}

			// Token: 0x0400239A RID: 9114
			private TimePeriodDataSet.TimePeriodsDataTable tableTimePeriods;
		}

		// Token: 0x02000725 RID: 1829
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class TimePeriodsRowChangeEvent : EventArgs
		{
			// Token: 0x0600B0DE RID: 45278 RVA: 0x00227879 File Offset: 0x00225A79
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimePeriodsRowChangeEvent(TimePeriodDataSet.TimePeriodsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170035E7 RID: 13799
			// (get) Token: 0x0600B0DF RID: 45279 RVA: 0x0022788F File Offset: 0x00225A8F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimePeriodDataSet.TimePeriodsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170035E8 RID: 13800
			// (get) Token: 0x0600B0E0 RID: 45280 RVA: 0x00227897 File Offset: 0x00225A97
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400239B RID: 9115
			private TimePeriodDataSet.TimePeriodsRow eventRow;

			// Token: 0x0400239C RID: 9116
			private DataRowAction eventAction;
		}
	}
}
