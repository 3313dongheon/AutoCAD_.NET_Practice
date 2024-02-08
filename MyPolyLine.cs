using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace MyPolyLine
{
    public class MyPolyLine
    {
        [CommandMethod("MyPolyLine")]
        public void Command()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor et = doc.Editor;

            PromptPointOptions _1stPtOpt = new PromptPointOptions("첫 번째 지점을 선택하세요.");
            PromptPointResult _1stPtResult = et.GetPoint(_1stPtOpt);
            if (_1stPtResult.Status != PromptStatus.OK)
                return;

            Point3d firstPoint = _1stPtResult.Value;

            PromptPointOptions _2ndPtOpt = new PromptPointOptions("두 번째 지점을 선택하세요.");
            _2ndPtOpt.UseBasePoint = true;
            _2ndPtOpt.BasePoint = firstPoint;

            while (true)
            {
                PromptPointResult _2ndPtResult = et.GetPoint(_2ndPtOpt);
                Point3d secondPoint = _2ndPtResult.Value;

                if (_2ndPtResult.Status == PromptStatus.OK)
                {
                    using (Transaction transaction = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        Line line = new Line(firstPoint, secondPoint);
                        btr.AppendEntity(line);
                        transaction.AddNewlyCreatedDBObject(line, true);
                        transaction.Commit();

                        firstPoint = secondPoint;
                        _2ndPtOpt.BasePoint = firstPoint;
                    }
                }
                else
                    break;
            }
            et.WriteMessage("\nMyPolyLine 종료.");
        }
    }
}
