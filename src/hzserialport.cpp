#include "hzserialport.h"
#include <QList>
#include <QSerialPort>
#include <QSerialPortInfo>
#include <QDebug>
#include <QObject>

hzSerialPort::hzSerialPort()
{
    GetDevicePortName();
    initDevice();
}

void hzSerialPort::GetDevicePortName()
{
    //QSerialPortInfo::availablePorts();
    QList<QSerialPortInfo> portInfo = QSerialPortInfo::availablePorts();
    QStringList _serialPortName;

    foreach(const QSerialPortInfo &info,QSerialPortInfo::availablePorts())
    {
        _serialPortName << info.portName();
        qDebug()<<"serialPortName:"<<info.portName();
    }
}


void hzSerialPort::initDevice() {
    _pSerialPort = new QSerialPort();
    if (_pSerialPort->isOpen()) {
        _pSerialPort->clear();
        _pSerialPort->close();
    }
    _pSerialPort->setPortName(_serialPortName[0]);
    if (!_pSerialPort->open(QIODevice::ReadWrite)) {
        qDebug() << _serialPortName[0] << "open failed!";
    }
    return ;

    _pSerialPort->setBaudRate(QSerialPort::Baud115200, QSerialPort::AllDirections);// 设置波特率和读写方向

    _pSerialPort->setDataBits(QSerialPort::Data8);
    _pSerialPort->setFlowControl(QSerialPort::NoFlowControl);
    _pSerialPort->setParity(QSerialPort::NoParity);
    _pSerialPort->setStopBits(QSerialPort::OneStop);

    QIODevice::connect(_pSerialPort, SIGNAL(readyRead()), static_cast<QObject*>(this), SLOT(receiveInfo()));
}

void hzSerialPort::receiveInfo() {
    QByteArray info = _pSerialPort->readAll();
    QByteArray hexData = info.toHex();

    qDebug() << hexData;
}

void hzSerialPort::convertStringToHex(const QString &str, QByteArray &byteData) {
    int hexdata,lowhexdata;
    int hexdatalen = 0;
    int len = str.length();
    byteData.resize(len/2);
    char lstr,hstr;
    for(int i=0; i<len; )
    {
        //char lstr,
        hstr=str[i].toLatin1();
        if(hstr == ' ')
        {
            i++;
            continue;
        }
        i++;
        if(i >= len)
            break;
        lstr = str[i].toLatin1();
        hexdata = convertCharToHex(hstr);
        lowhexdata = convertCharToHex(lstr);
        if((hexdata == 16) || (lowhexdata == 16))
            break;
        else
            hexdata = hexdata*16+lowhexdata;
        i++;
        byteData[hexdatalen] = (char)hexdata;
        hexdatalen++;
    }
    byteData.resize(hexdatalen);
}

char hzSerialPort::convertCharToHex(char ch) {
    /*
    0x30等于十进制的48，48也是0的ASCII值，，
    1-9的ASCII值是49-57，，所以某一个值－0x30，，
    就是将字符0-9转换为0-9
    */

    if((ch >= '0') && (ch <= '9'))
         return ch-0x30;
     else if((ch >= 'A') && (ch <= 'F'))
         return ch-'A'+10;
     else if((ch >= 'a') && (ch <= 'f'))
         return ch-'a'+10;
     else return (-1);
}

void hzSerialPort::sendInfo(char *info, int len) {
    for (int i = 0; i < len; ++i) {
        printf("0x%x\n", info[i]);
    }
    _pSerialPort->write(info, len);
}

void hzSerialPort::sendInfo(const QString &info) {
    QByteArray sendBuf;
    if (info.contains(" "))
    {
        //info.replace(QString("fl"),QString("f"));//我这里是把空格去掉，根据你们定的协议来
    }
    qDebug()<<"Write to serial: "<<info;
    convertStringToHex(info, sendBuf); //把QString 转换 为 hex 

    _pSerialPort->write(sendBuf);////这句是真正的给单片机发数据 用到的是QIODevice::write 具体可以看文档
}
