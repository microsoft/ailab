export const isArrayEmpty = (array: any[]) => !array || !array.length;

export const isValueInArray = (array: any[], value) => isArrayEmpty(array) ? false : (array.indexOf(value) >= 0);

export const addValueToArray = (array: any[], value) => {
  if (array) {
    return isValueInArray(array, value) ? array : [...array, value];
  } else {
    return [value];
  }  
};

export const removeValueFromArray = (array: any[], value) => {
  return isValueInArray(array, value) ? array.filter(v => v !== value) : array;
};

export const getUniqueStrings = (array: string[]) => ([
  ...new Set(array),
]);