import { ApprovalStatus, RelatedRecordsAction } from "./Constants/index";
import { ManualReviewAction, ManualReviewActionI18Ns } from "./Constants/ManualReviewActions";

export default class Utility {
    static getItemIds(items) {
        return items.map(item => item.id);
    }

    static checkHasApprovedItem(items) {
        return items.some(item => item.internalApprovedStatus === ApprovalStatus.Approved);
    }

    static checkHasDesotryAndNoDestoryItem(items) {
        const hasDestory = items.some(item => item.relatedRecordsAction === RelatedRecordsAction.Destory);
        const hasNoDestory = items.some(item => item.relatedRecordsAction === RelatedRecordsAction.NotDestory);
        return hasDestory && hasNoDestory;
    }

    static checkAllColumns(cells, uniqueStr) {
        let columnWidths = window["columnWidths"];
        return cells.map((cell) => {
            cell.visible = true;
            const columnUniqueId = `${uniqueStr}-${cell.id}`;
            if (columnWidths && (columnWidths[columnUniqueId] !== undefined)) {
                if (cell.width.length === 1) {
                    cell.width.push(columnWidths[columnUniqueId], "100%");
                } else {
                    cell.width[1] = columnWidths[columnUniqueId];
                }
            }
            return cell;
        });
    }

    static modifyItemsByIds(ids, items, modifiedAttr){
        return items.map((item)=>{
            item[modifiedAttr] = ids.includes(item.id);
            return item;
        });
    }

    static setCheckedOption(items, checkedValue){
        return items.map(item => {
            item.checked = item.value === checkedValue;
            return item;
        });
    }

    static convertToComboxItems(items){
        return items.map(item => {
            return {
                checked: false, 
                text: item, 
                value: item
            };
        });
    }

    static getCustomButtonNames(needCustomButton, customButtonNames){
        let approveButtonName = ManualReviewActionI18Ns.get(ManualReviewAction.Approve);
        let rejectButtonName = ManualReviewActionI18Ns.get(ManualReviewAction.Reject);
        if(needCustomButton && customButtonNames.length > 0){
            switch(RM.gData.currentLanguage){
                case "en-US":
                    approveButtonName = customButtonNames[0].englishName;
                    rejectButtonName = customButtonNames[1].englishName;
                    break;
                case "ja-JP" :
                    approveButtonName = customButtonNames[0].japaneseName;
                    rejectButtonName = customButtonNames[1].japaneseName;
                    break;
                case "zh-CN":
                    approveButtonName = customButtonNames[0].chineseName;
                    rejectButtonName = customButtonNames[1].chineseName;
                    break;
                case "ko-KR":
                    approveButtonName = customButtonNames[0].korean;
                    rejectButtonName = customButtonNames[1].korean;
                    break;
            }
        }
        return {
            approveButtonName : approveButtonName,
            rejectButtonName : rejectButtonName
        };
    }
}