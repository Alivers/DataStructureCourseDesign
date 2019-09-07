#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "subwindow.h"

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void fromSubWindow();

    void on_scanButton_clicked();

    void on_cancelButton_clicked();

    void on_confirmButton_clicked();

private:
    Ui::MainWindow *ui;
    SubWindow *subWindow;
};

#endif // MAINWINDOW_H
