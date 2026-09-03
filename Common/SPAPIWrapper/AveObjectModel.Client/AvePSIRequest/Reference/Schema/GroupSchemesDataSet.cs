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
	// Token: 0x02000169 RID: 361
	[XmlRoot("GroupSchemesDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class GroupSchemesDataSet : DataSet
	{
		// Token: 0x06001A63 RID: 6755 RVA: 0x0005556C File Offset: 0x0005376C
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupSchemes, new string[]
			{
				"WGROUP_SCHEME_UID",
				"WGROUP_SCHEME_ORDER",
				"WGROUP_SCHEME_NAME"
			});
		}

		// Token: 0x06001A64 RID: 6756 RVA: 0x000555AC File Offset: 0x000537AC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public GroupSchemesDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06001A65 RID: 6757 RVA: 0x00055600 File Offset: 0x00053800
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected GroupSchemesDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["GroupSchemes"] != null)
				{
					base.Tables.Add(new GroupSchemesDataSet.GroupSchemesDataTable(dataSet.Tables["GroupSchemes"]));
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

		// Token: 0x17000799 RID: 1945
		// (get) Token: 0x06001A66 RID: 6758 RVA: 0x0005575D File Offset: 0x0005395D
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GroupSchemesDataSet.GroupSchemesDataTable GroupSchemes
		{
			get
			{
				return this.tableGroupSchemes;
			}
		}

		// Token: 0x1700079A RID: 1946
		// (get) Token: 0x06001A67 RID: 6759 RVA: 0x00055765 File Offset: 0x00053965
		// (set) Token: 0x06001A68 RID: 6760 RVA: 0x0005576D File Offset: 0x0005396D
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

		// Token: 0x1700079B RID: 1947
		// (get) Token: 0x06001A69 RID: 6761 RVA: 0x00055776 File Offset: 0x00053976
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

		// Token: 0x1700079C RID: 1948
		// (get) Token: 0x06001A6A RID: 6762 RVA: 0x0005577E File Offset: 0x0005397E
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

		// Token: 0x06001A6B RID: 6763 RVA: 0x00055786 File Offset: 0x00053986
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06001A6C RID: 6764 RVA: 0x0005579C File Offset: 0x0005399C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			GroupSchemesDataSet groupSchemesDataSet = (GroupSchemesDataSet)base.Clone();
			groupSchemesDataSet.InitVars();
			groupSchemesDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return groupSchemesDataSet;
		}

		// Token: 0x06001A6D RID: 6765 RVA: 0x000557C8 File Offset: 0x000539C8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06001A6E RID: 6766 RVA: 0x000557CB File Offset: 0x000539CB
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06001A6F RID: 6767 RVA: 0x000557D0 File Offset: 0x000539D0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["GroupSchemes"] != null)
				{
					base.Tables.Add(new GroupSchemesDataSet.GroupSchemesDataTable(dataSet.Tables["GroupSchemes"]));
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

		// Token: 0x06001A70 RID: 6768 RVA: 0x00055898 File Offset: 0x00053A98
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06001A71 RID: 6769 RVA: 0x000558CC File Offset: 0x00053ACC
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06001A72 RID: 6770 RVA: 0x000558D5 File Offset: 0x00053AD5
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableGroupSchemes = (GroupSchemesDataSet.GroupSchemesDataTable)base.Tables["GroupSchemes"];
			if (initTable && this.tableGroupSchemes != null)
			{
				this.tableGroupSchemes.InitVars();
			}
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x00055908 File Offset: 0x00053B08
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "GroupSchemesDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/GroupSchemesDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableGroupSchemes = new GroupSchemesDataSet.GroupSchemesDataTable();
			base.Tables.Add(this.tableGroupSchemes);
		}

		// Token: 0x06001A74 RID: 6772 RVA: 0x00055960 File Offset: 0x00053B60
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGroupSchemes()
		{
			return false;
		}

		// Token: 0x06001A75 RID: 6773 RVA: 0x00055963 File Offset: 0x00053B63
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06001A76 RID: 6774 RVA: 0x00055974 File Offset: 0x00053B74
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			GroupSchemesDataSet groupSchemesDataSet = new GroupSchemesDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = groupSchemesDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = groupSchemesDataSet.GetSchemaSerializable();
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

		// Token: 0x040005BA RID: 1466
		private GroupSchemesDataSet.GroupSchemesDataTable tableGroupSchemes;

		// Token: 0x040005BB RID: 1467
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200016A RID: 362
		// (Invoke) Token: 0x06001A78 RID: 6776
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupSchemesRowChangeEventHandler(object sender, GroupSchemesDataSet.GroupSchemesRowChangeEvent e);

		// Token: 0x0200016B RID: 363
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupSchemesDataTable : DataTable, IEnumerable
		{
			// Token: 0x06001A7B RID: 6779 RVA: 0x00055ABC File Offset: 0x00053CBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSchemesDataTable()
			{
				base.TableName = "GroupSchemes";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06001A7C RID: 6780 RVA: 0x00055AE4 File Offset: 0x00053CE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupSchemesDataTable(DataTable table)
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

			// Token: 0x06001A7D RID: 6781 RVA: 0x00055B8C File Offset: 0x00053D8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected GroupSchemesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700079D RID: 1949
			// (get) Token: 0x06001A7E RID: 6782 RVA: 0x00055B9C File Offset: 0x00053D9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_SCHEME_NAMEColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_NAME;
				}
			}

			// Token: 0x1700079E RID: 1950
			// (get) Token: 0x06001A7F RID: 6783 RVA: 0x00055BA4 File Offset: 0x00053DA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WGROUP_SCHEME_ORDERColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_ORDER;
				}
			}

			// Token: 0x1700079F RID: 1951
			// (get) Token: 0x06001A80 RID: 6784 RVA: 0x00055BAC File Offset: 0x00053DAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WGROUP_SCHEME_UIDColumn
			{
				get
				{
					return this.columnWGROUP_SCHEME_UID;
				}
			}

			// Token: 0x170007A0 RID: 1952
			// (get) Token: 0x06001A81 RID: 6785 RVA: 0x00055BB4 File Offset: 0x00053DB4
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

			// Token: 0x170007A1 RID: 1953
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSchemesDataSet.GroupSchemesRow this[int index]
			{
				get
				{
					return (GroupSchemesDataSet.GroupSchemesRow)base.Rows[index];
				}
			}

			// Token: 0x1400012D RID: 301
			// (add) Token: 0x06001A83 RID: 6787 RVA: 0x00055BD4 File Offset: 0x00053DD4
			// (remove) Token: 0x06001A84 RID: 6788 RVA: 0x00055C0C File Offset: 0x00053E0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSchemesDataSet.GroupSchemesRowChangeEventHandler GroupSchemesRowChanging;

			// Token: 0x1400012E RID: 302
			// (add) Token: 0x06001A85 RID: 6789 RVA: 0x00055C44 File Offset: 0x00053E44
			// (remove) Token: 0x06001A86 RID: 6790 RVA: 0x00055C7C File Offset: 0x00053E7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSchemesDataSet.GroupSchemesRowChangeEventHandler GroupSchemesRowChanged;

			// Token: 0x1400012F RID: 303
			// (add) Token: 0x06001A87 RID: 6791 RVA: 0x00055CB4 File Offset: 0x00053EB4
			// (remove) Token: 0x06001A88 RID: 6792 RVA: 0x00055CEC File Offset: 0x00053EEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSchemesDataSet.GroupSchemesRowChangeEventHandler GroupSchemesRowDeleting;

			// Token: 0x14000130 RID: 304
			// (add) Token: 0x06001A89 RID: 6793 RVA: 0x00055D24 File Offset: 0x00053F24
			// (remove) Token: 0x06001A8A RID: 6794 RVA: 0x00055D5C File Offset: 0x00053F5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event GroupSchemesDataSet.GroupSchemesRowChangeEventHandler GroupSchemesRowDeleted;

			// Token: 0x06001A8B RID: 6795 RVA: 0x00055D91 File Offset: 0x00053F91
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGroupSchemesRow(GroupSchemesDataSet.GroupSchemesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06001A8C RID: 6796 RVA: 0x00055DA0 File Offset: 0x00053FA0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSchemesDataSet.GroupSchemesRow AddGroupSchemesRow(string WGROUP_SCHEME_NAME, int WGROUP_SCHEME_ORDER, Guid WGROUP_SCHEME_UID)
			{
				GroupSchemesDataSet.GroupSchemesRow groupSchemesRow = (GroupSchemesDataSet.GroupSchemesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WGROUP_SCHEME_NAME,
					WGROUP_SCHEME_ORDER,
					WGROUP_SCHEME_UID
				};
				groupSchemesRow.ItemArray = itemArray;
				base.Rows.Add(groupSchemesRow);
				return groupSchemesRow;
			}

			// Token: 0x06001A8D RID: 6797 RVA: 0x00055DEC File Offset: 0x00053FEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06001A8E RID: 6798 RVA: 0x00055DFC File Offset: 0x00053FFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				GroupSchemesDataSet.GroupSchemesDataTable groupSchemesDataTable = (GroupSchemesDataSet.GroupSchemesDataTable)base.Clone();
				groupSchemesDataTable.InitVars();
				return groupSchemesDataTable;
			}

			// Token: 0x06001A8F RID: 6799 RVA: 0x00055E1C File Offset: 0x0005401C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new GroupSchemesDataSet.GroupSchemesDataTable();
			}

			// Token: 0x06001A90 RID: 6800 RVA: 0x00055E24 File Offset: 0x00054024
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWGROUP_SCHEME_NAME = base.Columns["WGROUP_SCHEME_NAME"];
				this.columnWGROUP_SCHEME_ORDER = base.Columns["WGROUP_SCHEME_ORDER"];
				this.columnWGROUP_SCHEME_UID = base.Columns["WGROUP_SCHEME_UID"];
			}

			// Token: 0x06001A91 RID: 6801 RVA: 0x00055E74 File Offset: 0x00054074
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWGROUP_SCHEME_NAME = new DataColumn("WGROUP_SCHEME_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_NAME);
				this.columnWGROUP_SCHEME_ORDER = new DataColumn("WGROUP_SCHEME_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_ORDER);
				this.columnWGROUP_SCHEME_UID = new DataColumn("WGROUP_SCHEME_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWGROUP_SCHEME_UID);
			}

			// Token: 0x06001A92 RID: 6802 RVA: 0x00055F08 File Offset: 0x00054108
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSchemesDataSet.GroupSchemesRow NewGroupSchemesRow()
			{
				return (GroupSchemesDataSet.GroupSchemesRow)base.NewRow();
			}

			// Token: 0x06001A93 RID: 6803 RVA: 0x00055F15 File Offset: 0x00054115
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new GroupSchemesDataSet.GroupSchemesRow(builder);
			}

			// Token: 0x06001A94 RID: 6804 RVA: 0x00055F1D File Offset: 0x0005411D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(GroupSchemesDataSet.GroupSchemesRow);
			}

			// Token: 0x06001A95 RID: 6805 RVA: 0x00055F29 File Offset: 0x00054129
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupSchemesRowChanged != null)
				{
					this.GroupSchemesRowChanged(this, new GroupSchemesDataSet.GroupSchemesRowChangeEvent((GroupSchemesDataSet.GroupSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A96 RID: 6806 RVA: 0x00055F5C File Offset: 0x0005415C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupSchemesRowChanging != null)
				{
					this.GroupSchemesRowChanging(this, new GroupSchemesDataSet.GroupSchemesRowChangeEvent((GroupSchemesDataSet.GroupSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A97 RID: 6807 RVA: 0x00055F8F File Offset: 0x0005418F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupSchemesRowDeleted != null)
				{
					this.GroupSchemesRowDeleted(this, new GroupSchemesDataSet.GroupSchemesRowChangeEvent((GroupSchemesDataSet.GroupSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A98 RID: 6808 RVA: 0x00055FC2 File Offset: 0x000541C2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupSchemesRowDeleting != null)
				{
					this.GroupSchemesRowDeleting(this, new GroupSchemesDataSet.GroupSchemesRowChangeEvent((GroupSchemesDataSet.GroupSchemesRow)e.Row, e.Action));
				}
			}

			// Token: 0x06001A99 RID: 6809 RVA: 0x00055FF5 File Offset: 0x000541F5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupSchemesRow(GroupSchemesDataSet.GroupSchemesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06001A9A RID: 6810 RVA: 0x00056004 File Offset: 0x00054204
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				GroupSchemesDataSet groupSchemesDataSet = new GroupSchemesDataSet();
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
				xmlSchemaAttribute.FixedValue = groupSchemesDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupSchemesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = groupSchemesDataSet.GetSchemaSerializable();
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

			// Token: 0x040005BC RID: 1468
			private DataColumn columnWGROUP_SCHEME_NAME;

			// Token: 0x040005BD RID: 1469
			private DataColumn columnWGROUP_SCHEME_ORDER;

			// Token: 0x040005BE RID: 1470
			private DataColumn columnWGROUP_SCHEME_UID;
		}

		// Token: 0x0200016C RID: 364
		public class GroupSchemesRow : DataRow
		{
			// Token: 0x06001A9B RID: 6811 RVA: 0x000561FC File Offset: 0x000543FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupSchemesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupSchemes = (GroupSchemesDataSet.GroupSchemesDataTable)base.Table;
			}

			// Token: 0x170007A2 RID: 1954
			// (get) Token: 0x06001A9C RID: 6812 RVA: 0x00056218 File Offset: 0x00054418
			// (set) Token: 0x06001A9D RID: 6813 RVA: 0x0005625C File Offset: 0x0005445C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WGROUP_SCHEME_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableGroupSchemes.WGROUP_SCHEME_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_NAME' in table 'GroupSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSchemes.WGROUP_SCHEME_NAMEColumn] = value;
				}
			}

			// Token: 0x170007A3 RID: 1955
			// (get) Token: 0x06001A9E RID: 6814 RVA: 0x00056270 File Offset: 0x00054470
			// (set) Token: 0x06001A9F RID: 6815 RVA: 0x000562B4 File Offset: 0x000544B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WGROUP_SCHEME_ORDER
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableGroupSchemes.WGROUP_SCHEME_ORDERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_ORDER' in table 'GroupSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSchemes.WGROUP_SCHEME_ORDERColumn] = value;
				}
			}

			// Token: 0x170007A4 RID: 1956
			// (get) Token: 0x06001AA0 RID: 6816 RVA: 0x000562D0 File Offset: 0x000544D0
			// (set) Token: 0x06001AA1 RID: 6817 RVA: 0x00056314 File Offset: 0x00054514
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WGROUP_SCHEME_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableGroupSchemes.WGROUP_SCHEME_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WGROUP_SCHEME_UID' in table 'GroupSchemes' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableGroupSchemes.WGROUP_SCHEME_UIDColumn] = value;
				}
			}

			// Token: 0x06001AA2 RID: 6818 RVA: 0x0005632D File Offset: 0x0005452D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGROUP_SCHEME_NAMENull()
			{
				return base.IsNull(this.tableGroupSchemes.WGROUP_SCHEME_NAMEColumn);
			}

			// Token: 0x06001AA3 RID: 6819 RVA: 0x00056340 File Offset: 0x00054540
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGROUP_SCHEME_NAMENull()
			{
				base[this.tableGroupSchemes.WGROUP_SCHEME_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x06001AA4 RID: 6820 RVA: 0x00056358 File Offset: 0x00054558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWGROUP_SCHEME_ORDERNull()
			{
				return base.IsNull(this.tableGroupSchemes.WGROUP_SCHEME_ORDERColumn);
			}

			// Token: 0x06001AA5 RID: 6821 RVA: 0x0005636B File Offset: 0x0005456B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWGROUP_SCHEME_ORDERNull()
			{
				base[this.tableGroupSchemes.WGROUP_SCHEME_ORDERColumn] = Convert.DBNull;
			}

			// Token: 0x06001AA6 RID: 6822 RVA: 0x00056383 File Offset: 0x00054583
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWGROUP_SCHEME_UIDNull()
			{
				return base.IsNull(this.tableGroupSchemes.WGROUP_SCHEME_UIDColumn);
			}

			// Token: 0x06001AA7 RID: 6823 RVA: 0x00056396 File Offset: 0x00054596
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWGROUP_SCHEME_UIDNull()
			{
				base[this.tableGroupSchemes.WGROUP_SCHEME_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x040005C3 RID: 1475
			private GroupSchemesDataSet.GroupSchemesDataTable tableGroupSchemes;
		}

		// Token: 0x0200016D RID: 365
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupSchemesRowChangeEvent : EventArgs
		{
			// Token: 0x06001AA8 RID: 6824 RVA: 0x000563AE File Offset: 0x000545AE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GroupSchemesRowChangeEvent(GroupSchemesDataSet.GroupSchemesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170007A5 RID: 1957
			// (get) Token: 0x06001AA9 RID: 6825 RVA: 0x000563C4 File Offset: 0x000545C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupSchemesDataSet.GroupSchemesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170007A6 RID: 1958
			// (get) Token: 0x06001AAA RID: 6826 RVA: 0x000563CC File Offset: 0x000545CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040005C4 RID: 1476
			private GroupSchemesDataSet.GroupSchemesRow eventRow;

			// Token: 0x040005C5 RID: 1477
			private DataRowAction eventAction;
		}
	}
}
