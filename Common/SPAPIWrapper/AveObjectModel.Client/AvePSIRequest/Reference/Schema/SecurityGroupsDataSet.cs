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
	// Token: 0x020005F1 RID: 1521
	[ToolboxItem(true)]
	[DesignerCategory("code")]
	[HelpKeyword("vs.data.DataSet")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[XmlRoot("SecurityGroupsDataSet")]
	[Serializable]
	public class SecurityGroupsDataSet : DataSet
	{
		// Token: 0x06008FBC RID: 36796 RVA: 0x001C1F48 File Offset: 0x001C0148
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupMembers, new string[]
			{
				"RES_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityPrincipleCategoryRelations, new string[]
			{
				"WSEC_CAT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GlobalPermissions, new string[]
			{
				"WSEC_DENY",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.CategoryPermissions, new string[]
			{
				"WSEC_DENY",
				"WSEC_CAT_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityGroups, new string[]
			{
				"WSEC_GRP_AD_GUID",
				"WSEC_GRP_DESC",
				"WSEC_GRP_NAME",
				"WSEC_GRP_AD_GROUP",
				"WSEC_GRP_AD_LOG",
				"WSEC_GRP_AD_LASTSYNC",
				"WSEC_GRP_UID"
			});
		}

		// Token: 0x06008FBD RID: 36797 RVA: 0x001C2064 File Offset: 0x001C0264
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public SecurityGroupsDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06008FBE RID: 36798 RVA: 0x001C20B8 File Offset: 0x001C02B8
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected SecurityGroupsDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["SecurityGroups"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.SecurityGroupsDataTable(dataSet.Tables["SecurityGroups"]));
				}
				if (dataSet.Tables["SecurityPrincipleCategoryRelations"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable(dataSet.Tables["SecurityPrincipleCategoryRelations"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
				}
				if (dataSet.Tables["GroupMembers"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.GroupMembersDataTable(dataSet.Tables["GroupMembers"]));
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

		// Token: 0x17002B5C RID: 11100
		// (get) Token: 0x06008FBF RID: 36799 RVA: 0x001C22DD File Offset: 0x001C04DD
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public SecurityGroupsDataSet.SecurityGroupsDataTable SecurityGroups
		{
			get
			{
				return this.tableSecurityGroups;
			}
		}

		// Token: 0x17002B5D RID: 11101
		// (get) Token: 0x06008FC0 RID: 36800 RVA: 0x001C22E5 File Offset: 0x001C04E5
		[DebuggerNonUserCode]
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable SecurityPrincipleCategoryRelations
		{
			get
			{
				return this.tableSecurityPrincipleCategoryRelations;
			}
		}

		// Token: 0x17002B5E RID: 11102
		// (get) Token: 0x06008FC1 RID: 36801 RVA: 0x001C22ED File Offset: 0x001C04ED
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public SecurityGroupsDataSet.CategoryPermissionsDataTable CategoryPermissions
		{
			get
			{
				return this.tableCategoryPermissions;
			}
		}

		// Token: 0x17002B5F RID: 11103
		// (get) Token: 0x06008FC2 RID: 36802 RVA: 0x001C22F5 File Offset: 0x001C04F5
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[Browsable(false)]
		public SecurityGroupsDataSet.GlobalPermissionsDataTable GlobalPermissions
		{
			get
			{
				return this.tableGlobalPermissions;
			}
		}

		// Token: 0x17002B60 RID: 11104
		// (get) Token: 0x06008FC3 RID: 36803 RVA: 0x001C22FD File Offset: 0x001C04FD
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public SecurityGroupsDataSet.GroupMembersDataTable GroupMembers
		{
			get
			{
				return this.tableGroupMembers;
			}
		}

		// Token: 0x17002B61 RID: 11105
		// (get) Token: 0x06008FC4 RID: 36804 RVA: 0x001C2305 File Offset: 0x001C0505
		// (set) Token: 0x06008FC5 RID: 36805 RVA: 0x001C230D File Offset: 0x001C050D
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

		// Token: 0x17002B62 RID: 11106
		// (get) Token: 0x06008FC6 RID: 36806 RVA: 0x001C2316 File Offset: 0x001C0516
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

		// Token: 0x17002B63 RID: 11107
		// (get) Token: 0x06008FC7 RID: 36807 RVA: 0x001C231E File Offset: 0x001C051E
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

		// Token: 0x06008FC8 RID: 36808 RVA: 0x001C2326 File Offset: 0x001C0526
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06008FC9 RID: 36809 RVA: 0x001C233C File Offset: 0x001C053C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			SecurityGroupsDataSet securityGroupsDataSet = (SecurityGroupsDataSet)base.Clone();
			securityGroupsDataSet.InitVars();
			securityGroupsDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return securityGroupsDataSet;
		}

		// Token: 0x06008FCA RID: 36810 RVA: 0x001C2368 File Offset: 0x001C0568
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x06008FCB RID: 36811 RVA: 0x001C236B File Offset: 0x001C056B
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x06008FCC RID: 36812 RVA: 0x001C2370 File Offset: 0x001C0570
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["SecurityGroups"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.SecurityGroupsDataTable(dataSet.Tables["SecurityGroups"]));
				}
				if (dataSet.Tables["SecurityPrincipleCategoryRelations"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable(dataSet.Tables["SecurityPrincipleCategoryRelations"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
				}
				if (dataSet.Tables["GroupMembers"] != null)
				{
					base.Tables.Add(new SecurityGroupsDataSet.GroupMembersDataTable(dataSet.Tables["GroupMembers"]));
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

		// Token: 0x06008FCD RID: 36813 RVA: 0x001C2500 File Offset: 0x001C0700
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x06008FCE RID: 36814 RVA: 0x001C2534 File Offset: 0x001C0734
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x06008FCF RID: 36815 RVA: 0x001C2540 File Offset: 0x001C0740
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableSecurityGroups = (SecurityGroupsDataSet.SecurityGroupsDataTable)base.Tables["SecurityGroups"];
			if (initTable && this.tableSecurityGroups != null)
			{
				this.tableSecurityGroups.InitVars();
			}
			this.tableSecurityPrincipleCategoryRelations = (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Tables["SecurityPrincipleCategoryRelations"];
			if (initTable && this.tableSecurityPrincipleCategoryRelations != null)
			{
				this.tableSecurityPrincipleCategoryRelations.InitVars();
			}
			this.tableCategoryPermissions = (SecurityGroupsDataSet.CategoryPermissionsDataTable)base.Tables["CategoryPermissions"];
			if (initTable && this.tableCategoryPermissions != null)
			{
				this.tableCategoryPermissions.InitVars();
			}
			this.tableGlobalPermissions = (SecurityGroupsDataSet.GlobalPermissionsDataTable)base.Tables["GlobalPermissions"];
			if (initTable && this.tableGlobalPermissions != null)
			{
				this.tableGlobalPermissions.InitVars();
			}
			this.tableGroupMembers = (SecurityGroupsDataSet.GroupMembersDataTable)base.Tables["GroupMembers"];
			if (initTable && this.tableGroupMembers != null)
			{
				this.tableGroupMembers.InitVars();
			}
		}

		// Token: 0x06008FD0 RID: 36816 RVA: 0x001C2644 File Offset: 0x001C0844
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "SecurityGroupsDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/SecurityGroupsDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSecurityGroups = new SecurityGroupsDataSet.SecurityGroupsDataTable();
			base.Tables.Add(this.tableSecurityGroups);
			this.tableSecurityPrincipleCategoryRelations = new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable();
			base.Tables.Add(this.tableSecurityPrincipleCategoryRelations);
			this.tableCategoryPermissions = new SecurityGroupsDataSet.CategoryPermissionsDataTable();
			base.Tables.Add(this.tableCategoryPermissions);
			this.tableGlobalPermissions = new SecurityGroupsDataSet.GlobalPermissionsDataTable();
			base.Tables.Add(this.tableGlobalPermissions);
			this.tableGroupMembers = new SecurityGroupsDataSet.GroupMembersDataTable();
			base.Tables.Add(this.tableGroupMembers);
		}

		// Token: 0x06008FD1 RID: 36817 RVA: 0x001C270C File Offset: 0x001C090C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSecurityGroups()
		{
			return false;
		}

		// Token: 0x06008FD2 RID: 36818 RVA: 0x001C270F File Offset: 0x001C090F
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSecurityPrincipleCategoryRelations()
		{
			return false;
		}

		// Token: 0x06008FD3 RID: 36819 RVA: 0x001C2712 File Offset: 0x001C0912
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeCategoryPermissions()
		{
			return false;
		}

		// Token: 0x06008FD4 RID: 36820 RVA: 0x001C2715 File Offset: 0x001C0915
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGlobalPermissions()
		{
			return false;
		}

		// Token: 0x06008FD5 RID: 36821 RVA: 0x001C2718 File Offset: 0x001C0918
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGroupMembers()
		{
			return false;
		}

		// Token: 0x06008FD6 RID: 36822 RVA: 0x001C271B File Offset: 0x001C091B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06008FD7 RID: 36823 RVA: 0x001C272C File Offset: 0x001C092C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = securityGroupsDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

		// Token: 0x04001CD2 RID: 7378
		private SecurityGroupsDataSet.SecurityGroupsDataTable tableSecurityGroups;

		// Token: 0x04001CD3 RID: 7379
		private SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable tableSecurityPrincipleCategoryRelations;

		// Token: 0x04001CD4 RID: 7380
		private SecurityGroupsDataSet.CategoryPermissionsDataTable tableCategoryPermissions;

		// Token: 0x04001CD5 RID: 7381
		private SecurityGroupsDataSet.GlobalPermissionsDataTable tableGlobalPermissions;

		// Token: 0x04001CD6 RID: 7382
		private SecurityGroupsDataSet.GroupMembersDataTable tableGroupMembers;

		// Token: 0x04001CD7 RID: 7383
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x020005F2 RID: 1522
		// (Invoke) Token: 0x06008FD9 RID: 36825
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityGroupsRowChangeEventHandler(object sender, SecurityGroupsDataSet.SecurityGroupsRowChangeEvent e);

		// Token: 0x020005F3 RID: 1523
		// (Invoke) Token: 0x06008FDD RID: 36829
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityPrincipleCategoryRelationsRowChangeEventHandler(object sender, SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent e);

		// Token: 0x020005F4 RID: 1524
		// (Invoke) Token: 0x06008FE1 RID: 36833
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CategoryPermissionsRowChangeEventHandler(object sender, SecurityGroupsDataSet.CategoryPermissionsRowChangeEvent e);

		// Token: 0x020005F5 RID: 1525
		// (Invoke) Token: 0x06008FE5 RID: 36837
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GlobalPermissionsRowChangeEventHandler(object sender, SecurityGroupsDataSet.GlobalPermissionsRowChangeEvent e);

		// Token: 0x020005F6 RID: 1526
		// (Invoke) Token: 0x06008FE9 RID: 36841
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupMembersRowChangeEventHandler(object sender, SecurityGroupsDataSet.GroupMembersRowChangeEvent e);

		// Token: 0x020005F7 RID: 1527
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityGroupsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008FEC RID: 36844 RVA: 0x001C2874 File Offset: 0x001C0A74
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataTable()
			{
				base.TableName = "SecurityGroups";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008FED RID: 36845 RVA: 0x001C289C File Offset: 0x001C0A9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SecurityGroupsDataTable(DataTable table)
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

			// Token: 0x06008FEE RID: 36846 RVA: 0x001C2944 File Offset: 0x001C0B44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SecurityGroupsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002B64 RID: 11108
			// (get) Token: 0x06008FEF RID: 36847 RVA: 0x001C2954 File Offset: 0x001C0B54
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002B65 RID: 11109
			// (get) Token: 0x06008FF0 RID: 36848 RVA: 0x001C295C File Offset: 0x001C0B5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_GRP_NAMEColumn
			{
				get
				{
					return this.columnWSEC_GRP_NAME;
				}
			}

			// Token: 0x17002B66 RID: 11110
			// (get) Token: 0x06008FF1 RID: 36849 RVA: 0x001C2964 File Offset: 0x001C0B64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_DESCColumn
			{
				get
				{
					return this.columnWSEC_GRP_DESC;
				}
			}

			// Token: 0x17002B67 RID: 11111
			// (get) Token: 0x06008FF2 RID: 36850 RVA: 0x001C296C File Offset: 0x001C0B6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_AD_GROUPColumn
			{
				get
				{
					return this.columnWSEC_GRP_AD_GROUP;
				}
			}

			// Token: 0x17002B68 RID: 11112
			// (get) Token: 0x06008FF3 RID: 36851 RVA: 0x001C2974 File Offset: 0x001C0B74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_AD_GUIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_AD_GUID;
				}
			}

			// Token: 0x17002B69 RID: 11113
			// (get) Token: 0x06008FF4 RID: 36852 RVA: 0x001C297C File Offset: 0x001C0B7C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_GRP_AD_LOGColumn
			{
				get
				{
					return this.columnWSEC_GRP_AD_LOG;
				}
			}

			// Token: 0x17002B6A RID: 11114
			// (get) Token: 0x06008FF5 RID: 36853 RVA: 0x001C2984 File Offset: 0x001C0B84
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_GRP_AD_LASTSYNCColumn
			{
				get
				{
					return this.columnWSEC_GRP_AD_LASTSYNC;
				}
			}

			// Token: 0x17002B6B RID: 11115
			// (get) Token: 0x06008FF6 RID: 36854 RVA: 0x001C298C File Offset: 0x001C0B8C
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

			// Token: 0x17002B6C RID: 11116
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.SecurityGroupsRow this[int index]
			{
				get
				{
					return (SecurityGroupsDataSet.SecurityGroupsRow)base.Rows[index];
				}
			}

			// Token: 0x14000521 RID: 1313
			// (add) Token: 0x06008FF8 RID: 36856 RVA: 0x001C29AC File Offset: 0x001C0BAC
			// (remove) Token: 0x06008FF9 RID: 36857 RVA: 0x001C29E4 File Offset: 0x001C0BE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityGroupsRowChangeEventHandler SecurityGroupsRowChanging;

			// Token: 0x14000522 RID: 1314
			// (add) Token: 0x06008FFA RID: 36858 RVA: 0x001C2A1C File Offset: 0x001C0C1C
			// (remove) Token: 0x06008FFB RID: 36859 RVA: 0x001C2A54 File Offset: 0x001C0C54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityGroupsRowChangeEventHandler SecurityGroupsRowChanged;

			// Token: 0x14000523 RID: 1315
			// (add) Token: 0x06008FFC RID: 36860 RVA: 0x001C2A8C File Offset: 0x001C0C8C
			// (remove) Token: 0x06008FFD RID: 36861 RVA: 0x001C2AC4 File Offset: 0x001C0CC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityGroupsRowChangeEventHandler SecurityGroupsRowDeleting;

			// Token: 0x14000524 RID: 1316
			// (add) Token: 0x06008FFE RID: 36862 RVA: 0x001C2AFC File Offset: 0x001C0CFC
			// (remove) Token: 0x06008FFF RID: 36863 RVA: 0x001C2B34 File Offset: 0x001C0D34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityGroupsRowChangeEventHandler SecurityGroupsRowDeleted;

			// Token: 0x06009000 RID: 36864 RVA: 0x001C2B69 File Offset: 0x001C0D69
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSecurityGroupsRow(SecurityGroupsDataSet.SecurityGroupsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009001 RID: 36865 RVA: 0x001C2B78 File Offset: 0x001C0D78
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.SecurityGroupsRow AddSecurityGroupsRow(Guid WSEC_GRP_UID, string WSEC_GRP_NAME, string WSEC_GRP_DESC, string WSEC_GRP_AD_GROUP, Guid WSEC_GRP_AD_GUID, short WSEC_GRP_AD_LOG, DateTime WSEC_GRP_AD_LASTSYNC)
			{
				SecurityGroupsDataSet.SecurityGroupsRow securityGroupsRow = (SecurityGroupsDataSet.SecurityGroupsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_GRP_UID,
					WSEC_GRP_NAME,
					WSEC_GRP_DESC,
					WSEC_GRP_AD_GROUP,
					WSEC_GRP_AD_GUID,
					WSEC_GRP_AD_LOG,
					WSEC_GRP_AD_LASTSYNC
				};
				securityGroupsRow.ItemArray = itemArray;
				base.Rows.Add(securityGroupsRow);
				return securityGroupsRow;
			}

			// Token: 0x06009002 RID: 36866 RVA: 0x001C2BE4 File Offset: 0x001C0DE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.SecurityGroupsRow FindByWSEC_GRP_UID(Guid WSEC_GRP_UID)
			{
				return (SecurityGroupsDataSet.SecurityGroupsRow)base.Rows.Find(new object[]
				{
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009003 RID: 36867 RVA: 0x001C2C12 File Offset: 0x001C0E12
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009004 RID: 36868 RVA: 0x001C2C20 File Offset: 0x001C0E20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityGroupsDataSet.SecurityGroupsDataTable securityGroupsDataTable = (SecurityGroupsDataSet.SecurityGroupsDataTable)base.Clone();
				securityGroupsDataTable.InitVars();
				return securityGroupsDataTable;
			}

			// Token: 0x06009005 RID: 36869 RVA: 0x001C2C40 File Offset: 0x001C0E40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityGroupsDataSet.SecurityGroupsDataTable();
			}

			// Token: 0x06009006 RID: 36870 RVA: 0x001C2C48 File Offset: 0x001C0E48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_GRP_NAME = base.Columns["WSEC_GRP_NAME"];
				this.columnWSEC_GRP_DESC = base.Columns["WSEC_GRP_DESC"];
				this.columnWSEC_GRP_AD_GROUP = base.Columns["WSEC_GRP_AD_GROUP"];
				this.columnWSEC_GRP_AD_GUID = base.Columns["WSEC_GRP_AD_GUID"];
				this.columnWSEC_GRP_AD_LOG = base.Columns["WSEC_GRP_AD_LOG"];
				this.columnWSEC_GRP_AD_LASTSYNC = base.Columns["WSEC_GRP_AD_LASTSYNC"];
			}

			// Token: 0x06009007 RID: 36871 RVA: 0x001C2CF0 File Offset: 0x001C0EF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnWSEC_GRP_NAME = new DataColumn("WSEC_GRP_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_NAME);
				this.columnWSEC_GRP_DESC = new DataColumn("WSEC_GRP_DESC", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_DESC);
				this.columnWSEC_GRP_AD_GROUP = new DataColumn("WSEC_GRP_AD_GROUP", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_AD_GROUP);
				this.columnWSEC_GRP_AD_GUID = new DataColumn("WSEC_GRP_AD_GUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_AD_GUID);
				this.columnWSEC_GRP_AD_LOG = new DataColumn("WSEC_GRP_AD_LOG", typeof(short), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_AD_LOG);
				this.columnWSEC_GRP_AD_LASTSYNC = new DataColumn("WSEC_GRP_AD_LASTSYNC", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_AD_LASTSYNC);
				base.Constraints.Add(new UniqueConstraint("SecurityGroupsDataSetKey1", new DataColumn[]
				{
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_GRP_UID.Unique = true;
				this.columnWSEC_GRP_NAME.AllowDBNull = false;
			}

			// Token: 0x06009008 RID: 36872 RVA: 0x001C2E83 File Offset: 0x001C1083
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.SecurityGroupsRow NewSecurityGroupsRow()
			{
				return (SecurityGroupsDataSet.SecurityGroupsRow)base.NewRow();
			}

			// Token: 0x06009009 RID: 36873 RVA: 0x001C2E90 File Offset: 0x001C1090
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityGroupsDataSet.SecurityGroupsRow(builder);
			}

			// Token: 0x0600900A RID: 36874 RVA: 0x001C2E98 File Offset: 0x001C1098
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityGroupsDataSet.SecurityGroupsRow);
			}

			// Token: 0x0600900B RID: 36875 RVA: 0x001C2EA4 File Offset: 0x001C10A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityGroupsRowChanged != null)
				{
					this.SecurityGroupsRowChanged(this, new SecurityGroupsDataSet.SecurityGroupsRowChangeEvent((SecurityGroupsDataSet.SecurityGroupsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600900C RID: 36876 RVA: 0x001C2ED7 File Offset: 0x001C10D7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityGroupsRowChanging != null)
				{
					this.SecurityGroupsRowChanging(this, new SecurityGroupsDataSet.SecurityGroupsRowChangeEvent((SecurityGroupsDataSet.SecurityGroupsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600900D RID: 36877 RVA: 0x001C2F0A File Offset: 0x001C110A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityGroupsRowDeleted != null)
				{
					this.SecurityGroupsRowDeleted(this, new SecurityGroupsDataSet.SecurityGroupsRowChangeEvent((SecurityGroupsDataSet.SecurityGroupsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600900E RID: 36878 RVA: 0x001C2F3D File Offset: 0x001C113D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityGroupsRowDeleting != null)
				{
					this.SecurityGroupsRowDeleting(this, new SecurityGroupsDataSet.SecurityGroupsRowChangeEvent((SecurityGroupsDataSet.SecurityGroupsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600900F RID: 36879 RVA: 0x001C2F70 File Offset: 0x001C1170
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityGroupsRow(SecurityGroupsDataSet.SecurityGroupsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009010 RID: 36880 RVA: 0x001C2F80 File Offset: 0x001C1180
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
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
				xmlSchemaAttribute.FixedValue = securityGroupsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityGroupsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

			// Token: 0x04001CD8 RID: 7384
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001CD9 RID: 7385
			private DataColumn columnWSEC_GRP_NAME;

			// Token: 0x04001CDA RID: 7386
			private DataColumn columnWSEC_GRP_DESC;

			// Token: 0x04001CDB RID: 7387
			private DataColumn columnWSEC_GRP_AD_GROUP;

			// Token: 0x04001CDC RID: 7388
			private DataColumn columnWSEC_GRP_AD_GUID;

			// Token: 0x04001CDD RID: 7389
			private DataColumn columnWSEC_GRP_AD_LOG;

			// Token: 0x04001CDE RID: 7390
			private DataColumn columnWSEC_GRP_AD_LASTSYNC;
		}

		// Token: 0x020005F8 RID: 1528
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityPrincipleCategoryRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009011 RID: 36881 RVA: 0x001C3178 File Offset: 0x001C1378
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityPrincipleCategoryRelationsDataTable()
			{
				base.TableName = "SecurityPrincipleCategoryRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009012 RID: 36882 RVA: 0x001C31A0 File Offset: 0x001C13A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SecurityPrincipleCategoryRelationsDataTable(DataTable table)
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

			// Token: 0x06009013 RID: 36883 RVA: 0x001C3248 File Offset: 0x001C1448
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SecurityPrincipleCategoryRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002B6D RID: 11117
			// (get) Token: 0x06009014 RID: 36884 RVA: 0x001C3258 File Offset: 0x001C1458
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002B6E RID: 11118
			// (get) Token: 0x06009015 RID: 36885 RVA: 0x001C3260 File Offset: 0x001C1460
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002B6F RID: 11119
			// (get) Token: 0x06009016 RID: 36886 RVA: 0x001C3268 File Offset: 0x001C1468
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

			// Token: 0x17002B70 RID: 11120
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow this[int index]
			{
				get
				{
					return (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x14000525 RID: 1317
			// (add) Token: 0x06009018 RID: 36888 RVA: 0x001C3288 File Offset: 0x001C1488
			// (remove) Token: 0x06009019 RID: 36889 RVA: 0x001C32C0 File Offset: 0x001C14C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowChanging;

			// Token: 0x14000526 RID: 1318
			// (add) Token: 0x0600901A RID: 36890 RVA: 0x001C32F8 File Offset: 0x001C14F8
			// (remove) Token: 0x0600901B RID: 36891 RVA: 0x001C3330 File Offset: 0x001C1530
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowChanged;

			// Token: 0x14000527 RID: 1319
			// (add) Token: 0x0600901C RID: 36892 RVA: 0x001C3368 File Offset: 0x001C1568
			// (remove) Token: 0x0600901D RID: 36893 RVA: 0x001C33A0 File Offset: 0x001C15A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowDeleting;

			// Token: 0x14000528 RID: 1320
			// (add) Token: 0x0600901E RID: 36894 RVA: 0x001C33D8 File Offset: 0x001C15D8
			// (remove) Token: 0x0600901F RID: 36895 RVA: 0x001C3410 File Offset: 0x001C1610
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowDeleted;

			// Token: 0x06009020 RID: 36896 RVA: 0x001C3445 File Offset: 0x001C1645
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSecurityPrincipleCategoryRelationsRow(SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009021 RID: 36897 RVA: 0x001C3454 File Offset: 0x001C1654
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow AddSecurityPrincipleCategoryRelationsRow(Guid WSEC_GRP_UID, Guid WSEC_CAT_UID)
			{
				SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow securityPrincipleCategoryRelationsRow = (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_GRP_UID,
					WSEC_CAT_UID
				};
				securityPrincipleCategoryRelationsRow.ItemArray = itemArray;
				base.Rows.Add(securityPrincipleCategoryRelationsRow);
				return securityPrincipleCategoryRelationsRow;
			}

			// Token: 0x06009022 RID: 36898 RVA: 0x001C349C File Offset: 0x001C169C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow FindByWSEC_CAT_UIDWSEC_GRP_UID(Guid WSEC_CAT_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009023 RID: 36899 RVA: 0x001C34D3 File Offset: 0x001C16D3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009024 RID: 36900 RVA: 0x001C34E0 File Offset: 0x001C16E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable securityPrincipleCategoryRelationsDataTable = (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Clone();
				securityPrincipleCategoryRelationsDataTable.InitVars();
				return securityPrincipleCategoryRelationsDataTable;
			}

			// Token: 0x06009025 RID: 36901 RVA: 0x001C3500 File Offset: 0x001C1700
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable();
			}

			// Token: 0x06009026 RID: 36902 RVA: 0x001C3507 File Offset: 0x001C1707
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
			}

			// Token: 0x06009027 RID: 36903 RVA: 0x001C3538 File Offset: 0x001C1738
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				base.Constraints.Add(new UniqueConstraint("SecurityGroupsDataSetKey2", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.AllowDBNull = false;
			}

			// Token: 0x06009028 RID: 36904 RVA: 0x001C35E7 File Offset: 0x001C17E7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow NewSecurityPrincipleCategoryRelationsRow()
			{
				return (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)base.NewRow();
			}

			// Token: 0x06009029 RID: 36905 RVA: 0x001C35F4 File Offset: 0x001C17F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow(builder);
			}

			// Token: 0x0600902A RID: 36906 RVA: 0x001C35FC File Offset: 0x001C17FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow);
			}

			// Token: 0x0600902B RID: 36907 RVA: 0x001C3608 File Offset: 0x001C1808
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityPrincipleCategoryRelationsRowChanged != null)
				{
					this.SecurityPrincipleCategoryRelationsRowChanged(this, new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600902C RID: 36908 RVA: 0x001C363B File Offset: 0x001C183B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityPrincipleCategoryRelationsRowChanging != null)
				{
					this.SecurityPrincipleCategoryRelationsRowChanging(this, new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600902D RID: 36909 RVA: 0x001C366E File Offset: 0x001C186E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityPrincipleCategoryRelationsRowDeleted != null)
				{
					this.SecurityPrincipleCategoryRelationsRowDeleted(this, new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600902E RID: 36910 RVA: 0x001C36A1 File Offset: 0x001C18A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityPrincipleCategoryRelationsRowDeleting != null)
				{
					this.SecurityPrincipleCategoryRelationsRowDeleting(this, new SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600902F RID: 36911 RVA: 0x001C36D4 File Offset: 0x001C18D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveSecurityPrincipleCategoryRelationsRow(SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009030 RID: 36912 RVA: 0x001C36E4 File Offset: 0x001C18E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
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
				xmlSchemaAttribute.FixedValue = securityGroupsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityPrincipleCategoryRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

			// Token: 0x04001CE3 RID: 7395
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001CE4 RID: 7396
			private DataColumn columnWSEC_CAT_UID;
		}

		// Token: 0x020005F9 RID: 1529
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CategoryPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009031 RID: 36913 RVA: 0x001C38DC File Offset: 0x001C1ADC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CategoryPermissionsDataTable()
			{
				base.TableName = "CategoryPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009032 RID: 36914 RVA: 0x001C3904 File Offset: 0x001C1B04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CategoryPermissionsDataTable(DataTable table)
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

			// Token: 0x06009033 RID: 36915 RVA: 0x001C39AC File Offset: 0x001C1BAC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected CategoryPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002B71 RID: 11121
			// (get) Token: 0x06009034 RID: 36916 RVA: 0x001C39BC File Offset: 0x001C1BBC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002B72 RID: 11122
			// (get) Token: 0x06009035 RID: 36917 RVA: 0x001C39C4 File Offset: 0x001C1BC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002B73 RID: 11123
			// (get) Token: 0x06009036 RID: 36918 RVA: 0x001C39CC File Offset: 0x001C1BCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002B74 RID: 11124
			// (get) Token: 0x06009037 RID: 36919 RVA: 0x001C39D4 File Offset: 0x001C1BD4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002B75 RID: 11125
			// (get) Token: 0x06009038 RID: 36920 RVA: 0x001C39DC File Offset: 0x001C1BDC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002B76 RID: 11126
			// (get) Token: 0x06009039 RID: 36921 RVA: 0x001C39E4 File Offset: 0x001C1BE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17002B77 RID: 11127
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.CategoryPermissionsRow this[int index]
			{
				get
				{
					return (SecurityGroupsDataSet.CategoryPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000529 RID: 1321
			// (add) Token: 0x0600903B RID: 36923 RVA: 0x001C3A04 File Offset: 0x001C1C04
			// (remove) Token: 0x0600903C RID: 36924 RVA: 0x001C3A3C File Offset: 0x001C1C3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanging;

			// Token: 0x1400052A RID: 1322
			// (add) Token: 0x0600903D RID: 36925 RVA: 0x001C3A74 File Offset: 0x001C1C74
			// (remove) Token: 0x0600903E RID: 36926 RVA: 0x001C3AAC File Offset: 0x001C1CAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanged;

			// Token: 0x1400052B RID: 1323
			// (add) Token: 0x0600903F RID: 36927 RVA: 0x001C3AE4 File Offset: 0x001C1CE4
			// (remove) Token: 0x06009040 RID: 36928 RVA: 0x001C3B1C File Offset: 0x001C1D1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleting;

			// Token: 0x1400052C RID: 1324
			// (add) Token: 0x06009041 RID: 36929 RVA: 0x001C3B54 File Offset: 0x001C1D54
			// (remove) Token: 0x06009042 RID: 36930 RVA: 0x001C3B8C File Offset: 0x001C1D8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleted;

			// Token: 0x06009043 RID: 36931 RVA: 0x001C3BC1 File Offset: 0x001C1DC1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddCategoryPermissionsRow(SecurityGroupsDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009044 RID: 36932 RVA: 0x001C3BD0 File Offset: 0x001C1DD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.CategoryPermissionsRow AddCategoryPermissionsRow(Guid WSEC_GRP_UID, Guid WSEC_CAT_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityGroupsDataSet.CategoryPermissionsRow categoryPermissionsRow = (SecurityGroupsDataSet.CategoryPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_GRP_UID,
					WSEC_CAT_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				categoryPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(categoryPermissionsRow);
				return categoryPermissionsRow;
			}

			// Token: 0x06009045 RID: 36933 RVA: 0x001C3C38 File Offset: 0x001C1E38
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.CategoryPermissionsRow FindByWSEC_CAT_UIDWSEC_FEA_ACT_UIDWSEC_GRP_UID(Guid WSEC_CAT_UID, Guid WSEC_FEA_ACT_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityGroupsDataSet.CategoryPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_FEA_ACT_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009046 RID: 36934 RVA: 0x001C3C78 File Offset: 0x001C1E78
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009047 RID: 36935 RVA: 0x001C3C88 File Offset: 0x001C1E88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityGroupsDataSet.CategoryPermissionsDataTable categoryPermissionsDataTable = (SecurityGroupsDataSet.CategoryPermissionsDataTable)base.Clone();
				categoryPermissionsDataTable.InitVars();
				return categoryPermissionsDataTable;
			}

			// Token: 0x06009048 RID: 36936 RVA: 0x001C3CA8 File Offset: 0x001C1EA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityGroupsDataSet.CategoryPermissionsDataTable();
			}

			// Token: 0x06009049 RID: 36937 RVA: 0x001C3CB0 File Offset: 0x001C1EB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x0600904A RID: 36938 RVA: 0x001C3D2C File Offset: 0x001C1F2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("CategoryPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnWSEC_FEA_ACT_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x0600904B RID: 36939 RVA: 0x001C3EB1 File Offset: 0x001C20B1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.CategoryPermissionsRow NewCategoryPermissionsRow()
			{
				return (SecurityGroupsDataSet.CategoryPermissionsRow)base.NewRow();
			}

			// Token: 0x0600904C RID: 36940 RVA: 0x001C3EBE File Offset: 0x001C20BE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityGroupsDataSet.CategoryPermissionsRow(builder);
			}

			// Token: 0x0600904D RID: 36941 RVA: 0x001C3EC6 File Offset: 0x001C20C6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityGroupsDataSet.CategoryPermissionsRow);
			}

			// Token: 0x0600904E RID: 36942 RVA: 0x001C3ED2 File Offset: 0x001C20D2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CategoryPermissionsRowChanged != null)
				{
					this.CategoryPermissionsRowChanged(this, new SecurityGroupsDataSet.CategoryPermissionsRowChangeEvent((SecurityGroupsDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600904F RID: 36943 RVA: 0x001C3F05 File Offset: 0x001C2105
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CategoryPermissionsRowChanging != null)
				{
					this.CategoryPermissionsRowChanging(this, new SecurityGroupsDataSet.CategoryPermissionsRowChangeEvent((SecurityGroupsDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009050 RID: 36944 RVA: 0x001C3F38 File Offset: 0x001C2138
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CategoryPermissionsRowDeleted != null)
				{
					this.CategoryPermissionsRowDeleted(this, new SecurityGroupsDataSet.CategoryPermissionsRowChangeEvent((SecurityGroupsDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009051 RID: 36945 RVA: 0x001C3F6B File Offset: 0x001C216B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CategoryPermissionsRowDeleting != null)
				{
					this.CategoryPermissionsRowDeleting(this, new SecurityGroupsDataSet.CategoryPermissionsRowChangeEvent((SecurityGroupsDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009052 RID: 36946 RVA: 0x001C3F9E File Offset: 0x001C219E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveCategoryPermissionsRow(SecurityGroupsDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009053 RID: 36947 RVA: 0x001C3FAC File Offset: 0x001C21AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
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
				xmlSchemaAttribute.FixedValue = securityGroupsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CategoryPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

			// Token: 0x04001CE9 RID: 7401
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001CEA RID: 7402
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001CEB RID: 7403
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001CEC RID: 7404
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001CED RID: 7405
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x020005FA RID: 1530
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GlobalPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009054 RID: 36948 RVA: 0x001C41A4 File Offset: 0x001C23A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GlobalPermissionsDataTable()
			{
				base.TableName = "GlobalPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009055 RID: 36949 RVA: 0x001C41CC File Offset: 0x001C23CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GlobalPermissionsDataTable(DataTable table)
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

			// Token: 0x06009056 RID: 36950 RVA: 0x001C4274 File Offset: 0x001C2474
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected GlobalPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002B78 RID: 11128
			// (get) Token: 0x06009057 RID: 36951 RVA: 0x001C4284 File Offset: 0x001C2484
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002B79 RID: 11129
			// (get) Token: 0x06009058 RID: 36952 RVA: 0x001C428C File Offset: 0x001C248C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002B7A RID: 11130
			// (get) Token: 0x06009059 RID: 36953 RVA: 0x001C4294 File Offset: 0x001C2494
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002B7B RID: 11131
			// (get) Token: 0x0600905A RID: 36954 RVA: 0x001C429C File Offset: 0x001C249C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002B7C RID: 11132
			// (get) Token: 0x0600905B RID: 36955 RVA: 0x001C42A4 File Offset: 0x001C24A4
			[DebuggerNonUserCode]
			[Browsable(false)]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17002B7D RID: 11133
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GlobalPermissionsRow this[int index]
			{
				get
				{
					return (SecurityGroupsDataSet.GlobalPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x1400052D RID: 1325
			// (add) Token: 0x0600905D RID: 36957 RVA: 0x001C42C4 File Offset: 0x001C24C4
			// (remove) Token: 0x0600905E RID: 36958 RVA: 0x001C42FC File Offset: 0x001C24FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanging;

			// Token: 0x1400052E RID: 1326
			// (add) Token: 0x0600905F RID: 36959 RVA: 0x001C4334 File Offset: 0x001C2534
			// (remove) Token: 0x06009060 RID: 36960 RVA: 0x001C436C File Offset: 0x001C256C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanged;

			// Token: 0x1400052F RID: 1327
			// (add) Token: 0x06009061 RID: 36961 RVA: 0x001C43A4 File Offset: 0x001C25A4
			// (remove) Token: 0x06009062 RID: 36962 RVA: 0x001C43DC File Offset: 0x001C25DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleting;

			// Token: 0x14000530 RID: 1328
			// (add) Token: 0x06009063 RID: 36963 RVA: 0x001C4414 File Offset: 0x001C2614
			// (remove) Token: 0x06009064 RID: 36964 RVA: 0x001C444C File Offset: 0x001C264C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleted;

			// Token: 0x06009065 RID: 36965 RVA: 0x001C4481 File Offset: 0x001C2681
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGlobalPermissionsRow(SecurityGroupsDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009066 RID: 36966 RVA: 0x001C4490 File Offset: 0x001C2690
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.GlobalPermissionsRow AddGlobalPermissionsRow(Guid WSEC_GRP_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				SecurityGroupsDataSet.GlobalPermissionsRow globalPermissionsRow = (SecurityGroupsDataSet.GlobalPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_GRP_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				globalPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(globalPermissionsRow);
				return globalPermissionsRow;
			}

			// Token: 0x06009067 RID: 36967 RVA: 0x001C44EC File Offset: 0x001C26EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GlobalPermissionsRow FindByWSEC_FEA_ACT_UIDWSEC_GRP_UID(Guid WSEC_FEA_ACT_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityGroupsDataSet.GlobalPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_FEA_ACT_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009068 RID: 36968 RVA: 0x001C4523 File Offset: 0x001C2723
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009069 RID: 36969 RVA: 0x001C4530 File Offset: 0x001C2730
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SecurityGroupsDataSet.GlobalPermissionsDataTable globalPermissionsDataTable = (SecurityGroupsDataSet.GlobalPermissionsDataTable)base.Clone();
				globalPermissionsDataTable.InitVars();
				return globalPermissionsDataTable;
			}

			// Token: 0x0600906A RID: 36970 RVA: 0x001C4550 File Offset: 0x001C2750
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SecurityGroupsDataSet.GlobalPermissionsDataTable();
			}

			// Token: 0x0600906B RID: 36971 RVA: 0x001C4558 File Offset: 0x001C2758
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x0600906C RID: 36972 RVA: 0x001C45C0 File Offset: 0x001C27C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("GlobalPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_FEA_ACT_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x0600906D RID: 36973 RVA: 0x001C4703 File Offset: 0x001C2903
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GlobalPermissionsRow NewGlobalPermissionsRow()
			{
				return (SecurityGroupsDataSet.GlobalPermissionsRow)base.NewRow();
			}

			// Token: 0x0600906E RID: 36974 RVA: 0x001C4710 File Offset: 0x001C2910
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityGroupsDataSet.GlobalPermissionsRow(builder);
			}

			// Token: 0x0600906F RID: 36975 RVA: 0x001C4718 File Offset: 0x001C2918
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityGroupsDataSet.GlobalPermissionsRow);
			}

			// Token: 0x06009070 RID: 36976 RVA: 0x001C4724 File Offset: 0x001C2924
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GlobalPermissionsRowChanged != null)
				{
					this.GlobalPermissionsRowChanged(this, new SecurityGroupsDataSet.GlobalPermissionsRowChangeEvent((SecurityGroupsDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009071 RID: 36977 RVA: 0x001C4757 File Offset: 0x001C2957
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GlobalPermissionsRowChanging != null)
				{
					this.GlobalPermissionsRowChanging(this, new SecurityGroupsDataSet.GlobalPermissionsRowChangeEvent((SecurityGroupsDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009072 RID: 36978 RVA: 0x001C478A File Offset: 0x001C298A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GlobalPermissionsRowDeleted != null)
				{
					this.GlobalPermissionsRowDeleted(this, new SecurityGroupsDataSet.GlobalPermissionsRowChangeEvent((SecurityGroupsDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009073 RID: 36979 RVA: 0x001C47BD File Offset: 0x001C29BD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GlobalPermissionsRowDeleting != null)
				{
					this.GlobalPermissionsRowDeleting(this, new SecurityGroupsDataSet.GlobalPermissionsRowChangeEvent((SecurityGroupsDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009074 RID: 36980 RVA: 0x001C47F0 File Offset: 0x001C29F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGlobalPermissionsRow(SecurityGroupsDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009075 RID: 36981 RVA: 0x001C4800 File Offset: 0x001C2A00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
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
				xmlSchemaAttribute.FixedValue = securityGroupsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GlobalPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

			// Token: 0x04001CF2 RID: 7410
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001CF3 RID: 7411
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001CF4 RID: 7412
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001CF5 RID: 7413
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x020005FB RID: 1531
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupMembersDataTable : DataTable, IEnumerable
		{
			// Token: 0x06009076 RID: 36982 RVA: 0x001C49F8 File Offset: 0x001C2BF8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupMembersDataTable()
			{
				base.TableName = "GroupMembers";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06009077 RID: 36983 RVA: 0x001C4A20 File Offset: 0x001C2C20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal GroupMembersDataTable(DataTable table)
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

			// Token: 0x06009078 RID: 36984 RVA: 0x001C4AC8 File Offset: 0x001C2CC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupMembersDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002B7E RID: 11134
			// (get) Token: 0x06009079 RID: 36985 RVA: 0x001C4AD8 File Offset: 0x001C2CD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x17002B7F RID: 11135
			// (get) Token: 0x0600907A RID: 36986 RVA: 0x001C4AE0 File Offset: 0x001C2CE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002B80 RID: 11136
			// (get) Token: 0x0600907B RID: 36987 RVA: 0x001C4AE8 File Offset: 0x001C2CE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[Browsable(false)]
			[DebuggerNonUserCode]
			public int Count
			{
				get
				{
					return base.Rows.Count;
				}
			}

			// Token: 0x17002B81 RID: 11137
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.GroupMembersRow this[int index]
			{
				get
				{
					return (SecurityGroupsDataSet.GroupMembersRow)base.Rows[index];
				}
			}

			// Token: 0x14000531 RID: 1329
			// (add) Token: 0x0600907D RID: 36989 RVA: 0x001C4B08 File Offset: 0x001C2D08
			// (remove) Token: 0x0600907E RID: 36990 RVA: 0x001C4B40 File Offset: 0x001C2D40
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GroupMembersRowChangeEventHandler GroupMembersRowChanging;

			// Token: 0x14000532 RID: 1330
			// (add) Token: 0x0600907F RID: 36991 RVA: 0x001C4B78 File Offset: 0x001C2D78
			// (remove) Token: 0x06009080 RID: 36992 RVA: 0x001C4BB0 File Offset: 0x001C2DB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GroupMembersRowChangeEventHandler GroupMembersRowChanged;

			// Token: 0x14000533 RID: 1331
			// (add) Token: 0x06009081 RID: 36993 RVA: 0x001C4BE8 File Offset: 0x001C2DE8
			// (remove) Token: 0x06009082 RID: 36994 RVA: 0x001C4C20 File Offset: 0x001C2E20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GroupMembersRowChangeEventHandler GroupMembersRowDeleting;

			// Token: 0x14000534 RID: 1332
			// (add) Token: 0x06009083 RID: 36995 RVA: 0x001C4C58 File Offset: 0x001C2E58
			// (remove) Token: 0x06009084 RID: 36996 RVA: 0x001C4C90 File Offset: 0x001C2E90
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SecurityGroupsDataSet.GroupMembersRowChangeEventHandler GroupMembersRowDeleted;

			// Token: 0x06009085 RID: 36997 RVA: 0x001C4CC5 File Offset: 0x001C2EC5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddGroupMembersRow(SecurityGroupsDataSet.GroupMembersRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06009086 RID: 36998 RVA: 0x001C4CD4 File Offset: 0x001C2ED4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.GroupMembersRow AddGroupMembersRow(Guid WSEC_GRP_UID, Guid RES_UID)
			{
				SecurityGroupsDataSet.GroupMembersRow groupMembersRow = (SecurityGroupsDataSet.GroupMembersRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WSEC_GRP_UID,
					RES_UID
				};
				groupMembersRow.ItemArray = itemArray;
				base.Rows.Add(groupMembersRow);
				return groupMembersRow;
			}

			// Token: 0x06009087 RID: 36999 RVA: 0x001C4D1C File Offset: 0x001C2F1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GroupMembersRow FindByRES_UIDWSEC_GRP_UID(Guid RES_UID, Guid WSEC_GRP_UID)
			{
				return (SecurityGroupsDataSet.GroupMembersRow)base.Rows.Find(new object[]
				{
					RES_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06009088 RID: 37000 RVA: 0x001C4D53 File Offset: 0x001C2F53
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06009089 RID: 37001 RVA: 0x001C4D60 File Offset: 0x001C2F60
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				SecurityGroupsDataSet.GroupMembersDataTable groupMembersDataTable = (SecurityGroupsDataSet.GroupMembersDataTable)base.Clone();
				groupMembersDataTable.InitVars();
				return groupMembersDataTable;
			}

			// Token: 0x0600908A RID: 37002 RVA: 0x001C4D80 File Offset: 0x001C2F80
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SecurityGroupsDataSet.GroupMembersDataTable();
			}

			// Token: 0x0600908B RID: 37003 RVA: 0x001C4D87 File Offset: 0x001C2F87
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
				this.columnRES_UID = base.Columns["RES_UID"];
			}

			// Token: 0x0600908C RID: 37004 RVA: 0x001C4DB8 File Offset: 0x001C2FB8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				base.Constraints.Add(new UniqueConstraint("GroupMembersPrimaryKey", new DataColumn[]
				{
					this.columnRES_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnWSEC_GRP_UID.AllowDBNull = false;
				this.columnRES_UID.AllowDBNull = false;
			}

			// Token: 0x0600908D RID: 37005 RVA: 0x001C4E67 File Offset: 0x001C3067
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GroupMembersRow NewGroupMembersRow()
			{
				return (SecurityGroupsDataSet.GroupMembersRow)base.NewRow();
			}

			// Token: 0x0600908E RID: 37006 RVA: 0x001C4E74 File Offset: 0x001C3074
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SecurityGroupsDataSet.GroupMembersRow(builder);
			}

			// Token: 0x0600908F RID: 37007 RVA: 0x001C4E7C File Offset: 0x001C307C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override Type GetRowType()
			{
				return typeof(SecurityGroupsDataSet.GroupMembersRow);
			}

			// Token: 0x06009090 RID: 37008 RVA: 0x001C4E88 File Offset: 0x001C3088
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupMembersRowChanged != null)
				{
					this.GroupMembersRowChanged(this, new SecurityGroupsDataSet.GroupMembersRowChangeEvent((SecurityGroupsDataSet.GroupMembersRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009091 RID: 37009 RVA: 0x001C4EBB File Offset: 0x001C30BB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupMembersRowChanging != null)
				{
					this.GroupMembersRowChanging(this, new SecurityGroupsDataSet.GroupMembersRowChangeEvent((SecurityGroupsDataSet.GroupMembersRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009092 RID: 37010 RVA: 0x001C4EEE File Offset: 0x001C30EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupMembersRowDeleted != null)
				{
					this.GroupMembersRowDeleted(this, new SecurityGroupsDataSet.GroupMembersRowChangeEvent((SecurityGroupsDataSet.GroupMembersRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009093 RID: 37011 RVA: 0x001C4F21 File Offset: 0x001C3121
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupMembersRowDeleting != null)
				{
					this.GroupMembersRowDeleting(this, new SecurityGroupsDataSet.GroupMembersRowChangeEvent((SecurityGroupsDataSet.GroupMembersRow)e.Row, e.Action));
				}
			}

			// Token: 0x06009094 RID: 37012 RVA: 0x001C4F54 File Offset: 0x001C3154
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void RemoveGroupMembersRow(SecurityGroupsDataSet.GroupMembersRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06009095 RID: 37013 RVA: 0x001C4F64 File Offset: 0x001C3164
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SecurityGroupsDataSet securityGroupsDataSet = new SecurityGroupsDataSet();
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
				xmlSchemaAttribute.FixedValue = securityGroupsDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupMembersDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = securityGroupsDataSet.GetSchemaSerializable();
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

			// Token: 0x04001CFA RID: 7418
			private DataColumn columnWSEC_GRP_UID;

			// Token: 0x04001CFB RID: 7419
			private DataColumn columnRES_UID;
		}

		// Token: 0x020005FC RID: 1532
		public class SecurityGroupsRow : DataRow
		{
			// Token: 0x06009096 RID: 37014 RVA: 0x001C515C File Offset: 0x001C335C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityGroupsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityGroups = (SecurityGroupsDataSet.SecurityGroupsDataTable)base.Table;
			}

			// Token: 0x17002B82 RID: 11138
			// (get) Token: 0x06009097 RID: 37015 RVA: 0x001C5176 File Offset: 0x001C3376
			// (set) Token: 0x06009098 RID: 37016 RVA: 0x001C518E File Offset: 0x001C338E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityGroups.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002B83 RID: 11139
			// (get) Token: 0x06009099 RID: 37017 RVA: 0x001C51A7 File Offset: 0x001C33A7
			// (set) Token: 0x0600909A RID: 37018 RVA: 0x001C51BF File Offset: 0x001C33BF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WSEC_GRP_NAME
			{
				get
				{
					return (string)base[this.tableSecurityGroups.WSEC_GRP_NAMEColumn];
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_NAMEColumn] = value;
				}
			}

			// Token: 0x17002B84 RID: 11140
			// (get) Token: 0x0600909B RID: 37019 RVA: 0x001C51D4 File Offset: 0x001C33D4
			// (set) Token: 0x0600909C RID: 37020 RVA: 0x001C5218 File Offset: 0x001C3418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WSEC_GRP_DESC
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSecurityGroups.WSEC_GRP_DESCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_GRP_DESC' in table 'SecurityGroups' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_DESCColumn] = value;
				}
			}

			// Token: 0x17002B85 RID: 11141
			// (get) Token: 0x0600909D RID: 37021 RVA: 0x001C522C File Offset: 0x001C342C
			// (set) Token: 0x0600909E RID: 37022 RVA: 0x001C5270 File Offset: 0x001C3470
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WSEC_GRP_AD_GROUP
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSecurityGroups.WSEC_GRP_AD_GROUPColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_GRP_AD_GROUP' in table 'SecurityGroups' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_AD_GROUPColumn] = value;
				}
			}

			// Token: 0x17002B86 RID: 11142
			// (get) Token: 0x0600909F RID: 37023 RVA: 0x001C5284 File Offset: 0x001C3484
			// (set) Token: 0x060090A0 RID: 37024 RVA: 0x001C52C8 File Offset: 0x001C34C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_AD_GUID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSecurityGroups.WSEC_GRP_AD_GUIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_GRP_AD_GUID' in table 'SecurityGroups' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_AD_GUIDColumn] = value;
				}
			}

			// Token: 0x17002B87 RID: 11143
			// (get) Token: 0x060090A1 RID: 37025 RVA: 0x001C52E4 File Offset: 0x001C34E4
			// (set) Token: 0x060090A2 RID: 37026 RVA: 0x001C5328 File Offset: 0x001C3528
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public short WSEC_GRP_AD_LOG
			{
				get
				{
					short result;
					try
					{
						result = (short)base[this.tableSecurityGroups.WSEC_GRP_AD_LOGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_GRP_AD_LOG' in table 'SecurityGroups' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_AD_LOGColumn] = value;
				}
			}

			// Token: 0x17002B88 RID: 11144
			// (get) Token: 0x060090A3 RID: 37027 RVA: 0x001C5344 File Offset: 0x001C3544
			// (set) Token: 0x060090A4 RID: 37028 RVA: 0x001C5388 File Offset: 0x001C3588
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WSEC_GRP_AD_LASTSYNC
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableSecurityGroups.WSEC_GRP_AD_LASTSYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WSEC_GRP_AD_LASTSYNC' in table 'SecurityGroups' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSecurityGroups.WSEC_GRP_AD_LASTSYNCColumn] = value;
				}
			}

			// Token: 0x060090A5 RID: 37029 RVA: 0x001C53A1 File Offset: 0x001C35A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWSEC_GRP_DESCNull()
			{
				return base.IsNull(this.tableSecurityGroups.WSEC_GRP_DESCColumn);
			}

			// Token: 0x060090A6 RID: 37030 RVA: 0x001C53B4 File Offset: 0x001C35B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWSEC_GRP_DESCNull()
			{
				base[this.tableSecurityGroups.WSEC_GRP_DESCColumn] = Convert.DBNull;
			}

			// Token: 0x060090A7 RID: 37031 RVA: 0x001C53CC File Offset: 0x001C35CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWSEC_GRP_AD_GROUPNull()
			{
				return base.IsNull(this.tableSecurityGroups.WSEC_GRP_AD_GROUPColumn);
			}

			// Token: 0x060090A8 RID: 37032 RVA: 0x001C53DF File Offset: 0x001C35DF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWSEC_GRP_AD_GROUPNull()
			{
				base[this.tableSecurityGroups.WSEC_GRP_AD_GROUPColumn] = Convert.DBNull;
			}

			// Token: 0x060090A9 RID: 37033 RVA: 0x001C53F7 File Offset: 0x001C35F7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWSEC_GRP_AD_GUIDNull()
			{
				return base.IsNull(this.tableSecurityGroups.WSEC_GRP_AD_GUIDColumn);
			}

			// Token: 0x060090AA RID: 37034 RVA: 0x001C540A File Offset: 0x001C360A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWSEC_GRP_AD_GUIDNull()
			{
				base[this.tableSecurityGroups.WSEC_GRP_AD_GUIDColumn] = Convert.DBNull;
			}

			// Token: 0x060090AB RID: 37035 RVA: 0x001C5422 File Offset: 0x001C3622
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWSEC_GRP_AD_LOGNull()
			{
				return base.IsNull(this.tableSecurityGroups.WSEC_GRP_AD_LOGColumn);
			}

			// Token: 0x060090AC RID: 37036 RVA: 0x001C5435 File Offset: 0x001C3635
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWSEC_GRP_AD_LOGNull()
			{
				base[this.tableSecurityGroups.WSEC_GRP_AD_LOGColumn] = Convert.DBNull;
			}

			// Token: 0x060090AD RID: 37037 RVA: 0x001C544D File Offset: 0x001C364D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWSEC_GRP_AD_LASTSYNCNull()
			{
				return base.IsNull(this.tableSecurityGroups.WSEC_GRP_AD_LASTSYNCColumn);
			}

			// Token: 0x060090AE RID: 37038 RVA: 0x001C5460 File Offset: 0x001C3660
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWSEC_GRP_AD_LASTSYNCNull()
			{
				base[this.tableSecurityGroups.WSEC_GRP_AD_LASTSYNCColumn] = Convert.DBNull;
			}

			// Token: 0x04001D00 RID: 7424
			private SecurityGroupsDataSet.SecurityGroupsDataTable tableSecurityGroups;
		}

		// Token: 0x020005FD RID: 1533
		public class SecurityPrincipleCategoryRelationsRow : DataRow
		{
			// Token: 0x060090AF RID: 37039 RVA: 0x001C5478 File Offset: 0x001C3678
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SecurityPrincipleCategoryRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityPrincipleCategoryRelations = (SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Table;
			}

			// Token: 0x17002B89 RID: 11145
			// (get) Token: 0x060090B0 RID: 37040 RVA: 0x001C5492 File Offset: 0x001C3692
			// (set) Token: 0x060090B1 RID: 37041 RVA: 0x001C54AA File Offset: 0x001C36AA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityPrincipleCategoryRelations.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableSecurityPrincipleCategoryRelations.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002B8A RID: 11146
			// (get) Token: 0x060090B2 RID: 37042 RVA: 0x001C54C3 File Offset: 0x001C36C3
			// (set) Token: 0x060090B3 RID: 37043 RVA: 0x001C54DB File Offset: 0x001C36DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityPrincipleCategoryRelations.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableSecurityPrincipleCategoryRelations.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x04001D01 RID: 7425
			private SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsDataTable tableSecurityPrincipleCategoryRelations;
		}

		// Token: 0x020005FE RID: 1534
		public class CategoryPermissionsRow : DataRow
		{
			// Token: 0x060090B4 RID: 37044 RVA: 0x001C54F4 File Offset: 0x001C36F4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal CategoryPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCategoryPermissions = (SecurityGroupsDataSet.CategoryPermissionsDataTable)base.Table;
			}

			// Token: 0x17002B8B RID: 11147
			// (get) Token: 0x060090B5 RID: 37045 RVA: 0x001C550E File Offset: 0x001C370E
			// (set) Token: 0x060090B6 RID: 37046 RVA: 0x001C5526 File Offset: 0x001C3726
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002B8C RID: 11148
			// (get) Token: 0x060090B7 RID: 37047 RVA: 0x001C553F File Offset: 0x001C373F
			// (set) Token: 0x060090B8 RID: 37048 RVA: 0x001C5557 File Offset: 0x001C3757
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_CAT_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.WSEC_CAT_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_CAT_UIDColumn] = value;
				}
			}

			// Token: 0x17002B8D RID: 11149
			// (get) Token: 0x060090B9 RID: 37049 RVA: 0x001C5570 File Offset: 0x001C3770
			// (set) Token: 0x060090BA RID: 37050 RVA: 0x001C5588 File Offset: 0x001C3788
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x17002B8E RID: 11150
			// (get) Token: 0x060090BB RID: 37051 RVA: 0x001C55A1 File Offset: 0x001C37A1
			// (set) Token: 0x060090BC RID: 37052 RVA: 0x001C55B9 File Offset: 0x001C37B9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableCategoryPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002B8F RID: 11151
			// (get) Token: 0x060090BD RID: 37053 RVA: 0x001C55D2 File Offset: 0x001C37D2
			// (set) Token: 0x060090BE RID: 37054 RVA: 0x001C55EA File Offset: 0x001C37EA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableCategoryPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001D02 RID: 7426
			private SecurityGroupsDataSet.CategoryPermissionsDataTable tableCategoryPermissions;
		}

		// Token: 0x020005FF RID: 1535
		public class GlobalPermissionsRow : DataRow
		{
			// Token: 0x060090BF RID: 37055 RVA: 0x001C5603 File Offset: 0x001C3803
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GlobalPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGlobalPermissions = (SecurityGroupsDataSet.GlobalPermissionsDataTable)base.Table;
			}

			// Token: 0x17002B90 RID: 11152
			// (get) Token: 0x060090C0 RID: 37056 RVA: 0x001C561D File Offset: 0x001C381D
			// (set) Token: 0x060090C1 RID: 37057 RVA: 0x001C5635 File Offset: 0x001C3835
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableGlobalPermissions.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002B91 RID: 11153
			// (get) Token: 0x060090C2 RID: 37058 RVA: 0x001C564E File Offset: 0x001C384E
			// (set) Token: 0x060090C3 RID: 37059 RVA: 0x001C5666 File Offset: 0x001C3866
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_FEA_ACT_UID
			{
				get
				{
					return (Guid)base[this.tableGlobalPermissions.WSEC_FEA_ACT_UIDColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_FEA_ACT_UIDColumn] = value;
				}
			}

			// Token: 0x17002B92 RID: 11154
			// (get) Token: 0x060090C4 RID: 37060 RVA: 0x001C567F File Offset: 0x001C387F
			// (set) Token: 0x060090C5 RID: 37061 RVA: 0x001C5697 File Offset: 0x001C3897
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_ALLOW
			{
				get
				{
					return (bool)base[this.tableGlobalPermissions.WSEC_ALLOWColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_ALLOWColumn] = value;
				}
			}

			// Token: 0x17002B93 RID: 11155
			// (get) Token: 0x060090C6 RID: 37062 RVA: 0x001C56B0 File Offset: 0x001C38B0
			// (set) Token: 0x060090C7 RID: 37063 RVA: 0x001C56C8 File Offset: 0x001C38C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WSEC_DENY
			{
				get
				{
					return (bool)base[this.tableGlobalPermissions.WSEC_DENYColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.WSEC_DENYColumn] = value;
				}
			}

			// Token: 0x04001D03 RID: 7427
			private SecurityGroupsDataSet.GlobalPermissionsDataTable tableGlobalPermissions;
		}

		// Token: 0x02000600 RID: 1536
		public class GroupMembersRow : DataRow
		{
			// Token: 0x060090C8 RID: 37064 RVA: 0x001C56E1 File Offset: 0x001C38E1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupMembersRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupMembers = (SecurityGroupsDataSet.GroupMembersDataTable)base.Table;
			}

			// Token: 0x17002B94 RID: 11156
			// (get) Token: 0x060090C9 RID: 37065 RVA: 0x001C56FB File Offset: 0x001C38FB
			// (set) Token: 0x060090CA RID: 37066 RVA: 0x001C5713 File Offset: 0x001C3913
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableGroupMembers.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableGroupMembers.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x17002B95 RID: 11157
			// (get) Token: 0x060090CB RID: 37067 RVA: 0x001C572C File Offset: 0x001C392C
			// (set) Token: 0x060090CC RID: 37068 RVA: 0x001C5744 File Offset: 0x001C3944
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableGroupMembers.RES_UIDColumn];
				}
				set
				{
					base[this.tableGroupMembers.RES_UIDColumn] = value;
				}
			}

			// Token: 0x04001D04 RID: 7428
			private SecurityGroupsDataSet.GroupMembersDataTable tableGroupMembers;
		}

		// Token: 0x02000601 RID: 1537
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityGroupsRowChangeEvent : EventArgs
		{
			// Token: 0x060090CD RID: 37069 RVA: 0x001C575D File Offset: 0x001C395D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsRowChangeEvent(SecurityGroupsDataSet.SecurityGroupsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B96 RID: 11158
			// (get) Token: 0x060090CE RID: 37070 RVA: 0x001C5773 File Offset: 0x001C3973
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.SecurityGroupsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B97 RID: 11159
			// (get) Token: 0x060090CF RID: 37071 RVA: 0x001C577B File Offset: 0x001C397B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D05 RID: 7429
			private SecurityGroupsDataSet.SecurityGroupsRow eventRow;

			// Token: 0x04001D06 RID: 7430
			private DataRowAction eventAction;
		}

		// Token: 0x02000602 RID: 1538
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityPrincipleCategoryRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x060090D0 RID: 37072 RVA: 0x001C5783 File Offset: 0x001C3983
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityPrincipleCategoryRelationsRowChangeEvent(SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B98 RID: 11160
			// (get) Token: 0x060090D1 RID: 37073 RVA: 0x001C5799 File Offset: 0x001C3999
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B99 RID: 11161
			// (get) Token: 0x060090D2 RID: 37074 RVA: 0x001C57A1 File Offset: 0x001C39A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D07 RID: 7431
			private SecurityGroupsDataSet.SecurityPrincipleCategoryRelationsRow eventRow;

			// Token: 0x04001D08 RID: 7432
			private DataRowAction eventAction;
		}

		// Token: 0x02000603 RID: 1539
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CategoryPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x060090D3 RID: 37075 RVA: 0x001C57A9 File Offset: 0x001C39A9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CategoryPermissionsRowChangeEvent(SecurityGroupsDataSet.CategoryPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B9A RID: 11162
			// (get) Token: 0x060090D4 RID: 37076 RVA: 0x001C57BF File Offset: 0x001C39BF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.CategoryPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B9B RID: 11163
			// (get) Token: 0x060090D5 RID: 37077 RVA: 0x001C57C7 File Offset: 0x001C39C7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D09 RID: 7433
			private SecurityGroupsDataSet.CategoryPermissionsRow eventRow;

			// Token: 0x04001D0A RID: 7434
			private DataRowAction eventAction;
		}

		// Token: 0x02000604 RID: 1540
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GlobalPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x060090D6 RID: 37078 RVA: 0x001C57CF File Offset: 0x001C39CF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public GlobalPermissionsRowChangeEvent(SecurityGroupsDataSet.GlobalPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B9C RID: 11164
			// (get) Token: 0x060090D7 RID: 37079 RVA: 0x001C57E5 File Offset: 0x001C39E5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GlobalPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B9D RID: 11165
			// (get) Token: 0x060090D8 RID: 37080 RVA: 0x001C57ED File Offset: 0x001C39ED
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D0B RID: 7435
			private SecurityGroupsDataSet.GlobalPermissionsRow eventRow;

			// Token: 0x04001D0C RID: 7436
			private DataRowAction eventAction;
		}

		// Token: 0x02000605 RID: 1541
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupMembersRowChangeEvent : EventArgs
		{
			// Token: 0x060090D9 RID: 37081 RVA: 0x001C57F5 File Offset: 0x001C39F5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupMembersRowChangeEvent(SecurityGroupsDataSet.GroupMembersRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002B9E RID: 11166
			// (get) Token: 0x060090DA RID: 37082 RVA: 0x001C580B File Offset: 0x001C3A0B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SecurityGroupsDataSet.GroupMembersRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002B9F RID: 11167
			// (get) Token: 0x060090DB RID: 37083 RVA: 0x001C5813 File Offset: 0x001C3A13
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001D0D RID: 7437
			private SecurityGroupsDataSet.GroupMembersRow eventRow;

			// Token: 0x04001D0E RID: 7438
			private DataRowAction eventAction;
		}
	}
}
