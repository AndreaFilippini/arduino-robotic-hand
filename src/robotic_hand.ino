#include <Servo.h>

// constant for general element count (motors, sensors)
#define n_motors 5
#define n_sensors 5

// array of servos
Servo motors[n_motors];

// Pairs of values representing the max and min values of the bending sensor range
int array_val[n_motors << 1];

// Boolean to indicate whether the sensor calibration phase has occurred
boolean set = false;

// Boolean to indicate whether a request for communication with the serial port has occurred
boolean sequence = false;

// Pins index definition for motors, the buzzer, the button with which to start the calibration,
// the led used to indicate the operational status and the base pin index from which to read the bending sensors
const int BASE_MOTORS = 2;
const int BUZZER = 7;
const int BUTTON = 8;
const int ANALOG = 14;
const int LED = 13;

void setup()
{
	Serial.begin(9600);
	
	// Initialization of bending sensors input pins
	for (i=0; i < n_sensors; i++)
		pinMode((ANALOG + i), INPUT);
	  
	// Initialization of motors pins, also setting their initial position
	for (i=0; i < n_motors; i++){
		motors[i].attach(BASE_MOTORS + i);
		motors[i].write(180);
	}
	
	// Initialization of the pin type of the remaining elements
	pinMode(BUTTON, INPUT);                                                     //pin del bottone come porta di INPUT
	pinMode(BUZZER, OUTPUT);                                                    //pin del buzzer come porta di output
	pinMode(LED, OUTPUT); 
}

void loop(){

	// if communication has not been established and there has been no calibration phase
	if (!sequence && !set){
		// if the communication request byte is received, start the process
		if (Serial.available() && Serial.parseInt() == 255){
			digitalWrite(LED, HIGH);
			sequence = true;
		}
	}

	// if communication has not been established
	if (!sequence){
		// if the button to start calibration is pressed
		if (digitalRead(BUTTON)==HIGH){
			// wait for the button to be released
			while(digitalRead(BUTTON)==HIGH) delay(50);
			
			// the calibration process consists of two stages: a first stage of reading the closed hand values
			// and a second stage with the open hand values
			for (int k = 0; k < 2; k++){
				// define the beginning the two phases with the sound of a buzzer and wait two seconds
				BuzzerSound();
				delay(2000);
				
				// read the values for all the sensors
				for (int j = 0; j < n_sensors; j++){
					// the full index is defined as the current value of j multiplied by two, as each sensor is associated with two values,
					// while the outer index k defines whether it is reading the minimum or maximum value from the sensors
					int index = (j << 1) + k;
					// read 8 consecutive values for each sensor to have a more consistent data set
					for (int i = 0; i < 8; i++)
						array_val[index] += analogRead(ANALOG + j);
					// average the values to get more accurate range limit values
					array_val[index] >>= 3;
				}
			}
			// once the calibration phase is finished, set the corresponding flag and make the buzzer sound twice
			for (i = 0; i < 2; i++) BuzzerSound();
			set = true;
		}
		// if the sensors have already been calibrated
		if (set){
			for (int j = 0; j < n_motors; j++){
				int index = n_motors - j - 1;
				int val_mot = analogRead(ANALOG + index);
				
				// Filtering operation for sensors values that exceed the range defined during calibration
				if (val_mot > array_val[index << 1])
					val_mot = array_val[index << 1];
				if (val_mot < array_val[index << 1) + 1])
					val_mot = array_val[index << 1) + 1];
				
				//  map the sensor value read between 0 and 179 degrees and move the servomotor proportionally
				motors[j].write(map(val_mot, array_val[(index << 1) + 1], array_val[index << 1], 0, 179));
			}
		}
	}else{
		// if the communication has been started, read from the serial port the value
		int byteread = Serial.parseInt();
		// comunication format:
		// [3-bit unsed][1-bit bending 1/ distension 0][3-bit motor index][1-bit instruction type]
		// [1-bit unsed][6-bit milliseoncds wait multiplied by 100       ][1-bit instruction type]
		// if the last bit is 1, it means that it is an struction to move a motor
		if ((byteread & 1) == 1){
			motors[(byteread & 14) >> 1].write((byteread >> 4) * 179);
		}else{
			// otherwise it is a wait operation, imposing a propotional pause to the value obtained
			delay((byteread >> 1) * 100);
		}
		// send the value of execution end of the current operation
		Serial.println(254);
	}
}

// function to make the buzzer sound
void BuzzerSound(){
	digitalWrite(BUZZER, HIGH);
	delay(250);
	digitalWrite(BUZZER, LOW);
	delay(250);
}
