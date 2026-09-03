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
	// Token: 0x0200077D RID: 1917
	[HelpKeyword("vs.data.DataSet")]
	[ToolboxItem(true)]
	[XmlRoot("WebAdminDataSet")]
	[DesignerCategory("code")]
	[XmlSchemaProvider("GetTypedDataSetSchema")]
	[Serializable]
	public class WebAdminDataSet : DataSet
	{
		// Token: 0x0600B87A RID: 47226 RVA: 0x0023F53C File Offset: 0x0023D73C
		public new void EndInit()
		{
			base.EndInit();
			TypedDataSetUtilities.AllowNullsInNonTypedColumns(this.WebAdmin, new string[]
			{
				"WADMIN_REMINDER_TIMER_JOB_UID",
				"WADMIN_SERVER_CURRENCY",
				"WADMIN_DEFAULT_TRACKING_METHOD",
				"WADMIN_USE_PROJECT_STATE",
				"WADMIN_STAT_IMPORT_LINE_CLASSES",
				"MOD_REV_COUNTER",
				"WADMIN_STAT_MAX_HR_PER_TASK",
				"WADMIN_WSS_PWA_READER_ROLE_ID",
				"WADMIN_STAT_3PRD_1ST_END",
				"WADMIN_CURRENT_STS_SERVER_UID",
				"WADMIN_TS_MAXIMUM_LINE_ITEMS",
				"WADMIN_TS_MAX_HR_PER_DAY",
				"WADMIN_LOCK_PRO_DEFAULT_TASK_MODE",
				"WADMIN_PROJECT_BUILD",
				"WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID",
				"WADMIN_SHOW_WSS_NAV_LINKS",
				"WADMIN_INTRANET_SERVER_URL",
				"WADMIN_DEFAULT_LANGUAGE",
				"WADMIN_TS_IS_AUDIT_ENABLED",
				"WADMIN_AUTH_REQUIRED_FOR_PUBLISH",
				"WADMIN_EMAIL_CHARSET",
				"WADMIN_PROVISIONING_RESULT",
				"WADMIN_SQL_BATCHING_BUFFER_SIZE",
				"WADMIN_SMTP_SERVER_PORT",
				"WADMIN_ALWAYS_EXPAND_NAV_LINKS",
				"WADMIN_MONTHLY_3PRDS_2ND_END",
				"WADMIN_STAT_2PRD_1ST_START",
				"WADMIN_IS_TRACKING_METHOD_LOCKED",
				"WADMIN_GROUPINGGANTT_CACHE_VERSION",
				"WADMIN_IS_HOSTED_ORG",
				"WADMIN_TS_IS_FUTURE_REP_ALLOWED",
				"WADMIN_TS_CREATE_MODE_ENUM",
				"WADMIN_USE_BASELINE_SUMMARY_DATA",
				"WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS",
				"WADMIN_MONTHLY_2PRDS_1ST_START",
				"WADMIN_PROTECT_ACTUALS",
				"WADMIN_WEEK_START_ON_ENUM",
				"WADMIN_RES_SCHEDULED_TIME",
				"WADMIN_MAX_SSP_BATCH_SIZE",
				"WADMIN_STAT_LOOK_AHEAD_PERIODS",
				"WADMIN_ACTIVE_CACHE_DIR",
				"WADMIN_WSS_RESTRICT_WORKSPACE_CREATION",
				"WADMIN_DEFAULT_SITE_COLLECTION",
				"WADMIN_ENFORCE_CURRENCY",
				"WADMIN_NTFY_EMAIL_TRAILER",
				"WADMIN_TS_MAX_HR_PER_TS",
				"WADMIN_ONLY_PRO_PUBLISH",
				"WADMIN_DATABASE_CACHE_ENABLED",
				"WADMIN_MAX_HOUR_PER_DAY",
				"WADMIN_TIMEPERIOD_GRANULARITY",
				"MOD_DATE",
				"WADMIN_WSS_PWA_ADMIN_ROLE_ID",
				"WADMIN_TS_PROJECT_MANAGER_COORDINATION",
				"WADMIN_WORKFLOW_PROXY_ACCT",
				"WADMIN_TS_REPORT_UNIT_ENUM",
				"WADMIN_EXCHANGE_INTEGRATION_ENABLED",
				"WADMIN_IS_NOTIFICATION_ENABLED",
				"WADMIN_MONTHLY_3PRDS_1ST_START",
				"WADMIN_STAT_2PRD_1ST_END",
				"WADMIN_IS_DELETED",
				"WADMIN_LICENSES",
				"WADMIN_TS_FIXED_APPROVAL_ROUTING",
				"WADMIN_STAT_SPAN_MODE_ENUM",
				"WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID",
				"WADMIN_MIN_WINPROJ_BUILD_NUMBERS",
				"WADMIN_SERVER_FLAGS",
				"WADMIN_USE_ENGAGEMENTS",
				"WADMIN_SETTINGS_VERSION",
				"WADMIN_AUTHENTICATION_TYPE",
				"WADMIN_NEW_ACCOUNT_PRIVILEGE",
				"WADMIN_STAT_1PRD_1ST_START",
				"WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS",
				"WADMIN_LAST_STS_ADMIN_SYNCH_TIME",
				"WADMIN_STAT_REP_SCHED_ENUM",
				"WADMIN_ORG_EMAIL_ADDRESS",
				"TIMESHEET_CURRENT_VIEWSET_UID",
				"CREATED_DATE",
				"WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND",
				"WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID",
				"WADMIN_MIN_PASSWORD_LENGTH",
				"WADMIN_WEEK_STARTS_ON",
				"WADMIN_SQL_BATCHING_ENABLED",
				"WADMIN_STS_PRIMARY_OWNER_EMAIL",
				"WADMIN_MONTHLY_3PRDS_1ST_END",
				"WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED",
				"WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED",
				"WADMIN_MONTHLY_REPORTS_PER_MONTH",
				"WADMIN_AD_SYNC_REPLACE_CHAR",
				"WADMIN_AUTO_ADD_USER_TO_SUBWEB",
				"WADMIN_SITEMAP_CACHE_VERSION",
				"WADMIN_SPPERMMODE_LAST_SYNC",
				"WADMIN_STAT_LOOK_AHEAD",
				"WADMIN_RESOURCE_CAPACITY_JOB_UID",
				"WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID",
				"WADMIN_SMTP_SERVER_NAME",
				"WADMIN_WORKFLOW_PROXY_MOD_DATE",
				"WADMIN_AUTO_CREATE_SUBWEBS",
				"WADMIN_RESPLAN_FTE",
				"WADMIN_OVER_QUOTA",
				"WADMIN_STAT_TIMESHEET_TIED",
				"WADMIN_TS_PROJECT_MANAGER_APPROVAL",
				"WADMIN_WORKFLOW_PROXY_UID",
				"WADMIN_STAT_NUM_UPDATES_PER_MONTH",
				"WADMIN_CORE_SQL_TIMEOUT",
				"WADMIN_ENABLE_ENTERPRISE",
				"WADMIN_TS_MODE_ENUM",
				"WADMIN_STS_TEMPLATE_LCID",
				"WADMIN_TS_HOURS_PER_WEEK",
				"WADMIN_STAT_NUM_WK_SPANNED",
				"WADMIN_EXTRANET_SERVER_URL",
				"WADMIN_MONTHLY_1PRD_1ST_START",
				"WADMIN_STAT_ENABLE_DOWNLOAD",
				"WADMIN_STAT_ALLOW_FREEFORM_PERIODS",
				"WADMIN_STAT_PERIOD_TYPE",
				"WADMIN_IS_UPDATING",
				"WADMIN_ACTIVE_CACHE_MAX_SIZE_MB",
				"WADMIN_DISABLED_SYNC_THRESHOLD",
				"WADMIN_FULL_SYNC_THRESHOLD",
				"WADMIN_MAX_SQL_BATCH_SIZE",
				"WADMIN_NTFY_FROM_EMAIL",
				"WADMIN_EXCHANGE_URL_REFRESH_JOB_UID",
				"WADMIN_OFF_PEAK_SYNC_THRESHOLD",
				"WADMIN_BUILD_TEAM_BY_RBS",
				"WADMIN_DISPLAY_MASTER_IN_ENTERPRISE",
				"WADMIN_WORKFLOW_PROXY_WINDOWS",
				"WADMIN_TS_DEF_DISPLAY_ENUM",
				"WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS",
				"WADMIN_TS_DEF_ENTRY_MODE_ENUM",
				"WADMIN_SERVER_DEFAULT_TASK_MODE",
				"WADMIN_USER_SYNC_SETTING",
				"WADMIN_TIMESHEET_SPAN",
				"WADMIN_TS_ALLOW_PROJECT_LEVEL",
				"WADMIN_MONTHLY_2PRDS_1ST_END",
				"WADMIN_PUBLISH_MANUAL_TASKS",
				"WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD",
				"WADMIN_WORKFLOW_PROXY_MOD_BY",
				"WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID",
				"WADMIN_TRANS_HISTORY_DAYS",
				"WADMIN_SYNC_TASKS_TO_TASKLIST",
				"WADMIN_TS_IS_UNVERS_TASK_ALLOWED",
				"WADMIN_IS_DELEGATION_ALLOWED",
				"WADMIN_LOOKAHEAD",
				"WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS",
				"WADMIN_TS_TIED_MODE",
				"WADMIN_UIDFAKE",
				"WADMIN_TS_OUTLOOK_DISPLAY_ENUM",
				"WADMIN_PERMISSION_MODE",
				"WADMIN_TS_HOURS_PER_DAY",
				"WADMIN_STS_TEMPLATE_ID",
				"WADMIN_STAT_MAX_HR_PER_DAY",
				"WADMIN_LIST_SEPARATOR",
				"CREATED_REV_COUNTER",
				"WADMIN_LOGICAL_READONLY",
				"WADMIN_STAT_3PRD_1ST_START",
				"WADMIN_TS_MIN_HR_PER_TS",
				"WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE",
				"WADMIN_STAT_PROT_ACT",
				"WADMIN_STAT_3PRD_2ND_END"
			});
		}

		// Token: 0x0600B87B RID: 47227 RVA: 0x0023FB48 File Offset: 0x0023DD48
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public WebAdminDataSet()
		{
			base.BeginInit();
			this.InitClass();
			CollectionChangeEventHandler value = new CollectionChangeEventHandler(this.SchemaChanged);
			base.Tables.CollectionChanged += value;
			base.Relations.CollectionChanged += value;
			this.EndInit();
		}

		// Token: 0x0600B87C RID: 47228 RVA: 0x0023FB9C File Offset: 0x0023DD9C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected WebAdminDataSet(SerializationInfo info, StreamingContext context) : base(info, context, false)
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
				if (dataSet.Tables["WebAdmin"] != null)
				{
					base.Tables.Add(new WebAdminDataSet.WebAdminDataTable(dataSet.Tables["WebAdmin"]));
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

		// Token: 0x17003839 RID: 14393
		// (get) Token: 0x0600B87D RID: 47229 RVA: 0x0023FCF9 File Offset: 0x0023DEF9
		[Browsable(false)]
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WebAdminDataSet.WebAdminDataTable WebAdmin
		{
			get
			{
				return this.tableWebAdmin;
			}
		}

		// Token: 0x1700383A RID: 14394
		// (get) Token: 0x0600B87E RID: 47230 RVA: 0x0023FD01 File Offset: 0x0023DF01
		// (set) Token: 0x0600B87F RID: 47231 RVA: 0x0023FD09 File Offset: 0x0023DF09
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

		// Token: 0x1700383B RID: 14395
		// (get) Token: 0x0600B880 RID: 47232 RVA: 0x0023FD12 File Offset: 0x0023DF12
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

		// Token: 0x1700383C RID: 14396
		// (get) Token: 0x0600B881 RID: 47233 RVA: 0x0023FD1A File Offset: 0x0023DF1A
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

		// Token: 0x0600B882 RID: 47234 RVA: 0x0023FD22 File Offset: 0x0023DF22
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void InitializeDerivedDataSet()
		{
			base.BeginInit();
			this.InitClass();
			this.EndInit();
		}

		// Token: 0x0600B883 RID: 47235 RVA: 0x0023FD38 File Offset: 0x0023DF38
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public override DataSet Clone()
		{
			WebAdminDataSet webAdminDataSet = (WebAdminDataSet)base.Clone();
			webAdminDataSet.InitVars();
			webAdminDataSet.SchemaSerializationMode = this.SchemaSerializationMode;
			return webAdminDataSet;
		}

		// Token: 0x0600B884 RID: 47236 RVA: 0x0023FD64 File Offset: 0x0023DF64
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		// Token: 0x0600B885 RID: 47237 RVA: 0x0023FD67 File Offset: 0x0023DF67
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		// Token: 0x0600B886 RID: 47238 RVA: 0x0023FD6C File Offset: 0x0023DF6C
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
			{
				this.Reset();
				DataSet dataSet = new DataSet();
				dataSet.ReadXml(reader);
				if (dataSet.Tables["WebAdmin"] != null)
				{
					base.Tables.Add(new WebAdminDataSet.WebAdminDataTable(dataSet.Tables["WebAdmin"]));
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

		// Token: 0x0600B887 RID: 47239 RVA: 0x0023FE34 File Offset: 0x0023E034
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		protected override XmlSchema GetSchemaSerializable()
		{
			MemoryStream memoryStream = new MemoryStream();
			base.WriteXmlSchema(new XmlTextWriter(memoryStream, null));
			memoryStream.Position = 0L;
			return XmlSchema.Read(new XmlTextReader(memoryStream), null);
		}

		// Token: 0x0600B888 RID: 47240 RVA: 0x0023FE68 File Offset: 0x0023E068
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars()
		{
			this.InitVars(true);
		}

		// Token: 0x0600B889 RID: 47241 RVA: 0x0023FE71 File Offset: 0x0023E071
		[DebuggerNonUserCode]
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		internal void InitVars(bool initTable)
		{
			this.tableWebAdmin = (WebAdminDataSet.WebAdminDataTable)base.Tables["WebAdmin"];
			if (initTable && this.tableWebAdmin != null)
			{
				this.tableWebAdmin.InitVars();
			}
		}

		// Token: 0x0600B88A RID: 47242 RVA: 0x0023FEA4 File Offset: 0x0023E0A4
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void InitClass()
		{
			base.DataSetName = "WebAdminDataSet";
			base.Prefix = "";
			base.Namespace = "http://schemas.microsoft.com/office/project/server/webservices/WebAdminDataSet/";
			base.EnforceConstraints = false;
			this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
			this.tableWebAdmin = new WebAdminDataSet.WebAdminDataTable();
			base.Tables.Add(this.tableWebAdmin);
		}

		// Token: 0x0600B88C RID: 47244 RVA: 0x0023FEFF File Offset: 0x0023E0FF
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		private void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Remove)
			{
				this.InitVars();
			}
		}

		// Token: 0x0600B88D RID: 47245 RVA: 0x0023FF10 File Offset: 0x0023E110
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
		{
			WebAdminDataSet webAdminDataSet = new WebAdminDataSet();
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = webAdminDataSet.Namespace;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = webAdminDataSet.GetSchemaSerializable();
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

		// Token: 0x0400252A RID: 9514
		private WebAdminDataSet.WebAdminDataTable tableWebAdmin;

		// Token: 0x0400252B RID: 9515
		private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

		// Token: 0x0200077E RID: 1918
		// (Invoke) Token: 0x0600B88F RID: 47247
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public delegate void WebAdminRowChangeEventHandler(object sender, WebAdminDataSet.WebAdminRowChangeEvent e);

		// Token: 0x0200077F RID: 1919
		[XmlSchemaProvider("GetTypedTableSchema")]
		[Serializable]
		public class WebAdminDataTable : DataTable, IEnumerable
		{
			// Token: 0x0600B892 RID: 47250 RVA: 0x00240058 File Offset: 0x0023E258
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WebAdminDataTable()
			{
				base.TableName = "WebAdmin";
				this.BeginInit();
				this.InitClass();
				this.EndInit();
			}

			// Token: 0x0600B893 RID: 47251 RVA: 0x00240080 File Offset: 0x0023E280
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal WebAdminDataTable(DataTable table)
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

			// Token: 0x0600B894 RID: 47252 RVA: 0x00240128 File Offset: 0x0023E328
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected WebAdminDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				this.InitVars();
			}

			// Token: 0x1700383D RID: 14397
			// (get) Token: 0x0600B895 RID: 47253 RVA: 0x00240138 File Offset: 0x0023E338
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_UIDFAKEColumn
			{
				get
				{
					return this.columnWADMIN_UIDFAKE;
				}
			}

			// Token: 0x1700383E RID: 14398
			// (get) Token: 0x0600B896 RID: 47254 RVA: 0x00240140 File Offset: 0x0023E340
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_AUTHENTICATION_TYPEColumn
			{
				get
				{
					return this.columnWADMIN_AUTHENTICATION_TYPE;
				}
			}

			// Token: 0x1700383F RID: 14399
			// (get) Token: 0x0600B897 RID: 47255 RVA: 0x00240148 File Offset: 0x0023E348
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_NEW_ACCOUNT_PRIVILEGEColumn
			{
				get
				{
					return this.columnWADMIN_NEW_ACCOUNT_PRIVILEGE;
				}
			}

			// Token: 0x17003840 RID: 14400
			// (get) Token: 0x0600B898 RID: 47256 RVA: 0x00240150 File Offset: 0x0023E350
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_IS_DELEGATION_ALLOWEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_DELEGATION_ALLOWED;
				}
			}

			// Token: 0x17003841 RID: 14401
			// (get) Token: 0x0600B899 RID: 47257 RVA: 0x00240158 File Offset: 0x0023E358
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_AUTH_REQUIRED_FOR_PUBLISHColumn
			{
				get
				{
					return this.columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH;
				}
			}

			// Token: 0x17003842 RID: 14402
			// (get) Token: 0x0600B89A RID: 47258 RVA: 0x00240160 File Offset: 0x0023E360
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WEEK_STARTS_ONColumn
			{
				get
				{
					return this.columnWADMIN_WEEK_STARTS_ON;
				}
			}

			// Token: 0x17003843 RID: 14403
			// (get) Token: 0x0600B89B RID: 47259 RVA: 0x00240168 File Offset: 0x0023E368
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_MIN_PASSWORD_LENGTHColumn
			{
				get
				{
					return this.columnWADMIN_MIN_PASSWORD_LENGTH;
				}
			}

			// Token: 0x17003844 RID: 14404
			// (get) Token: 0x0600B89C RID: 47260 RVA: 0x00240170 File Offset: 0x0023E370
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_NTFY_FROM_EMAILColumn
			{
				get
				{
					return this.columnWADMIN_NTFY_FROM_EMAIL;
				}
			}

			// Token: 0x17003845 RID: 14405
			// (get) Token: 0x0600B89D RID: 47261 RVA: 0x00240178 File Offset: 0x0023E378
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_NTFY_EMAIL_TRAILERColumn
			{
				get
				{
					return this.columnWADMIN_NTFY_EMAIL_TRAILER;
				}
			}

			// Token: 0x17003846 RID: 14406
			// (get) Token: 0x0600B89E RID: 47262 RVA: 0x00240180 File Offset: 0x0023E380
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ORG_EMAIL_ADDRESSColumn
			{
				get
				{
					return this.columnWADMIN_ORG_EMAIL_ADDRESS;
				}
			}

			// Token: 0x17003847 RID: 14407
			// (get) Token: 0x0600B89F RID: 47263 RVA: 0x00240188 File Offset: 0x0023E388
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_EMAIL_CHARSETColumn
			{
				get
				{
					return this.columnWADMIN_EMAIL_CHARSET;
				}
			}

			// Token: 0x17003848 RID: 14408
			// (get) Token: 0x0600B8A0 RID: 47264 RVA: 0x00240190 File Offset: 0x0023E390
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_DEFAULT_LANGUAGEColumn
			{
				get
				{
					return this.columnWADMIN_DEFAULT_LANGUAGE;
				}
			}

			// Token: 0x17003849 RID: 14409
			// (get) Token: 0x0600B8A1 RID: 47265 RVA: 0x00240198 File Offset: 0x0023E398
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_DEFAULT_TRACKING_METHODColumn
			{
				get
				{
					return this.columnWADMIN_DEFAULT_TRACKING_METHOD;
				}
			}

			// Token: 0x1700384A RID: 14410
			// (get) Token: 0x0600B8A2 RID: 47266 RVA: 0x002401A0 File Offset: 0x0023E3A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTSColumn
			{
				get
				{
					return this.columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS;
				}
			}

			// Token: 0x1700384B RID: 14411
			// (get) Token: 0x0600B8A3 RID: 47267 RVA: 0x002401A8 File Offset: 0x0023E3A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_IS_TRACKING_METHOD_LOCKEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_TRACKING_METHOD_LOCKED;
				}
			}

			// Token: 0x1700384C RID: 14412
			// (get) Token: 0x0600B8A4 RID: 47268 RVA: 0x002401B0 File Offset: 0x0023E3B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TRANS_HISTORY_DAYSColumn
			{
				get
				{
					return this.columnWADMIN_TRANS_HISTORY_DAYS;
				}
			}

			// Token: 0x1700384D RID: 14413
			// (get) Token: 0x0600B8A5 RID: 47269 RVA: 0x002401B8 File Offset: 0x0023E3B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TIMESHEET_SPANColumn
			{
				get
				{
					return this.columnWADMIN_TIMESHEET_SPAN;
				}
			}

			// Token: 0x1700384E RID: 14414
			// (get) Token: 0x0600B8A6 RID: 47270 RVA: 0x002401C0 File Offset: 0x0023E3C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_WEEKLY_TIMESHEET_NUM_WEEKSColumn
			{
				get
				{
					return this.columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS;
				}
			}

			// Token: 0x1700384F RID: 14415
			// (get) Token: 0x0600B8A7 RID: 47271 RVA: 0x002401C8 File Offset: 0x0023E3C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_MONTHLY_REPORTS_PER_MONTHColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_REPORTS_PER_MONTH;
				}
			}

			// Token: 0x17003850 RID: 14416
			// (get) Token: 0x0600B8A8 RID: 47272 RVA: 0x002401D0 File Offset: 0x0023E3D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_MONTHLY_1PRD_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_1PRD_1ST_START;
				}
			}

			// Token: 0x17003851 RID: 14417
			// (get) Token: 0x0600B8A9 RID: 47273 RVA: 0x002401D8 File Offset: 0x0023E3D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MONTHLY_2PRDS_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_2PRDS_1ST_START;
				}
			}

			// Token: 0x17003852 RID: 14418
			// (get) Token: 0x0600B8AA RID: 47274 RVA: 0x002401E0 File Offset: 0x0023E3E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MONTHLY_2PRDS_1ST_ENDColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_2PRDS_1ST_END;
				}
			}

			// Token: 0x17003853 RID: 14419
			// (get) Token: 0x0600B8AB RID: 47275 RVA: 0x002401E8 File Offset: 0x0023E3E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_MONTHLY_3PRDS_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_3PRDS_1ST_START;
				}
			}

			// Token: 0x17003854 RID: 14420
			// (get) Token: 0x0600B8AC RID: 47276 RVA: 0x002401F0 File Offset: 0x0023E3F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MONTHLY_3PRDS_1ST_ENDColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_3PRDS_1ST_END;
				}
			}

			// Token: 0x17003855 RID: 14421
			// (get) Token: 0x0600B8AD RID: 47277 RVA: 0x002401F8 File Offset: 0x0023E3F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MONTHLY_3PRDS_2ND_ENDColumn
			{
				get
				{
					return this.columnWADMIN_MONTHLY_3PRDS_2ND_END;
				}
			}

			// Token: 0x17003856 RID: 14422
			// (get) Token: 0x0600B8AE RID: 47278 RVA: 0x00240200 File Offset: 0x0023E400
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MAX_HOUR_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_MAX_HOUR_PER_DAY;
				}
			}

			// Token: 0x17003857 RID: 14423
			// (get) Token: 0x0600B8AF RID: 47279 RVA: 0x00240208 File Offset: 0x0023E408
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_LOOKAHEADColumn
			{
				get
				{
					return this.columnWADMIN_LOOKAHEAD;
				}
			}

			// Token: 0x17003858 RID: 14424
			// (get) Token: 0x0600B8B0 RID: 47280 RVA: 0x00240210 File Offset: 0x0023E410
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TIMEPERIOD_GRANULARITYColumn
			{
				get
				{
					return this.columnWADMIN_TIMEPERIOD_GRANULARITY;
				}
			}

			// Token: 0x17003859 RID: 14425
			// (get) Token: 0x0600B8B1 RID: 47281 RVA: 0x00240218 File Offset: 0x0023E418
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_LICENSESColumn
			{
				get
				{
					return this.columnWADMIN_LICENSES;
				}
			}

			// Token: 0x1700385A RID: 14426
			// (get) Token: 0x0600B8B2 RID: 47282 RVA: 0x00240220 File Offset: 0x0023E420
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_AUTO_CREATE_SUBWEBSColumn
			{
				get
				{
					return this.columnWADMIN_AUTO_CREATE_SUBWEBS;
				}
			}

			// Token: 0x1700385B RID: 14427
			// (get) Token: 0x0600B8B3 RID: 47283 RVA: 0x00240228 File Offset: 0x0023E428
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_AUTO_ADD_USER_TO_SUBWEBColumn
			{
				get
				{
					return this.columnWADMIN_AUTO_ADD_USER_TO_SUBWEB;
				}
			}

			// Token: 0x1700385C RID: 14428
			// (get) Token: 0x0600B8B4 RID: 47284 RVA: 0x00240230 File Offset: 0x0023E430
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_CURRENT_STS_SERVER_UIDColumn
			{
				get
				{
					return this.columnWADMIN_CURRENT_STS_SERVER_UID;
				}
			}

			// Token: 0x1700385D RID: 14429
			// (get) Token: 0x0600B8B5 RID: 47285 RVA: 0x00240238 File Offset: 0x0023E438
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_DEFAULT_SITE_COLLECTIONColumn
			{
				get
				{
					return this.columnWADMIN_DEFAULT_SITE_COLLECTION;
				}
			}

			// Token: 0x1700385E RID: 14430
			// (get) Token: 0x0600B8B6 RID: 47286 RVA: 0x00240240 File Offset: 0x0023E440
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ENABLE_ENTERPRISEColumn
			{
				get
				{
					return this.columnWADMIN_ENABLE_ENTERPRISE;
				}
			}

			// Token: 0x1700385F RID: 14431
			// (get) Token: 0x0600B8B7 RID: 47287 RVA: 0x00240248 File Offset: 0x0023E448
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_DISPLAY_MASTER_IN_ENTERPRISEColumn
			{
				get
				{
					return this.columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE;
				}
			}

			// Token: 0x17003860 RID: 14432
			// (get) Token: 0x0600B8B8 RID: 47288 RVA: 0x00240250 File Offset: 0x0023E450
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISEColumn
			{
				get
				{
					return this.columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE;
				}
			}

			// Token: 0x17003861 RID: 14433
			// (get) Token: 0x0600B8B9 RID: 47289 RVA: 0x00240258 File Offset: 0x0023E458
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SERVER_CURRENCYColumn
			{
				get
				{
					return this.columnWADMIN_SERVER_CURRENCY;
				}
			}

			// Token: 0x17003862 RID: 14434
			// (get) Token: 0x0600B8BA RID: 47290 RVA: 0x00240260 File Offset: 0x0023E460
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ENFORCE_CURRENCYColumn
			{
				get
				{
					return this.columnWADMIN_ENFORCE_CURRENCY;
				}
			}

			// Token: 0x17003863 RID: 14435
			// (get) Token: 0x0600B8BB RID: 47291 RVA: 0x00240268 File Offset: 0x0023E468
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_LAST_STS_ADMIN_SYNCH_TIMEColumn
			{
				get
				{
					return this.columnWADMIN_LAST_STS_ADMIN_SYNCH_TIME;
				}
			}

			// Token: 0x17003864 RID: 14436
			// (get) Token: 0x0600B8BC RID: 47292 RVA: 0x00240270 File Offset: 0x0023E470
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_SMTP_SERVER_NAMEColumn
			{
				get
				{
					return this.columnWADMIN_SMTP_SERVER_NAME;
				}
			}

			// Token: 0x17003865 RID: 14437
			// (get) Token: 0x0600B8BD RID: 47293 RVA: 0x00240278 File Offset: 0x0023E478
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_SMTP_SERVER_PORTColumn
			{
				get
				{
					return this.columnWADMIN_SMTP_SERVER_PORT;
				}
			}

			// Token: 0x17003866 RID: 14438
			// (get) Token: 0x0600B8BE RID: 47294 RVA: 0x00240280 File Offset: 0x0023E480
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_INTRANET_SERVER_URLColumn
			{
				get
				{
					return this.columnWADMIN_INTRANET_SERVER_URL;
				}
			}

			// Token: 0x17003867 RID: 14439
			// (get) Token: 0x0600B8BF RID: 47295 RVA: 0x00240288 File Offset: 0x0023E488
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_EXTRANET_SERVER_URLColumn
			{
				get
				{
					return this.columnWADMIN_EXTRANET_SERVER_URL;
				}
			}

			// Token: 0x17003868 RID: 14440
			// (get) Token: 0x0600B8C0 RID: 47296 RVA: 0x00240290 File Offset: 0x0023E490
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ONLY_PRO_PUBLISHColumn
			{
				get
				{
					return this.columnWADMIN_ONLY_PRO_PUBLISH;
				}
			}

			// Token: 0x17003869 RID: 14441
			// (get) Token: 0x0600B8C1 RID: 47297 RVA: 0x00240298 File Offset: 0x0023E498
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_PROTECT_ACTUALSColumn
			{
				get
				{
					return this.columnWADMIN_PROTECT_ACTUALS;
				}
			}

			// Token: 0x1700386A RID: 14442
			// (get) Token: 0x0600B8C2 RID: 47298 RVA: 0x002402A0 File Offset: 0x0023E4A0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STS_TEMPLATE_LCIDColumn
			{
				get
				{
					return this.columnWADMIN_STS_TEMPLATE_LCID;
				}
			}

			// Token: 0x1700386B RID: 14443
			// (get) Token: 0x0600B8C3 RID: 47299 RVA: 0x002402A8 File Offset: 0x0023E4A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STS_TEMPLATE_IDColumn
			{
				get
				{
					return this.columnWADMIN_STS_TEMPLATE_ID;
				}
			}

			// Token: 0x1700386C RID: 14444
			// (get) Token: 0x0600B8C4 RID: 47300 RVA: 0x002402B0 File Offset: 0x0023E4B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STS_PRIMARY_OWNER_EMAILColumn
			{
				get
				{
					return this.columnWADMIN_STS_PRIMARY_OWNER_EMAIL;
				}
			}

			// Token: 0x1700386D RID: 14445
			// (get) Token: 0x0600B8C5 RID: 47301 RVA: 0x002402B8 File Offset: 0x0023E4B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_BUILD_TEAM_BY_RBSColumn
			{
				get
				{
					return this.columnWADMIN_BUILD_TEAM_BY_RBS;
				}
			}

			// Token: 0x1700386E RID: 14446
			// (get) Token: 0x0600B8C6 RID: 47302 RVA: 0x002402C0 File Offset: 0x0023E4C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_IS_HOSTED_ORGColumn
			{
				get
				{
					return this.columnWADMIN_IS_HOSTED_ORG;
				}
			}

			// Token: 0x1700386F RID: 14447
			// (get) Token: 0x0600B8C7 RID: 47303 RVA: 0x002402C8 File Offset: 0x0023E4C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_USE_BASELINE_SUMMARY_DATAColumn
			{
				get
				{
					return this.columnWADMIN_USE_BASELINE_SUMMARY_DATA;
				}
			}

			// Token: 0x17003870 RID: 14448
			// (get) Token: 0x0600B8C8 RID: 47304 RVA: 0x002402D0 File Offset: 0x0023E4D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_PROJECT_BUILDColumn
			{
				get
				{
					return this.columnWADMIN_PROJECT_BUILD;
				}
			}

			// Token: 0x17003871 RID: 14449
			// (get) Token: 0x0600B8C9 RID: 47305 RVA: 0x002402D8 File Offset: 0x0023E4D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_MODE_ENUM;
				}
			}

			// Token: 0x17003872 RID: 14450
			// (get) Token: 0x0600B8CA RID: 47306 RVA: 0x002402E0 File Offset: 0x0023E4E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED;
				}
			}

			// Token: 0x17003873 RID: 14451
			// (get) Token: 0x0600B8CB RID: 47307 RVA: 0x002402E8 File Offset: 0x0023E4E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn
			{
				get
				{
					return this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION;
				}
			}

			// Token: 0x17003874 RID: 14452
			// (get) Token: 0x0600B8CC RID: 47308 RVA: 0x002402F0 File Offset: 0x0023E4F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_PROJECT_MANAGER_APPROVALColumn
			{
				get
				{
					return this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL;
				}
			}

			// Token: 0x17003875 RID: 14453
			// (get) Token: 0x0600B8CD RID: 47309 RVA: 0x002402F8 File Offset: 0x0023E4F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MAXIMUM_LINE_ITEMSColumn
			{
				get
				{
					return this.columnWADMIN_TS_MAXIMUM_LINE_ITEMS;
				}
			}

			// Token: 0x17003876 RID: 14454
			// (get) Token: 0x0600B8CE RID: 47310 RVA: 0x00240300 File Offset: 0x0023E500
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_IS_AUDIT_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_AUDIT_ENABLED;
				}
			}

			// Token: 0x17003877 RID: 14455
			// (get) Token: 0x0600B8CF RID: 47311 RVA: 0x00240308 File Offset: 0x0023E508
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn
			{
				get
				{
					return this.columnWADMIN_TS_IS_FUTURE_REP_ALLOWED;
				}
			}

			// Token: 0x17003878 RID: 14456
			// (get) Token: 0x0600B8D0 RID: 47312 RVA: 0x00240310 File Offset: 0x0023E510
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn
			{
				get
				{
					return this.columnWADMIN_TS_FIXED_APPROVAL_ROUTING;
				}
			}

			// Token: 0x17003879 RID: 14457
			// (get) Token: 0x0600B8D1 RID: 47313 RVA: 0x00240318 File Offset: 0x0023E518
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_TIED_MODEColumn
			{
				get
				{
					return this.columnWADMIN_TS_TIED_MODE;
				}
			}

			// Token: 0x1700387A RID: 14458
			// (get) Token: 0x0600B8D2 RID: 47314 RVA: 0x00240320 File Offset: 0x0023E520
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_MIN_HR_PER_TSColumn
			{
				get
				{
					return this.columnWADMIN_TS_MIN_HR_PER_TS;
				}
			}

			// Token: 0x1700387B RID: 14459
			// (get) Token: 0x0600B8D3 RID: 47315 RVA: 0x00240328 File Offset: 0x0023E528
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MAX_HR_PER_TSColumn
			{
				get
				{
					return this.columnWADMIN_TS_MAX_HR_PER_TS;
				}
			}

			// Token: 0x1700387C RID: 14460
			// (get) Token: 0x0600B8D4 RID: 47316 RVA: 0x00240330 File Offset: 0x0023E530
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_MAX_HR_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_TS_MAX_HR_PER_DAY;
				}
			}

			// Token: 0x1700387D RID: 14461
			// (get) Token: 0x0600B8D5 RID: 47317 RVA: 0x00240338 File Offset: 0x0023E538
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_HOURS_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_TS_HOURS_PER_DAY;
				}
			}

			// Token: 0x1700387E RID: 14462
			// (get) Token: 0x0600B8D6 RID: 47318 RVA: 0x00240340 File Offset: 0x0023E540
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_HOURS_PER_WEEKColumn
			{
				get
				{
					return this.columnWADMIN_TS_HOURS_PER_WEEK;
				}
			}

			// Token: 0x1700387F RID: 14463
			// (get) Token: 0x0600B8D7 RID: 47319 RVA: 0x00240348 File Offset: 0x0023E548
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_DEF_DISPLAY_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_DEF_DISPLAY_ENUM;
				}
			}

			// Token: 0x17003880 RID: 14464
			// (get) Token: 0x0600B8D8 RID: 47320 RVA: 0x00240350 File Offset: 0x0023E550
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_OUTLOOK_DISPLAY_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM;
				}
			}

			// Token: 0x17003881 RID: 14465
			// (get) Token: 0x0600B8D9 RID: 47321 RVA: 0x00240358 File Offset: 0x0023E558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_CREATE_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_CREATE_MODE_ENUM;
				}
			}

			// Token: 0x17003882 RID: 14466
			// (get) Token: 0x0600B8DA RID: 47322 RVA: 0x00240360 File Offset: 0x0023E560
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_TS_REPORT_UNIT_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_REPORT_UNIT_ENUM;
				}
			}

			// Token: 0x17003883 RID: 14467
			// (get) Token: 0x0600B8DB RID: 47323 RVA: 0x00240368 File Offset: 0x0023E568
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WEEK_START_ON_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_WEEK_START_ON_ENUM;
				}
			}

			// Token: 0x17003884 RID: 14468
			// (get) Token: 0x0600B8DC RID: 47324 RVA: 0x00240370 File Offset: 0x0023E570
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_MAX_HR_PER_DAYColumn
			{
				get
				{
					return this.columnWADMIN_STAT_MAX_HR_PER_DAY;
				}
			}

			// Token: 0x17003885 RID: 14469
			// (get) Token: 0x0600B8DD RID: 47325 RVA: 0x00240378 File Offset: 0x0023E578
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_MAX_HR_PER_TASKColumn
			{
				get
				{
					return this.columnWADMIN_STAT_MAX_HR_PER_TASK;
				}
			}

			// Token: 0x17003886 RID: 14470
			// (get) Token: 0x0600B8DE RID: 47326 RVA: 0x00240380 File Offset: 0x0023E580
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_LOOK_AHEADColumn
			{
				get
				{
					return this.columnWADMIN_STAT_LOOK_AHEAD;
				}
			}

			// Token: 0x17003887 RID: 14471
			// (get) Token: 0x0600B8DF RID: 47327 RVA: 0x00240388 File Offset: 0x0023E588
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_LOOK_AHEAD_PERIODSColumn
			{
				get
				{
					return this.columnWADMIN_STAT_LOOK_AHEAD_PERIODS;
				}
			}

			// Token: 0x17003888 RID: 14472
			// (get) Token: 0x0600B8E0 RID: 47328 RVA: 0x00240390 File Offset: 0x0023E590
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_REP_SCHED_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_STAT_REP_SCHED_ENUM;
				}
			}

			// Token: 0x17003889 RID: 14473
			// (get) Token: 0x0600B8E1 RID: 47329 RVA: 0x00240398 File Offset: 0x0023E598
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_SPAN_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_STAT_SPAN_MODE_ENUM;
				}
			}

			// Token: 0x1700388A RID: 14474
			// (get) Token: 0x0600B8E2 RID: 47330 RVA: 0x002403A0 File Offset: 0x0023E5A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn
			{
				get
				{
					return this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM;
				}
			}

			// Token: 0x1700388B RID: 14475
			// (get) Token: 0x0600B8E3 RID: 47331 RVA: 0x002403A8 File Offset: 0x0023E5A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_PROT_ACTColumn
			{
				get
				{
					return this.columnWADMIN_STAT_PROT_ACT;
				}
			}

			// Token: 0x1700388C RID: 14476
			// (get) Token: 0x0600B8E4 RID: 47332 RVA: 0x002403B0 File Offset: 0x0023E5B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_PERIOD_TYPEColumn
			{
				get
				{
					return this.columnWADMIN_STAT_PERIOD_TYPE;
				}
			}

			// Token: 0x1700388D RID: 14477
			// (get) Token: 0x0600B8E5 RID: 47333 RVA: 0x002403B8 File Offset: 0x0023E5B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_ENABLE_DOWNLOADColumn
			{
				get
				{
					return this.columnWADMIN_STAT_ENABLE_DOWNLOAD;
				}
			}

			// Token: 0x1700388E RID: 14478
			// (get) Token: 0x0600B8E6 RID: 47334 RVA: 0x002403C0 File Offset: 0x0023E5C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_NUM_WK_SPANNEDColumn
			{
				get
				{
					return this.columnWADMIN_STAT_NUM_WK_SPANNED;
				}
			}

			// Token: 0x1700388F RID: 14479
			// (get) Token: 0x0600B8E7 RID: 47335 RVA: 0x002403C8 File Offset: 0x0023E5C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_NUM_UPDATES_PER_MONTHColumn
			{
				get
				{
					return this.columnWADMIN_STAT_NUM_UPDATES_PER_MONTH;
				}
			}

			// Token: 0x17003890 RID: 14480
			// (get) Token: 0x0600B8E8 RID: 47336 RVA: 0x002403D0 File Offset: 0x0023E5D0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_1PRD_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_STAT_1PRD_1ST_START;
				}
			}

			// Token: 0x17003891 RID: 14481
			// (get) Token: 0x0600B8E9 RID: 47337 RVA: 0x002403D8 File Offset: 0x0023E5D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_2PRD_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_STAT_2PRD_1ST_START;
				}
			}

			// Token: 0x17003892 RID: 14482
			// (get) Token: 0x0600B8EA RID: 47338 RVA: 0x002403E0 File Offset: 0x0023E5E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_2PRD_1ST_ENDColumn
			{
				get
				{
					return this.columnWADMIN_STAT_2PRD_1ST_END;
				}
			}

			// Token: 0x17003893 RID: 14483
			// (get) Token: 0x0600B8EB RID: 47339 RVA: 0x002403E8 File Offset: 0x0023E5E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_3PRD_1ST_STARTColumn
			{
				get
				{
					return this.columnWADMIN_STAT_3PRD_1ST_START;
				}
			}

			// Token: 0x17003894 RID: 14484
			// (get) Token: 0x0600B8EC RID: 47340 RVA: 0x002403F0 File Offset: 0x0023E5F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_3PRD_1ST_ENDColumn
			{
				get
				{
					return this.columnWADMIN_STAT_3PRD_1ST_END;
				}
			}

			// Token: 0x17003895 RID: 14485
			// (get) Token: 0x0600B8ED RID: 47341 RVA: 0x002403F8 File Offset: 0x0023E5F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_3PRD_2ND_ENDColumn
			{
				get
				{
					return this.columnWADMIN_STAT_3PRD_2ND_END;
				}
			}

			// Token: 0x17003896 RID: 14486
			// (get) Token: 0x0600B8EE RID: 47342 RVA: 0x00240400 File Offset: 0x0023E600
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_LIST_SEPARATORColumn
			{
				get
				{
					return this.columnWADMIN_LIST_SEPARATOR;
				}
			}

			// Token: 0x17003897 RID: 14487
			// (get) Token: 0x0600B8EF RID: 47343 RVA: 0x00240408 File Offset: 0x0023E608
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_ACTIVE_CACHE_DIRColumn
			{
				get
				{
					return this.columnWADMIN_ACTIVE_CACHE_DIR;
				}
			}

			// Token: 0x17003898 RID: 14488
			// (get) Token: 0x0600B8F0 RID: 47344 RVA: 0x00240410 File Offset: 0x0023E610
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ACTIVE_CACHE_MAX_SIZE_MBColumn
			{
				get
				{
					return this.columnWADMIN_ACTIVE_CACHE_MAX_SIZE_MB;
				}
			}

			// Token: 0x17003899 RID: 14489
			// (get) Token: 0x0600B8F1 RID: 47345 RVA: 0x00240418 File Offset: 0x0023E618
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_RES_SCHEDULED_TIMEColumn
			{
				get
				{
					return this.columnWADMIN_RES_SCHEDULED_TIME;
				}
			}

			// Token: 0x1700389A RID: 14490
			// (get) Token: 0x0600B8F2 RID: 47346 RVA: 0x00240420 File Offset: 0x0023E620
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_RESPLAN_FTEColumn
			{
				get
				{
					return this.columnWADMIN_RESPLAN_FTE;
				}
			}

			// Token: 0x1700389B RID: 14491
			// (get) Token: 0x0600B8F3 RID: 47347 RVA: 0x00240428 File Offset: 0x0023E628
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_RESOURCE_CAPACITY_MONTHS_BEHINDColumn
			{
				get
				{
					return this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND;
				}
			}

			// Token: 0x1700389C RID: 14492
			// (get) Token: 0x0600B8F4 RID: 47348 RVA: 0x00240430 File Offset: 0x0023E630
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_RESOURCE_CAPACITY_MONTHS_AHEADColumn
			{
				get
				{
					return this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD;
				}
			}

			// Token: 0x1700389D RID: 14493
			// (get) Token: 0x0600B8F5 RID: 47349 RVA: 0x00240438 File Offset: 0x0023E638
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_RESOURCE_CAPACITY_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_RESOURCE_CAPACITY_JOB_UID;
				}
			}

			// Token: 0x1700389E RID: 14494
			// (get) Token: 0x0600B8F6 RID: 47350 RVA: 0x00240440 File Offset: 0x0023E640
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_REMINDER_TIMER_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_REMINDER_TIMER_JOB_UID;
				}
			}

			// Token: 0x1700389F RID: 14495
			// (get) Token: 0x0600B8F7 RID: 47351 RVA: 0x00240448 File Offset: 0x0023E648
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_USE_PROJECT_STATEColumn
			{
				get
				{
					return this.columnWADMIN_USE_PROJECT_STATE;
				}
			}

			// Token: 0x170038A0 RID: 14496
			// (get) Token: 0x0600B8F8 RID: 47352 RVA: 0x00240450 File Offset: 0x0023E650
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SHOW_WSS_NAV_LINKSColumn
			{
				get
				{
					return this.columnWADMIN_SHOW_WSS_NAV_LINKS;
				}
			}

			// Token: 0x170038A1 RID: 14497
			// (get) Token: 0x0600B8F9 RID: 47353 RVA: 0x00240458 File Offset: 0x0023E658
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn
			{
				get
				{
					return this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS;
				}
			}

			// Token: 0x170038A2 RID: 14498
			// (get) Token: 0x0600B8FA RID: 47354 RVA: 0x00240460 File Offset: 0x0023E660
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WORKFLOW_PROXY_ACCTColumn
			{
				get
				{
					return this.columnWADMIN_WORKFLOW_PROXY_ACCT;
				}
			}

			// Token: 0x170038A3 RID: 14499
			// (get) Token: 0x0600B8FB RID: 47355 RVA: 0x00240468 File Offset: 0x0023E668
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_WORKFLOW_PROXY_UIDColumn
			{
				get
				{
					return this.columnWADMIN_WORKFLOW_PROXY_UID;
				}
			}

			// Token: 0x170038A4 RID: 14500
			// (get) Token: 0x0600B8FC RID: 47356 RVA: 0x00240470 File Offset: 0x0023E670
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WORKFLOW_PROXY_WINDOWSColumn
			{
				get
				{
					return this.columnWADMIN_WORKFLOW_PROXY_WINDOWS;
				}
			}

			// Token: 0x170038A5 RID: 14501
			// (get) Token: 0x0600B8FD RID: 47357 RVA: 0x00240478 File Offset: 0x0023E678
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WORKFLOW_PROXY_MOD_BYColumn
			{
				get
				{
					return this.columnWADMIN_WORKFLOW_PROXY_MOD_BY;
				}
			}

			// Token: 0x170038A6 RID: 14502
			// (get) Token: 0x0600B8FE RID: 47358 RVA: 0x00240480 File Offset: 0x0023E680
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_WORKFLOW_PROXY_MOD_DATEColumn
			{
				get
				{
					return this.columnWADMIN_WORKFLOW_PROXY_MOD_DATE;
				}
			}

			// Token: 0x170038A7 RID: 14503
			// (get) Token: 0x0600B8FF RID: 47359 RVA: 0x00240488 File Offset: 0x0023E688
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn CREATED_DATEColumn
			{
				get
				{
					return this.columnCREATED_DATE;
				}
			}

			// Token: 0x170038A8 RID: 14504
			// (get) Token: 0x0600B900 RID: 47360 RVA: 0x00240490 File Offset: 0x0023E690
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_DATEColumn
			{
				get
				{
					return this.columnMOD_DATE;
				}
			}

			// Token: 0x170038A9 RID: 14505
			// (get) Token: 0x0600B901 RID: 47361 RVA: 0x00240498 File Offset: 0x0023E698
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn CREATED_REV_COUNTERColumn
			{
				get
				{
					return this.columnCREATED_REV_COUNTER;
				}
			}

			// Token: 0x170038AA RID: 14506
			// (get) Token: 0x0600B902 RID: 47362 RVA: 0x002404A0 File Offset: 0x0023E6A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn MOD_REV_COUNTERColumn
			{
				get
				{
					return this.columnMOD_REV_COUNTER;
				}
			}

			// Token: 0x170038AB RID: 14507
			// (get) Token: 0x0600B903 RID: 47363 RVA: 0x002404A8 File Offset: 0x0023E6A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SERVER_FLAGSColumn
			{
				get
				{
					return this.columnWADMIN_SERVER_FLAGS;
				}
			}

			// Token: 0x170038AC RID: 14508
			// (get) Token: 0x0600B904 RID: 47364 RVA: 0x002404B0 File Offset: 0x0023E6B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MIN_WINPROJ_BUILD_NUMBERSColumn
			{
				get
				{
					return this.columnWADMIN_MIN_WINPROJ_BUILD_NUMBERS;
				}
			}

			// Token: 0x170038AD RID: 14509
			// (get) Token: 0x0600B905 RID: 47365 RVA: 0x002404B8 File Offset: 0x0023E6B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_EXCHANGE_INTEGRATION_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_EXCHANGE_INTEGRATION_ENABLED;
				}
			}

			// Token: 0x170038AE RID: 14510
			// (get) Token: 0x0600B906 RID: 47366 RVA: 0x002404C0 File Offset: 0x0023E6C0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_EXCHANGE_URL_REFRESH_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_EXCHANGE_URL_REFRESH_JOB_UID;
				}
			}

			// Token: 0x170038AF RID: 14511
			// (get) Token: 0x0600B907 RID: 47367 RVA: 0x002404C8 File Offset: 0x0023E6C8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID;
				}
			}

			// Token: 0x170038B0 RID: 14512
			// (get) Token: 0x0600B908 RID: 47368 RVA: 0x002404D0 File Offset: 0x0023E6D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID;
				}
			}

			// Token: 0x170038B1 RID: 14513
			// (get) Token: 0x0600B909 RID: 47369 RVA: 0x002404D8 File Offset: 0x0023E6D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_PUBLISH_MANUAL_TASKSColumn
			{
				get
				{
					return this.columnWADMIN_PUBLISH_MANUAL_TASKS;
				}
			}

			// Token: 0x170038B2 RID: 14514
			// (get) Token: 0x0600B90A RID: 47370 RVA: 0x002404E0 File Offset: 0x0023E6E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SERVER_DEFAULT_TASK_MODEColumn
			{
				get
				{
					return this.columnWADMIN_SERVER_DEFAULT_TASK_MODE;
				}
			}

			// Token: 0x170038B3 RID: 14515
			// (get) Token: 0x0600B90B RID: 47371 RVA: 0x002404E8 File Offset: 0x0023E6E8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_LOCK_PRO_DEFAULT_TASK_MODEColumn
			{
				get
				{
					return this.columnWADMIN_LOCK_PRO_DEFAULT_TASK_MODE;
				}
			}

			// Token: 0x170038B4 RID: 14516
			// (get) Token: 0x0600B90C RID: 47372 RVA: 0x002404F0 File Offset: 0x0023E6F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_TS_ALLOW_PROJECT_LEVELColumn
			{
				get
				{
					return this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL;
				}
			}

			// Token: 0x170038B5 RID: 14517
			// (get) Token: 0x0600B90D RID: 47373 RVA: 0x002404F8 File Offset: 0x0023E6F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED;
				}
			}

			// Token: 0x170038B6 RID: 14518
			// (get) Token: 0x0600B90E RID: 47374 RVA: 0x00240500 File Offset: 0x0023E700
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_OFF_PEAK_SYNC_THRESHOLDColumn
			{
				get
				{
					return this.columnWADMIN_OFF_PEAK_SYNC_THRESHOLD;
				}
			}

			// Token: 0x170038B7 RID: 14519
			// (get) Token: 0x0600B90F RID: 47375 RVA: 0x00240508 File Offset: 0x0023E708
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_DISABLED_SYNC_THRESHOLDColumn
			{
				get
				{
					return this.columnWADMIN_DISABLED_SYNC_THRESHOLD;
				}
			}

			// Token: 0x170038B8 RID: 14520
			// (get) Token: 0x0600B910 RID: 47376 RVA: 0x00240510 File Offset: 0x0023E710
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDColumn
			{
				get
				{
					return this.columnWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID;
				}
			}

			// Token: 0x170038B9 RID: 14521
			// (get) Token: 0x0600B911 RID: 47377 RVA: 0x00240518 File Offset: 0x0023E718
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_IMPORT_LINE_CLASSESColumn
			{
				get
				{
					return this.columnWADMIN_STAT_IMPORT_LINE_CLASSES;
				}
			}

			// Token: 0x170038BA RID: 14522
			// (get) Token: 0x0600B912 RID: 47378 RVA: 0x00240520 File Offset: 0x0023E720
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_DATABASE_CACHE_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_DATABASE_CACHE_ENABLED;
				}
			}

			// Token: 0x170038BB RID: 14523
			// (get) Token: 0x0600B913 RID: 47379 RVA: 0x00240528 File Offset: 0x0023E728
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_WSS_PWA_ADMIN_ROLE_IDColumn
			{
				get
				{
					return this.columnWADMIN_WSS_PWA_ADMIN_ROLE_ID;
				}
			}

			// Token: 0x170038BC RID: 14524
			// (get) Token: 0x0600B914 RID: 47380 RVA: 0x00240530 File Offset: 0x0023E730
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDColumn
			{
				get
				{
					return this.columnWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID;
				}
			}

			// Token: 0x170038BD RID: 14525
			// (get) Token: 0x0600B915 RID: 47381 RVA: 0x00240538 File Offset: 0x0023E738
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDColumn
			{
				get
				{
					return this.columnWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID;
				}
			}

			// Token: 0x170038BE RID: 14526
			// (get) Token: 0x0600B916 RID: 47382 RVA: 0x00240540 File Offset: 0x0023E740
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_WSS_PWA_READER_ROLE_IDColumn
			{
				get
				{
					return this.columnWADMIN_WSS_PWA_READER_ROLE_ID;
				}
			}

			// Token: 0x170038BF RID: 14527
			// (get) Token: 0x0600B917 RID: 47383 RVA: 0x00240548 File Offset: 0x0023E748
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_STAT_ALLOW_FREEFORM_PERIODSColumn
			{
				get
				{
					return this.columnWADMIN_STAT_ALLOW_FREEFORM_PERIODS;
				}
			}

			// Token: 0x170038C0 RID: 14528
			// (get) Token: 0x0600B918 RID: 47384 RVA: 0x00240550 File Offset: 0x0023E750
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_STAT_TIMESHEET_TIEDColumn
			{
				get
				{
					return this.columnWADMIN_STAT_TIMESHEET_TIED;
				}
			}

			// Token: 0x170038C1 RID: 14529
			// (get) Token: 0x0600B919 RID: 47385 RVA: 0x00240558 File Offset: 0x0023E758
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISColumn
			{
				get
				{
					return this.columnWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS;
				}
			}

			// Token: 0x170038C2 RID: 14530
			// (get) Token: 0x0600B91A RID: 47386 RVA: 0x00240560 File Offset: 0x0023E760
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISColumn
			{
				get
				{
					return this.columnWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS;
				}
			}

			// Token: 0x170038C3 RID: 14531
			// (get) Token: 0x0600B91B RID: 47387 RVA: 0x00240568 File Offset: 0x0023E768
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_MAX_SQL_BATCH_SIZEColumn
			{
				get
				{
					return this.columnWADMIN_MAX_SQL_BATCH_SIZE;
				}
			}

			// Token: 0x170038C4 RID: 14532
			// (get) Token: 0x0600B91C RID: 47388 RVA: 0x00240570 File Offset: 0x0023E770
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_CORE_SQL_TIMEOUTColumn
			{
				get
				{
					return this.columnWADMIN_CORE_SQL_TIMEOUT;
				}
			}

			// Token: 0x170038C5 RID: 14533
			// (get) Token: 0x0600B91D RID: 47389 RVA: 0x00240578 File Offset: 0x0023E778
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_MAX_SSP_BATCH_SIZEColumn
			{
				get
				{
					return this.columnWADMIN_MAX_SSP_BATCH_SIZE;
				}
			}

			// Token: 0x170038C6 RID: 14534
			// (get) Token: 0x0600B91E RID: 47390 RVA: 0x00240580 File Offset: 0x0023E780
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_USER_SYNC_SETTINGColumn
			{
				get
				{
					return this.columnWADMIN_USER_SYNC_SETTING;
				}
			}

			// Token: 0x170038C7 RID: 14535
			// (get) Token: 0x0600B91F RID: 47391 RVA: 0x00240588 File Offset: 0x0023E788
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_AD_SYNC_REPLACE_CHARColumn
			{
				get
				{
					return this.columnWADMIN_AD_SYNC_REPLACE_CHAR;
				}
			}

			// Token: 0x170038C8 RID: 14536
			// (get) Token: 0x0600B920 RID: 47392 RVA: 0x00240590 File Offset: 0x0023E790
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SQL_BATCHING_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_SQL_BATCHING_ENABLED;
				}
			}

			// Token: 0x170038C9 RID: 14537
			// (get) Token: 0x0600B921 RID: 47393 RVA: 0x00240598 File Offset: 0x0023E798
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SQL_BATCHING_BUFFER_SIZEColumn
			{
				get
				{
					return this.columnWADMIN_SQL_BATCHING_BUFFER_SIZE;
				}
			}

			// Token: 0x170038CA RID: 14538
			// (get) Token: 0x0600B922 RID: 47394 RVA: 0x002405A0 File Offset: 0x0023E7A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_WSS_RESTRICT_WORKSPACE_CREATIONColumn
			{
				get
				{
					return this.columnWADMIN_WSS_RESTRICT_WORKSPACE_CREATION;
				}
			}

			// Token: 0x170038CB RID: 14539
			// (get) Token: 0x0600B923 RID: 47395 RVA: 0x002405A8 File Offset: 0x0023E7A8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_FULL_SYNC_THRESHOLDColumn
			{
				get
				{
					return this.columnWADMIN_FULL_SYNC_THRESHOLD;
				}
			}

			// Token: 0x170038CC RID: 14540
			// (get) Token: 0x0600B924 RID: 47396 RVA: 0x002405B0 File Offset: 0x0023E7B0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn TIMESHEET_CURRENT_VIEWSET_UIDColumn
			{
				get
				{
					return this.columnTIMESHEET_CURRENT_VIEWSET_UID;
				}
			}

			// Token: 0x170038CD RID: 14541
			// (get) Token: 0x0600B925 RID: 47397 RVA: 0x002405B8 File Offset: 0x0023E7B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_PERMISSION_MODEColumn
			{
				get
				{
					return this.columnWADMIN_PERMISSION_MODE;
				}
			}

			// Token: 0x170038CE RID: 14542
			// (get) Token: 0x0600B926 RID: 47398 RVA: 0x002405C0 File Offset: 0x0023E7C0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_SPPERMMODE_LAST_SYNCColumn
			{
				get
				{
					return this.columnWADMIN_SPPERMMODE_LAST_SYNC;
				}
			}

			// Token: 0x170038CF RID: 14543
			// (get) Token: 0x0600B927 RID: 47399 RVA: 0x002405C8 File Offset: 0x0023E7C8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SITEMAP_CACHE_VERSIONColumn
			{
				get
				{
					return this.columnWADMIN_SITEMAP_CACHE_VERSION;
				}
			}

			// Token: 0x170038D0 RID: 14544
			// (get) Token: 0x0600B928 RID: 47400 RVA: 0x002405D0 File Offset: 0x0023E7D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_GROUPINGGANTT_CACHE_VERSIONColumn
			{
				get
				{
					return this.columnWADMIN_GROUPINGGANTT_CACHE_VERSION;
				}
			}

			// Token: 0x170038D1 RID: 14545
			// (get) Token: 0x0600B929 RID: 47401 RVA: 0x002405D8 File Offset: 0x0023E7D8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED;
				}
			}

			// Token: 0x170038D2 RID: 14546
			// (get) Token: 0x0600B92A RID: 47402 RVA: 0x002405E0 File Offset: 0x0023E7E0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SETTINGS_VERSIONColumn
			{
				get
				{
					return this.columnWADMIN_SETTINGS_VERSION;
				}
			}

			// Token: 0x170038D3 RID: 14547
			// (get) Token: 0x0600B92B RID: 47403 RVA: 0x002405E8 File Offset: 0x0023E7E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_IS_UPDATINGColumn
			{
				get
				{
					return this.columnWADMIN_IS_UPDATING;
				}
			}

			// Token: 0x170038D4 RID: 14548
			// (get) Token: 0x0600B92C RID: 47404 RVA: 0x002405F0 File Offset: 0x0023E7F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_PROVISIONING_RESULTColumn
			{
				get
				{
					return this.columnWADMIN_PROVISIONING_RESULT;
				}
			}

			// Token: 0x170038D5 RID: 14549
			// (get) Token: 0x0600B92D RID: 47405 RVA: 0x002405F8 File Offset: 0x0023E7F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_LOGICAL_READONLYColumn
			{
				get
				{
					return this.columnWADMIN_LOGICAL_READONLY;
				}
			}

			// Token: 0x170038D6 RID: 14550
			// (get) Token: 0x0600B92E RID: 47406 RVA: 0x00240600 File Offset: 0x0023E800
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_OVER_QUOTAColumn
			{
				get
				{
					return this.columnWADMIN_OVER_QUOTA;
				}
			}

			// Token: 0x170038D7 RID: 14551
			// (get) Token: 0x0600B92F RID: 47407 RVA: 0x00240608 File Offset: 0x0023E808
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataColumn WADMIN_IS_DELETEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_DELETED;
				}
			}

			// Token: 0x170038D8 RID: 14552
			// (get) Token: 0x0600B930 RID: 47408 RVA: 0x00240610 File Offset: 0x0023E810
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_SYNC_TASKS_TO_TASKLISTColumn
			{
				get
				{
					return this.columnWADMIN_SYNC_TASKS_TO_TASKLIST;
				}
			}

			// Token: 0x170038D9 RID: 14553
			// (get) Token: 0x0600B931 RID: 47409 RVA: 0x00240618 File Offset: 0x0023E818
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_USE_ENGAGEMENTSColumn
			{
				get
				{
					return this.columnWADMIN_USE_ENGAGEMENTS;
				}
			}

			// Token: 0x170038DA RID: 14554
			// (get) Token: 0x0600B932 RID: 47410 RVA: 0x00240620 File Offset: 0x0023E820
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DataColumn WADMIN_IS_NOTIFICATION_ENABLEDColumn
			{
				get
				{
					return this.columnWADMIN_IS_NOTIFICATION_ENABLED;
				}
			}

			// Token: 0x1400068D RID: 1677
			// (add) Token: 0x0600B935 RID: 47413 RVA: 0x00240648 File Offset: 0x0023E848
			// (remove) Token: 0x0600B936 RID: 47414 RVA: 0x00240680 File Offset: 0x0023E880
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WebAdminDataSet.WebAdminRowChangeEventHandler WebAdminRowChanging;

			// Token: 0x1400068E RID: 1678
			// (add) Token: 0x0600B937 RID: 47415 RVA: 0x002406B8 File Offset: 0x0023E8B8
			// (remove) Token: 0x0600B938 RID: 47416 RVA: 0x002406F0 File Offset: 0x0023E8F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WebAdminDataSet.WebAdminRowChangeEventHandler WebAdminRowChanged;

			// Token: 0x1400068F RID: 1679
			// (add) Token: 0x0600B939 RID: 47417 RVA: 0x00240728 File Offset: 0x0023E928
			// (remove) Token: 0x0600B93A RID: 47418 RVA: 0x00240760 File Offset: 0x0023E960
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WebAdminDataSet.WebAdminRowChangeEventHandler WebAdminRowDeleting;

			// Token: 0x14000690 RID: 1680
			// (add) Token: 0x0600B93B RID: 47419 RVA: 0x00240798 File Offset: 0x0023E998
			// (remove) Token: 0x0600B93C RID: 47420 RVA: 0x002407D0 File Offset: 0x0023E9D0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public event WebAdminDataSet.WebAdminRowChangeEventHandler WebAdminRowDeleted;

			// Token: 0x0600B93D RID: 47421 RVA: 0x00240805 File Offset: 0x0023EA05
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void AddWebAdminRow(WebAdminDataSet.WebAdminRow row)
			{
				base.Rows.Add(row);
			}

			// Token: 0x0600B93E RID: 47422 RVA: 0x00240814 File Offset: 0x0023EA14
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WebAdminDataSet.WebAdminRow AddWebAdminRow(Guid WADMIN_UIDFAKE, int WADMIN_AUTHENTICATION_TYPE, int WADMIN_NEW_ACCOUNT_PRIVILEGE, byte WADMIN_IS_DELEGATION_ALLOWED, byte WADMIN_AUTH_REQUIRED_FOR_PUBLISH, int WADMIN_WEEK_STARTS_ON, int WADMIN_MIN_PASSWORD_LENGTH, string WADMIN_NTFY_FROM_EMAIL, string WADMIN_NTFY_EMAIL_TRAILER, string WADMIN_ORG_EMAIL_ADDRESS, string WADMIN_EMAIL_CHARSET, int WADMIN_DEFAULT_LANGUAGE, int WADMIN_DEFAULT_TRACKING_METHOD, byte WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS, bool WADMIN_IS_TRACKING_METHOD_LOCKED, int WADMIN_TRANS_HISTORY_DAYS, byte WADMIN_TIMESHEET_SPAN, byte WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS, byte WADMIN_MONTHLY_REPORTS_PER_MONTH, byte WADMIN_MONTHLY_1PRD_1ST_START, byte WADMIN_MONTHLY_2PRDS_1ST_START, byte WADMIN_MONTHLY_2PRDS_1ST_END, byte WADMIN_MONTHLY_3PRDS_1ST_START, byte WADMIN_MONTHLY_3PRDS_1ST_END, byte WADMIN_MONTHLY_3PRDS_2ND_END, decimal WADMIN_MAX_HOUR_PER_DAY, int WADMIN_LOOKAHEAD, byte WADMIN_TIMEPERIOD_GRANULARITY, int WADMIN_LICENSES, byte WADMIN_AUTO_CREATE_SUBWEBS, byte WADMIN_AUTO_ADD_USER_TO_SUBWEB, Guid WADMIN_CURRENT_STS_SERVER_UID, string WADMIN_DEFAULT_SITE_COLLECTION, byte WADMIN_ENABLE_ENTERPRISE, byte WADMIN_DISPLAY_MASTER_IN_ENTERPRISE, byte WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE, string WADMIN_SERVER_CURRENCY, byte WADMIN_ENFORCE_CURRENCY, DateTime WADMIN_LAST_STS_ADMIN_SYNCH_TIME, string WADMIN_SMTP_SERVER_NAME, int WADMIN_SMTP_SERVER_PORT, string WADMIN_INTRANET_SERVER_URL, string WADMIN_EXTRANET_SERVER_URL, byte WADMIN_ONLY_PRO_PUBLISH, byte WADMIN_PROTECT_ACTUALS, int WADMIN_STS_TEMPLATE_LCID, string WADMIN_STS_TEMPLATE_ID, string WADMIN_STS_PRIMARY_OWNER_EMAIL, byte WADMIN_BUILD_TEAM_BY_RBS, byte WADMIN_IS_HOSTED_ORG, byte WADMIN_USE_BASELINE_SUMMARY_DATA, string WADMIN_PROJECT_BUILD, byte WADMIN_TS_MODE_ENUM, bool WADMIN_TS_IS_UNVERS_TASK_ALLOWED, bool WADMIN_TS_PROJECT_MANAGER_COORDINATION, bool WADMIN_TS_PROJECT_MANAGER_APPROVAL, int WADMIN_TS_MAXIMUM_LINE_ITEMS, bool WADMIN_TS_IS_AUDIT_ENABLED, bool WADMIN_TS_IS_FUTURE_REP_ALLOWED, bool WADMIN_TS_FIXED_APPROVAL_ROUTING, bool WADMIN_TS_TIED_MODE, decimal WADMIN_TS_MIN_HR_PER_TS, decimal WADMIN_TS_MAX_HR_PER_TS, decimal WADMIN_TS_MAX_HR_PER_DAY, decimal WADMIN_TS_HOURS_PER_DAY, decimal WADMIN_TS_HOURS_PER_WEEK, byte WADMIN_TS_DEF_DISPLAY_ENUM, byte WADMIN_TS_OUTLOOK_DISPLAY_ENUM, byte WADMIN_TS_CREATE_MODE_ENUM, byte WADMIN_TS_REPORT_UNIT_ENUM, byte WADMIN_WEEK_START_ON_ENUM, decimal WADMIN_STAT_MAX_HR_PER_DAY, decimal WADMIN_STAT_MAX_HR_PER_TASK, int WADMIN_STAT_LOOK_AHEAD, int WADMIN_STAT_LOOK_AHEAD_PERIODS, byte WADMIN_STAT_REP_SCHED_ENUM, byte WADMIN_STAT_SPAN_MODE_ENUM, byte WADMIN_TS_DEF_ENTRY_MODE_ENUM, bool WADMIN_STAT_PROT_ACT, byte WADMIN_STAT_PERIOD_TYPE, bool WADMIN_STAT_ENABLE_DOWNLOAD, int WADMIN_STAT_NUM_WK_SPANNED, int WADMIN_STAT_NUM_UPDATES_PER_MONTH, int WADMIN_STAT_1PRD_1ST_START, int WADMIN_STAT_2PRD_1ST_START, int WADMIN_STAT_2PRD_1ST_END, int WADMIN_STAT_3PRD_1ST_START, int WADMIN_STAT_3PRD_1ST_END, int WADMIN_STAT_3PRD_2ND_END, string WADMIN_LIST_SEPARATOR, string WADMIN_ACTIVE_CACHE_DIR, int WADMIN_ACTIVE_CACHE_MAX_SIZE_MB, long WADMIN_RES_SCHEDULED_TIME, decimal WADMIN_RESPLAN_FTE, int WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND, int WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD, Guid WADMIN_RESOURCE_CAPACITY_JOB_UID, Guid WADMIN_REMINDER_TIMER_JOB_UID, bool WADMIN_USE_PROJECT_STATE, bool WADMIN_SHOW_WSS_NAV_LINKS, bool WADMIN_ALWAYS_EXPAND_NAV_LINKS, string WADMIN_WORKFLOW_PROXY_ACCT, Guid WADMIN_WORKFLOW_PROXY_UID, bool WADMIN_WORKFLOW_PROXY_WINDOWS, string WADMIN_WORKFLOW_PROXY_MOD_BY, DateTime WADMIN_WORKFLOW_PROXY_MOD_DATE, DateTime CREATED_DATE, DateTime MOD_DATE, int CREATED_REV_COUNTER, int MOD_REV_COUNTER, int WADMIN_SERVER_FLAGS, string WADMIN_MIN_WINPROJ_BUILD_NUMBERS, bool WADMIN_EXCHANGE_INTEGRATION_ENABLED, Guid WADMIN_EXCHANGE_URL_REFRESH_JOB_UID, Guid WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID, Guid WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID, bool WADMIN_PUBLISH_MANUAL_TASKS, bool WADMIN_SERVER_DEFAULT_TASK_MODE, bool WADMIN_LOCK_PRO_DEFAULT_TASK_MODE, bool WADMIN_TS_ALLOW_PROJECT_LEVEL, bool WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED, int WADMIN_OFF_PEAK_SYNC_THRESHOLD, int WADMIN_DISABLED_SYNC_THRESHOLD, Guid WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID, bool WADMIN_STAT_IMPORT_LINE_CLASSES, bool WADMIN_DATABASE_CACHE_ENABLED, int WADMIN_WSS_PWA_ADMIN_ROLE_ID, int WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID, int WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID, int WADMIN_WSS_PWA_READER_ROLE_ID, bool WADMIN_STAT_ALLOW_FREEFORM_PERIODS, bool WADMIN_STAT_TIMESHEET_TIED, int WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS, int WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS, int WADMIN_MAX_SQL_BATCH_SIZE, int WADMIN_CORE_SQL_TIMEOUT, int WADMIN_MAX_SSP_BATCH_SIZE, short WADMIN_USER_SYNC_SETTING, string WADMIN_AD_SYNC_REPLACE_CHAR, int WADMIN_SQL_BATCHING_ENABLED, long WADMIN_SQL_BATCHING_BUFFER_SIZE, bool WADMIN_WSS_RESTRICT_WORKSPACE_CREATION, int WADMIN_FULL_SYNC_THRESHOLD, Guid TIMESHEET_CURRENT_VIEWSET_UID, int WADMIN_PERMISSION_MODE, string WADMIN_SPPERMMODE_LAST_SYNC, Guid WADMIN_SITEMAP_CACHE_VERSION, Guid WADMIN_GROUPINGGANTT_CACHE_VERSION, bool WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED, Guid WADMIN_SETTINGS_VERSION, bool WADMIN_IS_UPDATING, int WADMIN_PROVISIONING_RESULT, bool WADMIN_LOGICAL_READONLY, bool WADMIN_OVER_QUOTA, bool WADMIN_IS_DELETED, byte WADMIN_SYNC_TASKS_TO_TASKLIST, bool WADMIN_USE_ENGAGEMENTS, bool WADMIN_IS_NOTIFICATION_ENABLED)
			{
				WebAdminDataSet.WebAdminRow webAdminRow = (WebAdminDataSet.WebAdminRow)base.NewRow();
				object[] itemArray = new object[]
				{
					WADMIN_UIDFAKE,
					WADMIN_AUTHENTICATION_TYPE,
					WADMIN_NEW_ACCOUNT_PRIVILEGE,
					WADMIN_IS_DELEGATION_ALLOWED,
					WADMIN_AUTH_REQUIRED_FOR_PUBLISH,
					WADMIN_WEEK_STARTS_ON,
					WADMIN_MIN_PASSWORD_LENGTH,
					WADMIN_NTFY_FROM_EMAIL,
					WADMIN_NTFY_EMAIL_TRAILER,
					WADMIN_ORG_EMAIL_ADDRESS,
					WADMIN_EMAIL_CHARSET,
					WADMIN_DEFAULT_LANGUAGE,
					WADMIN_DEFAULT_TRACKING_METHOD,
					WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS,
					WADMIN_IS_TRACKING_METHOD_LOCKED,
					WADMIN_TRANS_HISTORY_DAYS,
					WADMIN_TIMESHEET_SPAN,
					WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS,
					WADMIN_MONTHLY_REPORTS_PER_MONTH,
					WADMIN_MONTHLY_1PRD_1ST_START,
					WADMIN_MONTHLY_2PRDS_1ST_START,
					WADMIN_MONTHLY_2PRDS_1ST_END,
					WADMIN_MONTHLY_3PRDS_1ST_START,
					WADMIN_MONTHLY_3PRDS_1ST_END,
					WADMIN_MONTHLY_3PRDS_2ND_END,
					WADMIN_MAX_HOUR_PER_DAY,
					WADMIN_LOOKAHEAD,
					WADMIN_TIMEPERIOD_GRANULARITY,
					WADMIN_LICENSES,
					WADMIN_AUTO_CREATE_SUBWEBS,
					WADMIN_AUTO_ADD_USER_TO_SUBWEB,
					WADMIN_CURRENT_STS_SERVER_UID,
					WADMIN_DEFAULT_SITE_COLLECTION,
					WADMIN_ENABLE_ENTERPRISE,
					WADMIN_DISPLAY_MASTER_IN_ENTERPRISE,
					WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE,
					WADMIN_SERVER_CURRENCY,
					WADMIN_ENFORCE_CURRENCY,
					WADMIN_LAST_STS_ADMIN_SYNCH_TIME,
					WADMIN_SMTP_SERVER_NAME,
					WADMIN_SMTP_SERVER_PORT,
					WADMIN_INTRANET_SERVER_URL,
					WADMIN_EXTRANET_SERVER_URL,
					WADMIN_ONLY_PRO_PUBLISH,
					WADMIN_PROTECT_ACTUALS,
					WADMIN_STS_TEMPLATE_LCID,
					WADMIN_STS_TEMPLATE_ID,
					WADMIN_STS_PRIMARY_OWNER_EMAIL,
					WADMIN_BUILD_TEAM_BY_RBS,
					WADMIN_IS_HOSTED_ORG,
					WADMIN_USE_BASELINE_SUMMARY_DATA,
					WADMIN_PROJECT_BUILD,
					WADMIN_TS_MODE_ENUM,
					WADMIN_TS_IS_UNVERS_TASK_ALLOWED,
					WADMIN_TS_PROJECT_MANAGER_COORDINATION,
					WADMIN_TS_PROJECT_MANAGER_APPROVAL,
					WADMIN_TS_MAXIMUM_LINE_ITEMS,
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
					WADMIN_TS_OUTLOOK_DISPLAY_ENUM,
					WADMIN_TS_CREATE_MODE_ENUM,
					WADMIN_TS_REPORT_UNIT_ENUM,
					WADMIN_WEEK_START_ON_ENUM,
					WADMIN_STAT_MAX_HR_PER_DAY,
					WADMIN_STAT_MAX_HR_PER_TASK,
					WADMIN_STAT_LOOK_AHEAD,
					WADMIN_STAT_LOOK_AHEAD_PERIODS,
					WADMIN_STAT_REP_SCHED_ENUM,
					WADMIN_STAT_SPAN_MODE_ENUM,
					WADMIN_TS_DEF_ENTRY_MODE_ENUM,
					WADMIN_STAT_PROT_ACT,
					WADMIN_STAT_PERIOD_TYPE,
					WADMIN_STAT_ENABLE_DOWNLOAD,
					WADMIN_STAT_NUM_WK_SPANNED,
					WADMIN_STAT_NUM_UPDATES_PER_MONTH,
					WADMIN_STAT_1PRD_1ST_START,
					WADMIN_STAT_2PRD_1ST_START,
					WADMIN_STAT_2PRD_1ST_END,
					WADMIN_STAT_3PRD_1ST_START,
					WADMIN_STAT_3PRD_1ST_END,
					WADMIN_STAT_3PRD_2ND_END,
					WADMIN_LIST_SEPARATOR,
					WADMIN_ACTIVE_CACHE_DIR,
					WADMIN_ACTIVE_CACHE_MAX_SIZE_MB,
					WADMIN_RES_SCHEDULED_TIME,
					WADMIN_RESPLAN_FTE,
					WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND,
					WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD,
					WADMIN_RESOURCE_CAPACITY_JOB_UID,
					WADMIN_REMINDER_TIMER_JOB_UID,
					WADMIN_USE_PROJECT_STATE,
					WADMIN_SHOW_WSS_NAV_LINKS,
					WADMIN_ALWAYS_EXPAND_NAV_LINKS,
					WADMIN_WORKFLOW_PROXY_ACCT,
					WADMIN_WORKFLOW_PROXY_UID,
					WADMIN_WORKFLOW_PROXY_WINDOWS,
					WADMIN_WORKFLOW_PROXY_MOD_BY,
					WADMIN_WORKFLOW_PROXY_MOD_DATE,
					CREATED_DATE,
					MOD_DATE,
					CREATED_REV_COUNTER,
					MOD_REV_COUNTER,
					WADMIN_SERVER_FLAGS,
					WADMIN_MIN_WINPROJ_BUILD_NUMBERS,
					WADMIN_EXCHANGE_INTEGRATION_ENABLED,
					WADMIN_EXCHANGE_URL_REFRESH_JOB_UID,
					WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID,
					WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID,
					WADMIN_PUBLISH_MANUAL_TASKS,
					WADMIN_SERVER_DEFAULT_TASK_MODE,
					WADMIN_LOCK_PRO_DEFAULT_TASK_MODE,
					WADMIN_TS_ALLOW_PROJECT_LEVEL,
					WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED,
					WADMIN_OFF_PEAK_SYNC_THRESHOLD,
					WADMIN_DISABLED_SYNC_THRESHOLD,
					WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID,
					WADMIN_STAT_IMPORT_LINE_CLASSES,
					WADMIN_DATABASE_CACHE_ENABLED,
					WADMIN_WSS_PWA_ADMIN_ROLE_ID,
					WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID,
					WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID,
					WADMIN_WSS_PWA_READER_ROLE_ID,
					WADMIN_STAT_ALLOW_FREEFORM_PERIODS,
					WADMIN_STAT_TIMESHEET_TIED,
					WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS,
					WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS,
					WADMIN_MAX_SQL_BATCH_SIZE,
					WADMIN_CORE_SQL_TIMEOUT,
					WADMIN_MAX_SSP_BATCH_SIZE,
					WADMIN_USER_SYNC_SETTING,
					WADMIN_AD_SYNC_REPLACE_CHAR,
					WADMIN_SQL_BATCHING_ENABLED,
					WADMIN_SQL_BATCHING_BUFFER_SIZE,
					WADMIN_WSS_RESTRICT_WORKSPACE_CREATION,
					WADMIN_FULL_SYNC_THRESHOLD,
					TIMESHEET_CURRENT_VIEWSET_UID,
					WADMIN_PERMISSION_MODE,
					WADMIN_SPPERMMODE_LAST_SYNC,
					WADMIN_SITEMAP_CACHE_VERSION,
					WADMIN_GROUPINGGANTT_CACHE_VERSION,
					WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED,
					WADMIN_SETTINGS_VERSION,
					WADMIN_IS_UPDATING,
					WADMIN_PROVISIONING_RESULT,
					WADMIN_LOGICAL_READONLY,
					WADMIN_OVER_QUOTA,
					WADMIN_IS_DELETED,
					WADMIN_SYNC_TASKS_TO_TASKLIST,
					WADMIN_USE_ENGAGEMENTS,
					WADMIN_IS_NOTIFICATION_ENABLED
				};
				webAdminRow.ItemArray = itemArray;
				base.Rows.Add(webAdminRow);
				return webAdminRow;
			}

			// Token: 0x0600B93F RID: 47423 RVA: 0x00240F08 File Offset: 0x0023F108
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WebAdminDataSet.WebAdminRow FindByWADMIN_UIDFAKE(Guid WADMIN_UIDFAKE)
			{
				return (WebAdminDataSet.WebAdminRow)base.Rows.Find(new object[]
				{
					WADMIN_UIDFAKE
				});
			}

			// Token: 0x0600B940 RID: 47424 RVA: 0x00240F36 File Offset: 0x0023F136
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public virtual IEnumerator GetEnumerator()
			{
				return base.Rows.GetEnumerator();
			}

			// Token: 0x0600B941 RID: 47425 RVA: 0x00240F44 File Offset: 0x0023F144
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public override DataTable Clone()
			{
				WebAdminDataSet.WebAdminDataTable webAdminDataTable = (WebAdminDataSet.WebAdminDataTable)base.Clone();
				webAdminDataTable.InitVars();
				return webAdminDataTable;
			}

			// Token: 0x0600B942 RID: 47426 RVA: 0x00240F64 File Offset: 0x0023F164
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataTable CreateInstance()
			{
				return new WebAdminDataSet.WebAdminDataTable();
			}

			// Token: 0x0600B943 RID: 47427 RVA: 0x00240F6C File Offset: 0x0023F16C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			internal void InitVars()
			{
				this.columnWADMIN_UIDFAKE = base.Columns["WADMIN_UIDFAKE"];
				this.columnWADMIN_AUTHENTICATION_TYPE = base.Columns["WADMIN_AUTHENTICATION_TYPE"];
				this.columnWADMIN_NEW_ACCOUNT_PRIVILEGE = base.Columns["WADMIN_NEW_ACCOUNT_PRIVILEGE"];
				this.columnWADMIN_IS_DELEGATION_ALLOWED = base.Columns["WADMIN_IS_DELEGATION_ALLOWED"];
				this.columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH = base.Columns["WADMIN_AUTH_REQUIRED_FOR_PUBLISH"];
				this.columnWADMIN_WEEK_STARTS_ON = base.Columns["WADMIN_WEEK_STARTS_ON"];
				this.columnWADMIN_MIN_PASSWORD_LENGTH = base.Columns["WADMIN_MIN_PASSWORD_LENGTH"];
				this.columnWADMIN_NTFY_FROM_EMAIL = base.Columns["WADMIN_NTFY_FROM_EMAIL"];
				this.columnWADMIN_NTFY_EMAIL_TRAILER = base.Columns["WADMIN_NTFY_EMAIL_TRAILER"];
				this.columnWADMIN_ORG_EMAIL_ADDRESS = base.Columns["WADMIN_ORG_EMAIL_ADDRESS"];
				this.columnWADMIN_EMAIL_CHARSET = base.Columns["WADMIN_EMAIL_CHARSET"];
				this.columnWADMIN_DEFAULT_LANGUAGE = base.Columns["WADMIN_DEFAULT_LANGUAGE"];
				this.columnWADMIN_DEFAULT_TRACKING_METHOD = base.Columns["WADMIN_DEFAULT_TRACKING_METHOD"];
				this.columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS = base.Columns["WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS"];
				this.columnWADMIN_IS_TRACKING_METHOD_LOCKED = base.Columns["WADMIN_IS_TRACKING_METHOD_LOCKED"];
				this.columnWADMIN_TRANS_HISTORY_DAYS = base.Columns["WADMIN_TRANS_HISTORY_DAYS"];
				this.columnWADMIN_TIMESHEET_SPAN = base.Columns["WADMIN_TIMESHEET_SPAN"];
				this.columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS = base.Columns["WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS"];
				this.columnWADMIN_MONTHLY_REPORTS_PER_MONTH = base.Columns["WADMIN_MONTHLY_REPORTS_PER_MONTH"];
				this.columnWADMIN_MONTHLY_1PRD_1ST_START = base.Columns["WADMIN_MONTHLY_1PRD_1ST_START"];
				this.columnWADMIN_MONTHLY_2PRDS_1ST_START = base.Columns["WADMIN_MONTHLY_2PRDS_1ST_START"];
				this.columnWADMIN_MONTHLY_2PRDS_1ST_END = base.Columns["WADMIN_MONTHLY_2PRDS_1ST_END"];
				this.columnWADMIN_MONTHLY_3PRDS_1ST_START = base.Columns["WADMIN_MONTHLY_3PRDS_1ST_START"];
				this.columnWADMIN_MONTHLY_3PRDS_1ST_END = base.Columns["WADMIN_MONTHLY_3PRDS_1ST_END"];
				this.columnWADMIN_MONTHLY_3PRDS_2ND_END = base.Columns["WADMIN_MONTHLY_3PRDS_2ND_END"];
				this.columnWADMIN_MAX_HOUR_PER_DAY = base.Columns["WADMIN_MAX_HOUR_PER_DAY"];
				this.columnWADMIN_LOOKAHEAD = base.Columns["WADMIN_LOOKAHEAD"];
				this.columnWADMIN_TIMEPERIOD_GRANULARITY = base.Columns["WADMIN_TIMEPERIOD_GRANULARITY"];
				this.columnWADMIN_LICENSES = base.Columns["WADMIN_LICENSES"];
				this.columnWADMIN_AUTO_CREATE_SUBWEBS = base.Columns["WADMIN_AUTO_CREATE_SUBWEBS"];
				this.columnWADMIN_AUTO_ADD_USER_TO_SUBWEB = base.Columns["WADMIN_AUTO_ADD_USER_TO_SUBWEB"];
				this.columnWADMIN_CURRENT_STS_SERVER_UID = base.Columns["WADMIN_CURRENT_STS_SERVER_UID"];
				this.columnWADMIN_DEFAULT_SITE_COLLECTION = base.Columns["WADMIN_DEFAULT_SITE_COLLECTION"];
				this.columnWADMIN_ENABLE_ENTERPRISE = base.Columns["WADMIN_ENABLE_ENTERPRISE"];
				this.columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE = base.Columns["WADMIN_DISPLAY_MASTER_IN_ENTERPRISE"];
				this.columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE = base.Columns["WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE"];
				this.columnWADMIN_SERVER_CURRENCY = base.Columns["WADMIN_SERVER_CURRENCY"];
				this.columnWADMIN_ENFORCE_CURRENCY = base.Columns["WADMIN_ENFORCE_CURRENCY"];
				this.columnWADMIN_LAST_STS_ADMIN_SYNCH_TIME = base.Columns["WADMIN_LAST_STS_ADMIN_SYNCH_TIME"];
				this.columnWADMIN_SMTP_SERVER_NAME = base.Columns["WADMIN_SMTP_SERVER_NAME"];
				this.columnWADMIN_SMTP_SERVER_PORT = base.Columns["WADMIN_SMTP_SERVER_PORT"];
				this.columnWADMIN_INTRANET_SERVER_URL = base.Columns["WADMIN_INTRANET_SERVER_URL"];
				this.columnWADMIN_EXTRANET_SERVER_URL = base.Columns["WADMIN_EXTRANET_SERVER_URL"];
				this.columnWADMIN_ONLY_PRO_PUBLISH = base.Columns["WADMIN_ONLY_PRO_PUBLISH"];
				this.columnWADMIN_PROTECT_ACTUALS = base.Columns["WADMIN_PROTECT_ACTUALS"];
				this.columnWADMIN_STS_TEMPLATE_LCID = base.Columns["WADMIN_STS_TEMPLATE_LCID"];
				this.columnWADMIN_STS_TEMPLATE_ID = base.Columns["WADMIN_STS_TEMPLATE_ID"];
				this.columnWADMIN_STS_PRIMARY_OWNER_EMAIL = base.Columns["WADMIN_STS_PRIMARY_OWNER_EMAIL"];
				this.columnWADMIN_BUILD_TEAM_BY_RBS = base.Columns["WADMIN_BUILD_TEAM_BY_RBS"];
				this.columnWADMIN_IS_HOSTED_ORG = base.Columns["WADMIN_IS_HOSTED_ORG"];
				this.columnWADMIN_USE_BASELINE_SUMMARY_DATA = base.Columns["WADMIN_USE_BASELINE_SUMMARY_DATA"];
				this.columnWADMIN_PROJECT_BUILD = base.Columns["WADMIN_PROJECT_BUILD"];
				this.columnWADMIN_TS_MODE_ENUM = base.Columns["WADMIN_TS_MODE_ENUM"];
				this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED = base.Columns["WADMIN_TS_IS_UNVERS_TASK_ALLOWED"];
				this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION = base.Columns["WADMIN_TS_PROJECT_MANAGER_COORDINATION"];
				this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL = base.Columns["WADMIN_TS_PROJECT_MANAGER_APPROVAL"];
				this.columnWADMIN_TS_MAXIMUM_LINE_ITEMS = base.Columns["WADMIN_TS_MAXIMUM_LINE_ITEMS"];
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
				this.columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM = base.Columns["WADMIN_TS_OUTLOOK_DISPLAY_ENUM"];
				this.columnWADMIN_TS_CREATE_MODE_ENUM = base.Columns["WADMIN_TS_CREATE_MODE_ENUM"];
				this.columnWADMIN_TS_REPORT_UNIT_ENUM = base.Columns["WADMIN_TS_REPORT_UNIT_ENUM"];
				this.columnWADMIN_WEEK_START_ON_ENUM = base.Columns["WADMIN_WEEK_START_ON_ENUM"];
				this.columnWADMIN_STAT_MAX_HR_PER_DAY = base.Columns["WADMIN_STAT_MAX_HR_PER_DAY"];
				this.columnWADMIN_STAT_MAX_HR_PER_TASK = base.Columns["WADMIN_STAT_MAX_HR_PER_TASK"];
				this.columnWADMIN_STAT_LOOK_AHEAD = base.Columns["WADMIN_STAT_LOOK_AHEAD"];
				this.columnWADMIN_STAT_LOOK_AHEAD_PERIODS = base.Columns["WADMIN_STAT_LOOK_AHEAD_PERIODS"];
				this.columnWADMIN_STAT_REP_SCHED_ENUM = base.Columns["WADMIN_STAT_REP_SCHED_ENUM"];
				this.columnWADMIN_STAT_SPAN_MODE_ENUM = base.Columns["WADMIN_STAT_SPAN_MODE_ENUM"];
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM = base.Columns["WADMIN_TS_DEF_ENTRY_MODE_ENUM"];
				this.columnWADMIN_STAT_PROT_ACT = base.Columns["WADMIN_STAT_PROT_ACT"];
				this.columnWADMIN_STAT_PERIOD_TYPE = base.Columns["WADMIN_STAT_PERIOD_TYPE"];
				this.columnWADMIN_STAT_ENABLE_DOWNLOAD = base.Columns["WADMIN_STAT_ENABLE_DOWNLOAD"];
				this.columnWADMIN_STAT_NUM_WK_SPANNED = base.Columns["WADMIN_STAT_NUM_WK_SPANNED"];
				this.columnWADMIN_STAT_NUM_UPDATES_PER_MONTH = base.Columns["WADMIN_STAT_NUM_UPDATES_PER_MONTH"];
				this.columnWADMIN_STAT_1PRD_1ST_START = base.Columns["WADMIN_STAT_1PRD_1ST_START"];
				this.columnWADMIN_STAT_2PRD_1ST_START = base.Columns["WADMIN_STAT_2PRD_1ST_START"];
				this.columnWADMIN_STAT_2PRD_1ST_END = base.Columns["WADMIN_STAT_2PRD_1ST_END"];
				this.columnWADMIN_STAT_3PRD_1ST_START = base.Columns["WADMIN_STAT_3PRD_1ST_START"];
				this.columnWADMIN_STAT_3PRD_1ST_END = base.Columns["WADMIN_STAT_3PRD_1ST_END"];
				this.columnWADMIN_STAT_3PRD_2ND_END = base.Columns["WADMIN_STAT_3PRD_2ND_END"];
				this.columnWADMIN_LIST_SEPARATOR = base.Columns["WADMIN_LIST_SEPARATOR"];
				this.columnWADMIN_ACTIVE_CACHE_DIR = base.Columns["WADMIN_ACTIVE_CACHE_DIR"];
				this.columnWADMIN_ACTIVE_CACHE_MAX_SIZE_MB = base.Columns["WADMIN_ACTIVE_CACHE_MAX_SIZE_MB"];
				this.columnWADMIN_RES_SCHEDULED_TIME = base.Columns["WADMIN_RES_SCHEDULED_TIME"];
				this.columnWADMIN_RESPLAN_FTE = base.Columns["WADMIN_RESPLAN_FTE"];
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND = base.Columns["WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND"];
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD = base.Columns["WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD"];
				this.columnWADMIN_RESOURCE_CAPACITY_JOB_UID = base.Columns["WADMIN_RESOURCE_CAPACITY_JOB_UID"];
				this.columnWADMIN_REMINDER_TIMER_JOB_UID = base.Columns["WADMIN_REMINDER_TIMER_JOB_UID"];
				this.columnWADMIN_USE_PROJECT_STATE = base.Columns["WADMIN_USE_PROJECT_STATE"];
				this.columnWADMIN_SHOW_WSS_NAV_LINKS = base.Columns["WADMIN_SHOW_WSS_NAV_LINKS"];
				this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS = base.Columns["WADMIN_ALWAYS_EXPAND_NAV_LINKS"];
				this.columnWADMIN_WORKFLOW_PROXY_ACCT = base.Columns["WADMIN_WORKFLOW_PROXY_ACCT"];
				this.columnWADMIN_WORKFLOW_PROXY_UID = base.Columns["WADMIN_WORKFLOW_PROXY_UID"];
				this.columnWADMIN_WORKFLOW_PROXY_WINDOWS = base.Columns["WADMIN_WORKFLOW_PROXY_WINDOWS"];
				this.columnWADMIN_WORKFLOW_PROXY_MOD_BY = base.Columns["WADMIN_WORKFLOW_PROXY_MOD_BY"];
				this.columnWADMIN_WORKFLOW_PROXY_MOD_DATE = base.Columns["WADMIN_WORKFLOW_PROXY_MOD_DATE"];
				this.columnCREATED_DATE = base.Columns["CREATED_DATE"];
				this.columnMOD_DATE = base.Columns["MOD_DATE"];
				this.columnCREATED_REV_COUNTER = base.Columns["CREATED_REV_COUNTER"];
				this.columnMOD_REV_COUNTER = base.Columns["MOD_REV_COUNTER"];
				this.columnWADMIN_SERVER_FLAGS = base.Columns["WADMIN_SERVER_FLAGS"];
				this.columnWADMIN_MIN_WINPROJ_BUILD_NUMBERS = base.Columns["WADMIN_MIN_WINPROJ_BUILD_NUMBERS"];
				this.columnWADMIN_EXCHANGE_INTEGRATION_ENABLED = base.Columns["WADMIN_EXCHANGE_INTEGRATION_ENABLED"];
				this.columnWADMIN_EXCHANGE_URL_REFRESH_JOB_UID = base.Columns["WADMIN_EXCHANGE_URL_REFRESH_JOB_UID"];
				this.columnWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID = base.Columns["WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID"];
				this.columnWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID = base.Columns["WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID"];
				this.columnWADMIN_PUBLISH_MANUAL_TASKS = base.Columns["WADMIN_PUBLISH_MANUAL_TASKS"];
				this.columnWADMIN_SERVER_DEFAULT_TASK_MODE = base.Columns["WADMIN_SERVER_DEFAULT_TASK_MODE"];
				this.columnWADMIN_LOCK_PRO_DEFAULT_TASK_MODE = base.Columns["WADMIN_LOCK_PRO_DEFAULT_TASK_MODE"];
				this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL = base.Columns["WADMIN_TS_ALLOW_PROJECT_LEVEL"];
				this.columnWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED = base.Columns["WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED"];
				this.columnWADMIN_OFF_PEAK_SYNC_THRESHOLD = base.Columns["WADMIN_OFF_PEAK_SYNC_THRESHOLD"];
				this.columnWADMIN_DISABLED_SYNC_THRESHOLD = base.Columns["WADMIN_DISABLED_SYNC_THRESHOLD"];
				this.columnWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID = base.Columns["WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID"];
				this.columnWADMIN_STAT_IMPORT_LINE_CLASSES = base.Columns["WADMIN_STAT_IMPORT_LINE_CLASSES"];
				this.columnWADMIN_DATABASE_CACHE_ENABLED = base.Columns["WADMIN_DATABASE_CACHE_ENABLED"];
				this.columnWADMIN_WSS_PWA_ADMIN_ROLE_ID = base.Columns["WADMIN_WSS_PWA_ADMIN_ROLE_ID"];
				this.columnWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID = base.Columns["WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID"];
				this.columnWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID = base.Columns["WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID"];
				this.columnWADMIN_WSS_PWA_READER_ROLE_ID = base.Columns["WADMIN_WSS_PWA_READER_ROLE_ID"];
				this.columnWADMIN_STAT_ALLOW_FREEFORM_PERIODS = base.Columns["WADMIN_STAT_ALLOW_FREEFORM_PERIODS"];
				this.columnWADMIN_STAT_TIMESHEET_TIED = base.Columns["WADMIN_STAT_TIMESHEET_TIED"];
				this.columnWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS = base.Columns["WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS"];
				this.columnWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS = base.Columns["WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS"];
				this.columnWADMIN_MAX_SQL_BATCH_SIZE = base.Columns["WADMIN_MAX_SQL_BATCH_SIZE"];
				this.columnWADMIN_CORE_SQL_TIMEOUT = base.Columns["WADMIN_CORE_SQL_TIMEOUT"];
				this.columnWADMIN_MAX_SSP_BATCH_SIZE = base.Columns["WADMIN_MAX_SSP_BATCH_SIZE"];
				this.columnWADMIN_USER_SYNC_SETTING = base.Columns["WADMIN_USER_SYNC_SETTING"];
				this.columnWADMIN_AD_SYNC_REPLACE_CHAR = base.Columns["WADMIN_AD_SYNC_REPLACE_CHAR"];
				this.columnWADMIN_SQL_BATCHING_ENABLED = base.Columns["WADMIN_SQL_BATCHING_ENABLED"];
				this.columnWADMIN_SQL_BATCHING_BUFFER_SIZE = base.Columns["WADMIN_SQL_BATCHING_BUFFER_SIZE"];
				this.columnWADMIN_WSS_RESTRICT_WORKSPACE_CREATION = base.Columns["WADMIN_WSS_RESTRICT_WORKSPACE_CREATION"];
				this.columnWADMIN_FULL_SYNC_THRESHOLD = base.Columns["WADMIN_FULL_SYNC_THRESHOLD"];
				this.columnTIMESHEET_CURRENT_VIEWSET_UID = base.Columns["TIMESHEET_CURRENT_VIEWSET_UID"];
				this.columnWADMIN_PERMISSION_MODE = base.Columns["WADMIN_PERMISSION_MODE"];
				this.columnWADMIN_SPPERMMODE_LAST_SYNC = base.Columns["WADMIN_SPPERMMODE_LAST_SYNC"];
				this.columnWADMIN_SITEMAP_CACHE_VERSION = base.Columns["WADMIN_SITEMAP_CACHE_VERSION"];
				this.columnWADMIN_GROUPINGGANTT_CACHE_VERSION = base.Columns["WADMIN_GROUPINGGANTT_CACHE_VERSION"];
				this.columnWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED = base.Columns["WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED"];
				this.columnWADMIN_SETTINGS_VERSION = base.Columns["WADMIN_SETTINGS_VERSION"];
				this.columnWADMIN_IS_UPDATING = base.Columns["WADMIN_IS_UPDATING"];
				this.columnWADMIN_PROVISIONING_RESULT = base.Columns["WADMIN_PROVISIONING_RESULT"];
				this.columnWADMIN_LOGICAL_READONLY = base.Columns["WADMIN_LOGICAL_READONLY"];
				this.columnWADMIN_OVER_QUOTA = base.Columns["WADMIN_OVER_QUOTA"];
				this.columnWADMIN_IS_DELETED = base.Columns["WADMIN_IS_DELETED"];
				this.columnWADMIN_SYNC_TASKS_TO_TASKLIST = base.Columns["WADMIN_SYNC_TASKS_TO_TASKLIST"];
				this.columnWADMIN_USE_ENGAGEMENTS = base.Columns["WADMIN_USE_ENGAGEMENTS"];
				this.columnWADMIN_IS_NOTIFICATION_ENABLED = base.Columns["WADMIN_IS_NOTIFICATION_ENABLED"];
			}

			// Token: 0x0600B944 RID: 47428 RVA: 0x00241D10 File Offset: 0x0023FF10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			private void InitClass()
			{
				this.columnWADMIN_UIDFAKE = new DataColumn("WADMIN_UIDFAKE", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_UIDFAKE);
				this.columnWADMIN_AUTHENTICATION_TYPE = new DataColumn("WADMIN_AUTHENTICATION_TYPE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_AUTHENTICATION_TYPE);
				this.columnWADMIN_NEW_ACCOUNT_PRIVILEGE = new DataColumn("WADMIN_NEW_ACCOUNT_PRIVILEGE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_NEW_ACCOUNT_PRIVILEGE);
				this.columnWADMIN_IS_DELEGATION_ALLOWED = new DataColumn("WADMIN_IS_DELEGATION_ALLOWED", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_DELEGATION_ALLOWED);
				this.columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH = new DataColumn("WADMIN_AUTH_REQUIRED_FOR_PUBLISH", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH);
				this.columnWADMIN_WEEK_STARTS_ON = new DataColumn("WADMIN_WEEK_STARTS_ON", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WEEK_STARTS_ON);
				this.columnWADMIN_MIN_PASSWORD_LENGTH = new DataColumn("WADMIN_MIN_PASSWORD_LENGTH", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MIN_PASSWORD_LENGTH);
				this.columnWADMIN_NTFY_FROM_EMAIL = new DataColumn("WADMIN_NTFY_FROM_EMAIL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_NTFY_FROM_EMAIL);
				this.columnWADMIN_NTFY_EMAIL_TRAILER = new DataColumn("WADMIN_NTFY_EMAIL_TRAILER", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_NTFY_EMAIL_TRAILER);
				this.columnWADMIN_ORG_EMAIL_ADDRESS = new DataColumn("WADMIN_ORG_EMAIL_ADDRESS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ORG_EMAIL_ADDRESS);
				this.columnWADMIN_EMAIL_CHARSET = new DataColumn("WADMIN_EMAIL_CHARSET", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EMAIL_CHARSET);
				this.columnWADMIN_DEFAULT_LANGUAGE = new DataColumn("WADMIN_DEFAULT_LANGUAGE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DEFAULT_LANGUAGE);
				this.columnWADMIN_DEFAULT_TRACKING_METHOD = new DataColumn("WADMIN_DEFAULT_TRACKING_METHOD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DEFAULT_TRACKING_METHOD);
				this.columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS = new DataColumn("WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS);
				this.columnWADMIN_IS_TRACKING_METHOD_LOCKED = new DataColumn("WADMIN_IS_TRACKING_METHOD_LOCKED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_TRACKING_METHOD_LOCKED);
				this.columnWADMIN_TRANS_HISTORY_DAYS = new DataColumn("WADMIN_TRANS_HISTORY_DAYS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TRANS_HISTORY_DAYS);
				this.columnWADMIN_TIMESHEET_SPAN = new DataColumn("WADMIN_TIMESHEET_SPAN", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TIMESHEET_SPAN);
				this.columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS = new DataColumn("WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS);
				this.columnWADMIN_MONTHLY_REPORTS_PER_MONTH = new DataColumn("WADMIN_MONTHLY_REPORTS_PER_MONTH", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_REPORTS_PER_MONTH);
				this.columnWADMIN_MONTHLY_1PRD_1ST_START = new DataColumn("WADMIN_MONTHLY_1PRD_1ST_START", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_1PRD_1ST_START);
				this.columnWADMIN_MONTHLY_2PRDS_1ST_START = new DataColumn("WADMIN_MONTHLY_2PRDS_1ST_START", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_2PRDS_1ST_START);
				this.columnWADMIN_MONTHLY_2PRDS_1ST_END = new DataColumn("WADMIN_MONTHLY_2PRDS_1ST_END", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_2PRDS_1ST_END);
				this.columnWADMIN_MONTHLY_3PRDS_1ST_START = new DataColumn("WADMIN_MONTHLY_3PRDS_1ST_START", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_3PRDS_1ST_START);
				this.columnWADMIN_MONTHLY_3PRDS_1ST_END = new DataColumn("WADMIN_MONTHLY_3PRDS_1ST_END", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_3PRDS_1ST_END);
				this.columnWADMIN_MONTHLY_3PRDS_2ND_END = new DataColumn("WADMIN_MONTHLY_3PRDS_2ND_END", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MONTHLY_3PRDS_2ND_END);
				this.columnWADMIN_MAX_HOUR_PER_DAY = new DataColumn("WADMIN_MAX_HOUR_PER_DAY", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MAX_HOUR_PER_DAY);
				this.columnWADMIN_LOOKAHEAD = new DataColumn("WADMIN_LOOKAHEAD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LOOKAHEAD);
				this.columnWADMIN_TIMEPERIOD_GRANULARITY = new DataColumn("WADMIN_TIMEPERIOD_GRANULARITY", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TIMEPERIOD_GRANULARITY);
				this.columnWADMIN_LICENSES = new DataColumn("WADMIN_LICENSES", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LICENSES);
				this.columnWADMIN_AUTO_CREATE_SUBWEBS = new DataColumn("WADMIN_AUTO_CREATE_SUBWEBS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_AUTO_CREATE_SUBWEBS);
				this.columnWADMIN_AUTO_ADD_USER_TO_SUBWEB = new DataColumn("WADMIN_AUTO_ADD_USER_TO_SUBWEB", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_AUTO_ADD_USER_TO_SUBWEB);
				this.columnWADMIN_CURRENT_STS_SERVER_UID = new DataColumn("WADMIN_CURRENT_STS_SERVER_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_CURRENT_STS_SERVER_UID);
				this.columnWADMIN_DEFAULT_SITE_COLLECTION = new DataColumn("WADMIN_DEFAULT_SITE_COLLECTION", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DEFAULT_SITE_COLLECTION);
				this.columnWADMIN_ENABLE_ENTERPRISE = new DataColumn("WADMIN_ENABLE_ENTERPRISE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ENABLE_ENTERPRISE);
				this.columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE = new DataColumn("WADMIN_DISPLAY_MASTER_IN_ENTERPRISE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE);
				this.columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE = new DataColumn("WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE);
				this.columnWADMIN_SERVER_CURRENCY = new DataColumn("WADMIN_SERVER_CURRENCY", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SERVER_CURRENCY);
				this.columnWADMIN_ENFORCE_CURRENCY = new DataColumn("WADMIN_ENFORCE_CURRENCY", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ENFORCE_CURRENCY);
				this.columnWADMIN_LAST_STS_ADMIN_SYNCH_TIME = new DataColumn("WADMIN_LAST_STS_ADMIN_SYNCH_TIME", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LAST_STS_ADMIN_SYNCH_TIME);
				this.columnWADMIN_SMTP_SERVER_NAME = new DataColumn("WADMIN_SMTP_SERVER_NAME", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SMTP_SERVER_NAME);
				this.columnWADMIN_SMTP_SERVER_PORT = new DataColumn("WADMIN_SMTP_SERVER_PORT", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SMTP_SERVER_PORT);
				this.columnWADMIN_INTRANET_SERVER_URL = new DataColumn("WADMIN_INTRANET_SERVER_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_INTRANET_SERVER_URL);
				this.columnWADMIN_EXTRANET_SERVER_URL = new DataColumn("WADMIN_EXTRANET_SERVER_URL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXTRANET_SERVER_URL);
				this.columnWADMIN_ONLY_PRO_PUBLISH = new DataColumn("WADMIN_ONLY_PRO_PUBLISH", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ONLY_PRO_PUBLISH);
				this.columnWADMIN_PROTECT_ACTUALS = new DataColumn("WADMIN_PROTECT_ACTUALS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PROTECT_ACTUALS);
				this.columnWADMIN_STS_TEMPLATE_LCID = new DataColumn("WADMIN_STS_TEMPLATE_LCID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STS_TEMPLATE_LCID);
				this.columnWADMIN_STS_TEMPLATE_ID = new DataColumn("WADMIN_STS_TEMPLATE_ID", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STS_TEMPLATE_ID);
				this.columnWADMIN_STS_PRIMARY_OWNER_EMAIL = new DataColumn("WADMIN_STS_PRIMARY_OWNER_EMAIL", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STS_PRIMARY_OWNER_EMAIL);
				this.columnWADMIN_BUILD_TEAM_BY_RBS = new DataColumn("WADMIN_BUILD_TEAM_BY_RBS", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_BUILD_TEAM_BY_RBS);
				this.columnWADMIN_IS_HOSTED_ORG = new DataColumn("WADMIN_IS_HOSTED_ORG", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_HOSTED_ORG);
				this.columnWADMIN_USE_BASELINE_SUMMARY_DATA = new DataColumn("WADMIN_USE_BASELINE_SUMMARY_DATA", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_USE_BASELINE_SUMMARY_DATA);
				this.columnWADMIN_PROJECT_BUILD = new DataColumn("WADMIN_PROJECT_BUILD", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PROJECT_BUILD);
				this.columnWADMIN_TS_MODE_ENUM = new DataColumn("WADMIN_TS_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_MODE_ENUM);
				this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED = new DataColumn("WADMIN_TS_IS_UNVERS_TASK_ALLOWED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED);
				this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION = new DataColumn("WADMIN_TS_PROJECT_MANAGER_COORDINATION", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_PROJECT_MANAGER_COORDINATION);
				this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL = new DataColumn("WADMIN_TS_PROJECT_MANAGER_APPROVAL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_PROJECT_MANAGER_APPROVAL);
				this.columnWADMIN_TS_MAXIMUM_LINE_ITEMS = new DataColumn("WADMIN_TS_MAXIMUM_LINE_ITEMS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_MAXIMUM_LINE_ITEMS);
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
				this.columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM = new DataColumn("WADMIN_TS_OUTLOOK_DISPLAY_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM);
				this.columnWADMIN_TS_CREATE_MODE_ENUM = new DataColumn("WADMIN_TS_CREATE_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_CREATE_MODE_ENUM);
				this.columnWADMIN_TS_REPORT_UNIT_ENUM = new DataColumn("WADMIN_TS_REPORT_UNIT_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_REPORT_UNIT_ENUM);
				this.columnWADMIN_WEEK_START_ON_ENUM = new DataColumn("WADMIN_WEEK_START_ON_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WEEK_START_ON_ENUM);
				this.columnWADMIN_STAT_MAX_HR_PER_DAY = new DataColumn("WADMIN_STAT_MAX_HR_PER_DAY", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_MAX_HR_PER_DAY);
				this.columnWADMIN_STAT_MAX_HR_PER_TASK = new DataColumn("WADMIN_STAT_MAX_HR_PER_TASK", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_MAX_HR_PER_TASK);
				this.columnWADMIN_STAT_LOOK_AHEAD = new DataColumn("WADMIN_STAT_LOOK_AHEAD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_LOOK_AHEAD);
				this.columnWADMIN_STAT_LOOK_AHEAD_PERIODS = new DataColumn("WADMIN_STAT_LOOK_AHEAD_PERIODS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_LOOK_AHEAD_PERIODS);
				this.columnWADMIN_STAT_REP_SCHED_ENUM = new DataColumn("WADMIN_STAT_REP_SCHED_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_REP_SCHED_ENUM);
				this.columnWADMIN_STAT_SPAN_MODE_ENUM = new DataColumn("WADMIN_STAT_SPAN_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_SPAN_MODE_ENUM);
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM = new DataColumn("WADMIN_TS_DEF_ENTRY_MODE_ENUM", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM);
				this.columnWADMIN_STAT_PROT_ACT = new DataColumn("WADMIN_STAT_PROT_ACT", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_PROT_ACT);
				this.columnWADMIN_STAT_PERIOD_TYPE = new DataColumn("WADMIN_STAT_PERIOD_TYPE", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_PERIOD_TYPE);
				this.columnWADMIN_STAT_ENABLE_DOWNLOAD = new DataColumn("WADMIN_STAT_ENABLE_DOWNLOAD", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_ENABLE_DOWNLOAD);
				this.columnWADMIN_STAT_NUM_WK_SPANNED = new DataColumn("WADMIN_STAT_NUM_WK_SPANNED", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_NUM_WK_SPANNED);
				this.columnWADMIN_STAT_NUM_UPDATES_PER_MONTH = new DataColumn("WADMIN_STAT_NUM_UPDATES_PER_MONTH", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_NUM_UPDATES_PER_MONTH);
				this.columnWADMIN_STAT_1PRD_1ST_START = new DataColumn("WADMIN_STAT_1PRD_1ST_START", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_1PRD_1ST_START);
				this.columnWADMIN_STAT_2PRD_1ST_START = new DataColumn("WADMIN_STAT_2PRD_1ST_START", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_2PRD_1ST_START);
				this.columnWADMIN_STAT_2PRD_1ST_END = new DataColumn("WADMIN_STAT_2PRD_1ST_END", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_2PRD_1ST_END);
				this.columnWADMIN_STAT_3PRD_1ST_START = new DataColumn("WADMIN_STAT_3PRD_1ST_START", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_3PRD_1ST_START);
				this.columnWADMIN_STAT_3PRD_1ST_END = new DataColumn("WADMIN_STAT_3PRD_1ST_END", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_3PRD_1ST_END);
				this.columnWADMIN_STAT_3PRD_2ND_END = new DataColumn("WADMIN_STAT_3PRD_2ND_END", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_3PRD_2ND_END);
				this.columnWADMIN_LIST_SEPARATOR = new DataColumn("WADMIN_LIST_SEPARATOR", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LIST_SEPARATOR);
				this.columnWADMIN_ACTIVE_CACHE_DIR = new DataColumn("WADMIN_ACTIVE_CACHE_DIR", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ACTIVE_CACHE_DIR);
				this.columnWADMIN_ACTIVE_CACHE_MAX_SIZE_MB = new DataColumn("WADMIN_ACTIVE_CACHE_MAX_SIZE_MB", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ACTIVE_CACHE_MAX_SIZE_MB);
				this.columnWADMIN_RES_SCHEDULED_TIME = new DataColumn("WADMIN_RES_SCHEDULED_TIME", typeof(long), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_RES_SCHEDULED_TIME);
				this.columnWADMIN_RESPLAN_FTE = new DataColumn("WADMIN_RESPLAN_FTE", typeof(decimal), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_RESPLAN_FTE);
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND = new DataColumn("WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND);
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD = new DataColumn("WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD);
				this.columnWADMIN_RESOURCE_CAPACITY_JOB_UID = new DataColumn("WADMIN_RESOURCE_CAPACITY_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_RESOURCE_CAPACITY_JOB_UID);
				this.columnWADMIN_REMINDER_TIMER_JOB_UID = new DataColumn("WADMIN_REMINDER_TIMER_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_REMINDER_TIMER_JOB_UID);
				this.columnWADMIN_USE_PROJECT_STATE = new DataColumn("WADMIN_USE_PROJECT_STATE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_USE_PROJECT_STATE);
				this.columnWADMIN_SHOW_WSS_NAV_LINKS = new DataColumn("WADMIN_SHOW_WSS_NAV_LINKS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SHOW_WSS_NAV_LINKS);
				this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS = new DataColumn("WADMIN_ALWAYS_EXPAND_NAV_LINKS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_ALWAYS_EXPAND_NAV_LINKS);
				this.columnWADMIN_WORKFLOW_PROXY_ACCT = new DataColumn("WADMIN_WORKFLOW_PROXY_ACCT", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WORKFLOW_PROXY_ACCT);
				this.columnWADMIN_WORKFLOW_PROXY_UID = new DataColumn("WADMIN_WORKFLOW_PROXY_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WORKFLOW_PROXY_UID);
				this.columnWADMIN_WORKFLOW_PROXY_WINDOWS = new DataColumn("WADMIN_WORKFLOW_PROXY_WINDOWS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WORKFLOW_PROXY_WINDOWS);
				this.columnWADMIN_WORKFLOW_PROXY_MOD_BY = new DataColumn("WADMIN_WORKFLOW_PROXY_MOD_BY", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WORKFLOW_PROXY_MOD_BY);
				this.columnWADMIN_WORKFLOW_PROXY_MOD_DATE = new DataColumn("WADMIN_WORKFLOW_PROXY_MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WORKFLOW_PROXY_MOD_DATE);
				this.columnCREATED_DATE = new DataColumn("CREATED_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_DATE);
				this.columnMOD_DATE = new DataColumn("MOD_DATE", typeof(DateTime), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_DATE);
				this.columnCREATED_REV_COUNTER = new DataColumn("CREATED_REV_COUNTER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnCREATED_REV_COUNTER);
				this.columnMOD_REV_COUNTER = new DataColumn("MOD_REV_COUNTER", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnMOD_REV_COUNTER);
				this.columnWADMIN_SERVER_FLAGS = new DataColumn("WADMIN_SERVER_FLAGS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SERVER_FLAGS);
				this.columnWADMIN_MIN_WINPROJ_BUILD_NUMBERS = new DataColumn("WADMIN_MIN_WINPROJ_BUILD_NUMBERS", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MIN_WINPROJ_BUILD_NUMBERS);
				this.columnWADMIN_EXCHANGE_INTEGRATION_ENABLED = new DataColumn("WADMIN_EXCHANGE_INTEGRATION_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXCHANGE_INTEGRATION_ENABLED);
				this.columnWADMIN_EXCHANGE_URL_REFRESH_JOB_UID = new DataColumn("WADMIN_EXCHANGE_URL_REFRESH_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXCHANGE_URL_REFRESH_JOB_UID);
				this.columnWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID = new DataColumn("WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID);
				this.columnWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID = new DataColumn("WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID);
				this.columnWADMIN_PUBLISH_MANUAL_TASKS = new DataColumn("WADMIN_PUBLISH_MANUAL_TASKS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PUBLISH_MANUAL_TASKS);
				this.columnWADMIN_SERVER_DEFAULT_TASK_MODE = new DataColumn("WADMIN_SERVER_DEFAULT_TASK_MODE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SERVER_DEFAULT_TASK_MODE);
				this.columnWADMIN_LOCK_PRO_DEFAULT_TASK_MODE = new DataColumn("WADMIN_LOCK_PRO_DEFAULT_TASK_MODE", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LOCK_PRO_DEFAULT_TASK_MODE);
				this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL = new DataColumn("WADMIN_TS_ALLOW_PROJECT_LEVEL", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_TS_ALLOW_PROJECT_LEVEL);
				this.columnWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED = new DataColumn("WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED);
				this.columnWADMIN_OFF_PEAK_SYNC_THRESHOLD = new DataColumn("WADMIN_OFF_PEAK_SYNC_THRESHOLD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_OFF_PEAK_SYNC_THRESHOLD);
				this.columnWADMIN_DISABLED_SYNC_THRESHOLD = new DataColumn("WADMIN_DISABLED_SYNC_THRESHOLD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DISABLED_SYNC_THRESHOLD);
				this.columnWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID = new DataColumn("WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID);
				this.columnWADMIN_STAT_IMPORT_LINE_CLASSES = new DataColumn("WADMIN_STAT_IMPORT_LINE_CLASSES", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_IMPORT_LINE_CLASSES);
				this.columnWADMIN_DATABASE_CACHE_ENABLED = new DataColumn("WADMIN_DATABASE_CACHE_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_DATABASE_CACHE_ENABLED);
				this.columnWADMIN_WSS_PWA_ADMIN_ROLE_ID = new DataColumn("WADMIN_WSS_PWA_ADMIN_ROLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WSS_PWA_ADMIN_ROLE_ID);
				this.columnWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID = new DataColumn("WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID);
				this.columnWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID = new DataColumn("WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID);
				this.columnWADMIN_WSS_PWA_READER_ROLE_ID = new DataColumn("WADMIN_WSS_PWA_READER_ROLE_ID", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WSS_PWA_READER_ROLE_ID);
				this.columnWADMIN_STAT_ALLOW_FREEFORM_PERIODS = new DataColumn("WADMIN_STAT_ALLOW_FREEFORM_PERIODS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_ALLOW_FREEFORM_PERIODS);
				this.columnWADMIN_STAT_TIMESHEET_TIED = new DataColumn("WADMIN_STAT_TIMESHEET_TIED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_STAT_TIMESHEET_TIED);
				this.columnWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS = new DataColumn("WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS);
				this.columnWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS = new DataColumn("WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS);
				this.columnWADMIN_MAX_SQL_BATCH_SIZE = new DataColumn("WADMIN_MAX_SQL_BATCH_SIZE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MAX_SQL_BATCH_SIZE);
				this.columnWADMIN_CORE_SQL_TIMEOUT = new DataColumn("WADMIN_CORE_SQL_TIMEOUT", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_CORE_SQL_TIMEOUT);
				this.columnWADMIN_MAX_SSP_BATCH_SIZE = new DataColumn("WADMIN_MAX_SSP_BATCH_SIZE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_MAX_SSP_BATCH_SIZE);
				this.columnWADMIN_USER_SYNC_SETTING = new DataColumn("WADMIN_USER_SYNC_SETTING", typeof(short), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_USER_SYNC_SETTING);
				this.columnWADMIN_AD_SYNC_REPLACE_CHAR = new DataColumn("WADMIN_AD_SYNC_REPLACE_CHAR", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_AD_SYNC_REPLACE_CHAR);
				this.columnWADMIN_SQL_BATCHING_ENABLED = new DataColumn("WADMIN_SQL_BATCHING_ENABLED", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SQL_BATCHING_ENABLED);
				this.columnWADMIN_SQL_BATCHING_BUFFER_SIZE = new DataColumn("WADMIN_SQL_BATCHING_BUFFER_SIZE", typeof(long), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SQL_BATCHING_BUFFER_SIZE);
				this.columnWADMIN_WSS_RESTRICT_WORKSPACE_CREATION = new DataColumn("WADMIN_WSS_RESTRICT_WORKSPACE_CREATION", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_WSS_RESTRICT_WORKSPACE_CREATION);
				this.columnWADMIN_FULL_SYNC_THRESHOLD = new DataColumn("WADMIN_FULL_SYNC_THRESHOLD", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_FULL_SYNC_THRESHOLD);
				this.columnTIMESHEET_CURRENT_VIEWSET_UID = new DataColumn("TIMESHEET_CURRENT_VIEWSET_UID", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnTIMESHEET_CURRENT_VIEWSET_UID);
				this.columnWADMIN_PERMISSION_MODE = new DataColumn("WADMIN_PERMISSION_MODE", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PERMISSION_MODE);
				this.columnWADMIN_SPPERMMODE_LAST_SYNC = new DataColumn("WADMIN_SPPERMMODE_LAST_SYNC", typeof(string), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SPPERMMODE_LAST_SYNC);
				this.columnWADMIN_SITEMAP_CACHE_VERSION = new DataColumn("WADMIN_SITEMAP_CACHE_VERSION", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SITEMAP_CACHE_VERSION);
				this.columnWADMIN_GROUPINGGANTT_CACHE_VERSION = new DataColumn("WADMIN_GROUPINGGANTT_CACHE_VERSION", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_GROUPINGGANTT_CACHE_VERSION);
				this.columnWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED = new DataColumn("WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED);
				this.columnWADMIN_SETTINGS_VERSION = new DataColumn("WADMIN_SETTINGS_VERSION", typeof(Guid), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SETTINGS_VERSION);
				this.columnWADMIN_IS_UPDATING = new DataColumn("WADMIN_IS_UPDATING", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_UPDATING);
				this.columnWADMIN_PROVISIONING_RESULT = new DataColumn("WADMIN_PROVISIONING_RESULT", typeof(int), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_PROVISIONING_RESULT);
				this.columnWADMIN_LOGICAL_READONLY = new DataColumn("WADMIN_LOGICAL_READONLY", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_LOGICAL_READONLY);
				this.columnWADMIN_OVER_QUOTA = new DataColumn("WADMIN_OVER_QUOTA", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_OVER_QUOTA);
				this.columnWADMIN_IS_DELETED = new DataColumn("WADMIN_IS_DELETED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_DELETED);
				this.columnWADMIN_SYNC_TASKS_TO_TASKLIST = new DataColumn("WADMIN_SYNC_TASKS_TO_TASKLIST", typeof(byte), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_SYNC_TASKS_TO_TASKLIST);
				this.columnWADMIN_USE_ENGAGEMENTS = new DataColumn("WADMIN_USE_ENGAGEMENTS", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_USE_ENGAGEMENTS);
				this.columnWADMIN_IS_NOTIFICATION_ENABLED = new DataColumn("WADMIN_IS_NOTIFICATION_ENABLED", typeof(bool), null, MappingType.Element);
				base.Columns.Add(this.columnWADMIN_IS_NOTIFICATION_ENABLED);
				base.Constraints.Add(new UniqueConstraint("WebAdminDataSetKey1", new DataColumn[]
				{
					this.columnWADMIN_UIDFAKE
				}, true));
				this.columnWADMIN_UIDFAKE.AllowDBNull = false;
				this.columnWADMIN_UIDFAKE.Unique = true;
				this.columnWADMIN_AUTHENTICATION_TYPE.AllowDBNull = false;
				this.columnWADMIN_NEW_ACCOUNT_PRIVILEGE.AllowDBNull = false;
				this.columnWADMIN_IS_DELEGATION_ALLOWED.AllowDBNull = false;
				this.columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH.AllowDBNull = false;
				this.columnWADMIN_WEEK_STARTS_ON.AllowDBNull = false;
				this.columnWADMIN_MIN_PASSWORD_LENGTH.AllowDBNull = false;
				this.columnWADMIN_DEFAULT_LANGUAGE.AllowDBNull = false;
				this.columnWADMIN_DEFAULT_TRACKING_METHOD.AllowDBNull = false;
				this.columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS.AllowDBNull = false;
				this.columnWADMIN_TRANS_HISTORY_DAYS.AllowDBNull = false;
				this.columnWADMIN_TIMESHEET_SPAN.AllowDBNull = false;
				this.columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_REPORTS_PER_MONTH.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_1PRD_1ST_START.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_2PRDS_1ST_START.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_2PRDS_1ST_END.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_3PRDS_1ST_START.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_3PRDS_1ST_END.AllowDBNull = false;
				this.columnWADMIN_MONTHLY_3PRDS_2ND_END.AllowDBNull = false;
				this.columnWADMIN_MAX_HOUR_PER_DAY.AllowDBNull = false;
				this.columnWADMIN_LOOKAHEAD.AllowDBNull = false;
				this.columnWADMIN_TIMEPERIOD_GRANULARITY.AllowDBNull = false;
				this.columnWADMIN_LICENSES.AllowDBNull = false;
				this.columnWADMIN_AUTO_CREATE_SUBWEBS.AllowDBNull = false;
				this.columnWADMIN_AUTO_ADD_USER_TO_SUBWEB.AllowDBNull = false;
				this.columnWADMIN_ENABLE_ENTERPRISE.AllowDBNull = false;
				this.columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE.AllowDBNull = false;
				this.columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE.AllowDBNull = false;
				this.columnWADMIN_SERVER_CURRENCY.AllowDBNull = false;
				this.columnWADMIN_ENFORCE_CURRENCY.AllowDBNull = false;
				this.columnWADMIN_INTRANET_SERVER_URL.AllowDBNull = false;
				this.columnWADMIN_EXTRANET_SERVER_URL.AllowDBNull = false;
				this.columnWADMIN_ONLY_PRO_PUBLISH.AllowDBNull = false;
				this.columnWADMIN_PROTECT_ACTUALS.AllowDBNull = false;
				this.columnWADMIN_BUILD_TEAM_BY_RBS.AllowDBNull = false;
				this.columnWADMIN_IS_HOSTED_ORG.AllowDBNull = false;
				this.columnWADMIN_TS_MODE_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_MIN_HR_PER_TS.AllowDBNull = false;
				this.columnWADMIN_TS_MAX_HR_PER_TS.AllowDBNull = false;
				this.columnWADMIN_TS_MAX_HR_PER_DAY.AllowDBNull = false;
				this.columnWADMIN_TS_HOURS_PER_DAY.AllowDBNull = false;
				this.columnWADMIN_TS_HOURS_PER_WEEK.AllowDBNull = false;
				this.columnWADMIN_TS_DEF_DISPLAY_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_CREATE_MODE_ENUM.AllowDBNull = false;
				this.columnWADMIN_TS_REPORT_UNIT_ENUM.AllowDBNull = false;
				this.columnWADMIN_WEEK_START_ON_ENUM.AllowDBNull = false;
				this.columnWADMIN_STAT_LOOK_AHEAD.AllowDBNull = false;
				this.columnWADMIN_STAT_LOOK_AHEAD_PERIODS.AllowDBNull = false;
				this.columnWADMIN_TS_DEF_ENTRY_MODE_ENUM.AllowDBNull = false;
				this.columnWADMIN_STAT_PERIOD_TYPE.AllowDBNull = false;
				this.columnWADMIN_LIST_SEPARATOR.AllowDBNull = false;
				this.columnWADMIN_RES_SCHEDULED_TIME.AllowDBNull = false;
				this.columnWADMIN_RESPLAN_FTE.AllowDBNull = false;
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND.AllowDBNull = false;
				this.columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD.AllowDBNull = false;
				this.columnCREATED_DATE.AllowDBNull = false;
				this.columnMOD_DATE.AllowDBNull = false;
				this.columnCREATED_REV_COUNTER.AllowDBNull = false;
				this.columnMOD_REV_COUNTER.AllowDBNull = false;
			}

			// Token: 0x0600B945 RID: 47429 RVA: 0x00243BF2 File Offset: 0x00241DF2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WebAdminDataSet.WebAdminRow NewWebAdminRow()
			{
				return (WebAdminDataSet.WebAdminRow)base.NewRow();
			}

			// Token: 0x0600B946 RID: 47430 RVA: 0x00243BFF File Offset: 0x00241DFF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new WebAdminDataSet.WebAdminRow(builder);
			}

			// Token: 0x0600B947 RID: 47431 RVA: 0x00243C07 File Offset: 0x00241E07
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override Type GetRowType()
			{
				return typeof(WebAdminDataSet.WebAdminRow);
			}

			// Token: 0x0600B948 RID: 47432 RVA: 0x00243C13 File Offset: 0x00241E13
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if (this.WebAdminRowChanged != null)
				{
					this.WebAdminRowChanged(this, new WebAdminDataSet.WebAdminRowChangeEvent((WebAdminDataSet.WebAdminRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B949 RID: 47433 RVA: 0x00243C46 File Offset: 0x00241E46
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if (this.WebAdminRowChanging != null)
				{
					this.WebAdminRowChanging(this, new WebAdminDataSet.WebAdminRowChangeEvent((WebAdminDataSet.WebAdminRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B94A RID: 47434 RVA: 0x00243C79 File Offset: 0x00241E79
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if (this.WebAdminRowDeleted != null)
				{
					this.WebAdminRowDeleted(this, new WebAdminDataSet.WebAdminRowChangeEvent((WebAdminDataSet.WebAdminRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B94B RID: 47435 RVA: 0x00243CAC File Offset: 0x00241EAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if (this.WebAdminRowDeleting != null)
				{
					this.WebAdminRowDeleting(this, new WebAdminDataSet.WebAdminRowChangeEvent((WebAdminDataSet.WebAdminRow)e.Row, e.Action));
				}
			}

			// Token: 0x0600B94C RID: 47436 RVA: 0x00243CDF File Offset: 0x00241EDF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void RemoveWebAdminRow(WebAdminDataSet.WebAdminRow row)
			{
				base.Rows.Remove(row);
			}

			// Token: 0x0600B94D RID: 47437 RVA: 0x00243CF0 File Offset: 0x00241EF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
			{
				XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
				XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
				WebAdminDataSet webAdminDataSet = new WebAdminDataSet();
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
				xmlSchemaAttribute.FixedValue = webAdminDataSet.Namespace;
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
				XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
				xmlSchemaAttribute2.Name = "tableTypeName";
				xmlSchemaAttribute2.FixedValue = "WebAdminDataTable";
				xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
				xmlSchemaComplexType.Particle = xmlSchemaSequence;
				XmlSchema schemaSerializable = webAdminDataSet.GetSchemaSerializable();
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

			// Token: 0x0400252C RID: 9516
			private DataColumn columnWADMIN_UIDFAKE;

			// Token: 0x0400252D RID: 9517
			private DataColumn columnWADMIN_AUTHENTICATION_TYPE;

			// Token: 0x0400252E RID: 9518
			private DataColumn columnWADMIN_NEW_ACCOUNT_PRIVILEGE;

			// Token: 0x0400252F RID: 9519
			private DataColumn columnWADMIN_IS_DELEGATION_ALLOWED;

			// Token: 0x04002530 RID: 9520
			private DataColumn columnWADMIN_AUTH_REQUIRED_FOR_PUBLISH;

			// Token: 0x04002531 RID: 9521
			private DataColumn columnWADMIN_WEEK_STARTS_ON;

			// Token: 0x04002532 RID: 9522
			private DataColumn columnWADMIN_MIN_PASSWORD_LENGTH;

			// Token: 0x04002533 RID: 9523
			private DataColumn columnWADMIN_NTFY_FROM_EMAIL;

			// Token: 0x04002534 RID: 9524
			private DataColumn columnWADMIN_NTFY_EMAIL_TRAILER;

			// Token: 0x04002535 RID: 9525
			private DataColumn columnWADMIN_ORG_EMAIL_ADDRESS;

			// Token: 0x04002536 RID: 9526
			private DataColumn columnWADMIN_EMAIL_CHARSET;

			// Token: 0x04002537 RID: 9527
			private DataColumn columnWADMIN_DEFAULT_LANGUAGE;

			// Token: 0x04002538 RID: 9528
			private DataColumn columnWADMIN_DEFAULT_TRACKING_METHOD;

			// Token: 0x04002539 RID: 9529
			private DataColumn columnWADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS;

			// Token: 0x0400253A RID: 9530
			private DataColumn columnWADMIN_IS_TRACKING_METHOD_LOCKED;

			// Token: 0x0400253B RID: 9531
			private DataColumn columnWADMIN_TRANS_HISTORY_DAYS;

			// Token: 0x0400253C RID: 9532
			private DataColumn columnWADMIN_TIMESHEET_SPAN;

			// Token: 0x0400253D RID: 9533
			private DataColumn columnWADMIN_WEEKLY_TIMESHEET_NUM_WEEKS;

			// Token: 0x0400253E RID: 9534
			private DataColumn columnWADMIN_MONTHLY_REPORTS_PER_MONTH;

			// Token: 0x0400253F RID: 9535
			private DataColumn columnWADMIN_MONTHLY_1PRD_1ST_START;

			// Token: 0x04002540 RID: 9536
			private DataColumn columnWADMIN_MONTHLY_2PRDS_1ST_START;

			// Token: 0x04002541 RID: 9537
			private DataColumn columnWADMIN_MONTHLY_2PRDS_1ST_END;

			// Token: 0x04002542 RID: 9538
			private DataColumn columnWADMIN_MONTHLY_3PRDS_1ST_START;

			// Token: 0x04002543 RID: 9539
			private DataColumn columnWADMIN_MONTHLY_3PRDS_1ST_END;

			// Token: 0x04002544 RID: 9540
			private DataColumn columnWADMIN_MONTHLY_3PRDS_2ND_END;

			// Token: 0x04002545 RID: 9541
			private DataColumn columnWADMIN_MAX_HOUR_PER_DAY;

			// Token: 0x04002546 RID: 9542
			private DataColumn columnWADMIN_LOOKAHEAD;

			// Token: 0x04002547 RID: 9543
			private DataColumn columnWADMIN_TIMEPERIOD_GRANULARITY;

			// Token: 0x04002548 RID: 9544
			private DataColumn columnWADMIN_LICENSES;

			// Token: 0x04002549 RID: 9545
			private DataColumn columnWADMIN_AUTO_CREATE_SUBWEBS;

			// Token: 0x0400254A RID: 9546
			private DataColumn columnWADMIN_AUTO_ADD_USER_TO_SUBWEB;

			// Token: 0x0400254B RID: 9547
			private DataColumn columnWADMIN_CURRENT_STS_SERVER_UID;

			// Token: 0x0400254C RID: 9548
			private DataColumn columnWADMIN_DEFAULT_SITE_COLLECTION;

			// Token: 0x0400254D RID: 9549
			private DataColumn columnWADMIN_ENABLE_ENTERPRISE;

			// Token: 0x0400254E RID: 9550
			private DataColumn columnWADMIN_DISPLAY_MASTER_IN_ENTERPRISE;

			// Token: 0x0400254F RID: 9551
			private DataColumn columnWADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE;

			// Token: 0x04002550 RID: 9552
			private DataColumn columnWADMIN_SERVER_CURRENCY;

			// Token: 0x04002551 RID: 9553
			private DataColumn columnWADMIN_ENFORCE_CURRENCY;

			// Token: 0x04002552 RID: 9554
			private DataColumn columnWADMIN_LAST_STS_ADMIN_SYNCH_TIME;

			// Token: 0x04002553 RID: 9555
			private DataColumn columnWADMIN_SMTP_SERVER_NAME;

			// Token: 0x04002554 RID: 9556
			private DataColumn columnWADMIN_SMTP_SERVER_PORT;

			// Token: 0x04002555 RID: 9557
			private DataColumn columnWADMIN_INTRANET_SERVER_URL;

			// Token: 0x04002556 RID: 9558
			private DataColumn columnWADMIN_EXTRANET_SERVER_URL;

			// Token: 0x04002557 RID: 9559
			private DataColumn columnWADMIN_ONLY_PRO_PUBLISH;

			// Token: 0x04002558 RID: 9560
			private DataColumn columnWADMIN_PROTECT_ACTUALS;

			// Token: 0x04002559 RID: 9561
			private DataColumn columnWADMIN_STS_TEMPLATE_LCID;

			// Token: 0x0400255A RID: 9562
			private DataColumn columnWADMIN_STS_TEMPLATE_ID;

			// Token: 0x0400255B RID: 9563
			private DataColumn columnWADMIN_STS_PRIMARY_OWNER_EMAIL;

			// Token: 0x0400255C RID: 9564
			private DataColumn columnWADMIN_BUILD_TEAM_BY_RBS;

			// Token: 0x0400255D RID: 9565
			private DataColumn columnWADMIN_IS_HOSTED_ORG;

			// Token: 0x0400255E RID: 9566
			private DataColumn columnWADMIN_USE_BASELINE_SUMMARY_DATA;

			// Token: 0x0400255F RID: 9567
			private DataColumn columnWADMIN_PROJECT_BUILD;

			// Token: 0x04002560 RID: 9568
			private DataColumn columnWADMIN_TS_MODE_ENUM;

			// Token: 0x04002561 RID: 9569
			private DataColumn columnWADMIN_TS_IS_UNVERS_TASK_ALLOWED;

			// Token: 0x04002562 RID: 9570
			private DataColumn columnWADMIN_TS_PROJECT_MANAGER_COORDINATION;

			// Token: 0x04002563 RID: 9571
			private DataColumn columnWADMIN_TS_PROJECT_MANAGER_APPROVAL;

			// Token: 0x04002564 RID: 9572
			private DataColumn columnWADMIN_TS_MAXIMUM_LINE_ITEMS;

			// Token: 0x04002565 RID: 9573
			private DataColumn columnWADMIN_TS_IS_AUDIT_ENABLED;

			// Token: 0x04002566 RID: 9574
			private DataColumn columnWADMIN_TS_IS_FUTURE_REP_ALLOWED;

			// Token: 0x04002567 RID: 9575
			private DataColumn columnWADMIN_TS_FIXED_APPROVAL_ROUTING;

			// Token: 0x04002568 RID: 9576
			private DataColumn columnWADMIN_TS_TIED_MODE;

			// Token: 0x04002569 RID: 9577
			private DataColumn columnWADMIN_TS_MIN_HR_PER_TS;

			// Token: 0x0400256A RID: 9578
			private DataColumn columnWADMIN_TS_MAX_HR_PER_TS;

			// Token: 0x0400256B RID: 9579
			private DataColumn columnWADMIN_TS_MAX_HR_PER_DAY;

			// Token: 0x0400256C RID: 9580
			private DataColumn columnWADMIN_TS_HOURS_PER_DAY;

			// Token: 0x0400256D RID: 9581
			private DataColumn columnWADMIN_TS_HOURS_PER_WEEK;

			// Token: 0x0400256E RID: 9582
			private DataColumn columnWADMIN_TS_DEF_DISPLAY_ENUM;

			// Token: 0x0400256F RID: 9583
			private DataColumn columnWADMIN_TS_OUTLOOK_DISPLAY_ENUM;

			// Token: 0x04002570 RID: 9584
			private DataColumn columnWADMIN_TS_CREATE_MODE_ENUM;

			// Token: 0x04002571 RID: 9585
			private DataColumn columnWADMIN_TS_REPORT_UNIT_ENUM;

			// Token: 0x04002572 RID: 9586
			private DataColumn columnWADMIN_WEEK_START_ON_ENUM;

			// Token: 0x04002573 RID: 9587
			private DataColumn columnWADMIN_STAT_MAX_HR_PER_DAY;

			// Token: 0x04002574 RID: 9588
			private DataColumn columnWADMIN_STAT_MAX_HR_PER_TASK;

			// Token: 0x04002575 RID: 9589
			private DataColumn columnWADMIN_STAT_LOOK_AHEAD;

			// Token: 0x04002576 RID: 9590
			private DataColumn columnWADMIN_STAT_LOOK_AHEAD_PERIODS;

			// Token: 0x04002577 RID: 9591
			private DataColumn columnWADMIN_STAT_REP_SCHED_ENUM;

			// Token: 0x04002578 RID: 9592
			private DataColumn columnWADMIN_STAT_SPAN_MODE_ENUM;

			// Token: 0x04002579 RID: 9593
			private DataColumn columnWADMIN_TS_DEF_ENTRY_MODE_ENUM;

			// Token: 0x0400257A RID: 9594
			private DataColumn columnWADMIN_STAT_PROT_ACT;

			// Token: 0x0400257B RID: 9595
			private DataColumn columnWADMIN_STAT_PERIOD_TYPE;

			// Token: 0x0400257C RID: 9596
			private DataColumn columnWADMIN_STAT_ENABLE_DOWNLOAD;

			// Token: 0x0400257D RID: 9597
			private DataColumn columnWADMIN_STAT_NUM_WK_SPANNED;

			// Token: 0x0400257E RID: 9598
			private DataColumn columnWADMIN_STAT_NUM_UPDATES_PER_MONTH;

			// Token: 0x0400257F RID: 9599
			private DataColumn columnWADMIN_STAT_1PRD_1ST_START;

			// Token: 0x04002580 RID: 9600
			private DataColumn columnWADMIN_STAT_2PRD_1ST_START;

			// Token: 0x04002581 RID: 9601
			private DataColumn columnWADMIN_STAT_2PRD_1ST_END;

			// Token: 0x04002582 RID: 9602
			private DataColumn columnWADMIN_STAT_3PRD_1ST_START;

			// Token: 0x04002583 RID: 9603
			private DataColumn columnWADMIN_STAT_3PRD_1ST_END;

			// Token: 0x04002584 RID: 9604
			private DataColumn columnWADMIN_STAT_3PRD_2ND_END;

			// Token: 0x04002585 RID: 9605
			private DataColumn columnWADMIN_LIST_SEPARATOR;

			// Token: 0x04002586 RID: 9606
			private DataColumn columnWADMIN_ACTIVE_CACHE_DIR;

			// Token: 0x04002587 RID: 9607
			private DataColumn columnWADMIN_ACTIVE_CACHE_MAX_SIZE_MB;

			// Token: 0x04002588 RID: 9608
			private DataColumn columnWADMIN_RES_SCHEDULED_TIME;

			// Token: 0x04002589 RID: 9609
			private DataColumn columnWADMIN_RESPLAN_FTE;

			// Token: 0x0400258A RID: 9610
			private DataColumn columnWADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND;

			// Token: 0x0400258B RID: 9611
			private DataColumn columnWADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD;

			// Token: 0x0400258C RID: 9612
			private DataColumn columnWADMIN_RESOURCE_CAPACITY_JOB_UID;

			// Token: 0x0400258D RID: 9613
			private DataColumn columnWADMIN_REMINDER_TIMER_JOB_UID;

			// Token: 0x0400258E RID: 9614
			private DataColumn columnWADMIN_USE_PROJECT_STATE;

			// Token: 0x0400258F RID: 9615
			private DataColumn columnWADMIN_SHOW_WSS_NAV_LINKS;

			// Token: 0x04002590 RID: 9616
			private DataColumn columnWADMIN_ALWAYS_EXPAND_NAV_LINKS;

			// Token: 0x04002591 RID: 9617
			private DataColumn columnWADMIN_WORKFLOW_PROXY_ACCT;

			// Token: 0x04002592 RID: 9618
			private DataColumn columnWADMIN_WORKFLOW_PROXY_UID;

			// Token: 0x04002593 RID: 9619
			private DataColumn columnWADMIN_WORKFLOW_PROXY_WINDOWS;

			// Token: 0x04002594 RID: 9620
			private DataColumn columnWADMIN_WORKFLOW_PROXY_MOD_BY;

			// Token: 0x04002595 RID: 9621
			private DataColumn columnWADMIN_WORKFLOW_PROXY_MOD_DATE;

			// Token: 0x04002596 RID: 9622
			private DataColumn columnCREATED_DATE;

			// Token: 0x04002597 RID: 9623
			private DataColumn columnMOD_DATE;

			// Token: 0x04002598 RID: 9624
			private DataColumn columnCREATED_REV_COUNTER;

			// Token: 0x04002599 RID: 9625
			private DataColumn columnMOD_REV_COUNTER;

			// Token: 0x0400259A RID: 9626
			private DataColumn columnWADMIN_SERVER_FLAGS;

			// Token: 0x0400259B RID: 9627
			private DataColumn columnWADMIN_MIN_WINPROJ_BUILD_NUMBERS;

			// Token: 0x0400259C RID: 9628
			private DataColumn columnWADMIN_EXCHANGE_INTEGRATION_ENABLED;

			// Token: 0x0400259D RID: 9629
			private DataColumn columnWADMIN_EXCHANGE_URL_REFRESH_JOB_UID;

			// Token: 0x0400259E RID: 9630
			private DataColumn columnWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID;

			// Token: 0x0400259F RID: 9631
			private DataColumn columnWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID;

			// Token: 0x040025A0 RID: 9632
			private DataColumn columnWADMIN_PUBLISH_MANUAL_TASKS;

			// Token: 0x040025A1 RID: 9633
			private DataColumn columnWADMIN_SERVER_DEFAULT_TASK_MODE;

			// Token: 0x040025A2 RID: 9634
			private DataColumn columnWADMIN_LOCK_PRO_DEFAULT_TASK_MODE;

			// Token: 0x040025A3 RID: 9635
			private DataColumn columnWADMIN_TS_ALLOW_PROJECT_LEVEL;

			// Token: 0x040025A4 RID: 9636
			private DataColumn columnWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED;

			// Token: 0x040025A5 RID: 9637
			private DataColumn columnWADMIN_OFF_PEAK_SYNC_THRESHOLD;

			// Token: 0x040025A6 RID: 9638
			private DataColumn columnWADMIN_DISABLED_SYNC_THRESHOLD;

			// Token: 0x040025A7 RID: 9639
			private DataColumn columnWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID;

			// Token: 0x040025A8 RID: 9640
			private DataColumn columnWADMIN_STAT_IMPORT_LINE_CLASSES;

			// Token: 0x040025A9 RID: 9641
			private DataColumn columnWADMIN_DATABASE_CACHE_ENABLED;

			// Token: 0x040025AA RID: 9642
			private DataColumn columnWADMIN_WSS_PWA_ADMIN_ROLE_ID;

			// Token: 0x040025AB RID: 9643
			private DataColumn columnWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID;

			// Token: 0x040025AC RID: 9644
			private DataColumn columnWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID;

			// Token: 0x040025AD RID: 9645
			private DataColumn columnWADMIN_WSS_PWA_READER_ROLE_ID;

			// Token: 0x040025AE RID: 9646
			private DataColumn columnWADMIN_STAT_ALLOW_FREEFORM_PERIODS;

			// Token: 0x040025AF RID: 9647
			private DataColumn columnWADMIN_STAT_TIMESHEET_TIED;

			// Token: 0x040025B0 RID: 9648
			private DataColumn columnWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS;

			// Token: 0x040025B1 RID: 9649
			private DataColumn columnWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS;

			// Token: 0x040025B2 RID: 9650
			private DataColumn columnWADMIN_MAX_SQL_BATCH_SIZE;

			// Token: 0x040025B3 RID: 9651
			private DataColumn columnWADMIN_CORE_SQL_TIMEOUT;

			// Token: 0x040025B4 RID: 9652
			private DataColumn columnWADMIN_MAX_SSP_BATCH_SIZE;

			// Token: 0x040025B5 RID: 9653
			private DataColumn columnWADMIN_USER_SYNC_SETTING;

			// Token: 0x040025B6 RID: 9654
			private DataColumn columnWADMIN_AD_SYNC_REPLACE_CHAR;

			// Token: 0x040025B7 RID: 9655
			private DataColumn columnWADMIN_SQL_BATCHING_ENABLED;

			// Token: 0x040025B8 RID: 9656
			private DataColumn columnWADMIN_SQL_BATCHING_BUFFER_SIZE;

			// Token: 0x040025B9 RID: 9657
			private DataColumn columnWADMIN_WSS_RESTRICT_WORKSPACE_CREATION;

			// Token: 0x040025BA RID: 9658
			private DataColumn columnWADMIN_FULL_SYNC_THRESHOLD;

			// Token: 0x040025BB RID: 9659
			private DataColumn columnTIMESHEET_CURRENT_VIEWSET_UID;

			// Token: 0x040025BC RID: 9660
			private DataColumn columnWADMIN_PERMISSION_MODE;

			// Token: 0x040025BD RID: 9661
			private DataColumn columnWADMIN_SPPERMMODE_LAST_SYNC;

			// Token: 0x040025BE RID: 9662
			private DataColumn columnWADMIN_SITEMAP_CACHE_VERSION;

			// Token: 0x040025BF RID: 9663
			private DataColumn columnWADMIN_GROUPINGGANTT_CACHE_VERSION;

			// Token: 0x040025C0 RID: 9664
			private DataColumn columnWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED;

			// Token: 0x040025C1 RID: 9665
			private DataColumn columnWADMIN_SETTINGS_VERSION;

			// Token: 0x040025C2 RID: 9666
			private DataColumn columnWADMIN_IS_UPDATING;

			// Token: 0x040025C3 RID: 9667
			private DataColumn columnWADMIN_PROVISIONING_RESULT;

			// Token: 0x040025C4 RID: 9668
			private DataColumn columnWADMIN_LOGICAL_READONLY;

			// Token: 0x040025C5 RID: 9669
			private DataColumn columnWADMIN_OVER_QUOTA;

			// Token: 0x040025C6 RID: 9670
			private DataColumn columnWADMIN_IS_DELETED;

			// Token: 0x040025C7 RID: 9671
			private DataColumn columnWADMIN_SYNC_TASKS_TO_TASKLIST;

			// Token: 0x040025C8 RID: 9672
			private DataColumn columnWADMIN_USE_ENGAGEMENTS;

			// Token: 0x040025C9 RID: 9673
			private DataColumn columnWADMIN_IS_NOTIFICATION_ENABLED;
		}

		// Token: 0x02000780 RID: 1920
		public class WebAdminRow : DataRow
		{
			// Token: 0x0600B94E RID: 47438 RVA: 0x00243EE8 File Offset: 0x002420E8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			internal WebAdminRow(DataRowBuilder rb) : base(rb)
			{
				this.tableWebAdmin = (WebAdminDataSet.WebAdminDataTable)base.Table;
			}

			// Token: 0x170038DD RID: 14557
			// (get) Token: 0x0600B94F RID: 47439 RVA: 0x00243F02 File Offset: 0x00242102
			// (set) Token: 0x0600B950 RID: 47440 RVA: 0x00243F1A File Offset: 0x0024211A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_UIDFAKE
			{
				get
				{
					return (Guid)base[this.tableWebAdmin.WADMIN_UIDFAKEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_UIDFAKEColumn] = value;
				}
			}

			// Token: 0x170038DE RID: 14558
			// (get) Token: 0x0600B951 RID: 47441 RVA: 0x00243F33 File Offset: 0x00242133
			// (set) Token: 0x0600B952 RID: 47442 RVA: 0x00243F4B File Offset: 0x0024214B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_AUTHENTICATION_TYPE
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_AUTHENTICATION_TYPEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_AUTHENTICATION_TYPEColumn] = value;
				}
			}

			// Token: 0x170038DF RID: 14559
			// (get) Token: 0x0600B953 RID: 47443 RVA: 0x00243F64 File Offset: 0x00242164
			// (set) Token: 0x0600B954 RID: 47444 RVA: 0x00243F7C File Offset: 0x0024217C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_NEW_ACCOUNT_PRIVILEGE
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_NEW_ACCOUNT_PRIVILEGEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_NEW_ACCOUNT_PRIVILEGEColumn] = value;
				}
			}

			// Token: 0x170038E0 RID: 14560
			// (get) Token: 0x0600B955 RID: 47445 RVA: 0x00243F95 File Offset: 0x00242195
			// (set) Token: 0x0600B956 RID: 47446 RVA: 0x00243FAD File Offset: 0x002421AD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_IS_DELEGATION_ALLOWED
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_IS_DELEGATION_ALLOWEDColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_DELEGATION_ALLOWEDColumn] = value;
				}
			}

			// Token: 0x170038E1 RID: 14561
			// (get) Token: 0x0600B957 RID: 47447 RVA: 0x00243FC6 File Offset: 0x002421C6
			// (set) Token: 0x0600B958 RID: 47448 RVA: 0x00243FDE File Offset: 0x002421DE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_AUTH_REQUIRED_FOR_PUBLISH
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_AUTH_REQUIRED_FOR_PUBLISHColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_AUTH_REQUIRED_FOR_PUBLISHColumn] = value;
				}
			}

			// Token: 0x170038E2 RID: 14562
			// (get) Token: 0x0600B959 RID: 47449 RVA: 0x00243FF7 File Offset: 0x002421F7
			// (set) Token: 0x0600B95A RID: 47450 RVA: 0x0024400F File Offset: 0x0024220F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_WEEK_STARTS_ON
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_WEEK_STARTS_ONColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WEEK_STARTS_ONColumn] = value;
				}
			}

			// Token: 0x170038E3 RID: 14563
			// (get) Token: 0x0600B95B RID: 47451 RVA: 0x00244028 File Offset: 0x00242228
			// (set) Token: 0x0600B95C RID: 47452 RVA: 0x00244040 File Offset: 0x00242240
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_MIN_PASSWORD_LENGTH
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_MIN_PASSWORD_LENGTHColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MIN_PASSWORD_LENGTHColumn] = value;
				}
			}

			// Token: 0x170038E4 RID: 14564
			// (get) Token: 0x0600B95D RID: 47453 RVA: 0x0024405C File Offset: 0x0024225C
			// (set) Token: 0x0600B95E RID: 47454 RVA: 0x002440A0 File Offset: 0x002422A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_NTFY_FROM_EMAIL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_NTFY_FROM_EMAILColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_NTFY_FROM_EMAIL' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_NTFY_FROM_EMAILColumn] = value;
				}
			}

			// Token: 0x170038E5 RID: 14565
			// (get) Token: 0x0600B95F RID: 47455 RVA: 0x002440B4 File Offset: 0x002422B4
			// (set) Token: 0x0600B960 RID: 47456 RVA: 0x002440F8 File Offset: 0x002422F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_NTFY_EMAIL_TRAILER
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_NTFY_EMAIL_TRAILERColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_NTFY_EMAIL_TRAILER' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_NTFY_EMAIL_TRAILERColumn] = value;
				}
			}

			// Token: 0x170038E6 RID: 14566
			// (get) Token: 0x0600B961 RID: 47457 RVA: 0x0024410C File Offset: 0x0024230C
			// (set) Token: 0x0600B962 RID: 47458 RVA: 0x00244150 File Offset: 0x00242350
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_ORG_EMAIL_ADDRESS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_ORG_EMAIL_ADDRESSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_ORG_EMAIL_ADDRESS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ORG_EMAIL_ADDRESSColumn] = value;
				}
			}

			// Token: 0x170038E7 RID: 14567
			// (get) Token: 0x0600B963 RID: 47459 RVA: 0x00244164 File Offset: 0x00242364
			// (set) Token: 0x0600B964 RID: 47460 RVA: 0x002441A8 File Offset: 0x002423A8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_EMAIL_CHARSET
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_EMAIL_CHARSETColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EMAIL_CHARSET' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EMAIL_CHARSETColumn] = value;
				}
			}

			// Token: 0x170038E8 RID: 14568
			// (get) Token: 0x0600B965 RID: 47461 RVA: 0x002441BC File Offset: 0x002423BC
			// (set) Token: 0x0600B966 RID: 47462 RVA: 0x002441D4 File Offset: 0x002423D4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_DEFAULT_LANGUAGE
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_DEFAULT_LANGUAGEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DEFAULT_LANGUAGEColumn] = value;
				}
			}

			// Token: 0x170038E9 RID: 14569
			// (get) Token: 0x0600B967 RID: 47463 RVA: 0x002441ED File Offset: 0x002423ED
			// (set) Token: 0x0600B968 RID: 47464 RVA: 0x00244205 File Offset: 0x00242405
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_DEFAULT_TRACKING_METHOD
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_DEFAULT_TRACKING_METHODColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DEFAULT_TRACKING_METHODColumn] = value;
				}
			}

			// Token: 0x170038EA RID: 14570
			// (get) Token: 0x0600B969 RID: 47465 RVA: 0x0024421E File Offset: 0x0024241E
			// (set) Token: 0x0600B96A RID: 47466 RVA: 0x00244236 File Offset: 0x00242436
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTS
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_CAN_PUBLISH_CONSOLIDATED_PROJECTSColumn] = value;
				}
			}

			// Token: 0x170038EB RID: 14571
			// (get) Token: 0x0600B96B RID: 47467 RVA: 0x00244250 File Offset: 0x00242450
			// (set) Token: 0x0600B96C RID: 47468 RVA: 0x00244294 File Offset: 0x00242494
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_IS_TRACKING_METHOD_LOCKED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_TRACKING_METHOD_LOCKED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn] = value;
				}
			}

			// Token: 0x170038EC RID: 14572
			// (get) Token: 0x0600B96D RID: 47469 RVA: 0x002442AD File Offset: 0x002424AD
			// (set) Token: 0x0600B96E RID: 47470 RVA: 0x002442C5 File Offset: 0x002424C5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_TRANS_HISTORY_DAYS
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_TRANS_HISTORY_DAYSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TRANS_HISTORY_DAYSColumn] = value;
				}
			}

			// Token: 0x170038ED RID: 14573
			// (get) Token: 0x0600B96F RID: 47471 RVA: 0x002442DE File Offset: 0x002424DE
			// (set) Token: 0x0600B970 RID: 47472 RVA: 0x002442F6 File Offset: 0x002424F6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_TIMESHEET_SPAN
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TIMESHEET_SPANColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TIMESHEET_SPANColumn] = value;
				}
			}

			// Token: 0x170038EE RID: 14574
			// (get) Token: 0x0600B971 RID: 47473 RVA: 0x0024430F File Offset: 0x0024250F
			// (set) Token: 0x0600B972 RID: 47474 RVA: 0x00244327 File Offset: 0x00242527
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_WEEKLY_TIMESHEET_NUM_WEEKS
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_WEEKLY_TIMESHEET_NUM_WEEKSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WEEKLY_TIMESHEET_NUM_WEEKSColumn] = value;
				}
			}

			// Token: 0x170038EF RID: 14575
			// (get) Token: 0x0600B973 RID: 47475 RVA: 0x00244340 File Offset: 0x00242540
			// (set) Token: 0x0600B974 RID: 47476 RVA: 0x00244358 File Offset: 0x00242558
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_REPORTS_PER_MONTH
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_REPORTS_PER_MONTHColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_REPORTS_PER_MONTHColumn] = value;
				}
			}

			// Token: 0x170038F0 RID: 14576
			// (get) Token: 0x0600B975 RID: 47477 RVA: 0x00244371 File Offset: 0x00242571
			// (set) Token: 0x0600B976 RID: 47478 RVA: 0x00244389 File Offset: 0x00242589
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_1PRD_1ST_START
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_1PRD_1ST_STARTColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_1PRD_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x170038F1 RID: 14577
			// (get) Token: 0x0600B977 RID: 47479 RVA: 0x002443A2 File Offset: 0x002425A2
			// (set) Token: 0x0600B978 RID: 47480 RVA: 0x002443BA File Offset: 0x002425BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_2PRDS_1ST_START
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_2PRDS_1ST_STARTColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_2PRDS_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x170038F2 RID: 14578
			// (get) Token: 0x0600B979 RID: 47481 RVA: 0x002443D3 File Offset: 0x002425D3
			// (set) Token: 0x0600B97A RID: 47482 RVA: 0x002443EB File Offset: 0x002425EB
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_2PRDS_1ST_END
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_2PRDS_1ST_ENDColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_2PRDS_1ST_ENDColumn] = value;
				}
			}

			// Token: 0x170038F3 RID: 14579
			// (get) Token: 0x0600B97B RID: 47483 RVA: 0x00244404 File Offset: 0x00242604
			// (set) Token: 0x0600B97C RID: 47484 RVA: 0x0024441C File Offset: 0x0024261C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_3PRDS_1ST_START
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_1ST_STARTColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x170038F4 RID: 14580
			// (get) Token: 0x0600B97D RID: 47485 RVA: 0x00244435 File Offset: 0x00242635
			// (set) Token: 0x0600B97E RID: 47486 RVA: 0x0024444D File Offset: 0x0024264D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_MONTHLY_3PRDS_1ST_END
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_1ST_ENDColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_1ST_ENDColumn] = value;
				}
			}

			// Token: 0x170038F5 RID: 14581
			// (get) Token: 0x0600B97F RID: 47487 RVA: 0x00244466 File Offset: 0x00242666
			// (set) Token: 0x0600B980 RID: 47488 RVA: 0x0024447E File Offset: 0x0024267E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_MONTHLY_3PRDS_2ND_END
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_2ND_ENDColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MONTHLY_3PRDS_2ND_ENDColumn] = value;
				}
			}

			// Token: 0x170038F6 RID: 14582
			// (get) Token: 0x0600B981 RID: 47489 RVA: 0x00244497 File Offset: 0x00242697
			// (set) Token: 0x0600B982 RID: 47490 RVA: 0x002444AF File Offset: 0x002426AF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal WADMIN_MAX_HOUR_PER_DAY
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_MAX_HOUR_PER_DAYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MAX_HOUR_PER_DAYColumn] = value;
				}
			}

			// Token: 0x170038F7 RID: 14583
			// (get) Token: 0x0600B983 RID: 47491 RVA: 0x002444C8 File Offset: 0x002426C8
			// (set) Token: 0x0600B984 RID: 47492 RVA: 0x002444E0 File Offset: 0x002426E0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_LOOKAHEAD
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_LOOKAHEADColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LOOKAHEADColumn] = value;
				}
			}

			// Token: 0x170038F8 RID: 14584
			// (get) Token: 0x0600B985 RID: 47493 RVA: 0x002444F9 File Offset: 0x002426F9
			// (set) Token: 0x0600B986 RID: 47494 RVA: 0x00244511 File Offset: 0x00242711
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_TIMEPERIOD_GRANULARITY
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TIMEPERIOD_GRANULARITYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TIMEPERIOD_GRANULARITYColumn] = value;
				}
			}

			// Token: 0x170038F9 RID: 14585
			// (get) Token: 0x0600B987 RID: 47495 RVA: 0x0024452A File Offset: 0x0024272A
			// (set) Token: 0x0600B988 RID: 47496 RVA: 0x00244542 File Offset: 0x00242742
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_LICENSES
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_LICENSESColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LICENSESColumn] = value;
				}
			}

			// Token: 0x170038FA RID: 14586
			// (get) Token: 0x0600B989 RID: 47497 RVA: 0x0024455B File Offset: 0x0024275B
			// (set) Token: 0x0600B98A RID: 47498 RVA: 0x00244573 File Offset: 0x00242773
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_AUTO_CREATE_SUBWEBS
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_AUTO_CREATE_SUBWEBSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_AUTO_CREATE_SUBWEBSColumn] = value;
				}
			}

			// Token: 0x170038FB RID: 14587
			// (get) Token: 0x0600B98B RID: 47499 RVA: 0x0024458C File Offset: 0x0024278C
			// (set) Token: 0x0600B98C RID: 47500 RVA: 0x002445A4 File Offset: 0x002427A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_AUTO_ADD_USER_TO_SUBWEB
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_AUTO_ADD_USER_TO_SUBWEBColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_AUTO_ADD_USER_TO_SUBWEBColumn] = value;
				}
			}

			// Token: 0x170038FC RID: 14588
			// (get) Token: 0x0600B98D RID: 47501 RVA: 0x002445C0 File Offset: 0x002427C0
			// (set) Token: 0x0600B98E RID: 47502 RVA: 0x00244604 File Offset: 0x00242804
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_CURRENT_STS_SERVER_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_CURRENT_STS_SERVER_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_CURRENT_STS_SERVER_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_CURRENT_STS_SERVER_UIDColumn] = value;
				}
			}

			// Token: 0x170038FD RID: 14589
			// (get) Token: 0x0600B98F RID: 47503 RVA: 0x00244620 File Offset: 0x00242820
			// (set) Token: 0x0600B990 RID: 47504 RVA: 0x00244664 File Offset: 0x00242864
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_DEFAULT_SITE_COLLECTION
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_DEFAULT_SITE_COLLECTIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_DEFAULT_SITE_COLLECTION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DEFAULT_SITE_COLLECTIONColumn] = value;
				}
			}

			// Token: 0x170038FE RID: 14590
			// (get) Token: 0x0600B991 RID: 47505 RVA: 0x00244678 File Offset: 0x00242878
			// (set) Token: 0x0600B992 RID: 47506 RVA: 0x00244690 File Offset: 0x00242890
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_ENABLE_ENTERPRISE
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_ENABLE_ENTERPRISEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ENABLE_ENTERPRISEColumn] = value;
				}
			}

			// Token: 0x170038FF RID: 14591
			// (get) Token: 0x0600B993 RID: 47507 RVA: 0x002446A9 File Offset: 0x002428A9
			// (set) Token: 0x0600B994 RID: 47508 RVA: 0x002446C1 File Offset: 0x002428C1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_DISPLAY_MASTER_IN_ENTERPRISE
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_DISPLAY_MASTER_IN_ENTERPRISEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DISPLAY_MASTER_IN_ENTERPRISEColumn] = value;
				}
			}

			// Token: 0x17003900 RID: 14592
			// (get) Token: 0x0600B995 RID: 47509 RVA: 0x002446DA File Offset: 0x002428DA
			// (set) Token: 0x0600B996 RID: 47510 RVA: 0x002446F2 File Offset: 0x002428F2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISE
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ALLOW_LOCAL_BASE_CALS_IN_ENTERPRISEColumn] = value;
				}
			}

			// Token: 0x17003901 RID: 14593
			// (get) Token: 0x0600B997 RID: 47511 RVA: 0x0024470B File Offset: 0x0024290B
			// (set) Token: 0x0600B998 RID: 47512 RVA: 0x00244723 File Offset: 0x00242923
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_SERVER_CURRENCY
			{
				get
				{
					return (string)base[this.tableWebAdmin.WADMIN_SERVER_CURRENCYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SERVER_CURRENCYColumn] = value;
				}
			}

			// Token: 0x17003902 RID: 14594
			// (get) Token: 0x0600B999 RID: 47513 RVA: 0x00244737 File Offset: 0x00242937
			// (set) Token: 0x0600B99A RID: 47514 RVA: 0x0024474F File Offset: 0x0024294F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_ENFORCE_CURRENCY
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_ENFORCE_CURRENCYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ENFORCE_CURRENCYColumn] = value;
				}
			}

			// Token: 0x17003903 RID: 14595
			// (get) Token: 0x0600B99B RID: 47515 RVA: 0x00244768 File Offset: 0x00242968
			// (set) Token: 0x0600B99C RID: 47516 RVA: 0x002447AC File Offset: 0x002429AC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime WADMIN_LAST_STS_ADMIN_SYNCH_TIME
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWebAdmin.WADMIN_LAST_STS_ADMIN_SYNCH_TIMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_LAST_STS_ADMIN_SYNCH_TIME' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LAST_STS_ADMIN_SYNCH_TIMEColumn] = value;
				}
			}

			// Token: 0x17003904 RID: 14596
			// (get) Token: 0x0600B99D RID: 47517 RVA: 0x002447C8 File Offset: 0x002429C8
			// (set) Token: 0x0600B99E RID: 47518 RVA: 0x0024480C File Offset: 0x00242A0C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_SMTP_SERVER_NAME
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_SMTP_SERVER_NAMEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SMTP_SERVER_NAME' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SMTP_SERVER_NAMEColumn] = value;
				}
			}

			// Token: 0x17003905 RID: 14597
			// (get) Token: 0x0600B99F RID: 47519 RVA: 0x00244820 File Offset: 0x00242A20
			// (set) Token: 0x0600B9A0 RID: 47520 RVA: 0x00244864 File Offset: 0x00242A64
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_SMTP_SERVER_PORT
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_SMTP_SERVER_PORTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SMTP_SERVER_PORT' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SMTP_SERVER_PORTColumn] = value;
				}
			}

			// Token: 0x17003906 RID: 14598
			// (get) Token: 0x0600B9A1 RID: 47521 RVA: 0x0024487D File Offset: 0x00242A7D
			// (set) Token: 0x0600B9A2 RID: 47522 RVA: 0x00244895 File Offset: 0x00242A95
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_INTRANET_SERVER_URL
			{
				get
				{
					return (string)base[this.tableWebAdmin.WADMIN_INTRANET_SERVER_URLColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_INTRANET_SERVER_URLColumn] = value;
				}
			}

			// Token: 0x17003907 RID: 14599
			// (get) Token: 0x0600B9A3 RID: 47523 RVA: 0x002448A9 File Offset: 0x00242AA9
			// (set) Token: 0x0600B9A4 RID: 47524 RVA: 0x002448C1 File Offset: 0x00242AC1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_EXTRANET_SERVER_URL
			{
				get
				{
					return (string)base[this.tableWebAdmin.WADMIN_EXTRANET_SERVER_URLColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXTRANET_SERVER_URLColumn] = value;
				}
			}

			// Token: 0x17003908 RID: 14600
			// (get) Token: 0x0600B9A5 RID: 47525 RVA: 0x002448D5 File Offset: 0x00242AD5
			// (set) Token: 0x0600B9A6 RID: 47526 RVA: 0x002448ED File Offset: 0x00242AED
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_ONLY_PRO_PUBLISH
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_ONLY_PRO_PUBLISHColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ONLY_PRO_PUBLISHColumn] = value;
				}
			}

			// Token: 0x17003909 RID: 14601
			// (get) Token: 0x0600B9A7 RID: 47527 RVA: 0x00244906 File Offset: 0x00242B06
			// (set) Token: 0x0600B9A8 RID: 47528 RVA: 0x0024491E File Offset: 0x00242B1E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_PROTECT_ACTUALS
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_PROTECT_ACTUALSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PROTECT_ACTUALSColumn] = value;
				}
			}

			// Token: 0x1700390A RID: 14602
			// (get) Token: 0x0600B9A9 RID: 47529 RVA: 0x00244938 File Offset: 0x00242B38
			// (set) Token: 0x0600B9AA RID: 47530 RVA: 0x0024497C File Offset: 0x00242B7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_STS_TEMPLATE_LCID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_LCIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STS_TEMPLATE_LCID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_LCIDColumn] = value;
				}
			}

			// Token: 0x1700390B RID: 14603
			// (get) Token: 0x0600B9AB RID: 47531 RVA: 0x00244998 File Offset: 0x00242B98
			// (set) Token: 0x0600B9AC RID: 47532 RVA: 0x002449DC File Offset: 0x00242BDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_STS_TEMPLATE_ID
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STS_TEMPLATE_ID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_IDColumn] = value;
				}
			}

			// Token: 0x1700390C RID: 14604
			// (get) Token: 0x0600B9AD RID: 47533 RVA: 0x002449F0 File Offset: 0x00242BF0
			// (set) Token: 0x0600B9AE RID: 47534 RVA: 0x00244A34 File Offset: 0x00242C34
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_STS_PRIMARY_OWNER_EMAIL
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_STS_PRIMARY_OWNER_EMAILColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STS_PRIMARY_OWNER_EMAIL' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STS_PRIMARY_OWNER_EMAILColumn] = value;
				}
			}

			// Token: 0x1700390D RID: 14605
			// (get) Token: 0x0600B9AF RID: 47535 RVA: 0x00244A48 File Offset: 0x00242C48
			// (set) Token: 0x0600B9B0 RID: 47536 RVA: 0x00244A60 File Offset: 0x00242C60
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_BUILD_TEAM_BY_RBS
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_BUILD_TEAM_BY_RBSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_BUILD_TEAM_BY_RBSColumn] = value;
				}
			}

			// Token: 0x1700390E RID: 14606
			// (get) Token: 0x0600B9B1 RID: 47537 RVA: 0x00244A79 File Offset: 0x00242C79
			// (set) Token: 0x0600B9B2 RID: 47538 RVA: 0x00244A91 File Offset: 0x00242C91
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_IS_HOSTED_ORG
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_IS_HOSTED_ORGColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_HOSTED_ORGColumn] = value;
				}
			}

			// Token: 0x1700390F RID: 14607
			// (get) Token: 0x0600B9B3 RID: 47539 RVA: 0x00244AAC File Offset: 0x00242CAC
			// (set) Token: 0x0600B9B4 RID: 47540 RVA: 0x00244AF0 File Offset: 0x00242CF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_USE_BASELINE_SUMMARY_DATA
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableWebAdmin.WADMIN_USE_BASELINE_SUMMARY_DATAColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_USE_BASELINE_SUMMARY_DATA' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_USE_BASELINE_SUMMARY_DATAColumn] = value;
				}
			}

			// Token: 0x17003910 RID: 14608
			// (get) Token: 0x0600B9B5 RID: 47541 RVA: 0x00244B0C File Offset: 0x00242D0C
			// (set) Token: 0x0600B9B6 RID: 47542 RVA: 0x00244B50 File Offset: 0x00242D50
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_PROJECT_BUILD
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_PROJECT_BUILDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PROJECT_BUILD' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PROJECT_BUILDColumn] = value;
				}
			}

			// Token: 0x17003911 RID: 14609
			// (get) Token: 0x0600B9B7 RID: 47543 RVA: 0x00244B64 File Offset: 0x00242D64
			// (set) Token: 0x0600B9B8 RID: 47544 RVA: 0x00244B7C File Offset: 0x00242D7C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_MODE_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_MODE_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x17003912 RID: 14610
			// (get) Token: 0x0600B9B9 RID: 47545 RVA: 0x00244B98 File Offset: 0x00242D98
			// (set) Token: 0x0600B9BA RID: 47546 RVA: 0x00244BDC File Offset: 0x00242DDC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_IS_UNVERS_TASK_ALLOWED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_UNVERS_TASK_ALLOWED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn] = value;
				}
			}

			// Token: 0x17003913 RID: 14611
			// (get) Token: 0x0600B9BB RID: 47547 RVA: 0x00244BF8 File Offset: 0x00242DF8
			// (set) Token: 0x0600B9BC RID: 47548 RVA: 0x00244C3C File Offset: 0x00242E3C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_PROJECT_MANAGER_COORDINATION
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_PROJECT_MANAGER_COORDINATION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn] = value;
				}
			}

			// Token: 0x17003914 RID: 14612
			// (get) Token: 0x0600B9BD RID: 47549 RVA: 0x00244C58 File Offset: 0x00242E58
			// (set) Token: 0x0600B9BE RID: 47550 RVA: 0x00244C9C File Offset: 0x00242E9C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_PROJECT_MANAGER_APPROVAL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_PROJECT_MANAGER_APPROVAL' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn] = value;
				}
			}

			// Token: 0x17003915 RID: 14613
			// (get) Token: 0x0600B9BF RID: 47551 RVA: 0x00244CB8 File Offset: 0x00242EB8
			// (set) Token: 0x0600B9C0 RID: 47552 RVA: 0x00244CFC File Offset: 0x00242EFC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_TS_MAXIMUM_LINE_ITEMS
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_TS_MAXIMUM_LINE_ITEMSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_MAXIMUM_LINE_ITEMS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_MAXIMUM_LINE_ITEMSColumn] = value;
				}
			}

			// Token: 0x17003916 RID: 14614
			// (get) Token: 0x0600B9C1 RID: 47553 RVA: 0x00244D18 File Offset: 0x00242F18
			// (set) Token: 0x0600B9C2 RID: 47554 RVA: 0x00244D5C File Offset: 0x00242F5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_IS_AUDIT_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_IS_AUDIT_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_AUDIT_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_IS_AUDIT_ENABLEDColumn] = value;
				}
			}

			// Token: 0x17003917 RID: 14615
			// (get) Token: 0x0600B9C3 RID: 47555 RVA: 0x00244D78 File Offset: 0x00242F78
			// (set) Token: 0x0600B9C4 RID: 47556 RVA: 0x00244DBC File Offset: 0x00242FBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_IS_FUTURE_REP_ALLOWED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_IS_FUTURE_REP_ALLOWED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn] = value;
				}
			}

			// Token: 0x17003918 RID: 14616
			// (get) Token: 0x0600B9C5 RID: 47557 RVA: 0x00244DD8 File Offset: 0x00242FD8
			// (set) Token: 0x0600B9C6 RID: 47558 RVA: 0x00244E1C File Offset: 0x0024301C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_TS_FIXED_APPROVAL_ROUTING
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_FIXED_APPROVAL_ROUTING' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn] = value;
				}
			}

			// Token: 0x17003919 RID: 14617
			// (get) Token: 0x0600B9C7 RID: 47559 RVA: 0x00244E38 File Offset: 0x00243038
			// (set) Token: 0x0600B9C8 RID: 47560 RVA: 0x00244E7C File Offset: 0x0024307C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_TIED_MODE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_TIED_MODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_TIED_MODE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_TIED_MODEColumn] = value;
				}
			}

			// Token: 0x1700391A RID: 14618
			// (get) Token: 0x0600B9C9 RID: 47561 RVA: 0x00244E95 File Offset: 0x00243095
			// (set) Token: 0x0600B9CA RID: 47562 RVA: 0x00244EAD File Offset: 0x002430AD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_MIN_HR_PER_TS
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_TS_MIN_HR_PER_TSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_MIN_HR_PER_TSColumn] = value;
				}
			}

			// Token: 0x1700391B RID: 14619
			// (get) Token: 0x0600B9CB RID: 47563 RVA: 0x00244EC6 File Offset: 0x002430C6
			// (set) Token: 0x0600B9CC RID: 47564 RVA: 0x00244EDE File Offset: 0x002430DE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal WADMIN_TS_MAX_HR_PER_TS
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_TS_MAX_HR_PER_TSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_MAX_HR_PER_TSColumn] = value;
				}
			}

			// Token: 0x1700391C RID: 14620
			// (get) Token: 0x0600B9CD RID: 47565 RVA: 0x00244EF7 File Offset: 0x002430F7
			// (set) Token: 0x0600B9CE RID: 47566 RVA: 0x00244F0F File Offset: 0x0024310F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_MAX_HR_PER_DAY
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_TS_MAX_HR_PER_DAYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_MAX_HR_PER_DAYColumn] = value;
				}
			}

			// Token: 0x1700391D RID: 14621
			// (get) Token: 0x0600B9CF RID: 47567 RVA: 0x00244F28 File Offset: 0x00243128
			// (set) Token: 0x0600B9D0 RID: 47568 RVA: 0x00244F40 File Offset: 0x00243140
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_TS_HOURS_PER_DAY
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_TS_HOURS_PER_DAYColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_HOURS_PER_DAYColumn] = value;
				}
			}

			// Token: 0x1700391E RID: 14622
			// (get) Token: 0x0600B9D1 RID: 47569 RVA: 0x00244F59 File Offset: 0x00243159
			// (set) Token: 0x0600B9D2 RID: 47570 RVA: 0x00244F71 File Offset: 0x00243171
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal WADMIN_TS_HOURS_PER_WEEK
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_TS_HOURS_PER_WEEKColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_HOURS_PER_WEEKColumn] = value;
				}
			}

			// Token: 0x1700391F RID: 14623
			// (get) Token: 0x0600B9D3 RID: 47571 RVA: 0x00244F8A File Offset: 0x0024318A
			// (set) Token: 0x0600B9D4 RID: 47572 RVA: 0x00244FA2 File Offset: 0x002431A2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_TS_DEF_DISPLAY_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_DEF_DISPLAY_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_DEF_DISPLAY_ENUMColumn] = value;
				}
			}

			// Token: 0x17003920 RID: 14624
			// (get) Token: 0x0600B9D5 RID: 47573 RVA: 0x00244FBB File Offset: 0x002431BB
			// (set) Token: 0x0600B9D6 RID: 47574 RVA: 0x00244FD3 File Offset: 0x002431D3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_TS_OUTLOOK_DISPLAY_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_OUTLOOK_DISPLAY_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_OUTLOOK_DISPLAY_ENUMColumn] = value;
				}
			}

			// Token: 0x17003921 RID: 14625
			// (get) Token: 0x0600B9D7 RID: 47575 RVA: 0x00244FEC File Offset: 0x002431EC
			// (set) Token: 0x0600B9D8 RID: 47576 RVA: 0x00245004 File Offset: 0x00243204
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_CREATE_MODE_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_CREATE_MODE_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_CREATE_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x17003922 RID: 14626
			// (get) Token: 0x0600B9D9 RID: 47577 RVA: 0x0024501D File Offset: 0x0024321D
			// (set) Token: 0x0600B9DA RID: 47578 RVA: 0x00245035 File Offset: 0x00243235
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_TS_REPORT_UNIT_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_REPORT_UNIT_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_REPORT_UNIT_ENUMColumn] = value;
				}
			}

			// Token: 0x17003923 RID: 14627
			// (get) Token: 0x0600B9DB RID: 47579 RVA: 0x0024504E File Offset: 0x0024324E
			// (set) Token: 0x0600B9DC RID: 47580 RVA: 0x00245066 File Offset: 0x00243266
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_WEEK_START_ON_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_WEEK_START_ON_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WEEK_START_ON_ENUMColumn] = value;
				}
			}

			// Token: 0x17003924 RID: 14628
			// (get) Token: 0x0600B9DD RID: 47581 RVA: 0x00245080 File Offset: 0x00243280
			// (set) Token: 0x0600B9DE RID: 47582 RVA: 0x002450C4 File Offset: 0x002432C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public decimal WADMIN_STAT_MAX_HR_PER_DAY
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_DAYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_MAX_HR_PER_DAY' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_DAYColumn] = value;
				}
			}

			// Token: 0x17003925 RID: 14629
			// (get) Token: 0x0600B9DF RID: 47583 RVA: 0x002450E0 File Offset: 0x002432E0
			// (set) Token: 0x0600B9E0 RID: 47584 RVA: 0x00245124 File Offset: 0x00243324
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal WADMIN_STAT_MAX_HR_PER_TASK
			{
				get
				{
					decimal result;
					try
					{
						result = (decimal)base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_TASKColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_MAX_HR_PER_TASK' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_TASKColumn] = value;
				}
			}

			// Token: 0x17003926 RID: 14630
			// (get) Token: 0x0600B9E1 RID: 47585 RVA: 0x0024513D File Offset: 0x0024333D
			// (set) Token: 0x0600B9E2 RID: 47586 RVA: 0x00245155 File Offset: 0x00243355
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_LOOK_AHEAD
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_STAT_LOOK_AHEADColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_LOOK_AHEADColumn] = value;
				}
			}

			// Token: 0x17003927 RID: 14631
			// (get) Token: 0x0600B9E3 RID: 47587 RVA: 0x0024516E File Offset: 0x0024336E
			// (set) Token: 0x0600B9E4 RID: 47588 RVA: 0x00245186 File Offset: 0x00243386
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_LOOK_AHEAD_PERIODS
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_STAT_LOOK_AHEAD_PERIODSColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_LOOK_AHEAD_PERIODSColumn] = value;
				}
			}

			// Token: 0x17003928 RID: 14632
			// (get) Token: 0x0600B9E5 RID: 47589 RVA: 0x002451A0 File Offset: 0x002433A0
			// (set) Token: 0x0600B9E6 RID: 47590 RVA: 0x002451E4 File Offset: 0x002433E4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_STAT_REP_SCHED_ENUM
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableWebAdmin.WADMIN_STAT_REP_SCHED_ENUMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_REP_SCHED_ENUM' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_REP_SCHED_ENUMColumn] = value;
				}
			}

			// Token: 0x17003929 RID: 14633
			// (get) Token: 0x0600B9E7 RID: 47591 RVA: 0x00245200 File Offset: 0x00243400
			// (set) Token: 0x0600B9E8 RID: 47592 RVA: 0x00245244 File Offset: 0x00243444
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_STAT_SPAN_MODE_ENUM
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableWebAdmin.WADMIN_STAT_SPAN_MODE_ENUMColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_SPAN_MODE_ENUM' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_SPAN_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x1700392A RID: 14634
			// (get) Token: 0x0600B9E9 RID: 47593 RVA: 0x0024525D File Offset: 0x0024345D
			// (set) Token: 0x0600B9EA RID: 47594 RVA: 0x00245275 File Offset: 0x00243475
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_TS_DEF_ENTRY_MODE_ENUM
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_DEF_ENTRY_MODE_ENUMColumn] = value;
				}
			}

			// Token: 0x1700392B RID: 14635
			// (get) Token: 0x0600B9EB RID: 47595 RVA: 0x00245290 File Offset: 0x00243490
			// (set) Token: 0x0600B9EC RID: 47596 RVA: 0x002452D4 File Offset: 0x002434D4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_STAT_PROT_ACT
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_STAT_PROT_ACTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_PROT_ACT' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_PROT_ACTColumn] = value;
				}
			}

			// Token: 0x1700392C RID: 14636
			// (get) Token: 0x0600B9ED RID: 47597 RVA: 0x002452ED File Offset: 0x002434ED
			// (set) Token: 0x0600B9EE RID: 47598 RVA: 0x00245305 File Offset: 0x00243505
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public byte WADMIN_STAT_PERIOD_TYPE
			{
				get
				{
					return (byte)base[this.tableWebAdmin.WADMIN_STAT_PERIOD_TYPEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_PERIOD_TYPEColumn] = value;
				}
			}

			// Token: 0x1700392D RID: 14637
			// (get) Token: 0x0600B9EF RID: 47599 RVA: 0x00245320 File Offset: 0x00243520
			// (set) Token: 0x0600B9F0 RID: 47600 RVA: 0x00245364 File Offset: 0x00243564
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_STAT_ENABLE_DOWNLOAD
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_STAT_ENABLE_DOWNLOADColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_ENABLE_DOWNLOAD' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_ENABLE_DOWNLOADColumn] = value;
				}
			}

			// Token: 0x1700392E RID: 14638
			// (get) Token: 0x0600B9F1 RID: 47601 RVA: 0x00245380 File Offset: 0x00243580
			// (set) Token: 0x0600B9F2 RID: 47602 RVA: 0x002453C4 File Offset: 0x002435C4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_STAT_NUM_WK_SPANNED
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_NUM_WK_SPANNEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_NUM_WK_SPANNED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_NUM_WK_SPANNEDColumn] = value;
				}
			}

			// Token: 0x1700392F RID: 14639
			// (get) Token: 0x0600B9F3 RID: 47603 RVA: 0x002453E0 File Offset: 0x002435E0
			// (set) Token: 0x0600B9F4 RID: 47604 RVA: 0x00245424 File Offset: 0x00243624
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_NUM_UPDATES_PER_MONTH
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_NUM_UPDATES_PER_MONTHColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_NUM_UPDATES_PER_MONTH' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_NUM_UPDATES_PER_MONTHColumn] = value;
				}
			}

			// Token: 0x17003930 RID: 14640
			// (get) Token: 0x0600B9F5 RID: 47605 RVA: 0x00245440 File Offset: 0x00243640
			// (set) Token: 0x0600B9F6 RID: 47606 RVA: 0x00245484 File Offset: 0x00243684
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_1PRD_1ST_START
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_1PRD_1ST_STARTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_1PRD_1ST_START' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_1PRD_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x17003931 RID: 14641
			// (get) Token: 0x0600B9F7 RID: 47607 RVA: 0x002454A0 File Offset: 0x002436A0
			// (set) Token: 0x0600B9F8 RID: 47608 RVA: 0x002454E4 File Offset: 0x002436E4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_2PRD_1ST_START
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_STARTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_2PRD_1ST_START' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x17003932 RID: 14642
			// (get) Token: 0x0600B9F9 RID: 47609 RVA: 0x00245500 File Offset: 0x00243700
			// (set) Token: 0x0600B9FA RID: 47610 RVA: 0x00245544 File Offset: 0x00243744
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_STAT_2PRD_1ST_END
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_ENDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_2PRD_1ST_END' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_ENDColumn] = value;
				}
			}

			// Token: 0x17003933 RID: 14643
			// (get) Token: 0x0600B9FB RID: 47611 RVA: 0x00245560 File Offset: 0x00243760
			// (set) Token: 0x0600B9FC RID: 47612 RVA: 0x002455A4 File Offset: 0x002437A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_3PRD_1ST_START
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_STARTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_3PRD_1ST_START' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_STARTColumn] = value;
				}
			}

			// Token: 0x17003934 RID: 14644
			// (get) Token: 0x0600B9FD RID: 47613 RVA: 0x002455C0 File Offset: 0x002437C0
			// (set) Token: 0x0600B9FE RID: 47614 RVA: 0x00245604 File Offset: 0x00243804
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_STAT_3PRD_1ST_END
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_ENDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_3PRD_1ST_END' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_ENDColumn] = value;
				}
			}

			// Token: 0x17003935 RID: 14645
			// (get) Token: 0x0600B9FF RID: 47615 RVA: 0x00245620 File Offset: 0x00243820
			// (set) Token: 0x0600BA00 RID: 47616 RVA: 0x00245664 File Offset: 0x00243864
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_STAT_3PRD_2ND_END
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_STAT_3PRD_2ND_ENDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_3PRD_2ND_END' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_3PRD_2ND_ENDColumn] = value;
				}
			}

			// Token: 0x17003936 RID: 14646
			// (get) Token: 0x0600BA01 RID: 47617 RVA: 0x0024567D File Offset: 0x0024387D
			// (set) Token: 0x0600BA02 RID: 47618 RVA: 0x00245695 File Offset: 0x00243895
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_LIST_SEPARATOR
			{
				get
				{
					return (string)base[this.tableWebAdmin.WADMIN_LIST_SEPARATORColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LIST_SEPARATORColumn] = value;
				}
			}

			// Token: 0x17003937 RID: 14647
			// (get) Token: 0x0600BA03 RID: 47619 RVA: 0x002456AC File Offset: 0x002438AC
			// (set) Token: 0x0600BA04 RID: 47620 RVA: 0x002456F0 File Offset: 0x002438F0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_ACTIVE_CACHE_DIR
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_DIRColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_ACTIVE_CACHE_DIR' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_DIRColumn] = value;
				}
			}

			// Token: 0x17003938 RID: 14648
			// (get) Token: 0x0600BA05 RID: 47621 RVA: 0x00245704 File Offset: 0x00243904
			// (set) Token: 0x0600BA06 RID: 47622 RVA: 0x00245748 File Offset: 0x00243948
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_ACTIVE_CACHE_MAX_SIZE_MB
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_MAX_SIZE_MBColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_ACTIVE_CACHE_MAX_SIZE_MB' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_MAX_SIZE_MBColumn] = value;
				}
			}

			// Token: 0x17003939 RID: 14649
			// (get) Token: 0x0600BA07 RID: 47623 RVA: 0x00245761 File Offset: 0x00243961
			// (set) Token: 0x0600BA08 RID: 47624 RVA: 0x00245779 File Offset: 0x00243979
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public long WADMIN_RES_SCHEDULED_TIME
			{
				get
				{
					return (long)base[this.tableWebAdmin.WADMIN_RES_SCHEDULED_TIMEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_RES_SCHEDULED_TIMEColumn] = value;
				}
			}

			// Token: 0x1700393A RID: 14650
			// (get) Token: 0x0600BA09 RID: 47625 RVA: 0x00245792 File Offset: 0x00243992
			// (set) Token: 0x0600BA0A RID: 47626 RVA: 0x002457AA File Offset: 0x002439AA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public decimal WADMIN_RESPLAN_FTE
			{
				get
				{
					return (decimal)base[this.tableWebAdmin.WADMIN_RESPLAN_FTEColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_RESPLAN_FTEColumn] = value;
				}
			}

			// Token: 0x1700393B RID: 14651
			// (get) Token: 0x0600BA0B RID: 47627 RVA: 0x002457C3 File Offset: 0x002439C3
			// (set) Token: 0x0600BA0C RID: 47628 RVA: 0x002457DB File Offset: 0x002439DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_RESOURCE_CAPACITY_MONTHS_BEHIND
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_MONTHS_BEHINDColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_MONTHS_BEHINDColumn] = value;
				}
			}

			// Token: 0x1700393C RID: 14652
			// (get) Token: 0x0600BA0D RID: 47629 RVA: 0x002457F4 File Offset: 0x002439F4
			// (set) Token: 0x0600BA0E RID: 47630 RVA: 0x0024580C File Offset: 0x00243A0C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_RESOURCE_CAPACITY_MONTHS_AHEAD
			{
				get
				{
					return (int)base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_MONTHS_AHEADColumn];
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_MONTHS_AHEADColumn] = value;
				}
			}

			// Token: 0x1700393D RID: 14653
			// (get) Token: 0x0600BA0F RID: 47631 RVA: 0x00245828 File Offset: 0x00243A28
			// (set) Token: 0x0600BA10 RID: 47632 RVA: 0x0024586C File Offset: 0x00243A6C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_RESOURCE_CAPACITY_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_RESOURCE_CAPACITY_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x1700393E RID: 14654
			// (get) Token: 0x0600BA11 RID: 47633 RVA: 0x00245888 File Offset: 0x00243A88
			// (set) Token: 0x0600BA12 RID: 47634 RVA: 0x002458CC File Offset: 0x00243ACC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WADMIN_REMINDER_TIMER_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_REMINDER_TIMER_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_REMINDER_TIMER_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_REMINDER_TIMER_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x1700393F RID: 14655
			// (get) Token: 0x0600BA13 RID: 47635 RVA: 0x002458E8 File Offset: 0x00243AE8
			// (set) Token: 0x0600BA14 RID: 47636 RVA: 0x0024592C File Offset: 0x00243B2C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_USE_PROJECT_STATE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_USE_PROJECT_STATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_USE_PROJECT_STATE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_USE_PROJECT_STATEColumn] = value;
				}
			}

			// Token: 0x17003940 RID: 14656
			// (get) Token: 0x0600BA15 RID: 47637 RVA: 0x00245948 File Offset: 0x00243B48
			// (set) Token: 0x0600BA16 RID: 47638 RVA: 0x0024598C File Offset: 0x00243B8C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_SHOW_WSS_NAV_LINKS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_SHOW_WSS_NAV_LINKSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SHOW_WSS_NAV_LINKS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SHOW_WSS_NAV_LINKSColumn] = value;
				}
			}

			// Token: 0x17003941 RID: 14657
			// (get) Token: 0x0600BA17 RID: 47639 RVA: 0x002459A8 File Offset: 0x00243BA8
			// (set) Token: 0x0600BA18 RID: 47640 RVA: 0x002459EC File Offset: 0x00243BEC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_ALWAYS_EXPAND_NAV_LINKS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_ALWAYS_EXPAND_NAV_LINKS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn] = value;
				}
			}

			// Token: 0x17003942 RID: 14658
			// (get) Token: 0x0600BA19 RID: 47641 RVA: 0x00245A08 File Offset: 0x00243C08
			// (set) Token: 0x0600BA1A RID: 47642 RVA: 0x00245A4C File Offset: 0x00243C4C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_WORKFLOW_PROXY_ACCT
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_ACCTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WORKFLOW_PROXY_ACCT' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_ACCTColumn] = value;
				}
			}

			// Token: 0x17003943 RID: 14659
			// (get) Token: 0x0600BA1B RID: 47643 RVA: 0x00245A60 File Offset: 0x00243C60
			// (set) Token: 0x0600BA1C RID: 47644 RVA: 0x00245AA4 File Offset: 0x00243CA4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WADMIN_WORKFLOW_PROXY_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WORKFLOW_PROXY_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_UIDColumn] = value;
				}
			}

			// Token: 0x17003944 RID: 14660
			// (get) Token: 0x0600BA1D RID: 47645 RVA: 0x00245AC0 File Offset: 0x00243CC0
			// (set) Token: 0x0600BA1E RID: 47646 RVA: 0x00245B04 File Offset: 0x00243D04
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_WORKFLOW_PROXY_WINDOWS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_WINDOWSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WORKFLOW_PROXY_WINDOWS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_WINDOWSColumn] = value;
				}
			}

			// Token: 0x17003945 RID: 14661
			// (get) Token: 0x0600BA1F RID: 47647 RVA: 0x00245B20 File Offset: 0x00243D20
			// (set) Token: 0x0600BA20 RID: 47648 RVA: 0x00245B64 File Offset: 0x00243D64
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_WORKFLOW_PROXY_MOD_BY
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_BYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WORKFLOW_PROXY_MOD_BY' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_BYColumn] = value;
				}
			}

			// Token: 0x17003946 RID: 14662
			// (get) Token: 0x0600BA21 RID: 47649 RVA: 0x00245B78 File Offset: 0x00243D78
			// (set) Token: 0x0600BA22 RID: 47650 RVA: 0x00245BBC File Offset: 0x00243DBC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DateTime WADMIN_WORKFLOW_PROXY_MOD_DATE
			{
				get
				{
					DateTime result;
					try
					{
						result = (DateTime)base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_DATEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WORKFLOW_PROXY_MOD_DATE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17003947 RID: 14663
			// (get) Token: 0x0600BA23 RID: 47651 RVA: 0x00245BD5 File Offset: 0x00243DD5
			// (set) Token: 0x0600BA24 RID: 47652 RVA: 0x00245BED File Offset: 0x00243DED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime CREATED_DATE
			{
				get
				{
					return (DateTime)base[this.tableWebAdmin.CREATED_DATEColumn];
				}
				set
				{
					base[this.tableWebAdmin.CREATED_DATEColumn] = value;
				}
			}

			// Token: 0x17003948 RID: 14664
			// (get) Token: 0x0600BA25 RID: 47653 RVA: 0x00245C06 File Offset: 0x00243E06
			// (set) Token: 0x0600BA26 RID: 47654 RVA: 0x00245C1E File Offset: 0x00243E1E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public DateTime MOD_DATE
			{
				get
				{
					return (DateTime)base[this.tableWebAdmin.MOD_DATEColumn];
				}
				set
				{
					base[this.tableWebAdmin.MOD_DATEColumn] = value;
				}
			}

			// Token: 0x17003949 RID: 14665
			// (get) Token: 0x0600BA27 RID: 47655 RVA: 0x00245C37 File Offset: 0x00243E37
			// (set) Token: 0x0600BA28 RID: 47656 RVA: 0x00245C4F File Offset: 0x00243E4F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int CREATED_REV_COUNTER
			{
				get
				{
					return (int)base[this.tableWebAdmin.CREATED_REV_COUNTERColumn];
				}
				set
				{
					base[this.tableWebAdmin.CREATED_REV_COUNTERColumn] = value;
				}
			}

			// Token: 0x1700394A RID: 14666
			// (get) Token: 0x0600BA29 RID: 47657 RVA: 0x00245C68 File Offset: 0x00243E68
			// (set) Token: 0x0600BA2A RID: 47658 RVA: 0x00245C80 File Offset: 0x00243E80
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int MOD_REV_COUNTER
			{
				get
				{
					return (int)base[this.tableWebAdmin.MOD_REV_COUNTERColumn];
				}
				set
				{
					base[this.tableWebAdmin.MOD_REV_COUNTERColumn] = value;
				}
			}

			// Token: 0x1700394B RID: 14667
			// (get) Token: 0x0600BA2B RID: 47659 RVA: 0x00245C9C File Offset: 0x00243E9C
			// (set) Token: 0x0600BA2C RID: 47660 RVA: 0x00245CE0 File Offset: 0x00243EE0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_SERVER_FLAGS
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_SERVER_FLAGSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SERVER_FLAGS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SERVER_FLAGSColumn] = value;
				}
			}

			// Token: 0x1700394C RID: 14668
			// (get) Token: 0x0600BA2D RID: 47661 RVA: 0x00245CFC File Offset: 0x00243EFC
			// (set) Token: 0x0600BA2E RID: 47662 RVA: 0x00245D40 File Offset: 0x00243F40
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public string WADMIN_MIN_WINPROJ_BUILD_NUMBERS
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_MIN_WINPROJ_BUILD_NUMBERSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_MIN_WINPROJ_BUILD_NUMBERS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MIN_WINPROJ_BUILD_NUMBERSColumn] = value;
				}
			}

			// Token: 0x1700394D RID: 14669
			// (get) Token: 0x0600BA2F RID: 47663 RVA: 0x00245D54 File Offset: 0x00243F54
			// (set) Token: 0x0600BA30 RID: 47664 RVA: 0x00245D98 File Offset: 0x00243F98
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_EXCHANGE_INTEGRATION_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_EXCHANGE_INTEGRATION_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EXCHANGE_INTEGRATION_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXCHANGE_INTEGRATION_ENABLEDColumn] = value;
				}
			}

			// Token: 0x1700394E RID: 14670
			// (get) Token: 0x0600BA31 RID: 47665 RVA: 0x00245DB4 File Offset: 0x00243FB4
			// (set) Token: 0x0600BA32 RID: 47666 RVA: 0x00245DF8 File Offset: 0x00243FF8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_EXCHANGE_URL_REFRESH_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_EXCHANGE_URL_REFRESH_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EXCHANGE_URL_REFRESH_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXCHANGE_URL_REFRESH_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x1700394F RID: 14671
			// (get) Token: 0x0600BA33 RID: 47667 RVA: 0x00245E14 File Offset: 0x00244014
			// (set) Token: 0x0600BA34 RID: 47668 RVA: 0x00245E58 File Offset: 0x00244058
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x17003950 RID: 14672
			// (get) Token: 0x0600BA35 RID: 47669 RVA: 0x00245E74 File Offset: 0x00244074
			// (set) Token: 0x0600BA36 RID: 47670 RVA: 0x00245EB8 File Offset: 0x002440B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x17003951 RID: 14673
			// (get) Token: 0x0600BA37 RID: 47671 RVA: 0x00245ED4 File Offset: 0x002440D4
			// (set) Token: 0x0600BA38 RID: 47672 RVA: 0x00245F18 File Offset: 0x00244118
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_PUBLISH_MANUAL_TASKS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_PUBLISH_MANUAL_TASKSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PUBLISH_MANUAL_TASKS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PUBLISH_MANUAL_TASKSColumn] = value;
				}
			}

			// Token: 0x17003952 RID: 14674
			// (get) Token: 0x0600BA39 RID: 47673 RVA: 0x00245F34 File Offset: 0x00244134
			// (set) Token: 0x0600BA3A RID: 47674 RVA: 0x00245F78 File Offset: 0x00244178
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_SERVER_DEFAULT_TASK_MODE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_SERVER_DEFAULT_TASK_MODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SERVER_DEFAULT_TASK_MODE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SERVER_DEFAULT_TASK_MODEColumn] = value;
				}
			}

			// Token: 0x17003953 RID: 14675
			// (get) Token: 0x0600BA3B RID: 47675 RVA: 0x00245F94 File Offset: 0x00244194
			// (set) Token: 0x0600BA3C RID: 47676 RVA: 0x00245FD8 File Offset: 0x002441D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_LOCK_PRO_DEFAULT_TASK_MODE
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_LOCK_PRO_DEFAULT_TASK_MODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_LOCK_PRO_DEFAULT_TASK_MODE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LOCK_PRO_DEFAULT_TASK_MODEColumn] = value;
				}
			}

			// Token: 0x17003954 RID: 14676
			// (get) Token: 0x0600BA3D RID: 47677 RVA: 0x00245FF4 File Offset: 0x002441F4
			// (set) Token: 0x0600BA3E RID: 47678 RVA: 0x00246038 File Offset: 0x00244238
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_TS_ALLOW_PROJECT_LEVEL
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_TS_ALLOW_PROJECT_LEVELColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_TS_ALLOW_PROJECT_LEVEL' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_TS_ALLOW_PROJECT_LEVELColumn] = value;
				}
			}

			// Token: 0x17003955 RID: 14677
			// (get) Token: 0x0600BA3F RID: 47679 RVA: 0x00246054 File Offset: 0x00244254
			// (set) Token: 0x0600BA40 RID: 47680 RVA: 0x00246098 File Offset: 0x00244298
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDColumn] = value;
				}
			}

			// Token: 0x17003956 RID: 14678
			// (get) Token: 0x0600BA41 RID: 47681 RVA: 0x002460B4 File Offset: 0x002442B4
			// (set) Token: 0x0600BA42 RID: 47682 RVA: 0x002460F8 File Offset: 0x002442F8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_OFF_PEAK_SYNC_THRESHOLD
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_OFF_PEAK_SYNC_THRESHOLDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_OFF_PEAK_SYNC_THRESHOLD' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_OFF_PEAK_SYNC_THRESHOLDColumn] = value;
				}
			}

			// Token: 0x17003957 RID: 14679
			// (get) Token: 0x0600BA43 RID: 47683 RVA: 0x00246114 File Offset: 0x00244314
			// (set) Token: 0x0600BA44 RID: 47684 RVA: 0x00246158 File Offset: 0x00244358
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_DISABLED_SYNC_THRESHOLD
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_DISABLED_SYNC_THRESHOLDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_DISABLED_SYNC_THRESHOLD' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DISABLED_SYNC_THRESHOLDColumn] = value;
				}
			}

			// Token: 0x17003958 RID: 14680
			// (get) Token: 0x0600BA45 RID: 47685 RVA: 0x00246174 File Offset: 0x00244374
			// (set) Token: 0x0600BA46 RID: 47686 RVA: 0x002461B8 File Offset: 0x002443B8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDColumn] = value;
				}
			}

			// Token: 0x17003959 RID: 14681
			// (get) Token: 0x0600BA47 RID: 47687 RVA: 0x002461D4 File Offset: 0x002443D4
			// (set) Token: 0x0600BA48 RID: 47688 RVA: 0x00246218 File Offset: 0x00244418
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_STAT_IMPORT_LINE_CLASSES
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_STAT_IMPORT_LINE_CLASSESColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_IMPORT_LINE_CLASSES' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_IMPORT_LINE_CLASSESColumn] = value;
				}
			}

			// Token: 0x1700395A RID: 14682
			// (get) Token: 0x0600BA49 RID: 47689 RVA: 0x00246234 File Offset: 0x00244434
			// (set) Token: 0x0600BA4A RID: 47690 RVA: 0x00246278 File Offset: 0x00244478
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_DATABASE_CACHE_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_DATABASE_CACHE_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_DATABASE_CACHE_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_DATABASE_CACHE_ENABLEDColumn] = value;
				}
			}

			// Token: 0x1700395B RID: 14683
			// (get) Token: 0x0600BA4B RID: 47691 RVA: 0x00246294 File Offset: 0x00244494
			// (set) Token: 0x0600BA4C RID: 47692 RVA: 0x002462D8 File Offset: 0x002444D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_WSS_PWA_ADMIN_ROLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_WSS_PWA_ADMIN_ROLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WSS_PWA_ADMIN_ROLE_ID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WSS_PWA_ADMIN_ROLE_IDColumn] = value;
				}
			}

			// Token: 0x1700395C RID: 14684
			// (get) Token: 0x0600BA4D RID: 47693 RVA: 0x002462F4 File Offset: 0x002444F4
			// (set) Token: 0x0600BA4E RID: 47694 RVA: 0x00246338 File Offset: 0x00244538
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_ID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDColumn] = value;
				}
			}

			// Token: 0x1700395D RID: 14685
			// (get) Token: 0x0600BA4F RID: 47695 RVA: 0x00246354 File Offset: 0x00244554
			// (set) Token: 0x0600BA50 RID: 47696 RVA: 0x00246398 File Offset: 0x00244598
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_ID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDColumn] = value;
				}
			}

			// Token: 0x1700395E RID: 14686
			// (get) Token: 0x0600BA51 RID: 47697 RVA: 0x002463B4 File Offset: 0x002445B4
			// (set) Token: 0x0600BA52 RID: 47698 RVA: 0x002463F8 File Offset: 0x002445F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_WSS_PWA_READER_ROLE_ID
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_WSS_PWA_READER_ROLE_IDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WSS_PWA_READER_ROLE_ID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WSS_PWA_READER_ROLE_IDColumn] = value;
				}
			}

			// Token: 0x1700395F RID: 14687
			// (get) Token: 0x0600BA53 RID: 47699 RVA: 0x00246414 File Offset: 0x00244614
			// (set) Token: 0x0600BA54 RID: 47700 RVA: 0x00246458 File Offset: 0x00244658
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_STAT_ALLOW_FREEFORM_PERIODS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_STAT_ALLOW_FREEFORM_PERIODSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_ALLOW_FREEFORM_PERIODS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_ALLOW_FREEFORM_PERIODSColumn] = value;
				}
			}

			// Token: 0x17003960 RID: 14688
			// (get) Token: 0x0600BA55 RID: 47701 RVA: 0x00246474 File Offset: 0x00244674
			// (set) Token: 0x0600BA56 RID: 47702 RVA: 0x002464B8 File Offset: 0x002446B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_STAT_TIMESHEET_TIED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_STAT_TIMESHEET_TIEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_STAT_TIMESHEET_TIED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_STAT_TIMESHEET_TIEDColumn] = value;
				}
			}

			// Token: 0x17003961 RID: 14689
			// (get) Token: 0x0600BA57 RID: 47703 RVA: 0x002464D4 File Offset: 0x002446D4
			// (set) Token: 0x0600BA58 RID: 47704 RVA: 0x00246518 File Offset: 0x00244718
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLIS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISColumn] = value;
				}
			}

			// Token: 0x17003962 RID: 14690
			// (get) Token: 0x0600BA59 RID: 47705 RVA: 0x00246534 File Offset: 0x00244734
			// (set) Token: 0x0600BA5A RID: 47706 RVA: 0x00246578 File Offset: 0x00244778
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLIS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISColumn] = value;
				}
			}

			// Token: 0x17003963 RID: 14691
			// (get) Token: 0x0600BA5B RID: 47707 RVA: 0x00246594 File Offset: 0x00244794
			// (set) Token: 0x0600BA5C RID: 47708 RVA: 0x002465D8 File Offset: 0x002447D8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_MAX_SQL_BATCH_SIZE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_MAX_SQL_BATCH_SIZEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_MAX_SQL_BATCH_SIZE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MAX_SQL_BATCH_SIZEColumn] = value;
				}
			}

			// Token: 0x17003964 RID: 14692
			// (get) Token: 0x0600BA5D RID: 47709 RVA: 0x002465F4 File Offset: 0x002447F4
			// (set) Token: 0x0600BA5E RID: 47710 RVA: 0x00246638 File Offset: 0x00244838
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_CORE_SQL_TIMEOUT
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_CORE_SQL_TIMEOUTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_CORE_SQL_TIMEOUT' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_CORE_SQL_TIMEOUTColumn] = value;
				}
			}

			// Token: 0x17003965 RID: 14693
			// (get) Token: 0x0600BA5F RID: 47711 RVA: 0x00246654 File Offset: 0x00244854
			// (set) Token: 0x0600BA60 RID: 47712 RVA: 0x00246698 File Offset: 0x00244898
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public int WADMIN_MAX_SSP_BATCH_SIZE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_MAX_SSP_BATCH_SIZEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_MAX_SSP_BATCH_SIZE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_MAX_SSP_BATCH_SIZEColumn] = value;
				}
			}

			// Token: 0x17003966 RID: 14694
			// (get) Token: 0x0600BA61 RID: 47713 RVA: 0x002466B4 File Offset: 0x002448B4
			// (set) Token: 0x0600BA62 RID: 47714 RVA: 0x002466F8 File Offset: 0x002448F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public short WADMIN_USER_SYNC_SETTING
			{
				get
				{
					short result;
					try
					{
						result = (short)base[this.tableWebAdmin.WADMIN_USER_SYNC_SETTINGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_USER_SYNC_SETTING' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_USER_SYNC_SETTINGColumn] = value;
				}
			}

			// Token: 0x17003967 RID: 14695
			// (get) Token: 0x0600BA63 RID: 47715 RVA: 0x00246714 File Offset: 0x00244914
			// (set) Token: 0x0600BA64 RID: 47716 RVA: 0x00246758 File Offset: 0x00244958
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_AD_SYNC_REPLACE_CHAR
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_AD_SYNC_REPLACE_CHARColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_AD_SYNC_REPLACE_CHAR' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_AD_SYNC_REPLACE_CHARColumn] = value;
				}
			}

			// Token: 0x17003968 RID: 14696
			// (get) Token: 0x0600BA65 RID: 47717 RVA: 0x0024676C File Offset: 0x0024496C
			// (set) Token: 0x0600BA66 RID: 47718 RVA: 0x002467B0 File Offset: 0x002449B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_SQL_BATCHING_ENABLED
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_SQL_BATCHING_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SQL_BATCHING_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SQL_BATCHING_ENABLEDColumn] = value;
				}
			}

			// Token: 0x17003969 RID: 14697
			// (get) Token: 0x0600BA67 RID: 47719 RVA: 0x002467CC File Offset: 0x002449CC
			// (set) Token: 0x0600BA68 RID: 47720 RVA: 0x00246810 File Offset: 0x00244A10
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public long WADMIN_SQL_BATCHING_BUFFER_SIZE
			{
				get
				{
					long result;
					try
					{
						result = (long)base[this.tableWebAdmin.WADMIN_SQL_BATCHING_BUFFER_SIZEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SQL_BATCHING_BUFFER_SIZE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SQL_BATCHING_BUFFER_SIZEColumn] = value;
				}
			}

			// Token: 0x1700396A RID: 14698
			// (get) Token: 0x0600BA69 RID: 47721 RVA: 0x0024682C File Offset: 0x00244A2C
			// (set) Token: 0x0600BA6A RID: 47722 RVA: 0x00246870 File Offset: 0x00244A70
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_WSS_RESTRICT_WORKSPACE_CREATION
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_WSS_RESTRICT_WORKSPACE_CREATIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_WSS_RESTRICT_WORKSPACE_CREATION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_WSS_RESTRICT_WORKSPACE_CREATIONColumn] = value;
				}
			}

			// Token: 0x1700396B RID: 14699
			// (get) Token: 0x0600BA6B RID: 47723 RVA: 0x0024688C File Offset: 0x00244A8C
			// (set) Token: 0x0600BA6C RID: 47724 RVA: 0x002468D0 File Offset: 0x00244AD0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_FULL_SYNC_THRESHOLD
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_FULL_SYNC_THRESHOLDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_FULL_SYNC_THRESHOLD' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_FULL_SYNC_THRESHOLDColumn] = value;
				}
			}

			// Token: 0x1700396C RID: 14700
			// (get) Token: 0x0600BA6D RID: 47725 RVA: 0x002468EC File Offset: 0x00244AEC
			// (set) Token: 0x0600BA6E RID: 47726 RVA: 0x00246930 File Offset: 0x00244B30
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid TIMESHEET_CURRENT_VIEWSET_UID
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.TIMESHEET_CURRENT_VIEWSET_UIDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'TIMESHEET_CURRENT_VIEWSET_UID' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.TIMESHEET_CURRENT_VIEWSET_UIDColumn] = value;
				}
			}

			// Token: 0x1700396D RID: 14701
			// (get) Token: 0x0600BA6F RID: 47727 RVA: 0x0024694C File Offset: 0x00244B4C
			// (set) Token: 0x0600BA70 RID: 47728 RVA: 0x00246990 File Offset: 0x00244B90
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_PERMISSION_MODE
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_PERMISSION_MODEColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PERMISSION_MODE' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PERMISSION_MODEColumn] = value;
				}
			}

			// Token: 0x1700396E RID: 14702
			// (get) Token: 0x0600BA71 RID: 47729 RVA: 0x002469AC File Offset: 0x00244BAC
			// (set) Token: 0x0600BA72 RID: 47730 RVA: 0x002469F0 File Offset: 0x00244BF0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public string WADMIN_SPPERMMODE_LAST_SYNC
			{
				get
				{
					string result;
					try
					{
						result = (string)base[this.tableWebAdmin.WADMIN_SPPERMMODE_LAST_SYNCColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SPPERMMODE_LAST_SYNC' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SPPERMMODE_LAST_SYNCColumn] = value;
				}
			}

			// Token: 0x1700396F RID: 14703
			// (get) Token: 0x0600BA73 RID: 47731 RVA: 0x00246A04 File Offset: 0x00244C04
			// (set) Token: 0x0600BA74 RID: 47732 RVA: 0x00246A48 File Offset: 0x00244C48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public Guid WADMIN_SITEMAP_CACHE_VERSION
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_SITEMAP_CACHE_VERSIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SITEMAP_CACHE_VERSION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SITEMAP_CACHE_VERSIONColumn] = value;
				}
			}

			// Token: 0x17003970 RID: 14704
			// (get) Token: 0x0600BA75 RID: 47733 RVA: 0x00246A64 File Offset: 0x00244C64
			// (set) Token: 0x0600BA76 RID: 47734 RVA: 0x00246AA8 File Offset: 0x00244CA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_GROUPINGGANTT_CACHE_VERSION
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_GROUPINGGANTT_CACHE_VERSIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_GROUPINGGANTT_CACHE_VERSION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_GROUPINGGANTT_CACHE_VERSIONColumn] = value;
				}
			}

			// Token: 0x17003971 RID: 14705
			// (get) Token: 0x0600BA77 RID: 47735 RVA: 0x00246AC4 File Offset: 0x00244CC4
			// (set) Token: 0x0600BA78 RID: 47736 RVA: 0x00246B08 File Offset: 0x00244D08
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDColumn] = value;
				}
			}

			// Token: 0x17003972 RID: 14706
			// (get) Token: 0x0600BA79 RID: 47737 RVA: 0x00246B24 File Offset: 0x00244D24
			// (set) Token: 0x0600BA7A RID: 47738 RVA: 0x00246B68 File Offset: 0x00244D68
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public Guid WADMIN_SETTINGS_VERSION
			{
				get
				{
					Guid result;
					try
					{
						result = (Guid)base[this.tableWebAdmin.WADMIN_SETTINGS_VERSIONColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SETTINGS_VERSION' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SETTINGS_VERSIONColumn] = value;
				}
			}

			// Token: 0x17003973 RID: 14707
			// (get) Token: 0x0600BA7B RID: 47739 RVA: 0x00246B84 File Offset: 0x00244D84
			// (set) Token: 0x0600BA7C RID: 47740 RVA: 0x00246BC8 File Offset: 0x00244DC8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_IS_UPDATING
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_IS_UPDATINGColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_UPDATING' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_UPDATINGColumn] = value;
				}
			}

			// Token: 0x17003974 RID: 14708
			// (get) Token: 0x0600BA7D RID: 47741 RVA: 0x00246BE4 File Offset: 0x00244DE4
			// (set) Token: 0x0600BA7E RID: 47742 RVA: 0x00246C28 File Offset: 0x00244E28
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public int WADMIN_PROVISIONING_RESULT
			{
				get
				{
					int result;
					try
					{
						result = (int)base[this.tableWebAdmin.WADMIN_PROVISIONING_RESULTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_PROVISIONING_RESULT' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_PROVISIONING_RESULTColumn] = value;
				}
			}

			// Token: 0x17003975 RID: 14709
			// (get) Token: 0x0600BA7F RID: 47743 RVA: 0x00246C44 File Offset: 0x00244E44
			// (set) Token: 0x0600BA80 RID: 47744 RVA: 0x00246C88 File Offset: 0x00244E88
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_LOGICAL_READONLY
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_LOGICAL_READONLYColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_LOGICAL_READONLY' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_LOGICAL_READONLYColumn] = value;
				}
			}

			// Token: 0x17003976 RID: 14710
			// (get) Token: 0x0600BA81 RID: 47745 RVA: 0x00246CA4 File Offset: 0x00244EA4
			// (set) Token: 0x0600BA82 RID: 47746 RVA: 0x00246CE8 File Offset: 0x00244EE8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_OVER_QUOTA
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_OVER_QUOTAColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_OVER_QUOTA' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_OVER_QUOTAColumn] = value;
				}
			}

			// Token: 0x17003977 RID: 14711
			// (get) Token: 0x0600BA83 RID: 47747 RVA: 0x00246D04 File Offset: 0x00244F04
			// (set) Token: 0x0600BA84 RID: 47748 RVA: 0x00246D48 File Offset: 0x00244F48
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_IS_DELETED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_IS_DELETEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_DELETED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_DELETEDColumn] = value;
				}
			}

			// Token: 0x17003978 RID: 14712
			// (get) Token: 0x0600BA85 RID: 47749 RVA: 0x00246D64 File Offset: 0x00244F64
			// (set) Token: 0x0600BA86 RID: 47750 RVA: 0x00246DA8 File Offset: 0x00244FA8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public byte WADMIN_SYNC_TASKS_TO_TASKLIST
			{
				get
				{
					byte result;
					try
					{
						result = (byte)base[this.tableWebAdmin.WADMIN_SYNC_TASKS_TO_TASKLISTColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_SYNC_TASKS_TO_TASKLIST' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_SYNC_TASKS_TO_TASKLISTColumn] = value;
				}
			}

			// Token: 0x17003979 RID: 14713
			// (get) Token: 0x0600BA87 RID: 47751 RVA: 0x00246DC4 File Offset: 0x00244FC4
			// (set) Token: 0x0600BA88 RID: 47752 RVA: 0x00246E08 File Offset: 0x00245008
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool WADMIN_USE_ENGAGEMENTS
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_USE_ENGAGEMENTSColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_USE_ENGAGEMENTS' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_USE_ENGAGEMENTSColumn] = value;
				}
			}

			// Token: 0x1700397A RID: 14714
			// (get) Token: 0x0600BA89 RID: 47753 RVA: 0x00246E24 File Offset: 0x00245024
			// (set) Token: 0x0600BA8A RID: 47754 RVA: 0x00246E68 File Offset: 0x00245068
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool WADMIN_IS_NOTIFICATION_ENABLED
			{
				get
				{
					bool result;
					try
					{
						result = (bool)base[this.tableWebAdmin.WADMIN_IS_NOTIFICATION_ENABLEDColumn];
					}
					catch (InvalidCastException innerException)
					{
						throw new StrongTypingException("The value for column 'WADMIN_IS_NOTIFICATION_ENABLED' in table 'WebAdmin' is DBNull.", innerException);
					}
					return result;
				}
				set
				{
					base[this.tableWebAdmin.WADMIN_IS_NOTIFICATION_ENABLEDColumn] = value;
				}
			}

			// Token: 0x0600BA8B RID: 47755 RVA: 0x00246E81 File Offset: 0x00245081
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_NTFY_FROM_EMAILNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_NTFY_FROM_EMAILColumn);
			}

			// Token: 0x0600BA8C RID: 47756 RVA: 0x00246E94 File Offset: 0x00245094
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_NTFY_FROM_EMAILNull()
			{
				base[this.tableWebAdmin.WADMIN_NTFY_FROM_EMAILColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA8D RID: 47757 RVA: 0x00246EAC File Offset: 0x002450AC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_NTFY_EMAIL_TRAILERNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_NTFY_EMAIL_TRAILERColumn);
			}

			// Token: 0x0600BA8E RID: 47758 RVA: 0x00246EBF File Offset: 0x002450BF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_NTFY_EMAIL_TRAILERNull()
			{
				base[this.tableWebAdmin.WADMIN_NTFY_EMAIL_TRAILERColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA8F RID: 47759 RVA: 0x00246ED7 File Offset: 0x002450D7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_ORG_EMAIL_ADDRESSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_ORG_EMAIL_ADDRESSColumn);
			}

			// Token: 0x0600BA90 RID: 47760 RVA: 0x00246EEA File Offset: 0x002450EA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_ORG_EMAIL_ADDRESSNull()
			{
				base[this.tableWebAdmin.WADMIN_ORG_EMAIL_ADDRESSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA91 RID: 47761 RVA: 0x00246F02 File Offset: 0x00245102
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_EMAIL_CHARSETNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EMAIL_CHARSETColumn);
			}

			// Token: 0x0600BA92 RID: 47762 RVA: 0x00246F15 File Offset: 0x00245115
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_EMAIL_CHARSETNull()
			{
				base[this.tableWebAdmin.WADMIN_EMAIL_CHARSETColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA93 RID: 47763 RVA: 0x00246F2D File Offset: 0x0024512D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_IS_TRACKING_METHOD_LOCKEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn);
			}

			// Token: 0x0600BA94 RID: 47764 RVA: 0x00246F40 File Offset: 0x00245140
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_IS_TRACKING_METHOD_LOCKEDNull()
			{
				base[this.tableWebAdmin.WADMIN_IS_TRACKING_METHOD_LOCKEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA95 RID: 47765 RVA: 0x00246F58 File Offset: 0x00245158
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_CURRENT_STS_SERVER_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_CURRENT_STS_SERVER_UIDColumn);
			}

			// Token: 0x0600BA96 RID: 47766 RVA: 0x00246F6B File Offset: 0x0024516B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_CURRENT_STS_SERVER_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_CURRENT_STS_SERVER_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA97 RID: 47767 RVA: 0x00246F83 File Offset: 0x00245183
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_DEFAULT_SITE_COLLECTIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_DEFAULT_SITE_COLLECTIONColumn);
			}

			// Token: 0x0600BA98 RID: 47768 RVA: 0x00246F96 File Offset: 0x00245196
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_DEFAULT_SITE_COLLECTIONNull()
			{
				base[this.tableWebAdmin.WADMIN_DEFAULT_SITE_COLLECTIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA99 RID: 47769 RVA: 0x00246FAE File Offset: 0x002451AE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_LAST_STS_ADMIN_SYNCH_TIMENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_LAST_STS_ADMIN_SYNCH_TIMEColumn);
			}

			// Token: 0x0600BA9A RID: 47770 RVA: 0x00246FC1 File Offset: 0x002451C1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_LAST_STS_ADMIN_SYNCH_TIMENull()
			{
				base[this.tableWebAdmin.WADMIN_LAST_STS_ADMIN_SYNCH_TIMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA9B RID: 47771 RVA: 0x00246FD9 File Offset: 0x002451D9
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SMTP_SERVER_NAMENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SMTP_SERVER_NAMEColumn);
			}

			// Token: 0x0600BA9C RID: 47772 RVA: 0x00246FEC File Offset: 0x002451EC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SMTP_SERVER_NAMENull()
			{
				base[this.tableWebAdmin.WADMIN_SMTP_SERVER_NAMEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA9D RID: 47773 RVA: 0x00247004 File Offset: 0x00245204
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SMTP_SERVER_PORTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SMTP_SERVER_PORTColumn);
			}

			// Token: 0x0600BA9E RID: 47774 RVA: 0x00247017 File Offset: 0x00245217
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_SMTP_SERVER_PORTNull()
			{
				base[this.tableWebAdmin.WADMIN_SMTP_SERVER_PORTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BA9F RID: 47775 RVA: 0x0024702F File Offset: 0x0024522F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STS_TEMPLATE_LCIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STS_TEMPLATE_LCIDColumn);
			}

			// Token: 0x0600BAA0 RID: 47776 RVA: 0x00247042 File Offset: 0x00245242
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STS_TEMPLATE_LCIDNull()
			{
				base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_LCIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAA1 RID: 47777 RVA: 0x0024705A File Offset: 0x0024525A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STS_TEMPLATE_IDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STS_TEMPLATE_IDColumn);
			}

			// Token: 0x0600BAA2 RID: 47778 RVA: 0x0024706D File Offset: 0x0024526D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STS_TEMPLATE_IDNull()
			{
				base[this.tableWebAdmin.WADMIN_STS_TEMPLATE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAA3 RID: 47779 RVA: 0x00247085 File Offset: 0x00245285
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STS_PRIMARY_OWNER_EMAILNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STS_PRIMARY_OWNER_EMAILColumn);
			}

			// Token: 0x0600BAA4 RID: 47780 RVA: 0x00247098 File Offset: 0x00245298
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STS_PRIMARY_OWNER_EMAILNull()
			{
				base[this.tableWebAdmin.WADMIN_STS_PRIMARY_OWNER_EMAILColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAA5 RID: 47781 RVA: 0x002470B0 File Offset: 0x002452B0
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_USE_BASELINE_SUMMARY_DATANull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_USE_BASELINE_SUMMARY_DATAColumn);
			}

			// Token: 0x0600BAA6 RID: 47782 RVA: 0x002470C3 File Offset: 0x002452C3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_USE_BASELINE_SUMMARY_DATANull()
			{
				base[this.tableWebAdmin.WADMIN_USE_BASELINE_SUMMARY_DATAColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAA7 RID: 47783 RVA: 0x002470DB File Offset: 0x002452DB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_PROJECT_BUILDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PROJECT_BUILDColumn);
			}

			// Token: 0x0600BAA8 RID: 47784 RVA: 0x002470EE File Offset: 0x002452EE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_PROJECT_BUILDNull()
			{
				base[this.tableWebAdmin.WADMIN_PROJECT_BUILDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAA9 RID: 47785 RVA: 0x00247106 File Offset: 0x00245306
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_IS_UNVERS_TASK_ALLOWEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn);
			}

			// Token: 0x0600BAAA RID: 47786 RVA: 0x00247119 File Offset: 0x00245319
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_IS_UNVERS_TASK_ALLOWEDNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_IS_UNVERS_TASK_ALLOWEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAAB RID: 47787 RVA: 0x00247131 File Offset: 0x00245331
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_PROJECT_MANAGER_COORDINATIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn);
			}

			// Token: 0x0600BAAC RID: 47788 RVA: 0x00247144 File Offset: 0x00245344
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_PROJECT_MANAGER_COORDINATIONNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_COORDINATIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAAD RID: 47789 RVA: 0x0024715C File Offset: 0x0024535C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_PROJECT_MANAGER_APPROVALNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn);
			}

			// Token: 0x0600BAAE RID: 47790 RVA: 0x0024716F File Offset: 0x0024536F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_PROJECT_MANAGER_APPROVALNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_PROJECT_MANAGER_APPROVALColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAAF RID: 47791 RVA: 0x00247187 File Offset: 0x00245387
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_MAXIMUM_LINE_ITEMSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_MAXIMUM_LINE_ITEMSColumn);
			}

			// Token: 0x0600BAB0 RID: 47792 RVA: 0x0024719A File Offset: 0x0024539A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_TS_MAXIMUM_LINE_ITEMSNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_MAXIMUM_LINE_ITEMSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAB1 RID: 47793 RVA: 0x002471B2 File Offset: 0x002453B2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_IS_AUDIT_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_IS_AUDIT_ENABLEDColumn);
			}

			// Token: 0x0600BAB2 RID: 47794 RVA: 0x002471C5 File Offset: 0x002453C5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_IS_AUDIT_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_IS_AUDIT_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAB3 RID: 47795 RVA: 0x002471DD File Offset: 0x002453DD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_IS_FUTURE_REP_ALLOWEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn);
			}

			// Token: 0x0600BAB4 RID: 47796 RVA: 0x002471F0 File Offset: 0x002453F0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_TS_IS_FUTURE_REP_ALLOWEDNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_IS_FUTURE_REP_ALLOWEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAB5 RID: 47797 RVA: 0x00247208 File Offset: 0x00245408
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_FIXED_APPROVAL_ROUTINGNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn);
			}

			// Token: 0x0600BAB6 RID: 47798 RVA: 0x0024721B File Offset: 0x0024541B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_FIXED_APPROVAL_ROUTINGNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_FIXED_APPROVAL_ROUTINGColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAB7 RID: 47799 RVA: 0x00247233 File Offset: 0x00245433
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_TS_TIED_MODENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_TIED_MODEColumn);
			}

			// Token: 0x0600BAB8 RID: 47800 RVA: 0x00247246 File Offset: 0x00245446
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_TS_TIED_MODENull()
			{
				base[this.tableWebAdmin.WADMIN_TS_TIED_MODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAB9 RID: 47801 RVA: 0x0024725E File Offset: 0x0024545E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_MAX_HR_PER_DAYNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_DAYColumn);
			}

			// Token: 0x0600BABA RID: 47802 RVA: 0x00247271 File Offset: 0x00245471
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_MAX_HR_PER_DAYNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_DAYColumn] = Convert.DBNull;
			}

			// Token: 0x0600BABB RID: 47803 RVA: 0x00247289 File Offset: 0x00245489
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_MAX_HR_PER_TASKNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_TASKColumn);
			}

			// Token: 0x0600BABC RID: 47804 RVA: 0x0024729C File Offset: 0x0024549C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_MAX_HR_PER_TASKNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_MAX_HR_PER_TASKColumn] = Convert.DBNull;
			}

			// Token: 0x0600BABD RID: 47805 RVA: 0x002472B4 File Offset: 0x002454B4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_REP_SCHED_ENUMNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_REP_SCHED_ENUMColumn);
			}

			// Token: 0x0600BABE RID: 47806 RVA: 0x002472C7 File Offset: 0x002454C7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_REP_SCHED_ENUMNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_REP_SCHED_ENUMColumn] = Convert.DBNull;
			}

			// Token: 0x0600BABF RID: 47807 RVA: 0x002472DF File Offset: 0x002454DF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_SPAN_MODE_ENUMNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_SPAN_MODE_ENUMColumn);
			}

			// Token: 0x0600BAC0 RID: 47808 RVA: 0x002472F2 File Offset: 0x002454F2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_SPAN_MODE_ENUMNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_SPAN_MODE_ENUMColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAC1 RID: 47809 RVA: 0x0024730A File Offset: 0x0024550A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_STAT_PROT_ACTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_PROT_ACTColumn);
			}

			// Token: 0x0600BAC2 RID: 47810 RVA: 0x0024731D File Offset: 0x0024551D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_PROT_ACTNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_PROT_ACTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAC3 RID: 47811 RVA: 0x00247335 File Offset: 0x00245535
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_STAT_ENABLE_DOWNLOADNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_ENABLE_DOWNLOADColumn);
			}

			// Token: 0x0600BAC4 RID: 47812 RVA: 0x00247348 File Offset: 0x00245548
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_ENABLE_DOWNLOADNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_ENABLE_DOWNLOADColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAC5 RID: 47813 RVA: 0x00247360 File Offset: 0x00245560
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_NUM_WK_SPANNEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_NUM_WK_SPANNEDColumn);
			}

			// Token: 0x0600BAC6 RID: 47814 RVA: 0x00247373 File Offset: 0x00245573
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_NUM_WK_SPANNEDNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_NUM_WK_SPANNEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAC7 RID: 47815 RVA: 0x0024738B File Offset: 0x0024558B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_STAT_NUM_UPDATES_PER_MONTHNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_NUM_UPDATES_PER_MONTHColumn);
			}

			// Token: 0x0600BAC8 RID: 47816 RVA: 0x0024739E File Offset: 0x0024559E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_NUM_UPDATES_PER_MONTHNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_NUM_UPDATES_PER_MONTHColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAC9 RID: 47817 RVA: 0x002473B6 File Offset: 0x002455B6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_1PRD_1ST_STARTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_1PRD_1ST_STARTColumn);
			}

			// Token: 0x0600BACA RID: 47818 RVA: 0x002473C9 File Offset: 0x002455C9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_1PRD_1ST_STARTNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_1PRD_1ST_STARTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BACB RID: 47819 RVA: 0x002473E1 File Offset: 0x002455E1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_2PRD_1ST_STARTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_STARTColumn);
			}

			// Token: 0x0600BACC RID: 47820 RVA: 0x002473F4 File Offset: 0x002455F4
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_2PRD_1ST_STARTNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_STARTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BACD RID: 47821 RVA: 0x0024740C File Offset: 0x0024560C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_2PRD_1ST_ENDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_ENDColumn);
			}

			// Token: 0x0600BACE RID: 47822 RVA: 0x0024741F File Offset: 0x0024561F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_2PRD_1ST_ENDNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_2PRD_1ST_ENDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BACF RID: 47823 RVA: 0x00247437 File Offset: 0x00245637
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_3PRD_1ST_STARTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_STARTColumn);
			}

			// Token: 0x0600BAD0 RID: 47824 RVA: 0x0024744A File Offset: 0x0024564A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_3PRD_1ST_STARTNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_STARTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAD1 RID: 47825 RVA: 0x00247462 File Offset: 0x00245662
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_3PRD_1ST_ENDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_ENDColumn);
			}

			// Token: 0x0600BAD2 RID: 47826 RVA: 0x00247475 File Offset: 0x00245675
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_STAT_3PRD_1ST_ENDNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_3PRD_1ST_ENDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAD3 RID: 47827 RVA: 0x0024748D File Offset: 0x0024568D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_3PRD_2ND_ENDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_3PRD_2ND_ENDColumn);
			}

			// Token: 0x0600BAD4 RID: 47828 RVA: 0x002474A0 File Offset: 0x002456A0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_3PRD_2ND_ENDNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_3PRD_2ND_ENDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAD5 RID: 47829 RVA: 0x002474B8 File Offset: 0x002456B8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_ACTIVE_CACHE_DIRNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_ACTIVE_CACHE_DIRColumn);
			}

			// Token: 0x0600BAD6 RID: 47830 RVA: 0x002474CB File Offset: 0x002456CB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_ACTIVE_CACHE_DIRNull()
			{
				base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_DIRColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAD7 RID: 47831 RVA: 0x002474E3 File Offset: 0x002456E3
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_ACTIVE_CACHE_MAX_SIZE_MBNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_ACTIVE_CACHE_MAX_SIZE_MBColumn);
			}

			// Token: 0x0600BAD8 RID: 47832 RVA: 0x002474F6 File Offset: 0x002456F6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_ACTIVE_CACHE_MAX_SIZE_MBNull()
			{
				base[this.tableWebAdmin.WADMIN_ACTIVE_CACHE_MAX_SIZE_MBColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAD9 RID: 47833 RVA: 0x0024750E File Offset: 0x0024570E
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_RESOURCE_CAPACITY_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_JOB_UIDColumn);
			}

			// Token: 0x0600BADA RID: 47834 RVA: 0x00247521 File Offset: 0x00245721
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_RESOURCE_CAPACITY_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_RESOURCE_CAPACITY_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BADB RID: 47835 RVA: 0x00247539 File Offset: 0x00245739
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_REMINDER_TIMER_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_REMINDER_TIMER_JOB_UIDColumn);
			}

			// Token: 0x0600BADC RID: 47836 RVA: 0x0024754C File Offset: 0x0024574C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_REMINDER_TIMER_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_REMINDER_TIMER_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BADD RID: 47837 RVA: 0x00247564 File Offset: 0x00245764
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_USE_PROJECT_STATENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_USE_PROJECT_STATEColumn);
			}

			// Token: 0x0600BADE RID: 47838 RVA: 0x00247577 File Offset: 0x00245777
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_USE_PROJECT_STATENull()
			{
				base[this.tableWebAdmin.WADMIN_USE_PROJECT_STATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BADF RID: 47839 RVA: 0x0024758F File Offset: 0x0024578F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SHOW_WSS_NAV_LINKSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SHOW_WSS_NAV_LINKSColumn);
			}

			// Token: 0x0600BAE0 RID: 47840 RVA: 0x002475A2 File Offset: 0x002457A2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_SHOW_WSS_NAV_LINKSNull()
			{
				base[this.tableWebAdmin.WADMIN_SHOW_WSS_NAV_LINKSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAE1 RID: 47841 RVA: 0x002475BA File Offset: 0x002457BA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_ALWAYS_EXPAND_NAV_LINKSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn);
			}

			// Token: 0x0600BAE2 RID: 47842 RVA: 0x002475CD File Offset: 0x002457CD
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_ALWAYS_EXPAND_NAV_LINKSNull()
			{
				base[this.tableWebAdmin.WADMIN_ALWAYS_EXPAND_NAV_LINKSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAE3 RID: 47843 RVA: 0x002475E5 File Offset: 0x002457E5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WORKFLOW_PROXY_ACCTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_ACCTColumn);
			}

			// Token: 0x0600BAE4 RID: 47844 RVA: 0x002475F8 File Offset: 0x002457F8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_WORKFLOW_PROXY_ACCTNull()
			{
				base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_ACCTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAE5 RID: 47845 RVA: 0x00247610 File Offset: 0x00245810
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_WORKFLOW_PROXY_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_UIDColumn);
			}

			// Token: 0x0600BAE6 RID: 47846 RVA: 0x00247623 File Offset: 0x00245823
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WORKFLOW_PROXY_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAE7 RID: 47847 RVA: 0x0024763B File Offset: 0x0024583B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WORKFLOW_PROXY_WINDOWSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_WINDOWSColumn);
			}

			// Token: 0x0600BAE8 RID: 47848 RVA: 0x0024764E File Offset: 0x0024584E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WORKFLOW_PROXY_WINDOWSNull()
			{
				base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_WINDOWSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAE9 RID: 47849 RVA: 0x00247666 File Offset: 0x00245866
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WORKFLOW_PROXY_MOD_BYNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_BYColumn);
			}

			// Token: 0x0600BAEA RID: 47850 RVA: 0x00247679 File Offset: 0x00245879
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WORKFLOW_PROXY_MOD_BYNull()
			{
				base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_BYColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAEB RID: 47851 RVA: 0x00247691 File Offset: 0x00245891
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_WORKFLOW_PROXY_MOD_DATENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_DATEColumn);
			}

			// Token: 0x0600BAEC RID: 47852 RVA: 0x002476A4 File Offset: 0x002458A4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WORKFLOW_PROXY_MOD_DATENull()
			{
				base[this.tableWebAdmin.WADMIN_WORKFLOW_PROXY_MOD_DATEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAED RID: 47853 RVA: 0x002476BC File Offset: 0x002458BC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SERVER_FLAGSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SERVER_FLAGSColumn);
			}

			// Token: 0x0600BAEE RID: 47854 RVA: 0x002476CF File Offset: 0x002458CF
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SERVER_FLAGSNull()
			{
				base[this.tableWebAdmin.WADMIN_SERVER_FLAGSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAEF RID: 47855 RVA: 0x002476E7 File Offset: 0x002458E7
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_MIN_WINPROJ_BUILD_NUMBERSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_MIN_WINPROJ_BUILD_NUMBERSColumn);
			}

			// Token: 0x0600BAF0 RID: 47856 RVA: 0x002476FA File Offset: 0x002458FA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_MIN_WINPROJ_BUILD_NUMBERSNull()
			{
				base[this.tableWebAdmin.WADMIN_MIN_WINPROJ_BUILD_NUMBERSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAF1 RID: 47857 RVA: 0x00247712 File Offset: 0x00245912
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_EXCHANGE_INTEGRATION_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EXCHANGE_INTEGRATION_ENABLEDColumn);
			}

			// Token: 0x0600BAF2 RID: 47858 RVA: 0x00247725 File Offset: 0x00245925
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_EXCHANGE_INTEGRATION_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_EXCHANGE_INTEGRATION_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAF3 RID: 47859 RVA: 0x0024773D File Offset: 0x0024593D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_EXCHANGE_URL_REFRESH_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EXCHANGE_URL_REFRESH_JOB_UIDColumn);
			}

			// Token: 0x0600BAF4 RID: 47860 RVA: 0x00247750 File Offset: 0x00245950
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_EXCHANGE_URL_REFRESH_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_EXCHANGE_URL_REFRESH_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAF5 RID: 47861 RVA: 0x00247768 File Offset: 0x00245968
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDColumn);
			}

			// Token: 0x0600BAF6 RID: 47862 RVA: 0x0024777B File Offset: 0x0024597B
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_EXCHANGE_SUBSCRIPTION_REFRESH_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAF7 RID: 47863 RVA: 0x00247793 File Offset: 0x00245993
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDColumn);
			}

			// Token: 0x0600BAF8 RID: 47864 RVA: 0x002477A6 File Offset: 0x002459A6
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_EXCHANGE_CALENDAR_OOF_SYNC_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAF9 RID: 47865 RVA: 0x002477BE File Offset: 0x002459BE
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_PUBLISH_MANUAL_TASKSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PUBLISH_MANUAL_TASKSColumn);
			}

			// Token: 0x0600BAFA RID: 47866 RVA: 0x002477D1 File Offset: 0x002459D1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_PUBLISH_MANUAL_TASKSNull()
			{
				base[this.tableWebAdmin.WADMIN_PUBLISH_MANUAL_TASKSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAFB RID: 47867 RVA: 0x002477E9 File Offset: 0x002459E9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_SERVER_DEFAULT_TASK_MODENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SERVER_DEFAULT_TASK_MODEColumn);
			}

			// Token: 0x0600BAFC RID: 47868 RVA: 0x002477FC File Offset: 0x002459FC
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SERVER_DEFAULT_TASK_MODENull()
			{
				base[this.tableWebAdmin.WADMIN_SERVER_DEFAULT_TASK_MODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAFD RID: 47869 RVA: 0x00247814 File Offset: 0x00245A14
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_LOCK_PRO_DEFAULT_TASK_MODENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_LOCK_PRO_DEFAULT_TASK_MODEColumn);
			}

			// Token: 0x0600BAFE RID: 47870 RVA: 0x00247827 File Offset: 0x00245A27
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_LOCK_PRO_DEFAULT_TASK_MODENull()
			{
				base[this.tableWebAdmin.WADMIN_LOCK_PRO_DEFAULT_TASK_MODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BAFF RID: 47871 RVA: 0x0024783F File Offset: 0x00245A3F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_TS_ALLOW_PROJECT_LEVELNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_TS_ALLOW_PROJECT_LEVELColumn);
			}

			// Token: 0x0600BB00 RID: 47872 RVA: 0x00247852 File Offset: 0x00245A52
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_TS_ALLOW_PROJECT_LEVELNull()
			{
				base[this.tableWebAdmin.WADMIN_TS_ALLOW_PROJECT_LEVELColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB01 RID: 47873 RVA: 0x0024786A File Offset: 0x00245A6A
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDColumn);
			}

			// Token: 0x0600BB02 RID: 47874 RVA: 0x0024787D File Offset: 0x00245A7D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_EXCHANGE_OOF_INTEGRATION_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB03 RID: 47875 RVA: 0x00247895 File Offset: 0x00245A95
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_OFF_PEAK_SYNC_THRESHOLDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_OFF_PEAK_SYNC_THRESHOLDColumn);
			}

			// Token: 0x0600BB04 RID: 47876 RVA: 0x002478A8 File Offset: 0x00245AA8
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_OFF_PEAK_SYNC_THRESHOLDNull()
			{
				base[this.tableWebAdmin.WADMIN_OFF_PEAK_SYNC_THRESHOLDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB05 RID: 47877 RVA: 0x002478C0 File Offset: 0x00245AC0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_DISABLED_SYNC_THRESHOLDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_DISABLED_SYNC_THRESHOLDColumn);
			}

			// Token: 0x0600BB06 RID: 47878 RVA: 0x002478D3 File Offset: 0x00245AD3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_DISABLED_SYNC_THRESHOLDNull()
			{
				base[this.tableWebAdmin.WADMIN_DISABLED_SYNC_THRESHOLDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB07 RID: 47879 RVA: 0x002478EB File Offset: 0x00245AEB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDColumn);
			}

			// Token: 0x0600BB08 RID: 47880 RVA: 0x002478FE File Offset: 0x00245AFE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDNull()
			{
				base[this.tableWebAdmin.WADMIN_PSMODE_OFF_PEAK_SYNC_JOB_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB09 RID: 47881 RVA: 0x00247916 File Offset: 0x00245B16
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_IMPORT_LINE_CLASSESNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_IMPORT_LINE_CLASSESColumn);
			}

			// Token: 0x0600BB0A RID: 47882 RVA: 0x00247929 File Offset: 0x00245B29
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_IMPORT_LINE_CLASSESNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_IMPORT_LINE_CLASSESColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB0B RID: 47883 RVA: 0x00247941 File Offset: 0x00245B41
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_DATABASE_CACHE_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_DATABASE_CACHE_ENABLEDColumn);
			}

			// Token: 0x0600BB0C RID: 47884 RVA: 0x00247954 File Offset: 0x00245B54
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_DATABASE_CACHE_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_DATABASE_CACHE_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB0D RID: 47885 RVA: 0x0024796C File Offset: 0x00245B6C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_WSS_PWA_ADMIN_ROLE_IDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WSS_PWA_ADMIN_ROLE_IDColumn);
			}

			// Token: 0x0600BB0E RID: 47886 RVA: 0x0024797F File Offset: 0x00245B7F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_WSS_PWA_ADMIN_ROLE_IDNull()
			{
				base[this.tableWebAdmin.WADMIN_WSS_PWA_ADMIN_ROLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB0F RID: 47887 RVA: 0x00247997 File Offset: 0x00245B97
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDColumn);
			}

			// Token: 0x0600BB10 RID: 47888 RVA: 0x002479AA File Offset: 0x00245BAA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDNull()
			{
				base[this.tableWebAdmin.WADMIN_WSS_PWA_PROJECT_MANAGER_ROLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB11 RID: 47889 RVA: 0x002479C2 File Offset: 0x00245BC2
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDColumn);
			}

			// Token: 0x0600BB12 RID: 47890 RVA: 0x002479D5 File Offset: 0x00245BD5
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDNull()
			{
				base[this.tableWebAdmin.WADMIN_WSS_PWA_TEAM_MEMBER_ROLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB13 RID: 47891 RVA: 0x002479ED File Offset: 0x00245BED
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_WSS_PWA_READER_ROLE_IDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WSS_PWA_READER_ROLE_IDColumn);
			}

			// Token: 0x0600BB14 RID: 47892 RVA: 0x00247A00 File Offset: 0x00245C00
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_WSS_PWA_READER_ROLE_IDNull()
			{
				base[this.tableWebAdmin.WADMIN_WSS_PWA_READER_ROLE_IDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB15 RID: 47893 RVA: 0x00247A18 File Offset: 0x00245C18
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_STAT_ALLOW_FREEFORM_PERIODSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_ALLOW_FREEFORM_PERIODSColumn);
			}

			// Token: 0x0600BB16 RID: 47894 RVA: 0x00247A2B File Offset: 0x00245C2B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_ALLOW_FREEFORM_PERIODSNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_ALLOW_FREEFORM_PERIODSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB17 RID: 47895 RVA: 0x00247A43 File Offset: 0x00245C43
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_STAT_TIMESHEET_TIEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_STAT_TIMESHEET_TIEDColumn);
			}

			// Token: 0x0600BB18 RID: 47896 RVA: 0x00247A56 File Offset: 0x00245C56
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_STAT_TIMESHEET_TIEDNull()
			{
				base[this.tableWebAdmin.WADMIN_STAT_TIMESHEET_TIEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB19 RID: 47897 RVA: 0x00247A6E File Offset: 0x00245C6E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISColumn);
			}

			// Token: 0x0600BB1A RID: 47898 RVA: 0x00247A81 File Offset: 0x00245C81
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISNull()
			{
				base[this.tableWebAdmin.WADMIN_PROJECT_READ_LOCK_TIMEOUT_MILLISColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB1B RID: 47899 RVA: 0x00247A99 File Offset: 0x00245C99
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISColumn);
			}

			// Token: 0x0600BB1C RID: 47900 RVA: 0x00247AAC File Offset: 0x00245CAC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISNull()
			{
				base[this.tableWebAdmin.WADMIN_PROJECT_WRITE_LOCK_TIMEOUT_MILLISColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB1D RID: 47901 RVA: 0x00247AC4 File Offset: 0x00245CC4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_MAX_SQL_BATCH_SIZENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_MAX_SQL_BATCH_SIZEColumn);
			}

			// Token: 0x0600BB1E RID: 47902 RVA: 0x00247AD7 File Offset: 0x00245CD7
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_MAX_SQL_BATCH_SIZENull()
			{
				base[this.tableWebAdmin.WADMIN_MAX_SQL_BATCH_SIZEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB1F RID: 47903 RVA: 0x00247AEF File Offset: 0x00245CEF
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_CORE_SQL_TIMEOUTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_CORE_SQL_TIMEOUTColumn);
			}

			// Token: 0x0600BB20 RID: 47904 RVA: 0x00247B02 File Offset: 0x00245D02
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_CORE_SQL_TIMEOUTNull()
			{
				base[this.tableWebAdmin.WADMIN_CORE_SQL_TIMEOUTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB21 RID: 47905 RVA: 0x00247B1A File Offset: 0x00245D1A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_MAX_SSP_BATCH_SIZENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_MAX_SSP_BATCH_SIZEColumn);
			}

			// Token: 0x0600BB22 RID: 47906 RVA: 0x00247B2D File Offset: 0x00245D2D
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_MAX_SSP_BATCH_SIZENull()
			{
				base[this.tableWebAdmin.WADMIN_MAX_SSP_BATCH_SIZEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB23 RID: 47907 RVA: 0x00247B45 File Offset: 0x00245D45
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_USER_SYNC_SETTINGNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_USER_SYNC_SETTINGColumn);
			}

			// Token: 0x0600BB24 RID: 47908 RVA: 0x00247B58 File Offset: 0x00245D58
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_USER_SYNC_SETTINGNull()
			{
				base[this.tableWebAdmin.WADMIN_USER_SYNC_SETTINGColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB25 RID: 47909 RVA: 0x00247B70 File Offset: 0x00245D70
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_AD_SYNC_REPLACE_CHARNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_AD_SYNC_REPLACE_CHARColumn);
			}

			// Token: 0x0600BB26 RID: 47910 RVA: 0x00247B83 File Offset: 0x00245D83
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_AD_SYNC_REPLACE_CHARNull()
			{
				base[this.tableWebAdmin.WADMIN_AD_SYNC_REPLACE_CHARColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB27 RID: 47911 RVA: 0x00247B9B File Offset: 0x00245D9B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SQL_BATCHING_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SQL_BATCHING_ENABLEDColumn);
			}

			// Token: 0x0600BB28 RID: 47912 RVA: 0x00247BAE File Offset: 0x00245DAE
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SQL_BATCHING_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_SQL_BATCHING_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB29 RID: 47913 RVA: 0x00247BC6 File Offset: 0x00245DC6
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SQL_BATCHING_BUFFER_SIZENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SQL_BATCHING_BUFFER_SIZEColumn);
			}

			// Token: 0x0600BB2A RID: 47914 RVA: 0x00247BD9 File Offset: 0x00245DD9
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SQL_BATCHING_BUFFER_SIZENull()
			{
				base[this.tableWebAdmin.WADMIN_SQL_BATCHING_BUFFER_SIZEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB2B RID: 47915 RVA: 0x00247BF1 File Offset: 0x00245DF1
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_WSS_RESTRICT_WORKSPACE_CREATIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_WSS_RESTRICT_WORKSPACE_CREATIONColumn);
			}

			// Token: 0x0600BB2C RID: 47916 RVA: 0x00247C04 File Offset: 0x00245E04
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_WSS_RESTRICT_WORKSPACE_CREATIONNull()
			{
				base[this.tableWebAdmin.WADMIN_WSS_RESTRICT_WORKSPACE_CREATIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB2D RID: 47917 RVA: 0x00247C1C File Offset: 0x00245E1C
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_FULL_SYNC_THRESHOLDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_FULL_SYNC_THRESHOLDColumn);
			}

			// Token: 0x0600BB2E RID: 47918 RVA: 0x00247C2F File Offset: 0x00245E2F
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_FULL_SYNC_THRESHOLDNull()
			{
				base[this.tableWebAdmin.WADMIN_FULL_SYNC_THRESHOLDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB2F RID: 47919 RVA: 0x00247C47 File Offset: 0x00245E47
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsTIMESHEET_CURRENT_VIEWSET_UIDNull()
			{
				return base.IsNull(this.tableWebAdmin.TIMESHEET_CURRENT_VIEWSET_UIDColumn);
			}

			// Token: 0x0600BB30 RID: 47920 RVA: 0x00247C5A File Offset: 0x00245E5A
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetTIMESHEET_CURRENT_VIEWSET_UIDNull()
			{
				base[this.tableWebAdmin.TIMESHEET_CURRENT_VIEWSET_UIDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB31 RID: 47921 RVA: 0x00247C72 File Offset: 0x00245E72
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_PERMISSION_MODENull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PERMISSION_MODEColumn);
			}

			// Token: 0x0600BB32 RID: 47922 RVA: 0x00247C85 File Offset: 0x00245E85
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_PERMISSION_MODENull()
			{
				base[this.tableWebAdmin.WADMIN_PERMISSION_MODEColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB33 RID: 47923 RVA: 0x00247C9D File Offset: 0x00245E9D
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SPPERMMODE_LAST_SYNCNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SPPERMMODE_LAST_SYNCColumn);
			}

			// Token: 0x0600BB34 RID: 47924 RVA: 0x00247CB0 File Offset: 0x00245EB0
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_SPPERMMODE_LAST_SYNCNull()
			{
				base[this.tableWebAdmin.WADMIN_SPPERMMODE_LAST_SYNCColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB35 RID: 47925 RVA: 0x00247CC8 File Offset: 0x00245EC8
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SITEMAP_CACHE_VERSIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SITEMAP_CACHE_VERSIONColumn);
			}

			// Token: 0x0600BB36 RID: 47926 RVA: 0x00247CDB File Offset: 0x00245EDB
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SITEMAP_CACHE_VERSIONNull()
			{
				base[this.tableWebAdmin.WADMIN_SITEMAP_CACHE_VERSIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB37 RID: 47927 RVA: 0x00247CF3 File Offset: 0x00245EF3
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_GROUPINGGANTT_CACHE_VERSIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_GROUPINGGANTT_CACHE_VERSIONColumn);
			}

			// Token: 0x0600BB38 RID: 47928 RVA: 0x00247D06 File Offset: 0x00245F06
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_GROUPINGGANTT_CACHE_VERSIONNull()
			{
				base[this.tableWebAdmin.WADMIN_GROUPINGGANTT_CACHE_VERSIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB39 RID: 47929 RVA: 0x00247D1E File Offset: 0x00245F1E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDColumn);
			}

			// Token: 0x0600BB3A RID: 47930 RVA: 0x00247D31 File Offset: 0x00245F31
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_IS_PSPERMMODE_OFFPEAK_SYNC_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB3B RID: 47931 RVA: 0x00247D49 File Offset: 0x00245F49
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_SETTINGS_VERSIONNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SETTINGS_VERSIONColumn);
			}

			// Token: 0x0600BB3C RID: 47932 RVA: 0x00247D5C File Offset: 0x00245F5C
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_SETTINGS_VERSIONNull()
			{
				base[this.tableWebAdmin.WADMIN_SETTINGS_VERSIONColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB3D RID: 47933 RVA: 0x00247D74 File Offset: 0x00245F74
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_IS_UPDATINGNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_IS_UPDATINGColumn);
			}

			// Token: 0x0600BB3E RID: 47934 RVA: 0x00247D87 File Offset: 0x00245F87
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_IS_UPDATINGNull()
			{
				base[this.tableWebAdmin.WADMIN_IS_UPDATINGColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB3F RID: 47935 RVA: 0x00247D9F File Offset: 0x00245F9F
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_PROVISIONING_RESULTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_PROVISIONING_RESULTColumn);
			}

			// Token: 0x0600BB40 RID: 47936 RVA: 0x00247DB2 File Offset: 0x00245FB2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_PROVISIONING_RESULTNull()
			{
				base[this.tableWebAdmin.WADMIN_PROVISIONING_RESULTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB41 RID: 47937 RVA: 0x00247DCA File Offset: 0x00245FCA
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_LOGICAL_READONLYNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_LOGICAL_READONLYColumn);
			}

			// Token: 0x0600BB42 RID: 47938 RVA: 0x00247DDD File Offset: 0x00245FDD
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_LOGICAL_READONLYNull()
			{
				base[this.tableWebAdmin.WADMIN_LOGICAL_READONLYColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB43 RID: 47939 RVA: 0x00247DF5 File Offset: 0x00245FF5
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_OVER_QUOTANull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_OVER_QUOTAColumn);
			}

			// Token: 0x0600BB44 RID: 47940 RVA: 0x00247E08 File Offset: 0x00246008
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_OVER_QUOTANull()
			{
				base[this.tableWebAdmin.WADMIN_OVER_QUOTAColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB45 RID: 47941 RVA: 0x00247E20 File Offset: 0x00246020
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_IS_DELETEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_IS_DELETEDColumn);
			}

			// Token: 0x0600BB46 RID: 47942 RVA: 0x00247E33 File Offset: 0x00246033
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_IS_DELETEDNull()
			{
				base[this.tableWebAdmin.WADMIN_IS_DELETEDColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB47 RID: 47943 RVA: 0x00247E4B File Offset: 0x0024604B
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public bool IsWADMIN_SYNC_TASKS_TO_TASKLISTNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_SYNC_TASKS_TO_TASKLISTColumn);
			}

			// Token: 0x0600BB48 RID: 47944 RVA: 0x00247E5E File Offset: 0x0024605E
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_SYNC_TASKS_TO_TASKLISTNull()
			{
				base[this.tableWebAdmin.WADMIN_SYNC_TASKS_TO_TASKLISTColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB49 RID: 47945 RVA: 0x00247E76 File Offset: 0x00246076
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_USE_ENGAGEMENTSNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_USE_ENGAGEMENTSColumn);
			}

			// Token: 0x0600BB4A RID: 47946 RVA: 0x00247E89 File Offset: 0x00246089
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public void SetWADMIN_USE_ENGAGEMENTSNull()
			{
				base[this.tableWebAdmin.WADMIN_USE_ENGAGEMENTSColumn] = Convert.DBNull;
			}

			// Token: 0x0600BB4B RID: 47947 RVA: 0x00247EA1 File Offset: 0x002460A1
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public bool IsWADMIN_IS_NOTIFICATION_ENABLEDNull()
			{
				return base.IsNull(this.tableWebAdmin.WADMIN_IS_NOTIFICATION_ENABLEDColumn);
			}

			// Token: 0x0600BB4C RID: 47948 RVA: 0x00247EB4 File Offset: 0x002460B4
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public void SetWADMIN_IS_NOTIFICATION_ENABLEDNull()
			{
				base[this.tableWebAdmin.WADMIN_IS_NOTIFICATION_ENABLEDColumn] = Convert.DBNull;
			}

			// Token: 0x040025CE RID: 9678
			private WebAdminDataSet.WebAdminDataTable tableWebAdmin;
		}

		// Token: 0x02000781 RID: 1921
		[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
		public class WebAdminRowChangeEvent : EventArgs
		{
			// Token: 0x0600BB4D RID: 47949 RVA: 0x00247ECC File Offset: 0x002460CC
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public WebAdminRowChangeEvent(WebAdminDataSet.WebAdminRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			// Token: 0x1700397B RID: 14715
			// (get) Token: 0x0600BB4E RID: 47950 RVA: 0x00247EE2 File Offset: 0x002460E2
			[DebuggerNonUserCode]
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			public WebAdminDataSet.WebAdminRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			// Token: 0x1700397C RID: 14716
			// (get) Token: 0x0600BB4F RID: 47951 RVA: 0x00247EEA File Offset: 0x002460EA
			[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
			[DebuggerNonUserCode]
			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}

			// Token: 0x040025CF RID: 9679
			private WebAdminDataSet.WebAdminRow eventRow;

			// Token: 0x040025D0 RID: 9680
			private DataRowAction eventAction;
		}
	}
}
