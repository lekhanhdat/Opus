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
	// Token: 0x0200063A RID: 1594
	[XmlRoot("SiteMapDataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[Serializable]
	public class SiteMapDataSet : DataSet
	{
		// Token: 0x0600937E RID: 37758 RVA: 0x001CDF5C File Offset: 0x001CC15C
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SiteMapConfig, new string[]
			{
				"WADMIN_SITEMAP_CACHE_VERSION",
				"WADMIN_ALWAYS_EXPAND_NAV_LINKS"
			});
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.SiteMap, new string[]
			{
				"SM_TITLE",
				"SM_URL",
				"SM_CUSTOM_TITLE",
				"SM_CUSTOM_URL",
				"SM_UID",
				"SM_HELP_ID",
				"SM_DEFAULT",
				"SM_TYPE",
				"SM_ORDER",
				"SM_HIDDEN",
				"SM_PARENT_UID"
			});
		}

		// Token: 0x0600937F RID: 37759 RVA: 0x001CE000 File Offset: 0x001CC200
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public SiteMapDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x06009380 RID: 37760 RVA: 0x001CE054 File Offset: 0x001CC254
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected SiteMapDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["SiteMap"] != null)
				{
					base.Tables.Add(new SiteMapDataSet.SiteMapDataTable(dataSet.Tables["SiteMap"]));
				}
				if (dataSet.Tables["SiteMapConfig"] != null)
				{
					base.Tables.Add(new SiteMapDataSet.SiteMapConfigDataTable(dataSet.Tables["SiteMapConfig"]));
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

		// Token: 0x17002C34 RID: 11316
		// (get) Token: 0x06009381 RID: 37761 RVA: 0x001CE1E3 File Offset: 0x001CC3E3
		[DebuggerNonUserCode]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public SiteMapDataSet.SiteMapDataTable SiteMap
		{
			get
			{
				return this.tableSiteMap;
			}
		}

		// Token: 0x17002C35 RID: 11317
		// (get) Token: 0x06009382 RID: 37762 RVA: 0x001CE1EB File Offset: 0x001CC3EB
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[Browsable(false)]
		public SiteMapDataSet.SiteMapConfigDataTable SiteMapConfig
		{
			get
			{
				return this.tableSiteMapConfig;
			}
		}

		// Token: 0x17002C36 RID: 11318
		// (get) Token: 0x06009383 RID: 37763 RVA: 0x001CE1F3 File Offset: 0x001CC3F3
		// (set) Token: 0x06009384 RID: 37764 RVA: 0x001CE1FB File Offset: 0x001CC3FB
		[DebuggerNonUserCode]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

		// Token: 0x17002C37 RID: 11319
		// (get) Token: 0x06009385 RID: 37765 RVA: 0x001CE204 File Offset: 0x001CC404
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

		// Token: 0x17002C38 RID: 11320
		// (get) Token: 0x06009386 RID: 37766 RVA: 0x001CE20C File Offset: 0x001CC40C
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

		// Token: 0x06009387 RID: 37767 RVA: 0x001CE214 File Offset: 0x001CC414
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x06009388 RID: 37768 RVA: 0x001CE228 File Offset: 0x001CC428
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			SiteMapDataSet siteMapDataSet = (SiteMapDataSet)base.Clone();
			siteMapDataSet.InitVars();
			siteMapDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return siteMapDataSet;
		}

		// Token: 0x06009389 RID: 37769 RVA: 0x001CE254 File Offset: 0x001CC454
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600938A RID: 37770 RVA: 0x001CE257 File Offset: 0x001CC457
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600938B RID: 37771 RVA: 0x001CE25C File Offset: 0x001CC45C
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["SiteMap"] != null)
				{
					base.Tables.Add(new SiteMapDataSet.SiteMapDataTable(dataSet.Tables["SiteMap"]));
				}
				if (dataSet.Tables["SiteMapConfig"] != null)
				{
					base.Tables.Add(new SiteMapDataSet.SiteMapConfigDataTable(dataSet.Tables["SiteMapConfig"]));
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

		// Token: 0x0600938C RID: 37772 RVA: 0x001CE354 File Offset: 0x001CC554
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600938D RID: 37773 RVA: 0x001CE388 File Offset: 0x001CC588
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600938E RID: 37774 RVA: 0x001CE394 File Offset: 0x001CC594
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		internal void InitVars(bool initTable)
		{
			this.tableSiteMap = (SiteMapDataSet.SiteMapDataTable)base.Tables["SiteMap"];
			if (initTable && this.tableSiteMap != null)
			{
				this.tableSiteMap.InitVars();
			}
			this.tableSiteMapConfig = (SiteMapDataSet.SiteMapConfigDataTable)base.Tables["SiteMapConfig"];
			if (initTable && this.tableSiteMapConfig != null)
			{
				this.tableSiteMapConfig.InitVars();
			}
		}

		// Token: 0x0600938F RID: 37775 RVA: 0x001CE404 File Offset: 0x001CC604
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private void InitClass()
		{
			base.DataSetName = "SiteMapDataSet";
			base.Prefix = "";
			base.Namespace = "http://microsoft.office.project.server/SiteMapDataSet/";
			base.EnforceConstraints = true;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableSiteMap = new SiteMapDataSet.SiteMapDataTable();
			base.Tables.Add(this.tableSiteMap);
			this.tableSiteMapConfig = new SiteMapDataSet.SiteMapConfigDataTable();
			base.Tables.Add(this.tableSiteMapConfig);
		}

		// Token: 0x06009390 RID: 37776 RVA: 0x001CE478 File Offset: 0x001CC678
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private bool ShouldSerializeSiteMap()
		{
			return false;
		}

		// Token: 0x06009391 RID: 37777 RVA: 0x001CE47B File Offset: 0x001CC67B
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		private bool ShouldSerializeSiteMapConfig()
		{
			return false;
		}

		// Token: 0x06009392 RID: 37778 RVA: 0x001CE47E File Offset: 0x001CC67E
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x06009393 RID: 37779 RVA: 0x001CE490 File Offset: 0x001CC690
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			SiteMapDataSet siteMapDataSet = new SiteMapDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = siteMapDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = siteMapDataSet.GetSchemaSerializable();
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

		// Token: 0x04001D99 RID: 7577
		private SiteMapDataSet.SiteMapDataTable tableSiteMap;

		// Token: 0x04001D9A RID: 7578
		private SiteMapDataSet.SiteMapConfigDataTable tableSiteMapConfig;

		// Token: 0x04001D9B RID: 7579
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200063B RID: 1595
		// (Invoke) Token: 0x06009395 RID: 37781
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SiteMapRowChangeEventHandler(object sender, SiteMapDataSet.SiteMapRowChangeEvent e);

		// Token: 0x0200063C RID: 1596
		// (Invoke) Token: 0x06009399 RID: 37785
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void SiteMapConfigRowChangeEventHandler(object sender, SiteMapDataSet.SiteMapConfigRowChangeEvent e);

		// Token: 0x0200063D RID: 1597
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SiteMapDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600939C RID: 37788 RVA: 0x001CE5D8 File Offset: 0x001CC7D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataTable()
			{
				base.TableName = "SiteMap";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600939D RID: 37789 RVA: 0x001CE600 File Offset: 0x001CC800
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SiteMapDataTable(DataTable table)
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

			// Token: 0x0600939E RID: 37790 RVA: 0x001CE6A8 File Offset: 0x001CC8A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected SiteMapDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002C39 RID: 11321
			// (get) Token: 0x0600939F RID: 37791 RVA: 0x001CE6B8 File Offset: 0x001CC8B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_UIDColumn
			{
				get
				{
					return this.columnSM_UID;
				}
			}

			// Token: 0x17002C3A RID: 11322
			// (get) Token: 0x060093A0 RID: 37792 RVA: 0x001CE6C0 File Offset: 0x001CC8C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SM_PARENT_UIDColumn
			{
				get
				{
					return this.columnSM_PARENT_UID;
				}
			}

			// Token: 0x17002C3B RID: 11323
			// (get) Token: 0x060093A1 RID: 37793 RVA: 0x001CE6C8 File Offset: 0x001CC8C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_TITLEColumn
			{
				get
				{
					return this.columnSM_TITLE;
				}
			}

			// Token: 0x17002C3C RID: 11324
			// (get) Token: 0x060093A2 RID: 37794 RVA: 0x001CE6D0 File Offset: 0x001CC8D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_URLColumn
			{
				get
				{
					return this.columnSM_URL;
				}
			}

			// Token: 0x17002C3D RID: 11325
			// (get) Token: 0x060093A3 RID: 37795 RVA: 0x001CE6D8 File Offset: 0x001CC8D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SM_TYPEColumn
			{
				get
				{
					return this.columnSM_TYPE;
				}
			}

			// Token: 0x17002C3E RID: 11326
			// (get) Token: 0x060093A4 RID: 37796 RVA: 0x001CE6E0 File Offset: 0x001CC8E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_ORDERColumn
			{
				get
				{
					return this.columnSM_ORDER;
				}
			}

			// Token: 0x17002C3F RID: 11327
			// (get) Token: 0x060093A5 RID: 37797 RVA: 0x001CE6E8 File Offset: 0x001CC8E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_DEFAULTColumn
			{
				get
				{
					return this.columnSM_DEFAULT;
				}
			}

			// Token: 0x17002C40 RID: 11328
			// (get) Token: 0x060093A6 RID: 37798 RVA: 0x001CE6F0 File Offset: 0x001CC8F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn SM_CUSTOM_URLColumn
			{
				get
				{
					return this.columnSM_CUSTOM_URL;
				}
			}

			// Token: 0x17002C41 RID: 11329
			// (get) Token: 0x060093A7 RID: 37799 RVA: 0x001CE6F8 File Offset: 0x001CC8F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_CUSTOM_TITLEColumn
			{
				get
				{
					return this.columnSM_CUSTOM_TITLE;
				}
			}

			// Token: 0x17002C42 RID: 11330
			// (get) Token: 0x060093A8 RID: 37800 RVA: 0x001CE700 File Offset: 0x001CC900
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_HELP_IDColumn
			{
				get
				{
					return this.columnSM_HELP_ID;
				}
			}

			// Token: 0x17002C43 RID: 11331
			// (get) Token: 0x060093A9 RID: 37801 RVA: 0x001CE708 File Offset: 0x001CC908
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn SM_HIDDENColumn
			{
				get
				{
					return this.columnSM_HIDDEN;
				}
			}

			// Token: 0x17002C44 RID: 11332
			// (get) Token: 0x060093AA RID: 37802 RVA: 0x001CE710 File Offset: 0x001CC910
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

			// Token: 0x17002C45 RID: 11333
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapDataSet.SiteMapRow this[int index]
			{
				get
				{
					return (SiteMapDataSet.SiteMapRow)base.Rows[index];
				}
			}

			// Token: 0x14000565 RID: 1381
			// (add) Token: 0x060093AC RID: 37804 RVA: 0x001CE730 File Offset: 0x001CC930
			// (remove) Token: 0x060093AD RID: 37805 RVA: 0x001CE768 File Offset: 0x001CC968
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapRowChangeEventHandler SiteMapRowChanging;

			// Token: 0x14000566 RID: 1382
			// (add) Token: 0x060093AE RID: 37806 RVA: 0x001CE7A0 File Offset: 0x001CC9A0
			// (remove) Token: 0x060093AF RID: 37807 RVA: 0x001CE7D8 File Offset: 0x001CC9D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapRowChangeEventHandler SiteMapRowChanged;

			// Token: 0x14000567 RID: 1383
			// (add) Token: 0x060093B0 RID: 37808 RVA: 0x001CE810 File Offset: 0x001CCA10
			// (remove) Token: 0x060093B1 RID: 37809 RVA: 0x001CE848 File Offset: 0x001CCA48
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapRowChangeEventHandler SiteMapRowDeleting;

			// Token: 0x14000568 RID: 1384
			// (add) Token: 0x060093B2 RID: 37810 RVA: 0x001CE880 File Offset: 0x001CCA80
			// (remove) Token: 0x060093B3 RID: 37811 RVA: 0x001CE8B8 File Offset: 0x001CCAB8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapRowChangeEventHandler SiteMapRowDeleted;

			// Token: 0x060093B4 RID: 37812 RVA: 0x001CE8ED File Offset: 0x001CCAED
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSiteMapRow(SiteMapDataSet.SiteMapRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060093B5 RID: 37813 RVA: 0x001CE8FC File Offset: 0x001CCAFC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataSet.SiteMapRow AddSiteMapRow(Guid SM_UID, Guid SM_PARENT_UID, string SM_TITLE, string SM_URL, byte SM_TYPE, int SM_ORDER, bool SM_DEFAULT, string SM_CUSTOM_URL, string SM_CUSTOM_TITLE, string SM_HELP_ID, bool SM_HIDDEN)
			{
				SiteMapDataSet.SiteMapRow siteMapRow = (SiteMapDataSet.SiteMapRow)base.NewRow();
				object[] itemArray = new object[]
				{
					SM_UID,
					SM_PARENT_UID,
					SM_TITLE,
					SM_URL,
					SM_TYPE,
					SM_ORDER,
					SM_DEFAULT,
					SM_CUSTOM_URL,
					SM_CUSTOM_TITLE,
					SM_HELP_ID,
					SM_HIDDEN
				};
				siteMapRow.ItemArray = itemArray;
				base.Rows.Add(siteMapRow);
				return siteMapRow;
			}

			// Token: 0x060093B6 RID: 37814 RVA: 0x001CE988 File Offset: 0x001CCB88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataSet.SiteMapRow FindBySM_UIDSM_TYPE(Guid SM_UID, byte SM_TYPE)
			{
				return (SiteMapDataSet.SiteMapRow)base.Rows.Find(new object[]
				{
					SM_UID,
					SM_TYPE
				});
			}

			// Token: 0x060093B7 RID: 37815 RVA: 0x001CE9BF File Offset: 0x001CCBBF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060093B8 RID: 37816 RVA: 0x001CE9CC File Offset: 0x001CCBCC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SiteMapDataSet.SiteMapDataTable siteMapDataTable = (SiteMapDataSet.SiteMapDataTable)base.Clone();
				siteMapDataTable.InitVars();
				return siteMapDataTable;
			}

			// Token: 0x060093B9 RID: 37817 RVA: 0x001CE9EC File Offset: 0x001CCBEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override DataTable CreateInstance()
			{
				return new SiteMapDataSet.SiteMapDataTable();
			}

			// Token: 0x060093BA RID: 37818 RVA: 0x001CE9F4 File Offset: 0x001CCBF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal void InitVars()
			{
				this.columnSM_UID = base.Columns["SM_UID"];
				this.columnSM_PARENT_UID = base.Columns["SM_PARENT_UID"];
				this.columnSM_TITLE = base.Columns["SM_TITLE"];
				this.columnSM_URL = base.Columns["SM_URL"];
				this.columnSM_TYPE = base.Columns["SM_TYPE"];
				this.columnSM_ORDER = base.Columns["SM_ORDER"];
				this.columnSM_DEFAULT = base.Columns["SM_DEFAULT"];
				this.columnSM_CUSTOM_URL = base.Columns["SM_CUSTOM_URL"];
				this.columnSM_CUSTOM_TITLE = base.Columns["SM_CUSTOM_TITLE"];
				this.columnSM_HELP_ID = base.Columns["SM_HELP_ID"];
				this.columnSM_HIDDEN = base.Columns["SM_HIDDEN"];
			}

			// Token: 0x060093BB RID: 37819 RVA: 0x001CEAF4 File Offset: 0x001CCCF4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnSM_UID = new DataColumn("SM_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSM_UID);
				this.columnSM_PARENT_UID = new DataColumn("SM_PARENT_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnSM_PARENT_UID);
				this.columnSM_TITLE = new DataColumn("SM_TITLE", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSM_TITLE);
				this.columnSM_URL = new DataColumn("SM_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSM_URL);
				this.columnSM_TYPE = new DataColumn("SM_TYPE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnSM_TYPE);
				this.columnSM_ORDER = new DataColumn("SM_ORDER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnSM_ORDER);
				this.columnSM_DEFAULT = new DataColumn("SM_DEFAULT", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnSM_DEFAULT);
				this.columnSM_CUSTOM_URL = new DataColumn("SM_CUSTOM_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSM_CUSTOM_URL);
				this.columnSM_CUSTOM_TITLE = new DataColumn("SM_CUSTOM_TITLE", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSM_CUSTOM_TITLE);
				this.columnSM_HELP_ID = new DataColumn("SM_HELP_ID", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnSM_HELP_ID);
				this.columnSM_HIDDEN = new DataColumn("SM_HIDDEN", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnSM_HIDDEN);
				base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[]
				{
					this.columnSM_UID,
					this.columnSM_TYPE
				}, true));
				this.columnSM_UID.AllowDBNull = false;
				this.columnSM_PARENT_UID.AllowDBNull = false;
				this.columnSM_TYPE.AllowDBNull = false;
				this.columnSM_ORDER.AllowDBNull = false;
				this.columnSM_DEFAULT.AllowDBNull = false;
			}

			// Token: 0x060093BC RID: 37820 RVA: 0x001CED5C File Offset: 0x001CCF5C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapDataSet.SiteMapRow NewSiteMapRow()
			{
				return (SiteMapDataSet.SiteMapRow)base.NewRow();
			}

			// Token: 0x060093BD RID: 37821 RVA: 0x001CED69 File Offset: 0x001CCF69
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SiteMapDataSet.SiteMapRow(builder);
			}

			// Token: 0x060093BE RID: 37822 RVA: 0x001CED71 File Offset: 0x001CCF71
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SiteMapDataSet.SiteMapRow);
			}

			// Token: 0x060093BF RID: 37823 RVA: 0x001CED7D File Offset: 0x001CCF7D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SiteMapRowChanged != null)
				{
					this.SiteMapRowChanged(this, new SiteMapDataSet.SiteMapRowChangeEvent((SiteMapDataSet.SiteMapRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093C0 RID: 37824 RVA: 0x001CEDB0 File Offset: 0x001CCFB0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SiteMapRowChanging != null)
				{
					this.SiteMapRowChanging(this, new SiteMapDataSet.SiteMapRowChangeEvent((SiteMapDataSet.SiteMapRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093C1 RID: 37825 RVA: 0x001CEDE3 File Offset: 0x001CCFE3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SiteMapRowDeleted != null)
				{
					this.SiteMapRowDeleted(this, new SiteMapDataSet.SiteMapRowChangeEvent((SiteMapDataSet.SiteMapRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093C2 RID: 37826 RVA: 0x001CEE16 File Offset: 0x001CD016
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SiteMapRowDeleting != null)
				{
					this.SiteMapRowDeleting(this, new SiteMapDataSet.SiteMapRowChangeEvent((SiteMapDataSet.SiteMapRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093C3 RID: 37827 RVA: 0x001CEE49 File Offset: 0x001CD049
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSiteMapRow(SiteMapDataSet.SiteMapRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060093C4 RID: 37828 RVA: 0x001CEE58 File Offset: 0x001CD058
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SiteMapDataSet siteMapDataSet = new SiteMapDataSet();
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
				xmlSchemaAttribute.FixedValue = siteMapDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SiteMapDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = siteMapDataSet.GetSchemaSerializable();
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

			// Token: 0x04001D9C RID: 7580
			private DataColumn columnSM_UID;

			// Token: 0x04001D9D RID: 7581
			private DataColumn columnSM_PARENT_UID;

			// Token: 0x04001D9E RID: 7582
			private DataColumn columnSM_TITLE;

			// Token: 0x04001D9F RID: 7583
			private DataColumn columnSM_URL;

			// Token: 0x04001DA0 RID: 7584
			private DataColumn columnSM_TYPE;

			// Token: 0x04001DA1 RID: 7585
			private DataColumn columnSM_ORDER;

			// Token: 0x04001DA2 RID: 7586
			private DataColumn columnSM_DEFAULT;

			// Token: 0x04001DA3 RID: 7587
			private DataColumn columnSM_CUSTOM_URL;

			// Token: 0x04001DA4 RID: 7588
			private DataColumn columnSM_CUSTOM_TITLE;

			// Token: 0x04001DA5 RID: 7589
			private DataColumn columnSM_HELP_ID;

			// Token: 0x04001DA6 RID: 7590
			private DataColumn columnSM_HIDDEN;
		}

		// Token: 0x0200063E RID: 1598
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class SiteMapConfigDataTable : DataTable, IEnumerable
		{
			// Token: 0x060093C5 RID: 37829 RVA: 0x001CF050 File Offset: 0x001CD250
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapConfigDataTable()
			{
				base.TableName = "SiteMapConfig";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x060093C6 RID: 37830 RVA: 0x001CF078 File Offset: 0x001CD278
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SiteMapConfigDataTable(DataTable table)
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

			// Token: 0x060093C7 RID: 37831 RVA: 0x001CF120 File Offset: 0x001CD320
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected SiteMapConfigDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x17002C46 RID: 11334
			// (get) Token: 0x060093C8 RID: 37832 RVA: 0x001CF130 File Offset: 0x001CD330
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_SITEMAP_CACHE_VERSIONColumn
			{
				get
				{
					return this.columnWADMIN_SITEMAP_CACHE_VERSION;
				}
			}

			// Token: 0x17002C47 RID: 11335
			// (get) Token: 0x060093C9 RID: 37833 RVA: 0x001CF138 File Offset: 0x001CD338
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn
			{
				get
				{
					return this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS;
				}
			}

			// Token: 0x17002C48 RID: 11336
			// (get) Token: 0x060093CA RID: 37834 RVA: 0x001CF140 File Offset: 0x001CD340
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

			// Token: 0x17002C49 RID: 11337
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapDataSet.SiteMapConfigRow this[int index]
			{
				get
				{
					return (SiteMapDataSet.SiteMapConfigRow)base.Rows[index];
				}
			}

			// Token: 0x14000569 RID: 1385
			// (add) Token: 0x060093CC RID: 37836 RVA: 0x001CF160 File Offset: 0x001CD360
			// (remove) Token: 0x060093CD RID: 37837 RVA: 0x001CF198 File Offset: 0x001CD398
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapConfigRowChangeEventHandler SiteMapConfigRowChanging;

			// Token: 0x1400056A RID: 1386
			// (add) Token: 0x060093CE RID: 37838 RVA: 0x001CF1D0 File Offset: 0x001CD3D0
			// (remove) Token: 0x060093CF RID: 37839 RVA: 0x001CF208 File Offset: 0x001CD408
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapConfigRowChangeEventHandler SiteMapConfigRowChanged;

			// Token: 0x1400056B RID: 1387
			// (add) Token: 0x060093D0 RID: 37840 RVA: 0x001CF240 File Offset: 0x001CD440
			// (remove) Token: 0x060093D1 RID: 37841 RVA: 0x001CF278 File Offset: 0x001CD478
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapConfigRowChangeEventHandler SiteMapConfigRowDeleting;

			// Token: 0x1400056C RID: 1388
			// (add) Token: 0x060093D2 RID: 37842 RVA: 0x001CF2B0 File Offset: 0x001CD4B0
			// (remove) Token: 0x060093D3 RID: 37843 RVA: 0x001CF2E8 File Offset: 0x001CD4E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event SiteMapDataSet.SiteMapConfigRowChangeEventHandler SiteMapConfigRowDeleted;

			// Token: 0x060093D4 RID: 37844 RVA: 0x001CF31D File Offset: 0x001CD51D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void AddSiteMapConfigRow(SiteMapDataSet.SiteMapConfigRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x060093D5 RID: 37845 RVA: 0x001CF32C File Offset: 0x001CD52C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataSet.SiteMapConfigRow AddSiteMapConfigRow(Guid WADMIN_SITEMAP_CACHE_VERSION, bool WADMIN_ALWAYS_EXPAND_NAV_LINKS)
			{
				SiteMapDataSet.SiteMapConfigRow siteMapConfigRow = (SiteMapDataSet.SiteMapConfigRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WADMIN_SITEMAP_CACHE_VERSION,
					WADMIN_ALWAYS_EXPAND_NAV_LINKS
				};
				siteMapConfigRow.ItemArray = itemArray;
				base.Rows.Add(siteMapConfigRow);
				return siteMapConfigRow;
			}

			// Token: 0x060093D6 RID: 37846 RVA: 0x001CF374 File Offset: 0x001CD574
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x060093D7 RID: 37847 RVA: 0x001CF384 File Offset: 0x001CD584
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				SiteMapDataSet.SiteMapConfigDataTable siteMapConfigDataTable = (SiteMapDataSet.SiteMapConfigDataTable)base.Clone();
				siteMapConfigDataTable.InitVars();
				return siteMapConfigDataTable;
			}

			// Token: 0x060093D8 RID: 37848 RVA: 0x001CF3A4 File Offset: 0x001CD5A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new SiteMapDataSet.SiteMapConfigDataTable();
			}

			// Token: 0x060093D9 RID: 37849 RVA: 0x001CF3AB File Offset: 0x001CD5AB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWADMIN_SITEMAP_CACHE_VERSION = base.Columns["WADMIN_SITEMAP_CACHE_VERSION"];
				this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS = base.Columns["WADMIN_ALWAYS_EXPAND_NAV_LINKS"];
			}

			// Token: 0x060093DA RID: 37850 RVA: 0x001CF3DC File Offset: 0x001CD5DC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			private void InitClass()
			{
				this.columnWADMIN_SITEMAP_CACHE_VERSION = new DataColumn("WADMIN_SITEMAP_CACHE_VERSION", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SITEMAP_CACHE_VERSION);
				this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS = new DataColumn("WADMIN_ALWAYS_EXPAND_NAV_LINKS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS);
			}

			// Token: 0x060093DB RID: 37851 RVA: 0x001CF443 File Offset: 0x001CD643
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataSet.SiteMapConfigRow NewSiteMapConfigRow()
			{
				return (SiteMapDataSet.SiteMapConfigRow)base.NewRow();
			}

			// Token: 0x060093DC RID: 37852 RVA: 0x001CF450 File Offset: 0x001CD650
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new SiteMapDataSet.SiteMapConfigRow(builder);
			}

			// Token: 0x060093DD RID: 37853 RVA: 0x001CF458 File Offset: 0x001CD658
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(SiteMapDataSet.SiteMapConfigRow);
			}

			// Token: 0x060093DE RID: 37854 RVA: 0x001CF464 File Offset: 0x001CD664
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.SiteMapConfigRowChanged != null)
				{
					this.SiteMapConfigRowChanged(this, new SiteMapDataSet.SiteMapConfigRowChangeEvent((SiteMapDataSet.SiteMapConfigRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093DF RID: 37855 RVA: 0x001CF497 File Offset: 0x001CD697
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.SiteMapConfigRowChanging != null)
				{
					this.SiteMapConfigRowChanging(this, new SiteMapDataSet.SiteMapConfigRowChangeEvent((SiteMapDataSet.SiteMapConfigRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093E0 RID: 37856 RVA: 0x001CF4CA File Offset: 0x001CD6CA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.SiteMapConfigRowDeleted != null)
				{
					this.SiteMapConfigRowDeleted(this, new SiteMapDataSet.SiteMapConfigRowChangeEvent((SiteMapDataSet.SiteMapConfigRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093E1 RID: 37857 RVA: 0x001CF4FD File Offset: 0x001CD6FD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.SiteMapConfigRowDeleting != null)
				{
					this.SiteMapConfigRowDeleting(this, new SiteMapDataSet.SiteMapConfigRowChangeEvent((SiteMapDataSet.SiteMapConfigRow)e.Row, e.Action));
				}
			}

			// Token: 0x060093E2 RID: 37858 RVA: 0x001CF530 File Offset: 0x001CD730
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveSiteMapConfigRow(SiteMapDataSet.SiteMapConfigRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x060093E3 RID: 37859 RVA: 0x001CF540 File Offset: 0x001CD740
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				SiteMapDataSet siteMapDataSet = new SiteMapDataSet();
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
				xmlSchemaAttribute.FixedValue = siteMapDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "SiteMapConfigDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = siteMapDataSet.GetSchemaSerializable();
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

			// Token: 0x04001DAB RID: 7595
			private DataColumn columnWADMIN_SITEMAP_CACHE_VERSION;

			// Token: 0x04001DAC RID: 7596
			private DataColumn columnWADMIN_ALWAYS_EXPAND_NAV_LINKS;
		}

		// Token: 0x0200063F RID: 1599
		public class SiteMapRow : DataRow
		{
			// Token: 0x060093E4 RID: 37860 RVA: 0x001CF738 File Offset: 0x001CD938
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal SiteMapRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSiteMap = (SiteMapDataSet.SiteMapDataTable)base.Table;
			}

			// Token: 0x17002C4A RID: 11338
			// (get) Token: 0x060093E5 RID: 37861 RVA: 0x001CF752 File Offset: 0x001CD952
			// (set) Token: 0x060093E6 RID: 37862 RVA: 0x001CF76A File Offset: 0x001CD96A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SM_UID
			{
				get
				{
					return (Guid)base[this.tableSiteMap.SM_UIDColumn];
				}
				set
				{
					base[this.tableSiteMap.SM_UIDColumn] = value;
				}
			}

			// Token: 0x17002C4B RID: 11339
			// (get) Token: 0x060093E7 RID: 37863 RVA: 0x001CF783 File Offset: 0x001CD983
			// (set) Token: 0x060093E8 RID: 37864 RVA: 0x001CF79B File Offset: 0x001CD99B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid SM_PARENT_UID
			{
				get
				{
					return (Guid)base[this.tableSiteMap.SM_PARENT_UIDColumn];
				}
				set
				{
					base[this.tableSiteMap.SM_PARENT_UIDColumn] = value;
				}
			}

			// Token: 0x17002C4C RID: 11340
			// (get) Token: 0x060093E9 RID: 37865 RVA: 0x001CF7B4 File Offset: 0x001CD9B4
			// (set) Token: 0x060093EA RID: 37866 RVA: 0x001CF7F8 File Offset: 0x001CD9F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SM_TITLE
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSiteMap.SM_TITLEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_TITLE' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_TITLEColumn] = value;
				}
			}

			// Token: 0x17002C4D RID: 11341
			// (get) Token: 0x060093EB RID: 37867 RVA: 0x001CF80C File Offset: 0x001CDA0C
			// (set) Token: 0x060093EC RID: 37868 RVA: 0x001CF850 File Offset: 0x001CDA50
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SM_URL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSiteMap.SM_URLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_URL' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_URLColumn] = value;
				}
			}

			// Token: 0x17002C4E RID: 11342
			// (get) Token: 0x060093ED RID: 37869 RVA: 0x001CF864 File Offset: 0x001CDA64
			// (set) Token: 0x060093EE RID: 37870 RVA: 0x001CF87C File Offset: 0x001CDA7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte SM_TYPE
			{
				get
				{
					return (byte)base[this.tableSiteMap.SM_TYPEColumn];
				}
				set
				{
					base[this.tableSiteMap.SM_TYPEColumn] = value;
				}
			}

			// Token: 0x17002C4F RID: 11343
			// (get) Token: 0x060093EF RID: 37871 RVA: 0x001CF895 File Offset: 0x001CDA95
			// (set) Token: 0x060093F0 RID: 37872 RVA: 0x001CF8AD File Offset: 0x001CDAAD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int SM_ORDER
			{
				get
				{
					return (int)base[this.tableSiteMap.SM_ORDERColumn];
				}
				set
				{
					base[this.tableSiteMap.SM_ORDERColumn] = value;
				}
			}

			// Token: 0x17002C50 RID: 11344
			// (get) Token: 0x060093F1 RID: 37873 RVA: 0x001CF8C6 File Offset: 0x001CDAC6
			// (set) Token: 0x060093F2 RID: 37874 RVA: 0x001CF8DE File Offset: 0x001CDADE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool SM_DEFAULT
			{
				get
				{
					return (bool)base[this.tableSiteMap.SM_DEFAULTColumn];
				}
				set
				{
					base[this.tableSiteMap.SM_DEFAULTColumn] = value;
				}
			}

			// Token: 0x17002C51 RID: 11345
			// (get) Token: 0x060093F3 RID: 37875 RVA: 0x001CF8F8 File Offset: 0x001CDAF8
			// (set) Token: 0x060093F4 RID: 37876 RVA: 0x001CF93C File Offset: 0x001CDB3C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SM_CUSTOM_URL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSiteMap.SM_CUSTOM_URLColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_CUSTOM_URL' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_CUSTOM_URLColumn] = value;
				}
			}

			// Token: 0x17002C52 RID: 11346
			// (get) Token: 0x060093F5 RID: 37877 RVA: 0x001CF950 File Offset: 0x001CDB50
			// (set) Token: 0x060093F6 RID: 37878 RVA: 0x001CF994 File Offset: 0x001CDB94
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SM_CUSTOM_TITLE
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSiteMap.SM_CUSTOM_TITLEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_CUSTOM_TITLE' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_CUSTOM_TITLEColumn] = value;
				}
			}

			// Token: 0x17002C53 RID: 11347
			// (get) Token: 0x060093F7 RID: 37879 RVA: 0x001CF9A8 File Offset: 0x001CDBA8
			// (set) Token: 0x060093F8 RID: 37880 RVA: 0x001CF9EC File Offset: 0x001CDBEC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string SM_HELP_ID
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableSiteMap.SM_HELP_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_HELP_ID' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_HELP_IDColumn] = value;
				}
			}

			// Token: 0x17002C54 RID: 11348
			// (get) Token: 0x060093F9 RID: 37881 RVA: 0x001CFA00 File Offset: 0x001CDC00
			// (set) Token: 0x060093FA RID: 37882 RVA: 0x001CFA44 File Offset: 0x001CDC44
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool SM_HIDDEN
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableSiteMap.SM_HIDDENColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'SM_HIDDEN' in table 'SiteMap' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMap.SM_HIDDENColumn] = value;
				}
			}

			// Token: 0x060093FB RID: 37883 RVA: 0x001CFA5D File Offset: 0x001CDC5D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSM_TITLENull()
			{
				return base.IsNull(this.tableSiteMap.SM_TITLEColumn);
			}

			// Token: 0x060093FC RID: 37884 RVA: 0x001CFA70 File Offset: 0x001CDC70
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSM_TITLENull()
			{
				base[this.tableSiteMap.SM_TITLEColumn] = Convert.DBNull;
			}

			// Token: 0x060093FD RID: 37885 RVA: 0x001CFA88 File Offset: 0x001CDC88
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSM_URLNull()
			{
				return base.IsNull(this.tableSiteMap.SM_URLColumn);
			}

			// Token: 0x060093FE RID: 37886 RVA: 0x001CFA9B File Offset: 0x001CDC9B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSM_URLNull()
			{
				base[this.tableSiteMap.SM_URLColumn] = Convert.DBNull;
			}

			// Token: 0x060093FF RID: 37887 RVA: 0x001CFAB3 File Offset: 0x001CDCB3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsSM_CUSTOM_URLNull()
			{
				return base.IsNull(this.tableSiteMap.SM_CUSTOM_URLColumn);
			}

			// Token: 0x06009400 RID: 37888 RVA: 0x001CFAC6 File Offset: 0x001CDCC6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSM_CUSTOM_URLNull()
			{
				base[this.tableSiteMap.SM_CUSTOM_URLColumn] = Convert.DBNull;
			}

			// Token: 0x06009401 RID: 37889 RVA: 0x001CFADE File Offset: 0x001CDCDE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSM_CUSTOM_TITLENull()
			{
				return base.IsNull(this.tableSiteMap.SM_CUSTOM_TITLEColumn);
			}

			// Token: 0x06009402 RID: 37890 RVA: 0x001CFAF1 File Offset: 0x001CDCF1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetSM_CUSTOM_TITLENull()
			{
				base[this.tableSiteMap.SM_CUSTOM_TITLEColumn] = Convert.DBNull;
			}

			// Token: 0x06009403 RID: 37891 RVA: 0x001CFB09 File Offset: 0x001CDD09
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSM_HELP_IDNull()
			{
				return base.IsNull(this.tableSiteMap.SM_HELP_IDColumn);
			}

			// Token: 0x06009404 RID: 37892 RVA: 0x001CFB1C File Offset: 0x001CDD1C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSM_HELP_IDNull()
			{
				base[this.tableSiteMap.SM_HELP_IDColumn] = Convert.DBNull;
			}

			// Token: 0x06009405 RID: 37893 RVA: 0x001CFB34 File Offset: 0x001CDD34
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsSM_HIDDENNull()
			{
				return base.IsNull(this.tableSiteMap.SM_HIDDENColumn);
			}

			// Token: 0x06009406 RID: 37894 RVA: 0x001CFB47 File Offset: 0x001CDD47
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetSM_HIDDENNull()
			{
				base[this.tableSiteMap.SM_HIDDENColumn] = Convert.DBNull;
			}

			// Token: 0x04001DB1 RID: 7601
			private SiteMapDataSet.SiteMapDataTable tableSiteMap;
		}

		// Token: 0x02000640 RID: 1600
		public class SiteMapConfigRow : DataRow
		{
			// Token: 0x06009407 RID: 37895 RVA: 0x001CFB5F File Offset: 0x001CDD5F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal SiteMapConfigRow(DataRowBuilder rb) : base(rb)
			{
				this.tableSiteMapConfig = (SiteMapDataSet.SiteMapConfigDataTable)base.Table;
			}

			// Token: 0x17002C55 RID: 11349
			// (get) Token: 0x06009408 RID: 37896 RVA: 0x001CFB7C File Offset: 0x001CDD7C
			// (set) Token: 0x06009409 RID: 37897 RVA: 0x001CFBC0 File Offset: 0x001CDDC0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WADMIN_SITEMAP_CACHE_VERSION
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableSiteMapConfig.WADMIN_SITEMAP_CACHE_VERSIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SITEMAP_CACHE_VERSION' in table 'SiteMapConfig' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMapConfig.WADMIN_SITEMAP_CACHE_VERSIONColumn] = value;
				}
			}

			// Token: 0x17002C56 RID: 11350
			// (get) Token: 0x0600940A RID: 37898 RVA: 0x001CFBDC File Offset: 0x001CDDDC
			// (set) Token: 0x0600940B RID: 37899 RVA: 0x001CFC20 File Offset: 0x001CDE20
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_ALWAYS_EXPAND_NAV_LINKS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableSiteMapConfig.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_ALWAYS_EXPAND_NAV_LINKS' in table 'SiteMapConfig' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableSiteMapConfig.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn] = value;
				}
			}

			// Token: 0x0600940C RID: 37900 RVA: 0x001CFC39 File Offset: 0x001CDE39
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SITEMAP_CACHE_VERSIONNull()
			{
				return base.IsNull(this.tableSiteMapConfig.WADMIN_SITEMAP_CACHE_VERSIONColumn);
			}

			// Token: 0x0600940D RID: 37901 RVA: 0x001CFC4C File Offset: 0x001CDE4C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_SITEMAP_CACHE_VERSIONNull()
			{
				base[this.tableSiteMapConfig.WADMIN_SITEMAP_CACHE_VERSIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600940E RID: 37902 RVA: 0x001CFC64 File Offset: 0x001CDE64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_ALWAYS_EXPAND_NAV_LINKSNull()
			{
				return base.IsNull(this.tableSiteMapConfig.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn);
			}

			// Token: 0x0600940F RID: 37903 RVA: 0x001CFC77 File Offset: 0x001CDE77
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_ALWAYS_EXPAND_NAV_LINKSNull()
			{
				base[this.tableSiteMapConfig.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn] = Convert.DBNull;
			}

			// Token: 0x04001DB2 RID: 7602
			private SiteMapDataSet.SiteMapConfigDataTable tableSiteMapConfig;
		}

		// Token: 0x02000641 RID: 1601
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SiteMapRowChangeEvent : EventArgs
		{
			// Token: 0x06009410 RID: 37904 RVA: 0x001CFC8F File Offset: 0x001CDE8F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapRowChangeEvent(SiteMapDataSet.SiteMapRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C57 RID: 11351
			// (get) Token: 0x06009411 RID: 37905 RVA: 0x001CFCA5 File Offset: 0x001CDEA5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public SiteMapDataSet.SiteMapRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C58 RID: 11352
			// (get) Token: 0x06009412 RID: 37906 RVA: 0x001CFCAD File Offset: 0x001CDEAD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001DB3 RID: 7603
			private SiteMapDataSet.SiteMapRow eventRow;

			// Token: 0x04001DB4 RID: 7604
			private DataRowAction eventAction;
		}

		// Token: 0x02000642 RID: 1602
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class SiteMapConfigRowChangeEvent : EventArgs
		{
			// Token: 0x06009413 RID: 37907 RVA: 0x001CFCB5 File Offset: 0x001CDEB5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapConfigRowChangeEvent(SiteMapDataSet.SiteMapConfigRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x17002C59 RID: 11353
			// (get) Token: 0x06009414 RID: 37908 RVA: 0x001CFCCB File Offset: 0x001CDECB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public SiteMapDataSet.SiteMapConfigRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x17002C5A RID: 11354
			// (get) Token: 0x06009415 RID: 37909 RVA: 0x001CFCD3 File Offset: 0x001CDED3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x04001DB5 RID: 7605
			private SiteMapDataSet.SiteMapConfigRow eventRow;

			// Token: 0x04001DB6 RID: 7606
			private DataRowAction eventAction;
		}
	}
}
