from services.azureml_service import AzureMlService

if __name__ == '__main__':
    
    config_json_path = 'config_deploy/azureml_config.json'
    image = AzureMlService(config_json_path).create_or_get_image()
    print(image)