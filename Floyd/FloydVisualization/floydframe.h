#ifndef FLOYDFRAME_H
#define FLOYDFRAME_H

#include <QWidget>
#include <QPaintEvent>
#include "graph.h"
#include "common.h"

class FloydFrame : public QWidget
{
    Q_OBJECT
public:
    explicit FloydFrame(QWidget *parent = nullptr);
    ~FloydFrame();
    Graph* getGraph() const;

protected:
    void paintEvent(QPaintEvent *event);

private:
    QWidget *parent;


};

#endif // FLOYDFRAME_H
