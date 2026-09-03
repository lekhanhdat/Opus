import { useEffect, useMemo, useRef, useState } from "react";

import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import TableTemplate from "./Template";
import SiteMapLinks from "../../../Constants/SiteMapLinks";

import "../../../Less/PRM/EditTemplate.less";
import RouterUrls from "../../../Constants/RouterUrls";
import { useStableCallback } from "../../BCM/AzureFileShareConfigureConnection/Hooks";
import { RAMessageType } from "./config";
import { showToast } from "../../../Utilities/CommonUtil";

function BarcodeManagement({ history }) {
    const [searchValue, setSearchValue] = useState("");
    const [barcodeList, setBarcodeList] = useState([]);
    const [selectedBarcodeItems, setSelectedBarcodeItems] = useState([]);
    const [pageInfo, setPageInfo] = useState({
        count: 0,
        pageIndex: 0,
        pageSize: 10,
    });

    const refTopButtons = useRef();

    const createButton = useMemo(() => {
        return [
            {
                isStatic: true,
                id: "raCreateTemplateBtn",
                name: RMResx.RM_PRM_TM_Records_Template_Create,
                onClick: () =>
                    history.push({
                        pathname: RouterUrls.PRM_BarcodeManagement_Create,
                    }),
            },
        ];
    }, []);

    const handleEditTemplate = useStableCallback(() => {
        let pathname = RouterUrls.PRM_BarcodeManagement_Edit;
        if (selectedBarcodeItems[0].IsDefault) {
            pathname = RouterUrls.PRM_BarcodeManagement_EditDefault;
        }
        history.push({
            pathname,
            search: `?suiteId=${selectedBarcodeItems[0].SuiteId}`,
        });
    });

    const editButton = useMemo(() => {
        return [
            {
                id: "raEditTemplate",
                name: RMResx.RM_PRM_TM_Records_Template_Edit,
                icon: "fia-edit",
                onClick: handleEditTemplate,
            },
        ];
    }, []);

    const onDeleteTemplate = useStableCallback(async () => {
        const option = {
            url: "/Api/TemplateManagementApi/BatchDeleteCustomBarcodeTemplateSuites",
            method: "POST",
            data: selectedBarcodeItems.map((item) => item.SuiteId),
        };

        $$.loading(true);
        const res = await fetchUtility(option);
        $$.loading(false);
        if (res.MessageType == RAMessageType.Successful) {
            showToast.success(RMResx.RM_PRM_TM_Records_Template_RemoveSuccess);
            getAllBarcodeTemplateSuites(searchValue);
        } else {
            showToast.error(res.ErrorMessage);
        }
    })

    const handleDeleteTemplate = () => {
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_PRM_TM_Records_Template_RemoveContent,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: () => $$.messagedialog(false), },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: onDeleteTemplate}, 
            ],
        });
    }

    const deleteButton = useMemo(() => {
        return [
            {
                id: "raRemoveTemplate",
                name: RMResx.RM_PRM_TM_Records_Template_Remove,
                icon: "fia-delete",
                onClick: handleDeleteTemplate,
            },
        ];
    }, []);

    const columns = useMemo(() => {
        return [
            {
                header: RMResx.RM_PRM_TM_Records_Template_Name,
                width: 250,
                resizeable: true,
            },
            {
                header: RMResx.RM_PRM_TM_Records_Template_Description,
                width: 450,
                resizeable: true,
            },
        ];
    });

    useEffect(() => {
        getAllBarcodeTemplateSuites("", pageInfo.pageIndex, pageInfo.pageSize);
    }, []);

    const getAllBarcodeTemplateSuites = async (searchValue, pageIndex, pageSize) => {
        const requestOption = {
            url: "/api/TemplateManagementApi/GetPagedBarcodeTemplateSuites",
            method: "POST",
            data: {
                SearchName: searchValue,
                PageIndex: pageIndex,
                PageSize: pageSize,
            }
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            setPageInfo({
                count: res.TotalCount,
                pageIndex: res.PageIndex,
                pageSize: res.PageSize,
            });
            if (res.Suites) {
                setBarcodeList(res.Suites);
            }
        }
    };

    const handleSearch = (value) => {
        setSearchValue(value);
        getAllBarcodeTemplateSuites(value, 0, 10);
    }

    const handleTableCheck = (selectedItems) => {
        setSelectedBarcodeItems(selectedItems);
        const isDefaultTemplate = selectedItems.some(
            (item) => item.IsDefault
        );
        if (selectedItems.length > 1 && !isDefaultTemplate) {
            refTopButtons.current.updateButtons([
                ...createButton,
                ...deleteButton,
            ]);
        } else if (selectedItems.length === 1) {
            if (isDefaultTemplate) {
                refTopButtons.current.updateButtons([
                    ...createButton,
                    ...editButton,
                ]);
            } else {
                refTopButtons.current.updateButtons([
                    ...createButton,
                    ...editButton,
                    ...deleteButton,
                ]);
            }
        } else {
            refTopButtons.current.updateButtons([...createButton]);
        }
    };

    const handlePageChange = async (pageIndex, pageSize, callback) => {
        setPageInfo((prev) => ({ ...prev, pageIndex, pageSize }));
        await getAllBarcodeTemplateSuites(searchValue, pageIndex, pageSize);
        callback(true);
    };

    return (
        <div id="rmBarcodeManagement" className="rm-tm-main-container">
            <section className="rm-tm-header">
                <$g.SiteMap data={[SiteMapLinks.PRM_BarcodeManagement]} />
            </section>
            <section id="bmContainer" className="rm-tm-content">
                <div className="barcode-management-wrapper">
                    <section className="barcode-management-search-section">
                        <R.Searchbox
                            width={280}
                            height={32}
                            placeholder={RMResx.RM_JS_TM_SearchTxt}
                            disabled={false}
                            onSearch={handleSearch}
                        />
                    </section>
                    <section className="barcode-management-actions-section flex align-center">
                        <div>
                            <TopButtonsComponent
                                ref={refTopButtons}
                                data={{ menuBtnItems: [...createButton] }}
                                showCount={3}
                            ></TopButtonsComponent>
                        </div>
                        <div className="barcode-management-format">
                            {RMResx.RM_Common_SelectTableItemsCounter.format(
                                selectedBarcodeItems.length,
                                pageInfo.count,
                            )}
                        </div>
                    </section>
                    <section className="margin-left-l margin-right-l">
                        <R.Table
                            id="raBarcodeTable"
                            rowTemplate={TableTemplate}
                            items={barcodeList}
                            columns={columns}
                            onCheck={handleTableCheck}
                            checkable
                        />
                    </section>
                    <section className="barcode-management-pager-section">
                        <$g.Pager
                            itemsCount={pageInfo.count}
                            pagerIndex={pageInfo.pageIndex}
                            pagerSize={pageInfo.pageSize}
                            showPagerSize={true}
                            showPagerCounter={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={handlePageChange}
                        />
                    </section>
                </div>
            </section>
        </div>
    );
}

export default BarcodeManagement;
