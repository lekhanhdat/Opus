import { useEffect, useState } from "react";
import _ from "lodash";

const SortBy = ({ sortByColumns, queryParameter, onChange, ariaId }) => {

    const [sortByColumnOptions, setSortByColumnOptions] = useState([]);

    useEffect(() => {
        const res = sortByColumns.map(item => ({
            id: item.internalName,
            name: item.displayName,
            checked: item.internalName === queryParameter.nodeQueryParameter.sortBy,
        }));

        if(_.isEmpty(queryParameter.nodeQueryParameter.sortBy)) {
            res[0].checked = true;
        }

        setSortByColumnOptions(res);
    }, [queryParameter]);

    const onInnerChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.nodeQueryParameter = {
            sortBy: args.newValue.id,
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
                            items={sortByColumnOptions}
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

export default SortBy;