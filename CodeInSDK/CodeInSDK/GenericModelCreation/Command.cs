﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CodeInSDK.GenericModelCreation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
#region Class Member Variables
        //Application of Revit
        private Autodesk.Revit.ApplicationServices.Application m_revit;
        //the document to create generic model family
        private Autodesk.Revit.DB.Document m_familyDocument;
        //FamilyItemFactory used to create family
        private Autodesk.Revit.Creation.FamilyItemFactory
            m_creationFamily = null;
        //Count error numbers
        private int m_errCount = 0;
        //Error information
        private string m_errorInfo = "";
#endregion


#region Class Interface Implementation
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_revit = commandData.Application.Application;
                m_familyDocument = commandData.Application.ActiveUIDocument.Document;
                //create new family document if active document is not a family document
                if (!m_familyDocument.IsFamilyDocument)
                {
                    m_familyDocument = m_revit.NewFamilyDocument("Generic Model.rft");
                    if (null ==m_familyDocument)
                    {
                        message = "Cannot open family document";
                        return Result.Failed;
                    }
                }
                m_creationFamily = m_familyDocument.FamilyCreate;


                //create generic model family in the document
                CreateGenericModel();
                if (0 == m_errCount)
                {
                    return Result.Succeeded;
                }
                else
                {
                    message = m_errorInfo;
                    return Result.Failed;
                }
            }
            catch (Exception e)
            {
                message = e.ToString();
                return Result.Failed;
            }
        }
#endregion

