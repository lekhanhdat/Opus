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
	// Token: 0x02000552 RID: 1362
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[ToolboxItem(true)]
	[XmlRoot("ResourceAuthorizationDataSet")]
	[HelpKeyword("vs.data.DataSet")]
	[DesignerCategory("code")]
	[Serializable]
	public class ResourceAuthorizationDataSet : DataSet
	{
		// Token: 0x0600826C RID: 33388 RVA: 0x00198A34 File Offset: 0x00196C34
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GroupMemberships, new string[]
			{
				"RES_UID",
				"WSEC_GRP_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SecurityPrincipleCategoryRelations, new string[]
			{
				"RES_UID",
				"WSEC_CAT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.Resources, new string[]
			{
				"RES_EXCHANGE_SYNC",
				"RES_PREVENT_ADSYNC",
				"RES_UID",
				"RES_IS_WINDOWS_USER",
				"WRES_ACCOUNT",
				"WRES_AD_GUID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.GlobalPermissions, new string[]
			{
				"WSEC_DENY",
				"RES_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.CategoryPermissions, new string[]
			{
				"WSEC_DENY",
				"RES_UID",
				"WSEC_CAT_UID",
				"WSEC_ALLOW",
				"WSEC_FEA_ACT_UID"
			});
		}

		// Token: 0x0600826D RID: 33389 RVA: 0x00198B48 File Offset: 0x00196D48
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public ResourceAuthorizationDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600826E RID: 33390 RVA: 0x00198B9C File Offset: 0x00196D9C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected ResourceAuthorizationDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["Resources"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.ResourcesDataTable(dataSet.Tables["Resources"]));
				}
				if (dataSet.Tables["SecurityPrincipleCategoryRelations"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable(dataSet.Tables["SecurityPrincipleCategoryRelations"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
				}
				if (dataSet.Tables["GroupMemberships"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.GroupMembershipsDataTable(dataSet.Tables["GroupMemberships"]));
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

		// Token: 0x17002777 RID: 10103
		// (get) Token: 0x0600826F RID: 33391 RVA: 0x00198DC1 File Offset: 0x00196FC1
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public ResourceAuthorizationDataSet.ResourcesDataTable Resources
		{
			get
			{
				return this.tableResources;
			}
		}

		// Token: 0x17002778 RID: 10104
		// (get) Token: 0x06008270 RID: 33392 RVA: 0x00198DC9 File Offset: 0x00196FC9
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable SecurityPrincipleCategoryRelations
		{
			get
			{
				return this.tableSecurityPrincipleCategoryRelations;
			}
		}

		// Token: 0x17002779 RID: 10105
		// (get) Token: 0x06008271 RID: 33393 RVA: 0x00198DD1 File Offset: 0x00196FD1
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public ResourceAuthorizationDataSet.CategoryPermissionsDataTable CategoryPermissions
		{
			get
			{
				return this.tableCategoryPermissions;
			}
		}

		// Token: 0x1700277A RID: 10106
		// (get) Token: 0x06008272 RID: 33394 RVA: 0x00198DD9 File Offset: 0x00196FD9
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public ResourceAuthorizationDataSet.GlobalPermissionsDataTable GlobalPermissions
		{
			get
			{
				return this.tableGlobalPermissions;
			}
		}

		// Token: 0x1700277B RID: 10107
		// (get) Token: 0x06008273 RID: 33395 RVA: 0x00198DE1 File Offset: 0x00196FE1
		[Browsable(false)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		public ResourceAuthorizationDataSet.GroupMembershipsDataTable GroupMemberships
		{
			get
			{
				return this.tableGroupMemberships;
			}
		}

		// Token: 0x1700277C RID: 10108
		// (get) Token: 0x06008274 RID: 33396 RVA: 0x00198DE9 File Offset: 0x00196FE9
		// (set) Token: 0x06008275 RID: 33397 RVA: 0x00198DF1 File Offset: 0x00196FF1
		[Browsable(true)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DebuggerNonUserCode]
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

		// Token: 0x1700277D RID: 10109
		// (get) Token: 0x06008276 RID: 33398 RVA: 0x00198DFA File Offset: 0x00196FFA
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

		// Token: 0x1700277E RID: 10110
		// (get) Token: 0x06008277 RID: 33399 RVA: 0x00198E02 File Offset: 0x00197002
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

		// Token: 0x06008278 RID: 33400 RVA: 0x00198E0A File Offset: 0x0019700A
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06008279 RID: 33401 RVA: 0x00198E20 File Offset: 0x00197020
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public override DataSet Clone()
		{
			ResourceAuthorizationDataSet resourceAuthorizationDataSet = (ResourceAuthorizationDataSet)base.Clone();
			resourceAuthorizationDataSet.InitVars();
			resourceAuthorizationDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return resourceAuthorizationDataSet;
		}

		// Token: 0x0600827A RID: 33402 RVA: 0x00198E4C File Offset: 0x0019704C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600827B RID: 33403 RVA: 0x00198E4F File Offset: 0x0019704F
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600827C RID: 33404 RVA: 0x00198E54 File Offset: 0x00197054
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["Resources"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.ResourcesDataTable(dataSet.Tables["Resources"]));
				}
				if (dataSet.Tables["SecurityPrincipleCategoryRelations"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable(dataSet.Tables["SecurityPrincipleCategoryRelations"]));
				}
				if (dataSet.Tables["CategoryPermissions"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.CategoryPermissionsDataTable(dataSet.Tables["CategoryPermissions"]));
				}
				if (dataSet.Tables["GlobalPermissions"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.GlobalPermissionsDataTable(dataSet.Tables["GlobalPermissions"]));
				}
				if (dataSet.Tables["GroupMemberships"] != null)
				{
					base.Tables.Add(new ResourceAuthorizationDataSet.GroupMembershipsDataTable(dataSet.Tables["GroupMemberships"]));
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

		// Token: 0x0600827D RID: 33405 RVA: 0x00198FE4 File Offset: 0x001971E4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600827E RID: 33406 RVA: 0x00199018 File Offset: 0x00197218
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600827F RID: 33407 RVA: 0x00199024 File Offset: 0x00197224
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableResources = (ResourceAuthorizationDataSet.ResourcesDataTable)base.Tables["Resources"];
			if (initTable && this.tableResources != null)
			{
				this.tableResources.InitVars();
			}
			this.tableSecurityPrincipleCategoryRelations = (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Tables["SecurityPrincipleCategoryRelations"];
			if (initTable && this.tableSecurityPrincipleCategoryRelations != null)
			{
				this.tableSecurityPrincipleCategoryRelations.InitVars();
			}
			this.tableCategoryPermissions = (ResourceAuthorizationDataSet.CategoryPermissionsDataTable)base.Tables["CategoryPermissions"];
			if (initTable && this.tableCategoryPermissions != null)
			{
				this.tableCategoryPermissions.InitVars();
			}
			this.tableGlobalPermissions = (ResourceAuthorizationDataSet.GlobalPermissionsDataTable)base.Tables["GlobalPermissions"];
			if (initTable && this.tableGlobalPermissions != null)
			{
				this.tableGlobalPermissions.InitVars();
			}
			this.tableGroupMemberships = (ResourceAuthorizationDataSet.GroupMembershipsDataTable)base.Tables["GroupMemberships"];
			if (initTable && this.tableGroupMemberships != null)
			{
				this.tableGroupMemberships.InitVars();
			}
		}

		// Token: 0x06008280 RID: 33408 RVA: 0x00199128 File Offset: 0x00197328
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "ResourceAuthorizationDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/ResourceAuthorizationDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableResources = new ResourceAuthorizationDataSet.ResourcesDataTable();
			base.Tables.Add(this.tableResources);
			this.tableSecurityPrincipleCategoryRelations = new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable();
			base.Tables.Add(this.tableSecurityPrincipleCategoryRelations);
			this.tableCategoryPermissions = new ResourceAuthorizationDataSet.CategoryPermissionsDataTable();
			base.Tables.Add(this.tableCategoryPermissions);
			this.tableGlobalPermissions = new ResourceAuthorizationDataSet.GlobalPermissionsDataTable();
			base.Tables.Add(this.tableGlobalPermissions);
			this.tableGroupMemberships = new ResourceAuthorizationDataSet.GroupMembershipsDataTable();
			base.Tables.Add(this.tableGroupMemberships);
		}

		// Token: 0x06008281 RID: 33409 RVA: 0x001991F0 File Offset: 0x001973F0
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeResources()
		{
			return false;
		}

		// Token: 0x06008282 RID: 33410 RVA: 0x001991F3 File Offset: 0x001973F3
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSecurityPrincipleCategoryRelations()
		{
			return false;
		}

		// Token: 0x06008283 RID: 33411 RVA: 0x001991F6 File Offset: 0x001973F6
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeCategoryPermissions()
		{
			return false;
		}

		// Token: 0x06008284 RID: 33412 RVA: 0x001991F9 File Offset: 0x001973F9
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeGlobalPermissions()
		{
			return false;
		}

		// Token: 0x06008285 RID: 33413 RVA: 0x001991FC File Offset: 0x001973FC
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeGroupMemberships()
		{
			return false;
		}

		// Token: 0x06008286 RID: 33414 RVA: 0x001991FF File Offset: 0x001973FF
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06008287 RID: 33415 RVA: 0x00199210 File Offset: 0x00197410
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = resourceAuthorizationDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

		// Token: 0x04001A17 RID: 6679
		private ResourceAuthorizationDataSet.ResourcesDataTable tableResources;

		// Token: 0x04001A18 RID: 6680
		private ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable tableSecurityPrincipleCategoryRelations;

		// Token: 0x04001A19 RID: 6681
		private ResourceAuthorizationDataSet.CategoryPermissionsDataTable tableCategoryPermissions;

		// Token: 0x04001A1A RID: 6682
		private ResourceAuthorizationDataSet.GlobalPermissionsDataTable tableGlobalPermissions;

		// Token: 0x04001A1B RID: 6683
		private ResourceAuthorizationDataSet.GroupMembershipsDataTable tableGroupMemberships;

		// Token: 0x04001A1C RID: 6684
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x02000553 RID: 1363
		// (Invoke) Token: 0x06008289 RID: 33417
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void ResourcesRowChangeEventHandler(object sender, ResourceAuthorizationDataSet.ResourcesRowChangeEvent e);

		// Token: 0x02000554 RID: 1364
		// (Invoke) Token: 0x0600828D RID: 33421
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SecurityPrincipleCategoryRelationsRowChangeEventHandler(object sender, ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent e);

		// Token: 0x02000555 RID: 1365
		// (Invoke) Token: 0x06008291 RID: 33425
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void CategoryPermissionsRowChangeEventHandler(object sender, ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEvent e);

		// Token: 0x02000556 RID: 1366
		// (Invoke) Token: 0x06008295 RID: 33429
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GlobalPermissionsRowChangeEventHandler(object sender, ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEvent e);

		// Token: 0x02000557 RID: 1367
		// (Invoke) Token: 0x06008299 RID: 33433
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void GroupMembershipsRowChangeEventHandler(object sender, ResourceAuthorizationDataSet.GroupMembershipsRowChangeEvent e);

		// Token: 0x02000558 RID: 1368
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class ResourcesDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600829C RID: 33436 RVA: 0x00199358 File Offset: 0x00197558
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourcesDataTable()
			{
				base.TableName = "Resources";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600829D RID: 33437 RVA: 0x00199380 File Offset: 0x00197580
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal ResourcesDataTable(DataTable table)
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

			// Token: 0x0600829E RID: 33438 RVA: 0x00199428 File Offset: 0x00197628
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected ResourcesDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700277F RID: 10111
			// (get) Token: 0x0600829F RID: 33439 RVA: 0x00199438 File Offset: 0x00197638
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002780 RID: 10112
			// (get) Token: 0x060082A0 RID: 33440 RVA: 0x00199440 File Offset: 0x00197640
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WRES_ACCOUNTColumn
			{
				get
				{
					return this.columnWRES_ACCOUNT;
				}
			}

			// Token: 0x17002781 RID: 10113
			// (get) Token: 0x060082A1 RID: 33441 RVA: 0x00199448 File Offset: 0x00197648
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn RES_IS_WINDOWS_USERColumn
			{
				get
				{
					return this.columnRES_IS_WINDOWS_USER;
				}
			}

			// Token: 0x17002782 RID: 10114
			// (get) Token: 0x060082A2 RID: 33442 RVA: 0x00199450 File Offset: 0x00197650
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_PREVENT_ADSYNCColumn
			{
				get
				{
					return this.columnRES_PREVENT_ADSYNC;
				}
			}

			// Token: 0x17002783 RID: 10115
			// (get) Token: 0x060082A3 RID: 33443 RVA: 0x00199458 File Offset: 0x00197658
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WRES_AD_GUIDColumn
			{
				get
				{
					return this.columnWRES_AD_GUID;
				}
			}

			// Token: 0x17002784 RID: 10116
			// (get) Token: 0x060082A4 RID: 33444 RVA: 0x00199460 File Offset: 0x00197660
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_EXCHANGE_SYNCColumn
			{
				get
				{
					return this.columnRES_EXCHANGE_SYNC;
				}
			}

			// Token: 0x17002785 RID: 10117
			// (get) Token: 0x060082A5 RID: 33445 RVA: 0x00199468 File Offset: 0x00197668
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

			// Token: 0x17002786 RID: 10118
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.ResourcesRow this[int index]
			{
				get
				{
					return (ResourceAuthorizationDataSet.ResourcesRow)base.Rows[index];
				}
			}

			// Token: 0x1400048D RID: 1165
			// (add) Token: 0x060082A7 RID: 33447 RVA: 0x00199488 File Offset: 0x00197688
			// (remove) Token: 0x060082A8 RID: 33448 RVA: 0x001994C0 File Offset: 0x001976C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.ResourcesRowChangeEventHandler ResourcesRowChanging;

			// Token: 0x1400048E RID: 1166
			// (add) Token: 0x060082A9 RID: 33449 RVA: 0x001994F8 File Offset: 0x001976F8
			// (remove) Token: 0x060082AA RID: 33450 RVA: 0x00199530 File Offset: 0x00197730
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.ResourcesRowChangeEventHandler ResourcesRowChanged;

			// Token: 0x1400048F RID: 1167
			// (add) Token: 0x060082AB RID: 33451 RVA: 0x00199568 File Offset: 0x00197768
			// (remove) Token: 0x060082AC RID: 33452 RVA: 0x001995A0 File Offset: 0x001977A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.ResourcesRowChangeEventHandler ResourcesRowDeleting;

			// Token: 0x14000490 RID: 1168
			// (add) Token: 0x060082AD RID: 33453 RVA: 0x001995D8 File Offset: 0x001977D8
			// (remove) Token: 0x060082AE RID: 33454 RVA: 0x00199610 File Offset: 0x00197810
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.ResourcesRowChangeEventHandler ResourcesRowDeleted;

			// Token: 0x060082AF RID: 33455 RVA: 0x00199645 File Offset: 0x00197845
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddResourcesRow(ResourceAuthorizationDataSet.ResourcesRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060082B0 RID: 33456 RVA: 0x00199654 File Offset: 0x00197854
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.ResourcesRow AddResourcesRow(Guid RES_UID, string WRES_ACCOUNT, bool RES_IS_WINDOWS_USER, bool RES_PREVENT_ADSYNC, Guid WRES_AD_GUID, bool RES_EXCHANGE_SYNC)
			{
				ResourceAuthorizationDataSet.ResourcesRow resourcesRow = (ResourceAuthorizationDataSet.ResourcesRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					WRES_ACCOUNT,
					RES_IS_WINDOWS_USER,
					RES_PREVENT_ADSYNC,
					WRES_AD_GUID,
					RES_EXCHANGE_SYNC
				};
				resourcesRow.ItemArray = itemArray;
				base.Rows.Add(resourcesRow);
				return resourcesRow;
			}

			// Token: 0x060082B1 RID: 33457 RVA: 0x001996C0 File Offset: 0x001978C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.ResourcesRow FindByRES_UID(Guid RES_UID)
			{
				return (ResourceAuthorizationDataSet.ResourcesRow)base.Rows.Find(new object[]
				{
					RES_UID
				});
			}

			// Token: 0x060082B2 RID: 33458 RVA: 0x001996EE File Offset: 0x001978EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060082B3 RID: 33459 RVA: 0x001996FC File Offset: 0x001978FC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				ResourceAuthorizationDataSet.ResourcesDataTable resourcesDataTable = (ResourceAuthorizationDataSet.ResourcesDataTable)base.Clone();
				resourcesDataTable.InitVars();
				return resourcesDataTable;
			}

			// Token: 0x060082B4 RID: 33460 RVA: 0x0019971C File Offset: 0x0019791C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceAuthorizationDataSet.ResourcesDataTable();
			}

			// Token: 0x060082B5 RID: 33461 RVA: 0x00199724 File Offset: 0x00197924
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWRES_ACCOUNT = base.Columns["WRES_ACCOUNT"];
				this.columnRES_IS_WINDOWS_USER = base.Columns["RES_IS_WINDOWS_USER"];
				this.columnRES_PREVENT_ADSYNC = base.Columns["RES_PREVENT_ADSYNC"];
				this.columnWRES_AD_GUID = base.Columns["WRES_AD_GUID"];
				this.columnRES_EXCHANGE_SYNC = base.Columns["RES_EXCHANGE_SYNC"];
			}

			// Token: 0x060082B6 RID: 33462 RVA: 0x001997B8 File Offset: 0x001979B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnWRES_ACCOUNT = new DataColumn("WRES_ACCOUNT", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWRES_ACCOUNT);
				this.columnRES_IS_WINDOWS_USER = new DataColumn("RES_IS_WINDOWS_USER", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_IS_WINDOWS_USER);
				this.columnRES_PREVENT_ADSYNC = new DataColumn("RES_PREVENT_ADSYNC", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_PREVENT_ADSYNC);
				this.columnWRES_AD_GUID = new DataColumn("WRES_AD_GUID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWRES_AD_GUID);
				this.columnRES_EXCHANGE_SYNC = new DataColumn("RES_EXCHANGE_SYNC", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnRES_EXCHANGE_SYNC);
				base.Constraints.Add(new UniqueConstraint("ResourcesPrimaryKey", new DataColumn[]
				{
					this.columnRES_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnRES_UID.Unique = true;
				this.columnRES_PREVENT_ADSYNC.DefaultValue = false;
			}

			// Token: 0x060082B7 RID: 33463 RVA: 0x00199923 File Offset: 0x00197B23
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.ResourcesRow NewResourcesRow()
			{
				return (ResourceAuthorizationDataSet.ResourcesRow)base.NewRow();
			}

			// Token: 0x060082B8 RID: 33464 RVA: 0x00199930 File Offset: 0x00197B30
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceAuthorizationDataSet.ResourcesRow(builder);
			}

			// Token: 0x060082B9 RID: 33465 RVA: 0x00199938 File Offset: 0x00197B38
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceAuthorizationDataSet.ResourcesRow);
			}

			// Token: 0x060082BA RID: 33466 RVA: 0x00199944 File Offset: 0x00197B44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.ResourcesRowChanged != null)
				{
					this.ResourcesRowChanged(this, new ResourceAuthorizationDataSet.ResourcesRowChangeEvent((ResourceAuthorizationDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082BB RID: 33467 RVA: 0x00199977 File Offset: 0x00197B77
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.ResourcesRowChanging != null)
				{
					this.ResourcesRowChanging(this, new ResourceAuthorizationDataSet.ResourcesRowChangeEvent((ResourceAuthorizationDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082BC RID: 33468 RVA: 0x001999AA File Offset: 0x00197BAA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.ResourcesRowDeleted != null)
				{
					this.ResourcesRowDeleted(this, new ResourceAuthorizationDataSet.ResourcesRowChangeEvent((ResourceAuthorizationDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082BD RID: 33469 RVA: 0x001999DD File Offset: 0x00197BDD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.ResourcesRowDeleting != null)
				{
					this.ResourcesRowDeleting(this, new ResourceAuthorizationDataSet.ResourcesRowChangeEvent((ResourceAuthorizationDataSet.ResourcesRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082BE RID: 33470 RVA: 0x00199A10 File Offset: 0x00197C10
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveResourcesRow(ResourceAuthorizationDataSet.ResourcesRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060082BF RID: 33471 RVA: 0x00199A20 File Offset: 0x00197C20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceAuthorizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "ResourcesDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A1D RID: 6685
			private DataColumn columnRES_UID;

			// Token: 0x04001A1E RID: 6686
			private DataColumn columnWRES_ACCOUNT;

			// Token: 0x04001A1F RID: 6687
			private DataColumn columnRES_IS_WINDOWS_USER;

			// Token: 0x04001A20 RID: 6688
			private DataColumn columnRES_PREVENT_ADSYNC;

			// Token: 0x04001A21 RID: 6689
			private DataColumn columnWRES_AD_GUID;

			// Token: 0x04001A22 RID: 6690
			private DataColumn columnRES_EXCHANGE_SYNC;
		}

		// Token: 0x02000559 RID: 1369
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SecurityPrincipleCategoryRelationsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060082C0 RID: 33472 RVA: 0x00199C18 File Offset: 0x00197E18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityPrincipleCategoryRelationsDataTable()
			{
				base.TableName = "SecurityPrincipleCategoryRelations";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060082C1 RID: 33473 RVA: 0x00199C40 File Offset: 0x00197E40
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

			// Token: 0x060082C2 RID: 33474 RVA: 0x00199CE8 File Offset: 0x00197EE8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SecurityPrincipleCategoryRelationsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002787 RID: 10119
			// (get) Token: 0x060082C3 RID: 33475 RVA: 0x00199CF8 File Offset: 0x00197EF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002788 RID: 10120
			// (get) Token: 0x060082C4 RID: 33476 RVA: 0x00199D00 File Offset: 0x00197F00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x17002789 RID: 10121
			// (get) Token: 0x060082C5 RID: 33477 RVA: 0x00199D08 File Offset: 0x00197F08
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

			// Token: 0x1700278A RID: 10122
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow this[int index]
			{
				get
				{
					return (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)base.Rows[index];
				}
			}

			// Token: 0x14000491 RID: 1169
			// (add) Token: 0x060082C7 RID: 33479 RVA: 0x00199D28 File Offset: 0x00197F28
			// (remove) Token: 0x060082C8 RID: 33480 RVA: 0x00199D60 File Offset: 0x00197F60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowChanging;

			// Token: 0x14000492 RID: 1170
			// (add) Token: 0x060082C9 RID: 33481 RVA: 0x00199D98 File Offset: 0x00197F98
			// (remove) Token: 0x060082CA RID: 33482 RVA: 0x00199DD0 File Offset: 0x00197FD0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowChanged;

			// Token: 0x14000493 RID: 1171
			// (add) Token: 0x060082CB RID: 33483 RVA: 0x00199E08 File Offset: 0x00198008
			// (remove) Token: 0x060082CC RID: 33484 RVA: 0x00199E40 File Offset: 0x00198040
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowDeleting;

			// Token: 0x14000494 RID: 1172
			// (add) Token: 0x060082CD RID: 33485 RVA: 0x00199E78 File Offset: 0x00198078
			// (remove) Token: 0x060082CE RID: 33486 RVA: 0x00199EB0 File Offset: 0x001980B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEventHandler SecurityPrincipleCategoryRelationsRowDeleted;

			// Token: 0x060082CF RID: 33487 RVA: 0x00199EE5 File Offset: 0x001980E5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddSecurityPrincipleCategoryRelationsRow(ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060082D0 RID: 33488 RVA: 0x00199EF4 File Offset: 0x001980F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow AddSecurityPrincipleCategoryRelationsRow(Guid RES_UID, Guid WSEC_CAT_UID)
			{
				ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow securityPrincipleCategoryRelationsRow = (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					WSEC_CAT_UID
				};
				securityPrincipleCategoryRelationsRow.ItemArray = itemArray;
				base.Rows.Add(securityPrincipleCategoryRelationsRow);
				return securityPrincipleCategoryRelationsRow;
			}

			// Token: 0x060082D1 RID: 33489 RVA: 0x00199F3C File Offset: 0x0019813C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow FindByWSEC_CAT_UIDRES_UID(Guid WSEC_CAT_UID, Guid RES_UID)
			{
				return (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					RES_UID
				});
			}

			// Token: 0x060082D2 RID: 33490 RVA: 0x00199F73 File Offset: 0x00198173
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060082D3 RID: 33491 RVA: 0x00199F80 File Offset: 0x00198180
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable securityPrincipleCategoryRelationsDataTable = (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Clone();
				securityPrincipleCategoryRelationsDataTable.InitVars();
				return securityPrincipleCategoryRelationsDataTable;
			}

			// Token: 0x060082D4 RID: 33492 RVA: 0x00199FA0 File Offset: 0x001981A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable();
			}

			// Token: 0x060082D5 RID: 33493 RVA: 0x00199FA7 File Offset: 0x001981A7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
			}

			// Token: 0x060082D6 RID: 33494 RVA: 0x00199FD8 File Offset: 0x001981D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnWSEC_CAT_UID = new DataColumn("WSEC_CAT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_CAT_UID);
				base.Constraints.Add(new UniqueConstraint("SecurityPrincipleCategoryRelationsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_CAT_UID,
					this.columnRES_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.AllowDBNull = false;
			}

			// Token: 0x060082D7 RID: 33495 RVA: 0x0019A087 File Offset: 0x00198287
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow NewSecurityPrincipleCategoryRelationsRow()
			{
				return (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)base.NewRow();
			}

			// Token: 0x060082D8 RID: 33496 RVA: 0x0019A094 File Offset: 0x00198294
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow(builder);
			}

			// Token: 0x060082D9 RID: 33497 RVA: 0x0019A09C File Offset: 0x0019829C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow);
			}

			// Token: 0x060082DA RID: 33498 RVA: 0x0019A0A8 File Offset: 0x001982A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SecurityPrincipleCategoryRelationsRowChanged != null)
				{
					this.SecurityPrincipleCategoryRelationsRowChanged(this, new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082DB RID: 33499 RVA: 0x0019A0DB File Offset: 0x001982DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SecurityPrincipleCategoryRelationsRowChanging != null)
				{
					this.SecurityPrincipleCategoryRelationsRowChanging(this, new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082DC RID: 33500 RVA: 0x0019A10E File Offset: 0x0019830E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SecurityPrincipleCategoryRelationsRowDeleted != null)
				{
					this.SecurityPrincipleCategoryRelationsRowDeleted(this, new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082DD RID: 33501 RVA: 0x0019A141 File Offset: 0x00198341
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SecurityPrincipleCategoryRelationsRowDeleting != null)
				{
					this.SecurityPrincipleCategoryRelationsRowDeleting(this, new ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRowChangeEvent((ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082DE RID: 33502 RVA: 0x0019A174 File Offset: 0x00198374
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSecurityPrincipleCategoryRelationsRow(ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060082DF RID: 33503 RVA: 0x0019A184 File Offset: 0x00198384
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceAuthorizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SecurityPrincipleCategoryRelationsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A27 RID: 6695
			private DataColumn columnRES_UID;

			// Token: 0x04001A28 RID: 6696
			private DataColumn columnWSEC_CAT_UID;
		}

		// Token: 0x0200055A RID: 1370
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class CategoryPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x060082E0 RID: 33504 RVA: 0x0019A37C File Offset: 0x0019857C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CategoryPermissionsDataTable()
			{
				base.TableName = "CategoryPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060082E1 RID: 33505 RVA: 0x0019A3A4 File Offset: 0x001985A4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x060082E2 RID: 33506 RVA: 0x0019A44C File Offset: 0x0019864C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected CategoryPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700278B RID: 10123
			// (get) Token: 0x060082E3 RID: 33507 RVA: 0x0019A45C File Offset: 0x0019865C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x1700278C RID: 10124
			// (get) Token: 0x060082E4 RID: 33508 RVA: 0x0019A464 File Offset: 0x00198664
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_CAT_UIDColumn
			{
				get
				{
					return this.columnWSEC_CAT_UID;
				}
			}

			// Token: 0x1700278D RID: 10125
			// (get) Token: 0x060082E5 RID: 33509 RVA: 0x0019A46C File Offset: 0x0019866C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x1700278E RID: 10126
			// (get) Token: 0x060082E6 RID: 33510 RVA: 0x0019A474 File Offset: 0x00198674
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x1700278F RID: 10127
			// (get) Token: 0x060082E7 RID: 33511 RVA: 0x0019A47C File Offset: 0x0019867C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002790 RID: 10128
			// (get) Token: 0x060082E8 RID: 33512 RVA: 0x0019A484 File Offset: 0x00198684
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

			// Token: 0x17002791 RID: 10129
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.CategoryPermissionsRow this[int index]
			{
				get
				{
					return (ResourceAuthorizationDataSet.CategoryPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000495 RID: 1173
			// (add) Token: 0x060082EA RID: 33514 RVA: 0x0019A4A4 File Offset: 0x001986A4
			// (remove) Token: 0x060082EB RID: 33515 RVA: 0x0019A4DC File Offset: 0x001986DC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanging;

			// Token: 0x14000496 RID: 1174
			// (add) Token: 0x060082EC RID: 33516 RVA: 0x0019A514 File Offset: 0x00198714
			// (remove) Token: 0x060082ED RID: 33517 RVA: 0x0019A54C File Offset: 0x0019874C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowChanged;

			// Token: 0x14000497 RID: 1175
			// (add) Token: 0x060082EE RID: 33518 RVA: 0x0019A584 File Offset: 0x00198784
			// (remove) Token: 0x060082EF RID: 33519 RVA: 0x0019A5BC File Offset: 0x001987BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleting;

			// Token: 0x14000498 RID: 1176
			// (add) Token: 0x060082F0 RID: 33520 RVA: 0x0019A5F4 File Offset: 0x001987F4
			// (remove) Token: 0x060082F1 RID: 33521 RVA: 0x0019A62C File Offset: 0x0019882C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEventHandler CategoryPermissionsRowDeleted;

			// Token: 0x060082F2 RID: 33522 RVA: 0x0019A661 File Offset: 0x00198861
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddCategoryPermissionsRow(ResourceAuthorizationDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060082F3 RID: 33523 RVA: 0x0019A670 File Offset: 0x00198870
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.CategoryPermissionsRow AddCategoryPermissionsRow(Guid RES_UID, Guid WSEC_CAT_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				ResourceAuthorizationDataSet.CategoryPermissionsRow categoryPermissionsRow = (ResourceAuthorizationDataSet.CategoryPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					WSEC_CAT_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				categoryPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(categoryPermissionsRow);
				return categoryPermissionsRow;
			}

			// Token: 0x060082F4 RID: 33524 RVA: 0x0019A6D8 File Offset: 0x001988D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.CategoryPermissionsRow FindByWSEC_CAT_UIDWSEC_FEA_ACT_UIDRES_UID(Guid WSEC_CAT_UID, Guid WSEC_FEA_ACT_UID, Guid RES_UID)
			{
				return (ResourceAuthorizationDataSet.CategoryPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_CAT_UID,
					WSEC_FEA_ACT_UID,
					RES_UID
				});
			}

			// Token: 0x060082F5 RID: 33525 RVA: 0x0019A718 File Offset: 0x00198918
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060082F6 RID: 33526 RVA: 0x0019A728 File Offset: 0x00198928
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ResourceAuthorizationDataSet.CategoryPermissionsDataTable categoryPermissionsDataTable = (ResourceAuthorizationDataSet.CategoryPermissionsDataTable)base.Clone();
				categoryPermissionsDataTable.InitVars();
				return categoryPermissionsDataTable;
			}

			// Token: 0x060082F7 RID: 33527 RVA: 0x0019A748 File Offset: 0x00198948
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceAuthorizationDataSet.CategoryPermissionsDataTable();
			}

			// Token: 0x060082F8 RID: 33528 RVA: 0x0019A750 File Offset: 0x00198950
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_CAT_UID = base.Columns["WSEC_CAT_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x060082F9 RID: 33529 RVA: 0x0019A7CC File Offset: 0x001989CC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
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
					this.columnRES_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_CAT_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x060082FA RID: 33530 RVA: 0x0019A951 File Offset: 0x00198B51
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.CategoryPermissionsRow NewCategoryPermissionsRow()
			{
				return (ResourceAuthorizationDataSet.CategoryPermissionsRow)base.NewRow();
			}

			// Token: 0x060082FB RID: 33531 RVA: 0x0019A95E File Offset: 0x00198B5E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceAuthorizationDataSet.CategoryPermissionsRow(builder);
			}

			// Token: 0x060082FC RID: 33532 RVA: 0x0019A966 File Offset: 0x00198B66
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceAuthorizationDataSet.CategoryPermissionsRow);
			}

			// Token: 0x060082FD RID: 33533 RVA: 0x0019A972 File Offset: 0x00198B72
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.CategoryPermissionsRowChanged != null)
				{
					this.CategoryPermissionsRowChanged(this, new ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEvent((ResourceAuthorizationDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082FE RID: 33534 RVA: 0x0019A9A5 File Offset: 0x00198BA5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.CategoryPermissionsRowChanging != null)
				{
					this.CategoryPermissionsRowChanging(this, new ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEvent((ResourceAuthorizationDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x060082FF RID: 33535 RVA: 0x0019A9D8 File Offset: 0x00198BD8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.CategoryPermissionsRowDeleted != null)
				{
					this.CategoryPermissionsRowDeleted(this, new ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEvent((ResourceAuthorizationDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008300 RID: 33536 RVA: 0x0019AA0B File Offset: 0x00198C0B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.CategoryPermissionsRowDeleting != null)
				{
					this.CategoryPermissionsRowDeleting(this, new ResourceAuthorizationDataSet.CategoryPermissionsRowChangeEvent((ResourceAuthorizationDataSet.CategoryPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008301 RID: 33537 RVA: 0x0019AA3E File Offset: 0x00198C3E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveCategoryPermissionsRow(ResourceAuthorizationDataSet.CategoryPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008302 RID: 33538 RVA: 0x0019AA4C File Offset: 0x00198C4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceAuthorizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "CategoryPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A2D RID: 6701
			private DataColumn columnRES_UID;

			// Token: 0x04001A2E RID: 6702
			private DataColumn columnWSEC_CAT_UID;

			// Token: 0x04001A2F RID: 6703
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001A30 RID: 6704
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001A31 RID: 6705
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x0200055B RID: 1371
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GlobalPermissionsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008303 RID: 33539 RVA: 0x0019AC44 File Offset: 0x00198E44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GlobalPermissionsDataTable()
			{
				base.TableName = "GlobalPermissions";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008304 RID: 33540 RVA: 0x0019AC6C File Offset: 0x00198E6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x06008305 RID: 33541 RVA: 0x0019AD14 File Offset: 0x00198F14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GlobalPermissionsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002792 RID: 10130
			// (get) Token: 0x06008306 RID: 33542 RVA: 0x0019AD24 File Offset: 0x00198F24
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002793 RID: 10131
			// (get) Token: 0x06008307 RID: 33543 RVA: 0x0019AD2C File Offset: 0x00198F2C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_FEA_ACT_UIDColumn
			{
				get
				{
					return this.columnWSEC_FEA_ACT_UID;
				}
			}

			// Token: 0x17002794 RID: 10132
			// (get) Token: 0x06008308 RID: 33544 RVA: 0x0019AD34 File Offset: 0x00198F34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_ALLOWColumn
			{
				get
				{
					return this.columnWSEC_ALLOW;
				}
			}

			// Token: 0x17002795 RID: 10133
			// (get) Token: 0x06008309 RID: 33545 RVA: 0x0019AD3C File Offset: 0x00198F3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WSEC_DENYColumn
			{
				get
				{
					return this.columnWSEC_DENY;
				}
			}

			// Token: 0x17002796 RID: 10134
			// (get) Token: 0x0600830A RID: 33546 RVA: 0x0019AD44 File Offset: 0x00198F44
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

			// Token: 0x17002797 RID: 10135
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.GlobalPermissionsRow this[int index]
			{
				get
				{
					return (ResourceAuthorizationDataSet.GlobalPermissionsRow)base.Rows[index];
				}
			}

			// Token: 0x14000499 RID: 1177
			// (add) Token: 0x0600830C RID: 33548 RVA: 0x0019AD64 File Offset: 0x00198F64
			// (remove) Token: 0x0600830D RID: 33549 RVA: 0x0019AD9C File Offset: 0x00198F9C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanging;

			// Token: 0x1400049A RID: 1178
			// (add) Token: 0x0600830E RID: 33550 RVA: 0x0019ADD4 File Offset: 0x00198FD4
			// (remove) Token: 0x0600830F RID: 33551 RVA: 0x0019AE0C File Offset: 0x0019900C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowChanged;

			// Token: 0x1400049B RID: 1179
			// (add) Token: 0x06008310 RID: 33552 RVA: 0x0019AE44 File Offset: 0x00199044
			// (remove) Token: 0x06008311 RID: 33553 RVA: 0x0019AE7C File Offset: 0x0019907C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleting;

			// Token: 0x1400049C RID: 1180
			// (add) Token: 0x06008312 RID: 33554 RVA: 0x0019AEB4 File Offset: 0x001990B4
			// (remove) Token: 0x06008313 RID: 33555 RVA: 0x0019AEEC File Offset: 0x001990EC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEventHandler GlobalPermissionsRowDeleted;

			// Token: 0x06008314 RID: 33556 RVA: 0x0019AF21 File Offset: 0x00199121
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGlobalPermissionsRow(ResourceAuthorizationDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008315 RID: 33557 RVA: 0x0019AF30 File Offset: 0x00199130
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GlobalPermissionsRow AddGlobalPermissionsRow(Guid RES_UID, Guid WSEC_FEA_ACT_UID, bool WSEC_ALLOW, bool WSEC_DENY)
			{
				ResourceAuthorizationDataSet.GlobalPermissionsRow globalPermissionsRow = (ResourceAuthorizationDataSet.GlobalPermissionsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					WSEC_FEA_ACT_UID,
					WSEC_ALLOW,
					WSEC_DENY
				};
				globalPermissionsRow.ItemArray = itemArray;
				base.Rows.Add(globalPermissionsRow);
				return globalPermissionsRow;
			}

			// Token: 0x06008316 RID: 33558 RVA: 0x0019AF8C File Offset: 0x0019918C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GlobalPermissionsRow FindByWSEC_FEA_ACT_UIDRES_UID(Guid WSEC_FEA_ACT_UID, Guid RES_UID)
			{
				return (ResourceAuthorizationDataSet.GlobalPermissionsRow)base.Rows.Find(new object[]
				{
					WSEC_FEA_ACT_UID,
					RES_UID
				});
			}

			// Token: 0x06008317 RID: 33559 RVA: 0x0019AFC3 File Offset: 0x001991C3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008318 RID: 33560 RVA: 0x0019AFD0 File Offset: 0x001991D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ResourceAuthorizationDataSet.GlobalPermissionsDataTable globalPermissionsDataTable = (ResourceAuthorizationDataSet.GlobalPermissionsDataTable)base.Clone();
				globalPermissionsDataTable.InitVars();
				return globalPermissionsDataTable;
			}

			// Token: 0x06008319 RID: 33561 RVA: 0x0019AFF0 File Offset: 0x001991F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceAuthorizationDataSet.GlobalPermissionsDataTable();
			}

			// Token: 0x0600831A RID: 33562 RVA: 0x0019AFF8 File Offset: 0x001991F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_FEA_ACT_UID = base.Columns["WSEC_FEA_ACT_UID"];
				this.columnWSEC_ALLOW = base.Columns["WSEC_ALLOW"];
				this.columnWSEC_DENY = base.Columns["WSEC_DENY"];
			}

			// Token: 0x0600831B RID: 33563 RVA: 0x0019B060 File Offset: 0x00199260
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnWSEC_FEA_ACT_UID = new DataColumn("WSEC_FEA_ACT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_FEA_ACT_UID);
				this.columnWSEC_ALLOW = new DataColumn("WSEC_ALLOW", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_ALLOW);
				this.columnWSEC_DENY = new DataColumn("WSEC_DENY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_DENY);
				base.Constraints.Add(new UniqueConstraint("GlobalPermissionsPrimaryKey", new DataColumn[]
				{
					this.columnWSEC_FEA_ACT_UID,
					this.columnRES_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_FEA_ACT_UID.AllowDBNull = false;
				this.columnWSEC_ALLOW.AllowDBNull = false;
				this.columnWSEC_ALLOW.DefaultValue = false;
				this.columnWSEC_DENY.AllowDBNull = false;
				this.columnWSEC_DENY.DefaultValue = false;
			}

			// Token: 0x0600831C RID: 33564 RVA: 0x0019B1A3 File Offset: 0x001993A3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GlobalPermissionsRow NewGlobalPermissionsRow()
			{
				return (ResourceAuthorizationDataSet.GlobalPermissionsRow)base.NewRow();
			}

			// Token: 0x0600831D RID: 33565 RVA: 0x0019B1B0 File Offset: 0x001993B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceAuthorizationDataSet.GlobalPermissionsRow(builder);
			}

			// Token: 0x0600831E RID: 33566 RVA: 0x0019B1B8 File Offset: 0x001993B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceAuthorizationDataSet.GlobalPermissionsRow);
			}

			// Token: 0x0600831F RID: 33567 RVA: 0x0019B1C4 File Offset: 0x001993C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GlobalPermissionsRowChanged != null)
				{
					this.GlobalPermissionsRowChanged(this, new ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEvent((ResourceAuthorizationDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008320 RID: 33568 RVA: 0x0019B1F7 File Offset: 0x001993F7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GlobalPermissionsRowChanging != null)
				{
					this.GlobalPermissionsRowChanging(this, new ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEvent((ResourceAuthorizationDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008321 RID: 33569 RVA: 0x0019B22A File Offset: 0x0019942A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GlobalPermissionsRowDeleted != null)
				{
					this.GlobalPermissionsRowDeleted(this, new ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEvent((ResourceAuthorizationDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008322 RID: 33570 RVA: 0x0019B25D File Offset: 0x0019945D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GlobalPermissionsRowDeleting != null)
				{
					this.GlobalPermissionsRowDeleting(this, new ResourceAuthorizationDataSet.GlobalPermissionsRowChangeEvent((ResourceAuthorizationDataSet.GlobalPermissionsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008323 RID: 33571 RVA: 0x0019B290 File Offset: 0x00199490
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveGlobalPermissionsRow(ResourceAuthorizationDataSet.GlobalPermissionsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008324 RID: 33572 RVA: 0x0019B2A0 File Offset: 0x001994A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceAuthorizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GlobalPermissionsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A36 RID: 6710
			private DataColumn columnRES_UID;

			// Token: 0x04001A37 RID: 6711
			private DataColumn columnWSEC_FEA_ACT_UID;

			// Token: 0x04001A38 RID: 6712
			private DataColumn columnWSEC_ALLOW;

			// Token: 0x04001A39 RID: 6713
			private DataColumn columnWSEC_DENY;
		}

		// Token: 0x0200055C RID: 1372
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class GroupMembershipsDataTable : DataTable, IEnumerable
		{
			// Token: 0x06008325 RID: 33573 RVA: 0x0019B498 File Offset: 0x00199698
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupMembershipsDataTable()
			{
				base.TableName = "GroupMemberships";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x06008326 RID: 33574 RVA: 0x0019B4C0 File Offset: 0x001996C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupMembershipsDataTable(DataTable table)
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

			// Token: 0x06008327 RID: 33575 RVA: 0x0019B568 File Offset: 0x00199768
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected GroupMembershipsDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002798 RID: 10136
			// (get) Token: 0x06008328 RID: 33576 RVA: 0x0019B578 File Offset: 0x00199778
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn RES_UIDColumn
			{
				get
				{
					return this.columnRES_UID;
				}
			}

			// Token: 0x17002799 RID: 10137
			// (get) Token: 0x06008329 RID: 33577 RVA: 0x0019B580 File Offset: 0x00199780
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WSEC_GRP_UIDColumn
			{
				get
				{
					return this.columnWSEC_GRP_UID;
				}
			}

			// Token: 0x1700279A RID: 10138
			// (get) Token: 0x0600832A RID: 33578 RVA: 0x0019B588 File Offset: 0x00199788
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

			// Token: 0x1700279B RID: 10139
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GroupMembershipsRow this[int index]
			{
				get
				{
					return (ResourceAuthorizationDataSet.GroupMembershipsRow)base.Rows[index];
				}
			}

			// Token: 0x1400049D RID: 1181
			// (add) Token: 0x0600832C RID: 33580 RVA: 0x0019B5A8 File Offset: 0x001997A8
			// (remove) Token: 0x0600832D RID: 33581 RVA: 0x0019B5E0 File Offset: 0x001997E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GroupMembershipsRowChangeEventHandler GroupMembershipsRowChanging;

			// Token: 0x1400049E RID: 1182
			// (add) Token: 0x0600832E RID: 33582 RVA: 0x0019B618 File Offset: 0x00199818
			// (remove) Token: 0x0600832F RID: 33583 RVA: 0x0019B650 File Offset: 0x00199850
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GroupMembershipsRowChangeEventHandler GroupMembershipsRowChanged;

			// Token: 0x1400049F RID: 1183
			// (add) Token: 0x06008330 RID: 33584 RVA: 0x0019B688 File Offset: 0x00199888
			// (remove) Token: 0x06008331 RID: 33585 RVA: 0x0019B6C0 File Offset: 0x001998C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GroupMembershipsRowChangeEventHandler GroupMembershipsRowDeleting;

			// Token: 0x140004A0 RID: 1184
			// (add) Token: 0x06008332 RID: 33586 RVA: 0x0019B6F8 File Offset: 0x001998F8
			// (remove) Token: 0x06008333 RID: 33587 RVA: 0x0019B730 File Offset: 0x00199930
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event ResourceAuthorizationDataSet.GroupMembershipsRowChangeEventHandler GroupMembershipsRowDeleted;

			// Token: 0x06008334 RID: 33588 RVA: 0x0019B765 File Offset: 0x00199965
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddGroupMembershipsRow(ResourceAuthorizationDataSet.GroupMembershipsRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x06008335 RID: 33589 RVA: 0x0019B774 File Offset: 0x00199974
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GroupMembershipsRow AddGroupMembershipsRow(Guid RES_UID, Guid WSEC_GRP_UID)
			{
				ResourceAuthorizationDataSet.GroupMembershipsRow groupMembershipsRow = (ResourceAuthorizationDataSet.GroupMembershipsRow)base.NewRow();
				object[] itemArray = new object[]
				{
					RES_UID,
					WSEC_GRP_UID
				};
				groupMembershipsRow.ItemArray = itemArray;
				base.Rows.Add(groupMembershipsRow);
				return groupMembershipsRow;
			}

			// Token: 0x06008336 RID: 33590 RVA: 0x0019B7BC File Offset: 0x001999BC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GroupMembershipsRow FindByRES_UIDWSEC_GRP_UID(Guid RES_UID, Guid WSEC_GRP_UID)
			{
				return (ResourceAuthorizationDataSet.GroupMembershipsRow)base.Rows.Find(new object[]
				{
					RES_UID,
					WSEC_GRP_UID
				});
			}

			// Token: 0x06008337 RID: 33591 RVA: 0x0019B7F3 File Offset: 0x001999F3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x06008338 RID: 33592 RVA: 0x0019B800 File Offset: 0x00199A00
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public override DataTable Clone()
			{
				ResourceAuthorizationDataSet.GroupMembershipsDataTable groupMembershipsDataTable = (ResourceAuthorizationDataSet.GroupMembershipsDataTable)base.Clone();
				groupMembershipsDataTable.InitVars();
				return groupMembershipsDataTable;
			}

			// Token: 0x06008339 RID: 33593 RVA: 0x0019B820 File Offset: 0x00199A20
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new ResourceAuthorizationDataSet.GroupMembershipsDataTable();
			}

			// Token: 0x0600833A RID: 33594 RVA: 0x0019B827 File Offset: 0x00199A27
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnRES_UID = base.Columns["RES_UID"];
				this.columnWSEC_GRP_UID = base.Columns["WSEC_GRP_UID"];
			}

			// Token: 0x0600833B RID: 33595 RVA: 0x0019B858 File Offset: 0x00199A58
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnRES_UID = new DataColumn("RES_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnRES_UID);
				this.columnWSEC_GRP_UID = new DataColumn("WSEC_GRP_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWSEC_GRP_UID);
				base.Constraints.Add(new UniqueConstraint("GroupMembershipsPrimaryKey", new DataColumn[]
				{
					this.columnRES_UID,
					this.columnWSEC_GRP_UID
				}, true));
				this.columnRES_UID.AllowDBNull = false;
				this.columnWSEC_GRP_UID.AllowDBNull = false;
			}

			// Token: 0x0600833C RID: 33596 RVA: 0x0019B907 File Offset: 0x00199B07
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GroupMembershipsRow NewGroupMembershipsRow()
			{
				return (ResourceAuthorizationDataSet.GroupMembershipsRow)base.NewRow();
			}

			// Token: 0x0600833D RID: 33597 RVA: 0x0019B914 File Offset: 0x00199B14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new ResourceAuthorizationDataSet.GroupMembershipsRow(builder);
			}

			// Token: 0x0600833E RID: 33598 RVA: 0x0019B91C File Offset: 0x00199B1C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(ResourceAuthorizationDataSet.GroupMembershipsRow);
			}

			// Token: 0x0600833F RID: 33599 RVA: 0x0019B928 File Offset: 0x00199B28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.GroupMembershipsRowChanged != null)
				{
					this.GroupMembershipsRowChanged(this, new ResourceAuthorizationDataSet.GroupMembershipsRowChangeEvent((ResourceAuthorizationDataSet.GroupMembershipsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008340 RID: 33600 RVA: 0x0019B95B File Offset: 0x00199B5B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.GroupMembershipsRowChanging != null)
				{
					this.GroupMembershipsRowChanging(this, new ResourceAuthorizationDataSet.GroupMembershipsRowChangeEvent((ResourceAuthorizationDataSet.GroupMembershipsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008341 RID: 33601 RVA: 0x0019B98E File Offset: 0x00199B8E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.GroupMembershipsRowDeleted != null)
				{
					this.GroupMembershipsRowDeleted(this, new ResourceAuthorizationDataSet.GroupMembershipsRowChangeEvent((ResourceAuthorizationDataSet.GroupMembershipsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008342 RID: 33602 RVA: 0x0019B9C1 File Offset: 0x00199BC1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.GroupMembershipsRowDeleting != null)
				{
					this.GroupMembershipsRowDeleting(this, new ResourceAuthorizationDataSet.GroupMembershipsRowChangeEvent((ResourceAuthorizationDataSet.GroupMembershipsRow)e.Row, e.Action));
				}
			}

			// Token: 0x06008343 RID: 33603 RVA: 0x0019B9F4 File Offset: 0x00199BF4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveGroupMembershipsRow(ResourceAuthorizationDataSet.GroupMembershipsRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x06008344 RID: 33604 RVA: 0x0019BA04 File Offset: 0x00199C04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				ResourceAuthorizationDataSet resourceAuthorizationDataSet = new ResourceAuthorizationDataSet();
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
				xmlSchemaAttribute.FixedValue = resourceAuthorizationDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "GroupMembershipsDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = resourceAuthorizationDataSet.GetSchemaSerializable();
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

			// Token: 0x04001A3E RID: 6718
			private DataColumn columnRES_UID;

			// Token: 0x04001A3F RID: 6719
			private DataColumn columnWSEC_GRP_UID;
		}

		// Token: 0x0200055D RID: 1373
		public class ResourcesRow : DataRow
		{
			// Token: 0x06008345 RID: 33605 RVA: 0x0019BBFC File Offset: 0x00199DFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal ResourcesRow(DataRowBuilder rb) : base(rb)
			{
				this.tableResources = (ResourceAuthorizationDataSet.ResourcesDataTable)base.Table;
			}

			// Token: 0x1700279C RID: 10140
			// (get) Token: 0x06008346 RID: 33606 RVA: 0x0019BC16 File Offset: 0x00199E16
			// (set) Token: 0x06008347 RID: 33607 RVA: 0x0019BC2E File Offset: 0x00199E2E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableResources.RES_UIDColumn];
				}
				set
				{
					base[this.tableResources.RES_UIDColumn] = value;
				}
			}

			// Token: 0x1700279D RID: 10141
			// (get) Token: 0x06008348 RID: 33608 RVA: 0x0019BC48 File Offset: 0x00199E48
			// (set) Token: 0x06008349 RID: 33609 RVA: 0x0019BC8C File Offset: 0x00199E8C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WRES_ACCOUNT
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableResources.WRES_ACCOUNTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WRES_ACCOUNT' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.WRES_ACCOUNTColumn] = value;
				}
			}

			// Token: 0x1700279E RID: 10142
			// (get) Token: 0x0600834A RID: 33610 RVA: 0x0019BCA0 File Offset: 0x00199EA0
			// (set) Token: 0x0600834B RID: 33611 RVA: 0x0019BCE4 File Offset: 0x00199EE4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_IS_WINDOWS_USER
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_IS_WINDOWS_USERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_IS_WINDOWS_USER' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_IS_WINDOWS_USERColumn] = value;
				}
			}

			// Token: 0x1700279F RID: 10143
			// (get) Token: 0x0600834C RID: 33612 RVA: 0x0019BD00 File Offset: 0x00199F00
			// (set) Token: 0x0600834D RID: 33613 RVA: 0x0019BD44 File Offset: 0x00199F44
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool RES_PREVENT_ADSYNC
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_PREVENT_ADSYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_PREVENT_ADSYNC' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_PREVENT_ADSYNCColumn] = value;
				}
			}

			// Token: 0x170027A0 RID: 10144
			// (get) Token: 0x0600834E RID: 33614 RVA: 0x0019BD60 File Offset: 0x00199F60
			// (set) Token: 0x0600834F RID: 33615 RVA: 0x0019BDA4 File Offset: 0x00199FA4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WRES_AD_GUID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableResources.WRES_AD_GUIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WRES_AD_GUID' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.WRES_AD_GUIDColumn] = value;
				}
			}

			// Token: 0x170027A1 RID: 10145
			// (get) Token: 0x06008350 RID: 33616 RVA: 0x0019BDC0 File Offset: 0x00199FC0
			// (set) Token: 0x06008351 RID: 33617 RVA: 0x0019BE04 File Offset: 0x0019A004
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool RES_EXCHANGE_SYNC
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableResources.RES_EXCHANGE_SYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'RES_EXCHANGE_SYNC' in table 'Resources' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableResources.RES_EXCHANGE_SYNCColumn] = value;
				}
			}

			// Token: 0x06008352 RID: 33618 RVA: 0x0019BE1D File Offset: 0x0019A01D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWRES_ACCOUNTNull()
			{
				return base.IsNull(this.tableResources.WRES_ACCOUNTColumn);
			}

			// Token: 0x06008353 RID: 33619 RVA: 0x0019BE30 File Offset: 0x0019A030
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWRES_ACCOUNTNull()
			{
				base[this.tableResources.WRES_ACCOUNTColumn] = Convert.DBNull;
			}

			// Token: 0x06008354 RID: 33620 RVA: 0x0019BE48 File Offset: 0x0019A048
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_IS_WINDOWS_USERNull()
			{
				return base.IsNull(this.tableResources.RES_IS_WINDOWS_USERColumn);
			}

			// Token: 0x06008355 RID: 33621 RVA: 0x0019BE5B File Offset: 0x0019A05B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_IS_WINDOWS_USERNull()
			{
				base[this.tableResources.RES_IS_WINDOWS_USERColumn] = Convert.DBNull;
			}

			// Token: 0x06008356 RID: 33622 RVA: 0x0019BE73 File Offset: 0x0019A073
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_PREVENT_ADSYNCNull()
			{
				return base.IsNull(this.tableResources.RES_PREVENT_ADSYNCColumn);
			}

			// Token: 0x06008357 RID: 33623 RVA: 0x0019BE86 File Offset: 0x0019A086
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_PREVENT_ADSYNCNull()
			{
				base[this.tableResources.RES_PREVENT_ADSYNCColumn] = Convert.DBNull;
			}

			// Token: 0x06008358 RID: 33624 RVA: 0x0019BE9E File Offset: 0x0019A09E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWRES_AD_GUIDNull()
			{
				return base.IsNull(this.tableResources.WRES_AD_GUIDColumn);
			}

			// Token: 0x06008359 RID: 33625 RVA: 0x0019BEB1 File Offset: 0x0019A0B1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWRES_AD_GUIDNull()
			{
				base[this.tableResources.WRES_AD_GUIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600835A RID: 33626 RVA: 0x0019BEC9 File Offset: 0x0019A0C9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsRES_EXCHANGE_SYNCNull()
			{
				return base.IsNull(this.tableResources.RES_EXCHANGE_SYNCColumn);
			}

			// Token: 0x0600835B RID: 33627 RVA: 0x0019BEDC File Offset: 0x0019A0DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetRES_EXCHANGE_SYNCNull()
			{
				base[this.tableResources.RES_EXCHANGE_SYNCColumn] = Convert.DBNull;
			}

			// Token: 0x04001A44 RID: 6724
			private ResourceAuthorizationDataSet.ResourcesDataTable tableResources;
		}

		// Token: 0x0200055E RID: 1374
		public class SecurityPrincipleCategoryRelationsRow : DataRow
		{
			// Token: 0x0600835C RID: 33628 RVA: 0x0019BEF4 File Offset: 0x0019A0F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SecurityPrincipleCategoryRelationsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSecurityPrincipleCategoryRelations = (ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable)base.Table;
			}

			// Token: 0x170027A2 RID: 10146
			// (get) Token: 0x0600835D RID: 33629 RVA: 0x0019BF0E File Offset: 0x0019A10E
			// (set) Token: 0x0600835E RID: 33630 RVA: 0x0019BF26 File Offset: 0x0019A126
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableSecurityPrincipleCategoryRelations.RES_UIDColumn];
				}
				set
				{
					base[this.tableSecurityPrincipleCategoryRelations.RES_UIDColumn] = value;
				}
			}

			// Token: 0x170027A3 RID: 10147
			// (get) Token: 0x0600835F RID: 33631 RVA: 0x0019BF3F File Offset: 0x0019A13F
			// (set) Token: 0x06008360 RID: 33632 RVA: 0x0019BF57 File Offset: 0x0019A157
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x04001A45 RID: 6725
			private ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsDataTable tableSecurityPrincipleCategoryRelations;
		}

		// Token: 0x0200055F RID: 1375
		public class CategoryPermissionsRow : DataRow
		{
			// Token: 0x06008361 RID: 33633 RVA: 0x0019BF70 File Offset: 0x0019A170
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal CategoryPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableCategoryPermissions = (ResourceAuthorizationDataSet.CategoryPermissionsDataTable)base.Table;
			}

			// Token: 0x170027A4 RID: 10148
			// (get) Token: 0x06008362 RID: 33634 RVA: 0x0019BF8A File Offset: 0x0019A18A
			// (set) Token: 0x06008363 RID: 33635 RVA: 0x0019BFA2 File Offset: 0x0019A1A2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableCategoryPermissions.RES_UIDColumn];
				}
				set
				{
					base[this.tableCategoryPermissions.RES_UIDColumn] = value;
				}
			}

			// Token: 0x170027A5 RID: 10149
			// (get) Token: 0x06008364 RID: 33636 RVA: 0x0019BFBB File Offset: 0x0019A1BB
			// (set) Token: 0x06008365 RID: 33637 RVA: 0x0019BFD3 File Offset: 0x0019A1D3
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

			// Token: 0x170027A6 RID: 10150
			// (get) Token: 0x06008366 RID: 33638 RVA: 0x0019BFEC File Offset: 0x0019A1EC
			// (set) Token: 0x06008367 RID: 33639 RVA: 0x0019C004 File Offset: 0x0019A204
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

			// Token: 0x170027A7 RID: 10151
			// (get) Token: 0x06008368 RID: 33640 RVA: 0x0019C01D File Offset: 0x0019A21D
			// (set) Token: 0x06008369 RID: 33641 RVA: 0x0019C035 File Offset: 0x0019A235
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

			// Token: 0x170027A8 RID: 10152
			// (get) Token: 0x0600836A RID: 33642 RVA: 0x0019C04E File Offset: 0x0019A24E
			// (set) Token: 0x0600836B RID: 33643 RVA: 0x0019C066 File Offset: 0x0019A266
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

			// Token: 0x04001A46 RID: 6726
			private ResourceAuthorizationDataSet.CategoryPermissionsDataTable tableCategoryPermissions;
		}

		// Token: 0x02000560 RID: 1376
		public class GlobalPermissionsRow : DataRow
		{
			// Token: 0x0600836C RID: 33644 RVA: 0x0019C07F File Offset: 0x0019A27F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GlobalPermissionsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGlobalPermissions = (ResourceAuthorizationDataSet.GlobalPermissionsDataTable)base.Table;
			}

			// Token: 0x170027A9 RID: 10153
			// (get) Token: 0x0600836D RID: 33645 RVA: 0x0019C099 File Offset: 0x0019A299
			// (set) Token: 0x0600836E RID: 33646 RVA: 0x0019C0B1 File Offset: 0x0019A2B1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableGlobalPermissions.RES_UIDColumn];
				}
				set
				{
					base[this.tableGlobalPermissions.RES_UIDColumn] = value;
				}
			}

			// Token: 0x170027AA RID: 10154
			// (get) Token: 0x0600836F RID: 33647 RVA: 0x0019C0CA File Offset: 0x0019A2CA
			// (set) Token: 0x06008370 RID: 33648 RVA: 0x0019C0E2 File Offset: 0x0019A2E2
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

			// Token: 0x170027AB RID: 10155
			// (get) Token: 0x06008371 RID: 33649 RVA: 0x0019C0FB File Offset: 0x0019A2FB
			// (set) Token: 0x06008372 RID: 33650 RVA: 0x0019C113 File Offset: 0x0019A313
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

			// Token: 0x170027AC RID: 10156
			// (get) Token: 0x06008373 RID: 33651 RVA: 0x0019C12C File Offset: 0x0019A32C
			// (set) Token: 0x06008374 RID: 33652 RVA: 0x0019C144 File Offset: 0x0019A344
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
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

			// Token: 0x04001A47 RID: 6727
			private ResourceAuthorizationDataSet.GlobalPermissionsDataTable tableGlobalPermissions;
		}

		// Token: 0x02000561 RID: 1377
		public class GroupMembershipsRow : DataRow
		{
			// Token: 0x06008375 RID: 33653 RVA: 0x0019C15D File Offset: 0x0019A35D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal GroupMembershipsRow(DataRowBuilder rb) : base(rb)
			{
				this.tableGroupMemberships = (ResourceAuthorizationDataSet.GroupMembershipsDataTable)base.Table;
			}

			// Token: 0x170027AD RID: 10157
			// (get) Token: 0x06008376 RID: 33654 RVA: 0x0019C177 File Offset: 0x0019A377
			// (set) Token: 0x06008377 RID: 33655 RVA: 0x0019C18F File Offset: 0x0019A38F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid RES_UID
			{
				get
				{
					return (Guid)base[this.tableGroupMemberships.RES_UIDColumn];
				}
				set
				{
					base[this.tableGroupMemberships.RES_UIDColumn] = value;
				}
			}

			// Token: 0x170027AE RID: 10158
			// (get) Token: 0x06008378 RID: 33656 RVA: 0x0019C1A8 File Offset: 0x0019A3A8
			// (set) Token: 0x06008379 RID: 33657 RVA: 0x0019C1C0 File Offset: 0x0019A3C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WSEC_GRP_UID
			{
				get
				{
					return (Guid)base[this.tableGroupMemberships.WSEC_GRP_UIDColumn];
				}
				set
				{
					base[this.tableGroupMemberships.WSEC_GRP_UIDColumn] = value;
				}
			}

			// Token: 0x04001A48 RID: 6728
			private ResourceAuthorizationDataSet.GroupMembershipsDataTable tableGroupMemberships;
		}

		// Token: 0x02000562 RID: 1378
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class ResourcesRowChangeEvent : EventArgs
		{
			// Token: 0x0600837A RID: 33658 RVA: 0x0019C1D9 File Offset: 0x0019A3D9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourcesRowChangeEvent(ResourceAuthorizationDataSet.ResourcesRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170027AF RID: 10159
			// (get) Token: 0x0600837B RID: 33659 RVA: 0x0019C1EF File Offset: 0x0019A3EF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.ResourcesRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170027B0 RID: 10160
			// (get) Token: 0x0600837C RID: 33660 RVA: 0x0019C1F7 File Offset: 0x0019A3F7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001A49 RID: 6729
			private ResourceAuthorizationDataSet.ResourcesRow eventRow;

			// Token: 0x04001A4A RID: 6730
			private DataRowAction eventAction;
		}

		// Token: 0x02000563 RID: 1379
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SecurityPrincipleCategoryRelationsRowChangeEvent : EventArgs
		{
			// Token: 0x0600837D RID: 33661 RVA: 0x0019C1FF File Offset: 0x0019A3FF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SecurityPrincipleCategoryRelationsRowChangeEvent(ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170027B1 RID: 10161
			// (get) Token: 0x0600837E RID: 33662 RVA: 0x0019C215 File Offset: 0x0019A415
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170027B2 RID: 10162
			// (get) Token: 0x0600837F RID: 33663 RVA: 0x0019C21D File Offset: 0x0019A41D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001A4B RID: 6731
			private ResourceAuthorizationDataSet.SecurityPrincipleCategoryRelationsRow eventRow;

			// Token: 0x04001A4C RID: 6732
			private DataRowAction eventAction;
		}

		// Token: 0x02000564 RID: 1380
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class CategoryPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x06008380 RID: 33664 RVA: 0x0019C225 File Offset: 0x0019A425
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public CategoryPermissionsRowChangeEvent(ResourceAuthorizationDataSet.CategoryPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170027B3 RID: 10163
			// (get) Token: 0x06008381 RID: 33665 RVA: 0x0019C23B File Offset: 0x0019A43B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.CategoryPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170027B4 RID: 10164
			// (get) Token: 0x06008382 RID: 33666 RVA: 0x0019C243 File Offset: 0x0019A443
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001A4D RID: 6733
			private ResourceAuthorizationDataSet.CategoryPermissionsRow eventRow;

			// Token: 0x04001A4E RID: 6734
			private DataRowAction eventAction;
		}

		// Token: 0x02000565 RID: 1381
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GlobalPermissionsRowChangeEvent : EventArgs
		{
			// Token: 0x06008383 RID: 33667 RVA: 0x0019C24B File Offset: 0x0019A44B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GlobalPermissionsRowChangeEvent(ResourceAuthorizationDataSet.GlobalPermissionsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170027B5 RID: 10165
			// (get) Token: 0x06008384 RID: 33668 RVA: 0x0019C261 File Offset: 0x0019A461
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public ResourceAuthorizationDataSet.GlobalPermissionsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170027B6 RID: 10166
			// (get) Token: 0x06008385 RID: 33669 RVA: 0x0019C269 File Offset: 0x0019A469
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001A4F RID: 6735
			private ResourceAuthorizationDataSet.GlobalPermissionsRow eventRow;

			// Token: 0x04001A50 RID: 6736
			private DataRowAction eventAction;
		}

		// Token: 0x02000566 RID: 1382
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class GroupMembershipsRowChangeEvent : EventArgs
		{
			// Token: 0x06008386 RID: 33670 RVA: 0x0019C271 File Offset: 0x0019A471
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public GroupMembershipsRowChangeEvent(ResourceAuthorizationDataSet.GroupMembershipsRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x170027B7 RID: 10167
			// (get) Token: 0x06008387 RID: 33671 RVA: 0x0019C287 File Offset: 0x0019A487
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public ResourceAuthorizationDataSet.GroupMembershipsRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x170027B8 RID: 10168
			// (get) Token: 0x06008388 RID: 33672 RVA: 0x0019C28F File Offset: 0x0019A48F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001A51 RID: 6737
			private ResourceAuthorizationDataSet.GroupMembershipsRow eventRow;

			// Token: 0x04001A52 RID: 6738
			private DataRowAction eventAction;
		}
	}
}
