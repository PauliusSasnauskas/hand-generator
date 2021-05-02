import socket               

s = socket.socket()         
s.connect(('localhost', 8052))
s.sendall(b'120 120 98 98')
s.close()     