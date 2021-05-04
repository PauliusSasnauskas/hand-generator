import socket               

s = socket.socket()         
s.connect(('localhost', 8052))
s.sendall(b'-59 9 23 -63 24 -44')
s.close()