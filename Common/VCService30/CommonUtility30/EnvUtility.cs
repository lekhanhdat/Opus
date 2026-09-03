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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;
using System.Xml;
using System.Net;
using System.IO;

using AvePoint.GCommon.Contract;


using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon;

namespace AvePoint.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public class OSVERSIONINFO
    {
        public Int32 dwOSVersionInfoSize;
        public Int32 dwMajorVersion;
        public Int32 dwMinorVersion;
        public Int32 dwBuildNumber;
        public Int32 dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public String szCSDVersion;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class OSVERSIONINFOEX : OSVERSIONINFO
    {
        public Int16 wServicePackMajor;
        public Int16 wServicePackMinor;
        public Int16 wSuiteMask;
        public Byte wProductType;
        public Byte wReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public uint dwOemId;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public uint dwProcessorLevel;
        public uint dwProcessorRevision;
    }

    public class WinVersion
    {
        [DllImport("kernel32.dll")]
        public extern static Boolean GetVersionEx([In, Out] OSVERSIONINFO versionInfo);

        public const Int32 VER_PLATFORM_WIN32s = 0;
        public const Int32 VER_PLATFORM_WIN32_WINDOWS = 1;
        public const Int32 VER_PLATFORM_WIN32_NT = 2;

        public const Int32 VER_NT_WORKSTATION = 1;
        public const Int32 VER_NT_DOMAIN_CONTROLLER = 2;
        public const Int32 VER_NT_SERVER = 3;

        // Microsoft Small Business Server 
        public const Int32 VER_SUITE_SMALLBUSINESS = 1;
        // Win2k Adv Server or .Net Enterprise Server 
        public const Int32 VER_SUITE_ENTERPRISE = 2;
        // Terminal Services is installed.   
        public const Int32 VER_SUITE_TERMINAL = 16;
        // Win2k Datacenter 
        public const Int32 VER_SUITE_DATACENTER = 128;
        // Terminal server in remote admin mode 
        public const Int32 VER_SUITE_SINGLEUSERTS = 256;
        public const Int32 VER_SUITE_PERSONAL = 512;
        // Microsoft .Net webserver installed 
        public const Int32 VER_SUITE_BLADE = 1024;

        private OSVERSIONINFO versionInfo;

        public Boolean DuplicateToken = false;

        public WinVersion()
        {

        }

        public String GetVersionName()
        {
            String name = String.Empty;
            Boolean success = true;
            Boolean bVersionInfoEx;

            versionInfo = new OSVERSIONINFOEX();
            versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
            bVersionInfoEx = GetVersionEx(versionInfo);

            if (!bVersionInfoEx)
            {
                versionInfo = new OSVERSIONINFO();
                versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
                success = GetVersionEx(versionInfo);

                if (!success)
                {
                    return "Cannot get system version information";// "æœªè‰¹æ™’å–ç³»ç»Ÿå§¹éž ?;
                }
            }
            //MessageBox.Show(versionInfo.dwMajorVersion + "     " + versionInfo.dwMinorVersion);

            switch (versionInfo.dwPlatformId)
            {
                // Win NT 
                case VER_PLATFORM_WIN32_NT:
                    if (versionInfo.dwMajorVersion == 6 &&
                        versionInfo.dwMinorVersion == 0)
                    {
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Vista ";
                        }
                        if (((OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Server 2008";
                        }
                        DuplicateToken = true;
                    }

                    if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 1)
                    {
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows 7 ";
                        }
                        if (((OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Server 2008 R2";
                        }
                        DuplicateToken = true;
                        //name = "Microsoft Windows Vista, Server 2008, R2, 7...";
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 2)
                    {
                        name = "Microsoft Windows Server 2003, ";
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 1)
                    {
                        name = "Microsoft Windows XP ";
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 0)
                    {
                        name = "Microsoft Windows 2000 ";
                    }

                    // è¯´ä¸ºWindows NT 4.0 SP6ç¢Œç³»ç»?
                    if (bVersionInfoEx)
                    {
                        // ç«?
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            if (versionInfo.dwMajorVersion == 4)
                            {
                                name += "Workstation 4.0 ";
                            }
                            else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
                            {
                                name += "Home Edition ";
                            }
                            else
                            {
                                name += "Professional Edition ";
                            }
                        }
                        //  
                        else if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_SERVER ||
                            ((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_DOMAIN_CONTROLLER)
                        {
                            if (versionInfo.dwMajorVersion == 5 &&
                                versionInfo.dwMinorVersion == 2)
                            {
                                if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                                {
                                    name += "Datacenter Edition ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                {
                                    name += "Enterprise Edition ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
                                {
                                    name += "Web Edition ";
                                }
                                else
                                {
                                    name += "Standard Edition ";
                                }
                            }
                            else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 0)
                            {
                                if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                                {
                                    name += "Datacenter Server ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                {
                                    name += "Advanced Server ";
                                }
                                else
                                {
                                    name += "Server ";
                                }
                            }
                            // Windows NT 4.0 
                            else
                            {
                                if (AveEnv.WindowsVersionIsX64)
                                {
                                    if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                    {
                                        name += "Enterprise x64 Edition ";
                                    }
                                    else
                                    {
                                        name += "Standard x64 Edition ";
                                    }
                                }
                                else
                                {
                                    if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                    {
                                        name += "Enterprise Edition ";
                                    }
                                    else
                                    {
                                        name += "Standard Edition ";
                                    }
                                }
                                //if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                //{
                                //    name += "Server 4.0, Enterprise Edition ";
                                //}
                                //else
                                //{
                                //    name += "Server 4.0 ";
                                //}
                            }
                        }
                    }
                    break;

                // Win 9X 
                case VER_PLATFORM_WIN32_WINDOWS:
                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 0)
                    {
                        name = "Microsoft Windows 95 ";
                        if (versionInfo.szCSDVersion[1] == 'C' ||
                            versionInfo.szCSDVersion[1] == 'B')
                        {
                            name += "OSR2 ";
                        }
                    }

                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 10)
                    {
                        name = "Microsoft Windows 98 ";
                        if (versionInfo.szCSDVersion[1] == 'A')
                        {
                            name = "SE ";
                        }
                    }

                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 90)
                    {
                        name = "Microsoft Windows Millennium Edition";
                    }
                    break;

                // Win32ç³»ç»Ÿ 
                case VER_PLATFORM_WIN32s:
                    name = "Microsoft Win32s";
                    break;

                default:
                    name = "Unknown System";
                    break;
            }

            name += versionInfo.szCSDVersion;

            return name;
        }

        public String GetVersionNameCEIP()
        {
            String name = String.Empty;
            Boolean success = true;
            Boolean bVersionInfoEx;

            versionInfo = new OSVERSIONINFOEX();
            versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
            bVersionInfoEx = GetVersionEx(versionInfo);

            if (!bVersionInfoEx)
            {
                versionInfo = new OSVERSIONINFO();
                versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);
                success = GetVersionEx(versionInfo);

                if (!success)
                {
                    return "Cannot get system version information";
                }
            }

            switch (versionInfo.dwPlatformId)
            {
                // Win NT 
                case VER_PLATFORM_WIN32_NT:
                    if (versionInfo.dwMajorVersion == 6 &&
                        versionInfo.dwMinorVersion == 0)
                    {
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Vista, ";
                        }
                        if (((OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Server 2008, ";
                        }
                        DuplicateToken = true;
                    }

                    if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 1)
                    {
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows 7, ";
                        }
                        if (((OSVERSIONINFOEX)versionInfo).wProductType != VER_NT_WORKSTATION)
                        {
                            name = "Microsoft Windows Server 2008 R2, ";
                        }
                        DuplicateToken = true;
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 2)
                    {
                        name = "Microsoft Windows Server 2003, ";
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 1)
                    {
                        name = "Microsoft Windows XP, ";
                    }

                    if (versionInfo.dwMajorVersion == 5 &&
                        versionInfo.dwMinorVersion == 0)
                    {
                        name = "Microsoft Windows 2000, ";
                    }

                    if (AveEnv.WindowsVersionIsX64)
                    {
                        name += "X64, ";
                    }
                    else
                    {
                        name += "X86, ";
                    }

                    if (bVersionInfoEx)
                    {
                        if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_WORKSTATION)
                        {
                            if (versionInfo.dwMajorVersion == 4)
                            {
                                name += "Workstation 4.0, ";
                            }
                            else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
                            {
                                name += "Home Edition, ";
                            }
                            else
                            {
                                name += "Professional Edition, ";
                            }
                        }
                        else if (((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_SERVER ||
                            ((OSVERSIONINFOEX)versionInfo).wProductType == VER_NT_DOMAIN_CONTROLLER)
                        {
                            if (versionInfo.dwMajorVersion == 5 &&
                                versionInfo.dwMinorVersion == 2)
                            {
                                if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                                {
                                    name += "Datacenter Edition, ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                {
                                    name += "Enterprise Edition, ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
                                {
                                    name += "Web Edition, ";
                                }
                                else
                                {
                                    name += "Standard Edition, ";
                                }
                            }
                            else if (versionInfo.dwMajorVersion == 5 && versionInfo.dwMinorVersion == 0)
                            {
                                if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                                {
                                    name += "Datacenter Server, ";
                                }
                                else if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                {
                                    name += "Advanced Server, ";
                                }
                                else
                                {
                                    name += "Server, ";
                                }
                            }
                            else
                            {
                                if ((((OSVERSIONINFOEX)versionInfo).wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                                {
                                    name += "Enterprise Edition, ";
                                }
                                else
                                {
                                    name += "Standard Edition, ";
                                }
                            }
                        }
                    }
                    break;

                // Win 9X 
                case VER_PLATFORM_WIN32_WINDOWS:
                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 0)
                    {
                        name = "Microsoft Windows 95 ";
                        if (versionInfo.szCSDVersion[1] == 'C' ||
                            versionInfo.szCSDVersion[1] == 'B')
                        {
                            name += "OSR2 ";
                        }
                    }

                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 10)
                    {
                        name = "Microsoft Windows 98 ";
                        if (versionInfo.szCSDVersion[1] == 'A')
                        {
                            name = "SE ";
                        }
                    }

                    if (versionInfo.dwMajorVersion == 4 && versionInfo.dwMinorVersion == 90)
                    {
                        name = "Microsoft Windows Millennium Edition";
                    }
                    break;

                case VER_PLATFORM_WIN32s:
                    name = "Microsoft Win32s";
                    break;

                default:
                    name = "Unknown System";
                    break;
            }

            name += versionInfo.szCSDVersion;

            return name;
        }
    }

    public class AgentRegisterUtil
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void RegisterAgent(string domain, string username, string password, string farmId)
        {
            IMAgentService agentControlService = CustomizeChannelFactory<IMAgentService>.CreateManagerChannel();

            var agentDto = new ServiceDto();
            agentDto.Schema = AveEnv.AgentSchema;
            agentDto.Name = AveEnv.AgentName;
            agentDto.Address = AveEnv.AgentAddress;
            agentDto.Port = AveEnv.AgentPort;
            agentDto.AgentType = AveEnv.AgentType;
            agentDto.Domain = domain;
            agentDto.UserName = username;
            agentDto.Password = password;
            agentDto.Version = AveEnv.AgentVersion;
            agentDto.SPVersion = (int)AveEnv.SPVersion;
            agentDto.MossOrWss = (int)AveEnv.MossOrWss;
            agentDto.FarmName = AveEnv.AgentFarmName;
            agentDto.FarmId = farmId;
            agentDto.LogLevel = AveEnv.AgentLogLevel;
            agentDto.ExtraInfo = string.Empty;

            mLog.Info("Agent Information: " + agentDto.ToString());
            agentControlService.Register(agentDto);
            (agentControlService as IDisposable).Dispose();
        }

        public static void SaveAgentAccount(string domain, string username, string password)
        {
            try
            {
                IMAgentService agentControlService = CustomizeChannelFactory<IMAgentService>.CreateManagerChannel();
                agentControlService.AgentConfig(new AgentQueryDto { AgentName = AveEnv.AgentName, AgentAddress = AveEnv.AgentAddress }, new AgentConfigInfo { Domain = domain, Username = username, Password = password });
                (agentControlService as IDisposable).Dispose();
            }
            catch (Exception ex)
            {
                mLog.Error("An error occured while saving agent account. ", ex);
            }
        }
    }
}
