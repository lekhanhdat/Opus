import {
    PhysicalObjectColumnType,
    PhysicalDefaultColumnIDs
} from "../Constants/Constants";

const PhyColumnUtil = {
    getDisplayValue(column, metaInfo) {
        let colId = column.uniqueId,
            colVal = metaInfo[colId],
            type = column.typeId;
        if (colVal) {
            switch (type) {
                case PhysicalObjectColumnType.SingleChoice:
                    return JSON.parse(colVal).Name;
                case PhysicalObjectColumnType.Taxonomy:
                    // if (colId == PhysicalDefaultColumnIDs.HomeLocation || colId == PhysicalDefaultColumnIDs.Classification) {
                    //     return colVal.split('|')[0];
                    // }
                    return JSON.parse(colVal).Name;
                case PhysicalObjectColumnType.MultipleChoice:
                    let colValArrNameArr = []
                    for(let item of JSON.parse(colVal)){
                        colValArrNameArr.push(item.Name);
                    }
                    return colValArrNameArr.join("; ");
                case PhysicalObjectColumnType.PeopleOrGroup:
                    let users = JSON.parse(colVal);
                    users = users.map(u => u.DisplayName);
                    return users.join("; ");
                case PhysicalObjectColumnType.DateTime:
                    let dt = JSON.parse(colVal);
                    let timeZoneInfo = RM.TimeUtil.getTimezoneInfo(dt.TimeZoneId, dt.IsSetDayLight);
                    return RM.TimeUtil.dateToString(new Date(dt.Date), timeZoneInfo, true);
                default:
                    return colVal;
            }
        } else {
            return '';
        }
    }
};
export default PhyColumnUtil;