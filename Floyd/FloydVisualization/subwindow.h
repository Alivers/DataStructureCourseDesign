#ifndef SUBWINDOW_H
#define SUBWINDOW_H

#include <QWidget>
#include <QPaintEvent>
#include <QTableWidgetItem>
#include <QTimer>

#include "common.h"
#include "graph.h"

namespace Ui {
class SubWindow;
}

struct pixel{
    std::vector<std::vector<double>> distance;
    std::vector<std::vector<int>>    path;
    int begin;
    int end;
    pixel(const decltype(distance) d, const decltype(path) p, int b, int e) : distance(d), path(p), begin(b), end(e) {}
};

class SubWindow : public QWidget
{
    Q_OBJECT

public:
    explicit SubWindow(QWidget *parent = nullptr);
    ~SubWindow();

    static void setDataFile(QString file = "");

    void prepareForDemo();

    void Floyd();
    void resizeMatrix();
    void setStates(int from, int to, State state, int end = 0);
    bool isEqual(double a, double b);
    void initTable();
    void updateTable();
signals:
    void showMainWindow();
    void negativeCircle();
    void demoCompleted();

protected:
    void paintEvent(QPaintEvent *event);
    void closeEvent(QCloseEvent *event);
    void mousePressEvent(QMouseEvent *event);
    void mouseMoveEvent(QMouseEvent *event);
    void mouseReleaseEvent(QMouseEvent *event);

private slots:
    void on_outFileButton_clicked();
    void showError();

    void on_playButton_clicked();
    void myUpdate();

    void on_pauseButton_clicked();

    void on_backButton_clicked();

    void onDemoCompleted();

private:
    static QString dataFile;
    Ui::SubWindow *ui;
    Graph *graph;
    std::vector<pixel> pixels;
    QTimer *timer;
    int totalCount;
    std::vector<std::vector<double>> result;
    int selectedIndex = -1;

    bool demoFinished = false;
    int startIndex = -1;
    int endIndex = -1;

    void readFile();
    void wirteFile(const QString &);

    void drawArcs(QPainter *painter);
    void drawVertexes(QPainter *painter);
    bool RealEndpoint(const QPointF &start, const QPointF &end, QPointF &realStart, QPointF &realEnd);

    void setVertexArcPos();
};

#endif // SUBWINDOW_H
