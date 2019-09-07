#include "floydframe.h"


FloydFrame::FloydFrame(QWidget *parent) : parent(parent)
{
    this->graph = new Graph();
}

FloydFrame::~FloydFrame()
{
    delete this->graph;
}

Graph *FloydFrame::getGraph() const
{
    return this->graph;
}


