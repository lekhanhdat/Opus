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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.BinarySerialization
{
#if DEBUG
    internal class LSReplaceDictionary:IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(LSReplaceDictionary));
        private SqlConnection mConn = null;
        private SqlCommand mCmd = null;
        internal LSReplaceDictionary()
        {
            mConn = new SqlConnection();
            try
            {
                mConn = new SqlConnection();
                mConn.ConnectionString = "Data Source=localhost;Integrated Security=SSPI;Initial Catalog=WSS_Content_7000_10";
                mConn.Open();
                mCmd = new SqlCommand();
                mCmd.Connection = mConn;
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ConnectDataBaseError, e.ToString());
            }
            //finally
            //{
            //    if (mConn != null)
            //        mConn.Close();
            //    mConn = null;
            //}
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Xoml.f19131f0_d823_4ee8_9566_8007a11f87b4.2.512.3.512")]
        internal Dictionary<string, object> GetDictionary(Guid instanceId)
        {
            Dictionary<string, object> repDictionary = new Dictionary<string, object>();
            mCmd.Parameters.Clear();
            mCmd.Parameters.AddWithValue("@Id", instanceId);
            mCmd.CommandText = "select modifications,siteid,webid,listid,tasklistid,itemid,itemguid from workflow where id=@Id";
            using(SqlDataReader sdr=mCmd.ExecuteReader())
            {
                if(sdr.Read())
                {
                    repDictionary.Add("modifications", sdr.GetString(0));

                    //*****************************************************site id
                    repDictionary.Add("B4CFFC46-0113-4E1C-A60F-649A8DC955E4", sdr.GetGuid(1));

                    //*****************************************************web id
                    repDictionary.Add("C35E91FC-3173-4E00-8EE5-1961C3F8C39E", sdr.GetGuid(2));

                    //*****************************************************parent list id
                    repDictionary.Add("98BD51DE-2E98-4C80-B0D1-4406E3011056", sdr.GetGuid(3));

                    //*****************************************************item guid
                    repDictionary.Add("DB60B279-183C-4B33-84BB-50B47D132B6D", sdr.GetGuid(6));

                    //*****************************************************instance id
                    repDictionary.Add("2C0D85AC-7F85-44F7-9C6F-396D63FFBD8E", instanceId);

                    //*****************************************************context id
                    repDictionary.Add("E2717F40-7DE5-4BF7-965A-78001B74F0D4", Guid.NewGuid());
                    //
                    repDictionary.Add("m_listId", sdr.GetGuid(3).ToString());
                    repDictionary.Add("taskListId", sdr.GetGuid(4).ToString());
                    repDictionary.Add("m_itemId", sdr.GetInt32(5));
                    repDictionary.Add("itemId", sdr.GetInt32(5));
                    repDictionary.Add("historyListId", "95B7AB12-C0B0-4C04-840B-4E1C74F31503");
                    

                    repDictionary.Add("Xoml.f7603786_2e3f_4379_a815_b87c1a848547.2.512.3.512, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "Xoml.f19131f0_d823_4ee8_9566_8007a11f87b4.2.512.3.512, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

                }
            }

            mCmd.Parameters.AddWithValue("@ListId", new Guid((string)repDictionary["taskListId"]));
            mCmd.CommandText = "select top(1) tp_id,tp_guid from AllUserData where tp_ListId=@ListId and tp_WorkflowInstanceID=@Id and tp_IsCurrent=1 order by tp_id desc";
            using(SqlDataReader sdr=mCmd.ExecuteReader())
            {
                if(sdr.Read())
                {
                    repDictionary.Add("_taskItemId", sdr.GetInt32(0));
                    //*****************************************************task item guid
                    repDictionary.Add("701072C7-580B-4DE3-A9A0-FB4747304F60", sdr.GetGuid(1));

                }
            }


            mCmd.Parameters.Clear();
            mCmd.Parameters.AddWithValue("@HostId", new Guid((string)repDictionary["m_listId"]));
            mCmd.Parameters.AddWithValue("@ItemId", (int)repDictionary["m_itemId"]);
            mCmd.Parameters.AddWithValue("@Data", "OnItemDeleted");
            mCmd.CommandText = "select Id from eventreceivers where hostid=@HostId and itemid=@ItemId and Data=@Data";
            //*****************************************************OnItemDeleted event id
            repDictionary.Add("2CC11D10-9B14-4E0D-AEE6-9D8BBB96813A", (Guid)mCmd.ExecuteScalar());

            mCmd.Parameters.Clear();
            mCmd.Parameters.AddWithValue("@HostId", new Guid((string)repDictionary["taskListId"]));
            mCmd.Parameters.AddWithValue("@ItemId", (int)repDictionary["_taskItemId"]);
            mCmd.Parameters.AddWithValue("@Data", "OnTaskDeleted");
            mCmd.CommandText = "select Id from eventreceivers where hostid=@HostId and itemid=@ItemId and Data=@Data";
            //*****************************************************OnTaskDelete id
            repDictionary.Add("43C56FA9-12D2-4AE1-B2D5-20B55C887DF3", new Guid("479D9BC6-3697-4D9A-A930-9BB73DA038FC"));

            mCmd.Parameters.Clear();
            mCmd.Parameters.AddWithValue("@HostId", new Guid((string)repDictionary["taskListId"]));
            mCmd.Parameters.AddWithValue("@ItemId", (int)repDictionary["_taskItemId"]);
            mCmd.Parameters.AddWithValue("@Data", "OnTaskChanged");
            mCmd.CommandText = "select Id from eventreceivers where hostid=@HostId and itemid=@ItemId and Data=@Data";
            //*****************************************************OnTaskChange id
            repDictionary.Add("D0EE94FF-6A3D-4933-AB37-0267976FE984", new Guid("CED56B0A-62ED-4164-822D-98AC801CF31D"));


            
            
            //*****************************************************execution id
            repDictionary.Add("F5583E85-9B22-48FB-A387-DA09F4A39744", Guid.NewGuid());
            
            
            
            
            //*****************************************************Modification ids
            //repDictionary.Add("321B5F91-4AA5-4F89-9B3E-13347E2DA374", new Guid("254B1FDE-7FF1-4164-B7F4-A2C4D52A6848"));
            //repDictionary.Add("50DE4DDD-3233-4668-824E-DC2FC85F76AA", new Guid("951893E1-6C2B-4BEA-A329-F5F8D183C836"));
            //repDictionary.Add("A938EABE-8DB1-45B9-87CB-B930728AFE10", new Guid("A938EABE-8DB1-45B9-87CB-B930728AFE10"));
            //repDictionary.Add("672218CA-6198-4E2A-8158-CDFF813D7EC3", new Guid("F2EAC6DD-2F95-4F7D-98EB-C1FBA1112DDB"));
            //repDictionary.Add("60AB381E-15CF-4B21-8F40-6C9607084B8E", new Guid("A3A1094A-4FD9-4467-9FDB-788EF7BEDBC7"));
            //repDictionary.Add("8AC90EB6-6C4A-469A-8977-8B8368083380", new Guid("64AEFD44-FD54-48C1-93C0-86F2F7C71BE5"));
            //repDictionary.Add("BB613B2D-0CD8-4E7D-9FF1-E37DFA136CCC", new Guid("1338283F-B9E7-4CB1-B343-3838EAA24657"));
            //repDictionary.Add("0418F62A-CFFC-4722-AE1A-5D2A7A50890B", new Guid("D61E2303-0206-4836-B2F0-5E68CFFD8FC0"));
            //repDictionary.Add("64AEFD44-FD54-48C1-93C0-86F2F7C71BE5", new Guid("8AC90EB6-6C4A-469A-8977-8B8368083380"));

            return repDictionary;
        }

        public void Dispose()
        {
            if (mCmd != null)
            {
                mConn.Dispose();
            }
        }
    }
#endif
}
