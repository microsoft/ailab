const appInsights  = require('applicationinsights');

const VERBOSE = 'VERBOSE'
const LOG = 'LOG';
const WARNING = 'WARNING';
const ERROR = 'ERROR';


class Trace {
    constructor(conversation, prefix) {
        this._conversation = conversation;
        this._prefix = prefix;
    }

    verbose(message, optionalParam) {
        if (process.env.TRACE_LEVEL != VERBOSE)
            return;

        this.log(message, optionalParam);
    }

    log(message, optionalParam) {
        if (process.env.TRACE_LEVEL === ERROR || process.env.TRACE_LEVEL=== WARNING)
            return;

        if (this._conversation)
            console.log(`${new Date().toISOString()} [${this._conversation.id}] ${this._prefix}\t${message}`);
        else
            console.log(`${new Date().toISOString()} ${this._prefix}\t${message}`);
        if (optionalParam && process.env.TRACE_LEVEL == VERBOSE)
            console.log(optionalParam.constructor.name + JSON.stringify(optionalParam, null, 2));
    }

    warning(message) {
        if (process.env.TRACE_LEVEL == ERROR)
            return;
        if (this._conversation)
            console.error(`${new Date().toISOString()} [${this._conversation.id}] ${this._prefix}\tWARNING: ${message}`);
        else 
            console.error(`${new Date().toISOString()} ${this._prefix}\tWARNING: ${message}`);
        if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY)
            appInsights.defaultClient.trackTrace({ message, severity: 2 });
    }

    error(message, optionalParam) {
        if (this._conversation)
            console.error(`${new Date().toISOString()} [${this._conversation.id}] ${this._prefix}\tERROR: ${message}`);
        else
            console.error(`${new Date().toISOString()} ${this._prefix}\tERROR: ${message}`);
        console.error(optionalParam);
        if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY)
            appInsights.defaultClient.trackException({ exception: new Error(message) });
    }
};

module.exports = Trace;