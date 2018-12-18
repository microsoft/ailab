import adal
from urllib.parse import urljoin
import http
from http import HTTPStatus
import json


class AzureModelManagement():

    BASE_URL = '{0}.modelmanagement.azureml.net'
    URL_SUB = '/api/subscriptions/{0}/resourceGroups/{1}/accounts/{2}/{3}/?api-version=2017-09-01-preview'

    def __init__(self, location, sub_info, auth_info):
        self.__auth_info = auth_info
        self.__sub_info = sub_info
        self.location = location
        self.url_model = AzureModelManagement.BASE_URL.format(location)
        self.connection = self.__init_connection()
        self.token = self.__get_auth_token()

    def __get_sub_url(self, api_method):
        return AzureModelManagement.URL_SUB.format(self.__sub_info['SUB_ID'],
                                                   self.__sub_info['RESOURCE'],
                                                   self.__sub_info['ACCOUNT_NAME'],
                                                   api_method)

    def __init_connection(self):
        return http.client.HTTPSConnection(self.url_model)

    def __get_auth_token(self):
        auth_parms = self.__auth_info
        url_auth = urljoin(auth_parms['auth_url'], auth_parms['tenant'])
        context = adal.AuthenticationContext(url_auth)
        token = context.acquire_token_with_client_credentials(
            auth_parms['resource'],
            auth_parms['appId'],
            auth_parms['key'])

        return token['accessToken']

    def __make_request(self, method, url, body=None):
        bearer = 'Bearer {0}'
        headers = {'content-type': 'application/json',
                   'authorization': bearer.format(self.token)}
        self.connection.request(method, url, body=body, headers=headers)
        response = self.connection.getresponse()
        if response.status == HTTPStatus.UNAUTHORIZED:
            self.token = self.__get_auth_token()
            headers['authorization'] = bearer.format(self.token)
            self.connection.request(method, url, body=body, headers=headers)
            response = self.connection.getresponse()

        return response

    def get_models_management(self):
        url_method = self.__get_sub_url('models')
        response = self.__make_request('GET', url_method)
        return response.read()

    def upload_model_management(self, model_info):

        url_method = self.__get_sub_url('models')
        json_data = json.dumps(model_info)
        response = self.__make_request('POST', url_method, body=json_data)
        return response.read()

    def __enter__(self):
        return self

    def __exit__(self, exception_type, exception_val, trace):
        self.connection.close()
