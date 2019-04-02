export const checkDuckType = function<T>(obj: T, property: keyof T) {
  return obj ? obj.hasOwnProperty(property) : false;
}

