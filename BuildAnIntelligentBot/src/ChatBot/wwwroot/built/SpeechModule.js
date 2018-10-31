"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tslib_1 = require("tslib");
var jspeech_1 = require("jspeech");
function prefixFallback(type, prefixes) {
    if (prefixes === void 0) { prefixes = ['moz', 'ms', 'webkit']; }
    return [''].concat(prefixes).reduce(function (found, prefix) { return found || window[prefix + type]; }, null);
}
function waitEvent(emitter, name) {
    return new Promise(function (resolve, reject) {
        var detach = function () {
            emitter.removeEventListener(name, rejectListener);
            emitter.removeEventListener(name, resolveListener);
        };
        var rejectListener = function (event) {
            detach();
            reject(event);
        };
        var resolveListener = function (event) {
            detach();
            resolve(event);
        };
        emitter.addEventListener(name, resolveListener);
        emitter.addEventListener('error', rejectListener);
    });
}
var Speech;
(function (Speech) {
    var SpeechRecognizer = (function () {
        function SpeechRecognizer() {
        }
        SpeechRecognizer.setSpeechRecognizer = function (recognizer) {
            SpeechRecognizer.instance = recognizer;
        };
        SpeechRecognizer.startRecognizing = function (locale, grammars, onIntermediateResult, onFinalResult, onAudioStreamStarted, onRecognitionFailed) {
            if (locale === void 0) { locale = 'en-US'; }
            if (onIntermediateResult === void 0) { onIntermediateResult = null; }
            if (onFinalResult === void 0) { onFinalResult = null; }
            if (onAudioStreamStarted === void 0) { onAudioStreamStarted = null; }
            if (onRecognitionFailed === void 0) { onRecognitionFailed = null; }
            return tslib_1.__awaiter(this, void 0, void 0, function () {
                return tslib_1.__generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!SpeechRecognizer.speechIsAvailable()) {
                                return [2 /*return*/];
                            }
                            if (!(locale && SpeechRecognizer.instance.locale !== locale)) return [3 /*break*/, 2];
                            return [4 /*yield*/, SpeechRecognizer.instance.stopRecognizing()];
                        case 1:
                            _a.sent();
                            SpeechRecognizer.instance.locale = locale; // to do this could invalidate warmup.
                            _a.label = 2;
                        case 2:
                            SpeechRecognizer.instance.setGrammars(grammars);
                            if (!SpeechRecognizer.alreadyRecognizing()) return [3 /*break*/, 4];
                            return [4 /*yield*/, SpeechRecognizer.stopRecognizing()];
                        case 3:
                            _a.sent();
                            _a.label = 4;
                        case 4:
                            SpeechRecognizer.instance.onIntermediateResult = onIntermediateResult;
                            SpeechRecognizer.instance.onFinalResult = onFinalResult;
                            SpeechRecognizer.instance.onAudioStreamingToService = onAudioStreamStarted;
                            SpeechRecognizer.instance.onRecognitionFailed = onRecognitionFailed;
                            return [4 /*yield*/, SpeechRecognizer.instance.startRecognizing()];
                        case 5:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        SpeechRecognizer.stopRecognizing = function () {
            return tslib_1.__awaiter(this, void 0, void 0, function () {
                return tslib_1.__generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!SpeechRecognizer.speechIsAvailable()) {
                                return [2 /*return*/];
                            }
                            return [4 /*yield*/, SpeechRecognizer.instance.stopRecognizing()];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        SpeechRecognizer.warmup = function () {
            if (!SpeechRecognizer.speechIsAvailable()) {
                return;
            }
            SpeechRecognizer.instance.warmup();
        };
        SpeechRecognizer.speechIsAvailable = function () {
            return SpeechRecognizer.instance != null && SpeechRecognizer.instance.speechIsAvailable();
        };
        SpeechRecognizer.alreadyRecognizing = function () {
            return SpeechRecognizer.instance ? SpeechRecognizer.instance.isStreamingToService : false;
        };
        return SpeechRecognizer;
    }());
    SpeechRecognizer.instance = null;
    Speech.SpeechRecognizer = SpeechRecognizer;
    var SpeechSynthesizer = (function () {
        function SpeechSynthesizer() {
        }
        SpeechSynthesizer.setSpeechSynthesizer = function (speechSynthesizer) {
            SpeechSynthesizer.instance = speechSynthesizer;
        };
        SpeechSynthesizer.speak = function (text, lang, onSpeakingStarted, onSpeakingFinished) {
            if (onSpeakingStarted === void 0) { onSpeakingStarted = null; }
            if (onSpeakingFinished === void 0) { onSpeakingFinished = null; }
            if (SpeechSynthesizer.instance == null)
                return;
            SpeechSynthesizer.instance.speak(text, lang, onSpeakingStarted, onSpeakingFinished);
        };
        SpeechSynthesizer.stopSpeaking = function () {
            if (SpeechSynthesizer.instance == null)
                return;
            SpeechSynthesizer.instance.stopSpeaking();
        };
        return SpeechSynthesizer;
    }());
    SpeechSynthesizer.instance = null;
    Speech.SpeechSynthesizer = SpeechSynthesizer;
    var BrowserSpeechRecognizer = (function () {
        function BrowserSpeechRecognizer() {
            var _this = this;
            this.locale = null;
            this.isStreamingToService = false;
            this.onIntermediateResult = null;
            this.onFinalResult = null;
            this.onAudioStreamingToService = null;
            this.onRecognitionFailed = null;
            this.recognizer = null;
            if (!window.webkitSpeechRecognition) {
                console.error("This browser does not support speech recognition");
                return;
            }
            this.recognizer = new window.webkitSpeechRecognition();
            this.recognizer.lang = 'en-US';
            this.recognizer.interimResults = true;
            this.recognizer.onaudiostart = function () {
                if (_this.onAudioStreamingToService) {
                    _this.onAudioStreamingToService();
                }
            };
            this.recognizer.onresult = function (srevent) {
                if (srevent.results == null || srevent.length == 0) {
                    return;
                }
                var result = srevent.results[0];
                if (result.isFinal === true && _this.onFinalResult != null) {
                    _this.onFinalResult(result[0].transcript);
                }
                else if (result.isFinal === false && _this.onIntermediateResult != null) {
                    var text = "";
                    for (var i = 0; i < srevent.results.length; ++i) {
                        text += srevent.results[i][0].transcript;
                    }
                    _this.onIntermediateResult(text);
                }
            };
            this.recognizer.onerror = function (err) {
                if (_this.onRecognitionFailed) {
                    _this.onRecognitionFailed();
                }
                throw err;
            };
            this.recognizer.onend = function () {
                _this.isStreamingToService = false;
            };
        }
        BrowserSpeechRecognizer.prototype.speechIsAvailable = function () {
            return this.recognizer != null;
        };
        BrowserSpeechRecognizer.prototype.warmup = function () {
        };
        BrowserSpeechRecognizer.prototype.startRecognizing = function () {
            this.isStreamingToService = true;
            this.recognizer.start();
            return waitEvent(this.recognizer, 'start').then(function () { });
        };
        BrowserSpeechRecognizer.prototype.stopRecognizing = function () {
            if (this.isStreamingToService) {
                this.recognizer.stop();
                return waitEvent(this.recognizer, 'end').then(function () { });
            }
            else {
                return Promise.resolve();
            }
        };
        BrowserSpeechRecognizer.prototype.setGrammars = function (grammars) {
            if (grammars === void 0) { grammars = []; }
            var list = new (prefixFallback('SpeechGrammarList'));
            if (!list) {
                if (grammars.length) {
                    console.warn('This browser does not support speech grammar list');
                }
                return;
            }
            else if (!grammars.length) {
                return;
            }
            var grammar = jspeech_1.default('listenfor');
            grammar.public.rule('hint', grammars.join(' | '));
            list.addFromString(grammar.stringify());
            this.recognizer.grammars = list;
        };
        return BrowserSpeechRecognizer;
    }());
    Speech.BrowserSpeechRecognizer = BrowserSpeechRecognizer;
    var BrowserSpeechSynthesizer = (function () {
        function BrowserSpeechSynthesizer() {
            this.lastOperation = null;
            this.audioElement = null;
            this.speakRequests = [];
        }
        BrowserSpeechSynthesizer.prototype.speak = function (text, lang, onSpeakingStarted, onSpeakingFinished) {
            var _this = this;
            if (onSpeakingStarted === void 0) { onSpeakingStarted = null; }
            if (onSpeakingFinished === void 0) { onSpeakingFinished = null; }
            if (!('SpeechSynthesisUtterance' in window) || !text)
                return;
            if (this.audioElement === null) {
                var audio = document.createElement('audio');
                audio.id = 'player';
                audio.autoplay = true;
                this.audioElement = audio;
            }
            var chunks = new Array();
            if (text[0] === '<') {
                if (text.indexOf('<speak') != 0)
                    text = '<speak>\n' + text + '\n</speak>\n';
                var parser = new DOMParser();
                var dom = parser.parseFromString(text, 'text/xml');
                var nodes = dom.documentElement.childNodes;
                this.processNodes(nodes, chunks);
            }
            else {
                chunks.push(text);
            }
            var onSpeakingFinishedWrapper = function () {
                if (onSpeakingFinished !== null)
                    onSpeakingFinished();
                // remove this from the queue since it's done:
                if (_this.speakRequests.length) {
                    _this.speakRequests[0].completed();
                    _this.speakRequests.splice(0, 1);
                }
                // If there are other speak operations in the queue, process them
                if (_this.speakRequests.length) {
                    _this.playNextTTS(_this.speakRequests[0], 0);
                }
            };
            var request = new SpeakRequest(chunks, lang, function (speakOp) { _this.lastOperation = speakOp; }, onSpeakingStarted, onSpeakingFinishedWrapper);
            if (this.speakRequests.length === 0) {
                this.speakRequests = [request];
                this.playNextTTS(this.speakRequests[0], 0);
            }
            else {
                this.speakRequests.push(request);
            }
        };
        BrowserSpeechSynthesizer.prototype.stopSpeaking = function () {
            if (('SpeechSynthesisUtterance' in window) === false)
                return;
            if (this.speakRequests.length) {
                if (this.audioElement)
                    this.audioElement.pause();
                this.speakRequests.forEach(function (req) {
                    req.abandon();
                });
                this.speakRequests = [];
                var ss = window.speechSynthesis;
                if (ss.speaking || ss.pending) {
                    if (this.lastOperation)
                        this.lastOperation.onend = null;
                    ss.cancel();
                }
            }
        };
        ;
        BrowserSpeechSynthesizer.prototype.playNextTTS = function (requestContainer, iCurrent) {
            // lang : string, onSpeakQueued: Func<SpeechSynthesisUtterance, void>, onSpeakStarted : Action, onFinishedSpeaking : Action
            var _this = this;
            var moveToNext = function () {
                _this.playNextTTS(requestContainer, iCurrent + 1);
            };
            if (iCurrent < requestContainer.speakChunks.length) {
                var current = requestContainer.speakChunks[iCurrent];
                if (typeof current === 'number') {
                    setTimeout(moveToNext, current);
                }
                else {
                    if (current.indexOf('http') === 0) {
                        var audio = this.audioElement; // document.getElementById('player');
                        audio.src = current;
                        audio.onended = moveToNext;
                        audio.onerror = moveToNext;
                        audio.play();
                    }
                    else {
                        var msg = new SpeechSynthesisUtterance();
                        // msg.voiceURI = 'native';
                        // msg.volume = 1; // 0 to 1
                        // msg.rate = 1; // 0.1 to 10
                        // msg.pitch = 2; //0 to 2
                        msg.text = current;
                        msg.lang = requestContainer.lang;
                        msg.onstart = iCurrent === 0 ? requestContainer.onSpeakingStarted : null;
                        msg.onend = moveToNext;
                        msg.onerror = moveToNext;
                        if (requestContainer.onSpeakQueued)
                            requestContainer.onSpeakQueued(msg);
                        window.speechSynthesis.speak(msg);
                    }
                }
            }
            else {
                if (requestContainer.onSpeakingFinished)
                    requestContainer.onSpeakingFinished();
            }
        };
        // process SSML markup into an array of either
        // * utterenance
        // * number which is delay in msg
        // * url which is an audio file
        BrowserSpeechSynthesizer.prototype.processNodes = function (nodes, output) {
            for (var i = 0; i < nodes.length; i++) {
                var node = nodes[i];
                switch (node.nodeName) {
                    case 'p':
                        this.processNodes(node.childNodes, output);
                        output.push(250);
                        break;
                    case 'break':
                        if (node.attributes.getNamedItem('strength')) {
                            var strength = node.attributes.getNamedItem('strength').nodeValue;
                            if (strength === 'weak') {
                                // output.push(50);
                            }
                            else if (strength === 'medium') {
                                output.push(50);
                            }
                            else if (strength === 'strong') {
                                output.push(100);
                            }
                            else if (strength === 'x-strong') {
                                output.push(250);
                            }
                        }
                        else if (node.attributes.getNamedItem('time')) {
                            output.push(JSON.parse(node.attributes.getNamedItem('time').value));
                        }
                        break;
                    case 'audio':
                        if (node.attributes.getNamedItem('src')) {
                            output.push(node.attributes.getNamedItem('src').value);
                        }
                        break;
                    case 'say-as':
                    case 'prosody': // ToDo: handle via msg.rate
                    case 'emphasis': // ToDo: can probably emulate via prosody + pitch
                    case 'w':
                    case 'phoneme': //
                    case 'voice':
                        this.processNodes(node.childNodes, output);
                        break;
                    default:
                        // Todo: coalesce consecutive non numeric / non html entries.
                        output.push(node.textContent);
                        break;
                }
            }
        };
        return BrowserSpeechSynthesizer;
    }());
    Speech.BrowserSpeechSynthesizer = BrowserSpeechSynthesizer;
    var SpeakRequest = (function () {
        function SpeakRequest(speakChunks, lang, onSpeakQueued, onSpeakingStarted, onSpeakingFinished) {
            if (onSpeakQueued === void 0) { onSpeakQueued = null; }
            if (onSpeakingStarted === void 0) { onSpeakingStarted = null; }
            if (onSpeakingFinished === void 0) { onSpeakingFinished = null; }
            this._onSpeakQueued = null;
            this._onSpeakingStarted = null;
            this._onSpeakingFinished = null;
            this._speakChunks = [];
            this._lang = null;
            this._onSpeakQueued = onSpeakQueued;
            this._onSpeakingStarted = onSpeakingStarted;
            this._onSpeakingFinished = onSpeakingFinished;
            this._speakChunks = speakChunks;
            this._lang = lang;
        }
        SpeakRequest.prototype.abandon = function () {
            this._speakChunks = [];
        };
        SpeakRequest.prototype.completed = function () {
            this._speakChunks = [];
        };
        Object.defineProperty(SpeakRequest.prototype, "onSpeakQueued", {
            get: function () { return this._onSpeakQueued; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SpeakRequest.prototype, "onSpeakingStarted", {
            get: function () { return this._onSpeakingStarted; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SpeakRequest.prototype, "onSpeakingFinished", {
            get: function () { return this._onSpeakingFinished; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SpeakRequest.prototype, "speakChunks", {
            get: function () { return this._speakChunks; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SpeakRequest.prototype, "lang", {
            get: function () { return this._lang; },
            enumerable: true,
            configurable: true
        });
        return SpeakRequest;
    }());
})(Speech = exports.Speech || (exports.Speech = {}));
//# sourceMappingURL=SpeechModule.js.map