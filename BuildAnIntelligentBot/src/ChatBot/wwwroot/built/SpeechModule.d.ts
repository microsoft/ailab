export declare type Action = () => void;
export declare type Func<T, TResult> = (item: T) => TResult;
export declare module Speech {
    interface ISpeechRecognizer {
        locale: string;
        isStreamingToService: boolean;
        referenceGrammarId: string;
        onIntermediateResult: Func<string, void>;
        onFinalResult: Func<string, void>;
        onAudioStreamingToService: Action;
        onRecognitionFailed: Action;
        warmup(): void;
        setGrammars(grammars?: string[]): void;
        startRecognizing(): Promise<void>;
        stopRecognizing(): Promise<void>;
        speechIsAvailable(): boolean;
    }
    interface ISpeechSynthesizer {
        speak(text: string, lang: string, onSpeakingStarted: Action, onspeakingFinished: Action): void;
        stopSpeaking(): void;
    }
    class SpeechRecognizer {
        private static instance;
        static setSpeechRecognizer(recognizer: ISpeechRecognizer): void;
        static startRecognizing(locale?: string, grammars?: string[], onIntermediateResult?: Func<string, void>, onFinalResult?: Func<string, void>, onAudioStreamStarted?: Action, onRecognitionFailed?: Action): Promise<void>;
        static stopRecognizing(): Promise<void>;
        static warmup(): void;
        static speechIsAvailable(): boolean;
        private static alreadyRecognizing();
    }
    class SpeechSynthesizer {
        private static instance;
        static setSpeechSynthesizer(speechSynthesizer: ISpeechSynthesizer): void;
        static speak(text: string, lang: string, onSpeakingStarted?: Action, onSpeakingFinished?: Action): void;
        static stopSpeaking(): void;
    }
    class BrowserSpeechRecognizer implements ISpeechRecognizer {
        locale: string;
        isStreamingToService: boolean;
        referenceGrammarId: string;
        onIntermediateResult: Func<string, void>;
        onFinalResult: Func<string, void>;
        onAudioStreamingToService: Action;
        onRecognitionFailed: Action;
        private recognizer;
        constructor();
        speechIsAvailable(): boolean;
        warmup(): void;
        startRecognizing(): Promise<void>;
        stopRecognizing(): Promise<void>;
        setGrammars(grammars?: string[]): void;
    }
    class BrowserSpeechSynthesizer implements ISpeechSynthesizer {
        private lastOperation;
        private audioElement;
        private speakRequests;
        speak(text: string, lang: string, onSpeakingStarted?: Action, onSpeakingFinished?: Action): void;
        stopSpeaking(): void;
        private playNextTTS(requestContainer, iCurrent);
        private processNodes(nodes, output);
    }
}
