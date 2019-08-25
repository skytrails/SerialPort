#ifndef HZSERIALPORT_H
#define HZSERIALPORT_H

#include <QString>
#include <QStringList>
#include <QSerialPort>

class hzSerialPort :public QObject
{
public:
    hzSerialPort();

    void sendInfo(const QString &info);
    void sendInfo(char *info, int len);
public slots:
    void receiveInfo();
private:
    void initDevice();
    void GetDevicePortName();

private:
    QStringList _serialPortName;
    QSerialPort *_pSerialPort = nullptr;
    char convertCharToHex(char ch);
    void convertStringToHex(const QString &str, QByteArray &byteData);
};

#endif // HZSERIALPORT_H
