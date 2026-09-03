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

namespace AvePoint.Wrapper.Common
{
    using System.Collections.Generic;

    public static class AveResourceUtility
    {
        private static Dictionary<int, string> customWebTemplateMapping;

        static AveResourceUtility()
        {
            InitCustomWebTemplateMapping();
        }

        private static void InitCustomWebTemplateMapping()
        {
            #region init mappings
            customWebTemplateMapping = new Dictionary<int, string>();
            customWebTemplateMapping.Add(1025, "مخصص#< تحديد القالب لاحقاً... >#إنشاء موقع فارغ واختيار قالب للموقع لاحقاً.");
            customWebTemplateMapping.Add(1061, "Kohandatud#< Vali mall hiljem... >#Saate luua tühja saidi ja selle jaoks hiljem malli valida.");
            customWebTemplateMapping.Add(1069, "Pertsonalizatua#< Hautatu txantiloia geroago... >#Sortu gune huts bat, eta hautatu gunerako txantiloia geroago.");
            customWebTemplateMapping.Add(1026, "По избор#< Изберете шаблон по-късно... >#Създайте празен сайт и изберете шаблон за сайта по-късно.");
            customWebTemplateMapping.Add(1045, "Niestandardowy#< Wybierz szablon później... >#Utwórz pustą witrynę i później wybierz szablon dla niej.");
            customWebTemplateMapping.Add(1042, "사용자 지정#< 나중에 서식 파일 선택... >#빈 사이트를 만들고 사이트 서식 파일은 나중에 선택합니다.");
            customWebTemplateMapping.Add(1030, "Brugerdefineret#< Vælg skabelon senere... >#Opret et tomt websted, eller vælg en skabelon for webstedet senere.");
            customWebTemplateMapping.Add(1031, "Benutzerdefiniert#< Vorlage später auswählen... >#Eine leere Website erstellen und zu einem späteren Zeitpunkt eine Vorlage für die Website auswählen.");
            customWebTemplateMapping.Add(1049, "Другие#< Выберите шаблон позже... >#Создать пустой сайт и выбрать шаблон для этого сайта позднее.");
            customWebTemplateMapping.Add(1036, "Personnalisé#< Sélectionner le modèle ultérieurement... >#Créez un site vide et choisissez un modèle pour le site ultérieurement.");
            customWebTemplateMapping.Add(1035, "Mukautettu#< Valitse malli myöhemmin... >#Luo tyhjä sivusto ja valitse siihen malli myöhemmin.");
            customWebTemplateMapping.Add(1087, "Өзгертпелі#< Үлгіні кейінірек таңдау... >#Бос торап жасап, бұл торап үшін үлгіні кейінірек таңдаңыз.");
            customWebTemplateMapping.Add(1043, "Aangepast#< Sjabloon later selecteren... >#Maak een lege site en kies een sjabloon voor de site op een later tijdstip.");
            customWebTemplateMapping.Add(1110, "Personalizado#< Seleccionar modelo máis tarde... >#Crear un sitio baleiro e escoller un modelo para o sitio máis tarde.");
            customWebTemplateMapping.Add(1027, "Personalització#< Selecciona la plantilla més tard... >#Creeu un lloc buit i trieu una plantilla per al lloc més tard.");
            customWebTemplateMapping.Add(1029, "Vlastní#< Vybrat šablonu později... >#Umožňuje vytvořit prázdný web a vybrat šablonu pro web později.");
            customWebTemplateMapping.Add(1050, "Prilagođeno#< odaberite predložak poslije... >#Stvorite prazno web-mjesto i poslije odaberite predložak za njega.");
            customWebTemplateMapping.Add(1062, "Pielāgots#< Atlasīt veidni vēlāk... >#Izveidojiet tukšu vietni un izvēlieties vietnes veidni vēlāk.");
            customWebTemplateMapping.Add(1063, "Pasirinktinis#< Šabloną pasirinkti vėliau... >#Sukurkite tuščią svetainę, o šabloną parinkite jai vėliau.");
            customWebTemplateMapping.Add(1048, "Particularizat#< Selectare șablon mai târziu... >#Creați un site gol și selectați un șablon pentru site mai târziu.");
            customWebTemplateMapping.Add(1086, "Tersuai#< Pilih templat kemudian nanti... >#Cipta tapak kosong dan pilih templat untuk tapak kemudian nanti.");
            customWebTemplateMapping.Add(1044, "Egendefinert#< Velg mal senere... >#Opprett et tomt område, og velg en mal for området senere.");
            customWebTemplateMapping.Add(1046, "Personalizado#< Selecionar modelo mais tarde... >#Crie um site vazio e selecione posteriormente um modelo para o site.");
            customWebTemplateMapping.Add(2070, "Personalizar#< Selecionar modelo mais tarde... >#Criar um site vazio e escolher um modelo para o site numa altura posterior.");
            customWebTemplateMapping.Add(1041, "ユーザー設定#< テンプレートを後で選択... >#空のサイトを作成し、そのサイト用のテンプレートを後で選択します。");
            customWebTemplateMapping.Add(1053, "Anpassad#< Välj mall senare... >#Skapa en tom webbplats och välj en mall åt webbplatsen senare.");
            customWebTemplateMapping.Add(2074, "Prilagođeno#< Izaberite predložak kasnije... >#Kreirajte praznu lokaciju i kasnije izaberite predložak za lokaciju.");
            customWebTemplateMapping.Add(1051, "Vlastné#< Šablónu vybrať neskôr... >#Vytvorí prázdnu lokalitu a umožní vybrať šablónu lokality neskôr.");
            customWebTemplateMapping.Add(1060, "Po meri#< Predlogo izberite pozneje ... >#Ustvarite prazno mesto, predlogo za mesto pa izberite pozneje.");
            customWebTemplateMapping.Add(1054, "กำหนดเอง#< เลือกเทมเพลตภายหลัง... >#สร้างไซต์ว่างและเลือกเทมเพลตสำหรับไซต์ภายหลัง");
            customWebTemplateMapping.Add(1055, "Özel#< Şablonu daha sonra seç... >#Boş bir site oluşturun ve daha sonra site için bir şablon seçin.");
            customWebTemplateMapping.Add(1058, "Настроюваний#< вибрати шаблон пізніше... >#Створити пустий сайт і вибрати шаблон для сайту пізніше.");
            customWebTemplateMapping.Add(3082, "Personalizado#< Seleccionar la plantilla más adelante... >#Cree un sitio vacío y seleccione una plantilla para el sitio en otro momento.");
            customWebTemplateMapping.Add(1037, "התאמה אישית#< בחירת תבנית מאוחר יותר... >#צור אתר ריק ובחר תבנית עבור האתר בשלב מאוחר יותר.");
            customWebTemplateMapping.Add(1032, "Προσαρμoγή#< Επιλογή προτύπου αργότερα... >#Δημιουργία κενής τοποθεσίας και επιλογή προτύπου για την τοποθεσία αργότερα.");
            customWebTemplateMapping.Add(1038, "Egyéni#< Sablon későbbi kiválasztása... >#Hozzon létre üres webhelyet, majd később válasszon sablont a webhelyhez.");
            customWebTemplateMapping.Add(1040, "Personalizzato#< Selezionare un modello dopo... >#Consente di creare un sito vuoto e di selezionare un modello per il sito in un secondo tempo.");
            customWebTemplateMapping.Add(1081, "कस्टम#< बाद में टेम्पलेट का चयन करें... >#रिक्त साइट बनाएँ और साइट के लिए बाद में किसी टेम्पलेट को चुनें.");
            customWebTemplateMapping.Add(1057, "Kustom#< Pilih pola dasar nanti... >#Buat situs kosong dan pilih pola dasar untuk situs belakangan.");
            customWebTemplateMapping.Add(1033, "Custom#< Select template later... >#Create an empty site and pick a template for the site at a later time.");
            customWebTemplateMapping.Add(1066, "Tùy chỉnh#< Chọn mẫu sau... >#Tạo trang trống và chọn mẫu cho trang sau.");
            customWebTemplateMapping.Add(1028, "自訂#< 稍後選取範本... >#建立空白網站，稍後再選擇網站範本。");
            customWebTemplateMapping.Add(2052, "自定义#< 稍后选择模板... >#稍后请创建一个空网站并为其选取模板。");
            #endregion
        }

        public static string[] GetCustomWebTemplateTexts(int lcid)
        {
            if (customWebTemplateMapping.ContainsKey(lcid))
            {
                return customWebTemplateMapping[lcid].Split('#');
            }
            else
            {
                return customWebTemplateMapping[1033].Split('#');
            }
        }
    }
}
