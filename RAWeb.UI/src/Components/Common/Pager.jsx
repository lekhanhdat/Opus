import {bindEvents} from '../../Utilities/CommonUtil';
import PropTypes from 'prop-types';

class Pager extends React.Component {
    constructor(props) {
        super(props);
        this.componentMounted = false;
        this.prevIndex = parseInt(props.pagerIndex, 10) + 1;
        this.state = {
            pagerIndex: this.prevIndex,
            pagerCount: Math.ceil(props.itemsCount / props.pagerSize)
        };
        this.pagerSize = {key: props.pagerSize, value: props.pagerSize};
        if (props.showPagerSize) {
            this.pagerSizeOptions = props.pagerSizeOptions
                .map((value) => {
                    return {key: value, value: value, checked: props.pagerSize == value};
                });
        }
        this.initBinding();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        this.prevIndex = this.state.pagerIndex;
        let newPageIdx = parseInt(nextProps.pagerIndex, 10) + 1;
        if (this.state.pagerIndex != newPageIdx) {
            this.setState({pagerIndex: newPageIdx});
        }
        if (this.state.itemsCount != nextProps.itemsCount) {
            let pCount = Math.ceil(nextProps.itemsCount / this.pagerSize.value);
            if (pCount == 0) {
                this.setState({
                    pagerCount: 1,
                    pagerIndex: 1
                });
            } else {
                this.setState({
                    pagerCount: pCount,
                    pagerIndex: newPageIdx > pCount ? pCount : newPageIdx
                });
            }
        }
    }

    componentDidMount() {
        this.componentMounted = true;
    }

    initBinding() {
        bindEvents(this, "onChange", "onKeyUp", "onBlur", "onBtnKeyUp",
            "onPagerSizeChanged", "prev", "next", "first", "last");
    }

    onPagerSizeChanged(args) {
        if (!this.componentMounted) return;
        let oldSize = args.oldValue;
        let newSize = args.newValue;
        if (this.props.onChange) {
            this.props.onChange(0, newSize.value, (isSuccess) => {
                if (isSuccess) {
                    this.pagerSize = newSize;
                    this.setState({
                        pagerIndex: 1,
                        pagerCount: Math.ceil(this.props.itemsCount / this.pagerSize.value)
                    });
                } else {
                    this.pagerSize = oldSize;
                }
            });
        } else {
            this.pagerSize = oldSize;
        }
    }

    onChange(e) {
        this.setState({pagerIndex: e.target.value});
    }

    onKeyUp(e) {
        if (e.keyCode === 13) {
            if (this.validPagerIndex(e.target.value) && e.target.value != this.prevIndex) {
                this.onBlur(e);
            } else {
                this.setState({pagerIndex: this.prevIndex});
                e.target.focus();
            }
        }
    }

    onBlur(e) {
        if (this.validPagerIndex(e.target.value) && e.target.value != this.prevIndex) {
            this.pagerChanged(parseInt(this.state.pagerIndex, 10));
        } else {
            this.setState({pagerIndex: this.prevIndex});
        }
    }

    prev() {
        this.gotoPage(this.state.pagerIndex - 1);
    }

    next() {
        this.gotoPage(this.state.pagerIndex * 1 + 1);
    }

    first() {
        this.gotoPage(1);
    }

    last() {
        this.gotoPage(this.state.pagerCount);
    }

    onBtnKeyUp(e) {
        if (e.keyCode === 13) {
            e.target.click();
        }
    }

    gotoPage(index) {
        if (this.validPagerIndex(index)) {
            this.setState({pagerIndex: index}, () => {
                this.pagerChanged(index);
            });
        } else {
            this.setState({pagerIndex: this.prevIndex});
        }
    }

    validPagerIndex(val) {
        val = Number(val);
        if (isNaN(val) || val < 1 || val > this.state.pagerCount) {
            return false;
        } else {
            return true;
        }
    }

