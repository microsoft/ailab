# Mensajes de *GoogleAssistantProxy*
> [here for English document](googleAssistantProxy.messages.md)

Utilizamos el objeto ***messagesObj*** en el fichero ***server.js*** del proyecto *GoogleAssitantProxy* para configurar como queremos que nuestro proxy responda automáticamente a distintos eventos.
```javascript
// Messages obj
const messagesObj = {
    welcomeMessages: [
        ['es', ['Hola, soy el asistente de XXXX. ¿En qué puedo ayudarte?', 'Hola, soy el asistente de ArenaGG. ¿te puedo ayudar?']],
        ['en', ['Hello, I´m a assitant of XXXX. ¿?']]
    ],
    startTriggers: [
        ['es', ['hola']],
        ['en', ['hello']]
    ],
    anythingElseMessages: [
        ['es', ['¿Qué más quieres saber?', 'Pregúntame más cosas']],
        ['en', ['Is there anything else I can help you with?', 'What\'s next for you ?']]
    ],
    exitMessages: [
        ['es', ['Muy bien, hasta la próxima!', 'Hasta otra, espero haberte ayudado!', 'Nos vemos!', 'Ok, pasa un buen día!']],
        ['en', ['Good bye!!!']]
    ],
    exitTriggers: [
        ['es', ['adios', 'adiós', 'salir', 'nada mas', 'nada más']],
        ['en', ['bye', 'exit', 'quit']]]
};

// Initialize instances
const app = express();

app.use(directLineToActionsOnGoogle(process.env.DIRECT_LINE_SECRET, messagesObj).router);
```

Cada propiedad está formada por un array de arrays, donde podemos añadir distintas frases a cada uno de los idiomas que soporte nuestro bot.

Podemos configurar los siguientes eventos:

## 1- Mensaje de bienvenida
La interacción con *Google Assitant* requiere que enviemos al usuario un mensaje de bienvenida antes de que este nos pregunte.


Lo podemos hacer de distintas formas, según como hayamos programado nuestro *Bot Framework*:

-  Nuestro bot envía un mensaje de bienvendia al detectar un ***ActivityType* *ConversationUpdate***:
   
   En este caso, enhorabuena!, tu bot tiene mucho potencial de estar muy bien construido!. Simplemente debes eliminar la propiedad *welcomeMessages*.

   Sino, [Aquí](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/03.welcome-user) tienes un ejemplo de como se puede conseguir.

   Para averigurar facilmente si realmente envia el mensaje, podemos usar el *Bot Framework Emulator*, para comprobar si nos envia un mensaje al contectarnos a nuestro bot.
   > A día de hoy (marzo del 2019), los ejemplos oficiales que hay no funcionan correctamente.
   >
   > El problema viene de que al conectarnos sobre los canales *webchat* o *directline* no se envía el usuario hasta que este manda un mensaje, por lo que la primera actividad de tipo *ConversationUpdate* la interpreta como del propio bot.
   >
   > Un *workarround* sería sustituir:
   >```c#
   >if (member.Id != turnContext.Activity.Recipient.Id)
   >{
   >    await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", ...
   >    await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
   >    await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
   >}
   >```
   >por
   >```c#
   >bool sendWelcomeMessage = false;
   >if (new[] { "webchat", "directline" }.Contains(turnContext.Activity.ChannelId))
   >{
   >    sendWelcomeMessage =  member.Id == turnContext.Activity.Recipient.Id;
   >}
   > else
   > {
   >    sendWelcomeMessage = member.Id != turnContext.Activity.Recipient.Id;
   >}
   >
   >if (sendWelcomeMessage)
   >{
   > await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", ...
   > await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
   > await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
   >}
   >```

- Nuestro bot envía un mensaje de bienvendida cuando recibe **una frase concreta** (como por ejemplo 'hola').

  En este caso tenemos un bot un poco perezoso y tenemos que ayudarle un poquito. Lo haremos utilizando la propiedad ***startTriggers*** con los textos que nuestro proxy enviará automáticamente cuando se inicie una intereacción con *Google Assistant*.

  ```javascript
  // Messages obj
  const messagesObj = {
    ...
    startTriggers: [
        ['es', ['hola']],
        ['en', ['hello']]
    ],
    ...
  }; 
  ```

