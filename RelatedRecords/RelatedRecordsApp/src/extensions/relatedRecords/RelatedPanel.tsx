/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved.
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
import * as React from 'react';
import {
    Icon,
    Panel,
    PanelType,
    SearchBox,
    Callout,
    DefaultButton,
    PrimaryButton,
    IconButton,
    TooltipHost,
    Spinner,
    MessageBar,
    MessageBarType,
    IDetailsList,
} from '@fluentui/react';
import {
    DetailsList,
    IColumn,
    SelectionMode,
    Selection,
    TooltipOverflowMode,
    SpinnerSize,
} from '@fluentui/react';
import { mergeStyleSets } from '@fluentui/react/lib/Styling';
import { IListViewCommandSetExecuteEventParameters } from '@microsoft/sp-listview-extensibility';
import { ExtensionContext } from '@microsoft/sp-extension-base';
import { initializeFileTypeIcons, getFileTypeIconProps, IFileTypeIconOptions, FileIconType } from '@fluentui/react-file-type-icons';
import { css } from '@fluentui/react/lib/Utilities';

import RelatedDetail from './RelatedDetail';
import PnpUtil from '../../common/PnpUtil';
import { checkRelatedRecordsEqual, getRelatedRecordsFromXmlData, removeCurlyBraces } from './common/RelatedRedordsUtil';
import { ResultState } from './constants/search-result';
import { MessageType } from './constants/related-record';
import { IRecordSaved, IRelatedInfo, IRelatedRecordItem, ISaveRelatedRecord } from './types/related-record';
import * as HttpClientUtil from '../../common/HttpClientUtil';

import './RelatedPanel.css'
import * as strings from 'RelatedRecordsCommandSetStrings';
import { NodeType } from './constants/node-type';
import { LazyLoadTable } from '../../components';

export interface IRelatedPanelProps {
    event: IListViewCommandSetExecuteEventParameters;
    spContext: ExtensionContext;
}

export interface IRelatedPanelState {
    showPanel: boolean;
    showDetailPanel: boolean;
    columns: IColumn[],
    items: IRelatedRecordItem[];
    selectedKeys: string[],
    deleteItems: IRelatedRecordItem[];
    searchItems: IRelatedRecordItem[];
    searchValue: string;
    isSearching: boolean;
    currentPage: number;
    totalPage: number;
    isCalloutVisible: boolean;
    payloadData: IRelatedRecordItem | null;
    isSaving: boolean;
    error: {
        isShow: boolean,
        message: string
    };
    existedRelatedRecords: IRelatedRecordItem[];
    selectedSearchKeys: Set<string>;
}

const classNames = mergeStyleSets({
    iconCell: {
        display: "flex !important",
        alignItems: "center",
    },
    fileIconHeaderIcon: {
        fontSize: 16,
    },
    itemWrap: {
        height: "100%",
        display: "flex",
        alignItems: "center",
        fontSize: 14,
    },
    itemText: {
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
        overflow: "hidden",
    },
    itemLink: {
        color: "#0072d0",
        textDecoration: "underline",
        cursor: "pointer",
    },
    actionIcon: {
        width: 20,
        height: 20,
    },
    // searchResultWrapper: {
    //     maxHeight: 325,
    //     overflowX: "hidden",
    //     overflowY: "auto",
    // },
    searchResultLoading: {
        height: 43,
        marginRight: 48,
    },
    searchResultEmpty: {
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        fontSize: 14,
    },
})

