#include "subwindow.h"
#include "ui_subwindow.h"

#include <QTextStream>
#include <QMessageBox>
#include <QMainWindow>
#include <QFileDialog>
#include <QDebug>

QString SubWindow::dataFile = nullptr;

SubWindow::SubWindow(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::SubWindow)
{
    ui->setupUi(this);
    this->graph = new Graph();
    this->resize(WindowWidth, WindowHeight);

    this->totalCount = 0;
    this->timer = new QTimer();

    connect(this, SIGNAL(demoCompleted()), this, SLOT(onDemoCompleted()));
    connect(this->timer, SIGNAL(timeout()), this, SLOT(myUpdate()));
    connect(this, SIGNAL(negativeCircle()), this, SLOT(showError()));

    this->setMouseTracking(true);

    this->selectedIndex = -1;

    this->demoFinished = false;
    this->startIndex = -1;
    this->endIndex = -1;
}

SubWindow::~SubWindow()
{
    delete this->ui;
    delete this->graph;
}

void SubWindow::setDataFile(QString file)
{
    dataFile = file;
}

void SubWindow::prepareForDemo()
{
    this->readFile();
    this->setVertexArcPos();
    this->resizeMatrix();
    this->initTable();
    this->Floyd();
}

void SubWindow::initTable()
{
    this->ui->distanceTable->setRowCount(this->graph->vertexNum);
    this->ui->distanceTable->setColumnCount(this->graph->vertexNum);
    this->ui->pathTable->setRowCount(this->graph->vertexNum);
    this->ui->pathTable->setColumnCount(this->graph->vertexNum);

    QStringList header;
    for (int i = 0; i < this->graph->vertexNum; ++i) {
        header << this->graph->vertexes[i].getName();
    }
    this->ui->distanceTable->setVerticalHeaderLabels(header);
    this->ui->distanceTable->setHorizontalHeaderLabels(header);

    this->ui->pathTable->setVerticalHeaderLabels(header);
    this->ui->pathTable->setHorizontalHeaderLabels(header);

    for (int i = 0; i < this->graph->vertexNum; ++i) {
        for (int j = 0; j < this->graph->vertexNum; ++j) {
            this->ui->distanceTable->setItem(i, j, new QTableWidgetItem("-"));
            this->ui->pathTable->setItem(i, j, new QTableWidgetItem("-"));
            this->ui->distanceTable->item(i, j)->setTextAlignment(Qt::AlignCenter);
            this->ui->pathTable->item(i, j)->setTextAlignment(Qt::AlignCenter);
        }
    }
}

void SubWindow::updateTable()
{
    for (int i = 0; i < this->graph->vertexNum; ++i) {
        for (int j = 0; j < this->graph->vertexNum; ++j) {
            if (i == j)
                continue;
            if (isEqual(this->graph->distance[i][j], Infinity))
                continue;
            this->ui->distanceTable->item(i, j)->setText(QString::number(this->graph->distance[i][j]));
            if (this->graph->path[i][j] == -1)
                this->ui->pathTable->item(i, j)->setText(this->graph->vertexes[j].getName());
            else
                this->ui->pathTable->item(i, j)->setText(this->graph->vertexes[this->graph->path[i][j]].getName());
        }
    }
}

void SubWindow::resizeMatrix()
{
    this->graph->distance.resize(this->graph->vertexNum);
    this->graph->path.resize(this->graph->vertexNum);
    for (int i = 0; i < this->graph->vertexNum; ++i) {
        this->graph->distance[i].resize(this->graph->vertexNum);
        this->graph->path[i].resize(this->graph->vertexNum);
    }
}

void SubWindow::setVertexArcPos()
{
    int n = int(this->graph->vertexes.size());

    for (auto i = 0; i < n; ++i) {
        this->graph->vertexes[i].setPos(QPointF(DemoCenterX + PolygonRadius[n] * cos(2 * PI * i / n),
                                                DemoCenterY + PolygonRadius[n] * sin(2 * PI * i / n)));
        this->graph->vertexes[i].setState(NORMAL);
    }
    for (auto i = 0; i < n; ++i) {
        for (auto j = 0; j < n; ++j) {
            if (i != j && fabs(this->graph->arcs[i][j].getWeight() - Infinity) > 1e-5) {
                QPointF start, end;
                this->RealEndpoint(this->graph->vertexes[i].getPos(), this->graph->vertexes[j].getPos(),
                                   start, end);
                this->graph->arcs[i][j].setPos(start, end);
                this->graph->arcs[i][j].setState(NORMAL);
                if (isEqual(this->graph->arcs[j][i].getWeight(), Infinity))
                    this->graph->arcs[i][j].setArcFlag(false);
                else
                    this->graph->arcs[i][j].setArcFlag(true);
            }
        }
    }
}

