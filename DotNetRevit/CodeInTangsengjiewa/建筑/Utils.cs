﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RevitDevelopmentFoudation.CodeInTangsengjiewa.建筑
{
    class Utils
    {
        public static bool CunBeam(FamilyInstance beam, XYZ cutpont)
        {
            var result = false;

            if (beam.Category.Id.IntegerValue != (int) BuiltInCategory.OST_StructuralFraming)
            {
                throw new Exception("Element being cut is not Beam!");
            }

            var locationline = beam.Location as LocationCurve;

            return result;
        }
    }
}