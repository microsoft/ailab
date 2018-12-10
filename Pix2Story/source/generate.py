"""
Story generation
"""
from generation import skipthoughts
from generation import decoder
from generation import embedding
import io, cv2, base64
import pickle as pkl
import numpy, copy, sys, skimage.transform
import config
import nltk
nltk.download('punkt')
import lasagne
from lasagne.layers import InputLayer, DenseLayer, NonlinearityLayer, DropoutLayer, MaxPool2DLayer as PoolLayer 
from lasagne.nonlinearities import softmax
from lasagne.utils import floatX
if not config.FLAG_CPU_MODE:
    from lasagne.layers.corrmm import Conv2DMMLayer as ConvLayer

from scipy import optimize, stats
from collections import OrderedDict, defaultdict, Counter
from numpy.random import RandomState
from scipy.linalg import norm
from PIL import Image, ImageFile
ImageFile.LOAD_TRUNCATED_IMAGES = True

class StoryGenerator(object):
    def __init__(self):
      self.models = load_all()

    def story(self, image_data=None, image_loc=None, k=100, bw=1, lyric=False):
        if image_loc is not None:
        # Load the image
            rawim, im = load_image(file_name=image_loc)
        else:
            rawim,im = load_image(image64=image_data)
        # Run image through convnet
        feats = compute_features(self.models['net'], im).flatten()
        feats /= norm(feats)
        # Embed image into joint space
        feats = embedding.encode_images(self.models['vse'], feats[None,:])
        # Compute the nearest neighbours
        scores = numpy.dot(feats, self.models['cvec'].T).flatten()
        sorted_args = numpy.argsort(scores)[::-1]
        sentences = [self.models['cap'][a] for a in sorted_args[:k]]
        # Compute skip-thought vectors for sentences
        svecs = skipthoughts.encode(self.models['stv'], sentences, verbose=False)
        # Style shifting
        shift = svecs.mean(0) - self.models['bneg'] + self.models['bpos']
        # Generate story conditioned on shift
        passage = decoder.run_sampler(self,image_data,image_loc,self.models['dec'], shift, beam_width=bw)
        return passage
        

def load_all():
    """
    Load everything we need for generating
    """
    print (config.paths['decmodel'])

    # Skip-thoughts
    print ('Loading skip-thoughts...')
    stv = skipthoughts.load_model(config.paths['skmodels'],
                                  config.paths['sktables'])

    # Decoder
    print('Loading decoder...')
    dec = decoder.load_model(config.paths['decmodel'],
                             config.paths['dictionary'])

    # Image-sentence embedding
    print ('Loading image-sentence embedding...')
    print(config.paths['vsemodel'])
    vse = embedding.load_model(config.paths['vsemodel'])

    # VGG-19
    print ('Loading and initializing ConvNet...')

    if config.FLAG_CPU_MODE:
        sys.path.insert(0, config.paths['pycaffe'])
        import caffe
        caffe.set_mode_cpu()
        net = caffe.Net(config.paths['vgg_proto_caffe'],
                        config.paths['vgg_model_caffe'],
                        caffe.TEST)
    else:
        net = build_convnet(config.paths['vgg'])

    # Captions
    print ('Loading captions...')
    cap = []
    with open(config.paths['captions'], 'rb') as f:
        for line in f:
            cap.append(line.strip().decode("utf-8"))

    # Caption embeddings
    print ('Embedding captions...')
    cvec = embedding.encode_sentences(vse, cap, verbose=False)

    # Biases
    print ('Loading biases...')
    bneg = numpy.load(config.paths['negbias'],encoding='latin1')
    bpos = numpy.load(config.paths['posbias'],encoding='latin1')

    # Pack up
    z = {}
    z['stv'] = stv
    z['dec'] = dec
    z['vse'] = vse
    z['net'] = net
    z['cap'] = cap
    z['cvec'] = cvec
    z['bneg'] = bneg
    z['bpos'] = bpos
    
    return z

def base64_image(base64str):
    base64img = base64str.encode('utf-8')
    r = base64.decodestring(base64img)
    numpy_buffer = numpy.frombuffer(r, dtype=numpy.uint8)
    img = cv2.imdecode(numpy_buffer, cv2.IMREAD_COLOR)
    rgb_img = cv2.cvtColor(img, cv2.COLOR_RGB2BGR)
    return rgb_img 

