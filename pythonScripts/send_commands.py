import socket               
import tinyik
import numpy as np


def get_degree_from_position(position=[3.3, 4.8, -.7]):
    # Hardcoded
    arm = tinyik.Actuator([[0, 0.45, 0], 'y',[.6, 0, 0], 'x', [0, 1.4, 0], 'y', [.6, 0, 0], 'x', [0, 1.2, 0], 'y', [.3, 0, 0], 'x', [0, .7, 0] ])


    position = np.array(position) / 2
    arm.ee = position
    
    tinyik.visualize(arm)

    return np.round(np.rad2deg(arm.angles))


get_degree_from_position()

#s = socket.socket()  
#s.connect(('localhost', 8052))

# get degrees from position
degrees = get_degree_from_position([3.3, 4.8, -.7])
degrees = [ str(x) for x in degrees ]
degrees = " ".join(degrees)
degrees = bytes(degrees, 'utf-8') # b"-59 9 23 -63 24 -44"

print(degrees)


#s.send(degrees)
#s.close()

x = input()
x = input()
