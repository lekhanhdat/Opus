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


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    internal class HtmlCmdLine
    {
        #region Static Members
        private static AveLogger Logger = AveLogger.GetInstance(typeof(HtmlCmdLine));
        internal static bool Help;

        #endregion

        #region Constructors

        static HtmlCmdLine()
        {
            Help = false;
            ParseArgs();
        }

        #endregion

        #region Internal Methods

        internal static string GetOption(string name, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetStringArg(args[i], name, ref p);
            }
            return p;
        }

        internal static string GetOption(int index, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            int j = 0;
            for (int i = 1; i < args.Length; i++)
            {
                if (GetStringArg(args[i], ref p))
                {
                    if (index == j)
                        return p;
                    else
                        p = def;
                    j++;
                }
            }
            return p;
        }

        internal static bool GetOption(string name, bool def)
        {
            bool p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetBoolArg(args[i], name, ref p);
            }
            return p;
        }

        internal static int GetOption(string name, int def)
        {
            int p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetIntArg(args[i], name, ref p);
            }
            return p;
        }

        #endregion

        #region Private Methods

        private static void GetBoolArg(string Arg, string Name, ref bool ArgValue)
        {
            if (Arg.Length < (Name.Length + 1)) // -name is 1 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
                ArgValue = true;
        }

        private static void GetIntArg(string Arg, string Name, ref int ArgValue)
        {
            if (Arg.Length < (Name.Length + 3)) // -name:12 is 3 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
            {
                try
                {
                    ArgValue = Convert.ToInt32(Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2));
                }
                catch(Exception e)
                {
                    Logger.Warn("failed to get int arg: {0}", e.ToString());
                }
            }
        }

        private static bool GetStringArg(string Arg, ref string ArgValue)
        {
            if (('/' == Arg[0]) || ('-' == Arg[0]))
                return false;
            ArgValue = Arg;
            return true;
        }

        private static void GetStringArg(string Arg, string Name, ref string ArgValue)
        {
            if (Arg.Length < (Name.Length + 3)) // -name:x is 3 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
                ArgValue = Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2);
        }

        private static void ParseArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                // help
                GetBoolArg(args[i], "?", ref Help);
                GetBoolArg(args[i], "h", ref Help);
                GetBoolArg(args[i], "help", ref Help);
            }
        }

        #endregion
    }
}