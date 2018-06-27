﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using sw_en_GUI;
using CRSC;
using BaseClasses;
using MATH;

namespace PFD
{
    /// <summary>
    /// Interaction logic for Pokus2DView.xaml
    /// </summary>
    public partial class Pokus2DView : Window
    {
        public Canvas CanvasSection2D = null;

        double modelMarginLeft_x;
        double modelMarginBottom_y;
        double fReal_Model_Zoom_Factor;

        double fModel_Length_x_page;
        double fModel_Length_y_page;

        double dModelDimension_Y_real;
        double dModelDimension_Z_real;

        double dPageWidth;
        double dPageHeight;

        float fTempMax_X;
        float fTempMin_X;
        float fTempMax_Y;
        float fTempMin_Y;
        float fTempMax_Z;
        float fTempMin_Z;

        public Pokus2DView()
        {
            InitializeComponent();

            canvasForImage.Children.Clear();
            CanvasSection2D = canvasForImage;
        }

        public Pokus2DView(CModel model)
        {
            InitializeComponent();
            canvasForImage.Children.Clear();

            dPageWidth = this.Width;
            dPageHeight = this.Height;

            if (model != null)
            {
                CalculateModelLimits(model, out fTempMax_X, out fTempMin_X, out fTempMax_Y, out fTempMin_Y, out fTempMax_Z, out fTempMin_Z);
                dModelDimension_Y_real = fTempMax_Y - fTempMin_Y;
                dModelDimension_Z_real = fTempMax_Z - fTempMin_Z;
            }

            CalculateBasicValue();

            // Set 3D environment data to generate 2D view
            // Point of View / Camera

            // View perpendicular to the global plane YZ (in "-X" direction)
            Point3D pCameraPosition = new Point3D(10,0,0);
            Vector3D pCameraViewDirection = new Vector3D(-1,0,0);
            float fViewDepth = 3; // [m]

            float fMinCoord_X = (float)(pCameraPosition.X + pCameraViewDirection.X * fViewDepth);
            float fMaxCoord_X = (float)pCameraPosition.X;

            for(int i = 0; i < model.m_arrMembers.Length; i++)
            {
                // Transform Units from 3D real model to 2D view (depends on size of window)
                Point pA = new Point(model.m_arrMembers[i].PointStart.Y * fReal_Model_Zoom_Factor, model.m_arrMembers[i].PointStart.Z * fReal_Model_Zoom_Factor);
                Point pB = new Point(model.m_arrMembers[i].PointEnd.Y * fReal_Model_Zoom_Factor, model.m_arrMembers[i].PointEnd.Z * fReal_Model_Zoom_Factor);
                double b = model.m_arrMembers[i].CrScStart.b * fReal_Model_Zoom_Factor; // Todo - to ci sa ma vykreslovat sirka alebo vyska prierezu zavisi od uhla theta smeru lokalnej osy z prierezu voci smeru pohladu
                double h = model.m_arrMembers[i].CrScStart.h * fReal_Model_Zoom_Factor;

                double width = b;
                //if (model.m_arrMembers[i].DTheta_x == 0 || model.m_arrMembers[i].DTheta_x == 2 * MathF.dPI)
                //    width = h;

                    if ((fMinCoord_X < model.m_arrMembers[i].PointStart.X && model.m_arrMembers[i].PointStart.X < fMaxCoord_X) &&
                   (fMinCoord_X < model.m_arrMembers[i].PointEnd.X && model.m_arrMembers[i].PointEnd.X < fMaxCoord_X))
                {
                    // Both definition points of the member are within interval - draw rectangle (L - length in the plane YZ)
                    double fRotationAboutX_rad = Geom2D.GetAlpha2D_CW((float)pA.X, (float)pB.X, (float)pA.Y, (float)pB.Y); // ToDo - dopocitat podla suradnic koncovych bodov v rovine pohladu YZ
                    double dLengthProjected = Math.Sqrt(MathF.Pow2(pB.X - pA.X) + MathF.Pow2(pB.Y - pA.Y));

                    DrawMember2D(Brushes.Black, Brushes.Azure, pA, width, dLengthProjected, fRotationAboutX_rad, canvasForImage);
                }
                else if (((fMinCoord_X < model.m_arrMembers[i].PointStart.X && model.m_arrMembers[i].PointStart.X < fMaxCoord_X) &&
                   (fMinCoord_X > model.m_arrMembers[i].PointEnd.X || model.m_arrMembers[i].PointEnd.X > fMaxCoord_X)) ||
                   ((fMinCoord_X < model.m_arrMembers[i].PointEnd.X && model.m_arrMembers[i].PointEnd.X < fMaxCoord_X) &&
                   (fMinCoord_X > model.m_arrMembers[i].PointStart.X || model.m_arrMembers[i].PointStart.X > fMaxCoord_X)))
                {
                    // Only one point is within interval for view depth - draw cross-section
                    // We should check that other point is perpendicular to the plane YZ otherwise modified cross-section shape (cut of he member) should be displayed

                    // Set centroid coordinates
                    Point p = new Point();
                    if (fMinCoord_X < model.m_arrMembers[i].PointStart.X && model.m_arrMembers[i].PointStart.X < fMaxCoord_X)
                    {
                        p.X = model.m_arrMembers[i].PointStart.Y * fReal_Model_Zoom_Factor;
                        p.Y = model.m_arrMembers[i].PointStart.Z * fReal_Model_Zoom_Factor;
                    }
                    else
                    {
                        p.X = model.m_arrMembers[i].PointEnd.Y * fReal_Model_Zoom_Factor;
                        p.Y = model.m_arrMembers[i].PointEnd.Z * fReal_Model_Zoom_Factor;
                    }

                    // Transform cross-section coordinates in 2D
                    float[,] crsccoordoutline = null;
                    float[,] crsccoordinline = null;

                    if (model.m_arrMembers[i].CrScStart.CrScPointsOut != null)
                    {
                        crsccoordoutline = new float[model.m_arrMembers[i].CrScStart.INoPointsOut, 2];
                        Array.Copy(model.m_arrMembers[i].CrScStart.CrScPointsOut, crsccoordoutline, model.m_arrMembers[i].CrScStart.CrScPointsOut.Length);

                        // Transfom coordinates to geometry center
                        crsccoordoutline = model.m_arrMembers[i].CrScStart.GetCoordinatesInGeometryRelatedToGeometryCenterPoint(crsccoordoutline);

                        for (int j = 0; j < model.m_arrMembers[i].CrScStart.INoPointsOut; j++)
                        {
                            float fx = (float)Geom2D.GetRotatedPosition_x_CCW(crsccoordoutline[j, 0], crsccoordoutline[j, 1], model.m_arrMembers[i].DTheta_x);
                            float fy = (float)Geom2D.GetRotatedPosition_y_CCW(crsccoordoutline[j, 0], crsccoordoutline[j, 1], model.m_arrMembers[i].DTheta_x);

                            crsccoordoutline[j, 0] = (float)(fx * fReal_Model_Zoom_Factor);
                            crsccoordoutline[j, 1] = (float)(fy * fReal_Model_Zoom_Factor);

                            crsccoordoutline[j, 0] += (float)p.X;
                            crsccoordoutline[j, 1] += (float)p.Y;
                        }
                    }

                    if (model.m_arrMembers[i].CrScStart.CrScPointsIn != null)
                    {
                        crsccoordinline = new float[model.m_arrMembers[i].CrScStart.INoPointsIn, 2];
                        Array.Copy(model.m_arrMembers[i].CrScStart.CrScPointsIn, crsccoordinline, model.m_arrMembers[i].CrScStart.CrScPointsIn.Length);

                        // Transfom coordinates to geometry center
                        crsccoordinline = model.m_arrMembers[i].CrScStart.GetCoordinatesInGeometryRelatedToGeometryCenterPoint(crsccoordinline);

                        for (int j = 0; j < model.m_arrMembers[i].CrScStart.INoPointsIn; j++)
                        {
                            float fx = (float)Geom2D.GetRotatedPosition_x_CCW(crsccoordinline[j, 0], crsccoordinline[j, 1], model.m_arrMembers[i].DTheta_x);
                            float fy = (float)Geom2D.GetRotatedPosition_y_CCW(crsccoordinline[j, 0], crsccoordinline[j, 1], model.m_arrMembers[i].DTheta_x);

                            crsccoordinline[j, 0] = (float)(fx * fReal_Model_Zoom_Factor);
                            crsccoordinline[j, 1] = (float)(fy * fReal_Model_Zoom_Factor);

                            crsccoordinline[j, 0] += (float)p.X;
                            crsccoordinline[j, 1] += (float)p.Y;
                        }
                    }

                    // Draw cross-section
                    DrawCrossSection(p, width, crsccoordoutline, crsccoordinline);
                }
                else
                {
                    // Member is outside the box (do not draw)
                }
            }

            CanvasSection2D = canvasForImage;
        }

