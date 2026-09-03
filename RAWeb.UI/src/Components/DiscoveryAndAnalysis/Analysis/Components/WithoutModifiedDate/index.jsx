import { useRef, useState } from "react";
import "./index.less";
import _ from "lodash";
import { useEffect } from "react";
import { TextUtil } from "../../Utils";
import { BasicDataRequester } from "../../requests";

const I18ns = {
    from: RMResx.RM_FA_Inactive_SummaryTab_ModifiedFrom,
    to: RMResx.RM_FA_Inactive_SummaryTab_ModifiedTo,
};

const WithoutModifiedDate = ({ title, queryParameter, onChange }) => {

    const availableDateOptions = useRef([]);

    const [fromToOptions, setFromToOptions] = useState({
        from: [],
        to: [],
    });

    useEffect(() => {
        const handler = async () => {
            const items = await BasicDataRequester.getWithoutInDateList();
            availableDateOptions.current = items;
            resetFromToOptions(queryParameter);
        };
        handler();
    }, []);

    useEffect(() => {
        resetFromToOptions(queryParameter);
    }, [queryParameter]);

    const resetFromToOptions = (queryParameter) => {
        const withoutDateQueryParameter = queryParameter.withoutDateQueryParameter;
        const fromOptions = getFromToOptions(
            false,
            withoutDateQueryParameter.to,
            withoutDateQueryParameter.from
        );
        const toOptions = getFromToOptions(
            true,
            withoutDateQueryParameter.from,
            withoutDateQueryParameter.to
        );
        setFromToOptions({
            from: fromOptions,
            to: toOptions,
        });
    };

    const getFromToOptions = (isGreaterThan, value, selectedValue) => {
        return availableDateOptions.current.filter((item) =>
            isGreaterThan
                ? item.id > value && item.id !== -1
                : item.id < value && item.id !== 999
        ).map((item) => {
            item.checked = item.id === selectedValue;
            return item;
        });
    };

    const onInnerChange = (field, value) => {
        const clonedValue = _.cloneDeep(queryParameter);
        clonedValue.withoutDateQueryParameter[field] = value;
        onChange(clonedValue);
    };

    const getGridTemplateColumnsStyle = () => {
        const calculateOption = {
            size: 14,
            family: "Open Sans",
        };
        const fromWidth = TextUtil.calculateTextWidth(
            I18ns.from,
            calculateOption
        );
        const toWidth = TextUtil.calculateTextWidth(I18ns.to, calculateOption);
        const comboboxWidth = `calc((100% - ${fromWidth + toWidth}px)/2 - 12px)`;
        return `${fromWidth}px ${comboboxWidth} ${toWidth}px ${comboboxWidth}`;
    };

    return (
        <div className="reco-without-modified-date">
            {title && <div className="reco-wmd-title" tabIndex="0">{title}</div>}
            <div
                className="reco-from-to"
                style={{ gridTemplateColumns: getGridTemplateColumnsStyle() }}
            >
                <div tabIndex="0">{I18ns.from}</div>
                <div>
                    <R.Combobox
                        id="raFrom"
                        width={"100%"}
                        popupMaxHeight={400}
                        searchable={false}
                        items={fromToOptions.from}
                        textField="name"
                        valueField="id"
                        onChange={(args) =>
                            onInnerChange("from", args.newValue.id)
                        }
                    />
                </div>
                <div tabIndex="0">{I18ns.to}</div>
                <div>
                    <R.Combobox
                        id="raTo"
                        width={"100%"}
                        popupMaxHeight={400}
                        searchable={false}
                        items={fromToOptions.to}
                        textField="name"
                        valueField="id"
                        onChange={(args) =>
                            onInnerChange("to", args.newValue.id)
                        }
                    />
                </div>
            </div>
        </div>
    );
};

export default WithoutModifiedDate;
