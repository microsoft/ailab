import os, re, requests, json
from azureml.core.model import Model
from azureml.core.webservice import Webservice, AksWebservice
from azureml.core.image import ContainerImage
from azureml.core import Workspace, Run
from azureml.core.conda_dependencies import CondaDependencies
from azureml.core.authentication import ServicePrincipalAuthentication
from azureml.core.compute import AksCompute, ComputeTarget
from helpers import filter_list
import glob
import json

class AzureMlService():

    def __init__(self, config_path):
        self.__config = json.loads(open(config_path).read())
        self.__ws = Workspace.from_config(path='config_deploy/config.json')
        print(self.__ws.name, self.__ws.location, self.__ws.resource_group, self.__ws.location, sep = '\t')

    def deployment(self):
        image = self.create_or_get_image()
        compute = self.__create_or_get_compute()
        self.__deploy_service(image, compute)

    def create_or_get_image(self):
        image_params = self.__config['image']
        images = ContainerImage.list(self.__ws,image_name=image_config['name'])
        image = images.find_by_property('version',image_config['version'])
        if image:
            return image

        image_config = ContainerImage.image_configuration(execution_script="score.py",
                                                    runtime="python",
                                                    conda_file="config_deploy/myenv.yml",
                                                    docker_file="config_deploy/Dockerfile",
                                                    enable_gpu=True,
                                                    dependencies=['generation', 'config.py', 'skipthoughts_vectors', 
                                                    'generate.py', 'preprocessing/text_moderator.py'])

        image = ContainerImage.create(name=image_params['name'],
                                    models = [],
                                    image_config = image_config,
                                    workspace = self.__ws)

        image.wait_for_creation(show_output = True)
        return image 


    def __create_or_get_compute(self):
        compute_config = self.__config['compute']
        compute_list = AksCompute.list(self.__ws)
        compute = compute_list.find_by_property('name',compute_config['name'])
        if compute:
            return compute
        prov_config = AksCompute.provisioning_configuration(agent_count=compute_config['agent_count'], 
            vm_size=compute_config['vm_size'], location=compute_config['location'])
        aks_name = compute_config['name']
        aks_target = ComputeTarget.create(workspace = self.__ws,
                                            name = aks_name,
                                            provisioning_configuration = prov_config)

        aks_target.wait_for_completion(show_output = True)
        print(aks_target.provisioning_state)
        print(aks_target.provisioning_errors)
        return aks_target

    def __deploy_service(self,image,compute):
        service_config = self.__config['deploy']
        services = AksWebservice.list(self.__ws)
        service = services.find_by_property('name',service_config['name'])
        if service:
            service.update(auth_enabled=service_config['auth'])
            service.wait_for_deployment(show_output = True)
            return service
        aks_config = AksWebservice.deploy_configuration(auth_enabled=True, max_request_wait_time=75000, 
            replica_max_concurrent_requests=100,autoscale_enabled=False,num_replicas=15)
        aks_service_name = service_config['name']
        aks_service = Webservice.deploy_from_image(workspace = self.__ws,
                                                    name = aks_service_name,
                                                    image = image,
                                                    deployment_config = aks_config,
                                                    deployment_target = compute)

        aks_service.wait_for_deployment(show_output = True)
        print(aks_service.state)
        return aks_service

  
