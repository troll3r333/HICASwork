using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
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
        private static int colorIndex = 1; // Khởi tạo chỉ số màu (biến tĩnh)

        [CommandMethod("DoiMauBalloon")]
        public void MauBalloon()
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

                    // Kiểm tra và gán màu cho các loại đối tượng
                    if (ent is Line || ent is Circle || ent is DBText)
                    {
                        ent.ColorIndex = colorIndex;
                        colorIndex = (colorIndex % 255) + 1; // Tăng chỉ số màu và quay lại 1 nếu vượt quá 255
                    }
                }

                tr.Commit();
            }
        }
        private static int linetypeIndex = 0; // Khởi tạo chỉ số kiểu đường nét (biến tĩnh)

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

                // Danh sách các kiểu đường nét sử dụng để thay đổi luân phiên
                string[] linetypes = { "Continuous", "Dashed", "Hidden", "Center" };

                // Tải kiểu đường nét nếu cần thiết
                foreach (string linetype in linetypes)
                {
                    LoadLinetypeIfNeeded(db, linetype);
                }

                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;

                    if (ent is Line || ent is Circle)
                    {
                        // Chuyển đổi kiểu đường nét theo chu kỳ dựa vào linetypeIndex
                        ent.Linetype = linetypes[linetypeIndex];
                        ed.WriteMessage($"\nĐã thay đổi kiểu đường nét cho {ent.GetType().Name} ID {objId} thành {ent.Linetype}.");

                        // Tăng chỉ số kiểu đường nét và quay lại 0 nếu vượt quá số kiểu có sẵn
                        linetypeIndex = (linetypeIndex + 1) % linetypes.Length;
                    }
                }

                tr.Commit();
            }

            // Vẽ lại màn hình để hiển thị thay đổi
            doc.Editor.Regen();
        }

        // Hàm hỗ trợ để tải kiểu đường nét nếu cần
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
        public void TaonhomBalloon()
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
        public void Kegocvuong()
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
        [CommandMethod("XaotrepBallonTest")]
        public void XaotrepBallonTest()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Danh sách lưu trữ các bản sao mới
                List<Entity> newEntities = new List<Entity>();

                // Lưu trữ chiều dài của line trên cùng
                double topLineLength = 0;

                // Tìm chiều dài của line trên cùng
                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;

                    // Tìm dòng đầu tiên (trong trường hợp bạn có nhiều dòng)
                    if (ent is Line line)
                    {
                        topLineLength = line.Length; // Lưu chiều dài
                        break; // Dừng lại sau khi tìm thấy dòng đầu tiên
                    }
                }

                // Tính toán khoảng cách di chuyển là 3 lần chiều dài của line
                double displacementAmount = topLineLength * 3;

                // Lặp lại để sao chép các balloon
                foreach (ObjectId objId in btr)
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;

                    // Kiểm tra xem đối tượng có phải là balloon không
                    if (ent != null) // Có thể điều chỉnh loại balloon tại đây
                    {
                        // Tạo một bản sao của đối tượng
                        Entity copiedEnt = ent.Clone() as Entity;

                        if (copiedEnt != null)
                        {
                            // Lấy vị trí hiện tại của balloon
                            Extents3d extents = ent.GeometricExtents;
                            Point3d currentPosition = extents.MinPoint;

                            // Tính toán vị trí mới
                            Point3d newPosition = new Point3d(currentPosition.X + displacementAmount, currentPosition.Y + displacementAmount, currentPosition.Z);
                            // Di chuyển bản sao đến vị trí mới
                            copiedEnt.TransformBy(Matrix3d.Displacement(newPosition - currentPosition));

                            // Thêm bản sao vào danh sách
                            newEntities.Add(copiedEnt);
                        }
                    }
                }

                // Sau khi đã hoàn tất việc sao chép và di chuyển, thêm tất cả các đối tượng mới vào không gian mô hình
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
