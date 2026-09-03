import React, { useRef, useState } from "react";
import "./index.less";
import _ from "lodash";
import { CustomizeConnectorOrigin } from "../Common/Constants";
import CreateOrEdit from "./CreateOrEdit";
import { ColumnTypeI18ns } from "../Common/I18ns";

const StaticText = ({ text }) => {
    return (
        <div
            className="static-text"
            data-tooltip="ifneed"
            aria-label={text}
            tabIndex="0"
        >
            {text}
        </div>
    );
};

const ColumnOrder = ({ order, maxOrder, onChange }) => {

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
            >
                <R.Selection
                    items={Array.from({ length: maxOrder }, (v, k) => k + 1)
                        .filter(item => item !== order)
                        .map(item => ({
                            name: item,
                            value: item
                        }))}
                    searchable={false}
                    textField="name"
                    onChange={onInternalChange}
                />
            </R.ComboboxShell>
        </div>
    );
};

const ColumnView = ({ columnInfo, maxOrder, onOrderChange, onEdit, onDelete }) => {
    return (
        <section className="column-view">
            <ColumnOrder
                order={columnInfo.order}
                maxOrder={maxOrder}
                onChange={onOrderChange}
            />
            <StaticText text={columnInfo.name} />
            <StaticText text={columnInfo.internalName} />
            <StaticText text={ColumnTypeI18ns.get(columnInfo.type)} />
            <StaticText text={columnInfo.isRequired ? RMResx.RM_EditTemplate_ColumnRequired : RMResx.RM_EditTemplate_ColumnNotRequired} />
            <div className="column-actions">
                {
                    columnInfo.origin !== CustomizeConnectorOrigin.BuildIn &&
                    <>
                        <R.Button
                            type="bald"
                            icon="fia-edit"
                            onClick={() => onEdit(columnInfo)}
                            tooltip={RMResx.RM_JS_Common_Edit}
                        />
                        <R.Button
                            type="bald"
                            icon="fia-delete"
                            onClick={() => onDelete(columnInfo)}
                            tooltip={RMResx.RM_JS_Common_Delete}
                        />
                    </>
                }
            </div>
        </section>
    );
};

const View = ({ columnInfoes = [], onChange }) => {

    const columnPanelRef = useRef();

    const onOrderChange = (oldOrder, newOrder) => {
        const oldOrderColumnInfo = columnInfoes.filter(item => item.order === oldOrder)[0];
        const newOrderColumnInfo = columnInfoes.filter(item => item.order === newOrder)[0];
        oldOrderColumnInfo.order = newOrder;
        newOrderColumnInfo.order = oldOrder;
        const newColumnInfoes = columnInfoes.filter(item => item.order !== oldOrder && item.order !== newOrder);
        newColumnInfoes.push(oldOrderColumnInfo, newOrderColumnInfo);
        onChange(_.orderBy(newColumnInfoes, ["order"], ["asc"]));
    };

    const onColumnEdit = (columnInfo) => {
        columnPanelRef.current.onShow(columnInfo);
    };

    const onColumnDelete = (columnInfo) => {
        const filteredColumnInfoes = columnInfoes.filter(item => item.order !== columnInfo.order);
        const orderedColumnInfoes = _.orderBy(filteredColumnInfoes, ["order"], ["asc"]);
        const newlyColumnInfoes = orderedColumnInfoes.map((v, i) => {
            v.order = v.order === -1 ? -1 : i;
            return v;
        });
        onChange(newlyColumnInfoes);
    };

    const onColumnNew = () => {
        columnPanelRef.current.onShow();
    };

    const onColumnChange = (columnInfo) => {

        const existColumnIndex = columnInfoes.findIndex(item => item.name === columnInfo.name);
        if(existColumnIndex > -1) {
            const existColumn = columnInfoes[existColumnIndex];
            if(existColumn.name === columnInfo.name && existColumn.order !== columnInfo.order) {
                return false;
            }
        }

        const clonedColumnInfoes = _.cloneDeep(columnInfoes);
        if(_.isNil(columnInfo.order)) {
            columnInfo.order = _.orderBy(clonedColumnInfoes, ["order"], ["desc"])[0].order + 1;
            clonedColumnInfoes.push(columnInfo);
        }
        else {
            const index = clonedColumnInfoes.findIndex(item => item.order === columnInfo.order);
            clonedColumnInfoes[index] = columnInfo;
        }
        onChange(clonedColumnInfoes);
        return true;
    };

    return (
        <div className="reco-customization-connector-columns-view">
            <section className="column-header column-view">
                <StaticText text={RMResx.RM_Connector_Column_Order} />
                <StaticText text={RMResx.RM_Connector_Column_DisplayName} />
                <StaticText text={RMResx.RM_Connector_Column_InternalName} />
                <StaticText text={RMResx.RM_Connector_Column_Type} />
                <StaticText text={RMResx.RM_Connector_Column_Required} />
                <StaticText text={RMResx.RM_Connector_Column_Action} />
            </section>
            {
                columnInfoes.filter(item => !item.isHidden).map(item =>
                    <ColumnView
                        key={item.order}
                        columnInfo={item}
                        maxOrder={_.orderBy(columnInfoes, ["order"], ["desc"])[0].order}
                        onOrderChange={onOrderChange}
                        onEdit={onColumnEdit}
                        onDelete={onColumnDelete}
                    />
                )
            }
            <div className="column-new-btn" tabIndex="0" onClick={onColumnNew}>
                <div className="fia-plus"></div>
                <div className="column-new-text">{RMResx.RM_PRM_TM_Btn_NewColumn}</div>
            </div>
            <CreateOrEdit
                ref={columnPanelRef}
                onChange={onColumnChange}
            />
        </div>
    );
};

export default View;