import socket               

s = socket.socket()         
s.connect(('localhost', 8052))
s.sendall(b'Here I am!')
s.close()     