    pagerChanged(pIndex) {
        let self = this;
        if (this.props.onChange) {
            this.props.onChange(pIndex - 1, this.pagerSize.value, (isSuccess) => {
                if (isSuccess) {
                    self.prevIndex = pIndex;
                } else {
                    self.setState({pagerIndex: self.prevIndex});
                }
            });
        }
    }

    isFirstPage() {
        return this.state.pagerIndex == 1;
    }

    isLastPage() {
        return this.state.pagerIndex == this.state.pagerCount;
    }

    getStartToEndText() {
        let start = 0;
        let end = 0;
        if (!(this.props.pagerIndex == 0 && this.props.itemsCount == 0)) {
            start = this.props.pagerIndex * this.props.pagerSize + 1;
            if (this.state.pagerCount - 1 > this.props.pagerIndex) {
                end = start + this.props.pagerSize * 1 - 1;
            } else {
                end = this.props.itemsCount;
            }

        }
        end = end || 0;
        if(end ==0 )
        {
            start = 0;
        }
        return `${start}-${end}`;
    }

    render() {
        let className = this.props.className ? "ra-pager " + this.props.className : "ra-pager";
        if (this.props.showPagerSize) {
            className += " ra-pager-actions";
        }
        let startToEndText = this.getStartToEndText();
        let itemsCount = this.props.itemsCount || 0; 
        return <div className='ra-pager-wrapper flex justify-between align-center flex-wrap'>
            {
                this.props.showPagerCounter && <div className="ra-pager-section ra-main-pager-counter">
                    {RMResx.RM_Common_EachPageCounter.format(startToEndText, itemsCount)}
                </div>
            }

            <div className={className}>
                {this.props.showPagerSize &&
                <div className="ra-pager-section ra-pager-size">
                    <div className="ra-pager-section margin-right-s" tabIndex='0'>
                        {RMResx.RM_Common_ShowRows}
                    </div>
                    <R.Combobox
                        width="60px"
                        height="20px"
                        compact
                        searchable={false}
                        textField='value'
                        valueField='key'
                        checkedField='checked'
                        items={this.pagerSizeOptions}
                        onChange={this.onPagerSizeChanged}
                    />
                </div>
                }
                <button className="ra-pager-section ra-pager-section-first fia-pager-first" role="button"
                    disabled={this.isFirstPage()} tabIndex={this.isFirstPage() ? -1 : 0}
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheFirstPage}
                    onClick={this.first} onKeyUp={this.onBtnKeyUp}>
                </button>
                <button className="ra-pager-section ra-pager-section-previous fia-pager-previous" role="button"
                    disabled={this.isFirstPage()} tabIndex={this.isFirstPage() ? -1 : 0}
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToThePreviousPage}
                    onClick={this.prev} onKeyUp={this.onBtnKeyUp}>
                </button>
                <div className="ra-pager-section">{RMResx.RM_JS_Common_AUI_Pager_Page}</div>
                <input type="text" className="ra-pager-section ra-pager-section-index"
                    aria-label={RMResx.RM_JS_Common_AUI_Pager_Page + this.state.pagerIndex + RMResx.RM_JS_Common_AUI_Pager_Of + this.state.pagerCount}
                    value={this.state.pagerIndex} onChange={this.onChange} onBlur={this.onBlur}
                    onKeyUp={this.onKeyUp}/>
                <div
                    className="ra-pager-section ra-pager-section-count">{RMResx.RM_JS_Common_AUI_Pager_Of + (this.state.pagerCount || 1)}</div>
                <button className="ra-pager-section ra-pager-section-next fia-pager-next" role="button"
                    disabled={this.isLastPage() || !this.state.pagerCount} tabIndex={this.isLastPage() ? -1 : 0}
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheNextPage}
                    onClick={this.next} onKeyUp={this.onBtnKeyUp}>
                </button>
                <button className="ra-pager-section ra-pager-section-last fia-pager-last" role="button"
                    disabled={this.isLastPage() || !this.state.pagerCount} tabIndex={this.isLastPage() ? -1 : 0}
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheLastPage}
                    onClick={this.last} onKeyUp={this.onBtnKeyUp}>
                </button>
            </div>
        </div>;
    }
}


