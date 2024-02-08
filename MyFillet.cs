using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using System.Data.Common;
using System.Diagnostics;



namespace MyFillet
{
    public class MyFillet
    {
        
        // 선을 직선처럼 : x값을 알 때, y를
        public double YwhenX(Line line, double x)
        {
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
            double x1 = line.StartPoint.X;
            double y1 = line.StartPoint.Y;
            double x2 = line.EndPoint.X;
            double y2 = line.EndPoint.Y;

            double slope = (y2 - y1) / (x2 - x1);

            double interceptY = y1 - (slope * x1);

            return (y - interceptY) / slope;
        }

        // 두 선 사이의 접점
        public Point2d ContactPoint(Line line1, Line line2)
        {
            double slope1 = (line1.StartPoint.Y - line1.EndPoint.Y) / (line1.StartPoint.X - line1.EndPoint.X);
            double slope2 = (line2.StartPoint.Y - line2.EndPoint.Y) / (line2.StartPoint.X - line2.EndPoint.X);

            double interceptY1 = line1.StartPoint.Y - (slope1 * line1.StartPoint.X);
            double interceptY2 = line2.StartPoint.Y - (slope2 * line2.StartPoint.X);

            double x = (interceptY2 - interceptY1) / (slope1 - slope2);
            double y = slope1 * x + interceptY1;

            Point2d point2D = new Point2d(x, y);

            return point2D;
        }

        // 2개 선 fillet
        public (Line, Line) MyFillet(Line line1, Line line2)
        {
            double slope1 = (line1.StartPoint.Y - line1.EndPoint.Y) / (line1.StartPoint.X - line1.EndPoint.X);
            double slope2 = (line2.StartPoint.Y - line2.EndPoint.Y) / (line2.StartPoint.X - line2.EndPoint.X);

            double interceptY1 = line1.StartPoint.Y - (slope1 * line1.StartPoint.X);
            double interceptY2 = line2.StartPoint.Y - (slope2 * line2.StartPoint.X);

            double x = (interceptY2 - interceptY1) / (slope1 - slope2);
            double y = slope1 * x + interceptY1;

            Point3d contactingPoint3d = new Point3d(x, y, 0);

            Point3d line1_s = new Point3d(line1.StartPoint.X, line1.StartPoint.Y, 0);
            Point3d line1_e = new Point3d(line1.EndPoint.X, line1.EndPoint.Y, 0);

            Point3d line2_s = new Point3d(line2.StartPoint.X, line2.StartPoint.Y, 0);
            Point3d line2_e = new Point3d(line2.EndPoint.X, line2.EndPoint.Y, 0);

            Line line_1;
            Line line_2;

            if (contactingPoint3d.DistanceTo(line1_s) > contactingPoint3d.DistanceTo(line1_e))
            {
                line_1 = new Line(line1_s, contactingPoint3d);
            }
            else
            {
                line_1 = new Line(line1_e, contactingPoint3d);
            }

            if (contactingPoint3d.DistanceTo(line2_s) > contactingPoint3d.DistanceTo(line2_e))
            {
                line_2 = new Line(line2_s, contactingPoint3d);
            }
            else
            {
                line_2 = new Line(line2_e, contactingPoint3d);
            }

            return (line_1, line_2);
        }

        [CommandMethod("MyFillet")]
        public void Command()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            while (true)
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    PromptEntityOptions selFirstLine = new PromptEntityOptions("첫 번째 선을 선택하세요.");
                    selFirstLine.SetRejectMessage("잘 못 선택했습니다.");
                    selFirstLine.AddAllowedClass(typeof(Line), false);
                    PromptEntityResult firstLine = editor.GetEntity(selFirstLine);
                    if (firstLine.Status != PromptStatus.OK)
                        return;

                    PromptEntityOptions selSecondLine = new PromptEntityOptions("두 번째 선을 선택하세요.");
                    selSecondLine.SetRejectMessage("잘 못 선택했습니다.");
                    selSecondLine.AddAllowedClass(typeof(Line), false);
                    PromptEntityResult secondLine = editor.GetEntity(selSecondLine);
                    if (secondLine.Status != PromptStatus.OK)
                        return;

                    BlockTable blockTable = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord blocktablerecord = transaction.GetObject
                        (blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Line FirstLine = transaction.GetObject(firstLine.ObjectId, OpenMode.ForWrite) as Line;
                    Line SecondLine = transaction.GetObject(secondLine.ObjectId, OpenMode.ForWrite) as Line;
                    
                    (Line line1, Line line2) = MyFillet(FirstLine, SecondLine);
                    FirstLine.Erase();
                    SecondLine.Erase();

                    #region 폐기
                    //Line line1 = new Line(new Point3d(line2d1.StartPoint.X, line2d1.StartPoint.Y, 0),
                    //    new Point3d(line2d1.EndPoint.X, line2d1.EndPoint.Y, 0));
                    //Line line2 = new Line(new Point3d(line2d2.StartPoint.X, line2d2.StartPoint.Y, 0),
                    //    new Point3d(line2d2.EndPoint.X, line2d2.EndPoint.Y, 0));
                    #endregion

                    blocktablerecord.AppendEntity(line1);
                    blocktablerecord.AppendEntity(line2);
                    transaction.AddNewlyCreatedDBObject(line1, true);
                    transaction.AddNewlyCreatedDBObject(line2, true);

                    transaction.Commit();
                }
            }
        }
    }
}
