import { useEffect, useState, useRef, forwardRef, useImperativeHandle } from 'react';
import TermTable from "../../Table/Index";
import _ from "lodash";
import { AddTermsTableColumns } from "../../Config/TableColumnsConfig";
import TableTemplate from "../../RowTemplate/AddTermTemplate";

const AddTermTable = ({searchValue}, ref) =>{
    
    const [usageTerms, setUsageTerms] = useState([]);
 
    const [selectedItems, setSelectedItems] = useState([]);

    const [ totalCount, setTotalCount ] = useState(0);

    const [ pagerIndex, setPagerIndex ] = useState(0);

    const [ currentPagerSize, setCurrentPagerSize ] = useState(10);

    const [ isReset, setIsReset] = useState(true);

    useEffect(()=>{
        loadUsageTerms();
    },[searchValue]);

    useImperativeHandle(ref, () => ({
        getSelectedItems: () => {
            return selectedItems;
        },
    }));

    const loadUsageTerms = async(pagerIndex, pagerSize) => {
        const requestOption = {
            url: "/api/RMMLTermApi/LoadUsageTerms",
            data: {
                PageSize: pagerSize || currentPagerSize,
                PageIndex: pagerIndex || 0,
                SearchValue: searchValue
            }
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        let isNeedReset = _.isUndefined(pagerIndex) || pagerSize != currentPagerSize;
        if(isNeedReset){
            setPagerIndex(0);
        }
        setIsReset(isNeedReset);
        setTotalCount(result.TotalCount);
        setUsageTerms(result?.UsageTerms);
        $$.loading(false);
    };

    const onSelectItems = (items) =>{ 
        setSelectedItems(items);
    };

    const onPagerChange = (pagerIndex, pagerSize, callback) => {
        setPagerIndex(pagerIndex);
        setCurrentPagerSize(pagerSize);
        loadUsageTerms(pagerIndex, pagerSize);
        callback(true);
    };

    return <div>
        <TermTable
            checkable
            isReset={isReset}
            columns={AddTermsTableColumns}
            items={usageTerms}
            template={TableTemplate}
            onSelect={onSelectItems}
        />
        <div className="ra-main-footer">
            <$g.Pager
                itemsCount={totalCount}
                showPagerSize={true}
                showPagerCounter={true}
                pagerIndex={pagerIndex}
                pagerSize={currentPagerSize}
                pagerSizeOptions={[5, 10, 15]}
                onChange={onPagerChange} />
        </div>
    </div>;
};

export default forwardRef(AddTermTable);