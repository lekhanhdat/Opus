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
	// Token: 0x0200076E RID: 1902
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlRoot("UserDelegationDataSet")]
	[ToolboxItem(true)]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class UserDelegationDataSet : DataSet
	{
		// Token: 0x0600B756 RID: 46934 RVA: 0x0023BB54 File Offset: 0x00239D54
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.ResourceDelegations, new string[]
			{
				"DELEGATION_UID",
				"RES_UID",
				"RES_NAME",
				"DELEGATION_FINISH",
				"DELEGATE_NAME",
				"DELEGATION_START",
				"DELEGATE_UID"
			});
		}

		// Token: 0x0600B757 RID: 46935 RVA: 0x0023BBB4 File Offset: 0x00239DB4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public UserDelegationDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600B758 RID: 46936 RVA: 0x0023BC08 File Offset: 0x00239E08
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected UserDelegationDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["ResourceDelegations"] != null)
				{
					base.Tables.Add(new UserDelegationDataSet.ResourceDelegationsDataTable(dataSet.Tables["ResourceDelegations"]));
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

		// Token: 0x170037E9 RID: 14313
		// (get) Token: 0x0600B759 RID: 46937 RVA: 0x0023BD65 File Offset: 0x00239F65
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public UserDelegationDataSet.ResourceDelegationsDataTable ResourceDelegations
		{
			get
			{
				return this.tableResourceDelegations;
			}
		}

		// Token: 0x170037EA RID: 14314
		// (get) Token: 0x0600B75A RID: 46938 RVA: 0x0023BD6D File Offset: 0x00239F6D
		// (set) Token: 0x0600B75B RID: 46939 RVA: 0x0023BD75 File Offset: 0x00239F75
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

		// Token: 0x170037EB RID: 14315
		// (get) Token: 0x0600B75C RID: 46940 RVA: 0x0023BD7E File Offset: 0x00239F7E
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

		// Token: 0x170037EC RID: 14316
		// (get) Token: 0x0600B75D RID: 46941 RVA: 0x0023BD86 File Offset: 0x00239F86
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

		// Token: 0x0600B75E RID: 46942 RVA: 0x0023BD8E File Offset: 0x00239F8E
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600B75F RID: 46943 RVA: 0x0023BDA4 File Offset: 0x00239FA4
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			UserDelegationDataSet userDelegationDataSet = (UserDelegationDataSet)base.Clone();
			userDelegationDataSet.InitVars();
			userDelegationDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return userDelegationDataSet;
		}

		// Token: 0x0600B760 RID: 46944 RVA: 0x0023BDD0 File Offset: 0x00239FD0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600B761 RID: 46945 RVA: 0x0023BDD3 File Offset: 0x00239FD3
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600B762 RID: 46946 RVA: 0x0023BDD8 File Offset: 0x00239FD8
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["ResourceDelegations"] != null)
				{
					base.Tables.Add(new UserDelegationDataSet.ResourceDelegationsDataTable(dataSet.Tables["ResourceDelegations"]));
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

		// Token: 0x0600B763 RID: 46947 RVA: 0x0023BEA0 File Offset: 0x0023A0A0
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600B764 RID: 46948 RVA: 0x0023BED4 File Offset: 0x0023A0D4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600B765 RID: 46949 RVA: 0x0023BEDD File Offset: 0x0023A0DD
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableResourceDelegations = (UserDelegationDataSet.ResourceDelegationsDataTable)base.Tables["ResourceDelegations"];
			if (initTable && this.tableResourceDelegations != null)
			{
				this.tableResourceDelegations.InitVars();
			}
		}

		// Token: 0x0600B766 RID: 46950 RVA: 0x0023BF10 File Offset: 0x0023A110
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "UserDelegationDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/UserDelegationDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableResourceDelegations = new UserDelegationDataSet.ResourceDelegationsDataTable();
			base.Tables.Add(this.tableResourceDelegations);
		}

		// Token: 0x0600B767 RID: 46951 RVA: 0x0023BF68 File Offset: 0x0023A168
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeResourceDelegations()
		{
			return false;
		}

		// Token: 0x0600B768 RID: 46952 RVA: 0x0023BF6B File Offset: 0x0023A16B
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600B769 RID: 46953 RVA: 0x0023BF7C File Offset: 0x0023A17C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			UserDelegationDataSet userDelegationDataSet = new UserDelegationDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = userDelegationDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = userDelegationDataSet.GetSchemaSerializable();
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

		// Token: 0x040024F3 RID: 9459
		private UserDelegationDataSet.ResourceDelegationsDataTable tableResourceDelegations;

		// Token: 0x040024F4 RID: 9460
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200076F RID: 1903
		// (Invoke) Token: 0x0600B76B RID: 46955
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourceDelegationsRowChangeEventHandler(object sender, UserDelegationDataSet.ResourceDelegationsRowChangeEvent e);

		// Token: 0x02000770 RID: 1904
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourceDelegationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B76E RID: 46958 RVA: 0x0023C0C4 File Offset: 0x0023A2C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDelegationsDataTable()
			{
				base.TableName = "ResourceDelegations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B76F RID: 46959 RVA: 0x0023C0EC File Offset: 0x0023A2EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourceDelegationsDataTable(DataTable table)
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

			// Token: 0x0600B770 RID: 46960 RVA: 0x0023C194 File Offset: 0x0023A394
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected ResourceDelegationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x170037ED RID: 14317
			// (get) Token: 0x0600B771 RID: 46961 RVA: 0x0023C1A4 File Offset: 0x0023A3A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DELEGATION_UIDColumn
			{
				get
				{
					return this.columnDELEGATION_UID;
				}
			}

			// Token: 0x170037EE RID: 14318
			// (get) Token: 0x0600B772 RID: 46962 RVA: 0x0023C1AC File Offset: 0x0023A3AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x170037EF RID: 14319
			// (get) Token: 0x0600B773 RID: 46963 RVA: 0x0023C1B4 File Offset: 0x0023A3B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_NAMEColumn
			{
				get
				{
					return this.columnRES_NAME;
				}
			}

			// Token: 0x170037F0 RID: 14320
			// (get) Token: 0x0600B774 RID: 46964 RVA: 0x0023C1BC File Offset: 0x0023A3BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DELEGATE_UIDColumn
			{
				get
				{
					return this.columnDELEGATE_UID;
				}
			}

			// Token: 0x170037F1 RID: 14321
			// (get) Token: 0x0600B775 RID: 46965 RVA: 0x0023C1C4 File Offset: 0x0023A3C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DELEGATE_NAMEColumn
			{
				get
				{
					return this.columnDELEGATE_NAME;
				}
			}

			// Token: 0x170037F2 RID: 14322
			// (get) Token: 0x0600B776 RID: 46966 RVA: 0x0023C1CC File Offset: 0x0023A3CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn DELEGATION_STARTColumn
			{
				get
				{
					return this.columnDELEGATION_START;
				}
			}

			// Token: 0x170037F3 RID: 14323
			// (get) Token: 0x0600B777 RID: 46967 RVA: 0x0023C1D4 File Offset: 0x0023A3D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn DELEGATION_FINISHColumn
			{
				get
				{
					return this.columnDELEGATION_FINISH;
				}
			}

			// Token: 0x170037F4 RID: 14324
			// (get) Token: 0x0600B778 RID: 46968 RVA: 0x0023C1DC File Offset: 0x0023A3DC
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

			// Token: 0x170037F5 RID: 14325
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserDelegationDataSet.ResourceDelegationsRow this[int index]
			{
				get
				{
					return (UserDelegationDataSet.ResourceDelegationsRow)base.Rows[index];
				}
			}

			// Token: 0x14000681 RID: 1665
			// (add) Token: 0x0600B77A RID: 46970 RVA: 0x0023C1FC File Offset: 0x0023A3FC
			// (remove) Token: 0x0600B77B RID: 46971 RVA: 0x0023C234 File Offset: 0x0023A434
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event UserDelegationDataSet.ResourceDelegationsRowChangeEventHandler ResourceDelegationsRowChanging;

			// Token: 0x14000682 RID: 1666
			// (add) Token: 0x0600B77C RID: 46972 RVA: 0x0023C26C File Offset: 0x0023A46C
			// (remove) Token: 0x0600B77D RID: 46973 RVA: 0x0023C2A4 File Offset: 0x0023A4A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event UserDelegationDataSet.ResourceDelegationsRowChangeEventHandler ResourceDelegationsRowChanged;

			// Token: 0x14000683 RID: 1667
			// (add) Token: 0x0600B77E RID: 46974 RVA: 0x0023C2DC File Offset: 0x0023A4DC
			// (remove) Token: 0x0600B77F RID: 46975 RVA: 0x0023C314 File Offset: 0x0023A514
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event UserDelegationDataSet.ResourceDelegationsRowChangeEventHandler ResourceDelegationsRowDeleting;

			// Token: 0x14000684 RID: 1668
			// (add) Token: 0x0600B780 RID: 46976 RVA: 0x0023C34C File Offset: 0x0023A54C
			// (remove) Token: 0x0600B781 RID: 46977 RVA: 0x0023C384 File Offset: 0x0023A584
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event UserDelegationDataSet.ResourceDelegationsRowChangeEventHandler ResourceDelegationsRowDeleted;

			// Token: 0x0600B782 RID: 46978 RVA: 0x0023C3B9 File Offset: 0x0023A5B9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddResourceDelegationsRow(UserDelegationDataSet.ResourceDelegationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B783 RID: 46979 RVA: 0x0023C3C8 File Offset: 0x0023A5C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserDelegationDataSet.ResourceDelegationsRow AddResourceDelegationsRow(Guid DELEGATION_UID, Guid RES_UID, string RES_NAME, Guid DELEGATE_UID, string DELEGATE_NAME, DateTime DELEGATION_START, DateTime DELEGATION_FINISH)
			{
				UserDelegationDataSet.ResourceDelegationsRow resourceDelegationsRow = (UserDelegationDataSet.ResourceDelegationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					DELEGATION_UID,
					RES_UID,
					RES_NAME,
					DELEGATE_UID,
					DELEGATE_NAME,
					DELEGATION_START,
					DELEGATION_FINISH
				};
				resourceDelegationsRow.ItemArray = itemArray;
				base.Rows.Add(resourceDelegationsRow);
				return resourceDelegationsRow;
			}

			// Token: 0x0600B784 RID: 46980 RVA: 0x0023C437 File Offset: 0x0023A637
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B785 RID: 46981 RVA: 0x0023C444 File Offset: 0x0023A644
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				UserDelegationDataSet.ResourceDelegationsDataTable resourceDelegationsDataTable = (UserDelegationDataSet.ResourceDelegationsDataTable)base.Clone();
				resourceDelegationsDataTable.InitVars();
				return resourceDelegationsDataTable;
			}

			// Token: 0x0600B786 RID: 46982 RVA: 0x0023C464 File Offset: 0x0023A664
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new UserDelegationDataSet.ResourceDelegationsDataTable();
			}

			// Token: 0x0600B787 RID: 46983 RVA: 0x0023C46C File Offset: 0x0023A66C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnDELEGATION_UID = base.Columns["DELEGATION_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnRES_NAME = base.Columns["RES_NAME"];
				this.columnDELEGATE_UID = base.Columns["DELEGATE_UID"];
				this.columnDELEGATE_NAME = base.Columns["DELEGATE_NAME"];
				this.columnDELEGATION_START = base.Columns["DELEGATION_START"];
				this.columnDELEGATION_FINISH = base.Columns["DELEGATION_FINISH"];
			}

			// Token: 0x0600B788 RID: 46984 RVA: 0x0023C514 File Offset: 0x0023A714
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnDELEGATION_UID = new DataColumn("DELEGATION_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDELEGATION_UID);
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnRES_NAME = new DataColumn("RES_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnRES_NAME);
				this.columnDELEGATE_UID = new DataColumn("DELEGATE_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnDELEGATE_UID);
				this.columnDELEGATE_NAME = new DataColumn("DELEGATE_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnDELEGATE_NAME);
				this.columnDELEGATION_START = new DataColumn("DELEGATION_START", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnDELEGATION_START);
				this.columnDELEGATION_FINISH = new DataColumn("DELEGATION_FINISH", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnDELEGATION_FINISH);
				this.columnDELEGATION_UID.AllowDBNull = false;
				this.columnRES_UID.AllowDBNull = false;
				this.columnRES_NAME.ReadOnly = true;
				this.columnDELEGATE_UID.AllowDBNull = false;
				this.columnDELEGATE_NAME.ReadOnly = true;
				this.columnDELEGATION_START.AllowDBNull = false;
				this.columnDELEGATION_FINISH.AllowDBNull = false;
			}

			// Token: 0x0600B789 RID: 46985 RVA: 0x0023C6B0 File Offset: 0x0023A8B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public UserDelegationDataSet.ResourceDelegationsRow NewResourceDelegationsRow()
			{
				return (UserDelegationDataSet.ResourceDelegationsRow)base.NewRow();
			}

			// Token: 0x0600B78A RID: 46986 RVA: 0x0023C6BD File Offset: 0x0023A8BD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new UserDelegationDataSet.ResourceDelegationsRow(builder);
			}

			// Token: 0x0600B78B RID: 46987 RVA: 0x0023C6C5 File Offset: 0x0023A8C5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(UserDelegationDataSet.ResourceDelegationsRow);
			}

			// Token: 0x0600B78C RID: 46988 RVA: 0x0023C6D1 File Offset: 0x0023A8D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourceDelegationsRowChanged != null)
				{
					this.ResourceDelegationsRowChanged(this, new UserDelegationDataSet.ResourceDelegationsRowChangeEvent((UserDelegationDataSet.ResourceDelegationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B78D RID: 46989 RVA: 0x0023C704 File Offset: 0x0023A904
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourceDelegationsRowChanging != null)
				{
					this.ResourceDelegationsRowChanging(this, new UserDelegationDataSet.ResourceDelegationsRowChangeEvent((UserDelegationDataSet.ResourceDelegationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B78E RID: 46990 RVA: 0x0023C737 File Offset: 0x0023A937
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourceDelegationsRowDeleted != null)
				{
					this.ResourceDelegationsRowDeleted(this, new UserDelegationDataSet.ResourceDelegationsRowChangeEvent((UserDelegationDataSet.ResourceDelegationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B78F RID: 46991 RVA: 0x0023C76A File Offset: 0x0023A96A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourceDelegationsRowDeleting != null)
				{
					this.ResourceDelegationsRowDeleting(this, new UserDelegationDataSet.ResourceDelegationsRowChangeEvent((UserDelegationDataSet.ResourceDelegationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B790 RID: 46992 RVA: 0x0023C79D File Offset: 0x0023A99D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveResourceDelegationsRow(UserDelegationDataSet.ResourceDelegationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B791 RID: 46993 RVA: 0x0023C7AC File Offset: 0x0023A9AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				UserDelegationDataSet userDelegationDataSet = new UserDelegationDataSet();
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
				xmlSchemaAttribute.FixedValue = userDelegationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourceDelegationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = userDelegationDataSet.GetSchemaSerializable();
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

			// Token: 0x040024F5 RID: 9461
			private DataColumn columnDELEGATION_UID;

			// Token: 0x040024F6 RID: 9462
			private DataColumn columnRES_UID;

			// Token: 0x040024F7 RID: 9463
			private DataColumn columnRES_NAME;

			// Token: 0x040024F8 RID: 9464
			private DataColumn columnDELEGATE_UID;

			// Token: 0x040024F9 RID: 9465
			private DataColumn columnDELEGATE_NAME;

			// Token: 0x040024FA RID: 9466
			private DataColumn columnDELEGATION_START;

			// Token: 0x040024FB RID: 9467
			private DataColumn columnDELEGATION_FINISH;
		}

		// Token: 0x02000771 RID: 1905
		public class ResourceDelegationsRow : DataRow
		{
			// Token: 0x0600B792 RID: 46994 RVA: 0x0023C9A4 File Offset: 0x0023ABA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourceDelegationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResourceDelegations = (UserDelegationDataSet.ResourceDelegationsDataTable)base.Table;
			}

			// Token: 0x170037F6 RID: 14326
			// (get) Token: 0x0600B793 RID: 46995 RVA: 0x0023C9BE File Offset: 0x0023ABBE
			// (set) Token: 0x0600B794 RID: 46996 RVA: 0x0023C9D6 File Offset: 0x0023ABD6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid DELEGATION_UID
			{
				get
				{
					return (Guid)base[this.tableResourceDelegations.DELEGATION_UIDColumn];
				}
				set
				{
					base[this.tableResourceDelegations.DELEGATION_UIDColumn] = value;
				}
			}

			// Token: 0x170037F7 RID: 14327
			// (get) Token: 0x0600B795 RID: 46997 RVA: 0x0023C9EF File Offset: 0x0023ABEF
			// (set) Token: 0x0600B796 RID: 46998 RVA: 0x0023CA07 File Offset: 0x0023AC07
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResourceDelegations.RES_UIDColumn];
				}
				set
				{
					base[this.tableResourceDelegations.RES_UIDColumn] = value;
				}
			}

			// Token: 0x170037F8 RID: 14328
			// (get) Token: 0x0600B797 RID: 46999 RVA: 0x0023CA20 File Offset: 0x0023AC20
			// (set) Token: 0x0600B798 RID: 47000 RVA: 0x0023CA64 File Offset: 0x0023AC64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string RES_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResourceDelegations.RES_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_NAME' in table 'ResourceDelegations' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceDelegations.RES_NAMEColumn] = value;
				}
			}

			// Token: 0x170037F9 RID: 14329
			// (get) Token: 0x0600B799 RID: 47001 RVA: 0x0023CA78 File Offset: 0x0023AC78
			// (set) Token: 0x0600B79A RID: 47002 RVA: 0x0023CA90 File Offset: 0x0023AC90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid DELEGATE_UID
			{
				get
				{
					return (Guid)base[this.tableResourceDelegations.DELEGATE_UIDColumn];
				}
				set
				{
					base[this.tableResourceDelegations.DELEGATE_UIDColumn] = value;
				}
			}

			// Token: 0x170037FA RID: 14330
			// (get) Token: 0x0600B79B RID: 47003 RVA: 0x0023CAAC File Offset: 0x0023ACAC
			// (set) Token: 0x0600B79C RID: 47004 RVA: 0x0023CAF0 File Offset: 0x0023ACF0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string DELEGATE_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResourceDelegations.DELEGATE_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'DELEGATE_NAME' in table 'ResourceDelegations' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResourceDelegations.DELEGATE_NAMEColumn] = value;
				}
			}

			// Token: 0x170037FB RID: 14331
			// (get) Token: 0x0600B79D RID: 47005 RVA: 0x0023CB04 File Offset: 0x0023AD04
			// (set) Token: 0x0600B79E RID: 47006 RVA: 0x0023CB1C File Offset: 0x0023AD1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime DELEGATION_START
			{
				get
				{
					return (DateTime)base[this.tableResourceDelegations.DELEGATION_STARTColumn];
				}
				set
				{
					base[this.tableResourceDelegations.DELEGATION_STARTColumn] = value;
				}
			}

			// Token: 0x170037FC RID: 14332
			// (get) Token: 0x0600B79F RID: 47007 RVA: 0x0023CB35 File Offset: 0x0023AD35
			// (set) Token: 0x0600B7A0 RID: 47008 RVA: 0x0023CB4D File Offset: 0x0023AD4D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime DELEGATION_FINISH
			{
				get
				{
					return (DateTime)base[this.tableResourceDelegations.DELEGATION_FINISHColumn];
				}
				set
				{
					base[this.tableResourceDelegations.DELEGATION_FINISHColumn] = value;
				}
			}

			// Token: 0x0600B7A1 RID: 47009 RVA: 0x0023CB66 File Offset: 0x0023AD66
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_NAMENull()
			{
				return base.IsNull(this.tableResourceDelegations.RES_NAMEColumn);
			}

			// Token: 0x0600B7A2 RID: 47010 RVA: 0x0023CB79 File Offset: 0x0023AD79
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_NAMENull()
			{
				base[this.tableResourceDelegations.RES_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600B7A3 RID: 47011 RVA: 0x0023CB91 File Offset: 0x0023AD91
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsDELEGATE_NAMENull()
			{
				return base.IsNull(this.tableResourceDelegations.DELEGATE_NAMEColumn);
			}

			// Token: 0x0600B7A4 RID: 47012 RVA: 0x0023CBA4 File Offset: 0x0023ADA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetDELEGATE_NAMENull()
			{
				base[this.tableResourceDelegations.DELEGATE_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x04002500 RID: 9472
			private UserDelegationDataSet.ResourceDelegationsDataTable tableResourceDelegations;
		}

		// Token: 0x02000772 RID: 1906
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourceDelegationsRowChangeEvent : EventArgs
		{
			// Token: 0x0600B7A5 RID: 47013 RVA: 0x0023CBBC File Offset: 0x0023ADBC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceDelegationsRowChangeEvent(UserDelegationDataSet.ResourceDelegationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170037FD RID: 14333
			// (get) Token: 0x0600B7A6 RID: 47014 RVA: 0x0023CBD2 File Offset: 0x0023ADD2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public UserDelegationDataSet.ResourceDelegationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170037FE RID: 14334
			// (get) Token: 0x0600B7A7 RID: 47015 RVA: 0x0023CBDA File Offset: 0x0023ADDA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04002501 RID: 9473
			private UserDelegationDataSet.ResourceDelegationsRow eventRow;

			// Token: 0x04002502 RID: 9474
			private DataRowAction eventAction;
		}
	}
}
