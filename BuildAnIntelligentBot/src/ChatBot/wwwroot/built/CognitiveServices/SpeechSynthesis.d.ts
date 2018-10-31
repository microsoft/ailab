import { Speech, Action } from '../SpeechModule';
export interface ICognitiveServicesSpeechSynthesisProperties {
    subscriptionKey?: string;
    gender?: SynthesisGender;
    voiceName?: string;
    customVoiceEndpointUrl?: string;
    fetchCallback?: (authFetchEventId: string) => Promise<string>;
    fetchOnExpiryCallback?: (authFetchEventId: string) => Promise<string>;
}
export declare enum SynthesisGender {
    Male = 0,
    Female = 1,
}
export declare class SpeechSynthesizer implements Speech.ISpeechSynthesizer {
    private _requestQueue;
    private _isPlaying;
    private _audioElement;
    private _helper;
    private _properties;
    constructor(properties: ICognitiveServicesSpeechSynthesisProperties);
    speak(text: string, lang: string, onSpeakingStarted?: Action, onSpeakingFinished?: Action): void;
    stopSpeaking(): void;
    private playAudio();
    private getSpeechData();
    private log(message);
}
