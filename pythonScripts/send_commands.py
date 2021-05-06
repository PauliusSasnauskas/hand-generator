import socket
import tinyik
import numpy as np

arm = None

def get_degree_from_position(position):
    global arm
    arm = tinyik.Actuator([ [0, 0.4, 0], 'y', [0, 0, -0.55], 'z', [2.8, 0, 0], [0, 0, 0.55], 'z', [-2.25, 0, 0], 'x', [0, 0, 0.55], 'z', [1.55, 0, 0] ])
    print("ee at:", arm.ee)
    arm.ee = np.array(position) / 2
    # tinyik.visualize(arm)
    return np.round(np.rad2deg(arm.angles))

def degrees_to_bytestring(degrees):
    degrees = " ".join([ str(x) for x in degrees ])
    return bytes(degrees, 'utf-8')

def send_command(strCmd):
    s = socket.socket()
    s.connect(('localhost', 8052))
    s.send(strCmd)
    s.close()


if __name__ == "__main__":
    eePos = input("Input position of end effector (X Y Z): ")
    targetCommand = bytes("target " + eePos, 'utf-8')

    eePos = list(map(float, eePos.split(" ")))

    degrees = get_degree_from_position(eePos)
    print("IK solver degrees:", degrees)

    degreeCommand = degrees_to_bytestring(degrees)

    print("Sending command:", degreeCommand)
    print("Sending command:", targetCommand)
    send_command(degreeCommand)
    send_command(targetCommand)

    tinyik.visualize(arm)