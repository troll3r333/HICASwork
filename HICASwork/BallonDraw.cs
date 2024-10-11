using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace HICASwork
{
    public class BallonDraw
    {
        [CommandMethod("VeBalloon")]
        public void VeBalloon()
        {
            //Khai báo command và khởi tạo môi trường
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //mở lệnh và đảm bảo lệnh chỉ đc áp dụng khi hoàn thành
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //lấy layer của bản vẽ
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                //kiểm tra xem có layer Ballon_1 chưa(chưa có thì tạo)
                if (!lt.Has("Balloon_1"))
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = "Balloon_1";
                    lt.UpgradeOpen();
                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                }

                // Set layer "Balloon_1" as active
                db.Clayer = lt["Balloon_1"];

                // Vẽ các balloon (circle, line, và text)
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                for (int i = 0; i < 5; i++)
                {
                    // Line
                    Line line = new Line(new Point3d(i * 20, 0, 0), new Point3d(i * 20 + 9, 8.5, 0));
                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);

                    // Circle
                    Circle circle = new Circle(new Point3d(i * 20 + 12.5199, 12.1303, 0), Vector3d.ZAxis, 5);
                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);

                    // Text
                    DBText text = new DBText();
                    text.Position = new Point3d(i * 20 + 12, 11, 0);
                    text.Height = 2;
                    text.TextString = (i + 1).ToString();
                    btr.AppendEntity(text);
                    tr.AddNewlyCreatedDBObject(text, true);
                }

                tr.Commit();
            }
        }
        [CommandMethod("DoiMauBalloon")]
        public void MauBalloon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                int colorIndex = 1; // Khởi tạo chỉ số màu

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;

                    // Kiểm tra xem ent có phải là Line hay không
                    if (ent is Line)
                    {
                        // Gán màu cho đường thẳng
                        ent.ColorIndex = colorIndex;
                        colorIndex++; // Tăng chỉ số màu cho đối tượng tiếp theo
                                      // Đảm bảo chỉ số màu không vượt quá 255
                        if (colorIndex > 255)
                        {
                            colorIndex = 1; // Quay lại chỉ số màu bắt đầu
                        }
                    }
                    if (ent is Circle)
                    {
                        // Gán màu cho đường thẳng
                        ent.ColorIndex = colorIndex;
                        colorIndex++; // Tăng chỉ số màu cho đối tượng tiếp theo
                                      // Đảm bảo chỉ số màu không vượt quá 255
                        if (colorIndex > 255)
                        {
                            colorIndex = 1; // Quay lại chỉ số màu bắt đầu
                        }
                    }
                    if (ent is DBText)
                    {
                        // Gán màu cho đường thẳng
                        ent.ColorIndex = colorIndex;
                        colorIndex++; // Tăng chỉ số màu cho đối tượng tiếp theo
                                      // Đảm bảo chỉ số màu không vượt quá 255
                        if (colorIndex > 255)
                        {
                            colorIndex = 1; // Quay lại chỉ số màu bắt đầu
                        }
                    }

                }

                tr.Commit();
            }

        }
        [CommandMethod("DuongnetBalloon")]
        public static void DuongnetBalloon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Tải kiểu đường nét nếu cần thiết
                LoadLinetypeIfNeeded(db, "Hidden");
                LoadLinetypeIfNeeded(db, "Continuous");
                LoadLinetypeIfNeeded(db, "Dashed");
                LoadLinetypeIfNeeded(db, "Center");

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    if (ent is Line)
                    {
                        // Chuyển đổi kiểu đường nét theo chu kỳ cho Line
                        if (ent.Linetype == "Hidden")
                            ent.Linetype = "Continuous";
                        else if (ent.Linetype == "Continuous")
                            ent.Linetype = "Dashed";
                        else
                            ent.Linetype = "Hidden";

                        ed.WriteMessage($"\nĐã thay đổi kiểu đường nét cho Line ID {objId} thành {ent.Linetype}.");
                    }
                    if (ent is Circle)
                    {
                        // Chuyển đổi kiểu đường nét theo chu kỳ cho Circle
                        if (ent.Linetype == "Center")
                            ent.Linetype = "Continuous";
                        else if (ent.Linetype == "Continuous")
                            ent.Linetype = "Dashed";
                        else
                            ent.Linetype = "Center";

                        ed.WriteMessage($"\nĐã thay đổi kiểu đường nét cho Circle ID {objId} thành {ent.Linetype}.");
                    }
                }

                tr.Commit();
            }

            // Vẽ lại màn hình để hiển thị thay đổi
            doc.Editor.Regen();
        }

        private static void LoadLinetypeIfNeeded(Database db, string linetypeName)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LinetypeTable lt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);

                // Nếu kiểu đường nét chưa có trong bản vẽ, tải nó từ file .lin
                if (!lt.Has(linetypeName))
                {
                    db.LoadLineTypeFile(linetypeName, "acad.lin");
                }

                tr.Commit();
            }
        }


        [CommandMethod("TaonhomBalloon")]
        public void GroupBalloon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //Tạo nhóm mới
                Group group = new Group("GroupBalloon", true);
                DBDictionary groupDict = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                groupDict.SetAt("GroupBalloon", group);
                tr.AddNewlyCreatedDBObject(group, true);
                // Thêm tất cả các đối tượng trong không gian mô hình vào nhóm
                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    group.Append(objId);
                }
                tr.Commit();
            }
        }
        [CommandMethod("MoveRight")]
        public void MoveRight()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Vector di chuyển sang phải 20 đơn vị
                Vector3d displacement = new Vector3d(20, 0, 0);

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;

                    if (ent != null)
                    {
                        // Di chuyển đối tượng bằng cách áp dụng phép biến đổi (transform)
                        ent.TransformBy(Matrix3d.Displacement(displacement));
                    }
                }

                tr.Commit();
            }
        }
        [CommandMethod("MoveLeft")]
        public void MoveLeft()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Vector di chuyển sang trái 20 đơn vị
                Vector3d displacement = new Vector3d(-20, 0, 0);

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;

                    if (ent != null)
                    {
                        // Di chuyển đối tượng bằng cách áp dụng phép biến đổi (transform)
                        ent.TransformBy(Matrix3d.Displacement(displacement));
                    }
                }
                tr.Commit();
            }
        }
        [CommandMethod("Kegocvuong")]
        public void Rotate90()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Matrix3d rotationMatrix = Matrix3d.Rotation(Math.PI / 2, Vector3d.ZAxis, new Point3d(0, 0, 0));

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    ent.TransformBy(rotationMatrix);
                }

                tr.Commit();
            }
        }
        [CommandMethod("AddBalloonTextA")]
        public void AddBalloonTextA()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (ObjectId objId in btr)
                {
                    DBText text = tr.GetObject(objId, OpenMode.ForWrite) as DBText;
                    if (text != null)
                    {
                        text.TextString = "A" + text.TextString;
                    }
                }

                tr.Commit();
            }
        }
        [CommandMethod("XaotrepBallon")]
        public void XaotrepBallon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Vector di chuyển các balloon lên trên 30 đơn vị 
                Vector3d displacement = new Vector3d(0, 30, 0);

                // Danh sách lưu trữ các bản sao mới
                List<Entity> newEntities = new List<Entity>();

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;

                    if (ent != null)
                    {
                        // Tạo một bản sao của đối tượng
                        Entity copiedEnt = ent.Clone() as Entity;

                        if (copiedEnt != null)
                        {
                            // Di chuyển bản sao bằng vector displacement
                            copiedEnt.TransformBy(Matrix3d.Displacement(displacement));

                            // Thêm bản sao vào danh sách
                            newEntities.Add(copiedEnt);
                        }
                    }
                }

                // Sau khi đã hoàn tất việc clone  thêm tất cả các đối tượng mới vào không gian mô hình
                foreach (Entity newEnt in newEntities)
                {
                    btr.AppendEntity(newEnt);
                    tr.AddNewlyCreatedDBObject(newEnt, true);
                }

                tr.Commit();
            }
        }

        [CommandMethod("XoaBalloon")]
        public void XoaBalloon()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    ent.Erase();
                }

                tr.Commit();
            }
        }

    }
}






    
   

