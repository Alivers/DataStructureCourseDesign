#include "arc.h"
#include <QDebug>

Arc::Arc(const QPointF &start, const QPointF &end, const QColor &color, State s, double weight)
{
    this->start = start;
    this->end = end;
    this->arcColor = color;
    this->curState = s;
    this->weight = weight;
    this->isArc = true;
}

Arc::Arc(double weight)
{
    this->weight = weight;
}

Arc::~Arc()
{

}

void Arc::setPos(const QPointF &start, const QPointF &end)
{
    this->start = start;
    this->end = end;
}

void Arc::setColor(const QColor &color)
{
    this->arcColor = color;
}

void Arc::setState(State s)
{
    this->curState = s;
    this->setColor(StateColor[s]);
}

void Arc::setWeight(double weight)
{
    this->weight = weight;
}

QColor Arc::getColor() const
{
    return this->arcColor;
}

State Arc::getState() const
{
    return this->curState;
}

double Arc::getWeight() const
{
    return  this->weight;
}

void Arc::setArcFlag(bool flag)
{
    this->isArc = flag;
}

void Arc::drawArc(QPainter *painter)
{
    painter->setPen(QPen(this->arcColor));

    auto length = PointDistance(this->start, this->end);
    auto azimuth = AzimuthTwoPoints(this->start, this->end);
    auto offset = (this->isArc ? 30.0 : 0);
    QPointF middle = PointAtDistance(this->start, azimuth - offset, (length / 2) / cos(AngleToRadian(offset)));
    QPainterPath path(this->start);
    if (this->isArc)
        path.quadTo(middle, this->end);
    else
        path.lineTo(this->end);

    painter->setBrush(QBrush(Qt::transparent));
    painter->drawPath(path);

    auto oppAzimuth = ConvertAzimuthOpposite(azimuth) + offset;

    middle = PointAtDistance(this->start, azimuth - offset / 2, (length / 2) / cos(AngleToRadian(offset / 2)));

    for (int off = -30; off <= 30; ++off) {
        QPointF arrow = PointAtDistance(this->end, oppAzimuth + off, (VertexRadius / 3) / cos(AngleToRadian(off)));
        painter->drawLine(this->end, arrow);
    }
    painter->drawText(QRect(int(middle.x() - VertexRadius), int(middle.y() - VertexRadius), VertexRadius * 2, VertexRadius * 2),
                      QString::number(this->weight, 'g', 3), QTextOption(Qt::AlignCenter));

    painter->setPen(QPen());
}