const columns4Drop: IColumn[] = [
    {
        key: 'key',
        name: 'Name',
        fieldName: 'name',
        minWidth: 100,
        maxWidth: 150,
        isRowHeader: false,
        isResizable: true,
        onRender: (item) => {
            if (item.key === ResultState.IS_SEARCHING) {
                return <Spinner size={SpinnerSize.medium} className={classNames.searchResultLoading} />
            }

            if (item.key === ResultState.EMPTY) {
                return <div className={`${classNames.searchResultLoading} ${classNames.searchResultEmpty}`}>{strings.Related_App_AddRelated_SearchResult_Empty}</div>
            }

            return (
                <div className='related-records-panel-search-result-col'>
                    <div style={{ width: 20 }}>
                        <Icon {...getFileTypeIconProps(item.iconOptions)} />
                    </div>
                    <div className='related-records-panel-search-result-col-info'>
                        <TooltipHost
                            id={item.key}
                            overflowMode={TooltipOverflowMode.Self}
                            hostClassName={css(classNames.itemText)}
                            content={item.name}
                        >
                            <strong className='related-records-detail-panel-section-info_value'>{item.name}</strong>
                        </TooltipHost>
                        <TooltipHost
                            id={item.key}
                            overflowMode={TooltipOverflowMode.Self}
                            hostClassName={css(classNames.itemText)}
                            content={item.path}
                        >
                            <i>{item.path}</i>
                        </TooltipHost>
                    </div>
                </div>
            )
        },
    }
];

export class RelatedPanel extends React.Component<IRelatedPanelProps, IRelatedPanelState> {
    private itemSelection: Selection;
    private componentRef: React.RefObject<IDetailsList>;
    private searchItemSelection: Selection;
    private spUtil: PnpUtil;

    constructor(props: IRelatedPanelProps) {
        super(props);
        this.spUtil = new PnpUtil(this.props.spContext);
        initializeFileTypeIcons();
        this.preCheck();
        const columns: IColumn[] = [
            {
                key: 'fileType',
                name: "File Type",
                fieldName: 'name',
                iconName: "Page",
                isIconOnly: true,
                iconClassName: classNames.fileIconHeaderIcon,
                minWidth: 20,
                maxWidth: 20,
                className: classNames.iconCell,
                onRender: (item) => <Icon {...getFileTypeIconProps(item.iconOptions)} />,
            },
            {
                key: 'name',
                name: strings.Related_App_RelatedColumn_Name,
                fieldName: 'name',
                minWidth: 150,
                maxWidth: 300,
                isRowHeader: true,
                isResizable: true,
                isSorted: true,
                isSortedDescending: false,
                onColumnClick: this.onColumnSort,
                onRender: (item) => (
                    <div className={classNames.itemWrap}>
                        <TooltipHost
                            id={item.key}
                            overflowMode={TooltipOverflowMode.Self}
                            hostClassName={css(classNames.itemText)}
                            content={item.name}
                        >
                            <p
                                aria-labelledby={item.name}
                                className={classNames.itemLink}
                                onClick={() => this.setState({ payloadData: item, showDetailPanel: true })}
                                onKeyDown={(e) => e.keyCode == 13 && this.setState({ payloadData: item, showDetailPanel: true })}
                            >
                                {item.name}
                            </p>
                        </TooltipHost>
                    </div>
                ),
            },
            {
                key: 'path',
                name: strings.Related_App_RelatedColumn_Location,
                fieldName: 'path',
                minWidth: 200,
                maxWidth: 350,
                isRowHeader: true,
                isResizable: true,
                isSorted: true,
                isSortedDescending: false,
                onColumnClick: this.onColumnSort,
                onRender: (item) => (
                    <div className={classNames.itemWrap}>
                        <TooltipHost
                            id={item.key}
                            overflowMode={TooltipOverflowMode.Self}
                            hostClassName={css(classNames.itemText)}
                            content={item.path}
                            aria-label={item.path}
                        >
                            {item.path}
                        </TooltipHost>
                    </div>
                ),
            },
            {
                key: 'action',
                name: strings.Related_App_RelatedColumn_Action,
                fieldName: 'action',
                minWidth: 100,
                isRowHeader: true,
                onRender: (item) => {
                    if (item.sourceFlag !== 1) { // is Physical folder
                        return null;
                    }

                    return (
                        <div className={classNames.itemWrap} onMouseDown={(e) => e.stopPropagation()}>
                            <TooltipHost id="Delete" content={strings.Related_App_Common_Delete} aria-label={strings.Related_App_Common_Delete}>
                                <IconButton className={classNames.actionIcon} iconProps={{ iconName: "Delete" }} onClick={() => this.onRemoveRecordItem(item)} />
                            </TooltipHost>
                        </div>
                    );
                },
            },
        ];

        this.state = {
            showPanel: true,
            showDetailPanel: false,
            columns,
            items: this.getRelatedRecords(props),
            selectedKeys: [],
            deleteItems: [],
            searchItems: [],
            searchValue: '',
            isSearching: false,
            currentPage: 0,
            totalPage: 0,
            isCalloutVisible: false,
            payloadData: null,
            isSaving: false,
            error: {
                isShow: false,
                message: "",
            },
            existedRelatedRecords: this.getRelatedRecords(props).filter((item) => item.sourceFlag === 1),
            selectedSearchKeys: new Set<string>(),
        };
        this.componentRef = React.createRef<IDetailsList>();
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        this.itemSelection = new Selection({
            canSelectItem(item, index) {
                return false;
            },
            onSelectionChanged: () => {
                const selectedItems = this.itemSelection.getSelection();
                this.setState({ selectedKeys: selectedItems.map((item) => item.key as string) });
            },
        });
        this.itemSelection.setAllSelected(true);
        this.searchItemSelection = new Selection({
            getKey: item => item.key?.toString() ?? '',
            canSelectItem(item, index) {
                if (item.key === ResultState.IS_SEARCHING || item.key === ResultState.EMPTY) {
                    return false;
                }

                return true;
            },
            onSelectionChanged() {
                const selectedItems = self.searchItemSelection?.getSelection();
                if (selectedItems && selectedItems.length > 0) {
                    self.setState({
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        selectedSearchKeys: new Set(selectedItems.map((item: any) => item.key as string))
                    });
                }
            },
        });
    }

