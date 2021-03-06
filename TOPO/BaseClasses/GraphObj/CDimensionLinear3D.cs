﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using MATH;
using BaseClasses.GraphObj.Objects_3D;

namespace BaseClasses.GraphObj
{    
    [Serializable]
    public class CDimensionLinear3D
    {
        private Point3D m_PointStart;
        private Point3D m_PointEnd;
        private Point3D m_PointText;
        
        private Point3D m_Point1_ExtensionLine1;
        private Point3D m_Point2_ExtensionLine1;
        private Point3D m_Point1_ExtensionLine2;
        private Point3D m_Point2_ExtensionLine2;

        private Point3D m_Point1_MainLine;
        private Point3D m_Point2_MainLine;

        private Vector3D m_DirectionInGCS; // Pouzije sa pre kotovanie priemetu do GCS axes
        
        private double m_ExtensionLine1Length;
        private double m_ExtensionLine2Length;
        private double m_ExtensionLines_OffsetBehindMainLine;
        
        private double m_DimensionMainLineDistance;

        private double m_fOffSetFromPoint;
        private float m_fMainLineLength;
        private double m_DimensionMainLinePositionIncludingOffset;

        private string m_Text;
        
        private bool bTextInside; // true - text je medzi vynasacimi ciarami, false - text je na opacnej strane nez vynasacie ciary
        private int iVectorOverFactor_LCS;
        private int iVectorUpFactor_LCS;

        private EGlobalPlane m_GlobalPlane; // Globalna rovina GCS do ktorej sa kota kresli 0 - XY, 1 - YZ, 2 - XZ, -1 nedefinovana (vseobecna kota)
        
        public int iVectorOfProjectionToHorizontalViewAxis; // -1 kota sa kresli horizontalne pod body, 1 kota sa kresli horizontalne nad body, 0 - nie je definovane
        public int iVectorOfProjectionToVerticalViewAxis; // -1 kota sa kresli vertikalne nalavo od bodov, 1 kota sa kresli vertiklane napravo od bodov, 0 - nie je definovane

        public Point3D PointStart
        {
            get
            {
                return m_PointStart;
            }

            set
            {
                m_PointStart = value;
            }
        }

        public Point3D PointEnd
        {
            get
            {
                return m_PointEnd;
            }

            set
            {
                m_PointEnd = value;
            }
        }
        
        public double DimensionMainLineDistance
        {
            get
            {
                return m_DimensionMainLineDistance;
            }

            set
            {
                m_DimensionMainLineDistance = value;
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }

            set
            {
                m_Text = value;
            }
        }

        public Vector3D DirectionInGCS
        {
            get
            {
                return m_DirectionInGCS;
            }

            set
            {
                m_DirectionInGCS = value;
            }
        }

        public Point3D PointText
        {
            get
            {
                return m_PointText;
            }

            set
            {
                m_PointText = value;
            }
        }
        
        public Point3D PointMainLine1
        {
            get
            {
                return m_Point1_MainLine;
            }

            set
            {
                m_Point1_MainLine = value;
            }
        }

        public Point3D PointMainLine2
        {
            get
            {
                return m_Point2_MainLine;
            }

            set
            {
                m_Point2_MainLine = value;
            }
        }

        public double OffSetFromPoint  // Odsadenie bodu vynasacej ciary (extension line) od kotovaneho bodu
        {
            get
            {
                return m_fOffSetFromPoint;
            }

            set
            {
                m_fOffSetFromPoint = value;
            }
        }

        public float MainLineLength  // Dlzka hlavnej kotovacej ciary
        {
            get
            {
                return m_fMainLineLength;
            }

            set
            {
                m_fMainLineLength = value;
            }
        }

        public double DimensionMainLinePositionIncludingOffset  // Finalna poloha hlavnej kotovacej ciary vratane offsetu od kotovaneho bodu
        {
            get
            {
                return m_DimensionMainLinePositionIncludingOffset;
            }

            set
            {
                m_DimensionMainLinePositionIncludingOffset = value;
            }
        }

        public bool TextInside
        {
            get
            {
                return bTextInside;
            }

            set
            {
                bTextInside = value;
            }
        }

        public int VectorOverFactor_LCS
        {
            get
            {
                return iVectorOverFactor_LCS;
            }

            set
            {
                iVectorOverFactor_LCS = value;
            }
        }

