﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using 唐僧解瓦.BinLibrary.Extensions;
using 唐僧解瓦.BinLibrary.Helpers;

namespace 唐僧解瓦.通用
{
    /// <summary>
    /// 创建局部三维视图
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    class Cmd_3DregionView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            var acview = doc.ActiveView;


            if (!(acview is View3D))
            {
                MessageBox.Show("请在平面视图使用此命令！");
                return Result.Cancelled;
            }

            var collector = new FilteredElementCollector(doc);

            var pickedbox = sel.PickBox(PickBoxStyle.Directional);

            var min = pickedbox.Min;
            var max = pickedbox.Max;

            Transform transf = Transform.Identity;
            transf.Origin = XYZ.Zero;
            transf.BasisX = XYZ.BasisX;
            transf.BasisY = XYZ.BasisY;
            transf.BasisZ = XYZ.BasisZ;

            var newmin = new XYZ(min.X, min.Y, acview.GenLevel.Elevation);
            var newmax = new XYZ(max.X, max.Y, acview.GenLevel.Elevation + 4000d.MetricToFeet());

            BoundingBoxXYZ box = new BoundingBoxXYZ();
            box.Transform = transf;
            box.Min = newmin;
            box.Max = newmax;

            var temline = Line.CreateBound(box.Min, box.Max);
            doc.NewLine(temline);

            doc.Invoke(m =>
            {
                var view_3d = Create3DView(doc, pickedbox) as View3D;
                doc.Regenerate();
                if(view_3d==null) MessageBox.Show("view_3d is null");
                view_3d.SetSectionBox(box);
                view_3d.LookupParameter("剖面框").Set(1);
            },"创建三维视图");

            return Result.Succeeded;
        }

        public View Create3DView(Document doc, PickedBox pickedbox)
        {
            var viewfamilytypes = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>().Where(m=>m.ViewFamily == ViewFamily.ThreeDimensional);
            var targetviewfamilytypeId = viewfamilytypes.First().Id;
            var result = View3D.CreateIsometric(doc, targetviewfamilytypeId);
            return result;
        }
    }

}
