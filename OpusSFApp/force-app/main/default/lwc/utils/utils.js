import { appAPIName } from 'c/store';
import Toast from "lightning/toast";
export { default as I18N } from './i18n';
export { default as UnitConvertingUtil } from './unitConverting';
const events = {};
export function subscribe(eventName, callback) {
    if (!events[eventName]) {
        events[eventName] = [];
    }
    events[eventName] = [callback];
}

export function publish(eventName, data) {
    if (!events[eventName]) return;
    events[eventName].forEach(callback => callback(data));
}

export function showToast(comp, label, message, variant, mode, labelLinks, messageLinks) {
    Toast.show({
        label,
        labelLinks,
        message,
        messageLinks,
        variant,
        mode,
    }, comp);
}

export function getPackageNamespace() {
    const splittedAppApiName = appAPIName.split("__");

    if (splittedAppApiName.length > 1) {
        return splittedAppApiName[0];
    }

    return "c"; //Dev env
}