        public void DrawMember2D(SolidColorBrush strokeColor, SolidColorBrush fillColor, Point pA, double Width, double Length, double fRotationAboutX_rad, Canvas imageCanvas)
        {
            Point lt = new Point(0, - 0.5f * Width);
            Point br = new Point(Length, 0.5f * Width);
            Rectangle rect = new Rectangle();
            rect.Stretch = Stretch.Fill;
            rect.Opacity = 0.7f;
            rect.Fill = fillColor;
            rect.Stroke = strokeColor;
            rect.Width = br.X - lt.X;
            rect.Height = br.Y - lt.Y;
            Canvas.SetTop(rect, modelMarginBottom_y - pA.Y - lt.Y);
            Canvas.SetLeft(rect, modelMarginLeft_x + pA.X + lt.X);

            // Rotate about (X-axis)
            RotateTransform Rotation2D = new RotateTransform(-fRotationAboutX_rad / MathF.dPI * 180, lt.X , -lt.Y); // TODO Doriesit vypocet uhla??? 
            rect.RenderTransform = Rotation2D;

            // Translate to the pA point coordinates in plane
            //TranslateTransform translate = new TranslateTransform(pA.X, pA.Y);
            //TranslateTransform translate = new TranslateTransform(200, 200);
            //rect.RenderTransform = translate;

            imageCanvas.Children.Add(rect);
        }

