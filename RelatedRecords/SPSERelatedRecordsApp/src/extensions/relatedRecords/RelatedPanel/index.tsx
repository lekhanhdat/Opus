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
import * as React from "react";
import {
  IListViewCommandSetExecuteEventParameters,
  ListViewCommandSetContext,
} from "@microsoft/sp-listview-extensibility";
import {
  Callout,
  css,
  DefaultButton,
  DetailsList,
  Dropdown,
  IColumn,
  IconButton,
  IDetailsList,
  IDropdownOption,
  ISelection,
  MessageBar,
  MessageBarType,
  Panel,
  PanelType,
  PrimaryButton,
  SearchBox,
  Selection,
  SelectionMode,
  Spinner,
  SpinnerSize,
  TooltipHost,
  TooltipOverflowMode,
} from "office-ui-fabric-react";

import * as strings from "RelatedRecordsCommandSetStrings";

import "./index.css";
import LazyLoadTable from "../components/LazyLoadTable";
import {
  IRelatedInfo,
  IRelatedRecordItem,
  ISaveRelatedRecord,
} from "../types/relatedRecord";
import { EResultState } from "../constants/searchResult";
import {
  checkRelatedRecordsEqual,
  getFileIcon,
  getPhysicalIcons,
  getRelatedRecordsFromXmlData,
  removeCurlyBraces,
} from "../common/RelatedRecordsUtil";
import classNames from "./styleSets";
import { ENodeType, SearchScopeKey } from "../constants/nodeType";
import { FarmSolutionService } from "../../../services/FarmSolutionService";
import { ESourceFlag } from "../constants/sourceFlags";
import { EMessageType } from "../constants/relatedRecord";
import { ReplaceConfigs } from "../../../config/AppConfigs";

export interface IRelatedPanelProps {
  sourceItemId: number;
  sourceListId: string;
  sourceWebUrl: string;
  uniqueId: string;
  event: IListViewCommandSetExecuteEventParameters;
  spContext: ListViewCommandSetContext;
  isOpen: boolean;
  onDismiss: () => void;
}

export interface IRelatedPanelState {
  isCalloutVisible: boolean;
  items: IRelatedRecordItem[];
  selectedKeys: string[];
  searchValue: string;
  searchItems: IRelatedRecordItem[];
  isSearching: boolean;
  currentPage: number;
  totalPage: number;
  deleteItems: IRelatedRecordItem[];
  columns: IColumn[];
  isSaving: boolean;
  error: {
    isShow: boolean;
    message: string;
  };
  existedRelatedRecords: IRelatedRecordItem[];
  selectedSearchKeys: Set<string>;
  sourceOptionKey?: number | string;
  tokenIndex: string;
}

const guidEmpty = "00000000-0000-0000-0000-000000000000";

const columns4Drop: IColumn[] = [
  {
    key: "key",
    name: "Name",
    fieldName: "name",
    minWidth: 100,
    maxWidth: 150,
    isRowHeader: false,
    isResizable: true,
    onRender: (item) => {
      if (item.key === EResultState.IS_SEARCHING) {
        return (
          <Spinner
            size={SpinnerSize.medium}
            className={classNames.searchResultLoading}
          />
        );
      }

      if (item.key === EResultState.EMPTY) {
        return (
          <div
            className={`${classNames.searchResultLoading} ${classNames.searchResultEmpty}`}
          >
            {strings.Related_App_AddRelated_SearchResult_Empty}
          </div>
        );
      }

      const isSelectPhy = item.sourceFlag === ESourceFlag.Physical

      const secondText = item.documentId ? `${item.path} (${strings.Related_App_Detail_Id}: ${item.documentId})` : item.path;
      const iconUrl = isSelectPhy ? getPhysicalIcons(item.nodeType) : getFileIcon(item.iconExtension).url

      return (
        <div className="related-records-panel-search-result-col">
          <div style={{ width: 20 }}>
            <img src={iconUrl} alt="icon" />
          </div>
          <div className="related-records-panel-search-result-col-info">
            <TooltipHost
              id={item.key}
              overflowMode={TooltipOverflowMode.Self}
              hostClassName={css(classNames.itemText)}
              content={item.name}
            >
              <strong className="related-records-detail-panel-section-info_value">
                {item.name}
              </strong>
            </TooltipHost>
            <TooltipHost
              id={item.key}
              overflowMode={TooltipOverflowMode.Self}
              hostClassName={css(classNames.itemText)}
              content={secondText}
            >
              <i>{secondText}</i>
            </TooltipHost>
          </div>
        </div>
      );
    },
  },
];

