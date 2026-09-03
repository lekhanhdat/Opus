import React, { useState } from "react";
import _ from "lodash";
import "./index.less";

export const OrderDropdown = ({ order, maxOrder, onChange }) => {
    const [isShow, setIsShow] = useState(false);

    const onInternalChange = (args) => {
        if (!_.isNil(onChange)) {
            onChange(order, args.newValue.value);
        }
        setIsShow(false);
    };

    return (
        <div className="reco-customization-connector-column-order">
            <R.ComboboxShell
                content={String(order)}
                width={60}
                height={"100%"}
                block={true}
                popupWidth={60}
                status={{ show: isShow }}
                compact={true}
                mini={true}
                disabled={maxOrder === 1}
                onShow={() => setIsShow(true)}
                onHide={() => setIsShow(false)}
            >
                <R.Selection
                    items={Array.from({ length: maxOrder }, (v, k) => k + 1)
                        .filter(item => item !== order)
                        .map(item => ({
                            name: item,
                            value: item
                        }))}
                    textField="name"
                    searchable={false}
                    onChange={onInternalChange}
                />
            </R.ComboboxShell>
        </div>
    );
};