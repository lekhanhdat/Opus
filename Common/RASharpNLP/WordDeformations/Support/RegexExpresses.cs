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
namespace AvePoint.RA.SharpNLP.WordDeformations
{
    //using AvePoint.RA.CommonUtil;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System;

    public class RegexExpresses
    {

        public Regex regexConnectWordEndS = new Regex(@"(?<=t's)$");
        public Regex regexConnectWordEndRE = new Regex(@"(?<='re)$");
        public Regex regexConnectWordEndT = new Regex(@"n't\b");
        public Regex regexConnectWordEndSP = new Regex(@"(?<=s')$");

        protected Regex regexVBZEndSES = new Regex(@"(?<=s)$|(?<=x)$|(?<=ch)$|(?<=sh)$|(?<=o)$");
        protected Regex regexVBZFuYinY = new Regex(@"[^aeiou]y\b"   );

        protected Regex regexVBGFuYinE = new Regex(@"[^aeiou]e\b");
        //Regex regexVBGCVC

        //protected Regex regexVBDFuyinE = new Regex(@"[^aeiou]e\b|ie\b");
        protected Regex regexVBDFuyinE = new Regex(@"e\b|ie\b");

        //Regex regexVBN  
        protected Regex regexVBNCEnd = new Regex(@"[^aeiou]y\b");
        protected Regex regexVBNEEnd = new Regex(@"e\b");


        protected Regex regexVBZEndIes = new Regex(@"ies\b");//y
        protected Regex regexVBZEndEs = new Regex(@"es\b");//e
        protected Regex regexVBZEndS = new Regex(@"s\b");//

        protected Regex regexVBGingEndT = new Regex(@"ting\b");
        //protected Regex regexVBGingEndEIng = new Regex(@"(?<![aeiou][aeiou][^aeiou]ing)$(?<=[aeiou][^aeiou]ing)$|(?<=[aeiou][^aeiou][^aeiou]ing)$");
        protected Regex regexVBGDouOOing = new Regex(@"([aeiou])\1[^aeiou]ing\b");//ed
        protected Regex regexVBGingEndEIng = new Regex(@"(?<=[aeiou][aeiou][^aeiou]ing)$|(?<=[aeiou][^aeiou]ing)$|(?<=[aeiou][^aeiou][^aeiou]ing)$");
        protected Regex regexVBGingEndIng = new Regex(@"ing\b");


        protected Regex regexVBDOrVBNEndied = new Regex(@"ied\b");
        protected Regex regexVBDOrVBNEndT = new Regex(@"ted\b|ped\b|shed\b|fed\b|qed\b");//去ed
        protected Regex regexVBDOrVBNDouOOed = new Regex(@"([aeiou])\1[^aeiou]ed\b");//ed
        protected Regex regexVBDOrVBNEndE = new Regex(@"(?<=[aeiou][aeiou][^aeiou]ed)$|(?<=[aeiou][^aeiou]ed)$|(?<=[aeiou][^aeiou][^aeiou]ed)$");//去d booked clothe 
        protected Regex regexVBDOrVBNEndEd = new Regex(@"ed\b");

        //不发音e
        public Regex regexBaseFormEndSilentE_VUGE = new Regex(@"ve\b|ue\b|ge\b|ce\b");//e-ing    e+d
        public Regex regexBaseFormEndSilentE_VCE = new Regex(@"[aeiou][^aeiour]e");//元音+^r的辅音+e  e-ing     e+d
        public Regex regexBaseFormEndSilentE_VCCE_VCLE = new Regex(@"[aeiou][^aeiou][^aeiou]+e\b|[aeiou][^aeiou]le\b");//元音+[辅音][辅音]+  +e||元+辅+le  e-ing    e+d
        public Regex regexBaseFormEndSilentE_Sp = new Regex(@"ere\b|are\b|ure\b|aire\b|the\b");//ere ,are,ure,aire,the  e-ing     e+d
        //to ing
        public Regex regexBaseToIngEndIe = new Regex(@"ie\b");//ie-ying
        public Regex regexBaseToIngEndY = new Regex(@"y\b");//ing
        //to ed
        public Regex regexBaseToEdEndIe = new Regex(@"ie");//ie-ied
        public Regex regexBaseToEdEndCY = new Regex(@"[^aeiou]y");//辅音+y结尾 y-ied
        public Regex regexBaseToEdEnd_NoTSoftC = new Regex(@"[pkfscrh]\b|sh\b|ch\b|th\b|tr\b");//^t清辅音结尾  ed
        public Regex regexBaseToEdEnd_VNoDHardC = new Regex(@"[aeiou][bgvzmnljw]\b|[aeiou]the\b|[aeiou]dr\b|[aeiou]ds\b");//元音+^d浊辅音结尾 ed

        public Regex regexIngToBaseEndSilentEING = new Regex(@"ering\b|aring\b|uring\b|airing\b|thing\b|asing\b|ving\b|uing\b|ging\b|cing\b");
        public Regex regexIngToBaseEndSilentEED = new Regex(@"ered\b|ared\b|ured\b|aired\b|thed\b|ased\b|ved\b|ued\b|ged\b|ced\b");
        public Regex regexIngToBaseEndSilentEs = new Regex(@"eres\b|ares\b|ures\b|aires\b|thes\b|ases\b|ves\b|ues\b|ges\b|ces\b");
       
        public bool IsSilent(string inputword)
        {
            RegexExpresses regexExpresses = new RegexExpresses();
            return regexExpresses.regexBaseFormEndSilentE_Sp.IsMatch(inputword) || regexExpresses.regexBaseFormEndSilentE_VCCE_VCLE.IsMatch(inputword)
                || regexExpresses.regexBaseFormEndSilentE_VCE.IsMatch(inputword) || regexExpresses.regexBaseFormEndSilentE_VUGE.IsMatch(inputword);
        }
        public string[] RemoveSP(List<string> originString) {
            RegexExpresses regexExpresses = new RegexExpresses();
            for (int i = 0; i < originString.Count; i++)
            {
                if (regexExpresses.regexConnectWordEndS.IsMatch(originString[i]))
                {
                    originString[i]=Regex.Replace(originString[i], @"'s\b", "");
                    originString.Insert(i+1,"is");
                }if (regexExpresses.regexConnectWordEndRE.IsMatch(originString[i]))
                {
                    originString[i]=Regex.Replace(originString[i], @"'re\b", "");
                    originString.Insert(i+1,"are");
                }
                if (regexExpresses.regexConnectWordEndT.IsMatch(originString[i]))
                {
                    originString[i]=Regex.Replace(originString[i], @"n't\b", "");
                }
                if (regexExpresses.regexConnectWordEndSP.IsMatch(originString[i]))
                {
                    originString[i]=Regex.Replace(originString[i], @"s'", "s");
                }
            }
            return originString.ToArray();
        }
        
    }
}