        public void DrawCrossSection(Point centroid, double fx, float[,] crsccoordoutline, float[,] crsccoordinline)
        {
            // Outer outline lines
            if (crsccoordoutline != null) // If is array of points not empty
            {
                double fCanvasTop = modelMarginBottom_y /*- fModel_Length_y_page*/ - centroid.Y;
                double fCanvasLeft = modelMarginLeft_x + centroid.X - 0.5f * fx;
                DrawPolyLine(crsccoordoutline, fCanvasTop, fCanvasLeft, Brushes.Black, PenLineCap.Flat, PenLineCap.Flat, 2, canvasForImage);
            }

            // Internal outline lines
            if (crsccoordinline != null) // If is array of points not empty
            {
                // TODO - doladit posun vonkajsieho obrysu voci vnutornemu (spravidla m_ft)
                double fCanvasTop = modelMarginBottom_y /*- fModel_Length_y_page*/ - centroid.Y;
                double fCanvasLeft = modelMarginLeft_x + centroid.X - 0.5f * fx;
                DrawPolyLine(crsccoordinline, fCanvasTop, fCanvasLeft, Brushes.Black, PenLineCap.Flat, PenLineCap.Flat, 2, canvasForImage);
            }
        }

        public void DrawPolyLine(float[,] arrPoints, double dCanvasTopTemp, double dCanvasLeftTemp, SolidColorBrush color, PenLineCap startCap, PenLineCap endCap, double thickness, Canvas imageCanvas)
        {
            PointCollection points = new PointCollection();

            for (int i = 0; i < arrPoints.Length / 2 + 1; i++)
            {
                if (i < ((arrPoints.Length / 2)))
                    points.Add(new Point(arrPoints[i, 0], arrPoints[i, 1]));
                else
                    points.Add(new Point(arrPoints[0, 0], arrPoints[0, 1])); // Last point is same as first one
            }

            Polyline myLine = new Polyline();
            myLine.Stretch = Stretch.Fill;
            myLine.Stroke = color;
            myLine.Points = points;
            myLine.StrokeThickness = thickness;
            myLine.StrokeStartLineCap = startCap;
            myLine.StrokeEndLineCap = endCap;
            Canvas.SetTop(myLine, dCanvasTopTemp);
            Canvas.SetLeft(myLine, dCanvasLeftTemp);
            imageCanvas.Children.Add(myLine);
        }

