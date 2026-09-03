import { SelectedProportionWord } from "../../../../../Utilities/CommonUtil";

const CheckedItemsCounter = ({selectedItemsCount, totalCount, actionComponent}) =>{
    
    return (
        <div className="ra-common-table-item">
            {actionComponent}
            <div className="ra-main-selected-counter">
                {SelectedProportionWord(selectedItemsCount, totalCount)}
            </div>
        </div>
    );
};

export default CheckedItemsCounter;