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
using System.Text;
using System.Reflection;
using AvePoint.Adonis.ReportCenter.Object;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.GCommon.Utility.PerformanceMonitor
{
    public class NetWorkDetail
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<NetworkAdapter> networkAdapterCollection = new List<NetworkAdapter>();
        private static readonly object m_LockHelper = new object();

        /// <summary>
        /// Get NetWork Info
        /// </summary>
        /// <returns></returns>
        public List<NetworkAdapterDetail> GetDetails()
        {

            List<NetworkAdapter> networkAdapterList = null;
            List<NetworkAdapterDetail> tmplist = new List<NetworkAdapterDetail>();
            if (networkAdapterCollection.Count != 0)
            {
                networkAdapterList = GetDetailsInfoWithReturnValue(networkAdapterCollection, false);
            }
            else if (networkAdapterCollection.Count == 0)
            {
                lock (m_LockHelper)
                {
                    if (networkAdapterCollection.Count == 0)
                    {
                        networkAdapterList = GetDetailsInfoWithReturnValue();
                    }
                }
            }
            foreach (NetworkAdapter item in networkAdapterList)
            {
                try
                {
                    NetworkAdapterDetail tmpDetail = new NetworkAdapterDetail();
                    tmpDetail.NetConnectionId = item.NetConnectionId;
                    tmpDetail.Status = item.Status;
                    tmpDetail.Caption = item.Caption;
                    tmpDetail.Description = item.Description;
                    tmpDetail.ServiceName = item.ServiceName;
                    tmpDetail.MacAddress = item.MacAddress;
                    tmpDetail.ProductName = item.ProductName;
                    tmpDetail.Name = item.Name;
                    tmpDetail.AdapterType = item.AdapterType;
                    tmpDetail.AdapterTypeId = item.AdapterTypeId;
                    tmpDetail.Speed = item.Speed.ToString();
                    tmpDetail.NetworkUtilization = ParseNetworkUtilization(item.LinkSpeed, item.Speed);
                    tmpDetail.LinkSpeed = item.Status.Equals("0") ? "0" : ParseLinkSpeed(item.LinkSpeed);
                    if (item.NetConnectionId.Equals("Wireless Network Connection", StringComparison.OrdinalIgnoreCase))
                    {
                        tmpDetail.AdapterTypeId = "9";
                    }
                    else if (item.ProductName.Contains("Wireless") || item.ProductName.Contains("wireless"))
                    {
                        tmpDetail.AdapterTypeId = "9";
                    }
                    tmplist.Add(tmpDetail);
                }
                catch (Exception e)
                {
                    logger.Warn("GetDetails() exception:{0}", e.ToString());
                }
            }
            return tmplist;

        }







        private List<NetworkAdapter> GetDetailsInfoWithReturnValue(List<NetworkAdapter> networkAdapterList, Boolean firstTime)
        {
            PerfFormattedData_Tcpip_NetworkInterface.PerfFormattedData_Tcpip_NetworkInterfaceCollection NetworkInterfaceCollection = PerfFormattedData_Tcpip_NetworkInterface.GetInstances();
            foreach (PerfFormattedData_Tcpip_NetworkInterface networkInterface in NetworkInterfaceCollection)
            {
                Boolean HasMatched = false;
                foreach (NetworkAdapter item in networkAdapterList)
                {
                    if (MatchingAdapter(item.Name, networkInterface.Name))
                    {
                        if (firstTime)
                        {
                            Double linkSpeedResult = 0;
                            if (Double.TryParse(networkInterface.CurrentBandwidth, out linkSpeedResult))
                            {
                                item.LinkSpeed = linkSpeedResult;
                                if (item.LinkSpeed == 0)
                                {
                                    //m_Log.Log(AveLogSeverity.Warn, "AveNetworkInfos00006", networkInterface.CurrentBandwidth);
                                    item.LinkSpeed = 100000000;
                                }
                            }
                            else
                            {
                                //m_Log.Log(AveLogSeverity.Warn, "AveNetworkInfos00005", networkInterface.CurrentBandwidth);
                                item.LinkSpeed = 100000000;
                            }
                        }
                        item.Speed = networkInterface.BytesTotalPersec;
                        HasMatched = true;
                        break;
                    }
                }

                if (HasMatched == false)
                {
                    //m_Log.Log(AveLogSeverity.Info, "AveNetworkInfos00002", networkInterface.Name);
                }
            }
            return networkAdapterList;
        }

        private List<NetworkAdapter> GetDetailsInfoWithReturnValue()
        {
            networkAdapterCollection.Clear();
            AvePoint.GCommon.Utility.NetworkAdapter.NetworkAdapterCollection netWorkAdapterCollection = AvePoint.GCommon.Utility.NetworkAdapter.GetInstances();

            foreach (AvePoint.GCommon.Utility.NetworkAdapter wmiNetWorkAdapter in netWorkAdapterCollection)
            {
                if (String.IsNullOrEmpty(wmiNetWorkAdapter.NetConnectionID))
                {
                    continue;
                }
                NetworkAdapter networkAdapter = new NetworkAdapter();
                networkAdapter.Manufacturer = wmiNetWorkAdapter.Manufacturer;
                networkAdapter.AdapterType = wmiNetWorkAdapter.AdapterType;
                networkAdapter.Caption = wmiNetWorkAdapter.Caption;
                networkAdapter.Description = wmiNetWorkAdapter.Description;
                networkAdapter.MacAddress = wmiNetWorkAdapter.MACAddress;
                networkAdapter.Name = wmiNetWorkAdapter.Name;
                networkAdapter.NetConnectionId = wmiNetWorkAdapter.NetConnectionID;
                networkAdapter.ProductName = wmiNetWorkAdapter.ProductName;
                networkAdapter.ServiceName = wmiNetWorkAdapter.ServiceName;
                networkAdapter.Status = wmiNetWorkAdapter.NetConnectionStatus.ToString();
                networkAdapter.AdapterTypeId = (((Int32)wmiNetWorkAdapter.AdapterTypeId)).ToString();
                networkAdapterCollection.Add(networkAdapter);
            }
            networkAdapterCollection = GetDetailsInfoWithReturnValue(networkAdapterCollection, true);
            return networkAdapterCollection;

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Gbps is ok.")]
        private string ParseLinkSpeed(Double linkSpeed)
        {
            String result = String.Empty;
            linkSpeed = linkSpeed / (Math.Pow(1000, 2));
            if (linkSpeed >= 1000)
            {
                result = (linkSpeed / Math.Pow(1000, 1)).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " Gbps";
            }
            else
            {
                result = linkSpeed.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + " Mbps";
            }
            return result;
        }

        private String ParseNetworkUtilization(Double linkSpeed, ulong speed)
        {
            String result = String.Empty;
            if (linkSpeed <= 0)
            {
                result = "0";
            }
            else
            {
                if (((speed * 8) / linkSpeed) >= 1)
                {
                    result = "1";
                }
                else
                {
                    result = ((speed * 8) / linkSpeed).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                }
            }
            return result;
        }

        private Boolean MatchingAdapter(String adapterName1, String adapterName2)
        {
            Boolean result = false;
            adapterName1 = CheckAdapterName(adapterName1);
            adapterName2 = CheckAdapterName(adapterName2);
            if (!String.IsNullOrEmpty(adapterName1)
                && !String.IsNullOrEmpty(adapterName2))
            {
                if (string.Equals(adapterName1, adapterName2, StringComparison.OrdinalIgnoreCase))
                {
                    result = true;
                }
            }
            return result;

        }

        private String CheckAdapterName(String adapterName)
        {
            String resultName = adapterName;
            if (!String.IsNullOrEmpty(adapterName)
                && !String.IsNullOrEmpty(adapterName.Trim()))
            {
                resultName = adapterName
                    .Replace('(', '[')
                    .Replace(')', ']')
                    .Replace('/', '_')
                    .Replace('#', '_').Trim();
            }
            return resultName;

        }
    }
}
