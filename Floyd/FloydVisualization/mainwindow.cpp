#include "mainwindow.h"
#include "ui_mainwindow.h"
#include <QFileDialog>
#include <QDir>
#include <QDebug>

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    this->subWindow = nullptr;
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::fromSubWindow()
{
    if (this->subWindow) {
        delete this->subWindow;
        this->subWindow = nullptr;
    }
    this->show();
}

void MainWindow::on_scanButton_clicked()
{
    QString filter = "All File (*.*)";
//    QString file_name = QFileDialog::getOpenFileName(this, "Open a file", QDir::homePath(), filter);
    QString file_name = QFileDialog::getOpenFileName(this, "Open a file", "F:\\ProfessionalCourse\\DataStructure\\FinalProject\\Floyd", filter);
    ui->lineEdit->setText(file_name);
}

void MainWindow::on_cancelButton_clicked()
{
    this->close();
}

void MainWindow::on_confirmButton_clicked()
{
    this->hide();
    SubWindow::setDataFile(ui->lineEdit->text());
    this->subWindow = new SubWindow();
    connect(this->subWindow, SIGNAL(showMainWindow()), this, SLOT(fromSubWindow()));
    this->subWindow->prepareForDemo();
    this->subWindow->show();
}