    private preCheck() {
        HttpClientUtil.callRecordsApi(this.props.spContext, "/API/AppActions/Test", null);
    }

    private searchBoxRef: React.RefObject<HTMLDivElement> = React.createRef();

    private getRelatedRecords = (props: IRelatedPanelProps): IRelatedRecordItem[] => {
        const xmlString = props.event.selectedRows[0].getValueByName("RecordsRelated");
        const relatedRecordsList = getRelatedRecordsFromXmlData(xmlString);
        const tempItems: IRelatedRecordItem[] = [];

        relatedRecordsList.forEach(item => {
            let extension = "";
            let name = item.name;
            let fileIconType = FileIconType.folder;
            if (item.SourceFlag === 1 || item.SourceFlag === 0) {
                name = item.name;
                extension = (item.name as string).substring((item.name as string).lastIndexOf("."));
                //5: sharepoint list item
                //6: sharepoint document
                //Maybe need a enum for level
                if (item.level == 5) {
                    fileIconType = FileIconType.listItem;
                    extension = "";
                }
            } else {
                name = item.recId;
                extension = "";
                if (item.NodeType == NodeType.PhyRecord) {
                    fileIconType = FileIconType.genericFile;
                }
            }

            tempItems.push({
                key: item.id,
                name: name,
                path: item.url,
                iconOptions: { type: fileIconType, extension: extension, size: 20 } as IFileTypeIconOptions,
                listId: item.ListId,
                webId: item.WebId,
                uniqueId: item.id,
                listItemId: item.DocLibRowId,
                sourceFlag: item.SourceFlag,
                siteUrl: item.SiteUrl,
                siteId: item.SiteId,
                nodeType: item.NodeType,
                needDelete: false,
                isRelatedRecord: true,
            })
        });

        return tempItems;
    }

    private getEmptyAndLoadingItems = (key: ResultState): IRelatedRecordItem => {
        return {
            key,
            name: key,
            path: key,
            iconOptions: { extension: "", size: 20 } as IFileTypeIconOptions,
            listId: "",
            webId: "",
            uniqueId: "",
            listItemId: 0,
            sourceFlag: 0,
            siteUrl: "",
            siteId: "",
            needDelete: false,
            isRelatedRecord: false,
        }
    }