def load_image(file_name=None,image64=None):
    """
    Load and preprocess an image
    """
    MEAN_VALUE = numpy.array([103.939, 116.779, 123.68]).reshape((3,1,1))
    if file_name != None:
        image = Image.open(file_name)
        im = numpy.array(image)
    else:
        im = base64_image(image64)
    # Resize so smallest dim = 256, preserving aspect ratio
    if len(im.shape) == 2:
        im = im[:, :, numpy.newaxis]
        im = numpy.repeat(im, 3, axis=2)
    h, w, _ = im.shape
    if h < w:
        im = skimage.transform.resize(im, (256, int(w*256/h)), preserve_range=True)
    else:
        im = skimage.transform.resize(im, (int(h*256/w), 256), preserve_range=True)
    # Central crop to 224x224
    h, w, _ = im.shape   
    im = im[h//2-112:h//2+112, w//2-112:w//2+112]
    print(im.shape)
    rawim = numpy.copy(im).astype('int') 
    # Shuffle axes to c01
    im = numpy.swapaxes(numpy.swapaxes(im, 1, 2), 0, 1)
    # Convert to BGR
    im = im[::-1, :, :]
    im = im - MEAN_VALUE
    return rawim, floatX(im[numpy.newaxis])

def compute_features(net, im):
    """
    Compute fc7 features for im
    """
    if config.FLAG_CPU_MODE:
        net.blobs['data'].reshape(* im.shape)
        net.blobs['data'].data[...] = im
        net.forward()
        fc7 = net.blobs['fc7'].data
    else:
        fc7 = numpy.array(lasagne.layers.get_output(net['fc7'], im,
                                                    deterministic=True).eval())
    return fc7

def build_convnet(path_to_vgg):
    """
    Construct VGG-19 convnet
    """
    net = {}
    net['input'] = InputLayer((None, 3, 224, 224))
    net['conv1_1'] = ConvLayer(net['input'], 64, 3, pad=1)
    net['conv1_2'] = ConvLayer(net['conv1_1'], 64, 3, pad=1)
    net['pool1'] = PoolLayer(net['conv1_2'], 2)
    net['conv2_1'] = ConvLayer(net['pool1'], 128, 3, pad=1)
    net['conv2_2'] = ConvLayer(net['conv2_1'], 128, 3, pad=1)
    net['pool2'] = PoolLayer(net['conv2_2'], 2)
    net['conv3_1'] = ConvLayer(net['pool2'], 256, 3, pad=1)
    net['conv3_2'] = ConvLayer(net['conv3_1'], 256, 3, pad=1)
    net['conv3_3'] = ConvLayer(net['conv3_2'], 256, 3, pad=1)
    net['conv3_4'] = ConvLayer(net['conv3_3'], 256, 3, pad=1)
    net['pool3'] = PoolLayer(net['conv3_4'], 2)
    net['conv4_1'] = ConvLayer(net['pool3'], 512, 3, pad=1)
    net['conv4_2'] = ConvLayer(net['conv4_1'], 512, 3, pad=1)
    net['conv4_3'] = ConvLayer(net['conv4_2'], 512, 3, pad=1)
    net['conv4_4'] = ConvLayer(net['conv4_3'], 512, 3, pad=1)
    net['pool4'] = PoolLayer(net['conv4_4'], 2)
    net['conv5_1'] = ConvLayer(net['pool4'], 512, 3, pad=1)
    net['conv5_2'] = ConvLayer(net['conv5_1'], 512, 3, pad=1)
    net['conv5_3'] = ConvLayer(net['conv5_2'], 512, 3, pad=1)
    net['conv5_4'] = ConvLayer(net['conv5_3'], 512, 3, pad=1)
    net['pool5'] = PoolLayer(net['conv5_4'], 2)
    net['fc6'] = DenseLayer(net['pool5'], num_units=4096)
    net['fc7'] = DenseLayer(net['fc6'], num_units=4096)
    net['fc8'] = DenseLayer(net['fc7'], num_units=1000, nonlinearity=None)
    net['prob'] = NonlinearityLayer(net['fc8'], softmax)
    print ('Loading parameters...')
    output_layer = net['prob']
    a=numpy.load(path_to_vgg,encoding='latin1')
    lasagne.layers.set_all_param_values(output_layer,a.tolist()) 
    return net