class RelatedPanel extends React.Component<
  IRelatedPanelProps,
  IRelatedPanelState
> {
  private _searchBoxElement: HTMLElement | null;
  private _componentRef: IDetailsList;
  private _itemSelection: ISelection;
  private _searchItemSelection: ISelection;
  private _farmSolutionService: FarmSolutionService;

  constructor(props: IRelatedPanelProps) {
    super(props);
    this.state = {
      isCalloutVisible: false,
      items: this.getRelatedRecords(props),
      selectedKeys: [],
      searchValue: '',
      isSearching: false,
      searchItems: [],
      currentPage: 0,
      totalPage: 0,
      deleteItems: [],
      columns: [],
      sourceOptionKey: 1,
      error: {
        isShow: false,
        message: "",
      },
      isSaving: false,
      existedRelatedRecords: this.getRelatedRecords(props).filter(
        (item) => item.sourceFlag === 1
      ),
      selectedSearchKeys: new Set<string>(),
      tokenIndex: ""
    };

    const self = this;

    this._farmSolutionService = new FarmSolutionService(this.props.spContext);

    this._itemSelection = new Selection({
      canSelectItem(item) {
        return false;
      },
      onSelectionChanged: () => {
        const selectedItems = this._itemSelection.getSelection();
        this.setState({
          selectedKeys: selectedItems.map((item) => item.key as string),
        });
      },
    });

    this._searchItemSelection = new Selection({
      getKey: item => item.key ? item.key.toString() : '',
      canSelectItem(item) {
        if (
          item.key === EResultState.IS_SEARCHING ||
          item.key === EResultState.EMPTY
        ) {
          return false;
        }

        return true;
      },
      onSelectionChanged() {
        const selectedItems = self._searchItemSelection ? self._searchItemSelection.getSelection() : [];
        if (selectedItems.length > 0) {
            self.setState({
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                selectedSearchKeys: new Set(selectedItems.map((item: any) => item.key as string))
            });
        }
      },
    });
  }

  componentDidMount(): void {
    const columns: IColumn[] = [
      {
        key: "fileType",
        name: "File Type",
        fieldName: "name",
        iconName: "Page",
        isIconOnly: true,
        iconClassName: classNames.fileIconHeaderIcon,
        minWidth: 20,
        maxWidth: 20,
        className: classNames.iconCell,
        onRender: (item) => (
          <div style={{ marginLeft: -2 }}>
            <img src={item.sourceFlag === ESourceFlag.Physical ? getPhysicalIcons(item.nodeType) : getFileIcon(item.iconExtension).url} alt="icon" />
          </div>
        ),
      },
      {
        key: "name",
        name: strings.Related_App_RelatedColumn_Name,
        fieldName: "name",
        headerClassName: "related-records-panel-list-item-header",
        minWidth: 150,
        maxWidth: 300,
        isRowHeader: true,
        isResizable: true,
        isSorted: true,
        isSortedDescending: false,
        onColumnClick: this.onColumnSort,
        onRender: (item) => {
          return (
            <div className={classNames.itemWrap}>
              <TooltipHost
                id={item.key}
                overflowMode={TooltipOverflowMode.Self}
                hostClassName={css(classNames.itemText)}
                content={item.name}
              >
                <p
                  aria-labelledby={item.name}
                >
                  {item.name}
                </p>
              </TooltipHost>
            </div>
          );
        },
      },
      {
        key: "path",
        name: strings.Related_App_RelatedColumn_Location,
        fieldName: "path",
        headerClassName: "related-records-panel-list-item-header",
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
        key: "action",
        name: strings.Related_App_RelatedColumn_Action,
        fieldName: "action",
        headerClassName: "related-records-panel-list-item-header",
        minWidth: 100,
        isRowHeader: true,
        onRender: (item) => {
          // if (item.sourceFlag !== ESourceFlag.SharePointOnPrem) {
          //   // is Physical folder
          //   return null;
          // }

          return (
            <div
              className={classNames.itemWrap}
              onMouseDown={(e) => e.stopPropagation()}
            >
              <TooltipHost
                id="Delete"
                content={strings.Related_App_Common_Delete}
                aria-label={strings.Related_App_Common_Delete}
              >
                <IconButton
                  className={classNames.actionIcon}
                  iconProps={{ iconName: "Delete" }}
                  onClick={() => this.onRemoveRecordItem(item)}
                />
              </TooltipHost>
            </div>
          );
        },
      },
    ];
    this.setState({
      columns,
    });
  }

  private getSourceOptions = () => {
    if(ReplaceConfigs.ClientId.indexOf("clientId") === 0){
      return [
        { key: SearchScopeKey.sharepointOnprem, text: strings.Related_App_Detail_Onpremises }
      ]
    }
    return [
      { key: SearchScopeKey.physical, text: strings.Related_App_Detail_PhysicalRecords },
      { key: SearchScopeKey.sharepointOnprem, text: strings.Related_App_Detail_Onpremises }
    ]
  }

  private handleChangeSourceOption = (item: IDropdownOption): void => {
    this.setState({
      sourceOptionKey: item.key,
      searchValue: ""
    });
  }

  private getRelatedRecords = (
    props: IRelatedPanelProps
  ): IRelatedRecordItem[] => {
    const xmlString =
      props.event.selectedRows[0].getValueByName("RecordsRelated");
    const relatedRecordsList = getRelatedRecordsFromXmlData(xmlString);
    const tempItems: IRelatedRecordItem[] = [];

    relatedRecordsList.forEach((item) => {
      let extension = "";
      let name = item.name;
      // let fileIconType = FileIconType.folder;
      if (item.SourceFlag === ESourceFlag.SharePointOnPrem || item.SourceFlag === ESourceFlag.All) {
        const fileType = (item.name as string).lastIndexOf(".");
        name = item.name;
        extension = Number(fileType) !== -1 ? (item.name as string).substring(fileType) : "";
        //5: sharepoint list item
        //6: sharepoint document
        //Maybe need a enum for level
        if (item.level == 5) {
          // fileIconType = FileIconType.listItem;
          extension = "aspx";
        }
      } else {
        name = item.recId;
        extension = "";
        if (item.NodeType == ENodeType.PhyRecord) {
          // fileIconType = EFileIconType.genericFile;
        }
      }

      tempItems.push({
        key: item.id,
        name: name,
        path: item.url,
        // iconOptions: { type: fileIconType, extension: extension, size: 20 } as IFileTypeIconOptions,
        iconExtension: extension,
        listId: item.ListId,
        webId: item.WebId,
        uniqueId: item.id,
        listItemId: item.DocLibRowId,
        sourceFlag: item.SourceFlag,
        siteUrl: item.SiteUrl,
        siteId: item.SiteId,
        nodeType: item.NodeType,
        needDelete: false,
        isRelatedRecord: true
      });
    });

    return tempItems;
  };

  private getEmptyAndLoadingItems = (key: EResultState): IRelatedRecordItem => {
    return {
      key,
      name: key,
      path: key,
      iconExtension: "",
      listId: "",
      webId: "",
      uniqueId: "",
      listItemId: 0,
      sourceFlag: 0,
      siteUrl: "",
      siteId: "",
      needDelete: false,
      isRelatedRecord: false,
    };
  };

  private onSearch = async (newValue: string): Promise<IRelatedRecordItem[]> => {
    const {currentPage, tokenIndex} = this.state;
    const tempItems: IRelatedRecordItem[] = []; 
    const { Datas, TotalPage, TokenIndex } = await this._farmSolutionService.searchRelatedRecords( // update response later
      newValue,
      currentPage,
      this.state.sourceOptionKey as number,
      tokenIndex
    );

    Datas.forEach((row) => {
      tempItems.push({
        key: removeCurlyBraces(row.UniqueId as string),
        name: (row.IsDocument ? row.FileName : row.Title) || "",
        path: row.Path || "",
        // iconOptions: { extension: row.FileExtension, size: 20 } as IFileTypeIconOptions,
        iconExtension: row.FileExtension,
        listId: row.ListId || "",
        webId: row.WebId || "",
        uniqueId: removeCurlyBraces(row.UniqueId as string) || "",
        listItemId: Number(row.ListItemID),
        sourceFlag: row.ListId && row.ListId !== guidEmpty ? ESourceFlag.SharePointOnPrem : ESourceFlag.Physical,
        siteUrl: row.SPSiteUrl,
        siteId: row.SiteId || "",
        nodeType: row.NodeType,
        isRelatedRecord: false,
        documentId: row.SourceFlag === ESourceFlag.SharePointOnPrem ? "" : row.DocumentId
      });
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
        removeCurlyBraces(item.key) !==
          removeCurlyBraces(this.props.uniqueId)
    );
    const searchItems = newSearchItems.length
      ? newSearchItems
      : [this.getEmptyAndLoadingItems(EResultState.EMPTY)];
    this.setState({
      totalPage: TotalPage,
      isCalloutVisible: true,
      tokenIndex: TokenIndex
    });
    return searchItems;
  }

  private handleSearch = (newValue?: string): void => {
    if (newValue) {
      this.setState({
        isSearching: true,
        searchItems: [this.getEmptyAndLoadingItems(EResultState.IS_SEARCHING)],
        currentPage: 0,
        tokenIndex: "",
        isCalloutVisible: true,
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
  };

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
        this.getEmptyAndLoadingItems(EResultState.IS_SEARCHING),
      ],
      currentPage: prev.currentPage < prev.totalPage ? prev.currentPage + 1 : prev.currentPage,
    }), () => {
      this.onSearch(this.state.searchValue)
        .then((newSearchItems) => {
          const { searchItems } = this.state;
          const itemsWithoutLoading = searchItems.filter(
            (item) => item.key !== EResultState.IS_SEARCHING
          );
          const updatedNewSearchItems = [...itemsWithoutLoading, ...newSearchItems];

          this._searchItemSelection.setChangeEvents(false, false);
          this._searchItemSelection.setItems(updatedNewSearchItems, false);

          this.setState({
              isSearching: false,
              searchItems: updatedNewSearchItems,
          });


          setTimeout(() => {
            if (this._searchItemSelection) {
              updatedNewSearchItems.forEach((item) => {
                  if (this.state.selectedSearchKeys.has(item.key as string)) {
                      this._searchItemSelection.setKeySelected(item.key, true, false);
                  }
              });
              this._searchItemSelection.setChangeEvents(true, true);
              if (this._componentRef) {
                this._componentRef.forceUpdate();
              }
            }
          }, 0);
        })
        .catch(() => console.error("Search related records for SPOnPremises error!"));
    });
  }

  private onDismissCallout = (): void => {
    this.setState({ isSearching: false, currentPage: 0, isCalloutVisible: false });
  };

  private onAddRelated = (): void => {
    const searchItemSelection = this._searchItemSelection.getSelection();

    this.setState(
      (prev) => ({
        items: [
          ...prev.items,
          ...(searchItemSelection as IRelatedRecordItem[]),
        ],
      }),
      () => {
        const searchItemSelectionMap = new Map<string, boolean>(
          searchItemSelection.map<[string, boolean]>((item) => [
            removeCurlyBraces(item.key as string),
            true,
          ])
        );
        this.setState((prev) => ({
          deleteItems: prev.deleteItems.filter(
            (item) => !searchItemSelectionMap.has(item.key)
          ),
        }));
      }
    );
    this.onDismissCallout();
  };

  private onError = (isShowError: boolean, errorMsg: string): void => {
    this.setState({
      error: {
        isShow: isShowError,
        message: errorMsg,
      },
    });
  };

  private onCopyAndSort<T>(
    items: T[],
    columnKey: string,
    isSortedDescending?: boolean
  ): T[] {
    const key = columnKey as keyof T;
    return items
      .slice(0)
      .sort((a: T, b: T) =>
        (isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1
      );
  }

  private onColumnSort = (
    ev: React.MouseEvent<HTMLElement>,
    column: IColumn
  ): void => {
    const { columns, items } = this.state;
    const newColumns: IColumn[] = columns.slice();
    const currColumn: IColumn = newColumns.filter(
      (currCol) => column.key === currCol.key
    )[0];

    newColumns.forEach((newCol: IColumn) => {
      if (newCol === currColumn) {
        currColumn.isSortedDescending = !currColumn.isSortedDescending;
        currColumn.isSorted = true;
      }
    });

    const newItems = this.onCopyAndSort(
      items,
      currColumn.fieldName!,
      currColumn.isSortedDescending
    );

    this.setState({
      columns: newColumns,
      items: newItems,
    });
  };

  private onRemoveRecordItem = (item: any): void => {
    const newItems = this.state.items.filter((row) => row.key !== item.key);
    const map = new Map<string, boolean>();

    this.setState((prev) => ({
      items: newItems,
      deleteItems: [
        ...prev.deleteItems,
        {
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
        },
      ].filter((item) => {
        if (!map.has(item.key)) {
          map.set(item.key, true);
          return true;
        }

        return false;
      }),
    }));
  };

  private onDismissPanel = (): void => {
    this.props.onDismiss();
  };

  private onCheckPermissionOfFile = async (
    itemInfo: IRelatedInfo
  ): Promise<boolean> => {
    return await this._farmSolutionService.checkItemHasEditPermission(
      itemInfo.SiteUrl,
      itemInfo.WebId,
      itemInfo.ListId,
      itemInfo.ListItemId
    );
  };

  onSubmitRelatedItems = async (params: ISaveRelatedRecord) => {
    try {
      const res = await this._farmSolutionService.saveRelatedRecords(
        params
      );
      if (res.MessageType === EMessageType.Failed) {
          this.onError(true, res.ErrorMessage);
      } else {
          this.onDismissPanel();
          window.location.reload();
      }
    } catch (error) {
      this.onError(true, error.message || error);
      console.error("Save error: ", error);
    } finally {
      this.setState({ isSaving: false });
    }
  }

  private checkIsRecordFromOpus = () => {
    const params: any = {
        ListId:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.list
            ? this.props.spContext.pageContext.list.id.toString()
            : "",
        WebId:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.web
            ? this.props.spContext.pageContext.web.id.toString()
            : "",
        ListItemId: Number(
          this.props.event.selectedRows[0].getValueByName("ID")
        ),
        SiteUrl:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.site
            ? this.props.spContext.pageContext.site.absoluteUrl.toString()
            : "",
      } 

      return this._farmSolutionService.checkIsRecordFromOpus(params)
  }

  private onSaveRelatedRecords = async () => {
    this.setState({ isSaving: true });
    let checkIsRecordFromOpus: any = await this.checkIsRecordFromOpus();
    if(!checkIsRecordFromOpus.success){
      this.onError(true, strings.Related_App_AddRecord_Failed_Msg);
      this.setState({ isSaving: false });
      return;
    }

    const addRelatedRecords: IRelatedRecordItem[] = this.state.items;
    const deleteRelatedRecords: IRelatedRecordItem[] =
      this.state.deleteItems.filter(
        (item) => item.isRelatedRecord
      );

    // Remove duplicate data when delete real file 2, then search and add file 2 again (local temp) and click save (mean no change)
    const relatedInfos: IRelatedInfo[] = [
      ...addRelatedRecords,
      ...deleteRelatedRecords,
    ]
      .filter((item, index, self) => {
        const isDuplicate = self.some(
          (otherItem, otherIndex) =>
            otherIndex !== index && otherItem.key === item.key
        );
        return !isDuplicate;
      })
      .map((item) => ({
        Name: item.name,
        ListId: item.listId || guidEmpty,
        WebId: item.webId || guidEmpty,
        UniqueId: item.uniqueId,
        ListItemId: item.listItemId || 0,
        SiteUrl: item.siteUrl || "",
        SiteId: item.siteId || guidEmpty,
        NeedDelete: item.needDelete || false,
      }));

    const params: ISaveRelatedRecord = {
      CurrentInfo: {
        Name: "",
        ListId:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.list
            ? this.props.spContext.pageContext.list.id.toString()
            : "",
        WebId:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.web
            ? this.props.spContext.pageContext.web.id.toString()
            : "",
        UniqueId: removeCurlyBraces(this.props.uniqueId),
        ListItemId: Number(
          this.props.event.selectedRows[0].getValueByName("ID")
        ),
        SiteUrl:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.site
            ? this.props.spContext.pageContext.site.absoluteUrl.toString()
            : "",
        SiteId:
          this.props.spContext &&
          this.props.spContext.pageContext &&
          this.props.spContext.pageContext.site
            ? this.props.spContext.pageContext.site.id.toString()
            : "",
        NeedDelete: false,
        RecordId: checkIsRecordFromOpus.data.Record.Id
      },
      RelatedInfos: relatedInfos,
    };

    const isEqual = checkRelatedRecordsEqual(
      relatedInfos,
      this.state.existedRelatedRecords
    );


    if (isEqual) {
      await this.onSubmitRelatedItems(params);
      return;
    }

    // Check permission of the file outside related panel
      const hasPermissionOfOutsideFile = await this.onCheckPermissionOfFile(
        params.CurrentInfo
      );

      if (!hasPermissionOfOutsideFile) {
        this.setState({ isSaving: false });
        this.onError(true, strings.Related_App_EditRelatedRecord_Error_Msg);
        return;
      }

      let permission = true;

      // Check permission for per file in related table
      for (let i = 0; i < relatedInfos.length; i++) {
        if(relatedInfos[i].ListId && relatedInfos[i].ListId !== guidEmpty){
          const hasPermission = await this.onCheckPermissionOfFile(
            relatedInfos[i]
          );
          permission = hasPermission;

          if (!hasPermission) {
            this.setState({ isSaving: false });
            this.onError(
              true,
              `${strings.Related_App_EditRelatedRecord_File_Error_Msg} ${relatedInfos[i].Name}`
            );
            break;
          }
        }
      }

      if (permission) {
        await this.onSubmitRelatedItems(params);
        this.setState({ isSaving: false });
      }
  };

  // Renders
  private onRenderHeader = (): JSX.Element => {
    let content: JSX.Element = (
      <div className="related-records-panel-header">
        <span tabIndex={0} aria-label={strings.Related_App_RelatedRecords}>
          {strings.Related_App_RelatedRecords}
        </span>
        <IconButton
          iconProps={{ iconName: "ChromeClose" }}
          style={{ height: 24, color: "#323130" }}
          title="Close"
          tabIndex={0}
          ariaLabel="Close"
          onClick={this.onDismissPanel}
        />
      </div>
    );

    return (
      <div style={{ position: "relative", paddingTop: 18 }}>
        {content}
        <div className="related-records-panel-header-separate"></div>
      </div>
    );
  };

  private onRenderFooter = (): JSX.Element => {
    return (
      <div className="related-records-panel-footer">
        <PrimaryButton onClick={this.onSaveRelatedRecords}>
          {strings.Related_App_Common_Save}
        </PrimaryButton>
        <DefaultButton onClick={this.props.onDismiss}>
          {strings.Related_App_Common_Cancel}
        </DefaultButton>
      </div>
    );
  };

  render(): JSX.Element | null | false {
    const { isOpen } = this.props;
    const {
      searchValue,
      isSearching,
      searchItems,
      currentPage,
      totalPage,
      isCalloutVisible,
      isSaving,
      error,
      sourceOptionKey
    } = this.state;

    if (!isOpen) {
      return null;
    }

    return (
      <div>
        <Panel
          isOpen={isOpen}
          type={PanelType.medium}
          hasCloseButton={false}
          isFooterAtBottom
          onRenderHeader={() => this.onRenderHeader()}
          onRenderFooter={this.onRenderFooter}
          onDismiss={this.onDismissPanel}
        >
          <div className="related-records-panel-wrapper">
            <div className="related-records-panel-search-wrapper">
              <strong style={{ fontWeight: 600 }}>
                {strings.Related_App_AddRelatedTitle}
              </strong>
              <div className="related-records-panel-search-content">
                <Dropdown
                  selectedKey={sourceOptionKey}
                  onChanged={this.handleChangeSourceOption}
                  options={this.getSourceOptions()}
                  style={{width: 200}}
                />
                <div ref={(searchBox) => (this._searchBoxElement = searchBox)} className="related-records-panel-searchbox">
                  <SearchBox
                    placeholder={
                      sourceOptionKey === SearchScopeKey.sharepointOnprem ? strings.Related_App_AddRelated_SearchBox_Placeholder : strings.Related_App_AddRelated_SearchBox_Placeholder_Physical
                    } 
                    disableAnimation
                    className="related-records-panel-searchbox"
                    value={searchValue}
                    onSearch={this.handleSearch}
                    onChange={this.onChangeSearchValue}
                    onClear={this.onDismissCallout}
                  />
                </div>
              </div>
              {isCalloutVisible && (
                <Callout
                  target={this._searchBoxElement}
                  onDismiss={this.onDismissCallout}
                  setInitialFocus
                  isBeakVisible={false}
                  calloutWidth={this._searchBoxElement.clientWidth}
                >
                  <LazyLoadTable
                    componentRef={(ref) => this._componentRef = ref}
                    isSearching={isSearching}
                    items={searchItems}
                    currentPage={currentPage}
                    totalPage={totalPage}
                    onScroll={this.onScrollSearchResult}
                    onDismissCallout={this.onDismissCallout}
                    onAddRelated={this.onAddRelated}
                    columns={columns4Drop}
                    selection={this._searchItemSelection}
                    selectionMode={SelectionMode.multiple}
                    className="related-records-search-result-wrapper"
                    onRenderDetailsHeader={() => null}
                    onRenderDetailsFooter={() => null}
                  />
                </Callout>
              )}
              {isSaving && (
                <div>
                  <div className="related-records-panel-overlay"></div>
                  <div className="related-records-panel-loading">
                    <Spinner
                      size={SpinnerSize.medium}
                      style={{ marginRight: 0 }}
                      className={classNames.searchResultLoading}
                    />
                  </div>
                </div>
              )}
            </div>
            {error.isShow && (
              <div className="related-records-error-text">
                <MessageBar
                  messageBarType={MessageBarType.error}
                  onDismiss={() => this.onError(false, "")}
                  dismissButtonAriaLabel="Close"
                >
                  {error.message}
                </MessageBar>
              </div>
            )}
            <DetailsList
              items={this.state.items}
              columns={this.state.columns}
              selectionMode={SelectionMode.none}
            />
          </div>
        </Panel>
      </div>
    );
  }
}

export default RelatedPanel;