        public int VectorUpFactor_LCS
        {
            get
            {
                return iVectorUpFactor_LCS;
            }

            set
            {
                iVectorUpFactor_LCS = value;
            }
        }

        public EGlobalPlane GlobalPlane
        {
            get
            {
                return m_GlobalPlane;
            }

            set
            {
                m_GlobalPlane = value;
            }
        }

        public double ExtensionLine1Length
        {
            get
            {
                return m_ExtensionLine1Length;
            }

            set
            {
                m_ExtensionLine1Length = value;
            }
        }

        public double ExtensionLine2Length
        {
            get
            {
                return m_ExtensionLine2Length;
            }

            set
            {
                m_ExtensionLine2Length = value;
            }
        }

        public double ExtensionLines_OffsetBehindMainLine
        {
            get
            {
                return m_ExtensionLines_OffsetBehindMainLine;
            }

            set
            {
                m_ExtensionLines_OffsetBehindMainLine = value;
            }
        }

        public Transform3DGroup TransformGr;
        
        
        
        // iVectorOfProjectionToHorizontalViewAxis_temp
        // iVectorOfProjectionToVerticalViewAxis_temp

        // Mozno by sa dala poloha textu ci je nad kotou alebo pod kotou urcovat nejako z vektorov pohladu ??? podla toho ci je kota nad/pod alebo vlavo/vpravo od kotovanych bodov
        // ale obcas sa moze hodit ze je to nastavitelne natvrdo
        // Podla mna je dobre takto defaultne testIsInside a ked bude potrebne ho inde nakreslit,tak sa to da nastavit

        public CDimensionLinear3D(Point3D pointStart, Point3D pointEnd,
            EGlobalPlane globalPlane, // Globalna rovina GCS do ktorej sa kota kresli 0 - XY, 1 - YZ, 2 - XZ, -1 nedefinovana (vseobecna kota)            
            int iVectorOfProjectionToHorizontalViewAxis_temp, // -1 kota sa kresli horizontalne pod body, 1 kota sa kresli horizontalne nad body, 0 - nie je definovane
            int iVectorOfProjectionToVerticalViewAxis_temp, // -1 kota sa kresli vertikalne nalavo od bodov, 1 kota sa kresli vertikalne napravo od bodov, 0 - nie je definovane
            double extensionLine1Length,
            double extensionLine2Length,
            double fExtensionLine_OffsetBehindMainLine,
            double fOffsetFromPoint,
            string text,
            bool textIsInside = false)  //default by mohol byt text vo vnutri  // true - text je medzi vynasacimi ciarami, false - text je na opacnej strane nez vynasacie ciary
        {
            m_PointStart = pointStart;
            m_PointEnd = pointEnd;

            bTextInside = textIsInside;
            
            m_GlobalPlane = globalPlane; // Globalna rovina GCS do ktorej sa kota kresli 0 - XY, 1 - YZ, 2 - XZ, -1 nedefinovana (vseobecna kota)
            iVectorOfProjectionToHorizontalViewAxis = iVectorOfProjectionToHorizontalViewAxis_temp; // -1 kota sa kresli horizontalne pod body, 1 kota sa kresli horizontalne nad body, 0 - nie je definovane
            iVectorOfProjectionToVerticalViewAxis = iVectorOfProjectionToVerticalViewAxis_temp; // -1 kota sa kresli vertikalne nalavo od bodov, 1 kota sa kresli vertikalne napravo od bodov, 0 - nie je definovane

            m_ExtensionLine1Length = extensionLine1Length;
            m_ExtensionLine2Length = extensionLine2Length;
            m_ExtensionLines_OffsetBehindMainLine = fExtensionLine_OffsetBehindMainLine;
            m_fOffSetFromPoint = fOffsetFromPoint; // Odsadenie bodu vynasacej ciary (extension line) od kotovaneho bodu
            m_Text = text;

            m_fMainLineLength = Drawing3D.GetPoint3DDistanceFloat(pointStart, pointEnd);

            SetPoints_LCS();

            // Main line distance (distance from start of extension line EL1_P1 to the main line point ML_P1
            m_DimensionMainLineDistance = m_ExtensionLine1Length - m_ExtensionLines_OffsetBehindMainLine;

            // Suradnica y main line
            m_DimensionMainLinePositionIncludingOffset = -(m_DimensionMainLineDistance + m_fOffSetFromPoint);

            SetTextPointInLCS(); // Text v LCS
        }

