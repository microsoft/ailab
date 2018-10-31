"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.log = function (message) {
    var optionalParams = [];
    for (var _i = 1; _i < arguments.length; _i++) {
        optionalParams[_i - 1] = arguments[_i];
    }
    if (typeof (window) !== 'undefined' && window["botchatDebug"] && message)
        console.log.apply(console, [message].concat(optionalParams));
};
//# sourceMappingURL=Konsole.js.map