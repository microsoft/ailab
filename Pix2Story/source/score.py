import os, json, base64
import generate 

def init():
    global st_gen
    st_gen = generate.StoryGenerator()

def run(input_df):
    data = json.loads(input_df)
    text_base64 = data['data']
    bw = data['bw'] if 'bw' in data else 1
    prediction = st_gen.story(image_data=text_base64, bw=bw)
    return json.dumps({'prediction': prediction})