import numpy
import copy

class HomogeneousData():

    def __init__(self, data, batch_size=128, maxlen=None):
        self.data = data
        self.batch_size = batch_size
        self.maxlen = maxlen

        self.prepare()
        self.reset()

    def prepare(self):
        self.caps = self.data[0]
        self.feats = self.data[1]
        self.feats2 = self.data[2]

        # find the unique lengths
        self.lengths = [len(cc.split()) for cc in self.caps]
        self.len_unique = numpy.unique(self.lengths)
        # remove any overly long sentences
        if self.maxlen:
            self.len_unique = [ll for ll in self.len_unique if ll <= self.maxlen]

        # indices of unique lengths
        self.len_indices = dict()
        self.len_counts = dict()
        for ll in self.len_unique:
            self.len_indices[ll] = numpy.where(self.lengths == ll)[0]
            self.len_counts[ll] = len(self.len_indices[ll])

        # current counter
        self.len_curr_counts = copy.copy(self.len_counts)

    def reset(self):
        self.len_curr_counts = copy.copy(self.len_counts)
        self.len_unique = numpy.random.permutation(self.len_unique)
        self.len_indices_pos = dict()
        for ll in self.len_unique:
            self.len_indices_pos[ll] = 0
            self.len_indices[ll] = numpy.random.permutation(self.len_indices[ll])
        self.len_idx = -1

    def __next__(self):
        count = 0
        while True:
            self.len_idx = numpy.mod(self.len_idx+1, len(self.len_unique))
            if self.len_curr_counts[self.len_unique[self.len_idx]] > 0:
                break
            count += 1
            if count >= len(self.len_unique):
                break
        if count >= len(self.len_unique):
            self.reset()
            raise StopIteration()

        # get the batch size
        curr_batch_size = numpy.minimum(self.batch_size, self.len_curr_counts[self.len_unique[self.len_idx]])
        curr_pos = self.len_indices_pos[self.len_unique[self.len_idx]]
        # get the indices for the current batch
        curr_indices = self.len_indices[self.len_unique[self.len_idx]][curr_pos:curr_pos+curr_batch_size]
        self.len_indices_pos[self.len_unique[self.len_idx]] += curr_batch_size
        self.len_curr_counts[self.len_unique[self.len_idx]] -= curr_batch_size

        # 'feats' corresponds to the after and before sentences
        caps = [self.caps[ii] for ii in curr_indices]
        feats = [self.feats[ii] for ii in curr_indices]
        feats2 = [self.feats2[ii] for ii in curr_indices]

        return caps, feats, feats2

    def __iter__(self):
        return self

def prepare_data(seqs_x, seqs_y, seqs_z, worddict, maxlen=None, n_words=20000):
    """
    Put the data into format useable by the model
    """
    seqs_x, seqs_y, seqs_z = fill_seqs(seqs_x, seqs_y, seqs_z, worddict, n_words)
    
    lengths_x = [len(s) for s in seqs_x]
    lengths_y = [len(s) for s in seqs_y]
    lengths_z = [len(s) for s in seqs_z]

    if maxlen != None:
        new_seqs_x = []
        new_seqs_y = []
        new_seqs_z = []
        new_lengths_x = []
        new_lengths_y = []
        new_lengths_z = []
        for l_x, s_x, l_y, s_y, l_z, s_z in zip(lengths_x, seqs_x, lengths_y, seqs_y, lengths_z, seqs_z):
            if l_x < maxlen and l_y < maxlen and l_z < maxlen:
                new_seqs_x.append(s_x)
                new_lengths_x.append(l_x)
                new_seqs_y.append(s_y)
                new_lengths_y.append(l_y)
                new_seqs_z.append(s_z)
                new_lengths_z.append(l_z)
        lengths_x = new_lengths_x
        seqs_x = new_seqs_x
        lengths_y = new_lengths_y
        seqs_y = new_seqs_y
        lengths_z = new_lengths_z
        seqs_z = new_seqs_z

        if len(lengths_x) < 1 or len(lengths_y) < 1 or len(lengths_z) < 1:
            return None, None, None, None, None, None

    n_samples = len(seqs_x)
    maxlen_x = numpy.max(lengths_x) + 1
    maxlen_y = numpy.max(lengths_y) + 1
    maxlen_z = numpy.max(lengths_z) + 1

    x = numpy.zeros((maxlen_x, n_samples)).astype('int64')
    y = numpy.zeros((maxlen_y, n_samples)).astype('int64')
    z = numpy.zeros((maxlen_z, n_samples)).astype('int64')
    x_mask = numpy.zeros((maxlen_x, n_samples)).astype('float32')
    y_mask = numpy.zeros((maxlen_y, n_samples)).astype('float32')
    z_mask = numpy.zeros((maxlen_z, n_samples)).astype('float32')
    for idx, [s_x, s_y, s_z] in enumerate(zip(seqs_x,seqs_y,seqs_z)):
        x[:lengths_x[idx],idx] = s_x
        x_mask[:lengths_x[idx]+1,idx] = 1.
        y[:lengths_y[idx],idx] = s_y
        y_mask[:lengths_y[idx]+1,idx] = 1.
        z[:lengths_z[idx],idx] = s_z
        z_mask[:lengths_z[idx]+1,idx] = 1.

    return x, x_mask, y, y_mask, z, z_mask

def grouper(text):
    """
    Group text into triplets
    """
    source = text[1:][:-1]
    forward = text[2:]
    backward = text[:-2]
    X = (source, forward, backward)
    return X


def fill_seqs(seqs_x, seqs_y, seqs_z, worddict, n_words):

    seqsx = []
    seqsy = []
    seqsz = []
    for cc in seqs_x:
        seqsx.append([worddict[w] if worddict[w] < n_words else 1 for w in cc.split()])
    for cc in seqs_y:
        seqsy.append([worddict[w] if worddict[w] < n_words else 1 for w in cc.split()])
    for cc in seqs_z:
        seqsz.append([worddict[w] if worddict[w] < n_words else 1 for w in cc.split()])
    seqs_x = seqsx
    seqs_y = seqsy
    seqs_z = seqsz
    return seqs_x, seqs_y, seqs_z