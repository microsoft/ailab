"""
Configuration for the generate module
"""

#-----------------------------------------------------------------------------#
# Flags for running on CPU
#-----------------------------------------------------------------------------#
FLAG_CPU_MODE = False

#-----------------------------------------------------------------------------#
# Paths to models and biases
#-----------------------------------------------------------------------------#
paths = dict()
settings = dict()

# Skip-thoughts
paths['skmodels'] = '../models/skvmodel_adventure.npz'
paths['sktables'] = '../models/'

# Decoder
paths['decmodel'] = '../models/decmodel.npz'
paths['dictionary'] ='../models/dict.pkl'

# Image-sentence embedding
paths['vsemodel'] ='../models/coco_embedding.npz'

# VGG-19 convnet
paths['vgg'] ='../models/vgg_weights.npy'
paths['pycaffe'] = 'models/python'
paths['vgg_proto_caffe'] = 'models/VGG_ILSVRC_19_layers_deploy.prototxt'
paths['vgg_model_caffe'] = 'models/VGG_ILSVRC_19_layers.caffemodel'


# COCO training captions
paths['captions'] = '../models/coco_train_caps.txt'

# Biases
paths['negbias'] = '../models/caption_style.npy'
paths['posbias'] = '../models/adventure_style.npy'

#Processed books
paths['text'] = '../models/text.pkl'

#Google news vectors
paths['v_expansion'] = '../models/GoogleNews-vectors-negative300.bin'

#Books
paths['books'] = '../books/adventure/*.txt'

# Train settings
settings['decoder'] = {'dimctx' : 4800,'dim_word' : 620, 'dim' : 2400, 'encoder' : 'gru', 'decoder' : 'gru',
        'doutput' : False, 'max_epochs' : 1, 'disp_freq' : 1, 'decay_c' : 0., 'grad_clip' : 5., 'n_words' : 20000, 
        'maxlen_w' : 200, 'optimizer' : 'adam', 'batch_size' : 1, 'saveto' : paths['decmodel'], 
        'dictionary' : paths['dictionary'], 'embeddings' : None, 'save_freq' : 200, 'sample_freq' : 20, 'reload_' : True}

settings['encoder'] = {'dim_word' : 620, 'dim' : 4800, 'encoder' : 'gru', 'decoder' : 'gru',
        'max_epochs' : 1, 'disp_freq' : 1, 'decay_c' : 0., 'grad_clip' : 5., 'n_words' : 20000, 
        'maxlen_w' : 30, 'optimizer' : 'adam', 'batch_size' : 1, 'saveto' : paths['skmodels'], 
        'dictionary' : paths['dictionary'], 'save_freq' : 200, 'reload_' : True}
