#include "vertex.h"
#include <QDebug>

Vertex::Vertex(const QString &name)
{
    this->name = name;
}

Vertex::Vertex(const QPointF &center, const QString &n, const QColor &color)
{
    this->center = center;
    this->name = n;
    this->vertexColor = color;
}

void Vertex::setPos(const QPointF &center)
{
    this->center = center;
}

void Vertex::setColor(const QColor &color)
{
    this->vertexColor = color;
}

QPointF Vertex::getPos() const
{
    return   this->center;
}

QColor Vertex::getColor() const
{
    return this->vertexColor;
}

void Vertex::setState(const State &s)
{
    this->curState = s;
    this->setColor(BrushColor[s]);
}

State Vertex::getState() const
{
    return this->curState;
}

QString Vertex::getName() const
{
    return this->name;
}

void Vertex::drawVertex(QPainter *painter)
{
    painter->setPen(QPen(QColor(0, 0, 0, 255)));
    painter->setBrush(QBrush(this->vertexColor));
    QRect rect(int(this->center.x() - VertexRadius), int(this->center.y() - VertexRadius), VertexRadius * 2, VertexRadius * 2);

    painter->drawEllipse(rect);
    painter->drawText(rect, this->name, QTextOption(Qt::AlignCenter));
    painter->setBrush(QBrush());
    painter->setPen(QPen());
}
