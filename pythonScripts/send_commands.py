import socket               

s = socket.socket()         
s.connect(('localhost', 8052))
s.sendall(b'0 50 -50 50')
s.close()     