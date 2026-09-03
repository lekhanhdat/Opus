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


using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Gateway.Object
{

    public enum Country
	{
        [Description("")]
        None = 0,
        [Description("Afghanistan")]
        Afghanistan = 1,
        [Description("Albania")]
        Albania = 302,
        [Description("Algeria")]
        Algeria = 303,
        [Description("American Samoa")]
        American_Samoa = 304,
        [Description("Andorra")]
        Andorra = 305,
        [Description("Angola")]
        Angola = 306,
        [Description("Anguilla")]
        Anguilla = 307,
        [Description("Antarctica")]
        Antarctica = 308,
        [Description("Antigua and Barbuda")]
        Antigua_and_Barbuda = 309,
        [Description("Argentina")]
        Argentina = 10,
        [Description("Armenia")]
        Armenia = 311,
        [Description("Aruba")]
        Aruba = 312,
        [Description("Australia")]
        Australia = 14,
        [Description("Austria")]
        Austria = 15,
        [Description("Azerbaijan")]
        Azerbaijan = 315,
        [Description("Bahrain")]
        Bahrain = 316,
        [Description("Bangladesh")]
        Bangladesh = 317,
        [Description("Barbados")]
        Barbados = 318,
        [Description("Belarus")]
        Belarus = 319,
        [Description("Belgium")]
        Belgium = 22,
        [Description("Belize")]
        Belize = 321,
        [Description("Benin")]
        Benin = 322,
        [Description("Bermuda")]
        Bermuda = 323,
        [Description("Bhutan")]
        Bhutan = 324,
        [Description("Bolivia")]
        Bolivia = 325,
        [Description("Bosnia and Herzegovina")]
        Bosnia_and_Herzegovina = 326,
        [Description("Botswana")]
        Botswana = 327,
        [Description("Brazil")]
        Brazil = 328,
        [Description("British Virgin Islands")]
        British_Virgin_Islands = 234,
        [Description("Brunei Darussalam")]
        Brunei_Darussalam = 330,
        [Description("Bulgaria")]
        Bulgaria = 331,
        [Description("Burkina Faso")]
        Burkina_Faso = 332,
        [Description("Myanmar")]
        Myanmar = 333,  //----
        [Description("Burundi")]
        Burundi = 334,
        [Description("Cambodia")]
        Cambodia = 335,
        [Description("Cameroon")]
        Cameroon = 336,
        [Description("Canada")]
        Canada = 39,
        [Description("Cape Verde")]
        Cape_Verde = 338,
        [Description("Cayman Islands")]
        Cayman_Islands = 339,
        [Description("Central African Republic")]
        Central_African_Republic = 340,
        [Description("Chad")]
        Chad = 341,
        [Description("Chile")]
        Chile = 342,
        [Description("China")]
        China = 45,
        [Description("China Taiwan")]
        China_Taiwan = 344,
        [Description("Christmas Island")]
        Christmas_Island = 345,
        [Description("Cocos (Keeling) Islands")]
        Cocos_Keeling_Islands = 346,
        [Description("Colombia")]
        Colombia = 347,
        [Description("Comoros")]
        Comoros = 348,
        [Description("Cook Islands")]
        Cook_Islands = 349,
        [Description("Costa Rica")]
        Costa_Rica = 350,
        [Description("Cote d'Ivoire")]
        Cote_d_Ivoire = 351,
        [Description("Croatia")]
        Croatia = 352,
        [Description("Cuba")]
        Cuba = 353,
        [Description("Cyprus")]
        Cyprus = 354,
        [Description("Czech Republic")]
        Czech_Republic = 57,
        [Description("Democratic Republic of the Congo")]
        Democratic_Republic_of_the_Congo = 356,
        [Description("Denmark")]
        Denmark = 58,
        [Description("Djibouti")]
        Djibouti = 358,
        [Description("Dominica")]
        Dominica = 359,
        [Description("Dominican Republic")]
        Dominican_Republic = 360,
        [Description("Ecuador")]
        Ecuador = 361,
        [Description("Egypt")]
        Egypt = 64,
        [Description("El Salvador")]
        El_Salvador = 363,
        [Description("Equatorial Guinea")]
        Equatorial_Guinea = 364,
        [Description("Eritrea")]
        Eritrea = 365,
        [Description("Estonia")]
        Estonia = 366,
        [Description("Ethiopia")]
        Ethiopia = 367,
        [Description("Falkland Islands (Islas Malvinas)")]
        Falkland_Islands_Islas_Malvinas = 368,
        [Description("Faroe Islands")]
        Faroe_Islands = 369,
        [Description("Federated States of Micronesia")]
        Federated_States_of_Micronesia = 370,
        [Description("Fiji")]
        Fiji = 371,
        [Description("Finland")]
        Finland = 372,
        [Description("France")]
        France = 74,
        [Description("French Guiana")]
        French_Guiana = 374,
        [Description("French Polynesia")]
        French_Polynesia = 375,
        [Description("Gabon")]
        Gabon = 376,
        [Description("Georgia")]
        Georgia = 377,
        [Description("Germany")]
        Germany = 82,
        [Description("Ghana")]
        Ghana = 379,
        [Description("Gibraltar")]
        Gibraltar = 380,
        [Description("Greece")]
        Greece = 381,
        [Description("Greenland")]
        Greenland = 382,
        [Description("Grenada")]
        Grenada = 383,
        [Description("Guadeloupe")]
        Guadeloupe = 384,
        [Description("Guam")]
        Guam = 385,
        [Description("Guatemala")]
        Guatemala = 386,
        [Description("Guernsey")]
        Guernsey = 387,
        [Description("Guinea")]
        Guinea = 388,
        [Description("Guinea-Bissau")]
        Guinea_Bissau = 389,
        [Description("Guyana")]
        Guyana = 390,
        [Description("Haiti")]
        Haiti = 391,
        [Description("Holy See (Vatican City)")]
        Holy_See_Vatican_City = 392,
        [Description("Honduras")]
        Honduras = 393,
        [Description("Hong Kong (SAR)")]
        Hong_Kong_SAR = 394,
        [Description("Hungary")]
        Hungary = 395,
        [Description("Iceland")]
        Iceland = 396,
        [Description("India")]
        India = 103,
        [Description("Indonesia")]
        Indonesia = 398,
        [Description("Iraq")]
        Iraq = 400,
        [Description("Ireland")]
        Ireland = 107,
        [Description("Israel")]
        Israel = 109,
        [Description("Italy")]
        Italy = 110,
        [Description("Jamaica")]
        Jamaica = 405,
        [Description("Japan")]
        Japan = 113,
        [Description("Jordan")]
        Jordan = 407,
        [Description("Kazakhstan")]
        Kazakhstan = 408,
        [Description("Kenya")]
        Kenya = 409,
        [Description("Kiribati")]
        Kiribati = 410,
        [Description("Korea, South")]
        Korea_South = 412,
        [Description("Kuwait")]
        Kuwait = 413,
        [Description("Kyrgyzstan")]
        Kyrgyzstan = 414,
        [Description("Laos")]
        Laos = 415,
        [Description("Latvia")]
        Latvia = 416,
        [Description("Lebanon")]
        Lebanon = 417,
        [Description("Lesotho")]
        Lesotho = 418,
        [Description("Liberia")]
        Liberia = 419,
        [Description("Libya")]
        Libya = 420,
        [Description("Liechtenstein")]
        Liechtenstein = 421,
        [Description("Lithuania")]
        Lithuania = 422,
        [Description("Luxembourg")]
        Luxembourg = 423,
        [Description("Macao")]
        Macao = 424,
        [Description("Madagascar")]
        Madagascar = 425,
        [Description("Malawi")]
        Malawi = 426,
        [Description("Malaysia")]
        Malaysia = 136,
        [Description("Maldives")]
        Maldives = 428,
        [Description("Mali")]
        Mali = 429,
        [Description("Malta")]
        Malta = 430,
        [Description("Marshall Islands")]
        Marshall_Islands = 431,
        [Description("Martinique")]
        Martinique = 432,
        [Description("Mauritania")]
        Mauritania = 433,
        [Description("Mauritius")]
        Mauritius = 434,
        [Description("Mayotte")]
        Mayotte = 435,
        [Description("Mexico")]
        Mexico = 436,
        [Description("Moldova")]
        Moldova = 437,
        [Description("Monaco")]
        Monaco = 438,
        [Description("Mongolia")]
        Mongolia = 439,
        [Description("Montserrat")]
        Montserrat = 440,
        [Description("Morocco")]
        Morocco = 441,
        [Description("Mozambique")]
        Mozambique = 442,
        [Description("Namibia")]
        Namibia = 443,
        [Description("Nauru")]
        Nauru = 444,
        [Description("Nepal")]
        Nepal = 445,
        [Description("Netherlands")]
        Netherlands = 158,
        [Description("Netherlands Antilles")]
        Netherlands_Antilles = 447,
        [Description("New Caledonia")]
        New_Caledonia = 448,
        [Description("New Zealand")]
        New_Zealand = 161,
        [Description("Nicaragua")]
        Nicaragua = 450,
        [Description("Niger")]
        Niger = 451,
        [Description("Nigeria")]
        Nigeria = 164,
        [Description("Niue")]
        Niue = 453,
        [Description("Norfolk Island")]
        Norfolk_Island = 454,
        [Description("Northern Mariana Islands")]
        Northern_Mariana_Islands = 455,
        [Description("Norway")]
        Norway = 168,
        [Description("Oman")]
        Oman = 457,
        [Description("Pakistan")]
        Pakistan = 458,
        [Description("Palau")]
        Palau = 459,
        [Description("Panama")]
        Panama = 460,
        [Description("Papua New Guinea")]
        Papua_New_Guinea = 461,
        [Description("Paraguay")]
        Paraguay = 462,
        [Description("Peru")]
        Peru = 463,
        [Description("Philippines")]
        Philippines = 177,
        [Description("Poland")]
        Poland = 179,
        [Description("Portugal")]
        Portugal = 466,
        [Description("Puerto Rico")]
        Puerto_Rico = 467,
        [Description("Qatar")]
        Qatar = 468,
        [Description("Republic of the Congo")]
        Republic_of_the_Congo = 469,
        [Description("Reunion")]
        Reunion = 470,
        [Description("Romania")]
        Romania = 471,
        [Description("Russia")]
        Russia = 185,
        [Description("Rwanda")]
        Rwanda = 473,
        [Description("Saint Helena")]
        Saint_Helena = 474,
        [Description("Saint Kitts and Nevis")]
        Saint_Kitts_and_Nevis = 475,
        [Description("Saint Lucia")]
        Saint_Lucia = 476,
        [Description("Saint Vincent and the Grenadines")]
        Saint_Vincent_and_the_Grenadines = 478,
        [Description("Samoa")]
        Samoa = 479,
        [Description("San Marino")]
        San_Marino = 480,
        [Description("Sao Tome and Principe")]
        Sao_Tome_and_Principe = 481,
        [Description("Saudi Arabia")]
        Saudi_Arabia = 482,
        [Description("Senegal")]
        Senegal = 483,
        [Description("Serbia and Montenegro")]
        Serbia_and_Montenegro = 484,
        [Description("Seychelles")]
        Seychelles = 485,
        [Description("Sierra Leone")]
        Sierra_Leone = 486,
        [Description("Singapore")]
        Singapore = 199,
        [Description("Slovakia")]
        Slovakia = 488,
        [Description("Slovenia")]
        Slovenia = 489,
        [Description("Solomon Islands")]
        Solomon_Islands = 490,
        [Description("Somalia")]
        Somalia = 491,
        [Description("South Africa")]
        South_Africa = 204,
        [Description("Spain")]
        Spain = 206,
        [Description("Sri Lanka")]
        Sri_Lanka = 494,
        [Description("Sudan")]
        Sudan = 495,
        [Description("Suriname")]
        Suriname = 496,

        [Description("Swaziland")]
        Swaziland = 498,
        [Description("Sweden")]
        Sweden = 213,
        [Description("Switzerland")]
        Switzerland = 500,
        [Description("Tajikistan")]
        Tajikistan = 502,
        [Description("Tanzania")]
        Tanzania = 503,
        [Description("Thailand")]
        Thailand = 504,
        [Description("The Bahamas")]
        The_Bahamas = 505,
        [Description("The Gambia")]
        The_Gambia = 507,
        [Description("Togo")]
        Togo = 508,
        [Description("Tokelau")]
        Tokelau = 509,
        [Description("Tonga")]
        Tonga = 510,
        [Description("Trinidad and Tobago")]
        Trinidad_and_Tobago = 511,
        [Description("Tunisia")]
        Tunisia = 512,
        [Description("Turkey")]
        Turkey = 227,
        [Description("Turkmenistan")]
        Turkmenistan = 514,
        [Description("Turks and Caicos Islands")]
        Turks_and_Caicos_Islands = 515,
        [Description("Tuvalu")]
        Tuvalu = 516,
        [Description("Uganda")]
        Uganda = 517,
        [Description("Ukraine")]
        Ukraine = 518,
        [Description("United Arab Emirates")]
        United_Arab_Emirates = 519,
        [Description("United Kingdom")]
        United_Kingdom = 235,
        [Description("United States")]
        United_States = 236,
        [Description("Uruguay")]
        Uruguay = 522,
        [Description("Uzbekistan")]
        Uzbekistan = 523,
        [Description("Vanuatu")]
        Vanuatu = 524,
        [Description("Venezuela")]
        Venezuela = 525,
        [Description("Vietnam")]
        Vietnam = 526,
        [Description("Virgin Islands")]
        Virgin_Islands = 527,
        [Description("Yemen")]
        Yemen = 529,
        [Description("Yugoslavia")]
        Yugoslavia = 530,
        [Description("Zambia")]
        Zambia = 531,
        [Description("Zimbabwe")]
        Zimbabwe = 532,
	}
    


    public enum State
    {
        [Description("")]
        None = 0,
        #region US states

        [Description("Alabama")]
        Alabama = 1,
        [Description("Alaska")]
        Alaska = 2,
        [Description("Arizona")]
        Arizona = 3,
        [Description("Arkansas")]
        Arkansas = 4,
        [Description("California")]
        California = 5,
        [Description("Colorado")]
        Colorado = 6,
        [Description("Connecticut")]
        Connecticut = 7,
        [Description("Delaware")]
        Delaware = 8,
        [Description("District of Columbia")]
        District_of_Columbia = 9,
        [Description("Florida")]
        Florida = 10,
        [Description("Georgia")]
        Georgia = 11,
        [Description("Hawaii")]
        Hawaii = 12,
        [Description("Idaho")]
        Idaho = 13,
        [Description("Illinois")]
        Illinois = 14,
        [Description("Indiana")]
        Indiana = 15,
        [Description("Iowa")]
        Iowa = 16,
        [Description("Kansas")]
        Kansas = 17,
        [Description("Kentucky")]
        Kentucky = 18,
        [Description("Louisiana")]
        Louisiana = 19,
        [Description("Maine")]
        Maine = 20,
        [Description("Maryland")]
        Maryland = 21,
        [Description("Massachusetts")]
        Massachusetts = 22,
        [Description("Michigan")]
        Michigan = 23,
        [Description("Minnesota")]
        Minnesota = 24,
        [Description("Mississippi")]
        Mississippi = 25,
        [Description("Missouri")]
        Missouri = 26,
        [Description("Montana")]
        Montana = 27,
        [Description("Nebraska")]
        Nebraska = 28,
        [Description("Nevada")]
        Nevada = 29,
        [Description("New Hampshire")]
        New_Hampshire = 30,
        [Description("New Jersey")]
        New_Jersey = 31,
        [Description("New Mexico")]
        New_Mexico = 32,
        [Description("New York")]
        New_York = 33,
        [Description("North Carolina")]
        North_Carolina = 34,
        [Description("North Dakota")]
        North_Dakota = 35,
        [Description("Ohio")]
        Ohio = 36,
        [Description("Oklahoma")]
        Oklahoma = 37,
        [Description("Oregon")]
        Oregon = 38,
        [Description("Pennsylvania")]
        Pennsylvania = 39,
        [Description("Rhode Island")]
        Rhode_island = 40,
        [Description("South Carolina")]
        South_Carolina = 41,
        [Description("South Dakota")]
        South_Dakota = 42,
        [Description("Tennessee")]
        Tennessee = 43,
        [Description("Texas")]
        Texas = 44,
        [Description("Utah")]
        Utah = 45,
        [Description("Vermont")]
        Vermont = 46,
        [Description("Virginia")]
        Virginia = 47,
        [Description("Washington")]
        Washington = 48,
        [Description("West Virginia")]
        West_Virginia = 49,
        [Description("Wisconsin")]
        Wisconsin = 50,
        [Description("Wyoming")]
        Wyoming = 51,
        #endregion

        #region Canada states

        [Description("Alberta")]
        Alberta = 52,
        [Description("British Columbia")]
        British_Columbia = 53,
        [Description("Manitoba")]
        Manitoba = 54,
        [Description("Newfoundland and Labrador")]
        Newfoundland_and_Labrador = 55,
        [Description("New Brunswick")]
        New_Brunswick = 56,
        [Description("Northwest Territories")]
        Northwest_Territories = 57,
        [Description("Nova Scotia")]
        Nova_Scotia = 58,
        [Description("Nunavut")]
        Nunavut = 59,
        [Description("Ontario")]
        Ontario = 60,
        [Description("Prince Edward Island")]
        Prince_Edward_Island = 61,
        [Description("Quebec")]
        Quebec = 62,
        [Description("Saskatchewan")]
        Saskatchewan = 63,
        [Description("Yukon")]
        Yukon = 64,
        #endregion
    }
}
