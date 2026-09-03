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
	// Token: 0x0200016E RID: 366
	[ToolboxItem(true)]
	[XmlRoot("GroupSettingsDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class GroupSettingsDataSet : DataSet
	{
		// Token: 0x06001AAB RID: 6827 RVA: 0x000563D4 File Offset: 0x000545D4
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupSettings, new string[]
			{
				"WGROUP_ROW_PATTERN",
				"WGROUP_SCHEME_UID",
				"WGROUP_TEXT_COLOR",
				"WGROUP_ROW_COLOR",
				"WGROUP_STYLE_ID",
				"WGROUP_SCHEME_ORDER",
				"WGROUP_FONT_STYLE",
				"WGROUP_SCHEME_NAME",
				"WGROUP_STYLE_NAME"
			});
		}

		// Token: 0x06001AAC RID: 6828 RVA: 0x00056444 File Offset: 0x00054644
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public GroupSettingsDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06001AAD RID: 6829 RVA: 0x00056498 File Offset: 0x00054698
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected GroupSettingsDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["GroupSettings"] != null)
				{
					base.Tables.Add(new GroupSettingsDataSet.GroupSettingsDataTable(dataSet.Tables["GroupSettings"]));
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

		// Token: 0x170007A7 RID: 1959
		// (get) Token: 0x06001AAE RID: 6830 RVA: 0x000565F5 File Offset: 0x000547F5
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public GroupSettingsDataSet.GroupSettingsDataTable GroupSettings
		{
			get
			{
				return this.tableGroupSettings;
			}
		}

		// Token: 0x170007A8 RID: 1960
		// (get) Token: 0x06001AAF RID: 6831 RVA: 0x000565FD File Offset: 0x000547FD
		// (set) Token: 0x06001AB0 RID: 6832 RVA: 0x00056605 File Offset: 0x00054805
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

		// Token: 0x170007A9 RID: 1961
		// (get) Token: 0x06001AB1 RID: 6833 RVA: 0x0005660E File Offset: 0x0005480E
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

		// Token: 0x170007AA RID: 1962
		// (get) Token: 0x06001AB2 RID: 6834 RVA: 0x00056616 File Offset: 0x00054816
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public new DataRelationCollection Relations
		{
			get
			{
				return base.Relations;
			}
		}

		// Token: 0x06001AB3 RID: 6835 RVA: 0x0005661E File Offset: 0x0005481E
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06001AB4 RID: 6836 RVA: 0x00056634 File Offset: 0x00054834
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			GroupSettingsDataSet groupSettingsDataSet = (GroupSettingsDataSet)base.Clone();
			groupSettingsDataSet.InitVars();
			groupSettingsDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return groupSettingsDataSet;
		}

		// Token: 0x06001AB5 RID: 6837 RVA: 0x00056660 File Offset: 0x00054860
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06001AB6 RID: 6838 RVA: 0x00056663 File Offset: 0x00054863
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001AB7 RID: 6839 RVA: 0x00056668 File Offset: 0x00054868
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["GroupSettings"] != null)
				{
					base.Tables.Add(new GroupSettingsDataSet.GroupSettingsDataTable(dataSet.Tables["GroupSettings"]));
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

		// Token: 0x06001AB8 RID: 6840 RVA: 0x00056730 File Offset: 0x00054930
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001AB9 RID: 6841 RVA: 0x00056764 File Offset: 0x00054964
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x0005676D File Offset: 0x0005496D
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableGroupSettings = (GroupSettingsDataSet.GroupSettingsDataTable)base.Tables["GroupSettings"];
			if (initTable && this.tableGroupSettings != null)
			{
				this.tableGroupSettings.InitVars();
			}
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x000567A0 File Offset: 0x000549A0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "GroupSettingsDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/GroupSettingsDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableGroupSettings = new GroupSettingsDataSet.GroupSettingsDataTable();
			base.Tables.Add(this.tableGroupSettings);
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x000567F8 File Offset: 0x000549F8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupSettings()
		{
			return false;
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x000567FB File Offset: 0x000549FB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x0005680C File Offset: 0x00054A0C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			GroupSettingsDataSet groupSettingsDataSet = new GroupSettingsDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = groupSettingsDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = groupSettingsDataSet.GetSchemaSerializable();
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

		// Token: 0x040005C6 RID: 1478
		private GroupSettingsDataSet.GroupSettingsDataTable tableGroupSettings;

		// Token: 0x040005C7 RID: 1479
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200016F RID: 367
		// (Invoke) Token: 0x06001AC0 RID: 6848
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupSettingsRowChangeEventHandler(object sender, GroupSettingsDataSet.GroupSettingsRowChangeEvent e);

		// Token: 0x02000170 RID: 368
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupSettingsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001AC3 RID: 6851 RVA: 0x00056954 File Offset: 0x00054B54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSettingsDataTable()
			{
				base.TableName = "GroupSettings";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001AC4 RID: 6852 RVA: 0x0005697C File Offset: 0x00054B7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupSettingsDataTable(DataTable table)
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

			// Token: 0x06001AC5 RID: 6853 RVA: 0x00056A24 File Offset: 0x00054C24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected GroupSettingsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170007AB RID: 1963
			// (get) Token: 0x06001AC6 RID: 6854 RVA: 0x00056A34 File Offset: 0x00054C34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_UID;
				}
			}

			// Token: 0x170007AC RID: 1964
			// (get) Token: 0x06001AC7 RID: 6855 RVA: 0x00056A3C File Offset: 0x00054C3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_STYLE_IDColumn
			{
				get
				{
					return this.columnWGROUP_STYLE_ID;
				}
			}

			// Token: 0x170007AD RID: 1965
			// (get) Token: 0x06001AC8 RID: 6856 RVA: 0x00056A44 File Offset: 0x00054C44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_ROW_COLORColumn
			{
				get
				{
					return this.columnWGROUP_ROW_COLOR;
				}
			}

			// Token: 0x170007AE RID: 1966
			// (get) Token: 0x06001AC9 RID: 6857 RVA: 0x00056A4C File Offset: 0x00054C4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_ROW_PATTERNColumn
			{
				get
				{
					return this.columnWGROUP_ROW_PATTERN;
				}
			}

			// Token: 0x170007AF RID: 1967
			// (get) Token: 0x06001ACA RID: 6858 RVA: 0x00056A54 File Offset: 0x00054C54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_TEXT_COLORColumn
			{
				get
				{
					return this.columnWGROUP_TEXT_COLOR;
				}
			}

			// Token: 0x170007B0 RID: 1968
			// (get) Token: 0x06001ACB RID: 6859 RVA: 0x00056A5C File Offset: 0x00054C5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_FONT_STYLEColumn
			{
				get
				{
					return this.columnWGROUP_FONT_STYLE;
				}
			}

			// Token: 0x170007B1 RID: 1969
			// (get) Token: 0x06001ACC RID: 6860 RVA: 0x00056A64 File Offset: 0x00054C64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_SCHEME_ORDERColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_ORDER;
				}
			}

			// Token: 0x170007B2 RID: 1970
			// (get) Token: 0x06001ACD RID: 6861 RVA: 0x00056A6C File Offset: 0x00054C6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_SCHEME_NAMEColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_NAME;
				}
			}

			// Token: 0x170007B3 RID: 1971
			// (get) Token: 0x06001ACE RID: 6862 RVA: 0x00056A74 File Offset: 0x00054C74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_STYLE_NAMEColumn
			{
				get
				{
					return this.columnWGROUP_STYLE_NAME;
				}
			}

			// Token: 0x170007B4 RID: 1972
			// (get) Token: 0x06001ACF RID: 6863 RVA: 0x00056A7C File Offset: 0x00054C7C
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

			// Token: 0x170007B5 RID: 1973
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSettingsDataSet.GroupSettingsRow this[int index]
			{
				get
				{
					return (GroupSettingsDataSet.GroupSettingsRow)base.Rows[index];
				}
			}

			// Token: 0x14000131 RID: 305
			// (add) Token: 0x06001AD1 RID: 6865 RVA: 0x00056A9C File Offset: 0x00054C9C
			// (remove) Token: 0x06001AD2 RID: 6866 RVA: 0x00056AD4 File Offset: 0x00054CD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSettingsDataSet.GroupSettingsRowChangeEventHandler GroupSettingsRowChanging;

			// Token: 0x14000132 RID: 306
			// (add) Token: 0x06001AD3 RID: 6867 RVA: 0x00056B0C File Offset: 0x00054D0C
			// (remove) Token: 0x06001AD4 RID: 6868 RVA: 0x00056B44 File Offset: 0x00054D44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSettingsDataSet.GroupSettingsRowChangeEventHandler GroupSettingsRowChanged;

			// Token: 0x14000133 RID: 307
			// (add) Token: 0x06001AD5 RID: 6869 RVA: 0x00056B7C File Offset: 0x00054D7C
			// (remove) Token: 0x06001AD6 RID: 6870 RVA: 0x00056BB4 File Offset: 0x00054DB4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSettingsDataSet.GroupSettingsRowChangeEventHandler GroupSettingsRowDeleting;

			// Token: 0x14000134 RID: 308
			// (add) Token: 0x06001AD7 RID: 6871 RVA: 0x00056BEC File Offset: 0x00054DEC
			// (remove) Token: 0x06001AD8 RID: 6872 RVA: 0x00056C24 File Offset: 0x00054E24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSettingsDataSet.GroupSettingsRowChangeEventHandler GroupSettingsRowDeleted;

			// Token: 0x06001AD9 RID: 6873 RVA: 0x00056C59 File Offset: 0x00054E59
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGroupSettingsRow(GroupSettingsDataSet.GroupSettingsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001ADA RID: 6874 RVA: 0x00056C68 File Offset: 0x00054E68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSettingsDataSet.GroupSettingsRow AddGroupSettingsRow(Guid WGROUP_SCHEME_UID, int WGROUP_STYLE_ID, int WGROUP_ROW_COLOR, int WGROUP_ROW_PATTERN, int WGROUP_TEXT_COLOR, int WGROUP_FONT_STYLE, int WGROUP_SCHEME_ORDER, string WGROUP_SCHEME_NAME, string WGROUP_STYLE_NAME)
			{
				GroupSettingsDataSet.GroupSettingsRow groupSettingsRow = (GroupSettingsDataSet.GroupSettingsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WGROUP_SCHEME_UID,
					WGROUP_STYLE_ID,
					WGROUP_ROW_COLOR,
					WGROUP_ROW_PATTERN,
					WGROUP_TEXT_COLOR,
					WGROUP_FONT_STYLE,
					WGROUP_SCHEME_ORDER,
					WGROUP_SCHEME_NAME,
					WGROUP_STYLE_NAME
				};
				groupSettingsRow.ItemArray = itemArray;
				base.Rows.Add(groupSettingsRow);
				return groupSettingsRow;
			}

			// Token: 0x06001ADB RID: 6875 RVA: 0x00056CEC File Offset: 0x00054EEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001ADC RID: 6876 RVA: 0x00056CFC File Offset: 0x00054EFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				GroupSettingsDataSet.GroupSettingsDataTable groupSettingsDataTable = (GroupSettingsDataSet.GroupSettingsDataTable)base.Clone();
				groupSettingsDataTable.InitVars();
				return groupSettingsDataTable;
			}

			// Token: 0x06001ADD RID: 6877 RVA: 0x00056D1C File Offset: 0x00054F1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new GroupSettingsDataSet.GroupSettingsDataTable();
			}

			// Token: 0x06001ADE RID: 6878 RVA: 0x00056D24 File Offset: 0x00054F24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWGROUP_SCHEME_UID = base.Columns["WGROUP_SCHEME_UID"];
				this.columnWGROUP_STYLE_ID = base.Columns["WGROUP_STYLE_ID"];
				this.columnWGROUP_ROW_COLOR = base.Columns["WGROUP_ROW_COLOR"];
				this.columnWGROUP_ROW_PATTERN = base.Columns["WGROUP_ROW_PATTERN"];
				this.columnWGROUP_TEXT_COLOR = base.Columns["WGROUP_TEXT_COLOR"];
				this.columnWGROUP_FONT_STYLE = base.Columns["WGROUP_FONT_STYLE"];
				this.columnWGROUP_SCHEME_ORDER = base.Columns["WGROUP_SCHEME_ORDER"];
				this.columnWGROUP_SCHEME_NAME = base.Columns["WGROUP_SCHEME_NAME"];
				this.columnWGROUP_STYLE_NAME = base.Columns["WGROUP_STYLE_NAME"];
			}

			// Token: 0x06001ADF RID: 6879 RVA: 0x00056DF8 File Offset: 0x00054FF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWGROUP_SCHEME_UID = new DataColumn("WGROUP_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_UID);
				this.columnWGROUP_STYLE_ID = new DataColumn("WGROUP_STYLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_STYLE_ID);
				this.columnWGROUP_ROW_COLOR = new DataColumn("WGROUP_ROW_COLOR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_ROW_COLOR);
				this.columnWGROUP_ROW_PATTERN = new DataColumn("WGROUP_ROW_PATTERN", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_ROW_PATTERN);
				this.columnWGROUP_TEXT_COLOR = new DataColumn("WGROUP_TEXT_COLOR", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_TEXT_COLOR);
				this.columnWGROUP_FONT_STYLE = new DataColumn("WGROUP_FONT_STYLE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_FONT_STYLE);
				this.columnWGROUP_SCHEME_ORDER = new DataColumn("WGROUP_SCHEME_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_ORDER);
				this.columnWGROUP_SCHEME_NAME = new DataColumn("WGROUP_SCHEME_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_NAME);
				this.columnWGROUP_STYLE_NAME = new DataColumn("WGROUP_STYLE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_STYLE_NAME);
			}

			// Token: 0x06001AE0 RID: 6880 RVA: 0x00056F9A File Offset: 0x0005519A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupSettingsDataSet.GroupSettingsRow NewGroupSettingsRow()
			{
				return (GroupSettingsDataSet.GroupSettingsRow)base.NewRow();
			}

			// Token: 0x06001AE1 RID: 6881 RVA: 0x00056FA7 File Offset: 0x000551A7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new GroupSettingsDataSet.GroupSettingsRow(builder);
			}

			// Token: 0x06001AE2 RID: 6882 RVA: 0x00056FAF File Offset: 0x000551AF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(GroupSettingsDataSet.GroupSettingsRow);
			}

			// Token: 0x06001AE3 RID: 6883 RVA: 0x00056FBB File Offset: 0x000551BB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupSettingsRowChanged != null)
				{
					this.GroupSettingsRowChanged(this, new GroupSettingsDataSet.GroupSettingsRowChangeEvent((GroupSettingsDataSet.GroupSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001AE4 RID: 6884 RVA: 0x00056FEE File Offset: 0x000551EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupSettingsRowChanging != null)
				{
					this.GroupSettingsRowChanging(this, new GroupSettingsDataSet.GroupSettingsRowChangeEvent((GroupSettingsDataSet.GroupSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001AE5 RID: 6885 RVA: 0x00057021 File Offset: 0x00055221
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupSettingsRowDeleted != null)
				{
					this.GroupSettingsRowDeleted(this, new GroupSettingsDataSet.GroupSettingsRowChangeEvent((GroupSettingsDataSet.GroupSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001AE6 RID: 6886 RVA: 0x00057054 File Offset: 0x00055254
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupSettingsRowDeleting != null)
				{
					this.GroupSettingsRowDeleting(this, new GroupSettingsDataSet.GroupSettingsRowChangeEvent((GroupSettingsDataSet.GroupSettingsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001AE7 RID: 6887 RVA: 0x00057087 File Offset: 0x00055287
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupSettingsRow(GroupSettingsDataSet.GroupSettingsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001AE8 RID: 6888 RVA: 0x00057098 File Offset: 0x00055298
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				GroupSettingsDataSet groupSettingsDataSet = new GroupSettingsDataSet();
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
				xmlSchemaAttribute.FixedValue = groupSettingsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupSettingsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = groupSettingsDataSet.GetSchemaSerializable();
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

			// Token: 0x040005C8 RID: 1480
			private DataColumn columnWGROUP_SCHEME_UID;

			// Token: 0x040005C9 RID: 1481
			private DataColumn columnWGROUP_STYLE_ID;

			// Token: 0x040005CA RID: 1482
			private DataColumn columnWGROUP_ROW_COLOR;

			// Token: 0x040005CB RID: 1483
			private DataColumn columnWGROUP_ROW_PATTERN;

			// Token: 0x040005CC RID: 1484
			private DataColumn columnWGROUP_TEXT_COLOR;

			// Token: 0x040005CD RID: 1485
			private DataColumn columnWGROUP_FONT_STYLE;

			// Token: 0x040005CE RID: 1486
			private DataColumn columnWGROUP_SCHEME_ORDER;

			// Token: 0x040005CF RID: 1487
			private DataColumn columnWGROUP_SCHEME_NAME;

			// Token: 0x040005D0 RID: 1488
			private DataColumn columnWGROUP_STYLE_NAME;
		}

		// Token: 0x02000171 RID: 369
		public class GroupSettingsRow : DataRow
		{
			// Token: 0x06001AE9 RID: 6889 RVA: 0x00057290 File Offset: 0x00055490
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupSettingsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupSettings = (GroupSettingsDataSet.GroupSettingsDataTable)base.Table;
			}

			// Token: 0x170007B6 RID: 1974
			// (get) Token: 0x06001AEA RID: 6890 RVA: 0x000572AC File Offset: 0x000554AC
			// (set) Token: 0x06001AEB RID: 6891 RVA: 0x000572F0 File Offset: 0x000554F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WGROUP_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableGroupSettings.WGROUP_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_UID' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x170007B7 RID: 1975
			// (get) Token: 0x06001AEC RID: 6892 RVA: 0x0005730C File Offset: 0x0005550C
			// (set) Token: 0x06001AED RID: 6893 RVA: 0x00057350 File Offset: 0x00055550
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGROUP_STYLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_STYLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_STYLE_ID' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_STYLE_IDColumn] = value;
				}
			}

			// Token: 0x170007B8 RID: 1976
			// (get) Token: 0x06001AEE RID: 6894 RVA: 0x0005736C File Offset: 0x0005556C
			// (set) Token: 0x06001AEF RID: 6895 RVA: 0x000573B0 File Offset: 0x000555B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGROUP_ROW_COLOR
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_ROW_COLORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_ROW_COLOR' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_ROW_COLORColumn] = value;
				}
			}

			// Token: 0x170007B9 RID: 1977
			// (get) Token: 0x06001AF0 RID: 6896 RVA: 0x000573CC File Offset: 0x000555CC
			// (set) Token: 0x06001AF1 RID: 6897 RVA: 0x00057410 File Offset: 0x00055610
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGROUP_ROW_PATTERN
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_ROW_PATTERNColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_ROW_PATTERN' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_ROW_PATTERNColumn] = value;
				}
			}

			// Token: 0x170007BA RID: 1978
			// (get) Token: 0x06001AF2 RID: 6898 RVA: 0x0005742C File Offset: 0x0005562C
			// (set) Token: 0x06001AF3 RID: 6899 RVA: 0x00057470 File Offset: 0x00055670
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WGROUP_TEXT_COLOR
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_TEXT_COLORColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_TEXT_COLOR' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_TEXT_COLORColumn] = value;
				}
			}

			// Token: 0x170007BB RID: 1979
			// (get) Token: 0x06001AF4 RID: 6900 RVA: 0x0005748C File Offset: 0x0005568C
			// (set) Token: 0x06001AF5 RID: 6901 RVA: 0x000574D0 File Offset: 0x000556D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGROUP_FONT_STYLE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_FONT_STYLEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_FONT_STYLE' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_FONT_STYLEColumn] = value;
				}
			}

			// Token: 0x170007BC RID: 1980
			// (get) Token: 0x06001AF6 RID: 6902 RVA: 0x000574EC File Offset: 0x000556EC
			// (set) Token: 0x06001AF7 RID: 6903 RVA: 0x00057530 File Offset: 0x00055730
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGROUP_SCHEME_ORDER
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSettings.WGROUP_SCHEME_ORDERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_ORDER' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_SCHEME_ORDERColumn] = value;
				}
			}

			// Token: 0x170007BD RID: 1981
			// (get) Token: 0x06001AF8 RID: 6904 RVA: 0x0005754C File Offset: 0x0005574C
			// (set) Token: 0x06001AF9 RID: 6905 RVA: 0x00057590 File Offset: 0x00055790
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WGROUP_SCHEME_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableGroupSettings.WGROUP_SCHEME_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_NAME' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_SCHEME_NAMEColumn] = value;
				}
			}

			// Token: 0x170007BE RID: 1982
			// (get) Token: 0x06001AFA RID: 6906 RVA: 0x000575A4 File Offset: 0x000557A4
			// (set) Token: 0x06001AFB RID: 6907 RVA: 0x000575E8 File Offset: 0x000557E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WGROUP_STYLE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableGroupSettings.WGROUP_STYLE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_STYLE_NAME' in table 'GroupSettings' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSettings.WGROUP_STYLE_NAMEColumn] = value;
				}
			}

			// Token: 0x06001AFC RID: 6908 RVA: 0x000575FC File Offset: 0x000557FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_SCHEME_UIDColumn);
			}

			// Token: 0x06001AFD RID: 6909 RVA: 0x0005760F File Offset: 0x0005580F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_SCHEME_UIDNull()
			{
				base[this.tableGroupSettings.WGROUP_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x06001AFE RID: 6910 RVA: 0x00057627 File Offset: 0x00055827
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_STYLE_IDNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_STYLE_IDColumn);
			}

			// Token: 0x06001AFF RID: 6911 RVA: 0x0005763A File Offset: 0x0005583A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_STYLE_IDNull()
			{
				base[this.tableGroupSettings.WGROUP_STYLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x06001B00 RID: 6912 RVA: 0x00057652 File Offset: 0x00055852
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_ROW_COLORNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_ROW_COLORColumn);
			}

			// Token: 0x06001B01 RID: 6913 RVA: 0x00057665 File Offset: 0x00055865
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_ROW_COLORNull()
			{
				base[this.tableGroupSettings.WGROUP_ROW_COLORColumn] = Convert.DBNull;
			}

			// Token: 0x06001B02 RID: 6914 RVA: 0x0005767D File Offset: 0x0005587D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_ROW_PATTERNNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_ROW_PATTERNColumn);
			}

			// Token: 0x06001B03 RID: 6915 RVA: 0x00057690 File Offset: 0x00055890
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_ROW_PATTERNNull()
			{
				base[this.tableGroupSettings.WGROUP_ROW_PATTERNColumn] = Convert.DBNull;
			}

			// Token: 0x06001B04 RID: 6916 RVA: 0x000576A8 File Offset: 0x000558A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_TEXT_COLORNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_TEXT_COLORColumn);
			}

			// Token: 0x06001B05 RID: 6917 RVA: 0x000576BB File Offset: 0x000558BB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_TEXT_COLORNull()
			{
				base[this.tableGroupSettings.WGROUP_TEXT_COLORColumn] = Convert.DBNull;
			}

			// Token: 0x06001B06 RID: 6918 RVA: 0x000576D3 File Offset: 0x000558D3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_FONT_STYLENull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_FONT_STYLEColumn);
			}

			// Token: 0x06001B07 RID: 6919 RVA: 0x000576E6 File Offset: 0x000558E6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_FONT_STYLENull()
			{
				base[this.tableGroupSettings.WGROUP_FONT_STYLEColumn] = Convert.DBNull;
			}

			// Token: 0x06001B08 RID: 6920 RVA: 0x000576FE File Offset: 0x000558FE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_SCHEME_ORDERNull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_SCHEME_ORDERColumn);
			}

			// Token: 0x06001B09 RID: 6921 RVA: 0x00057711 File Offset: 0x00055911
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_SCHEME_ORDERNull()
			{
				base[this.tableGroupSettings.WGROUP_SCHEME_ORDERColumn] = Convert.DBNull;
			}

			// Token: 0x06001B0A RID: 6922 RVA: 0x00057729 File Offset: 0x00055929
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_SCHEME_NAMENull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_SCHEME_NAMEColumn);
			}

			// Token: 0x06001B0B RID: 6923 RVA: 0x0005773C File Offset: 0x0005593C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_SCHEME_NAMENull()
			{
				base[this.tableGroupSettings.WGROUP_SCHEME_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06001B0C RID: 6924 RVA: 0x00057754 File Offset: 0x00055954
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_STYLE_NAMENull()
			{
				return base.IsNull(this.tableGroupSettings.WGROUP_STYLE_NAMEColumn);
			}

			// Token: 0x06001B0D RID: 6925 RVA: 0x00057767 File Offset: 0x00055967
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGROUP_STYLE_NAMENull()
			{
				base[this.tableGroupSettings.WGROUP_STYLE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x040005D5 RID: 1493
			private GroupSettingsDataSet.GroupSettingsDataTable tableGroupSettings;
		}

		// Token: 0x02000172 RID: 370
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupSettingsRowChangeEvent : EventArgs
		{
			// Token: 0x06001B0E RID: 6926 RVA: 0x0005777F File Offset: 0x0005597F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupSettingsRowChangeEvent(GroupSettingsDataSet.GroupSettingsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170007BF RID: 1983
			// (get) Token: 0x06001B0F RID: 6927 RVA: 0x00057795 File Offset: 0x00055995
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupSettingsDataSet.GroupSettingsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170007C0 RID: 1984
			// (get) Token: 0x06001B10 RID: 6928 RVA: 0x0005779D File Offset: 0x0005599D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040005D6 RID: 1494
			private GroupSettingsDataSet.GroupSettingsRow eventRow;

			// Token: 0x040005D7 RID: 1495
			private DataRowAction eventAction;
		}
	}
}
