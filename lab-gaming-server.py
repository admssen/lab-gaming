import socket

ip="192.168.0.22"

with open("wordle_en.txt", 'r', encoding='utf-8') as f:
    array_en = [line[0:5] for line in f]
en_power = len(array_en)

print(f"LOADED EN, {en_power} WORDS")

with open("wordle_es.txt", 'r', encoding='utf-8') as f:
    array_es = [line[0:5] for line in f]
es_power = len(array_es)

print(f"LOADED ES, {es_power} WORDS")

with open("wordle_ru.txt", 'r', encoding='utf-8') as f:
    array_ru = [line[0:5] for line in f]
ru_power = len(array_ru)

print(f"LOADED RU, {ru_power} WORDS")

def generate_response(message):
    marr = message.split('+');
    
    if marr[0] == "ping":
        return "ECHO"
    
    elif marr[0] == "wordleindex":
        if marr[1] == "en":
            return array_en[int(marr[2])%en_power]
        elif marr[1] == "es":
            return array_es[int(marr[2])%es_power]
        elif marr[1] == "ru":
            return array_ru[int(marr[2])%ru_power]
            
    elif marr[0] == 'wordleword':
        if marr[1] == "en":
            if marr[2] in array_en:
                return "TRUE"
            else:
                return "FAIL"
        elif marr[1] == "es":
            if marr[2] in array_es:
                return "TRUE"
            else:
                return "FAIL"
        elif marr[1] == "ru":
            if marr[2] in array_ru:
                return "TRUE"
            else:
                return "FAIL"

def start_server(host=ip, port=65432):

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((host, port))
        s.listen()
        print(f"LISTENING: {host}:{port}")

        while True:
            conn, addr = s.accept()
            with conn:
                print(f"\nCONNECTION: {addr}")
                while True:
                    s.settimeout(4)
                    data = conn.recv(1024)
                    s.settimeout(None)
                    if not data:
                        break
                    
                    message = data.decode('utf-8')
                    print(f"REQUEST: {message}")
                    
                    response = generate_response(message)
                    print(f"RESPONSE: {response}")
                    conn.sendall(response.encode('utf-8'))

start_server()
