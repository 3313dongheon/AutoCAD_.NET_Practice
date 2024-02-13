using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace MyLibrary
{
    public class MyMethod
    {
        // 선분의 시작점은 왼쪽 혹은 (수직선일 경우)아래에 있다.
        public Line EditStartingPoint(Line line)
        {
            Point3d point3D;

            // line이 수직선이 아닐 때, 시작점이 왼쪽에 있는 선으로 바꾸기
            if (line.StartPoint.X > line.EndPoint.X)
            {
                point3D = line.StartPoint;
                line.StartPoint = line.EndPoint;
                line.EndPoint = point3D;
                return line;
            }

            // line이 수직선일때 시작점이 아래에 있는 선으로 바꾸기
            else if (line.StartPoint.X == line.EndPoint.X)
            {
                if (line.StartPoint.Y > line.EndPoint.Y)
                {
                    point3D = line.StartPoint;
                    line.StartPoint = line.EndPoint;
                    line.EndPoint = point3D;
                    return line;
                }

                else
                {
                    return line;
                }
            }

            else { return line; }
        }

        // 두 점 사이의 거리
        public double PointToPoint(Point3d pointA, Point3d pointB)
        {
            Line myLine = new Line(pointA, pointB);
            myLine = EditStartingPoint(myLine);

            double x1 = myLine.StartPoint.X;
            double y1 = myLine.StartPoint.Y;
            double x2 = myLine.EndPoint.X;
            double y2 = myLine.EndPoint.Y;

            double slope = (y2 - y1) / (x2 - x1);

            if (myLine.StartPoint.X == myLine.EndPoint.X)
            {
                return myLine.EndPoint.Y - myLine.StartPoint.Y;
            }

            else if (myLine.StartPoint.Y == myLine.EndPoint.Y)
            {
                return myLine.EndPoint.X - myLine.StartPoint.X;
            }

            else if (slope < 0)
            {
                return Math.Sqrt((Math.Pow(myLine.EndPoint.X - myLine.StartPoint.X, 2) + Math.Pow(myLine.StartPoint.Y - myLine.EndPoint.Y, 2)));
            }

            else
            {
                return Math.Sqrt((Math.Pow(myLine.EndPoint.X - myLine.StartPoint.X, 2) + Math.Pow(myLine.EndPoint.Y - myLine.StartPoint.Y, 2)));
            }
        }

        // 선을 직선처럼 : x값을 알 때, y를
        public double YwhenX(Line line, double x)
        {
            line = EditStartingPoint(line);

            double x1 = line.StartPoint.X;
            double y1 = line.StartPoint.Y;
            double x2 = line.EndPoint.X;
            double y2 = line.EndPoint.Y;

            double slope = (y2 - y1) / (x2 - x1);

            double interceptY = y1 - (slope * x1);

            return (slope * x) + interceptY;
        }

        // 선을 직선처럼 : y값을 알 때, x를
        public double XwhenY(Line line, double y)
        {
            line = EditStartingPoint(line);

            double x1 = line.StartPoint.X;
            double y1 = line.StartPoint.Y;
            double x2 = line.EndPoint.X;
            double y2 = line.EndPoint.Y;

            double slope = (y2 - y1) / (x2 - x1);

            double interceptY = y1 - (slope * x1);

            return (y - interceptY) / slope;
        }

        // 직선 만들기 : 기울기와 지나는 점, x값을 넣으면 해당 y값을 ouput
        public double MakeLine(double slope, Point3d point, double x_value)
        { 
            // b = y - ax
            double interceptY = point.Y - (slope*point.X);
            // ax + b = y
            return (slope * x_value) + interceptY;
        }

        // 두 선 사이의 접점
        public Point3d ContactPoint(Line line1, Line line2)
        {
            line1 = EditStartingPoint(line1);
            line2 = EditStartingPoint(line2);

            double slope1 = (line1.EndPoint.Y - line1.StartPoint.Y) / (line1.EndPoint.X - line1.StartPoint.X);
            double slope2 = (line2.EndPoint.Y - line2.StartPoint.Y) / (line2.EndPoint.X - line2.StartPoint.X);

            double interceptY1 = line1.StartPoint.Y - (slope1 * line1.StartPoint.X);
            double interceptY2 = line2.StartPoint.Y - (slope2 * line2.StartPoint.X);

            double x = (interceptY2 - interceptY1) / (slope1 - slope2);
            double y = slope1 * x + interceptY1;

            Point3d point3D = new Point3d(x, y, 0);

            return point3D;
        }

        // 점과 선 사이의 거리(선을 향한 점의 수선의 발이 선 외부에 있을 경우 끝점까지의 거리로 계산)
        public double DistancePointToLine(Point3d point, Line line)
        {
            Line myLine = EditStartingPoint(line);
            double lineSlope;
            double _lineSlope;

            // 점이 선 위에 있을 때
            if (YwhenX(line, point.X) == point.Y)
            {
                return 0;
            }

            // 점이 선 밖에 있을 때
            else 
            {
                // line이 수직선이 아닐 경우 (기울기 구해놓기)
                if (line.EndPoint.X != myLine.StartPoint.X)
                {
                    lineSlope = (line.EndPoint.Y - myLine.StartPoint.Y) / (line.EndPoint.X - myLine.StartPoint.X);
                    _lineSlope = -1 / lineSlope;
                }

                // line이 수직선일 경우
                else
                {
                    // 기울기는 쓰지 않고
                    lineSlope = 0;
                    _lineSlope = 0;

                    // 점이 선보다 우측에 있을 경우
                    if (point.X > XwhenY(line, point.Y))
                    {
                        return point.X - XwhenY(line, point.Y);
                    }

                    // 선이 점보다 우측에 있을 경우
                    else
                    {
                        return XwhenY(line, point.Y) - point.X;
                    }
                }

                // line의 기울기가 음일 때
                if (lineSlope < 0)
                {
                    // 선을 향한 점의 수선의 발이 선의 시작점 왼쪽에 있을 때
                    if (point.Y > MakeLine(_lineSlope, myLine.StartPoint, point.X))
                    {
                        return PointToPoint(point, myLine.StartPoint);
                    }

                    // 선을 향한 점의 수선의 발이 선의 끝점 오른쪽에 있을 때
                    else if (point.Y < MakeLine(_lineSlope, myLine.EndPoint, point.X))
                    {
                        return PointToPoint(point, myLine.EndPoint);
                    }

                    // 선을 향한 점의 수선의 발이 선 내부에 있을 때
                    else
                    {
                        Line tempLine = new Line(point, new Point3d(point.X+1, point.Y+_lineSlope, 0));
                        return PointToPoint(ContactPoint(tempLine, line), point);
                    }
                }

                // line의 기울기가 양일 때
                else if (lineSlope < 0)
                {
                    // 선을 향한 점의 수선의 발이 선의 시작점 왼쪽에 있을 때
                    if (point.Y < MakeLine(_lineSlope, myLine.StartPoint, point.X))
                    {
                        return PointToPoint(point, myLine.StartPoint);
                    }

                    // 선을 향한 점의 수선의 발이 선의 끝점 오른쪽에 있을 때
                    else if (point.Y > MakeLine(_lineSlope, myLine.EndPoint, point.X))
                    {
                        return PointToPoint(point, myLine.EndPoint);
                    }

                    // 선을 향한 점의 수선의 발이 선 내부에 있을 때
                    else
                    {
                        Line tempLine = new Line(point, new Point3d(point.X + 1, point.Y + _lineSlope, 0));
                        return PointToPoint(ContactPoint(tempLine, line), point);
                    }
                }

                // line이 수평선일 때
                else 
                {
                    if (line.EndPoint.X < point.X)
                    {
                        return PointToPoint(line.EndPoint, point);
                    }

                    else if (point.X < line.StartPoint.X)
                    {
                        return PointToPoint(line.StartPoint, point);
                    }

                    else if(YwhenX(line, point.X) < point.Y)
                    {
                        return point.Y - YwhenX(line, point.X);
                    }

                    else
                        return YwhenX(line, point.X) - point.Y;
                }
            }
        }

        // 수평일 경우 true, 아닐 경우 false
        public bool AreLinesParallel(Line line1, Line line2)
        {

            // Compute the direction vectors of the lines
            Vector3d direction1 = line1.EndPoint - line1.StartPoint;
            Vector3d direction2 = line2.EndPoint - line2.StartPoint;

            // Check if the cross product of the direction vectors is zero
            return direction1.CrossProduct(direction2).Length <= Tolerance.Global.EqualVector;
        }
    }
}