void SubWindow::paintEvent(QPaintEvent *event)
{
    QPainter painter(this);
    // 设置抗锯齿
    painter.setRenderHint(QPainter::Antialiasing);
    painter.setFont(QFont("宋体", 16));
    painter.fillRect(painter.window(), Qt::white);

    if (this->graph->vertexNum > MaxVertexNum || this->graph->arcNum > MaxArcNum) {
        this->ui->infoLabel->setText("<h1><font color=red>顶点数或边数超过可视范围，请选择直接将结果输出到文件！</font></h1>");
        this->ui->infoLabel->setTextFormat(Qt::AutoText);
        this->ui->infoLabel->adjustSize();
        this->ui->infoLabel->setWordWrap(true);
    }
    else {
        this->drawVertexes(&painter);
        this->drawArcs(&painter);
    }

    event->setAccepted(true);
}

void SubWindow::closeEvent(QCloseEvent *event)
{
//    emit showMainWindow();
    event->setAccepted(true);
}

void SubWindow::mousePressEvent(QMouseEvent *event)
{
    if (this->selectedIndex == -1 && (event->button() == Qt::LeftButton)){
        for (int i = 0; i < this->graph->vertexNum; ++i){
            if (PointDistance(event->pos(), this->graph->vertexes[i].getPos()) < VertexRadius){
                this->selectedIndex = i;
                break;
            }
        }
        if (this->demoFinished) {
            if (this->startIndex != -1 && this->endIndex != -1) {
                this->setStates(this->startIndex, this->endIndex, NORMAL, this->endIndex);
                this->repaint();
            }
            this->startIndex = this->selectedIndex;
            this->endIndex = -1;
        }
    }
    event->setAccepted(true);
}

void SubWindow::mouseMoveEvent(QMouseEvent *event)
{
    if (this->selectedIndex != -1 && (event->buttons() & Qt::LeftButton)) {
        this->graph->vertexes[this->selectedIndex].setPos(event->pos());

        for (int v = this->selectedIndex, w = 0; w < this->graph->vertexNum; ++w){
            if (this->isEqual(this->graph->arcs[v][w].getWeight(), Infinity))
                continue;
            QPointF realStart, realEnd;
            this->RealEndpoint(this->graph->vertexes[v].getPos(), this->graph->vertexes[w].getPos(), realStart, realEnd);
            this->graph->arcs[v][w].setPos(realStart, realEnd);
        }

        for (int v = 0, w = this->selectedIndex; v < this->graph->vertexNum; ++v){
            if (this->isEqual(this->graph->arcs[v][w].getWeight(), Infinity))
                continue;
            QPointF realStart, realEnd;
            this->RealEndpoint(this->graph->vertexes[v].getPos(), this->graph->vertexes[w].getPos(), realStart, realEnd);
            this->graph->arcs[v][w].setPos(realStart, realEnd);
        }

        this->repaint();
    }
    if (this->demoFinished) {
        int temp = -1;
        for (int i = 0; i < this->graph->vertexNum; ++i){
            if (PointDistance(event->pos(), this->graph->vertexes[i].getPos()) < VertexRadius){
                temp = i;
                break;
            }
        }

        if (this->startIndex != -1 && this->endIndex != -1) {
            this->setStates(this->startIndex, this->endIndex, NORMAL, this->endIndex);
            this->endIndex = -1;
        }

        if (temp != this->startIndex && this->startIndex != -1 && temp != -1) {
            this->endIndex = temp;
            this->setStates(this->startIndex, temp, SELECTED, temp);
        }
        this->repaint();
    }

    event->setAccepted(true);
}