        public void SetTextPointInLCS()
        {
            // To Ondrej - toto vsetko treba poupravovat aby to malo nejaky koncept a hlavu a patu.
            // Potrebujeme riesit to ze ked kota otocena tak ci onak, ci je pB s nejakou nizsou suradnicou nez pA, tak text je v rovine pohladu a je citatelny horizontalne, pripadne zprava
            // podobne ako sme to robili pre Plates v SystemComponent Viewer, mozno by sa dalo z toho nieco pouzit aby by sme celu tuto ulohu pre koty zuzili na 2D problem 
            // ak uz vieme v akej rovine GCS kreslime

            if (bTextInside) // Text je medzi extension lines (vektory over - doprava a up - nahor)
            {
                iVectorOverFactor_LCS = 1;
                iVectorUpFactor_LCS = 1;
            }
            else // Text na opacnej strane (vektory over - dolava a up - nadol)
            {
                iVectorOverFactor_LCS = -1;
                iVectorUpFactor_LCS = -1;
            }

            float fOffsetFromMainLine;

            if (bTextInside)
                fOffsetFromMainLine = 0.1f; // Mezera medzi ciarou a textom (kladna - text nad ciarou (+y), zaporna, text pod ciarou (-y))
            else
                fOffsetFromMainLine = -0.1f;

            m_PointText = new Point3D()
            {
                X = 0.5 * m_fMainLineLength,
                Y = m_DimensionMainLinePositionIncludingOffset + fOffsetFromMainLine,
                Z = 0
            };
        }

        
        public void SetPoints_LCS()
        {
            // Body koty v LCS, rovina XY, main line sa kresli v smere +X, extension lines sa kreslia v smere -Y
            // Origin je v pA [0,0,0] - LCS coordinates

            // pA                         pE
            // * LCS plane XY [0,0,0]     * [mainLineLength, 0, 0]
            //
            // EL1_P1                     EL2_P1
            // |                          |
            // |                          |
            // |  ML_P1    value          |  ML_P2
            // |/________________________\|
            // |\                        /|
            // EL1_P2                      EL2_P2

            m_Point1_ExtensionLine1 = new Point3D()
            {
                X = 0,
                Y = -OffSetFromPoint,
                Z = 0,
            };
            m_Point2_ExtensionLine1 = new Point3D()
            {
                X = 0,
                Y = - OffSetFromPoint - ExtensionLine1Length,
                Z = 0,
            };

            m_Point1_ExtensionLine2 = new Point3D()
            {
                X = m_fMainLineLength,
                Y = -OffSetFromPoint,
                Z = 0,
            };
            m_Point2_ExtensionLine2 = new Point3D()
            {
                X = m_fMainLineLength,
                Y = -OffSetFromPoint - ExtensionLine2Length,
                Z = 0,
            };

            m_Point1_MainLine = new Point3D()
            {
                X = 0,
                Y = -OffSetFromPoint - ExtensionLine1Length + ExtensionLines_OffsetBehindMainLine,
                Z = 0,
            };
            m_Point2_MainLine = new Point3D()
            {
                X = m_fMainLineLength,
                Y = -OffSetFromPoint - ExtensionLine2Length + ExtensionLines_OffsetBehindMainLine,
                Z = 0,
            };
        }

