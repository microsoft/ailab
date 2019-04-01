/**
 * CNC = Class Name Componser.
 * So you pas an array of names (whether they are null, undefined,
 * or have a valid value) and it filter invalid ones and join them
 * together to compose a full style class name.
 */

 export const cnc = (...names) => names.filter(n => n).join(" ")