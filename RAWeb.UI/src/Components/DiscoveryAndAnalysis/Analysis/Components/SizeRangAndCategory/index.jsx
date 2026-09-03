import { useEffect, useState } from "react";
import Category from "../Category";
import { BasicDataRequester } from "../../requests";
import _ from "lodash";

const SizeRangAndCategory = ({ queryParameter, onChange, o365TenantId }) => {

    const [fileSizeItems, setFileSizeItems] = useState([]);

    const [fileExtensionItems, setFileExtensionItems] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const fileExtensions = await BasicDataRequester.getFileExtensions(o365TenantId);
            fileExtensions.map(item => {
                item.checked = true;
            });
            setFileExtensionItems(fileExtensions.sort(sortFun));

            const fileSizes = await BasicDataRequester.getSizeRangeList();
            setFileSizeItems([{ id: -1, name: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeAll, checked: true }].concat(fileSizes));
        };
        handler();
    }, [o365TenantId]);

    const sortFun = (a, b) => {
        const nameA = a.name.toUpperCase();
        const nameB = b.name.toUpperCase();
        if (nameA < nameB) {
            return -1;
        }
        if (nameA > nameB) {
            return 1;
        }
        return 0;
    };

    const onSelectedFileExtensionInfo = (ids) => {
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.fileExtensionQueryParameter = {
            fileExtensions : ids.length ===  fileExtensionItems.length ? [] : ids,
        };
        onChange(clonedValue);
    };

    const onFileSizeRangeChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        if(args.newValue.id === -1) {
            clonedValue.sizeRangeQueryParameter = {};
        }
        else {
            clonedValue.sizeRangeQueryParameter = {
                sizeRange: args.newValue.id,
                queryMode : args.newValue.queryMode,
            };
        }
        
        onChange(clonedValue);
    };

    return (
        <div className="reco-file-range">
            <div className="reco-fr-content">
                <div className="reco-fr-content-style">
                    <div className="reco-fr-title" tabIndex="0">{RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle}</div>
                    <div>
                        <R.Combobox
                            id="raSizeCom"
                            width="100%"
                            popupMaxHeight={400}
                            searchable={false}
                            items={fileSizeItems}
                            textField="name"
                            valueField="id"
                            tooltipField="name"
                            checkedField="checked"
                            onChange={onFileSizeRangeChange}
                        />
                    </div>
                </div>
            </div>
            <div className="reco-fr-content">
                <div className="reco-fr-content-style">
                    <Category
                        title={RMResx.RM_FA_Inactive_OptimizationTab_FileCategoryTitle}
                        categoryItems={fileExtensionItems}
                        queryParameter={queryParameter}
                        onChange={onSelectedFileExtensionInfo}
                    />
                </div>
            </div>
        </div>
    );
};

export default SizeRangAndCategory;