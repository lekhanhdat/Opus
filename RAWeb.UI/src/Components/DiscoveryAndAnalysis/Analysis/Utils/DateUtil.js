class DateUtil {

    static i18nMonth = (intMonth) => {
        // const monthEnglish = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
        const monthEnglish = [
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Jan,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Feb,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Mar,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Apr,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_May,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Jun,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Jul,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Aug,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Sep,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Oct,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Nov,
            RMResx.RM_JS_Common_AUI_Datepicker_ShortMonths_Dec,
        ];
        return monthEnglish[intMonth-1];
    }

}

export default DateUtil;