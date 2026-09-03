import { Component } from "react";
import { NodeType } from "../../../Constants/DAEnums";
import Tree from './Instances/Physical/PhyDestinationTree';

export default class TreeTest extends Component {
    constructor(props) {
        super(props);

        this.state = {treeData: null};
    }

    onSelectedNodeChanged(selectedItem, treeData) {
        //console.log(selectedItem, treeData);
    }

    saveTreeData = () => {
        let treeData = this.tree.getTreeData();
        window.localStorage.setItem("treeData", JSON.stringify(treeData));
    }

    setTreeData = () => {
        let str = window.localStorage.getItem("treeData");
        let treeData = JSON.parse(str);
        this.setState({treeData: treeData});
    }

    render() {
        return (<div>
            <Tree
                ref={r => this.tree=r}
                treeData={this.state.treeData}
                leafNodeType={NodeType.PhyBox}
                onSelectedNodeChanged={this.onSelectedNodeChanged}
            />
            <button onClick={this.setTreeData}>set tree data</button>
            <br />
            <button onClick={this.saveTreeData}>save tree data</button>
        </div>);
    }
}