    private onSearch = async (newValue: string): Promise<IRelatedRecordItem[]> => {
        const tempItems: IRelatedRecordItem[] = [];
        const { datas, totalPages } = await this.spUtil.searchAllSites(newValue, this.state.currentPage);

        datas.forEach(row => {
            tempItems.push({
                key: removeCurlyBraces(row.UniqueId as string),
                name: (row.IsDocument as any).toLowerCase() === 'true' ? row.Filename : row.Title ?? "",
                path: row.Path || "",
                iconOptions: { extension: row.FileExtension, size: 20 } as IFileTypeIconOptions,
                listId: row.ListId || "",
                webId: row.WebId || "",
                uniqueId: removeCurlyBraces(row.UniqueId as string) || "",
                listItemId: Number(row.ListItemID),
                sourceFlag: 1,
                siteUrl: row.SPSiteUrl,
                siteId: row.SiteId || "",
                isRelatedRecord: false
            })
        });

        // Handle duplicate element in tempItems and this.state.items
        const availableRecords = new Map(
            this.state.items.map<[string, boolean]>((item) => [
                removeCurlyBraces(item.key),
                true,
            ])
        ); // Remove {} of key in search result
        const newSearchItems = tempItems.filter(
            (item) =>
                !availableRecords.has(removeCurlyBraces(item.key)) &&
                (removeCurlyBraces(item.key) !==
                    removeCurlyBraces(this.props.event.selectedRows[0].getValueByName("UniqueId").toLowerCase())));
        const searchItems = newSearchItems.length
            ? newSearchItems
            : [this.getEmptyAndLoadingItems(ResultState.EMPTY)];
        this.setState({
            totalPage: totalPages,
            isCalloutVisible: true
        });
        return searchItems;
    }

    private handleSearch = (newValue?: string): void => {
        if (newValue) {
            this.setState({
                isSearching: true,
                searchItems: [this.getEmptyAndLoadingItems(ResultState.IS_SEARCHING)],
                currentPage: 0,
                isCalloutVisible: true
            }, () => {
                this.onSearch(newValue)
                    .then((searchItems) => {
                        this.setState({ isSearching: false, searchItems });
                    })
                    .catch(() => console.error("Search related records for SPOnPremises error!"));
            });
        } else {
            this.setState({ isSearching: false, searchItems: [], isCalloutVisible: false });
        }
    }

    private onColumnSort = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        const { columns, items } = this.state;
        const newColumns: IColumn[] = columns.slice();
        const currColumn: IColumn = newColumns.filter(currCol => column.key === currCol.key)[0];

        newColumns.forEach((newCol: IColumn) => {
            if (newCol === currColumn) {
                currColumn.isSortedDescending = !currColumn.isSortedDescending;
                currColumn.isSorted = true;
            }
        });

        const newItems = this.onCopyAndSort(items, currColumn.fieldName!, currColumn.isSortedDescending);

