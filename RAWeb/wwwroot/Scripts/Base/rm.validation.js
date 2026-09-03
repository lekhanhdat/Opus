/*Covered by AvePoint copyright and license agreement*/
$$page("rm.validation", {
    _create: function () {
        this._initValidationConfig();
    },
    _initValidationConfig: function () {
        ko.validation.init({
            registerExtenders: true,
            messagesOnModified: true,
            insertMessages: true,
            parseInputAttributes: true,
            messageTemplate: "RMValidationTemplate"
        });
    }
});
