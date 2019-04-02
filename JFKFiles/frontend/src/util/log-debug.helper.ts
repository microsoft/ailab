export const consoleDebug = (function () {
  console.debug = process.env.DEBUG_TRACES ? 
    console.log.bind(console, "[Debug]") : () => {};
})();
