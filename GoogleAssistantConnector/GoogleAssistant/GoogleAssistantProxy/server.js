//=========================================================
// Import modules
//=========================================================
const directLineToActionsOnGoogle = require('DirectLineToActionsOnGoogleLib');
const express = require('express');

// Messages obj
const messagesObj = {
    //welcomeMessages: [
    //    ['es', ['Hola, soy el asistente de Bot. ¿En qué puedo ayudarte?', 'Hola, soy el asistente de Bot. ¿te puedo ayudar?']],
    //    ['en', ['Hello, I´m a assitant of Bot. ¿?']]
    //],
    startTriggers: [
        ['es', ['hola']],
        ['en', ['hello']]
    ]
    //anythingElseMessages: [
    //    ['es', ['¿Qué más quieres saber?', 'Pregúntame más cosas']],
    //    ['en', ['Is there anything else I can help you with?', 'What\'s next for you ?']]
    //],
    //exitMessages: [
    //    ['es', ['Muy bien, hasta la próxima!', 'Hasta otra, espero haberte ayudado!', 'Nos vemos!', 'Ok, pasa un buen día!']],
    //    ['en', ['Good bye!!!']]
    //],
    //exitTriggers: [
    //    ['es', ['adios', 'adiós', 'salir', 'nada mas', 'nada más']],
    //    ['en', ['bye', 'exit', 'quit']]]
};

// Initialize instances
const app = express();

app.get('', (req, res) => res.send('GoogleAssistantProxyReady'));
app.use(directLineToActionsOnGoogle(process.env.DIRECT_LINE_SECRET, messagesObj).router);

app.listen(process.env.PORT, () => console.log('Express server listening on port ' + process.env.PORT));