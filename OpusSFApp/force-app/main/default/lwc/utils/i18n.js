import TRANSLATIONS_EN from '@salesforce/resourceUrl/translation_en';
import TRANSLATIONS_JA from '@salesforce/resourceUrl/translation_ja';
import LOCALE from '@salesforce/i18n/lang';
// import LOCALE from '@salesforce/i18n/locale';

class I18NTool {
    constructor() {
        if (!I18NTool.instance) {
            I18NTool.instance = this;
            this.translations = {};
            this.language = 'en';
            //get localize by local
            this.locale = LOCALE;
            // get localize by browser setting
            // this.locale = navigator.language || navigator.userLanguage;
        }
        return I18NTool.instance;
    }

    initialize(){
        this.detectLanguage();
        return this.loadTranslations();
    }
    detectLanguage() {
        if (this.locale.startsWith('en')) {
            this.language = 'en';
        } else if (this.locale.startsWith('ja')) {
            this.language = 'ja';
        } else {
            this.language = 'en';
        }
    }

    async loadTranslations() {
        let url;
        switch (this.language) {
            case 'en':
                url = TRANSLATIONS_EN;
                break;
            case 'ja':
                url = TRANSLATIONS_JA;
                break;
            default:
                url = TRANSLATIONS_EN;
                break;
        }

        try {
            const response = await fetch(`${url}`);
            if (response.ok) {
                this.translations = await response.json();
            } else {
                console.error('Failed to load translations');
            }
        } catch (error) {
            console.error('Error fetching translations:', error);
        }
    }

    async detectAndSetLanguage() {
        const locale = getLocale();

    }

    setLanguage(lang) {
        this.language = lang;
        return this.loadTranslations();
    }

    get(key) {
        return this.translations[key] || key;
    }
}

const I18N = new I18NTool();
export default I18N;