import _ from "lodash";
import { useState, useEffect } from "react";
import { BasicDataRequester } from "../../../../requests";

const SizeRange = ({ dataOptimizeParameter, onChange, o365TenantId }) => {

    const [fileSizeItems, setFileSizeItems] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const clonedParameter = _.cloneDeep(dataOptimizeParameter);
            const fileSizes = await BasicDataRequester.getSizeRangeList();

            if (!_.isEmpty(clonedParameter.sizeRangeQueryParameter)) {
                fileSizes.map(item => {
                    if (item.id === clonedParameter.sizeRangeQueryParameter.sizeRange) {
                        item.checked = true;
                    } else {
                        item.checked = false;
                    }
                });
                setFileSizeItems([{ id: -1, name: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeAll, checked: false }].concat(fileSizes));
            } else {
                setFileSizeItems([{ id: -1, name: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeAll, checked: true }].concat(fileSizes));
            }
        };
        handler();
    }, [dataOptimizeParameter]);

    const onFileSizeRangeChange = (args) => {
        const clonedValue = _.cloneDeep(dataOptimizeParameter);
        if (args.newValue.id === -1) {
            clonedValue.sizeRangeQueryParameter = {};
        } else {
            clonedValue.sizeRangeQueryParameter = {
                sizeRange: args.newValue.id,
                queryMode: args.newValue.queryMode,
            };
        }
        onChange(clonedValue);
    };

    return (
        <div>
            <div id="" className="reco-optimize-title require">{RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle}</div>
            <div>
                <R.Validation
                    element="Combobox"
                    require={RMResx.RM_FA_DataOptimize_Validation_ErrorMsg}>
                    <R.Combobox
                        id="raSizeCom"
                        width="100%"
                        searchable={false}
                        items={fileSizeItems}
                        textField="name"
                        valueField="id"
                        tooltipField="name"
                        checkedField="checked"
                        onChange={onFileSizeRangeChange}
                    />
                </R.Validation>
            </div>
        </div>
    );
};

export default SizeRange;