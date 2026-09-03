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
	// Token: 0x02000085 RID: 133
	[XmlRoot("CalendarDataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[DesignerCategory("code")]
	[Serializable]
	public class CalendarDataSet : DataSet
	{
		// Token: 0x06000974 RID: 2420 RVA: 0x0001FBDC File Offset: 0x0001DDDC
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Calendars, new string[]
			{
				"CAL_IS_STANDARD_CAL",
				"MOD_DATE",
				"CREATED_DATE",
				"CAL_CHECKOUTDATE",
				"CAL_CHECKOUTBY",
				"CAL_UID",
				"CAL_NAME",
				"CalendarUniqueIdToDuplicate"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.CalendarExceptions, new string[]
			{
				"Shift1Start",
				"RecurrenceMonth",
				"RecurrenceFrequency",
				"RecurrenceType",
				"Start",
				"Shift4Start",
				"Shift3Finish",
				"RecurrencePosition",
				"RecurrenceMonthDay",
				"Shift1Finish",
				"RecurrenceDays",
				"Finish",
				"Shift2Start",
				"Shift3Start",
				"CAL_UID",
				"Shift5Start",
				"Name",
				"Shift4Finish",
				"Shift2Finish",
				"Shift5Finish"
			});
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0001FD04 File Offset: 0x0001DF04
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public CalendarDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0001FD58 File Offset: 0x0001DF58
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected CalendarDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Calendars"] != null)
				{
					base.Tables.Add(new CalendarDataSet.CalendarsDataTable(dataSet.Tables["Calendars"]));
				}
				if (dataSet.Tables["CalendarExceptions"] != null)
				{
					base.Tables.Add(new CalendarDataSet.CalendarExceptionsDataTable(dataSet.Tables["CalendarExceptions"]));
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

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000977 RID: 2423 RVA: 0x0001FEE7 File Offset: 0x0001E0E7
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CalendarDataSet.CalendarsDataTable Calendars
		{
			get
			{
				return this.tableCalendars;
			}
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000978 RID: 2424 RVA: 0x0001FEEF File Offset: 0x0001E0EF
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public CalendarDataSet.CalendarExceptionsDataTable CalendarExceptions
		{
			get
			{
				return this.tableCalendarExceptions;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000979 RID: 2425 RVA: 0x0001FEF7 File Offset: 0x0001E0F7
		// (set) Token: 0x0600097A RID: 2426 RVA: 0x0001FEFF File Offset: 0x0001E0FF
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x0600097B RID: 2427 RVA: 0x0001FF08 File Offset: 0x0001E108
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

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x0600097C RID: 2428 RVA: 0x0001FF10 File Offset: 0x0001E110
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

		// Token: 0x0600097D RID: 2429 RVA: 0x0001FF18 File Offset: 0x0001E118
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x0001FF2C File Offset: 0x0001E12C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			CalendarDataSet calendarDataSet = (CalendarDataSet)base.Clone();
			calendarDataSet.InitVars();
			calendarDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return calendarDataSet;
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x0001FF58 File Offset: 0x0001E158
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x0001FF5B File Offset: 0x0001E15B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0001FF60 File Offset: 0x0001E160
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Calendars"] != null)
				{
					base.Tables.Add(new CalendarDataSet.CalendarsDataTable(dataSet.Tables["Calendars"]));
				}
				if (dataSet.Tables["CalendarExceptions"] != null)
				{
					base.Tables.Add(new CalendarDataSet.CalendarExceptionsDataTable(dataSet.Tables["CalendarExceptions"]));
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

		// Token: 0x06000982 RID: 2434 RVA: 0x00020058 File Offset: 0x0001E258
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x0002008C File Offset: 0x0001E28C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00020098 File Offset: 0x0001E298
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableCalendars = (CalendarDataSet.CalendarsDataTable)base.Tables["Calendars"];
			if (initTable && this.tableCalendars != null)
			{
				this.tableCalendars.InitVars();
			}
			this.tableCalendarExceptions = (CalendarDataSet.CalendarExceptionsDataTable)base.Tables["CalendarExceptions"];
			if (initTable && this.tableCalendarExceptions != null)
			{
				this.tableCalendarExceptions.InitVars();
			}
			this.relationCalendarCalendarExceptions = this.Relations["CalendarCalendarExceptions"];
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x00020120 File Offset: 0x0001E320
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "CalendarDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/CalendarExceptionDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableCalendars = new CalendarDataSet.CalendarsDataTable();
			base.Tables.Add(this.tableCalendars);
			this.tableCalendarExceptions = new CalendarDataSet.CalendarExceptionsDataTable();
			base.Tables.Add(this.tableCalendarExceptions);
			ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("CalendarCalendarExceptions", new DataColumn[]
			{
				this.tableCalendars.CAL_UIDColumn
			}, new DataColumn[]
			{
				this.tableCalendarExceptions.CAL_UIDColumn
			});
			this.tableCalendarExceptions.Constraints.Add(foreignKeyConstraint);
			foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
			foreignKeyConstraint.DeleteRule = Rule.Cascade;
			foreignKeyConstraint.UpdateRule = Rule.Cascade;
			this.relationCalendarCalendarExceptions = new DataRelation("CalendarCalendarExceptions", new DataColumn[]
			{
				this.tableCalendars.CAL_UIDColumn
			}, new DataColumn[]
			{
				this.tableCalendarExceptions.CAL_UIDColumn
			}, false);
			this.Relations.Add(this.relationCalendarCalendarExceptions);
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00020242 File Offset: 0x0001E442
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeCalendars()
		{
			return false;
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x00020245 File Offset: 0x0001E445
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeCalendarExceptions()
		{
			return false;
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00020248 File Offset: 0x0001E448
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x0002025C File Offset: 0x0001E45C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			CalendarDataSet calendarDataSet = new CalendarDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = calendarDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = calendarDataSet.GetSchemaSerializable();
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

		// Token: 0x04000250 RID: 592
		private CalendarDataSet.CalendarsDataTable tableCalendars;

		// Token: 0x04000251 RID: 593
		private CalendarDataSet.CalendarExceptionsDataTable tableCalendarExceptions;

		// Token: 0x04000252 RID: 594
		private DataRelation relationCalendarCalendarExceptions;

		// Token: 0x04000253 RID: 595
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000086 RID: 134
		// (Invoke) Token: 0x0600098B RID: 2443
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CalendarsRowChangeEventHandler(object sender, CalendarDataSet.CalendarsRowChangeEvent e);

		// Token: 0x02000087 RID: 135
		// (Invoke) Token: 0x0600098F RID: 2447
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CalendarExceptionsRowChangeEventHandler(object sender, CalendarDataSet.CalendarExceptionsRowChangeEvent e);

		// Token: 0x02000088 RID: 136
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CalendarsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06000992 RID: 2450 RVA: 0x000203A4 File Offset: 0x0001E5A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarsDataTable()
			{
				base.TableName = "Calendars";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06000993 RID: 2451 RVA: 0x000203CC File Offset: 0x0001E5CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal CalendarsDataTable(DataTable table)
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

			// Token: 0x06000994 RID: 2452 RVA: 0x00020474 File Offset: 0x0001E674
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected CalendarsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170002DC RID: 732
			// (get) Token: 0x06000995 RID: 2453 RVA: 0x00020484 File Offset: 0x0001E684
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CAL_UIDColumn
			{
				get
				{
					return this.columnCAL_UID;
				}
			}

			// Token: 0x170002DD RID: 733
			// (get) Token: 0x06000996 RID: 2454 RVA: 0x0002048C File Offset: 0x0001E68C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CAL_NAMEColumn
			{
				get
				{
					return this.columnCAL_NAME;
				}
			}

			// Token: 0x170002DE RID: 734
			// (get) Token: 0x06000997 RID: 2455 RVA: 0x00020494 File Offset: 0x0001E694
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CAL_IS_STANDARD_CALColumn
			{
				get
				{
					return this.columnCAL_IS_STANDARD_CAL;
				}
			}

			// Token: 0x170002DF RID: 735
			// (get) Token: 0x06000998 RID: 2456 RVA: 0x0002049C File Offset: 0x0001E69C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CAL_CHECKOUTBYColumn
			{
				get
				{
					return this.columnCAL_CHECKOUTBY;
				}
			}

			// Token: 0x170002E0 RID: 736
			// (get) Token: 0x06000999 RID: 2457 RVA: 0x000204A4 File Offset: 0x0001E6A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CAL_CHECKOUTDATEColumn
			{
				get
				{
					return this.columnCAL_CHECKOUTDATE;
				}
			}

			// Token: 0x170002E1 RID: 737
			// (get) Token: 0x0600099A RID: 2458 RVA: 0x000204AC File Offset: 0x0001E6AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x170002E2 RID: 738
			// (get) Token: 0x0600099B RID: 2459 RVA: 0x000204B4 File Offset: 0x0001E6B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x170002E3 RID: 739
			// (get) Token: 0x0600099C RID: 2460 RVA: 0x000204BC File Offset: 0x0001E6BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CalendarUniqueIdToDuplicateColumn
			{
				get
				{
					return this.columnCalendarUniqueIdToDuplicate;
				}
			}

			// Token: 0x170002E4 RID: 740
			// (get) Token: 0x0600099D RID: 2461 RVA: 0x000204C4 File Offset: 0x0001E6C4
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

			// Token: 0x170002E5 RID: 741
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarsRow this[int index]
			{
				get
				{
					return (CalendarDataSet.CalendarsRow)base.Rows[index];
				}
			}

			// Token: 0x14000065 RID: 101
			// (add) Token: 0x0600099F RID: 2463 RVA: 0x000204E4 File Offset: 0x0001E6E4
			// (remove) Token: 0x060009A0 RID: 2464 RVA: 0x0002051C File Offset: 0x0001E71C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarsRowChangeEventHandler CalendarsRowChanging;

			// Token: 0x14000066 RID: 102
			// (add) Token: 0x060009A1 RID: 2465 RVA: 0x00020554 File Offset: 0x0001E754
			// (remove) Token: 0x060009A2 RID: 2466 RVA: 0x0002058C File Offset: 0x0001E78C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarsRowChangeEventHandler CalendarsRowChanged;

			// Token: 0x14000067 RID: 103
			// (add) Token: 0x060009A3 RID: 2467 RVA: 0x000205C4 File Offset: 0x0001E7C4
			// (remove) Token: 0x060009A4 RID: 2468 RVA: 0x000205FC File Offset: 0x0001E7FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarsRowChangeEventHandler CalendarsRowDeleting;

			// Token: 0x14000068 RID: 104
			// (add) Token: 0x060009A5 RID: 2469 RVA: 0x00020634 File Offset: 0x0001E834
			// (remove) Token: 0x060009A6 RID: 2470 RVA: 0x0002066C File Offset: 0x0001E86C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarsRowChangeEventHandler CalendarsRowDeleted;

			// Token: 0x060009A7 RID: 2471 RVA: 0x000206A1 File Offset: 0x0001E8A1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddCalendarsRow(CalendarDataSet.CalendarsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060009A8 RID: 2472 RVA: 0x000206B0 File Offset: 0x0001E8B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarDataSet.CalendarsRow AddCalendarsRow(Guid CAL_UID, string CAL_NAME, bool CAL_IS_STANDARD_CAL, Guid CAL_CHECKOUTBY, DateTime CAL_CHECKOUTDATE, DateTime CREATED_DATE, DateTime MOD_DATE, Guid CalendarUniqueIdToDuplicate)
			{
				CalendarDataSet.CalendarsRow calendarsRow = (CalendarDataSet.CalendarsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					CAL_UID,
					CAL_NAME,
					CAL_IS_STANDARD_CAL,
					CAL_CHECKOUTBY,
					CAL_CHECKOUTDATE,
					CREATED_DATE,
					MOD_DATE,
					CalendarUniqueIdToDuplicate
				};
				calendarsRow.ItemArray = itemArray;
				base.Rows.Add(calendarsRow);
				return calendarsRow;
			}

			// Token: 0x060009A9 RID: 2473 RVA: 0x00020730 File Offset: 0x0001E930
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarsRow FindByCAL_UID(Guid CAL_UID)
			{
				return (CalendarDataSet.CalendarsRow)base.Rows.Find(new object[]
				{
					CAL_UID
				});
			}

			// Token: 0x060009AA RID: 2474 RVA: 0x0002075E File Offset: 0x0001E95E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060009AB RID: 2475 RVA: 0x0002076C File Offset: 0x0001E96C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				CalendarDataSet.CalendarsDataTable calendarsDataTable = (CalendarDataSet.CalendarsDataTable)base.Clone();
				calendarsDataTable.InitVars();
				return calendarsDataTable;
			}

			// Token: 0x060009AC RID: 2476 RVA: 0x0002078C File Offset: 0x0001E98C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new CalendarDataSet.CalendarsDataTable();
			}

			// Token: 0x060009AD RID: 2477 RVA: 0x00020794 File Offset: 0x0001E994
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnCAL_UID = base.Columns["CAL_UID"];
				this.columnCAL_NAME = base.Columns["CAL_NAME"];
				this.columnCAL_IS_STANDARD_CAL = base.Columns["CAL_IS_STANDARD_CAL"];
				this.columnCAL_CHECKOUTBY = base.Columns["CAL_CHECKOUTBY"];
				this.columnCAL_CHECKOUTDATE = base.Columns["CAL_CHECKOUTDATE"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnCalendarUniqueIdToDuplicate = base.Columns["CalendarUniqueIdToDuplicate"];
			}

			// Token: 0x060009AE RID: 2478 RVA: 0x00020854 File Offset: 0x0001EA54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnCAL_UID = new DataColumn("CAL_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_UID);
				this.columnCAL_NAME = new DataColumn("CAL_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_NAME);
				this.columnCAL_IS_STANDARD_CAL = new DataColumn("CAL_IS_STANDARD_CAL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_IS_STANDARD_CAL);
				this.columnCAL_CHECKOUTBY = new DataColumn("CAL_CHECKOUTBY", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_CHECKOUTBY);
				this.columnCAL_CHECKOUTDATE = new DataColumn("CAL_CHECKOUTDATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_CHECKOUTDATE);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnCalendarUniqueIdToDuplicate = new DataColumn("CalendarUniqueIdToDuplicate", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCalendarUniqueIdToDuplicate);
				base.Constraints.Add(new UniqueConstraint("CalendarDataSetKey1", new DataColumn[]
				{
					this.columnCAL_UID
				}, true));
				this.columnCAL_UID.AllowDBNull = false;
				this.columnCAL_UID.Unique = true;
				this.columnCAL_IS_STANDARD_CAL.ReadOnly = true;
				this.columnCAL_IS_STANDARD_CAL.DefaultValue = false;
				this.columnCAL_CHECKOUTBY.ReadOnly = true;
				this.columnCAL_CHECKOUTDATE.ReadOnly = true;
				this.columnCREATED_DATE.ReadOnly = true;
				this.columnMOD_DATE.ReadOnly = true;
			}

			// Token: 0x060009AF RID: 2479 RVA: 0x00020A55 File Offset: 0x0001EC55
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarsRow NewCalendarsRow()
			{
				return (CalendarDataSet.CalendarsRow)base.NewRow();
			}

			// Token: 0x060009B0 RID: 2480 RVA: 0x00020A62 File Offset: 0x0001EC62
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new CalendarDataSet.CalendarsRow(builder);
			}

			// Token: 0x060009B1 RID: 2481 RVA: 0x00020A6A File Offset: 0x0001EC6A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(CalendarDataSet.CalendarsRow);
			}

			// Token: 0x060009B2 RID: 2482 RVA: 0x00020A76 File Offset: 0x0001EC76
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CalendarsRowChanged != null)
				{
					this.CalendarsRowChanged(this, new CalendarDataSet.CalendarsRowChangeEvent((CalendarDataSet.CalendarsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009B3 RID: 2483 RVA: 0x00020AA9 File Offset: 0x0001ECA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CalendarsRowChanging != null)
				{
					this.CalendarsRowChanging(this, new CalendarDataSet.CalendarsRowChangeEvent((CalendarDataSet.CalendarsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009B4 RID: 2484 RVA: 0x00020ADC File Offset: 0x0001ECDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CalendarsRowDeleted != null)
				{
					this.CalendarsRowDeleted(this, new CalendarDataSet.CalendarsRowChangeEvent((CalendarDataSet.CalendarsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009B5 RID: 2485 RVA: 0x00020B0F File Offset: 0x0001ED0F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CalendarsRowDeleting != null)
				{
					this.CalendarsRowDeleting(this, new CalendarDataSet.CalendarsRowChangeEvent((CalendarDataSet.CalendarsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009B6 RID: 2486 RVA: 0x00020B42 File Offset: 0x0001ED42
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveCalendarsRow(CalendarDataSet.CalendarsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060009B7 RID: 2487 RVA: 0x00020B50 File Offset: 0x0001ED50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				CalendarDataSet calendarDataSet = new CalendarDataSet();
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
				xmlSchemaAttribute.FixedValue = calendarDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CalendarsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = calendarDataSet.GetSchemaSerializable();
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

			// Token: 0x04000254 RID: 596
			private DataColumn columnCAL_UID;

			// Token: 0x04000255 RID: 597
			private DataColumn columnCAL_NAME;

			// Token: 0x04000256 RID: 598
			private DataColumn columnCAL_IS_STANDARD_CAL;

			// Token: 0x04000257 RID: 599
			private DataColumn columnCAL_CHECKOUTBY;

			// Token: 0x04000258 RID: 600
			private DataColumn columnCAL_CHECKOUTDATE;

			// Token: 0x04000259 RID: 601
			private DataColumn columnCREATED_DATE;

			// Token: 0x0400025A RID: 602
			private DataColumn columnMOD_DATE;

			// Token: 0x0400025B RID: 603
			private DataColumn columnCalendarUniqueIdToDuplicate;
		}

		// Token: 0x02000089 RID: 137
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CalendarExceptionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060009B8 RID: 2488 RVA: 0x00020D48 File Offset: 0x0001EF48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarExceptionsDataTable()
			{
				base.TableName = "CalendarExceptions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060009B9 RID: 2489 RVA: 0x00020D70 File Offset: 0x0001EF70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CalendarExceptionsDataTable(DataTable table)
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

			// Token: 0x060009BA RID: 2490 RVA: 0x00020E18 File Offset: 0x0001F018
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected CalendarExceptionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170002E6 RID: 742
			// (get) Token: 0x060009BB RID: 2491 RVA: 0x00020E28 File Offset: 0x0001F028
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CAL_UIDColumn
			{
				get
				{
					return this.columnCAL_UID;
				}
			}

			// Token: 0x170002E7 RID: 743
			// (get) Token: 0x060009BC RID: 2492 RVA: 0x00020E30 File Offset: 0x0001F030
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn NameColumn
			{
				get
				{
					return this.columnName;
				}
			}

			// Token: 0x170002E8 RID: 744
			// (get) Token: 0x060009BD RID: 2493 RVA: 0x00020E38 File Offset: 0x0001F038
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn StartColumn
			{
				get
				{
					return this.columnStart;
				}
			}

			// Token: 0x170002E9 RID: 745
			// (get) Token: 0x060009BE RID: 2494 RVA: 0x00020E40 File Offset: 0x0001F040
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn FinishColumn
			{
				get
				{
					return this.columnFinish;
				}
			}

			// Token: 0x170002EA RID: 746
			// (get) Token: 0x060009BF RID: 2495 RVA: 0x00020E48 File Offset: 0x0001F048
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift1StartColumn
			{
				get
				{
					return this.columnShift1Start;
				}
			}

			// Token: 0x170002EB RID: 747
			// (get) Token: 0x060009C0 RID: 2496 RVA: 0x00020E50 File Offset: 0x0001F050
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift1FinishColumn
			{
				get
				{
					return this.columnShift1Finish;
				}
			}

			// Token: 0x170002EC RID: 748
			// (get) Token: 0x060009C1 RID: 2497 RVA: 0x00020E58 File Offset: 0x0001F058
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift2StartColumn
			{
				get
				{
					return this.columnShift2Start;
				}
			}

			// Token: 0x170002ED RID: 749
			// (get) Token: 0x060009C2 RID: 2498 RVA: 0x00020E60 File Offset: 0x0001F060
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift2FinishColumn
			{
				get
				{
					return this.columnShift2Finish;
				}
			}

			// Token: 0x170002EE RID: 750
			// (get) Token: 0x060009C3 RID: 2499 RVA: 0x00020E68 File Offset: 0x0001F068
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift3StartColumn
			{
				get
				{
					return this.columnShift3Start;
				}
			}

			// Token: 0x170002EF RID: 751
			// (get) Token: 0x060009C4 RID: 2500 RVA: 0x00020E70 File Offset: 0x0001F070
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift3FinishColumn
			{
				get
				{
					return this.columnShift3Finish;
				}
			}

			// Token: 0x170002F0 RID: 752
			// (get) Token: 0x060009C5 RID: 2501 RVA: 0x00020E78 File Offset: 0x0001F078
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift4StartColumn
			{
				get
				{
					return this.columnShift4Start;
				}
			}

			// Token: 0x170002F1 RID: 753
			// (get) Token: 0x060009C6 RID: 2502 RVA: 0x00020E80 File Offset: 0x0001F080
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift4FinishColumn
			{
				get
				{
					return this.columnShift4Finish;
				}
			}

			// Token: 0x170002F2 RID: 754
			// (get) Token: 0x060009C7 RID: 2503 RVA: 0x00020E88 File Offset: 0x0001F088
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn Shift5StartColumn
			{
				get
				{
					return this.columnShift5Start;
				}
			}

			// Token: 0x170002F3 RID: 755
			// (get) Token: 0x060009C8 RID: 2504 RVA: 0x00020E90 File Offset: 0x0001F090
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn Shift5FinishColumn
			{
				get
				{
					return this.columnShift5Finish;
				}
			}

			// Token: 0x170002F4 RID: 756
			// (get) Token: 0x060009C9 RID: 2505 RVA: 0x00020E98 File Offset: 0x0001F098
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RecurrenceTypeColumn
			{
				get
				{
					return this.columnRecurrenceType;
				}
			}

			// Token: 0x170002F5 RID: 757
			// (get) Token: 0x060009CA RID: 2506 RVA: 0x00020EA0 File Offset: 0x0001F0A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceFrequencyColumn
			{
				get
				{
					return this.columnRecurrenceFrequency;
				}
			}

			// Token: 0x170002F6 RID: 758
			// (get) Token: 0x060009CB RID: 2507 RVA: 0x00020EA8 File Offset: 0x0001F0A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RecurrenceDaysColumn
			{
				get
				{
					return this.columnRecurrenceDays;
				}
			}

			// Token: 0x170002F7 RID: 759
			// (get) Token: 0x060009CC RID: 2508 RVA: 0x00020EB0 File Offset: 0x0001F0B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RecurrenceMonthDayColumn
			{
				get
				{
					return this.columnRecurrenceMonthDay;
				}
			}

			// Token: 0x170002F8 RID: 760
			// (get) Token: 0x060009CD RID: 2509 RVA: 0x00020EB8 File Offset: 0x0001F0B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrenceMonthColumn
			{
				get
				{
					return this.columnRecurrenceMonth;
				}
			}

			// Token: 0x170002F9 RID: 761
			// (get) Token: 0x060009CE RID: 2510 RVA: 0x00020EC0 File Offset: 0x0001F0C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RecurrencePositionColumn
			{
				get
				{
					return this.columnRecurrencePosition;
				}
			}

			// Token: 0x170002FA RID: 762
			// (get) Token: 0x060009CF RID: 2511 RVA: 0x00020EC8 File Offset: 0x0001F0C8
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

			// Token: 0x170002FB RID: 763
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarExceptionsRow this[int index]
			{
				get
				{
					return (CalendarDataSet.CalendarExceptionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000069 RID: 105
			// (add) Token: 0x060009D1 RID: 2513 RVA: 0x00020EE8 File Offset: 0x0001F0E8
			// (remove) Token: 0x060009D2 RID: 2514 RVA: 0x00020F20 File Offset: 0x0001F120
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowChanging;

			// Token: 0x1400006A RID: 106
			// (add) Token: 0x060009D3 RID: 2515 RVA: 0x00020F58 File Offset: 0x0001F158
			// (remove) Token: 0x060009D4 RID: 2516 RVA: 0x00020F90 File Offset: 0x0001F190
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowChanged;

			// Token: 0x1400006B RID: 107
			// (add) Token: 0x060009D5 RID: 2517 RVA: 0x00020FC8 File Offset: 0x0001F1C8
			// (remove) Token: 0x060009D6 RID: 2518 RVA: 0x00021000 File Offset: 0x0001F200
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowDeleting;

			// Token: 0x1400006C RID: 108
			// (add) Token: 0x060009D7 RID: 2519 RVA: 0x00021038 File Offset: 0x0001F238
			// (remove) Token: 0x060009D8 RID: 2520 RVA: 0x00021070 File Offset: 0x0001F270
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event CalendarDataSet.CalendarExceptionsRowChangeEventHandler CalendarExceptionsRowDeleted;

			// Token: 0x060009D9 RID: 2521 RVA: 0x000210A5 File Offset: 0x0001F2A5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddCalendarExceptionsRow(CalendarDataSet.CalendarExceptionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060009DA RID: 2522 RVA: 0x000210B4 File Offset: 0x0001F2B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarDataSet.CalendarExceptionsRow AddCalendarExceptionsRow(CalendarDataSet.CalendarsRow parentCalendarsRowByCalendarCalendarExceptions, string Name, DateTime Start, DateTime Finish, int Shift1Start, int Shift1Finish, int Shift2Start, int Shift2Finish, int Shift3Start, int Shift3Finish, int Shift4Start, int Shift4Finish, int Shift5Start, int Shift5Finish, int RecurrenceType, int RecurrenceFrequency, int RecurrenceDays, int RecurrenceMonthDay, int RecurrenceMonth, int RecurrencePosition)
			{
				CalendarDataSet.CalendarExceptionsRow calendarExceptionsRow = (CalendarDataSet.CalendarExceptionsRow)base.NewRow();
				object[] array = new object[]
				{
					null,
					Name,
					Start,
					Finish,
					Shift1Start,
					Shift1Finish,
					Shift2Start,
					Shift2Finish,
					Shift3Start,
					Shift3Finish,
					Shift4Start,
					Shift4Finish,
					Shift5Start,
					Shift5Finish,
					RecurrenceType,
					RecurrenceFrequency,
					RecurrenceDays,
					RecurrenceMonthDay,
					RecurrenceMonth,
					RecurrencePosition
				};
				if (parentCalendarsRowByCalendarCalendarExceptions != null)
				{
					array[0] = parentCalendarsRowByCalendarCalendarExceptions[0];
				}
				calendarExceptionsRow.ItemArray = array;
				base.Rows.Add(calendarExceptionsRow);
				return calendarExceptionsRow;
			}

			// Token: 0x060009DB RID: 2523 RVA: 0x000211BA File Offset: 0x0001F3BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060009DC RID: 2524 RVA: 0x000211C8 File Offset: 0x0001F3C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				CalendarDataSet.CalendarExceptionsDataTable calendarExceptionsDataTable = (CalendarDataSet.CalendarExceptionsDataTable)base.Clone();
				calendarExceptionsDataTable.InitVars();
				return calendarExceptionsDataTable;
			}

			// Token: 0x060009DD RID: 2525 RVA: 0x000211E8 File Offset: 0x0001F3E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new CalendarDataSet.CalendarExceptionsDataTable();
			}

			// Token: 0x060009DE RID: 2526 RVA: 0x000211F0 File Offset: 0x0001F3F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnCAL_UID = base.Columns["CAL_UID"];
				this.columnName = base.Columns["Name"];
				this.columnStart = base.Columns["Start"];
				this.columnFinish = base.Columns["Finish"];
				this.columnShift1Start = base.Columns["Shift1Start"];
				this.columnShift1Finish = base.Columns["Shift1Finish"];
				this.columnShift2Start = base.Columns["Shift2Start"];
				this.columnShift2Finish = base.Columns["Shift2Finish"];
				this.columnShift3Start = base.Columns["Shift3Start"];
				this.columnShift3Finish = base.Columns["Shift3Finish"];
				this.columnShift4Start = base.Columns["Shift4Start"];
				this.columnShift4Finish = base.Columns["Shift4Finish"];
				this.columnShift5Start = base.Columns["Shift5Start"];
				this.columnShift5Finish = base.Columns["Shift5Finish"];
				this.columnRecurrenceType = base.Columns["RecurrenceType"];
				this.columnRecurrenceFrequency = base.Columns["RecurrenceFrequency"];
				this.columnRecurrenceDays = base.Columns["RecurrenceDays"];
				this.columnRecurrenceMonthDay = base.Columns["RecurrenceMonthDay"];
				this.columnRecurrenceMonth = base.Columns["RecurrenceMonth"];
				this.columnRecurrencePosition = base.Columns["RecurrencePosition"];
			}

			// Token: 0x060009DF RID: 2527 RVA: 0x000213B8 File Offset: 0x0001F5B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnCAL_UID = new DataColumn("CAL_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnCAL_UID);
				this.columnName = new DataColumn("Name", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnName);
				this.columnStart = new DataColumn("Start", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnStart);
				this.columnFinish = new DataColumn("Finish", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnFinish);
				this.columnShift1Start = new DataColumn("Shift1Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift1Start);
				this.columnShift1Finish = new DataColumn("Shift1Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift1Finish);
				this.columnShift2Start = new DataColumn("Shift2Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift2Start);
				this.columnShift2Finish = new DataColumn("Shift2Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift2Finish);
				this.columnShift3Start = new DataColumn("Shift3Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift3Start);
				this.columnShift3Finish = new DataColumn("Shift3Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift3Finish);
				this.columnShift4Start = new DataColumn("Shift4Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift4Start);
				this.columnShift4Finish = new DataColumn("Shift4Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift4Finish);
				this.columnShift5Start = new DataColumn("Shift5Start", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift5Start);
				this.columnShift5Finish = new DataColumn("Shift5Finish", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnShift5Finish);
				this.columnRecurrenceType = new DataColumn("RecurrenceType", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceType);
				this.columnRecurrenceFrequency = new DataColumn("RecurrenceFrequency", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceFrequency);
				this.columnRecurrenceDays = new DataColumn("RecurrenceDays", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceDays);
				this.columnRecurrenceMonthDay = new DataColumn("RecurrenceMonthDay", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceMonthDay);
				this.columnRecurrenceMonth = new DataColumn("RecurrenceMonth", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrenceMonth);
				this.columnRecurrencePosition = new DataColumn("RecurrencePosition", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnRecurrencePosition);
				this.columnCAL_UID.AllowDBNull = false;
			}

			// Token: 0x060009E0 RID: 2528 RVA: 0x00021755 File Offset: 0x0001F955
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarExceptionsRow NewCalendarExceptionsRow()
			{
				return (CalendarDataSet.CalendarExceptionsRow)base.NewRow();
			}

			// Token: 0x060009E1 RID: 2529 RVA: 0x00021762 File Offset: 0x0001F962
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new CalendarDataSet.CalendarExceptionsRow(builder);
			}

			// Token: 0x060009E2 RID: 2530 RVA: 0x0002176A File Offset: 0x0001F96A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(CalendarDataSet.CalendarExceptionsRow);
			}

			// Token: 0x060009E3 RID: 2531 RVA: 0x00021776 File Offset: 0x0001F976
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CalendarExceptionsRowChanged != null)
				{
					this.CalendarExceptionsRowChanged(this, new CalendarDataSet.CalendarExceptionsRowChangeEvent((CalendarDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009E4 RID: 2532 RVA: 0x000217A9 File Offset: 0x0001F9A9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CalendarExceptionsRowChanging != null)
				{
					this.CalendarExceptionsRowChanging(this, new CalendarDataSet.CalendarExceptionsRowChangeEvent((CalendarDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009E5 RID: 2533 RVA: 0x000217DC File Offset: 0x0001F9DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CalendarExceptionsRowDeleted != null)
				{
					this.CalendarExceptionsRowDeleted(this, new CalendarDataSet.CalendarExceptionsRowChangeEvent((CalendarDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009E6 RID: 2534 RVA: 0x0002180F File Offset: 0x0001FA0F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CalendarExceptionsRowDeleting != null)
				{
					this.CalendarExceptionsRowDeleting(this, new CalendarDataSet.CalendarExceptionsRowChangeEvent((CalendarDataSet.CalendarExceptionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060009E7 RID: 2535 RVA: 0x00021842 File Offset: 0x0001FA42
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveCalendarExceptionsRow(CalendarDataSet.CalendarExceptionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060009E8 RID: 2536 RVA: 0x00021850 File Offset: 0x0001FA50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				CalendarDataSet calendarDataSet = new CalendarDataSet();
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
				xmlSchemaAttribute.FixedValue = calendarDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CalendarExceptionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = calendarDataSet.GetSchemaSerializable();
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

			// Token: 0x04000260 RID: 608
			private DataColumn columnCAL_UID;

			// Token: 0x04000261 RID: 609
			private DataColumn columnName;

			// Token: 0x04000262 RID: 610
			private DataColumn columnStart;

			// Token: 0x04000263 RID: 611
			private DataColumn columnFinish;

			// Token: 0x04000264 RID: 612
			private DataColumn columnShift1Start;

			// Token: 0x04000265 RID: 613
			private DataColumn columnShift1Finish;

			// Token: 0x04000266 RID: 614
			private DataColumn columnShift2Start;

			// Token: 0x04000267 RID: 615
			private DataColumn columnShift2Finish;

			// Token: 0x04000268 RID: 616
			private DataColumn columnShift3Start;

			// Token: 0x04000269 RID: 617
			private DataColumn columnShift3Finish;

			// Token: 0x0400026A RID: 618
			private DataColumn columnShift4Start;

			// Token: 0x0400026B RID: 619
			private DataColumn columnShift4Finish;

			// Token: 0x0400026C RID: 620
			private DataColumn columnShift5Start;

			// Token: 0x0400026D RID: 621
			private DataColumn columnShift5Finish;

			// Token: 0x0400026E RID: 622
			private DataColumn columnRecurrenceType;

			// Token: 0x0400026F RID: 623
			private DataColumn columnRecurrenceFrequency;

			// Token: 0x04000270 RID: 624
			private DataColumn columnRecurrenceDays;

			// Token: 0x04000271 RID: 625
			private DataColumn columnRecurrenceMonthDay;

			// Token: 0x04000272 RID: 626
			private DataColumn columnRecurrenceMonth;

			// Token: 0x04000273 RID: 627
			private DataColumn columnRecurrencePosition;
		}

		// Token: 0x0200008A RID: 138
		public class CalendarsRow : DataRow
		{
			// Token: 0x060009E9 RID: 2537 RVA: 0x00021A48 File Offset: 0x0001FC48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CalendarsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCalendars = (CalendarDataSet.CalendarsDataTable)base.Table;
			}

			// Token: 0x170002FC RID: 764
			// (get) Token: 0x060009EA RID: 2538 RVA: 0x00021A62 File Offset: 0x0001FC62
			// (set) Token: 0x060009EB RID: 2539 RVA: 0x00021A7A File Offset: 0x0001FC7A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CAL_UID
			{
				get
				{
					return (Guid)base[this.tableCalendars.CAL_UIDColumn];
				}
				set
				{
					base[this.tableCalendars.CAL_UIDColumn] = value;
				}
			}

			// Token: 0x170002FD RID: 765
			// (get) Token: 0x060009EC RID: 2540 RVA: 0x00021A94 File Offset: 0x0001FC94
			// (set) Token: 0x060009ED RID: 2541 RVA: 0x00021AD8 File Offset: 0x0001FCD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string CAL_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableCalendars.CAL_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CAL_NAME' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CAL_NAMEColumn] = value;
				}
			}

			// Token: 0x170002FE RID: 766
			// (get) Token: 0x060009EE RID: 2542 RVA: 0x00021AEC File Offset: 0x0001FCEC
			// (set) Token: 0x060009EF RID: 2543 RVA: 0x00021B30 File Offset: 0x0001FD30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool CAL_IS_STANDARD_CAL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableCalendars.CAL_IS_STANDARD_CALColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CAL_IS_STANDARD_CAL' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CAL_IS_STANDARD_CALColumn] = value;
				}
			}

			// Token: 0x170002FF RID: 767
			// (get) Token: 0x060009F0 RID: 2544 RVA: 0x00021B4C File Offset: 0x0001FD4C
			// (set) Token: 0x060009F1 RID: 2545 RVA: 0x00021B90 File Offset: 0x0001FD90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CAL_CHECKOUTBY
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableCalendars.CAL_CHECKOUTBYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CAL_CHECKOUTBY' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CAL_CHECKOUTBYColumn] = value;
				}
			}

			// Token: 0x17000300 RID: 768
			// (get) Token: 0x060009F2 RID: 2546 RVA: 0x00021BAC File Offset: 0x0001FDAC
			// (set) Token: 0x060009F3 RID: 2547 RVA: 0x00021BF0 File Offset: 0x0001FDF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CAL_CHECKOUTDATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableCalendars.CAL_CHECKOUTDATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CAL_CHECKOUTDATE' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CAL_CHECKOUTDATEColumn] = value;
				}
			}

			// Token: 0x17000301 RID: 769
			// (get) Token: 0x060009F4 RID: 2548 RVA: 0x00021C0C File Offset: 0x0001FE0C
			// (set) Token: 0x060009F5 RID: 2549 RVA: 0x00021C50 File Offset: 0x0001FE50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime CREATED_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableCalendars.CREATED_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CREATED_DATE' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17000302 RID: 770
			// (get) Token: 0x060009F6 RID: 2550 RVA: 0x00021C6C File Offset: 0x0001FE6C
			// (set) Token: 0x060009F7 RID: 2551 RVA: 0x00021CB0 File Offset: 0x0001FEB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableCalendars.MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'MOD_DATE' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17000303 RID: 771
			// (get) Token: 0x060009F8 RID: 2552 RVA: 0x00021CCC File Offset: 0x0001FECC
			// (set) Token: 0x060009F9 RID: 2553 RVA: 0x00021D10 File Offset: 0x0001FF10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CalendarUniqueIdToDuplicate
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableCalendars.CalendarUniqueIdToDuplicateColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CalendarUniqueIdToDuplicate' in table 'Calendars' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendars.CalendarUniqueIdToDuplicateColumn] = value;
				}
			}

			// Token: 0x060009FA RID: 2554 RVA: 0x00021D29 File Offset: 0x0001FF29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCAL_NAMENull()
			{
				return base.IsNull(this.tableCalendars.CAL_NAMEColumn);
			}

			// Token: 0x060009FB RID: 2555 RVA: 0x00021D3C File Offset: 0x0001FF3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCAL_NAMENull()
			{
				base[this.tableCalendars.CAL_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x060009FC RID: 2556 RVA: 0x00021D54 File Offset: 0x0001FF54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCAL_IS_STANDARD_CALNull()
			{
				return base.IsNull(this.tableCalendars.CAL_IS_STANDARD_CALColumn);
			}

			// Token: 0x060009FD RID: 2557 RVA: 0x00021D67 File Offset: 0x0001FF67
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCAL_IS_STANDARD_CALNull()
			{
				base[this.tableCalendars.CAL_IS_STANDARD_CALColumn] = Convert.DBNull;
			}

			// Token: 0x060009FE RID: 2558 RVA: 0x00021D7F File Offset: 0x0001FF7F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCAL_CHECKOUTBYNull()
			{
				return base.IsNull(this.tableCalendars.CAL_CHECKOUTBYColumn);
			}

			// Token: 0x060009FF RID: 2559 RVA: 0x00021D92 File Offset: 0x0001FF92
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCAL_CHECKOUTBYNull()
			{
				base[this.tableCalendars.CAL_CHECKOUTBYColumn] = Convert.DBNull;
			}

			// Token: 0x06000A00 RID: 2560 RVA: 0x00021DAA File Offset: 0x0001FFAA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCAL_CHECKOUTDATENull()
			{
				return base.IsNull(this.tableCalendars.CAL_CHECKOUTDATEColumn);
			}

			// Token: 0x06000A01 RID: 2561 RVA: 0x00021DBD File Offset: 0x0001FFBD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCAL_CHECKOUTDATENull()
			{
				base[this.tableCalendars.CAL_CHECKOUTDATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000A02 RID: 2562 RVA: 0x00021DD5 File Offset: 0x0001FFD5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCREATED_DATENull()
			{
				return base.IsNull(this.tableCalendars.CREATED_DATEColumn);
			}

			// Token: 0x06000A03 RID: 2563 RVA: 0x00021DE8 File Offset: 0x0001FFE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCREATED_DATENull()
			{
				base[this.tableCalendars.CREATED_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000A04 RID: 2564 RVA: 0x00021E00 File Offset: 0x00020000
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsMOD_DATENull()
			{
				return base.IsNull(this.tableCalendars.MOD_DATEColumn);
			}

			// Token: 0x06000A05 RID: 2565 RVA: 0x00021E13 File Offset: 0x00020013
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetMOD_DATENull()
			{
				base[this.tableCalendars.MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x06000A06 RID: 2566 RVA: 0x00021E2B File Offset: 0x0002002B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsCalendarUniqueIdToDuplicateNull()
			{
				return base.IsNull(this.tableCalendars.CalendarUniqueIdToDuplicateColumn);
			}

			// Token: 0x06000A07 RID: 2567 RVA: 0x00021E3E File Offset: 0x0002003E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetCalendarUniqueIdToDuplicateNull()
			{
				base[this.tableCalendars.CalendarUniqueIdToDuplicateColumn] = Convert.DBNull;
			}

			// Token: 0x06000A08 RID: 2568 RVA: 0x00021E56 File Offset: 0x00020056
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarDataSet.CalendarExceptionsRow[] GetCalendarExceptionsRows()
			{
				if (base.Table.ChildRelations["CalendarCalendarExceptions"] == null)
				{
					return new CalendarDataSet.CalendarExceptionsRow[0];
				}
				return (CalendarDataSet.CalendarExceptionsRow[])base.GetChildRows(base.Table.ChildRelations["CalendarCalendarExceptions"]);
			}

			// Token: 0x04000278 RID: 632
			private CalendarDataSet.CalendarsDataTable tableCalendars;
		}

		// Token: 0x0200008B RID: 139
		public class CalendarExceptionsRow : DataRow
		{
			// Token: 0x06000A09 RID: 2569 RVA: 0x00021E96 File Offset: 0x00020096
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CalendarExceptionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCalendarExceptions = (CalendarDataSet.CalendarExceptionsDataTable)base.Table;
			}

			// Token: 0x17000304 RID: 772
			// (get) Token: 0x06000A0A RID: 2570 RVA: 0x00021EB0 File Offset: 0x000200B0
			// (set) Token: 0x06000A0B RID: 2571 RVA: 0x00021EC8 File Offset: 0x000200C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid CAL_UID
			{
				get
				{
					return (Guid)base[this.tableCalendarExceptions.CAL_UIDColumn];
				}
				set
				{
					base[this.tableCalendarExceptions.CAL_UIDColumn] = value;
				}
			}

			// Token: 0x17000305 RID: 773
			// (get) Token: 0x06000A0C RID: 2572 RVA: 0x00021EE4 File Offset: 0x000200E4
			// (set) Token: 0x06000A0D RID: 2573 RVA: 0x00021F28 File Offset: 0x00020128
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string Name
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableCalendarExceptions.NameColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Name' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.NameColumn] = value;
				}
			}

			// Token: 0x17000306 RID: 774
			// (get) Token: 0x06000A0E RID: 2574 RVA: 0x00021F3C File Offset: 0x0002013C
			// (set) Token: 0x06000A0F RID: 2575 RVA: 0x00021F80 File Offset: 0x00020180
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime Start
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableCalendarExceptions.StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.StartColumn] = value;
				}
			}

			// Token: 0x17000307 RID: 775
			// (get) Token: 0x06000A10 RID: 2576 RVA: 0x00021F9C File Offset: 0x0002019C
			// (set) Token: 0x06000A11 RID: 2577 RVA: 0x00021FE0 File Offset: 0x000201E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime Finish
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableCalendarExceptions.FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.FinishColumn] = value;
				}
			}

			// Token: 0x17000308 RID: 776
			// (get) Token: 0x06000A12 RID: 2578 RVA: 0x00021FFC File Offset: 0x000201FC
			// (set) Token: 0x06000A13 RID: 2579 RVA: 0x00022040 File Offset: 0x00020240
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift1Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift1StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift1Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift1StartColumn] = value;
				}
			}

			// Token: 0x17000309 RID: 777
			// (get) Token: 0x06000A14 RID: 2580 RVA: 0x0002205C File Offset: 0x0002025C
			// (set) Token: 0x06000A15 RID: 2581 RVA: 0x000220A0 File Offset: 0x000202A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Shift1Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift1FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift1Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift1FinishColumn] = value;
				}
			}

			// Token: 0x1700030A RID: 778
			// (get) Token: 0x06000A16 RID: 2582 RVA: 0x000220BC File Offset: 0x000202BC
			// (set) Token: 0x06000A17 RID: 2583 RVA: 0x00022100 File Offset: 0x00020300
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift2Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift2StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift2Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift2StartColumn] = value;
				}
			}

			// Token: 0x1700030B RID: 779
			// (get) Token: 0x06000A18 RID: 2584 RVA: 0x0002211C File Offset: 0x0002031C
			// (set) Token: 0x06000A19 RID: 2585 RVA: 0x00022160 File Offset: 0x00020360
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Shift2Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift2FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift2Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift2FinishColumn] = value;
				}
			}

			// Token: 0x1700030C RID: 780
			// (get) Token: 0x06000A1A RID: 2586 RVA: 0x0002217C File Offset: 0x0002037C
			// (set) Token: 0x06000A1B RID: 2587 RVA: 0x000221C0 File Offset: 0x000203C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Shift3Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift3StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift3Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift3StartColumn] = value;
				}
			}

			// Token: 0x1700030D RID: 781
			// (get) Token: 0x06000A1C RID: 2588 RVA: 0x000221DC File Offset: 0x000203DC
			// (set) Token: 0x06000A1D RID: 2589 RVA: 0x00022220 File Offset: 0x00020420
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift3Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift3FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift3Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift3FinishColumn] = value;
				}
			}

			// Token: 0x1700030E RID: 782
			// (get) Token: 0x06000A1E RID: 2590 RVA: 0x0002223C File Offset: 0x0002043C
			// (set) Token: 0x06000A1F RID: 2591 RVA: 0x00022280 File Offset: 0x00020480
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Shift4Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift4StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift4Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift4StartColumn] = value;
				}
			}

			// Token: 0x1700030F RID: 783
			// (get) Token: 0x06000A20 RID: 2592 RVA: 0x0002229C File Offset: 0x0002049C
			// (set) Token: 0x06000A21 RID: 2593 RVA: 0x000222E0 File Offset: 0x000204E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift4Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift4FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift4Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift4FinishColumn] = value;
				}
			}

			// Token: 0x17000310 RID: 784
			// (get) Token: 0x06000A22 RID: 2594 RVA: 0x000222FC File Offset: 0x000204FC
			// (set) Token: 0x06000A23 RID: 2595 RVA: 0x00022340 File Offset: 0x00020540
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int Shift5Start
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift5StartColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift5Start' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift5StartColumn] = value;
				}
			}

			// Token: 0x17000311 RID: 785
			// (get) Token: 0x06000A24 RID: 2596 RVA: 0x0002235C File Offset: 0x0002055C
			// (set) Token: 0x06000A25 RID: 2597 RVA: 0x000223A0 File Offset: 0x000205A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Shift5Finish
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.Shift5FinishColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'Shift5Finish' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.Shift5FinishColumn] = value;
				}
			}

			// Token: 0x17000312 RID: 786
			// (get) Token: 0x06000A26 RID: 2598 RVA: 0x000223BC File Offset: 0x000205BC
			// (set) Token: 0x06000A27 RID: 2599 RVA: 0x00022400 File Offset: 0x00020600
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceType
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceTypeColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceType' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceTypeColumn] = value;
				}
			}

			// Token: 0x17000313 RID: 787
			// (get) Token: 0x06000A28 RID: 2600 RVA: 0x0002241C File Offset: 0x0002061C
			// (set) Token: 0x06000A29 RID: 2601 RVA: 0x00022460 File Offset: 0x00020660
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int RecurrenceFrequency
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceFrequencyColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceFrequency' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceFrequencyColumn] = value;
				}
			}

			// Token: 0x17000314 RID: 788
			// (get) Token: 0x06000A2A RID: 2602 RVA: 0x0002247C File Offset: 0x0002067C
			// (set) Token: 0x06000A2B RID: 2603 RVA: 0x000224C0 File Offset: 0x000206C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int RecurrenceDays
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceDaysColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceDays' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceDaysColumn] = value;
				}
			}

			// Token: 0x17000315 RID: 789
			// (get) Token: 0x06000A2C RID: 2604 RVA: 0x000224DC File Offset: 0x000206DC
			// (set) Token: 0x06000A2D RID: 2605 RVA: 0x00022520 File Offset: 0x00020720
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int RecurrenceMonthDay
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceMonthDayColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceMonthDay' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceMonthDayColumn] = value;
				}
			}

			// Token: 0x17000316 RID: 790
			// (get) Token: 0x06000A2E RID: 2606 RVA: 0x0002253C File Offset: 0x0002073C
			// (set) Token: 0x06000A2F RID: 2607 RVA: 0x00022580 File Offset: 0x00020780
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int RecurrenceMonth
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrenceMonthColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrenceMonth' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrenceMonthColumn] = value;
				}
			}

			// Token: 0x17000317 RID: 791
			// (get) Token: 0x06000A30 RID: 2608 RVA: 0x0002259C File Offset: 0x0002079C
			// (set) Token: 0x06000A31 RID: 2609 RVA: 0x000225E0 File Offset: 0x000207E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int RecurrencePosition
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableCalendarExceptions.RecurrencePositionColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RecurrencePosition' in table 'CalendarExceptions' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableCalendarExceptions.RecurrencePositionColumn] = value;
				}
			}

			// Token: 0x17000318 RID: 792
			// (get) Token: 0x06000A32 RID: 2610 RVA: 0x000225F9 File Offset: 0x000207F9
			// (set) Token: 0x06000A33 RID: 2611 RVA: 0x0002261B File Offset: 0x0002081B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarsRow CalendarsRow
			{
				get
				{
					return (CalendarDataSet.CalendarsRow)base.GetParentRow(base.Table.ParentRelations["CalendarCalendarExceptions"]);
				}
				set
				{
					base.SetParentRow(value, base.Table.ParentRelations["CalendarCalendarExceptions"]);
				}
			}

			// Token: 0x06000A34 RID: 2612 RVA: 0x00022639 File Offset: 0x00020839
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsNameNull()
			{
				return base.IsNull(this.tableCalendarExceptions.NameColumn);
			}

			// Token: 0x06000A35 RID: 2613 RVA: 0x0002264C File Offset: 0x0002084C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetNameNull()
			{
				base[this.tableCalendarExceptions.NameColumn] = Convert.DBNull;
			}

			// Token: 0x06000A36 RID: 2614 RVA: 0x00022664 File Offset: 0x00020864
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsStartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.StartColumn);
			}

			// Token: 0x06000A37 RID: 2615 RVA: 0x00022677 File Offset: 0x00020877
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetStartNull()
			{
				base[this.tableCalendarExceptions.StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A38 RID: 2616 RVA: 0x0002268F File Offset: 0x0002088F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsFinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.FinishColumn);
			}

			// Token: 0x06000A39 RID: 2617 RVA: 0x000226A2 File Offset: 0x000208A2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetFinishNull()
			{
				base[this.tableCalendarExceptions.FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A3A RID: 2618 RVA: 0x000226BA File Offset: 0x000208BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift1StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift1StartColumn);
			}

			// Token: 0x06000A3B RID: 2619 RVA: 0x000226CD File Offset: 0x000208CD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift1StartNull()
			{
				base[this.tableCalendarExceptions.Shift1StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A3C RID: 2620 RVA: 0x000226E5 File Offset: 0x000208E5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift1FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift1FinishColumn);
			}

			// Token: 0x06000A3D RID: 2621 RVA: 0x000226F8 File Offset: 0x000208F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift1FinishNull()
			{
				base[this.tableCalendarExceptions.Shift1FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A3E RID: 2622 RVA: 0x00022710 File Offset: 0x00020910
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift2StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift2StartColumn);
			}

			// Token: 0x06000A3F RID: 2623 RVA: 0x00022723 File Offset: 0x00020923
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift2StartNull()
			{
				base[this.tableCalendarExceptions.Shift2StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A40 RID: 2624 RVA: 0x0002273B File Offset: 0x0002093B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift2FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift2FinishColumn);
			}

			// Token: 0x06000A41 RID: 2625 RVA: 0x0002274E File Offset: 0x0002094E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift2FinishNull()
			{
				base[this.tableCalendarExceptions.Shift2FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A42 RID: 2626 RVA: 0x00022766 File Offset: 0x00020966
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift3StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift3StartColumn);
			}

			// Token: 0x06000A43 RID: 2627 RVA: 0x00022779 File Offset: 0x00020979
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift3StartNull()
			{
				base[this.tableCalendarExceptions.Shift3StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A44 RID: 2628 RVA: 0x00022791 File Offset: 0x00020991
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift3FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift3FinishColumn);
			}

			// Token: 0x06000A45 RID: 2629 RVA: 0x000227A4 File Offset: 0x000209A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift3FinishNull()
			{
				base[this.tableCalendarExceptions.Shift3FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A46 RID: 2630 RVA: 0x000227BC File Offset: 0x000209BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift4StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift4StartColumn);
			}

			// Token: 0x06000A47 RID: 2631 RVA: 0x000227CF File Offset: 0x000209CF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift4StartNull()
			{
				base[this.tableCalendarExceptions.Shift4StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A48 RID: 2632 RVA: 0x000227E7 File Offset: 0x000209E7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift4FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift4FinishColumn);
			}

			// Token: 0x06000A49 RID: 2633 RVA: 0x000227FA File Offset: 0x000209FA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetShift4FinishNull()
			{
				base[this.tableCalendarExceptions.Shift4FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A4A RID: 2634 RVA: 0x00022812 File Offset: 0x00020A12
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift5StartNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift5StartColumn);
			}

			// Token: 0x06000A4B RID: 2635 RVA: 0x00022825 File Offset: 0x00020A25
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift5StartNull()
			{
				base[this.tableCalendarExceptions.Shift5StartColumn] = Convert.DBNull;
			}

			// Token: 0x06000A4C RID: 2636 RVA: 0x0002283D File Offset: 0x00020A3D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsShift5FinishNull()
			{
				return base.IsNull(this.tableCalendarExceptions.Shift5FinishColumn);
			}

			// Token: 0x06000A4D RID: 2637 RVA: 0x00022850 File Offset: 0x00020A50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetShift5FinishNull()
			{
				base[this.tableCalendarExceptions.Shift5FinishColumn] = Convert.DBNull;
			}

			// Token: 0x06000A4E RID: 2638 RVA: 0x00022868 File Offset: 0x00020A68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrenceTypeNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceTypeColumn);
			}

			// Token: 0x06000A4F RID: 2639 RVA: 0x0002287B File Offset: 0x00020A7B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrenceTypeNull()
			{
				base[this.tableCalendarExceptions.RecurrenceTypeColumn] = Convert.DBNull;
			}

			// Token: 0x06000A50 RID: 2640 RVA: 0x00022893 File Offset: 0x00020A93
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrenceFrequencyNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceFrequencyColumn);
			}

			// Token: 0x06000A51 RID: 2641 RVA: 0x000228A6 File Offset: 0x00020AA6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrenceFrequencyNull()
			{
				base[this.tableCalendarExceptions.RecurrenceFrequencyColumn] = Convert.DBNull;
			}

			// Token: 0x06000A52 RID: 2642 RVA: 0x000228BE File Offset: 0x00020ABE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrenceDaysNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceDaysColumn);
			}

			// Token: 0x06000A53 RID: 2643 RVA: 0x000228D1 File Offset: 0x00020AD1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrenceDaysNull()
			{
				base[this.tableCalendarExceptions.RecurrenceDaysColumn] = Convert.DBNull;
			}

			// Token: 0x06000A54 RID: 2644 RVA: 0x000228E9 File Offset: 0x00020AE9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrenceMonthDayNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceMonthDayColumn);
			}

			// Token: 0x06000A55 RID: 2645 RVA: 0x000228FC File Offset: 0x00020AFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrenceMonthDayNull()
			{
				base[this.tableCalendarExceptions.RecurrenceMonthDayColumn] = Convert.DBNull;
			}

			// Token: 0x06000A56 RID: 2646 RVA: 0x00022914 File Offset: 0x00020B14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrenceMonthNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrenceMonthColumn);
			}

			// Token: 0x06000A57 RID: 2647 RVA: 0x00022927 File Offset: 0x00020B27
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRecurrenceMonthNull()
			{
				base[this.tableCalendarExceptions.RecurrenceMonthColumn] = Convert.DBNull;
			}

			// Token: 0x06000A58 RID: 2648 RVA: 0x0002293F File Offset: 0x00020B3F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsRecurrencePositionNull()
			{
				return base.IsNull(this.tableCalendarExceptions.RecurrencePositionColumn);
			}

			// Token: 0x06000A59 RID: 2649 RVA: 0x00022952 File Offset: 0x00020B52
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetRecurrencePositionNull()
			{
				base[this.tableCalendarExceptions.RecurrencePositionColumn] = Convert.DBNull;
			}

			// Token: 0x04000279 RID: 633
			private CalendarDataSet.CalendarExceptionsDataTable tableCalendarExceptions;
		}

		// Token: 0x0200008C RID: 140
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CalendarsRowChangeEvent : EventArgs
		{
			// Token: 0x06000A5A RID: 2650 RVA: 0x0002296A File Offset: 0x00020B6A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarsRowChangeEvent(CalendarDataSet.CalendarsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17000319 RID: 793
			// (get) Token: 0x06000A5B RID: 2651 RVA: 0x00022980 File Offset: 0x00020B80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CalendarDataSet.CalendarsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700031A RID: 794
			// (get) Token: 0x06000A5C RID: 2652 RVA: 0x00022988 File Offset: 0x00020B88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400027A RID: 634
			private CalendarDataSet.CalendarsRow eventRow;

			// Token: 0x0400027B RID: 635
			private DataRowAction eventAction;
		}

		// Token: 0x0200008D RID: 141
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CalendarExceptionsRowChangeEvent : EventArgs
		{
			// Token: 0x06000A5D RID: 2653 RVA: 0x00022990 File Offset: 0x00020B90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarExceptionsRowChangeEvent(CalendarDataSet.CalendarExceptionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700031B RID: 795
			// (get) Token: 0x06000A5E RID: 2654 RVA: 0x000229A6 File Offset: 0x00020BA6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public CalendarDataSet.CalendarExceptionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700031C RID: 796
			// (get) Token: 0x06000A5F RID: 2655 RVA: 0x000229AE File Offset: 0x00020BAE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x0400027C RID: 636
			private CalendarDataSet.CalendarExceptionsRow eventRow;

			// Token: 0x0400027D RID: 637
			private DataRowAction eventAction;
		}
	}
}
