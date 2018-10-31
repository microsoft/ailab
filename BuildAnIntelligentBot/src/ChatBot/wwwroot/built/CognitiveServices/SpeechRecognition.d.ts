import { Speech, Func, Action } from '../SpeechModule';
export interface ISpeechContextDgiGroup {
    Type: string;
    Hints?: {
        ReferenceGrammar: string;
    };
    Items?: {
        Text: string;
    }[];
}
export interface ISpeechContext {
    dgi: {
        Groups: ISpeechContextDgiGroup[];
    };
}
export interface ICognitiveServicesSpeechRecognizerProperties {
    locale?: string;
    subscriptionKey?: string;
    fetchCallback?: (authFetchEventId: string) => Promise<string>;
    fetchOnExpiryCallback?: (authFetchEventId: string) => Promise<string>;
    recognitionAPI?: number;
    endpointId?: string;
}
export declare class SpeechRecognizer implements Speech.ISpeechRecognizer {
    audioStreamStartInitiated: boolean;
    isStreamingToService: boolean;
    onIntermediateResult: Func<string, void>;
    onFinalResult: Func<string, void>;
    onAudioStreamingToService: Action;
    onRecognitionFailed: Action;
    locale: string;
    referenceGrammarId: string;
    private actualRecognizer;
    private grammars;
    private properties;
    constructor(properties?: ICognitiveServicesSpeechRecognizerProperties);
    warmup(): void;
    setGrammars(grammars: string[]): void;
    startRecognizing(): Promise<any>;
    speechIsAvailable(): boolean;
    stopRecognizing(): Promise<void>;
    private log(message);
}
