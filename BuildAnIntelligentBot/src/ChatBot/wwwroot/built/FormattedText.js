"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var MarkdownIt = require("markdown-it");
var React = require("react");
exports.FormattedText = function (props) {
    if (!props.text || props.text === '')
        return null;
    switch (props.format) {
        case "xml":
        case "plain":
            return renderPlainText(props.text);
        default:
            return renderMarkdown(props.text, props.onImageLoad);
    }
};
var renderPlainText = function (text) {
    var lines = text.replace('\r', '').split('\n');
    var elements = lines.map(function (line, i) { return React.createElement("span", { key: i },
        line,
        React.createElement("br", null)); });
    return React.createElement("span", { className: "format-plain" }, elements);
};
var markdownIt = new MarkdownIt({ html: false, xhtmlOut: true, breaks: true, linkify: true, typographer: true });
//configure MarkdownIt to open links in new tab
//from https://github.com/markdown-it/markdown-it/blob/master/docs/architecture.md#renderer
// Remember old renderer, if overriden, or proxy to default renderer
var defaultRender = markdownIt.renderer.rules.link_open || (function (tokens, idx, options, env, self) {
    return self.renderToken(tokens, idx, options);
});
markdownIt.renderer.rules.link_open = function (tokens, idx, options, env, self) {
    // If you are sure other plugins can't add `target` - drop check below
    var targetIndex = tokens[idx].attrIndex('target');
    if (targetIndex < 0) {
        tokens[idx].attrPush(['target', '_blank']); // add new attribute
    }
    else {
        tokens[idx].attrs[targetIndex][1] = '_blank'; // replace value of existing attr
    }
    // pass token to default renderer.
    return defaultRender(tokens, idx, options, env, self);
};
var renderMarkdown = function (text, onImageLoad) {
    var __html;
    if (text.trim()) {
        var src = text
            .replace(/<br\s*\/?>/ig, '\n')
            .replace(/\[(.*?)\]\((.*?)( +".*?"){0,1}\)/ig, function (match, text, url, title) { return "[" + text + "](" + markdownIt.normalizeLink(url) + (title === undefined ? '' : title) + ")"; });
        var arr = src.split(/\n *\n|\r\n *\r\n|\r *\r/);
        var ma = arr.map(function (a) { return markdownIt.render(a); });
        __html = ma.join('<br/>');
    }
    else {
        // Replace spaces with non-breaking space Unicode characters
        __html = text.replace(/ */, '\u00A0');
    }
    return React.createElement("div", { className: "format-markdown", dangerouslySetInnerHTML: { __html: __html } });
};
//# sourceMappingURL=FormattedText.js.map