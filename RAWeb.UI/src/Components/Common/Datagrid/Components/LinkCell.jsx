import {Component} from 'react';

class LinkCell extends Component {
    constructor(props) {
        super(props);
        this.state = {};
        this.initBinding();
    }

    //初始化bind this
    initBinding() {
        const eventsArr = ['onClick', 'onKeyDown'];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    componentDidMount() {

    }

    onClick() {
        this.props.onClick(this.props.rowData);
    }

    onKeyDown(e) {
        this.props.onKeyDown(this.props.rowData, e);
    }

    render() {
        let content = this.props.rowData[this.props.name];
        return <div>
            <a className="ra-linkcell ra-cursor-pointer"
                tabIndex='0'
                onClick={this.onClick}
                onKeyDown={this.onKeyDown}>
                {content}
            </a>
        </div>;
    }
}

export { LinkCell };