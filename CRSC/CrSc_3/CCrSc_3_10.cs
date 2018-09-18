﻿using System.Collections.Generic;
using System.Windows;

namespace CRSC
{
    public class CCrSc_3_10 : CCrSc_0_05
    {
      // Solid rectangle

      public CCrSc_3_10(float fh, float fb, float ft)
      {
          //ITotNoPoints = 4;
          h = fh;
          b = fb;

          // Create Array - allocate memory
          //CrScPointsOut = new float[ITotNoPoints, 2];
            CrScPointsOut = new List<Point>(ITotNoPoints);
            // Fill Array Data
            CalcCrSc_Coord();
      }
    }
}
