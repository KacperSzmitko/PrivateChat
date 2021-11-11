# About the project
Private Chat is a system consisting of a server, two databases (MySQL, Redis) and a client application designed for Windows 7/8/10. The system is based on client-server architecture. Text messages can be exchanged between friends. Adding friends is done by a system of invitations. Messages are sent and stored in Redis database in an encrypted way. Other data required for the operation of the system are stored in the MySQL database.  A user can initiate multiple conversations simultaneously and switch between them. The system implements end-to-end message encryption, and any messages sent using the proprietary text-to-speech protocol are sent using the TLS protocol.


# Technologies and languages used
* **C#**
* **BouncyCastle** for cryptographic functions
* **MySQL**
* **OpenVPN** for testing 
