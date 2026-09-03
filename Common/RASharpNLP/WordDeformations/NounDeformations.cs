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
using PluralizeService.Core;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.SharpNLP.WordDeformations
{
    public class NounDeformations
    {

        private RALogger logger = RALogger.GetInstance(typeof(NounDeformations));
        private string[] words;
        private Dictionary<string, List<string>> wordSingularPlural = new Dictionary<string, List<string>>();
        /// <summary>
        /// Input nouns, find singular or plural, if it's singualr equals to it's plural, pass.
        /// </summary>
        /// <param name="inputWords">Nouns from FindVerbAndNoun.GetNoun()</param>
        public NounDeformations(string[] inputWords)
        {
            logger.Info("InputNouns:{0}", string.Join(", ", inputWords));

            words = inputWords;
            words = words.Distinct().ToArray();

            List<string> noun = new List<string>();
            int[] isStill = { 0, 0 };

            for (int i = 0; i < words.Count(); i++)
            {
                noun.Clear();

                if (PluralizationProvider.IsSingular(words[i]))
                {
                    noun.Add(PluralizationProvider.Pluralize(words[i]));
                }
                else if (PluralizationProvider.IsPlural(words[i]))
                {
                    noun.Add(PluralizationProvider.Singularize(words[i]));
                }
                else
                { //既不是单数也不是复数
                    logger.Info("{0} is neither singular nor plural", words[i]);
                }

                SortedSet<string> sortedWords = new SortedSet<string>(noun);
                sortedWords.Add(words[i]);
                logger.Info("After removing repeat words' count :{0}", sortedWords.Count);
                wordSingularPlural.Add(words[i], sortedWords.ToList());
                sortedWords.Clear();
            }
        }
        public Dictionary<string, List<string>> GetDeformation()
        {
            logger.Info("output nouns' count:{}", wordSingularPlural.Keys.Count);
            return wordSingularPlural;
        }

    }
}
