import { useEffect, useRef, useState } from "react";
import { BasicDataRequester } from "../../requests";
import _ from "lodash";
import { DiscoverySizeRangeQueryMode } from "../../Constants";

const SizeRang = ({ queryParameter, onChange, ariaId }) => {

    const availabelSizeRangeOptions = useRef([]);

    const [sizeRangeOptions, setSizeRangeOptions] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const items = await BasicDataRequester.getSizeRangeList();
            availabelSizeRangeOptions.current = items;
        };
        handler();
    }, []);

    useEffect(() => {
        const sizeRanges = availabelSizeRangeOptions.current;
        const res = [{ id: -1, name: RMResx.RM_FA_Inactive_OptimizationTab_FileSizeRangeAll, queryMode: DiscoverySizeRangeQueryMode.GenerateThanEqual }].concat(sizeRanges).map(item => ({
            id: item.id,
            name: item.name,
            queryMode: item.queryMode,
            checked: item.id === queryParameter.sizeRangeQueryParameter.sizeRange,
        }));
        setSizeRangeOptions(res);
    }, [queryParameter]);

    const onInnerChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.sizeRangeQueryParameter = {
            sizeRange: args.newValue.id,
            queryMode : args.newValue.queryMode,
        };
        
        onChange(clonedValue);
    };

    return (
        <div className="reco-size-range">
            <div className="reco-fr-content">
                <div className="reco-fr-content-style">
                    <div>
                        <R.Combobox
                            id="raSizeRangeCombobox"
                            width="100%"
                            popupMaxHeight={400}
                            searchable={false}
                            items={sizeRangeOptions}
                            textField="name"
                            valueField="id"
                            tooltipField="name"
                            checkedField="checked"
                            onChange={onInnerChange}
                            aria={{ ariaLabel: ariaId }}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SizeRang;