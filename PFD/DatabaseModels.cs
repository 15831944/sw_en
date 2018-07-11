﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFD
{
    public class DatabaseModels
    {
        //mena premmennych treba zvolit tak aby bolo mozne im rozumiet b,L,L1,h,FrNo  to nema sancu nikto rozkodovat co znamena
        public float fb; // Building Width (X-axis)
        public float fL; // Building Length (Y-axis)
        public float fL1; // Bay Length / Distance between frames (Y-axis)
        public float fh; // Building Wall Height (Z-axis)
        public int iFrNo; // Number of Frames
        public float fRoof_Pitch_deg; // Building roof pitch
        public float fdist_girt; // Distance between girts (Z-axis)
        public float fdist_purlin; // Distance between purlins (X-axis in roof pitch slope)
        public float fdist_frontcolumn; // Distance between front columns (X-axis)
        public float fdist_girt_bottom; // Distance between bottom girt and column slab (Z-axis)
        public float fRakeAngleFrontFrame_deg; // Angle between X-axis and first frame
        public float fRakeAngleBackFrame_deg; // Angle between X-axis and last frame

        public DatabaseModels()
        { }

        public DatabaseModels(int iSelectedIndex)
        {
            fb = arr_Models_Dimensions[iSelectedIndex, 0];
            fL = arr_Models_Dimensions[iSelectedIndex, 1];
            fh = arr_Models_Dimensions[iSelectedIndex, 2];
            iFrNo = (int)arr_Models_Dimensions[iSelectedIndex, 3];
            fL1 = fL / (iFrNo - 1);
            fRoof_Pitch_deg = 15;
            fdist_girt = 0.25f * fL1;
            fdist_purlin = 0.25f * fL1;
            fdist_frontcolumn = 0.5f * fL1;
            fdist_girt_bottom = 0.3f; // Distance from concrete foundation to the centerline
            fRakeAngleFrontFrame_deg = 0.0f; // Angle between first frame and global X-axis
            fRakeAngleBackFrame_deg = 0.0f; // Angle between last frame and global X-axis
        }

        //MODEL GABLE WIDTH (M) LENGTH (M) WALL HEIGHT(M) PORTAL SPACING(M)
        public string []arr_ModelNames = new string[33]
            {"GB0810L3",
             "GB0810L4",
             "GB0812L3",
             "GB0812M3",
             "GB0812M4",
             "GB1012L3",
             "GB1012M3",
             "GB1012M4",
             "GB1212L3",
             "GB1212L4",
             "GB1212M3",
             "GB1212M4",
             "GB1215M4",
             "GB1218M6",
             "GB1218M5",
             "GB1218L4",
             "GB1518M4",
             "GB1218H4",
             "GB1818M6",
             "GB1818M5",
             "GB1820H5",
             "GB1820H4",
             "GB1824H5",
             "GB1824XH5",
             "GB2024H5",
             "GB2024XH5",
             "GB2430H6",
             "GB2430H7",
             "GB2430XH6",
             "GB3036H7",
             "GB3036XH7",
             "GB3042H8",
             "GB3042XH8"};

        public float[,] arr_Models_Dimensions = new float[33, 4]
            {
            {08, 10, 3.6f, 3},
            {08, 10, 3.6f, 4},
            {08, 12, 3.6f, 3},
            {08, 12, 4.2f, 3},
            {08, 12, 4.2f, 4},
            {10, 12, 3.6f, 3},
            {10, 12, 4.2f, 3},
            {10, 12, 4.2f, 4},
            {12, 12, 3.6f, 3},
            {12, 12, 3.6f, 4},
            {12, 12, 4.2f, 3},
            {12, 12, 4.2f, 4},
            {12, 15, 4.2f, 4},
            {12, 18, 4.2f, 4},
            {12, 18, 4.2f, 5},
            {12, 18, 4.8f, 4},
            {15, 18, 4.2f, 4},
            {15, 18, 4.8f, 4},
            {18, 20, 4.2f, 6},
            {18, 20, 4.2f, 5},
            {18, 20, 4.8f, 5},
            {18, 20, 4.8f, 4},
            {18, 24, 4.8f, 5},
            {18, 24, 6.0f, 5},
            {20, 24, 4.8f, 5},
            {20, 24, 6.0f, 5},
            {24, 30, 4.8f, 6},
            {24, 30, 4.8f, 7},
            {24, 30, 6.0f, 6},
            {30, 36, 4.8f, 7},
            {30, 36, 6.0f, 7},
            {30, 42, 4.8f, 8},
            {30, 42, 6.0f, 8}
            };
     }
}























































