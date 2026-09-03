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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharpNLP.WordDeformations;

namespace AvePoint.RA.SharpNLP
{
    public class RASharpNLPUtility
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RASharpNLPUtility));
        /// <summary>
        /// POS TAG -->Verb Tense + Noun Signular&Plurar
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        //public static Dictionary<string, List<string>> AnalyzeStringTerms_AllPOS(string[] terms)
        //{
        //    Dictionary<string, List<string>> keyValues = new Dictionary<string, List<string>>();
        //    try
        //    {
        //        RegexExpresses regexExpresses = new RegexExpresses();
        //        terms = regexExpresses.RemoveSP(terms.ToList());
        //        terms = terms.Where(s => !string.IsNullOrEmpty(s)).ToArray();

        //        logger.Info("Analyze terms {0}", string.Join(",", terms));  
        //        terms = terms.Distinct().ToArray();
        //        MixVerbNoun mixVerbNoun = new MixVerbNoun(terms);
        //        keyValues = mixVerbNoun.GetResult();
        //        logger.Info("Analyze terms result {0}", keyValues.Count);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.Message, e);
        //    }
        //    return keyValues;
        //}
        /// <summary>
        /// Noun Signular&Plurar, without POS TAG
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> AnalyzeStringTerms(string[] terms)
        {
            Dictionary<string, List<string>> keyValues = new Dictionary<string, List<string>>();
            try
            {
                RegexExpresses regexExpresses = new RegexExpresses();
                terms = regexExpresses.RemoveSP(terms.ToList());
                terms = terms.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                
                logger.Info("Analyze terms {0}", string.Join(",", terms));
                terms = terms.Distinct().ToArray();
                NounDeformations nounDeformations = new NounDeformations(terms);
                keyValues = nounDeformations.GetDeformation();
                logger.Info("Analyze terms result {0}", string.Join(";", keyValues.Select(a => { return a.Key + ":" + string.Join(",", a.Value.ToArray()); }).ToArray()));
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            return keyValues;
        }
    }
}
