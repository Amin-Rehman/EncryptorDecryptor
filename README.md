# EncryptorDecryptor

This experimental project contains two separate programs, 

1) Encryptor 

2) Decryptor

Upon Encryption, the user is asked for a password.
This uses the deprecated TrueCrypt drivers to mount TrueCrpyt containers and store data in them. There is another layer of security on top of TrueCrypt

The idea of the project is to experiment with a simple security algorithm that I wanted to work on. The aim is to make the password "Content-dependant", so in case the content inside the TrueCrypt container changes, the password will not be valid anymore.
It relies on a settings.json file which will be created by the Encryptor generating a key1 and key2.
The simple algorithm is as follows:

### Encryption
* Ask user for password
* Mount TrueCrypt container and copy content into it.
* Generate a GUID
* Generate Key1. Key1 = GUID XOR Password
* Generate Key. Key2 = MD5 of part of file (XOR) GUID
* Save Key1 and Key2 in settings.json

### Decryption
* Ask user for password.
* Read settings.json and read Key1 and Key2.
* Attempt to get a GUIDx. GUIDx = Key2 XOR MD5 of part of the file.
* Attempt to evaluate the password. Lets call it passwordX. passwordX = Key1 XOR GUIDx
* if passwordX == password, allow user access.


For fun, the Encryptor now writes everything to a DVD drive using IMAPI 2.0 drivers. 

### Future work
* Ability to create a new container of a different size. Have had some trouble with that, but still working on it.
* Some UI improvements