void SubWindow::mouseReleaseEvent(QMouseEvent *event)
{
    if (this->selectedIndex == -1)
        return;
    if (!(event->button() & Qt::LeftButton))
        return;
    this->selectedIndex = -1;

    event->setAccepted(true);
}

void SubWindow::readFile()
{
    if (dataFile.isNull()) {
        return ;
    }
    QFile file(dataFile);

    if (!file.open(QFile::ReadOnly | QFile::Text)) {
        QMessageBox::warning(this, "Warning", "文件打开失败！");
        this->close();
        emit showMainWindow();
    }
    QTextStream fin(&file);
    QString buff("");

    bool node = false, side = false;
    while (!fin.atEnd()) {
        buff = fin.readLine();

        if (buff.trimmed().isEmpty()) {
            continue;
        }

        if (buff == "[node]") {
            node = true;
            continue;
        }
        if (buff == "[line]") {
            node = false;
            side = true;

            this->graph->arcs.resize(this->graph->vertexes.size());
            for (decltype (this->graph->arcs.size()) i = 0; i < this->graph->arcs.size(); ++i) {
                this->graph->arcs[i].resize(this->graph->vertexes.size());
                for (decltype (i) j = 0; j < this->graph->arcs[i].size(); ++j) {
                    if (i == j)
                        this->graph->arcs[i][j].setWeight(0);
                    else
                        this->graph->arcs[i][j].setWeight(Infinity);
                }
            }

            continue;
        }
        if (node) {
            auto spe = buff.indexOf('=');
            if (spe <= 0 )
                continue;
            buff.remove(0, spe + 1);
            this->graph->vertexes.push_back({buff});
            ++this->graph->vertexNum;
        }

        if (side) {
            auto list = buff.split(",");
            if (list.size() != 3)
                continue;
            auto start = list[0].toInt();
            auto end = list[1].toInt();
            auto weight = list[2].toDouble();
            this->graph->arcs[start - 1][end - 1].setWeight(weight);

            ++this->graph->arcNum;
        }
    }
    file.close();
    return ;
}

void SubWindow::drawArcs(QPainter *painter)
{
    for (decltype (this->graph->arcs.size()) i = 0; i < this->graph->arcs.size(); ++i) {
        for (decltype (i) j = 0; j < this->graph->arcs[i].size(); ++j) {
            if (i != j && fabs(this->graph->arcs[i][j].getWeight() - Infinity) > 1e-5) {
                this->graph->arcs[i][j].drawArc(painter);
            }
        }
    }
}

void SubWindow::drawVertexes(QPainter *painter)
{
    for (decltype (this->graph->vertexes.size()) i = 0; i < this->graph->vertexes.size(); ++i) {
        this->graph->vertexes[i].drawVertex(painter);
    }
}

/* 由于需要考虑顶点的半径，不能从圆心开始画弧，所以需要计算实际圆上的起点和终点 */
bool SubWindow::RealEndpoint(const QPointF &start, const QPointF &end, QPointF &realStart, QPointF &realEnd)
{
    // 计算起点相对于终点的角度
    qreal azimuth = AzimuthTwoPoints(end, start);
    realEnd = PointAtDistance(end, azimuth, VertexRadius);

    // 此距离实际为起点圆心到终点圆上最近点的距离
    auto distance = PointDistance(start, realEnd);

    // 此距离若小于等于半径，说明两圆相交或相切
    if (distance <= double(VertexRadius))
        return false;

    realStart = QPointF(start.x() + VertexRadius * (realEnd.x() - start.x()) / distance,
                        start.y() + VertexRadius * (realEnd.y() - start.y()) / distance);

    return true;
}

void SubWindow::on_outFileButton_clicked()
{
    QString filter = "All File (*.*)";
//    QString file_name = QFileDialog::getOpenFileName(this, "Open a file", QDir::homePath(), filter);
    QString file_name = QFileDialog::getOpenFileName(this, "Open a file", "F:\\ProfessionalCourse\\DataStructure\\FinalProject\\Floyd", filter);

    this->wirteFile(file_name);
    if (!file_name.isEmpty())
        QMessageBox::information(this, "提示", file_name + "保存成功！");
}

void SubWindow::showError()
{
    QMessageBox::warning(this, "Warning", "图中出现负环，搜索失败！");
//    this->close();
}

