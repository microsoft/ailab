@echo off
"%CD%/../exec/gactions" update --project bot-demo --action_package "%CD%/GoogleAction/action.es.json" --action_package "%CD%/GoogleAction/action.en.json"
pause