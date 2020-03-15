﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CodeInTangsengjiewa4.BinLibrary.Helpers
{
    public static class CollectorHelper
    {
        public static IList<T> TCollector<T>(this Document doc)
        {
            Type type = typeof(T);
            return new FilteredElementCollector(doc).OfClass(type).Cast<T>().ToList();
        }
    }
}