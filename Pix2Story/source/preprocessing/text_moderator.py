import http.client, urllib.request, urllib.parse, urllib.error, base64, json

def text_moderator(text):

    headers = {
        # Request headers
        'Content-Type': 'text/plain',
        'Ocp-Apim-Subscription-Key': 'subscription_key',
    }

    params = urllib.parse.urlencode({
        # Request parameters
        'autocorrect': True,
        'PII': False,
        'classify': 'True'
    })

    try:
        conn = http.client.HTTPSConnection('eastus.api.cognitive.microsoft.com')
        conn.request("POST", "/contentmoderator/moderate/v1.0/ProcessText/Screen?%s" % params, text, headers)
        response = conn.getresponse()
        data = response.read()
        conn.close()
        
    except Exception as e:
        print("[Errno {0}] {1}".format(e.errno, e.strerror))

    d = data.decode('utf-8')
    data = json.loads(d)
    if 'Classification' in data:   
        out = data['Classification']['ReviewRecommended']
    else:
        out = False
    return out