export interface Item {
  title: string;
  subtitle?: string;
  thumbnail?: string;
  excerpt?: string;
  rating?: number;
  extraFields?: any[];
  metadata?: any;
  demoInitialPage?: number;
  type?: string;
  highlightWords: string[];
}

export type ItemCollection = Item[];
