from azure.storage.blob import BlockBlobService
from settings import keys


class BlobService():
    __instance = None

    def __new__(cls, account_name,account_key):
        if BlobService.__instance is None:
            BlobService.__instance = object.__new__(cls)
        return BlobService.__instance

    def __init__(self, account_name, account_key):
        if not 'block_service' in dir(self):
            self.block_service = BlockBlobService(account_name,account_key)
