import PropTypes from 'prop-types';
class Icon extends R.Component {
    constructor(props) {
        super(props);
    
    }

    getLetterList(){
        let litterList = [];
        for(let i = 0; i < 25; i++){
            litterList.push(`-${String.fromCharCode(98+i)}`);
        }
        return litterList;
    }

    getIconPaths(iconClass){
        let letterList = this.getLetterList();
        let iconClassWithoutBlank = $.trim(iconClass);
        let iconPath = [];
        if(iconClass){
            let lastTwoWOrds = iconClassWithoutBlank.substr(iconClassWithoutBlank.length - 2);
            let wordIndex = letterList.findIndex(item => item == lastTwoWOrds);
            if(wordIndex != -1){
                let pathNum = wordIndex + 2;
                for(var i = 1; i <= pathNum; i++){ iconPath.push(`path${i}`);}
            }else{
                return [];
            }
        }
        return iconPath;
    }

    render() {
        let iconPaths = this.getIconPaths(this.props.className);
        return <div className={this.props.className} aria-hidden="true">
            {
                iconPaths.map((item,key)=>{
                    return <span key={key} className={item}></span>;
                })
            }
        </div>;
    }
}

Icon.propTypes = {
    className: PropTypes.string
};
Icon.defaultProps = {
    className: "",
};

export{Icon};