- Nuestro bot es **directo**.
  
  Es un bot poco sociable y solo contesta a preguntas. Para que *Google Assistant* acepte nuestras preguntas, primero requiere un mensaje de bienvenida. Para ello utilizaremos la propiedad ***welcomeMessages*** para que nuestro proxy la envíe.
   ```javascript
  const messagesObj = {
    welcomeMessages: [
        ['es', ['Hola, soy el asistente de XXXX. ¿En qué puedo ayudarte?', 'Hola, soy el asistente de ArenaGG. ¿te puedo ayudar?']],
        ['en', ['Hello, I´m a assitant of XXXX. ¿?']]
    ],
    ...
  };
  ```
  > Por cada idioma puedes añadir una colección de frases (no tienen que tener el mismo número de frases casa idioma), y el proxy responderá de forma aleatoria cada una de ellas.

## 2- Mensaje de *algo más*
Otro requerimiento de *Google Assistant* es que despues de una interacción con el usuario le preguntes si desea otra cosa para así mantener la conversación abierta.

De nuevo, podemos configurar el proxy según como esté diseñado nuestro bot:

- Nuestro bot responde siempre con una pregunta para incentivar al usuario.

  Perfecto!. Solamente hay que eliminar la propiedad ***anythingElseMessages***.

- Nuestro bot requiere una ayudita y nos gustaría que se mandase automáticamente.
  Añadimos la propiedad ***anythingElseMessages*** con los mensajes que *funcionen* con la personaldad de nuestro bot.
   ```javascript
  const messagesObj = {
    ...
    anythingElseMessages: [
        ['es', ['¿Qué más quieres saber?', 'Pregúntame más cosas']],
        ['en', ['Is there anything else I can help you with?', 'What\'s next for you ?']]
    ],
    ...
  };
  ```
  > Por cada idioma puedes añadir una colección de frases (no tienen que tener el mismo número de frases casa idioma), y el proxy responderá de forma aleatoria cada una de ellas.

  >  Nuestro Proxy no añadirá una frase de *algo más* en las siguientes situaciones:
  > - Si alguna de las frases que se han enviado contiene una interrogación.
  > - Si alguna respuesta es un listado de sugerencias:
  >   - Una *activity* que tiene *suggestedActions* 
  >   - o un *attachment* de tipo *HeroCard* sólo con un listado de botones de tipo *imBack*.


## 3- Mensajes para cancelar la conversación
Nuestro proxy cancela la conversación si recive un mensaje de tipo ***googleAssistantProxy.messages***.

Si nuestro bot no tiene en cuenta esas situaciones, nuestro proxy puede analizar las contestaciones del usuario para ver si contiene alguno de los mensajes de la propiedad ***exitTriggers*** y cancelar la conversación por nosotros.
```javascript
  const messagesObj = {
    ...
    exitTriggers: [
        ['es', ['adios', 'adiós', 'salir', 'nada mas', 'nada más']],
        ['en', ['bye', 'exit', 'quit']]]
  };
  ```

## 4- Mensaje de despedida
  Nuestro bot debe enviar un mensaje cuando cerramos al comunicación con el usuario.

  Si nuestro bot, no envía mensaje de despedia, podemos configurar nuestro *proxy* añadiendo la propiedad ***exitMessages***.
```javascript
  const messagesObj = {
    ...
   exitMessages: [
        ['es', ['Muy bien, hasta la próxima!', 'Hasta otra, espero haberte ayudado!', 'Nos vemos!', 'Ok, pasa un buen día!']],
        ['en', ['Good bye!!!']]
    ],
    ...
  };
  ```googleAssistantProxy.messages
> Asumo que si el bot envía un mensaje de tipo *googleAssistantProxy.messages* no hace mandar automáticamente ningún mensaje de despedia (se supone que el bot lo envia el solito). 

