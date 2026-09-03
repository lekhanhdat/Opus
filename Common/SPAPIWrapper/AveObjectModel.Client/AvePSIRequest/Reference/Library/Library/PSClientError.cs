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
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace Microsoft.Office.Project.Server.Library
{
    // Token: 0x02000E6B RID: 3691
    [Serializable]
	public class PSClientError : ISerializable
	{
		public virtual void GetObjectData(SerializationInfo s, StreamingContext c)
		{
			string allErrorsXmlString = this.GetAllErrorsXmlString();
			if (s != null)
			{
				s.AddValue(PSClientError.SerializationKey, allErrorsXmlString);
			}
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x00057BBC File Offset: 0x00055DBC
		protected PSClientError(SerializationInfo s, StreamingContext c)
		{
			string @string = s.GetString(PSClientError.SerializationKey);
			this.errorInfoXml = GeneralUtility.StringToXMLDoc(@string);
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x00057BE8 File Offset: 0x00055DE8
		//public PSClientError(SoapException e)
		//{
		//	this.errorInfoXml = new XmlDocument();
		//	XmlElement xmlElement = null;
		//	if (e != null && e.Detail != null)
		//	{
		//		xmlElement = (XmlElement)e.Detail.SelectSingleNode("errinfo");
		//	}
		//	if (xmlElement != null)
		//	{
		//		XmlNode newChild = this.errorInfoXml.ImportNode(xmlElement, true);
		//		this.errorInfoXml.AppendChild(newChild);
		//	}
		//}

		// Token: 0x06000F0C RID: 3852 RVA: 0x00057C47 File Offset: 0x00055E47
		public PSClientError(string xmlString)
		{
			this.errorInfoXml = GeneralUtility.StringToXMLDoc(xmlString);
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x00057C5B File Offset: 0x00055E5B
		public PSErrorID LastError
		{
			get
			{
				return this.lastError;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000F0E RID: 3854 RVA: 0x00057C64 File Offset: 0x00055E64
		public int Count
		{
			get
			{
				XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
				if (xmlElement != null)
				{
					XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("error");
					return elementsByTagName.Count;
				}
				return 0;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000F0F RID: 3855 RVA: 0x00057CA0 File Offset: 0x00055EA0
		public bool HasErrors
		{
			get
			{
				XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
				if (xmlElement != null)
				{
					XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("error");
					return elementsByTagName.Count > 0;
				}
				return false;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x00057CE0 File Offset: 0x00055EE0
		public bool HasDataSetErrors
		{
			get
			{
				XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
				if (xmlElement != null)
				{
					XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("dataset");
					return elementsByTagName.Count > 0;
				}
				return false;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000F11 RID: 3857 RVA: 0x00057D20 File Offset: 0x00055F20
		public bool HasArrayErrors
		{
			get
			{
				XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
				if (xmlElement != null)
				{
					XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("array");
					return elementsByTagName.Count > 0;
				}
				return false;
			}
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x00057D5D File Offset: 0x00055F5D
		private string MungeGeneralErrorName(string name)
		{
			return name.Replace('\'', '_');
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x00057D6C File Offset: 0x00055F6C
		public bool HasGeneralErrors
		{
			get
			{
				XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
				if (xmlElement != null)
				{
					XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("general");
					return elementsByTagName.Count > 0;
				}
				return false;
			}
		}

		// Token: 0x06000F14 RID: 3860 RVA: 0x00057DA9 File Offset: 0x00055FA9
		public string[] GetDataSetNames()
		{
			return this.GetErrorClassNames("dataset");
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x00057DB6 File Offset: 0x00055FB6
		public DataSet GetErrorDataSet(string dataSetName)
		{
			return this.GetErrorDataSetInternal(dataSetName, null);
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x00057DC0 File Offset: 0x00055FC0
		public DataSet GetErrorDataSet(string dataSetName, out List<PSErrorInfo[]> errInfoList)
		{
			errInfoList = new List<PSErrorInfo[]>();
			return this.GetErrorDataSetInternal(dataSetName, errInfoList);
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x00057DD4 File Offset: 0x00055FD4
		public PSErrorInfo[] GetRowErrors(DataRow dr)
		{
			XmlElement xmlElement = this.FindRowElem(dr);
			if (xmlElement == null)
			{
				return null;
			}
			XmlNodeList errNodes = xmlElement.SelectNodes("error");
			return this.GetErrorInfoList(errNodes);
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x00057E04 File Offset: 0x00056004
		private DataSet GetErrorDataSetInternal(string dataSetName, List<PSErrorInfo[]> errInfoList)
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("dataset[@name='" + dataSetName + "']");
			if (xmlElement2 == null)
			{
				return null;
			}
			DataSet result;
			using (DataSet dataSet = new DataSet(dataSetName))
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				XmlNodeList xmlNodeList = xmlElement2.SelectNodes("table");
				for (int i = 0; i < xmlNodeList.Count; i++)
				{
					XmlElement xmlElement3 = (XmlElement)xmlNodeList[i];
					DataTable dataTable = dataSet.Tables.Add(xmlElement3.GetAttribute("name"));
					dataTable.Columns.Add("id", typeof(Guid));
					if (errInfoList != null)
					{
						dataTable.Columns.Add("ErrorIndex", typeof(int));
					}
					XmlNodeList xmlNodeList2 = xmlElement3.SelectNodes("row");
					for (int j = 0; j < xmlNodeList2.Count; j++)
					{
						XmlElement xmlElement4 = (XmlElement)xmlNodeList2[j];
						if (xmlElement4.Attributes.Count == 1)
						{
							DataRow dataRow = dataTable.NewRow();
							XmlAttribute xmlAttribute = xmlElement4.Attributes[0];
							dataRow[0] = this.ConvertStringToObject(xmlAttribute.Value, typeof(Guid));
							dataTable.Rows.Add(dataRow);
							if (errInfoList != null)
							{
								XmlNodeList errNodes = xmlElement4.SelectNodes("error");
								PSErrorInfo[] errorInfoList = this.GetErrorInfoList(errNodes);
								errInfoList.Add(errorInfoList);
								dataRow["ErrorIndex"] = errInfoList.IndexOf(errorInfoList);
							}
						}
					}
				}
				result = dataSet;
			}
			return result;
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x00057FE0 File Offset: 0x000561E0
		private XmlElement FindRowElem(DataRow dr)
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			string dataSetName = dr.Table.DataSet.DataSetName;
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("dataset[@name='" + dataSetName + "']");
			if (xmlElement2 == null)
			{
				return null;
			}
			string tableName = dr.Table.TableName;
			XmlElement xmlElement3 = (XmlElement)xmlElement2.SelectSingleNode("table[@name='" + tableName + "']");
			if (xmlElement3 == null)
			{
				return null;
			}
			return this.FindRowElem(dr, xmlElement3);
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00058070 File Offset: 0x00056270
		private XmlElement FindRowElem(DataRow dr, XmlNode tableNode)
		{
			StringBuilder stringBuilder = new StringBuilder("row");
			DataColumn[] array;
			if (dr.Table.PrimaryKey.Length > 0)
			{
				array = dr.Table.PrimaryKey;
			}
			else
			{
				array = new DataColumn[dr.Table.Columns.Count];
				for (int i = 0; i < dr.Table.Columns.Count; i++)
				{
					array[i] = dr.Table.Columns[i];
				}
			}
			foreach (DataColumn dataColumn in array)
			{
				stringBuilder.Append("[@");
				stringBuilder.Append(dataColumn.ColumnName);
				stringBuilder.Append("='");
				stringBuilder.Append(dr[dataColumn.ColumnName].ToString());
				stringBuilder.Append("']");
			}
			return (XmlElement)tableNode.SelectSingleNode(stringBuilder.ToString());
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x0005815A File Offset: 0x0005635A
		public string[] GetArrayNames()
		{
			return this.GetErrorClassNames("array");
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x00058167 File Offset: 0x00056367
		public object[] GetErrorArray(string arrayName)
		{
			return this.GetErrorArrayInternal(arrayName, null);
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x00058171 File Offset: 0x00056371
		public object[] GetErrorArray(string arrayName, out List<PSErrorInfo[]> errorInfoList)
		{
			errorInfoList = new List<PSErrorInfo[]>();
			return this.GetErrorArrayInternal(arrayName, errorInfoList);
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x00058184 File Offset: 0x00056384
		internal XmlNode GetErrorArrayXml(string arrayName)
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("array[@name='" + arrayName + "']");
			if (xmlElement2 == null)
			{
				return null;
			}
			return xmlElement2.Clone();
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x000581D0 File Offset: 0x000563D0
		public PSErrorInfo[] GetArrayItemErrors(string arrayName, object item)
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("array[@name='" + arrayName + "']");
			if (xmlElement2 == null)
			{
				return null;
			}
			XmlElement xmlElement3 = (XmlElement)xmlElement2.SelectSingleNode("item[@value='" + ((item == null) ? string.Empty : item.ToString()) + "']");
			if (xmlElement3 == null)
			{
				return null;
			}
			XmlNodeList errNodes = xmlElement3.SelectNodes("error");
			return this.GetErrorInfoList(errNodes);
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x00058258 File Offset: 0x00056458
		private object[] GetErrorArrayInternal(string arrayName, List<PSErrorInfo[]> errorInfoList)
		{
			Type type;
			return this.GetErrorArrayInternal(arrayName, out type, errorInfoList);
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00058270 File Offset: 0x00056470
		private object[] GetErrorArrayInternal(string arrayName, out Type itemType, List<PSErrorInfo[]> errorInfoList)
		{
			object[] array = null;
			itemType = typeof(object);
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("array[@name='" + arrayName + "']");
			if (xmlElement2 != null)
			{
				itemType = Type.GetType(xmlElement2.GetAttribute("type"));
				XmlNodeList xmlNodeList = xmlElement2.SelectNodes("item");
				array = new object[xmlNodeList.Count];
				for (int i = 0; i < xmlNodeList.Count; i++)
				{
					XmlElement xmlElement3 = (XmlElement)xmlNodeList[i];
					array[i] = this.ConvertStringToObject(xmlElement3.GetAttribute("value"), itemType);
					if (errorInfoList != null)
					{
						XmlNodeList errNodes = xmlElement3.SelectNodes("error");
						errorInfoList.Insert(i, this.GetErrorInfoList(errNodes));
					}
				}
			}
			return array;
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x0005834B File Offset: 0x0005654B
		internal string[] GetGeneralNames()
		{
			return this.GetErrorClassNames("class");
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x00058358 File Offset: 0x00056558
		internal PSErrorInfo[] GetGeneralErrors(string name)
		{
			name = this.MungeGeneralErrorName(name);
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("general");
			if (xmlElement2 == null)
			{
				return null;
			}
			XmlElement xmlElement3 = (XmlElement)xmlElement2.SelectSingleNode("class[@name='" + name + "']");
			if (xmlElement3 == null)
			{
				return null;
			}
			XmlNodeList errNodes = xmlElement3.SelectNodes("error");
			return this.GetErrorInfoList(errNodes);
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x000583D0 File Offset: 0x000565D0
		public PSErrorInfo[] GetAllGeneralErrors()
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			XmlElement xmlElement2 = (XmlElement)xmlElement.SelectSingleNode("general");
			if (xmlElement2 == null)
			{
				return null;
			}
			XmlNodeList elementsByTagName = xmlElement2.GetElementsByTagName("error");
			return this.GetErrorInfoList(elementsByTagName);
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x0005841C File Offset: 0x0005661C
		public PSErrorInfo[] GetAllErrors()
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			if (xmlElement == null)
			{
				return new PSErrorInfo[0];
			}
			XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("error");
			return this.GetErrorInfoList(elementsByTagName);
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x0005845C File Offset: 0x0005665C
		internal XmlNode GetAllErrorsXml()
		{
			XmlElement xmlElement = (XmlElement)this.errorInfoXml.SelectSingleNode("errinfo");
			if (xmlElement == null)
			{
				return null;
			}
			XmlNodeList elementsByTagName = xmlElement.GetElementsByTagName("error");
			if (elementsByTagName.Count == 0)
			{
				return null;
			}
			return xmlElement.Clone();
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x000584A0 File Offset: 0x000566A0
		internal string GetAllErrorsXmlString()
		{
			string result;
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				this.errorInfoXml.Save(stringWriter);
				result = stringWriter.ToString();
			}
			return result;
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x000584E8 File Offset: 0x000566E8
		private PSErrorInfo[] GetErrorInfoList(XmlNodeList errNodes)
		{
			int num = 0;
			if (errNodes != null)
			{
				num = errNodes.Count;
			}
			PSErrorInfo[] array = new PSErrorInfo[num];
			for (int i = 0; i < num; i++)
			{
				ArgumentCheck.NotNull(errNodes, nameof(errNodes));
				XmlElement xmlElement = (XmlElement)errNodes[i];
				PSErrorID errId = (PSErrorID)int.Parse(xmlElement.GetAttribute("id"), CultureInfo.InvariantCulture);
				Guid errUid = new Guid(xmlElement.GetAttribute("uid"));
				string attribute = xmlElement.GetAttribute("name");
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				if (xmlElement.Attributes.Count > 3)
				{
					for (int j = 0; j < xmlElement.Attributes.Count; j++)
					{
						XmlAttribute xmlAttribute = xmlElement.Attributes[j];
						if (xmlAttribute.Name != "id" && xmlAttribute.Name != "uid" && xmlAttribute.Name != "name")
						{
							list.Add(xmlAttribute.Name);
							list2.Add(xmlAttribute.Value);
						}
					}
				}
				array[i] = new PSErrorInfo(errId, errUid, attribute, list.ToArray(), list2.ToArray());
			}
			return array;
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x00058614 File Offset: 0x00056814
		private string[] GetErrorClassNames(string containerTag)
		{
			string[] array = null;
			XmlNodeList elementsByTagName = this.errorInfoXml.GetElementsByTagName(containerTag);
			if (elementsByTagName.Count > 0)
			{
				array = new string[elementsByTagName.Count];
				for (int i = 0; i < elementsByTagName.Count; i++)
				{
					XmlElement xmlElement = (XmlElement)elementsByTagName[i];
					array[i] = xmlElement.GetAttribute("name");
				}
			}
			return array;
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x00058672 File Offset: 0x00056872
		private object ConvertStringToObject(string str, Type objType)
		{
			if (objType == typeof(Guid))
			{
				return new Guid(str);
			}
			return Convert.ChangeType(str, objType, CultureInfo.InvariantCulture);
		}

		// Token: 0x04004431 RID: 17457
		public const string ErrorColumnName = "ErrorIndex";







		// Token: 0x04004434 RID: 17460
		//private const string tableTag = "table";

		// Token: 0x04004435 RID: 17461
		//private const string rowTag = "row";









		// Token: 0x04004444 RID: 17476
		private static readonly string SerializationKey = "errorxml";

		// Token: 0x04004445 RID: 17477
		private XmlDocument errorInfoXml;

		// Token: 0x04004446 RID: 17478
		private PSErrorID lastError;
	}
}
