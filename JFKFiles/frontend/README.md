# JFK Files

Azure Search demo based on declassified JFK Files.

## To get started

1. Install Nodejs.
2. Download this repo.
3. Open the command line of your choice, cd to the root directory of this repo on your machine.
4. Download and install third partie packages, Execute from the command prompt:
   > Note: if you are not starting from a clean cut, check if there is an existing `node_modules` folder and remove it's content.
   
```cmd
  npm install
```
5. Start the application:
   
```cmd
   npm start
```

   You can open a browser and navigate to the following route:

  ```cmd
  http://localhost:8082
  ```

  ## Deploy

  If you want to deploy this website you only need to run

```cmd
  npm run build:prod
```

 And copy the content generated from the _dist_ folder into your web server.

 > This process can be easily automated and enclosed on a CD enviroment.