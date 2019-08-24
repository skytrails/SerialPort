#include "hzserialport.h"
#include <QList>
#include <QSerialPort>
#include <QSerialPortInfo>
#include <QDebug>

hzSerialPort::hzSerialPort()
{
    initDevice();
}

void hzSerialPort::initDevice()
{
    //QSerialPortInfo::availablePorts();
    QList<QSerialPortInfo> portInfo = QSerialPortInfo::availablePorts();
    QStringList m_serialPortName;

    foreach(const QSerialPortInfo &info,QSerialPortInfo::availablePorts())
    {
        m_serialPortName << info.portName();
        qDebug()<<"serialPortName:"<<info.portName();
    }

}
