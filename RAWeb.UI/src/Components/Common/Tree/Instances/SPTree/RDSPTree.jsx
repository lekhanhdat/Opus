import { showToast } from "../../../../../Utilities/CommonUtil";
import { Component } from 'react';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import SPNodeContent from '../../NodeContents/CRM/SPNodeContent';

class RDSPTree extends Component {
    constructor(props) {
        super(props);
        
        this.state = {
            items: [],
            errorToast: ""
        };
        let mainComponent = this;
        this.initDataStr = null;
        this.DesignLists = [];
        this.treeCache = [];
        this.siteCollectionUrl = this.props.siteCollectionUrl;
        this.pagerSize = 10;
        this.isSearchTree = false;

        this.treeContext = {
            nodeContentComponent: SPNodeContent,
            singleSelection: true,
            transToTreeNodeObject(oitem) {
                let children = oitem.infos || [];
                return {
                    origin: oitem,
                    nodeKey: oitem.id,
                    nodeType: oitem.Level,
                    isLeafNode: false,
                    disableSelect: oitem.NodeLevel <= NodeLevel.Sites,
                    text: oitem.name,
                    checked: oitem.Checked,
                    loaded: oitem.Loaded,
                    expanded: oitem.NodeLevel == NodeLevel.SiteCollection ? true : oitem.Loaded ,
                    enableContextMenu: false,
                    items: children,
                    itemsCount: oitem.ChildrenCount,
                    hasChildren: true,
                    treeSource: mainComponent.props.treeSource,
                    pagerByServer: true,
                    pagerSize: mainComponent.pagerSize,
                    pagerIndex: !oitem.PageIndex || oitem.PageIndex * mainComponent.pagerSize >= children.length ? 0 : oitem.PageIndex,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Checked = item.checked;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PageIndex = item.pagerIndex + 1;
                oitem.PageSize = item.pagerSize;
            },
            
            onLoadNodes(parentItem, funcSuccess) {
                mainComponent.onLoadTree(parentItem.origin, (items)=>{
                    parentItem.ChildrenCount = items.ChildrenCount;
                    funcSuccess(items.infos, parentItem);
                });
            },

            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
        };
    }

    componentDidMount() {
        this.onLoadTree();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if(nextProps.siteCollectionUrl != this.props.siteCollectionUrl){
            this.siteCollectionUrl = nextProps.siteCollectionUrl;
            this.isSearchTree = true;
            this.onLoadTree();
        }
    }

    getErrorSiteCollectionNode(){
        return {
            name: this.siteCollectionUrl,
            ChildrenCount: 0,
            FolderId: "00000000-0000-0000-0000-000000000000",
            ListId: "00000000-0000-0000-0000-000000000000",
            NodeLevel: 0,
            PageIndex: null,
            PageSize: null,
            ServerRelativeUrl: null,
            WebId: "00000000-0000-0000-0000-000000000000",
            WebUrl: null,
            infos: [],
            Loaded: true
        };
    }

    onLoadTree(parentItem, callback) {
        let option = {
            url: "/api/RelatedRecordsApi/Browser",
            data: {
                WebUrl: this.siteCollectionUrl,
                PageIndex: 0,
                PageSize: this.pagerSize
            }
        };
        if(parentItem){
            option.data = { 
                PageSize: this.pagerSize,
                PageIndex: parentItem.PageIndex,
                WebId: parentItem.WebId,
                NodeLevel: parentItem.NodeLevel,
                ListId: parentItem.ListId,
                FolderId: parentItem.FolderId,
                ServerRelativeUrl: parentItem.ServerRelativeUrl,
                WebUrl: parentItem.WebUrl
            };
        }else{
            $$.loading(true);
        }
        fetchUtility(option).then((res) => {
            let treeItem = JSON.parse(res);
            if(parentItem){
                callback(treeItem);
            }else{
                $$.loading(false);
                treeItem.name = this.siteCollectionUrl;
                treeItem.NodeLevel = NodeLevel.SiteCollection;
                treeItem.Loaded = treeItem.ChildrenCount > 0;
                this.setState({ items: [treeItem]});
                if(this.isSearchTree){
                    this.isSearchTree = false;
                    this.onCloseLoadTreeErrorToast();
                    showToast.success(RMResx.RM_JS_RD_BrowserSuccess);
                }
            }
        }).catch((e) => {
            $$.loading(false);
            this.isSearchTree = false;
            if(!parentItem){
                this.setState({items: [this.getErrorSiteCollectionNode()]});
            }
            this.onCloseLoadTreeErrorToast();
            this.setLoadTreeErrorToast();
        });
    }

    onCloseLoadTreeErrorToast(){
        if(this.state.errorToast){
            this.state.errorToast.close();
        }
    }

    setLoadTreeErrorToast() {
        let value = $$.toast({
            classify: 'error',
            content: RMResx.RM_JS_RD_FailedBrowser,
            timeout: 0
        });
        Object.assign(this.state, { errorToast: value });
    }

    render() {
        return <$g.TreeView
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }
}

export default RDSPTree;