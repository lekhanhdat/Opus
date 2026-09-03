import { useState, useRef, forwardRef, useImperativeHandle } from "react";
import AddTermTable from "./AddTermTable";
import SearchBox from "../../Search/SearchBox";
import { ShowResultMsg } from '../../Common';
import { showToast } from "../../../../../Utilities/CommonUtil";
import { useStableCallback } from "../../../../Common/Hooks/index";
import { Messagebox, MessageboxContentWithItems } from "../../Common";

const AddTermPanel = ({ doAction, placeholder }, ref) =>{
    
    const [showAddTermPanel, setShowAddTermPanel] = useState(false);

    const [searchValue, setSearchValue] = useState("");

    const addTermTableRef = useRef();

    useImperativeHandle(ref, () => ({
        openAddTermPanel: () => {
            setShowAddTermPanel(true);
        },
    }));

    const onCloseAddTermPanel = () => {
        setSearchValue("");
        setShowAddTermPanel(false);
    };

    const onSearch = (searchValue) => {
        setSearchValue(searchValue);
    };

    const onAddTerm = async() => {
        let selectItems = addTermTableRef.current.getSelectedItems(); 
        let termIds = selectItems.map((item)=>{
            return item.Id;
        });
        let requestOption = {
            data: termIds,
            url: "/api/RMMLTermApi/ValidateDefaultTerm",
        };

        $$.loading(true);
        let defaultTermInfo = await fetchUtility(requestOption);
        $$.loading(false);

        let { IsExists, DefaultTermNames } = defaultTermInfo;
        if(IsExists && DefaultTermNames.length > 0){
            let messageboxContent = MessageboxContentWithItems(
                {
                    msgboxDes: RMResx.RM_ML_AddTermExsitDefaultTermMsg,
                    itemsTitle: RMResx.RM_ML_DefaultTermMsg,
                    items: DefaultTermNames
                }
            );
            Messagebox({ content: messageboxContent, actionFun: onSureAddTerm, classify: "warn" });
            return false;
        }
        onSureAddTerm() ;
    };

    const onSureAddTerm =  useStableCallback(async() => {
        let selectItems = addTermTableRef.current.getSelectedItems(); 
        if(selectItems.length == 0){
            showToast.error(RMResx.RM_ML_AddTerm_NotSelectMsg);
            return false;
        }
        let param = selectItems.map((item)=>{
            return {
                Id: item.Id,
                Name: item.Name,
                Description: item.Description,
            };
        });
        const requestOption = {
            url: "/api/RMMLTermApi/AddTerms",
            data: param
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        let hasError = result.HasError;
        if(!hasError){
            setShowAddTermPanel(false);
            doAction("ADD_TERM");
        }
        ShowResultMsg(result, RMResx.RM_ML_AddTerm_Success_Tip, RMResx.RM_ML_AddTerm_Failed_Tip);
        return !hasError;
    });

    return <div>   
        <R.Panel  
            id="raMtAddTermDialog"
            header={RMResx.RM_ML_Train_AddTerm}
            size={664}
            status={{ show: showAddTermPanel }}
            onHide={onCloseAddTermPanel}
            destroy={true}   
        >
            <div id="raMlAddTermTable">
                <div className="margin-bottom-l">
                    <SearchBox onSearch={onSearch} placeholder={placeholder}/>
                </div>
                <AddTermTable 
                    ref={addTermTableRef}
                    searchValue={searchValue}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCloseAddTermPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onAddTerm} />
            </>
        </R.Panel>
    </div>;
};

export default forwardRef(AddTermPanel);