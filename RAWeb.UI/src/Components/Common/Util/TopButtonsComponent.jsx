export default class TopButtonsComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            menuBtnItems : [...this.props.data.menuBtnItems],
        };
    }

    updateButtons(menuBtnItems){
        this.setState({ menuBtnItems: menuBtnItems });
    }
    
    render() {
        let showCount = this.props.showCount ? this.props.showCount : 4;
        let menuBtnItems = [...this.state.menuBtnItems];
        let menuBtnItemsInMore = menuBtnItems.splice(showCount);
        return <div id="crm-menu" className="margin-top-m margin-bottom-m">
            {
                menuBtnItems.map((item, key) => {
                    if (item.isGroup) {
                        //RECO-12678
                        //Since the tag "role: MenuItem" will not be added when updating the sub element of groupbutton, resulting in problems in vpat, 
                        //the following methods are used to force the update
                        return <span key={key} style={{ display: "inline-block" }}>
                            {item.buttons.length % 2 == 0 && <R.ButtonGroup
                                id={item.id}
                                text={item.name}
                                classify={item.classify || "theme"}
                                icon={item.icon}
                            >
                                {item.buttons.map((item) => {
                                    return <R.Button key={item.id} onClick={item.onClick} id={item.id} text={item.name} />;
                                })}
                            </R.ButtonGroup>}
                            {item.buttons.length % 2 == 1 && <R.ButtonGroup
                                id={item.id}
                                text={item.name}
                                classify={item.classify || "theme"}
                                icon={item.icon}
                            >
                                {item.buttons.map((item) => {
                                    return <R.Button key={item.id} onClick={item.onClick} id={item.id} text={item.name} />;
                                })}
                            </R.ButtonGroup>}
                        </span>;
                    } else {
                        if (item.isStatic) {
                            return <R.Button
                                primary={true}
                                classify={"theme"}
                                key={item.id}
                                id={item.id}
                                icon={item.icon}
                                text={item.name}
                                onClick={item.onClick} />;
                        } else {
                            return <R.Button
                                primary={false}
                                classify={"default"}
                                key={item.id}
                                id={item.id}
                                icon={item.icon}
                                text={item.name}
                                onClick={item.onClick} />;
                        }
                    }
                })
            }
            {menuBtnItemsInMore && menuBtnItemsInMore.length > 0 &&
                <span className="more-button" style={{ display: "inline-block" }}>
                    <R.ButtonGroup type="action" tooltip={RMResx.RM_PRM_PRE_More}>
                        {
                            menuBtnItemsInMore.map((item, key) => (
                                <R.Button
                                    key={key}
                                    id={item.id}
                                    text={item.name}
                                    tooltip={item.name}
                                    onClick={item.onClick} />
                            ))
                        }
                    </R.ButtonGroup>
                </span>
            }
        </div>;
    }
}