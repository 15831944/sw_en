﻿using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BaseClasses.GraphObj.Objects_3D
{
    public class CurvedLineArrow3D
    {
        public Point3D pCenter;

        public float fConeHeight;
        public float fCylinderHeight;
        public float fTotalLength;
        public float fLineRadius;
        public float fRadius;

        public float[,] fAnnulusOutPoints;
        public float[,] fAnnulusInPoints;

        public Point3DCollection ArrowPoints;
        //public Color SurfaceColor;
        //public float m_fOpacity;

        const int number_of_segments = 72;

        public CurvedLineArrow3D()
        { }

        public CurvedLineArrow3D(Point3D pCPoint, float lineRadius)
        {
            pCenter = pCPoint;
            fLineRadius = lineRadius;
            //SurfaceColor = cColor;
            //m_fOpacity = fOpacity;

            fRadius = 0.005f * lineRadius;
        }

        public GeometryModel3D GetTorusGeometryModel3D(DiffuseMaterial mat)
        {
            ParametricSurface ps = new ParametricSurface(fLineRadius, fRadius, pCenter);

            ps.Umin = 0;
            ps.Umax = /*2 **/ Math.PI; // hlavny uhol
            ps.Vmin = 0;
            ps.Vmax = 2 * Math.PI;
            ps.Nu = 60; // delenie
            ps.Nv = 30;
            ps.CreateSurface(mat);

            return ps.GeometryModel3D;
        }
    }
}