class SimplePager extends React.Component {
    constructor(props) {
        super(props);
        if (props.showPagerSize) {
            this.pagerSizeOptions = props.pagerSizeOptions
                .map((value) => {
                    return {key: value, value: value, checked: props.pagerSize == value};
                });
        }
        this.initBinding();
    }

    initBinding() {
        bindEvents(this, "onBtnKeyUp", "prev", "next", "first");
    }

    getPagerText() {
        let start = 0;
        let end = 0;
        if (!(this.props.pagerIndex == 0 && this.props.shownCount == 0)) {
            start = this.props.pagerIndex * this.props.pagerSize + 1;
            end = start + (this.props.hasNext ? this.props.pagerSize : this.props.shownCount) - 1;
        }
        return `${start}-${end}`;
    }

    gotoPage(index) {
        this.props.onChange(index, this.props.pagerSize);
    }

    isFirstPage() {
        return this.props.pagerIndex == 0;
    }

    isLastPage() {
        return !this.props.hasNext;
    }

    prev() {
        if (!this.isFirstPage()) {
            this.gotoPage(this.props.pagerIndex - 1);
        }
    }

    next() {
        if (!this.isLastPage()) {
            this.gotoPage(this.props.pagerIndex + 1);
        }
    }

    first() {
        if (!this.isFirstPage()) {
            this.gotoPage(0);
        }
    }

    onPagerSizeChanged = (args) =>{
        this.props.onChange(0, args.newValue.value);
    }

    onBtnKeyUp(e) {
        if (e.keyCode === 13) {
            e.target.click();
        }
    }

    render() {
        let className = this.props.className ? "ra-pager " + this.props.className : "ra-pager";
        return <React.Fragment>
            {
                this.props.showPagerCounter && <div className="ra-pager-section ra-main-pager-counter">
                    {RMResx.RM_Common_EachPageCounter.format(this.getPagerText(), this.props.totalCount)}
                </div>
            }
            <div className={className}>
                {this.props.showPagerSize &&
                    <div className="ra-pager-section ra-pager-simple-size">
                        <div className="ra-pager-section" tabIndex='0'>
                            {RMResx.RM_Common_ShowRows}
                        </div>
                        <R.Combobox
                            width="60px"
                            compact
                            height={20}
                            searchable={false}
                            textField='value'
                            valueField='key'
                            checkedField='checked'
                            items={this.pagerSizeOptions}
                            onChange={this.onPagerSizeChanged}
                        />
                    </div>
                }
                <button className="ra-pager-section ra-pager-section-first fia-pager-first" role="button"
                    disabled={this.isFirstPage()} tabIndex="0"
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheFirstPage}
                    onClick={this.first} onKeyUp={this.onBtnKeyUp}>
                </button>
                <button className="ra-pager-section ra-pager-section-previous fia-pager-previous" role="button"
                    disabled={this.isFirstPage()} tabIndex="0"
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToThePreviousPage}
                    onClick={this.prev} onKeyUp={this.onBtnKeyUp}>
                </button>
                <div className="ra-pager-section ra-pager-label" tabIndex="0">{this.getPagerText()}</div>
                <button className="ra-pager-section ra-pager-section-next fia-pager-next" role="button"
                    disabled={this.isLastPage()} tabIndex="0"
                    data-tooltip aria-label={RMResx.RM_JS_Common_GoToTheNextPage}
                    onClick={this.next} onKeyUp={this.onBtnKeyUp}>
                </button>
            </div>
        </React.Fragment>;
    }
}

SimplePager.propTypes = {
    pagerIndex: PropTypes.number,
    pagerSize: PropTypes.number,
    shownCount: PropTypes.number,
    hasNext: PropTypes.bool,
    onChange: PropTypes.func
};


export {Pager, SimplePager};
