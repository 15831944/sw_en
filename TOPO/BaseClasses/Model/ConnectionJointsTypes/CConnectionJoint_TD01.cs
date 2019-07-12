﻿using BaseClasses.GraphObj;

namespace BaseClasses
{
    public class CConnectionJoint_TD01 : CConnectionJointTypes
    {
        // TODO - neviem ci je to potrebne ale urobil som samostatne triedy TA01 - TD01 pre rozne typy pripoja ku zakladu, mozno by stacil len jeden objekt kde sa bude menit len velkost plate a screw arrangement

        // Personnel Door Frame to Foundation Connection
        float m_ft;
        int m_iHoleNo;
        float m_fd_hole;
        float m_flip;

        public CConnectionJoint_TD01() { }

        public CConnectionJoint_TD01(CNode Node_temp, CMember MainFrameColumn_temp, bool bIsDisplayed_temp)
        {
            bIsJointDefinedinGCS = false;

            m_Node = Node_temp;
            m_MainMember = MainFrameColumn_temp;
            BIsGenerated = true;
            BIsDisplayed = bIsDisplayed_temp;

            Name = "Personnel Door Frame Base Joint";

            // Plate properties
            // Todo - set correct dimensions of plate acc. to column cross-section size
            m_ft = 0.003f;
            m_iHoleNo = 2;
            m_fd_hole = 0.02f;
            m_flip = 0.075f;

            float fTolerance = 0.001f; // Gap between cross-section surface and plate surface
            float fb_plate = (float)(MainFrameColumn_temp.CrScStart.b + 2 * fTolerance + 2 * m_ft);
            float fh_plate = (float)(MainFrameColumn_temp.CrScStart.h);

            float fAlignment_x = 0; // Odsadenie plechu od definicneho uzla pruta

            float flocaleccentricity_y = m_MainMember.EccentricityStart == null ? 0f : m_MainMember.EccentricityStart.MFy_local;
            float flocaleccentricity_z = m_MainMember.EccentricityStart == null ? 0f : m_MainMember.EccentricityStart.MFz_local;

            CPoint ControlPoint_P1 = new CPoint(0, fAlignment_x, m_MainMember.CrScStart.y_min + flocaleccentricity_y - m_ft - fTolerance, -0.5f * fh_plate + flocaleccentricity_z, 0);
            CAnchor referenceAnchor = new CAnchor(0.016f, 0.0141f, 0.18f, 0.5f, true);
            CScrew referenceScrew = new CScrew("TEK", "14");

            CAnchorArrangement_BB_BG anchorArrangement = new CAnchorArrangement_BB_BG(0.5f * fh_plate, referenceAnchor);
            CScrewArrangement screwArrangement;
            string sPlatePrefix;

            SetPlateTypeAndScrewArrangement(m_MainMember.CrScStart.Name_short, referenceScrew, fh_plate, out sPlatePrefix, out screwArrangement);

            m_arrPlates = new CPlate[1];
            m_arrPlates[0] = new CConCom_Plate_BB_BG(sPlatePrefix, ControlPoint_P1, fb_plate, fh_plate, m_flip, m_ft, 90, 0, 90, anchorArrangement, screwArrangement, bIsDisplayed_temp); // Rotation angle in degrees

            if (m_Node.ID != m_MainMember.NodeStart.ID) // If true - joint at start node, if false joint at end node (se we need to rotate joint about z-axis 180 deg)
            {
                // Rotate and move joint defined in the start point [0,0,0] to the end point
                ControlPoint_P1 = new CPoint(0, m_MainMember.FLength - fAlignment_x, m_MainMember.CrScStart.y_max + flocaleccentricity_y + m_ft + fTolerance, -0.5f * fh_plate + flocaleccentricity_z, 0);
                m_arrPlates[0] = new CConCom_Plate_BB_BG(sPlatePrefix, ControlPoint_P1, fb_plate, fh_plate, m_flip, m_ft, 90, 0, 180+90, anchorArrangement, screwArrangement, bIsDisplayed_temp); // Rotation angle in degrees
            }
        }

        public override CConnectionJointTypes RecreateJoint()
        {
            return new CConnectionJoint_TD01(m_Node, m_MainMember, BIsDisplayed);
        }
    }
}