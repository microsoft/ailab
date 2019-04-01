import { getUniqueStrings } from "../../util";

export const buildTargetWords = (activeSearch: string) => {
  const activeTargetWords = getActiveSearchTargetWords(activeSearch);

  const targetWords = [
    ...activeTargetWords,
  ];
  
  return getUniqueStrings(targetWords);
};

const getActiveSearchTargetWords = (activeSearch: string) => (
  Boolean(activeSearch) ?
    activeSearch.split(" ") :
    []
);

