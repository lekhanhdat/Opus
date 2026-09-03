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
	// Token: 0x02000338 RID: 824
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("PWAViewReportsDataSet")]
	[ToolboxItem(true)]
	[Serializable]
	public class PWAViewReportsDataSet : DataSet
	{
		// Token: 0x06005590 RID: 21904 RVA: 0x0010CBE4 File Offset: 0x0010ADE4
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ViewReportFields, new string[]
			{
				"CONV_STRING",
				"WVIEW_FIELD_AUTOSIZE",
				"WVIEW_FIELD_IS_HIDDEN",
				"WFIELD_IS_CUSTOM_FIELD",
				"WFIELD_TEXTCONV_TYPE",
				"WFIELD_NAME_CONV_VALUE",
				"WFIELD_GROUP",
				"WVIEW_UID",
				"WFIELD_IS_FORMULA",
				"WFIELD_NAME_SQL",
				"WVIEW_FIELD_WIDTH",
				"WVIEW_FIELD_CUSTOM_LABEL",
				"WVIEW_FIELD_IS_READ_ONLY",
				"WFIELD_LOOKUP_TABLE_UID",
				"WVIEW_FIELD_ORDER",
				"WFIELD_UID",
				"WFIELD_IS_MULTI_VALUE"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityCategoryObjects, new string[]
			{
				"WSEC_CAT_NAME",
				"WSEC_OBJ_UID",
				"WSEC_CAT_UID",
				"WSEC_OBJ_TYPE_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ViewReports, new string[]
			{
				"WTABLE_UID",
				"WVIEW_TYPE",
				"WVIEW_UID",
				"WVIEW_OUTLINE_LEVEL",
				"WVIEW_FILTER",
				"WVIEW_DESCRIPTION",
				"WGANTT_SCHEME_UID",
				"WVIEW_FILTER_BY_RBS",
				"WVIEW_PATH",
				"WVIEW_DEFAULT_LAYOUT",
				"WGROUP_SCHEME_UID",
				"WVIEW_GROUPING_SORTING_PARAMS",
				"WVIEW_DISPLAY_TYPE",
				"WVIEW_SPLITTER_POS",
				"WVIEW_NAME",
				"WVIEW_TIMESTAMP"
			});
		}

		// Token: 0x06005591 RID: 21905 RVA: 0x0010CD6C File Offset: 0x0010AF6C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public PWAViewReportsDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06005592 RID: 21906 RVA: 0x0010CDC0 File Offset: 0x0010AFC0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected PWAViewReportsDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["SecurityCategoryObjects"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.SecurityCategoryObjectsDataTable(dataSet.Tables["SecurityCategoryObjects"]));
				}
				if (dataSet.Tables["ViewReports"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.ViewReportsDataTable(dataSet.Tables["ViewReports"]));
				}
				if (dataSet.Tables["ViewReportFields"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.ViewReportFieldsDataTable(dataSet.Tables["ViewReportFields"]));
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

		// Token: 0x17001B4A RID: 6986
		// (get) Token: 0x06005593 RID: 21907 RVA: 0x0010CF81 File Offset: 0x0010B181
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public PWAViewReportsDataSet.SecurityCategoryObjectsDataTable SecurityCategoryObjects
		{
			get
			{
				return this.tableSecurityCategoryObjects;
			}
		}

		// Token: 0x17001B4B RID: 6987
		// (get) Token: 0x06005594 RID: 21908 RVA: 0x0010CF89 File Offset: 0x0010B189
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public PWAViewReportsDataSet.ViewReportsDataTable ViewReports
		{
			get
			{
				return this.tableViewReports;
			}
		}

		// Token: 0x17001B4C RID: 6988
		// (get) Token: 0x06005595 RID: 21909 RVA: 0x0010CF91 File Offset: 0x0010B191
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public PWAViewReportsDataSet.ViewReportFieldsDataTable ViewReportFields
		{
			get
			{
				return this.tableViewReportFields;
			}
		}

		// Token: 0x17001B4D RID: 6989
		// (get) Token: 0x06005596 RID: 21910 RVA: 0x0010CF99 File Offset: 0x0010B199
		// (set) Token: 0x06005597 RID: 21911 RVA: 0x0010CFA1 File Offset: 0x0010B1A1
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

		// Token: 0x17001B4E RID: 6990
		// (get) Token: 0x06005598 RID: 21912 RVA: 0x0010CFAA File Offset: 0x0010B1AA
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

		// Token: 0x17001B4F RID: 6991
		// (get) Token: 0x06005599 RID: 21913 RVA: 0x0010CFB2 File Offset: 0x0010B1B2
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

		// Token: 0x0600559A RID: 21914 RVA: 0x0010CFBA File Offset: 0x0010B1BA
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600559B RID: 21915 RVA: 0x0010CFD0 File Offset: 0x0010B1D0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			PWAViewReportsDataSet pwaviewReportsDataSet = (PWAViewReportsDataSet)base.Clone();
			pwaviewReportsDataSet.InitVars();
			pwaviewReportsDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return pwaviewReportsDataSet;
		}

		// Token: 0x0600559C RID: 21916 RVA: 0x0010CFFC File Offset: 0x0010B1FC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600559D RID: 21917 RVA: 0x0010CFFF File Offset: 0x0010B1FF
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600559E RID: 21918 RVA: 0x0010D004 File Offset: 0x0010B204
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["SecurityCategoryObjects"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.SecurityCategoryObjectsDataTable(dataSet.Tables["SecurityCategoryObjects"]));
				}
				if (dataSet.Tables["ViewReports"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.ViewReportsDataTable(dataSet.Tables["ViewReports"]));
				}
				if (dataSet.Tables["ViewReportFields"] != null)
				{
					base.Tables.Add(new PWAViewReportsDataSet.ViewReportFieldsDataTable(dataSet.Tables["ViewReportFields"]));
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

		// Token: 0x0600559F RID: 21919 RVA: 0x0010D130 File Offset: 0x0010B330
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x060055A0 RID: 21920 RVA: 0x0010D164 File Offset: 0x0010B364
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x060055A1 RID: 21921 RVA: 0x0010D170 File Offset: 0x0010B370
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSecurityCategoryObjects = (PWAViewReportsDataSet.SecurityCategoryObjectsDataTable)base.Tables["SecurityCategoryObjects"];
			if (initTable && this.tableSecurityCategoryObjects != null)
			{
				this.tableSecurityCategoryObjects.InitVars();
			}
			this.tableViewReports = (PWAViewReportsDataSet.ViewReportsDataTable)base.Tables["ViewReports"];
			if (initTable && this.tableViewReports != null)
			{
				this.tableViewReports.InitVars();
			}
			this.tableViewReportFields = (PWAViewReportsDataSet.ViewReportFieldsDataTable)base.Tables["ViewReportFields"];
			if (initTable && this.tableViewReportFields != null)
			{
				this.tableViewReportFields.InitVars();
			}
		}

		// Token: 0x060055A2 RID: 21922 RVA: 0x0010D210 File Offset: 0x0010B410
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "PWAViewReportsDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/PWAViewReportsDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSecurityCategoryObjects = new PWAViewReportsDataSet.SecurityCategoryObjectsDataTable();
			base.Tables.Add(this.tableSecurityCategoryObjects);
			this.tableViewReports = new PWAViewReportsDataSet.ViewReportsDataTable();
			base.Tables.Add(this.tableViewReports);
			this.tableViewReportFields = new PWAViewReportsDataSet.ViewReportFieldsDataTable();
			base.Tables.Add(this.tableViewReportFields);
		}

		// Token: 0x060055A3 RID: 21923 RVA: 0x0010D2A0 File Offset: 0x0010B4A0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSecurityCategoryObjects()
		{
			return false;
		}

		// Token: 0x060055A4 RID: 21924 RVA: 0x0010D2A3 File Offset: 0x0010B4A3
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeViewReports()
		{
			return false;
		}

		// Token: 0x060055A5 RID: 21925 RVA: 0x0010D2A6 File Offset: 0x0010B4A6
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeViewReportFields()
		{
			return false;
		}

		// Token: 0x060055A6 RID: 21926 RVA: 0x0010D2A9 File Offset: 0x0010B4A9
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x060055A7 RID: 21927 RVA: 0x0010D2BC File Offset: 0x0010B4BC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			PWAViewReportsDataSet pwaviewReportsDataSet = new PWAViewReportsDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = pwaviewReportsDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = pwaviewReportsDataSet.GetSchemaSerializable();
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

		// Token: 0x040011CD RID: 4557
		private PWAViewReportsDataSet.SecurityCategoryObjectsDataTable tableSecurityCategoryObjects;

		// Token: 0x040011CE RID: 4558
		private PWAViewReportsDataSet.ViewReportsDataTable tableViewReports;

		// Token: 0x040011CF RID: 4559
		private PWAViewReportsDataSet.ViewReportFieldsDataTable tableViewReportFields;

		// Token: 0x040011D0 RID: 4560
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000339 RID: 825
		// (Invoke) Token: 0x060055A9 RID: 21929
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityCategoryObjectsRowChangeEventHandler(object sender, PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEvent e);

		// Token: 0x0200033A RID: 826
		// (Invoke) Token: 0x060055AD RID: 21933
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ViewReportsRowChangeEventHandler(object sender, PWAViewReportsDataSet.ViewReportsRowChangeEvent e);

		// Token: 0x0200033B RID: 827
		// (Invoke) Token: 0x060055B1 RID: 21937
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ViewReportFieldsRowChangeEventHandler(object sender, PWAViewReportsDataSet.ViewReportFieldsRowChangeEvent e);

		// Token: 0x0200033C RID: 828
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityCategoryObjectsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060055B4 RID: 21940 RVA: 0x0010D404 File Offset: 0x0010B604
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoryObjectsDataTable()
			{
				base.TableName = "SecurityCategoryObjects";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060055B5 RID: 21941 RVA: 0x0010D42C File Offset: 0x0010B62C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SecurityCategoryObjectsDataTable(DataTable table)
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

			// Token: 0x060055B6 RID: 21942 RVA: 0x0010D4D4 File Offset: 0x0010B6D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SecurityCategoryObjectsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001B50 RID: 6992
			// (get) Token: 0x060055B7 RID: 21943 RVA: 0x0010D4E4 File Offset: 0x0010B6E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17001B51 RID: 6993
			// (get) Token: 0x060055B8 RID: 21944 RVA: 0x0010D4EC File Offset: 0x0010B6EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_OBJ_TYPE_UIDColumn
			{
				get
				{
					return this.columnWSEC_OBJ_TYPE_UID;
				}
			}

			// Token: 0x17001B52 RID: 6994
			// (get) Token: 0x060055B9 RID: 21945 RVA: 0x0010D4F4 File Offset: 0x0010B6F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_OBJ_UIDColumn
			{
				get
				{
					return this.columnWSEC_OBJ_UID;
				}
			}

			// Token: 0x17001B53 RID: 6995
			// (get) Token: 0x060055BA RID: 21946 RVA: 0x0010D4FC File Offset: 0x0010B6FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_NAMEColumn
			{
				get
				{
					return this.columnWSEC_CAT_NAME;
				}
			}

			// Token: 0x17001B54 RID: 6996
			// (get) Token: 0x060055BB RID: 21947 RVA: 0x0010D504 File Offset: 0x0010B704
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

			// Token: 0x17001B55 RID: 6997
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.SecurityCategoryObjectsRow this[int index]
			{
				get
				{
					return (PWAViewReportsDataSet.SecurityCategoryObjectsRow)base.Rows[index];
				}
			}

			// Token: 0x140002D1 RID: 721
			// (add) Token: 0x060055BD RID: 21949 RVA: 0x0010D524 File Offset: 0x0010B724
			// (remove) Token: 0x060055BE RID: 21950 RVA: 0x0010D55C File Offset: 0x0010B75C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowChanging;

			// Token: 0x140002D2 RID: 722
			// (add) Token: 0x060055BF RID: 21951 RVA: 0x0010D594 File Offset: 0x0010B794
			// (remove) Token: 0x060055C0 RID: 21952 RVA: 0x0010D5CC File Offset: 0x0010B7CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowChanged;

			// Token: 0x140002D3 RID: 723
			// (add) Token: 0x060055C1 RID: 21953 RVA: 0x0010D604 File Offset: 0x0010B804
			// (remove) Token: 0x060055C2 RID: 21954 RVA: 0x0010D63C File Offset: 0x0010B83C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowDeleting;

			// Token: 0x140002D4 RID: 724
			// (add) Token: 0x060055C3 RID: 21955 RVA: 0x0010D674 File Offset: 0x0010B874
			// (remove) Token: 0x060055C4 RID: 21956 RVA: 0x0010D6AC File Offset: 0x0010B8AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEventHandler SecurityCategoryObjectsRowDeleted;

			// Token: 0x060055C5 RID: 21957 RVA: 0x0010D6E1 File Offset: 0x0010B8E1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSecurityCategoryObjectsRow(PWAViewReportsDataSet.SecurityCategoryObjectsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060055C6 RID: 21958 RVA: 0x0010D6F0 File Offset: 0x0010B8F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.SecurityCategoryObjectsRow AddSecurityCategoryObjectsRow(Guid WSEC_CAT_UID, Guid WSEC_OBJ_TYPE_UID, Guid WSEC_OBJ_UID, string WSEC_CAT_NAME)
			{
				PWAViewReportsDataSet.SecurityCategoryObjectsRow securityCategoryObjectsRow = (PWAViewReportsDataSet.SecurityCategoryObjectsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_TYPE_UID,
					WSEC_OBJ_UID,
					WSEC_CAT_NAME
				};
				securityCategoryObjectsRow.ItemArray = itemArray;
				base.Rows.Add(securityCategoryObjectsRow);
				return securityCategoryObjectsRow;
			}

			// Token: 0x060055C7 RID: 21959 RVA: 0x0010D748 File Offset: 0x0010B948
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.SecurityCategoryObjectsRow FindByWSEC_CAT_UIDWSEC_OBJ_TYPE_UIDWSEC_OBJ_UID(Guid WSEC_CAT_UID, Guid WSEC_OBJ_TYPE_UID, Guid WSEC_OBJ_UID)
			{
				return (PWAViewReportsDataSet.SecurityCategoryObjectsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_OBJ_TYPE_UID,
					WSEC_OBJ_UID
				});
			}

			// Token: 0x060055C8 RID: 21960 RVA: 0x0010D788 File Offset: 0x0010B988
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060055C9 RID: 21961 RVA: 0x0010D798 File Offset: 0x0010B998
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				PWAViewReportsDataSet.SecurityCategoryObjectsDataTable securityCategoryObjectsDataTable = (PWAViewReportsDataSet.SecurityCategoryObjectsDataTable)base.Clone();
				securityCategoryObjectsDataTable.InitVars();
				return securityCategoryObjectsDataTable;
			}

			// Token: 0x060055CA RID: 21962 RVA: 0x0010D7B8 File Offset: 0x0010B9B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PWAViewReportsDataSet.SecurityCategoryObjectsDataTable();
			}

			// Token: 0x060055CB RID: 21963 RVA: 0x0010D7C0 File Offset: 0x0010B9C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_OBJ_TYPE_UID = base.Columns["WSEC_OBJ_TYPE_UID"];
				this.columnWSEC_OBJ_UID = base.Columns["WSEC_OBJ_UID"];
				this.columnWSEC_CAT_NAME = base.Columns["WSEC_CAT_NAME"];
			}

			// Token: 0x060055CC RID: 21964 RVA: 0x0010D828 File Offset: 0x0010BA28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_OBJ_TYPE_UID = new DataColumn("WSEC_OBJ_TYPE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_TYPE_UID);
				this.columnWSEC_OBJ_UID = new DataColumn("WSEC_OBJ_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_OBJ_UID);
				this.columnWSEC_CAT_NAME = new DataColumn("WSEC_CAT_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_NAME);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_OBJ_TYPE_UID,
					this.columnWSEC_OBJ_UID
				}, true));
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_TYPE_UID.AllowDBNull = false;
				this.columnWSEC_OBJ_UID.AllowDBNull = false;
			}

			// Token: 0x060055CD RID: 21965 RVA: 0x0010D946 File Offset: 0x0010BB46
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.SecurityCategoryObjectsRow NewSecurityCategoryObjectsRow()
			{
				return (PWAViewReportsDataSet.SecurityCategoryObjectsRow)base.NewRow();
			}

			// Token: 0x060055CE RID: 21966 RVA: 0x0010D953 File Offset: 0x0010BB53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PWAViewReportsDataSet.SecurityCategoryObjectsRow(builder);
			}

			// Token: 0x060055CF RID: 21967 RVA: 0x0010D95B File Offset: 0x0010BB5B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(PWAViewReportsDataSet.SecurityCategoryObjectsRow);
			}

			// Token: 0x060055D0 RID: 21968 RVA: 0x0010D967 File Offset: 0x0010BB67
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityCategoryObjectsRowChanged != null)
				{
					this.SecurityCategoryObjectsRowChanged(this, new PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEvent((PWAViewReportsDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060055D1 RID: 21969 RVA: 0x0010D99A File Offset: 0x0010BB9A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityCategoryObjectsRowChanging != null)
				{
					this.SecurityCategoryObjectsRowChanging(this, new PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEvent((PWAViewReportsDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060055D2 RID: 21970 RVA: 0x0010D9CD File Offset: 0x0010BBCD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityCategoryObjectsRowDeleted != null)
				{
					this.SecurityCategoryObjectsRowDeleted(this, new PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEvent((PWAViewReportsDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060055D3 RID: 21971 RVA: 0x0010DA00 File Offset: 0x0010BC00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityCategoryObjectsRowDeleting != null)
				{
					this.SecurityCategoryObjectsRowDeleting(this, new PWAViewReportsDataSet.SecurityCategoryObjectsRowChangeEvent((PWAViewReportsDataSet.SecurityCategoryObjectsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060055D4 RID: 21972 RVA: 0x0010DA33 File Offset: 0x0010BC33
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityCategoryObjectsRow(PWAViewReportsDataSet.SecurityCategoryObjectsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060055D5 RID: 21973 RVA: 0x0010DA44 File Offset: 0x0010BC44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PWAViewReportsDataSet pwaviewReportsDataSet = new PWAViewReportsDataSet();
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
				xmlSchemaAttribute.FixedValue = pwaviewReportsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityCategoryObjectsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = pwaviewReportsDataSet.GetSchemaSerializable();
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

			// Token: 0x040011D1 RID: 4561
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x040011D2 RID: 4562
			private DataColumn columnWSEC_OBJ_TYPE_UID;

			// Token: 0x040011D3 RID: 4563
			private DataColumn columnWSEC_OBJ_UID;

			// Token: 0x040011D4 RID: 4564
			private DataColumn columnWSEC_CAT_NAME;
		}

		// Token: 0x0200033D RID: 829
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ViewReportsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060055D6 RID: 21974 RVA: 0x0010DC3C File Offset: 0x0010BE3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ViewReportsDataTable()
			{
				base.TableName = "ViewReports";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060055D7 RID: 21975 RVA: 0x0010DC64 File Offset: 0x0010BE64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ViewReportsDataTable(DataTable table)
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

			// Token: 0x060055D8 RID: 21976 RVA: 0x0010DD0C File Offset: 0x0010BF0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected ViewReportsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001B56 RID: 6998
			// (get) Token: 0x060055D9 RID: 21977 RVA: 0x0010DD1C File Offset: 0x0010BF1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_UIDColumn
			{
				get
				{
					return this.columnWVIEW_UID;
				}
			}

			// Token: 0x17001B57 RID: 6999
			// (get) Token: 0x060055DA RID: 21978 RVA: 0x0010DD24 File Offset: 0x0010BF24
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_NAMEColumn
			{
				get
				{
					return this.columnWVIEW_NAME;
				}
			}

			// Token: 0x17001B58 RID: 7000
			// (get) Token: 0x060055DB RID: 21979 RVA: 0x0010DD2C File Offset: 0x0010BF2C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_DESCRIPTIONColumn
			{
				get
				{
					return this.columnWVIEW_DESCRIPTION;
				}
			}

			// Token: 0x17001B59 RID: 7001
			// (get) Token: 0x060055DC RID: 21980 RVA: 0x0010DD34 File Offset: 0x0010BF34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_TYPEColumn
			{
				get
				{
					return this.columnWVIEW_TYPE;
				}
			}

			// Token: 0x17001B5A RID: 7002
			// (get) Token: 0x060055DD RID: 21981 RVA: 0x0010DD3C File Offset: 0x0010BF3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_DISPLAY_TYPEColumn
			{
				get
				{
					return this.columnWVIEW_DISPLAY_TYPE;
				}
			}

			// Token: 0x17001B5B RID: 7003
			// (get) Token: 0x060055DE RID: 21982 RVA: 0x0010DD44 File Offset: 0x0010BF44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGANTT_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGANTT_SCHEME_UID;
				}
			}

			// Token: 0x17001B5C RID: 7004
			// (get) Token: 0x060055DF RID: 21983 RVA: 0x0010DD4C File Offset: 0x0010BF4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WTABLE_UIDColumn
			{
				get
				{
					return this.columnWTABLE_UID;
				}
			}

			// Token: 0x17001B5D RID: 7005
			// (get) Token: 0x060055E0 RID: 21984 RVA: 0x0010DD54 File Offset: 0x0010BF54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FILTERColumn
			{
				get
				{
					return this.columnWVIEW_FILTER;
				}
			}

			// Token: 0x17001B5E RID: 7006
			// (get) Token: 0x060055E1 RID: 21985 RVA: 0x0010DD5C File Offset: 0x0010BF5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_PATHColumn
			{
				get
				{
					return this.columnWVIEW_PATH;
				}
			}

			// Token: 0x17001B5F RID: 7007
			// (get) Token: 0x060055E2 RID: 21986 RVA: 0x0010DD64 File Offset: 0x0010BF64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_UID;
				}
			}

			// Token: 0x17001B60 RID: 7008
			// (get) Token: 0x060055E3 RID: 21987 RVA: 0x0010DD6C File Offset: 0x0010BF6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_TIMESTAMPColumn
			{
				get
				{
					return this.columnWVIEW_TIMESTAMP;
				}
			}

			// Token: 0x17001B61 RID: 7009
			// (get) Token: 0x060055E4 RID: 21988 RVA: 0x0010DD74 File Offset: 0x0010BF74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_GROUPING_SORTING_PARAMSColumn
			{
				get
				{
					return this.columnWVIEW_GROUPING_SORTING_PARAMS;
				}
			}

			// Token: 0x17001B62 RID: 7010
			// (get) Token: 0x060055E5 RID: 21989 RVA: 0x0010DD7C File Offset: 0x0010BF7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_FILTER_BY_RBSColumn
			{
				get
				{
					return this.columnWVIEW_FILTER_BY_RBS;
				}
			}

			// Token: 0x17001B63 RID: 7011
			// (get) Token: 0x060055E6 RID: 21990 RVA: 0x0010DD84 File Offset: 0x0010BF84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_OUTLINE_LEVELColumn
			{
				get
				{
					return this.columnWVIEW_OUTLINE_LEVEL;
				}
			}

			// Token: 0x17001B64 RID: 7012
			// (get) Token: 0x060055E7 RID: 21991 RVA: 0x0010DD8C File Offset: 0x0010BF8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_SPLITTER_POSColumn
			{
				get
				{
					return this.columnWVIEW_SPLITTER_POS;
				}
			}

			// Token: 0x17001B65 RID: 7013
			// (get) Token: 0x060055E8 RID: 21992 RVA: 0x0010DD94 File Offset: 0x0010BF94
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_DEFAULT_LAYOUTColumn
			{
				get
				{
					return this.columnWVIEW_DEFAULT_LAYOUT;
				}
			}

			// Token: 0x17001B66 RID: 7014
			// (get) Token: 0x060055E9 RID: 21993 RVA: 0x0010DD9C File Offset: 0x0010BF9C
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

			// Token: 0x17001B67 RID: 7015
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportsRow this[int index]
			{
				get
				{
					return (PWAViewReportsDataSet.ViewReportsRow)base.Rows[index];
				}
			}

			// Token: 0x140002D5 RID: 725
			// (add) Token: 0x060055EB RID: 21995 RVA: 0x0010DDBC File Offset: 0x0010BFBC
			// (remove) Token: 0x060055EC RID: 21996 RVA: 0x0010DDF4 File Offset: 0x0010BFF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportsRowChangeEventHandler ViewReportsRowChanging;

			// Token: 0x140002D6 RID: 726
			// (add) Token: 0x060055ED RID: 21997 RVA: 0x0010DE2C File Offset: 0x0010C02C
			// (remove) Token: 0x060055EE RID: 21998 RVA: 0x0010DE64 File Offset: 0x0010C064
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportsRowChangeEventHandler ViewReportsRowChanged;

			// Token: 0x140002D7 RID: 727
			// (add) Token: 0x060055EF RID: 21999 RVA: 0x0010DE9C File Offset: 0x0010C09C
			// (remove) Token: 0x060055F0 RID: 22000 RVA: 0x0010DED4 File Offset: 0x0010C0D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportsRowChangeEventHandler ViewReportsRowDeleting;

			// Token: 0x140002D8 RID: 728
			// (add) Token: 0x060055F1 RID: 22001 RVA: 0x0010DF0C File Offset: 0x0010C10C
			// (remove) Token: 0x060055F2 RID: 22002 RVA: 0x0010DF44 File Offset: 0x0010C144
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportsRowChangeEventHandler ViewReportsRowDeleted;

			// Token: 0x060055F3 RID: 22003 RVA: 0x0010DF79 File Offset: 0x0010C179
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddViewReportsRow(PWAViewReportsDataSet.ViewReportsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060055F4 RID: 22004 RVA: 0x0010DF88 File Offset: 0x0010C188
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportsRow AddViewReportsRow(Guid WVIEW_UID, string WVIEW_NAME, string WVIEW_DESCRIPTION, int WVIEW_TYPE, int WVIEW_DISPLAY_TYPE, Guid WGANTT_SCHEME_UID, Guid WTABLE_UID, string WVIEW_FILTER, string WVIEW_PATH, Guid WGROUP_SCHEME_UID, Guid WVIEW_TIMESTAMP, string WVIEW_GROUPING_SORTING_PARAMS, byte WVIEW_FILTER_BY_RBS, short WVIEW_OUTLINE_LEVEL, int WVIEW_SPLITTER_POS, short WVIEW_DEFAULT_LAYOUT)
			{
				PWAViewReportsDataSet.ViewReportsRow viewReportsRow = (PWAViewReportsDataSet.ViewReportsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WVIEW_UID,
					WVIEW_NAME,
					WVIEW_DESCRIPTION,
					WVIEW_TYPE,
					WVIEW_DISPLAY_TYPE,
					WGANTT_SCHEME_UID,
					WTABLE_UID,
					WVIEW_FILTER,
					WVIEW_PATH,
					WGROUP_SCHEME_UID,
					WVIEW_TIMESTAMP,
					WVIEW_GROUPING_SORTING_PARAMS,
					WVIEW_FILTER_BY_RBS,
					WVIEW_OUTLINE_LEVEL,
					WVIEW_SPLITTER_POS,
					WVIEW_DEFAULT_LAYOUT
				};
				viewReportsRow.ItemArray = itemArray;
				base.Rows.Add(viewReportsRow);
				return viewReportsRow;
			}

			// Token: 0x060055F5 RID: 22005 RVA: 0x0010E04C File Offset: 0x0010C24C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.ViewReportsRow FindByWVIEW_UID(Guid WVIEW_UID)
			{
				return (PWAViewReportsDataSet.ViewReportsRow)base.Rows.Find(new object[]
				{
					WVIEW_UID
				});
			}

			// Token: 0x060055F6 RID: 22006 RVA: 0x0010E07A File Offset: 0x0010C27A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060055F7 RID: 22007 RVA: 0x0010E088 File Offset: 0x0010C288
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				PWAViewReportsDataSet.ViewReportsDataTable viewReportsDataTable = (PWAViewReportsDataSet.ViewReportsDataTable)base.Clone();
				viewReportsDataTable.InitVars();
				return viewReportsDataTable;
			}

			// Token: 0x060055F8 RID: 22008 RVA: 0x0010E0A8 File Offset: 0x0010C2A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PWAViewReportsDataSet.ViewReportsDataTable();
			}

			// Token: 0x060055F9 RID: 22009 RVA: 0x0010E0B0 File Offset: 0x0010C2B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWVIEW_UID = base.Columns["WVIEW_UID"];
				this.columnWVIEW_NAME = base.Columns["WVIEW_NAME"];
				this.columnWVIEW_DESCRIPTION = base.Columns["WVIEW_DESCRIPTION"];
				this.columnWVIEW_TYPE = base.Columns["WVIEW_TYPE"];
				this.columnWVIEW_DISPLAY_TYPE = base.Columns["WVIEW_DISPLAY_TYPE"];
				this.columnWGANTT_SCHEME_UID = base.Columns["WGANTT_SCHEME_UID"];
				this.columnWTABLE_UID = base.Columns["WTABLE_UID"];
				this.columnWVIEW_FILTER = base.Columns["WVIEW_FILTER"];
				this.columnWVIEW_PATH = base.Columns["WVIEW_PATH"];
				this.columnWGROUP_SCHEME_UID = base.Columns["WGROUP_SCHEME_UID"];
				this.columnWVIEW_TIMESTAMP = base.Columns["WVIEW_TIMESTAMP"];
				this.columnWVIEW_GROUPING_SORTING_PARAMS = base.Columns["WVIEW_GROUPING_SORTING_PARAMS"];
				this.columnWVIEW_FILTER_BY_RBS = base.Columns["WVIEW_FILTER_BY_RBS"];
				this.columnWVIEW_OUTLINE_LEVEL = base.Columns["WVIEW_OUTLINE_LEVEL"];
				this.columnWVIEW_SPLITTER_POS = base.Columns["WVIEW_SPLITTER_POS"];
				this.columnWVIEW_DEFAULT_LAYOUT = base.Columns["WVIEW_DEFAULT_LAYOUT"];
			}

			// Token: 0x060055FA RID: 22010 RVA: 0x0010E220 File Offset: 0x0010C420
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWVIEW_UID = new DataColumn("WVIEW_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_UID);
				this.columnWVIEW_NAME = new DataColumn("WVIEW_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_NAME);
				this.columnWVIEW_DESCRIPTION = new DataColumn("WVIEW_DESCRIPTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_DESCRIPTION);
				this.columnWVIEW_TYPE = new DataColumn("WVIEW_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_TYPE);
				this.columnWVIEW_DISPLAY_TYPE = new DataColumn("WVIEW_DISPLAY_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_DISPLAY_TYPE);
				this.columnWGANTT_SCHEME_UID = new DataColumn("WGANTT_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGANTT_SCHEME_UID);
				this.columnWTABLE_UID = new DataColumn("WTABLE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWTABLE_UID);
				this.columnWVIEW_FILTER = new DataColumn("WVIEW_FILTER", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FILTER);
				this.columnWVIEW_PATH = new DataColumn("WVIEW_PATH", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_PATH);
				this.columnWGROUP_SCHEME_UID = new DataColumn("WGROUP_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_UID);
				this.columnWVIEW_TIMESTAMP = new DataColumn("WVIEW_TIMESTAMP", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_TIMESTAMP);
				this.columnWVIEW_GROUPING_SORTING_PARAMS = new DataColumn("WVIEW_GROUPING_SORTING_PARAMS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_GROUPING_SORTING_PARAMS);
				this.columnWVIEW_FILTER_BY_RBS = new DataColumn("WVIEW_FILTER_BY_RBS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FILTER_BY_RBS);
				this.columnWVIEW_OUTLINE_LEVEL = new DataColumn("WVIEW_OUTLINE_LEVEL", typeof(short), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_OUTLINE_LEVEL);
				this.columnWVIEW_SPLITTER_POS = new DataColumn("WVIEW_SPLITTER_POS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_SPLITTER_POS);
				this.columnWVIEW_DEFAULT_LAYOUT = new DataColumn("WVIEW_DEFAULT_LAYOUT", typeof(short), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_DEFAULT_LAYOUT);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWVIEW_UID
				}, true));
				this.columnWVIEW_UID.AllowDBNull = false;
				this.columnWVIEW_UID.Unique = true;
				this.columnWVIEW_NAME.AllowDBNull = false;
			}

			// Token: 0x060055FB RID: 22011 RVA: 0x0010E548 File Offset: 0x0010C748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportsRow NewViewReportsRow()
			{
				return (PWAViewReportsDataSet.ViewReportsRow)base.NewRow();
			}

			// Token: 0x060055FC RID: 22012 RVA: 0x0010E555 File Offset: 0x0010C755
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PWAViewReportsDataSet.ViewReportsRow(builder);
			}

			// Token: 0x060055FD RID: 22013 RVA: 0x0010E55D File Offset: 0x0010C75D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(PWAViewReportsDataSet.ViewReportsRow);
			}

			// Token: 0x060055FE RID: 22014 RVA: 0x0010E569 File Offset: 0x0010C769
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ViewReportsRowChanged != null)
				{
					this.ViewReportsRowChanged(this, new PWAViewReportsDataSet.ViewReportsRowChangeEvent((PWAViewReportsDataSet.ViewReportsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060055FF RID: 22015 RVA: 0x0010E59C File Offset: 0x0010C79C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ViewReportsRowChanging != null)
				{
					this.ViewReportsRowChanging(this, new PWAViewReportsDataSet.ViewReportsRowChangeEvent((PWAViewReportsDataSet.ViewReportsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06005600 RID: 22016 RVA: 0x0010E5CF File Offset: 0x0010C7CF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ViewReportsRowDeleted != null)
				{
					this.ViewReportsRowDeleted(this, new PWAViewReportsDataSet.ViewReportsRowChangeEvent((PWAViewReportsDataSet.ViewReportsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06005601 RID: 22017 RVA: 0x0010E602 File Offset: 0x0010C802
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ViewReportsRowDeleting != null)
				{
					this.ViewReportsRowDeleting(this, new PWAViewReportsDataSet.ViewReportsRowChangeEvent((PWAViewReportsDataSet.ViewReportsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06005602 RID: 22018 RVA: 0x0010E635 File Offset: 0x0010C835
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveViewReportsRow(PWAViewReportsDataSet.ViewReportsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06005603 RID: 22019 RVA: 0x0010E644 File Offset: 0x0010C844
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PWAViewReportsDataSet pwaviewReportsDataSet = new PWAViewReportsDataSet();
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
				xmlSchemaAttribute.FixedValue = pwaviewReportsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ViewReportsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = pwaviewReportsDataSet.GetSchemaSerializable();
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

			// Token: 0x040011D9 RID: 4569
			private DataColumn columnWVIEW_UID;

			// Token: 0x040011DA RID: 4570
			private DataColumn columnWVIEW_NAME;

			// Token: 0x040011DB RID: 4571
			private DataColumn columnWVIEW_DESCRIPTION;

			// Token: 0x040011DC RID: 4572
			private DataColumn columnWVIEW_TYPE;

			// Token: 0x040011DD RID: 4573
			private DataColumn columnWVIEW_DISPLAY_TYPE;

			// Token: 0x040011DE RID: 4574
			private DataColumn columnWGANTT_SCHEME_UID;

			// Token: 0x040011DF RID: 4575
			private DataColumn columnWTABLE_UID;

			// Token: 0x040011E0 RID: 4576
			private DataColumn columnWVIEW_FILTER;

			// Token: 0x040011E1 RID: 4577
			private DataColumn columnWVIEW_PATH;

			// Token: 0x040011E2 RID: 4578
			private DataColumn columnWGROUP_SCHEME_UID;

			// Token: 0x040011E3 RID: 4579
			private DataColumn columnWVIEW_TIMESTAMP;

			// Token: 0x040011E4 RID: 4580
			private DataColumn columnWVIEW_GROUPING_SORTING_PARAMS;

			// Token: 0x040011E5 RID: 4581
			private DataColumn columnWVIEW_FILTER_BY_RBS;

			// Token: 0x040011E6 RID: 4582
			private DataColumn columnWVIEW_OUTLINE_LEVEL;

			// Token: 0x040011E7 RID: 4583
			private DataColumn columnWVIEW_SPLITTER_POS;

			// Token: 0x040011E8 RID: 4584
			private DataColumn columnWVIEW_DEFAULT_LAYOUT;
		}

		// Token: 0x0200033E RID: 830
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ViewReportFieldsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06005604 RID: 22020 RVA: 0x0010E83C File Offset: 0x0010CA3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ViewReportFieldsDataTable()
			{
				base.TableName = "ViewReportFields";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06005605 RID: 22021 RVA: 0x0010E864 File Offset: 0x0010CA64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ViewReportFieldsDataTable(DataTable table)
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

			// Token: 0x06005606 RID: 22022 RVA: 0x0010E90C File Offset: 0x0010CB0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ViewReportFieldsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17001B68 RID: 7016
			// (get) Token: 0x06005607 RID: 22023 RVA: 0x0010E91C File Offset: 0x0010CB1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WFIELD_UIDColumn
			{
				get
				{
					return this.columnWFIELD_UID;
				}
			}

			// Token: 0x17001B69 RID: 7017
			// (get) Token: 0x06005608 RID: 22024 RVA: 0x0010E924 File Offset: 0x0010CB24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WVIEW_UIDColumn
			{
				get
				{
					return this.columnWVIEW_UID;
				}
			}

			// Token: 0x17001B6A RID: 7018
			// (get) Token: 0x06005609 RID: 22025 RVA: 0x0010E92C File Offset: 0x0010CB2C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_ORDERColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_ORDER;
				}
			}

			// Token: 0x17001B6B RID: 7019
			// (get) Token: 0x0600560A RID: 22026 RVA: 0x0010E934 File Offset: 0x0010CB34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_WIDTHColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_WIDTH;
				}
			}

			// Token: 0x17001B6C RID: 7020
			// (get) Token: 0x0600560B RID: 22027 RVA: 0x0010E93C File Offset: 0x0010CB3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_AUTOSIZEColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_AUTOSIZE;
				}
			}

			// Token: 0x17001B6D RID: 7021
			// (get) Token: 0x0600560C RID: 22028 RVA: 0x0010E944 File Offset: 0x0010CB44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_CUSTOM_LABELColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_CUSTOM_LABEL;
				}
			}

			// Token: 0x17001B6E RID: 7022
			// (get) Token: 0x0600560D RID: 22029 RVA: 0x0010E94C File Offset: 0x0010CB4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_IS_READ_ONLYColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_IS_READ_ONLY;
				}
			}

			// Token: 0x17001B6F RID: 7023
			// (get) Token: 0x0600560E RID: 22030 RVA: 0x0010E954 File Offset: 0x0010CB54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CONV_STRINGColumn
			{
				get
				{
					return this.columnCONV_STRING;
				}
			}

			// Token: 0x17001B70 RID: 7024
			// (get) Token: 0x0600560F RID: 22031 RVA: 0x0010E95C File Offset: 0x0010CB5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_TEXTCONV_TYPEColumn
			{
				get
				{
					return this.columnWFIELD_TEXTCONV_TYPE;
				}
			}

			// Token: 0x17001B71 RID: 7025
			// (get) Token: 0x06005610 RID: 22032 RVA: 0x0010E964 File Offset: 0x0010CB64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_NAME_SQLColumn
			{
				get
				{
					return this.columnWFIELD_NAME_SQL;
				}
			}

			// Token: 0x17001B72 RID: 7026
			// (get) Token: 0x06005611 RID: 22033 RVA: 0x0010E96C File Offset: 0x0010CB6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_NAME_CONV_VALUEColumn
			{
				get
				{
					return this.columnWFIELD_NAME_CONV_VALUE;
				}
			}

			// Token: 0x17001B73 RID: 7027
			// (get) Token: 0x06005612 RID: 22034 RVA: 0x0010E974 File Offset: 0x0010CB74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_IS_CUSTOM_FIELDColumn
			{
				get
				{
					return this.columnWFIELD_IS_CUSTOM_FIELD;
				}
			}

			// Token: 0x17001B74 RID: 7028
			// (get) Token: 0x06005613 RID: 22035 RVA: 0x0010E97C File Offset: 0x0010CB7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_IS_FORMULAColumn
			{
				get
				{
					return this.columnWFIELD_IS_FORMULA;
				}
			}

			// Token: 0x17001B75 RID: 7029
			// (get) Token: 0x06005614 RID: 22036 RVA: 0x0010E984 File Offset: 0x0010CB84
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_GROUPColumn
			{
				get
				{
					return this.columnWFIELD_GROUP;
				}
			}

			// Token: 0x17001B76 RID: 7030
			// (get) Token: 0x06005615 RID: 22037 RVA: 0x0010E98C File Offset: 0x0010CB8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_IS_MULTI_VALUEColumn
			{
				get
				{
					return this.columnWFIELD_IS_MULTI_VALUE;
				}
			}

			// Token: 0x17001B77 RID: 7031
			// (get) Token: 0x06005616 RID: 22038 RVA: 0x0010E994 File Offset: 0x0010CB94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WFIELD_LOOKUP_TABLE_UIDColumn
			{
				get
				{
					return this.columnWFIELD_LOOKUP_TABLE_UID;
				}
			}

			// Token: 0x17001B78 RID: 7032
			// (get) Token: 0x06005617 RID: 22039 RVA: 0x0010E99C File Offset: 0x0010CB9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WVIEW_FIELD_IS_HIDDENColumn
			{
				get
				{
					return this.columnWVIEW_FIELD_IS_HIDDEN;
				}
			}

			// Token: 0x17001B79 RID: 7033
			// (get) Token: 0x06005618 RID: 22040 RVA: 0x0010E9A4 File Offset: 0x0010CBA4
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

			// Token: 0x17001B7A RID: 7034
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportFieldsRow this[int index]
			{
				get
				{
					return (PWAViewReportsDataSet.ViewReportFieldsRow)base.Rows[index];
				}
			}

			// Token: 0x140002D9 RID: 729
			// (add) Token: 0x0600561A RID: 22042 RVA: 0x0010E9C4 File Offset: 0x0010CBC4
			// (remove) Token: 0x0600561B RID: 22043 RVA: 0x0010E9FC File Offset: 0x0010CBFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportFieldsRowChangeEventHandler ViewReportFieldsRowChanging;

			// Token: 0x140002DA RID: 730
			// (add) Token: 0x0600561C RID: 22044 RVA: 0x0010EA34 File Offset: 0x0010CC34
			// (remove) Token: 0x0600561D RID: 22045 RVA: 0x0010EA6C File Offset: 0x0010CC6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportFieldsRowChangeEventHandler ViewReportFieldsRowChanged;

			// Token: 0x140002DB RID: 731
			// (add) Token: 0x0600561E RID: 22046 RVA: 0x0010EAA4 File Offset: 0x0010CCA4
			// (remove) Token: 0x0600561F RID: 22047 RVA: 0x0010EADC File Offset: 0x0010CCDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportFieldsRowChangeEventHandler ViewReportFieldsRowDeleting;

			// Token: 0x140002DC RID: 732
			// (add) Token: 0x06005620 RID: 22048 RVA: 0x0010EB14 File Offset: 0x0010CD14
			// (remove) Token: 0x06005621 RID: 22049 RVA: 0x0010EB4C File Offset: 0x0010CD4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event PWAViewReportsDataSet.ViewReportFieldsRowChangeEventHandler ViewReportFieldsRowDeleted;

			// Token: 0x06005622 RID: 22050 RVA: 0x0010EB81 File Offset: 0x0010CD81
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddViewReportFieldsRow(PWAViewReportsDataSet.ViewReportFieldsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06005623 RID: 22051 RVA: 0x0010EB90 File Offset: 0x0010CD90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportFieldsRow AddViewReportFieldsRow(Guid WFIELD_UID, Guid WVIEW_UID, int WVIEW_FIELD_ORDER, int WVIEW_FIELD_WIDTH, byte WVIEW_FIELD_AUTOSIZE, string WVIEW_FIELD_CUSTOM_LABEL, byte WVIEW_FIELD_IS_READ_ONLY, string CONV_STRING, int WFIELD_TEXTCONV_TYPE, string WFIELD_NAME_SQL, int WFIELD_NAME_CONV_VALUE, int WFIELD_IS_CUSTOM_FIELD, int WFIELD_IS_FORMULA, byte WFIELD_GROUP, bool WFIELD_IS_MULTI_VALUE, Guid WFIELD_LOOKUP_TABLE_UID, bool WVIEW_FIELD_IS_HIDDEN)
			{
				PWAViewReportsDataSet.ViewReportFieldsRow viewReportFieldsRow = (PWAViewReportsDataSet.ViewReportFieldsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WFIELD_UID,
					WVIEW_UID,
					WVIEW_FIELD_ORDER,
					WVIEW_FIELD_WIDTH,
					WVIEW_FIELD_AUTOSIZE,
					WVIEW_FIELD_CUSTOM_LABEL,
					WVIEW_FIELD_IS_READ_ONLY,
					CONV_STRING,
					WFIELD_TEXTCONV_TYPE,
					WFIELD_NAME_SQL,
					WFIELD_NAME_CONV_VALUE,
					WFIELD_IS_CUSTOM_FIELD,
					WFIELD_IS_FORMULA,
					WFIELD_GROUP,
					WFIELD_IS_MULTI_VALUE,
					WFIELD_LOOKUP_TABLE_UID,
					WVIEW_FIELD_IS_HIDDEN
				};
				viewReportFieldsRow.ItemArray = itemArray;
				base.Rows.Add(viewReportFieldsRow);
				return viewReportFieldsRow;
			}

			// Token: 0x06005624 RID: 22052 RVA: 0x0010EC68 File Offset: 0x0010CE68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.ViewReportFieldsRow FindByWFIELD_UIDWVIEW_UID(Guid WFIELD_UID, Guid WVIEW_UID)
			{
				return (PWAViewReportsDataSet.ViewReportFieldsRow)base.Rows.Find(new object[]
				{
					WFIELD_UID,
					WVIEW_UID
				});
			}

			// Token: 0x06005625 RID: 22053 RVA: 0x0010EC9F File Offset: 0x0010CE9F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06005626 RID: 22054 RVA: 0x0010ECAC File Offset: 0x0010CEAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				PWAViewReportsDataSet.ViewReportFieldsDataTable viewReportFieldsDataTable = (PWAViewReportsDataSet.ViewReportFieldsDataTable)base.Clone();
				viewReportFieldsDataTable.InitVars();
				return viewReportFieldsDataTable;
			}

			// Token: 0x06005627 RID: 22055 RVA: 0x0010ECCC File Offset: 0x0010CECC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new PWAViewReportsDataSet.ViewReportFieldsDataTable();
			}

			// Token: 0x06005628 RID: 22056 RVA: 0x0010ECD4 File Offset: 0x0010CED4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWFIELD_UID = base.Columns["WFIELD_UID"];
				this.columnWVIEW_UID = base.Columns["WVIEW_UID"];
				this.columnWVIEW_FIELD_ORDER = base.Columns["WVIEW_FIELD_ORDER"];
				this.columnWVIEW_FIELD_WIDTH = base.Columns["WVIEW_FIELD_WIDTH"];
				this.columnWVIEW_FIELD_AUTOSIZE = base.Columns["WVIEW_FIELD_AUTOSIZE"];
				this.columnWVIEW_FIELD_CUSTOM_LABEL = base.Columns["WVIEW_FIELD_CUSTOM_LABEL"];
				this.columnWVIEW_FIELD_IS_READ_ONLY = base.Columns["WVIEW_FIELD_IS_READ_ONLY"];
				this.columnCONV_STRING = base.Columns["CONV_STRING"];
				this.columnWFIELD_TEXTCONV_TYPE = base.Columns["WFIELD_TEXTCONV_TYPE"];
				this.columnWFIELD_NAME_SQL = base.Columns["WFIELD_NAME_SQL"];
				this.columnWFIELD_NAME_CONV_VALUE = base.Columns["WFIELD_NAME_CONV_VALUE"];
				this.columnWFIELD_IS_CUSTOM_FIELD = base.Columns["WFIELD_IS_CUSTOM_FIELD"];
				this.columnWFIELD_IS_FORMULA = base.Columns["WFIELD_IS_FORMULA"];
				this.columnWFIELD_GROUP = base.Columns["WFIELD_GROUP"];
				this.columnWFIELD_IS_MULTI_VALUE = base.Columns["WFIELD_IS_MULTI_VALUE"];
				this.columnWFIELD_LOOKUP_TABLE_UID = base.Columns["WFIELD_LOOKUP_TABLE_UID"];
				this.columnWVIEW_FIELD_IS_HIDDEN = base.Columns["WVIEW_FIELD_IS_HIDDEN"];
			}

			// Token: 0x06005629 RID: 22057 RVA: 0x0010EE58 File Offset: 0x0010D058
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWFIELD_UID = new DataColumn("WFIELD_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_UID);
				this.columnWVIEW_UID = new DataColumn("WVIEW_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_UID);
				this.columnWVIEW_FIELD_ORDER = new DataColumn("WVIEW_FIELD_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_ORDER);
				this.columnWVIEW_FIELD_WIDTH = new DataColumn("WVIEW_FIELD_WIDTH", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_WIDTH);
				this.columnWVIEW_FIELD_AUTOSIZE = new DataColumn("WVIEW_FIELD_AUTOSIZE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_AUTOSIZE);
				this.columnWVIEW_FIELD_CUSTOM_LABEL = new DataColumn("WVIEW_FIELD_CUSTOM_LABEL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_CUSTOM_LABEL);
				this.columnWVIEW_FIELD_IS_READ_ONLY = new DataColumn("WVIEW_FIELD_IS_READ_ONLY", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_IS_READ_ONLY);
				this.columnCONV_STRING = new DataColumn("CONV_STRING", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnCONV_STRING);
				this.columnWFIELD_TEXTCONV_TYPE = new DataColumn("WFIELD_TEXTCONV_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_TEXTCONV_TYPE);
				this.columnWFIELD_NAME_SQL = new DataColumn("WFIELD_NAME_SQL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_NAME_SQL);
				this.columnWFIELD_NAME_CONV_VALUE = new DataColumn("WFIELD_NAME_CONV_VALUE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_NAME_CONV_VALUE);
				this.columnWFIELD_IS_CUSTOM_FIELD = new DataColumn("WFIELD_IS_CUSTOM_FIELD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_IS_CUSTOM_FIELD);
				this.columnWFIELD_IS_FORMULA = new DataColumn("WFIELD_IS_FORMULA", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_IS_FORMULA);
				this.columnWFIELD_GROUP = new DataColumn("WFIELD_GROUP", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_GROUP);
				this.columnWFIELD_IS_MULTI_VALUE = new DataColumn("WFIELD_IS_MULTI_VALUE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_IS_MULTI_VALUE);
				this.columnWFIELD_LOOKUP_TABLE_UID = new DataColumn("WFIELD_LOOKUP_TABLE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWFIELD_LOOKUP_TABLE_UID);
				this.columnWVIEW_FIELD_IS_HIDDEN = new DataColumn("WVIEW_FIELD_IS_HIDDEN", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWVIEW_FIELD_IS_HIDDEN);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnWFIELD_UID,
					this.columnWVIEW_UID
				}, true));
				this.columnWFIELD_UID.AllowDBNull = false;
				this.columnWVIEW_UID.AllowDBNull = false;
			}

			// Token: 0x0600562A RID: 22058 RVA: 0x0010F1AA File Offset: 0x0010D3AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.ViewReportFieldsRow NewViewReportFieldsRow()
			{
				return (PWAViewReportsDataSet.ViewReportFieldsRow)base.NewRow();
			}

			// Token: 0x0600562B RID: 22059 RVA: 0x0010F1B7 File Offset: 0x0010D3B7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new PWAViewReportsDataSet.ViewReportFieldsRow(builder);
			}

			// Token: 0x0600562C RID: 22060 RVA: 0x0010F1BF File Offset: 0x0010D3BF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(PWAViewReportsDataSet.ViewReportFieldsRow);
			}

			// Token: 0x0600562D RID: 22061 RVA: 0x0010F1CB File Offset: 0x0010D3CB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ViewReportFieldsRowChanged != null)
				{
					this.ViewReportFieldsRowChanged(this, new PWAViewReportsDataSet.ViewReportFieldsRowChangeEvent((PWAViewReportsDataSet.ViewReportFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600562E RID: 22062 RVA: 0x0010F1FE File Offset: 0x0010D3FE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ViewReportFieldsRowChanging != null)
				{
					this.ViewReportFieldsRowChanging(this, new PWAViewReportsDataSet.ViewReportFieldsRowChangeEvent((PWAViewReportsDataSet.ViewReportFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600562F RID: 22063 RVA: 0x0010F231 File Offset: 0x0010D431
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ViewReportFieldsRowDeleted != null)
				{
					this.ViewReportFieldsRowDeleted(this, new PWAViewReportsDataSet.ViewReportFieldsRowChangeEvent((PWAViewReportsDataSet.ViewReportFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06005630 RID: 22064 RVA: 0x0010F264 File Offset: 0x0010D464
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ViewReportFieldsRowDeleting != null)
				{
					this.ViewReportFieldsRowDeleting(this, new PWAViewReportsDataSet.ViewReportFieldsRowChangeEvent((PWAViewReportsDataSet.ViewReportFieldsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06005631 RID: 22065 RVA: 0x0010F297 File Offset: 0x0010D497
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveViewReportFieldsRow(PWAViewReportsDataSet.ViewReportFieldsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06005632 RID: 22066 RVA: 0x0010F2A8 File Offset: 0x0010D4A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				PWAViewReportsDataSet pwaviewReportsDataSet = new PWAViewReportsDataSet();
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
				xmlSchemaAttribute.FixedValue = pwaviewReportsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ViewReportFieldsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = pwaviewReportsDataSet.GetSchemaSerializable();
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

			// Token: 0x040011ED RID: 4589
			private DataColumn columnWFIELD_UID;

			// Token: 0x040011EE RID: 4590
			private DataColumn columnWVIEW_UID;

			// Token: 0x040011EF RID: 4591
			private DataColumn columnWVIEW_FIELD_ORDER;

			// Token: 0x040011F0 RID: 4592
			private DataColumn columnWVIEW_FIELD_WIDTH;

			// Token: 0x040011F1 RID: 4593
			private DataColumn columnWVIEW_FIELD_AUTOSIZE;

			// Token: 0x040011F2 RID: 4594
			private DataColumn columnWVIEW_FIELD_CUSTOM_LABEL;

			// Token: 0x040011F3 RID: 4595
			private DataColumn columnWVIEW_FIELD_IS_READ_ONLY;

			// Token: 0x040011F4 RID: 4596
			private DataColumn columnCONV_STRING;

			// Token: 0x040011F5 RID: 4597
			private DataColumn columnWFIELD_TEXTCONV_TYPE;

			// Token: 0x040011F6 RID: 4598
			private DataColumn columnWFIELD_NAME_SQL;

			// Token: 0x040011F7 RID: 4599
			private DataColumn columnWFIELD_NAME_CONV_VALUE;

			// Token: 0x040011F8 RID: 4600
			private DataColumn columnWFIELD_IS_CUSTOM_FIELD;

			// Token: 0x040011F9 RID: 4601
			private DataColumn columnWFIELD_IS_FORMULA;

			// Token: 0x040011FA RID: 4602
			private DataColumn columnWFIELD_GROUP;

			// Token: 0x040011FB RID: 4603
			private DataColumn columnWFIELD_IS_MULTI_VALUE;

			// Token: 0x040011FC RID: 4604
			private DataColumn columnWFIELD_LOOKUP_TABLE_UID;

			// Token: 0x040011FD RID: 4605
			private DataColumn columnWVIEW_FIELD_IS_HIDDEN;
		}

		// Token: 0x0200033F RID: 831
		public class SecurityCategoryObjectsRow : DataRow
		{
			// Token: 0x06005633 RID: 22067 RVA: 0x0010F4A0 File Offset: 0x0010D6A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityCategoryObjectsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityCategoryObjects = (PWAViewReportsDataSet.SecurityCategoryObjectsDataTable)base.Table;
			}

			// Token: 0x17001B7B RID: 7035
			// (get) Token: 0x06005634 RID: 22068 RVA: 0x0010F4BA File Offset: 0x0010D6BA
			// (set) Token: 0x06005635 RID: 22069 RVA: 0x0010F4D2 File Offset: 0x0010D6D2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17001B7C RID: 7036
			// (get) Token: 0x06005636 RID: 22070 RVA: 0x0010F4EB File Offset: 0x0010D6EB
			// (set) Token: 0x06005637 RID: 22071 RVA: 0x0010F503 File Offset: 0x0010D703
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_OBJ_TYPE_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_OBJ_TYPE_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_OBJ_TYPE_UIDColumn] = value;
				}
			}

			// Token: 0x17001B7D RID: 7037
			// (get) Token: 0x06005638 RID: 22072 RVA: 0x0010F51C File Offset: 0x0010D71C
			// (set) Token: 0x06005639 RID: 22073 RVA: 0x0010F534 File Offset: 0x0010D734
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_OBJ_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityCategoryObjects.WSEC_OBJ_UIDColumn];
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_OBJ_UIDColumn] = value;
				}
			}

			// Token: 0x17001B7E RID: 7038
			// (get) Token: 0x0600563A RID: 22074 RVA: 0x0010F550 File Offset: 0x0010D750
			// (set) Token: 0x0600563B RID: 22075 RVA: 0x0010F594 File Offset: 0x0010D794
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WSEC_CAT_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSecurityCategoryObjects.WSEC_CAT_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_CAT_NAME' in table 'SecurityCategoryObjects' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityCategoryObjects.WSEC_CAT_NAMEColumn] = value;
				}
			}

			// Token: 0x0600563C RID: 22076 RVA: 0x0010F5A8 File Offset: 0x0010D7A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWSEC_CAT_NAMENull()
			{
				return base.IsNull(this.tableSecurityCategoryObjects.WSEC_CAT_NAMEColumn);
			}

			// Token: 0x0600563D RID: 22077 RVA: 0x0010F5BB File Offset: 0x0010D7BB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWSEC_CAT_NAMENull()
			{
				base[this.tableSecurityCategoryObjects.WSEC_CAT_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x04001202 RID: 4610
			private PWAViewReportsDataSet.SecurityCategoryObjectsDataTable tableSecurityCategoryObjects;
		}

		// Token: 0x02000340 RID: 832
		public class ViewReportsRow : DataRow
		{
			// Token: 0x0600563E RID: 22078 RVA: 0x0010F5D3 File Offset: 0x0010D7D3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ViewReportsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableViewReports = (PWAViewReportsDataSet.ViewReportsDataTable)base.Table;
			}

			// Token: 0x17001B7F RID: 7039
			// (get) Token: 0x0600563F RID: 22079 RVA: 0x0010F5ED File Offset: 0x0010D7ED
			// (set) Token: 0x06005640 RID: 22080 RVA: 0x0010F605 File Offset: 0x0010D805
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WVIEW_UID
			{
				get
				{
					return (Guid)base[this.tableViewReports.WVIEW_UIDColumn];
				}
				set
				{
					base[this.tableViewReports.WVIEW_UIDColumn] = value;
				}
			}

			// Token: 0x17001B80 RID: 7040
			// (get) Token: 0x06005641 RID: 22081 RVA: 0x0010F61E File Offset: 0x0010D81E
			// (set) Token: 0x06005642 RID: 22082 RVA: 0x0010F636 File Offset: 0x0010D836
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WVIEW_NAME
			{
				get
				{
					return (string)base[this.tableViewReports.WVIEW_NAMEColumn];
				}
				set
				{
					base[this.tableViewReports.WVIEW_NAMEColumn] = value;
				}
			}

			// Token: 0x17001B81 RID: 7041
			// (get) Token: 0x06005643 RID: 22083 RVA: 0x0010F64C File Offset: 0x0010D84C
			// (set) Token: 0x06005644 RID: 22084 RVA: 0x0010F690 File Offset: 0x0010D890
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WVIEW_DESCRIPTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReports.WVIEW_DESCRIPTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_DESCRIPTION' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_DESCRIPTIONColumn] = value;
				}
			}

			// Token: 0x17001B82 RID: 7042
			// (get) Token: 0x06005645 RID: 22085 RVA: 0x0010F6A4 File Offset: 0x0010D8A4
			// (set) Token: 0x06005646 RID: 22086 RVA: 0x0010F6E8 File Offset: 0x0010D8E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WVIEW_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReports.WVIEW_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_TYPE' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_TYPEColumn] = value;
				}
			}

			// Token: 0x17001B83 RID: 7043
			// (get) Token: 0x06005647 RID: 22087 RVA: 0x0010F704 File Offset: 0x0010D904
			// (set) Token: 0x06005648 RID: 22088 RVA: 0x0010F748 File Offset: 0x0010D948
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WVIEW_DISPLAY_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReports.WVIEW_DISPLAY_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_DISPLAY_TYPE' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_DISPLAY_TYPEColumn] = value;
				}
			}

			// Token: 0x17001B84 RID: 7044
			// (get) Token: 0x06005649 RID: 22089 RVA: 0x0010F764 File Offset: 0x0010D964
			// (set) Token: 0x0600564A RID: 22090 RVA: 0x0010F7A8 File Offset: 0x0010D9A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WGANTT_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableViewReports.WGANTT_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGANTT_SCHEME_UID' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WGANTT_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x17001B85 RID: 7045
			// (get) Token: 0x0600564B RID: 22091 RVA: 0x0010F7C4 File Offset: 0x0010D9C4
			// (set) Token: 0x0600564C RID: 22092 RVA: 0x0010F808 File Offset: 0x0010DA08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WTABLE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableViewReports.WTABLE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WTABLE_UID' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WTABLE_UIDColumn] = value;
				}
			}

			// Token: 0x17001B86 RID: 7046
			// (get) Token: 0x0600564D RID: 22093 RVA: 0x0010F824 File Offset: 0x0010DA24
			// (set) Token: 0x0600564E RID: 22094 RVA: 0x0010F868 File Offset: 0x0010DA68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WVIEW_FILTER
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReports.WVIEW_FILTERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FILTER' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_FILTERColumn] = value;
				}
			}

			// Token: 0x17001B87 RID: 7047
			// (get) Token: 0x0600564F RID: 22095 RVA: 0x0010F87C File Offset: 0x0010DA7C
			// (set) Token: 0x06005650 RID: 22096 RVA: 0x0010F8C0 File Offset: 0x0010DAC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WVIEW_PATH
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReports.WVIEW_PATHColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_PATH' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_PATHColumn] = value;
				}
			}

			// Token: 0x17001B88 RID: 7048
			// (get) Token: 0x06005651 RID: 22097 RVA: 0x0010F8D4 File Offset: 0x0010DAD4
			// (set) Token: 0x06005652 RID: 22098 RVA: 0x0010F918 File Offset: 0x0010DB18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WGROUP_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableViewReports.WGROUP_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_UID' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WGROUP_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x17001B89 RID: 7049
			// (get) Token: 0x06005653 RID: 22099 RVA: 0x0010F934 File Offset: 0x0010DB34
			// (set) Token: 0x06005654 RID: 22100 RVA: 0x0010F978 File Offset: 0x0010DB78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WVIEW_TIMESTAMP
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableViewReports.WVIEW_TIMESTAMPColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_TIMESTAMP' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_TIMESTAMPColumn] = value;
				}
			}

			// Token: 0x17001B8A RID: 7050
			// (get) Token: 0x06005655 RID: 22101 RVA: 0x0010F994 File Offset: 0x0010DB94
			// (set) Token: 0x06005656 RID: 22102 RVA: 0x0010F9D8 File Offset: 0x0010DBD8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WVIEW_GROUPING_SORTING_PARAMS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReports.WVIEW_GROUPING_SORTING_PARAMSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_GROUPING_SORTING_PARAMS' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_GROUPING_SORTING_PARAMSColumn] = value;
				}
			}

			// Token: 0x17001B8B RID: 7051
			// (get) Token: 0x06005657 RID: 22103 RVA: 0x0010F9EC File Offset: 0x0010DBEC
			// (set) Token: 0x06005658 RID: 22104 RVA: 0x0010FA30 File Offset: 0x0010DC30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WVIEW_FILTER_BY_RBS
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableViewReports.WVIEW_FILTER_BY_RBSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FILTER_BY_RBS' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_FILTER_BY_RBSColumn] = value;
				}
			}

			// Token: 0x17001B8C RID: 7052
			// (get) Token: 0x06005659 RID: 22105 RVA: 0x0010FA4C File Offset: 0x0010DC4C
			// (set) Token: 0x0600565A RID: 22106 RVA: 0x0010FA90 File Offset: 0x0010DC90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public short WVIEW_OUTLINE_LEVEL
			{
				get
				{
					short result;
					try
					{
						result = (short)base[this.tableViewReports.WVIEW_OUTLINE_LEVELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_OUTLINE_LEVEL' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_OUTLINE_LEVELColumn] = value;
				}
			}

			// Token: 0x17001B8D RID: 7053
			// (get) Token: 0x0600565B RID: 22107 RVA: 0x0010FAAC File Offset: 0x0010DCAC
			// (set) Token: 0x0600565C RID: 22108 RVA: 0x0010FAF0 File Offset: 0x0010DCF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WVIEW_SPLITTER_POS
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReports.WVIEW_SPLITTER_POSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_SPLITTER_POS' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_SPLITTER_POSColumn] = value;
				}
			}

			// Token: 0x17001B8E RID: 7054
			// (get) Token: 0x0600565D RID: 22109 RVA: 0x0010FB0C File Offset: 0x0010DD0C
			// (set) Token: 0x0600565E RID: 22110 RVA: 0x0010FB50 File Offset: 0x0010DD50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public short WVIEW_DEFAULT_LAYOUT
			{
				get
				{
					short result;
					try
					{
						result = (short)base[this.tableViewReports.WVIEW_DEFAULT_LAYOUTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_DEFAULT_LAYOUT' in table 'ViewReports' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReports.WVIEW_DEFAULT_LAYOUTColumn] = value;
				}
			}

			// Token: 0x0600565F RID: 22111 RVA: 0x0010FB69 File Offset: 0x0010DD69
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_DESCRIPTIONNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_DESCRIPTIONColumn);
			}

			// Token: 0x06005660 RID: 22112 RVA: 0x0010FB7C File Offset: 0x0010DD7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_DESCRIPTIONNull()
			{
				base[this.tableViewReports.WVIEW_DESCRIPTIONColumn] = Convert.DBNull;
			}

			// Token: 0x06005661 RID: 22113 RVA: 0x0010FB94 File Offset: 0x0010DD94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_TYPENull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_TYPEColumn);
			}

			// Token: 0x06005662 RID: 22114 RVA: 0x0010FBA7 File Offset: 0x0010DDA7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_TYPENull()
			{
				base[this.tableViewReports.WVIEW_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x06005663 RID: 22115 RVA: 0x0010FBBF File Offset: 0x0010DDBF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_DISPLAY_TYPENull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_DISPLAY_TYPEColumn);
			}

			// Token: 0x06005664 RID: 22116 RVA: 0x0010FBD2 File Offset: 0x0010DDD2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWVIEW_DISPLAY_TYPENull()
			{
				base[this.tableViewReports.WVIEW_DISPLAY_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x06005665 RID: 22117 RVA: 0x0010FBEA File Offset: 0x0010DDEA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGANTT_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableViewReports.WGANTT_SCHEME_UIDColumn);
			}

			// Token: 0x06005666 RID: 22118 RVA: 0x0010FBFD File Offset: 0x0010DDFD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGANTT_SCHEME_UIDNull()
			{
				base[this.tableViewReports.WGANTT_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06005667 RID: 22119 RVA: 0x0010FC15 File Offset: 0x0010DE15
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWTABLE_UIDNull()
			{
				return base.IsNull(this.tableViewReports.WTABLE_UIDColumn);
			}

			// Token: 0x06005668 RID: 22120 RVA: 0x0010FC28 File Offset: 0x0010DE28
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWTABLE_UIDNull()
			{
				base[this.tableViewReports.WTABLE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06005669 RID: 22121 RVA: 0x0010FC40 File Offset: 0x0010DE40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FILTERNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_FILTERColumn);
			}

			// Token: 0x0600566A RID: 22122 RVA: 0x0010FC53 File Offset: 0x0010DE53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FILTERNull()
			{
				base[this.tableViewReports.WVIEW_FILTERColumn] = Convert.DBNull;
			}

			// Token: 0x0600566B RID: 22123 RVA: 0x0010FC6B File Offset: 0x0010DE6B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_PATHNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_PATHColumn);
			}

			// Token: 0x0600566C RID: 22124 RVA: 0x0010FC7E File Offset: 0x0010DE7E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_PATHNull()
			{
				base[this.tableViewReports.WVIEW_PATHColumn] = Convert.DBNull;
			}

			// Token: 0x0600566D RID: 22125 RVA: 0x0010FC96 File Offset: 0x0010DE96
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableViewReports.WGROUP_SCHEME_UIDColumn);
			}

			// Token: 0x0600566E RID: 22126 RVA: 0x0010FCA9 File Offset: 0x0010DEA9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_SCHEME_UIDNull()
			{
				base[this.tableViewReports.WGROUP_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600566F RID: 22127 RVA: 0x0010FCC1 File Offset: 0x0010DEC1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_TIMESTAMPNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_TIMESTAMPColumn);
			}

			// Token: 0x06005670 RID: 22128 RVA: 0x0010FCD4 File Offset: 0x0010DED4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_TIMESTAMPNull()
			{
				base[this.tableViewReports.WVIEW_TIMESTAMPColumn] = Convert.DBNull;
			}

			// Token: 0x06005671 RID: 22129 RVA: 0x0010FCEC File Offset: 0x0010DEEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_GROUPING_SORTING_PARAMSNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_GROUPING_SORTING_PARAMSColumn);
			}

			// Token: 0x06005672 RID: 22130 RVA: 0x0010FCFF File Offset: 0x0010DEFF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWVIEW_GROUPING_SORTING_PARAMSNull()
			{
				base[this.tableViewReports.WVIEW_GROUPING_SORTING_PARAMSColumn] = Convert.DBNull;
			}

			// Token: 0x06005673 RID: 22131 RVA: 0x0010FD17 File Offset: 0x0010DF17
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_FILTER_BY_RBSNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_FILTER_BY_RBSColumn);
			}

			// Token: 0x06005674 RID: 22132 RVA: 0x0010FD2A File Offset: 0x0010DF2A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FILTER_BY_RBSNull()
			{
				base[this.tableViewReports.WVIEW_FILTER_BY_RBSColumn] = Convert.DBNull;
			}

			// Token: 0x06005675 RID: 22133 RVA: 0x0010FD42 File Offset: 0x0010DF42
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_OUTLINE_LEVELNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_OUTLINE_LEVELColumn);
			}

			// Token: 0x06005676 RID: 22134 RVA: 0x0010FD55 File Offset: 0x0010DF55
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWVIEW_OUTLINE_LEVELNull()
			{
				base[this.tableViewReports.WVIEW_OUTLINE_LEVELColumn] = Convert.DBNull;
			}

			// Token: 0x06005677 RID: 22135 RVA: 0x0010FD6D File Offset: 0x0010DF6D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_SPLITTER_POSNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_SPLITTER_POSColumn);
			}

			// Token: 0x06005678 RID: 22136 RVA: 0x0010FD80 File Offset: 0x0010DF80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_SPLITTER_POSNull()
			{
				base[this.tableViewReports.WVIEW_SPLITTER_POSColumn] = Convert.DBNull;
			}

			// Token: 0x06005679 RID: 22137 RVA: 0x0010FD98 File Offset: 0x0010DF98
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_DEFAULT_LAYOUTNull()
			{
				return base.IsNull(this.tableViewReports.WVIEW_DEFAULT_LAYOUTColumn);
			}

			// Token: 0x0600567A RID: 22138 RVA: 0x0010FDAB File Offset: 0x0010DFAB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_DEFAULT_LAYOUTNull()
			{
				base[this.tableViewReports.WVIEW_DEFAULT_LAYOUTColumn] = Convert.DBNull;
			}

			// Token: 0x04001203 RID: 4611
			private PWAViewReportsDataSet.ViewReportsDataTable tableViewReports;
		}

		// Token: 0x02000341 RID: 833
		public class ViewReportFieldsRow : DataRow
		{
			// Token: 0x0600567B RID: 22139 RVA: 0x0010FDC3 File Offset: 0x0010DFC3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ViewReportFieldsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableViewReportFields = (PWAViewReportsDataSet.ViewReportFieldsDataTable)base.Table;
			}

			// Token: 0x17001B8F RID: 7055
			// (get) Token: 0x0600567C RID: 22140 RVA: 0x0010FDDD File Offset: 0x0010DFDD
			// (set) Token: 0x0600567D RID: 22141 RVA: 0x0010FDF5 File Offset: 0x0010DFF5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WFIELD_UID
			{
				get
				{
					return (Guid)base[this.tableViewReportFields.WFIELD_UIDColumn];
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_UIDColumn] = value;
				}
			}

			// Token: 0x17001B90 RID: 7056
			// (get) Token: 0x0600567E RID: 22142 RVA: 0x0010FE0E File Offset: 0x0010E00E
			// (set) Token: 0x0600567F RID: 22143 RVA: 0x0010FE26 File Offset: 0x0010E026
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WVIEW_UID
			{
				get
				{
					return (Guid)base[this.tableViewReportFields.WVIEW_UIDColumn];
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_UIDColumn] = value;
				}
			}

			// Token: 0x17001B91 RID: 7057
			// (get) Token: 0x06005680 RID: 22144 RVA: 0x0010FE40 File Offset: 0x0010E040
			// (set) Token: 0x06005681 RID: 22145 RVA: 0x0010FE84 File Offset: 0x0010E084
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WVIEW_FIELD_ORDER
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WVIEW_FIELD_ORDERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_ORDER' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_ORDERColumn] = value;
				}
			}

			// Token: 0x17001B92 RID: 7058
			// (get) Token: 0x06005682 RID: 22146 RVA: 0x0010FEA0 File Offset: 0x0010E0A0
			// (set) Token: 0x06005683 RID: 22147 RVA: 0x0010FEE4 File Offset: 0x0010E0E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WVIEW_FIELD_WIDTH
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WVIEW_FIELD_WIDTHColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_WIDTH' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_WIDTHColumn] = value;
				}
			}

			// Token: 0x17001B93 RID: 7059
			// (get) Token: 0x06005684 RID: 22148 RVA: 0x0010FF00 File Offset: 0x0010E100
			// (set) Token: 0x06005685 RID: 22149 RVA: 0x0010FF44 File Offset: 0x0010E144
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WVIEW_FIELD_AUTOSIZE
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableViewReportFields.WVIEW_FIELD_AUTOSIZEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_AUTOSIZE' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_AUTOSIZEColumn] = value;
				}
			}

			// Token: 0x17001B94 RID: 7060
			// (get) Token: 0x06005686 RID: 22150 RVA: 0x0010FF60 File Offset: 0x0010E160
			// (set) Token: 0x06005687 RID: 22151 RVA: 0x0010FFA4 File Offset: 0x0010E1A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WVIEW_FIELD_CUSTOM_LABEL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReportFields.WVIEW_FIELD_CUSTOM_LABELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_CUSTOM_LABEL' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_CUSTOM_LABELColumn] = value;
				}
			}

			// Token: 0x17001B95 RID: 7061
			// (get) Token: 0x06005688 RID: 22152 RVA: 0x0010FFB8 File Offset: 0x0010E1B8
			// (set) Token: 0x06005689 RID: 22153 RVA: 0x0010FFFC File Offset: 0x0010E1FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WVIEW_FIELD_IS_READ_ONLY
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableViewReportFields.WVIEW_FIELD_IS_READ_ONLYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_IS_READ_ONLY' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_IS_READ_ONLYColumn] = value;
				}
			}

			// Token: 0x17001B96 RID: 7062
			// (get) Token: 0x0600568A RID: 22154 RVA: 0x00110018 File Offset: 0x0010E218
			// (set) Token: 0x0600568B RID: 22155 RVA: 0x0011005C File Offset: 0x0010E25C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string CONV_STRING
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReportFields.CONV_STRINGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'CONV_STRING' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.CONV_STRINGColumn] = value;
				}
			}

			// Token: 0x17001B97 RID: 7063
			// (get) Token: 0x0600568C RID: 22156 RVA: 0x00110070 File Offset: 0x0010E270
			// (set) Token: 0x0600568D RID: 22157 RVA: 0x001100B4 File Offset: 0x0010E2B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WFIELD_TEXTCONV_TYPE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WFIELD_TEXTCONV_TYPEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_TEXTCONV_TYPE' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_TEXTCONV_TYPEColumn] = value;
				}
			}

			// Token: 0x17001B98 RID: 7064
			// (get) Token: 0x0600568E RID: 22158 RVA: 0x001100D0 File Offset: 0x0010E2D0
			// (set) Token: 0x0600568F RID: 22159 RVA: 0x00110114 File Offset: 0x0010E314
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WFIELD_NAME_SQL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableViewReportFields.WFIELD_NAME_SQLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_NAME_SQL' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_NAME_SQLColumn] = value;
				}
			}

			// Token: 0x17001B99 RID: 7065
			// (get) Token: 0x06005690 RID: 22160 RVA: 0x00110128 File Offset: 0x0010E328
			// (set) Token: 0x06005691 RID: 22161 RVA: 0x0011016C File Offset: 0x0010E36C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WFIELD_NAME_CONV_VALUE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WFIELD_NAME_CONV_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_NAME_CONV_VALUE' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_NAME_CONV_VALUEColumn] = value;
				}
			}

			// Token: 0x17001B9A RID: 7066
			// (get) Token: 0x06005692 RID: 22162 RVA: 0x00110188 File Offset: 0x0010E388
			// (set) Token: 0x06005693 RID: 22163 RVA: 0x001101CC File Offset: 0x0010E3CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WFIELD_IS_CUSTOM_FIELD
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WFIELD_IS_CUSTOM_FIELDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_IS_CUSTOM_FIELD' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_IS_CUSTOM_FIELDColumn] = value;
				}
			}

			// Token: 0x17001B9B RID: 7067
			// (get) Token: 0x06005694 RID: 22164 RVA: 0x001101E8 File Offset: 0x0010E3E8
			// (set) Token: 0x06005695 RID: 22165 RVA: 0x0011022C File Offset: 0x0010E42C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WFIELD_IS_FORMULA
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableViewReportFields.WFIELD_IS_FORMULAColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_IS_FORMULA' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_IS_FORMULAColumn] = value;
				}
			}

			// Token: 0x17001B9C RID: 7068
			// (get) Token: 0x06005696 RID: 22166 RVA: 0x00110248 File Offset: 0x0010E448
			// (set) Token: 0x06005697 RID: 22167 RVA: 0x0011028C File Offset: 0x0010E48C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WFIELD_GROUP
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableViewReportFields.WFIELD_GROUPColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_GROUP' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_GROUPColumn] = value;
				}
			}

			// Token: 0x17001B9D RID: 7069
			// (get) Token: 0x06005698 RID: 22168 RVA: 0x001102A8 File Offset: 0x0010E4A8
			// (set) Token: 0x06005699 RID: 22169 RVA: 0x001102EC File Offset: 0x0010E4EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WFIELD_IS_MULTI_VALUE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableViewReportFields.WFIELD_IS_MULTI_VALUEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_IS_MULTI_VALUE' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_IS_MULTI_VALUEColumn] = value;
				}
			}

			// Token: 0x17001B9E RID: 7070
			// (get) Token: 0x0600569A RID: 22170 RVA: 0x00110308 File Offset: 0x0010E508
			// (set) Token: 0x0600569B RID: 22171 RVA: 0x0011034C File Offset: 0x0010E54C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WFIELD_LOOKUP_TABLE_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableViewReportFields.WFIELD_LOOKUP_TABLE_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WFIELD_LOOKUP_TABLE_UID' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WFIELD_LOOKUP_TABLE_UIDColumn] = value;
				}
			}

			// Token: 0x17001B9F RID: 7071
			// (get) Token: 0x0600569C RID: 22172 RVA: 0x00110368 File Offset: 0x0010E568
			// (set) Token: 0x0600569D RID: 22173 RVA: 0x001103AC File Offset: 0x0010E5AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WVIEW_FIELD_IS_HIDDEN
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableViewReportFields.WVIEW_FIELD_IS_HIDDENColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WVIEW_FIELD_IS_HIDDEN' in table 'ViewReportFields' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableViewReportFields.WVIEW_FIELD_IS_HIDDENColumn] = value;
				}
			}

			// Token: 0x0600569E RID: 22174 RVA: 0x001103C5 File Offset: 0x0010E5C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FIELD_ORDERNull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_ORDERColumn);
			}

			// Token: 0x0600569F RID: 22175 RVA: 0x001103D8 File Offset: 0x0010E5D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FIELD_ORDERNull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_ORDERColumn] = Convert.DBNull;
			}

			// Token: 0x060056A0 RID: 22176 RVA: 0x001103F0 File Offset: 0x0010E5F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FIELD_WIDTHNull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_WIDTHColumn);
			}

			// Token: 0x060056A1 RID: 22177 RVA: 0x00110403 File Offset: 0x0010E603
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWVIEW_FIELD_WIDTHNull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_WIDTHColumn] = Convert.DBNull;
			}

			// Token: 0x060056A2 RID: 22178 RVA: 0x0011041B File Offset: 0x0010E61B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FIELD_AUTOSIZENull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_AUTOSIZEColumn);
			}

			// Token: 0x060056A3 RID: 22179 RVA: 0x0011042E File Offset: 0x0010E62E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWVIEW_FIELD_AUTOSIZENull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_AUTOSIZEColumn] = Convert.DBNull;
			}

			// Token: 0x060056A4 RID: 22180 RVA: 0x00110446 File Offset: 0x0010E646
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWVIEW_FIELD_CUSTOM_LABELNull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_CUSTOM_LABELColumn);
			}

			// Token: 0x060056A5 RID: 22181 RVA: 0x00110459 File Offset: 0x0010E659
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FIELD_CUSTOM_LABELNull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_CUSTOM_LABELColumn] = Convert.DBNull;
			}

			// Token: 0x060056A6 RID: 22182 RVA: 0x00110471 File Offset: 0x0010E671
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FIELD_IS_READ_ONLYNull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_IS_READ_ONLYColumn);
			}

			// Token: 0x060056A7 RID: 22183 RVA: 0x00110484 File Offset: 0x0010E684
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FIELD_IS_READ_ONLYNull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_IS_READ_ONLYColumn] = Convert.DBNull;
			}

			// Token: 0x060056A8 RID: 22184 RVA: 0x0011049C File Offset: 0x0010E69C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsCONV_STRINGNull()
			{
				return base.IsNull(this.tableViewReportFields.CONV_STRINGColumn);
			}

			// Token: 0x060056A9 RID: 22185 RVA: 0x001104AF File Offset: 0x0010E6AF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetCONV_STRINGNull()
			{
				base[this.tableViewReportFields.CONV_STRINGColumn] = Convert.DBNull;
			}

			// Token: 0x060056AA RID: 22186 RVA: 0x001104C7 File Offset: 0x0010E6C7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWFIELD_TEXTCONV_TYPENull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_TEXTCONV_TYPEColumn);
			}

			// Token: 0x060056AB RID: 22187 RVA: 0x001104DA File Offset: 0x0010E6DA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWFIELD_TEXTCONV_TYPENull()
			{
				base[this.tableViewReportFields.WFIELD_TEXTCONV_TYPEColumn] = Convert.DBNull;
			}

			// Token: 0x060056AC RID: 22188 RVA: 0x001104F2 File Offset: 0x0010E6F2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWFIELD_NAME_SQLNull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_NAME_SQLColumn);
			}

			// Token: 0x060056AD RID: 22189 RVA: 0x00110505 File Offset: 0x0010E705
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFIELD_NAME_SQLNull()
			{
				base[this.tableViewReportFields.WFIELD_NAME_SQLColumn] = Convert.DBNull;
			}

			// Token: 0x060056AE RID: 22190 RVA: 0x0011051D File Offset: 0x0010E71D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWFIELD_NAME_CONV_VALUENull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_NAME_CONV_VALUEColumn);
			}

			// Token: 0x060056AF RID: 22191 RVA: 0x00110530 File Offset: 0x0010E730
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWFIELD_NAME_CONV_VALUENull()
			{
				base[this.tableViewReportFields.WFIELD_NAME_CONV_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x060056B0 RID: 22192 RVA: 0x00110548 File Offset: 0x0010E748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWFIELD_IS_CUSTOM_FIELDNull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_IS_CUSTOM_FIELDColumn);
			}

			// Token: 0x060056B1 RID: 22193 RVA: 0x0011055B File Offset: 0x0010E75B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFIELD_IS_CUSTOM_FIELDNull()
			{
				base[this.tableViewReportFields.WFIELD_IS_CUSTOM_FIELDColumn] = Convert.DBNull;
			}

			// Token: 0x060056B2 RID: 22194 RVA: 0x00110573 File Offset: 0x0010E773
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWFIELD_IS_FORMULANull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_IS_FORMULAColumn);
			}

			// Token: 0x060056B3 RID: 22195 RVA: 0x00110586 File Offset: 0x0010E786
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFIELD_IS_FORMULANull()
			{
				base[this.tableViewReportFields.WFIELD_IS_FORMULAColumn] = Convert.DBNull;
			}

			// Token: 0x060056B4 RID: 22196 RVA: 0x0011059E File Offset: 0x0010E79E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWFIELD_GROUPNull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_GROUPColumn);
			}

			// Token: 0x060056B5 RID: 22197 RVA: 0x001105B1 File Offset: 0x0010E7B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWFIELD_GROUPNull()
			{
				base[this.tableViewReportFields.WFIELD_GROUPColumn] = Convert.DBNull;
			}

			// Token: 0x060056B6 RID: 22198 RVA: 0x001105C9 File Offset: 0x0010E7C9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWFIELD_IS_MULTI_VALUENull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_IS_MULTI_VALUEColumn);
			}

			// Token: 0x060056B7 RID: 22199 RVA: 0x001105DC File Offset: 0x0010E7DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFIELD_IS_MULTI_VALUENull()
			{
				base[this.tableViewReportFields.WFIELD_IS_MULTI_VALUEColumn] = Convert.DBNull;
			}

			// Token: 0x060056B8 RID: 22200 RVA: 0x001105F4 File Offset: 0x0010E7F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWFIELD_LOOKUP_TABLE_UIDNull()
			{
				return base.IsNull(this.tableViewReportFields.WFIELD_LOOKUP_TABLE_UIDColumn);
			}

			// Token: 0x060056B9 RID: 22201 RVA: 0x00110607 File Offset: 0x0010E807
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWFIELD_LOOKUP_TABLE_UIDNull()
			{
				base[this.tableViewReportFields.WFIELD_LOOKUP_TABLE_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x060056BA RID: 22202 RVA: 0x0011061F File Offset: 0x0010E81F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWVIEW_FIELD_IS_HIDDENNull()
			{
				return base.IsNull(this.tableViewReportFields.WVIEW_FIELD_IS_HIDDENColumn);
			}

			// Token: 0x060056BB RID: 22203 RVA: 0x00110632 File Offset: 0x0010E832
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWVIEW_FIELD_IS_HIDDENNull()
			{
				base[this.tableViewReportFields.WVIEW_FIELD_IS_HIDDENColumn] = Convert.DBNull;
			}

			// Token: 0x04001204 RID: 4612
			private PWAViewReportsDataSet.ViewReportFieldsDataTable tableViewReportFields;
		}

		// Token: 0x02000342 RID: 834
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityCategoryObjectsRowChangeEvent : EventArgs
		{
			// Token: 0x060056BC RID: 22204 RVA: 0x0011064A File Offset: 0x0010E84A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityCategoryObjectsRowChangeEvent(PWAViewReportsDataSet.SecurityCategoryObjectsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001BA0 RID: 7072
			// (get) Token: 0x060056BD RID: 22205 RVA: 0x00110660 File Offset: 0x0010E860
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.SecurityCategoryObjectsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001BA1 RID: 7073
			// (get) Token: 0x060056BE RID: 22206 RVA: 0x00110668 File Offset: 0x0010E868
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001205 RID: 4613
			private PWAViewReportsDataSet.SecurityCategoryObjectsRow eventRow;

			// Token: 0x04001206 RID: 4614
			private DataRowAction eventAction;
		}

		// Token: 0x02000343 RID: 835
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ViewReportsRowChangeEvent : EventArgs
		{
			// Token: 0x060056BF RID: 22207 RVA: 0x00110670 File Offset: 0x0010E870
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ViewReportsRowChangeEvent(PWAViewReportsDataSet.ViewReportsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001BA2 RID: 7074
			// (get) Token: 0x060056C0 RID: 22208 RVA: 0x00110686 File Offset: 0x0010E886
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public PWAViewReportsDataSet.ViewReportsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001BA3 RID: 7075
			// (get) Token: 0x060056C1 RID: 22209 RVA: 0x0011068E File Offset: 0x0010E88E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001207 RID: 4615
			private PWAViewReportsDataSet.ViewReportsRow eventRow;

			// Token: 0x04001208 RID: 4616
			private DataRowAction eventAction;
		}

		// Token: 0x02000344 RID: 836
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ViewReportFieldsRowChangeEvent : EventArgs
		{
			// Token: 0x060056C2 RID: 22210 RVA: 0x00110696 File Offset: 0x0010E896
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ViewReportFieldsRowChangeEvent(PWAViewReportsDataSet.ViewReportFieldsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17001BA4 RID: 7076
			// (get) Token: 0x060056C3 RID: 22211 RVA: 0x001106AC File Offset: 0x0010E8AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public PWAViewReportsDataSet.ViewReportFieldsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17001BA5 RID: 7077
			// (get) Token: 0x060056C4 RID: 22212 RVA: 0x001106B4 File Offset: 0x0010E8B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001209 RID: 4617
			private PWAViewReportsDataSet.ViewReportFieldsRow eventRow;

			// Token: 0x0400120A RID: 4618
			private DataRowAction eventAction;
		}
	}
}