        public Model3DGroup GetDimensionModelNew(System.Windows.Media.Color color, float fLineCylinderRadius)
        {
            // Zakladny model koty - hlavna kotovacia ciara - smer X, vynasacie ciary - smer Y
            // TEXT by som kreslil v LCS koty do roviny XY a potom ho otacal s kotou (system potom mozeme pouzit aj pre popisy prutov, staci vyplnut zobrazenie koty a ostane len text)

            Model3DGroup model_gr = new Model3DGroup();
            DiffuseMaterial material = new DiffuseMaterial(new System.Windows.Media.SolidColorBrush(color));

            // Main Line - uvazuje sa ze [0,0,0] je v kotovanom bode
            // Main line
            // Default tip (cone height is 20% from length)
            Objects_3D.StraightLineArrow3D arrow = new Objects_3D.StraightLineArrow3D(new Point3D(0, m_DimensionMainLinePositionIncludingOffset,0), m_fMainLineLength, fLineCylinderRadius, 0, true);
            model_gr.Children.Add(arrow.GetModel3D(material));  // Add straight arrow

            // Add other lines
            // TODO - Zapracovat nastavitelnu dlzku a nastavitelny offset pre extension lines
            short NumberOfCirclePoints = 16 + 1; // Toto by malo byt rovnake ako u arrow, je potrebne to zjednotit, pridany jeden bod pre stred

            // TODO Ondrej - Tento prevod z double na float by sme mohli asi radsej odstranit
            float fExtensionLine1_Length = (float)ExtensionLine1Length;
            float fExtensionLine2_Length = (float)ExtensionLine2Length;

            float fExtensionLine1_OffsetBehindMainLine = (float)ExtensionLines_OffsetBehindMainLine;
            float fExtensionLine2_OffsetBehindMainLine = (float)ExtensionLines_OffsetBehindMainLine;

            // Extension line 1 (start)
            model_gr.Children.Add(Cylinder.CreateM_G_M_3D_Volume_Cylinder(m_Point2_ExtensionLine1, NumberOfCirclePoints, fLineCylinderRadius, fExtensionLine1_Length, material, 1));

            // Extension line 2 (end)
            model_gr.Children.Add(Cylinder.CreateM_G_M_3D_Volume_Cylinder(m_Point2_ExtensionLine2, NumberOfCirclePoints, fLineCylinderRadius, fExtensionLine2_Length, material, 1));

            RotateTransform3D rotateX = new RotateTransform3D();
            RotateTransform3D rotateY = new RotateTransform3D();
            RotateTransform3D rotateZ = new RotateTransform3D();

            // Pre sikme koty potrebujeme urcit uhly pootocenia v priestore
            // LCS koty lezi v rovine XY, [0,0,0] zodpoveda prvemu kotovanemu bodu, kota sa kresli do zaporneho smeru Y
            // Spocitame priemety 
            double dDeltaX = m_PointEnd.X - m_PointStart.X;
            double dDeltaY = m_PointEnd.Y - m_PointStart.Y;
            double dDeltaZ = m_PointEnd.Z - m_PointStart.Z;

            // Returns transformed coordinates of member nodes
            // Angles
            double dAlphaX = 0, dBetaY = 0, dGammaZ = 0;

            // Uhly pootocenia LCS okolo osi GCS
            // Angles
            dAlphaX = Geom2D.GetAlpha2D_CW(dDeltaY, dDeltaZ);
            dBetaY = Geom2D.GetAlpha2D_CW_2(dDeltaX, dDeltaZ); // !!! Pre pootocenie okolo Y su pouzite ine kvadranty !!!
            dGammaZ = Geom2D.GetAlpha2D_CW(dDeltaX, dDeltaY);

            if (m_GlobalPlane == EGlobalPlane.XY) // Kota v rovine XY - pohlad zhora
            {
                // Defaultne je kota v rovine XY, main line smeruje v smere X

                // About X (preklopenie)
                AxisAngleRotation3D axisAngleRotation3dX = new AxisAngleRotation3D();
                axisAngleRotation3dX.Axis = new Vector3D(1, 0, 0);
                axisAngleRotation3dX.Angle = iVectorOfProjectionToHorizontalViewAxis == 1 ? 180 : 0; // Ak sa ma kota kreslit nad bodmi preklopime celu kotu na druhu stranu (okolo LCS x)
                rotateX.Rotation = axisAngleRotation3dX;

                if (MathF.d_equal(dDeltaX, 0)) // Kota v smere Y
                {
                    // About Z
                    AxisAngleRotation3D axisAngleRotation3dZ = new AxisAngleRotation3D();
                    axisAngleRotation3dZ.Axis = new Vector3D(0, 0, 1);
                    axisAngleRotation3dZ.Angle = Geom2D.RadiansToDegrees(dGammaZ);
                    rotateZ.Rotation = axisAngleRotation3dZ;
                }
            }
            else if (m_GlobalPlane == EGlobalPlane.XZ)
            {
                // About X (otocenie do roviny XZ)
                AxisAngleRotation3D axisAngleRotation3dX = new AxisAngleRotation3D();
                axisAngleRotation3dX.Axis = new Vector3D(1, 0, 0);
                axisAngleRotation3dX.Angle = iVectorOfProjectionToHorizontalViewAxis == -1 ? 90 : 270; // Otocena nadol 90 od kotovanych bodov alebo nahor 270 okolo LCS x
                rotateX.Rotation = axisAngleRotation3dX;

                // Sikma kota v rovine XZ
                if (!MathF.d_equal(dDeltaZ, 0))
                {
                    if (dBetaY > Math.PI / 4) // Ak je kota viac zvislo nez horizontalne
                    {
                        axisAngleRotation3dX.Angle = iVectorOfProjectionToVerticalViewAxis == -1 ? 270 : 90; // Otocena nadol 90 od kotovanych bodov alebo nahor 270 okolo LCS x
                        rotateX.Rotation = axisAngleRotation3dX;
                    }

                    // About Y
                    AxisAngleRotation3D axisAngleRotation3dY = new AxisAngleRotation3D();
                    axisAngleRotation3dY.Axis = new Vector3D(0, 1, 0);
                    axisAngleRotation3dY.Angle = Geom2D.RadiansToDegrees(dBetaY);
                    rotateY.Rotation = axisAngleRotation3dY;
                }
            }
            else if (m_GlobalPlane == EGlobalPlane.YZ)
            {
                // About X (otocenie do roviny XZ)
                AxisAngleRotation3D axisAngleRotation3dX = new AxisAngleRotation3D();
                axisAngleRotation3dX.Axis = new Vector3D(1, 0, 0);
                axisAngleRotation3dX.Angle = iVectorOfProjectionToHorizontalViewAxis == -1 ? 90 : 270; // Otocena nadol 90 od kotovanych bodov alebo nahor 270 okolo LCS x
                rotateX.Rotation = axisAngleRotation3dX;

                // About Z (otocenie do roviny YZ)
                AxisAngleRotation3D axisAngleRotation3dZ = new AxisAngleRotation3D();
                axisAngleRotation3dZ.Axis = new Vector3D(0, 0, 1);
                axisAngleRotation3dZ.Angle = 90;
                rotateZ.Rotation = axisAngleRotation3dZ;

                // Sikma kota v rovine YZ
                if (!MathF.d_equal(dDeltaZ, 0))
                {
                    if (dAlphaX > Math.PI / 4) // Ak je kota viac zvislo nez horizontalne
                    {
                        axisAngleRotation3dX.Angle = iVectorOfProjectionToVerticalViewAxis == -1 ? 90 : 270; // Otocena nadol 90 od kotovanych bodov alebo nahor 270 okolo LCS x
                        rotateX.Rotation = axisAngleRotation3dX;
                    }

                    // About Y
                    AxisAngleRotation3D axisAngleRotation3dY = new AxisAngleRotation3D();
                    axisAngleRotation3dY.Axis = new Vector3D(0, 1, 0);
                    axisAngleRotation3dY.Angle = Geom2D.RadiansToDegrees(-dAlphaX); // !!! zaporny uhol ???
                    rotateY.Rotation = axisAngleRotation3dY;
                }
            }
            else // obecne v priestore alebo mimo rovin GCS
            {
                // TODO - toto asi nie je az tak zlozite implementovat.
                // Da sa pouzit zakladna transformacia LCS - GCS ako to robime pre prut, len by sme potrebovali urcit treti bod, aby bolo mozne urcit kam smeruju extension lines koty (3 body urcia rovinu v ktorej kota lezi)
                // U pruta je defaultne treti bod s Z = infinity (urcuje sa potom uhol pootocenia okolo x - theta)
                throw new NotImplementedException("General position of dimension in 3D is not implemented");
            }

            TranslateTransform3D translateOrigin = new TranslateTransform3D(m_PointStart.X, m_PointStart.Y, m_PointStart.Z);

            TransformGr = new Transform3DGroup();
            TransformGr.Children.Add(rotateX);
            TransformGr.Children.Add(rotateY);
            TransformGr.Children.Add(rotateZ);
            TransformGr.Children.Add(translateOrigin); // Presun celej koty v ramci GCS

            model_gr.Transform = TransformGr;

            return model_gr;
        }
    }
}
