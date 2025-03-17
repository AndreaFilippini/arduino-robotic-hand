# Robotic hand prototype with Arduino and VB.NET
Robotic hand prototype based on Arduino controllable with a glove, mirroring hand movements, or automating hand movements with a visual basic (VB.NET) application

# Dependencies
[Arduino](https://www.arduino.cc/)

[VB.NET](https://learn.microsoft.com/it-it/dotnet/visual-basic/) (Only if it is necessary to modify the VB.NET application for automizing a sequence of actions to be applied to the hand)

# Functioning

Once the glove is on, the sensor calibration phase can start by pressing the corresponding button on the board.
The beginning of the process will be defined by the sound of a buzzer, after which the user must hold the hand open until the next buzzer sound.
After that, the hand will must be closed in order to calibrate both positions and define the range of possible values of each bending sensor.
Next, depending on the value read from each sensor, the movement of the servo motors' heads, which will each be attached to a finger of the robotic hand, will be mapped in degrees and executed accordingly.
Alternatively, it's possible to connect arduino via USB port to a PC and use an application written in VB.NET to determine a sequence of movements for each finger.

# Electrical Schematics

The project involves the use of five 2.2-inch bending sensors, each above each finger of the glove to determine the bending of each finger of the hand, and five SG90 Servomotor motors to move the fingers of the robotic hand proportionally.
Each bending sensor is placed inside a voltage divider with 10kohm resistors in order to read their value through arduino's analog inputs, **A0** to **A5**.
The motors are externally powered via a USB port, which can be excluded at any time via a physical switch.
A button is included on the board with which to interface with arduino to determine the start of the calibration phase of the bending sensors.
Finally, a buzzer is used to alert the user of the start and end of the sensor calibration phase.

<img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/Schemes/electric_c.png" width="300" height="300"><img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/Schemes/electric_1.png" width="300" height="300"><img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/Schemes/electric_2.png" width="300" height="300">

Images of the schematics of the physical boards are available in the “**src/Schemes**” folder.

# VB.NET Application

<img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/VB/hand_vb.png" width="400" height="300">

The communication between VB.NET and Arduino can be established by selecting the port on which the device is connected and pressing the “connection to the port” button.
The application provides a grid in which each box can contain an action.
The action can be entered with the “save instruction” button after filling in the various input fields on the right side.
Once all actions needed are defined, it's possible to execute the sequence of operations using the “execute instructions” button.
The source code is available in the “**src/VB/Form1.vb**” folder, while the “**.exe**” application is available in the same folder.

# VB.NET and Arduino communication protocol

Arduino communicates with the VB.NET application with single bytes (8-bit each).
The communication process begins by sending an **0xFF** byte from the VB.NET application to the Arduino.
The latter will then remain listening for subsequent commands.
There are two types of commands that can be set within the application: a motor action and a pause action.
The first action allows specifying an action, flexing or stretching, of a specific motor, while the second defines a pause expressed in milliseconds between **0.1** and **5s**, between individual operations in the sequence.
The action of the motor follows the following format:
<div align="center">
[3-bit unused][1-bit bending/distension][3-bit motor index][1-bit instruction type]
</div></br>

The pause action, on the other hand, follows the following format:
<div align="center">
[1-bit unused][6-bit pause value multiplied by 100][1-bit instruction type]
</div></br>

The discrimination between the two types of instructions is done by checking the value of the last bit, precisely 1 for the motor action and 0 for the pause action, while the corresponding values for action execution will be extracted from the other byte fields described above.

# Result
<img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/Results/hand.jpg" width="200" height="400"><img src="https://github.com/AndreaFilippini/arduino-robotic-hand/blob/main/src/images/Results/glove.jpg" width="200" height="200">