void SubWindow::wirteFile(const QString & file_name)
{
    if (file_name.isNull() || file_name.isEmpty())
        return;
    QFile outfile(file_name);
    if (!outfile.open(QFile::WriteOnly | QFile::Text))
        return;
    QTextStream fout(&outfile);

    for (int i = 0; i < this->result.size(); ++i) {
        for (int j = 0; j < this->result[i].size(); ++j) {
            if (i == j)
                continue;
            if (fabs(this->result[i][j] - Infinity) > 1e-5) {
                fout << this->graph->vertexes[i].getName() << "->"
                     << this->graph->vertexes[j].getName() << ": "
                     << this->result[i][j] << endl;
            }
            else {
                fout << this->graph->vertexes[i].getName() << "->"
                     << this->graph->vertexes[j].getName() << ": "
                     << tr("不可达") << endl;
            }
        }
    }

    outfile.close();
}


void SubWindow::Floyd()
{
    for (int v = 0; v < this->graph->vertexNum; ++v) {
        for (int w = 0; w < this->graph->vertexNum; ++w) {
            this->graph->distance[v][w] = this->graph->arcs[v][w].getWeight();
            this->graph->path[v][w] = -1;
        }
    }
//    this->updateTable();

    this->pixels.push_back({this->graph->distance, this->graph->path, 0, 0});

    for (int u = 0; u < this->graph->vertexNum; ++u) {
        for (int v = 0; v < this->graph->vertexNum; ++v) {
            for (int w = 0; w < this->graph->vertexNum; ++w) {
                auto temp = (isEqual(this->graph->distance[v][u], Infinity) || isEqual(this->graph->distance[u][w], Infinity))
                            ? Infinity : (this->graph->distance[v][u] + this->graph->distance[u][w]);
                if (temp < this->graph->distance[v][w]) {
                    this->graph->distance[v][w] = temp;
                    this->graph->path[v][w] = u;

                    this->pixels.push_back({this->graph->distance, this->graph->path, v, w});
                }
            }
            if (this->graph->distance[v][v] < 0) {
                emit negativeCircle();
                return;
            }
        }
    }
    this->result = this->pixels.back().distance;
}

bool SubWindow::isEqual(double a, double b)
{
    return (fabs(a - b) < 1e-5);
}


void SubWindow::setStates(int from, int to, State state, int end)
{
    if (this->graph->path[from][to] != -1) {
        setStates(from, this->graph->path[from][to], state, to);
        setStates(this->graph->path[from][to], to, state, to);
    }
    else {
        if (to == end && state != NORMAL)
            this->graph->vertexes[to].setState(END);
        else if (to == end && state == NORMAL)
            this->graph->vertexes[to].setState(state);
        else
            this->graph->vertexes[to].setState(state);

        if (from != to) {
            this->graph->vertexes[from].setState(state);
            this->graph->arcs[from][to].setState(state);
        }
    }
}

void SubWindow::on_playButton_clicked()
{
    this->timer->start(DurationTime);
}

void SubWindow::myUpdate()
{
    if ((this->totalCount / 4 + 1) >= this->pixels.size()) {
        this->totalCount = 0;
        this->timer->stop();
        this->demoFinished = true;
        emit demoCompleted();
        return;
    }
    int index = (this->totalCount / 4 + (this->totalCount / 2) % 2);
    State curState;
    switch (this->totalCount % 4) {
        case 0 :
            curState = FIXED;
            break;
        case 1 :
            curState = NORMAL;
            break;
        case 2 :
            curState = SELECTED;
            break;
        case 3 :
            curState = NORMAL;
            break;
    }

    this->graph->distance = this->pixels[index].distance;
    this->graph->path = this->pixels[index].path;

    int begin = this->pixels[this->totalCount / 4 + 1].begin;
    int end = this->pixels[this->totalCount / 4 + 1].end;

    this->setStates(begin, end, curState, end);
    this->updateTable();

    this->repaint();

    ++this->totalCount;
}

void SubWindow::on_pauseButton_clicked()
{
    if (this->timer->isActive())
        this->timer->stop();
}

void SubWindow::on_backButton_clicked()
{
    this->close();
    emit showMainWindow();
}

void SubWindow::onDemoCompleted()
{
    QMessageBox::information(this, "提示", "算法演示结束！");
}
