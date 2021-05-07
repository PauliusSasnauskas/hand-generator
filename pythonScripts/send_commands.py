import socket
import tinyik
import numpy as np

arm = tinyik.Actuator([ [0, 0.4, 0], 'y', [0, 0, -0.55], 'z', [2.8, 0, 0], [0, 0, 0.55], 'z', [-2.25, 0, 0], 'x', [0, 0, 0.55], 'z', [1.55, 0, 0] ])

def get_degree_from_position(position, verbose=False):
    global arm
    # arm = tinyik.Actuator([ [0, 0.4, 0], 'y', [0, 0, -0.55], 'z', [2.8, 0, 0], [0, 0, 0.55], 'z', [-2.25, 0, 0], 'x', [0, 0, 0.55], 'z', [1.55, 0, 0] ])
    arm.ee = np.array(position)
    if verbose: print("ee at:", arm.ee)
    return np.round(np.rad2deg(arm.angles))

def degrees_to_bytestring(degrees):
    degrees = " ".join([ str(x) for x in degrees ])
    return bytes(degrees, 'utf-8')

def send_command(strCmd):
    s = socket.socket()
    s.connect(('localhost', 8053))
    s.send(strCmd)
    s.close()

def ik(x, y, z, showVisualization=False, verbose=False):
    targetCommand = bytes(f"target {x} {y} {z}", 'utf-8')

    targetPos = [x, y, z]

    degrees = get_degree_from_position(targetPos, verbose)

    degreeCommand = degrees_to_bytestring(degrees)

    if verbose:
        print("IK solver degrees:", degrees)
        print("Sending command:", degreeCommand)
        print("Sending command:", targetCommand)
    send_command(degreeCommand)
    send_command(targetCommand)

    if showVisualization:
        tinyik.visualize(arm)

if __name__ == "__main__":
    eePos = input("Input position of end effector (X Y Z): ")
    targetPos = [list(map(float, eePos.split(" ")))]
    calc_and_send(target[0], target[1], target[2])

    