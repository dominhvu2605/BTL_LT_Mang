#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <Winsock.h>
#include <iostream>
#include <vector>
#include <sstream>
#pragma comment (lib, "ws2_32.lib")

using namespace std;

class Client {
private:
	int clientID;
	int roomID;
	SOCKET clientSocket;
public:
	Client(int clientID, SOCKET clientSocket) {
		this->clientID = clientID;
		this->roomID = -1;
		this->clientSocket = clientSocket;
	}
	int getClientID() {
		return clientID;
	}
	int getRoomID() {
		return roomID;
	}
	void setRoomID(int roomID) {
		this->roomID = roomID;
	}
	SOCKET getClientSocket() {
		return clientSocket;
	}
};

SOCKET serverSocket;
vector<Client> connections;
WSAData wsaData;
SOCKADDR_IN serverAddress;

int nextID;

vector<string> getTokens(string input, char delimiter) {
	vector<string> tokens;
	istringstream f(input);
	string s;
	while (getline(f, s, delimiter)) {
		tokens.push_back(s);
	}
	return tokens;
}

int clientCountInRoom(int roomID) {
	int count = 0;
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			count++;
		}
	}
	return count;
}

void playClient(int roomID) {
	char *sent = new char[256];
	//client vao phong dau tien luon chơi ben den
	bool black = true;

	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			if (black) {
				sprintf(sent, "%s", "[Play]Black");
				black = false;
			}
			else {
				sprintf(sent, "%s", "[Play]White");
			}
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void exitClient(int roomID) {
	char *sent = new char[256];
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			sprintf(sent, "%s", "[Exit]");
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void putClient(int roomID, int x, int y) {
	char *sent = new char[256];
	for (int i = 0; i < connections.size(); i++) {
		if (connections[i].getRoomID() == roomID) {
			ZeroMemory(sent, 256);
			string data = "[Put]" + to_string(x) + "," + to_string(y);
			sprintf(sent, "%s", data.c_str());
			send(connections[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void ServerThread(Client *client) {
	char *sent = new char[256];
	char *received = new char[256];
	int size = 0;
	while (true) {
		ZeroMemory(received, 256);
		if ((size = recv(client->getClientSocket(), received, 256, NULL)) > 0) {
			string receivedString = string(received);
			vector<string> tokens = getTokens(receivedString, ']');
			if (receivedString.find("[Enter]") != -1) {
				/* Tim client gui token */
				for (int i = 0; i < connections.size(); i++) {
					string roomID = tokens[1];
					int roomInt = atoi(roomID.c_str());
					if (connections[i].getClientSocket() == client->getClientSocket()) {
						/* So luong client trong phong co ID la roonInt */
						int clientCount = clientCountInRoom(roomInt);
						/* Neu phong da co 2 nguoi thi khong them vao phong va gui thong bao phong day */
						if (clientCount >= 2) {
							ZeroMemory(sent, 256);
							sprintf(sent, "%s", "[Full]");
							send(connections[i].getClientSocket(), sent, 256, 0);
							break;
						}
						cout << "Client ["<< client->getClientID() <<"]: " << " truy cap vao phong: " << roomID << endl;
						/* Cap nhat thong tin truy cap phong cho nguoi dung */
						Client *newClient = new Client(*client);
						newClient->setRoomID(roomInt);
						connections[i] = *newClient;
						/* Gui tin nhan truy cap phong thanh cong */
						ZeroMemory(sent, 256);
						sprintf(sent, "%s", "[Enter]");
						send(connections[i].getClientSocket(), sent, 256, 0);
						/* Bat dau tro choi neu phong da du nguoi */
						if (clientCount == 1) {
							playClient(roomInt);
						}
					}
				}
			}
			else if (receivedString.find("[Put]") != -1) {
				/* Nhan thong tin client gui tin nhan */
				string data = tokens[1];
				vector<string> dataTokens = getTokens(data, ',');
				int roomID = atoi(dataTokens[0].c_str());
				int x = atoi(dataTokens[1].c_str());
				int y = atoi(dataTokens[2].c_str());
				/* Truyen vi tri cua con co duoc danh */
				putClient(roomID, x, y);
			}
			else if (receivedString.find("[Play]") != -1) {
				string roomID = tokens[1];
				int roomInt = atoi(roomID.c_str());
				/* Set thuoc tinh black while cho moi client */
				playClient(roomInt);
			}
		}
		else {
			ZeroMemory(sent, 256);
			sprintf(sent, "Client [%i] logout!", client->getClientID());
			cout << sent << endl;
			/* Tim nguoi choi roi khoi phong */
			for (int i = 0; i < connections.size(); i++) {
				if (connections[i].getClientID() == client->getClientID()) {
					/* Client dang choi game voi 1 client khac da logout */
					if (connections[i].getRoomID() != -1 &&
						clientCountInRoom(connections[i].getRoomID()) == 2) {
						/* Thong bao cho cac client khac */
						exitClient(connections[i].getRoomID());
					}
					connections.erase(connections.begin() + i);
					break;
				}
			}
			delete client;
			break;
		}
	}
}

int main() {
	WSAStartup(MAKEWORD(2, 2), &wsaData);
	serverSocket = socket(AF_INET, SOCK_STREAM, NULL);

	serverAddress.sin_addr.s_addr = inet_addr("127.0.0.1");
	serverAddress.sin_port = htons(9876);
	serverAddress.sin_family = AF_INET;

	cout << "[Tao may chu cho game]: OK" << endl;
	bind(serverSocket, (SOCKADDR*)&serverAddress, sizeof(serverAddress));
	listen(serverSocket, 32);

	int addressLength = sizeof(serverAddress);
	while (true) {
		SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, NULL);
		if (clientSocket = accept(serverSocket, (SOCKADDR*)&serverAddress, &addressLength)) {
			Client *client = new Client(nextID, clientSocket);
			cout << "[Co 1 client moi ket noi]: " << (int) client << endl;
			CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)ServerThread, (LPVOID)client, NULL, NULL);
			connections.push_back(*client);
			nextID++;
		}
		Sleep(100);
	}
}