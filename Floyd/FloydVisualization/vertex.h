#ifndef VERTEX_H
#define VERTEX_H

#include <QPainter>
#include "common.h"

const std::vector<QColor> BrushColor{QColor(0, 0, 255, 125),
                                     QColor(0, 255,0, 125),
                                     QColor(255, 0, 0, 125),
                                     QColor(255, 255, 0, 125),
                                     QColor(00, 255, 255, 125)};

class Vertex
{
public:
    Vertex() = default;
    Vertex(const QString &name);
    Vertex(const QPointF &center, const QString &n, const QColor &color);
    void setPos(const QPointF &center);
    void setColor(const QColor &color);
    QPointF getPos() const;
    QColor getColor() const;
    void setState(const State &s);
    State getState() const;
    QString getName() const;


    void drawVertex(QPainter *painter);

private:
    QString name{};
    QPointF center{};
    QColor vertexColor{};
    State curState{};
};

#endif // VERTEX_H
