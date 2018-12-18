import io
import glob
import nltk
from nltk.tokenize import word_tokenize
nltk.download('punkt')

def read_data(path,min_len=50):
    files = glob.glob(path)
    tokens = []
    counter = 0
    for file in files:
        counter+=1
        print(counter)
        with io.open(file, "r", encoding='utf-8') as words_file:
            try:
                doc = words_file.read()
            except:
                print('cant decode byte')
                continue
            doc_list = doc.split('\n')
            doc_list = [x for x in doc_list if len(x)>min_len]
            tokens+=doc_list
    return tokens

def join_small_sents(text_list,min_sent_size=200):
    new_text = []
    buffer_sent = ''
    counter=0
    for sent in text_list:
        counter+=1
        print(counter)
        if len((buffer_sent + sent).split(' ')) < min_sent_size:
            buffer_sent += sent
        else:
            tokens = word_tokenize(buffer_sent + sent)
            result = ' ' + ' '.join(tokens)
            new_text.append(result)
            buffer_sent = ''
    return new_text

