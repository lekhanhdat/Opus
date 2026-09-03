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
	// Token: 0x02000764 RID: 1892
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[XmlRoot("TimeSheetSettingsDataSet")]
	[Serializable]
	public class TimeSheetSettingsDataSet : DataSet
	{
		// Token: 0x0600B671 RID: 46705 RVA: 0x00238E18 File Offset: 0x00237018
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.TimeSheetSettings, new string[]
			{
				"WADMIN_DEFAULT_TRACKING_METHOD",
				"WADMIN_TS_REPORT_UNIT_ENUM",
				"WADMIN_TS_IS_UNVERS_TASK_ALLOWED",
				"WADMIN_IS_TRACKING_METHOD_LOCKED",
				"WADMIN_TS_PROJECT_MANAGER_APPROVAL",
				"WADMIN_UIDFAKE",
				"WADMIN_TS_FIXED_APPROVAL_ROUTING",
				"WADMIN_TS_TIED_MODE",
				"WADMIN_TS_DEF_DISPLAY_ENUM",
				"WADMIN_TS_IS_FUTURE_REP_ALLOWED",
				"WADMIN_TS_MAX_HR_PER_DAY",
				"WADMIN_TS_MAX_HR_PER_TS",
				"WADMIN_TS_HOURS_PER_WEEK",
				"WADMIN_TS_HOURS_PER_DAY",
				"WADMIN_TS_CREATE_MODE_ENUM",
				"WADMIN_TS_DEF_ENTRY_MODE_ENUM",
				"WADMIN_TS_ALLOW_PROJECT_LEVEL",
				"WADMIN_TS_PROJECT_MANAGER_COORDINATION",
				"WADMIN_TS_MIN_HR_PER_TS",
				"WADMIN_TS_IS_AUDIT_ENABLED"
			});
		}

		// Token: 0x0600B672 RID: 46706 RVA: 0x00238EEC File Offset: 0x002370EC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public TimeSheetSettingsDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600B673 RID: 46707 RVA: 0x00238F40 File Offset: 0x00237140
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected TimeSheetSettingsDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["TimeSheetSettings"] != null)
				{
					base.Tables.Add(new TimeSheetSettingsDataSet.TimeSheetSettingsDataTable(dataSet.Tables["TimeSheetSettings"]));
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

		// Token: 0x170037A1 RID: 14241
		// (get) Token: 0x0600B674 RID: 46708 RVA: 0x0023909D File Offset: 0x0023729D
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public TimeSheetSettingsDataSet.TimeSheetSettingsDataTable TimeSheetSettings
		{
			get
			{
				return this.tableTimeSheetSettings;
			}
		}

		// Token: 0x170037A2 RID: 14242
		// (get) Token: 0x0600B675 RID: 46709 RVA: 0x002390A5 File Offset: 0x002372A5
		// (set) Token: 0x0600B676 RID: 46710 RVA: 0x002390AD File Offset: 0x002372AD
		[DebuggerNonUserCode]
		[Browsable(true)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

		// Token: 0x170037A3 RID: 14243
		// (get) Token: 0x0600B677 RID: 46711 RVA: 0x002390B6 File Offset: 0x002372B6
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataTableCollection Tables
		{
			get
			{
				return base.Tables;
			}
		}

		// Token: 0x170037A4 RID: 14244
		// (get) Token: 0x0600B678 RID: 46712 RVA: 0x002390BE File Offset: 0x002372BE
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

		// Token: 0x0600B679 RID: 46713 RVA: 0x002390C6 File Offset: 0x002372C6
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600B67A RID: 46714 RVA: 0x002390DC File Offset: 0x002372DC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			TimeSheetSettingsDataSet timeSheetSettingsDataSet = (TimeSheetSettingsDataSet)base.Clone();
			timeSheetSettingsDataSet.InitVars();
			timeSheetSettingsDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return timeSheetSettingsDataSet;
		}

		// Token: 0x0600B67B RID: 46715 RVA: 0x00239108 File Offset: 0x00237308
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600B67C RID: 46716 RVA: 0x0023910B File Offset: 0x0023730B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600B67D RID: 46717 RVA: 0x00239110 File Offset: 0x00237310
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["TimeSheetSettings"] != null)
				{
					base.Tables.Add(new TimeSheetSettingsDataSet.TimeSheetSettingsDataTable(dataSet.Tables["TimeSheetSettings"]));
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

		// Token: 0x0600B67E RID: 46718 RVA: 0x002391D8 File Offset: 0x002373D8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600B67F RID: 46719 RVA: 0x0023920C File Offset: 0x0023740C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600B680 RID: 46720 RVA: 0x00239215 File Offset: 0x00237415
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableTimeSheetSettings = (TimeSheetSettingsDataSet.TimeSheetSettingsDataTable)base.Tables["TimeSheetSettings"];
			if (initTable && this.tableTimeSheetSettings != null)
			{
				this.tableTimeSheetSettings.InitVars();
			}
		}

		// Token: 0x0600B681 RID: 46721 RVA: 0x00239248 File Offset: 0x00237448
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "TimeSheetSettingsDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/TimeSheetSettingsDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableTimeSheetSettings = new TimeSheetSettingsDataSet.TimeSheetSettingsDataTable();
			base.Tables.Add(this.tableTimeSheetSettings);
		}

		// Token: 0x0600B682 RID: 46722 RVA: 0x002392A0 File Offset: 0x002374A0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeTimeSheetSettings()
		{
			return false;
		}

		// Token: 0x0600B683 RID: 46723 RVA: 0x002392A3 File Offset: 0x002374A3
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600B684 RID: 46724 RVA: 0x002392B4 File Offset: 0x002374B4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			TimeSheetSettingsDataSet timeSheetSettingsDataSet = new TimeSheetSettingsDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = timeSheetSettingsDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = timeSheetSettingsDataSet.GetSchemaSerializable();
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

		// Token: 0x040024C5 RID: 9413
		private TimeSheetSettingsDataSet.TimeSheetSettingsDataTable tableTimeSheetSettings;

		// Token: 0x040024C6 RID: 9414
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000765 RID: 1893
		// (Invoke) Token: 0x0600B686 RID: 46726
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void TimeSheetSettingsRowChangeEventHandler(object sender, TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEvent e);

		// Token: 0x02000766 RID: 1894
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class TimeSheetSettingsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B689 RID: 46729 RVA: 0x002393FC File Offset: 0x002375FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimeSheetSettingsDataTable()
			{
				base.TableName = "TimeSheetSettings";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B68A RID: 46730 RVA: 0x00239424 File Offset: 0x00237624
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal TimeSheetSettingsDataTable(DataTable table)
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

			// Token: 0x0600B68B RID: 46731 RVA: 0x002394CC File Offset: 0x002376CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected TimeSheetSettingsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170037A5 RID: 14245
			// (get) Token: 0x0600B68C RID: 46732 RVA: 0x002394DC File Offset: 0x002376DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_UIDFAKEColumn
			{
				get
				{
					return this.columnWADMIN_UIDFAKE;
				}
			}

			// Token: 0x170037A6 RID: 14246
			// (get) Token: 0x0600B68D RID: 46733 RVA: 0x002394E4 File Offset: 0x002376E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED;
				}
			}

			// Token: 0x170037A7 RID: 14247
			// (get) Token: 0x0600B68E RID: 46734 RVA: 0x002394EC File Offset: 0x002376EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn
			{
				get
				{
					return this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION;
				}
			}

			// Token: 0x170037A8 RID: 14248
			// (get) Token: 0x0600B68F RID: 46735 RVA: 0x002394F4 File Offset: 0x002376F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_PROJECT_MANAGER_APPROVALColumn
			{
				get
				{
					return this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL;
				}
			}

			// Token: 0x170037A9 RID: 14249
			// (get) Token: 0x0600B690 RID: 46736 RVA: 0x002394FC File Offset: 0x002376FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_IS_AUDIT_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_AUDIT_ENABLED;
				}
			}

			// Token: 0x170037AA RID: 14250
			// (get) Token: 0x0600B691 RID: 46737 RVA: 0x00239504 File Offset: 0x00237704
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_FUTURE_REP_ALLOWED;
				}
			}

			// Token: 0x170037AB RID: 14251
			// (get) Token: 0x0600B692 RID: 46738 RVA: 0x0023950C File Offset: 0x0023770C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn
			{
				get
				{
					return this.columnWADMIN_TS_FIXED_APPROVAL_ROUTING;
				}
			}

			// Token: 0x170037AC RID: 14252
			// (get) Token: 0x0600B693 RID: 46739 RVA: 0x00239514 File Offset: 0x00237714
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_TIED_MODEColumn
			{
				get
				{
					return this.columnWADMIN_TS_TIED_MODE;
				}
			}

			// Token: 0x170037AD RID: 14253
			// (get) Token: 0x0600B694 RID: 46740 RVA: 0x0023951C File Offset: 0x0023771C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MIN_HR_PER_TSColumn
			{
				get
				{
					return this.columnWADMIN_TS_MIN_HR_PER_TS;
				}
			}

			// Token: 0x170037AE RID: 14254
			// (get) Token: 0x0600B695 RID: 46741 RVA: 0x00239524 File Offset: 0x00237724
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MAX_HR_PER_TSColumn
			{
				get
				{
					return this.columnWADMIN_TS_MAX_HR_PER_TS;
				}
			}

			// Token: 0x170037AF RID: 14255
			// (get) Token: 0x0600B696 RID: 46742 RVA: 0x0023952C File Offset: 0x0023772C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MAX_HR_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_TS_MAX_HR_PER_DAY;
				}
			}

			// Token: 0x170037B0 RID: 14256
			// (get) Token: 0x0600B697 RID: 46743 RVA: 0x00239534 File Offset: 0x00237734
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_HOURS_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_TS_HOURS_PER_DAY;
				}
			}

			// Token: 0x170037B1 RID: 14257
			// (get) Token: 0x0600B698 RID: 46744 RVA: 0x0023953C File Offset: 0x0023773C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_HOURS_PER_WEEKColumn
			{
				get
				{
					return this.columnWADMIN_TS_HOURS_PER_WEEK;
				}
			}

			// Token: 0x170037B2 RID: 14258
			// (get) Token: 0x0600B699 RID: 46745 RVA: 0x00239544 File Offset: 0x00237744
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_DEF_DISPLAY_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_DEF_DISPLAY_ENUM;
				}
			}

			// Token: 0x170037B3 RID: 14259
			// (get) Token: 0x0600B69A RID: 46746 RVA: 0x0023954C File Offset: 0x0023774C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_CREATE_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_CREATE_MODE_ENUM;
				}
			}

			// Token: 0x170037B4 RID: 14260
			// (get) Token: 0x0600B69B RID: 46747 RVA: 0x00239554 File Offset: 0x00237754
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_REPORT_UNIT_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_REPORT_UNIT_ENUM;
				}
			}

			// Token: 0x170037B5 RID: 14261
			// (get) Token: 0x0600B69C RID: 46748 RVA: 0x0023955C File Offset: 0x0023775C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM;
				}
			}

			// Token: 0x170037B6 RID: 14262
			// (get) Token: 0x0600B69D RID: 46749 RVA: 0x00239564 File Offset: 0x00237764
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_DEFAULT_TRACKING_METHODColumn
			{
				get
				{
					return this.columnWADMIN_DEFAULT_TRACKING_METHOD;
				}
			}

			// Token: 0x170037B7 RID: 14263
			// (get) Token: 0x0600B69E RID: 46750 RVA: 0x0023956C File Offset: 0x0023776C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_IS_TRACKING_METHOD_LOCKEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_TRACKING_METHOD_LOCKED;
				}
			}

			// Token: 0x170037B8 RID: 14264
			// (get) Token: 0x0600B69F RID: 46751 RVA: 0x00239574 File Offset: 0x00237774
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_ALLOW_PROJECT_LEVELColumn
			{
				get
				{
					return this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL;
				}
			}

			// Token: 0x170037B9 RID: 14265
			// (get) Token: 0x0600B6A0 RID: 46752 RVA: 0x0023957C File Offset: 0x0023777C
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

			// Token: 0x170037BA RID: 14266
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimeSheetSettingsDataSet.TimeSheetSettingsRow this[int index]
			{
				get
				{
					return (TimeSheetSettingsDataSet.TimeSheetSettingsRow)base.Rows[index];
				}
			}

			// Token: 0x14000679 RID: 1657
			// (add) Token: 0x0600B6A2 RID: 46754 RVA: 0x0023959C File Offset: 0x0023779C
			// (remove) Token: 0x0600B6A3 RID: 46755 RVA: 0x002395D4 File Offset: 0x002377D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEventHandler TimeSheetSettingsRowChanging;

			// Token: 0x1400067A RID: 1658
			// (add) Token: 0x0600B6A4 RID: 46756 RVA: 0x0023960C File Offset: 0x0023780C
			// (remove) Token: 0x0600B6A5 RID: 46757 RVA: 0x00239644 File Offset: 0x00237844
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEventHandler TimeSheetSettingsRowChanged;

			// Token: 0x1400067B RID: 1659
			// (add) Token: 0x0600B6A6 RID: 46758 RVA: 0x0023967C File Offset: 0x0023787C
			// (remove) Token: 0x0600B6A7 RID: 46759 RVA: 0x002396B4 File Offset: 0x002378B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEventHandler TimeSheetSettingsRowDeleting;

			// Token: 0x1400067C RID: 1660
			// (add) Token: 0x0600B6A8 RID: 46760 RVA: 0x002396EC File Offset: 0x002378EC
			// (remove) Token: 0x0600B6A9 RID: 46761 RVA: 0x00239724 File Offset: 0x00237924
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEventHandler TimeSheetSettingsRowDeleted;

			// Token: 0x0600B6AA RID: 46762 RVA: 0x00239759 File Offset: 0x00237959
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddTimeSheetSettingsRow(TimeSheetSettingsDataSet.TimeSheetSettingsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B6AB RID: 46763 RVA: 0x00239768 File Offset: 0x00237968
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimeSheetSettingsDataSet.TimeSheetSettingsRow AddTimeSheetSettingsRow(Guid WADMIN_UIDFAKE, bool WADMIN_TS_IS_UNVERS_TASK_ALLOWED, bool WADMIN_TS_PROJECT_MANAGER_COORDINATION, bool WADMIN_TS_PROJECT_MANAGER_APPROVAL, bool WADMIN_TS_IS_AUDIT_ENABLED, bool WADMIN_TS_IS_FUTURE_REP_ALLOWED, bool WADMIN_TS_FIXED_APPROVAL_ROUTING, bool WADMIN_TS_TIED_MODE, decimal WADMIN_TS_MIN_HR_PER_TS, decimal WADMIN_TS_MAX_HR_PER_TS, decimal WADMIN_TS_MAX_HR_PER_DAY, decimal WADMIN_TS_HOURS_PER_DAY, decimal WADMIN_TS_HOURS_PER_WEEK, byte WADMIN_TS_DEF_DISPLAY_ENUM, byte WADMIN_TS_CREATE_MODE_ENUM, byte WADMIN_TS_REPORT_UNIT_ENUM, byte WADMIN_TS_DEF_ENTRY_MODE_ENUM, int WADMIN_DEFAULT_TRACKING_METHOD, bool WADMIN_IS_TRACKING_METHOD_LOCKED, bool WADMIN_TS_ALLOW_PROJECT_LEVEL)
			{
				TimeSheetSettingsDataSet.TimeSheetSettingsRow timeSheetSettingsRow = (TimeSheetSettingsDataSet.TimeSheetSettingsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WADMIN_UIDFAKE,
					WADMIN_TS_IS_UNVERS_TASK_ALLOWED,
					WADMIN_TS_PROJECT_MANAGER_COORDINATION,
					WADMIN_TS_PROJECT_MANAGER_APPROVAL,
					WADMIN_TS_IS_AUDIT_ENABLED,
					WADMIN_TS_IS_FUTURE_REP_ALLOWED,
					WADMIN_TS_FIXED_APPROVAL_ROUTING,
					WADMIN_TS_TIED_MODE,
					WADMIN_TS_MIN_HR_PER_TS,
					WADMIN_TS_MAX_HR_PER_TS,
					WADMIN_TS_MAX_HR_PER_DAY,
					WADMIN_TS_HOURS_PER_DAY,
					WADMIN_TS_HOURS_PER_WEEK,
					WADMIN_TS_DEF_DISPLAY_ENUM,
					WADMIN_TS_CREATE_MODE_ENUM,
					WADMIN_TS_REPORT_UNIT_ENUM,
					WADMIN_TS_DEF_ENTRY_MODE_ENUM,
					WADMIN_DEFAULT_TRACKING_METHOD,
					WADMIN_IS_TRACKING_METHOD_LOCKED,
					WADMIN_TS_ALLOW_PROJECT_LEVEL
				};
				timeSheetSettingsRow.ItemArray = itemArray;
				base.Rows.Add(timeSheetSettingsRow);
				return timeSheetSettingsRow;
			}

			// Token: 0x0600B6AC RID: 46764 RVA: 0x0023986F File Offset: 0x00237A6F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B6AD RID: 46765 RVA: 0x0023987C File Offset: 0x00237A7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				TimeSheetSettingsDataSet.TimeSheetSettingsDataTable timeSheetSettingsDataTable = (TimeSheetSettingsDataSet.TimeSheetSettingsDataTable)base.Clone();
				timeSheetSettingsDataTable.InitVars();
				return timeSheetSettingsDataTable;
			}

			// Token: 0x0600B6AE RID: 46766 RVA: 0x0023989C File Offset: 0x00237A9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new TimeSheetSettingsDataSet.TimeSheetSettingsDataTable();
			}

			// Token: 0x0600B6AF RID: 46767 RVA: 0x002398A4 File Offset: 0x00237AA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWADMIN_UIDFAKE = base.Columns["WADMIN_UIDFAKE"];
				this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED = base.Columns["WADMIN_TS_IS_UNVERS_TASK_ALLOWED"];
				this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION = base.Columns["WADMIN_TS_PROJECT_MANAGER_COORDINATION"];
				this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL = base.Columns["WADMIN_TS_PROJECT_MANAGER_APPROVAL"];
				this.columnWADMIN_TS_IS_AUDIT_ENABLED = base.Columns["WADMIN_TS_IS_AUDIT_ENABLED"];
				this.columnWADMIN_TS_IS_FUTURE_REP_ALLOWED = base.Columns["WADMIN_TS_IS_FUTURE_REP_ALLOWED"];
				this.columnWADMIN_TS_FIXED_APPROVAL_ROUTING = base.Columns["WADMIN_TS_FIXED_APPROVAL_ROUTING"];
				this.columnWADMIN_TS_TIED_MODE = base.Columns["WADMIN_TS_TIED_MODE"];
				this.columnWADMIN_TS_MIN_HR_PER_TS = base.Columns["WADMIN_TS_MIN_HR_PER_TS"];
				this.columnWADMIN_TS_MAX_HR_PER_TS = base.Columns["WADMIN_TS_MAX_HR_PER_TS"];
				this.columnWADMIN_TS_MAX_HR_PER_DAY = base.Columns["WADMIN_TS_MAX_HR_PER_DAY"];
				this.columnWADMIN_TS_HOURS_PER_DAY = base.Columns["WADMIN_TS_HOURS_PER_DAY"];
				this.columnWADMIN_TS_HOURS_PER_WEEK = base.Columns["WADMIN_TS_HOURS_PER_WEEK"];
				this.columnWADMIN_TS_DEF_DISPLAY_ENUM = base.Columns["WADMIN_TS_DEF_DISPLAY_ENUM"];
				this.columnWADMIN_TS_CREATE_MODE_ENUM = base.Columns["WADMIN_TS_CREATE_MODE_ENUM"];
				this.columnWADMIN_TS_REPORT_UNIT_ENUM = base.Columns["WADMIN_TS_REPORT_UNIT_ENUM"];
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM = base.Columns["WADMIN_TS_DEF_ENTRY_MODE_ENUM"];
				this.columnWADMIN_DEFAULT_TRACKING_METHOD = base.Columns["WADMIN_DEFAULT_TRACKING_METHOD"];
				this.columnWADMIN_IS_TRACKING_METHOD_LOCKED = base.Columns["WADMIN_IS_TRACKING_METHOD_LOCKED"];
				this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL = base.Columns["WADMIN_TS_ALLOW_PROJECT_LEVEL"];
			}

			// Token: 0x0600B6B0 RID: 46768 RVA: 0x00239A6C File Offset: 0x00237C6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWADMIN_UIDFAKE = new DataColumn("WADMIN_UIDFAKE", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_UIDFAKE);
				this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED = new DataColumn("WADMIN_TS_IS_UNVERS_TASK_ALLOWED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED);
				this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION = new DataColumn("WADMIN_TS_PROJECT_MANAGER_COORDINATION", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION);
				this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL = new DataColumn("WADMIN_TS_PROJECT_MANAGER_APPROVAL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL);
				this.columnWADMIN_TS_IS_AUDIT_ENABLED = new DataColumn("WADMIN_TS_IS_AUDIT_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_IS_AUDIT_ENABLED);
				this.columnWADMIN_TS_IS_FUTURE_REP_ALLOWED = new DataColumn("WADMIN_TS_IS_FUTURE_REP_ALLOWED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_IS_FUTURE_REP_ALLOWED);
				this.columnWADMIN_TS_FIXED_APPROVAL_ROUTING = new DataColumn("WADMIN_TS_FIXED_APPROVAL_ROUTING", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_FIXED_APPROVAL_ROUTING);
				this.columnWADMIN_TS_TIED_MODE = new DataColumn("WADMIN_TS_TIED_MODE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_TIED_MODE);
				this.columnWADMIN_TS_MIN_HR_PER_TS = new DataColumn("WADMIN_TS_MIN_HR_PER_TS", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_MIN_HR_PER_TS);
				this.columnWADMIN_TS_MAX_HR_PER_TS = new DataColumn("WADMIN_TS_MAX_HR_PER_TS", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_MAX_HR_PER_TS);
				this.columnWADMIN_TS_MAX_HR_PER_DAY = new DataColumn("WADMIN_TS_MAX_HR_PER_DAY", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_MAX_HR_PER_DAY);
				this.columnWADMIN_TS_HOURS_PER_DAY = new DataColumn("WADMIN_TS_HOURS_PER_DAY", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_HOURS_PER_DAY);
				this.columnWADMIN_TS_HOURS_PER_WEEK = new DataColumn("WADMIN_TS_HOURS_PER_WEEK", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_HOURS_PER_WEEK);
				this.columnWADMIN_TS_DEF_DISPLAY_ENUM = new DataColumn("WADMIN_TS_DEF_DISPLAY_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_DEF_DISPLAY_ENUM);
				this.columnWADMIN_TS_CREATE_MODE_ENUM = new DataColumn("WADMIN_TS_CREATE_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_CREATE_MODE_ENUM);
				this.columnWADMIN_TS_REPORT_UNIT_ENUM = new DataColumn("WADMIN_TS_REPORT_UNIT_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_REPORT_UNIT_ENUM);
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM = new DataColumn("WADMIN_TS_DEF_ENTRY_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM);
				this.columnWADMIN_DEFAULT_TRACKING_METHOD = new DataColumn("WADMIN_DEFAULT_TRACKING_METHOD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DEFAULT_TRACKING_METHOD);
				this.columnWADMIN_IS_TRACKING_METHOD_LOCKED = new DataColumn("WADMIN_IS_TRACKING_METHOD_LOCKED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_TRACKING_METHOD_LOCKED);
				this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL = new DataColumn("WADMIN_TS_ALLOW_PROJECT_LEVEL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL);
				this.columnWADMIN_UIDFAKE.AllowDBNull = false;
				this.columnWADMIN_TS_MIN_HR_PER_TS.AllowDBNull = false;
				this.columnWADMIN_TS_MAX_HR_PER_TS.AllowDBNull = false;
				this.columnWADMIN_TS_MAX_HR_PER_DAY.AllowDBNull = false;
				this.columnWADMIN_TS_HOURS_PER_DAY.AllowDBNull = false;
				this.columnWADMIN_TS_HOURS_PER_WEEK.AllowDBNull = false;
				this.columnWADMIN_TS_DEF_DISPLAY_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_CREATE_MODE_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_REPORT_UNIT_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM.AllowDBNull = false;
				this.columnWADMIN_DEFAULT_TRACKING_METHOD.AllowDBNull = false;
			}

			// Token: 0x0600B6B1 RID: 46769 RVA: 0x00239E81 File Offset: 0x00238081
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimeSheetSettingsDataSet.TimeSheetSettingsRow NewTimeSheetSettingsRow()
			{
				return (TimeSheetSettingsDataSet.TimeSheetSettingsRow)base.NewRow();
			}

			// Token: 0x0600B6B2 RID: 46770 RVA: 0x00239E8E File Offset: 0x0023808E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new TimeSheetSettingsDataSet.TimeSheetSettingsRow(builder);
			}

			// Token: 0x0600B6B3 RID: 46771 RVA: 0x00239E96 File Offset: 0x00238096
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(TimeSheetSettingsDataSet.TimeSheetSettingsRow);
			}

			// Token: 0x0600B6B4 RID: 46772 RVA: 0x00239EA2 File Offset: 0x002380A2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.TimeSheetSettingsRowChanged != null)
				{
					this.TimeSheetSettingsRowChanged(this, new TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEvent((TimeSheetSettingsDataSet.TimeSheetSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B6B5 RID: 46773 RVA: 0x00239ED5 File Offset: 0x002380D5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.TimeSheetSettingsRowChanging != null)
				{
					this.TimeSheetSettingsRowChanging(this, new TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEvent((TimeSheetSettingsDataSet.TimeSheetSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B6B6 RID: 46774 RVA: 0x00239F08 File Offset: 0x00238108
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.TimeSheetSettingsRowDeleted != null)
				{
					this.TimeSheetSettingsRowDeleted(this, new TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEvent((TimeSheetSettingsDataSet.TimeSheetSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B6B7 RID: 46775 RVA: 0x00239F3B File Offset: 0x0023813B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.TimeSheetSettingsRowDeleting != null)
				{
					this.TimeSheetSettingsRowDeleting(this, new TimeSheetSettingsDataSet.TimeSheetSettingsRowChangeEvent((TimeSheetSettingsDataSet.TimeSheetSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B6B8 RID: 46776 RVA: 0x00239F6E File Offset: 0x0023816E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveTimeSheetSettingsRow(TimeSheetSettingsDataSet.TimeSheetSettingsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B6B9 RID: 46777 RVA: 0x00239F7C File Offset: 0x0023817C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				TimeSheetSettingsDataSet timeSheetSettingsDataSet = new TimeSheetSettingsDataSet();
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
				xmlSchemaAttribute.FixedValue = timeSheetSettingsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "TimeSheetSettingsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = timeSheetSettingsDataSet.GetSchemaSerializable();
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

			// Token: 0x040024C7 RID: 9415
			private DataColumn columnWADMIN_UIDFAKE;

			// Token: 0x040024C8 RID: 9416
			private DataColumn columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED;

			// Token: 0x040024C9 RID: 9417
			private DataColumn columnWADMIN_TS_PROJECT_MANAGER_COORDINATION;

			// Token: 0x040024CA RID: 9418
			private DataColumn columnWADMIN_TS_PROJECT_MANAGER_APPROVAL;

			// Token: 0x040024CB RID: 9419
			private DataColumn columnWADMIN_TS_IS_AUDIT_ENABLED;

			// Token: 0x040024CC RID: 9420
			private DataColumn columnWADMIN_TS_IS_FUTURE_REP_ALLOWED;

			// Token: 0x040024CD RID: 9421
			private DataColumn columnWADMIN_TS_FIXED_APPROVAL_ROUTING;

			// Token: 0x040024CE RID: 9422
			private DataColumn columnWADMIN_TS_TIED_MODE;

			// Token: 0x040024CF RID: 9423
			private DataColumn columnWADMIN_TS_MIN_HR_PER_TS;

			// Token: 0x040024D0 RID: 9424
			private DataColumn columnWADMIN_TS_MAX_HR_PER_TS;

			// Token: 0x040024D1 RID: 9425
			private DataColumn columnWADMIN_TS_MAX_HR_PER_DAY;

			// Token: 0x040024D2 RID: 9426
			private DataColumn columnWADMIN_TS_HOURS_PER_DAY;

			// Token: 0x040024D3 RID: 9427
			private DataColumn columnWADMIN_TS_HOURS_PER_WEEK;

			// Token: 0x040024D4 RID: 9428
			private DataColumn columnWADMIN_TS_DEF_DISPLAY_ENUM;

			// Token: 0x040024D5 RID: 9429
			private DataColumn columnWADMIN_TS_CREATE_MODE_ENUM;

			// Token: 0x040024D6 RID: 9430
			private DataColumn columnWADMIN_TS_REPORT_UNIT_ENUM;

			// Token: 0x040024D7 RID: 9431
			private DataColumn columnWADMIN_TS_DEF_ENTRY_MODE_ENUM;

			// Token: 0x040024D8 RID: 9432
			private DataColumn columnWADMIN_DEFAULT_TRACKING_METHOD;

			// Token: 0x040024D9 RID: 9433
			private DataColumn columnWADMIN_IS_TRACKING_METHOD_LOCKED;

			// Token: 0x040024DA RID: 9434
			private DataColumn columnWADMIN_TS_ALLOW_PROJECT_LEVEL;
		}

		// Token: 0x02000767 RID: 1895
		public class TimeSheetSettingsRow : DataRow
		{
			// Token: 0x0600B6BA RID: 46778 RVA: 0x0023A174 File Offset: 0x00238374
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal TimeSheetSettingsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableTimeSheetSettings = (TimeSheetSettingsDataSet.TimeSheetSettingsDataTable)base.Table;
			}

			// Token: 0x170037BB RID: 14267
			// (get) Token: 0x0600B6BB RID: 46779 RVA: 0x0023A18E File Offset: 0x0023838E
			// (set) Token: 0x0600B6BC RID: 46780 RVA: 0x0023A1A6 File Offset: 0x002383A6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_UIDFAKE
			{
				get
				{
					return (Guid)base[this.tableTimeSheetSettings.WADMIN_UIDFAKEColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_UIDFAKEColumn] = value;
				}
			}

			// Token: 0x170037BC RID: 14268
			// (get) Token: 0x0600B6BD RID: 46781 RVA: 0x0023A1C0 File Offset: 0x002383C0
			// (set) Token: 0x0600B6BE RID: 46782 RVA: 0x0023A204 File Offset: 0x00238404
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_IS_UNVERS_TASK_ALLOWED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_UNVERS_TASK_ALLOWED' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn] = value;
				}
			}

			// Token: 0x170037BD RID: 14269
			// (get) Token: 0x0600B6BF RID: 46783 RVA: 0x0023A220 File Offset: 0x00238420
			// (set) Token: 0x0600B6C0 RID: 46784 RVA: 0x0023A264 File Offset: 0x00238464
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_PROJECT_MANAGER_COORDINATION
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_PROJECT_MANAGER_COORDINATION' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn] = value;
				}
			}

			// Token: 0x170037BE RID: 14270
			// (get) Token: 0x0600B6C1 RID: 46785 RVA: 0x0023A280 File Offset: 0x00238480
			// (set) Token: 0x0600B6C2 RID: 46786 RVA: 0x0023A2C4 File Offset: 0x002384C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_PROJECT_MANAGER_APPROVAL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_PROJECT_MANAGER_APPROVAL' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn] = value;
				}
			}

			// Token: 0x170037BF RID: 14271
			// (get) Token: 0x0600B6C3 RID: 46787 RVA: 0x0023A2E0 File Offset: 0x002384E0
			// (set) Token: 0x0600B6C4 RID: 46788 RVA: 0x0023A324 File Offset: 0x00238524
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_IS_AUDIT_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_IS_AUDIT_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_AUDIT_ENABLED' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_IS_AUDIT_ENABLEDColumn] = value;
				}
			}

			// Token: 0x170037C0 RID: 14272
			// (get) Token: 0x0600B6C5 RID: 46789 RVA: 0x0023A340 File Offset: 0x00238540
			// (set) Token: 0x0600B6C6 RID: 46790 RVA: 0x0023A384 File Offset: 0x00238584
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_IS_FUTURE_REP_ALLOWED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_FUTURE_REP_ALLOWED' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn] = value;
				}
			}

			// Token: 0x170037C1 RID: 14273
			// (get) Token: 0x0600B6C7 RID: 46791 RVA: 0x0023A3A0 File Offset: 0x002385A0
			// (set) Token: 0x0600B6C8 RID: 46792 RVA: 0x0023A3E4 File Offset: 0x002385E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_FIXED_APPROVAL_ROUTING
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_FIXED_APPROVAL_ROUTING' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn] = value;
				}
			}

			// Token: 0x170037C2 RID: 14274
			// (get) Token: 0x0600B6C9 RID: 46793 RVA: 0x0023A400 File Offset: 0x00238600
			// (set) Token: 0x0600B6CA RID: 46794 RVA: 0x0023A444 File Offset: 0x00238644
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_TIED_MODE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_TIED_MODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_TIED_MODE' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_TIED_MODEColumn] = value;
				}
			}

			// Token: 0x170037C3 RID: 14275
			// (get) Token: 0x0600B6CB RID: 46795 RVA: 0x0023A45D File Offset: 0x0023865D
			// (set) Token: 0x0600B6CC RID: 46796 RVA: 0x0023A475 File Offset: 0x00238675
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_MIN_HR_PER_TS
			{
				get
				{
					return (decimal)base[this.tableTimeSheetSettings.WADMIN_TS_MIN_HR_PER_TSColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_MIN_HR_PER_TSColumn] = value;
				}
			}

			// Token: 0x170037C4 RID: 14276
			// (get) Token: 0x0600B6CD RID: 46797 RVA: 0x0023A48E File Offset: 0x0023868E
			// (set) Token: 0x0600B6CE RID: 46798 RVA: 0x0023A4A6 File Offset: 0x002386A6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_MAX_HR_PER_TS
			{
				get
				{
					return (decimal)base[this.tableTimeSheetSettings.WADMIN_TS_MAX_HR_PER_TSColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_MAX_HR_PER_TSColumn] = value;
				}
			}

			// Token: 0x170037C5 RID: 14277
			// (get) Token: 0x0600B6CF RID: 46799 RVA: 0x0023A4BF File Offset: 0x002386BF
			// (set) Token: 0x0600B6D0 RID: 46800 RVA: 0x0023A4D7 File Offset: 0x002386D7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_MAX_HR_PER_DAY
			{
				get
				{
					return (decimal)base[this.tableTimeSheetSettings.WADMIN_TS_MAX_HR_PER_DAYColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_MAX_HR_PER_DAYColumn] = value;
				}
			}

			// Token: 0x170037C6 RID: 14278
			// (get) Token: 0x0600B6D1 RID: 46801 RVA: 0x0023A4F0 File Offset: 0x002386F0
			// (set) Token: 0x0600B6D2 RID: 46802 RVA: 0x0023A508 File Offset: 0x00238708
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_HOURS_PER_DAY
			{
				get
				{
					return (decimal)base[this.tableTimeSheetSettings.WADMIN_TS_HOURS_PER_DAYColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_HOURS_PER_DAYColumn] = value;
				}
			}

			// Token: 0x170037C7 RID: 14279
			// (get) Token: 0x0600B6D3 RID: 46803 RVA: 0x0023A521 File Offset: 0x00238721
			// (set) Token: 0x0600B6D4 RID: 46804 RVA: 0x0023A539 File Offset: 0x00238739
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_HOURS_PER_WEEK
			{
				get
				{
					return (decimal)base[this.tableTimeSheetSettings.WADMIN_TS_HOURS_PER_WEEKColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_HOURS_PER_WEEKColumn] = value;
				}
			}

			// Token: 0x170037C8 RID: 14280
			// (get) Token: 0x0600B6D5 RID: 46805 RVA: 0x0023A552 File Offset: 0x00238752
			// (set) Token: 0x0600B6D6 RID: 46806 RVA: 0x0023A56A File Offset: 0x0023876A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_DEF_DISPLAY_ENUM
			{
				get
				{
					return (byte)base[this.tableTimeSheetSettings.WADMIN_TS_DEF_DISPLAY_ENUMColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_DEF_DISPLAY_ENUMColumn] = value;
				}
			}

			// Token: 0x170037C9 RID: 14281
			// (get) Token: 0x0600B6D7 RID: 46807 RVA: 0x0023A583 File Offset: 0x00238783
			// (set) Token: 0x0600B6D8 RID: 46808 RVA: 0x0023A59B File Offset: 0x0023879B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_CREATE_MODE_ENUM
			{
				get
				{
					return (byte)base[this.tableTimeSheetSettings.WADMIN_TS_CREATE_MODE_ENUMColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_CREATE_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x170037CA RID: 14282
			// (get) Token: 0x0600B6D9 RID: 46809 RVA: 0x0023A5B4 File Offset: 0x002387B4
			// (set) Token: 0x0600B6DA RID: 46810 RVA: 0x0023A5CC File Offset: 0x002387CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_REPORT_UNIT_ENUM
			{
				get
				{
					return (byte)base[this.tableTimeSheetSettings.WADMIN_TS_REPORT_UNIT_ENUMColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_REPORT_UNIT_ENUMColumn] = value;
				}
			}

			// Token: 0x170037CB RID: 14283
			// (get) Token: 0x0600B6DB RID: 46811 RVA: 0x0023A5E5 File Offset: 0x002387E5
			// (set) Token: 0x0600B6DC RID: 46812 RVA: 0x0023A5FD File Offset: 0x002387FD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_DEF_ENTRY_MODE_ENUM
			{
				get
				{
					return (byte)base[this.tableTimeSheetSettings.WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x170037CC RID: 14284
			// (get) Token: 0x0600B6DD RID: 46813 RVA: 0x0023A616 File Offset: 0x00238816
			// (set) Token: 0x0600B6DE RID: 46814 RVA: 0x0023A62E File Offset: 0x0023882E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_DEFAULT_TRACKING_METHOD
			{
				get
				{
					return (int)base[this.tableTimeSheetSettings.WADMIN_DEFAULT_TRACKING_METHODColumn];
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_DEFAULT_TRACKING_METHODColumn] = value;
				}
			}

			// Token: 0x170037CD RID: 14285
			// (get) Token: 0x0600B6DF RID: 46815 RVA: 0x0023A648 File Offset: 0x00238848
			// (set) Token: 0x0600B6E0 RID: 46816 RVA: 0x0023A68C File Offset: 0x0023888C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_IS_TRACKING_METHOD_LOCKED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_TRACKING_METHOD_LOCKED' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn] = value;
				}
			}

			// Token: 0x170037CE RID: 14286
			// (get) Token: 0x0600B6E1 RID: 46817 RVA: 0x0023A6A8 File Offset: 0x002388A8
			// (set) Token: 0x0600B6E2 RID: 46818 RVA: 0x0023A6EC File Offset: 0x002388EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_ALLOW_PROJECT_LEVEL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableTimeSheetSettings.WADMIN_TS_ALLOW_PROJECT_LEVELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_ALLOW_PROJECT_LEVEL' in table 'TimeSheetSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableTimeSheetSettings.WADMIN_TS_ALLOW_PROJECT_LEVELColumn] = value;
				}
			}

			// Token: 0x0600B6E3 RID: 46819 RVA: 0x0023A705 File Offset: 0x00238905
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_IS_UNVERS_TASK_ALLOWEDNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn);
			}

			// Token: 0x0600B6E4 RID: 46820 RVA: 0x0023A718 File Offset: 0x00238918
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_TS_IS_UNVERS_TASK_ALLOWEDNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6E5 RID: 46821 RVA: 0x0023A730 File Offset: 0x00238930
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_PROJECT_MANAGER_COORDINATIONNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn);
			}

			// Token: 0x0600B6E6 RID: 46822 RVA: 0x0023A743 File Offset: 0x00238943
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_PROJECT_MANAGER_COORDINATIONNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6E7 RID: 46823 RVA: 0x0023A75B File Offset: 0x0023895B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_PROJECT_MANAGER_APPROVALNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn);
			}

			// Token: 0x0600B6E8 RID: 46824 RVA: 0x0023A76E File Offset: 0x0023896E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_PROJECT_MANAGER_APPROVALNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6E9 RID: 46825 RVA: 0x0023A786 File Offset: 0x00238986
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_IS_AUDIT_ENABLEDNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_IS_AUDIT_ENABLEDColumn);
			}

			// Token: 0x0600B6EA RID: 46826 RVA: 0x0023A799 File Offset: 0x00238999
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_IS_AUDIT_ENABLEDNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_IS_AUDIT_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6EB RID: 46827 RVA: 0x0023A7B1 File Offset: 0x002389B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_IS_FUTURE_REP_ALLOWEDNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn);
			}

			// Token: 0x0600B6EC RID: 46828 RVA: 0x0023A7C4 File Offset: 0x002389C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_IS_FUTURE_REP_ALLOWEDNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6ED RID: 46829 RVA: 0x0023A7DC File Offset: 0x002389DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_FIXED_APPROVAL_ROUTINGNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn);
			}

			// Token: 0x0600B6EE RID: 46830 RVA: 0x0023A7EF File Offset: 0x002389EF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_FIXED_APPROVAL_ROUTINGNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6EF RID: 46831 RVA: 0x0023A807 File Offset: 0x00238A07
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_TIED_MODENull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_TIED_MODEColumn);
			}

			// Token: 0x0600B6F0 RID: 46832 RVA: 0x0023A81A File Offset: 0x00238A1A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_TIED_MODENull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_TIED_MODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6F1 RID: 46833 RVA: 0x0023A832 File Offset: 0x00238A32
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_IS_TRACKING_METHOD_LOCKEDNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn);
			}

			// Token: 0x0600B6F2 RID: 46834 RVA: 0x0023A845 File Offset: 0x00238A45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_IS_TRACKING_METHOD_LOCKEDNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600B6F3 RID: 46835 RVA: 0x0023A85D File Offset: 0x00238A5D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_ALLOW_PROJECT_LEVELNull()
			{
				return base.IsNull(this.tableTimeSheetSettings.WADMIN_TS_ALLOW_PROJECT_LEVELColumn);
			}

			// Token: 0x0600B6F4 RID: 46836 RVA: 0x0023A870 File Offset: 0x00238A70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_TS_ALLOW_PROJECT_LEVELNull()
			{
				base[this.tableTimeSheetSettings.WADMIN_TS_ALLOW_PROJECT_LEVELColumn] = Convert.DBNull;
			}

			// Token: 0x040024DF RID: 9439
			private TimeSheetSettingsDataSet.TimeSheetSettingsDataTable tableTimeSheetSettings;
		}

		// Token: 0x02000768 RID: 1896
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class TimeSheetSettingsRowChangeEvent : EventArgs
		{
			// Token: 0x0600B6F5 RID: 46837 RVA: 0x0023A888 File Offset: 0x00238A88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public TimeSheetSettingsRowChangeEvent(TimeSheetSettingsDataSet.TimeSheetSettingsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170037CF RID: 14287
			// (get) Token: 0x0600B6F6 RID: 46838 RVA: 0x0023A89E File Offset: 0x00238A9E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public TimeSheetSettingsDataSet.TimeSheetSettingsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170037D0 RID: 14288
			// (get) Token: 0x0600B6F7 RID: 46839 RVA: 0x0023A8A6 File Offset: 0x00238AA6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040024E0 RID: 9440
			private TimeSheetSettingsDataSet.TimeSheetSettingsRow eventRow;

			// Token: 0x040024E1 RID: 9441
			private DataRowAction eventAction;
		}
	}
}