#region Class Implementation
        /// <summary>
        /// Examples for creation in generic model families.
        /// Create extrusion .blend ,revolution, sweep, swept, blend.
        /// </summary>
        public void CreateGenericModel()
        {
            // use transaction if the family document is not active document
            Transaction transaction = new Transaction(m_familyDocument, "CreateGenericModel");
            transaction.Start();
            CreateExtrusion();
            CreateBlend();
            CreateRevolution();
            CreateSweep();
            CreateSweptBlend();
            transaction.Commit();
            return;
        }

        /// <summary>
        /// Create one rectangle extrusion
        /// </summary>
        private void CreateExtrusion()
        {
            try
            {
#region Create rectangle profile
                CurveArrArray curveArrArray = new CurveArrArray();
                CurveArray curveArray1 = new CurveArray();

                XYZ normal = XYZ.BasisZ;
                SketchPlane sketchPlane = CreateSketchPlane(normal, XYZ.Zero);

                //create one rectangle extrusion
                XYZ p0 = XYZ.Zero;
                XYZ p1 = new XYZ(10, 0, 0);
                XYZ p2 = new XYZ(10, 10, 0);
                XYZ p3 = new XYZ(0, 10, 0);
                Line line1 = Line.CreateBound(p0, p1);
                Line line2 = Line.CreateBound(p1, p2);
                Line line3 = Line.CreateBound(p2, p3);
                Line line4 = Line.CreateBound(p3, p0);
                curveArray1.Append(line1);
                curveArray1.Append(line2);
                curveArray1.Append(line3);
                curveArray1.Append(line4);

                curveArrArray.Append(curveArray1);
#endregion

                //here create rectangle extrusion
                Extrusion recExtrusion = m_creationFamily.NewExtrusion(true, curveArrArray, sketchPlane, 10);
                //move to proper place
                XYZ transPoint1 = new XYZ(-16, 0, 0);
                ElementTransformUtils.MoveElement(m_familyDocument, recExtrusion.Id, transPoint1);
            }
            catch (Exception e)
            {
                m_errCount++;
                m_errorInfo += "Unexpected exceptions occur in CreateExtrusion: " + e.ToString() + "\r\n";
            }
        }

        /// <summary>
        /// Create one blend
        /// </summary>
        private void CreateBlend()
        {
            try
            {
#region Create top and base profiles
                CurveArray topProfile = new CurveArray();
                CurveArray baseProfile = new CurveArray();

                XYZ normal = XYZ.BasisZ;
                SketchPlane sketchPlane = CreateSketchPlane(normal, XYZ.Zero);

                //Create one blend
                XYZ p00 = XYZ.Zero;
                XYZ p01 = new XYZ(10, 0, 0);
                XYZ p02 = new XYZ(10, 10, 0);
                XYZ p03 = new XYZ(0, 10, 0);
                Line line01 = Line.CreateBound(p00, p01);
                Line line02 = Line.CreateBound(p01, p02);
                Line line03 = Line.CreateBound(p02, p03);
                Line line04 = Line.CreateBound(p03, p00);

                baseProfile.Append(line01);
                baseProfile.Append(line02);
                baseProfile.Append(line03);
                baseProfile.Append(line04);

                XYZ p10 = new XYZ(5, 2, 10);
                XYZ p11 = new XYZ(8, 5, 10);
                XYZ p12 = new XYZ(5, 8, 10);
                XYZ p13 = new XYZ(2, 5, 10);
                Line line11 = Line.CreateBound(p10, p11);
                Line line12 = Line.CreateBound(p11, p12);
                Line line13 = Line.CreateBound(p12, p13);
                Line line14 = Line.CreateBound(p13, p10);

                topProfile.Append(line11);
                topProfile.Append(line12);
                topProfile.Append(line13);
                topProfile.Append(line14);
#endregion

                //here create one blend
                Blend blend = m_creationFamily.NewBlend(true, topProfile, baseProfile, sketchPlane);
                //move to proper place
                XYZ transPoint1 = new XYZ(0, 11, 0);
                ElementTransformUtils.MoveElement(m_familyDocument, blend.Id, transPoint1);
            }
            catch (Exception e)
            {
                m_errCount++;
                m_errorInfo += "Unexpected exceptions occur in CreateBlend: " + e.ToString() + "\r\n";
            }
        }


        /// <summary>
        /// Create one rectangular profile revolution
        /// </summary>
        private void CreateRevolution()
        {
            try
            {
#region Create recangular profile
                CurveArrArray curveArrArray = new CurveArrArray();
                CurveArray curveArray = new CurveArray();
                XYZ normal = XYZ.BasisZ;
                SketchPlane sketchPlane = CreateSketchPlane(normal, XYZ.Zero);

                //Create one rectangular profile revolution
                XYZ p0 = XYZ.Zero;
                XYZ p1 = new XYZ(10, 0, 0);
                XYZ p2 = new XYZ(10, 10, 0);
                XYZ p3 = new XYZ(0, 10, 0);

                Line line1 = Line.CreateBound(p0, p1);
                Line line2 = Line.CreateBound(p1, p2);
                Line line3 = Line.CreateBound(p2, p3);
                Line line4 = Line.CreateBound(p3, p0);

                XYZ pp = new XYZ(1, -1, 0);
                Line axis1 = Line.CreateBound(XYZ.Zero, pp);
                curveArray.Append(line1);
                curveArray.Append(line2);
                curveArray.Append(line3);
                curveArray.Append(line4);

                curveArrArray.Append(curveArray);
#endregion

                //here create rectangular profile revolution
                Revolution revolution1 =
                    m_creationFamily.NewRevolution(true, curveArrArray, sketchPlane, axis1, -Math.PI, 0);
                //move to proper place
                XYZ transPoint1 = new XYZ(0, 32, 0);
                ElementTransformUtils.MoveElement(m_familyDocument, revolution1.Id, transPoint1);
            }
            catch (Exception e)
            {
                m_errCount++;
                m_errorInfo += "Unexpected exceptions occur in CreateRevolution: " + e.ToString() + "\r\n";
            }
        }

        /// <summary>
        /// Create one sweep
        /// </summary>
        private void CreateSweep()
        {
            try
            {
#region Create rectangular profile and path curve
                CurveArrArray arrArray = new CurveArrArray();
                CurveArray arr = new CurveArray();

                XYZ normal = XYZ.BasisZ;
                SketchPlane sketchPlane = CreateSketchPlane(normal, XYZ.Zero);

                XYZ pnt1 = new XYZ(0, 0, 0);
                XYZ pnt2 = new XYZ(2, 0, 0);
                XYZ pnt3 = new XYZ(1, 1, 0);
                arr.Append(Arc.Create(pnt2, 1.0d, 0.0d, 180.0d, XYZ.BasisX, XYZ.BasisY));
                arr.Append(Arc.Create(pnt1, pnt3, pnt2));
                arrArray.Append(arr);
                SweepProfile profile = m_revit.Create.NewCurveLoopsProfile(arrArray);

                XYZ pnt4 = new XYZ(10, 0, 0);
                XYZ pnt5 = new XYZ(0, 10, 0);
                Curve curve = Line.CreateBound(pnt4, pnt5);

                CurveArray curves = new CurveArray();
                curves.Append(curve);

                //here create one sweep with two arcs formed the profile
                Sweep sweep =
                    m_creationFamily.NewSweep(true, curves, sketchPlane, profile, 0, ProfilePlaneLocation.Start);
                // move to proper place
                XYZ transPoint1 = new XYZ(11, 0, 0);
                ElementTransformUtils.MoveElement(m_familyDocument, sweep.Id, transPoint1);
#endregion
            }
            catch (Exception e)
            {
                m_errCount++;
                m_errorInfo += "Unexpected exceptions occur in CreateSweep: " + e.ToString() + "\r\n";
            }
        }

        /// <summary>
        /// Create one SweptBlend
        /// </summary>
        private void CreateSweptBlend()
        {
            try
            {
#region Create top and bottom profiles and path curve
                XYZ pnt1 = new XYZ(0, 0, 0);
                XYZ pnt2 = new XYZ(1, 0, 0);
                XYZ pnt3 = new XYZ(1, 1, 0);
                XYZ pnt4 = new XYZ(0, 1, 0);
                XYZ pnt5 = new XYZ(0, 0, 1);

                CurveArrArray arrarr1 = new CurveArrArray();
                CurveArray arr1 = new CurveArray();
                arr1.Append(Line.CreateBound(pnt1, pnt2));
                arr1.Append(Line.CreateBound(pnt2, pnt3));
                arr1.Append(Line.CreateBound(pnt3, pnt4));
                arr1.Append(Line.CreateBound(pnt4, pnt1));
                arrarr1.Append(arr1);

                XYZ pnt6 = new XYZ(0.5, 0, 0);
                XYZ pnt7 = new XYZ(1, 0.5, 0);
                XYZ pnt8 = new XYZ(0.5, 1, 0);
                XYZ pnt9 = new XYZ(0, 0.5, 0);

                CurveArrArray arrarr2 = new CurveArrArray();
                CurveArray arr2 = new CurveArray();
                arr2.Append(Line.CreateBound(pnt6, pnt7));
                arr2.Append(Line.CreateBound(pnt7, pnt8));
                arr2.Append(Line.CreateBound(pnt8, pnt9));
                arr2.Append(Line.CreateBound(pnt9, pnt6));
                arrarr2.Append(arr2);

                SweepProfile bottomProfile = m_revit.Create.NewCurveLoopsProfile(arrarr1);
                SweepProfile topProfile = m_revit.Create.NewCurveLoopsProfile(arrarr2);

                XYZ pnt10 = new XYZ(5, 0, 0);
                XYZ pnt11 = new XYZ(0, 20, 0);
                Curve curve = Line.CreateBound(pnt10, pnt11);

                XYZ normal = XYZ.BasisZ;
                SketchPlane sketchPlane = CreateSketchPlane(normal, XYZ.Zero);
#endregion

                //here create one swept blend
                SweptBlend newSweptBlend1 =
                    m_creationFamily.NewSweptBlend(true, curve, sketchPlane, bottomProfile, topProfile);

                //move to proper place
                XYZ transPoint1 = new XYZ(11, 32, 0);
                ElementTransformUtils.MoveElement(m_familyDocument, newSweptBlend1.Id, transPoint1);
            }
            catch (Exception e)
            {
                m_errCount++;
                m_errorInfo += "Unexpected exceptions occur in CreateSweepBlend :" + e.ToString() + "\r\n";
            }
        }

        /// <summary>
        /// get element by its id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eid"></param>
        /// <returns></returns>
        private T GetElement<T>(int eid) where T : Element
        {
            ElementId elementId = new ElementId(eid);
            return m_familyDocument.GetElement(elementId) as T;
        }


        internal SketchPlane CreateSketchPlane(XYZ normal, XYZ origin)
        {
            //first create a Geometry.Plane which need in NewSketchPlane() method
            Plane geometryPlane = Plane.CreateByNormalAndOrigin(normal, origin);
            if (null == geometryPlane)
            {
                throw new Exception("Create the geometry plane failed.");
            }
            //Then create a sketch plane using ths Geometry.Plane
            SketchPlane plane = SketchPlane.Create(m_familyDocument, geometryPlane);
            //throw exception if creation failed
            if (null == plane)
            {
                throw new Exception("Create the Sketch plane failed.");

            }
            return plane;
        }
#endregion
    }
}