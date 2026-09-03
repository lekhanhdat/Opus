import { Component } from "react";
import { bindEvents } from "../../Utilities/CommonUtil";
import * as Constants from "./Constants";

import "../../Less/PRM/TemplateManagement.less";

const DefaultSuiteUniqueIds = ["6feecea2-2076-4557-ae9c-a90f9eb91617", "c7a9a849-c9a3-4c0b-ba38-ba0db43af048"];
const DefaultTemplateUniqueIds = ["f0b53a20-d955-476b-bb83-41488cfb2750", "b775e3c7-20a8-4141-98fc-49824a028331", "01bd2c27-d4d5-4714-8ef3-e460323a977b"];
export class TemplateCardView extends Component {
    constructor(props){
        super(props);
        this.state = {
            curType: this.props.type,
            suiteItem: this.props.suiteItem
        };
        bindEvents(this, "handleCreateBoxTemplate", "handleCreateFolderTemplate", "browseTemplate", "onClickFolderOrRecordMenu");   
    }

    handleCreateBoxTemplate(){
        this.props.handleCreateBoxTemplate(this.state.suiteItem.SuiteUniqueId);
    }

    handleCreateFolderTemplate(){
        this.props.handleCreateFolderTemplate(this.state.suiteItem.SuiteUniqueId);
    }

    onClickSuiteMenu(item){
        this.props.onClickSuiteMenu(item, this.state.suiteItem.SuiteUniqueId);
    }

    onClickBoxOrFolderMenu(item){
        this.props.onClickBoxOrFolderMenu(item, this.state.suiteItem);
    }

    onClickFolderOrRecordMenu(item){
        this.props.onClickFolderOrRecordMenu(item, this.state.suiteItem.TemplateUniqueId);
    }

    browseTemplate(e){
        if($(e.target).closest(".aui-popover").length > 0 || $(e.target).closest(".aui-buttongroup").length > 0
        ||$(e.target).closest(".aui-buttongroup-inner").length > 0
        ){
            return false;
        }
        if(this.state.suiteItem.ViewDataLevel != Constants.ViewDataLevel.Record){
            this.props.browseTemplate(this.state.suiteItem);
        }
    }

