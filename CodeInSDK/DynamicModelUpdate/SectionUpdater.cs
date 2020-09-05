﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using View = Autodesk.Revit.DB.View;

namespace DynamicModelUpdate
{
    /// <summary>
    /// updater to automatically move a section in conjunction with the location of a window. 
    /// </summary>
    public class SectionUpdater : IUpdater
    {
        internal SectionUpdater(AddInId addInId)
        {
            m_updaterId = new UpdaterId(addInId, new Guid("FBF3F6B2-4C06-42d4-97C1-D1B4EB593EFF"));

        }

        //registers itself with revit
        internal void Register(Document doc)
        {
            //register the section updater if the updater is not registered   
            if (! UpdaterRegistry.IsUpdaterRegistered(m_updaterId))
            {
                UpdaterRegistry.RegisterUpdater(this, doc);
            }
        }

        internal void AddTriggerForUpdater
            (Document doc, List<ElementId> idsToWatch, ElementId sectionId, Element sectionElement)
        {
            if (idsToWatch.Count == 0)
            {
                return;
            }

            m_windowId = idsToWatch[0];
            m_sectionId = sectionId;
            m_sectionElement = sectionElement;
            UpdaterRegistry.AddTrigger(m_updaterId,doc,idsToWatch,Element.GetChangeTypeGeometry());

        }

#region IUpdater members

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                //iterate through modified elements to find the one we want the section to follow
                foreach (ElementId id in data.GetModifiedElementIds())
                {
                    if (id == m_windowId)
                    {
                        FamilyInstance window =doc.GetElement(m_windowId) as FamilyInstance;
                        ViewSection section =doc.GetElement(m_sectionId) as ViewSection;
                        RejustSectionView(doc,window,section);
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Exception", e.ToString());
            }
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public string GetUpdaterName()
        {
            return "Associative Section Updater";
        }

        public string GetAdditionalInformation()
        {
            return "Automatically moves a section to maintain its position relative to a window";
        }

#endregion
        internal void RejustSectionView(Document doc, Element elem, ViewSection section)
        {
            XYZ position = XYZ.Zero;
            XYZ fOrientation = XYZ.Zero;

            if (elem is FamilyInstance)
            {
                FamilyInstance familyInstance = elem as FamilyInstance;
                if (familyInstance.Location != null && familyInstance.Location is LocationPoint)
                {
                    LocationPoint locationPoint = familyInstance.Location as LocationPoint;
                }
                fOrientation = familyInstance.FacingOrientation;
            }

            XYZ sOrigin = section.Origin;
            XYZ sDirection = section.ViewDirection;

            XYZ fRectOrientation = fOrientation.CrossProduct(XYZ.BasisZ);

            //Rotate the section element
            double angle = fOrientation.AngleTo(sDirection);

            //need to adjust the rotation angle based on the direction of rotation (not covered by AngleTo)
            XYZ cross = fRectOrientation.CrossProduct(sDirection).Normalize();
            double sign = 1.0;

            if (!cross.IsAlmostEqualTo(XYZ.BasisZ))
            {
                sign = -1.0;
            }

            double rotateAngle = 0;
            if (Math.Abs(angle) > 0 && Math.Abs(angle) <= Math.PI / 2.0)
            {
                if (angle < 0)
                {
                    rotateAngle = Math.PI / 2.0+ angle;
                }
                else
                {
                    rotateAngle = Math.PI / 2.0 - angle;
                }
            }

            else if (Math.Abs(angle) > Math.PI /2.0)
            {
                if (angle <0)
                {
                    rotateAngle = angle + Math.PI / 2.0;
                }
                else
                {
                    rotateAngle = angle - Math.PI / 2.0;
                }
            }

            rotateAngle *= sign;

            if (Math.Abs(rotateAngle) > 0)
            {
                Line axis = Line.CreateBound(sOrigin, sOrigin + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc,m_sectionElement.Id,axis,rotateAngle);
            }

            //Regenerate the document
            doc.Regenerate();

            //move the section element
            double dotF = position.DotProduct(fRectOrientation);
            double dotS = sOrigin.DotProduct(fRectOrientation);
            double moveDot = dotF - dotS;
            XYZ sNewDirection = section.ViewDirection;// get the new direction after rotation.
            double correction = fRectOrientation.DotProduct(sNewDirection);
            XYZ translationVec = sNewDirection * correction * moveDot;

            if (! translationVec.IsZeroLength())
            {
                ElementTransformUtils.MoveElement(doc,m_sectionElement.Id,translationVec);
            }
        }

        //private data :
        private UpdaterId m_updaterId = null;
        private ElementId m_windowId = null;
        private ElementId m_sectionId = null;    //the real viewSection that contains the Origin and ViewDirection;
        private Element m_sectionElement = null; // The view section element to move and rotate
    }
}