        public void CalculateBasicValue()
        {
            fModel_Length_x_page = dModelDimension_Y_real;
            fModel_Length_y_page = dModelDimension_Z_real;

            // Calculate maximum zoom factor
            // Original ratio
            double dFactor_x = fModel_Length_x_page / dPageWidth;
            double dFactor_y = fModel_Length_y_page / dPageHeight;

            // Calculate new model dimensions (zoom of model size is 90%)
            fReal_Model_Zoom_Factor = 0.9f / MathF.Max(dFactor_x, dFactor_y);

            // Set new size of model on the page
            fModel_Length_x_page = fReal_Model_Zoom_Factor * dModelDimension_Y_real;
            fModel_Length_y_page = fReal_Model_Zoom_Factor * dModelDimension_Z_real;

            modelMarginLeft_x = 0.5 * (dPageWidth - fModel_Length_x_page);

            modelMarginBottom_y = fModel_Length_y_page + 0.5 * (dPageHeight - fModel_Length_y_page);
        }

        public void CalculateModelLimits(CModel cmodel,
    out float fTempMax_X,
    out float fTempMin_X,
    out float fTempMax_Y,
    out float fTempMin_Y,
    out float fTempMax_Z,
    out float fTempMin_Z
    )
        {
            fTempMax_X = float.MinValue;
            fTempMin_X = float.MaxValue;
            fTempMax_Y = float.MinValue;
            fTempMin_Y = float.MaxValue;
            fTempMax_Z = float.MinValue;
            fTempMin_Z = float.MaxValue;

            if (cmodel.m_arrNodes != null) // Some nodes exist
            {
                for (int i = 0; i < cmodel.m_arrNodes.Length; i++)
                {
                    // Maximum X - coordinate
                    if (cmodel.m_arrNodes[i].X > fTempMax_X)
                        fTempMax_X = cmodel.m_arrNodes[i].X;

                    // Minimum X - coordinate
                    if (cmodel.m_arrNodes[i].X < fTempMin_X)
                        fTempMin_X = cmodel.m_arrNodes[i].X;

                    // Maximum Y - coordinate
                    if (cmodel.m_arrNodes[i].Y > fTempMax_Y)
                        fTempMax_Y = cmodel.m_arrNodes[i].Y;

                    // Minimum Y - coordinate
                    if (cmodel.m_arrNodes[i].Y < fTempMin_Y)
                        fTempMin_Y = cmodel.m_arrNodes[i].Y;

                    // Maximum Z - coordinate
                    if (cmodel.m_arrNodes[i].Z > fTempMax_Z)
                        fTempMax_Z = cmodel.m_arrNodes[i].Z;

                    // Minimum Z - coordinate
                    if (cmodel.m_arrNodes[i].Z < fTempMin_Z)
                        fTempMin_Z = cmodel.m_arrNodes[i].Z;
                }
            }
            else if (cmodel.m_arrGOPoints != null) // Some points exist
            {
                for (int i = 0; i < cmodel.m_arrGOPoints.Length; i++)
                {
                    // Maximum X - coordinate
                    if (cmodel.m_arrGOPoints[i].X > fTempMax_X)
                        fTempMax_X = (float)cmodel.m_arrGOPoints[i].X;

                    // Minimum X - coordinate
                    if (cmodel.m_arrGOPoints[i].X < fTempMin_X)
                        fTempMin_X = (float)cmodel.m_arrGOPoints[i].X;

                    // Maximum Y - coordinate
                    if (cmodel.m_arrGOPoints[i].Y > fTempMax_Y)
                        fTempMax_Y = (float)cmodel.m_arrGOPoints[i].Y;

                    // Minimum Y - coordinate
                    if (cmodel.m_arrGOPoints[i].Y < fTempMin_Y)
                        fTempMin_Y = (float)cmodel.m_arrGOPoints[i].Y;

                    // Maximum Z - coordinate
                    if (cmodel.m_arrGOPoints[i].Z > fTempMax_Z)
                        fTempMax_Z = (float)cmodel.m_arrGOPoints[i].Z;

                    // Minimum Z - coordinate
                    if (cmodel.m_arrGOPoints[i].Z < fTempMin_Z)
                        fTempMin_Z = (float)cmodel.m_arrGOPoints[i].Z;
                }
            }
            else
            {
                // Exception - no definition nodes or points
            }
        }
    }
}
