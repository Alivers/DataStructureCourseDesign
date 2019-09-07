#ifndef COMMON_H
#define COMMON_H

#include <QPointF>
#include <cmath>
#include <numeric>
#include <vector>
#include <QColor>

constexpr auto   PI           = 3.1415926;
constexpr auto   VertexRadius = 25;
constexpr auto   Infinity     = (std::numeric_limits<double>::max)();
constexpr auto   MaxVertexNum = 10;
constexpr auto   MaxArcNum    = 20;

const     int    PolygonRadius[] = {0, 300 / 2, 300 / 2, 350 / 2, 350 / 2, 350 / 2, 350 / 2, 400 / 2, 400 / 2, 500 / 2, 550 / 2};
constexpr auto   WindowWidth   = 1400;
constexpr auto   WindowHeight  = 800;
constexpr auto   DemoCenterX   = (WindowWidth / 4 * 3) / 2;
constexpr auto   DemoCenterY   = (WindowHeight / 2);
constexpr auto   DurationTime  = 500;

const std::vector<QColor> StateColor = {QColor(0, 0, 255, 255),
                                        QColor(0, 255, 0, 255),
                                        QColor(255, 0, 0, 255),
                                        QColor(255, 255, 0, 255),
                                        QColor(0, 0, 0, 255)
                                        };

enum State {
    NORMAL,
    SELECTED,
    FIXED,
    START,
    END
};

/* 计算平方 */
inline qreal Square(const qreal &n)
{
    return n * n;
}

/* 计算两点距离 */
inline qreal PointDistance(const QPointF & a, const QPointF & b)
{
    return sqrt(Square((a.x() - b.x())) + Square(a.y() - b.y()));
}

/* 弧度转角度 */
inline qreal RadianToAngle(const qreal & r)
{
    return (r * 180 / PI);
}

/* 角度转弧度 */
inline qreal AngleToRadian(const qreal & a)
{
    return (a * PI / 180);
}

/* 计算给定点：angle 指定角度的方向上，距离该点长为length的点 */
inline QPointF PointAtDistance(const QPointF &c, qreal angle, qreal length)
{
    qreal r = AngleToRadian(angle);
    return QPointF(c.x() + length * cos(r), c.y() + length * sin(r));
}

inline qreal ConvertAzimuthOpposite(const qreal &angle)
{
    if (fabs(angle) <= 1e-5)
        return 180;
    else if (fabs(angle - 180) <= 1e-5)
        return 0;
    else if (angle > 0)
        return angle - 180;
    else
        return angle + 180;
}

/* 计算方位角 */
/* 以a为坐标原点，x轴正方向水平向右，y轴正方向水平向下, 计算 b 相对于x轴正方向的角度 */
/* 这里逆时针为负，顺时针为正 */
inline qreal AzimuthTwoPoints(const QPointF & a, const QPointF &b)
{
    /* x 相等 */
    if (fabs(a.x() - b.x()) <= 1e-5) {
        return (a.y() > b.y() ? -90 : 90);
    }
    /* y 相等 */
    else if (fabs(a.y() - b.y()) <= 1e-5) {
        return (a.x() > b.x() ? 180 : 0);
    }
    else {
        // 计算弧度并转为角度
        qreal angle = RadianToAngle(atan2(b.y() - a.y(), b.x() - a.x()));
        return angle;
    }
}

/* 计算两点的中点 */
inline QPointF MiddlePoint(const QPointF & a, const QPointF & b)
{
    return QPointF((a.x() + b.x()) / 2, (a.y() + b.y()) / 2);
}

#endif // COMMON_H
