﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace RevitDevelopmentFoudation.PracticeBookInRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class R0213LinqNameSpaceDetegate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            string info = "";

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WhereElementIsNotElementType();

            var numAll = collector
                .OfClass(typeof(FamilyInstance))
                .Count();
            info += numAll + "\n";

            var numOfColumn = collector
                .OfCategory(BuiltInCategory.OST_Columns)
                .OfClass(typeof(FamilyInstance))
                .Count();
            info += numOfColumn + "\n";

            Func<Element, bool> myDel = new Func<Element, bool>(IsFamilyInstanceAndColumn);
            //Element 是 in
            //bool 是 out

            var numOfColumn2 = collector
                .Count(myDel);
            info += numOfColumn2 + "\n";

            TaskDialog.Show("tips", info);
            return Result.Succeeded;
        }

        //需要委托的方法
        static bool IsFamilyInstanceAndColumn(Element e)
        {
            return (e is FamilyInstance) && (e.Category.Id == new ElementId(BuiltInCategory.OST_Columns));
        }
    }
}