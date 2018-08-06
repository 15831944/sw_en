﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BaseClasses;
using MATERIAL;
using CRSC;

namespace sw_en_GUI.EXAMPLES._3D
{
    public class CBlock_3D_002_WindowInBay : CBlock
    {
        public int iNumberOfGirtsUnderWindow;

        public CBlock_3D_002_WindowInBay(
            string sBuildingSide_temp,
            float fWindowHeight,
            float fWindowWidth,
            float fWindowCoordinateXinBay,
            float fWindowCoordinateZinBay,
            int iNumberOfWindowColumns,
            float fLimitDistanceFromColumn,
            float fBottomGirtPosition,
            float fDist_Girt,
            CMember referenceGirt_temp,
            CMember Colummn,
            float fBayWidth,
            float fBayHeight,
            bool bIsReverseGirtSession = false,
            bool bIsFirstBayInFrontorBackSide = false,
            bool bIsLastBayInFrontorBackSide = false)
        {
            BuildingSide = sBuildingSide_temp;
            ReferenceGirt = referenceGirt_temp;

            //iNumberOfWindowColumns = 2; // Minimum is 2
            int iNumberOfHeaders = iNumberOfWindowColumns - 1;
            int iNumberOfSills = iNumberOfWindowColumns - 1;

            // TODO napojit premennu na hlavny model a pripadne dat moznost uzivatelovi nastavit hodnotu 0 - 30 mm
            float fCutOffOneSide = 0.005f; // Cut 5 mm from each side of member

            // Basic validation
            if ((fWindowWidth + fWindowCoordinateXinBay) > fBayWidth)
                throw new Exception(); // Window is defined out of frame bay

            if ((fWindowHeight + fWindowCoordinateZinBay) > fBayHeight)
                throw new Exception(); // Window is defined out of frame height

            float fDistanceBetweenWindowColumns = fWindowWidth / (iNumberOfWindowColumns - 1);

            m_arrMat = new CMat[1];
            m_arrCrSc = new CCrSc[2];

            // Materials
            // Materials List - Materials Array - Fill Data of Materials Array
            m_arrMat[0] = new CMat_03_00();

            // Cross-sections
            // TODO - add to cross-section parameters

            // CrSc List - CrSc Array - Fill Data of Cross-sections Array
            m_arrCrSc[0] = ReferenceGirt.CrScStart; // Girts
            m_arrCrSc[1] = new CCrSc_3_10075_BOX(0.1f, 0.1f, 0.00075f, Colors.Red); // Window frame
            m_arrCrSc[1].Name = "Box 10075";

            iNumberOfGirtsUnderWindow = (int)((fWindowCoordinateZinBay - fBottomGirtPosition) / fDist_Girt) + 1;
            float fCoordinateZOfGirtUnderWindow = (iNumberOfGirtsUnderWindow - 1) * fDist_Girt + fBottomGirtPosition;

            if (fWindowCoordinateZinBay <= fBottomGirtPosition)
            {
                iNumberOfGirtsUnderWindow = 0;
                fCoordinateZOfGirtUnderWindow = 0f;
            }

            INumberOfGirtsToDeactivate = (int)((fWindowHeight + fWindowCoordinateZinBay - fCoordinateZOfGirtUnderWindow) / fDist_Girt); // Number of intermediate girts to deactivate

            bool bWindowToCloseToLeftColumn = false; // true - generate girts only on one side, false - generate girts on both sides of window
            bool bWindowToCloseToRightColumn = false; // true - generate girts only on one side, false - generate girts on both sides of window

            if (fWindowCoordinateXinBay < fLimitDistanceFromColumn)
                bWindowToCloseToLeftColumn = true; // Window is to close to the left column

            if((fBayWidth - (fWindowCoordinateXinBay + fWindowWidth)) < fLimitDistanceFromColumn)
                bWindowToCloseToRightColumn = true; // Window is to close to the right column

            int iNumberOfGirtsSequences;

            if (bWindowToCloseToLeftColumn && bWindowToCloseToRightColumn || fBottomGirtPosition > (fWindowCoordinateZinBay + fWindowHeight))
                iNumberOfGirtsSequences = 0;  // No girts (not generate girts, just window frame members)
            else if (bWindowToCloseToLeftColumn || bWindowToCloseToRightColumn)
                iNumberOfGirtsSequences = 1; // Girts only on one side of window
            else
                iNumberOfGirtsSequences = 2; // Girts on both sides of window

            int iNodesForGirts = INumberOfGirtsToDeactivate * iNumberOfGirtsSequences * 2;
            int iMembersGirts = INumberOfGirtsToDeactivate * iNumberOfGirtsSequences;
            int iNodesForWindowColumns = iNumberOfWindowColumns * 2;
            int iNodesForWindowHeaders = iNumberOfHeaders + 1;
            int iNodesForWindowSills = iNumberOfSills + 1;

            float fLimitOfHeaderOrSillAndGirtDistance = 0.1f;

            if ((fBottomGirtPosition + (INumberOfGirtsToDeactivate + iNumberOfGirtsUnderWindow) * fDist_Girt) - (fWindowCoordinateZinBay + fWindowHeight) < fLimitOfHeaderOrSillAndGirtDistance)
            {
                iNumberOfHeaders = 0; // Not generate header - girt is close to the top edge of window
                iNodesForWindowHeaders = 0;
            }

            if (fWindowCoordinateZinBay - (fBottomGirtPosition + ((iNumberOfGirtsUnderWindow - 1) * fDist_Girt)) < fLimitOfHeaderOrSillAndGirtDistance)
            {
                iNumberOfSills = 0; // Not generate sill - girt is close to the bottom edge of window
                iNodesForWindowSills = 0;
            }

            m_arrNodes = new CNode[iNodesForGirts + 2 * iNumberOfWindowColumns + iNodesForWindowHeaders + iNodesForWindowSills];
            m_arrMembers = new CMember[iMembersGirts + iNumberOfWindowColumns + iNumberOfHeaders + iNumberOfSills];

            // Block Nodes Coordinates
            // Coordinates of girt nodes

            for (int i = 0; i < iNumberOfGirtsSequences; i++) // (Girts on the left side and the right side of window)
            {
                int iNumberOfNodesOnOneSide = INumberOfGirtsToDeactivate * 2;

                float fxcoordinate_start = i * (fWindowCoordinateXinBay + fWindowWidth);
                float fxcoordinate_end = i == 0 ? fWindowCoordinateXinBay : fBayWidth;

                if (bWindowToCloseToLeftColumn) // Generate only second sequence of girt nodes
                {
                    fxcoordinate_start = fWindowCoordinateXinBay + fWindowWidth;
                    fxcoordinate_end = fBayWidth;
                }

                for (int j = 0; j < INumberOfGirtsToDeactivate; j++)
                {
                    // Start node of member
                    m_arrNodes[i * iNumberOfNodesOnOneSide + j * 2] = new CNode(i * iNumberOfNodesOnOneSide + j * 2 + 1, fxcoordinate_start, 0, fBottomGirtPosition + iNumberOfGirtsUnderWindow * fDist_Girt + j * fDist_Girt, 0);

                    // End node of member
                    m_arrNodes[i * iNumberOfNodesOnOneSide + j * 2 + 1] = new CNode(i * iNumberOfNodesOnOneSide + j * 2 + 1 + 1, fxcoordinate_end, 0, fBottomGirtPosition + iNumberOfGirtsUnderWindow * fDist_Girt + j * fDist_Girt, 0);
                }
            }

            // Coordinates of window columns nodes
            for (int i = 0; i < iNumberOfWindowColumns; i++) // (Column on the left side and the right side of window and also intermediate columns if necessary)
            {
                m_arrNodes[iNodesForGirts + i * 2] = new CNode(iNodesForGirts + i * 2 + 1, fWindowCoordinateXinBay + i * fDistanceBetweenWindowColumns, 0, fCoordinateZOfGirtUnderWindow, 0);
                m_arrNodes[iNodesForGirts + i * 2 + 1] = new CNode(iNodesForGirts + i * 2 + 1 + 1, fWindowCoordinateXinBay + i * fDistanceBetweenWindowColumns, 0, fBottomGirtPosition + iNumberOfGirtsUnderWindow * fDist_Girt + INumberOfGirtsToDeactivate * fDist_Girt, 0);
            }

            // Coordinates of window header nodes
            for (int i = 0; i < iNodesForWindowHeaders; i++) // (Headers between columns)
            {
                m_arrNodes[iNodesForGirts + iNodesForWindowColumns + i] = new CNode(iNodesForGirts + iNodesForWindowColumns + i + 1, fWindowCoordinateXinBay + i * fDistanceBetweenWindowColumns, 0, fWindowCoordinateZinBay + fWindowHeight, 0);
            }

            // Coordinates of window sill nodes
            for (int i = 0; i < iNodesForWindowSills; i++) // (Sills between columns)
            {
                m_arrNodes[iNodesForGirts + iNodesForWindowColumns + iNodesForWindowHeaders + i] = new CNode(iNodesForGirts + iNodesForWindowColumns + iNodesForWindowHeaders + i + 1, fWindowCoordinateXinBay + i * fDistanceBetweenWindowColumns, 0, fWindowCoordinateZinBay, 0);
            }

            // Block Members
            // TODO - add to block parameters

            float fGirtAllignmentStart = bIsReverseGirtSession ? ReferenceGirt.FAlignment_End : ReferenceGirt.FAlignment_Start; // Main column of a frame
            float fGirtAllignmentEnd = -0.5f * (float)m_arrCrSc[1].b - fCutOffOneSide; // Window column
            CMemberEccentricity eccentricityGirtStart = bIsReverseGirtSession ? ReferenceGirt.EccentricityEnd : ReferenceGirt.EccentricityStart;
            CMemberEccentricity eccentricityGirtEnd = bIsReverseGirtSession ? ReferenceGirt.EccentricityStart : ReferenceGirt.EccentricityEnd;
            CMemberEccentricity eccentricityGirtStart_temp;
            CMemberEccentricity eccentricityGirtEnd_temp;
            float fGirtsRotation = bIsReverseGirtSession ? (float)(ReferenceGirt.DTheta_x + Math.PI) : (float)ReferenceGirt.DTheta_x;

            // Girt Members
            for (int i = 0; i < iNumberOfGirtsSequences; i++) // (Girts on the left side and the right side of window)
            {
                int iNumberOfNodesOnOneSide = INumberOfGirtsToDeactivate * 2;

                //if (bWindowToCloseToLeftColumn) // Generate only second sequence of girt nodes
                //    i = 1;

                for (int j = 0; j < INumberOfGirtsToDeactivate; j++)
                {
                    // Alignment - switch start and end allignment for girts on the left side of window and the right side of window
                    float fGirtStartTemp = fGirtAllignmentStart;
                    float fGirtEndTemp = fGirtAllignmentEnd;

                    eccentricityGirtStart_temp = eccentricityGirtStart;
                    eccentricityGirtEnd_temp = eccentricityGirtEnd;

                    if (i == 1 || bWindowToCloseToLeftColumn) // If just right sequence of girts is generated switch allignment and eccentricity (???) need testing;
                    {
                        if (!bIsLastBayInFrontorBackSide) // Change allignment (different columns on bay sides)
                        {
                            fGirtStartTemp = fGirtAllignmentEnd;
                            fGirtEndTemp = fGirtAllignmentStart;

                            if (bIsFirstBayInFrontorBackSide) // First bay, right side, end connection to the intermediate column
                                fGirtEndTemp = ReferenceGirt.FAlignment_End;
                        }

                        eccentricityGirtStart_temp = eccentricityGirtEnd; // TODO - we need probably to change signs of values
                        eccentricityGirtEnd_temp = eccentricityGirtStart; // TODO - we need probably to change signs of values
                    }

                    m_arrMembers[i * INumberOfGirtsToDeactivate + j] = new CMember(i * INumberOfGirtsToDeactivate + j + 1, m_arrNodes[i * iNumberOfNodesOnOneSide + j * 2], m_arrNodes[i * iNumberOfNodesOnOneSide + j * 2 + 1], m_arrCrSc[0], EMemberType_FormSteel.eG, eccentricityGirtStart_temp, eccentricityGirtEnd_temp, fGirtStartTemp, fGirtEndTemp, fGirtsRotation, 0);
                    m_arrMembers[i * INumberOfGirtsToDeactivate + j].BIsDisplayed = true;
                }
            }

            INumberOfGirtsGeneratedInBlock = iNumberOfGirtsSequences * INumberOfGirtsToDeactivate;

            // TODO - add to block parameters
            float fWindowColumnStart = 0.0f;

            if(fBottomGirtPosition >= fCoordinateZOfGirtUnderWindow) // Window column is connected to the girt
                fWindowColumnStart = (float)ReferenceGirt.CrScStart.y_min - fCutOffOneSide;

            float fWindowColumnEnd = (float)ReferenceGirt.CrScStart.y_min - fCutOffOneSide;
            CMemberEccentricity feccentricityWindowColumnStart = new CMemberEccentricity(0f, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            CMemberEccentricity feccentricityWindowColumnEnd = new CMemberEccentricity(0f, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            float fWindowColumnRotation = (float)Math.PI;

            // Set eccentricity sign depending on global rotation angle and building side (left / right)
            if (BuildingSide == "Left")
            {
                feccentricityWindowColumnStart.MFz_local *= -1.0f;
                feccentricityWindowColumnEnd.MFz_local *= -1.0f;
            }

            // Window columns
            for (int i = 0; i < iNumberOfWindowColumns; i++)
            {
                m_arrMembers[iMembersGirts + i] = new CMember(iMembersGirts + i + 1, m_arrNodes[iNodesForGirts + i * 2], m_arrNodes[iNodesForGirts + i * 2 + 1], m_arrCrSc[1], EMemberType_FormSteel.eDF, feccentricityWindowColumnStart, feccentricityWindowColumnEnd, fWindowColumnStart, fWindowColumnEnd, fWindowColumnRotation, 0);
                m_arrMembers[iMembersGirts + i].BIsDisplayed = true;
            }

            // Window (header)
            // TODO - add to block parameters
            float fWindowHeaderStart = -0.5f * (float)m_arrCrSc[1].h - fCutOffOneSide;
            float fWindowHeaderEnd = -0.5f * (float)m_arrCrSc[1].h - fCutOffOneSide;
            CMemberEccentricity feccentricityWindowHeaderStart = new CMemberEccentricity(0, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            CMemberEccentricity feccentricityWindowHeaderEnd = new CMemberEccentricity(0, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            float fWindowHeaderRotation = (float)Math.PI / 2;

            // Set eccentricity sign depending on global rotation angle and building side (left / right)
            if (BuildingSide == "Left")
            {
                feccentricityWindowHeaderStart.MFz_local *= -1.0f;
                feccentricityWindowHeaderEnd.MFz_local *= -1.0f;
            }

            for (int i = 0; i < iNumberOfHeaders; i++)
            {
                m_arrMembers[iMembersGirts + iNumberOfWindowColumns + i] = new CMember(iMembersGirts + iNumberOfWindowColumns + i + 1, m_arrNodes[iNodesForGirts + iNodesForWindowColumns + i], m_arrNodes[iNodesForGirts + iNodesForWindowColumns + i + 1], m_arrCrSc[1], EMemberType_FormSteel.eDF, feccentricityWindowHeaderStart, feccentricityWindowHeaderEnd, fWindowHeaderStart, fWindowHeaderEnd, fWindowHeaderRotation, 0);
                m_arrMembers[iMembersGirts + iNumberOfWindowColumns + i].BIsDisplayed = true;
            }

            // Window (Sills)
            // TODO - add to block parameters
            float fWindowSillStart = -0.5f * (float)m_arrCrSc[1].h - fCutOffOneSide;
            float fWindowSillEnd = -0.5f * (float)m_arrCrSc[1].h - fCutOffOneSide;
            CMemberEccentricity feccentricityWindowSillStart = new CMemberEccentricity(0, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            CMemberEccentricity feccentricityWindowSillEnd = new CMemberEccentricity(0, eccentricityGirtStart.MFz_local > 0 ? eccentricityGirtStart.MFz_local + 0.5f * (float)m_arrCrSc[1].h : -eccentricityGirtStart.MFz_local - 0.5f * (float)m_arrCrSc[1].h);
            float fWindowSillRotation = (float)Math.PI / 2;

            // Set eccentricity sign depending on global rotation angle and building side (left / right)
            if (BuildingSide == "Left")
            {
                feccentricityWindowSillStart.MFz_local *= -1.0f;
                feccentricityWindowSillEnd.MFz_local *= -1.0f;
            }

            for (int i = 0; i < iNumberOfSills; i++)
            {
                m_arrMembers[iMembersGirts + iNumberOfWindowColumns + iNumberOfHeaders + i] = new CMember(iMembersGirts + iNumberOfWindowColumns + iNumberOfHeaders + i + 1, m_arrNodes[iNodesForGirts + iNodesForWindowColumns + iNodesForWindowHeaders + i], m_arrNodes[iNodesForGirts + iNodesForWindowColumns + iNodesForWindowHeaders + i + 1], m_arrCrSc[1], EMemberType_FormSteel.eDF, feccentricityWindowSillStart, feccentricityWindowSillEnd, fWindowSillStart, fWindowSillEnd, fWindowSillRotation, 0);
                m_arrMembers[iMembersGirts + iNumberOfWindowColumns + iNumberOfHeaders + i].BIsDisplayed = true;
            }

            // Connection Joints
            m_arrConnectionJoints = new List<CConnectionJointTypes>();

            // Girt Joints
            if (iMembersGirts > 0)
            {
                for (int i = 0; i < iMembersGirts; i++) // Each created girt
                {
                    CMember current_member = m_arrMembers[i];
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeStart, Colummn, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                    m_arrConnectionJoints.Add(new CConnectionJoint_T001("LH", current_member.NodeEnd, Colummn, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                }
            }

            // Column Joints
            for (int i = 0; i < iNumberOfWindowColumns; i++) // Each created column
            {
                CMember current_member = m_arrMembers[iMembersGirts + i];
                // TODO - dopracovat moznosti kedy je stlpik okna pripojeny k eave purlin, main rafter a podobne (nemusi to byt vzdy girt)

                // Bottom - columns is connected to the concrete foundation of girt (use different type of plate ???)
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeStart, iNumberOfGirtsUnderWindow == 0 ? null : ReferenceGirt, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, iNumberOfGirtsUnderWindow == 0 ? false : true, true));
                // Top
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeEnd, ReferenceGirt, current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
            }

            // Window Header Joint
            for (int i = 0; i < iNumberOfHeaders; i++) // Each created header
            {
                CMember current_member = m_arrMembers[iMembersGirts + iNumberOfWindowColumns + i];
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeStart, m_arrMembers[iMembersGirts + i], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeEnd, m_arrMembers[iMembersGirts + i + 1], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
            }

            // Window Sill Joint
            for (int i = 0; i < iNumberOfSills; i++) // Each created sill
            {
                CMember current_member = m_arrMembers[iMembersGirts + iNumberOfWindowColumns + iNumberOfHeaders + i];
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeStart, m_arrMembers[iMembersGirts + i], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
                m_arrConnectionJoints.Add(new CConnectionJoint_T001("LJ", current_member.NodeEnd, m_arrMembers[iMembersGirts + i + 1], current_member, 0, EPlateNumberAndPositionInJoint.eTwoPlates, true, true));
            }
        }
    }
}