    getTemplateActionMenuItems(type){
        let items = [];
        switch(type){
            case Constants.TemplateTypes.Box:
                items = [
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_EditBox, index: 1, isRemoved: false},
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_DeleteBox, index: 2, isRemoved: false},
                ]; 
                break;
            case Constants.TemplateTypes.Folder:
                items = [
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_EditFolder, index: 1, isRemoved: false},
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_DeleteFolder, index: 2, isRemoved: false},
                ];
                break;
            case Constants.TemplateTypes.Records:
                items = [
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_EditRecord, index: 1, isRemoved: false},
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_DeleteRecord, index: 2, isRemoved: false},
                ];
                break;
            default:
                items = [
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_EditSuite, index: 1, isRemoved: false},
                    {displayName: RMResx.RM_PRM_TM_MenuBtn_DeleteSuite, index: 2, isRemoved: false},
                ];
                break;
        }

        if(this.isDefaultSuiteOrTemplate()){
            items.splice(1);
        }
        return items;
    }

    isDefaultSuiteOrTemplate(){
        if(DefaultTemplateUniqueIds.indexOf(this.state.suiteItem.TemplateUniqueId) > -1 || DefaultSuiteUniqueIds.indexOf(this.state.suiteItem.SuiteUniqueId) > -1){
            return true;
        }
        return false;
    }

    wrapperI18N(str){
        return RMResx[str]? RMResx[str]: str;
    }

    renderTemplate(){
        switch(this.state.curType){
            case Constants.CardType.EmptyBoxSuite:
            case Constants.CardType.EmptyFolderSuite:
                return this.renderEmptySuiteTemplate();
            case Constants.CardType.Folder:
            case Constants.CardType.Record:
                return this.renderFolderOrRecordTemplate();
            default:
                return this.renderSuiteAndRootTemplate();
        }
    }

    renderEmptySuiteTemplate(){
        let linkNewBox = <div>
            <a className="ra-link-a" onClick={this.handleCreateBoxTemplate.bind(this)}>{RMResx.RM_PRM_TM_Link_NewBoxTemplateForSuite}</a>
        </div>;
        let linkNewFolder = <a onClick={this.handleCreateFolderTemplate.bind(this)}>{RMResx.RM_PRM_TM_Link_NewFolderTemplateForSuite}</a>;
        return <div className="template-card-main">
            {/* <div className="template-card-header">
                
            </div> */}
            <div className="template-card-body-empty">
                <div className="temp-info-main-empty bn">
                    {this.state.curType == Constants.CardType.EmptyBoxSuite && linkNewBox}
                    {this.state.curType == Constants.CardType.EmptyFolderSuite && linkNewFolder}
                </div>
            </div>
            <div className="template-card-footer">
                {this.renderSuiteFooter()}
            </div>
        </div>;
    }

    renderSuiteAndRootTemplate(){
        let isStartFromBox = this.state.suiteItem.StartFromType == Constants.StartFromType.Box,
            menuItems = this.getTemplateActionMenuItems(isStartFromBox? Constants.TemplateTypes.Box: Constants.TemplateTypes.Folder),
            templateName = this.wrapperI18N(this.state.suiteItem.TemplateName);
        return <div className="template-card-main" onClick={this.browseTemplate}>
            <div className="template-card-header">
                <div className="info-left">
                    <div className="info-label" data-tooltip aria-label={templateName}>{templateName}</div>
                    {this.state.suiteItem.TemplateDescription && <$g.Popover icon="fia-status-info icon-tip-info" width='340px'>{this.state.suiteItem.TemplateDescription}</$g.Popover>}
                </div>
                <div className="info-right">
                    <R.ButtonGroup type="action" icon="fia-splitter-col" height={200}>
                        {
                            menuItems.map((item, key) => (
                                <R.Button
                                    key={key}
                                    onClick={this.onClickBoxOrFolderMenu.bind(this, item)}
                                    text={item.displayName} />
                            ))
                        }
                    </R.ButtonGroup></div>
            </div>
            <div className="template-card-body">{this.renderChildTemplateCountInfo(isStartFromBox? Constants.TemplateTypes.Box: Constants.TemplateTypes.Folder )}</div>
            <div className="template-card-footer">
                {this.renderSuiteFooter()}
            </div>
        </div>;
    }

    renderFolderOrRecordTemplate(){
        let isFolderTemplate = this.state.suiteItem.ViewDataLevel == Constants.ViewDataLevel.Folder,
            menuItems = this.getTemplateActionMenuItems(isFolderTemplate? Constants.TemplateTypes.Folder: Constants.TemplateTypes.Records),
            templateName = this.wrapperI18N(this.state.suiteItem.TemplateName);

        return <div className={isFolderTemplate? "template-card-main" : "template-card-main template-card-main-record"} onClick={this.browseTemplate}>
            <div className={isFolderTemplate? "template-card-header": "template-card-header template-card-header-record"}>
                <div className="info-left">
                    <div className="info-label" data-tooltip aria-label={templateName}>{templateName}</div>
                    {this.state.suiteItem.TemplateDescription && <$g.Popover icon="fia-status-info icon-tip-info" width='340px'>{this.state.suiteItem.TemplateDescription}</$g.Popover>}
                </div>
                <div className="info-right">
                    <R.ButtonGroup type="action" icon="fia-splitter-col" height={200}>
                        {
                            menuItems.map((item, key) => (
                                <R.Button
                                    key={key}
                                    onClick={this.onClickFolderOrRecordMenu.bind(this, item)}
                                    text={item.displayName} />
                            ))
                        }
                    </R.ButtonGroup></div>
            </div>
            {isFolderTemplate &&<div className="template-card-body">
                {this.renderChildTemplateCountInfo(Constants.TemplateTypes.Records)}
            </div>}
            <div className="template-card-footer temp-info-footer-time">
                {RMResx.RM_DSB_Created} {this.state.suiteItem.CreatedOn}
            </div>
        </div>;
    }

    renderSuiteFooter(){
        let menuItems = this.getTemplateActionMenuItems(Constants.TemplateTypes.None),
            suiteName = this.wrapperI18N(this.state.suiteItem.SuiteName);
        return  <div>
            <div className="info-left">
                <div className="info-label h50" data-tooltip aria-label={suiteName}>{suiteName}</div>
                {this.state.suiteItem.Description && <$g.Popover icon="fia-status-info icon-tip-info" width='340px'>{this.state.suiteItem.Description}</$g.Popover>}
            </div>
            <div className="info-right">
                <R.ButtonGroup type="action" icon="fia-splitter-col" height={200}>
                    {
                        menuItems.map((item, key) => (
                            <R.Button
                                key={key}
                                onClick={this.onClickSuiteMenu.bind(this, item)}
                                text={item.displayName} />
                        ))
                    }
                </R.ButtonGroup></div>
        </div>;
    }

    renderChildTemplateCountInfo(type){
        let folderCountRow = <div className="temp-info-row">
            <div className="temp-info-left" tabIndex="0">{RMResx.RM_PRM_TM_FolderTemplateCountTip}</div>
            <div className="temp-info-right" tabIndex="0">{this.state.suiteItem.FolderTemplateCount}</div>
        </div>;
        let recordCountRow = <div className="temp-info-row  bbn">
            <div className="temp-info-left" tabIndex="0">{RMResx.RM_PRM_TM_RecordTemplateCountTip}</div>
            <div className="temp-info-right" tabIndex="0">{this.state.suiteItem.RecordTemplateCount}</div>
        </div>;

        return <div className="temp-info-main">
            {type == Constants.TemplateTypes.Box && folderCountRow}
            {recordCountRow}
        </div>;
    }

    render(){
        return <React.Fragment>
            <div className="col-xlg-3 col-xs-3">
                <div className="ra-section">
                    {this.renderTemplate()}
                </div>
            </div>
        </React.Fragment>;
    }
}

