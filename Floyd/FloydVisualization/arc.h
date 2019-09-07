#ifndef ARC_H
#define ARC_H

#include <QPainter>
#include <QGraphicsItem>
#include "common.h"

class Arc
{
public:

    Arc() = default;
    Arc(double weight);
    Arc(const QPointF &start, const QPointF &end, const QColor &color, State s, double weight);
    ~Arc();
    void setPos(const QPointF &start, const QPointF &end);
    void setColor(const QColor &color);
    void setState(State s);
    void setWeight(double weight);

    QColor getColor() const;
    State getState() const;
    double getWeight() const;

    void setArcFlag(bool flag);

    void drawArc(QPainter *painter);

private:
    QPointF start{};
    QPointF end{};
    double weight = 0;
    QColor arcColor{};
    State curState{};
    bool isArc;
};
#endif // ARC_H
