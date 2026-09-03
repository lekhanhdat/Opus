import { ApprovalStatus } from "./Constants/index";

export default class Utility {
    static getItemIds(items) {
        return items.map(item => item.id);
    }

    static checkHasApprovedItem(items) {
        return items.some(item => item.internalApprovedStatus === ApprovalStatus.Approved);
    }
}