import { I18N } from "c/utils";
import { LightningElement, api } from 'lwc';
export default class ErrorPage extends LightningElement {
    @api type = "no-license";
    @api target = 'app';
    @api statusCode = 200;

    userguideLink = "";

    get isForWidget() {
        return this.target === 'widget';
    }

    get containerClass() {
        if (this.isForWidget) {
            return "no-license-page widget";
        }
        return "no-license-page app";
    }

    get isMissingConfigError() {
        return this.statusCode === 407;
    }

    get isMissingLicenseError() {
        return this.statusCode === 200
    }

    get noLicenseDescription() {
        return I18N.get("");
    }

    get missingConfigPrefix() {
        return I18N.get("");
    }

    get missingConfigGuide() {
        return I18N.get(""); 
    }

    get missingConfigSuffix() {
        return I18N.get("");
    }

    get errorMessage() {
        switch (this.statusCode) {
            case 200:
                return I18N.get("OpusApp.Error.NoSubscription.Description");
            case 403:
                return I18N.get("OpusApp.Error.AccessDenied.Description");
            case 404:
                return I18N.get("OpusApp.Error.PageNotFound.Description");
            case 500:
                return I18N.get("OpusApp.Error.InternalServerError.Description");
            case 503:
                return I18N.get("OpusApp.Error.ServiceUnavailable.Description");
            case 407:
                return I18N.get("OpusApp.Error.HasNoConfigure.Description");
            default:
                return I18N.get("OpusApp.Error.UnexpectedError.Description");
        }
    }

    get messageTitle() {
        switch (this.statusCode) {
            case 200:
                return I18N.get("OpusApp.Error.NoSubscription.Title");
            case 403:
                return I18N.get("OpusApp.Error.AccessDenied.Title");
            case 404:
                return I18N.get("OpusApp.Error.PageNotFound.Title");
            case 500:
                return I18N.get("OpusApp.Error.InternalServerError.Title");
            case 503:
                return I18N.get("OpusApp.Error.ServiceUnavailable.Title");
            case 407:
                return I18N.get("OpusApp.Error.HasNoConfigure.Title");
            default:
                return I18N.get("OpusApp.Error.UnexpectedError.Title");
        }

    }
}