//=========================================================
// Port (default 1337)
//=========================================================
process.env.PORT = process.env.PORT || 1337;

//=========================================================
// Console trace level (default LOG)
//=========================================================
// VERBOSE, LOG, WARNING, ERROR
//=========================================================
process.env.TRACE_LEVEL = process.env.TRACE_LEVEL || 'LOG';

//=========================================================
// Direct Line endpoint (default directline.botframework.com)
//=========================================================
// - directline.botframework.com routes your client to the nearest datacenter.
//   This is the best option if you do not know where your client is located.
// - asia.directline.botframework.com routes only to Direct Line servers in Eastern Asia.
// - europe.directline.botframework.com routes only to Direct Line servers in Europe.
// - northamerica.directline.botframework.com routes only to Direct Line servers in North America.
//=========================================================
process.env.DIRECTLINE_ENDPOINT = process.env.DIRECTLINE_ENDPOINT || 'directline.botframework.com';

//=========================================================
// Polling interval (default 500ms)
//=========================================================
process.env.POLLING_INTERVAL = process.env.POLLING_INTERVAL || 500;

//=========================================================
// Conversation Timeout (in sg) (default 10 min)
//=========================================================
process.env.CONVERSATION_TIMEOUT = process.env.CONVERSATION_TIMEOUT || 10 * 60;


process.env.APPINSIGHTS_INSTRUMENTATIONKEY