﻿using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;

namespace CRSC
{
    public class CCrSc_3_270XX_C_BACK_TO_BACK : CCrSc_3_I_LIPS
    {
        // Thin-walled -back to back template // TODO - zapracovat system vykreslovania zlozenych prierezov (napr. 2 x C - prierez))

        public CCrSc_3_270XX_C_BACK_TO_BACK() { }

        public CCrSc_3_270XX_C_BACK_TO_BACK(float fh, float fb, float fc_lip, float ft) : this(0, fh, fb, fc_lip, ft, Colors.Red) { }

        public CCrSc_3_270XX_C_BACK_TO_BACK(int iID_temp, float fh, float fb, float fc_lip, float ft, Color color_temp) : base(iID_temp, fh, fb, fc_lip, ft, color_temp)
        {
            ID = iID_temp;

            Name_long = "C " + (fh * 1000).ToString() + (ft * 1000 * 100).ToString() + " back to back";
            Name_short = (fh * 1000).ToString() + (ft * 1000 * 100).ToString() + "btb";

            CSColor = color_temp;  // Set cross-section color

            IsShapeSolid = true;
            ITotNoPoints = INoPointsOut = 20;

            h = fh;
            b = fb;
            Ft_f = ft;
            Ft_w = ft;
            Fc_lip = fc_lip;

            CSColor = color_temp;
            Fd = (float)h - 2 * ft;

            // Load Cross-section Database Values - based on cross-section Name_short
            FillCrScPropertiesByTableData();

            // Create Array - allocate memory
            CrScPointsOut = new List<Point>(ITotNoPoints);

            // Fill Array Data
            CalcCrSc_Coord();

            ChangeCoordToCentroid();

            // SOLID MODEL
            // Fill list of indices for drawing of surface - triangles edges
            // Particular indices - distinguished colors of member surfaces
            loadCrScIndicesFrontSide();
            loadCrScIndicesShell();
            loadCrScIndicesBackSide();

            // Complex indices - one color of member
            loadCrScIndices();

            // WIREFRAME MODEL
            // Complex indices
            loadCrScWireFrameIndices();
        }

        public void ChangeCoordToCentroid() // Prepocita suradnice outline podla suradnic taziska
        {
            y_min = -b / 2;
            y_max = b / 2;
            z_min = -h / 2;
            z_max = h / 2;

            for (int i = 0; i < INoPointsOut; i++)
            {
                Point p = CrScPointsOut[i];
                p.X += 0;
                p.Y += 0;
                CrScPointsOut[i] = p;
            }

            for (int i = 0; i < INoPointsIn; i++)
            {
                Point p = CrScPointsIn[i];
                p.X += 0;
                p.Y += 0;
                CrScPointsIn[i] = p;
            }
        }
    }
}
