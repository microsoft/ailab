import io
import os
from azure.storage.blob import BlockBlobService, PublicAccess
from services.blob_service import BlobService
from settings import keys


def upload_model(model_name, path='models'):
    filepath = '../' + path + '/' + model_name
    blob_service = BlobService(
        keys.TRAINED_MODELS_STORAGE['account_name'] , keys.TRAINED_MODELS_STORAGE['key'])
    blob_service.block_service.create_blob_from_path(
        keys.TRAINED_MODELS_STORAGE['containername'], path + '/' + model_name, filepath)
        

def download_model(model_name,path='models'):
    if not os.path.exists('models'):
        os.mkdir('models')
    filepath = model_name
    if not os.path.exists(filepath):
        blob_service = BlobService(
            keys.TRAINED_MODELS_STORAGE['account_name'], keys.TRAINED_MODELS_STORAGE['key'])
        blob_service.block_service.get_blob_to_path(
            keys.TRAINED_MODELS_STORAGE['containername'], model_name, filepath)


def list_blob_files(folder):
    blob_service = BlobService(
        keys.TRAINED_MODELS_STORAGE['account_name'], keys.TRAINED_MODELS_STORAGE['key'])
    generator = blob_service.block_service.list_blobs(
        keys.TRAINED_MODELS_STORAGE['containername'], folder + '/', delimiter='/')
    list_blob_names = []
    for blob in generator:
        list_blob_names.append(blob.name)
    return list_blob_names