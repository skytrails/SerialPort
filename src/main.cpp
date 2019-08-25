#include <QCoreApplication>
#include "hzserialport.h"

int main(int argc, char *argv[])
{
    QCoreApplication a(argc, argv);
    hzSerialPort device;

    return a.exec();
}
