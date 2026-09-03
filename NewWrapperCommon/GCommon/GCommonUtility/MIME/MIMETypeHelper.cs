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



namespace AvePoint.GCommon.Utility
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using AvePoint.Common;
    using Microsoft.Win32;

    #endregion using directives

    /// <summary>
    /// Map the file extension to MIME type
    /// </summary>
    public class MIMETypeHelper : ISingleton
    {
        private Dictionary<String, String> mimeTypes = new Dictionary<String, String>();
        private object syncRootObject = new object();

        private MIMETypeHelper()
        {
            this.InitMimeTypes();
        }

        /// <summary>
        /// Access the data and find the mime type, if you have a file with
        /// wrong file extension name, this will be the right one
        /// </summary>
        /// <param name="filename">file path</param>
        /// <returns>the mime type string</returns>
        public String GetMimeFromFile(String filename)
        {
            var result = default(String);
            lock (syncRootObject)
            {
                if (!File.Exists(filename))
                    throw new FileNotFoundException(filename + " not found");

                var buffer = new Byte[256];
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length >= 256)
                        stream.Read(buffer, 0, 256);
                    else
                        stream.Read(buffer, 0, (Int32)stream.Length);
                }

                var mimetype = default(UInt32);
                Win32Native.FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                var mimeTypePtr = new IntPtr(mimetype);
                result = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
            }
            return result;
        }

        /// <summary>
        /// get the mime type of special file
        /// </summary>
        /// <param name="filename">file name with extension</param>
        /// <returns>the mime type string </returns>
        public String GetMimeTypeForFile(String filename)
        {
            var result = String.Empty;
            var extension = Path.GetExtension(filename);
            if (!String.IsNullOrEmpty(extension))
            {
                if (this.mimeTypes.ContainsKey(extension))
                    result = this.mimeTypes[extension];

                if (String.IsNullOrEmpty(result))
                {
                    using (var mimeDatabaseKey =
                        Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type", false))
                    {
                        if (mimeDatabaseKey != null)
                        {
                            Array.ForEach<String>(mimeDatabaseKey.GetSubKeyNames(), subkeyName =>
                            {
                                using (var contentTypeKey = mimeDatabaseKey.OpenSubKey(subkeyName))
                                {
                                    var contentTypeValue = (String)contentTypeKey.GetValue("Content Type");
                                    if (!String.IsNullOrEmpty(contentTypeValue))
                                        this.mimeTypes[contentTypeValue] = subkeyName;
                                }
                            });
                        }
                    }

                    if (this.mimeTypes.ContainsKey(extension))
                        result = this.mimeTypes[extension];
                    else
                    {
                        using (var extensionKey = Registry.ClassesRoot.OpenSubKey(extension, false))
                        {
                            if (extensionKey != null)
                            {
                                var contentType = (String)extensionKey.GetValue("Content Type");
                                if (!String.IsNullOrEmpty(contentType)) result = contentType;
                            }
                        }
                    }
                }
            }
            result = String.IsNullOrEmpty(result) ? "application/octet-stream" : result;
            return result;
        }

        /// <summary>
        /// Init a dictionary of the
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "MIME Type is unmodifiable as the cause of being referenced.")]
        private void InitMimeTypes()
        {
            this.mimeTypes.Clear();
            this.mimeTypes.Add(".3dm", "x-world/x-3dmf");
            this.mimeTypes.Add(".3dmf", "x-world/x-3dmf");
            this.mimeTypes.Add(".aab", "application/x-authorware-bin");
            this.mimeTypes.Add(".aam", "application/x-authorware-map");
            this.mimeTypes.Add(".aas", "application/x-authorware-seg");
            this.mimeTypes.Add(".abc", "text/vnd.abc");
            this.mimeTypes.Add(".acgi", "text/html");
            this.mimeTypes.Add(".afl", "video/animaflex");
            this.mimeTypes.Add(".ai", "application/postscript");
            this.mimeTypes.Add(".aif", "audio/aiff");
            this.mimeTypes.Add(".aifc", "audio/aiff");
            this.mimeTypes.Add(".aiff", "audio/aiff");
            this.mimeTypes.Add(".aim", "application/x-aim");
            this.mimeTypes.Add(".aip", "text/x-audiosoft-intra");
            this.mimeTypes.Add(".ani", "application/x-navi-animation");
            this.mimeTypes.Add(".aos", "application/x-nokia-9000-communicator-add-on-software");
            this.mimeTypes.Add(".aps", "application/mime");
            this.mimeTypes.Add(".art", "image/x-jg");
            this.mimeTypes.Add(".asf", "video/x-ms-asf");
            this.mimeTypes.Add(".asm", "text/x-asm");
            this.mimeTypes.Add(".asp", "text/asp");
            this.mimeTypes.Add(".asx", "application/x-mplayer2");
            this.mimeTypes.Add(".au", "audio/x-au");
            this.mimeTypes.Add(".avi", "video/avi");
            this.mimeTypes.Add(".avs", "video/avs-video");
            this.mimeTypes.Add(".bcpio", "application/x-bcpio");
            this.mimeTypes.Add(".bm", "image/bmp");
            this.mimeTypes.Add(".bmp", "image/bmp");
            this.mimeTypes.Add(".boo", "application/book");
            this.mimeTypes.Add(".book", "application/book");
            this.mimeTypes.Add(".boz", "application/x-bzip2");
            this.mimeTypes.Add(".bsh", "application/x-bsh");
            this.mimeTypes.Add(".bz", "application/x-bzip");
            this.mimeTypes.Add(".bz2", "application/x-bzip2");
            this.mimeTypes.Add(".c", "text/plain");
            this.mimeTypes.Add(".c++", "text/plain");
            this.mimeTypes.Add(".cat", "application/vnd.ms-pki.seccat");
            this.mimeTypes.Add(".cc", "text/plain");
            this.mimeTypes.Add(".ccad", "application/clariscad");
            this.mimeTypes.Add(".cco", "application/x-cocoa");
            this.mimeTypes.Add(".cdf", "application/cdf");
            this.mimeTypes.Add(".cer", "application/pkix-cert");
            this.mimeTypes.Add(".cha", "application/x-chat");
            this.mimeTypes.Add(".chat", "application/x-chat");
            this.mimeTypes.Add(".class", "application/java");
            this.mimeTypes.Add(".conf", "text/plain");
            this.mimeTypes.Add(".cpio", "application/x-cpio");
            this.mimeTypes.Add(".cpp", "text/plain");
            this.mimeTypes.Add(".cpt", "application/x-cpt");
            this.mimeTypes.Add(".crl", "application/pkix-crl");
            this.mimeTypes.Add(".crt", "application/pkix-cert");
            this.mimeTypes.Add(".csh", "application/x-csh");
            this.mimeTypes.Add(".css", "text/css");
            this.mimeTypes.Add(".cxx", "text/plain");
            this.mimeTypes.Add(".dcr", "application/x-director");
            this.mimeTypes.Add(".deepv", "application/x-deepv");
            this.mimeTypes.Add(".def", "text/plain");
            this.mimeTypes.Add(".der", "application/x-x509-ca-cert");
            this.mimeTypes.Add(".dif", "video/x-dv");
            this.mimeTypes.Add(".dir", "application/x-director");
            this.mimeTypes.Add(".dl", "video/dl");
            this.mimeTypes.Add(".doc", "application/msword");
            this.mimeTypes.Add(".dot", "application/msword");
            this.mimeTypes.Add(".dp", "application/commonground");
            this.mimeTypes.Add(".drw", "application/drafting");
            this.mimeTypes.Add(".dv", "video/x-dv");
            this.mimeTypes.Add(".dvi", "application/x-dvi");
            this.mimeTypes.Add(".dwf", "drawing/x-dwf (old)");
            this.mimeTypes.Add(".dwg", "application/acad");
            this.mimeTypes.Add(".dxf", "application/dxf");
            this.mimeTypes.Add(".dxr", "application/x-director");
            this.mimeTypes.Add(".el", "text/x-script.elisp");
            this.mimeTypes.Add(".elc", "application/x-elc");
            this.mimeTypes.Add(".eps", "application/postscript");
            this.mimeTypes.Add(".es", "application/x-esrehber");
            this.mimeTypes.Add(".etx", "text/x-setext");
            this.mimeTypes.Add(".evy", "application/envoy");
            this.mimeTypes.Add(".f", "text/plain");
            this.mimeTypes.Add(".f77", "text/plain");
            this.mimeTypes.Add(".f90", "text/plain");
            this.mimeTypes.Add(".fdf", "application/vnd.fdf");
            this.mimeTypes.Add(".fif", "image/fif");
            this.mimeTypes.Add(".fli", "video/fli");
            this.mimeTypes.Add(".flo", "image/florian");
            this.mimeTypes.Add(".flx", "text/vnd.fmi.flexstor");
            this.mimeTypes.Add(".fmf", "video/x-atomic3d-feature");
            this.mimeTypes.Add(".for", "text/plain");
            this.mimeTypes.Add(".fpx", "image/vnd.fpx");
            this.mimeTypes.Add(".frl", "application/freeloader");
            this.mimeTypes.Add(".funk", "audio/make");
            this.mimeTypes.Add(".g", "text/plain");
            this.mimeTypes.Add(".g3", "image/g3fax");
            this.mimeTypes.Add(".gif", "image/gif");
            this.mimeTypes.Add(".gl", "video/gl");
            this.mimeTypes.Add(".gsd", "audio/x-gsm");
            this.mimeTypes.Add(".gsm", "audio/x-gsm");
            this.mimeTypes.Add(".gsp", "application/x-gsp");
            this.mimeTypes.Add(".gss", "application/x-gss");
            this.mimeTypes.Add(".gtar", "application/x-gtar");
            this.mimeTypes.Add(".gz", "application/x-gzip");
            this.mimeTypes.Add(".gzip", "application/x-gzip");
            this.mimeTypes.Add(".h", "text/plain");
            this.mimeTypes.Add(".hdf", "application/x-hdf");
            this.mimeTypes.Add(".help", "application/x-helpfile");
            this.mimeTypes.Add(".hgl", "application/vnd.hp-HPGL");
            this.mimeTypes.Add(".hh", "text/plain");
            this.mimeTypes.Add(".hlb", "text/x-script");
            this.mimeTypes.Add(".hlp", "application/x-helpfile");
            this.mimeTypes.Add(".hpg", "application/vnd.hp-HPGL");
            this.mimeTypes.Add(".hpgl", "application/vnd.hp-HPGL");
            this.mimeTypes.Add(".hqx", "application/binhex");
            this.mimeTypes.Add(".hta", "application/hta");
            this.mimeTypes.Add(".htc", "text/x-component");
            this.mimeTypes.Add(".htm", "text/html");
            this.mimeTypes.Add(".html", "text/html");
            this.mimeTypes.Add(".htmls", "text/html");
            this.mimeTypes.Add(".htt", "text/webviewhtml");
            this.mimeTypes.Add(".htx", "text/html");
            this.mimeTypes.Add(".ice", "x-conference/x-cooltalk");
            this.mimeTypes.Add(".ico", "image/x-icon");
            this.mimeTypes.Add(".idc", "text/plain");
            this.mimeTypes.Add(".ief", "image/ief");
            this.mimeTypes.Add(".iefs", "image/ief");
            this.mimeTypes.Add(".iges", "application/iges");
            this.mimeTypes.Add(".igs", "application/iges");
            this.mimeTypes.Add(".ima", "application/x-ima");
            this.mimeTypes.Add(".imap", "application/x-httpd-imap");
            this.mimeTypes.Add(".inf", "application/inf");
            this.mimeTypes.Add(".ins", "application/x-internett-signup");
            this.mimeTypes.Add(".ip", "application/x-ip2");
            this.mimeTypes.Add(".isu", "video/x-isvideo");
            this.mimeTypes.Add(".it", "audio/it");
            this.mimeTypes.Add(".iv", "application/x-inventor");
            this.mimeTypes.Add(".ivr", "i-world/i-vrml");
            this.mimeTypes.Add(".ivy", "application/x-livescreen");
            this.mimeTypes.Add(".jam", "audio/x-jam");
            this.mimeTypes.Add(".jav", "text/plain");
            this.mimeTypes.Add(".java", "text/plain");
            this.mimeTypes.Add(".jcm", "application/x-java-commerce");
            this.mimeTypes.Add(".jfif", "image/jpeg");
            this.mimeTypes.Add(".jfif-tbnl", "image/jpeg");
            this.mimeTypes.Add(".jpe", "image/jpeg");
            this.mimeTypes.Add(".jpeg", "image/jpeg");
            this.mimeTypes.Add(".jpg", "image/jpeg");
            this.mimeTypes.Add(".jps", "image/x-jps");
            this.mimeTypes.Add(".js", "application/x-javascript");
            this.mimeTypes.Add(".jut", "image/jutvision");
            this.mimeTypes.Add(".kar", "audio/midi");
            this.mimeTypes.Add(".ksh", "text/x-script.ksh");
            this.mimeTypes.Add(".la", "audio/nspaudio");
            this.mimeTypes.Add(".lam", "audio/x-liveaudio");
            this.mimeTypes.Add(".latex", "application/x-latex");
            this.mimeTypes.Add(".list", "text/plain");
            this.mimeTypes.Add(".lma", "audio/nspaudio");
            this.mimeTypes.Add(".log", "text/plain");
            this.mimeTypes.Add(".lsp", "application/x-lisp");
            this.mimeTypes.Add(".lst", "text/plain");
            this.mimeTypes.Add(".lsx", "text/x-la-asf");
            this.mimeTypes.Add(".ltx", "application/x-latex");
            this.mimeTypes.Add(".m", "text/plain");
            this.mimeTypes.Add(".m1v", "video/mpeg");
            this.mimeTypes.Add(".m2a", "audio/mpeg");
            this.mimeTypes.Add(".m2v", "video/mpeg");
            this.mimeTypes.Add(".m3u", "audio/x-mpequrl");
            this.mimeTypes.Add(".man", "application/x-troff-man");
            this.mimeTypes.Add(".map", "application/x-navimap");
            this.mimeTypes.Add(".mar", "text/plain");
            this.mimeTypes.Add(".mbd", "application/mbedlet");
            this.mimeTypes.Add(".mc$", "application/x-magic-cap-package-1.0");
            this.mimeTypes.Add(".mcd", "application/mcad");
            this.mimeTypes.Add(".mcf", "image/vasa");
            this.mimeTypes.Add(".mcp", "application/netmc");
            this.mimeTypes.Add(".me", "application/x-troff-me");
            this.mimeTypes.Add(".mht", "message/rfc822");
            this.mimeTypes.Add(".mhtml", "message/rfc822");
            this.mimeTypes.Add(".mid", "audio/midi");
            this.mimeTypes.Add(".midi", "audio/midi");
            this.mimeTypes.Add(".mif", "application/x-mif");
            this.mimeTypes.Add(".mime", "message/rfc822");
            this.mimeTypes.Add(".mjf", "audio/x-vnd.AudioExplosion.MjuiceMediaFile");
            this.mimeTypes.Add(".mjpg", "video/x-motion-jpeg");
            this.mimeTypes.Add(".mm", "application/base64");
            this.mimeTypes.Add(".mme", "application/base64");
            this.mimeTypes.Add(".mod", "audio/mod");
            this.mimeTypes.Add(".moov", "video/quicktime");
            this.mimeTypes.Add(".mov", "video/quicktime");
            this.mimeTypes.Add(".movie", "video/x-sgi-movie");
            this.mimeTypes.Add(".mp2", "video/mpeg");
            this.mimeTypes.Add(".mp3", "audio/mpeg3");
            this.mimeTypes.Add(".mpa", "audio/mpeg");
            this.mimeTypes.Add(".mpc", "application/x-project");
            this.mimeTypes.Add(".mpe", "video/mpeg");
            this.mimeTypes.Add(".mpeg", "video/mpeg");
            this.mimeTypes.Add(".mpg", "video/mpeg");
            this.mimeTypes.Add(".mpga", "audio/mpeg");
            this.mimeTypes.Add(".mpp", "application/vnd.ms-project");
            this.mimeTypes.Add(".mpt", "application/x-project");
            this.mimeTypes.Add(".mpv", "application/x-project");
            this.mimeTypes.Add(".mpx", "application/x-project");
            this.mimeTypes.Add(".mrc", "application/marc");
            this.mimeTypes.Add(".ms", "application/x-troff-ms");
            this.mimeTypes.Add(".mv", "video/x-sgi-movie");
            this.mimeTypes.Add(".my", "audio/make");
            this.mimeTypes.Add(".mzz", "application/x-vnd.AudioExplosion.mzz");
            this.mimeTypes.Add(".nap", "image/naplps");
            this.mimeTypes.Add(".naplps", "image/naplps");
            this.mimeTypes.Add(".nc", "application/x-netcdf");
            this.mimeTypes.Add(".ncm", "application/vnd.nokia.configuration-message");
            this.mimeTypes.Add(".nif", "image/x-niff");
            this.mimeTypes.Add(".niff", "image/x-niff");
            this.mimeTypes.Add(".nix", "application/x-mix-transfer");
            this.mimeTypes.Add(".nsc", "application/x-conference");
            this.mimeTypes.Add(".nvd", "application/x-navidoc");
            this.mimeTypes.Add(".oda", "application/oda");
            this.mimeTypes.Add(".omc", "application/x-omc");
            this.mimeTypes.Add(".omcd", "application/x-omcdatamaker");
            this.mimeTypes.Add(".omcr", "application/x-omcregerator");
            this.mimeTypes.Add(".p", "text/x-pascal");
            this.mimeTypes.Add(".p10", "application/pkcs10");
            this.mimeTypes.Add(".p12", "application/pkcs-12");
            this.mimeTypes.Add(".p7a", "application/x-pkcs7-signature");
            this.mimeTypes.Add(".p7c", "application/pkcs7-mime");
            this.mimeTypes.Add(".p7m", "application/pkcs7-mime");
            this.mimeTypes.Add(".p7r", "application/x-pkcs7-certreqresp");
            this.mimeTypes.Add(".p7s", "application/pkcs7-signature");
            this.mimeTypes.Add(".part", "application/pro_eng");
            this.mimeTypes.Add(".pas", "text/pascal");
            this.mimeTypes.Add(".pbm", "image/x-portable-bitmap");
            this.mimeTypes.Add(".pcl", "application/x-pcl");
            this.mimeTypes.Add(".pct", "image/x-pict");
            this.mimeTypes.Add(".pcx", "image/x-pcx");
            this.mimeTypes.Add(".pdb", "chemical/x-pdb");
            this.mimeTypes.Add(".pdf", "application/pdf");
            this.mimeTypes.Add(".pfunk", "audio/make");
            this.mimeTypes.Add(".pgm", "image/x-portable-graymap");
            this.mimeTypes.Add(".pic", "image/pict");
            this.mimeTypes.Add(".pict", "image/pict");
            this.mimeTypes.Add(".pkg", "application/x-newton-compatible-pkg");
            this.mimeTypes.Add(".pko", "application/vnd.ms-pki.pko");
            this.mimeTypes.Add(".pl", "text/plain");
            this.mimeTypes.Add(".plx", "application/x-PiXCLscript");
            this.mimeTypes.Add(".pm", "image/x-xpixmap");
            this.mimeTypes.Add(".pm4", "application/x-pagemaker");
            this.mimeTypes.Add(".pm5", "application/x-pagemaker");
            this.mimeTypes.Add(".png", "image/png");
            this.mimeTypes.Add(".pnm", "application/x-portable-anymap");
            this.mimeTypes.Add(".pot", "application/mspowerpoint");
            this.mimeTypes.Add(".pov", "model/x-pov");
            this.mimeTypes.Add(".ppa", "application/vnd.ms-powerpoint");
            this.mimeTypes.Add(".ppm", "image/x-portable-pixmap");
            this.mimeTypes.Add(".pps", "application/mspowerpoint");
            this.mimeTypes.Add(".ppt", "application/mspowerpoint");
            this.mimeTypes.Add(".ppz", "application/mspowerpoint");
            this.mimeTypes.Add(".pre", "application/x-freelance");
            this.mimeTypes.Add(".prt", "application/pro_eng");
            this.mimeTypes.Add(".ps", "application/postscript");
            this.mimeTypes.Add(".pvu", "paleovu/x-pv");
            this.mimeTypes.Add(".pwz", "application/vnd.ms-powerpoint");
            this.mimeTypes.Add(".py", "text/x-script.phyton");
            this.mimeTypes.Add(".pyc", "applicaiton/x-bytecode.python");
            this.mimeTypes.Add(".qcp", "audio/vnd.qcelp");
            this.mimeTypes.Add(".qd3", "x-world/x-3dmf");
            this.mimeTypes.Add(".qd3d", "x-world/x-3dmf");
            this.mimeTypes.Add(".qif", "image/x-quicktime");
            this.mimeTypes.Add(".qt", "video/quicktime");
            this.mimeTypes.Add(".qtc", "video/x-qtc");
            this.mimeTypes.Add(".qti", "image/x-quicktime");
            this.mimeTypes.Add(".qtif", "image/x-quicktime");
            this.mimeTypes.Add(".ra", "audio/x-pn-realaudio");
            this.mimeTypes.Add(".ram", "audio/x-pn-realaudio");
            this.mimeTypes.Add(".ras", "application/x-cmu-raster");
            this.mimeTypes.Add(".rast", "image/cmu-raster");
            this.mimeTypes.Add(".rexx", "text/x-script.rexx");
            this.mimeTypes.Add(".rf", "image/vnd.rn-realflash");
            this.mimeTypes.Add(".rgb", "image/x-rgb");
            this.mimeTypes.Add(".rm", "application/vnd.rn-realmedia");
            this.mimeTypes.Add(".rmi", "audio/mid");
            this.mimeTypes.Add(".rmm", "audio/x-pn-realaudio");
            this.mimeTypes.Add(".rmp", "audio/x-pn-realaudio");
            this.mimeTypes.Add(".rng", "application/ringing-tones");
            this.mimeTypes.Add(".rnx", "application/vnd.rn-realplayer");
            this.mimeTypes.Add(".roff", "application/x-troff");
            this.mimeTypes.Add(".rp", "image/vnd.rn-realpix");
            this.mimeTypes.Add(".rpm", "audio/x-pn-realaudio-plugin");
            this.mimeTypes.Add(".rss", "text/xml");
            this.mimeTypes.Add(".rt", "text/richtext");
            this.mimeTypes.Add(".rtf", "text/richtext");
            this.mimeTypes.Add(".rtx", "text/richtext");
            this.mimeTypes.Add(".rv", "video/vnd.rn-realvideo");
            this.mimeTypes.Add(".s", "text/x-asm");
            this.mimeTypes.Add(".s3m", "audio/s3m");
            this.mimeTypes.Add(".sbk", "application/x-tbook");
            this.mimeTypes.Add(".scm", "application/x-lotusscreencam");
            this.mimeTypes.Add(".sdml", "text/plain");
            this.mimeTypes.Add(".sdp", "application/sdp");
            this.mimeTypes.Add(".sdr", "application/sounder");
            this.mimeTypes.Add(".sea", "application/sea");
            this.mimeTypes.Add(".set", "application/set");
            this.mimeTypes.Add(".sgm", "text/sgml");
            this.mimeTypes.Add(".sgml", "text/sgml");
            this.mimeTypes.Add(".sh", "text/x-script.sh");
            this.mimeTypes.Add(".shar", "application/x-bsh");
            this.mimeTypes.Add(".shtml", "text/html");
            this.mimeTypes.Add(".sid", "audio/x-psid");
            this.mimeTypes.Add(".sit", "application/x-sit");
            this.mimeTypes.Add(".skd", "application/x-koan");
            this.mimeTypes.Add(".skm", "application/x-koan");
            this.mimeTypes.Add(".skp", "application/x-koan");
            this.mimeTypes.Add(".skt", "application/x-koan");
            this.mimeTypes.Add(".sl", "application/x-seelogo");
            this.mimeTypes.Add(".smi", "application/smil");
            this.mimeTypes.Add(".smil", "application/smil");
            this.mimeTypes.Add(".snd", "audio/basic");
            this.mimeTypes.Add(".sol", "application/solids");
            this.mimeTypes.Add(".spc", "application/x-pkcs7-certificates");
            this.mimeTypes.Add(".spl", "application/futuresplash");
            this.mimeTypes.Add(".spr", "application/x-sprite");
            this.mimeTypes.Add(".sprite", "application/x-sprite");
            this.mimeTypes.Add(".src", "application/x-wais-source");
            this.mimeTypes.Add(".ssi", "text/x-server-parsed-html");
            this.mimeTypes.Add(".ssm", "application/streamingmedia");
            this.mimeTypes.Add(".sst", "application/vnd.ms-pki.certstore");
            this.mimeTypes.Add(".step", "application/step");
            this.mimeTypes.Add(".stl", "application/sla");
            this.mimeTypes.Add(".stp", "application/step");
            this.mimeTypes.Add(".sv4cpio", "application/x-sv4cpio");
            this.mimeTypes.Add(".sv4crc", "application/x-sv4crc");
            this.mimeTypes.Add(".svf", "image/x-dwg");
            this.mimeTypes.Add(".svr", "application/x-world");
            this.mimeTypes.Add(".swf", "application/x-shockwave-flash");
            this.mimeTypes.Add(".t", "application/x-troff");
            this.mimeTypes.Add(".talk", "text/x-speech");
            this.mimeTypes.Add(".tar", "application/x-tar");
            this.mimeTypes.Add(".tbk", "application/toolbook");
            this.mimeTypes.Add(".tcl", "text/x-script.tcl");
            this.mimeTypes.Add(".tcsh", "text/x-script.tcsh");
            this.mimeTypes.Add(".tex", "application/x-tex");
            this.mimeTypes.Add(".texi", "application/x-texinfo");
            this.mimeTypes.Add(".texinfo", "application/x-texinfo");
            this.mimeTypes.Add(".text", "text/plain");
            this.mimeTypes.Add(".tgz", "application/x-compressed");
            this.mimeTypes.Add(".tif", "image/tiff");
            this.mimeTypes.Add(".tiff", "image/tiff");
            this.mimeTypes.Add(".tr", "application/x-troff");
            this.mimeTypes.Add(".tsi", "audio/tsp-audio");
            this.mimeTypes.Add(".tsp", "audio/tsplayer");
            this.mimeTypes.Add(".tsv", "text/tab-separated-values");
            this.mimeTypes.Add(".turbot", "image/florian");
            this.mimeTypes.Add(".txt", "text/plain");
            this.mimeTypes.Add(".uil", "text/x-uil");
            this.mimeTypes.Add(".uni", "text/uri-list");
            this.mimeTypes.Add(".unis", "text/uri-list");
            this.mimeTypes.Add(".unv", "application/i-deas");
            this.mimeTypes.Add(".uri", "text/uri-list");
            this.mimeTypes.Add(".uris", "text/uri-list");
            this.mimeTypes.Add(".ustar", "multipart/x-ustar");
            this.mimeTypes.Add(".uu", "text/x-uuencode");
            this.mimeTypes.Add(".uue", "text/x-uuencode");
            this.mimeTypes.Add(".vcd", "application/x-cdlink");
            this.mimeTypes.Add(".vcs", "text/x-vCalendar");
            this.mimeTypes.Add(".vda", "application/vda");
            this.mimeTypes.Add(".vdo", "video/vdo");
            this.mimeTypes.Add(".vew", "application/groupwise");
            this.mimeTypes.Add(".viv", "video/vivo");
            this.mimeTypes.Add(".vivo", "video/vivo");
            this.mimeTypes.Add(".vmd", "application/vocaltec-media-desc");
            this.mimeTypes.Add(".vmf", "application/vocaltec-media-file");
            this.mimeTypes.Add(".voc", "audio/voc");
            this.mimeTypes.Add(".vos", "video/vosaic");
            this.mimeTypes.Add(".vox", "audio/voxware");
            this.mimeTypes.Add(".vqe", "audio/x-twinvq-plugin");
            this.mimeTypes.Add(".vqf", "audio/x-twinvq");
            this.mimeTypes.Add(".vql", "audio/x-twinvq-plugin");
            this.mimeTypes.Add(".vrml", "application/x-vrml");
            this.mimeTypes.Add(".vrt", "x-world/x-vrt");
            this.mimeTypes.Add(".vsd", "application/x-visio");
            this.mimeTypes.Add(".vst", "application/x-visio");
            this.mimeTypes.Add(".vsw", "application/x-visio");
            this.mimeTypes.Add(".w60", "application/wordperfect6.0");
            this.mimeTypes.Add(".w61", "application/wordperfect6.1");
            this.mimeTypes.Add(".w6w", "application/msword");
            this.mimeTypes.Add(".wav", "audio/wav");
            this.mimeTypes.Add(".wb1", "application/x-qpro");
            this.mimeTypes.Add(".wbmp", "image/vnd.wap.wbmp");
            this.mimeTypes.Add(".web", "application/vnd.xara");
            this.mimeTypes.Add(".wiz", "application/msword");
            this.mimeTypes.Add(".wk1", "application/x-123");
            this.mimeTypes.Add(".wmf", "windows/metafile");
            this.mimeTypes.Add(".wml", "text/vnd.wap.wml");
            this.mimeTypes.Add(".wmlc", "application/vnd.wap.wmlc");
            this.mimeTypes.Add(".wmls", "text/vnd.wap.wmlscript");
            this.mimeTypes.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
            this.mimeTypes.Add(".word", "application/msword");
            this.mimeTypes.Add(".wp", "application/wordperfect");
            this.mimeTypes.Add(".wp5", "application/wordperfect");
            this.mimeTypes.Add(".wp6", "application/wordperfect");
            this.mimeTypes.Add(".wpd", "application/wordperfect");
            this.mimeTypes.Add(".wq1", "application/x-lotus");
            this.mimeTypes.Add(".wri", "application/mswrite");
            this.mimeTypes.Add(".wrl", "application/x-world");
            this.mimeTypes.Add(".wrz", "model/vrml");
            this.mimeTypes.Add(".wsc", "text/scriplet");
            this.mimeTypes.Add(".wsrc", "application/x-wais-source");
            this.mimeTypes.Add(".wtk", "application/x-wintalk");
            this.mimeTypes.Add(".xbm", "image/x-xbitmap");
            this.mimeTypes.Add(".xdr", "video/x-amt-demorun");
            this.mimeTypes.Add(".xgz", "xgl/drawing");
            this.mimeTypes.Add(".xif", "image/vnd.xiff");
            this.mimeTypes.Add(".xl", "application/excel");
            this.mimeTypes.Add(".xla", "application/excel");
            this.mimeTypes.Add(".xlb", "application/excel");
            this.mimeTypes.Add(".xlc", "application/excel");
            this.mimeTypes.Add(".xld", "application/excel");
            this.mimeTypes.Add(".xlk", "application/excel");
            this.mimeTypes.Add(".xll", "application/excel");
            this.mimeTypes.Add(".xlm", "application/excel");
            this.mimeTypes.Add(".xls", "application/excel");
            this.mimeTypes.Add(".xlt", "application/excel");
            this.mimeTypes.Add(".xlv", "application/excel");
            this.mimeTypes.Add(".xlw", "application/excel");
            this.mimeTypes.Add(".xm", "audio/xm");
            this.mimeTypes.Add(".xml", "text/xml");
            this.mimeTypes.Add(".xmz", "xgl/movie");
            this.mimeTypes.Add(".xpix", "application/x-vnd.ls-xpix");
            this.mimeTypes.Add(".xpm", "image/xpm");
            this.mimeTypes.Add(".x-png", "image/png");
            this.mimeTypes.Add(".xsr", "video/x-amt-showrun");
            this.mimeTypes.Add(".xwd", "image/x-xwd");
            this.mimeTypes.Add(".xyz", "chemical/x-pdb");
            this.mimeTypes.Add(".z", "application/x-compressed");
            this.mimeTypes.Add(".zip", "application/zip");
            this.mimeTypes.Add(".zsh", "text/x-script.zsh");
        }
    }
}