#ifndef GRAPH_H
#define GRAPH_H

#include "vertex.h"
#include "arc.h"
#include <vector>
#include <QFrame>

class Graph
{
public:
    Graph() = default;
    Graph(int vNum, int aNum) : vertexNum(vNum), arcNum(aNum) {}

   std::vector<Vertex> vertexes{};
   std::vector<std::vector<Arc>> arcs{};
   int vertexNum = 0;
   int arcNum = 0;
   std::vector<std::vector<double>> distance{};
   std::vector<std::vector<int>> path{};
};

#endif // GRAPH_H