        this.setState({
            columns: newColumns,
            items: newItems,
        });
    }

    onCopyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
        const key = columnKey as keyof T;
        return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
    }

    onRemoveRecordItem = (item: any) => {
        const newItems = this.state.items.filter((row) => row.key !== item.key);
        const map = new Map();

        this.setState((prev) => ({
            items: newItems,
            deleteItems: [...prev.deleteItems, {
                key: item.key,
                name: item.name,
                path: item.path,
                iconOptions: item.iconOptions,
                listId: item.listId,
                webId: item.webId,
                uniqueId: item.uniqueId,
                listItemId: item.listItemId,
                sourceFlag: item.sourceFlag,
                siteUrl: item.siteUrl,
                siteId: item.siteId,
                needDelete: true,
                isRelatedRecord: item.isRelatedRecord,
            }].filter((item) => {
                if (!map.has(item.key)) {
                    map.set(item.key, true);
                    return true;
                }

                return false;
            })
        }));
    }

    private onChangeSearchValue = (newValue: string): void => {
        this.setState({
            searchValue: newValue,
            searchItems: []
        });
        this.onDismissCallout();
    }

    private onScrollSearchResult = (): void => {
        this.setState((prev) => ({
            isSearching: true,
            searchItems: [
                ...prev.searchItems,
                this.getEmptyAndLoadingItems(ResultState.IS_SEARCHING),
            ],
            currentPage: prev.currentPage < prev.totalPage ? prev.currentPage + 1 : prev.currentPage,
        }), () => {
            this.onSearch(this.state.searchValue)
                .then((newSearchItems) => {
                    const { searchItems } = this.state;
                    const itemsWithoutLoading = searchItems.filter(
                        (item) => item.key !== ResultState.IS_SEARCHING
                    );
                    const updatedNewSearchItems = [...itemsWithoutLoading, ...newSearchItems];

                    this.searchItemSelection.setChangeEvents(false, false);
                    this.searchItemSelection.setItems(updatedNewSearchItems, false);

                    this.setState({
                        isSearching: false,
                        searchItems: updatedNewSearchItems,
                    });

                    setTimeout(() => {
                        updatedNewSearchItems.forEach((item) => {
                            if (this.state.selectedSearchKeys.has(item.key as string)) {
                                this.searchItemSelection.setKeySelected(item.key, true, false);
                            }
                        });
                        this.searchItemSelection.setChangeEvents(true, true);
                        this.componentRef.current?.forceUpdate();
                    }, 0);
                })
                .catch(() => console.error("Search related records for SPO error!"));
        });
    }

    onDismissCallout = () => {
        this.setState({ isSearching: false, currentPage: 0, isCalloutVisible: false, selectedSearchKeys: new Set<string>() });
    };

    onDismissPanel = () => {
        !this.state.showDetailPanel && this.setState({ showPanel: false });
    }

    onError = (isShowError: boolean, errorMsg: string) => {
        this.setState({
            error: {
                isShow: isShowError,
                message: errorMsg,
            }
        });
    }

    onSubmitRelatedItems = async (params: ISaveRelatedRecord) => {
        try {
            const res: IRecordSaved = await HttpClientUtil.callRecordsApi(this.props.spContext, "/API/AppActions/SubmitRelatedItems", params);
            if (res.MessageType === MessageType.Failed) {
                this.onError(true, res.ErrorMessage);
            } else {
                this.setState({ showPanel: false });
                window.location.reload();
            }
        } catch (error) {
            this.onError(true, error?.message ?? error);
            console.error("Save error: ", error);
        } finally {
            this.setState({ isSaving: false });
        }
    }

    onCheckPermissionOfFile = async (siteURL: string, listId: string, listItemId: number): Promise<boolean> => {
        return await this.spUtil.checkItemHasEditPermission(siteURL, listId, listItemId);
    }

    onSaveRelatedRecords = async () => {
        const addRelatedRecords: IRelatedRecordItem[] = this.state.items.filter((item) => item.sourceFlag === 1)
        const deleteRelatedRecords: IRelatedRecordItem[] = this.state.deleteItems.filter((item) => item.sourceFlag === 1 && item.isRelatedRecord);

        // Remove duplicate data when delete real file 2, then search and add file 2 again (local temp) and click save (mean no change)
        const relatedInfos: IRelatedInfo[] = [...addRelatedRecords, ...deleteRelatedRecords].filter((item, index, self) => {
            const isDuplicate = self.some((otherItem, otherIndex) => otherIndex !== index && otherItem.key === item.key);
            return !isDuplicate;
        }).map((item) => ({
            Name: item.name,
            ListId: item.listId,
            WebId: item.webId,
            UniqueId: item.uniqueId,
            ListItemId: item.listItemId,
            SiteUrl: item.siteUrl,
            SiteId: item.siteId,
            NeedDelete: item.needDelete || false,
        }));

        const params: ISaveRelatedRecord = {
            CurrentInfo: {
                Name: "",
                ListId: this.props.spContext.pageContext.list?.id?.toString() ?? "",
                WebId: this.props.spContext.pageContext.web?.id?.toString() ?? "",
                UniqueId: removeCurlyBraces(this.props.event.selectedRows[0].getValueByName("UniqueId")),
                ListItemId: Number(this.props.event.selectedRows[0].getValueByName("ID")),
                SiteUrl: this.props.spContext.pageContext.site?.absoluteUrl?.toString() ?? "",
                SiteId: this.props.spContext.pageContext.site?.id?.toString() ?? "",
                NeedDelete: false,
            },
            RelatedInfos: relatedInfos,
        };

        const isEqual = checkRelatedRecordsEqual(relatedInfos, this.state.existedRelatedRecords);

        this.setState({ isSaving: true });

        if (isEqual) {
            await this.onSubmitRelatedItems(params);
            return;
        }

        // Check permission of the file outside related panel
        const hasPermissionOfOutsideFile = await this.onCheckPermissionOfFile(params.CurrentInfo.SiteUrl, params.CurrentInfo.ListId, params.CurrentInfo.ListItemId);

        if (!hasPermissionOfOutsideFile) {
            this.setState({ isSaving: false });
            this.onError(true, strings.Related_App_EditRelatedRecord_Error_Msg);
            return;
        }

        let permission = false;

        // Check permission for per file in related table
        for (let i = 0; i < relatedInfos.length; i++) {
            const hasPermission = await this.onCheckPermissionOfFile(relatedInfos[i].SiteUrl, relatedInfos[i].ListId, relatedInfos[i].ListItemId);
            permission = hasPermission;

            if (!hasPermission) {
                this.setState({ isSaving: false });
                this.onError(true, `${strings.Related_App_EditRelatedRecord_File_Error_Msg} ${relatedInfos[i].Name}`);
                break;
            }
        }

        if (permission) {
            await this.onSubmitRelatedItems(params);
        }
    }

    onAddRelated = () => {
        const searchItemSelection = this.searchItemSelection.getSelection();

        this.setState((prev) => ({
            items: [...prev.items, ...(searchItemSelection as IRelatedRecordItem[])],
        }), () => {
            const searchItemSelectionMap = new Map(searchItemSelection.map((item) => [removeCurlyBraces(item.key as string), true]));
            this.setState((prev) => ({ deleteItems: prev.deleteItems.filter((item) => !searchItemSelectionMap.has(item.key)) }));
        });
        this.onDismissCallout();
    }

    onRenderSearchResultsFooter = () => {
        if (this.state.searchItems.length && this.state.searchItems[0].key !== ResultState.IS_SEARCHING && this.state.searchItems[0].key !== ResultState.EMPTY) {
            return (
                <div className='related-records-search-results-footer'>
                    <DefaultButton
                        onClick={this.onDismissCallout}
                    >
                        {strings.Related_App_Common_Cancel}
                    </DefaultButton>
                    <PrimaryButton
                        onClick={this.onAddRelated}
                    >
                        {strings.Related_App_AddRelatedBtn}
                    </PrimaryButton>
                </div>
            );
        }

        return null;
    }

    onRenderFooter = () => {
        return (
            <div className='related-records-panel-footer'>
                <PrimaryButton
                    onClick={this.onSaveRelatedRecords}
                >
                    {strings.Related_App_Common_Save}
                </PrimaryButton>
                <DefaultButton
                    onClick={this.onDismissPanel}
                >
                    {strings.Related_App_Common_Cancel}
                </DefaultButton>
            </div>
        );
    }

    onRenderPanelHeader = (isDetail: boolean) => {
        return (
            <>
                {isDetail ? (
                    <div className='related-records-panel-header'>
                        <IconButton
                            iconProps={{ iconName: "ChromeBack" }}
                            style={{ height: 24, color: "#323130" }}
                            title="Back"
                            tabIndex={0}
                            ariaLabel="Back"
                            onClick={() => this.setState({ payloadData: null, showDetailPanel: false })}
                        />
                        <span tabIndex={0} aria-label={strings.Related_App_RelatedRecords_ViewDetails}>{strings.Related_App_RelatedRecords_ViewDetails}</span>
                    </div>
                ) : (
                    <div className='related-records-panel-header'>
                        <span tabIndex={0} aria-label={strings.Related_App_RelatedRecords}>{strings.Related_App_RelatedRecords}</span>
                    </div>
                )}
                <div className="related-records-panel-header-separate"></div>
            </>
        );
    }

    onRenderDetailPanel = () => {
        return (
            <Panel
                isOpen={this.state.showDetailPanel}
                type={PanelType.medium}
                hasCloseButton={false}
                onRenderHeader={() => this.onRenderPanelHeader(true)}
            >
                <RelatedDetail context={this.props.spContext} payloadData={this.state.payloadData} classNames={classNames} />
            </Panel>
        );
    }

    public render(): React.ReactElement<IRelatedPanelProps> {
        return (
            <div>
                <Panel
                    isOpen={this.state.showPanel}
                    type={PanelType.medium}
                    onRenderHeader={() => this.onRenderPanelHeader(false)}
                    onDismiss={this.onDismissPanel}
                    onRenderFooter={this.onRenderFooter}
                    isFooterAtBottom
                >
                    <div className='related-records-panel-wrapper'>
                        <div className='related-records-panel-search-wrapper'>
                            <strong style={{ fontWeight: 600 }}>{strings.Related_App_AddRelatedTitle}</strong>
                            <div ref={this.searchBoxRef}>
                                <SearchBox
                                    placeholder={strings.Related_App_AddRelated_SearchBox_Placeholder}
                                    disableAnimation
                                    showIcon
                                    className='related-records-panel-searchbox'
                                    styles={{ root: { borderColor: "#ddd !important", } }}
                                    iconProps={{ iconName: "Search", style: { color: "#323130" } }} // transform: "rotate(90deg)"
                                    value={this.state.searchValue}
                                    onChange={(_, newValue) => this.onChangeSearchValue(newValue || '')}
                                    onSearch={this.handleSearch}
                                    onClear={() => this.setState({ isCalloutVisible: false })}
                                />
                            </div>
                            {this.state.isCalloutVisible && this.searchBoxRef?.current && (
                                <Callout
                                    target={this.searchBoxRef.current}
                                    onDismiss={this.onDismissCallout}
                                    setInitialFocus
                                    isBeakVisible={false}
                                    calloutWidth={this.searchBoxRef.current.clientWidth}
                                >
                                    <LazyLoadTable
                                        componentRef={this.componentRef}
                                        isSearching={this.state.isSearching}
                                        items={this.state.searchItems}
                                        currentPage={this.state.currentPage}
                                        totalPage={this.state.totalPage}
                                        onScroll={this.onScrollSearchResult}
                                        columns={columns4Drop}
                                        selection={this.searchItemSelection}
                                        selectionMode={SelectionMode.multiple}
                                        onRenderDetailsHeader={() => null}
                                        onRenderDetailsFooter={() => null}
                                        onRenderFooter={this.onRenderSearchResultsFooter}
                                    />
                                </Callout>
                            )}
                            {this.state.isSaving && (
                                <>
                                    <div className='related-records-panel-overlay'></div>
                                    <div className='related-records-panel-loading'>
                                        <Spinner size={SpinnerSize.medium} style={{ marginRight: 0 }} className={classNames.searchResultLoading} />
                                    </div>
                                </>
                            )}
                        </div>
                        {this.state.error.isShow && (
                            <MessageBar
                                messageBarType={MessageBarType.error}
                                styles={{ root: { margin: "8px 0" }, innerText: { fontSize: 14 } }}
                                onDismiss={() => this.onError(false, "")}
                                dismissButtonAriaLabel="Close"
                            >
                                {this.state.error.message}
                            </MessageBar>
                        )}
                        <DetailsList
                            items={this.state.items}
                            columns={this.state.columns}
                            selectionMode={SelectionMode.none}
                            // selection={this.itemSelection}
                        />
                    </div>
                </Panel>
                {this.state.showDetailPanel && this.onRenderDetailPanel()}
            </div>
        );
    }
}