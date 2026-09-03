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
using AvePoint.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public static class BrowserTreeUtility
    {
        //private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(BrowserTreeUtility));

        #region Design List
        private static List<string> list;
        public static List<string> DesignLists
        {
            get
            {
                if (list == null)
                {
                    #region initialize list
                    list = new List<string>();
                    list.Add("FormServerTemplates,101".ToUpper());
                    list.Add("PublishingImages,851".ToUpper());
                    list.Add("Pages,850".ToUpper());
                    list.Add("SiteAssets,101".ToUpper());
                    list.Add("SiteCollectionDocuments,101".ToUpper());
                    list.Add("SiteCollectionImages,851".ToUpper());
                    list.Add("SitePages,119".ToUpper());
                    list.Add("Style Library,101".ToUpper());
                    list.Add("WorkflowTasks,107".ToUpper());
                    list.Add("Cache Profiles,100".ToUpper());
                    list.Add("ContentTypeSyncLog,100".ToUpper());
                    list.Add("RoutingRules,100".ToUpper());
                    list.Add("IWConvertedForms,10102".ToUpper());
                    list.Add("HoldReports,101".ToUpper());
                    list.Add("ContentTypeAppLog,100".ToUpper());
                    list.Add("Holds,100".ToUpper());
                    list.Add("Long Running Operation Status,100".ToUpper());
                    list.Add("masterpage,116".ToUpper());
                    list.Add("Notification Pages,100".ToUpper());
                    list.Add("Quick Deploy Items,100".ToUpper());
                    list.Add("Relationships List,100".ToUpper());
                    list.Add("Reporting Metadata,100".ToUpper());
                    list.Add("Reporting Templates,101".ToUpper());
                    list.Add("PackageList,100".ToUpper());
                    list.Add("solutions,121".ToUpper());
                    list.Add("Submitted E-mail Records,104".ToUpper());
                    list.Add("PublishedLinks,100".ToUpper());
                    list.Add("TaxonomyHiddenList,100".ToUpper());
                    list.Add("theme,123".ToUpper());
                    list.Add("wp,113".ToUpper());
                    list.Add("lt,114".ToUpper());
                    list.Add("wfpub,122".ToUpper());
                    list.Add("users,112".ToUpper());
                    list.Add("variation labels,100".ToUpper());

                    //German
                    list.Add("Bilder,851".ToUpper());  //PublishingImages
                    list.Add("Bilder der Websitesammlung,851".ToUpper());   //SiteCollectionImages
                    list.Add("Seiten,850".ToUpper());   //Pages
                    list.Add("Websiteobjekte,101".ToUpper());  //SiteAssets
                    list.Add("Websiteseiten,119".ToUpper());  //SitePages
                    list.Add("Workflowaufgaben,107".ToUpper());   //WorkflowTasks
                    list.Add("Konvertierte Formulare,10102".ToUpper());    //IWConvertedForms
                    list.Add("Gestaltungsvorlagenkatalog,116".ToUpper());  //masterpage
                    list.Add("Benachrichtigungsliste,100".ToUpper());  //Notification Pages  
                    list.Add("Elemente für schnelles Bereitstellen,100".ToUpper());  //Quick Deploy Items
                    list.Add("Liste der Beziehungen,100".ToUpper());   // Relationships List
                    list.Add("Fehlerprotokoll für die Inhaltstyp-Dienstanwendung,100".ToUpper());   //ContentTypeAppLog
                    list.Add("Cacheprofile,100".ToUpper());  //Cache Profiles
                    list.Add("Freigegebene Pakete,100".ToUpper());  //PackageList
                    list.Add("Vorgeschlagene Speicherorte für Inhaltsbrowser,100".ToUpper()); //PublishedLinks
                    list.Add("Fehlerprotokoll für die Inhaltstypveröffentlichung,100".ToUpper()); //ContentTypeSyncLog
                    list.Add("Formatbibliothek,101".ToUpper()); //Style Library
                    list.Add("Formularvorlagen,101".ToUpper());  //FormServerTemplates
                    list.Add("Dokumente der Websitesammlung,101".ToUpper());   //SiteCollectionDocuments
                    list.Add("Lösungskatalog,121".ToUpper());   //olutions
                    list.Add("Listenvorlagenkatalog,114".ToUpper());  //lt
                    list.Add("Benutzerinformationsliste,112".ToUpper()); //users
                    list.Add("Designkatalog,123".ToUpper());  //theme
                    list.Add("Webpartkatalog,113".ToUpper());  //wp

                    //French
                    list.Add("Images,851".ToUpper());  //PublishingImages
                    list.Add("Images de la collection de sites,851".ToUpper());   //SiteCollectionImages
                    list.Add("Pièces jointes,101".ToUpper());  //SiteAssets
                    list.Add("Documents de la collection de sites,101".ToUpper());  //SiteCollectionDocuments
                    list.Add("Rapports de suspension,101".ToUpper());  //HoldReports
                    list.Add("Pages du site,119".ToUpper()); //SitePages
                    list.Add("Modèles de formulaire,101".ToUpper());  // FormServerTemplates
                    list.Add("Bibliothèque de styles,101".ToUpper()); //Style Library
                    list.Add("Taches de flux de travail,107".ToUpper());  //WorkflowTasks
                    list.Add("Profils de cache,100".ToUpper());  //Cache Profiles
                    list.Add("Journal des erreurs de publication de type de contenu,100".ToUpper());  //ContentTypeSyncLog
                    list.Add("Règles de l’organisateur de contenu,100".ToUpper());  //RoutingRules
                    list.Add("Liste de notifications,100".ToUpper());  //Notification Pages
                    list.Add("éléments de déploiement rapide,100".ToUpper());  //Quick Deploy Items
                    list.Add("Liste de relations,100".ToUpper());  //Relationships List 
                    list.Add("Packages partagés,100".ToUpper());   //PackageList
                    list.Add("Journal des erreurs de publication de type de contenu,100".ToUpper());  //ContentTypeAppLog
                    list.Add("Suspensions,100".ToUpper());   //Holds
                    list.Add("Emplacements de navigateur de contenu suggérés,100".ToUpper()); //PublishedLinks
                    list.Add("étiquettes de variantes,100".ToUpper());  //variation labels
                    list.Add("Formulaires convertis,10102".ToUpper());  //IWConvertedForms
                    list.Add("Galerie Pages ma?tres,116".ToUpper());  //masterpage 
                    list.Add("Galerie Solutions,121".ToUpper());  //solutions
                    list.Add("Galerie Thèmes,123".ToUpper());  //theme
                    list.Add("Galerie de composants WebPart,113".ToUpper());  //wp
                    list.Add("Galerie Modèles de listes,114".ToUpper());  //lt
                    list.Add("Liste d'informations utilisateur,112".ToUpper());  //users

                    //Japanese
                    list.Add("ページ,850".ToUpper());  //Pages
                    list.Add("イメージ,851".ToUpper());
                    list.Add("サイト コレクションのイメージ,851".ToUpper());  //SiteCollectionImages
                    list.Add("サイトのページ,119".ToUpper()); //SitePages
                    list.Add("ワークフロー タスク,107".ToUpper());  //WorkflowTasks
                    list.Add("変換されたフォーム,10102".ToUpper());  //IWConvertedForms
                    list.Add("マスター ページ ギャラリー,116".ToUpper());  //masterpage
                    list.Add("バリエーション ラベル,100".ToUpper());  //variation labels
                    list.Add("通知リスト,100".ToUpper());  //Notification Pages
                    list.Add("簡易展開アイテム,100".ToUpper());  //Quick Deploy Items
                    list.Add("リレーションシップ リスト,100".ToUpper()); //Relationships List
                    list.Add("推奨されるコンテンツ ブラウザーの場所,100".ToUpper());  //PublishedLinks
                    list.Add("共有パッケージ,100".ToUpper());  //PackageList
                    list.Add("コンテンツ タイプの発行エラー ログ,100".ToUpper());//
                    list.Add("キャッシュ プロファイル,100".ToUpper()); //Cache Profiles
                    list.Add("コンテンツ タイプ サービス アプリケーションのエラー ログ,100".ToUpper());//
                    list.Add("コンテンツ オーガナイザーのルール,100".ToUpper());  //RoutingRules
                    list.Add("保留リスト,100".ToUpper());  //Holds
                    list.Add("フォーム テンプレート,101".ToUpper());  //FormServerTemplates
                    list.Add("サイトのリソース ファイル,101".ToUpper());   //SiteAssets
                    list.Add("サイト コレクションのドキュメント,101".ToUpper()); //SiteCollectionDocuments
                    list.Add("スタイル ライブラリ,101".ToUpper());  //Style Library
                    list.Add("保留リスト レポート,101".ToUpper());   //HoldReports
                    list.Add("ソリューション ギャラリー,121".ToUpper());  //solutions
                    list.Add("テーマ ギャラリー,123".ToUpper());   //theme
                    list.Add("リスト テンプレート ギャラリー,114".ToUpper());  //lt
                    list.Add("ユーザー情報リスト,112".ToUpper());  //users
                    #endregion
                }
                return list;
            }
        }
        #endregion
    }
}
