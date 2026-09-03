import { subscribe, I18N } from 'c/utils';
import { LightningElement, track } from 'lwc';
export default class Loading extends LightningElement {
    @track isLoading = true;
    @track translation = {};
    @track isRendered = false;

    async appConstructor() {
        if(this.isRendered) {
            return;
        }

        await I18N.initialize();
        this.translation = {
            loading: I18N.get("OpusApp.Common.Loading")
        }
        this.isRendered = true;
    }

    connectedCallback() {
        this.appConstructor();
        
        console.debug("loading: connected")
        subscribe('loadingEvent', this.handleLoadingEvent.bind(this));
    }

    handleLoadingEvent(isLoading) {
        console.debug("loading:handleLoadingEvent", isLoading);
        this.isLoading = isLoading;
    }
}