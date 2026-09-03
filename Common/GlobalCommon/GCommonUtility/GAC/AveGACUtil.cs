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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using AvePoint.GCommon.Utility;
    using Microsoft.Win32;

    #endregion

    /// <summary>
    /// 这个类的两个public方法用来安装和卸载assembly到GAC
    /// </summary>
    public class AveGACUtil
    {
        public static bool QueryAssemblyInfo(string assemblyName, out string assemblyPath)
        {
            assemblyPath = string.Empty;
            string publicToken = string.Empty;
            string version = string.Empty;
            string culture = string.Empty;
            string name = GetAssemblyFullName(assemblyName, out publicToken, out version, out culture);
            if (!string.IsNullOrEmpty(name))
            {
                assemblyPath = AssemblyCache.QueryAssemblyInfo(name);
                if (CheckTheSameAssembly(assemblyPath, publicToken, version, culture))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安装Assembly
        /// </summary>
        /// <param name="AssemblyPath"></param>
        public static void InstallAssembly(string AssemblyPath)
        {
            AssemblyCache.InstallAssembly(AssemblyPath, null, AssemblyCommitFlags.Force);
        }

        /// <summary>
        /// 卸载Assembly
        /// </summary>
        /// <param name="AssemblyPath"></param>
        public static void RemoveAssembly(string AssemblyPath)
        {
            AssemblyCacheEnum AssembCache = new AssemblyCacheEnum(null);
            string ShortAssemblyName = AssemblyPath.Substring(AssemblyPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            ShortAssemblyName = ShortAssemblyName.Substring(0, ShortAssemblyName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
            string FullAssembName = null;

            while (true)
            {
                string AssembNameLoc = AssembCache.GetNextAssembly();

                if (AssembNameLoc == null)
                    break;

                string pt, vt, ct;
                string ShortName = GetAssemblyFullName(AssembNameLoc, out pt, out vt, out ct);

                if (ShortAssemblyName.Equals(ShortName, StringComparison.OrdinalIgnoreCase)) //same name
                {
                    if (CheckTheSameAssembly(AssemblyPath, pt, vt, ct)) // same public taken,version,culture
                    {
                        FullAssembName = AssembNameLoc;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if (FullAssembName != null)
            {
                ClearTheRegistryKey(ShortAssemblyName, Registry.CurrentUser);
                ClearTheRegistryKey(ShortAssemblyName, Registry.LocalMachine);
                AssemblyCacheUninstallDisposition UninstDisp;
                AssemblyCache.UninstallAssembly(FullAssembName, null, out UninstDisp);
            }
        }

        /// <summary>
        /// 判断Assembly是否在GAC中
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static bool IsAssemblyInGAC(string assemblyPath)
        {
            AssemblyCacheEnum AssembCache = new AssemblyCacheEnum(null);
            string ShortAssemblyName = assemblyPath.Substring(assemblyPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            ShortAssemblyName = ShortAssemblyName.Substring(0, ShortAssemblyName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
            bool exist = false;

            while (true)
            {
                string AssembNameLoc = AssembCache.GetNextAssembly();

                if (AssembNameLoc == null)
                    break;

                string pt, vt, ct;
                string ShortName = GetAssemblyFullName(AssembNameLoc, out pt, out vt, out ct);

                if (ShortAssemblyName.Equals(ShortName, StringComparison.OrdinalIgnoreCase)) //same name
                {
                    if (CheckTheSameAssembly(assemblyPath, pt, vt, ct)) // same public taken,version,culture
                    {
                        //FullAssembName = AssembNameLoc;
                        exist = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return exist;
        }

        private static string GetAssemblyFullName(string FullName, out string PublicToken, out string vertion, out string culture)
        {
            PublicToken = null;
            vertion = null;
            culture = null;
            if (FullName == null)
                return null;
            string[] Strings = FullName.Split(',');
            foreach (string name in Strings)
            {
                int index = name.IndexOf("PublicKeyToken", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    index = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index != -1)
                    {
                        PublicToken = name.Substring(index + 1);
                        PublicToken = PublicToken.Trim();
                        continue;
                    }
                }
                int index1 = name.IndexOf("Version", StringComparison.OrdinalIgnoreCase);
                if (index1 != -1)
                {
                    index1 = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index1 != -1)
                    {
                        vertion = name.Substring(index1 + 1);
                        vertion = vertion.Trim();
                        continue;
                    }
                }
                int index2 = name.IndexOf("Culture", StringComparison.OrdinalIgnoreCase);
                if (index2 != -1)
                {
                    index2 = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index2 != -1)
                    {
                        culture = name.Substring(index2 + 1);
                        culture = culture.Trim();
                        continue;
                    }
                }
            }

            string Sout = Strings[0];
            return Sout;
        }

        private static bool CheckTheSameAssembly(string assemblypath, string pt, string vt, string ct)
        {
            Assembly soures = Assembly.LoadFile(assemblypath);
            string sourespt = null;
            string souresvt = null;
            string sourect = null;
            string[] Strings = soures.FullName.Split(',');
            foreach (string name in Strings)
            {
                int index = name.IndexOf("PublicKeyToken", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    index = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index != -1)
                    {
                        sourespt = name.Substring(index + 1);
                        sourespt = sourespt.Trim();
                        continue;
                    }
                }
                int index1 = name.IndexOf("Version", StringComparison.OrdinalIgnoreCase);
                if (index1 != -1)
                {
                    index1 = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index1 != -1)
                    {
                        souresvt = name.Substring(index1 + 1);
                        souresvt = souresvt.Trim();
                        continue;
                    }
                }
                int index2 = name.IndexOf("Culture", StringComparison.OrdinalIgnoreCase);
                if (index2 != -1)
                {
                    index2 = name.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                    if (index2 != -1)
                    {
                        sourect = name.Substring(index2 + 1);
                        sourect = sourect.Trim();
                        continue;
                    }
                }
            }
            ArgumentCheck.NotNull(sourespt, nameof(sourespt));
            ArgumentCheck.NotNull(souresvt, nameof(souresvt));
            ArgumentCheck.NotNull(sourect, nameof(sourect));
            if (sourespt.ToLower() != pt.ToLower() ||
                souresvt.ToLower() != vt.ToLower() ||
                sourect.ToLower() != ct.ToLower())
            {
                return false;
            }
            return true;
        }

        private static void ClearTheRegistryKey(string AssemblyShortName, RegistryKey CLKey)
        {
            RegistryKey key = CLKey.OpenSubKey(@"Software\Microsoft\Installer\Assemblies\Global", true);

            if (key != null)
            {
                string[] names = key.GetValueNames();

                foreach (string Name in names)
                {
                    string[] propties = Name.Split(',');
                    string assenblyname = propties[0];
                    if (AssemblyShortName.Equals(assenblyname, StringComparison.OrdinalIgnoreCase))
                    {
                        key.SetValue(Name, "", RegistryValueKind.String);
                        key.Close();
                        return;
                    }
                }
            }
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    internal interface IAssemblyCache
    {
        [PreserveSig()]
        int UninstallAssembly(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            String assemblyName,
                            InstallReference refData,
                            out AssemblyCacheUninstallDisposition disposition);

        [PreserveSig()]
        int QueryAssemblyInfo(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            String assemblyName,
                            ref AssemblyInfo assemblyInfo);
        [PreserveSig()]
        int Reserved(
                            int flags,
                            IntPtr pvReserved,
                            out Object ppAsmItem,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            String assemblyName);
        [PreserveSig()]
        int Reserved(out Object ppAsmScavenger);

        [PreserveSig()]
        int InstallAssembly(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            String assemblyFilePath,
                            InstallReference refData);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
    internal interface IAssemblyName
    {
        [PreserveSig()]
        int SetProperty(
                int PropertyId,
                IntPtr pvProperty,
                int cbProperty);

        [PreserveSig()]
        int GetProperty(
                int PropertyId,
                IntPtr pvProperty,
                ref int pcbProperty);

        [PreserveSig()]
        int Finalize();

        [PreserveSig()]
        int GetDisplayName(
                StringBuilder pDisplayName,
                ref int pccDisplayName,
                int displayFlags);

        [PreserveSig()]
        int Reserved(ref Guid guid,
            Object obj1,
            Object obj2,
            String string1,
            Int64 llFlags,
            IntPtr pvReserved,
            int cbReserved,
            out IntPtr ppv);

        [PreserveSig()]
        int GetName(
                ref int pccBuffer,
                StringBuilder pwzName);

        [PreserveSig()]
        int GetVersion(
                out int versionHi,
                out int versionLow);
        [PreserveSig()]
        int IsEqual(
                IAssemblyName pAsmName,
                int cmpFlags);

        [PreserveSig()]
        int Clone(out IAssemblyName pAsmName);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
    internal interface IAssemblyEnum
    {
        [PreserveSig()]
        int GetNextAssembly(
                IntPtr pvReserved,
                out IAssemblyName ppName,
                int flags);
        [PreserveSig()]
        int Reset();
        [PreserveSig()]
        int Clone(out IAssemblyEnum ppEnum);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("582dac66-e678-449f-aba6-6faaec8a9394")]
    internal interface IInstallReferenceItem
    {
        [PreserveSig()]
        int GetReference(
                out IntPtr pRefData,
                int flags,
                IntPtr pvReserced);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("56b1a988-7c0c-4aa2-8639-c3eb5a90226f")]
    internal interface IInstallReferenceEnum
    {
        [PreserveSig()]
        int GetNextInstallReferenceItem(
                out IInstallReferenceItem ppRefItem,
                int flags,
                IntPtr pvReserced);
    }

    internal enum AssemblyCommitFlags
    {
        Default = 1,
        Force = 2
    }

    internal enum AssemblyCacheUninstallDisposition
    {
        Unknown = 0,
        Uninstalled = 1,
        StillInUse = 2,
        AlreadyUninstalled = 3,
        DeletePending = 4,
        HasInstallReference = 5,
        ReferenceNotFound = 6
    }

    [Flags]
    internal enum AssemblyCacheFlags
    {
        GAC = 2,
    }

    internal enum CreateAssemblyNameObjectFlags
    {
        CANOF_DEFAULT = 0,
        CANOF_PARSE_DISPLAY_NAME = 1,
    }

    [Flags]
    internal enum AssemblyNameDisplayFlags
    {
        VERSION = 0x01,
        CULTURE = 0x02,
        PUBLIC_KEY_TOKEN = 0x04,
        PROCESSORARCHITECTURE = 0x20,
        RETARGETABLE = 0x80,
        ALL = VERSION | CULTURE | PUBLIC_KEY_TOKEN | PROCESSORARCHITECTURE | RETARGETABLE
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class InstallReference
    {
        public InstallReference(Guid guid, String id, String data)
        {
            cbSize = (int)(2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2);
            flags = 0;
            if (flags == 0) { }
            guidScheme = guid;
            identifier = id;
            description = data;
        }

        public Guid GuidScheme
        {
            get { return guidScheme; }
        }

        public String Identifier
        {
            get { return identifier; }
        }

        public String Description
        {
            get { return description; }
        }

        int cbSize;
        int flags;
        Guid guidScheme;
        [MarshalAs(UnmanagedType.LPWStr)]
        String identifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        String description;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyInfo
    {
        public int cbAssemblyInfo;
        public int assemblyFlags;
        public long assemblySizeInKB;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String currentAssemblyPath;
        public int cchBuf;
    }

    [ComVisible(false)]
    internal class InstallReferenceGuid
    {
        public static bool IsValidGuidScheme(Guid guid)
        {
            return (guid.Equals(UninstallSubkeyGuid) || guid.Equals(FilePathGuid) || guid.Equals(OpaqueGuid) || guid.Equals(Guid.Empty));
        }

        public readonly static Guid UninstallSubkeyGuid = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
        public readonly static Guid FilePathGuid = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
        public readonly static Guid OpaqueGuid = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");

        public readonly static Guid MsiGuid = new Guid("25df0fc1-7f97-4070-add7-4b13bbfd7cb8");
        public readonly static Guid OsInstallGuid = new Guid("d16d444c-56d8-11d5-882d-0080c847b195");
    }

    [ComVisible(false)]
    internal static class AssemblyCache
    {
        public static void InstallAssembly(String assemblyPath, InstallReference reference, AssemblyCommitFlags flags)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            IAssemblyCache ac = null;

            int hr = 0;

            hr = Utils.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.InstallAssembly((int)flags, assemblyPath, reference);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public static void UninstallAssembly(String assemblyName, InstallReference reference, out AssemblyCacheUninstallDisposition disp)
        {
            AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            IAssemblyCache ac = null;

            int hr = Utils.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.UninstallAssembly(0, assemblyName, reference, out dispResult);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            disp = dispResult;
        }

        public static String QueryAssemblyInfo(String assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid name", "assemblyName");
            }

            AssemblyInfo aInfo = new AssemblyInfo();

            aInfo.cchBuf = 1024;
            aInfo.currentAssemblyPath = new String('\0', aInfo.cchBuf);

            IAssemblyCache ac = null;
            int hr = Utils.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.QueryAssemblyInfo(0, assemblyName, ref aInfo);
            }
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return aInfo.currentAssemblyPath;
        }
    }

    [ComVisible(false)]
    internal class AssemblyCacheEnum
    {
        public AssemblyCacheEnum(String assemblyName)
        {
            IAssemblyName fusionName = null;
            int hr = 0;

            if (assemblyName != null)
            {
                hr = Utils.CreateAssemblyNameObject(out fusionName, assemblyName, CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero);
            }

            if (hr >= 0)
            {
                hr = Utils.CreateAssemblyEnum(out m_AssemblyEnum, IntPtr.Zero, fusionName, AssemblyCacheFlags.GAC, IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public String GetNextAssembly()
        {
            int hr = 0;
            IAssemblyName fusionName = null;

            if (done)
            {
                return null;
            }
            hr = m_AssemblyEnum.GetNextAssembly((IntPtr)0, out fusionName, 0);

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            if (fusionName != null)
            {
                return GetFullName(fusionName);
            }
            else
            {
                done = true;
                return null;
            }
        }

        private String GetFullName(IAssemblyName fusionAsmName)
        {
            StringBuilder sDisplayName = new StringBuilder(1024);
            int iLen = 1024;

            int hr = fusionAsmName.GetDisplayName(sDisplayName, ref iLen, (int)AssemblyNameDisplayFlags.ALL);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return sDisplayName.ToString();
        }

        private IAssemblyEnum m_AssemblyEnum = null;
        private bool done;
    }

    internal class Utils
    {
        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IntPtr pUnkReserved, IAssemblyName pName, AssemblyCacheFlags flags, IntPtr pvReserved);

        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyNameObject(out IAssemblyName ppAssemblyNameObj, [MarshalAs(UnmanagedType.LPWStr)] String szAssemblyName, CreateAssemblyNameObjectFlags flags, IntPtr pvReserved);

        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);

        [DllImport("fusion.dll")]
        internal static extern int CreateInstallReferenceEnum(out IInstallReferenceEnum ppRefEnum, IAssemblyName pName, int dwFlags, IntPtr pvReserved);
    }

    internal class AssemblyCacheInstallReferenceEnum
    {
        public AssemblyCacheInstallReferenceEnum(String assemblyName)
        {
            IAssemblyName fusionName = null;

            int hr = Utils.CreateAssemblyNameObject(
                        out fusionName,
                        assemblyName,
                        CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME,
                        IntPtr.Zero);

            if (hr >= 0)
            {
                hr = Utils.CreateInstallReferenceEnum(out refEnum, fusionName, 0, IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public InstallReference GetNextReference()
        {
            IInstallReferenceItem item = null;
            int hr = refEnum.GetNextInstallReferenceItem(out item, 0, IntPtr.Zero);
            if ((uint)hr == 0x80070103)
            {
                return null;
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            IntPtr refData;
            InstallReference instRef = new InstallReference(Guid.Empty, String.Empty, String.Empty);

            hr = item.GetReference(out refData, 0, IntPtr.Zero);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            Marshal.PtrToStructure(refData, instRef);
            return instRef;
        }

        private IInstallReferenceEnum refEnum;
    }
}
