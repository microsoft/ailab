# Otros settings
> [here for English document](otherSettings.md)

## Puerto
Puerto en el que escucha nuestro proxy (**1337 por defecto**)
```javascript
process.env.PORT = process.env.PORT || 1337;
```

## Nivel de trazas
Tipo de trazas que se nuestran por consola (**LOG por defecto**). 

Posibles valores: ***VERBOSE, LOG, WARNING, ERROR***
```javascript
process.env.TRACE_LEVEL = process.env.TRACE_LEVEL || 'LOG';
```

## *Endpoint* de *Direct Line*
Endpoint al que accede a tu bot vía Direct Line. (**directline.botframework.com por defecto**).
- ***directline.botframework.com*** routes your client to the nearest datacenter. This is the best option if you do not know where your client is located.
- ***asia.directline.botframework.com*** routes only to Direct Line servers in Eastern Asia.
- ***europe.directline.botframework.com*** routes only to Direct Line servers in Europe.
 - ***northamerica.directline.botframework.com*** routes only to Direct Line servers in North America.
```javascript
process.env.DIRECTLINE_ENDPOINT = process.env.DIRECTLINE_ENDPOINT || 'directline.botframework.com';
```

## Intérvalo de *polling*
Cada cuento tiempo se comprueban y procesan nuevos mensajes (**500ms por defecto**)
```javascript
process.env.POLLING_INTERVAL = process.env.POLLING_INTERVAL || 500;
```

## Timeout de conversación
Cuanto tiempo tiene que pasar para dar por cerrada una conversación que no ha tenido actividad  (**10 min. por defecto**).
```javascript
process.env.CONVERSATION_TIMEOUT = process.env.CONVERSATION_TIMEOUT || 10 * 60;
```

## *InstrumentationKey* de *AppInsights*
Si quieres trazar *errores* y *warnings* en *AppInsights*
```javascript
process.env.APPINSIGHTS_INSTRUMENTATIONKEY  = YOURKEY;
```

> Nota: todos estos *settings* se deberian configurar directamente en los *settings* del *App Service* tal como explica [aquí](Azure.settings.es.md).
> 
> Por Ejemplo, para configurar el nivel *verbose* las trazas de la consola, sería incluir el los *settings*  ***TRACE_LEVEL*** con el valor ***VERBOSE***
