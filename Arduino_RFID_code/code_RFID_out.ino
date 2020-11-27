/**
 * ----------------------------------------------------------------------------
 * This is a MFRC522 library example; see https://github.com/miguelbalboa/rfid
 * for further details and other examples.
 * 
 * NOTE: The library file MFRC522.h has a lot of useful info. Please read it.
 * 
 * Released into the public domain.
 * ----------------------------------------------------------------------------
 * This sample shows how to read and write data blocks on a MIFARE Classic PICC
 * (= card/tag).
 * 
 * BEWARE: Data will be written to the PICC, in sector #1 (blocks #4 to #7).
 * 
 * 
 * Typical pin layout used:
 * -----------------------------------------------------------------------------------------
 *             MFRC522      Arduino       Arduino   Arduino    Arduino          Arduino
 *             Reader/PCD   Uno/101       Mega      Nano v3    Leonardo/Micro   Pro Micro
 * Signal      Pin          Pin           Pin       Pin        Pin              Pin
 * -----------------------------------------------------------------------------------------
 * RST/Reset   RST          9             5         D9         RESET/ICSP-5     RST
 * SPI SS      SDA(SS)      10            53        D10        10               10
 * SPI MOSI    MOSI         11 / ICSP-4   51        D11        ICSP-4           16
 * SPI MISO    MISO         12 / ICSP-1   50        D12        ICSP-1           14
 * SPI SCK     SCK          13 / ICSP-3   52        D13        ICSP-3           15
 * 
 */

#include <SPI.h>
#include <MFRC522.h>
#include <Servo.h>

#define RST_PIN         9           // Configurable, see typical pin layout above
#define SS_PIN          10          // Configurable, see typical pin layout above
Servo myservo_ra;
int BELL_PIN = 2;
int sv;

MFRC522 mfrc522(SS_PIN, RST_PIN);   // Create MFRC522 instance.

void setup() 
{
    pinMode(BELL_PIN, OUTPUT);
    Serial.begin(9600); // Initialize serial communications with the PC
    while (!Serial);    // Do nothing if no serial port is opened (added for Arduinos based on ATMEGA32U4)
    SPI.begin();        // Init SPI bus
    mfrc522.PCD_Init(); // Init MFRC522 card
    myservo_ra.attach(4);
    myservo_ra.write(0);
}
void loop() {
    // Look for new cards
    if ( ! mfrc522.PICC_IsNewCardPresent())
        return;
    // Select one of the cards
    if ( ! mfrc522.PICC_ReadCardSerial())
        return;
  //String content= "";
    // Show some details of the PICC (that is: the tag/card)

    
  /*    Serial.print("ci");
      Serial.print(mfrc522.uid.uidByte[0], HEX);
      Serial.print(mfrc522.uid.uidByte[1], HEX);
      Serial.print(mfrc522.uid.uidByte[2], HEX);
      Serial.println(mfrc522.uid.uidByte[3], HEX);
    //Serial.println(mfrc522.uid.uidByte[4], HEX);    
   
 /*   for ( byte i=0; i <mfrc522.uid.size; i++)
  {
    Serial.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
    Serial.print(mfrc522.uid.uidByte[i],HEX);
    content.concat(String(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " "));
    content.concat(String(mfrc522.uid.uidByte[i],HEX));
  }
    Serial.println(); */
  //  }
  //  else
  //  {
      Serial.print("co");
      Serial.print(mfrc522.uid.uidByte[0], HEX);
      Serial.print(mfrc522.uid.uidByte[1], HEX);
      Serial.print(mfrc522.uid.uidByte[2], HEX);
      Serial.println(mfrc522.uid.uidByte[3], HEX);
    //Serial.print(mfrc522.uid.uidByte[4], HEX);
 /*
    for ( byte i=0; i <mfrc522.uid.size; i++)
  {
    Serial.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
    Serial.print(mfrc522.uid.uidByte[i],HEX);
    content.concat(String(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " "));
    content.concat(String(mfrc522.uid.uidByte[i],HEX));
  }
    Serial.println(); */
  //  }
    
    
      digitalWrite(BELL_PIN, HIGH);
      delay(50);
      digitalWrite(BELL_PIN, LOW);
      delay(50);
      digitalWrite(BELL_PIN, HIGH);
      delay(50);
      digitalWrite(BELL_PIN, LOW);
      delay(1000);

    while (Serial.available()) 
    {
        sv = Serial.read();
        Serial.println(sv);
        delay(200);
    }
    if (sv == '1') 
    {
        myservo_ra.write(100); 
        delay(3000);
        myservo_ra.write(0);
    } 
 /*   else if (sv == '2') 
    {
        myservo_ra.write(100); 
        delay(3000);
        myservo_ra.write(0);
    }
    //mfrc522.PICC_HaltA();  */
}


